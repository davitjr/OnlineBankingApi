using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using XBS;
using static OnlineBankingApi.Enumerations;
using utils = OnlineBankingLibrary.Utilities.Utils;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class TemplateController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;

        public TemplateController(XBService xbService, CacheHelper cacheHelper)
        {
            _xbService = xbService;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Ձևանմուշի կարգավիճակի փոփոխում
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="templateStatus"></param>
        /// <returns></returns>
        [HttpPost("ChangeTemplateStatus")]
        public IActionResult ChangeTemplateStatus(ChangeTemplateStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<XBS.ActionResult> response = new SingleResponse<XBS.ActionResult>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.ChangeTemplateStatus(request.TemplateId, request.TemplateStatus);
                response.Description = utils.GetActionResultErrors(response.Result.Errors);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        

        /// <summary>
        /// Վերադարձնում է Փոխանցում ՀՀ տարածքում/Փոխանցում հաշիվների միջև ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost("GetPaymentOrderTemplate")]
        public IActionResult GetPaymentOrderTemplate(TemplateIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<PaymentOrderTemplate> respone = new SingleResponse<PaymentOrderTemplate>();
                respone.Result = _xbService.GetPaymentOrderTemplate(request.TemplateId);
                respone.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(respone); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է բյուջե փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost("GetBudgetPaymentOrderTemplate")]
        public IActionResult GetBudgetPaymentOrderTemplate(TemplateIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<BudgetPaymentOrderTemplate> respone = new SingleResponse<BudgetPaymentOrderTemplate>();
                respone.Result = _xbService.GetBudgetPaymentOrderTemplate(request.TemplateId);
                respone.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(respone); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկի մարման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost("GetLoanMatureOrderTemplate")]
        public IActionResult GetLoanMatureOrderTemplate(TemplateIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<LoanMatureOrderTemplate> response = new SingleResponse<LoanMatureOrderTemplate>();
                response.Result = _xbService.GetLoanMatureOrderTemplate(request.TemplateId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost("GetUtilityPaymentOrderTemplate")]
        public IActionResult GetUtilityPaymentOrderTemplate(TemplateIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<UtilityPaymentOrderTemplate> response = new SingleResponse<UtilityPaymentOrderTemplate>();
                response.Result = _xbService.GetUtilityPaymentOrderTemplate(request.TemplateId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի ձևանմուշների ցանկը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerTemplates")]
        public IActionResult GetCustomerTemplates()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Template>> response = new SingleResponse<List<Template>>();
                response.Result = _xbService.GetCustomerTemplates();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտից քարտ փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost("GetCardToCardOrderTemplate")]
        public IActionResult GetCardToCardOrderTemplate(TemplateIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<CardToCardOrderTemplate> response = new SingleResponse<CardToCardOrderTemplate>();
                response.Result = _xbService.GetCardToCardOrderTemplate(request.TemplateId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Քարտից քարտ փոխանցման ձևանմուշի պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost("SaveCardToCardOrderTemplate")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCardToCardOrderTemplate(CardToCardOrderTemplateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                request.Template.TemplateCustomerNumber = _cacheHelper.GetAuthorizedCustomer().CustomerNumber;
                XBS.ActionResult result = _xbService.SaveCardToCardOrderTemplate(request.Template);
                response.Result = result.Id;
                response.Description = utils.GetActionResultErrors(result.Errors);
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Միջազգային փոխանցման ձևանմուշի/խմբային ծառայության պահպանում/խմբագրում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost("SaveInternationalOrderTemplate")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveInternationalOrderTemplate(InternationalOrderTemplateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xbService.SaveInternationalOrderTemplate(request.Template);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Միջազգային փոխանցման ձևանմուշի GET
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpPost("GetInternationalOrderTemplate")]
        public IActionResult GetInternationalOrderTemplate(TemplateIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<InternationalOrderTemplate> response = new SingleResponse<InternationalOrderTemplate>();
                response.Result = _xbService.GetInternationalOrderTemplate(request.TemplateId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Բյուջե փոխանցման ձևանմուշի/խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost("SaveBudgetPaymentOrderTemplate")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveBudgetPaymentOrderTemplate(BudgetOrderTemplateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                request.Template.TemplateCustomerNumber = _cacheHelper.GetAuthorizedCustomer().CustomerNumber;
                XBS.ActionResult result = _xbService.SaveBudgetPaymentOrderTemplate(request.Template);
                response.Result = result.Id;
                response.Description = utils.GetActionResultErrors(result.Errors);
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Փոխարկման ձևանմուշի/խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost("SaveCurrencyExchangeOrderTemplate")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveCurrencyExchangeOrderTemplate(CurrencyExchangeOrderTemplateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                request.Template.TemplateCustomerNumber = _cacheHelper.GetAuthorizedCustomer().CustomerNumber;
                XBS.ActionResult result = _xbService.SaveCurrencyExchangeOrderTemplate(request.Template);
                response.Result = result.Id;
                response.Description = utils.GetActionResultErrors(result.Errors);
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վարկի մարման՝ որպես խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost("SaveLoanMatureOrderTemplate")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public IActionResult SaveLoanMatureOrderTemplate(LoanMatureOrderTemplateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<long> response = new SingleResponse<long>();
                XBS.ActionResult result = _xbService.SaveLoanMatureOrderTemplate(request.Template);
                response.Result = result.Id;
                response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
                response.Description = utils.GetActionResultErrors(result.Errors);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է Ձևանմուշների քանակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCustomerTemplatesCount")]
        public IActionResult GetCustomerTemplatesCount()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                response.Result = _xbService.GetCustomerTemplatesCount();
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