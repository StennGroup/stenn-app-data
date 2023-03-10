using System.Collections.Generic;
using Stenn.AppData;

namespace Stenn.TestModel.Domain.AppService.Tests
{
    internal sealed class TestModelDataService : AppDataService<TestModelAppDataServiceDbContext, ITestModelEntity>, ITestModelDataService
    {
        /// <inheritdoc />
        public TestModelDataService(TestModelAppDataServiceDbContext dbContext, IEnumerable<IAppDataProjection<ITestModelEntity>> projections)
            : base(dbContext, projections)
        {
        }
    }
}