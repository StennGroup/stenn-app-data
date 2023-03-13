using Microsoft.EntityFrameworkCore;
using Seedwork.Network.Rpc;
using Stenn.AppData.Contracts.RequestOptions;
using Stenn.TestModel.AppService.Contracts.Models;
using Stenn.TestModel.Domain.Tests;
using Stenn.TestModel.Domain.Tests.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stenn.TestModel.AppService.Server.Handlers
{
    internal class CountryRequestHandler : IRemoteCallAsyncHandler<CountryRequest, CountryResponse>
    {
        private readonly TestModelDbContext _dbContext;

        public CountryRequestHandler(TestModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CountryResponse> HandleAsync(CountryRequest request, IRemoteCallContext<CountryRequest, CountryResponse> context, CancellationToken token)
        {
            var countries = await _dbContext.Set<Country>().ApplyOptions(request.RequestOptions).ToListAsync();

            return new CountryResponse { Countries = countries }; // be carefull showing entities. use DTOs with appropriate fields
        }
    }
}
