// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.UpdateHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class UpdateHelper
  {
    public static async Task<UpdateModel> CheckUpdate()
    {
      if (Utils.IsNetworkAvailable())
      {
        try
        {
          string str1 = await Communicator.CheckUpdate();
          if (!string.IsNullOrWhiteSpace(str1))
          {
            UpdateModel updateModel = new UpdateModel()
            {
              downLoadUri = str1
            };
            string str2 = str1;
            char[] chArray = new char[1]{ '/' };
            updateModel.fileName = ((IEnumerable<string>) str2.Split(chArray)).Last<string>();
            updateModel.versionNum = double.Parse(((IEnumerable<string>) ((IEnumerable<string>) updateModel.fileName.Split('.')).First<string>().Split('_')).Last<string>());
            ReleaseNoteModel releaseNote = await Utils.GetReleaseNote();
            if (releaseNote != null)
            {
              DateTime result;
              if (DateTime.TryParseExact(releaseNote.release_date.ToString(), "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                updateModel.publishDate = result;
              updateModel.forceUpdate = releaseNote.IsForced();
              updateModel.greyForced = releaseNote.IsGreyForced();
            }
            return updateModel;
          }
        }
        catch (Exception ex)
        {
          return (UpdateModel) null;
        }
      }
      return (UpdateModel) null;
    }

    public static bool NeedUpgradeVersion(double next, double current) => next > current;

    public static int GetGreyDate(DateTime publishDate)
    {
      int num1 = new Random().Next(1, 100);
      int num2 = num1 <= 5 ? 0 : (num1 <= 10 ? 1 : (num1 <= 20 ? 2 : (num1 <= 50 ? 3 : 4)));
      return DateUtils.GetDateNum(publishDate.AddDays((double) num2));
    }

    internal static void TryDeleteOriginPackage()
    {
      Task.Run((Action) (() =>
      {
        if (!Directory.Exists(AppPaths.PackageDir))
          return;
        foreach (string file in Directory.GetFiles(AppPaths.PackageDir))
        {
          FileInfo fileInfo = new FileInfo(file);
          if ((DateTime.Today - fileInfo.LastWriteTime).TotalDays > 40.0)
          {
            fileInfo.Attributes = FileAttributes.Normal;
            fileInfo.Delete();
          }
        }
      }));
    }
  }
}
