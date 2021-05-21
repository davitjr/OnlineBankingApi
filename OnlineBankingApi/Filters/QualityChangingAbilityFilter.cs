using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
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
    public class QualityChangingAbilityFilter : ActionFilterAttribute
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;

        //SaveAndApproveTokenReplacementOrder


        public QualityChangingAbilityFilter(XBService xbService, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            long id = 0;


            ApproveIdRequest request = null;
            OrderRejectionRequest rejectionRequest = null;
            RemovalOrderRequest removalOrderRequest = null;
            OrderType orderType = OrderType.NotDefined;

            foreach (var argument in context.ActionArguments.Values.Where(v => v is ApproveIdRequest))
            {
                request = argument as ApproveIdRequest;
                id = request.Id;
                break;
            }

            foreach (var argument in context.ActionArguments.Values.Where(v => v is OrderRejectionRequest))
            {
                rejectionRequest = argument as OrderRejectionRequest;
                id = rejectionRequest.OrderRejection.OrderId;
                break;
            }

            foreach (var argument in context.ActionArguments.Values.Where(v => v is RemovalOrderRequest))
            {
                removalOrderRequest = argument as RemovalOrderRequest;
                id = removalOrderRequest.Order.RemovingOrderId;
                orderType = OrderType.RemoveTransaction;
                break;
            }

            AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

            if ((!_xbService.IsAbleToChangeQuality(authorizedCustomer.UserName, (int)id) && orderType != OrderType.RemoveTransaction) ||
                (authorizedCustomer.Permission != 3 && authorizedCustomer.Permission != 2) || (orderType == OrderType.RemoveTransaction && !authorizedCustomer.IsLastConfirmer))
            {
                Response response = new Response();
                byte language = _cacheHelper.GetLanguage() == 0 ? (byte)2 : (byte)1;

                response.ResultCode = ResultCodes.validationError;
                response.Description = _xbService.GetTerm(1689, null, (Languages)language);

                context.Result = ResponseExtensions.ToHttpResponse(response);
            }
        }
    }
}
