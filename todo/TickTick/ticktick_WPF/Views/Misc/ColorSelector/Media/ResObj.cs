// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.Media.ResObj
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.IO;
using System.Reflection;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector.Media
{
  public class ResObj
  {
    private static Stream Get(Assembly assembly, string path)
    {
      return assembly.GetManifestResourceStream(assembly.GetName().Name + "." + path);
    }

    public static string GetString(Assembly assembly, string path)
    {
      try
      {
        return StreamObj.ToString(ResObj.Get(assembly, path));
      }
      catch
      {
        return (string) null;
      }
    }

    public static ImageSource GetImageSource(Assembly assembly, string path)
    {
      try
      {
        return StreamObj.ToImageSource(ResObj.Get(assembly, path));
      }
      catch
      {
        return (ImageSource) null;
      }
    }
  }
}
