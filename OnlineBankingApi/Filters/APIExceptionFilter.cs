using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using OnlineBankingApi.Models;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Utilities;
using System;
using System.ServiceModel;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class APIExceptionFilter : Attribute, IExceptionFilter
    {
        private readonly IConfiguration _config;
        private readonly CacheHelper _cacheHelper;

        public APIExceptionFilter(IConfiguration config, CacheHelper cacheHelper)
        {
            _config = config;
            _cacheHelper = cacheHelper;
        }

        public void OnException(ExceptionContext context)
        {
            LoggerUtility.WriteLog(context.Exception, _config.GetConnectionString("AppLog"), _cacheHelper.GetAuthorizedCustomer()?.UserName??"- - - -");
            Response response = new Response();

            if (context.Exception is FaultException)
                response.Description = context.Exception.Message;
            else if (context.Exception is RpcException && (context.Exception as RpcException).Status.StatusCode == StatusCode.InvalidArgument)
                response.Description = context.Exception.Message;
            else
                response.Description = ("Տեղի ունեցավ սխալ");

            response.ResultCode = ResultCodes.failed;
            context.Result = ResponseExtensions.ToHttpResponse(response);
        }
    }
}
