using Microsoft.Extensions.DependencyInjection;
using Stenn.AppData.Contracts;
using Stenn.TestModel.AppService.Contracts;

namespace Stenn.TestModel.AppService.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTestModelAppDataService(this IServiceCollection services)
        {
            services.AddScoped<IAppDataServiceClient<ITestServiceRequest, ITestServiceResponse>, TestServiceClient<ITestServiceRequest, ITestServiceResponse>>();

            return services;
        }
    }
}
