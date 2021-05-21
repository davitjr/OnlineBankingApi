using ArcaCardAttachmentService;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Controllers
{
    /// <summary>
    /// AttachedCardController designed to handle Other Bank Cards attach, deattach and other processes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class AttachedCardController : ControllerBase
    {
        private readonly CacheHelper _cacheHelper;
        private readonly ArcaCard.ArcaCardClient _client;
        private readonly XBService _xBService;
        public AttachedCardController(CacheHelper cacheHelper, ArcaCard.ArcaCardClient client, XBService xBService)
        {
            _xBService = xBService;
            _cacheHelper = cacheHelper;
            _client = client;
        }
        /// <summary>
        ///Այլ բանկերի քարտերի կցում
        /// </summary>
        /// <returns></returns>
        [HttpPost("AttachCardValidation")]
        public IActionResult AttachCardValidation()
        {
            SingleResponse<string> httpResponse = new SingleResponse<string>
            {
                ResultCode = ResultCodes.normal
            };
            if (!ValidateCards(out string message))
            {
                httpResponse.ResultCode = ResultCodes.validationError;
                httpResponse.Description = message;
            }
            return ResponseExtensions.ToHttpResponse(httpResponse);
        }
        /// <summary>
        ///Այլ բանկերի քարտերի կցում
        /// </summary>
        /// <returns></returns>
        [HttpPost("AttachCard")]
        public async Task<IActionResult> AttachCard([FromBody] AttachCardRequest request)
        {
            SingleResponse<CardAttachmentResponse> httpResponse = new SingleResponse<CardAttachmentResponse>();
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            var language = _cacheHelper.GetLanguage();
            var source = _cacheHelper.GetSourceType();
            XBS.ActionResult actionResult = _xBService.ValidateAttachCard(request.CardNumber.ToString(), authorizedCustomer.CustomerNumber, request.CardHolderName);
            if (actionResult.Errors.Count == 0)
            {
                var response = await _client.AtachCardOrderAsync(new CardAttachmentRequest
                {
                    CustomerNumber = authorizedCustomer.CustomerNumber,
                    UserId = authorizedCustomer.UserId,
                    CardNumber = request.CardNumber,
                    CardHolderName = request.CardHolderName,
                    ExpireMonth = request.ExpireMonth,
                    ExpireYear = request.ExpireYear,
                    Cvc = request.Cvv,
                    Language = language == 1 ? "hy" : "en",
                    PageView = source == SourceType.AcbaOnline ? "DESKTOP" : "MOBILE"
                });
                if (response != null)
                {
                    httpResponse.ResultCode = ResultCodes.normal;
                    httpResponse.Result = response;
                }
                else
                {
                    httpResponse.ResultCode = ResultCodes.failed;
                    httpResponse.Description = "Քարտի կցման ժամանակ տեղի ունեցավ սխալ։";
                }
            }
            else
            {
                httpResponse.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(actionResult.ResultCode);
                httpResponse.Description = _xBService.GetTerm(actionResult?.Errors[0]?.Code ?? 0, null, (Languages)language);
            }
            return ResponseExtensions.ToHttpResponse(httpResponse);
        }

        /// <summary>
        ///Այլ բանկերի քարտերի կցում (Պատասխանի ստացում)
        /// </summary>
        /// <returns></returns>
        [HttpPost("AttachCardCompletion")]
        public async Task<IActionResult> AttachCardCompletion([FromBody] AttachCardCompletionRequest request)
        {
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            var response = await _client.AtachCardOrderCompletionAsync(new CardAttachmentCompletionRequest
            {
                UserId = authorizedCustomer.UserId,
                MdOrder = request.MdOrder
            });
            if (response.Attached)
            {
                return ResponseExtensions.ToHttpResponse(new Response
                {
                    ResultCode = ResultCodes.normal,
                    Description = "Ձեր քարտը կցվել է:"
                });
            }
            else
            {
                return ResponseExtensions.ToHttpResponse(new Response
                {
                    ResultCode = ResultCodes.failed,
                    Description = "Քարտը հնարավոր չէ կցել: Անհրաժեշտ է դիմել քարտը թողարկող բանկ:"
                });
            }
        }

        /// <summary>
        /// Կցված քարտի հեռացում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("DeleteAttachedCard")]
        public async Task<IActionResult> DeleteAttachedCardById([FromBody] AttachCardDeleteRequest request)
        {
            var response = await _client.DeleteAttachedCardByIdAsync(new DeleteAttachedCardRequest
            {
                Id = request.Id
            });
            if (response.Deleted)
            {
                return ResponseExtensions.ToHttpResponse(new Response
                {
                    ResultCode = ResultCodes.normal,
                    Description = "Քարտը հեռացված է:"
                });
            }
            else
            {
                return ResponseExtensions.ToHttpResponse(new Response
                {
                    ResultCode = ResultCodes.failed,
                    Description = "Քարտի հեռացման ժամանակ տեղի ունեցավ սխալ:"
                });
            }
        }

        /// <summary>
        /// Payment With Binding
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("BindingPaymentWithCard")]
        [TypeFilter(typeof(IsAbleToSaveFilter))]
        public async Task<IActionResult> SaveAndApproveWithAttachedCard([FromBody] AttachCardBindingRequest request)
        {
            if (ModelState.IsValid)
            {
                if (request?.Order == null)
                {
                    return ResponseExtensions.ToHttpResponse(new Response
                    {
                        ResultCode = ResultCodes.failed,
                        Description = "Տեղի ունեցավ սխալ:"
                    });
                }
                var orderResult = SaveAttachedCardOrder(request);
                if (orderResult.ResultCode == ResultCodes.normal && orderResult.Result != 0)
                {
                    var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                    var language = _cacheHelper.GetLanguage();
                    var source = _cacheHelper.GetSourceType();
                    var response = await _client.ArcaOrderBindingPaymentAsync(new CardBindingPaymentRequest
                    {
                        BindingId = request.Order.BindingId,
                        Amount = request.Order.Amount,
                        Currency = GetOrderTypeBasedCurrency(request),
                        OrderType = XbsOrderEnumToProto(request.Order.Type),
                        OrderSubType = request.Order.SubType,
                        CustomerNumber = authorizedCustomer.CustomerNumber,
                        UserId = authorizedCustomer.UserId,
                        Language = language == 1 ? "hy" : "en",
                        PageView = source == SourceType.AcbaOnline ? "DESKTOP" : "MOBILE"
                    });
                    if (response.Payed)
                    {
                        var saveResponse = await _client.SaveAttachedCardOrderDetailsAsync(new OrderDetailsRequest
                        {
                            DocID = orderResult.Result,
                            CardId = response.CardId,
                            MdOrder = response.MdOrder
                        });
                        if (saveResponse.Saved)
                        {
                            var approvalRequest = new AttachCardPaymentApprovalRequest
                            {
                                Id = orderResult.Result,
                                SubType = request.Order.SubType,
                                Type = request.Order.Type
                            };
                            var approveResult = ApproveAttachedCardOrder(approvalRequest);
                            if (approveResult.ResultCode == ResultCodes.normal)
                            {
                                return ResponseExtensions.ToHttpResponse(new Response
                                {
                                    ResultCode = ResultCodes.normal,
                                    Description = "Վճարումը կատարված է:"
                                });
                            }
                            else
                            {
                                return ResponseExtensions.ToHttpResponse(new Response
                                {
                                    ResultCode = ResultCodes.failed,
                                    Description = "Վճարման հաստատման ժամանակ տեղի ունեցավ սխալ, խնդրում ենք դիմել բանկ:"
                                });
                            }
                        }
                        else
                        {
                            return ResponseExtensions.ToHttpResponse(new Response
                            {
                                ResultCode = ResultCodes.failed,
                                Description = "Հայտի մանրամասների պահպանման տեղի ունեցավ սխալ:"
                            });
                        }
                    }
                    else
                    {
                        // Quality desclined
                        return ResponseExtensions.ToHttpResponse(new Response
                        {
                            ResultCode = ResultCodes.failed,
                            Description = "Վճարման ժամանակ տեղի ունեցավ սխալ:"
                        });
                    }
                }
                else
                {
                    // Quality desclined
                    return ResponseExtensions.ToHttpResponse(new Response
                    {
                        ResultCode = ResultCodes.failed,
                        Description = "Տեղի ունեցավ սխալ:"
                    });
                }
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Կցված քարտերի GET
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAttachedCards")]
        public async Task<IActionResult> GetAttachedCards()
        {
            SingleResponse<List<AttachedCard>> httpResponse = new SingleResponse<List<AttachedCard>>()
            {
                ResultCode = ResultCodes.normal,
                Result = new List<AttachedCard>()
            };
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();

            var response = await _client.GetAttachedCardsAsync(new AttachedCardRequest
            {
                UserId = authorizedCustomer.UserId
            });
            foreach (var card in response.AttachedCards)
            {
                httpResponse.Result.Add(new AttachedCard
                {
                    Id = card.Id,
                    BankName = card.BankName,
                    CardHolderName = card.CardHolderName,
                    CardNumber = card.CardNumber,
                    ExpireDateString = card.ExpireDateString
                });
            }
            return Ok(httpResponse);
        }

        private bool ValidateCards(out string message)
        {
            var authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
            var response = _client.ValidateAttachedCards(new AttachedCardValidationRequest
            {
                UserId = authorizedCustomer.UserId
            });
            message = response.Message;
            return response.IsValid;
        }
        private CardBindingPaymentRequest.Types.OrderTypeEnum XbsOrderEnumToProto(OrderType orderType)
        {
            CardBindingPaymentRequest.Types.OrderTypeEnum orderTypeEnum = CardBindingPaymentRequest.Types.OrderTypeEnum.NotDefined;
            switch (orderType)
            {
                case OrderType.RATransfer:
                    {
                        orderTypeEnum = CardBindingPaymentRequest.Types.OrderTypeEnum.RaTransfer;
                    }
                    break;
                case OrderType.LoanMature:
                    {
                        orderTypeEnum = CardBindingPaymentRequest.Types.OrderTypeEnum.LoanMature;
                    }
                    break;
                case OrderType.CommunalPayment:
                    {
                        orderTypeEnum = CardBindingPaymentRequest.Types.OrderTypeEnum.CommunalPayment;
                    }
                    break;
                default:
                    break;
            }
            return orderTypeEnum;
        }
        private string GetOrderTypeBasedCurrency(AttachCardBindingRequest request)
        {
            string CurrencyCode = string.Empty;
            switch (request.Order.Type)
            {
                case OrderType.RATransfer:
                    {
                        if (request.Order.SubType == 3)
                        {
                            CurrencyCode = request.Order.ReceiverAccount.Currency;
                        }
                        //else if (request.Order.SubType == 6)
                        //{
                        //    CurrencyCode = "AMD";
                        //}
                    }
                    break;
                case OrderType.LoanMature:
                    {
                        CurrencyCode = request.Order.Currency;
                    }
                    break;
                case OrderType.CommunalPayment:
                    {
                        CurrencyCode = "AMD";
                    }
                    break;
                default:
                    break;
            }
            return CurrencyCode;
        }
        private SingleResponse<long> SaveAttachedCardOrder(AttachCardBindingRequest request)
        {
            SingleResponse<long> response = new SingleResponse<long>()
            {
                ResultCode = ResultCodes.normal
            };
            XBS.ActionResult result = new XBS.ActionResult();
            //Տարանցիկ հաշվի ստացում դեբետագրելու համար , կախված հայտի տեսակից 
            string accountNumber = _client.GetMerchantAccountNumberByOrderType(new AttachedCardOrderTypeRequest
            {
                Currency = GetOrderTypeBasedCurrency(request),
                OrderType = (int)request.Order.Type,
                OrderSubType = request.Order.SubType
            })?.AccountNumber;
            if (string.IsNullOrEmpty(accountNumber))
            {
                response.ResultCode = ResultCodes.failed;
                return response;
            }
            switch (request.Order.Type)
            {
                case OrderType.RATransfer:
                    {
                        if (request.Order.SubType == 3)
                        {
                            PaymentOrder order = new PaymentOrder
                            {
                                Amount = request.Order.Amount,
                                Currency = request.Order.Currency,
                                SubType = request.Order.SubType,
                                Type = request.Order.Type,
                                ReceiverBankCode = request.Order.ReceiverBankCode,
                                ReceiverAccount = new Account
                                {
                                    AccountNumber = request.Order.ReceiverAccount.AccountNumber,
                                    Currency = request.Order.ReceiverAccount.Currency
                                },
                                DebitAccount = new Account
                                {
                                    AccountNumber = accountNumber,
                                    Currency = request.Order.DebitAccount.Currency,
                                    IsAttachedCard = true
                                }
                            };
                            result = _xBService.SavePaymentOrder(order);
                        }
                        //else if (request.Order.SubType == 6)
                        //{
                        //    BudgetPaymentOrder order = new BudgetPaymentOrder
                        //    {
                        //        Amount = request.Order.Amount,
                        //        Currency = request.Order.Currency,
                        //        SubType = request.Order.SubType,
                        //        Type = request.Order.Type,
                        //        ReceiverBankCode = request.Order.ReceiverBankCode,
                        //        UseCreditLine = request.Order.UseCreditLine,
                        //        Description = request.Order.Description,
                        //        TransferFee = request.Order.TransferFee,
                        //        Receiver = request.Order.Receiver,
                        //        PoliceResponseDetailsId = request.Order.PoliceResponseDetailsId,
                        //        LTACode = request.Order.LTACode,
                        //        FeeAccount = new Account
                        //        {
                        //            AccountNumber = request.Order.FeeAccount.AccountNumber,
                        //            Currency = request.Order.FeeAccount.Currency
                        //        },
                        //        ReceiverAccount = new Account
                        //        {
                        //            AccountNumber = request.Order.ReceiverAccount.AccountNumber
                        //        },
                        //        DebitAccount = new Account
                        //        {
                        //            AccountNumber = accountNumber,
                        //            Currency = request.Order.DebitAccount.Currency,
                        //            IsAttachedCard = true
                        //        }
                        //    };
                        //    result = _xBService.SaveBudgetPaymentOrder(order);
                        //}
                    }
                    break;
                case OrderType.LoanMature:
                    {
                        string loanAccNumber = _xBService.GetLiabilitiesAccountNumberByAppId(request.Order.ProductId);
                        if (string.IsNullOrEmpty(loanAccNumber))
                        {
                            response.ResultCode = ResultCodes.failed;
                            return response;
                        }
                        PaymentOrder order = new PaymentOrder
                        {
                            Amount = request.Order.Amount,
                            Currency = request.Order.Currency,
                            SubType = 3,
                            Type = OrderType.RATransfer,
                            ReceiverBankCode = request.Order.ReceiverBankCode,
                            ReceiverAccount = new Account
                            {
                                AccountNumber = loanAccNumber,
                                Currency = request.Order.Currency
                            },
                            DebitAccount = new Account
                            {
                                AccountNumber = accountNumber,
                                Currency = request.Order.Currency,
                                IsAttachedCard = true
                            }
                        };
                        result = _xBService.SavePaymentOrder(order);
                        if (result.ResultCode == ResultCode.Normal)
                        {
                            PaymentOrder paymOrder = _xBService.GetPaymentOrder(result.Id);
                            result = _xBService.ApprovePaymentOrder(paymOrder);
                            if (result.ResultCode == ResultCode.Normal)
                            {
                                MatureOrder matureOrder = new MatureOrder
                                {
                                    ProductId = request.Order.ProductId,
                                    Amount = request.Order.Amount,
                                    Currency = request.Order.Currency,
                                    SubType = request.Order.SubType,
                                    Type = request.Order.Type,
                                    MatureType = request.Order.MatureType,
                                    MatureMode = request.Order.MatureMode,
                                    Account = new Account
                                    {
                                        AccountNumber = loanAccNumber,
                                        Currency = request.Order.Currency,
                                        IsAttachedCard = true
                                    }
                                };
                                result = _xBService.SaveMatureOrder(matureOrder);
                            }
                        }
                    }
                    break;
                case OrderType.CommunalPayment:
                    {
                        UtilityPaymentOrder order = new UtilityPaymentOrder
                        {
                            Amount = request.Order.Amount,
                            Currency = request.Order.Currency,
                            Type = request.Order.Type,
                            Code = request.Order.Code,
                            AbonentType = request.Order.AbonentType,
                            CommunalType = request.Order.CommunalType,
                            Branch = request.Order.Branch,
                            AbonentFilialCode = request.Order.AbonentFilialCode,
                            PaymentType = request.Order.PaymentType,
                            DebitAccount = new Account
                            {
                                AccountNumber = accountNumber,
                                Currency = "AMD",
                                IsAttachedCard = true
                            }
                        };
                        result = _xBService.SaveUtilityPaymentOrder(order);
                    }
                    break;
                default:
                    break;
            }
            response.Result = result.Id;
            response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
            response.Description = Utils.GetActionResultErrors(result.Errors);
            return response;
        }
        private SingleResponse<long> ApproveAttachedCardOrder(AttachCardPaymentApprovalRequest request)
        {
            SingleResponse<long> response = new SingleResponse<long>()
            {
                ResultCode = ResultCodes.normal
            };
            XBS.ActionResult result = new XBS.ActionResult();
            switch (request.Type)
            {
                case OrderType.RATransfer:
                    {
                        if (request.SubType == 3)
                        {
                            PaymentOrder order = _xBService.GetPaymentOrder(request.Id);
                            result = _xBService.ApprovePaymentOrder(order);
                        }
                        //else if (request.SubType == 6)
                        //{
                        //    BudgetPaymentOrder order = _xBService.GetBudgetPaymentOrder(request.Id);
                        //    result = _xBService.ApprovePaymentOrder(order);
                        //}
                    }
                    break;
                case OrderType.LoanMature:
                    {
                        MatureOrder order = _xBService.GetMatureOrder(request.Id);
                        result = _xBService.ApproveMatureOrder(order);
                    }
                    break;
                case OrderType.CommunalPayment:
                    {
                        UtilityPaymentOrder order = _xBService.GetUtilityPaymentOrder(request.Id);
                        result = _xBService.ApproveUtilityPaymentOrder(order);
                    }
                    break;
                default:
                    break;
            }
            response.Result = result.Id;
            response.ResultCode = ResultCodeFormatter.FromPersonalAccountSecurityService(result.ResultCode);
            response.Description = Utils.GetActionResultErrors(result.Errors);
            return response;
        }
    }
}
