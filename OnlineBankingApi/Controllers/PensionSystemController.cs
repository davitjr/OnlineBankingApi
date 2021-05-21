using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class PensionSystemController : ControllerBase 
    {
        private readonly XBService _xbService;

        public PensionSystemController(XBService xbService)
        {
            _xbService = xbService;
        }

        /// <summary>
        /// Վերադարձնում է կենսաթոշակային ֆոնդի մնացորդը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPensionSystemBalance")]
        public IActionResult GetPensionSystemBalance()
        {

            if (ModelState.IsValid)
            {
                var response = new SingleResponse<dynamic>();
                var pensionSystemBalance = _xbService.GetPensionSystemBalance();
                //var pensionSystemBalance = new PensionSystem();
                var result = new {  FirstName = pensionSystemBalance.FirstName, 
                                    LastName = pensionSystemBalance.LastName, 
                                    BirthDate = pensionSystemBalance.BirthDate,
                                    PSN = pensionSystemBalance.PSN, 
                                    Balance = pensionSystemBalance.Balance, };

                response.Result = result;
                //response.ResultCode = ResultCodes.normal;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(pensionSystemBalance.ResultCode);
                response.Description = Utils.GetActionResultErrors(pensionSystemBalance.Errors);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}
