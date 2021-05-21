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
    public class DepositCaseController : ControllerBase
    {
        private readonly XBService _xbService;

        public DepositCaseController(XBService xbService)
        {
            _xbService = xbService;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի  պահատուփերի ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositCases")]
        public IActionResult GetDepositCases(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<DepositCase>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositCases(request.Filter);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ պահատուփի մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositCase")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Deposit })]
        public IActionResult GetDepositCase(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<DepositCase>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositCase(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}