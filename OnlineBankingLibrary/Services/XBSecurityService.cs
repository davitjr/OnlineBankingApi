using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using OnlineBankingLibrary.Models;
using XBSecurity;
using System.Data;
using LoginInfo = XBSecurity.LoginInfo;
using XBS;
using OnlineBankingLibrary.Utilities;

namespace OnlineBankingLibrary.Services
{
    public class XBSecurityService
    {
        private readonly IConfiguration _config;
        private readonly CacheHelper _cacheHelper;

        public XBSecurityService(IConfiguration config, CacheHelper cacheHelper)
        {
            _config = config;
            _cacheHelper = cacheHelper;
        }

        public void Use(Action<IOnlineBankingSecurityCas> action)
        {
            IOnlineBankingSecurityCas client;
            string endpointUrl = _config["WCFExternalServices:XBSecurityService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            
            if (endpointUrl.Contains("http://"))
            {
                client = ProxyManager<IOnlineBankingSecurityCas>.GetProxy(nameof(IOnlineBankingSecurityCas), new BasicHttpBinding(), endpoint);
            }
            else
            {
                client = ProxyManager<IOnlineBankingSecurityCas>.GetProxy(nameof(IOnlineBankingSecurityCas), new BasicHttpsBinding(), endpoint);
            }

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

        public OnlineBankingUser AuthorizeUser(LoginInfo loginInfo, byte lang)
        {
            var result = new OnlineBankingUser();
            this.Use(
                client => { result = client.AuthorizeUserAsync(loginInfo, lang).Result; }
            );
            return result;
        }

        public bool ValidateOTP(string sessionId, string OTP, string ipAddress, byte language)
        {
            bool result = false;
            this.Use(
                client => { result = client.VerifyTokenAsync(sessionId, OTP, ipAddress, language).Result; }
            );
            return result;
        }

        public bool CheckRegistrationCode(string registrationCode, string deviceTypeDescription, string userName)
        {
            bool result = false;
            this.Use(
                client => { result = client.CheckRegistrationCodeAsync(registrationCode, deviceTypeDescription,userName).Result; }
            );
            return result;
        }

        public OnlineBankingUser ChangeUserPassword(ChangePasswordInfo changePasswordInfo, string sessionId, byte language)
        {
            var result = new OnlineBankingUser();
            this.Use(
                client => { result = client.ChangeUserPasswordAsync(changePasswordInfo, sessionId, language).Result; }
            );
            return result;
        }

        public OnlineBankingUser CheckAuthorization(string sessionId, byte language)
        {
            var result = new OnlineBankingUser();
            this.Use(
                client => { result = client.CheckAuthorizationAsync(sessionId, language).Result; }
            );
            return result;
        }

        public bool SingData(string sessionId, string otp, Dictionary<string, string> signData, byte language)
        {
            var result = false;
            string userName = _cacheHelper.GetAuthorizedCustomer().UserName;
            string ipAddress = _cacheHelper.GetClientIp();

            this.Use(
                client => { result = client.SingDataAsync(sessionId, otp, signData, language, userName, ipAddress).Result; }
            );
            return result;
        }

        public OnlineBankingUser AuthorizeUserByUserPassword(XBSecurity.LoginInfo loginInfo, byte language, string hostName = "")
        {
            OnlineBankingUser onlineBankingUser = null;
            this.Use(
                client => { onlineBankingUser = client.AuthorizeUserByUserPasswordAsync(loginInfo, language, hostName).Result; }
            );
            return onlineBankingUser;
        }

        public OnlineBankingUser AuthorizeUserByToken(LoginInfo loginInfo, LoginResult loginResult, byte language, string hostName)
        {
            OnlineBankingUser onlineBankingUser = null;
            this.Use(
                client => { onlineBankingUser = client.AuthorizeUserByTokenAsync(loginInfo, loginResult, language, hostName).Result; }
            );
            return onlineBankingUser;
        }

        public bool ResetUserPassword(XBSecurity.LoginInfo loginInfo)
        {
            bool isReset = false;
            this.Use(
                client => { isReset = client.ResetUserPasswordAsync(loginInfo).Result; }
            );
            return isReset;
        }

        public void LogOut(string sessoinId)
        {
            this.Use(client => client.LogOutAsync(sessoinId));
        }
    }
}
