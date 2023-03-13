using Seedwork.Network.Contracts.Rpc;
using Seedwork.Network.Rpc;
using Stenn.AppData.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Stenn.TestModel.AppService.Client
{
    public class TestServiceClient<BaseRequest, BaseResponse> : IAppDataServiceClient<BaseRequest, BaseResponse>
        where BaseRequest : IAppDataRequest
        where BaseResponse : IAppDataResponse
    {
        private readonly IRemoteCallClient _httpRpcClient;

        public TestServiceClient(IRemoteCallClient httpRpcClient)
        {
            _httpRpcClient = httpRpcClient;
        }

        public Task<TResponse> CallAsync<TRequest, TResponse>(IRemoteCallRequest<TRequest, TResponse> request, CancellationToken token)
            where TRequest : class, IRemoteCallRequest<TRequest, TResponse>, BaseRequest, new()
            where TResponse : class, IRemoteCallResponse<TRequest, TResponse>, BaseResponse, new()
        {
            return _httpRpcClient.CallAsync(request, token);
        }

        public Task<TResponse> CallAsync<TRequest, TResponse>(IRemoteCallRequest<TRequest, TResponse> request, RemoteCallOptions options, CancellationToken token)
            where TRequest : class, IRemoteCallRequest<TRequest, TResponse>, BaseRequest, new()
            where TResponse : class, IRemoteCallResponse<TRequest, TResponse>, BaseResponse, new()
        {
            return _httpRpcClient.CallAsync(request, options, token);
        }
    }
}
