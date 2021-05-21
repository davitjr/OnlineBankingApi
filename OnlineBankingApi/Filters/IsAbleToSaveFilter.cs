using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class IsAbleToSaveFilter: ActionFilterAttribute
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public IsAbleToSaveFilter(XBService xbService, CacheHelper cacheHelper, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
            _localizer = localizer;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

            if (authorizedCustomer.Permission != 5 && authorizedCustomer.Permission != 3)
            {
                Response response = new Response();
                response.ResultCode = ResultCodes.validationError;
                response.Description = authorizedCustomer.UserName + _localizer[" օգտագործողը չունի մուտքագրման իրավունք"];

                context.Result = ResponseExtensions.ToHttpResponse(response);
            }
        }
    }
}
