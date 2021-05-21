using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using OnlineBankingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Utilities
{
    public class ResultCodeFormatter
    {
        public static ResultCodes FromPersonalAccountSecurityService(XBS.ResultCode xbsResultCode)
        {
            ResultCodes resultCode = new ResultCodes();

            switch (xbsResultCode)
            {
                case XBS.ResultCode.Normal:
                    resultCode = ResultCodes.normal;
                    break;
                case XBS.ResultCode.Failed:
                    resultCode = ResultCodes.failed;
                    break;
                case XBS.ResultCode.NotAutorized:
                    resultCode = ResultCodes.notAuthorized;
                    break;
                case XBS.ResultCode.ValidationError:
                    resultCode = ResultCodes.validationError;
                    break;
                case XBS.ResultCode.SavedNotConfirmed:
                    resultCode = ResultCodes.normal;
                    break;
                case XBS.ResultCode.Warning:
                    resultCode = ResultCodes.normal;
                    break;
                case XBS.ResultCode.NoneAutoConfirm:
                    resultCode = ResultCodes.failed;
                    break;
                case XBS.ResultCode.SaveAndSendToConfirm:
                    resultCode = ResultCodes.normal;
                    break;
                case XBS.ResultCode.DoneAndReturnedValues:
                    resultCode = ResultCodes.normal;
                    break;
                case XBS.ResultCode.InvalidRequest:
                    resultCode = ResultCodes.failed;
                    break;
                case XBS.ResultCode.PartiallyCompleted:
                    resultCode = ResultCodes.failed;
                    break;
                default:
                    break;
            }
            return resultCode;
        }

        public static ResultCodes FromCustomerRegistrationProcess(OnlineBankingLibrary.Models.CustomerRegistration.ResultCode processResultCode)
        {
            ResultCodes resultCode = new ResultCodes();

            switch (processResultCode)
            {
                case OnlineBankingLibrary.Models.CustomerRegistration.ResultCode.Failed:
                    resultCode = ResultCodes.failed;
                    break;
                case OnlineBankingLibrary.Models.CustomerRegistration.ResultCode.Normal:
                    resultCode = ResultCodes.normal;
                    break;
                case OnlineBankingLibrary.Models.CustomerRegistration.ResultCode.ValidationError:
                    resultCode = ResultCodes.validationError;
                    break;
                default:
                    break;
            }
            return resultCode;
        }

        public static ResultCodes FromPushNotificationService(PushNotificationService.ResultCode actionResult)
        {
            ResultCodes resultCode = new ResultCodes();

            switch (actionResult)
            {
                case PushNotificationService.ResultCode.Normal:
                    resultCode = ResultCodes.normal;
                    break;
            }
            return resultCode;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ValidationError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        public string ValidationMessage { get; }

        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;

            if (message.Contains("Could not convert"))
                ValidationMessage = "Մուտքագրված տվյալների սխալ ֆորմատ։ Wrong format of entered data."; 
            else
                ValidationMessage = message;
        }

        public static IEnumerable<ValidationError> GetValidationErrors(ModelStateDictionary modelState)
        {
            IEnumerable<ValidationError> errorList;

            errorList = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)).ToList()); ;
            return errorList;
        }

        public static string GetFormattedErrorMessage(List<string> errorList)
        {
            string errorMessage = "";

            foreach (string item in errorList)
            {
                errorMessage += (item + " "); //Համաձայն 123․ Ավանդ bug-ի
            }

            errorMessage = errorMessage.Remove(errorMessage.Length - 1);
            return errorMessage;
        }

        public static IActionResult GetValidationErrorResponse(ModelStateDictionary modelState)
        {
            SingleResponse<IEnumerable<ValidationError>> response = new SingleResponse<IEnumerable<ValidationError>>();
            response.ResultCode = ResultCodes.frontEndValidationError;
            response.Result = ValidationError.GetValidationErrors(modelState);
            return ResponseExtensions.ToHttpResponse(response);
        }

        public static IActionResult GetTypeValidationErrorResponse(string key, string message)
        {
            SingleResponse<List<ValidationError>> response = new SingleResponse<List<ValidationError>>();
            response.ResultCode = ResultCodes.frontEndValidationError;
            response.Result = new List<ValidationError>();
            response.Result.Add(new ValidationError(key, message));
            return ResponseExtensions.ToHttpResponse(response);
        }
    }
}
