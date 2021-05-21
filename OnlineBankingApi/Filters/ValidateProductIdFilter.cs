using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Resources;
using OnlineBankingLibrary.Services;
using System.Linq;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Filters
{
    public class ValidateProductIdFilter : ActionFilterAttribute
    {
        private readonly XBService _xbService;
        private readonly ProductType _type;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ValidateProductIdFilter(XBService xbService, ProductType type, IStringLocalizer<SharedResource> localizer)
        {
            _xbService = xbService;
            _type = type;
            _localizer = localizer;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {


            if (context.ActionArguments.Count != 0)
            {
                ulong productId = 0;
                var argument = context.ActionArguments.Values.First();
                if (argument is ProductIdRequest)
                {
                    ProductIdRequest idRequest = argument as ProductIdRequest;
                    productId = idRequest.ProductId;
                }
                else if (argument is CVVNoteRequest)
                {
                    CVVNoteRequest idRequest = argument as CVVNoteRequest;
                    productId = idRequest.ProductId;
                }

                if (!_xbService.ValidateProductId(productId, _type))
                {
                    Response response = new Response
                    {
                        ResultCode = ResultCodes.validationError,
                        Description = _localizer["Պրոդուկտի ունիկալ համարը սխալ է։"]
                    };
                    context.Result = ResponseExtensions.ToHttpResponse(response);
                } 
            }
        }
    }
}
