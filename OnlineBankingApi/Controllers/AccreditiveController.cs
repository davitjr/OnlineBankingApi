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
    public class AccreditiveController : ControllerBase
    {
        private readonly XBService _xbService;

        public AccreditiveController(XBService xbService)
        {
            _xbService = xbService;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ակրեդիտիվների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccreditives")]
        public IActionResult GetAccreditives(ProductQualityFilterRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<Accreditive>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccreditives(request.Filter);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ ակրեդիտիվի մանրամասները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAccreditive")]
        public IActionResult GetAccreditive(ProductIdRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<Accreditive>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetAccreditive(request.ProductId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}