using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using XBS;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class EmployeeSalaryController : ControllerBase
    {
        private readonly XBService _xbService;        
        private readonly CacheHelper _cacheHelper;

        public EmployeeSalaryController(XBService xbService, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Վերադարձնում է նշված ժամանակահատվածում ստացված աշխատավարձի տվյալները
        /// </summary>
        /// <param name="startDateEndDateRequest"></param>
        /// <returns></returns>
        [HttpPost("GetEmployeeSalaryList")]
        public IActionResult GetEmployeeSalaryList(StartDateEndDateRequest startDateEndDateRequest)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<List<EmployeeSalary>>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetEmployeeSalaryList(startDateEndDateRequest.StartDate, startDateEndDateRequest.EndDate);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Տվյալ աշխատավարձի մանրամամասն տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost("GetEmployeeSalaryDetails")]
        public IActionResult GetEmployeeSalaryDetails(GenericIdRequest ID)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<EmployeeSalaryDetails>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetEmployeeSalaryDetails(ID.Id);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Աշխատակցի մանրամասն տվյալներ
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetEmployeePersonalDetails")]
        public IActionResult GetEmployeePersonalDetails()
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<EmployeePersonalDetails>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetEmployeePersonalDetails();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

    }
}