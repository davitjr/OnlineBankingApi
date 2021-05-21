using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using OnlineBankingLibrary.Models;
using ACBAServiceReference;
using System.Data;
using Person = ACBAServiceReference.Person;
using Customer = ACBAServiceReference.Customer;
using SearchCustomers = ACBAServiceReference.SearchCustomers;
using CustomerPhoto = ACBAServiceReference.CustomerPhoto;
using CustomerDocument = ACBAServiceReference.CustomerDocument;
using AttachmentDocument = ACBAServiceReference.AttachmentDocument;
using PhysicalCustomer = ACBAServiceReference.PhysicalCustomer;
using CustomerMainData = ACBAServiceReference.CustomerMainData;
using CustomerEmail = ACBAServiceReference.CustomerEmail;
using ActionResult = ACBAServiceReference.Result;
using CustomerPhone = ACBAServiceReference.CustomerPhone;
using OnlineBankingLibrary.Utilities;
using System.Security.Cryptography;

namespace OnlineBankingLibrary.Services
{
    public class ACBAOperationService
    {
        private readonly IConfiguration _config;
        private readonly CacheHelper _cacheHelper;


        public ACBAOperationService(IConfiguration config, CacheHelper cacheHelper)
        {
            _config = config;
            _cacheHelper = cacheHelper;
        }

        public void Use(Action<IACBAOperationService> action)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxBufferPoolSize = 5242880;
            binding.MaxBufferSize = 6553600;
            binding.MaxReceivedMessageSize = 6553600;
            binding.ReaderQuotas.MaxArrayLength = 2500000;
            string endpointUrl = _config["WCFExternalServices:ACBAOperationService:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            IACBAOperationService client = ProxyManager<IACBAOperationService>.GetProxy(nameof(IACBAOperationService), binding, endpoint);
            var operationServiceUser = _cacheHelper.GetOPUser();
            operationServiceUser.UserSessionToken = _config["WCFExternalServices:ACBAOperationService:IdentificationToken"];
            client.SetCurrentUserAsync(operationServiceUser).Wait();
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


        public Person FindPersonInNorq(string documentValue)
        {
            Person result = new Person();
            this.Use(client => { result = client.FindPersonInNorqAsync(documentValue).Result; }
            );
            return result;
        }

        public Customer GetIdentifiedCustomer(SearchCustomers param)
        {
            Customer result = new Customer();
            this.Use(client => { result = client.GetIdentifiedCustomerAsync(param).Result; }
            );
            return result;
        }

        public CustomerPhoto GetCustomerPhoto(ulong customerNumber)
        {
            CustomerPhoto result = new CustomerPhoto();
            this.Use(client => { result = client.GetCustomerPhotoAsync(customerNumber).Result; }
            );
            return result;
        }

        public CustomerIdentificationResult IdentifyCustomer(PhysicalCustomer customer)
        {
            CustomerIdentificationResult result = new CustomerIdentificationResult();
            this.Use(client => { result = client.IdentifyCustomerAsync(customer, CustomerRegistrationSource.DigitalBanking).Result; }
            );
            return result;
        }

        public byte[] GetCustomerOnePhoto(ulong photoId)
        {
            byte[] result = null;
            this.Use(client => { result = client.GetCustomerOnePhotoAsync(photoId).Result; }
            );
            return result;
        }

        public List<CustomerDocument> GetCustomerDocumentList(uint identityId)
        {
            List<CustomerDocument> result = new List<CustomerDocument>();
            this.Use(client => { result = client.GetCustomerDocumentListAsync(identityId, 1).Result; }
            );
            return result;
        }


        public List<AttachmentDocument> GetAttachmentDocumentList(ulong documentId)
        {
            List<AttachmentDocument> result = new List<AttachmentDocument>();
            this.Use(client => { result = client.GetAttachmentDocumentListAsync(documentId, true, 1).Result; }
            );
            return result;
        }


        public byte[] GetOneAttachment(ulong attachmentId)
        {
            byte[] result = null;
            this.Use(client => { result = client.GetOneAttachmentAsync(attachmentId).Result; }
            );
            return result;
        }

        public PhysicalCustomer GetCustomerForExternalRequest(Dictionary<ACBAServiceReference.DocumentTypes, string> docList, Dictionary<ACBAServiceReference.ExternalRequestOptions, bool> options, ACBAServiceReference.NorqEkengPriority norqEkengPriority)
        {
            var result = new PhysicalCustomer();
            this.Use(client => { result = client.GetCustomerForExternalRequestNewAsync(docList, options, norqEkengPriority).Result; }
            );
            return result;
        }

        public short GetCustomerQuality(ulong customerNumber)
        {
            short result = 0;
            this.Use(client => { result = client.GetCustomerQualityAsync(customerNumber).Result; }
            );
            return result;
        }

        public CustomerMainData GetCustomerMainData(ulong customerNumber)
        {
            var result = new CustomerMainData();
            this.Use(client => { result = (CustomerMainData)client.GetCustomerMainDataAsync(customerNumber).Result; }
            );
            return result;
        }

        public ActionResult SaveCustomerEmail(ulong customerNumber, CustomerEmail email)
        {
            var result = new ActionResult();
            this.Use(client => { result = client.SaveCustomerEmailAsync(customerNumber, email).Result; }
            );
            return result;
        }

        public ActionResult SaveCustomerPhone(ulong customerNumber, CustomerPhone phone)
        {
            var result = new ActionResult();
            this.Use(client => { result = client.SaveCustomerPhoneAsync(customerNumber, phone).Result; }
            );
            return result;
        }

        public byte GetCustomerType(ulong customerNumber)
        {
            byte result = 0;
            this.Use(client => { result = client.GetCustomerTypeAsync(customerNumber).Result; }
            );
            return result;
        }

        public void SaveOnlineUserPhoto(ulong onlineUserId, byte[] photo, string extension)
        {

            this.Use(client => { client.SaveOnlineUserPhotoAsync(onlineUserId, photo, extension); }
            );

        }

        public void DeleteOnlineUserPhoto(ulong onlineUserId)
        {

            this.Use(client => { client.DeleteOnlineUserPhotoAsync(onlineUserId); }
            );

        }

        public byte[] GetOnlineUserPhoto(ulong onlineUserId)
        {
            byte[] result=null;

            this.Use(client => { result = client.GetOnlineUserPhotoAsync(onlineUserId).Result; }
            );

            return result;

        }



        internal PhysicalCustomer GetCustomer(ulong customerNumber)
        {
            PhysicalCustomer result = null;
            this.Use(client => { result = (PhysicalCustomer)client.GetPhysicalCustomerFromCRMAsync(customerNumber).Result; }
            );
            return result;
        }

        internal bool CheckCustomerUpdateExpired(ulong customerNumber)
        {
            bool result = false;
            this.Use(client => { result = Convert.ToBoolean(client.CheckCustomerUpdateExpiredAsync(customerNumber).Result); }
            );
            return result;
        }

        public ulong GetIdentityId(ulong customerNumber)
        {
            ulong result = 0;
            this.Use(client => { result = client.GetIdentityIdAsync(customerNumber).Result; }
            );
            return result;
        }
    }
}
