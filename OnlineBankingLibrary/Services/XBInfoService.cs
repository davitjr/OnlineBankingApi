using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using OnlineBankingLibrary.Models;
using XBS;
using XBSInfo;
using System.Linq;
using Newtonsoft.Json;
using OnlineBankingLibrary.Utilities;
using Microsoft.AspNetCore.Http;

namespace OnlineBankingLibrary.Services
{
    public class XBInfoService
    {
        private readonly IConfiguration _config;
        private readonly CacheHelper _cacheHelper;


        public XBInfoService(IConfiguration config, CacheHelper cacheHelper)
        {
            _config = config;
            _cacheHelper = cacheHelper;
        }



        public void Use(Action<IXBInfoService> action)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string endpointUrl = _config["WCFExternalServices:XBInfoService:EndpointAddress"];
            binding.MaxBufferPoolSize = 5242880;
            binding.MaxBufferSize = 6553600;
            binding.MaxReceivedMessageSize = 6553600;
            binding.ReaderQuotas.MaxArrayLength = 2500000;
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            IXBInfoService client = ProxyManager<IXBInfoService>.GetProxy(nameof(IXBInfoService), binding, endpoint);


            byte Language = _cacheHelper.GetLanguage();
            string ClientIP = _cacheHelper.GetClientIp();
            XBSInfo.SourceType Source = (XBSInfo.SourceType)_cacheHelper.GetSourceType();
            XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            XBSInfo.AuthorizedCustomer authorizedCustomerInfo = new XBSInfo.AuthorizedCustomer
            {
                CustomerNumber = authorizedCustomer.CustomerNumber,
                UserName = authorizedCustomer.UserName,
                UserId = authorizedCustomer.UserId

            };

            client.InitOnlineAsync(ClientIP, Language, Source, authorizedCustomerInfo).Wait();


            bool success = false;
            try
            {
                action(client);
                ((IClientChannel)client).Close();
                success = true;
            }
            catch (FaultException ex)
            {
                ((IClientChannel)client).Close();
                throw;
            }
            catch (TimeoutException ex)
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


