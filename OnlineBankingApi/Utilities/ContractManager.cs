using ContractServiceRef;
using Microsoft.AspNetCore.Http;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Utilities
{
    public class ContractManager
    {
        private readonly XBService _xBService;
        private readonly XBInfoService _xBInfoService;
        private readonly ContractService _contractService;
        private readonly CacheHelper _cacheHelper;

        public ContractManager(XBService xBService, XBInfoService xBInfoService, ContractService contractService, CacheHelper cacheHelper)
        {
            _xBService = xBService;
            _xBInfoService = xBInfoService;
            _contractService = contractService;
            _cacheHelper = cacheHelper;
        }
        public string GetLoansDramContract(long docId, int productType, bool fromApprove, ulong customerNumber)
        {
            string result = null;
            result = _xBService.GetLoansDramContract(docId,productType,fromApprove,customerNumber);
            return result;
        }

        public string PrintFastOverdraftContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            string result = null;
            LoanProductOrder productOrder = _xBService.GetCreditLineOrder(docId);
            Card card = _xBService.GetCardWithOutBallance(productOrder.ProductAccount.AccountNumber);
            Dictionary<string, string> contractInfo = _xBService.GetDepositCreditLineContractInfo((int)docId).ToDictionary(x => x.Key, x => x.Value);
          
            CreditLinePrecontractData precontractData = _xBService.GetCreditLinePrecontractData(productOrder.StartDate, productOrder.EndDate, productOrder.InterestRate,
                0, card.CardNumber, productOrder.Currency, productOrder.Amount, 8);
            short filialCode = _xBService.GetCustomerFilial();
            double penaltyRate = _xBInfoService.GetPenaltyRateOfLoans(54, productOrder.StartDate);
            string contractName = "CreditLineContract";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumberHB", value: customerNumber.ToString());

            Contract contract = null;
            if (fromApprove == true)
            {
                contract = new Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 12;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }

            parameters.Add(key: "appID", value: "0");
            parameters.Add(key: "bln_with_enddate", value: "True");
            parameters.Add(key: "visaNumberHB", value: card.CardNumber);
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "dateOfBeginningHB", value: productOrder.StartDate.ToString("dd/MMM/yy"));


            parameters.Add(key: "currencyHB", value: productOrder.Currency);
            parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
            parameters.Add(key: "startCapitalHB", value: productOrder.Amount.ToString());
            parameters.Add(key: "clientTypeHB", value: "");
            parameters.Add(key: "filialCodeHB", value: filialCode.ToString());
            parameters.Add(key: "creditLineTypeHB", value: "54");
            parameters.Add(key: "provisionNumberHB", value: "01");
            parameters.Add(key: "interestRateHB", value: productOrder.InterestRate.ToString());
            parameters.Add(key: "securityCodeHB", value: contractInfo["security_code_2"].ToString());

            parameters.Add(key: "loanProvisionPercentHB", value: "0");

            parameters.Add(key: "interestRateFullHB", value: (precontractData.InterestRate / 100).ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: productOrder.ProductAccount.AccountNumber);
            parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFee", value: precontractData.InterestRateEffectiveWithoutAccountServiceFee.ToString());
            parameters.Add(key: "dateOfNormalEndHB", value: productOrder.EndDate.ToString("dd/MMM/yy"));
            parameters.Add(key: "RepaymentKurs", value: contractInfo["repayment_kurs"].ToString() == null ? "0" : contractInfo["repayment_kurs"].ToString());

            result = _contractService.RenderContract(contractName, parameters, "FastOverdraftContract.pdf",contract);

            //File.WriteAllBytes(@"C:\YourPDFLoan.pdf", result);
            return result;

        }
        
        public string PrintDepositLoanContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            string result = null;
            result = _xBService.PrintDepositLoanContract(docId, customerNumber, fromApprove);

            return result;
        }

        public string PrintDepositCreditLineContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            string result = null;
            LoanProductOrder loanProductOrder = _xBService.GetCreditLineOrder(docId);
            Card card = _xBService.GetCardWithOutBallance(loanProductOrder.ProductAccount.AccountNumber);
            CreditLinePrecontractData preContractDate = _xBService.GetCreditLinePrecontractData(loanProductOrder.StartDate, 
                (loanProductOrder.ProductType == 51 || loanProductOrder.ProductType == 50) ? loanProductOrder.ValidationDate.Value : loanProductOrder.EndDate,
                loanProductOrder.InterestRate, 0, card.CardNumber, loanProductOrder.Currency, loanProductOrder.Amount, 8);
            Dictionary<string, string> contractInfo = _xBService.GetDepositCreditLineContractInfo((int)docId).ToDictionary(x => x.Key, x => x.Value);
            short fillialCode = _xBService.GetCustomerFilial();
            int cardType = _xBService.GetCardType(card.CardNumber);
            double kursForLoan = _xBService.GetCBKursForDate(loanProductOrder.StartDate, loanProductOrder.Currency);
            double kursForProvision = _xBService.GetCBKursForDate(loanProductOrder.StartDate, loanProductOrder.PledgeCurrency);
            double decreasingAmount = _xBService.GetCreditLineDecreasingAmount(loanProductOrder.Amount, loanProductOrder.Currency,
                loanProductOrder.StartDate, (loanProductOrder.ProductType == 51 || loanProductOrder.ProductType == 50) ? loanProductOrder.ValidationDate.Value : loanProductOrder.EndDate);
            double penaltyRate = _xBInfoService.GetPenaltyRateOfLoans(30, loanProductOrder.StartDate);

            string contractName = string.Empty;


            if (cardType != 40)
            {
                contractName = "CreditLineContract";
            }
            else
            {
                contractName = "CreditLineAmex";
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumberHB", value: customerNumber.ToString());
            Contract contract = null;
            if (fromApprove == true)
            {
                contract = new Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 12;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }


            parameters.Add(key: "appID", value: "0");
            parameters.Add(key: "bln_with_enddate", value: "True");
            parameters.Add(key: "visaNumberHB", value: card.CardNumber);
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "dateOfBeginningHB", value: loanProductOrder.StartDate.ToString("MM/dd/yyyy"));
            parameters.Add(key: "currencyHB", value: loanProductOrder.Currency);
            parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
            parameters.Add(key: "startCapitalHB", value: loanProductOrder.Amount.ToString());
            parameters.Add(key: "provisionNumberHB", value: "01");
            parameters.Add(key: "clientTypeHB", value: "");
            parameters.Add(key: "filialCodeHB", value: fillialCode.ToString());
            parameters.Add(key: "creditLineTypeHB", value: loanProductOrder.ProductType.ToString());

            parameters.Add(key: "securityCodeHB", value: contractInfo["security_code_2"].ToString());
            parameters.Add(key: "loanProvisionPercentHB", value: ((loanProductOrder.Amount * kursForLoan) / (loanProductOrder.ProvisionAmount * kursForProvision) * 100).ToString());
            parameters.Add(key: "interestRateHB", value: loanProductOrder.InterestRate.ToString());


            parameters.Add(key: "interestRateFullHB", value: (preContractDate.InterestRate / 100).ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: loanProductOrder.ProductAccount.AccountNumber.ToString());
            parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFeeHB", value: preContractDate.InterestRateEffectiveWithoutAccountServiceFee.ToString());
            if (cardType == 40)
            {
                parameters.Add(key: "contractPersonCountHB", value: "2");

            }

            parameters.Add(key: "repaymentPercentHB", value: contractInfo["repayment_percent"].ToString());
            parameters.Add(key: "RepaymentKurs", value: contractInfo["repayment_kurs"].ToString() == null ? "0" : contractInfo["repayment_kurs"].ToString());
            parameters.Add(key: "decrAmountHB", value: loanProductOrder.ProductType == 50 ? decreasingAmount.ToString() : "0");
            
            result = _contractService.RenderContract(contractName, parameters, contractName + ".pdf", contract);
            return result;
        }


        /// <summary>
        /// Էական պայմանների անհատական թերթիկի ստացում byte[] ֆորմատով
        /// </summary>
        /// <param name="loanType"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        private string GetLoanTermsSheetContent(byte loanType, long orderid)
        {
            string fileContent = null;

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string contractName = "";
            if (loanType == 1) contractName = "LoanTerms";
            else if (loanType == 2 || loanType == 3) contractName = "CreditLineTerms";

            parameters.Add(key: "hbDocID", value: orderid.ToString());
            parameters.Add(key: "HB", value: "True");

            fileContent = _contractService.RenderContract(contractName, parameters, "PrintLoanTermsSheet");

            return fileContent;
        }

        public string PrintLoanTermsSheetBase64(byte loanType, long orderid)
        {
            string content = null;
            content = GetLoanTermsSheetContent(loanType, orderid);
            return content;
        }

        public string GetCardToCardReceipt(long id)
        {
            string result = null;
            SingleResponse<byte[]> depositOrder = new SingleResponse<byte[]>();
            CardToCardOrder order = _xBService.GetCardToCardOrder(id);
            string customerFullName = _xBService.GetEmbossingName(order.DebitCardNumber);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "registrationDate", value: order.RegistrationDate.ToString("MM/dd/yyyy HH:mm")); //order.RegistrationDate.ToString()
            parameters.Add(key: "transferId", value: order.OrderId.ToString());
            parameters.Add(key: "docId", value: order.Id.ToString());
            parameters.Add(key: "debitCardNumber", value: order.DebitCardNumber.ToString());
            parameters.Add(key: "rrn", value: order.RRN.ToString());
            parameters.Add(key: "authid", value: order.AuthId.ToString());
            parameters.Add(key: "creditCardNumber", value: order.CreditCardNumber.ToString());
            parameters.Add(key: "customerFullName", value: customerFullName.ToString());
            parameters.Add(key: "embossingName", value: order.EmbossingName.ToString());
            parameters.Add(key: "amount", value: order.Amount.ToString() + " " + order.Currency);
            parameters.Add(key: "feeAmount", value: order.FeeAmount.ToString() + " " + order.Currency);
            parameters.Add(key: "totalAmount", value: (order.FeeAmount + order.Amount).ToString() + " " + order.Currency);


            result = _contractService.RenderContract("CardToCardReceipt", parameters, "CardToCardReceipt.pdf");
            return result;
        }

        public string PrintLoanTermsSheet(byte loanType, long orderid)
        {
            string fileContent = null;
            fileContent = GetLoanTermsSheetContent(loanType, orderid);
            return fileContent;
        }



        public string GetCurrentAccountContractBefore(long docId)
        {
            string result = null;
            AccountOrder order = new AccountOrder();
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            int fillialCode = _cacheHelper.GetAuthorizedCustomer().BranchCode;
            order = _xBService.GetAccountOrder(docId);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumber", value: authorizedCustomer.CustomerNumber.ToString());
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "currencyHB", value: order.Currency);
            parameters.Add(key: "reopen", value: "0");
            parameters.Add(key: "armNumber", value: "0");
            parameters.Add(key: "armNumberStr", value: "0");
            parameters.Add(key: "accountTypeHB", value: (order.AccountType - 1).ToString());
            parameters.Add(key: "thirdPersonCustomerNumberHB", value: "0");
            parameters.Add(key: "filialCode", value: fillialCode.ToString());
            parameters.Add(key: "receiveTypeHB", value: order.StatementDeliveryType.ToString());
            result = _contractService.RenderContract("CurrentAccContract", parameters, "CurrentAccContract.pdf");
            return result;
        }

        public string GetExistingDepositContract(ulong appID)
        {
            string result = null;
            Deposit deposit  = _xBService.GetDeposit(appID);
            result = Convert.ToBase64String(_xBService.GetExistingDepositContract(deposit.DocID, 1));
            return result;
        }

        internal string PrintVisaVirtualCardCondition() 
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumber", value: authorizedCustomer.CustomerNumber.ToString());
            return _contractService.RenderContract("VisaVirtualCardCondition", parameters, "VisaVirtualCardCondition.pdf");
        }  
    }
}
