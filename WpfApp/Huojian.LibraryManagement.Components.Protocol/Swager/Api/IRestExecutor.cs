using RestSharp;

namespace Huojian.LibraryManagement.Components.Protocol.Swager.Api
{
    public interface IRestExecutor
    {
        RestClient RestClient { get; }

        JsonResult Execute(RestRequest request, Method httpMethod);

        Task<JsonResult> ExecuteAsync(RestRequest request, Method httpMethod, CancellationToken token = default);

        Task<JsonResult> ExecuteWithNewClientAsync(RestRequest request, Method httpMethod, int timeout, CancellationToken token = default);

        void DownloadFile(RestRequest request, string filename);

        string DownloadFileToPath(RestRequest request, string filePath);

        Task DownloadFileAsync(RestRequest request, string filename);
    }
}