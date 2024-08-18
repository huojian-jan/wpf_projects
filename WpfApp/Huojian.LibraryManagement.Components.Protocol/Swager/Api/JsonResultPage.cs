using Newtonsoft.Json;

namespace Huojian.LibraryManagement.Components.Protocol.Swager.Api
{
    public class Page<T>
    {
        public Page(T data, JsonResultPage page)
        {
            Data = data;
            PageInfo = page;
        }

        public T Data { get; set; }

        public JsonResultPage PageInfo { get; set; }
    }

    public class JsonResultPage
    {
        //"page":{"total":66,"size":30,"page":1,"pages":3,"offset":0},

        [JsonProperty("total")] public int Total { get; set; }

        [JsonProperty("size")] public int Size { get; set; }
        [JsonProperty("pages")] public int Pages { get; set; }
        [JsonProperty("page")] public int Page { get; set; }

        public bool TotallyLoaded => Page >= Pages; // > 在第一页不满足 PageSize 但是出现了滚动条得情况下出现

    }
}