        public List<KeyValuePair<string, string>> GetCountries()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => { result = client.GetCountriesAsync().Result; }
            );
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetEmbassyList(List<ushort> referenceTypes)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetEmbassyListAsync(referenceTypes).Result;
            });
            return result.ToList();
        }
        public List<KeyValuePair<string, string>> GetFilialList()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            List<KeyValuePair<long, String>> filialList = new List<KeyValuePair<long, string>>();
            this.Use(client =>
            {
                filialList = client.GetFilialAddressListAsync().Result;
            });

            filialList.ForEach(m =>
            {
                result.Add(m.Key.ToString(), m.Value);
            });

            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetReferenceLanguages()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetReferenceLanguagesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetReferenceTypes()
        {
            List<KeyValuePair<long, string>> referenceTypes = new List<KeyValuePair<long, string>>();
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                referenceTypes = client.GetReferenceTypesAsync().Result;
            });
            referenceTypes.ForEach(m =>
            {
                result.Add(m.Key.ToString(), m.Value);
            });

            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetListOfLoanApplicationAmounts()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetListOfLoanApplicationAmountsAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCommunicationTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCommunicationTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetRegions(int country)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetRegionsAsync(country).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetArmenianPlaces(int region)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetArmenianPlacesAsync(region).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetTransferSystemCurrency(int transferSystem)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetTransferSystemCurrencyAsync(transferSystem).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetFastTransferSystemTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetTransferTypesAsync(1).Result;
                result.Remove("12");
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetDepositTypeCurrencies(short depositType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetDepositTypeCurrencyAsync(depositType).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetActiveDepositTypesForNewDepositOrder(int accountType, int customerType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetActiveDepositTypesForNewDepositOrderAsync(accountType, customerType).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetJointTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetJointTypesAsync().Result;
                result.Remove("1");
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetCardClosingReasons()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCardClosingReasonsAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetCashOrderCurrencies()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCashOrderCurrenciesAsync().Result;
            });

            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetAssigneeOperationGroupTypes(int typeOfClient)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetAssigneeOperationGroupTypesAsync(typeOfClient).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCurrentAccountOrderCurrencies()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCurrentAccountCurrenciesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetStatementDeliveryTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetStatementDeliveryTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetDepositClosingReasonTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetDepositClosingReasonTypesAsync().Result;
                result = result.Where(r => r.Key != "17" && r.Key != "18").ToDictionary(r => r.Key, r => r.Value);
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetReasonsForCardTransactionAction()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            bool useBank = false;
            this.Use(client =>
            {
                result = client.GetReasonsForDigitalCardTransactionActionAsync(useBank).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetActionsForCardTransaction()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetActionsForCardTransactionAsync().Result;
            });
            result.Remove(key: "2");
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetAccountClosingReasons()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetAccountClosingReasonsHBAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCardLimitChangeOrderActionTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCardLimitChangeOrderActionTypesAsync().Result;
            });
            result.Remove("2");
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCommunalBranchList(XBSInfo.CommunalTypes communalType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (communalType == (XBSInfo.CommunalTypes)3 || communalType == (XBSInfo.CommunalTypes)4 ||
                communalType == (XBSInfo.CommunalTypes)5 || communalType == (XBSInfo.CommunalTypes)6 ||
                communalType == (XBSInfo.CommunalTypes)9)
            {
                this.Use(client =>
                {
                    result = client.GetCommunalBranchListAsync(communalType, XBSInfo.Languages.hy).Result;
                });
            }
            else
            {
                ActionResult res = new ActionResult();
                res.ResultCode = ResultCode.ValidationError;
                res.Errors = new List<ActionError>();

                ActionError actionError = new ActionError();
                actionError.Description = "Wrong CommunalType";

                res.Errors.Add(actionError);
            }

            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetSyntheticStatuses()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetSyntheticStatusesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCreditLineMandatoryInstallmentTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCreditLineMandatoryInstallmentTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetTransferMethod()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetTransferMethodAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCardSystemTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCardSystemTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCardTypes(int cardSystem)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCardTypesAsync(cardSystem).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCurrenciesPlasticCardOrder(ushort cardType, short periodicityType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCurrenciesPlasticCardOrderAsync(cardType, periodicityType).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCardReportReceivingTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCardReportReceivingTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCardPINCodeReceivingTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCardPINCodeReceivingTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCurrencies()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCurrenciesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetOrderTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetOrderTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetOrderQualityTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Dictionary<string, string> typesList = new Dictionary<string, string>();
            Dictionary<string, string> typesListNew = new Dictionary<string, string>();

            this.Use(client =>
            {
                typesList = client.GetOrderQualityTypesAsync().Result;
            });
            foreach (var item in typesList)
            {
                if (item.Key == "2" || item.Key == "5" || item.Key == "20" || item.Key == "32" || item.Key == "6" || item.Key == "31" || item.Key == "30"
                    || item.Key == "1" || item.Key == "50" || item.Key == "41")
                {
                    typesListNew.Add(item.Key != "2" ? item.Key : "3", item.Value);
                }
            }
            result = typesListNew;
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCurrenciesForReceivedFastTransfer()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCurrenciesForReceivedFastTransferAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetPeriodicityTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetPeriodicityTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetSentOrderQualityTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Dictionary<string, string> typesList = new Dictionary<string, string>();
            Dictionary<string, string> typesListNew = new Dictionary<string, string>();
            this.Use(client =>
            {
                typesList = client.GetOrderQualityTypesAsync().Result;
            });
            foreach (var item in typesList)
            {
                if (item.Key == "2" || item.Key == "5" || item.Key == "20" || item.Key == "32" || item.Key == "6" || item.Key == "31" || item.Key == "30" || item.Key == "50")
                {
                    typesListNew.Add(item.Key != "2" ? item.Key : "3", item.Value);
                }
            }

            result = typesListNew;
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCreditLineTypesForOnlineMobile(int cardType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                //poxelu entaka
                result = client.GetCreditLineTypesForOnlineMobileAsync(XBSInfo.Languages.hy).Result;
            });
            if (cardType != 35)
            {
                result.Remove("51");
            }
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }


        public List<KeyValuePair<string, string>> GetTemplateDocumentTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetTemplateDocumentTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetAttachedCardTypes(string mainCardNumber)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetAttachedCardTypesAsync(mainCardNumber).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetLTACodes()
        {
            Dictionary<string, string> ltaCodes = new Dictionary<string, string>();
            this.Use(client =>
            {
                ltaCodes = client.GetLTACodesAsync().Result;
            });
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in ltaCodes)
            {
                result.Add(item.Key, item.Key + " " + item.Value);
            }
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCredentialTypes(int typeOfCustomer, int customerFilialCode, int userFilialCode)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetCredentialTypesAsync(typeOfCustomer, customerFilialCode, userFilialCode).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<Tuple<int, int, string, bool>> GetAssigneeOperationTypes(int groupId, int typeOfClient)
        {
            List<Tuple<int, int, string, bool>> result = new List<Tuple<int, int, string, bool>>();
            this.Use(client => result = client.GetAssigneeOperationTypesAsync(groupId, typeOfClient).Result);
            return result;
        }

        public double GetPenaltyRateOfLoans(int productType, DateTime startDate)
        {
            double result = 0;
            this.Use(client => result = client.GetPenaltyRateOfLoansAsync(productType, startDate).Result);
            return result;
        }

        public DateTime GetFastOverdrafEndDate(DateTime startDate)
        {
            DateTime result = new DateTime();
            this.Use(client => result = client.GetFastOverdrafEndDateAsync(startDate).Result);
            return result;
        }

        public FastOverdraftDates GetFastOverdrafStartAndEndDate(DateTime startDate)
        {
            FastOverdraftDates result = new FastOverdraftDates();
            DateTime end = new DateTime();
            DateTime start = startDate;
            this.Use(client => end = client.GetFastOverdrafEndDateAsync(start).Result);
            result.StartDate = start;
            result.EndDate = end;
            return result;
        }

        public double GetFastOverdraftFeeAmount(double amount)
        {
            double result = 0;
            this.Use(client => result = client.GetFastOverdraftFeeAmountAsync(amount).Result);
            return result;
        }

        public string GetNewCardPassword(ulong customerNumber)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetCustomerLastMotherNameAsync(customerNumber).Result);
            if (result == null)
            {
                Random rd = new Random();
                string possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                string newCardPassword = String.Empty;
                for (int i = 0; i < 6; i++)
                {
                    newCardPassword += possible[rd.Next(possible.Length)];
                }
                result = newCardPassword;
            }
            return result;

        }

        public List<KeyValuePair<string, string>> GetCashOrderTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => result = client.GetCashOrderTypesAsync().Result);
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<XBSInfo.DepositOption> GetBusinesDepositOptions()
        {
            List<XBSInfo.DepositOption> result = new List<XBSInfo.DepositOption>();
            this.Use(client => result = client.GetBusinesDepositOptionsAsync().Result);
            return result;
        }

        public bool IsWorkingDay(DateTime dateWorkingDay)
        {
            bool result = false;
            this.Use(client => result = client.IsWorkingDayAsync(dateWorkingDay).Result);
            return result;
        }
        public string GetBankName(int code)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetBankAsync(code, XBSInfo.Languages.hy).Result);
            return result;
        }

        public XBSInfo.CardTariffContract GetCardTariffsByCardType(ushort cardType, short periodicityType)
        {
            XBSInfo.CardTariffContract result = new XBSInfo.CardTariffContract();
            this.Use(client => result = client.GetCardTariffsByCardTypeAsync(cardType, periodicityType).Result);
            return result;
        }

        public double GetCardServiceFee(int cardType, int officeId, string currency)
        {
            double result = 0;
            this.Use(client => result = client.GetCardServiceFeeAsync(cardType, officeId, currency).Result);
            return result;
        }

        public List<KeyValuePair<string, string>> GetPoliceCodes(string accountNumber = "")
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetPoliceCodesAsync(accountNumber).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<byte, string>> GetUtilityAbonentTypes()
        {
            List<KeyValuePair<byte, string>> result = new List<KeyValuePair<byte, string>>();

            this.Use(client =>
            {
                result = client.GetUtilityAbonentTypesAsync().Result;
            });


            return result;
        }

        public List<KeyValuePair<short, string>> GetUtilityPaymentTypesOnlineBanking()
        {
            List<KeyValuePair<short, string>> result = new List<KeyValuePair<short, string>>();

            this.Use(client =>
            {
                result = client.GetUtilityPaymentTypesOnlineBankingAsync().Result;
            });


            return result;
        }

        public List<KeyValuePair<string, string>> GetPeriodicsSubTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetPeriodicsSubTypesAsync(XBSInfo.Languages.hy).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<short, string>> GetPayIfNoDebtTypes()
        {
            List<KeyValuePair<short, string>> result = new List<KeyValuePair<short, string>>();

            this.Use(client =>
            {
                result = client.GetPayIfNoDebtTypesAsync().Result;
            });


            return result;
        }

        public ulong GetLastKeyNumber(ushort filialCode, int keyID)
        {
            ulong key = 0;
            this.Use(client =>
            {
                key = client.GetLastKeyNumberAsync(keyID, filialCode).Result;
            });
            return key;
        }

        public List<KeyValuePair<int, string>> GetServiceFeePeriodocityTypes()
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            this.Use(client =>
            {
                result = client.GetServiceFeePeriodocityTypesAsync().Result;
            });

            return result.ToList() ?? new List<KeyValuePair<int, string>>();
        }

        public List<KeyValuePair<string, string>> GetLinkedCardTariffsByCardType(ushort cardType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetLinkedCardTariffsByCardTypeAsync(cardType).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public XBSInfo.CardTariff GetAttachedCardTariffs(string mainCardNumber, uint cardType)
        {
            XBSInfo.CardTariff cardTariff = new XBSInfo.CardTariff();
            this.Use(client => cardTariff = client.GetAttachedCardTariffsAsync(mainCardNumber, cardType).Result);
            return cardTariff;
        }

        public List<KeyValuePair<string, string>> GetLoanMatureTypesForIBanking()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetLoanMatureTypesForIBankingAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<string, string>> GetTypeOfLoanRepaymentSource()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetTypeOfLoanRepaymentSourceAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }


        public List<KeyValuePair<string, string>> GetPlasticCardSmsServiceActions(string cardNumber)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => result = client.GetPlasticCardSmsServiceActionsAsync(cardNumber).Result);
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
            return null;
        }


        public List<KeyValuePair<string, string>> GetTypeOfPlasticCardsSMS()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client => result = client.GetTypeOfPlasticCardsSMSAsync().Result);
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetDetailsOfCharges()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            this.Use(client => result = client.GetDetailsOfChargesAsync().Result);
            return result ?? new List<KeyValuePair<string, string>>();
        }

        public string GetFilialName(int filialCode)
        {
            string result = String.Empty;
            this.Use(client => result = client.GetFilialNameAsync(filialCode).Result);
            return result;
        }

        public List<KeyValuePair<string, string>> GetDigitalOrderTypes(TypeOfHbProductTypes hbProductType)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetDigitalOrderTypesAsync(hbProductType).Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetStatementFrequency()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetStatementFrequencyAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetInternationalPaymentCurrencies()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            this.Use(client =>
            {
                result = client.GetInternationalPaymentCurrenciesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<string, string>>();
        }
        public List<KeyValuePair<byte, string>> GetTransferReceiverTypes()
        {
            Dictionary<byte, string> result = new Dictionary<byte, string>();
            this.Use(client =>
            {
                result = client.GetTransferReceiverTypesAsync().Result;
            });
            return result.ToList() ?? new List<KeyValuePair<byte, string>>();
        }

        public byte CommunicationTypeExistence(ulong customerNumber)
        {
            byte result = 0;
            this.Use(client => result = client.CommunicationTypeExistenceAsync(customerNumber).Result);
            return result;
        }

        public string GetCurrencyLetter(string currency, byte operationType)
        {
            string result = "";
            this.Use(client => result = client.GetCurrencyLetterAsync(currency, operationType).Result);
            return result;
        }

        public string GetMandatoryEntryInfo(byte id)
        {
            string result = string.Empty;
            this.Use(client => result = client.GetMandatoryEntryInfoAsync(id).Result);
            return result;
        }
    }
}
