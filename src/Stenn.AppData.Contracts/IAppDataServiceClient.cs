using Seedwork.Network.Rpc;
using System.Threading.Tasks;
using System.Threading;
using Seedwork.Network.Contracts.Rpc;

namespace Stenn.AppData.Contracts
{
    /// <summary>
    /// Based on IRemoteRequestClient, but with additional constraints IAppDataRequest and IAppDataResponse
    /// </summary>
    public interface IAppDataServiceClient<BaseRequest, BaseResponse> //: IRemoteCallClient
        where BaseRequest : IAppDataRequest
        where BaseResponse : IAppDataResponse
    {
        public Task<TResponse> CallAsync<TRequest, TResponse>(IRemoteCallRequest<TRequest, TResponse> request, CancellationToken token) 
            where TRequest : class, IRemoteCallRequest<TRequest, TResponse>, BaseRequest, new() 
            where TResponse : class, IRemoteCallResponse<TRequest, TResponse>, BaseResponse, new();

        public Task<TResponse> CallAsync<TRequest, TResponse>(IRemoteCallRequest<TRequest, TResponse> request, RemoteCallOptions options, CancellationToken token) 
            where TRequest : class, IRemoteCallRequest<TRequest, TResponse>, BaseRequest, new() 
            where TResponse : class, IRemoteCallResponse<TRequest, TResponse>, BaseResponse, new();
    }
}
