using ReportingService;
using System;
using System.Collections.Generic;
using System.Text;
using OnlineBankingLibrary.Utilities;
using System.ServiceModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace OnlineBankingLibrary.Services
{

    /// <summary>
    /// File Export Formats.
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>Image</summary>
        Image,
        /// <summary>PDF</summary>
        PDF,
        /// <summary>Excel</summary>
        Excel,
        /// <summary>Word</summary>
        Word
    }

    public class ReportService
    {

        private readonly IConfiguration _config;


        public ReportService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<byte[]> RenderReport(string reportPath, IDictionary<string, string> parameters, ExportFormat exportFormat, string fileName)
        {
            byte[] fileContent = null;
            try
            {
                //My binding setup, since ASP.NET Core apps don't use a web.config file
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                binding.MaxReceivedMessageSize = 10485760; //I wanted a 10MB size limit on response to allow for larger PDFs

                string _reportingServicesUrl = _config["WCFExternalServices:ReportingService:EndpointAddress"];

                //Create the execution service SOAP Client
                var rsExec = new ReportExecutionServiceSoapClient(binding, new EndpointAddress(_reportingServicesUrl));

                //Setup access credentials. I use windows credentials, yours may differ
                var clientCredentials = new NetworkCredential();
                //rsExec.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                rsExec.ClientCredentials.Windows.ClientCredential = clientCredentials;

                //This handles the problem of "Missing session identifier"
                rsExec.Endpoint.EndpointBehaviors.Add(new ReportingServicesEndpointBehavior());

                string historyID = null;
                TrustedUserHeader trustedUserHeader = new TrustedUserHeader();
                ExecutionHeader execHeader = new ExecutionHeader();

                trustedUserHeader.UserName = clientCredentials.UserName;

                //Load the report
                var taskLoadReport = await rsExec.LoadReportAsync(trustedUserHeader, reportPath, historyID);
                // Fixed the exception of "session identifier is missing".
                execHeader.ExecutionID = taskLoadReport.executionInfo.ExecutionID;

                //
                //Set the parameteres asked for by the report
                //

                string lang = "";

                if (fileName == "LoanStatement" || fileName == "Payment_order" || fileName == "CreditLineStatement")
                {

                    if (Convert.ToInt32(parameters["lang_id"]) == 1)
                        lang = "hy-am";
                    else
                        lang = "en-us";
                }
                else
                {
                    lang = "en-us";
                }

                ParameterValue[] reportParameters = null;
                if (parameters != null && parameters.Count > 0)
                {
                    reportParameters = taskLoadReport.executionInfo.Parameters.Where(x => parameters.ContainsKey(x.Name)).Select(x => new ParameterValue() { Name = x.Name, Value = parameters[x.Name] != null ? parameters[x.Name].ToString() : null }).ToArray();
                }
                await rsExec.SetExecutionParametersAsync(execHeader, trustedUserHeader, reportParameters, lang);

                //run the report
                string format = GetExportFormatString(exportFormat);
                // run the report
                const string deviceInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

                var response = await rsExec.RenderAsync(new RenderRequest(execHeader, trustedUserHeader, format, deviceInfo));

                //spit out the result
                return response.Result;
            }
            catch (Exception ex)
            {

               // System.Web.HttpContext.Current.Response.BufferOutput = true;
             //   System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
              //  System.Web.HttpContext.Current.Response.StatusCode = 422;
              //  System.Web.HttpContext.Current.Response.StatusDescription = Utils.ConvertAnsiToUnicode(ex.Message);
             throw ex;
            }
            return fileContent;
        }

        /// <summary>
        /// Gets the string export format of the specified enum.
        /// </summary>
        /// <param name="f">export format enum</param>
        /// <returns>enum equivalent string export format</returns>
        public static string GetExportFormatString(ExportFormat f)
        {
            switch (f)
            {
                case ExportFormat.Image: return "IMAGE";
                case ExportFormat.PDF: return "PDF";
                case ExportFormat.Excel: return "Excel";
                case ExportFormat.Word: return "WORD";

                default:
                    return "PDF";
            }
        }


        /// <summary>
        /// Gets the enum export format of the specified extension.
        /// </summary>
        /// <param name="f">format string</param>
        /// <returns>equivalent enum export format</returns>
        public static ExportFormat GetExportFormatEnumeration(string f)
        {
            switch (f)
            {
                case "jpg": return ExportFormat.Image;
                case "jpeg": return ExportFormat.Image;
                case "png": return ExportFormat.Image;
                case "pdf": return ExportFormat.PDF;
                case ".jpg": return ExportFormat.Image;
                case ".jpeg": return ExportFormat.Image;
                case ".png": return ExportFormat.Image;
                case ".pdf": return ExportFormat.PDF;
                case ".xls": return ExportFormat.Excel;
                case "xls": return ExportFormat.Excel;
                default:
                    return ExportFormat.PDF;
            }
        }
    }
}
