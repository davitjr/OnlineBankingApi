using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Microsoft.Extensions.Configuration;
using SMSMessagingServiceReference;
using XBManagement;
using User = SMSMessagingServiceReference.User;

namespace OnlineBankingLibrary.Services
{
    public class SMSMessagingService
    {
        private readonly IConfiguration _config;

        public SMSMessagingService(IConfiguration config)
        {
            this._config = config;
        }

        public void Use(Action<ISMSMessagingService> action)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string endpointUrl = _config["WCFExternalServices:SMSMessagingService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            ISMSMessagingService client = ProxyManager<ISMSMessagingService>.GetProxy(nameof(ISMSMessagingService), binding, endpoint);

            var user = new User();
            user.userID = 88;
            client.SetCurrentUserAsync(user).Wait();

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

        public void SendMessage(string phoneNumber, ulong customerNumber, string messageText, int userId, int messageType)
        {
            var message = new OneMessage();
            message.PhoneNumber = phoneNumber;
            message.CustomerNumber = customerNumber;
            message.Message = messageText;
            message.RegistrationSetNumber = userId;
            message.MessageType = messageType;
            message.Source = 1;
            message.ExternalId = null;
            this.Use(client =>
            {
                client.SendOneMessageAsync(message).Wait();
            }
          );
        }



    }
}
