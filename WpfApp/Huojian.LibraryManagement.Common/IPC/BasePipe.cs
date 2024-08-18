using System.IO.Pipes;

namespace Huojian.LibraryManagement.Common.IPC
{
    abstract class BasePipe : IDisposable
    {
        private readonly PipeStream _pipeStream;
        private readonly string _pipeName;

        public BasePipe(PipeStream pipeStream, string pipeName)
        {
            _pipeStream = pipeStream;
            _pipeName = pipeName;
        }

        public byte[] ReadBytes(int count)
        {
            var data = new byte[count];
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read;
                try
                {
                    read = _pipeStream.Read(data, offset, remaining);
                }
                catch (Exception ex)
                {
                    throw new IPCPipeException(ex.Message, ex);
                }
                if (read <= 0)
                    throw new IPCEndOfStreamException($"pipe[{_pipeName}] end of stream reached with {remaining} bytes left to read");
                remaining -= read;
                offset += read;
            }
            return data;
        }

        public void WriteBytes(byte[] buffer)
        {
            try
            {
                _pipeStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                throw new IPCPipeException(ex.Message, ex);
            }
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        _pipeStream.Close();
                        _pipeStream.Dispose();
                    }
                    catch (Exception e)
                    {
                        Logging.Error(e);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}