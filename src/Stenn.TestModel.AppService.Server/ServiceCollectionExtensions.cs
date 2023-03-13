using Microsoft.Extensions.DependencyInjection;
using Seedwork.Network.Rpc.Http;
using Stenn.TestModel.AppService.Server.Handlers;

namespace Stenn.TestModel.AppService.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTestServiceRpcHandlers(this IServiceCollection services)
        {
            services.AddAsRpcService<CountryRequestHandler>();

            return services;
        }
    }
}
