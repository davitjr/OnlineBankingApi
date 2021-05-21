using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingLibrary.Utilities;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Services;
using XBS;
using static OnlineBankingApi.Enumerations;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class ProductNoteController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;
        public ProductNoteController(XBService xbService, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Պահպանում է հաճախորդի կողմից կատարված նշումը տվյալ պրոդուկտին (օրինակ՝ ընթացիկ հաշվին
        /// </summary>
        /// <param name="productNote"></param>
        /// <returns></returns>
        [HttpPost("SaveProductNote")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveProductNote(ProductNoteRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                response.ResultCode = ResultCodes.normal;
                XBS.ActionResult result = _xbService.SaveProductNote(request.ProductNote);
                response.Result = result.Id;
                response.Description = Utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ պրոդուկտին համապատասխան հաճախորդի կողմից արված նշումը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetProductNote")]

        public IActionResult GetProductNote(UniqueIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                bool hasPermission = true;
                var order = _xbService.GetProductNote(request.UniqueId); // ????
                SingleResponse<ProductNote> response = new SingleResponse<ProductNote>();
                if (authorizedCustomer.LimitedAccess != 0)
                {
                    if (!_xbService.HasProductPermission(order.UniqueId.ToString()))
                    {
                        hasPermission = false;
                        response.ResultCode = ResultCodes.failed;
                        response.Result.Description = "Տվյալները հասանելի չեն։";
                    }
                }

                if (hasPermission)
                {
                    response.Result = order;
                    response.ResultCode = ResultCodes.normal;
                }
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}