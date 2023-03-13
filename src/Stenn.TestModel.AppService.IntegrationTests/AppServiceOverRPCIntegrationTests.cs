using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Seedwork.Configuration.Contracts;
using Seedwork.Network.Core.Abstractions;
using Seedwork.Network.Rpc;
using Seedwork.Network.Rpc.Http;
using Seedwork.Network.Rpc.Http.AspNet;
using Stenn.AppData.Contracts;
using Stenn.AppData.Contracts.RequestOptions;
using Stenn.TestModel.AppService.Client;
using Stenn.TestModel.AppService.Contracts;
using Stenn.TestModel.AppService.Contracts.Models;
using Stenn.TestModel.AppService.Web;
using Stenn.TestModel.Domain.Tests.Entities;

namespace Stenn.TestModel.AppService.IntegrationTests
{
    [TestFixture]
    public class AppServiceOverRPCIntegrationTests
    {
        private static DbContext GetDbContext()
        {
            return Program.GetDbContext();
        }

        private readonly CancellationToken TestCancellationToken = CancellationToken.None;

        private readonly WebApplicationFactory<Program> _applicationFactory = new();

        protected IAppDataServiceClient<ITestServiceRequest, ITestServiceResponse> _testServiceClient { get; set; } = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var serviceCollection = InitServicesClient();

            var serviceProviderClient = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;

            _testServiceClient = serviceProviderClient.GetRequiredService<IAppDataServiceClient<ITestServiceRequest, ITestServiceResponse>>();

            var dbContext = GetDbContext();
            await dbContext.Database.EnsureCreatedAsync(TestCancellationToken);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            var dbContext = GetDbContext();
            await dbContext.Database.EnsureDeletedAsync(TestCancellationToken);
        }

        private HttpClient GetHttpClient(string? uri = null)
        {
            var client = _applicationFactory.CreateClient();
            SetBaseAddress(client, uri);
            return client;
        }

