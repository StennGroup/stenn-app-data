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
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions.Common;
using System.Reflection.Metadata.Ecma335;
using System.Net.Http;

namespace Stenn.TestModel.IntegrationTests
{
    [TestFixture]
    public class AppServiceClientIntegrationTests
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

        public HttpClient GetClient(string? uri = null)
        { 
            var client = _applicationFactory.CreateClient();
            if (!string.IsNullOrEmpty(uri)) client.BaseAddress = new Uri(client.BaseAddress!, uri);
            return client;
        }

        private ServiceProvider? ServiceProvider { get; set; }

        private ITestModelDataService? AppDataService { get; set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var serviceCollection = InitServices();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            AppDataService = ServiceProvider.GetRequiredService<ITestModelDataService>();

            var dbContext = ServiceProvider.GetRequiredService<TestModelDbContext>();
            await dbContext.Database.EnsureCreatedAsync(TestCancellationToken);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            var dbContext = ServiceProvider!.GetRequiredService<TestModelDbContext>();
            await dbContext.Database.EnsureDeletedAsync(TestCancellationToken);
        }

        internal IServiceCollection InitServices(string dbName = DBName)
        {
            var services = new ServiceCollection();

            var connString = GetConnectionString(dbName);
            services.AddDbContext<TestModelDbContext>(builder =>
            {
                builder.UseSqlServer(connString);
            });

            services.AddTestModelAppDataService(connString);

            services.AddTransient(i => GetClient("TestService/ExecuteSerializedExpression"));
            services.AddTransient<TestModelClient>();

            return services;
        }

        /// <summary>
        /// Checking if webApp can process responses
        /// </summary>
        [Test]
        public async Task GetIndexTest()
        {
            var hpptClient = GetClient();
            var response = await hpptClient.GetAsync("/TestService/Hello", TestCancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// AppDataClient builds expression and sends it to webApp. webApp executes expression on TestModelService to get data, serializes data and sends it back to client.
        /// </summary>
        [Test]
        public void ClientQueryTest()
        {
            var expectedResult = AppDataService!.Query<TestModelCountry>().Take(5).ToList();

            var appDataClient = ServiceProvider!.GetRequiredService<TestModelClient>();

            var result = appDataClient.Query<TestModelCountry>().Take(5).ToList();

            result.Should().BeEquivalentTo(expectedResult);
        }

        /// <summary>
        /// AppDataClient builds expression and sends it to webApp. webApp executes expression on TestModelService to get data, serializes data and sends it back to client.
        /// </summary>
        [Test]
        public void ClientQueryWithAnonymousTypeTest()
        {
            var expectedResult = AppDataService!.Query<TestModelCountry>().Select(i => new { i.Name, Code = i.Alpha3Code }).ToList();

            var appDataClient = ServiceProvider!.GetRequiredService<TestModelClient>();

            var result = appDataClient.Query<TestModelCountry>().Select(i => new { i.Name, Code = i.Alpha3Code }).ToList();

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void ClientQueryIncludeTest()
        {
            var expectedResult = AppDataService!.Query<TestModelCountryState>().Take(5).Include(s => s.Country).ToList();

            var appDataClient = ServiceProvider!.GetRequiredService<TestModelClient>();

            var result = appDataClient.Query<TestModelCountryState>().Take(5).Include(s => s.Country).ToList();

            result.Should().BeEquivalentTo(expectedResult);
            result.First().Country.Should().NotBeNull();
        }

        [Test]
        public async Task ClientQueryJoinTest()
        {
            var expectedResult = await AppDataService!.Query<TestModelCountry>()
                .Where(i => i.Id == "US")
                .Join(AppDataService!.Query<TestModelCountryState>(), c => c.Id, s => s.CountryId, (c, s) => new { Text = c.Name, Code = s.Description })
                .ToListAsync(cancellationToken: TestCancellationToken);

            var appDataClient = ServiceProvider!.GetRequiredService<TestModelClient>();

            var result = await appDataClient.Query<TestModelCountry>()
                .Where(i => i.Id == "US")
                .Join(appDataClient.Query<TestModelCountryState>(), c => c.Id, s => s.CountryId, (c, s) => new { Text = c.Name, Code = s.Description })
                .ToListAsync(cancellationToken: TestCancellationToken);

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void ClientQueryOrderTest()
        {
            var expectedResult = AppDataService!.Query<TestModelCountryState>().OrderBy(i=>i.Description).Take(5).ToList();

            var appDataClient = ServiceProvider!.GetRequiredService<TestModelClient>();

            var result = appDataClient.Query<TestModelCountryState>().OrderBy(i => i.Description).Take(5).ToList();

            result.Should().BeEquivalentTo(expectedResult);
        }

        /// <summary>
        /// We can manually craft Expression to read remote service config. Remote service should validate expression before execution
        /// </summary>
        [Test]
        public void ReadRemoteConfigTest()
        {
            var httpClient = GetClient();

            var pathExpression = Expression.Constant(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var ex = Expression.Call(typeof(File), "ReadAllText", null, pathExpression);
            var param = Expression.Parameter(typeof(object), "service");
            Expression<Func<object, string>> le = Expression.Lambda<Func<object, string>>(ex, param);

            var response = httpClient.PostAsync("/TestService/ExecuteSerializedExpression", new StringContent(SerializeExpression(le))).Result;
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        private string SerializeExpression(Expression expression)
        {
            var serializer = new ExpressionSerializer();
            var slimExpression = serializer.Lift(expression);
            return serializer.Serialize(slimExpression);
        }
    }
}