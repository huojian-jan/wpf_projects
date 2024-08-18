using Newtonsoft.Json;
using RestSharp;
//using RestSharp.Serializers.NewtonsoftJson;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.Components.Protocol.Swager;
using Huojian.LibraryManagement.Components.Protocol.Swager.Api;
using Autofac;
using Huojian.LibraryManagement.Common.Utilities;
using Huojian.LibraryManagement.Components.Swagger;

namespace ShadowBot.Components.Swagger
{
    public class ApiClient : IApiClient, IRestExecutor
    {
        private readonly ILifetimeScope _scope;
        private RestClient _restClient;

        public ApiClient()
        {
            var options = new RestClientOptions();
            options.RemoteCertificateValidationCallback += RemoteCertificateValidate;

            _restClient = new RestClient(options);
            Ipv4Helper.TryGetIpv4(out string ipv4);
            if (!string.IsNullOrEmpty(ipv4))
            {
                _restClient.AddDefaultHeader("ipv4", ipv4);
            }

            // 配置IoC容器
            var builder = new ContainerBuilder();
            builder.RegisterModule(new SwaggerAutofacModule());
            builder.RegisterInstance(this) // 注册IServiceProvider(IoC container)
                 .As<IRestExecutor>()
                 .As<IApiClient>();
            _scope = builder.Build().BeginLifetimeScope();
        }

        public string AurhorizationUser { get; set; }
        public string AuthorizationPassword { get; set; } = "NYI4CFOGQVd8NZuk";

        public string AuthorizationUser { get; set; } = "client";

        public Uri BaseUrl
        {

            get => default;
            set{}
            //get => Utility.GetObjectPropertyValue<RestClientOptions>(_restClient, "Options").BaseUrl;
            //set
            //{
            //    var options = Utility.GetObjectPropertyValue<RestClientOptions>(_restClient, "Options");
            //    options.BaseUrl = value;
            //}
        }

        public RestClient RestClient => _restClient;

        public T Get<T>()
        {
            return _scope.Resolve<T>();
        }

        public JsonResult Execute(RestRequest request, Method httpMethod)
        {
            BeforeRequest?.Invoke(request);
            var response = _restClient.ExecuteAsync(request, httpMethod).Result;
            var result = GetJsonResult(response);
            result.Response = response;
            return result;
        }

        public async Task<JsonResult> ExecuteAsync(RestRequest request, Method httpMethod, CancellationToken token = default)
        {
            BeforeRequest?.Invoke(request);
            request.Method = httpMethod;
            var response = await _restClient.ExecuteAsync(request, token);
            var result = GetJsonResult(response);
            result.Response = response;
            return result;
        }

        public async Task<JsonResult> ExecuteWithNewClientAsync(RestRequest request, Method httpMethod, int timeout, CancellationToken token = default)
        {
            BeforeRequest?.Invoke(request);
            request.Method = httpMethod;
            var options = new RestClientOptions()
            {
                BaseUrl = BaseUrl,
                //Timeout = timeout * 1000,
                //Proxy = DefaultWinAutoWebProxyBuilder.Build(),
            };
            options.RemoteCertificateValidationCallback += RemoteCertificateValidate;
            var headers = _restClient.DefaultParameters.Select(p => KeyValuePair.Create(p.Name, p.Value?.ToString()));
            using var client = new RestClient(options);
            client.AddDefaultHeaders(new Dictionary<string, string>(headers));
            var response = await client.ExecuteAsync(request, token);
            var result = GetJsonResult(response);
            result.Response = response;
            return result;
        }

        public void DownloadFile(RestRequest request, string filename)
        {
        }

        public string DownloadFileToPath(RestRequest request, string filePath)
        {
            return default;
        }

        public  Task DownloadFileAsync(RestRequest request, string filename)
        {
            return Task.CompletedTask;
        }

        public JsonResult GetJsonResult(RestResponse resp)
        {
            return default;
        }

        private static bool HandleWebError(RestResponse resp, out string error)
        {
            error = default;
            return default;
        }

        public event EventHandler<ApiErrorEventArgs> Error;

        public Func<RestRequest, RestRequest> BeforeRequest;

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
    }
}
