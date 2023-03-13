using Seedwork.Network.Contracts.Rpc;
using Seedwork.Network.Rpc.Http;

namespace Stenn.TestModel.AppService.IntegrationTests
{
    public class TestHttpEndpointDiscoverer : IHttpEndpointDiscoverer
    {
        /// <inheritdoc />
        public Uri GetEndpointUri<TRequest, TResponse>(IRemoteCallRequest<TRequest, TResponse> request)
            where TRequest : class, IRemoteCallRequest<TRequest, TResponse>, new()
            where TResponse : class, IRemoteCallResponse<TRequest, TResponse>, new()
        {
            return new Uri("http://localhost:5000");
        }
    }
}
