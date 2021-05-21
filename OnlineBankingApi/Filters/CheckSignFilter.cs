using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingLibrary;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class CheckSignFilter : ActionFilterAttribute
    {
        private readonly ApprovalOrderType _type;
        private readonly XBService _xbService;
        private readonly XBSecurityService _xbSecurityService;
        private readonly CacheHelper _cacheHelper;
        private readonly IConfiguration _config;
        private string TransactionID = "", SenderAccount = "", RecepientAccount = "", Amount = "", IpAddress = "";
        public CheckSignFilter(XBService xbService, XBSecurityService xbSecurityService, CacheHelper cacheHelper, ApprovalOrderType type, IConfiguration config)
        {
            _xbService = xbService;
            _xbSecurityService = xbSecurityService;
            _type = type;
            _cacheHelper = cacheHelper;
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ApproveIdRequest request = null;
            ProductIdApproveRequest productIdRequest = null;
            ApproveLoanProductOrderRequest approveLoan = null;
            ListDocIdRequest listRequest = null;
            Dictionary<long, ApprovalOrderType> Types = new Dictionary<long, ApprovalOrderType>();
            string sessionId = "";
            string otp = "";
            byte language = 0;
            bool isSigned = false;
            string ipAddress="";
            Dictionary<string, string> signData = null;
            SourceType sourceType = SourceType.NotSpecified;

            // հայտի մուտքագրման աղբյուրի ստացում Header-ից
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["SourceType"]))
                Enum.TryParse(context.HttpContext.Request.Headers["SourceType"], out sourceType);


            // Սեսիայի ստացում Header-ից
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["SessionId"]))
                sessionId = context.HttpContext.Request.Headers["SessionId"];


            // Լեզվի ստացում Header-ից
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["language"]))
                byte.TryParse(context.HttpContext.Request.Headers["language"], out language);

            // IP հասցեի ստացում
            if (!string.IsNullOrEmpty(context.HttpContext.Request.Headers["LocalIPAddress"]))
                ipAddress = context.HttpContext.Request.Headers["LocalIPAddress"];

            // Փոխանցված պարամետրի ստացում
            var argument = context.ActionArguments.Values.First();

            //Approve մեթոդների համար
            if (argument is ApproveIdRequest)
            {
                request = argument as ApproveIdRequest;
                Types.Add(request.Id, _type);
                otp = request.OTP;
            }
            //ApproveOrders մեթոդի համար
            else if (argument is ListDocIdRequest)
            {
                listRequest = argument as ListDocIdRequest;
                foreach (var item in listRequest.ListDocId)
                {
                    Types.Add(item, GetOrderType(_xbService.GetDocumentType(item)));
                }
                otp = listRequest.OTP;
            }
            else if (argument is ProductIdApproveRequest)
            {
                productIdRequest = argument as ProductIdApproveRequest;
                Types.Add((long)productIdRequest.ProductId, _type);
                otp = productIdRequest.OTP;
            }
            else if(argument is ApproveLoanProductOrderRequest)
            {
                approveLoan = argument as ApproveLoanProductOrderRequest;
                Types.Add(approveLoan.Id, _type);
                otp = approveLoan.OTP;
            }

            //Հայտի ստեղծում, քեշավորում, և Sign լինող պարամետրերի փոխանցում
            foreach (var x in Types)
            {
                switch (x.Value)
                {
                    case ApprovalOrderType.PaymentOrder:
                        {
                            PaymentOrder order = (PaymentOrder)_cacheHelper.SetApprovalOrder(_xbService.GetPaymentOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitAccount.AccountNumber.ToString(), order.ReceiverAccount.AccountNumber.ToString(),
                                Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.PlasticCardOrder:
                        {
                            PlasticCardOrder order = (PlasticCardOrder)_cacheHelper.SetApprovalOrder(_xbService.GetPlasticCardOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.PlasticCard.Currency, ipAddress);
                        }
                        break;
                    case ApprovalOrderType.UtilityPaymentOrder:
                        {
                            UtilityPaymentOrder order = (UtilityPaymentOrder)_cacheHelper.SetApprovalOrder(_xbService.GetUtilityPaymentOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitAccount.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.MatureOrder:
                        {
                            MatureOrder order = (MatureOrder)_cacheHelper.SetApprovalOrder(_xbService.GetMatureOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.Account.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.ReferenceOrder:
                        {
                            ReferenceOrder order = (ReferenceOrder)_cacheHelper.SetApprovalOrder(_xbService.GetReferenceOrder(x.Key));
                            if (order.FeeAccount != null)
                                CollectParameters(order.Id.ToString(), order.FeeAccount.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                            else
                                CollectParameters(order.Id.ToString(), "0", "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.LoanProductOrder:
                        {
                            LoanProductOrder order = null;
                            var type = _xbService.GetDocumentType((int)x.Key);
                            switch (type)
                            {
                                case OrderType.CreditSecureDeposit:
                                    order = (LoanProductOrder)_cacheHelper.SetApprovalOrder(_xbService.GetLoanOrder(x.Key));
                                    break;
                                default:
                                    order = (LoanProductOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCreditLineOrder(x.Key));
                                    break;
                            }
                            CollectParameters(order.Id.ToString(), "0", "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.ReceivedFastTransferPaymentOrder:
                        {
                            ReceivedFastTransferPaymentOrder order = (ReceivedFastTransferPaymentOrder)_cacheHelper.SetApprovalOrder(_xbService.GetReceivedFastTransferPaymentOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", order.ReceiverAccount.AccountNumber.ToString(), Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.AccountClosingOrder:
                        {
                            AccountClosingOrder order = (AccountClosingOrder)_cacheHelper.SetApprovalOrder(_xbService.GetAccountClosingOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", "0", ipAddress);
                        }
                        break;
                    case ApprovalOrderType.SwiftCopyOrder:
                        {
                            SwiftCopyOrder order = (SwiftCopyOrder)_cacheHelper.SetApprovalOrder(_xbService.GetSwiftCopyOrder(x.Key));
                            if (order.FeeAccount != null)
                                CollectParameters(order.Id.ToString(), order.FeeAccount.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                            else
                                CollectParameters(order.Id.ToString(), "0", "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CredentialOrder:
                        {
                            CredentialOrder order = (CredentialOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCredentialOrder(x.Key));
                            if (order.Fees != null && order.Fees[0] != null && order.Fees[0].Account != null)
                                CollectParameters(order.Id.ToString(), order.Fees[0].Account.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                            else
                                CollectParameters(order.Id.ToString(), "0", "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.DepositOrder:
                        {
                            DepositOrder order = (DepositOrder)_cacheHelper.SetApprovalOrder(_xbService.GetDepositorder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitAccount.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.AccountOrder:
                        {
                            AccountOrder order = (AccountOrder)_cacheHelper.SetApprovalOrder(_xbService.GetAccountOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.Currency, ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CashOrder:
                        {
                            CashOrder order = (CashOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCashOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CreditLineTerminationOrder:
                        {
                            CreditLineTerminationOrder order = (CreditLineTerminationOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCreditLineTerminationOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.Currency, ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CardClosingOrder:
                        {
                            CardClosingOrder order = (CardClosingOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCardClosingOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.ProductId.ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CustomerDataOrder:
                        {
                            CustomerDataOrder order = (CustomerDataOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCustomerDataOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.Password, ipAddress);
                        }
                        break;
                    case ApprovalOrderType.StatmentByEmailOrder:
                        {
                            StatmentByEmailOrder order = (StatmentByEmailOrder)_cacheHelper.SetApprovalOrder(_xbService.GetStatmentByEmailOrder(x.Key));
                        }
                        break;
                    case ApprovalOrderType.DepositTerminationOrder:
                        {
                            DepositTerminationOrder order = (DepositTerminationOrder)_cacheHelper.SetApprovalOrder(_xbService.GetDepositTerminationOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.ProductId.ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.ReestrTransferOrder:
                        {
                            ReestrTransferOrder order = (ReestrTransferOrder)_cacheHelper.SetApprovalOrder(_xbService.GetReestrTransferOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitAccount.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.ArcaCardsTransactionOrder:
                        {
                            ArcaCardsTransactionOrder order = (ArcaCardsTransactionOrder)_cacheHelper.SetApprovalOrder(_xbService.GetArcaCardsTransactionOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.CardNumber.Substring(0,10), "0", "0", ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CardToCardOrder:
                        {
                            CardToCardOrder order = (CardToCardOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCardToCardOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitCardNumber.Substring(0,10), order.CreditCardNumber, Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CardLimitChangeOrder:
                        {
                            CardLimitChangeOrder order = (CardLimitChangeOrder)_cacheHelper.SetApprovalOrder(_xbService.GetCardLimitChangeOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", Math.Truncate(order.Limits[0].LimitValue).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.PeriodicPaymentOrder:
                        {
                            PaymentOrder order = (PaymentOrder)_cacheHelper.SetApprovalOrder(_xbService.GetPaymentOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitAccount.AccountNumber.ToString(), "0", Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.InternationalPaymentOrder:
                        {
                            InternationalPaymentOrder order = (InternationalPaymentOrder)_cacheHelper.SetApprovalOrder(_xbService.GetInternationalPaymentOrder(x.Key));
                            CollectParameters(order.Id.ToString(), order.DebitAccount.AccountNumber.ToString(), order.ReceiverAccount.AccountNumber.ToString(),
                                Math.Truncate(order.Amount).ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.AccountReOpenOrder:
                        {
                            AccountReOpenOrder order = (AccountReOpenOrder)_cacheHelper.SetApprovalOrder(_xbService.GetAccountReOpenOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", "0", ipAddress);
                        }
                        break;
                    case ApprovalOrderType.PlasticCardSmsServiceOrder:
                        {
                            PlasticCardSMSServiceOrder order = (PlasticCardSMSServiceOrder)_cacheHelper.SetApprovalOrder(_xbService.GetPlasticCardSMSServiceOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.ProductID.ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.RemovalOrder:
                        {
                            RemovalOrder order = context.ActionArguments.Values.First() as RemovalOrder;
                            CollectParameters("0", order.RemovingOrderId.ToString(), "0", "0", ipAddress);
                        }
                        break;
                    case ApprovalOrderType.PeriodicTerminationOrder:
                        {
                            PeriodicTerminationOrder order = (PeriodicTerminationOrder)_cacheHelper.SetApprovalOrder(_xbService.GetPeriodicTerminationOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.ProductId.ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.PeriodicDataChangeOrder:
                        {
                            PeriodicTransferDataChangeOrder order = (PeriodicTransferDataChangeOrder)_cacheHelper.SetApprovalOrder(_xbService.GetPeriodicDataChangeOrder(x.Key));
                            CollectParameters(order.Id.ToString(), "0", "0", order.ProductId.ToString(), ipAddress);
                        }
                        break;
                    case ApprovalOrderType.CardActivationOrder:
                        {
                            CollectParameters(x.Key.ToString(), "0", "0", "0", ipAddress);
                        }
                        break;
                    default:
                        break;
                }
            };

            //CheckSign Filter-ն անհրաժեշտ է աշխատի միայն sourceType-ը 5-ի՝ MobileBanking-ի դեպքում
            if (sourceType != SourceType.MobileBanking)
            {
                return;
            }
            else
            {
                signData = this.GenerateSignData(TransactionID, SenderAccount, RecepientAccount, Amount, IpAddress);
            }

            isSigned = _xbSecurityService.SingData(sessionId, otp, signData, language);

            //թեստային միջավայրի համար
            if ((sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de" || otp == "0123456") && Convert.ToBoolean(_config["TestVersion"]))
            {
                return;
            }
            if (!isSigned)
            {
                Response response = new Response();
                response.ResultCode = ResultCodes.validationError;
                response.Description = (Languages)language == Languages.hy ? "Սխալ PIN կոդ։" : "Incorrect PIN code.";
                context.Result = ResponseExtensions.ToHttpResponse(response);
            }
        }

        /// <summary>
        /// Sign լինող տվյալների հավաքագրում Dictionary-ի մեջ
        /// </summary>
        /// <param name="TransactionID">գորժարքի կոդ</param>
        /// <param name="SenderAccount">ուղարկողի հաշվեհամար</param>
        /// <param name="RecepientAccount"> ստացողի հաշվեհամար</param>
        /// <param name="Amount">գումար</param>
        /// <param name="IpAddress">IP հասցե</param>
        /// <returns></returns>
        private Dictionary<string, string> GenerateSignData(string TransactionID, string SenderAccount, string RecepientAccount, string Amount, string IpAddress)
        {
            var signData = new Dictionary<string, string>();
            signData.Add(nameof(TransactionID), TransactionID);
            signData.Add(nameof(SenderAccount), SenderAccount);
            signData.Add(nameof(RecepientAccount), RecepientAccount);
            signData.Add(nameof(Amount), Amount);
            signData.Add(nameof(IpAddress), IpAddress);
            return signData;
        }

        /// <summary>
        /// XBS-ի Հայտի տեսակի (OrderType) համատեղում OnlineBankingApi-ի հայտի տեսակի (ApprovalOrderType) հետ
        /// </summary>
        /// <param name="type">XBS -ի Հայտի տեսակ</param>
        /// <returns></returns>
        private ApprovalOrderType GetOrderType(OrderType type)
        {
            switch (type)
            {
                case OrderType.RATransfer:
                    return ApprovalOrderType.PaymentOrder;
                case OrderType.Convertation:
                    return ApprovalOrderType.PaymentOrder;
                case OrderType.InternationalTransfer:
                    return ApprovalOrderType.InternationalPaymentOrder;
                case OrderType.DepositTermination:
                    return ApprovalOrderType.DepositTerminationOrder;
                case OrderType.LoanMature:
                    return ApprovalOrderType.MatureOrder;
                case OrderType.CurrentAccountOpen:
                    return ApprovalOrderType.AccountOrder;
                case OrderType.Deposit:
                    return ApprovalOrderType.DepositOrder;
                case OrderType.PeriodicTransfer:
                    return ApprovalOrderType.PeriodicPaymentOrder;
                case OrderType.CurrentAccountReOpen:
                    return ApprovalOrderType.AccountReOpenOrder;
                case OrderType.CreditSecureDeposit:
                    return ApprovalOrderType.LoanProductOrder;
                case OrderType.CommunalPayment:
                    return ApprovalOrderType.UtilityPaymentOrder;
                case OrderType.CancelTransaction:
                    return ApprovalOrderType.RemovalOrder;
                case OrderType.RemoveTransaction:
                    return ApprovalOrderType.RemovalOrder;
                case OrderType.ReferenceOrder:
                    return ApprovalOrderType.ReferenceOrder;
                case OrderType.CreditLineMature:
                    return ApprovalOrderType.CreditLineTerminationOrder;
                case OrderType.CashOrder:
                    return ApprovalOrderType.CashOrder;
                case OrderType.StatmentByEmailOrder:
                    return ApprovalOrderType.StatmentByEmailOrder;
                case OrderType.SwiftCopyOrder:
                    return ApprovalOrderType.SwiftCopyOrder;
                case OrderType.CustomerDataOrder:
                    return ApprovalOrderType.CustomerDataOrder;
                case OrderType.CurrentAccountClose:
                    return ApprovalOrderType.AccountClosingOrder;
                case OrderType.CardClosing:
                    return ApprovalOrderType.CardClosingOrder;
                case OrderType.CredentialOrder:
                    return ApprovalOrderType.CredentialOrder;
                case OrderType.ReceivedFastTransferPaymentOrder:
                    return ApprovalOrderType.ReceivedFastTransferPaymentOrder;
                case OrderType.ArcaCardsTransactionOrder:
                    return ApprovalOrderType.ArcaCardsTransactionOrder;
                case OrderType.CardLimitChangeOrder:
                    return ApprovalOrderType.CardLimitChangeOrder;
                case OrderType.CardToCardOrder:
                    return ApprovalOrderType.CardToCardOrder;
                case OrderType.PlasticCardOrder:
                    return ApprovalOrderType.PlasticCardOrder;
                case OrderType.AttachedPlasticCardOrder:
                    return ApprovalOrderType.PlasticCardOrder;
                case OrderType.LinkedPlasticCardOrder:
                    return ApprovalOrderType.PlasticCardOrder;
                case OrderType.PlasticCardSMSServiceOrder:
                    return ApprovalOrderType.PlasticCardSmsServiceOrder;
                case OrderType.PeriodicTransferStop:
                    return ApprovalOrderType.PeriodicTerminationOrder;
                case OrderType.PeriodicTransferDataChangeOrder:
                    return ApprovalOrderType.PeriodicDataChangeOrder;
                case OrderType.ReestrTransferOrder:
                    return ApprovalOrderType.ReestrTransferOrder;
            }
            return ApprovalOrderType.NotDefined;
        }


        /// <summary>
        /// GenerateSignData մեթոդի համար անհրաժեշտ պարամետրերի հավաքում / կազմում
        /// </summary>
        /// <param name="transactionID">գորժարքի կոդ</param>
        /// <param name="senderAccount">ուղարկողի հաշվեհամար</param>
        /// <param name="recepientAccount"> ստացողի հաշվեհամար</param>
        /// <param name="amount">գումար</param>
        /// <param name="ipAddress">IP հասցե</param>
        private void CollectParameters(string transactionID, string senderAccount, string recepientAccount, string amount, string ipAddress)
        {
            TransactionID += transactionID;
            SenderAccount += senderAccount;
            RecepientAccount += recepientAccount;
            Amount += amount;
            IpAddress += ipAddress;
        }
    }
}
