using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingLibrary.Services;
using System.Linq;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class ValidateDocIdFilter : ActionFilterAttribute
    {
        private readonly XBService _xbService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ValidateDocIdFilter(XBService xbService, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _localizer = localizer;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.Count != 0)
            {
                long id = 0;
                var argument = context.ActionArguments.Values.First();
                if (argument is IdRequest)
                {
                    IdRequest idRequest = argument as IdRequest;
                    id = idRequest.Id;
                }
                else if (argument is DocIdRequest)
                {
                    DocIdRequest docIdRequest = argument as DocIdRequest;
                    id = docIdRequest.DocId;
                }
                else if (argument is OrderIdRequest)
                {
                    OrderIdRequest orderIdRequest = argument as OrderIdRequest;
                    id = orderIdRequest.OrderId;
                }

                if (!_xbService.ValidateDocId(id))
                {
                    Response response = new Response
                    {
                        ResultCode = ResultCodes.validationError,
                        Description = _localizer["Գործարքի համարը սխալ է։"]
                    };
                    context.Result = ResponseExtensions.ToHttpResponse(response);
                }
            }
        }
    }
}
