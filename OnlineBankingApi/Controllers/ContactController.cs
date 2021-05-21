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
using XBS;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class ContactController : ControllerBase
    {
        private readonly XBService _xbService;

        public ContactController(XBService xbService)
        {
            _xbService = xbService;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ կոնտակտային անձի տվյալները
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [HttpPost("GetContact")]
        public IActionResult GetContact(ContactIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Contact> response = new SingleResponse<Contact>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetContact(request.ContactId);
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոնտակտային անձանց տվյալները
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetContacts")]
        public IActionResult GetContacts()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<List<Contact>> response = new SingleResponse<List<Contact>>();
                response.ResultCode = ResultCodes.normal;
                response.Result = _xbService.GetContacts();
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավելացնում է նոր կոնտակտային անձ
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPost("AddContact")]
        public IActionResult AddContact(ContactRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                try
                {
                    _xbService.AddContact(request.Contact);
                    response.ResultCode = ResultCodes.normal;
                }
                catch
                {
                    response.ResultCode = ResultCodes.failed;
                }
                response.Result = 0;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Թարմացնում է կոնտակտային անձի տվյալները
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPost("UpdateContact")]
        public IActionResult UpdateContact(UpdateContactRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                try
                {
                    _xbService.UpdateContact(request.Contact);
                    response.ResultCode = ResultCodes.normal;
                }
                catch
                {
                    response.ResultCode = ResultCodes.failed;
                }
                response.Result = 0;
                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Հեռացնում է կոնտակտային անձին
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [HttpPost("DeleteContact")]
        public IActionResult DeleteContact(ContactIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<int> response = new SingleResponse<int>();
                try
                {
                    _xbService.DeleteContact(request.ContactId);
                    response.ResultCode = ResultCodes.normal;
                }
                catch
                {
                    response.ResultCode = ResultCodes.failed;
                }
                response.Result = 0;

                return ResponseExtensions.ToHttpResponse(response); 
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}