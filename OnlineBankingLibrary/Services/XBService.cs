using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using OnlineBankingLibrary.Models;
using XBS;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using OnlineBankingLibrary.Utilities;
using System.Threading.Tasks;

namespace OnlineBankingLibrary.Services
{
    public class XBService
    {
        private readonly IConfiguration _config;
        private readonly XBInfoService _xbInfoService;
        private readonly CacheHelper _cacheHelper;

        public XBService(XBInfoService xbInfoService, IConfiguration config, CacheHelper cacheHelper)
        {
            _config = config;
            _xbInfoService = xbInfoService;
            _cacheHelper = cacheHelper;
        }

        public void Use(Action<IXBService> action)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxBufferPoolSize = 5242880;
            binding.MaxBufferSize = 6553600;
            binding.MaxReceivedMessageSize = 6553600;
            binding.ReaderQuotas.MaxArrayLength = 2500000;
            string endpointUrl = _config["WCFExternalServices:XBService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            IXBService client = ProxyManager<IXBService>.GetProxy(nameof(IXBService), binding, endpoint);
            User user = _cacheHelper.GetUser();
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            byte language = _cacheHelper.GetLanguage();
            string clientIP = _cacheHelper.GetClientIp();

            client.SetUserAsync(authorizedCustomer, language, clientIP, user, _cacheHelper.GetSourceType()).Wait();
            bool success = false;
            try
            {
                action(client);
                ((IClientChannel)client).Close();
                success = true;
            }
            catch (FaultException)
            {
                ((IClientChannel)client).Close();
                throw;
            }
            catch (TimeoutException)
            {

            }
            catch (Exception ex)
            {
                ((IClientChannel)client).Abort();
                throw;
            }
            finally
            {
                if (!success)
                {
                    ((IClientChannel)client).Abort();
                }

                ((IClientChannel)client).Close();
                ((IClientChannel)client).Dispose();
            }
        }

        public bool IsAbleToChangeQuality(string userName, int id)
        {
            bool result = false;
            this.Use(client => { result = client.IsAbleToChangeQualityAsync(userName, id).Result; }
            );
            return result;
        }

        public AuthorizedCustomer GetTestMobileBankingUser()
        {
            var authorizedCustomer = new AuthorizedCustomer();
            NetTcpBinding binding = new NetTcpBinding();
            string endpointUrl = _config["WCFExternalServices:XBService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            XBServiceClient proxy = new XBServiceClient(binding, endpoint);
            authorizedCustomer = proxy.GetTestMobileBankingUserAsync().Result;
            return authorizedCustomer;
        }



        public string ValidateSearchData(SearchCommunal searchCommunal)
        {

            string errorDescription = "";

            if ((searchCommunal.CommunalType == XBS.CommunalTypes.ArmenTel || searchCommunal.CommunalType == XBS.CommunalTypes.VivaCell || searchCommunal.CommunalType == XBS.CommunalTypes.Orange) && searchCommunal.PhoneNumber.Length != 8)
            {
                errorDescription = "Հեռախոսահամարի երկարությունը պետք է լինի 8 նիշ:";

            }
            else if (searchCommunal.CommunalType == XBS.CommunalTypes.UCom && (searchCommunal.AbonentNumber.Length < 7 || searchCommunal.AbonentNumber.Length > 8))
            {
                errorDescription = "Պայմանագրի համարի երկարությունը պետք է լինի 7 կամ 8 նիշ:";

            }
            else if (searchCommunal.CommunalType == XBS.CommunalTypes.COWater)
            {
                if (string.IsNullOrEmpty(searchCommunal.Branch))
                {
                    errorDescription = "Ընտրեք մասնաճյուղը:";

                }
                else if (string.IsNullOrEmpty(searchCommunal.AbonentNumber) && string.IsNullOrEmpty(searchCommunal.Name))
                {
                    errorDescription = "Լրացրեք ջրօգտ.կոդը կամ Անուն Ազգանունը:";

                }


            }
            return errorDescription;
        }

        public List<Account> GetAccounts()
        {
            List<Account> result = new List<Account>();
            this.Use(client => { result = client.GetAccountsAsync().Result; }
            );
            return result;
        }

        public Account GetAccount(string accountNumber)
        {
            Account result = new Account();
            this.Use(client => { result = client.GetCurrentAccountAsync(accountNumber).Result; }
            );
            return result;
        }

        public List<Account> GetCurrentAccounts(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Account> result = new List<Account>();
            this.Use(client => { result = client.GetCurrentAccountsAsync(filter).Result; }
            );
            return result;
        }

        public List<Card> GetCards(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Card> result = new List<Card>();
            if(filter == ProductQualityFilter.Closed)
            {
                this.Use(client => result = client.GetClosedCardsForDigitalBankingAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
            }
            else
            {
                this.Use(client => {
                    result = client.GetCardsAsync(filter).Result;
                    if (filter == ProductQualityFilter.All)
                    {
                        result.RemoveAll(m => m.ClosingDate != null);
                        result.AddRange(client.GetClosedCardsForDigitalBankingAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
                        List<Card> notActivatedVirtualCards = client.GetNotActivatedVirtualCardsAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result;
                        notActivatedVirtualCards.ForEach(m => m.CardNumber = null);
                        result.AddRange(notActivatedVirtualCards);
                    }
                }); 
                
            }
            return result;
        }

        public Card GetCard(ulong productId)
        {
            Card result = new Card();
            this.Use(client => { result = client.GetCardAsync(productId).Result; }
            );
            return result;
        }

        public List<Deposit> GetDeposits(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Deposit> result = new List<Deposit>();
            this.Use(client => { result = client.GetDepositsAsync(filter).Result; }
            );
            return result;
        }

        public Deposit GetDeposit(ulong productId)
        {
            Deposit result = new Deposit();
            this.Use(client => { result = client.GetDepositAsync(productId).Result; }
            );
            return result;
        }

        public List<Loan> GetLoans(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Loan> result = new List<Loan>();
            this.Use(client => { result = client.GetLoansAsync(filter).Result; }
            );
            return result;
        }

        public Loan GetLoan(ulong productId)
        {
            Loan result = new Loan();
            this.Use(client => { result = client.GetLoanAsync(productId).Result; }
            );
            return result;
        }

        public List<PeriodicTransfer> GetPeriodicTransfers(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<PeriodicTransfer> result = new List<PeriodicTransfer>();
            this.Use(client => { result = client.GetPeriodicTransfersAsync(filter).Result; }
            );
            return result;
        }

        public PeriodicTransfer GetPeriodicTransfer(ulong productId)
        {
            PeriodicTransfer result = new PeriodicTransfer();
            this.Use(client => { result = client.GetPeriodicTransferAsync(productId).Result; }
            );
            return result;
        }

        public CardStatement GetCardStatement(string cardNumber, DateTime dateFrom, DateTime dateTo,
            double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0,
            short orderByAscDesc = 0)
        {
            CardStatement result = new CardStatement();
            this.Use(client =>
            {
                result = client.GetCardStatementAsync(cardNumber, dateFrom, dateTo, minAmount, maxAmount, debCred,
                    transactionsCount, orderByAscDesc).Result;
            }
            );

            if (minAmount == -1 && maxAmount == -1 && orderByAscDesc == 0)
            {
                result?.Transactions?.Sort((x, y) => y.OperationDate.CompareTo(x.OperationDate));

            }
            else if (orderByAscDesc == 1) //գումարի աճման
            {
                result?.Transactions?.Sort((x, y) => x.Amount.CompareTo(y.Amount));
            }
            else if (orderByAscDesc == 2) //գումարի նվազման
            {
                result?.Transactions?.Sort((x, y) => y.Amount.CompareTo(x.Amount));
            }
            return result;
        }

        public AccountStatement GetAccountStatement(string accountNumber, DateTime dateFrom, DateTime dateTo,
            double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0,
            short orderByAscDesc = 0)
        {
            AccountStatement result = new AccountStatement();

            this.Use(client =>
            {
                result = client.AccountStatementAsync(accountNumber, dateFrom, dateTo, minAmount, maxAmount,
                    debCred, transactionsCount, orderByAscDesc).Result;
            }
            );

            if (minAmount == -1 && maxAmount == -1 && orderByAscDesc == 0)
            {
                result?.Transactions?.Sort((x, y) => y.TransactionDate.CompareTo(x.TransactionDate));

            }
            else if (orderByAscDesc == 1) //գումարի աճման
            {
                result?.Transactions?.Sort((x, y) => x.Amount.CompareTo(y.Amount));
            }
            else if (orderByAscDesc == 2) //գումարի նվազման
            {
                result?.Transactions?.Sort((x, y) => y.Amount.CompareTo(x.Amount));
            }


            return result;
        }

        public double? GetArcaBalance(string cardNumber)
        {
            KeyValuePair<string, double> result = new KeyValuePair<string, double>();
            this.Use(client => { result = client.GetArCaBalanceAsync(cardNumber).Result; }
            );
            if (result.Key == "00")
                return result.Value;
            else
                return null;
        }

        public List<LoanRepaymentGrafik> GetLoanRepaymentGrafik(ulong productId)
        {
            List<LoanRepaymentGrafik> result = new List<LoanRepaymentGrafik>();
            this.Use(client =>
            {
                Loan loan = client.GetLoanAsync(productId).Result;
                result = client.GetLoanGrafikAsync(loan).Result;
            }
            );
            return result;
        }

        public List<DepositRepayment> GetDepositRepaymentGrafik(ulong productId)
        {
            List<DepositRepayment> result = new List<DepositRepayment>();
            this.Use(client => { result = client.GetDepositRepaymentsAsync(productId).Result; }
            );
            return result;
        }

        public ActionResult SaveMatureOrder(MatureOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                if(order.Type == OrderType.LoanMature)
                {
                    Loan loan = client.GetLoanAsync(order.ProductId).Result;
                    order.ProductAccount = loan.LoanAccount;
                    order.ProductCurrency = loan.Currency;
                    order.DayOfProductRateCalculation = loan.DayOfRateCalculation.Value;
                }
                else if(order.Type == OrderType.OverdraftRepayment)
                {
                    CreditLine creditLine = client.GetCreditLineAsync(order.ProductId).Result;
                    order.ProductAccount = creditLine.LoanAccount;
                    order.ProductCurrency = creditLine.Currency;
                    order.DayOfProductRateCalculation = creditLine.DayOfRateCalculation.Value;
                    order.MatureType = MatureType.PartialRepayment;
                }
               
                if (order.Account != null && order.Account.AccountNumber == "0")
                {
                    order.Account = null;
                }
               

                result = client.SaveMatureOrderAsync(order).Result;
            }
            );
            return result;
        }

        public ActionResult ApproveMatureOrder(MatureOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => { result = client.ApproveMatureOrderAsync(order).Result; }
            );
            return result;
        }

        public MatureOrder GetMatureOrder(long id)
        {
            MatureOrder result = new MatureOrder();
            this.Use(client => { result = client.GetMatureOrderAsync(id).Result; }
            );
            return result;
        }


        public MembershipRewards GetCardMembershipRewards(string cardNumber)
        {
            MembershipRewards result = new MembershipRewards();
            this.Use(client => { result = client.GetCardMembershipRewardsAsync(cardNumber).Result; }
            );
            return result;
        }

        public List<Guarantee> GetGuarantees(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Guarantee> result = new List<Guarantee>();
            this.Use(client => { result = client.GetGuaranteesAsync(filter).Result; }
            );
            return result;
        }

        public Guarantee GetGuarantee(ulong productId)
        {
            Guarantee result = new Guarantee();
            this.Use(client => { result = client.GetGuaranteeAsync(productId).Result; }
            );
            return result;
        }

        public List<Accreditive> GetAccreditives(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Accreditive> result = new List<Accreditive>();
            this.Use(client => { result = client.GetAccreditivesAsync(filter).Result; }
            );
            return result;
        }

        public Accreditive GetAccreditive(ulong productId)
        {
            Accreditive result = new Accreditive();
            this.Use(client => { result = client.GetAccreditiveAsync(productId).Result; }
            );
            return result;
        }


