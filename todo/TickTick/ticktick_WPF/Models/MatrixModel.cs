// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.MatrixModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Models
{
  public class MatrixModel : PreferenceBaseModel
  {
    public const int CurrentVersion = 1;

    public List<QuadrantModel> quadrants { get; set; }

    public bool? show_completed { get; set; }

    public int version { get; set; }

    public override bool SetRemoteValue(PreferenceBaseModel model)
    {
      bool flag = false;
      if (model is MatrixModel remote)
      {
        if (!remote.IsEmpty())
        {
          if (remote.mtime > this.mtime)
          {
            this.quadrants = remote.quadrants;
            this.show_completed = remote.show_completed;
            this.version = remote.version;
            this.mtime = remote.mtime;
            DataChangedNotifier.NotifyMatrixQuadrantChanged(0);
            UtilLog.Info("MatrixModel.SetRemoteValue");
          }
          else
            this.CheckSortOrder(remote);
          if (remote.mtime < this.mtime)
            flag = true;
        }
        else
        {
          this.mtime = Utils.GetNowTimeStampInMills();
          flag = true;
        }
        if (!this.show_completed.HasValue)
        {
          this.show_completed = new bool?(true);
          flag = true;
        }
      }
      return flag;
    }

    public static MatrixModel GetDefault()
    {
      UtilLog.Info("GetDefaultMatrix");
      MatrixModel matrixModel = new MatrixModel();
      matrixModel.mtime = 1L;
      matrixModel.version = 1;
      matrixModel.quadrants = new List<QuadrantModel>();
      for (int level = 1; level <= 4; ++level)
        matrixModel.quadrants.Add(QuadrantModel.GetDefault(level));
      return matrixModel;
    }

    public bool IsEmpty() => this.quadrants == null || this.quadrants.Count == 0;

    public void SetShowComplete(bool show)
    {
      this.show_completed = new bool?(show);
      this.mtime = Utils.GetNowTimeStampInMills();
    }

    public void CheckSortOrder(MatrixModel remote)
    {
      if (remote?.quadrants == null || !remote.quadrants.Any<QuadrantModel>((Func<QuadrantModel, bool>) (q => q.sortOrder.HasValue)) || !this.quadrants.Any<QuadrantModel>((Func<QuadrantModel, bool>) (q => !q.sortOrder.HasValue)))
        return;
      this.quadrants = remote.quadrants;
      DataChangedNotifier.NotifyMatrixQuadrantChanged(0);
    }

    public void ResetRule(bool isSimple = true)
    {
      foreach (QuadrantModel quadrant in this.quadrants)
      {
        if (quadrant.id.Length > 8)
        {
          int result;
          int.TryParse(quadrant.id.Substring(8), out result);
          quadrant.rule = MatrixManager.GetDefaultQuadrantRule(isSimple ? MatrixType.Simple : MatrixType.Complex, result);
        }
      }
    }

    public void InitSortOption()
    {
      foreach (QuadrantModel quadrant in this.quadrants)
        quadrant.SortOption = quadrant.GetSortOption();
      this.mtime = Utils.GetNowTimeStampInMills();
    }
  }
}
