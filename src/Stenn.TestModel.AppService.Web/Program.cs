using Microsoft.EntityFrameworkCore;
using Seedwork.Configuration.Contracts;
using Seedwork.Network.Rpc.Http.AspNet;
using Stenn.TestModel.AppService.Server;
using Stenn.TestModel.Domain.Tests;
using Microsoft.Extensions.DependencyInjection;
using Seedwork.DependencyInjection.Netcore;
using Seedwork.Network.Core.Abstractions;
using Seedwork.Logging;

namespace Stenn.TestModel.AppService.Web
{
    public class Program
    {

#if NET6_0
        private const string DBName = "test-appdata-service_net6";
#elif NET7_0
        protected const string DBName = "test-appdata-service_net7";
#elif NET8_0
        protected const string DBName = "test-appdata-service_net8";
#endif

        private static string GetConnectionString()
        {
            return $@"Data Source=.\SQLEXPRESS;Initial Catalog={DBName};MultipleActiveResultSets=True;Integrated Security=SSPI;Encrypt=False";
        }

        internal static DbContext GetDbContext()
        {
            var services = new ServiceCollection();
            var connString = GetConnectionString();
            AddPersistanceDbContext(services, connString);
            return services.BuildServiceProvider().GetRequiredService<TestModelDbContext>();
        }

        private static void AddPersistanceDbContext(IServiceCollection services, string connString)
        {
            services.AddDbContext<TestModelDbContext>(builder =>
            {
                builder.UseSqlServer(connString);
            });
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AddPersistanceDbContext(builder.Services, GetConnectionString());

            builder.Services.AddSeedworkNetCoreDi();

            builder.Services.AddSingleton(new EnvironmentConfig("RpcTests", "Test")); // add environment config

            builder.Services.AddTestServiceRpcHandlers(); // register rpc handlers

            builder.Services.AddControllers()
                            .AddNewtonsoftJson() // crucial for RPC serialization/deserialization
                            .AddAspNetRpcServer();

            builder.Services.AddScopedContext<ILogContext>();
            builder.Services.AddScopedContext<IOperationContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}