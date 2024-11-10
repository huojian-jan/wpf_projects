// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MatrixManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class MatrixManager
  {
    private static MatrixRuleModel _defaultMatrixRuleModel;

    public static QuadrantRulesModel GetDefaultQuadrantRules(MatrixType type)
    {
      if (MatrixManager._defaultMatrixRuleModel == null)
      {
        try
        {
          MatrixManager._defaultMatrixRuleModel = JsonConvert.DeserializeObject<MatrixRuleModel>(IOUtils.GetFileContentInResourceFile("/Assets/MatrixRulesJson.json"));
        }
        catch (Exception ex)
        {
          UtilLog.Warn(ex.Message);
        }
      }
      return MatrixManager._defaultMatrixRuleModel == null ? QuadrantRulesModel.GetDefault() : MatrixManager._defaultMatrixRuleModel.GetQuadrantRules(type);
    }

    public static string GetDefaultQuadrantRule(MatrixType type, int level)
    {
      QuadrantRulesModel defaultQuadrantRules = MatrixManager.GetDefaultQuadrantRules(type);
      if (defaultQuadrantRules != null)
      {
        switch (level)
        {
          case 1:
            return defaultQuadrantRules.Quadrant1;
          case 2:
            return defaultQuadrantRules.Quadrant2;
          case 3:
            return defaultQuadrantRules.Quadrant3;
          case 4:
            return defaultQuadrantRules.Quadrant4;
        }
      }
      return string.Empty;
    }

    public static QuadrantModel GetQuadrantByLevel(int level)
    {
      QuadrantModel quadrantModel = (QuadrantModel) null;
      MatrixModel matrixModel = LocalSettings.Settings.MatrixModel;
      if (matrixModel != null)
        quadrantModel = matrixModel.quadrants.FirstOrDefault<QuadrantModel>((Func<QuadrantModel, bool>) (q => q.id == "quadrant" + level.ToString()));
      return quadrantModel ?? QuadrantModel.GetDefault(level);
    }

    public static async Task<(int, string)> GetTaskQuadrantChangeString(int previous, string taskId)
    {
      int quadrantTaskBelong = await TaskService.GetQuadrantTaskBelong(taskId, previous);
      if (quadrantTaskBelong == previous)
        return (previous, (string) null);
      if (quadrantTaskBelong <= 0)
        return (-1, Utils.GetString("MoveOutOfMatrix"));
      string str = string.Format(Utils.GetString("MovedTo"), (object) MatrixManager.GetQuadrantNameByLevel(quadrantTaskBelong));
      return (quadrantTaskBelong, str);
    }

    public static string GetQuadrantNameByLevel(int level)
    {
      QuadrantModel quadrantByLevel = MatrixManager.GetQuadrantByLevel(level);
      return string.IsNullOrEmpty(quadrantByLevel.name) ? Utils.GetString("MatrixTitleQuadrant" + level.ToString()) : quadrantByLevel.name;
    }

    public static async Task UpdateAffectedQuadrantsOnTagChanged(string original, string revised)
    {
      if (LocalSettings.Settings.MatrixModel.quadrants == null)
        return;
      bool flag = false;
      foreach (QuadrantModel quadrant in LocalSettings.Settings.MatrixModel.quadrants)
      {
        string oldValue = "\"" + original + "\"";
        string newValue = "\"" + revised?.ToLower() + "\"";
        if (quadrant.rule != null && quadrant.rule.Contains(oldValue) && !quadrant.rule.Contains(newValue))
        {
          quadrant.rule = quadrant.rule.Replace(oldValue, newValue);
          flag = true;
        }
      }
      if (!flag)
        return;
      LocalSettings.Settings.OnMatrixQuadrantChanged();
    }

    public static void UpdateQuadrantsSortOrder(Dictionary<string, long> orders, object sender)
    {
      foreach (QuadrantModel quadrant in LocalSettings.Settings.MatrixModel.quadrants)
      {
        if (!string.IsNullOrEmpty(quadrant.id) && orders.ContainsKey(quadrant.id))
          quadrant.sortOrder = new long?(orders[quadrant.id]);
      }
      GlobalEventManager.NotifyQuadrantSortChanged(sender);
      LocalSettings.Settings.OnMatrixQuadrantChanged();
    }

    public static bool CheckNeedUpdateDefaultRule(MatrixModel model)
    {
      if (model.version >= 1)
        return false;
      foreach (QuadrantModel quadrant in model.quadrants)
      {
        int result;
        if (quadrant.id.Length <= 8 || !int.TryParse(quadrant.id.Substring(8), out result) || MatrixManager.GetDefaultQuadrantRule(MatrixType.Version0, result) != quadrant.rule)
          return false;
      }
      return true;
    }

    public static async Task ResetMatrixRule(bool isSimple = true)
    {
      LocalSettings.Settings.MatrixModel.mtime = Utils.GetNowTimeStampInMills();
      LocalSettings.Settings.MatrixModel.ResetRule(isSimple);
      LocalSettings.Settings.MatrixModel.version = 1;
      DataChangedNotifier.NotifyMatrixQuadrantChanged(0);
      await SettingsHelper.PushLocalPreference();
    }
  }
}
