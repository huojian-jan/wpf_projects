// Decompiled with JetBrains decompiler
// Type: IWshRuntimeLibrary.IWshShell3
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable disable
namespace IWshRuntimeLibrary
{
  [CompilerGenerated]
  [Guid("41904400-BE18-11D3-A28B-00104BD35090")]
  [TypeIdentifier]
  [ComImport]
  public interface IWshShell3 : IWshShell2
  {
    [SpecialName]
    [MethodImpl(MethodCodeType = MethodCodeType.Runtime)]
    sealed extern void _VtblGap1_4();

    [DispId(1002)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.IDispatch)]
    object CreateShortcut([MarshalAs(UnmanagedType.BStr), In] string PathLink);
  }
}
