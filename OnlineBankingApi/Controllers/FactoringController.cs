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
    public class FactoringController : ControllerBase
    {
        private readonly XBService _xbService;

        public FactoringController(XBService xbService)
        {
            _xbService = xbService;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ֆակտորինգների ցուցակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetFactorings")]
        public IActionResult GetFactorings(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<Factoring>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetFactorings(request.Filter);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ ֆակտորինգի մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetFactoring")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Factoring })]
        public IActionResult GetFactoring(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<Factoring>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetFactoring(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}