using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Utilities.LoggerUtility;

namespace OnlineBankingApi.Middlewares
{
    // https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    // https://gist.github.com/elanderson/c50b2107de8ee2ed856353dfed9168a2
    // https://stackoverflow.com/a/52328142/3563013
    // https://stackoverflow.com/a/43404745/3563013
    // https://gist.github.com/elanderson/c50b2107de8ee2ed856353dfed9168a2#gistcomment-2319007
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly Action<RequestProfilerModel> _requestResponseHandler;
        private const int ReadChunkBufferLength = 4096;
        private readonly IConfiguration _config;


        public RequestResponseLoggingMiddleware(RequestDelegate next, Action<RequestProfilerModel> requestResponseHandler, IConfiguration config)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _requestResponseHandler = requestResponseHandler;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            bool isProdVersion = !Convert.ToBoolean(_config["TestVersion"]);

            if (isProdVersion)
            {
                try
                {
                    var controllerName = this.GetControllerName(context.Request.Path.Value);
                    var crnControllContext = Type.GetType(controllerName);
                    var containsAttribute = crnControllContext.CustomAttributes.Any(attr => attr.AttributeType == typeof(LoggerOff));
                    if (containsAttribute)
                    {
                        await this._next.Invoke(context);
                        return;
                    }
                    var actionName = this.GetActionName(context.Request.Path.Value);
                    var action = crnControllContext.GetMethod(actionName);
                    containsAttribute = action.CustomAttributes.Any(attr => attr.AttributeType == typeof(LoggerOff));
                    if (containsAttribute)
                    {
                        await this._next.Invoke(context);
                        return;
                    }
                }
                catch
                {
                    await this._next.Invoke(context);
                    return;
                }
            }
            
            

            var model = new RequestProfilerModel
            {
                RequestTime = new DateTimeOffset(),
                Context = context,
                Request = await FormatRequest(context)
            };

            Stream originalBody = context.Response.Body;

            using (MemoryStream newResponseBody = _recyclableMemoryStreamManager.GetStream())
            {
                context.Response.Body = newResponseBody;

                await _next(context);

                newResponseBody.Seek(0, SeekOrigin.Begin);
                await newResponseBody.CopyToAsync(originalBody);

                newResponseBody.Seek(0, SeekOrigin.Begin);
                model.Response = FormatResponse(context, newResponseBody);
                model.ResponseTime = new DateTimeOffset();
                _requestResponseHandler(model);
            }
        }

        private string GetControllerName(string path)
        {
            String[] spearator = { "/" };
            var strlist = path.Split(spearator,
           StringSplitOptions.RemoveEmptyEntries);
            return "OnlineBankingApi.Controllers." + strlist[1] + "Controller";
        }

        private string GetActionName(string path)
        {
            String[] spearator = { "/" };
            var strlist = path.Split(spearator,
           StringSplitOptions.RemoveEmptyEntries);
            return strlist[2];
        }

        private string FormatResponse(HttpContext context, MemoryStream newResponseBody)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            var responseResultLogging = Convert.ToBoolean(_config["LogContentObject"]);
            if (responseResultLogging)
                return $"Http Response Information: {Environment.NewLine}" +
                    $"Schema:{request.Scheme} {Environment.NewLine}" +
                    $"Host: {request.Host} {Environment.NewLine}" +
                    $"Path: {request.Path} {Environment.NewLine}" +
                    $"QueryString: {request.QueryString} {Environment.NewLine}" +
                    $"StatusCode: {response.StatusCode} {Environment.NewLine}" +
                    $"Response Body: {ReadStreamInChunks(newResponseBody)}";
            else
                return $"{ReadStreamInChunks(newResponseBody)}";
        }

        private async Task<string> FormatRequest(HttpContext context)
        {
            HttpRequest request = context.Request;

            return $"Http Request Information: {Environment.NewLine}" +
                        $"Schema:{request.Scheme} {Environment.NewLine}" +
                        $"Host: {request.Host} {Environment.NewLine}" +
                        $"Path: {request.Path} {Environment.NewLine}" +
                        $"QueryString: {request.QueryString} {Environment.NewLine}" +
                        $"Request Body: {await GetRequestBody(request)}";
        }

        public async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            //request.EnableRewind(); .net core 3.1 EnableBuffering internal call enablerewind dont need to call it
            using (var requestStream = _recyclableMemoryStreamManager.GetStream())
            {
                await request.Body.CopyToAsync(requestStream);
                request.Body.Seek(0, SeekOrigin.Begin);
                return ReadStreamInChunks(requestStream);
            }
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            string result;
            using (var textWriter = new StringWriter())
            using (var reader = new StreamReader(stream))
            {
                var readChunk = new char[ReadChunkBufferLength];
                int readChunkLength;
                //do while: is useful for the last iteration in case readChunkLength < chunkLength
                do
                {
                    readChunkLength = reader.ReadBlock(readChunk, 0, ReadChunkBufferLength);
                    textWriter.Write(readChunk, 0, readChunkLength);
                } while (readChunkLength > 0);

                result = textWriter.ToString();
            }

            return result;
        }

        public class RequestProfilerModel
        {
            public DateTimeOffset RequestTime { get; set; }
            public HttpContext Context { get; set; }
            public string Request { get; set; }
            public string Response { get; set; }
            public DateTimeOffset ResponseTime { get; set; }
        }
    }

}