        public List<DepositCase> GetDepositCases(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<DepositCase> result = new List<DepositCase>();
            this.Use(client => { result = client.GetDepositCasesAsync(filter).Result; }
            );
            return result;
        }

        public DepositCase GetDepositCase(ulong productId)
        {
            DepositCase result = new DepositCase();
            this.Use(client => { result = client.GetDepositCaseAsync(productId).Result; }
            );
            return result;
        }

        public List<PeriodicTransferHistory> GetPeriodicTransferHistory(long productId, DateTime dateFrom,
            DateTime dateTo)
        {
            List<PeriodicTransferHistory> result = new List<PeriodicTransferHistory>();
            this.Use(client => { result = client.GetPeriodicTransferHistoryAsync(productId, dateFrom, dateTo).Result; }
            );
            return result;
        }

        public List<Factoring> GetFactorings(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Factoring> result = new List<Factoring>();
            this.Use(client => { result = client.GetFactoringsAsync(filter).Result; }
            );
            return result;
        }

        public Factoring GetFactoring(ulong productId)
        {
            Factoring result = new Factoring();
            this.Use(client => { result = client.GetFactoringAsync(productId).Result; }
            );
            return result;
        }

        public double GetLoanMatureCapitalPenalty(MatureOrder order)
        {
            double result = 0;
            this.Use(client =>
            {
                if (order.Account != null && order.Account.AccountNumber != "")
                {
                    order.Account = client.GetAccountAsync(order.Account.AccountNumber).Result;
                }

                if (order.PercentAccount != null && order.PercentAccount.AccountNumber != "")
                {
                    order.PercentAccount = client.GetAccountAsync(order.PercentAccount.AccountNumber).Result;
                }

                result = client.GetLoanMatureCapitalPenaltyAsync(order, new User() { userID = 88 }).Result;
            }
            );
            return result;
        }

        public CreditLine GetCreditLine(ulong productId)
        {
            CreditLine result = new CreditLine();
            this.Use(client => { result = client.GetCreditLineAsync(productId).Result; }
            );
            return result;
        }

        public List<CreditLineGrafik> GetCreditLineGrafik(ulong productId)
        {
            List<CreditLineGrafik> result = new List<CreditLineGrafik>();
            this.Use(client => { result = client.GetCreditLineGrafikAsync(productId).Result; }
            );

            return result;
        }

        public ActionResult SaveLoanProductOrder(LoanProductOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveLoanProductOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult ApproveLoanProductOrder(LoanProductOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.ApproveLoanProductOrderAsync(order).Result; }
            );
            return result;
        }

        public LoanProductOrder GetLoanOrder(long id)
        {
            LoanProductOrder result = new LoanProductOrder();

            this.Use(client => { result = client.GetLoanOrderAsync(id).Result; }
            );
            return result;
        }

        public LoanProductOrder GetCreditLineOrder(long id)
        {
            LoanProductOrder result = new LoanProductOrder();

            this.Use(client => { result = client.GetCreditLineOrderAsync(id).Result; }
            );

            result.FirstRepaymentDate = result.EndDate;

            return result;
        }

        public double GetLoanProductInterestRate(LoanProductOrder order, string cardNumber)
        {
            double result = 0;

            this.Use(client => { result = client.GetLoanProductInterestRateAsync(order, cardNumber).Result; }
            );

            return result;
        }

        public string GetAccountDescription(string accountNumber)
        {
            string result = "";

            this.Use(client => { result = client.GetAccountDescriptionAsync(accountNumber).Result; }
            );

            return result;
        }

        public List<ActionError> FastOverdraftValidations(string cardNumber)
        {
            List<ActionError> result = new List<ActionError>();

            this.Use(client => { result = client.FastOverdraftValidationsAsync(cardNumber).Result; }
            );
            return result;
        }

        public ActionResult ApprovePeriodicPaymentOrder(long id)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                Order periodicOrder = client.GetOrderAsync(id).Result;

