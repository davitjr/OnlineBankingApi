using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Enumerations;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using OnlineBankingLibrary;
using Microsoft.Extensions.Localization;
using OnlineBankingApi.Resources;

namespace OnlineBankingApi.Filters
{
    public class ValidationMessage
    {   
        public static readonly string LogicValidationTypeDescription = "LogicValidationError";
    }
    public class CardTariffsByCardTypeRequestValidation : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
                .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
            if (value is CardTariffsByCardTypeRequest)
            {
                CardTariffsByCardTypeRequest item = value as CardTariffsByCardTypeRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (String.IsNullOrEmpty(item.Currency))
                {
                    ErrorMessages.Add(_localizer["Անհրաժեշտ է փոխանցել արժույթը։"]);
                    hasError = true;
                }

                if (item.CardType == 40 && item.PeriodicityType <= 0)
                {
                    ErrorMessages.Add(_localizer["AMEX քարտերի համար անհրաժեշտ է փոխանցել հաճախականություն։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class BudgetPaymentOrderRequestValidation : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
                .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is BudgetPaymentOrderRequest)
            {
                BudgetPaymentOrderRequest item = value as BudgetPaymentOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();
                if (item.Order.ReceiverAccount.AccountNumber.ToString()[0] != '9' && !item.Order.ReceiverAccount.AccountNumber.ToString().StartsWith("103009"))
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշվեհամարը բյուջե փոխանցման հաշվեհամար չէ:"]);
                    hasError = true;
                }

                if (item.Order.Type != XBS.OrderType.RATransfer)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }
                else
                {

                    if (item.Order.LTACode <= 0)
                    {
                        ErrorMessages.Add(_localizer["ՏՀՏ կոդը ընտրված չէ։"]);
                        hasError = true;
                    }
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class AccountStatementRequestValidation : ValidationAttribute 
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is AccountStatementRequest)
            {
                AccountStatementRequest item = value as AccountStatementRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                // SQL-ում նվազագույն արժեք
                if (item.DateFrom < Convert.ToDateTime("01/01/1753"))
                {
                    ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (item.DateTo < item.DateFrom)
                {
                    ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (String.IsNullOrWhiteSpace(item.AccountNumber))
                {
                    ErrorMessages.Add(_localizer["Հաշվեհամարը մուտքագրված չէ։"]);
                    hasError = true;
                }

                if (!item.AccountNumber.All(char.IsNumber))
                {
                    ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class DepositRepaymentsPriorRequestValidation : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is DepositRepaymentsPriorRequest)
            {
                DepositRepaymentsPriorRequest item = value as DepositRepaymentsPriorRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                // SQL-ում նվազագույն արժեք
                if (item.Request.StartDate < Convert.ToDateTime("01/01/1753"))
                {
                    ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (item.Request.EndDate < item.Request.StartDate)
                {
                    ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (item.Request.InterestRate < 0)
                {
                    ErrorMessages.Add(_localizer["Տոկոսադրույքը մուտքագրված չէ։"]);
                    hasError = true;
                }

                if (item.Request.StartCapital < 0)
                {
                    ErrorMessages.Add(_localizer["Սկզբնական գումարը նշված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.DepositType), (short)item.Request.DepositType))
                {
                    ErrorMessages.Add(_localizer["Ավանդի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (item.Request.AccountType < 1 || item.Request.AccountType > 3)
                {
                    ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if ((item.Request.AccountType == 2 || item.Request.AccountType == 3) && item.Request.ThirdPersonCustomerNumbers == null)
                {
                    if (item.Request.AccountType == 2)
                        ErrorMessages.Add(_localizer["Համատեղ շահառուն ընտրված չէ։"]);
                    else
                        ErrorMessages.Add(_localizer["Երրորդ անձն ընտրված չէ։"]);

                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class ActiveDepositTypesRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is ActiveDepositTypesRequest)
            {
                ActiveDepositTypesRequest item = value as ActiveDepositTypesRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();


                if (!Enum.IsDefined(typeof(XBS.OrderAccountType), (byte)item.AccountType))
                {
                    ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.CustomerTypes), (ushort)item.CustomerType))
                {
                    ErrorMessages.Add(_localizer["Հաճախորդի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.OrderAccountType), (byte)item.AccountType))
                {
                    ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.CustomerTypes), (ushort)item.CustomerType))
                {
                    ErrorMessages.Add(_localizer["Հաճախորդի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }

            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class AccountForOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is AccountForOrderRequest)
            {
                AccountForOrderRequest item = value as AccountForOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (!Enum.IsDefined(typeof(XBS.OrderType), (short)item.OrderType))
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.OrderAccountType), (byte)item.AccountType))
                {
                    ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class AccountOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is AccountOrderRequest)
            {
                AccountOrderRequest item = value as AccountOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Order.SubType != 1)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                    hasError = true;
                }

                if (item.Order.Type != XBS.OrderType.CurrentAccountOpen)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակն ընտրված չէ"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.OrderAccountType), (byte)item.Order.AccountType))
                {
                    ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class CurrencyExchangeOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is CurrencyExchangeOrderRequest)
            {
                CurrencyExchangeOrderRequest item = value as CurrencyExchangeOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();


                if (item.Order.Type != XBS.OrderType.Convertation)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                //if (!Enum.IsDefined(typeof(XBS.ExchangeRoundingDirectionType), (byte)item.Order.RoundingDirection))
                //{
                //    ErrorMessages.Add("Փոխարկման կլորացման ուղղությունը նշված չէ");
                //    hasError = true;
                //}

                if (item.Order.ReceiverAccount != null)
                {
                    //if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.Currency))
                    //{
                    //    ErrorMessages.Add("Մուտքագրվող հաշվի արժույթ դաշտը պարտադիր է։");
                    //    hasError = true;
                    //}

                    if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.ReceiverAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Order.DebitAccount != null)
                {
                    //if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.Currency))
                    //{
                    //    ErrorMessages.Add("Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։");
                    //    hasError = true;
                    //}

                    if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.DebitAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Order.Currency))
                {
                    ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                    hasError = true;
                }

                if (item.Order.UseCreditLine)
                {
                    ErrorMessages.Add(_localizer["Տարբեր արժույթով հաշիվների միջև փոխանցում հնարավոր չէ իրականացնել վարկային գծի միջոցների հաշվին։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class CodeRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is CodeRequest)
            {
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();
                CodeRequest item = value as CodeRequest;

                if (item.Code <= 0)
                {
                    ErrorMessages.Add(_localizer["Բանկի կոդը սխալ է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class ProductQualityFilterRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is ProductQualityFilterRequest)
            {
                ProductQualityFilterRequest item = value as ProductQualityFilterRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (!Enum.IsDefined(typeof(XBS.ProductQualityFilter), (short)item.Filter))
                {
                    ErrorMessages.Add(_localizer["Փոխանցված կարգավիճակի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class PaymentOrderTemplateRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is PaymentOrderTemplateRequest)
            {
                PaymentOrderTemplateRequest item = value as PaymentOrderTemplateRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Template.PaymentOrder.DebitAccount != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Template.PaymentOrder.DebitAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.PaymentOrder.DebitAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Template.PaymentOrder.DebitAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Template.PaymentOrder.ReceiverAccount != null)
                {
                    if (!(item.Template.TemplateDocumentType == XBS.OrderType.RATransfer && (item.Template.TemplateDocumentSubType == 2 || item.Template.PaymentOrder.SubType == 5))
                        && string.IsNullOrWhiteSpace(item.Template.PaymentOrder.ReceiverAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.PaymentOrder.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Template.PaymentOrder.ReceiverAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentType != XBS.OrderType.RATransfer)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.ExchangeDirection), (int)item.Template.TemplateType))
                {
                    ErrorMessages.Add(_localizer["Ձևանմուշի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentSubType < 1 || item.Template.TemplateDocumentSubType > 6)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class CardToCardOrderTemplateRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is CardToCardOrderTemplateRequest)
            {
                CardToCardOrderTemplateRequest item = value as CardToCardOrderTemplateRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Template.TemplateDocumentType != XBS.OrderType.CardToCardOrder)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.ExchangeDirection), (int)item.Template.TemplateType))
                {
                    ErrorMessages.Add(_localizer["Ձևանմուշի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentSubType != 1)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակն ընտրված չէ։"]);
                    hasError = true;
                }


                if (item.Template.CardToCardOrder != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Template.CardToCardOrder.Currency))
                    {
                        ErrorMessages.Add(_localizer["Քարտից քարտ փոխանցման արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if(String.IsNullOrEmpty(item.Template.CardToCardOrder.CreditCardNumber))
                    {
                        ErrorMessages.Add(_localizer["Ստացողի քարտը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (String.IsNullOrEmpty(item.Template.CardToCardOrder.DebitCardNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող քարտը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                }
                else
                {
                    ErrorMessages.Add(_localizer["Քարտից քարտ փոխանցման տվյալները լրացված չեն։"]);
                    hasError = true;
                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class CardToCardTransferFeeRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is CardToCardTransferFeeRequest)
            {
                CardToCardTransferFeeRequest item = value as CardToCardTransferFeeRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (!item.DebitCardNumber.All(char.IsNumber))
                {
                    ErrorMessages.Add(_localizer["Անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                    hasError = true;
                }

                if (item.CreditCardNumber != null && !item.CreditCardNumber.All(char.IsNumber))
                {
                    ErrorMessages.Add(_localizer["Անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class CardToCardOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is CardToCardOrderRequest)
            {
                CardToCardOrderRequest item = value as CardToCardOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (string.IsNullOrWhiteSpace(item.Order.EmbossingName))
                {
                    ErrorMessages.Add(_localizer["Անուն Ազգանունը բացակայում է։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Order.DebitCardNumber))
                {
                    ErrorMessages.Add(_localizer["Ելքագրվող  քարտի համարը բացակայում է։"]);
                    hasError = true;
                }
                else
                {
                    if (!item.Order.DebitCardNumber.All(char.IsNumber))
                    {
                        ErrorMessages.Add(_localizer["Անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                        hasError = true;
                    }
                }

                if (string.IsNullOrWhiteSpace(item.Order.CreditCardNumber))
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող քարտի համարը բացակայում է։"]);
                    hasError = true;
                }
                else
                {
                    if (!item.Order.CreditCardNumber.All(char.IsNumber))
                    {
                        ErrorMessages.Add(_localizer["Անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                        hasError = true;
                    }
                }

                if (item.Order.Amount <= 0)
                {
                    ErrorMessages.Add(_localizer["Գումարը պետք է մեծ լինի 0-ից։"]);
                    hasError = true;
                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class ReceivedFastTransferPaymentOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is ReceivedFastTransferPaymentOrderRequest)
            {
                ReceivedFastTransferPaymentOrderRequest item = value as ReceivedFastTransferPaymentOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Order.Type != XBS.OrderType.ReceivedFastTransferPaymentOrder)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }

                if (item.Order.SubType != 0)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                    hasError = true;
                }

                if (item.Order.ReceiverAccount != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Հավշեհամարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.ReceiverAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    } 
                }
                else
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class LastExchangeRateRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is LastExchangeRateRequest)
            {
                LastExchangeRateRequest item = value as LastExchangeRateRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (!Enum.IsDefined(typeof(XBS.RateType), (int)item.RateType))
                {
                    ErrorMessages.Add(_localizer["Փոխարժեքի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.ExchangeDirection), (int)item.Direction))
                {
                    ErrorMessages.Add(_localizer["Փոխարկման տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Currency))
                {
                    ErrorMessages.Add(_localizer["Արժույթն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class AccountClosingOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is AccountClosingOrderRequest)
            {
                AccountClosingOrderRequest item = value as AccountClosingOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Order.Type != XBS.OrderType.CurrentAccountClose)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }

                for (int i = 0; i < item.Order.ClosingAccounts.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(item.Order.ClosingAccounts[i].Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }
                    if (string.IsNullOrWhiteSpace(item.Order.ClosingAccounts[i].AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Հավշեհամարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.ClosingAccounts[i].AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class DepositTerminationOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is DepositTerminationOrderRequest)
            {
                DepositTerminationOrderRequest item = value as DepositTerminationOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Order.ClosingReasonType == 0)
                {
                    ErrorMessages.Add(_localizer["Պատճառն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class OrderListFilterRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is OrderListFilterRequest)
            {
                OrderListFilterRequest item = value as OrderListFilterRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                // SQL-ում նվազագույն արժեք
                if (item.OrderListFilter.DateTo < Convert.ToDateTime("01/01/1753"))
                {
                    ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (item.OrderListFilter.DateTo < item.OrderListFilter.DateFrom)
                {
                    ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.SourceType), (short)item.OrderListFilter.Source))
                {
                    ErrorMessages.Add(_localizer["Աղբյուրն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class TemplateIdRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is TemplateIdRequest)
            {
                TemplateIdRequest item = value as TemplateIdRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.TemplateId <= 0)
                {
                    ErrorMessages.Add(_localizer["Ձևանմուշի ունիկալ համարը բացակայում է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

        }
    }
    public class SaveOrderGroupRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is SaveOrderGroupRequest)
            {
                SaveOrderGroupRequest item = value as SaveOrderGroupRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (string.IsNullOrWhiteSpace(item.Group.GroupName))
                {
                    ErrorMessages.Add(_localizer["Խմբի անվանումը նշված չէ։"]);
                    hasError = true;
                }

                if (item.Group.Type != XBS.OrderGroupType.CreatedByCustomer && item.Group.Type != XBS.OrderGroupType.CreatedAutomatically)
                {
                    ErrorMessages.Add(_localizer["Գործարքների խմբի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.OrderGroupStatus), (short)item.Group.Status))
                {
                    ErrorMessages.Add(_localizer["Խմբի կարգավիճակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class PeriodicPaymentOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is PeriodicPaymentOrderRequest)
            {
                PeriodicPaymentOrderRequest item = value as PeriodicPaymentOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Order.Type != XBS.OrderType.PeriodicTransfer)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }

                if (item.Order.SubType < 1 || item.Order.SubType > 5)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Order.PeriodicDescription) && item.Order.SubType != 3)
                {
                    ErrorMessages.Add(_localizer["Պարբերական փոխանցման նկարագրությունը պարտադիր է։"]);
                    hasError = true;
                }

                if (item.Order.GroupId < 0)
                {
                    ErrorMessages.Add(_localizer["Խմբի համարը չի կարող բացասական լինել։"]);
                    hasError = true;
                }
                if (item.Order.ChargeType != 1)
                {
                    ErrorMessages.Add(_localizer["Պարբերական փոխանցման գումարի գանձման եղանակը սխալ է։"]);
                    hasError = true;
                }

                if (item.Order.PaymentOrder.DebitAccount != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Order.PaymentOrder.DebitAccount.Currency) && item.Order.SubType != 1)
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.PaymentOrder.DebitAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.PaymentOrder.DebitAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Order.PaymentOrder.ReceiverAccount != null)
                {
                    if (!(item.Order.SubType == 1 && (item.Order.PeriodicType == 2 || item.Order.PeriodicType == 1)) && string.IsNullOrWhiteSpace(item.Order.PaymentOrder.ReceiverAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.PaymentOrder.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.PaymentOrder.ReceiverAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class ListDocIdRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is ListDocIdRequest)
            {
                ListDocIdRequest item = value as ListDocIdRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.ListDocId.Count == 0)
                {
                    ErrorMessages.Add(_localizer["գործարքի համարը բացակայում է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class IdRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is IdRequest)
            {
                IdRequest item = value as IdRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Id <= 0)
                {
                    ErrorMessages.Add(_localizer["գործարքի համարը բացակայում է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }


    public class CardStatementRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is CardStatementRequest)
            {
                CardStatementRequest item = value as CardStatementRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                // SQL-ում նվազագույն արժեք
                if (item.DateFrom < Convert.ToDateTime("01/01/1753"))
                {
                    ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (item.DateTo < item.DateFrom)
                {
                    ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                    hasError = true;
                }

                if (String.IsNullOrWhiteSpace(item.CardNumber))
                {
                    ErrorMessages.Add(_localizer["Քարտի համարը մուտքագրված չէ։"]);
                    hasError = true;
                }
                else
                {
                    if (!item.CardNumber.All(char.IsNumber))
                    {
                        ErrorMessages.Add(_localizer["Քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                        hasError = true;
                    }
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }

        public class OrderGroupRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is OrderGroupRequest)
                {
                    OrderGroupRequest item = value as OrderGroupRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.OrderGroupStatus), (short)item.Status))
                    {
                        ErrorMessages.Add(_localizer["Կարգավիճակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (!Enum.IsDefined(typeof(XBS.OrderGroupType), (short)item.GroupType))
                    {
                        ErrorMessages.Add(_localizer["Խմբի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class ProductIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ProductIdRequest)
                {
                    ProductIdRequest item = value as ProductIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();



                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class DepositTypeRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DepositTypeRequest)
                {
                    DepositTypeRequest item = value as DepositTypeRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (!Enum.IsDefined(typeof(XBS.DepositType), item.DepositType))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class AccountNumberRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AccountNumberRequest)
                {
                    AccountNumberRequest item = value as AccountNumberRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Հավշեհամարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class CardNumberRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardNumberRequest)
                {
                    CardNumberRequest item = value as CardNumberRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.CardNumber))
                    {
                        ErrorMessages.Add(_localizer["Քարտի համարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.CardNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    if (item.CardNumber.Length > 20)
                    {
                        ErrorMessages.Add(_localizer["Քարտի համարը առավելագույնը կարող է լինել 20 նիշ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class DocIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                if (value is DocIdRequest)
                {
                    DocIdRequest item = value as DocIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.DocId <= 0)
                    {
                        ErrorMessages.Add(_localizer["Գործարքի համարը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class PeriodicTerminationOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PeriodicTerminationOrderRequest)
                {
                    PeriodicTerminationOrderRequest item = value as PeriodicTerminationOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.Type != XBS.OrderType.PeriodicTransferStop)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.DebitAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.DebitAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.CreditAccount))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.CreditAccount.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class PeriodicTransferDataChangeOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PeriodicTransferDataChangeOrderRequest)
                {
                    PeriodicTransferDataChangeOrderRequest item = value as PeriodicTransferDataChangeOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ArcaCardsTransactionOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ArcaCardsTransactionOrderRequest)
                {
                    ArcaCardsTransactionOrderRequest item = value as ArcaCardsTransactionOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (String.IsNullOrWhiteSpace(item.Order.CardNumber))
                    {
                        ErrorMessages.Add(_localizer["Քարտի համարը մուտքագրված չէ։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.CardNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    if (item.Order.ActionType != 1 && item.Order.ActionType != 2)
                    {
                        ErrorMessages.Add(_localizer["Գործողության տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.ActionReasonId != 9 && item.Order.ActionReasonId != 13)
                    {
                        ErrorMessages.Add(_localizer["Բլոկավորման պատճառն ընտրված չէ։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CardLimitChangeOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardLimitChangeOrderRequest)
                {
                    CardLimitChangeOrderRequest item = value as CardLimitChangeOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (String.IsNullOrWhiteSpace(item.Order.CardNumber))
                    {
                        ErrorMessages.Add(_localizer["Քարտի համարը մուտքագրված չէ։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Order.CardNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                    if (item.Order.GroupId < 0)
                    {
                        ErrorMessages.Add(_localizer["Խմբի համարը չի կարող բացասական լինել։"]);
                        hasError = true;
                    }
                    if (item.Order.Limits != null)
                    {
                        foreach (var Limits in item.Order.Limits)
                        {
                            if (!Enum.IsDefined(typeof(XBS.LimitType), (short)Limits.Limit))
                            {
                                ErrorMessages.Add(_localizer["Սահմանաչափի տեսակ ընտրված չէ։"]);
                                hasError = true;
                            }
                            if (Limits.LimitValue <= 0)
                            {
                                ErrorMessages.Add(_localizer["Սահմանաչափի արժեքն ընտրված չէ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Սահմանաչափերն ընտրված չեն։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class PaymentOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PaymentOrderRequest)
                {
                    PaymentOrderRequest item = value as PaymentOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.Type != XBS.OrderType.RATransfer)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.ReceiverAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.ReceiverAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }

                            if (item.Order.ReceiverBankCode.ToString() != item.Order.ReceiverAccount.AccountNumber.Substring(0, 5))
                            {
                                ErrorMessages.Add(_localizer["Ստացող բանկի կոդը չի համապատասխանում փոխանցվող հաշվեհամարին։"]);
                                hasError = true;
                            }
                        }

                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.DebitAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.Currency))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                            hasError = true;
                        }

                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.DebitAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.SubType != 3)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.Description))
                        {
                            ErrorMessages.Add(_localizer["Հաղորդագրության դաշտը պարտադիր է:"]);
                            hasError = true;
                        }
                    }
                    if (item.Order.SubType < 1 || item.Order.SubType > 4)
                    {
                        ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                        hasError = true;
                    }
                    else
                    {
                        switch (item.Order.SubType)
                        {
                            case 2:

                                if (item.Order.CreditorStatus == 0)
                                {
                                    if (string.IsNullOrWhiteSpace(item.Order.Receiver))
                                    {
                                        ErrorMessages.Add(_localizer["Ստացողի անուն ազգանունը բացակայում է։"]);
                                        hasError = true;
                                    }
                                }
                                else
                                {

                                    if (string.IsNullOrWhiteSpace(item.Order.CreditorDescription))
                                    {
                                        ErrorMessages.Add(_localizer["Այլ անձի (պարտատիրոջ) անուն ազգանուն / անվանումը բացակայում է։"]);
                                        hasError = true;
                                    }

                                    if (string.IsNullOrWhiteSpace(item.Order.CreditorDocumentNumber))
                                    {
                                        ErrorMessages.Add(_localizer["Այլ անձի (պարտատիրոջ) փաստաթղթի համարը բացակայում է։"]);
                                        hasError = true;
                                    }
                                    else
                                    {
                                        if (!item.Order.CreditorDocumentNumber.All(char.IsNumber))
                                        {
                                            ErrorMessages.Add(_localizer["Փաստաթղթի համարը դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                            hasError = true;
                                        }
                                    }

                                }
                                break;
                            case 3:
                                if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.Currency))
                                {
                                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի Արժույթը պարտադիր է։"]);
                                    hasError = true;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    if (item.Order.Amount <= 0)
                    {
                        ErrorMessages.Add(_localizer["Գումար մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class CardFeeRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardFeeRequest)
                {
                    CardFeeRequest item = value as CardFeeRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.OrderType), (short)item.Order.Type))
                    {
                        ErrorMessages.Add(_localizer["Տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.DebitAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.Currency))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                            hasError = true;
                        }

                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.DebitAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.Amount <= 0)
                    {
                        ErrorMessages.Add(_localizer["Գումարը պետք է մեծ լինի 0-ից։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class PaymentOrderFeeRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PaymentOrderFeeRequest)
                {
                    PaymentOrderFeeRequest item = value as PaymentOrderFeeRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.ReceiverAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.ReceiverAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                            if (item.Order.ReceiverBankCode.ToString() != item.Order.ReceiverAccount.AccountNumber.Substring(0, 5))
                            {
                                ErrorMessages.Add(_localizer["Ստացող բանկի կոդը չի համապատասխանում փոխանցվող հաշվեհամարին։"]);
                                hasError = true;
                            }
                        }

                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.DebitAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.Currency))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                            hasError = true;
                        }

                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.DebitAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.Amount <= 0)
                    {
                        ErrorMessages.Add(_localizer["Գումարը պետք է մեծ լինի 0-ից։"]);
                        hasError = true;
                    }


                    if (item.Order.SubType < 1 || item.Order.SubType > 6)
                    {
                        ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class CheckDepositOrderConditionRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CheckDepositOrderConditionRequest)
                {
                    CheckDepositOrderConditionRequest item = value as CheckDepositOrderConditionRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.DepositType), (short)item.Order.DepositType))
                    {
                        ErrorMessages.Add(_localizer["Տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.Deposit.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    // SQL-ում նվազագույն արժեք
                    if (item.Order.Deposit.StartDate < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class DepositOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DepositOrderRequest)
                {
                    DepositOrderRequest item = value as DepositOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.Type != XBS.OrderType.Deposit)
                    {
                        ErrorMessages.Add(_localizer["Տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.DepositType == XBS.DepositType.None)
                    {
                        ErrorMessages.Add(_localizer["Տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.SubType != 1)
                    {
                        ErrorMessages.Add(_localizer["Հայտի Ենթատեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.PercentAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.PercentAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Տոկոսագումարի հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.PercentAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Տոկոսագումարի հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Տոկոսագումարի հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.Deposit.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.AccountType < 1 || item.Order.AccountType > 3)
                    {
                        ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.DepositType == XBS.DepositType.BusinesDeposit)
                    {
                        if (item.Order.Deposit.DepositOption == null)
                        {
                            ErrorMessages.Add(_localizer["Ավանդի տեսակն ընտրված չէ։"]);
                            hasError = true;
                        }
                    }

                    if (item.Order.Deposit.EndDate <= DateTime.Now)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class AccountsForDepositPercentAccountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AccountsForDepositPercentAccountRequest)
                {
                    AccountsForDepositPercentAccountRequest item = value as AccountsForDepositPercentAccountRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.AccountType < 1 || item.Order.AccountType > 3)
                    {
                        ErrorMessages.Add("Հաշվի տեսակն ընտրված չէ։");
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class AccountsForNewDepositRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                  .GetService(typeof(IStringLocalizer<SharedResource>));

                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AccountsForNewDepositRequest)
                {
                    AccountsForNewDepositRequest item = value as AccountsForNewDepositRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.AccountType < 1 || item.Order.AccountType > 3)
                    {
                        ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (!Enum.IsDefined(typeof(XBS.DepositType), (short)item.Order.DepositType))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class DepositConditionRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DepositConditionRequest)
                {
                    DepositConditionRequest item = value as DepositConditionRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Order.Deposit.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ավանդի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.Deposit.EndDate < item.Order.Deposit.StartDate)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.AccountType < 1 || item.Order.AccountType > 3)
                    {
                        ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    // SQL-ում նվազագույն արժեք
                    if (item.Order.Deposit.StartDate < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class PlasticCardOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PlasticCardOrderRequest)
                {
                    PlasticCardOrderRequest item = value as PlasticCardOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.PlasticCard.SupplementaryType != XBS.SupplementaryType.Attached)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.MotherName))
                        {
                            ErrorMessages.Add(_localizer["Գաղտնաբառ դաշտը պարտադիր է։"]);
                            hasError = true;
                        }
                    }

                    if (item.Order.CardActionType != 0)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.PlasticCard != null)
                    {
                        if (!Enum.IsDefined(typeof(XBS.SupplementaryType), (uint)item.Order.PlasticCard.SupplementaryType))
                        {
                            ErrorMessages.Add(_localizer["Քարտի տեսակն ընտրված չէ։"]);
                            hasError = true;
                        }
                        else
                        {
                            switch (item.Order.PlasticCard.SupplementaryType)
                            {
                                case XBS.SupplementaryType.Main:

                                    if (item.Order.PlasticCard.CardType == 0)
                                    {
                                        ErrorMessages.Add(_localizer["Քարտի տեսակն ընտրված չէ։"]);
                                        hasError = true;
                                    }
                                    if (string.IsNullOrWhiteSpace(item.Order.PlasticCard.Currency))
                                    {
                                        ErrorMessages.Add(_localizer["Քարտի արժույթն ընտրված չէ։"]);
                                        hasError = true;
                                    }
                                    break;
                                case XBS.SupplementaryType.Linked:

                                    if (string.IsNullOrWhiteSpace(item.Order.CardSMSPhone))
                                    {
                                        ErrorMessages.Add(_localizer["Հեռախոսահամարը մուտքագրված չէ։"]);
                                        hasError = true;
                                    }
                                    else
                                    {
                                        if (!item.Order.CardSMSPhone.All(char.IsNumber))
                                        {
                                            ErrorMessages.Add(_localizer["Հեռախոսահամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                            hasError = true;
                                        }
                                    }

                                    if (String.IsNullOrWhiteSpace(item.Order.PlasticCard.MainCardNumber))
                                    {
                                        ErrorMessages.Add(_localizer["Հիմնական քարտի համարը մուտքագրված չէ։"]);
                                        hasError = true;
                                    }
                                    else
                                    {
                                        if (!item.Order.PlasticCard.MainCardNumber.All(char.IsNumber))
                                        {
                                            ErrorMessages.Add(_localizer["Հիմնական քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                            hasError = true;
                                        }
                                    }
                                    break;
                                case XBS.SupplementaryType.Attached:
                                    if (item.Order.PlasticCard.CardType == 0)
                                    {
                                        ErrorMessages.Add(_localizer["Քարտի տեսակն ընտրված չէ։"]);
                                        hasError = true;
                                    }

                                    if (String.IsNullOrWhiteSpace(item.Order.PlasticCard.MainCardNumber))
                                    {
                                        ErrorMessages.Add(_localizer["Հիմնական քարտի համարը մուտքագրված չէ։"]);
                                        hasError = true;
                                    }
                                    else
                                    {
                                        if (!item.Order.PlasticCard.MainCardNumber.All(char.IsNumber))
                                        {
                                            ErrorMessages.Add(_localizer["Հիմնական քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                            hasError = true;
                                        }
                                    }

                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Պլաստիկ քարտն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class AccountNumberAndDateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                //string str = Resources.AccountNumberAndDateRequest[_localizer["Հարցման պարունակությունը դատարկ է։"]];
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                     .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AccountNumberAndDateRequest)
                {
                    AccountNumberAndDateRequest item = value as AccountNumberAndDateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.DateTo < item.DateFrom)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    // SQL-ում նվազագույն արժեք
                    if (item.DateFrom < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }
                    if (string.IsNullOrWhiteSpace(item.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class DepositAndCurrentAccCurrenciesRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));

                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DepositAndCurrentAccCurrenciesRequest)
                {
                    DepositAndCurrentAccCurrenciesRequest item = value as DepositAndCurrentAccCurrenciesRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.OrderType), (short)item.OrderType))
                    {
                        ErrorMessages.Add(_localizer["Քարտի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (!Enum.IsDefined(typeof(XBS.OrderAccountType), (byte)item.OrderAccountType))
                    {
                        ErrorMessages.Add(_localizer["Քարտի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class LoanFullNumberRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LoanFullNumberRequest)
                {
                    LoanFullNumberRequest item = value as LoanFullNumberRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.LoanFullNumber))
                    {
                        ErrorMessages.Add(_localizer["Հարկային հաշիվը նշված չէ։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.LoanFullNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Վարկային հաշիվ դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CardClosingOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardClosingOrderRequest)
                {
                    CardClosingOrderRequest item = value as CardClosingOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.Type != XBS.OrderType.CardClosing)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class LinkedAndAttachedCardsRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LinkedAndAttachedCardsRequest)
                {
                    LinkedAndAttachedCardsRequest item = value as LinkedAndAttachedCardsRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.ProductId == 0)
                    {
                        ErrorMessages.Add(_localizer["Պրոդուկտի ունիկալ համարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (item.ProductFilter != XBS.ProductQualityFilter.Opened && item.ProductFilter != XBS.ProductQualityFilter.Closed && item.ProductFilter != XBS.ProductQualityFilter.All)
                    {
                        ErrorMessages.Add(_localizer["Փոխանցված կարգավիճակի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class CVVNoteRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                  .GetService(typeof(IStringLocalizer<SharedResource>));

                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CVVNoteRequest)
                {
                    CVVNoteRequest item = value as CVVNoteRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.ProductId == 0)
                    {
                        ErrorMessages.Add(_localizer["Պրոդուկտի ունիկալ համարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (!String.IsNullOrEmpty(item.CVVNote))
                    {
                        if (item.CVVNote.Length != 3 && item.CVVNote.Length != 4)
                        {
                            ErrorMessages.Add(_localizer["CVV կոդը պետք է լինի 3 կամ 4 նիշ։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class OrderIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is OrderIdRequest)
                {
                    OrderIdRequest item = value as OrderIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.OrderId <= 0)
                    {
                        ErrorMessages.Add(_localizer["գործարքի համարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ContactRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ContactRequest)
                {
                    ContactRequest item = value as ContactRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Contact.ContactAccountList != null)
                    {
                        foreach (var AccList in item.Contact.ContactAccountList)
                        {
                            if (string.IsNullOrWhiteSpace(AccList.AccountNumber))
                            {
                                ErrorMessages.Add(_localizer["Հաշվեհամարը բացակայում է։"]);
                                hasError = true;
                            }
                            else
                            {
                                if (!AccList.AccountNumber.All(char.IsNumber))
                                {
                                    ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                    hasError = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Հաշվեհամարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class UpdateContactRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is UpdateContactRequest)
                {
                    UpdateContactRequest item = value as UpdateContactRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Contact.ContactAccountList != null)
                    {
                        foreach (var AccList in item.Contact.ContactAccountList)
                        {
                            if (string.IsNullOrWhiteSpace(AccList.AccountNumber))
                            {
                                ErrorMessages.Add(_localizer["Հաշվեհամարը բացակայում է։"]);
                                hasError = true;
                            }
                            else
                            {
                                if (!AccList.AccountNumber.All(char.IsNumber))
                                {
                                    ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                    hasError = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Հաշվեհամարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Contact.Id <= 0)
                    {
                        ErrorMessages.Add(_localizer["Կոնտակտի ունիկալ համարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class LoanContractRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LoanContractRequest)
                {
                    LoanContractRequest item = value as LoanContractRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();



                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class PrintContractRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PrintContractRequest)
                {
                    PrintContractRequest item = value as PrintContractRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();



                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class PrintCardStatementRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PrintCardStatementRequest)
                {
                    PrintCardStatementRequest item = value as PrintCardStatementRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (string.IsNullOrWhiteSpace(item.CardNumber))
                    {
                        ErrorMessages.Add(_localizer["Քարտի համարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.CardNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Քարտի Համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    DateTime isDate = default;
                    DateTime.TryParse(item.DateFrom, out isDate);
                    if (isDate == default)
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթիվը բացակայում է։"]);
                        hasError = true;
                    }
                    DateTime.TryParse(item.DateTo, out isDate);
                    if (isDate == default)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class PrintLoanTermSheetRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PrintLoanTermSheetRequest)
                {
                    PrintLoanTermSheetRequest item = value as PrintLoanTermSheetRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();



                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CreditLineTerminationOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CreditLineTerminationOrderRequest)
                {
                    CreditLineTerminationOrderRequest item = value as CreditLineTerminationOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (item.Order.ProductId <= 0)
                    {
                        ErrorMessages.Add(_localizer["Պրոդուկտի ունիկալ համարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CreditLineLastDateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CreditLineLastDateRequest)
                {
                    CreditLineLastDateRequest item = value as CreditLineLastDateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (string.IsNullOrWhiteSpace(item.CardNumber))
                    {
                        ErrorMessages.Add(_localizer["Քարտի համարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.CardNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Քարտի համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CreditLineProvisionAmountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CreditLineProvisionAmountRequest)
                {
                    CreditLineProvisionAmountRequest item = value as CreditLineProvisionAmountRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Amount <= 0)
                    {
                        ErrorMessages.Add(_localizer["Գումար մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (item.CreditLineType <= 0)
                    {
                        ErrorMessages.Add(_localizer["Վարկային գծի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.LoanCurrency))
                    {
                        ErrorMessages.Add(_localizer["Վարկի արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.ProvisionCurrency))
                    {
                        ErrorMessages.Add(_localizer["Գրավադրված գումարի արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CurrencyRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CurrencyRequest)
                {
                    CurrencyRequest item = value as CurrencyRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CardTypeRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardTypeRequest)
                {
                    CardTypeRequest item = value as CardTypeRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();



                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CustomerNumberRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CustomerNumberRequest)
                {
                    CustomerNumberRequest item = value as CustomerNumberRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();



                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class DepositLoanAndProvisionAmountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DepositLoanAndProvisionAmountRequest)
                {
                    DepositLoanAndProvisionAmountRequest item = value as DepositLoanAndProvisionAmountRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.LoanCurrency))
                    {
                        ErrorMessages.Add(_localizer["Վարկի արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.ProvisionCurrency))
                    {
                        ErrorMessages.Add(_localizer["Գրավադրված գումարի արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class RedemptionAmountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is RedemptionAmountRequest)
                {
                    RedemptionAmountRequest item = value as RedemptionAmountRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    // SQL-ում նվազագույն արժեք
                    if (item.DateOfBeginning < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.DateOfNormalEnd < item.DateOfBeginning)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    // SQL-ում նվազագույն արժեք
                    if (item.FirstRepaymentDate < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Առաջին վճարման օրվա արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CommisionAmountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CommisionAmountRequest)
                {
                    CommisionAmountRequest item = value as CommisionAmountRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    // SQL-ում նվազագույն արժեք
                    if (item.DateOfBeginning < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.DateofNormalEnd < item.DateOfBeginning)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }


                    if (item.StartCapital < 0)
                    {
                        ErrorMessages.Add(_localizer["Սկզբնական գումարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class UploadedFileRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is UploadedFileRequest)
                {
                    UploadedFileRequest item = value as UploadedFileRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.UploadedFile.FileType))
                    {
                        ErrorMessages.Add(_localizer["Ֆայլի տեսակը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.UploadedFile.FileName))
                    {
                        ErrorMessages.Add(_localizer["Ֆայլի անունը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.UploadedFile.FileName))
                    {
                        ErrorMessages.Add(_localizer["Ֆայլի անունը բացակայում է։"]);
                        hasError = true;
                    }

                    if (String.IsNullOrEmpty(item.UploadedFile.FileInBase64))
                    {
                        ErrorMessages.Add(_localizer["Ֆայլը բացակայում է։"]);
                        hasError = true;
                    }
                    //if (item.UploadedFile.CustomerNumber == 0)
                    //{
                    //    ErrorMessages.Add("Հաճախորդի համարը բացակայում է։");
                    //    hasError = true;
                    //}

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class GroupIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is GroupIdRequest)
                {
                    GroupIdRequest item = value as GroupIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.GroupId == 0)
                    {
                        ErrorMessages.Add(_localizer["Խմբի ունիկալ համարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class HBServletRequestOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is HBServletRequestOrderRequest)
                {
                    HBServletRequestOrderRequest item = value as HBServletRequestOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.HBtoken != null)
                    {
                        if (item.Order.HBtoken.ID == 0)
                        {
                            ErrorMessages.Add(_localizer["Տոկենի ունիկալ համարը բացակայում է։"]);
                            hasError = true;
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Տոկենի ունիկալ համարը բացակայում է։"]);
                        hasError = true;
                    }
                    if (item.Order.ServletRequest != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.ServletRequest.OTP))
                        {
                            ErrorMessages.Add(_localizer["Մեկանգամյա գաղտնաբառը մուտքագրված չէ։"]);
                            hasError = true;
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Մեկանգամյա գաղտնաբառը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.Type != XBS.OrderType.HBServletRequestTokenActivationOrder)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ReferenceTypesRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ReferenceTypesRequest)
                {
                    ReferenceTypesRequest item = value as ReferenceTypesRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.ReferenceTypes.Count == 0)
                    {
                        ErrorMessages.Add(_localizer["Ներկայացման վայրն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class TransferSystemRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is TransferSystemRequest)
                {
                    TransferSystemRequest item = value as TransferSystemRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.TransferSystem == 0)
                    {
                        ErrorMessages.Add(_localizer["Փոխանցման համակարգը ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CommunalTypesRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CommunalTypesRequest)
                {
                    CommunalTypesRequest item = value as CommunalTypesRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.CommunalTypes), (short)item.CommunalType))
                    {
                        ErrorMessages.Add(_localizer["Կոմունալի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CardSystemRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardSystemRequest)
                {
                    CardSystemRequest item = value as CardSystemRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CurrenciesPlasticCardRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CurrenciesPlasticCardRequest)
                {
                    CurrenciesPlasticCardRequest item = value as CurrenciesPlasticCardRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CredentialTypesRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                  .GetService(typeof(IStringLocalizer<SharedResource>));

                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CredentialTypesRequest)
                {
                    CredentialTypesRequest item = value as CredentialTypesRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.CustomerTypes), (ushort)item.TypeOfCustomer))
                    {
                        ErrorMessages.Add(_localizer["Հաճախորդի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CBKursForDateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CBKursForDateRequest)
                {
                    CBKursForDateRequest item = value as CBKursForDateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    // SQL-ում նվազագույն արժեք
                    if (item.Date < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class AmountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AmountRequest)
                {
                    AmountRequest item = value as AmountRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Amount != 50000 && item.Amount != 100000)
                    {
                        ErrorMessages.Add(_localizer["Արագ օվերդրաֆթի գումարն ընտրված չէ։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class BusinesDepositOptionRateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is BusinesDepositOptionRateRequest)
                {
                    BusinesDepositOptionRateRequest item = value as BusinesDepositOptionRateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class DateWorkingDayRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DateWorkingDayRequest)
                {
                    DateWorkingDayRequest item = value as DateWorkingDayRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    // SQL-ում նվազագույն արժեք
                    if (item.DateWorkingDay < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CardServiceFeeRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CardServiceFeeRequest)
                {
                    CardServiceFeeRequest item = value as CardServiceFeeRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class AttachedCardTariffsRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AttachedCardTariffsRequest)
                {
                    AttachedCardTariffsRequest item = value as AttachedCardTariffsRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.MainCardNumber))
                    {
                        ErrorMessages.Add(_localizer["Հիմնական քարտի համարը մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class AccountReOpenOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is AccountReOpenOrderRequest)
                {
                    AccountReOpenOrderRequest item = value as AccountReOpenOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.ReOpeningAccounts != null)
                    {
                        foreach (var acc in item.Order.ReOpeningAccounts)
                        {
                            if (string.IsNullOrWhiteSpace(acc.AccountNumber))
                            {
                                ErrorMessages.Add(_localizer["Վերաբացվող հաշվի հաշվեհամարը բացակայում է։"]);
                                hasError = true;
                            }
                            else
                            {
                                if (!acc.AccountNumber.All(char.IsNumber))
                                {
                                    ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                    hasError = true;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(acc.Currency))
                            {
                                ErrorMessages.Add(_localizer["Վերաբացվող հաշվի արժույթը բացակայում է։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Վերաբացվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.ReopenReasonDescription))
                    {
                        ErrorMessages.Add(_localizer["Վերաբացման պատճառը բացակայում է։"]);
                        hasError = true;
                    }

                    if (!Enum.IsDefined(typeof(XBS.OrderAccountType), (byte)item.Order.AccountType))
                    {
                        ErrorMessages.Add(_localizer["Հաշվի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.Type != XBS.OrderType.CurrentAccountReOpen)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class MatureOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is MatureOrderRequest)
                {
                    MatureOrderRequest item = value as MatureOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.Account != null)
                    {

                        if (string.IsNullOrWhiteSpace(item.Order.Account.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Հաշվի հաշվեհամարը բացակայում է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.Account.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }


                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.Type != XBS.OrderType.OverdraftRepayment)
                    {
                        if (!Enum.IsDefined(typeof(XBS.MatureType), (short)item.Order.MatureType))
                        {
                            ErrorMessages.Add(_localizer["Մարման տեսակն ընտրված չէ։"]);
                            hasError = true;
                        }
                    }


                    if (item.Order.Type != XBS.OrderType.LoanMature && item.Order.Type != XBS.OrderType.OverdraftRepayment)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.ProductId == 0)
                    {
                        ErrorMessages.Add(_localizer["Վարկի ունիկալ համարը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.Amount == 0)
                    {
                        ErrorMessages.Add(_localizer["Գումար մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.SubType < 1 || item.Order.SubType > 6)
                    {
                        ErrorMessages.Add(_localizer["Ենթատեսակը ընտրված չէ։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (item.Order.SubType == 3)
                        {
                            ErrorMessages.Add(_localizer["Ենթատեսակը սխալ է ընտրված։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class LoanMatureCapitalPenaltRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LoanMatureCapitalPenaltRequest)
                {
                    LoanMatureCapitalPenaltRequest item = value as LoanMatureCapitalPenaltRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.MatureType), (short)item.Order.MatureType))
                    {
                        ErrorMessages.Add(_localizer["Մարման տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.ProductId == 0)
                    {
                        ErrorMessages.Add(_localizer["Վարկի ունիկալ համարը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ApproveLoanProductOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ApproveLoanProductOrderRequest)
                {
                    ApproveLoanProductOrderRequest item = value as ApproveLoanProductOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.ProductType < 1 || item.ProductType > 3)
                    {
                        ErrorMessages.Add(_localizer["Տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Id == 0)
                    {
                        ErrorMessages.Add(_localizer["Ունիկալ համարը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        public class LoanProductInterestRateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LoanProductInterestRateRequest)
                {
                    LoanProductInterestRateRequest item = value as LoanProductInterestRateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.OrderType), (short)item.Order.Type))
                    {
                        ErrorMessages.Add(_localizer["Տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.Type == XBS.OrderType.CreditSecureDeposit)
                    {
                        // SQL-ում նվազագույն արժեք
                        if (item.Order.StartDate < Convert.ToDateTime("01/01/1753"))
                        {
                            ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                            hasError = true;
                        }

                        if (item.Order.EndDate < item.Order.StartDate)
                        {
                            ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                            hasError = true;
                        }
                    }
                    if (item.Order.Type == XBS.OrderType.CreditLineSecureDeposit)
                    {
                        if (string.IsNullOrWhiteSpace(item.CardNumber))
                        {
                            ErrorMessages.Add(_localizer["Քարտի համարը մուտքագրված չէ։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class LoanOrCreditLineContractRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LoanOrCreditLineContractRequest)
                {
                    LoanOrCreditLineContractRequest item = value as LoanOrCreditLineContractRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.LoanNumber))
                    {
                        ErrorMessages.Add(_localizer["Վարկի ունիկալ համարը բացակայում է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.LoanNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Վարկի ունիկալ համար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class LoanStatementRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is LoanStatementRequest)
                {
                    LoanStatementRequest item = value as LoanStatementRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.ProductId <= 0)
                    {
                        ErrorMessages.Add(_localizer["Պրոդուկտի ունիկալ համարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (item.DateTo < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.MaxAmount < item.MinAmount)
                    {
                        ErrorMessages.Add(_localizer["Գումարի առավելագույն չափը փոքր է նվազագույն չափից։"]);
                        hasError = true;
                    }

                    if (item.DateTo < item.DateFrom)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class MessagesRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is MessagesRequest)
                {
                    MessagesRequest item = value as MessagesRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    //SQL
                    if (item.DateFrom < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.DateTo < item.DateFrom)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (!Enum.IsDefined(typeof(XBS.MessageType), item.Type))
                    {
                        ErrorMessages.Add(_localizer["Հաղորդագրությունների տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class NumberOfMessagesRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                  .GetService(typeof(IStringLocalizer<SharedResource>));

                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is NumberOfMessagesRequest)
                {
                    NumberOfMessagesRequest item = value as NumberOfMessagesRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (!Enum.IsDefined(typeof(XBS.MessageType), (short)item.Type))
                    {
                        ErrorMessages.Add(_localizer["Հաղորդագրությունների տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class MessageRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                  .GetService(typeof(IStringLocalizer<SharedResource>));

                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is MessageRequest)
                {
                    MessageRequest item = value as MessageRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Message.Subject))
                    {
                        ErrorMessages.Add(_localizer["Վերնագիրը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Message.Description))
                    {
                        ErrorMessages.Add(_localizer["Հաղորդագրությունը բացակայում է:"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class MessageIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is MessageIdRequest)
                {
                    MessageIdRequest item = value as MessageIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.MessageId == 0)
                    {
                        ErrorMessages.Add(_localizer["Հաղորդագրության համարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class MessageTypeRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is MessageTypeRequest)
                {
                    MessageTypeRequest item = value as MessageTypeRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (!Enum.IsDefined(typeof(XBS.MessageType), (short)item.Type))
                    {
                        ErrorMessages.Add(_localizer["Հաղորդագրությունների տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class DateFromDateToRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is DateFromDateToRequest)
                {
                    DateFromDateToRequest item = value as DateFromDateToRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    // SQL-ում նվազագույն արժեք
                    if (item.DateFrom < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.DateTo < item.DateFrom)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class StartDateEndDateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is StartDateEndDateRequest)
                {
                    StartDateEndDateRequest item = value as StartDateEndDateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    // SQL-ում նվազագույն արժեք
                    if (item.StartDate < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.EndDate < item.StartDate)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class UtilityPaymentOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is UtilityPaymentOrderRequest)
                {
                    UtilityPaymentOrderRequest item = value as UtilityPaymentOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (item.Order.CommunalType == XBS.CommunalTypes.ENA || item.Order.CommunalType == XBS.CommunalTypes.Gas || item.Order.CommunalType == XBS.CommunalTypes.ArmWater)
                    {
                        if (!Enum.IsDefined(typeof(XBS.AbonentTypes), (short)item.Order.AbonentType))
                        {
                            ErrorMessages.Add(_localizer["Բաժանորդի տեսակն ընտրված չէ։"]);
                            hasError = true;
                        }
                        if (string.IsNullOrWhiteSpace(item.Order.Branch))
                        {
                            ErrorMessages.Add(_localizer["Մասնաճյուղն ընտրված չէ։"]);
                            hasError = true;
                        }
                    }


                    if (string.IsNullOrWhiteSpace(item.Order.Code))
                    {
                        ErrorMessages.Add(_localizer["Բաժանորդի կոդը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.CommunalType == 0)
                    {
                        ErrorMessages.Add(_localizer["Կոմունալի տեսակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.Amount == 0 && item.Order.ServiceAmount == 0)
                    {
                        ErrorMessages.Add(_localizer["Գումար մուտքագրված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.Type != XBS.OrderType.CommunalPayment)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.DebitAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DebitAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.DebitAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ViolationIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ViolationIdRequest)
                {
                    ViolationIdRequest item = value as ViolationIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class VehicleViolationByPsnVehRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is VehicleViolationByPsnVehRequest)
                {
                    VehicleViolationByPsnVehRequest item = value as VehicleViolationByPsnVehRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    Regex regPsn = new Regex(@"^[a-zA-Z]{2}[0-9]{6}$");
                    Regex regVehNum1 = new Regex(@"^[0-9]{2}[a-zA-Z]{2}[0-9]{3}$");
                    Regex regVehNum2 = new Regex(@"^[0-9]{3}[a-zA-Z]{2}[0-9]{2}$");

                    if (!regPsn.IsMatch(item.Psn))
                    {
                        ErrorMessages.Add(_localizer["Հաշվառման վկայագրի համարը սխալ է։"]);
                        hasError = true;
                    }

                    if (!regVehNum1.IsMatch(item.VehNum) && !regVehNum2.IsMatch(item.VehNum))
                    {
                        ErrorMessages.Add(_localizer["Հաշվառման համարանիշը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class PeriodicTransferHistoryRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PeriodicTransferHistoryRequest)
                {
                    PeriodicTransferHistoryRequest item = value as PeriodicTransferHistoryRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    // SQL-ում նվազագույն արժեք
                    if (item.DateFrom < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.ProductId == 0)
                    {
                        ErrorMessages.Add(_localizer["Պրոդուկտի ունիկալ համարը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.DateTo < item.DateFrom)
                    {
                        ErrorMessages.Add(_localizer["Վերջի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class PeriodicBudgetPaymentOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is PeriodicBudgetPaymentOrderRequest)
                {
                    PeriodicBudgetPaymentOrderRequest item = value as PeriodicBudgetPaymentOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.Order.BudgetPaymentOrder.ReceiverAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.BudgetPaymentOrder.ReceiverAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.BudgetPaymentOrder.ReceiverAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }

                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.BudgetPaymentOrder.DebitAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.BudgetPaymentOrder.DebitAccount.Currency))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                            hasError = true;
                        }

                        if (string.IsNullOrWhiteSpace(item.Order.BudgetPaymentOrder.DebitAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.BudgetPaymentOrder.DebitAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.BudgetPaymentOrder.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.BudgetPaymentOrder.ReceiverBankCode == 0)
                    {
                        ErrorMessages.Add(_localizer["Բանկի կոդը նշված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Order.PeriodicDescription))
                    {
                        ErrorMessages.Add(_localizer["Պարբերական փոխանցման Փոխանցման նպատակը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.Type != XBS.OrderType.PeriodicTransfer)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.SubType < 1 || item.Order.SubType > 5)
                    {
                        ErrorMessages.Add(_localizer["Ենթատեսակը ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (item.Order.FeeAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.FeeAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Միջնորդավճարի հաշվեհամարը բացակայում է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.FeeAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Միջնորդավճարի հաշվեհամարը  դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Միջնորդավճարի հաշվեհամարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ProductNoteRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ProductNoteRequest)
                {
                    ProductNoteRequest item = value as ProductNoteRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.ProductNote.UniqueId == 0)
                    {
                        ErrorMessages.Add(_localizer["Ունիկալ համարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class UniqueIdRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is UniqueIdRequest)
                {
                    UniqueIdRequest item = value as UniqueIdRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (item.UniqueId == 0)
                    {
                        ErrorMessages.Add(_localizer["Ունիկալ համարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class SwiftCopyOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is SwiftCopyOrderRequest)
                {
                    SwiftCopyOrderRequest item = value as SwiftCopyOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (item.Order.ContractNumber == 0)
                    {
                        ErrorMessages.Add(_localizer["Գործարքի համարը բացակայում է։"]);
                        hasError = true;
                    }

                    if (item.Order.FeeAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.FeeAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Միջնորդավճարի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!item.Order.FeeAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Միջնորդավճարի հաշվեհամարը  դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Միջնորդավճարի հաշիվը բացակայում է։"]);
                        hasError = true;
                    }


                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CredentialOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CredentialOrderRequest)
                {
                    CredentialOrderRequest item = value as CredentialOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if (item.Order.Type != XBS.OrderType.CredentialOrder)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (item.Order.Fees == null)
                    {
                        ErrorMessages.Add(_localizer["Միջնորդավճարի հաշիվը ընտրված չէ:"]);
                        hasError = true;
                    }

                    //if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    //{
                    //    ErrorMessages.Add("Արժույթ դաշտը պարտադիր է։");
                    //    hasError = true;
                    //}

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CashOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CashOrderRequest)
                {
                    CashOrderRequest item = value as CashOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (item.Order.CashDate < Convert.ToDateTime("01/01/1753"))
                    {
                        ErrorMessages.Add(_localizer["Սկզբի ամսաթվի արժեքը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class ChangeTemplateStatusRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is ChangeTemplateStatusRequest)
                {
                    ChangeTemplateStatusRequest item = value as ChangeTemplateStatusRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (item.TemplateId == 0)
                    {
                        ErrorMessages.Add(_localizer["Ձևանմուշի ունիկալ համարը նշված չէ։"]);
                        hasError = true;
                    }

                    if (!Enum.IsDefined(typeof(XBS.TemplateStatus), (short)item.TemplateStatus))
                    {
                        ErrorMessages.Add(_localizer["Ձևանմուշի կարգավիճակն ընտրված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class InternationalOrderTemplateRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is InternationalOrderTemplateRequest)
                {
                    InternationalOrderTemplateRequest item = value as InternationalOrderTemplateRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Template.InternationalPaymentOrder.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }


                    if (item.Template.InternationalPaymentOrder.ReceiverAccount != null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Template.InternationalPaymentOrder.ReceiverAccount.AccountNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                    }
                    else
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.InternationalPaymentOrder.Receiver))
                    {
                        ErrorMessages.Add(_localizer["Ստացողը նշված չէ։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.InternationalPaymentOrder.DetailsOfCharges))
                    {
                        ErrorMessages.Add(_localizer["Փոխանցման եղանակ նշված չէ։"]);
                        hasError = true;
                    }

                    if (item.Template.TemplateType == 0)
                    {
                        ErrorMessages.Add(_localizer["Ձևանմուշի տեսակը նշված չէ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class SaveReestrTransferOrderRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is SaveReestrTransferOrderRequest)
                {
                    SaveReestrTransferOrderRequest item = value as SaveReestrTransferOrderRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    if (string.IsNullOrWhiteSpace(item.Order.Currency))
                    {
                        ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }


                    foreach (var i in item.Order.ReestrTransferAdditionalDetails)
                    {
                        if (string.IsNullOrWhiteSpace(i.NameSurename))
                        {
                            ErrorMessages.Add(_localizer["Անուն Ազգանունը  բացակայում է։"]);
                            hasError = true;
                        }

                        if (i.CreditAccount != null)
                        {
                            if (string.IsNullOrWhiteSpace(i.CreditAccount.AccountNumber))
                            {
                                ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                                hasError = true;
                            }
                            else
                            {
                                if (!i.CreditAccount.AccountNumber.All(char.IsNumber))
                                {
                                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                    hasError = true;
                                }
                            }
                        }
                        else
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                            hasError = true;
                        }

                    }

                    if (item.Order.SubType < 1 || item.Order.SubType > 2)
                    {
                        ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                        hasError = true;
                    }
                    else if (item.Order.SubType == 1 && string.IsNullOrWhiteSpace(item.Order.Description))
                    {
                        ErrorMessages.Add(_localizer["Հաղորդագրության դաշտը պարտադիր է:"]);
                        hasError = true;
                    }

                    if (item.Order.Type != XBS.OrderType.RosterTransfer)
                    {
                        ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }
        public class CheckExcelRowsRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var _localizer = (IStringLocalizer<SharedResource>)validationContext
                    .GetService(typeof(IStringLocalizer<SharedResource>));
                if (value == null)
                {
                    return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is CheckExcelRowsRequest)
                {
                    CheckExcelRowsRequest item = value as CheckExcelRowsRequest;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();

                    foreach (var i in item.ReestrTransferAdditionalDetails)
                    {
                        if (string.IsNullOrWhiteSpace(i.NameSurename))
                        {
                            ErrorMessages.Add(_localizer["Անուն Ազգանունը  բացակայում է։"]);
                            hasError = true;
                        }
                        if (string.IsNullOrWhiteSpace(i.Description))
                        {
                            ErrorMessages.Add(_localizer["Հաղորդագրությունը նշված չէ:"]);
                            hasError = true;
                        }
                        if (i.CreditAccount == null || (i.CreditAccount != null && string.IsNullOrWhiteSpace(i.CreditAccount.AccountNumber)))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (!i.CreditAccount.AccountNumber.All(char.IsNumber))
                            {
                                ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                                hasError = true;
                            }
                        }

                        if (i.Index < 1)
                        {
                            ErrorMessages.Add(_localizer["Հերթական համարը պարտադիր է։"]);
                            hasError = true;
                        }

                    }


                    if (!String.IsNullOrEmpty(item.DebetAccount) && !item.DebetAccount.All(char.IsNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                        hasError = true;
                    }

                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }

        //public class CustomerRegParamsRequestValidation : ValidationAttribute
        //{
        //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        //    {

        //        if (value == null)
        //        {
        //            return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
        //        }

        //        if (value is CustomerRegParamsRequest)
        //        {
        //            CustomerRegParamsRequest item = value as CustomerRegParamsRequest;
        //            bool hasError = false;
        //            List<string> ErrorMessages = new List<string>();

        //            if (!Enum.IsDefined(typeof(TypeOfPhysicalCustomerRegistration), (short)item.RegParams.RegType))
        //            {
        //                ErrorMessages.Add("Գրանցման տեսակն ընտրված չէ։");
        //                hasError = true;
        //            }

        //            if (!Enum.IsDefined(typeof(DocumentType), (short)item.RegParams.DocumentType))
        //            {
        //                ErrorMessages.Add("Փաստաթղթի տեսակն ընտրված չէ։");
        //                hasError = true;
        //            }

        //            if (string.IsNullOrWhiteSpace(item.RegParams.DocumentValue))
        //            {
        //                ErrorMessages.Add("Փաստաթղթի համարը բացակայում է։");
        //                hasError = true;
        //            }

        //            if (!Enum.IsDefined(typeof(RegistrationProductType), (short)item.RegParams.ProductType))
        //            {
        //                ErrorMessages.Add("Պրոդուկտի տեսակն ընտրված չէ։");
        //                hasError = true;
        //            }

        //            if (hasError == true)
        //            {
        //                return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
        //            }
        //            else
        //            {
        //                return ValidationResult.Success;
        //            }
        //        }
        //        else
        //        {
        //            return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
        //        }
        //    }
        //}

        public class AvailableAmountRequestValidation : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {

                if (value == null)
                {
                    return new ValidationResult("Հարցման պարունակությունը դատարկ է։", new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }

                if (value is GetMaxAvailableAmountForNewCreditLine)
                {
                    GetMaxAvailableAmountForNewCreditLine item = value as GetMaxAvailableAmountForNewCreditLine;
                    bool hasError = false;
                    List<string> ErrorMessages = new List<string>();


                    if(item.CreditLineType == 50 || item.CreditLineType == 51 || item.CreditLineType == 30)
                    {
                        hasError = false;
                    }
                    else
                    {
                        ErrorMessages.Add("Վարկային գծի տեսակը սխալ է ընտրված");
                        hasError = true;
                    }

                    if (item.ProductId == 0)
                    {
                        ErrorMessages.Add("Պրոդուկտի ունիկալ համարը սխալ է:");
                        hasError = true;
                    }

                    if(string.IsNullOrWhiteSpace(item.ProvisionCurrency))
                    {
                        ErrorMessages.Add("Սխալ արժույթ։");
                        hasError = true;
                    }




                    if (hasError == true)
                    {
                        return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return new ValidationResult("Առկա է դաշտերի անհամապատասխանություն։", new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
            }
        }


    }

public class GenericIdRequestValidation : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var _localizer = (IStringLocalizer<SharedResource>)validationContext
           .GetService(typeof(IStringLocalizer<SharedResource>));
        if (value == null)
        {
            return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
        }
        if (value is GenericIdRequest)
        {
            GenericIdRequest item = value as GenericIdRequest;
            bool hasError = false;
            List<string> ErrorMessages = new List<string>();

                if (item.Id == 0)
                {
                    ErrorMessages.Add(_localizer["Id ունիկալ համարը լրացված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }


    public class BudgetPaymentOrderTemplateRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

        if (value is BudgetOrderTemplateRequest)
        {
            BudgetOrderTemplateRequest item = value as BudgetOrderTemplateRequest;
            bool hasError = false;
            List<string> ErrorMessages = new List<string>();

                if (item.Template.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString()[0] != '9')
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշվեհամարի առաջին նիշը պետք է լինի 9։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentType != XBS.OrderType.RATransfer)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }
                else
                {

                    if (item.Template.BudgetPaymentOrder.LTACode <= 0)
                    {
                        ErrorMessages.Add(_localizer["ՏՀՏ կոդը ընտրված չէ։"]);
                        hasError = true;
                    }
                }

                if (item.Template.BudgetPaymentOrder.DebitAccount != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Template.BudgetPaymentOrder.DebitAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.BudgetPaymentOrder.DebitAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Template.BudgetPaymentOrder.DebitAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Template.BudgetPaymentOrder.ReceiverAccount != null)
                {
                    if (!(item.Template.TemplateDocumentType == XBS.OrderType.RATransfer && (item.Template.TemplateDocumentSubType == 2 || item.Template.TemplateDocumentSubType == 5))
                        && string.IsNullOrWhiteSpace(item.Template.BudgetPaymentOrder.ReceiverAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.BudgetPaymentOrder.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Template.BudgetPaymentOrder.ReceiverAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentType != XBS.OrderType.RATransfer)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.ExchangeDirection), (int)item.Template.TemplateType))
                {
                    ErrorMessages.Add(_localizer["Ձևանմուշի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentSubType < 1 || item.Template.TemplateDocumentSubType > 6)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class CreditLineOrderTypeRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
            if (value is CreditLineOrderTypeRequest)
            {
                CreditLineOrderTypeRequest item = value as CreditLineOrderTypeRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.OrderType != XBS.OrderType.FastOverdraftApplication)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակի պետք է լինի 159։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class CurrencyExchangeOrderTemplateRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

        if (value is CurrencyExchangeOrderTemplateRequest)
        {
            CurrencyExchangeOrderTemplateRequest item = value as CurrencyExchangeOrderTemplateRequest;
            bool hasError = false;
            List<string> ErrorMessages = new List<string>();

                if (item.Template.CurrencyExchangeOrder.DebitAccount != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Template.CurrencyExchangeOrder.DebitAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.CurrencyExchangeOrder.DebitAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Ելքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Template.CurrencyExchangeOrder.DebitAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Ելքագրվող հաշվում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Ելքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Template.CurrencyExchangeOrder.ReceiverAccount != null)
                {
                    if (!(item.Template.TemplateDocumentType == XBS.OrderType.RATransfer && (item.Template.TemplateDocumentSubType == 2 || item.Template.CurrencyExchangeOrder.SubType == 5))
                        && string.IsNullOrWhiteSpace(item.Template.CurrencyExchangeOrder.ReceiverAccount.Currency))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի արժույթ դաշտը պարտադիր է։"]);
                        hasError = true;
                    }

                    if (string.IsNullOrWhiteSpace(item.Template.CurrencyExchangeOrder.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամարը պարտադիր է։"]);
                        hasError = true;
                    }
                    else
                    {
                        if (!item.Template.CurrencyExchangeOrder.ReceiverAccount.AccountNumber.All(char.IsNumber))
                        {
                            ErrorMessages.Add(_localizer["Մուտքագրվող հաշվի հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                            hasError = true;
                        }
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Մուտքագրվող հաշիվը բացակայում է։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentType != XBS.OrderType.Convertation)
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }

                if (!Enum.IsDefined(typeof(XBS.ExchangeDirection), (int)item.Template.TemplateType))
                {
                    ErrorMessages.Add(_localizer["Ձևանմուշի տեսակն ընտրված չէ։"]);
                    hasError = true;
                }

                if (item.Template.TemplateDocumentSubType < 1 || item.Template.TemplateDocumentSubType > 3)
                {
                    ErrorMessages.Add(_localizer["Հայտի ենթատեսակը սխալ է։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Template.CurrencyExchangeOrder.Currency))
                {
                    ErrorMessages.Add(_localizer["Արժույթ դաշտը պարտադիր է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }


    public class PasswordResetRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
            if (value is PasswordResetRequest)
            {
                PasswordResetRequest item = value as PasswordResetRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (String.IsNullOrEmpty(item.UserName))
                {
                    ErrorMessages.Add(_localizer["Մուտքանունը լրացված չէ։"]);
                    hasError = true;
                }

                if (String.IsNullOrEmpty(item.OTP))
                {
                    ErrorMessages.Add(_localizer["Մեկանգամյա թվայինն կոդը լրացված չէ։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }


    public class OrderRejectionRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
            if (value is OrderRejectionRequest)
            {
                OrderRejectionRequest item = value as OrderRejectionRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (String.IsNullOrEmpty(item.OrderRejection.RejectReason))
                {
                    ErrorMessages.Add(_localizer["Մերժման պատճառը մուտքագրված չէ։"]);
                    hasError = true;
                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class UtilityperiodicPaymentRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
            if (value is PeriodicUtilityPaymentOrderRequest)
            {
                PeriodicUtilityPaymentOrderRequest item = value as PeriodicUtilityPaymentOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.Order.Periodicity != 30)
                {
                    ErrorMessages.Add(_localizer["Կոմունալ վճարումների պարբերականությունը պետք է լինի 1 ամիս։"]);
                    hasError = true;
                }



                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class SearchCommunalRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
            if (value is SearchCommunalRequest)
            {
                SearchCommunalRequest item = value as SearchCommunalRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (item.SearchCommunal.CommunalType == XBS.CommunalTypes.Gas)
                {
                    if (!string.IsNullOrWhiteSpace(item.SearchCommunal.AbonentNumber) && item.SearchCommunal.AbonentNumber.Length != 6)
                    {
                        ErrorMessages.Add(_localizer["Բաժանորդի համարի երկարությունը պետք է լինի 6 նիշ։"]);
                        hasError = true;
                    }
                }
                else if (item.SearchCommunal.CommunalType == XBS.CommunalTypes.ENA)
                {
                    if (!string.IsNullOrWhiteSpace(item.SearchCommunal.AbonentNumber) && item.SearchCommunal.AbonentNumber.Length != 7)
                    {
                        ErrorMessages.Add(_localizer["Բաժանորդի համարի երկարությունը պետք է լինի 7 նիշ։"]);
                        hasError = true;
                    }
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class OrderTypeValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

        if (value is OrderTypeRequest)
        {
            OrderTypeRequest item = value as OrderTypeRequest;
            bool hasError = false;
            List<string> ErrorMessages = new List<string>();


                if (!Enum.IsDefined(typeof(XBS.OrderType), (short)item.OrderType))
                {
                    ErrorMessages.Add(_localizer["Հայտի տեսակը սխալ է։"]);
                    hasError = true;
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class ReferenceOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

        if (value is ReferenceOrderRequest)
        {
            ReferenceOrderRequest item = value as ReferenceOrderRequest;
            bool hasError = false;
            List<string> ErrorMessages = new List<string>();

                //եթե ընտրվել է "Այլ"
                if (item.Order.ReferenceType == 8)
                {
                    if (String.IsNullOrWhiteSpace(item.Order.OtherTypeDescription))
                    {
                        ErrorMessages.Add(_localizer["Տեղեկանքի տեսակը լրացված չէ։"]);
                        hasError = true;
                    }
                }

                //եթե ընտրվել է "Հաշիվների շարժի վերաբերյալ"
                if (item.Order.ReferenceType == 3)
                {
                    if (item.Order.DateFrom == null )
                    {
                        ErrorMessages.Add(_localizer["Սկիզբ դաշտը լրացված չէ։"]);
                        hasError = true;
                    }
                    else if (item.Order.DateTo == null)
                    {
                        ErrorMessages.Add(_localizer["Ավարտ դաշտը լրացված չէ։"]);
                        hasError = true;
                    }
                    else if (item.Order.DateFrom > item.Order.DateTo)
                    {
                        ErrorMessages.Add(_localizer["Ավարտի ամսաթիվը պետք է մեծ լինի սկիզբի ամսաթվից։"]);
                        hasError = true;
                    }
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class SearchBudgetAccountRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

        if (value is SearchBudgetAccountRequest)
        {
            SearchBudgetAccountRequest item = value as SearchBudgetAccountRequest;
            bool hasError = false;
            List<string> ErrorMessages = new List<string>();

                if (string.IsNullOrWhiteSpace(item.SearchAccount.AccountNumber))
                {
                    ErrorMessages.Add(_localizer["Հաշվեհամարը բացակայում է։"]);
                    hasError = true;
                }
                else
                {
                    if (!item.SearchAccount.AccountNumber.All(char.IsNumber))
                    {
                        ErrorMessages.Add(_localizer["Հաշվեհամար դաշտում անհրաժեշտ է մուտքագրել միայն թվանշաններ։"]);
                        hasError = true;
                    }
                    else if (item.SearchAccount.AccountNumber.Length != 12)
                    {
                        ErrorMessages.Add(_localizer["Հաշվեհամարը պետք է լինի 12 նիշ։"]);
                        hasError = true;
                    }
                    else if (int.Parse(item.SearchAccount.AccountNumber.Substring(0, 5)) < 90000 || int.Parse(item.SearchAccount.AccountNumber.Substring(0, 5)) > 90048)
                    {
                        ErrorMessages.Add(_localizer["Բյուջետային հաշվեհամարը սխալ է։"]);
                        hasError = true;
                    }

                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class InternationalPaymentOrderRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is InternationalPaymentOrderRequest)
            {
                InternationalPaymentOrderRequest item = value as InternationalPaymentOrderRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();

                if (string.IsNullOrWhiteSpace(item.Order.Currency))
                {
                    ErrorMessages.Add(_localizer["Արժույթը մուտքագրված չէ։"]);
                    hasError = true;
                }
                else
                {
                    if (item.Order.Currency != "RUR")
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPayment))
                        {
                            ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                            hasError = true;
                        }
                        if (string.IsNullOrWhiteSpace(item.Order.ReceiverBankSwift) && string.IsNullOrWhiteSpace(item.Order.FedwireRoutingCode))
                        {
                            ErrorMessages.Add(_localizer["Անհարժեշտ է լրացնել Ստացողի բանկի SWIFT կոդ կամ Fedwire Routing կոդ։"]);
                            hasError = true;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPaymentRUR1))
                        {
                            ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                            hasError = true;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPaymentRUR2))
                            {
                                ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                                hasError = true;
                            }
                            if (item.Order.DescriptionForPaymentRUR1 != "Материальная помощь" && item.Order.DescriptionForPaymentRUR1 != "Другое")
                            {
                                if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPaymentRUR3))
                                {
                                    ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                                    hasError = true;
                                }
                                if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPaymentRUR4))
                                {
                                    ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                                    hasError = true;
                                }
                                if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPaymentRUR5))
                                {
                                    ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                                    hasError = true;
                                }
                                else
                                {
                                    if (item.Order.DescriptionForPaymentRUR5 == "с НДС")
                                    {
                                        if (string.IsNullOrWhiteSpace(item.Order.DescriptionForPaymentRUR6))
                                        {
                                            ErrorMessages.Add(_localizer["Վճարման մանրամասները մուտքագրված չէ։"]);
                                            hasError = true;
                                        }
                                    }
                                }

                            }
                        }

                        if (string.IsNullOrWhiteSpace(item.Order.Receiver))
                        {
                            ErrorMessages.Add(_localizer["Ստացողի անուն ազգանունը մուտքագրված չէ։"]);
                            hasError = true;
                        }
                        if (string.IsNullOrWhiteSpace(item.Order.CorrAccount))
                        {
                            ErrorMessages.Add(_localizer["Թղթակցային հաշիվը մուտքագրված չէ։"]);
                            hasError = true;
                        }
                        if (string.IsNullOrWhiteSpace(item.Order.BIK))
                        {
                            ErrorMessages.Add(_localizer["Ստացող բանկի ԲԻԿ կոդը մուտքագրված չէ։"]);
                            hasError = true;
                        }
                        if (string.IsNullOrWhiteSpace(item.Order.BIK))
                        {
                            ErrorMessages.Add(_localizer["Ստացող բանկի ԲԻԿ կոդը մուտքագրված չէ։"]);
                            hasError = true;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(item.Order.SenderAddress))
                {
                    ErrorMessages.Add(_localizer["Գրանցման հասցեն մուտքագրված չէ։"]);
                    hasError = true;
                }

                if (item.Order.ReceiverAccount != null)
                {
                    if (string.IsNullOrWhiteSpace(item.Order.ReceiverAccount.AccountNumber))
                    {
                        ErrorMessages.Add(_localizer["Ստացողի հաշվեհամարը մուտքագրված չէ։"]);
                        hasError = true;
                    }
                }
                else
                {
                    ErrorMessages.Add(_localizer["Ստացողի հաշվեհամարը մուտքագրված չէ։"]);
                    hasError = true;
                }


                if (string.IsNullOrWhiteSpace(item.Order.Receiver))
                {
                    ErrorMessages.Add(_localizer["Ստացողի անվանումը մուտքագրված չէ։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Order.DetailsOfCharges))
                {
                    ErrorMessages.Add(_localizer["Փոխանցման եղանակը մուտքագրված չէ։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Order.Sender))
                {
                    ErrorMessages.Add(_localizer["Անուն Ազգանունը մուտքագրված չէ։"]);
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(item.Order.ReceiverBank))
                {
                    ErrorMessages.Add(_localizer["Ստացող բանկի տվյալները մուտքագրված չէ։"]);
                    hasError = true;
                }


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }
    public class InternationalPaymentOrderFeeRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
               .GetService(typeof(IStringLocalizer<SharedResource>));

            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is InternationalPaymentOrderFeeRequest)
            {
                InternationalPaymentOrderFeeRequest item = value as InternationalPaymentOrderFeeRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();


                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class ProductIdApproveRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
           .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is ProductIdApproveRequest)
            {
                ProductIdRequest item = value as ProductIdRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();



                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    public class ChangeUserPasswordRequestValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizer = (IStringLocalizer<SharedResource>)validationContext
           .GetService(typeof(IStringLocalizer<SharedResource>));
            if (value == null)
            {
                return new ValidationResult(_localizer["Հարցման պարունակությունը դատարկ է։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }

            if (value is ChangeUserPasswordRequest)
            {
                ChangeUserPasswordRequest item = value as ChangeUserPasswordRequest;
                bool hasError = false;
                List<string> ErrorMessages = new List<string>();


                if (string.IsNullOrWhiteSpace(item.NewPassword))
                {
                    ErrorMessages.Add(_localizer["Նոր գաղտնաբառը մուտքագրված չէ։"]);
                    hasError = true;
                }
                else
                {
                    if(item.NewPassword != item.RetypeNewPassword)
                    {
                        ErrorMessages.Add(_localizer["Նոր գաղտնաբառի երկու տարբերակները չեն համընկնում։"]);
                        hasError = true;
                    }
                    else
                    {
                        if(item.NewPassword == item.Password)
                        {
                            ErrorMessages.Add(_localizer["Նոր և հին գաղտնաբառերը չեն կարող համընկնել։"]);
                            hasError = true;
                        }
                        else
                        {
                            Regex reForPassword = new Regex(@"^[0-9a-zA-Z'!#$%&'*+/=?^_`{|}~.-]*$");

                            if(!reForPassword.IsMatch(item.NewPassword))
                            {
                                ErrorMessages.Add(_localizer["Գաղտնաբառը պետք է պարունակի լատինատառ, թվային նիշ, հատուկ նշաններ։"]);
                                hasError = true;
                            }

                            if(item.NewPassword.Length < 6)
                            {
                                ErrorMessages.Add(_localizer["Գաղտնաբառը պետք է պարունակի առնվազն 6 նիշ։"]);
                                hasError = true;
                            }
                        }
                    }
                }

                if (hasError == true)
                {
                    return new ValidationResult(ValidationError.GetFormattedErrorMessage(ErrorMessages), new List<string>() { ValidationMessage.LogicValidationTypeDescription });
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
            else
            {
                return new ValidationResult(_localizer["Առկա է դաշտերի անհամապատասխանություն։"], new List<string>() { ValidationMessage.LogicValidationTypeDescription });
            }
        }
    }

    
}
