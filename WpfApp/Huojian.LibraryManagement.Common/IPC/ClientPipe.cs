using System.IO.Pipes;
using System.Text;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Huojian.LibraryManagement.Common.IPC
{
    class ClientPipe : BasePipe
    {
        // 防止字符串自动转换为日期格式 Fix#ISSUE-936
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        };

        private readonly NamedPipeClientStream _stream;
        private readonly string _pipeName;

        private ClientPipe(NamedPipeClientStream stream, string pipeName)
            : base(stream, pipeName)
        {
            _stream = stream;
            _pipeName = pipeName;
        }

        public IPCResponse ReadResponse(int timeout)
        {
            var task = Task.Run(() =>
            {
                var sizeBytes = ReadBytes(4);
                var size = BitConverter.ToInt32(sizeBytes, 0);
                return ReadBytes(size);
            });
            if (!task.Wait(timeout))
                throw new IPCPipeTimeoutException($"xxx");
            byte[] bytes = task.Result;
            var content = Encoding.UTF8.GetString(bytes);
            var response = JsonConvert.DeserializeObject<IPCResponse>(content, _jsonSerializerSettings);
            return response;
        }

        public void WriteRequest(IPCRequest request, int timeout)
        {
            var content = JsonConvert.SerializeObject(request);
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var lengthBytes = BitConverter.GetBytes(contentBytes.Length);
            var task = Task.Run(() =>
            {
                WriteBytes(lengthBytes);
                WriteBytes(contentBytes);
            });
            if (!task.Wait(timeout))
                throw new IPCPipeTimeoutException("xxx");
        }

        public static ClientPipe Connect(string pipeName, int timeout = -1)
        {
            var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            if (timeout <= 0)
                stream.Connect();
            else
                stream.Connect(timeout);
            return new ClientPipe(stream, pipeName);
        }
    }
}