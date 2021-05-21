using OnlineBankingLibrary.Models.CustomerRegistration;
using OnlineBankingLibrary.Services;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ResultCode = XBManagement.ResultCode;

namespace OnlineBankingLibrary.Models
{
    public class OnlineBankingRegistrationManager
    {
        private readonly XBManagementService _xbManagementService;
        private readonly XBService _xbService;
        private readonly XBInfoService _xbInfoService;

        public OnlineBankingRegistrationManager(XBManagementService xbManagementService, XBService xbService, XBInfoService xbInfoService)
        {
            _xbManagementService = xbManagementService;
            _xbService = xbService;
        }

        /// <summary>
        /// Գեներացնում է օնլայն բանկինգ
        /// </summary>
        /// <param name="customerData"></param>
        /// <returns></returns>
        public RegistrationActionResult GenerateAcbaOnline(RegistrationCustomerData customerData)
        {
            var result = new RegistrationActionResult();
            var actionResult = _xbManagementService.GenerateAcbaOnline(customerData.UserName, customerData.Password, customerData.CustomerNumber, customerData.PhoneNumber,customerData.CustomerQuality,customerData.Email);

            StringBuilder stringBuilder = new StringBuilder();
            if (actionResult?.Errors != null)
            {
                foreach (var item in actionResult.Errors)
                {
                    if (actionResult.Errors.Count() == 1)
                    {
                        stringBuilder = stringBuilder.Append(item.Description);
                    }
                    else
                    {
                        stringBuilder = stringBuilder.AppendLine(item.Description);
                    }
                }
                result.Description = stringBuilder.ToString();
            }
            if (actionResult.ResultCode == ResultCode.Normal)
            {
                result.ResultCode = CustomerRegistration.ResultCode.Normal;
            }
            else if (actionResult.ResultCode == ResultCode.ValidationError)
            {
                result.ResultCode = CustomerRegistration.ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = CustomerRegistration.ResultCode.Failed;
            }
            return result;
        }


        /// <summary>
        /// Մինչ օնլայն բանկինգի գրանցման ստուգումներ
        /// </summary>
        /// <returns></returns>
        internal RegistrationActionResult ValidateAcbaOnlineCreating(RegistrationCustomerData customerData)
        {
            var resultValidation = new RegistrationActionResult
            {
                ResultCode = CustomerRegistration.ResultCode.Normal
            };

            if (!customerData.IsPhoneVerified)
            {
                resultValidation.ResultCode = CustomerRegistration.ResultCode.ValidationError;
                resultValidation.Description = "Not verified phone";
                return resultValidation;
            }

            if (!String.IsNullOrEmpty(customerData.UserName) && !String.IsNullOrEmpty(customerData.Password))
            {
                if (!(customerData.UserName.All(c => Char.IsLetterOrDigit(c)) &&
                    customerData.UserName.Length > 3 &&
                    customerData.Password.Length > 7 &&
                    customerData.Password.All(c => Char.IsLetterOrDigit(c)) &&
                    customerData.Password.Any(char.IsUpper) &&
                    customerData.Password.Any(char.IsNumber) &&
                    customerData.Password.Any(char.IsDigit)
                    ))
                {
                    resultValidation.ResultCode = CustomerRegistration.ResultCode.ValidationError;
                    resultValidation.Description = "Login pass error";
                    return resultValidation;
                }
            }
            else
            {
                resultValidation.ResultCode = CustomerRegistration.ResultCode.ValidationError;
                resultValidation.Description = "UserName or password is empty";
                return resultValidation;
            }

            if (String.IsNullOrEmpty(customerData.Email))
            {
                resultValidation.ResultCode = CustomerRegistration.ResultCode.ValidationError;
                resultValidation.Description = "Email does not exist";
                return resultValidation;
            }

            if (customerData.CustomerQuality != 1)
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(customerData.Email);
                if (!match.Success)
                {
                    resultValidation.ResultCode = CustomerRegistration.ResultCode.ValidationError;
                    resultValidation.Description = "Email regexp error";
                    return resultValidation;
                }
            }

            return resultValidation;
        }
    }
}

