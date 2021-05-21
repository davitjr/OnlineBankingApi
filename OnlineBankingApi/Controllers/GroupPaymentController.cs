using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class GroupPaymentController : ControllerBase
    {
        private readonly XBService _xBService;

        public GroupPaymentController(XBService xBService)
        {
            _xBService = xBService;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ ծառայությունների խմբերը
        /// </summary>
        /// <param name="status"></param>
        /// <param name="groupType"></param>
        /// <returns></returns>
        [HttpPost("GetOrderGroups")]
        public IActionResult GetOrderGroups(OrderGroupRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<OrderGroup>> response = new SingleResponse<List<OrderGroup>>();
                response.Result = _xBService.GetOrderGroups(request.Status, request.GroupType);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Գործարքների խմբի հեռացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("DeleteOrderGroup")]
        public IActionResult DeleteOrderGroup(GroupIdRequest request)
        {
            if (ModelState.IsValid)
            {
                Response response = new Response();
                _xBService.DeleteOrderGroup(request.GroupId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Պահպանում է խմբում ներառված ծառայությունները՝ որպես գործարք
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SaveGroupPayment")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveGroupPayment(GroupPaymentRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<SingleResponse<long>>> response = new SingleResponse<List<SingleResponse<long>>>();
                response.Result = new List<SingleResponse<long>>();
                List<XBS.ActionResult> result = _xBService.SaveGroupPayment(request.GroupPayment);
                for (int i = 0; i < result.Count; i++)
                {
                    SingleResponse<long> res = new SingleResponse<long>();
                    res.ResultCode = (ResultCodes)result[i].ResultCode;
                    for (int j = 0; j < result[i].Errors.Count; j++)
                    {
                        res.Description += result[i].Errors[j].Description;
                    }
                    response.Result.Add(res);
                }

                string actionErrors = "";
                response.ResultCode = ResultCodes.failed;

                foreach (SingleResponse<long> result2 in response.Result)
                {
                    if (result2.ResultCode == ResultCodes.normal)
                    {
                        response.ResultCode = ResultCodes.normal;
                    }

                    if (result2.Description != null && result2.Description != "")
                    {
                        actionErrors += result2.Description;
                        actionErrors = actionErrors.Remove(actionErrors.Length - 1);
                    }
                }
                response.Description = actionErrors;

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }
    }
}