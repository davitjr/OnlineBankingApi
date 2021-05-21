using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Models;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Services
{
    public static class ServiceProviderExtensions
    {
        private static void InjectConnectedExternalServicesIntoDependencies(this IServiceCollection services)
        {
            services.AddSingleton<XBService>();
            services.AddSingleton<XBInfoService>();
            services.AddSingleton<ContractService>();
            services.AddSingleton<XBSecurityService>();
            services.AddSingleton<ReportService>();
            services.AddSingleton<XBManagementService>();
            services.AddSingleton<ACBAOperationService>();
            services.AddSingleton<SMSMessagingService>();
            services.AddSingleton<XBSecurityPushNotificationService>();
        }

        private static void InjectInternalTypesIntoDependencies(this IServiceCollection services)
        {
            services.AddScoped<CustomerRegistrationManager>();
            services.AddScoped<OnlineBankingRegistrationManager>();
            services.AddSingleton<CacheManager>();
            services.AddSingleton<ContractManager>();
            services.AddSingleton<ReportManager>();
            services.AddSingleton<CacheHelper>();
        }

        public static void InjectTypesIntoDependencies (this IServiceCollection services)
        {
            InjectConnectedExternalServicesIntoDependencies(services);
            InjectInternalTypesIntoDependencies(services);
        }
    }
}
