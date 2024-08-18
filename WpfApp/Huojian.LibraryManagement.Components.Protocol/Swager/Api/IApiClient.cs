using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Huojian.LibraryManagement.Components.Protocol.Swager
{
    public interface IApiClient
    {
        Uri BaseUrl { get; set; }

        string AurhorizationUser { get; set; }

        string AuthorizationPassword { get; set; }

        T Get<T>();

        event EventHandler<ApiErrorEventArgs> Error;
    }

        public class ApiErrorEventArgs : EventArgs
        {
            public ApiErrorEventArgs(RestResponse response)
            {
                Response = response;
            }

            public RestResponse Response { get; set; }
            public int Code { get; private set; }

            public String Error { get; private set; }
        }

    }
