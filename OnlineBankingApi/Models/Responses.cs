using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static OnlineBankingApi.Enumerations;

namespace OnlineBankingApi.Models
{
    public interface IResponse
    {
        string Description { get; set; }
        ResultCodes ResultCode { get; set; }
        string ResultCodeDescription { get; }
    }

    public interface ISingleResponse<TModel> : IResponse
    {
        TModel Result { get; set; }
    }

    public abstract class BaseResponse : IResponse
    {
        public abstract string Description { get; set; }
        public abstract ResultCodes ResultCode { get; set; }
        public string ResultCodeDescription
        {
            get
            {
                return this.ResultCode.ToString();
            }
        }
    }

    /// <summary>
    /// Response-ների մինիմալ ֆորմատ
    /// </summary>
    public class Response : BaseResponse
    {
        /// <summary>
        /// Հաղորդագրություն
        /// </summary>
        public override string Description { get; set; }

        /// <summary>
        /// Արդյունքի կոդ
        /// </summary>
        public override ResultCodes ResultCode { get; set; }
    }

    public abstract class BaseSingleResponse<TModel> : BaseResponse
    {

    }

    public class SingleResponse<TModel> : BaseSingleResponse<TModel>
    {
        public override string Description { get; set; }

        public override ResultCodes ResultCode { get; set; }

        public TModel Result { get; set; }
    }

    public static class ResponseExtensions
    {
        public static IActionResult ToHttpResponse(this IResponse response)
        {
            var status = response.ResultCode == ResultCodes.failed ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;

            return new ObjectResult(response)
            {
                StatusCode = (int)status
            };
        }
    }

}
