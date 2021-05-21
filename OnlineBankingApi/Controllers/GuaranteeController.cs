using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using XBS;
using static OnlineBankingApi.Enumerations;
using ActionResult = XBS.ActionResult;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;
using utils = OnlineBankingLibrary.Utilities.Utils;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class GuaranteeController : ControllerBase
    {

        private readonly XBService _xbService;

        public GuaranteeController(XBService xbService)
        {
            _xbService = xbService;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի երաշխիքների ցուցակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetGuarantees")]
        public IActionResult GetGuarantees(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<Guarantee>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetGuarantees(request.Filter);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ երաշխիքի մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetGuarantee")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Guarantee })]
        public IActionResult GetGuarantee(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<Guarantee>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetGuarantee(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


    }
}