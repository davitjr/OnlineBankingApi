using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using OnlineBankingLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using XBS;

namespace OnlineBankingLibrary.Utilities
{
    public class CacheHelper
    {
        private readonly CacheManager _cache;
        private readonly IHttpContextAccessor _contextAccessor;
        private string SessionId;

        public CacheHelper(CacheManager cache, IHttpContextAccessor contextAccessor)
        {
            _cache = cache;
            _contextAccessor = contextAccessor;
        }

        public AuthorizedCustomer GetAuthorizedCustomer()
        {
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.Get<string, AuthorizedCustomer>(SessionId + "_authorizedCustomer");
        }

        public string GetClientIp()
        {
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.Get<string, string>(SessionId + "_ClientIp");
        }

        public byte GetLanguage()
        {
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.Get<string, byte>(SessionId + "_Language");
        }

        public User GetUser()
        {
            return _cache.Get<string, User>("user");
        }

        public SourceType GetSourceType()
        {
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.Get<string, SourceType>(SessionId + "_SourceType");
        }

        public T GetApprovalOrder<T> (long orderId)
        {
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.Get<string,T>(SessionId + "_order_" + orderId.ToString());
        }

        public Order SetApprovalOrder(Order order)
        {
            //_cache.Set(SessionId + "_order_" + order.Id.ToString(),order);
            //return order;
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.GetOrAdd(SessionId + "_order_" + order.Id.ToString(), order);
        }

        public XBManagement.User GetXBMUser()
        {
            return _cache.Get<string, XBManagement.User>("XBMuser");
        }

        public ACBAServiceReference.User GetOPUser()
        {
            return _cache.Get<string, ACBAServiceReference.User>("OperationServiceUser");
        }
        /// <summary>
        /// Save Customer Token Info in cache with session Id
        /// </summary>
        /// <param name="customerTokenInfo"> Customer Token Operation Information</param>
        public void SetCustomerTokenInfo(CustomerTokenInfo customerTokenInfo)
        {
            _cache.Set(customerTokenInfo.SessionId + "_token_info", customerTokenInfo, 15);
        }
        /// <summary>
        /// Get Customer Token Info with session Id from cache
        /// </summary>
        /// <returns></returns>
        public CustomerTokenInfo GetCustomerTokenInfo()
        {
            SessionId = _contextAccessor.HttpContext.Request.Headers["SessionId"].ToString();
            return _cache.Get<string, CustomerTokenInfo>(SessionId + "_token_info");
        }
    }
}
