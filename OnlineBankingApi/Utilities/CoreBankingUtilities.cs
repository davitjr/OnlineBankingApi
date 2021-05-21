using OnlineBankingLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBSInfo;
using XBS;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Models.Requests;

namespace OnlineBankingApi.Utilities
{
    public class CoreBankingUtilities
    {
        private readonly XBInfoService _xBInfoService;
        private readonly XBService _xBService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CoreBankingUtilities(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, XBService xBService, XBInfoService xBInfoService)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = configuration;
            _xBInfoService = xBInfoService;
            _xBService = xBService;
        }

        public string PerformBudgetPaymentOrderValidation(BudgetPaymentOrderRequest request)
        {
            bool hasError = false;


            List<string> ErrorMessages = new List<string>();

            if (_xBService.IsPoliceAccount(request.Order.ReceiverAccount.AccountNumber) && request.Order.PoliceCode <= 0)
            {
                ErrorMessages.Add("Անհրաժեշտ է մուտքագրել ոստիկանության կոդը։");
                hasError = true;
            }

            if (hasError == true)
            {
                return ValidationError.GetFormattedErrorMessage(ErrorMessages);
            }
            else
            {
                return String.Empty;
            }
        }
    }
}
