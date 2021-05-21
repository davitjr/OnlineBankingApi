using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using OnlineBankingLibrary.Models;
using PushNotificationService;
using System.Data;
using LoginInfo = XBSecurity.LoginInfo;


namespace OnlineBankingLibrary.Services
{
    public class XBSecurityPushNotificationService
    {
        private readonly IConfiguration _config;

        public XBSecurityPushNotificationService(IConfiguration config)
        {
            this._config = config;
           // BasicHttpsBinding binding = new BasicHttpsBinding();
           // string endpointUrl = _config["WCFExternalServices:PushNotificationService:EndpointAddress"];
           // EndpointAddress endpoint = new EndpointAddress(endpointUrl);
           //// NotificationServiceClient proxy = new NotificationServiceClient(binding, endpoint);
        }

        public void Use(Action<INotificationService> action)
        {
            //BasicHttpBinding binding = new BasicHttpBinding(); for local and development
            BasicHttpsBinding binding = new BasicHttpsBinding(); //for prodoction

            string endpointUrl = _config["WCFExternalServices:PushNotificationService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            INotificationService client = ProxyManager<INotificationService>.GetProxy(nameof(INotificationService), binding, endpoint);

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

        public ActionResult SaveNotificationToken(NotificationToken notificationToken)
        {
            var result = new ActionResult();
            this.Use(
                client => { result = client.SaveNotificationTokenAsync(notificationToken).Result;}
            );
            return result;
        }
    }
}