                if (periodicOrder.SubType == 1 || periodicOrder.SubType == 3)
                {
                    PeriodicPaymentOrder periodicPaymentOrder = client.GetPeriodicPaymentOrderAsync(id).Result;

                    result = client.ApprovePeriodicPaymentOrderAsync(periodicPaymentOrder).Result;
                }
                else
                {
                    result = client.ApprovePeriodicPaymentOrderAsync(periodicOrder).Result;
                }
            }
            );
            return result;
        }

        public ActionResult SavePeriodicPaymentOrder(PeriodicPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SavePeriodicPaymentOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveBudgetPeriodicPaymentOrder(PeriodicBudgetPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SavePeriodicBudgetPaymentOrderAsync(order).Result; }
            );
            return result;
        }

        public PeriodicPaymentOrder GetPeriodicPaymentOrder(long id)
        {
            PeriodicPaymentOrder result = new PeriodicPaymentOrder();

            this.Use(client => { result = client.GetPeriodicPaymentOrderAsync(id).Result; }
            );
            return result;
        }

        public PeriodicBudgetPaymentOrder GetPeriodicBudgetPaymentOrder(long id)
        {
            PeriodicBudgetPaymentOrder result = new PeriodicBudgetPaymentOrder();

            this.Use(client => { result = client.GetPeriodicBudgetPaymentOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult SaveAndApprovePeriodicPaymentOrder(PeriodicPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAndAprovePeriodicPaymentOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveUtilityPeriodicPaymentOrder(PeriodicUtilityPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SavePeriodicUtilityPaymentOrderAsync(order).Result; }
            );
            return result;
        }

        public PeriodicUtilityPaymentOrder GetPeriodicUtilityPaymentOrder(long id)
        {
            PeriodicUtilityPaymentOrder result = new PeriodicUtilityPaymentOrder();

            this.Use(client => { result = client.GetPeriodicUtilityPaymentOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult SaveAndApprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAndAprovePeriodicUtilityPaymentOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveAccountClosingOrder(AccountClosingOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAccountClosingOrderAsync(order).Result; }
            );
            return result;
        }

        public AccountClosingOrder GetAccountClosingOrder(long id)
        {
            AccountClosingOrder result = new AccountClosingOrder();

            this.Use(client => { result = client.GetAccountClosingOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult SaveAndApproveAccountClosingOrder(AccountClosingOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAndApproveAccountClosingAsync(order).Result; }
            );
            return result;
        }

        public ActionResult ApproveAccountClosingOrder(AccountClosingOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.ApproveAccountClosingOrderAsync(order).Result; }
            );
            return result;
        }

        public AccountOrder GetAccountOrder(long id)
        {
            AccountOrder result = new AccountOrder();

            this.Use(client => { result = client.GetAccountOrderAsync(id).Result; }
            );
            return result;
        }

        public DepositOrder GetDepositOrder(long id)
        {
            DepositOrder result = new DepositOrder();

            this.Use(client => { result = client.GetDepositorderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult ApproveDepositOrder(DepositOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.ApproveDepositOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveAndApproveDepositOrder(DepositOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAndApproveDepositOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveDepositOrder(DepositOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveDepositOrderAsync(order).Result; }
            );
            return result;
        }

        public DepositOrderCondition GetDepositCondition(DepositOrder order)
        {
            DepositOrderCondition result = new DepositOrderCondition();

            this.Use(client => { result = client.GetDepositConditionAsync(order).Result; }
            );
            return result;
        }

        public List<Account> GetAccountsForDepositPercentAccount(DepositOrder order)
        {
            List<Account> result = new List<Account>();

            if (order.AccountType == 1)
            {
                if (order.Currency?.Length != 3)
                {
                    return result;
                }
            }
            else if (order.AccountType == 2)
            {
                if (order.Currency.Length != 3 || order.ThirdPersonCustomerNumbers == null ||
                    order.ThirdPersonCustomerNumbers.Count == 0)
                {
                    return result;
                }
            }
            else if (order.AccountType == 3)
            {
                if (order.Currency.Length != 3 || order.ThirdPersonCustomerNumbers == null ||
                    order.ThirdPersonCustomerNumbers.Count == 0)
                {
                    return result;
                }
            }

            this.Use(client =>
            {
                result = client.GetAccountsForNewDepositAsync(order).Result;

                if (order.Currency != "AMD")
                {
                    order.Currency = "AMD";
                    List<Account> AMDAccounts = client.GetAccountsForNewDepositAsync(order).Result;
                    result.AddRange(AMDAccounts);
                }

            }
            );
            return result;
        }

        public List<Account> GetAccountsForNewDeposit(DepositOrder order)
        {
            List<Account> result = new List<Account>();

            if (order.AccountType == 1)
            {
                if (order.Currency?.Length != 3)
                {
                    return result;
                }
            }
            else if (order.AccountType == 2)
            {
                if (order.Currency.Length != 3 || order.ThirdPersonCustomerNumbers == null ||
                    order.ThirdPersonCustomerNumbers.Count == 0)
                {
                    return result;
                }
            }
            else if (order.AccountType == 3)
            {
                if (order.Currency.Length != 3 || order.ThirdPersonCustomerNumbers == null ||
                    order.ThirdPersonCustomerNumbers.Count == 0)
                {
                    return result;
                }
            }

            this.Use(client => { result = client.GetAccountsForNewDepositAsync(order).Result; }
            );




            return result;
        }

        public ActionResult SaveAndApproveAccountOrder(AccountOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAndApproveAccountOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult ApproveAccountOrder(AccountOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.ApproveAccountOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveAccountOrder(AccountOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveAccountOrderAsync(order).Result; }
            );
            return result;
        }

        public List<KeyValuePair<ulong, string>> GetThirdPersons()
        {
            Dictionary<ulong, string> result = new Dictionary<ulong, string>();

            this.Use(client => { result = client.GetThirdPersonsAsync().Result; }
            );
            return result.ToList() ?? new List<KeyValuePair<ulong, string>>();
        }

        public CreditLineTerminationOrder GetCreditLineTerminationOrder(long id)
        {
            CreditLineTerminationOrder result = new CreditLineTerminationOrder();

            this.Use(client => { result = client.GetCreditLineTerminationOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult SaveCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client => { result = client.SaveCreditLineTerminationOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveAndApproveCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveAndApproveCreditLineTerminationOrderAsync(order).Result;

            }
            );
            return result;
        }

        public CardClosingOrder GetCardClosingOrder(long id)
        {
            CardClosingOrder result = new CardClosingOrder();

            this.Use(client =>
            {
                result = client.GetCardClosingOrderAsync(id).Result;

            }
            );
            return result;
        }

        public ActionResult SaveAndApproveCardClosingOrder(CardClosingOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveAndApproveCardClosingOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult ApproveCardClosingOrder(CardClosingOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApproveCardClosingOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult SaveCardClosingOrder(CardClosingOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveCardClosingOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult SaveAndApproveLoanProductOrder(LoanProductOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveAndApproveLoanProductOrderAsync(order).Result;

            }
            );
            return result;
        }

        public string GetSwiftMessage940Statement(DateTime dateFrom, DateTime dateTo, string accountNumber)
        {
            string result = "";

            this.Use(client =>
            {
                result = client.GetSwiftMessage940StatementAsync(dateFrom, dateTo, accountNumber,_cacheHelper.GetSourceType()).Result;

            }
            );
            return result;
        }

        public string GetSwiftMessage950Statement(DateTime dateFrom, DateTime dateTo, string accountNumber)
        {
            string result = "";

            this.Use(client =>
            {
                result = client.GetSwiftMessage950StatementAsync(dateFrom, dateTo, accountNumber,_cacheHelper.GetSourceType()).Result;

            }
            );
            return result;
        }

        public List<DepositRepayment> GetDepositRepaymentsPrior(DepositRepaymentRequest request)
        {
            List<DepositRepayment> result = new List<DepositRepayment>();

            DepositOrderCondition depositOrderCondition = new DepositOrderCondition();
            DepositOrder order = new DepositOrder();
            order.AccountType = request.AccountType;
            order.Currency = request.Currency;
            order.Deposit = new Deposit();
            order.Deposit.EndDate = request.EndDate;
            order.Deposit.StartDate = request.StartDate;
            order.Deposit.Currency = request.Currency;
            order.Deposit.DepositType = (byte)request.DepositType;
            order.DepositType = request.DepositType;

            this.Use(client =>
            {
                depositOrderCondition = client.GetDepositConditionAsync(order).Result;
                result = client.GetDepositRepaymentsPriorAsync(request).Result;
            }
            );

            return result;
        }

        public ActionResult SaveDepositTerminationOrder(DepositTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveDepositTerminationAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult ApproveDepositTerminationOrder(DepositTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApproveDepositTerminationAsync(order).Result;

            }
            );
            return result;
        }

        public DepositTerminationOrder GetDepositTerminationOrder(long id)
        {
            DepositTerminationOrder result = new DepositTerminationOrder();

            this.Use(client =>
            {
                result = client.GetDepositTerminationOrderAsync(id).Result;

            }
            );
            return result;
        }

        public ActionResult SaveArcaCardsTransactionOrder(ArcaCardsTransactionOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveArcaCardsTransactionOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ArcaCardsTransactionOrder GetArcaCardsTransactionOrder(long id)
        {
            ArcaCardsTransactionOrder result = new ArcaCardsTransactionOrder();

            this.Use(client =>
            {
                result = client.GetArcaCardsTransactionOrderAsync(id).Result;

            }
            );
            return result;
        }

        public ActionResult ApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApproveArcaCardsTransactionOrderAsync(order).Result;

            }
            );
            return result;
        }

        public string GetEmbossingName(string cardNumber)
        {
            string result = "";

            this.Use(client =>
            {
                result = client.GetEmbossingNameAsync(cardNumber).Result;

            }
            );
            return result;
        }

        public List<KeyValuePair<string, string>> GetCardLimits(long productId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            this.Use(client =>
            {
                result = client.GetCardLimitsAsync(productId).Result;
            }
            );
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public CardLimitChangeOrder GetCardLimitChangeOrder(long id)
        {
            CardLimitChangeOrder result = new CardLimitChangeOrder();

            this.Use(client =>
            {
                result = client.GetCardLimitChangeOrderAsync(id).Result;

            }
            );
            return result;
        }

        public ActionResult ApproveCardLimitChangeOrder(CardLimitChangeOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApproveCardLimitChangeOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult SaveCardLimitChangeOrder(CardLimitChangeOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SaveCardLimitChangeOrderAsync(order).Result;

            }
            );
            return result;
        }

        public List<JointCustomer> GetAccountJointCustomers(string accountNumber)
        {
            List<JointCustomer> result = new List<JointCustomer>();

            this.Use(client =>
            {
                List<KeyValuePair<ulong, double>> jointCustomersKeyValueList = client.GetAccountJointCustomersAsync(accountNumber).Result;

                foreach (KeyValuePair<ulong, double> p in jointCustomersKeyValueList)
                {
                    JointCustomer c = new JointCustomer
                    {
                        CustomerNumber = p.Key,
                        Part = p.Value
                    };

                    CustomerMainData mainData = client.GetCustomerMainDataAsync(c.CustomerNumber).Result;
                    c.FullName = mainData.CustomerDescription;

                    result.Add(c);
                }
            }
            );
            return result;
        }

        public ActionResult SaveAndApprovePlasticCardOrder(PlasticCardOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SavePlasticCardOrderAsync(order).Result;

            }
            );
            return result;
        }

        public PlasticCardOrder GetPlasticCardOrder(long orderID)
        {
            PlasticCardOrder result = new PlasticCardOrder();

            this.Use(client =>
            {
                result = client.GetPlasticCardOrderAsync(orderID).Result;

            }
            );
            return result;
        }

        public ActionResult CheckDepositOrderCondition(DepositOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.CheckDepositOrderConditionAsync(order).Result;

            }
            );
            return result;
        }


        public double GetCardFee(PaymentOrder order)
        {
            double result = 0;

            this.Use(client =>
            {
                result = client.GetCardFeeAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult SavePlasticCardOrder(PlasticCardOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SavePlasticCardOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult ApprovePlasticCardOrder(PlasticCardOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApprovePlasticCardOrderAsync(order).Result;

            }
            );
            return result;
        }

        public bool IsCurrentAccount(string accountNumber)
        {
            bool result = false;

            this.Use(client =>
            {
                result = client.IsCurrentAccountAsync(accountNumber).Result;

            }
            );
            return result;
        }

        public Card GetCardByAccountNumber(string accountNumber)
        {
            Card result = new Card();

            this.Use(client =>
            {
                result = client.GetCardByAccountNumberAsync(accountNumber).Result;

            }
            );
            return result;
        }

        public bool CheckAccountForPSN(string accountNumber)
        {
            bool result = false;

            this.Use(client =>
            {
                result = client.CheckAccountForPSNAsync(accountNumber).Result;

            }
            );
            return result;
        }

        public Card GetFullCardByAccountNumber(string accountNumber)
        {
            Card result = new Card();

            this.Use(client =>
            {
                result = client.GetCardByAccountNumberAsync(accountNumber).Result;

            }
            );
            return result;
        }


        public CreditLine GetCreditLineByAccountNumber(string loanFullNumber)
        {
            CreditLine result = new CreditLine();

            this.Use(client =>
            {
                result = client.GetCreditLineByAccountNumberAsync(loanFullNumber).Result;

            }
            );
            return result;
        }

        public List<CreditLineGrafik> GetCreditLineRepayment(ulong productId)
        {
            List<CreditLineGrafik> result = new List<CreditLineGrafik>();

            this.Use(client =>
            {
                result = client.GetCreditLineGrafikAsync(productId).Result;

            }
            );

            if (result.Count > 1)
            {
                result.Sort((x, y) => x.EndDate.CompareTo(y.EndDate));
                result.RemoveRange(0, result.Count - 1);
            }

            return result;
        }

        public ActionResult ApproveCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApproveCreditLineTerminationOrderAsync(order).Result;

            }
            );
            return result;
        }

        public CreditLine GetCardOverdraft(string cardNumber)
        {
            CreditLine result = new CreditLine();

            this.Use(client =>
            {
                result = client.GetCardOverDraftAsync(cardNumber).Result;

            }
            );
            return result;
        }

        public double GetMRFeeAMD(string cardNumber)
        {
            double result = 0;

            this.Use(client =>
            {
                result = client.GetMRFeeAMDAsync(cardNumber).Result;

            }
            );
            return result;
        }

        public List<Card> GetAvailableCardsForCreditLine(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            List<Card> result = new List<Card>();

            this.Use(client =>
            {
                result = client.GetCardsAsync(filter).Result;

            }
            );

            result.RemoveAll(x => x.CreditLine != null);
            result.RemoveAll(x => x.Overdraft != null);
            result.RemoveAll(x => x.Type == 38 || x.Type == 51);
            result.RemoveAll(m => m.SupplementaryType != SupplementaryType.Main);
            return result;
        }

        public LoanProductOrder GetDepositLoanOrder(long id)
        {
            LoanProductOrder result = new LoanProductOrder();

            this.Use(client =>
            {
                result = client.GetLoanOrderAsync(id).Result;

            }
            );

            result.FirstRepaymentDate = result.EndDate;
            return result;
        }

        public string GetCardTypeName(string cardNumber)
        {
            string result = "";

            this.Use(client =>
            {
                result = client.GetCardTypeNameAsync(cardNumber).Result;

            }
            );

            return result;
        }


        public Card GetCardByCardNumber(string cardNumber)
        {
            Card result = new Card();

            this.Use(client =>
            {
                result = client.GetCardByCardNumberAsync(cardNumber).Result;

            }
            );

            return result;
        }

        public ActionResult SavePeriodicTerminationOrder(PeriodicTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SavePeriodicTerminationOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult ApprovePeriodicTerminationOrder(PeriodicTerminationOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApprovePeriodicTerminationOrderAsync(order).Result;

            }
            );
            return result;
        }

        public PeriodicTerminationOrder GetPeriodicTerminationOrder(long id)
        {
            PeriodicTerminationOrder result = new PeriodicTerminationOrder();

            this.Use(client =>
            {
                result = client.GetPeriodicTerminationOrderAsync(id).Result;

            }
            );
            return result;
        }

        public ActionResult SavePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.SavePeriodicDataChangeOrderAsync(order).Result;

            }
            );
            return result;
        }

        public ActionResult ApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ApprovePeriodicDataChangeOrderAsync(order).Result;

            }
            );
            return result;
        }

        public List<Message> GetMessages(DateTime dateFrom, DateTime dateTo, MessageType type)
        {
            List<Message> result = new List<Message>();
            this.Use(client =>
            {
                result = client.GetMessagesAsync(dateFrom, dateTo, (short)type).Result;
            });
            return result;
        }

        public List<Message> GetNumberOfMessages(short messageCount, MessageType type)
        {
            List<Message> result = new List<Message>();
            this.Use(client =>
            {
                result = client.GetNumberOfMessagesAsync(messageCount, type).Result;
            });
            return result;
        }

        public void AddMessage(Message message)
        {
            this.Use(client =>
            {
                client.AddMessageAsync(message);
            });
        }

        public void DeleteMessage(int messageId)
        {
            this.Use(client =>
            {
                client.DeleteMessageAsync(messageId);
            });
        }

        public void MarkMessageReaded(int messageId)
        {
            this.Use(client =>
            {
                client.MarkMessageReadedAsync(messageId);
            });
        }

        public Contact GetContact(ulong contactId)
        {
            Contact result = new Contact();
            this.Use(client =>
            {
                result = client.GetContactAsync(contactId).Result;
            });
            return result;
        }

        public List<Contact> GetContacts()
        {
            List<Contact> result = new List<Contact>();
            this.Use(client =>
            {
                result = client.GetContactsAsync().Result;
            });
            return result;
        }

        public int AddContact(Contact contact)
        {
            int result = 0;
            this.Use(client => result = client.AddContactAsync(contact).Result);
            return result;
        }

        public int UpdateContact(Contact contact)
        {
            int result = 0;
            this.Use(client => result = client.UpdateContactAsync(contact).Result);
            return result;
        }

        public int DeleteContact(ulong contactId)
        {
            int result = 0;
            this.Use(client => result = client.DeleteContactAsync(contactId).Result);
            return result;
        }

        public int GetUnreadMessagesCountByType(MessageType type)
        {
            int result = 0;
            this.Use(client =>
            {
                result = client.GetUnreadMessagesCountByTypeAsync(type).Result;
            });
            return result;
        }

        public ActionResult SaveProductNote(ProductNote productNote)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveProductNoteAsync(productNote).Result;
            });
            return result;
        }
        public ProductNote GetProductNote(double uniqueId)
        {
            ProductNote result = new ProductNote();
            this.Use(client =>
            {
                result = client.GetProductNoteAsync(uniqueId).Result;
            });
            // կիսատ է
            // HasProductPermission պետք է ավելացվի
            return result;
        }

        public ActionResult ChangeTemplateStatus(int templateId, TemplateStatus templateStatus)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ChangeTemplateStatusAsync(templateId, templateStatus).Result;
            });
            return result;
        }
        public Order GetOrder(long id)
        {
            Order result = new Order();
            this.Use(client =>
            {
                result = client.GetOrderAsync(id).Result;
            });

            return result;
        }


        public PeriodicTransferDataChangeOrder GetPeriodicDataChangeOrder(long id)
        {
            PeriodicTransferDataChangeOrder result = new PeriodicTransferDataChangeOrder();

            this.Use(client =>
            {
                result = client.GetPeriodicDataChangeOrderAsync(id).Result;

            }
            );
            return result;
        }

        public ActionResult SaveSwiftCopyOrder(SwiftCopyOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveSwiftCopyOrderAsync(order).Result;
            }
            );
            return result;
        }


        public ActionResult ApproveSwiftCopyOrder(SwiftCopyOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => { result = client.ApproveSwiftCopyOrderAsync(order).Result; }
            );
            return result;
        }

        public SwiftCopyOrder GetSwiftCopyOrder(long id)
        {
            SwiftCopyOrder result = new SwiftCopyOrder();
            this.Use(client => { result = client.GetSwiftCopyOrderAsync(id).Result; }
            );
            return result;
        }

        public double GetSwiftCopyOrderFee()
        {
            double result = 0;

            this.Use(client =>
            {
                result = client.GetOrderServiceFeeAsync(XBS.OrderType.SwiftCopyOrder, 0).Result;

            }
            );
            return result;
        }

        public CredentialOrder GetCredentialOrder(long id)
        {
            CredentialOrder result = new CredentialOrder();
            this.Use(client => { result = client.GetCredentialOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult SaveCredentialOrder(CredentialOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveCredentialOrderAsync(order).Result;
            }
            );
            return result;
        }

        public ActionResult ApproveCredentialOrder(CredentialOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => { result = client.ApproveCredentialOrderAsync(order).Result; }
            );
            return result;
        }

        public CustomerDataOrder GetCustomerDataOrder(long id)
        {
            CustomerDataOrder result = new CustomerDataOrder();
            this.Use(client => { result = client.GetCustomerDataOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult SaveCustomerDataOrder(CustomerDataOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveCustomerDataOrderAsync(order).Result;
            }
            );
            return result;
        }

        public ActionResult ApproveCustomerDataOrder(CustomerDataOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => { result = client.ApproveCustomerDataOrderAsync(order).Result; }
            );
            return result;
        }
        public ActionResult SavePaymentOrder(PaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SavePaymentOrderAsync(order).Result);
            return result;
        }

        public PaymentOrder GetPaymentOrder(long id)
        {
            PaymentOrder result = new PaymentOrder();
            this.Use(client => result = client.GetPaymentOrderAsync(id).Result);
            //կիսատ է։
            //HasProductPermission պետք է ավելացվի
            return result;
        }

        public ActionResult ApprovePaymentOrder(PaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ApprovePaymentOrderAsync(order).Result);
            return result;
        }

        public InternationalPaymentOrder GetInternationalPaymentOrder(long id)
        {
            InternationalPaymentOrder result = new InternationalPaymentOrder();
            this.Use(client => result = client.GetInternationalPaymentOrderAsync(id).Result);
            // կիսատ է
            // HasProductPermission պետք է ավելացվի
            return result;
        }

        public ActionResult SaveInternationalPaymentOrder(InternationalPaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveInternationalPaymentOrderAsync(order).Result);
            return result;
        }

        public ActionResult ApproveInternationalPaymentOrder(InternationalPaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ApproveInternationalPaymentOrderAsync(order).Result);
            return result;
        }

        public ActionResult SaveUtilityPaymentOrder(UtilityPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            if (order.CommunalType == CommunalTypes.Trash)
                order.CommunalType = CommunalTypes.UCom;

            this.Use(client => result = client.SaveUtiliyPaymentOrderAsync(order).Result);
            return result;
        }

        public UtilityPaymentOrder GetUtilityPaymentOrder(long id)
        {
            UtilityPaymentOrder result = new UtilityPaymentOrder();
            this.Use(client => result = client.GetUtilityPaymentOrderAsync(id).Result);
            // կիսատ է
            // HasProductPermission պետք է ավելացվի
            return result;
        }

        public ActionResult ApproveUtilityPaymentOrder(UtilityPaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ApproveUtilityPaymentOrderAsync(order).Result);
            return result;
        }

        public List<DAHKFreezing> GetDAHKFreezings()
        {
            List<DAHKFreezing> result = new List<DAHKFreezing>();
            this.Use(client => result = client.GetDahkFreezingsAsync().Result);
            return result;
        }

        public List<Account> GetAccountsForCredential(int operationType)
        {
            List<Account> result = new List<Account>();
            this.Use(client => result = client.GetAccountsForCredentialAsync(operationType).Result);
            return result;
        }

        public DateTime GetNextOperDay()
        {
            DateTime result = new DateTime();
            this.Use(client => result = client.GetNextOperDayAsync().Result);
            return result;
        }

        public List<KeyValuePair<string, string>> GetDepositCreditLineContractInfo(int docId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => result = client.GetDepositCreditLineContractInfoAsync(docId).Result);
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetDepositLoanContractInfo(int docId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => result = client.GetDepositLoanContractInfoAsync(docId).Result);
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public short GetCustomerFilial()
        {
            short result = 0;
            this.Use(client => result = client.GetCustomerFilialAsync().Result);
            return result;
        }

        public double GetCBKursForDate(DateTime date, string currency)
        {
            double result = 0;
            this.Use(client => result = client.GetCBKursForDateAsync(date, currency).Result);
            return result;
        }

        public string GetConnectAccountFullNumber(string currency)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetConnectAccountFullNumberAsync(currency).Result);
            return result;
        }

        public Card GetCardWithOutBallance(string accountNumber)
        {
            Card result = new Card();
            this.Use(client => result = client.GetCardWithOutBallanceAsync(accountNumber).Result);
            return result;
        }

        public CreditLinePrecontractData GetCreditLinePrecontractData(DateTime startDate, DateTime endDate, double interestRate, double repaymentPercent, string cardNumber, string currency, double amount, int loanType)
        {
            CreditLinePrecontractData result = new CreditLinePrecontractData();
            this.Use(client => result = client.GetCreditLinePrecontractDataAsync(startDate, endDate, interestRate, repaymentPercent, cardNumber, currency, amount, loanType).Result);
            return result;
        }

        public int GetCardType(string cardNumber)
        {
            int result = 0;
            this.Use(client => result = client.GetCardTypeAsync(cardNumber).Result);
            return result;
        }

        public double GetCreditLineDecreasingAmount(double startCapital, string currency, DateTime startDate, DateTime endDate)
        {
            double result = 0;
            this.Use(client => result = client.GetCreditLineDecreasingAmountAsync(startCapital, currency, startDate, endDate).Result);
            return result;
        }

        public List<KeyValuePair<string, string>> ProvisionContract(long docId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => result = client.ProvisionContractAsync(docId).Result);
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public ActionResult SaveAndApproveRemovalOrder(RemovalOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveAndApproveRemovalOrderAsync(order).Result);
            return result;
        }

        public RemovalOrder GetRemovalOrder(long id)
        {
            RemovalOrder result = new RemovalOrder(); ;
            this.Use(client => result = client.GetRemovalOrderAsync(id).Result);
            return result;
        }

        //Diana
        public List<VehicleViolationResponse> GetVehicleViolationById(string violationId)
        {
            List<VehicleViolationResponse> result = new List<VehicleViolationResponse>(0);
            this.Use(client => result = client.GetVehicleViolationByIdAsync(violationId).Result);

            //REMOVE DIANA!!!!!!!
            //VehicleViolationResponse item1 = new VehicleViolationResponse();
            //item1.Id = 176709;
            //item1.FineAmount = 2000;
            //item1.PayableAmount = 2000;
            //item1.PenaltyAmount = 0;
            //item1.PayedAmount = 0;
            //item1.PoliceAccount = "900013150058";
            //item1.RequestedAmount = 2000;
            //item1.ResponseId = 166716;
            //item1.VehicleModel = "KIA RIO 1.4";
            //item1.VehicleNumber = "133OU64";
            //item1.VehiclePassport = "SC067423";
            //item1.ViolationDate = Convert.ToDateTime("2019-04-04 12:17:23.000");
            //item1.ViolationNumber = violationId;
            //result.Add(item1);
            //REMOVE END


            return result;
        }

        //Diana
        public List<VehicleViolationResponse> GetVehicleViolationByPsnVehNum(string psn, string vehNum)
        {
            List<VehicleViolationResponse> result = new List<VehicleViolationResponse>(0);
            this.Use(client => result = client.GetVehicleViolationByPsnVehNumAsync(psn,vehNum).Result);

            //REMOVE DIANA!!!!!!!!
            //VehicleViolationResponse item1 = new VehicleViolationResponse();
            //VehicleViolationResponse item2 = new VehicleViolationResponse();
            //item1.Id = 176709;
            //item1.FineAmount = 2000;
            //item1.PayableAmount = 2000;
            //item1.PenaltyAmount = 0;
            //item1.PayedAmount = 0;
            //item1.PoliceAccount = "900013150058";
            //item1.RequestedAmount = 2000;
            //item1.ResponseId = 166716;
            //item1.VehicleModel = "KIA RIO 1.4";
            //item1.VehicleNumber = vehNum;
            //item1.VehiclePassport = psn;
            //item1.ViolationDate = Convert.ToDateTime("2019-04-04 12:17:23.000");
            //item1.ViolationNumber = "1909388733";

            //item2.Id = 176708;
            //item2.FineAmount = 4000;
            //item2.PayableAmount = 4000;
            //item2.PenaltyAmount = 0;
            //item2.PayedAmount = 0;
            //item2.PoliceAccount = "900013150058";
            //item2.RequestedAmount = 4000;
            //item2.ResponseId = 166715;
            //item2.VehicleModel = "LEXUS GX 460";
            //item2.VehicleNumber = vehNum;
            //item2.VehiclePassport = psn;
            //item2.ViolationDate = Convert.ToDateTime("2019-04-05 19:15:13.000");
            //item2.ViolationNumber = "1909395132";

            //result.Add(item1);
            //result.Add(item2);
            //REMOVE END

            return result;
        }

        public BudgetPaymentOrder GetBudgetPaymentOrder(long id)
        {
            BudgetPaymentOrder result = new BudgetPaymentOrder();
            this.Use(client => result = client.GetBudgetPaymentOrderAsync(id).Result);
            return result;
        }


        public CustomerMainData GetCustomerMainData(ulong customerNumber)
        {
            var result = new CustomerMainData();
            this.Use(client => result = client.GetCustomerMainDataAsync(customerNumber).Result);
            return result;
        }

        public List<HBProductPermission> GetUserProductsPermissions(string userName)
        {
            var result = new List<HBProductPermission>();
            this.Use(client => result = client.GetHBUserProductsPermissionsAsync(userName).Result);
            return result;
        }

        public object GetBusinesDepositOptionRate(ushort depositOption, string currency)
        {
            double result = 0;
            this.Use(client => result = client.GetBusinesDepositOptionRateAsync(depositOption, currency).Result);
            return result;
        }

        public List<OrderGroup> GetOrderGroups(OrderGroupStatus status = OrderGroupStatus.Active, OrderGroupType groupType = OrderGroupType.CreatedByCustomer)
        {
            List<OrderGroup> result = new List<OrderGroup>();
            this.Use(client => result = client.GetOrderGroupsAsync(status, groupType).Result);
            return result;
        }

        public PaymentOrderTemplate GetPaymentOrderTemplate(int templateId)
        {
            PaymentOrderTemplate result = new PaymentOrderTemplate();
            this.Use(client => result = client.GetPaymentOrderTemplateAsync(templateId).Result);
            return result;
        }

        public BudgetPaymentOrderTemplate GetBudgetPaymentOrderTemplate(int templateId)
        {
            BudgetPaymentOrderTemplate result = new BudgetPaymentOrderTemplate();
            this.Use(client => result = client.GetBudgetPaymentOrderTemplateAsync(templateId).Result);
            return result;
        }

        public LoanMatureOrderTemplate GetLoanMatureOrderTemplate(int templateId)
        {
            LoanMatureOrderTemplate result = new LoanMatureOrderTemplate();
            this.Use(client => result = client.GetLoanMatureOrderTemplateAsync(templateId).Result);
            return result;
        }

        public UtilityPaymentOrderTemplate GetUtilityPaymentOrderTemplate(int templateId)
        {
            UtilityPaymentOrderTemplate result = new UtilityPaymentOrderTemplate();
            this.Use(client => result = client.GetUtilityPaymentOrderTemplateAsync(templateId).Result);
            return result;
        }


        public ActionResult SaveBudgetPaymentOrder(BudgetPaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveBudgetPaymentOrderAsync(order).Result);
            return result;
        }

        public double GetLastExchangeRate(string currency, byte rateType, byte direction)
        {
            double result = 0;
            this.Use(client =>
            {
                result = client.GetLastExchangeRateAsync(currency, (RateType)rateType, (ExchangeDirection)direction, 22000).Result;
            }
            );
            return result;
        }

        public PaymentOrderFutureBalance GetPaymentOrderFutureBalance(long id)
        {
            PaymentOrderFutureBalance result = new PaymentOrderFutureBalance();
            this.Use(client =>
            {
                result = client.GetPaymentOrderFutureBalanceByIdAsync(id).Result;
            }
            );
            return result;
        }

        public double GetPaymentOrderFee(PaymentOrder order)
        {
            double result = 0;

            this.Use(client =>
            {
                result = client.GetPaymentOrderFeeAsync(order, 0).Result;

            }
            );
            return result;
        }

        public List<Account> GetAccountsForOrder(short orderType, byte orderSubType, byte accountType)
        {
            List<Account> result = new List<Account>();

            this.Use(client =>
            {
                result = client.GetAccountsForOrderAsync(orderType, orderSubType, accountType).Result;

            }
            );
            return result;
        }

        public List<Loan> GetAparikTexumLoans()
        {
            List<Loan> result = new List<Loan>();

            this.Use(client =>
            {
                result = client.GetAparikTexumLoansAsync().Result;

            }
            );
            return result;
        }

        public double GetReferenceOrderFee(bool UrgentSign)
        {
            double result = 0;

            this.Use(client =>
            {
                result = client.GetOrderServiceFeeAsync(XBS.OrderType.ReferenceOrder, Convert.ToInt32(UrgentSign)).Result;

            }
            );
            return result;
        }

        public double GetInternationalPaymentOrderFee(InternationalPaymentOrder order)
        {
            double result = 0;

            this.Use(client =>
            {
                result = client.GetInternationalPaymentOrderFeeAsync(order).Result;

            }
            );
            return result;
        }
        public List<KeyValuePair<string, string>> GetPlasticCardOrderCardTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetPlasticCardOrderCardTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<Order> GetDraftOrders(DateTime dateFrom, DateTime dateTo)
        {
            List<Order> result = new List<Order>();
            this.Use(client => result = client.GetDraftOrdersAsync(dateFrom, dateTo).Result);
            return result;
        }

        public List<Order> GetSentOrders(DateTime dateFrom, DateTime dateTo)
        {
            List<Order> result = new List<Order>();
            this.Use(client =>
            {
                result = client.GetSentOrdersAsync(dateFrom, dateTo).Result;
                result.RemoveAll(m => m.Source == SourceType.Bank);
            });

            return result;
        }

        public List<Order> GetApproveReqOrder(DateTime dateFrom, DateTime dateTo)
        {
            List<Order> result = new List<Order>();
            this.Use(client => result = client.GetApproveReqOrderAsync(dateFrom, dateTo).Result);
            return result;
        }

        public List<Communal> GetCommunals(SearchCommunal searchCommunal)
        {
            List<Communal> result = new List<Communal>();
            this.Use(client => result = client.GetCommunalsAsync(searchCommunal,true).Result);
            return result;
        }

        public List<CommunalDetails> GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType = AbonentTypes.physical)
        {
            List<CommunalDetails> result = new List<CommunalDetails>();
            if (abonentType == 0)
                abonentType = AbonentTypes.physical;
            this.Use(client => result = client.GetCommunalDetailsAsync(communalType, abonentNumber, checkType, branchCode, abonentType).Result);
            return result;
        }

        public StatmentByEmailOrder GetStatementByEmailOrder(long id)
        {
            StatmentByEmailOrder result = new StatmentByEmailOrder();
            this.Use(client => { result = client.GetStatmentByEmailOrderAsync(id).Result; }
            );
            return result;
        }

        public ActionResult ApproveStatementByEmailOrder(StatmentByEmailOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => { result = client.ApproveStatmentByEmailOrderAsync(order).Result; }
            );
            return result;
        }

        public ActionResult SaveStatementByEmailOrder(StatmentByEmailOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveStatmentByEmailOrderAsync(order).Result;
            }
            );
            return result;
        }


        public List<Order> GetOrders(OrderFilter orderFilter)
        {
            List<Order> result = new List<Order>();
            this.Use(client => { result = client.GetOrdersByFilterAsync(orderFilter).Result; }
            );
            return result;
        }

        public ActionResult DeletePaymentOrder(long id)
        {
            ActionResult result = new ActionResult();
            Order order = new Order();
            this.Use(client =>
            {
                order = client.GetOrderAsync(id).Result;
                result = client.DeleteOrderAsync(order).Result;
            });
            return result;
        }

        public int GetUnreadedMessagesCount()
        {
            int result = 0;
            this.Use(client => result = client.GetUnreadedMessagesCountAsync().Result);
            return result;
        }

        public ReceivedFastTransferPaymentOrder GetReceivedFastTransferPaymentOrder(long id)
        {
            ReceivedFastTransferPaymentOrder result = new ReceivedFastTransferPaymentOrder();
            this.Use(client => result = client.GetReceivedFastTransferPaymentOrderAsync(id, "").Result);
            return result;
        }

        public ActionResult SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveReceivedFastTransferPaymentOrderAsync(order).Result;
            });
            return result;
        }

        public ActionResult ApproveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ApproveFastTransferPaymentOrderAsync(order).Result;
            });
            return result;
        }

        public string GetReceivedFastTransferOrderRejectReason(int orderId)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetReceivedFastTransferOrderRejectReasonAsync(orderId).Result);
            return result;
        }

        public List<OrderHistory> GetOnlineOrderHistory(int orderId)
        {
            List<OrderHistory> result = new List<OrderHistory>();
            this.Use(client => result = client.GetOnlineOrderHistoryAsync(orderId).Result);
            return result;
        }

        public List<Order> GetConfirmRequiredOrders(DateTime startDate, DateTime endDate, string userName)
        {
            List<Order> result = new List<Order>();
            this.Use(client =>
            {
                result = client.GetConfirmRequiredOrdersAsync(userName, 0, startDate, endDate, "", "", "", true, null, -1).Result;
                result.RemoveAll(m => m.Source == SourceType.Bank);
            });
            return result;
        }

        public ActionResult SaveCashOrder(CashOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveCashOrderAsync(order).Result);
            return result;
        }

        public ActionResult ApproveCashOrder(CashOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ApproveCashOrderAsync(order).Result);
            return result;
        }

        public CashOrder GetCashOrder(long id)
        {
            CashOrder result = new CashOrder();
            this.Use(client => result = client.GetCashOrderAsync(id).Result);
            return result;
        }

        public double GetCredentialOrderFee()
        {
            double result = 0;
            this.Use(client => result = client.GetServiceProvidedOrderFeeAsync(XBS.OrderType.FeeForServiceProvided, 215).Result);
            return result;
        }

        public ActionResult SaveOrderGroup(OrderGroup group, int userId)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                group.UserId = userId;
                result = client.SaveOrderGroupAsync(group).Result;
            });
            return result;
        }

        public ReestrTransferOrder GetReestrTransferOrder(long id)
        {
            ReestrTransferOrder result = new ReestrTransferOrder();
            this.Use(client => result = client.GetReestrTransferOrderAsync(id).Result);
            return result;
        }

        public ActionResult ApproveReestrTransferOrder(ReestrTransferOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ApproveReestrTransferOrderAsync(order).Result;
            });

            return result;

        }

        public string GetPasswordForCustomerDataOrder()
        {
            string result = String.Empty;
            this.Use(client => result = client.GetPasswordForCustomerDataOrderAsync().Result);
            return result;
        }

        public string GetEmailForCustomerDataOrder()
        {
            string result = String.Empty;
            this.Use(client => result = client.GetEmailForCustomerDataOrderAsync().Result);
            return result;
        }

        public double GetCardToCardTransferFee(string debitCardNumber, string creditCardNumber, double amount, string currency)
        {
            double result = 0;
            this.Use(client => result = client.GetCardToCardTransferFeeAsync(debitCardNumber, creditCardNumber, amount, currency).Result);
            return result;
        }

        public CardToCardOrder GetCardToCardOrder(long id)
        {
            CardToCardOrder result = new CardToCardOrder();
            this.Use(client => result = client.GetCardToCardOrderAsync(id).Result);
            return result;
        }

        public ActionResult SaveCardToCardOrder(CardToCardOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveCardToCardOrderAsync(order).Result);
            return result;
        }
        public ActionResult ToCardWithECommerce(CardToCardOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ToCardWithECommerceAsync(order).Result);
            return result;
        }       
        public ActionResult ApproveCardToCardOrder(CardToCardOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ApproveCardToCardOrderAsync(order).Result;
            });
            return result;
        }


        public ActionResult SavePaymentOrderTemplate(PaymentOrderTemplate template)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SavePaymentOrderTemplateAsync(template).Result);
            return result;
        }

        public List<Communal> GetCommunalsByPhoneNumber(SearchCommunal searchCommunal)
        {
            List<Communal> result = new List<Communal>();
            this.Use(client =>
            {
                searchCommunal.CommunalType = XBS.CommunalTypes.ArmenTel;
                result.AddRange(client.GetCommunalsAsync(searchCommunal,true).Result);

                searchCommunal.CommunalType = XBS.CommunalTypes.VivaCell;
                result.AddRange(client.GetCommunalsAsync(searchCommunal,true).Result);

                searchCommunal.CommunalType = XBS.CommunalTypes.Orange;
                result.AddRange(client.GetCommunalsAsync(searchCommunal,true).Result);
            });
            return result;
        }

        public ActionResult SaveLoanMatureOrderTemplate(LoanMatureOrderTemplate template)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveLoanMatureOrderTemplateAsync(template).Result);
            return result;
        }

        public InternationalPaymentOrder GetCustomerDateForInternationalPayment()
        {
            InternationalPaymentOrder result = new InternationalPaymentOrder();
            this.Use(client => result = client.GetCustomerDateForInternationalPaymentAsync().Result);
            return result;
        }

        public string SaveUploadedFile(UploadedFile uploadedFile)
        {
            string result = String.Empty;
            this.Use(client => result = client.SaveUploadedFileAsync(uploadedFile).Result);
            return result;
        }

        public ActionResult SaveHBServletTokenUnBlockRequestOrder(HBServletRequestOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                order.ServletAction = HBServletAction.UnlockToken;
                result = client.SaveHBServletRequestOrderAsync(order).Result;
            });
            return result;
        }

        public ActionResult ApproveHBServletTokenUnBlockRequestOrder(HBServletRequestOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ApproveHBServletRequestOrderAsync(order).Result;
            });
            return result;
        }

        public HBServletRequestOrder GetHBServletRequestOrder(long id)
        {
            HBServletRequestOrder result = new HBServletRequestOrder();
            this.Use(client => result = client.GetHBServletRequestOrderAsync(id).Result);
            return result;
        }

        public ReadXmlFileAndLog ReadXmlFile(string fileId)
        {
            ReadXmlFileAndLog result = new ReadXmlFileAndLog();
            short filialCode;
            this.Use(client =>
            {
                filialCode = client.GetCustomerFilialAsync().Result;
                result = client.ReadXmlFileAsync(fileId, filialCode).Result;
            });
            return result;
        }

        public HBApplicationOrder GetHBApplicationOrder(long id)
        {
            HBApplicationOrder result = new HBApplicationOrder();
            this.Use(client => result = client.GetHBApplicationOrderAsync(id).Result);
            return result;
        }

        public HBActivationOrder GetHBActivationOrder(long id)
        {
            HBActivationOrder result = new HBActivationOrder();
            this.Use(client => result = client.GetHBActivationOrderAsync(id).Result);
            return result;
        }

        public ActionResult ApproveOrder(Order order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ApproveOrderAsync(order).Result);
            return result;
        }

        public OrderType GetDocumentType(int docId)
        {
            OrderType result = 0;
            this.Use(client => result = client.GetDocumentTypeAsync(docId).Result);
            return result;
        }

        public void DeleteOrderGroup(int groupId)
        {
            this.Use(client =>
            {
                client.DeleteOrderGroupAsync(groupId);
            });
        }

        public ReferenceOrder GetReferenceOrder(long id)
        {
            ReferenceOrder order = new ReferenceOrder();
            this.Use(client => order = client.GetReferenceOrderAsync(id).Result);
            return order;
        }

        public ActionResult SaveReferenceOrder(ReferenceOrder order)
        {
            var result = new ActionResult();
            this.Use(clinet => result = clinet.SaveReferenceOrderAsync(order).Result);
            return result;
        }

        public ActionResult ApproveReferenceOrder(ReferenceOrder order)
        {
            var result = new ActionResult();
            this.Use(clinet => result = clinet.ApproveReferenceOrderAsync(order).Result);
            return result;
        }

        public List<Template> GetCustomerTemplates()
        {
            List<Template> result = new List<Template>();
            this.Use(client => result = client.GetCustomerTemplatesAsync(TemplateStatus.Active).Result);
            return result;
        }

        public CardToCardOrderTemplate GetCardToCardOrderTemplate(int templateId)
        {
            CardToCardOrderTemplate result = new CardToCardOrderTemplate();
            this.Use(client => result = client.GetCardToCardOrderTemplateAsync(templateId).Result);
            return result;
        }

        public List<string> GetDepositAndCurrentAccountCurrencies(XBS.OrderType orderType, byte orderSubtype, OrderAccountType orderAccountType)
        {
            List<string> result = new List<string>();
            this.Use(client => result = client.GetDepositAndCurrentAccountCurrenciesAsync(orderType, orderSubtype, orderAccountType).Result);
            return result;
        }

        public double GetCustomerAvailableAmount(string currency)
        {
            double result = 0;
            this.Use(client => result = client.GetCustomerAvailableAmountAsync(currency).Result);
            return result;
        }

        public double GetCreditLineProvisionAmount(double amount, string loanCurrency, string provisionCurrency, bool mandatoryPayment, int creditLineType,
            double kursForLoan, double kursForProvision)
        {
            double result = 0;
            double percent = 0;
            this.Use(client =>
            {
                percent = client.GetDepositLoanCreditLineAndProfisionCoefficentAsync(loanCurrency, provisionCurrency, mandatoryPayment, creditLineType).Result;
                result = amount / percent * kursForLoan / kursForProvision;
                result = Math.Round(result, 0);
            });
            return result;
        }

        public double GetDepositLoanAndProvisionAmount(double amount, string loanCurrency, string provisionCurrency, double kursForLoan, double kursForProvision)
        {
            double result = 0;
            double percent = 0;
            this.Use(client =>
            {
                percent = client.GetDepositLoanAndProvisionCoefficentAsync(loanCurrency, provisionCurrency).Result;
                result = amount / percent * kursForLoan / kursForProvision;
                result = Math.Round(result, 0);
            });
            return result;
        }

        public double GetRedemptionAmountForDepositLoan(double startCapital, double interestRate, DateTime dateOfBeginning, DateTime dateOfNormalEnd, DateTime firstRepaymentDate)
        {
            double result = 0;
            this.Use(client => result = client.GetRedemptionAmountForDepositLoanAsync(startCapital, interestRate, dateOfBeginning, dateOfNormalEnd, firstRepaymentDate).Result);
            return result;
        }

        public double GetCommisionAmountForDepositLoan(double startCapital, DateTime dateOfBeginning, DateTime dateofNormalEnd, string currency)
        {
            double result = 0;
            this.Use(client => result = client.GetCommisionAmountForDepositLoanAsync(startCapital, dateOfBeginning, dateofNormalEnd, currency).Result);
            return result;
        }

        public List<ActionResult> SaveGroupPayment(GroupPayment groupPayment)
        {
            List<ActionResult> result = new List<ActionResult>();
            this.Use(client => result = client.SaveGroupPaymentAsync(groupPayment).Result);
            return result;
        }

        public Account GetCreditLineConnectAccount(string LoanFullNumber)
        {
            Account result = new Account();
            CreditLine creditLine = new CreditLine();
            this.Use(client =>
            {
                creditLine = client.GetCreditLineByAccountNumberAsync(LoanFullNumber).Result;
                result = creditLine.ConnectAccount;
            });
            return result;
        }

        public List<CustomerPhone> GetPhoneNumbers(ulong customerNumber)
        {
            List<CustomerPhone> result = new List<CustomerPhone>();
            this.Use(client => result = client.GetCustomerMainDataAsync(customerNumber).Result.Phones);
            return result;
        }

        public List<CustomerEmail> GetEmails(ulong customerNumber)
        {
            List<CustomerEmail> result = new List<CustomerEmail>();
            this.Use(client => result = client.GetCustomerMainDataAsync(customerNumber).Result.Emails);
            return result;
        }

        public ActionResult SaveCardToCardOrderTemplate(CardToCardOrderTemplate template)
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            template.TemplateCustomerNumber = authorizedCustomer.CustomerNumber;
            ActionResult result = new ActionResult();
            template.TemplateCustomerNumber = authorizedCustomer.CustomerNumber;
            this.Use(client => result = client.SaveCardToCardOrderTemplateAsync(template).Result);
            return result;
        }

        public List<Order> GetOrdersList(OrderListFilter order)
        {
            List<Order> result = new List<Order>();
            this.Use(client => result = client.GetOrdersListAsync(order).Result);
            return result;
        }

        public List<ExchangeRate> GetExchangeRates()
        {
            List<ExchangeRate> result = new List<ExchangeRate>();
            this.Use(client => result = client.GetExchangeRatesAsync().Result);
            return result;
        }

        public ActionResult SaveCurrencyExchangeOrder(CurrencyExchangeOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveCurrencyExchangeOrderAsync(order).Result);
            return result;
        }

        public ActionResult SaveInternationalOrderTemplate(InternationalOrderTemplate template)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveInternationalOrderTemplateAsync(template).Result);
            return result;
        }

        public InternationalOrderTemplate GetInternationalOrderTemplate(int templateId)
        {
            InternationalOrderTemplate result = new InternationalOrderTemplate();
            this.Use(client => result = client.GetInternationalOrderTemplateAsync(templateId).Result);
            return result;
        }

        public ActionResult CheckExcelRows(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails, string debetAccount, long orderId)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.CheckExcelRowsAsync(reestrTransferAdditionalDetails, debetAccount, orderId).Result);
            return result;
        }

        public ActionResult SaveReestrTransferOrder(ReestrTransferOrder order, string fileId)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveReestrTransferOrderAsync(order, fileId).Result);
            return result;
        }

        public string GetInternationalTransferSentTime(int docId)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetInternationalTransferSentTimeAsync(docId).Result);
            return result;
        }

        public List<KeyValuePair<string, string>> GetCommunalReportParametersIBanking(long orderId, CommunalTypes communalTypes)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            this.Use(client => result = client.GetCommunalReportParametersIBankingAsync(orderId, communalTypes).Result);
            return result;
        }

        public byte[] GetDepositLoanOrDepositCreditLineContract(string loanNumber, byte type)
        {
            byte[] result = null;
            this.Use(client => result = client.GetDepositLoanOrDepositCreditLineContractAsync(loanNumber, type).Result);
            return result;
        }

        public byte[] GetOpenedAccountContract(string accountNumber)
        {
            byte[] result = null;
            this.Use(client => result = client.GetOpenedAccountContractAsync(accountNumber).Result);
            return result;
        }

        public DepositRateTariff GetDepositRateTariff(DepositType depositType)
        {
            DepositRateTariff result = new DepositRateTariff();
            this.Use(client => result = client.GetDepositRateTariffAsync(depositType).Result);
            return result;
        }

        public int GetTransferTypeByAppId(ulong appId)
        {
            int result = 0;
            this.Use(client => result = client.GetTransferTypeByAppIdAsync(appId).Result);
            return result;
        }

        public ActionResult SaveAndApproveTokenOrder(HBServletRequestOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveAndApproveHBServletRequestOrderAsync(order).Result);
            return result;
        }

        public ActionResult SaveAndApproveHBApplicationReplacmentOrder(HBApplicationOrder order, ref HBToken token, ulong id, int userId, string tokenSerial, double dayLimit, double transLimit)
        {
            HBToken hBToken = new HBToken();
            HBToken deactivatehbToken = new HBToken();
            HBUser hBUser = new HBUser();
            CustomerTokenInfo customerTokenInfo = _cacheHelper.GetCustomerTokenInfo();
            token = null;
            ActionResult result = new ActionResult();
            short filialCode = 0;
            this.Use(client =>
            {
                HBApplication hBApplication = GetHBApplication();
                order.HBApplication = new HBApplication
                {
                    ApplicationDate = hBApplication.ApplicationDate,
                    ChiefAcc = hBApplication.ChiefAcc,
                    ContractDate = hBApplication.ContractDate,
                    ContractNumber = hBApplication.ContractNumber,
                    ContractType = hBApplication.ContractType,
                    CustomerNumber = hBApplication.CustomerNumber,
                    Description = hBApplication.Description,
                    FilialCode = hBApplication.FilialCode,
                    FullName = hBApplication.FullName,
                    ID = hBApplication.ID,
                    InvolvingSetNumber = hBApplication.InvolvingSetNumber,
                    Manager = hBApplication.Manager,
                    Position = hBApplication.Position,
                    Quality = hBApplication.Quality,
                    QualityDescription = hBApplication.QualityDescription,
                    SetID = hBApplication.SetID,
                    SetName = hBApplication.SetName,
                    StatusChangeDate = hBApplication.StatusChangeDate,
                    StatusChangeSetID = hBApplication.StatusChangeSetID,
                    PermissionType = hBApplication.PermissionType
                };
                order.HBApplicationUpdate = new HBApplicationUpdate();
                order.HBApplicationUpdate.AddedItems = new List<object>();
                order.HBApplicationUpdate.DeactivatedItems = new List<object>();
                order.HBApplicationUpdate.UpdatedItems = new List<object>();

                filialCode = client.GetCustomerFilialAsync().Result;

                hBUser = client.GetHBUserAsync(userId).Result;
                hBToken = CreateToken(filialCode, id, HBTokenSubType.Replacement, dayLimit, transLimit);
                hBToken.HBUser = hBUser;
                hBToken.HBUser.Email.email.emailAddress = customerTokenInfo.Email;
                deactivatehbToken = client.GetHBTokenWithSerialNumberAsync(tokenSerial).Result;
                order.OperationDate = client.GetCurrentOperDayAsync().Result;
                order.FilialCode = 22000;
                order.HBApplicationUpdate.AddedItems.Add(hBToken);
                order.HBApplicationUpdate.DeactivatedItems.Add(deactivatehbToken);
                order.Type = XBS.OrderType.HBApplicationUpdateOrder;
                order.RegistrationDate = DateTime.Now;
                order.SubType = 1;
                result = client.SaveAndApproveHBApplicationReplacmentOrderAsync(order).Result;
            });
            token = hBToken;
            return result;
        }
        public ActionResult SaveAndApproveHBApplicationNewOrder(HBApplicationOrder order, out HBToken token, ulong id, int userId, double dayLimit, double transLimit)
        {
            ActionResult result = new ActionResult()
            {
                Errors = new List<ActionError>()
            };
            HBToken hBToken = new HBToken()
            {
                HBUser = new HBUser()
            };
            CustomerTokenInfo customerTokenInfo = _cacheHelper.GetCustomerTokenInfo();
            short filialCode = 0;
            Use(client =>
            {
                order.HBApplication = GetHBApplication();
                filialCode = client.GetCustomerFilialAsync().Result;
                hBToken = CreateToken(filialCode, id, HBTokenSubType.New, dayLimit, transLimit);
                hBToken.HBUser = client.GetHBUserAsync(userId).Result;
                hBToken.HBUser.Email.email.emailAddress = customerTokenInfo.Email;
                order.OperationDate = client.GetCurrentOperDayAsync().Result;
                order.FilialCode = 22000;
            });
            token = hBToken;
            if ((order?.HBApplication?.ID ?? 0) != 0)
            {
                order.HBApplicationUpdate.AddedItems.Add(hBToken);
                order.Type = XBS.OrderType.HBApplicationUpdateOrder;
                order.RegistrationDate = DateTime.Now;
                order.SubType = 1;
                this.Use(client => result = client.SaveAndApproveHBApplicationReplacmentOrderAsync(order).Result);
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(new ActionError() { Code = 1740 });
            }
            return result;
        }

        public HBApplication GetHBApplication()
        {
            HBApplication result = new HBApplication();
            this.Use(client => result = client.GetHBApplicationAsync().Result);
            return result;
        }
        private HBToken CreateToken(short fillialCode, ulong id, HBTokenSubType tokenSubType, double dayLimit, double transLimit)
        {
            HBToken hBToken = new HBToken();
            string tokenNumber = "";
            this.Use(client => tokenNumber = client.GetTempTokenListAsync(1).Result.FirstOrDefault());
            hBToken.ID = Convert.ToInt32(id);
            hBToken.TokenType = HBTokenTypes.MobileBanking;
            hBToken.TokenSubType = tokenSubType;
            hBToken.GID = "03"; //GID 01 Fiz. token, 02 mobile token (chi gorcum), 03 mobile banking
            hBToken.Quality = HBTokenQuality.StillNotConfirmed;
            hBToken.DayLimit = dayLimit  != 0 ? dayLimit : 400000;
            hBToken.TransLimit = transLimit != 0 ? transLimit : 400000;
            hBToken.TokenSubTypeDescription = "Մոբայլ բանկինգ";
            hBToken.TokenNumber = tokenNumber;
            return hBToken;
        }
        public List<HBActivationRequest> GetHBRequests()
        {
            List<HBActivationRequest> list = new List<HBActivationRequest>();
            this.Use(client => list = client.GetHBRequestsAsync().Result);
            return list;
        }

        public DateTime GetCurrentOperDay()
        {
            DateTime result = new DateTime();
            this.Use(client => result = client.GetCurrentOperDayAsync().Result);
            return result;
        }

        public ActionResult SaveAndApproveHBActivationOrder(HBActivationOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveAndApproveHBActivationOrderAsync(order).Result);
            return result;
        }

        public string GetTerm(short id, List<string> param, Languages language)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetTermAsync(id, param, language).Result);
            return result;
        }

        public List<Card> GetLinkedCards()
        {
            List<Card> result = new List<Card>();
            this.Use(client => result = client.GetLinkedCardsAsync().Result);
            return result;
        }

        public List<Card> GetLinkedAndAttachedCards(ulong productId, ProductQualityFilter productFilter = ProductQualityFilter.Opened)
        {
            List<Card> result = new List<Card>();
            this.Use(client => result = client.GetLinkedAndAttachedCardsAsync(productId, productFilter).Result);
            return result;
        }

        public List<Account> GetClosedAccounts()
        {
            List<Account> result = new List<Account>();
            this.Use(client => result = client.GetClosedAccountsAsync().Result);
            return result;
        }

        public ulong GetAccountCustomerNumber(string accountNumber)
        {
            ulong result = 0;
            this.Use(client => result = client.GetAccountCustomerNumberAsync(new Account() { AccountNumber = accountNumber }).Result);
            return result;
        }

        public ulong GetCardCustomerNumber(string cardNumber)
        {
            ulong result = 0;
            this.Use(client => result = client.GetCardCustomerNumberAsync(cardNumber).Result);
            return result;
        }

        public bool HasCustomerOnlineBanking(ulong customerNumber)
        {
            bool result = false;
            this.Use(client => result = client.HasCustomerOnlineBankingAsync(customerNumber).Result);
            return result;
        }

        public bool IsPoliceAccount(string accountNumber)
        {
            bool result = false;
            this.Use(client => result = client.IsPoliceAccountAsync(accountNumber).Result);
            return result;
        }

        public CurrencyExchangeOrder GetCurrencyExchangeOrder(long id)
        {
            CurrencyExchangeOrder result = new CurrencyExchangeOrder();

            this.Use(client => { result = client.GetCurrencyExchangeOrderAsync(id).Result; }
            );
            return result;
        }

        public CardStatus GetCardStatus(ulong productId)
        {
            CardStatus result = new CardStatus();
            this.Use(client => result = client.GetCardStatusAsync(productId).Result);
            return result;
        }

        public string GetCardMotherName(ulong productId)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetCardMotherNameAsync(productId).Result);
            return result;
        }

        public CardAdditionalInfo GetCardAdditionalInfo(ulong productId)
        {
            CardAdditionalInfo result = new CardAdditionalInfo();
            this.Use(client => result = client.GetCardAdditionalInfoAsync(productId).Result);
            return result;
        }

        public double GetCashBackAmount(ulong productId)
        {
            double result = 0;
            this.Use(client => result = client.GetCashBackAmountAsync(productId).Result);
            return result;
        }

        public LoanStatement GetLoanStatement(string account, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            LoanStatement result = new LoanStatement();
            this.Use(client =>
            {
                result = client.GetLoanStatementAsync(account, dateFrom, dateTo, minAmount, maxAmount, debCred,
                    transactionsCount, orderByAscDesc).Result;
            }
            );

            if (minAmount == -1 && maxAmount == -1 && orderByAscDesc == 0)
            {
                result?.Transactions?.Sort((x, y) => y.OperationDate.CompareTo(x.OperationDate));

            }
            else if (orderByAscDesc == 1) //գումարի աճման
            {
                result?.Transactions?.Sort((x, y) => x.OperationAmount.CompareTo(y.OperationAmount));
            }
            else if (orderByAscDesc == 2) //գումարի նվազման
            {
                result?.Transactions?.Sort((x, y) => y.OperationAmount.CompareTo(x.OperationAmount));
            }
            return result;
        }

        public byte[] GetExistingDepositContract(long id, int type)
        {
            byte[] result = null;
            this.Use(client => result = client.GetExistingDepositContractAsync(id, type).Result);
            return result;
        }

        public PensionSystem GetPensionSystemBalance()
        {
            PensionSystem result = new PensionSystem();
            this.Use(client => result = client.GetPensionSystemBalanceAsync().Result);
            return result;
        }

        public ChequeBookOrder GetChequeBookOrder(long id)
        {
            ChequeBookOrder order = null;
            this.Use(client => order = client.GetChequeBookOrderAsync(id).Result);
            return order;
        }

        public StatmentByEmailOrder GetStatmentByEmailOrder(long id)
        {
            StatmentByEmailOrder order = null;
            this.Use(client => order = client.GetStatmentByEmailOrderAsync(id).Result);
            return order;
        }

        public DepositOrder GetDepositorder(long id)
        {
            DepositOrder order = null;
            this.Use(client => order = client.GetDepositorderAsync(id).Result);
            return order;
        }

        public CardTariff GetCardTariff(ulong productId)
        {
            CardTariff cardTariff = null;
            this.Use(client => cardTariff = client.GetCardTariffAsync(productId).Result);
            return cardTariff;
        }

        public ActionResult SaveCVVNote(ulong productId, string CVVNote)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SaveCVVNoteAsync(productId, CVVNote).Result;
            });
            return result;
        }


        public bool ValidateProductId(ulong productId, ProductType productType)
        {
            bool access = false;
            this.Use(client => access = client.ValidateProductIdAsync(productId, productType).Result);
            return access;
        }
        public bool ValidateDocId(long docId)
        {
            bool access = false;
            this.Use(client => access = client.ValidateDocIdAsync(docId).Result);
            return access;
        }
        public bool ValidateAccountNumber(string accountNumber)
        {
            bool access = false;
            this.Use(client => access = client.ValidateAccountNumberAsync(accountNumber).Result);
            return access;
        }

        public string GetCVVNote(ulong productId)
        {
            string result = "";
            this.Use(client =>
            {
                result = client.GetCVVNoteAsync(productId).Result;
            });
            //HasProductPermission
            return result;
        }

        public bool ValidateCardNumber(string cardNumber)
        {
            bool access = false;
            this.Use(client => access = client.ValidateCardNumberAsync(cardNumber).Result);
            return access;
        }

        public bool HasProductPermission(string accountNumber)
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            List<HBProductPermission> _userProductPermission = GetUserProductsPermissions(authorizedCustomer.UserName);
            bool hasPermission = false;
            if (_userProductPermission.Exists(m => m.ProductAccountNumber == accountNumber && m.ProductType != HBProductPermissionType.Periodic))
            {
                hasPermission = true;
            }
            return hasPermission;
        }

        public bool HasProductPermission(string accountNumber, ulong productID)
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            List<HBProductPermission> _userProductPermission = GetUserProductsPermissions(authorizedCustomer.UserName);
            bool hasPermission = false;
            if (_userProductPermission.Exists(m => m.ProductAccountNumber == accountNumber && m.ProductAppID == productID))
            {
                hasPermission = true;
            }
            return hasPermission;
        }

        public bool HasProductPermissionByProductID(ulong productID)
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            List<HBProductPermission> _userProductPermission = GetUserProductsPermissions(authorizedCustomer.UserName);
            bool hasPermission = false;
            if (_userProductPermission.Exists(m => m.ProductAppID == productID))
            {
                hasPermission = true;
            }
            return hasPermission;
        }

        public AccountReOpenOrder GetAccountReOpenOrder(long Id)
        {
            AccountReOpenOrder result = new AccountReOpenOrder();
            this.Use(client => result = client.GetAccountReOpenOrderAsync(Id).Result);
            return result;
        }
        public ActionResult SaveAccountReOpenOrder(AccountReOpenOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveAccountReOpenOrderAsync(order).Result);
            return result;
        }
        public ActionResult ApproveAccountReOpenOrder(AccountReOpenOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ApproveAccountReOpenOrderAsync(order).Result);
            return result;
        }

        public ushort GetCrossConvertationVariant(string debitCurrency, string creditCurrency)
        {
            ushort result = 0;
            this.Use(client => result = client.GetCrossConvertationVariantAsync(debitCurrency, creditCurrency).Result);
            return result;
        }

        public VirtualCardDetails GetVirtualCardDetails(string cardNumber)
        {
            VirtualCardDetails result = new VirtualCardDetails();
            this.Use(client => result = client.GetVirtualCardDetailsAsync(cardNumber, _cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
            return result;
        }

        public ActionResult ActivateAndOpenProductAccounts(ulong productId)
        {
            ActionResult result = new ActionResult();

            this.Use(client =>
            {
                result = client.ActivateAndOpenProductAccountsAsync(productId, _cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result;
            });

            return result;
        }

        public bool ManuallyRateChangingAccess(double amount, string currency)
        {
            bool result = false;
            this.Use(client => result = client.ManuallyRateChangingAccessAsync(amount, currency, "AMD",_cacheHelper.GetSourceType()).Result);
            return result;
        }

        public List<Account> GetAccountsForCurrency(string currency)
        {
            List<Account> accounts = new List<Account>();
            this.Use(client => accounts = client.GetAccountsForOrderAsync(1, 2, 1).Result.FindAll(m => m.AccountType == 10 && m.Currency == currency));
            return accounts;
        }
        public IBankingHomePage GetIBankingHomePage()
        {
            IBankingHomePage homePage = new IBankingHomePage();
            this.Use(client => homePage = client.GetIBankingHomePageAsync().Result);
            
            return homePage;
        }
        public List<CreditLine> GetCreditLines(ProductQualityFilter filter)
        {
            List<CreditLine> creditLines = new List<CreditLine>();
            this.Use(client => creditLines = client.GetCreditLinesAsync(filter).Result);
            return creditLines;
        }

        public List<EmployeeSalary> GetEmployeeSalaryList(DateTime startDate, DateTime endDate)
        {
            List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
            this.Use(client => employeeSalaries = client.GetEmployeeSalaryListAsync(startDate, endDate).Result);
            return employeeSalaries;
        }
        public EmployeeSalaryDetails GetEmployeeSalaryDetails(int ID)
        {
            EmployeeSalaryDetails employeeSalaryDetails = new EmployeeSalaryDetails();
            this.Use(client => employeeSalaryDetails = client.GetEmployeeSalaryDetailsAsync(ID).Result);
            return employeeSalaryDetails;
        }

        public EmployeePersonalDetails GetEmployeePersonalDetails()
        {
            EmployeePersonalDetails employeePersonalDetails = new EmployeePersonalDetails();
            this.Use(client => employeePersonalDetails = client.GetEmployeePersonalDetailsAsync().Result);
            return employeePersonalDetails;
        }

        public bool IsPOSAccount(string accountNumber)
        {
            bool result = false;

            this.Use(client =>
            {
                result = client.IsPOSAccountAsync(accountNumber).Result;

            }
            );
            return result;
        }

        public ActionResult SaveBudgetPaymentOrderTemplate(BudgetPaymentOrderTemplate template)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveBudgetPaymentOrderTemplateAsync(template).Result);
            return result;
        }

        public List<Card> GetCardsForNewCreditLine(OrderType orderType)
        {
            List<Card> result = new List<Card>();
            this.Use(client => { result = client.GetCardsForNewCreditLineAsync(orderType).Result; }
            );
            return result;
        }

        public bool IsEmployee(ulong customerNumber)
        {
            bool result = false;

            this.Use(client =>
            {
                result = client.IsEmployeeAsync(customerNumber).Result;
            }
            );
            return result;
        }

        public ActionResult SavePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.SavePlasticCardSMSServiceOrderAsync(order).Result;
            }
            );
            return result;
        }
        public ActionResult ApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ApprovePlasticCardSMSServiceOrderAsync(order).Result;
            }
            );
            return result;
        }
        public PlasticCardSMSServiceOrder GetPlasticCardSMSServiceOrder(long orderID)
        {
            PlasticCardSMSServiceOrder result = new PlasticCardSMSServiceOrder();
            this.Use(client =>
            {
                result = client.GetPlasticCardSMSServiceOrderAsync(orderID).Result;
            }
            );
            return result;
        }
        public List<float> GetPlasticCardSMSServiceTariff(ulong productId)
        {
            List<float> result = new List<float>();
            this.Use(client => result = client.GetPlasticCardSMSServiceTariffAsync(productId).Result);
            return result;
        }

        public byte[] GetExistingLoansDramContract(string accountNumber)
        {
            byte[] result = null;
            this.Use(client => result = client.LoansDramContractAsync(accountNumber).Result);
            return result;
        }

        public PlasticCardSMSServiceHistory GetPlasticCardSMSServiceHistory(string cardNumber)
        {
            PlasticCardSMSServiceHistory result = new PlasticCardSMSServiceHistory();
            this.Use(client => result = client.GetPlasticCardSMSServiceHistoryAsync(cardNumber).Result);
            return result;
        }

        public ActionResult SaveCurrencyExchangeOrderTemplate(CurrencyExchangeOrderTemplate template)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.SaveCurrencyExchangeOrderTemplateAsync(template).Result);
            return result;
        }
        public double GetServiceProvidedOrderFee(ushort index)
        {
            double result = 0;
            this.Use(client => result = client.GetServiceProvidedOrderFeeAsync(OrderType.FeeForServiceProvided, index).Result);
            return result;
        }

        /// <summary>
        /// Պայմանագրի առկայության ստուգում
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="type">0 վարկի դեպքում, 1 վարկային գծի դեպքում</param>
        /// <returns></returns>
        public bool HasUploadedContract(string accountNumber, byte type)
        {
            bool result = false;
            this.Use(client => result = client.HasUploadedContractAsync(accountNumber, type).Result);
            return result;
        }

        public OrderAttachment GetMessageAttachmentById(int msgId)
        {
            OrderAttachment result = new OrderAttachment();
            this.Use(client => result = client.GetMessageAttachmentByIdAsync(msgId).Result);
            return result;
        }

        public DigitalAccountRestConfigurations GetCustomerAccountRestConfig(int digitalUserId)
        {
            DigitalAccountRestConfigurations result = new DigitalAccountRestConfigurations();
            this.Use(client => result = client.GetCustomerAccountRestConfigAsync(digitalUserId).Result);
            return result;
        }

        public ActionResult UpdateCustomerAccountRestConfig(List<DigitalAccountRestConfigurationItem> ConfigurationItems)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.UpdateCustomerAccountRestConfigAsync(ConfigurationItems).Result);
            return result;
        }

        public ActionResult ResetCustomerAccountRestConfig(int digitalUserId)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.ResetCustomerAccountRestConfigAsync(digitalUserId).Result);
            return result;
        }

        public ActionResult RejectOrder(OrderRejection rejection)
        {
            ActionResult result = new ActionResult();
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            rejection.UserId = authorizedCustomer.UserId;
            this.Use(client => result = client.RejectOrderAsync(rejection).Result);
            return result;
        }

        public string GetOrderAttachmentInBase64(string attachememntId)
        {
            string result = null;
            this.Use(client => result = client.GetOrderAttachmentInBase64Async(attachememntId).Result);
            return result;
        }

        public long GetCardProductId(string cardNumber, ulong customerNumber)
        {
            long result = 0;
            this.Use(client => result = client.GetCardProductIdAsync(cardNumber, customerNumber).Result);
            return result;
        }


        public ActionResult MigrateOldUserToCas(int hbUserId)
        {
            ActionResult result = new ActionResult();
            this.Use(client => result = client.MigrateOldUserToCasAsync(hbUserId).Result);
            return result;
        }

        public string GetLoansDramContract(long docId, int productType, bool fromApprove, ulong customerNumber)
        {
            byte[] result = null;
            this.Use(client => result = client.GetLoansDramContractAsync(docId, productType, fromApprove, customerNumber).Result);
            return Convert.ToBase64String(result);
        }

        public string PrintDepositLoanContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            byte[] result = null;
            this.Use(client => result = client.PrintDepositLoanContractAsync(docId, customerNumber, fromApprove).Result);
            return Convert.ToBase64String(result);
        }
        public ulong GetCardProductIdByAccountNumber(string cardAccountNumber, ulong customerNumber)
        {
            ulong result = 0;
            this.Use(client => result = client.GetCardProductIdByAccountNumberAsync(cardAccountNumber, customerNumber).Result);
            return result;
        }

        public double GetOrderServiceFee(OrderType type, int urgent)
        {
            double fee = 0;
            this.Use(client => fee = client.GetOrderServiceFeeAsync(type, urgent).Result);
            return fee;
        }

        public int GetCardSystem(string cardNumber)
        {
            int cardSystem = 0;
            this.Use(client => cardSystem = client.GetCardSystemAsync(cardNumber).Result);
            return cardSystem;
        }
        public List<Card> GetnotActivatedVirtualCards()
        {
            List<Card> result = new List<Card>();
            this.Use(client => result = client.GetNotActivatedVirtualCardsAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
            return result;
        }
        public string GetCardNumber(long productid)
        {
            string cardNumber = null;
            this.Use(client => cardNumber = client.GetCardNumberAsync(productid).Result);
            return cardNumber;
        }
        public List<Card> GetClosedCardsForDigitalBanking()
        {
            List<Card> cards = new List<Card>();
            this.Use(client => cards = client.GetClosedCardsForDigitalBankingAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
            return cards;
        }

        public string GetLiabilitiesAccountNumberByAppId(ulong appId)
        {
            string accountNumber = string.Empty;
            this.Use(client => accountNumber = client.GetLiabilitiesAccountNumberByAppIdAsync(appId).Result);
            return accountNumber;
        }

        public List<SearchBudgetAccount> GetSearchedBudgetAccount(SearchBudgetAccount searchParams)
        {
            List<SearchBudgetAccount> result = new List<SearchBudgetAccount>();

            this.Use(client => { result = client.GetSearchedBudgetAccountAsync(searchParams).Result; }
           );
            
            return result;
        }
       

        public List<OrderAttachment> GetOrderAttachments(long orderId)
        {
            List<OrderAttachment> result = new List<OrderAttachment>();
            this.Use(client => result = client.GetOrderAttachmentsAsync(orderId).Result);
            return result;
        }

        
        public async Task<string> GetStatement(string accountNumber, DateTime dateFrom, DateTime dateTo, string exportFormat)
        {
            string getStatement = "";
            this.Use(client => getStatement = client.GetStatementAsync(accountNumber, dateFrom, dateTo, (byte)ReportService.GetExportFormatEnumeration(exportFormat)).Result);
            return getStatement;
        }

        public InternationalOrderPrefilledData GetInternationalOrderPrefilledData()
        {
            InternationalOrderPrefilledData data = new InternationalOrderPrefilledData();
            this.Use(client => data = client.GetInternationalOrderPrefilledDataAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
            return data;
        }

        public List<SearchSwiftCodes> GetSearchedSwiftCodes(string swiftCode)
        {
            SearchSwiftCodes codes = new SearchSwiftCodes { SwiftCode = swiftCode };
            List<SearchSwiftCodes> result = new List<SearchSwiftCodes>();
            this.Use(client => result = client.GetSearchedSwiftCodesAsync(codes).Result);
            return result;
        }

        public string GetOrderRejectReason(long orderId, OrderType type)
        {
            string result = "";
            this.Use(client => result = client.GetOrderRejectReasonAsync(orderId, type).Result);
            return result;
        }

        public OrderAttachment GetOrderAttachment(string attachmentId)
        {
            OrderAttachment result = new OrderAttachment();
            this.Use(client => result = client.GetOrderAttachmentAsync(attachmentId).Result);
            return result;
        }

        public ActionResult ValidateAttachCard(string cardNumber, ulong customerNumber, string cardHolderName)
        {
            ActionResult result = new ActionResult();
            this.Use(client =>
            {
                result = client.ValidateAttachCardAsync(cardNumber, customerNumber, cardHolderName).Result;
            });
            return result;
        }

        public void SendReminderNote(ulong customerNumber)
        {
            this.Use(client => client.SendReminderNoteAsync(customerNumber));
        }

        public bool IsAbleToApplyForLoan(LoanProductType type)
        {
            bool result = false;
            this.Use(client => { result = client.IsAbleToApplyForLoanAsync(type).Result; }
            );
            return result;
        }

        public List<Account> GetAccountsDigitalBanking()
        {
            List<Account> result = new List<Account>();
            this.Use(client => { result = client.GetAccountsDigitalBankingAsync().Result; }
            );
            return result;
        }

        public double GetMaxAvailableAmountForNewCreditLine(double productId, int creditLineType, string provisionCurrency, bool existRequiredEntries, ulong customerNumber)
        {
            double result = 0;
            this.Use(client =>
            {
                result = client.GetMaxAvailableAmountForNewCreditLineAsync(productId, creditLineType, provisionCurrency, existRequiredEntries, customerNumber).Result;
            });
            return result;
        }

        public double GetMaxAvailableAmountForNewLoan(string provisionCurrency, ulong customerNumber)
        {
            double result = 0;
            this.Use(client =>
            {
                result = client.GetMaxAvailableAmountForNewLoanAsync(provisionCurrency, customerNumber).Result;
            });

            return result;
        }

        public double GetUserTotalAvailableBalance(int digitalUserId, ulong customerNumber)
        {
            double result = 0;
            this.Use(client => result = client.GetUserTotalAvailableBalanceAsync(digitalUserId, customerNumber, _cacheHelper.GetUser().userID).Result);
            return result;
        }

        public CreditLine GetCardOverDraft(string cardNumber)
        {
            CreditLine result = new CreditLine();
            this.Use(client => result = client.GetCardOverDraftAsync(cardNumber).Result);
            return result;
        }

        public string PrintDepositContract(long id, bool attachedFile)
        {
            byte[] result = null;
            this.Use(client => result = client.PrintDepositContractAsync(id, attachedFile).Result);
            return Convert.ToBase64String(result);
        }


        public List<ProductOtherFee> GetProductOtherFees(long productId)
        {
            List<ProductOtherFee> result = new List<ProductOtherFee>();


            this.Use(client => { result = client.GetProductOtherFeesAsync((ulong)productId).Result; });

            return result;
            
        }

        public Customer GetCustomer(ulong customerNumber)
        {
            Customer result = new Customer();

            this.Use(client => { result = client.GetCustomerAsync((ulong)customerNumber).Result; });

            return result;

        }

        public Dictionary<string, string> GetOrderDetailsForReport(long orderId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            this.Use(client => { result = client.GetOrderDetailsForReportAsync(orderId).Result; });

            return result;

        }

        public int GetCustomerTemplatesCount()
        {
            int count = 0;
            this.Use(clinet => count = clinet.GetCustomerTemplatesCountsAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber).Result);
            return count;
        }
        public int GetPeriodicTransfersCount(PeriodicTransferTypes transferType)
        {
            int count = 0;
            this.Use(client => count = client.GetPeriodicTransfersCountAsync(_cacheHelper.GetAuthorizedCustomer().CustomerNumber, transferType).Result);
            return count;
        }
    }
}
