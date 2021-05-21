using Microsoft.Extensions.Logging;
using NLog;
using NLog.Targets;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OnlineBankingApi.Middlewares.RequestResponseLoggingMiddleware;

namespace OnlineBankingApi.Utilities
{
    public class LoggerUtility
    {
        public static void WriteLog(Exception ex, string connString, string userName)
        {
            Logger logger = LogManager.GetLogger("errorLog");
            string stackTrace = $"{ex.StackTrace ?? " "} \n InnerException StackTrace: {ex?.InnerException?.StackTrace ?? ""}";
            string message = $"{ex.Message ?? " "} \n InnerException: {ex?.InnerException?.Message ?? ""}";
            GlobalDiagnosticsContext.Set("ClientIp", "");
            GlobalDiagnosticsContext.Set("Logger", "OnlineBankingApi");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());
            GlobalDiagnosticsContext.Set("UserName", userName ?? "----");
            DatabaseTarget databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("ErrorDatabase");
            databaseTarget.ConnectionString = connString;
            LogManager.ReconfigExistingLoggers();
            logger.Error(message);
        }

        public static void WriteRequestLog(RequestProfilerModel requestObject, string connString, bool isProdVersion, bool logContentObject)
        {
            Logger logger = LogManager.GetLogger("requestLog");
            var builder = new StringBuilder(Environment.NewLine);
            GlobalDiagnosticsContext.Set("ClientIp", requestObject.Context.Connection.RemoteIpAddress.ToString());
            GlobalDiagnosticsContext.Set("Logger", "OnlineBankingApi");
            GlobalDiagnosticsContext.Set("CallMethod", requestObject.Context.Request.Host.Value + requestObject.Context.Request.Path.Value);
            GlobalDiagnosticsContext.Set("Request", requestObject.Request);
            GlobalDiagnosticsContext.Set("Response", requestObject.Response);

            string logJson = "";
            if (logContentObject)
                logJson = requestObject.Request + requestObject.Response;
            else
            {
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(requestObject.Response);
                logJson = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            }

            GlobalDiagnosticsContext.Set("Json", logJson);

            Parallel.ForEach(requestObject.Context.Request.Headers, x =>
             {
                 if (!(isProdVersion && x.Key == "SessionId"))
                        builder.AppendLine($"{x.Key}:{x.Value}");
             });
            GlobalDiagnosticsContext.Set("Headers", builder.ToString());
            GlobalDiagnosticsContext.Set("UserName", "----");
            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("RequestDatabase");
            databaseTarget.ConnectionString = connString;
            LogManager.ReconfigExistingLoggers();
            logger.Info("Request Logger");
        }

        public class LoggerOff : Attribute
        {
        }
    }
}
