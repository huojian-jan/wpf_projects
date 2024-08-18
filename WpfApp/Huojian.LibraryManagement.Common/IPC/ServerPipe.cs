using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json;

namespace Huojian.LibraryManagement.Common.IPC
{
    class ServerPipe : BasePipe
    {
        // 防止字符串自动转换为日期格式 Fix#ISSUE-936
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        };

        private readonly NamedPipeServerStream _stream;
        private readonly string _pipeName;

        private ServerPipe(NamedPipeServerStream stream, string pipeName)
            : base(stream, pipeName)
        {
            _stream = stream;
            _pipeName = pipeName;
        }

        public string PipeName => _pipeName;

        public IPCRequest ReadRequest()
        {
            var sizeBytes = ReadBytes(4);
            var size = BitConverter.ToInt32(sizeBytes, 0);
            var bytes = ReadBytes(size);
            var content = Encoding.UTF8.GetString(bytes);
            try
            {
                return JsonConvert.DeserializeObject<IPCRequest>(content, _jsonSerializerSettings);
            }
            catch (InvalidCastException)
            {
                throw new IPCException(IPCResponseCode.BadRequest, $"xxx");
            }
        }

        public void WriteResponse(IPCResponse response)
        {
            string content;
            content = JsonConvert.SerializeObject(response);
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var lengthBytes = BitConverter.GetBytes(contentBytes.Length);
            WriteBytes(lengthBytes);
            WriteBytes(contentBytes);
        }

        public void WaitForConnection()
        {
            _stream.WaitForConnection();
        }

        public static ServerPipe CreateNew(string pipeName)
        {
            var stream = new NamedPipeServerStream(pipeName,
                PipeDirection.InOut, -1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
            return new ServerPipe(stream, pipeName);
        }
    }
}
