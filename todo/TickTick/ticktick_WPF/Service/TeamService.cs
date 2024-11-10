// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.TeamService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class TeamService
  {
    public static async Task<bool> PullTeams()
    {
      if (Utils.IsNetworkAvailable())
      {
        List<TeamModel> allTeams = await Communicator.GetAllTeams();
        if (allTeams != null)
          return await TeamDao.MergeTeams(allTeams);
      }
      return false;
    }

    public static ImageSource GetTeamLogo(TeamModel team)
    {
      string str = ThemeUtil.GetColor("PrimaryColor").Color.ToString();
      Border element = new Border();
      element.Width = 36.0;
      element.Height = 36.0;
      element.Background = string.IsNullOrEmpty(str) ? (Brush) Brushes.Transparent : (Brush) ThemeUtil.GetColorInString("#1A" + str.Substring(str.Length - 6));
      element.CornerRadius = new CornerRadius(18.0);
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 16.0;
      textBlock.Text = team.name.Substring(0, 1);
      textBlock.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      element.Child = (UIElement) textBlock;
      return (ImageSource) ThemeUtil.SaveAsWriteableBitmap((FrameworkElement) element);
    }

    public static async Task<bool> CheckFreeTeamShareCount(string teamId, Window owner = null)
    {
      TeamModel teamById = CacheManager.GetTeamById(teamId);
      if (teamById != null && !teamById.IsPro())
      {
        bool flag = CacheManager.GetProjects().Count<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == teamId && p.IsShareList())) >= 3;
        if (!flag)
          flag = await Communicator.GetTeamShareCount(teamById.id) <= 0L;
        if (flag)
        {
          ProChecker.ShowUpgradeDialog(ProType.TeamShareLimit, owner, teamId);
          return false;
        }
      }
      return true;
    }

    public static string GetTeamId()
    {
      if (!UserManager.IsTeamUser())
        return "";
      return CacheManager.GetTeam()?.id;
    }
  }
}
