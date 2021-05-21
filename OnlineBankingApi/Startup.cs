using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Middlewares;
using OnlineBankingApi.Services;
using OnlineBankingApi.Utilities;
using System;
using System.IO;
using XBS;
using Microsoft.OpenApi.Models;
using static OnlineBankingApi.Middlewares.RequestResponseLoggingMiddleware;
using ArcaCardAttachmentService;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.StaticFiles;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using OnlineBankingApi.Resources;

namespace OnlineBankingApi
{
    public class Startup
    {

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder().
                SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddLocalization();
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(APIExceptionFilter));
                options.Filters.Add(new ProducesResponseTypeAttribute(200));
                options.Filters.Add(new ProducesResponseTypeAttribute(500));
            })
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            });
            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportedCultures = new List<CultureInfo> {
                    new CultureInfo("en"),
                    new CultureInfo("hy")
                  };

                opts.DefaultRequestCulture = new RequestCulture("en");
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                opts.SupportedUICultures = supportedCultures;
                opts.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>

                {
                    if (context.Request.Headers["language"].ToString() == "1")
                    {
                        return new ProviderCultureResult("hy");
                    }
                    // My custom request culture logic
                    return new ProviderCultureResult("en");
                }));
            });

            services.AddHttpClient("cToken", c =>
            {
                c.BaseAddress = new Uri(Configuration["CTokenURL"]);
                c.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ACBA Online Api", Version = "v1", Description = "<a href=" + "https://www.acba.am" + " target=" + "_blank>" + "<b>«ԱԿԲԱ-ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ»</b></a>" });
                c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "OnlineBankingApi.xml"));
                c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "OnlineBankingLibrary.xml"));
                c.OperationFilter<HeaderFilter>();
                c.CustomSchemaIds(x => x.FullName);
            });
            
            services.AddMemoryCache();

            services.InjectTypesIntoDependencies();

            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });


            services.AddGrpcClient<ArcaCard.ArcaCardClient>(o =>
            {
                o.Address = new Uri(Configuration["ArcaCardAttachmentService"]);
            });

            ////Fluent Validations Start
            //services.AddSingleton<IValidator<AccountNumberRequest>, FluentValidationFilter>();

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddFluentValidation();

            //services.Configure<ApiBehaviorOptions>(options =>
            //{
            //    options.InvalidModelStateResponseFactory = (context) =>
            //    {
            //        var errors = context.ModelState.Values.SelectMany(x => x.Errors.Select(p => p.ErrorMessage)).ToList();
            //        var result = new
            //        {
            //            Code = "00009",
            //            Message = "FluentValidation errors",
            //            Errors = errors
            //        };
            //        return new BadRequestObjectResult(result);
            //    };
            //});
            ////Fluent Validations End
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMemoryCache cache)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();
            //app.UseStaticFiles();
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Add(".exe", "application/vnd.microsoft.portable-executable"); //file ext, ContentType
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            //localization
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ACBA Online Api v1");
            });

            //Request_Response logging
            Action<RequestProfilerModel> requestResponseHandler = requestProfilerModel =>
            {
                LoggerUtility.WriteRequestLog(requestProfilerModel, Configuration.GetConnectionString("AppLog"), !Convert.ToBoolean(Configuration["TestVersion"]), Convert.ToBoolean(Configuration["LogContentObject"]));
            };
            app.UseMiddleware<RequestResponseLoggingMiddleware>(requestResponseHandler);
            app.UseMvc();
            cache.Set("user", new User() { userID = Convert.ToInt16(Configuration["UserId"]), filialCode = 22000 }, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
            cache.Set("XBMuser", new XBManagement.User() { userID = Convert.ToInt16(Configuration["UserId"]), filialCode = 22000 }, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
            cache.Set("OperationServiceUser", new ACBAServiceReference.User() { userID = Convert.ToInt16(Configuration["UserId"]), filialCode = 22000 }, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });

        }

    }
}
