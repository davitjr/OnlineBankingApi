using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace OnlineBankingApi.Filters
{

    public class HeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "SessionId",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "language",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "CustomerNumber",
                In = ParameterLocation.Header,           
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "SourceType",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Required = false
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "LocalIPAddress",
                In = ParameterLocation.Header,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Required = false
            });
        }
    }
}
