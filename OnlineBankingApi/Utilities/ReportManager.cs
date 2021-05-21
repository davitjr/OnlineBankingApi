using ContractServiceRef;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Enumerations;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace OnlineBankingApi.Utilities
{
    public class ReportManager
    {

        private readonly XBService _xBService;
        private readonly ReportService _reportService;
        private readonly CacheHelper _cacheHelper;
        private readonly XBService _xbService;
        private readonly IConfiguration _config;
        private readonly string _reportName;
        private readonly XBInfoService _xBInfoService;

        public ReportManager(XBService xBService, ReportService reportService, CacheHelper cacheHelper,IConfiguration config, XBInfoService xBInfoService)
        {
            _xBService = xBService;
            _reportService = reportService;
            _cacheHelper = cacheHelper;
            _config = config;
            _xBInfoService = xBInfoService;
            if (Convert.ToBoolean(_config["TestVersion"]))
                _reportName = "ACBAReportsTeam1";
            else
                _reportName = "ACBAReports";
        }

        public async Task<SingleResponse<byte[]>> PrintCurrentAccountStatement(string accountNumber, string dateFrom, string dateTo, ushort includingExchangeRate, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            bool hasPermission = true;
            if (authorizedCustomer.LimitedAccess != 0)
            {
                if (!_xbService.HasProductPermission(accountNumber))
                {
                    hasPermission = false;
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Տվյալները հասանելի չեն։";
                }
            }
            if (hasPermission)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(key: "account_gl", value: accountNumber);
                parameters.Add(key: "start_date", value: Convert.ToDateTime(dateFrom).ToString("MM/dd/yyyy"));
                parameters.Add(key: "end_date", value: Convert.ToDateTime(dateTo).ToString("MM/dd/yyyy"));
                parameters.Add(key: "lang_id", value: "1");
                parameters.Add(key: "set_number", value: "0");
                parameters.Add(key: "payerData", value: "2");
                parameters.Add(key: "additionalInformationByCB", value: "0");
                parameters.Add(key: "filial_code", value: "22000");
                parameters.Add(key: "averageRest", value: "1");
                parameters.Add(key: "currencyRegulation", includingExchangeRate == 1 ? "2" : "1");
                parameters.Add(key: "includingExchangeRate", value: includingExchangeRate == 1 ? "2" : "1");
                string path = $"/{_reportName}/CurrentAccountStatementNew";
                string fileName = "CurrentAccountStatement";
                response.Result = await _reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName);
                response.ResultCode = ResultCodes.normal;

            }

            return response;

        }

        public async Task<SingleResponse<byte[]>> PrintDepositAccountStatement(long productId, string accountNumber, string dateFrom, string dateTo, ushort includingExchangeRate, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            bool hasPermission = true;
            if (authorizedCustomer.LimitedAccess != 0)
            {
                if (!_xbService.HasProductPermission(accountNumber))
                {
                    hasPermission = false;
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Տվյալները հասանելի չեն։";
                }
            }
            if (hasPermission)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(key: "product_id", value: productId.ToString());
                parameters.Add(key: "account_gl", value: accountNumber);
                parameters.Add(key: "start_date", value: Convert.ToDateTime(dateFrom).ToString("MM/dd/yyyy"));
                parameters.Add(key: "end_date", value: Convert.ToDateTime(dateTo).ToString("MM/dd/yyyy"));
                parameters.Add(key: "lang_id", value: "1");
                parameters.Add(key: "set_number", value: "0");
                parameters.Add(key: "payerData", value: "2");
                parameters.Add(key: "additionalInformationByCB", value: "0");
                parameters.Add(key: "filial_code", value: "22000");
                parameters.Add(key: "averageRest", value: "1");
                parameters.Add(key: "currencyRegulation", value: "1");
                string path = $"/{_reportName}/DepositAccountStatementNew";
                string fileName = "DepositAccountStatementNew";
                response.Result = await _reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName);
                response.ResultCode = ResultCodes.normal;

            }

            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintMemorialOrder(string accountNumber, DateTime dateFrom, DateTime dateTo)
        {

            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            bool hasPermission = true;
            if (authorizedCustomer.LimitedAccess != 0)
            {
                if (!_xbService.HasProductPermission(accountNumber))
                {
                    hasPermission = false;
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Տվյալները հասանելի չեն։";
                }
            }

            if (hasPermission)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(key: "account", value: accountNumber);
                parameters.Add(key: "start_date", value: dateFrom.ToString("dd/MMM/yy"));
                parameters.Add(key: "end_date", value: dateTo.ToString("dd/MMM/yy"));
                parameters.Add(key: "correct_mo", value: "false");
                parameters.Add(key: "bankCode", value: "22000");
                parameters.Add(key: "filter_str", value: String.Empty);

                string filename = "Memorial_by_Period";
                string path = $"/{_reportName}/Memorial_by_Period";
                response.Result = await _reportService.RenderReport(path, parameters, ExportFormat.PDF, filename);
                response.ResultCode = ResultCodes.normal;
            }

            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintInternationalOrder(long orderId)
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            string AmountInDebetCurrency = String.Empty;
            string sentTime = "";
            InternationalPaymentOrder paymentOrder = new InternationalPaymentOrder();
            paymentOrder = _xBService.GetInternationalPaymentOrder(orderId);
            if (paymentOrder.Quality != OrderQuality.Completed)
            {
                response.ResultCode = ResultCodes.failed;
                response.Description = "Գործարքը կատարված չէ։";
                return response;
            }
            sentTime = _xBService.GetInternationalTransferSentTime((int)orderId);

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(key: "Number", value: paymentOrder.OrderNumber);

            parameters.Add(key: "TransactionDate", value: paymentOrder.RegistrationDate.ToString("dd/MMM/yy"));

            parameters.Add(key: "TransactionTime", value: sentTime);
            parameters.Add(key: "OurBen", value: paymentOrder.DetailsOfCharges);
            parameters.Add(key: "FastSystem", value: "0");
            parameters.Add(key: "System", value: "SWIFT");
            parameters.Add(key: "Swift", value: "1");
            parameters.Add(key: "TransferSystem", value: "SWIFT");
            parameters.Add(key: "AcbaTransfer", value: "");
            parameters.Add(key: "SenderName", value: paymentOrder.Sender);
            parameters.Add(key: "SenderAddress", value: paymentOrder.SenderAddress);
            if (paymentOrder.SenderType == 6)
            {
                parameters.Add(key: "SenderPassport", value: paymentOrder.SenderPassport);
                parameters.Add(key: "SenderDateOfBirth", value: paymentOrder.SenderDateOfBirth.ToString("dd/MM/yy"));
                parameters.Add(key: "SenderEmail", value: paymentOrder.SenderEmail);
            }
            else
            {
                parameters.Add(key: "SenderTaxCode", value: paymentOrder.SenderCodeOfTax);
            }

            parameters.Add(key: "SenderPhone", value: paymentOrder.SenderPhone);
            parameters.Add(key: "SenderOtherBankAccount", value: paymentOrder.SenderOtherBankAccount);
            parameters.Add(key: "Amount", value: paymentOrder.Amount.ToString());
            parameters.Add(key: "Currency", value: paymentOrder.Currency);
            parameters.Add(key: "Receiver", value: paymentOrder.Receiver);
            parameters.Add(key: "ReceiverAddInf", value: paymentOrder.ReceiverAddInf);
            parameters.Add(key: "ReceiverAccount", value: paymentOrder.ReceiverAccount.AccountNumber);
            parameters.Add(key: "INN", value: paymentOrder.INN);
            if (paymentOrder.Currency == "USD" && paymentOrder.Country == "840" && !String.IsNullOrEmpty(paymentOrder.FedwireRoutingCode))
                parameters.Add(key: "ReceiverBankSwift", value: "FW" + paymentOrder.FedwireRoutingCode);
            else
                parameters.Add(key: "ReceiverBankSwift", value: paymentOrder.ReceiverBankSwift);


            parameters.Add(key: "ReceiverAccountBank", value: paymentOrder.ReceiverBank != null ? paymentOrder.ReceiverBank : "");
            parameters.Add(key: "ReceiverBankAddInf", value: paymentOrder.ReceiverBankAddInf != null ? paymentOrder.ReceiverBankAddInf : "");
            parameters.Add(key: "Bik", value: paymentOrder.BIK);
            parameters.Add(key: "Ks", value: paymentOrder.CorrAccount);
            parameters.Add(key: "Kpp", value: paymentOrder.KPP);
            parameters.Add(key: "IntermidateBankSwift", value: paymentOrder.IntermediaryBankSwift);
            parameters.Add(key: "IntermidateBank", value: paymentOrder.IntermediaryBank);
            parameters.Add(key: "PaymentDescription", value: paymentOrder.DescriptionForPayment);
            parameters.Add(key: "PaymentDescriptionRUR", value: paymentOrder.DescriptionForPaymentRUR1);
            if (paymentOrder.Currency == "RUR")
            {
                string descriptionForPaymentRUR = "";
                descriptionForPaymentRUR = (paymentOrder.DescriptionForPaymentRUR2 == null ? "" : paymentOrder.DescriptionForPaymentRUR2) + " " + (paymentOrder.DescriptionForPaymentRUR3 == null ? "" : paymentOrder.DescriptionForPaymentRUR3) + " " + (paymentOrder.DescriptionForPaymentRUR4 == null ? "" : paymentOrder.DescriptionForPaymentRUR4) + " " + (paymentOrder.DescriptionForPaymentRUR5 == null ? "" : paymentOrder.DescriptionForPaymentRUR5);
                if (paymentOrder.DescriptionForPaymentRUR5 == "с НДС")
                {
                    descriptionForPaymentRUR = descriptionForPaymentRUR + " " + paymentOrder.DescriptionForPaymentRUR6 + " RUB";
                }
                parameters.Add(key: "PaymentDescriptionRUROther", value: descriptionForPaymentRUR);

            }

            parameters.Add(key: "DebetAccount", value: paymentOrder.DebitAccount.AccountNumber);
            parameters.Add(key: "MT", value: paymentOrder.DebitAccount.AccountNumber + " " + paymentOrder.DebitAccount.Currency);
            if (paymentOrder.DebitAccount.Currency == paymentOrder.Currency)
                AmountInDebetCurrency = "";
            else if (paymentOrder.DebitAccount.Currency == "AMD")
                AmountInDebetCurrency = Math.Round(paymentOrder.Amount * paymentOrder.ConvertationRate, 1).ToString("#,###.0") + " " + paymentOrder.DebitAccount.Currency;
            else
                AmountInDebetCurrency = Math.Round(paymentOrder.Amount * (paymentOrder.ConvertationRate1 / paymentOrder.ConvertationRate), 2).ToString("#,###.#0") + " " + paymentOrder.DebitAccount.Currency;

            parameters.Add(key: "Commission", value: paymentOrder.TransferFee.ToString());
            parameters.Add(key: "FileName", value: "InternationalTransferApplicationForm");
            parameters.Add(key: "AmountInDebetCurrency", value: AmountInDebetCurrency);

            string path = $"/{_reportName}/InternationalTransferApplicationForm";
            string fileName = "InternationalTransferApplicationForm";
            response.Result = await _reportService.RenderReport(path, parameters, ExportFormat.PDF, fileName);
            response.ResultCode = ResultCodes.normal;
            return response;
        }


        public async Task<SingleResponse<byte[]>> PrintUtilitylOrder(long orderId)
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            List<KeyValuePair<string, string>> parametersForReport = new List<KeyValuePair<string, string>>();

            UtilityPaymentOrder paymentOrder = new UtilityPaymentOrder();
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

            paymentOrder = _xBService.GetUtilityPaymentOrder(orderId);
            if (paymentOrder.Quality != OrderQuality.Completed)
            {
                response.ResultCode = ResultCodes.failed;
                response.Description = "Գործարքը կատարված չէ։";
                return response;
            }

            parametersForReport = _xBService.GetCommunalReportParametersIBanking(orderId, paymentOrder.CommunalType);
            switch (paymentOrder.CommunalType)
            {
                case XBS.CommunalTypes.ENA:
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "DocID", value: paymentOrder.Id.ToString());
                        parameters.Add(key: "FilialCode", value: paymentOrder.FilialCode.ToString());
                        parameters.Add(key: "OrderNum", value: paymentOrder.OrderNumber.ToString());
                        parameters.Add(key: "RePrint", value: "1");
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "F_J", value: (paymentOrder.AbonentType == 1 ? "F" : "J"));
                        parameters.Add(key: "Branch", value: paymentOrder.Branch.ToString());
                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/ElentricityPaymentReportPlPor", parameters, ExportFormat.PDF, "ElectricityPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.Gas:
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");
                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/GasProm_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "GasPromPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.ArmWater:
                    {
                        XBS.CustomerMainData customerData = _xBService.GetCustomerMainData(authorizedCustomer.CustomerNumber);
                        string fullName = customerData.CustomerDescription;
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");
                        parameters.Add(key: "PayerName", value: fullName);
                        parameters.Add(key: "DebetAccount", value: paymentOrder.DebitAccount.AccountNumber);
                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/ArmWater_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "ArmWaterPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.YerWater:
                    {
                        XBS.CustomerMainData customerData = _xBService.GetCustomerMainData(authorizedCustomer.CustomerNumber);
                        string fullName = customerData.CustomerDescription;
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");
                        parameters.Add(key: "PayerName", value: fullName);
                        parameters.Add(key: "DebetAccount", value: paymentOrder.DebitAccount.AccountNumber);
                        parameters.Add(key: "OrderNumber", value: paymentOrder.OrderNumber);
                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/VeoliaJur_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "VeoliaJurPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.ArmenTel:
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");
                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/Armentel_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "ArmentelPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.VivaCell:
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");

                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/VivaCell_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "VivaCellPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.Orange:
                    {
                        XBS.CustomerMainData customerData = _xBService.GetCustomerMainData(authorizedCustomer.CustomerNumber);
                        string fullName = customerData.CustomerDescription;
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");
                        parameters.Add(key: "eFOCode", value: "eFO 75-00-87/1#3");
                        parameters.Add(key: "AmountCurrency", value: "AMD");






                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/Orange_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "OrangePaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                case XBS.CommunalTypes.UCom:
                    {
                        XBS.CustomerMainData customerData = _xBService.GetCustomerMainData(authorizedCustomer.CustomerNumber);
                        string fullName = customerData.CustomerDescription;
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add(key: "Cash", value: "0");
                        parameters.Add(key: "RePrint", value: "1");
                        foreach (KeyValuePair<string, string> oneParameter in parametersForReport)
                        {
                            parameters.Add(key: oneParameter.Key, value: oneParameter.Value);
                        }
                        response.Result = await _reportService.RenderReport($"/{_reportName}/UCOM_Payment_Report_Pl_Por", parameters, ExportFormat.PDF, "UCOMPaymentReport");
                        response.ResultCode = ResultCodes.normal;
                        break;
                    }
                default:
                    {
                        response.ResultCode = ResultCodes.failed;
                        response.Description = "Կոմունալի տեսակը որոշված չէ։";
                        break;
                    }
            }
            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintCardStatement(string cardNumber, string dateFrom, string dateTo, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            //Card card = _xBService.GetCardByCardNumber(cardNumber);
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            bool hasPermission = true;
            if (authorizedCustomer.LimitedAccess != 0)
            {
                if (!_xbService.HasProductPermission(cardNumber)) // ?????
                {
                    hasPermission = false;
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Տվյալները հասանելի չեն։";
                }
            }
            if (hasPermission)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(key: "card", value: cardNumber);
                parameters.Add(key: "start_date", value: Convert.ToDateTime(dateFrom).ToString("MM/dd/yyyy"));
                parameters.Add(key: "end_date", value: Convert.ToDateTime(dateTo).ToString("MM/dd/yyyy"));
                parameters.Add(key: "fil", value: "22000");
                parameters.Add(key: "showAdvertisement", value: "false");
                string path = $"/{_reportName}/CardStatement";
                string fileName = "CardStatement";
                response.Result = await _reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName);
                response.ResultCode = ResultCodes.normal;
            }
            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintCardSwiftDetails(string accountNumber)
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            bool hasPermission = true;
            if (authorizedCustomer.LimitedAccess != 0)
            {
                if (!_xbService.HasProductPermission(accountNumber))
                {
                    hasPermission = false;
                    response.ResultCode = ResultCodes.failed;
                    response.Description = "Տվյալները հասանելի չեն։";
                }
            }
            if (hasPermission)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(key: "card_account", value: accountNumber);
                string path = $"/{_reportName}/Card_Swift_Details";
                string fileName = "CardSwiftDetails";
                response.Result = await _reportService.RenderReport(path, parameters, ExportFormat.PDF, fileName);
            }

            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintTransfersAcbaStatement(long id, int lang, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();

            PaymentOrder paymentOrder = _xBService.GetPaymentOrder(id);

            switch (paymentOrder.Type)
            {
                case XBS.OrderType.Convertation:
                    return PrintExchangeOrderStatement(id, lang, paymentOrder, exportFormat).Result;
            }
            if (paymentOrder.Quality == OrderQuality.Completed)
            {
                string policeCode = "";
                string description = paymentOrder.Description;
                long policeResponseDetailsId = 0;
                int regCode = 0;

                if (!String.IsNullOrEmpty(paymentOrder.CreditCode))
                {
                    description += ", " + paymentOrder.CreditCode + ", " + paymentOrder.Borrower + ", " + paymentOrder.MatureTypeDescription;
                }


                if (paymentOrder.Type == OrderType.RATransfer)
                {
                    BudgetPaymentOrder budgetPaymentOrder = new BudgetPaymentOrder();
                    if (paymentOrder.SubType != 5 && paymentOrder.SubType != 6)  //ՀՀ տարածքում / Հաշիվների միջև
                    {
                        paymentOrder = _xBService.GetPaymentOrder(id);
                    }
                    else  //Բյուջե / Ճանապարհային Ոստիկանություն փոխանցում
                    {
                        budgetPaymentOrder = _xBService.GetBudgetPaymentOrder(id);
                        paymentOrder = budgetPaymentOrder;

                        policeCode = budgetPaymentOrder.PoliceCode == 0 ? "" : budgetPaymentOrder.PoliceCode.ToString();
                        policeResponseDetailsId = budgetPaymentOrder.PoliceResponseDetailsId;
                        regCode = budgetPaymentOrder.LTACode;

                    }
                }

                AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                CustomerMainData customerData = _xBService.GetCustomerMainData(authorizedCustomer.CustomerNumber);
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                string sentTime = _xBService.GetInternationalTransferSentTime((int)id);

                parameters.Add(key: "cred_acc", value: paymentOrder.ReceiverAccount.AccountNumber + policeCode);
                parameters.Add(key: "deb_acc", value: paymentOrder.DebitAccount.AccountNumber.ToString());
                parameters.Add(key: "deb_bank", value: paymentOrder.DebitAccount.AccountNumber.Substring(0, 5).ToString());
                parameters.Add(key: "reg_date", value: paymentOrder.OperationDate.Value.ToString("dd/MMM/yyyy"));
                parameters.Add(key: "lang_id", value: lang.ToString());
                parameters.Add(key: "credit_bank", value: paymentOrder.ReceiverAccount.AccountNumber.Substring(0, 5));
                parameters.Add(key: "amount", value: paymentOrder.Amount.ToString());
                parameters.Add(key: "currency", value: paymentOrder.Currency.ToString());
                parameters.Add(key: "descr", value: description);
                parameters.Add(key: "confirm_date", value: paymentOrder.ConfirmationDate.Value.ToString("dd/MMM/yyyy"));
                parameters.Add(key: "for_HB", value: "1");
                parameters.Add(key: "doc_id", value: id.ToString());
                parameters.Add(key: "document_number", value: paymentOrder.OrderNumber.ToString());
                parameters.Add(key: "reciever", value: paymentOrder.Receiver != null ? paymentOrder.Receiver : "");

                if (paymentOrder.CreditorDescription != null)
                {
                    description += ", " + paymentOrder.CreditorDescription;
                    parameters.Add(key: "debtor_Name", value: paymentOrder.CreditorDescription);
                }

                if (paymentOrder.CreditorDocumentNumber != null)
                {
                    if (paymentOrder.CreditorDocumentType == 1)
                    {
                        description += ", ՀԾՀ " + paymentOrder.CreditorDocumentNumber;
                        parameters.Add(key: "debtor_soccard", value: paymentOrder.CreditorDocumentNumber);

                    }
                    else if (paymentOrder.CreditorDocumentType == 2)
                    {
                        description += ", Պարտատիրոջ ՀԾՀ չստանալու մասին տեղեկանքի համար " + paymentOrder.CreditorDocumentNumber;
                        parameters.Add(key: "debtor_soccard", value: paymentOrder.CreditorDocumentNumber);
                    }
                    else if (paymentOrder.CreditorDocumentType == 3)
                        description += ", Անձնագիր " + paymentOrder.CreditorDocumentNumber;
                    else if (paymentOrder.CreditorDocumentType == 4)
                    {
                        description += ", ՀՎՀՀ " + paymentOrder.CreditorDocumentNumber;
                        parameters.Add(key: "debtor_code_of_tax", value: paymentOrder.CreditorDocumentNumber);
                    }
                }

                if (paymentOrder.CreditorDeathDocument != null)
                {
                    description += ", Մահվան վկայական " + paymentOrder.CreditorDeathDocument;

                }
                if (paymentOrder.Fees != null)
                {
                    if (paymentOrder.Fees.Exists(m => m.Type == 20 || m.Type == 5))
                    {
                        double transferFee = paymentOrder.Fees.Find(m => m.Type == 20 || m.Type == 5).Amount;
                        parameters.Add(key: "commission", value: transferFee.ToString());
                    }
                }

                parameters.Add(key: "TransactionTime", value: sentTime);


                parameters.Add(key: "print_soc_card", value: customerData.CustomerType == 6 ? "True" : "False");
                parameters.Add(key: "is_copy", value: "False");
                parameters.Add(key: "reciever_tax_code", value: "");
                parameters.Add(key: "reg_code", value: regCode.ToString());
                parameters.Add(key: "cust_name", value: customerData.CustomerDescription);
                if (customerData.CustomerType != 6)
                {
                    parameters.Add(key: "tax_code", value: customerData.TaxCode);
                }

                parameters.Add(key: "quality", value: ((short)paymentOrder.Quality).ToString());
                parameters.Add(key: "police_code", value: policeResponseDetailsId.ToString());


                response.Result = await _reportService.RenderReport($"/{_reportName}/Payment_order", parameters, ReportService.GetExportFormatEnumeration(exportFormat), "Payment_order");
            }
            else
            {
                paymentOrder.Description = "Հայտը կատարված չէ։";
            }
            response.ResultCode = ResultCodes.normal;
            return response;
        }


        public async Task<SingleResponse<byte[]>> TransfersReestrByPageStatement(long id)
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "doc_id", value: id.ToString());
            response.Result = await _reportService.RenderReport($"/{_reportName}/Transfers_Registry_By_Page", parameters, ExportFormat.PDF, "Transfers_Registry_By_Page");
            response.ResultCode = ResultCodes.normal;
            return response;
        }

        public async Task<SingleResponse<Byte[]>> TransfersReestrStatement(long id)
        {
            SingleResponse<Byte[]> response = new SingleResponse<byte[]>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "doc_id", value: id.ToString());
            string path = $"/{_reportName}/Transfers_Registry";
            string fileName = "Transfers_Registry";
            response.Result = await _reportService.RenderReport(path, parameters, ExportFormat.PDF, fileName);
            response.ResultCode = ResultCodes.normal;
            return response;
        }

        public async Task<SingleResponse<byte[]>> PeriodicTransfer(ulong appId)
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            int transferType;
            string reportPath;

            transferType = _xBService.GetTransferTypeByAppId(appId);
            if (transferType != 100)
            {
                reportPath = $"/{_reportName}/PaymentInstructionByPeriodFromAppID";
                parameters.Add("edit", "False");
            }
            else
            {
                reportPath = $"/{_reportName}/PaymentInstructionByPeriodSwiftExtractFromAppID";
            }

            parameters.Add("app_ID", appId.ToString());
            response.Result = await _reportService.RenderReport(reportPath, parameters, ExportFormat.PDF, "Periodic_Transfer");
            response.ResultCode = ResultCodes.normal;
            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintLoanStatement(ulong productId, DateTime dateFrom, DateTime dateTo, int lang, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            Loan loan = _xBService.GetLoan(productId);
            string account = loan.LoanAccount.AccountNumber.ToString();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "account", value: account);
            parameters.Add(key: "start_date", value: dateFrom.ToString("dd/MMM/yy"));
            parameters.Add(key: "end_date", value: dateTo.ToString("dd/MMM/yy"));
            parameters.Add(key: "filial_code", value: "22000");
            parameters.Add(key: "lang_id", value: lang.ToString());
            parameters.Add(key: "Product_id", value: productId.ToString());
            string path = $"/{_reportName}/Loan_Statment_NEW";
            string fileName = "LoanStatement";
            response.Result = await _reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName);
            response.ResultCode = ResultCodes.normal;
            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintCreditLineStatement(ulong productId, DateTime dateFrom, DateTime dateTo, int lang, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            CreditLine creditLine = _xBService.GetCreditLine(productId);
            string account = creditLine.LoanAccount.AccountNumber.ToString();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "account", value: account);
            parameters.Add(key: "start_date", value: dateFrom.ToString("dd/MMM/yy"));
            parameters.Add(key: "end_date", value: dateTo.ToString("dd/MMM/yy"));
            parameters.Add(key: "filial_code", value: "22000");
            parameters.Add(key: "lang_id", value: lang.ToString());
            string path = $"/{_reportName}/Loan_Statment";
            string fileName = "CreditLineStatement";
            response.Result = await _reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName);
            response.ResultCode = ResultCodes.normal;
            return response;
        }


        public SingleResponse<string> PrintCardAccountDetails(string cardNumber, string exportFormat = "pdf")
        {
            SingleResponse<string> response = new SingleResponse<string>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "cardNumber", value: cardNumber);
            parameters.Add(key: "fileName", value: exportFormat);
            string path = $"/{_reportName}/CardAccountDetails";
            string fileName = "CardAccountDetails";
            response.Result =  Convert.ToBase64String(_reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName).Result);
            response.ResultCode = ResultCodes.normal;
            return response;
        }

        internal async Task<SingleResponse<string>> PrintPOSStatement(string accountNumber, DateTime dateFrom, DateTime dateTo, string exportFormat, short filialCode)
        {
            SingleResponse<string> response = new SingleResponse<string>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "ArmNumber", value: accountNumber);
            parameters.Add(key: "startDate", value: dateFrom.ToString("dd/MMM/yy"));
            parameters.Add(key: "endDate", value: dateTo.ToString("dd/MMM/yy"));
            parameters.Add(key: "StatmentOption", value: "1");
            parameters.Add(key: "filialCode", value: filialCode.ToString());

            string path = $"/{_reportName}/POS_Statement_Report";
            string fileName = "POS_Statement_Report";
            response.Result = Convert.ToBase64String(_reportService.RenderReport(path, parameters, ReportService.GetExportFormatEnumeration(exportFormat), fileName).Result);
            response.ResultCode = ResultCodes.normal;
            return response;
        }

        public async Task<SingleResponse<byte[]>> PrintExchangeOrderStatement(long id, int lang, PaymentOrder paymentOrder, string exportFormat = "pdf")
        {
            SingleResponse<byte[]> response = new SingleResponse<byte[]>();
            byte convType = paymentOrder.SubType;
            string report = "";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            report = paymentOrder.SubType == 3 ? "Cross_Convertation_NonCash" : "Convertation_NonCash";
            parameters.Add(key: "User", value: "");
            parameters.Add(key: "nn", value: paymentOrder.Id.ToString());
            parameters.Add(key: "Filial_code", value: paymentOrder.FilialCode.ToString());
            parameters.Add(key: "customer_number", value: paymentOrder.CustomerNumber.ToString());
            parameters.Add(key: "purpose", value: paymentOrder.Description);
            parameters.Add(key: "customer_debit_account", value: paymentOrder.DebitAccount.AccountNumber);
            parameters.Add(key: "customer_credit_account", value: paymentOrder.ReceiverAccount.AccountNumber);
            if (convType != 3)
            {
                parameters.Add(key: "amount", value: convType == 2 ? paymentOrder.Amount.ToString() : (paymentOrder.Amount * paymentOrder.ConvertationRate).ToString());
                parameters.Add(key: "currency", value: paymentOrder.ReceiverAccount.Currency);
                parameters.Add(key: "exch_rate", value: paymentOrder.ConvertationRate.ToString());
                parameters.Add(key: "amount_exch", value: convType == 2 ? (paymentOrder.Amount * paymentOrder.ConvertationRate).ToString() : paymentOrder.Amount.ToString());  
                parameters.Add(key: "currency_exch", value: paymentOrder.DebitAccount.Currency);
                parameters.Add(key: "oper", value: convType == 2 ? "Առք" : "Վաճառք");
                parameters.Add(key: "fileName", value: "Convertation_NonCash");
                parameters.Add(key: "transaction_purpose", value: "Ներբանկային փոխարկման հայտ");
                parameters.Add(key: "transaction_purpose1", value: "Արտարժույթի առք ու վաճառքի գործառնությունների վերաբերյալ");
                parameters.Add(key: "Hb_quality", value: ((int)paymentOrder.Quality).ToString());
                if (!String.IsNullOrEmpty(paymentOrder.ConfirmationDate.ToString()))
                {
                    parameters.Add(key: "Hb_confirmation_date", value: paymentOrder.ConfirmationDate.Value.Date.ToString("dd/MM/yy"));
                }
            }
            else
            {
                parameters.Add(key: "amount_buy", value: paymentOrder.Amount.ToString());
                parameters.Add(key: "currency_buy", value: paymentOrder.ReceiverAccount.Currency);
                parameters.Add(key: "kurs_buy", value: paymentOrder.ConvertationRate.ToString());
                parameters.Add(key: "amount_sell", value: (paymentOrder.Amount * paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1).ToString());
                parameters.Add(key: "currency_sell", value: paymentOrder.ReceiverAccount.Currency);
                parameters.Add(key: "Kurs_sell", value: paymentOrder.ConvertationRate1.ToString());
                parameters.Add(key: "diff_inAMD", value: ((paymentOrder.Amount * paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1) - paymentOrder.Amount * paymentOrder.ConvertationRate1).ToString());
                parameters.Add(key: "cross_kurs", value: paymentOrder.ConvertationCrossRate.ToString());
                parameters.Add(key: "cred_acc_descr", value: " ");
                parameters.Add(key: "quality", value: paymentOrder.Quality.ToString());
                if (!String.IsNullOrEmpty(paymentOrder.ConfirmationDate.ToString()))
                {
                    parameters.Add(key: "confirm_date", value: paymentOrder.ConfirmationDate.Value.Date.ToString("dd/MM/yy"));
                }
                parameters.Add(key: "DocID", value: id.ToString());
            }
            parameters.Add(key: "Hb_Doc_ID", value: id.ToString());
            if (convType != 3)
            {
                parameters.Add(key: "Hb_time", value: paymentOrder.ConfirmationDate.Value.TimeOfDay.ToString());
                parameters.Add(key: "Hb_send_date", value: paymentOrder.ConfirmationDate.Value.Date.ToString("dd/MM/yy"));
            }
            else
            {
                parameters.Add(key: "ConfirmDateText", value: paymentOrder.ConfirmationDate.ToString() + ", " + paymentOrder.RegistrationTime);
                parameters.Add(key: "SentDate", value: paymentOrder.ConfirmationDate.Value.Date.ToString("dd/MM/yy"));
            }
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            CustomerMainData customerData = _xBService.GetCustomerMainData(authorizedCustomer.CustomerNumber);
            parameters.Add(key: "seria", value: _xBInfoService.GetCurrencyLetter(paymentOrder.ReceiverAccount.Currency, convType));
            Dictionary<string, string> customerDetails = new Dictionary<string, string>();
            customerDetails = _xBService.GetOrderDetailsForReport(id);
            parameters.Add(key: "Customer_Info", value: customerData.CustomerDescription);
            parameters.Add(key: "Customer_address", value: customerDetails["cust_adress"]);
            parameters.Add(key: "cust_pass", value: customerDetails["passport_number"] + " " + customerDetails["passport_inf"] + " " + customerDetails["passport_date"]);


            response.Result = await _reportService.RenderReport($"/{_reportName}/{report}", parameters, ReportService.GetExportFormatEnumeration(exportFormat), $"{report}");
            response.ResultCode = ResultCodes.normal;
            return response;
        }

    }
}
