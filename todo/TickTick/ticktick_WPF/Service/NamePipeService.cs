// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.NamePipeService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public class NamePipeService
  {
    public static void SendPipeText(string arg, string pipeName)
    {
      NamedPipeClientStream state = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
      state.Connect(30000);
      byte[] bytes = Encoding.Unicode.GetBytes(arg);
      state.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(NamePipeService.AsyncSend), (object) state);
    }

    private static void AsyncSend(IAsyncResult iar)
    {
      try
      {
        NamedPipeClientStream asyncState = (NamedPipeClientStream) iar.AsyncState;
        asyncState.EndWrite(iar);
        asyncState.Flush();
        asyncState.Close();
        asyncState.Dispose();
      }
      catch (Exception ex)
      {
      }
    }

    public static void InitPipe(string pipeName, Action<string> handlePipeCommand)
    {
      try
      {
        NamedPipeServerStream state = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        state.BeginWaitForConnection(new AsyncCallback(HandleClientConnection), (object) state);
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }

      void HandleClientConnection(IAsyncResult ar)
      {
        try
        {
          using (NamedPipeServerStream asyncState = (NamedPipeServerStream) ar.AsyncState)
          {
            asyncState.EndWaitForConnection(ar);
            handlePipeCommand(IOUtils.ReadString(new Func<byte[], int, int, int>(((Stream) asyncState).Read)));
          }
          NamedPipeServerStream state = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
          state.BeginWaitForConnection(new AsyncCallback(HandleClientConnection), (object) state);
        }
        catch (Exception ex)
        {
        }
      }
    }
  }
}
