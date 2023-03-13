using Seedwork.Network.Core.Abstractions;

namespace Stenn.TestModel.AppService.IntegrationTests
{
    internal class TestInitialOperationContext : IOperationContext
    {
        public string OperationId { get; } = Guid.NewGuid().ToString();
        public string DistributedOperationId { get; } = Guid.NewGuid().ToString();
        public string CallerOperationId { get; } = null;
        public Guid? UserId { get; } = null;
        public string UserName { get; } = null;
        public string ActionName { get; set; } = "TestOperation";
    }
}