using Microsoft.Extensions.Configuration;
using System;
using System.Web;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using OnlineBankingLibrary.Models;
using XBS;
using XBSInfo;
using ContractServiceRef;
using System.Linq;
using System.Threading.Tasks;
using OnlineBankingLibrary.Utilities;
using Microsoft.AspNetCore.Http;

namespace OnlineBankingLibrary.Services
{
    public class ContractService
    {
        private readonly IConfiguration _config;

        public ContractService(IConfiguration config)
        {
            _config = config;
        }

        public void Use(Action<IContractOerationService> action)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxBufferPoolSize = 5242880;
            binding.MaxBufferSize = 6553600;
            binding.MaxReceivedMessageSize = 6553600;
            binding.ReaderQuotas.MaxArrayLength = 2500000;


            string endpointUrl = _config["WCFExternalServices:ContractServiceRef:EndpointAddress"];
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            IContractOerationService client = ProxyManager<IContractOerationService>.GetProxy(nameof(IContractOerationService), binding, endpoint);

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
        private string RenderContractHTML(string contractName, Dictionary<string, string> parameters, string fileName)
        {
            string fileContent = "";
            Contract contract = new Contract();
            contract.ContractName = contractName;

            contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();

            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> param in parameters)
                {
                    ContractServiceRef.StringKeyValue oneParam = new ContractServiceRef.StringKeyValue();
                    oneParam.Key = param.Key;
                    oneParam.Value = param.Value;
                    contract.ParametersList.Add(oneParam);
                }
            }

            this.Use(client => fileContent = client.DownloadContractHTMLAsync(contract, "HB", "0").Result);
            return fileContent;
        }

        public string RenderContract(string contractName, Dictionary<string, string> parameters, string fileName, Contract contract = null)
        {
            string fileContent = null;
            if (contract == null)
            {
                contract = new Contract();
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
                contract.ContractName = contractName;
            }
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> param in parameters)
                {
                    ContractServiceRef.StringKeyValue oneParam = new ContractServiceRef.StringKeyValue();
                    oneParam.Key = param.Key;
                    oneParam.Value = param.Value;
                    contract.ParametersList.Add(oneParam);
                }
            }

            this.Use(client => fileContent = Convert.ToBase64String(client.DownloadContractAsync(contract, "HB", "0").Result));
            return fileContent;
        }
    }
}