        private static void SetBaseAddress(HttpClient client, string? uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                client.BaseAddress = new Uri(client.BaseAddress!, uri);
            }
        }

        private IServiceCollection InitServicesClient()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new EnvironmentConfig("RpcTests", "Test")); // add environment config

            services.AddScoped(c => GetHttpClient());
            services.AddSingleton<IHttpEndpointDiscoverer, TestHttpEndpointDiscoverer>();
            services.AddAspNetRpcClient();

            services.AddTestModelAppDataService();

            services.AddSingleton<IOperationContext>(new TestInitialOperationContext());

            return services;
        }

        /// <summary>
        /// Checking if webApp can process responses
        /// </summary>
        [Test]
        public async Task GetIndexTest()
        {
            var httpClient = GetHttpClient("/Hello");
            var response = await httpClient.GetAsync((string?)null, TestCancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Request country over RPC
        /// </summary>
        [Test]
        public async Task ClientQueryTest()
        {
            var expected = await GetDbContext().Set<Country>().ToListAsync();
            var actual = await _testServiceClient.CallAsync(new CountryRequest(), TestCancellationToken); // intellisense will show error if request does not implement ITestServiceRequest
            actual.Countries.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Request country with Condition type Unspecified filter
        /// </summary>
        [Test]
        public async Task FilterConditionTypeUnspecifiedTest()
        {
            var filter = new Filter
            {
                CurrentToken = new Condition { FieldName = nameof(Country.Alpha3Code), ConditionType = ConditionType.Unspecified, RightValue = "CYP" }
            };

            Func<Task> act = async () => await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { Filter = filter } }, TestCancellationToken);

            await act.Should().ThrowAsync<RemoteException>().WithMessage("Specified argument was out of the range of valid values. (Parameter 'Unspecified')");
        }

        /// <summary>
        /// Request country with Condition type Equals filter (and "Or" condition)
        /// </summary>
        [Test]
        [TestCase("CYP", "Chad")]
        public async Task FilterConditionTypeEqualsTest(string alpha3code, string name)
        {
            var expected = GetDbContext().Set<Country>().Where(i => i.Alpha3Code == alpha3code || i.Name == name);

            var filter = new Filter
            {
                CurrentToken = new Or
                {
                    First = new Condition { FieldName = nameof(Country.Alpha3Code), ConditionType = ConditionType.Equals, RightValue = alpha3code },
                    Second = new Condition { FieldName = nameof(Country.Name), ConditionType = ConditionType.Equals, RightValue = name },
                }
            };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { Filter = filter }}, TestCancellationToken);
            actual.Countries.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Request country with Condition type Equals filter
        /// </summary>
        [Test]
        [TestCase(20_000)]
        public async Task FilterConditionTypeGreaterTest(decimal GNPValue)
        {
            var expected = GetDbContext().Set<Country>().Where(i => i.NominalGnp > GNPValue);

            var filter = new Filter
            {
                CurrentToken = new Condition { FieldName = nameof(Country.NominalGnp), ConditionType = ConditionType.Greater, RightValue = GNPValue }
            };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { Filter = filter } }, TestCancellationToken);
            actual.Countries.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Request country with Condition type Equals filter
        /// </summary>
        [Test]
        [TestCase(20_000)]
        public async Task FilterConditionTypeLessTest(decimal GNPValue)
        {
            var expected = GetDbContext().Set<Country>().Where(i => i.NominalGnp < GNPValue);

            var filter = new Filter
            {
                CurrentToken = new Condition { FieldName = nameof(Country.NominalGnp), ConditionType = ConditionType.Less, RightValue = GNPValue }
            };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { Filter = filter } }, TestCancellationToken);
            actual.Countries.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Request country with ContainsCondition filter (and "And" condition)
        /// </summary>
        [Test]
        [TestCase(new string[] { "CYP", "USA", "ESP" }, new string[] { "Cyprus", "United States of America", "Taiwan" })]
        public async Task FilterContainsConditionTest(string[] alpha3codeArray, string[] nameArray)
        {
            var expected = GetDbContext().Set<Country>().Where(
                i => alpha3codeArray.Contains(i.Alpha3Code)
                && nameArray.Contains(i.Name)
            );

            var filter = new Filter
            {
                CurrentToken = new And
                {
                    First = new ContainsCondition { FieldName = nameof(Country.Alpha3Code), RightValue = alpha3codeArray },
                    Second = new ContainsCondition { FieldName = nameof(Country.Name), RightValue = nameArray },
                }
            };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { Filter = filter } }, TestCancellationToken);
            actual.Countries.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Request country with Condition type Equals filter
        /// </summary>
        [Test]
        public async Task FilterConditionDateTimeTest()
        {
            var expected = GetDbContext().Set<Country>().Where(i => i.Created < DateTime.Now.AddDays(-5));

            var filter = new Filter
            {
                CurrentToken = new Condition { FieldName = nameof(Country.Created), ConditionType = ConditionType.Less, RightValue = DateTime.Now.AddDays(-5) }
            };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { Filter = filter } }, TestCancellationToken);
            actual.Countries.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Request country with two sortings
        /// </summary>
        [Test]
        public async Task SortingAscTest()
        {
            var expected = GetDbContext().Set<Country>().OrderBy(i => i.NominalGnp);

            var sortOptions = new SortOptions { new SortItem { FieldName = nameof(Country.NominalGnp) } };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { SortOptions = sortOptions } }, TestCancellationToken);

            actual.Countries.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        /// <summary>
        /// Request country with two sortings
        /// </summary>
        [Test]
        public async Task SortingAscDoubleTest()
        {
            var expected = await GetDbContext().Set<Country>().OrderBy(i => i.NominalGnp).ThenBy(i => i.Name).ToListAsync();

            var sortOptions = new SortOptions { new SortItem { FieldName = nameof(Country.NominalGnp) }, new SortItem { FieldName = nameof(Country.Name) } };

            var actual = await _testServiceClient.CallAsync(new CountryRequest { RequestOptions = new RequestOptions { SortOptions = sortOptions } }, TestCancellationToken);
            
            actual.Countries.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        /// <summary>
        /// Request country with two sortings and pagination
        /// </summary>
        [Test]
        [TestCase(10, 3)]
        public async Task SortingAscDoubleTestWithLimit(int pageSize, int pageNumber)
        {
            var expected = GetDbContext().Set<Country>().OrderBy(i => i.NominalGnp).ThenBy(i => i.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var requestOptions = new RequestOptions
            {
                SortOptions = new SortOptions { new SortItem { FieldName = nameof(Country.NominalGnp) }, new SortItem { FieldName = nameof(Country.Name) } },
                Paging = new PagingOptions(pageSize, pageNumber)
            };

            var actual = await GetDbContext().Set<Country>().ApplyOptions(requestOptions).ToListAsync();

            actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }
    }
}
