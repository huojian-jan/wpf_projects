// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Coaches.CoachModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Coaches.CoachCommands;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Coaches
{
  public class CoachModel
  {
    public string Title { get; set; }

    public string Info { get; set; }

    public string GifPath { get; set; }

    public BitmapImage Image { get; set; }

    public CoachModel Next { get; set; }

    public CoachModel Pre { get; set; }

    public string CommandTitle { get; set; }

    public ICommand Command { get; set; }

    public static async Task<CoachModel> GetTimeLineCoach()
    {
      CoachModel first = await BuildTimeLineGuideModel(1);
      CoachModel second = await BuildTimeLineGuideModel(2);
      CoachModel coachModel = await BuildTimeLineGuideModel(3);
      first.Next = second;
      second.Pre = first;
      second.Next = coachModel;
      coachModel.Pre = second;
      coachModel.Command = (ICommand) new TimelineCoachCommand();
      coachModel.CommandTitle = Utils.GetString("LearnMore");
      CoachModel timeLineCoach = first;
      first = (CoachModel) null;
      second = (CoachModel) null;
      return timeLineCoach;

      static async Task<CoachModel> BuildTimeLineGuideModel(int i)
      {
        string fileName = string.Format("TimelineGuide{0}{1}.gif", Utils.IsCn() ? (object) "_cn_" : (object) "_en_", (object) i);
        string str = AppPaths.ResourceDir + fileName;
        CoachModel model = new CoachModel()
        {
          Title = Utils.GetString("TimeLineGuideTitle" + i.ToString()),
          Info = Utils.GetString("TimeLineGuideInfo" + i.ToString()),
          GifPath = str
        };
        string remotePath = "https://" + BaseUrl.GetPullDomain() + "/windows/static/" + fileName;
        int num = await IOUtils.CheckResourceExist(AppPaths.ResourceDir, fileName, remotePath) ? 1 : 0;
        CoachModel coachModel = model;
        model = (CoachModel) null;
        return coachModel;
      }
    }
  }
}
