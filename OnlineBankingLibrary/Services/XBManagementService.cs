using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineBankingLibrary.Utilities;
using System;
using System.ServiceModel;
using XBManagement;

namespace OnlineBankingLibrary.Services
{
    public class XBManagementService
    {
        private readonly IConfiguration _config;
        private readonly CacheHelper _authData;
        private readonly IHttpContextAccessor _contextAccessor;
        private string SessionId;
        private string clientIP;
        private byte language;
        public XBManagementService(IConfiguration config, CacheHelper authData, IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _authData = authData;
            _contextAccessor = contextAccessor;
        }

        public void Use(Action<IXBManagementService> action)
        {
            NetTcpBinding binding = new NetTcpBinding
            {
                MaxBufferPoolSize = 5242880,
                MaxBufferSize = 6553600,
                MaxReceivedMessageSize = 6553600
            };
            binding.ReaderQuotas.MaxArrayLength = 2500000;
            string endpointUrl = _config["WCFExternalServices:XBManagementService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            IXBManagementService client = ProxyManager<IXBManagementService>.GetProxy(nameof(IXBManagementService), binding, endpoint);
            User user = _authData.GetXBMUser();
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            if (!string.IsNullOrEmpty(_contextAccessor.HttpContext.Request.Headers["language"]))
                byte.TryParse(_contextAccessor.HttpContext.Request.Headers["language"], out language);
            if (SessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de" && Convert.ToBoolean(_config["TestVersion"]))
            {
                clientIP = "169.169.169.166";
            }
            else
            {
                clientIP = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            bool success = false;
            bool checkCustomerSession = true;

            try
            {
                checkCustomerSession = client.InitAsync(SessionId, language, clientIP, user, (SourceType)_authData.GetSourceType(), ServiceType.CustomerService).Result;
                if (!checkCustomerSession)
                {
                    throw new Exception();
                }
                else
                {
                    action(client);
                    ((IClientChannel)client).Close();
                    success = true;
                }
            }
            catch (FaultException)
            {
                ((IClientChannel)client).Close();
                throw;
            }
            catch (TimeoutException)
            {

            }
            catch (Exception)
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
        public ActionResult GenerateAcbaOnline(string userName, string password, ulong customerNumber, string phoneNumber, int customerQuality,string email)
        {
            var result = new ActionResult();
            this.Use(client => { result = client.GenerateAcbaOnlineAsync(userName, password, customerNumber, phoneNumber, customerQuality, email).Result; }
            );
            return result;
        }
        public ActionResult AddEmailForCustomer(string emailAddress, ulong customerNumber)
        {
            var result = new ActionResult();
            this.Use(client => { result = client.AddEmailForCustomerAsync(emailAddress, customerNumber).Result; }
            );
            return result;
        }

    }
}
