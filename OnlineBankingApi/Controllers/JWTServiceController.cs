using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Models;
using OnlineBankingLibrary.Utilities;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using OnlineBankingApi.Filters;
using XBManagement;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class JWTServiceController : ControllerBase
    {
        private readonly CacheHelper _cache;
        private readonly IHttpClientFactory _clientFactory;

        public JWTServiceController(CacheHelper cacheHelper,IHttpClientFactory clientFactory)
        {
            _cache = cacheHelper;
            _clientFactory = clientFactory;
        }
        /// <summary>
		/// Կանչվում է մոբայլից քարտի թոքենիզացման ժամանակ վերադարձնում է թոքեն և քարտի տվյալներ էնկռիպտ վիճակում
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
        [HttpPost("GenerateToken")]
        public IActionResult GenerateToken(GenerateTokenRequest request)
        {
            var authorizedCustomer = _cache.GetAuthorizedCustomer();
            if (authorizedCustomer!=null)
            {
                request.MobileUserName = authorizedCustomer.UserName;
            }
            


            var client = _clientFactory.CreateClient("cToken");
            string content = JsonConvert.SerializeObject(new {ProductId=request.ProductId,MobileUserName=request.MobileUserName });
            var httpResponse = client.PostAsync("generateToken", new StringContent(content, Encoding.Default, "application/json")).GetAwaiter().GetResult();
            var response = httpResponse.Content.ReadAsAsync<SingleResponse<GenerateTokenResponse>>().GetAwaiter().GetResult();

            return ResponseExtensions.ToHttpResponse(response);
        }
    }
}