using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
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
    public class FileController : ControllerBase
    {
        private readonly XBService _xbService;
   

        public FileController(XBService xBService)
        {
            _xbService = xBService;
        }

        /// <summary>
        /// Պահպանում է և վերադարձնում է պահպանված ռեեստրի ֆայլի ID- ին 
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost("SaveUploadedFile")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveUploadedFile(UploadedFileRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _xbService.SaveUploadedFile(request.UploadedFile);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

    }

}