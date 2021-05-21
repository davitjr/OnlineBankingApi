using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using XBS;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class MessageController : ControllerBase
    {
        private readonly XBService _xbService;

        public MessageController(XBService xbService)
        {
            _xbService = xbService;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հաղորդագրությունների ցանկը՝ կախված տեսակից (Ուղարկված/Ստացված)
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost("GetMessages")]
        public IActionResult GetMessages(MessagesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Message>> response = new SingleResponse<List<Message>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetMessages(request.DateFrom, request.DateTo, request.Type);
                if(request.Type == MessageType.Message)
                {
                    response.Result.AddRange(_xbService.GetMessages(request.DateFrom, request.DateTo, MessageType.Reminder));
                    response.Result.AddRange(_xbService.GetMessages(request.DateFrom, request.DateTo, MessageType.TransactionRefusal));
                }


                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                for (int i = 0; i < response.Result.Count; i++)
                {
                    response.Result[i].Description = regex.Replace(response.Result[i].Description, " ");
                }

                response.Result = response.Result.OrderByDescending(x => x.SentDate).ToList();
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ նշված քանակի, նշված տեսակի հաղորդագրությունների ցանկը
        /// </summary>
        /// <param name="messageCount"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost("GetNumberOfMessages")]
        public IActionResult GetNumberOfMessages(NumberOfMessagesRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Message>> response = new SingleResponse<List<Message>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetNumberOfMessages(request.MessageCount, request.Type);

                if (request.Type == MessageType.Message)
                {
                    response.Result.AddRange(_xbService.GetNumberOfMessages(request.MessageCount, MessageType.Reminder));
                    response.Result.AddRange(_xbService.GetNumberOfMessages(request.MessageCount, MessageType.TransactionRefusal));
                }

                response.Result = response.Result.OrderByDescending(x => x.SentDate).Take(request.MessageCount).ToList();
                return ResponseExtensions.ToHttpResponse(response);

            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Ուղարկում է տվյալ հաղորդագրությունը
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost("AddMessage")]
        public IActionResult AddMessage(MessageRequest request)
        {
            if (ModelState.IsValid)
            {
                Response response = new Response();
                _xbService.AddMessage(request.Message);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հեռացնում է տվյալ հաղորդագրությունը
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost("DeleteMessage")]
        public IActionResult DeleteMessage(MessageIdRequest request)
        {
            if (ModelState.IsValid)
            {
                Response response = new Response();
                _xbService.DeleteMessage(request.MessageId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Նշում է տվյալ հաղորդագրությունը՝ որպես ընթերցված
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost("MarkMessageAsRead")]
        public IActionResult MarkMessageAsRead(MessageIdRequest request)
        {
            if (ModelState.IsValid)
            {
                Response response = new Response();
                _xbService.MarkMessageReaded(request.MessageId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի չկարդացված հաղորդագրությունների քանակը՝ կախված տեսակից (Ուղարկված/Ստացված)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost("GetUnreadMessagesCountByType")]
        public IActionResult GetUnreadMessagesCountByType(MessageTypeRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetUnreadMessagesCountByType(request.Type);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի չկարդացված հաղորդագրությունների քանակը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUnreadedMessagesCount")]
        public IActionResult GetUnreadedMessagesCount()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                response.Result = _xbService.GetUnreadedMessagesCount();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաղորդագրության կից ֆայլը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetMessageAttachmentById")]
        public IActionResult GetMessageAttachmentById(MessageIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<OrderAttachment> response = new SingleResponse<OrderAttachment>();
                response.Result = _xbService.GetMessageAttachmentById(request.MessageId);
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