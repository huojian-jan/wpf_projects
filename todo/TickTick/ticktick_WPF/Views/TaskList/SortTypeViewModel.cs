// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.SortTypeViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class SortTypeViewModel : BaseViewModel
  {
    private Geometry _icon;
    private string _id;
    private bool _selected;
    private string _title;

    public SortTypeViewModel(string id, bool inTimeLine = false)
    {
      this._id = id;
      this._title = SortTypeViewModel.GetTitleById(id, inTimeLine);
    }

    public string Id
    {
      get => this._id;
      set
      {
        this._id = value;
        this.OnPropertyChanged(nameof (Id));
      }
    }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public Geometry Icon
    {
      get => this._icon;
      set
      {
        this._icon = value;
        this.OnPropertyChanged(nameof (Icon));
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public static string GetTitleById(string id, bool inTimeline)
    {
      if (id != null)
      {
        switch (id.Length)
        {
          case 3:
            if (id == "tag")
              return Utils.GetString("tag");
            break;
          case 4:
            if (id == "none")
              return Utils.GetString("none");
            break;
          case 5:
            if (id == "title")
              return Utils.GetString("Title");
            break;
          case 7:
            switch (id[0])
            {
              case 'd':
                if (id == "dueDate")
                  return Utils.GetString("TimeToDate");
                break;
              case 'p':
                if (id == "project")
                  return Utils.GetString("List");
                break;
            }
            break;
          case 8:
            switch (id[0])
            {
              case 'a':
                if (id == "assignee")
                  return Utils.GetString("assignee");
                break;
              case 'p':
                if (id == "priority")
                  return Utils.GetString("priority");
                break;
            }
            break;
          case 9:
            if (id == "sortOrder")
              return Utils.GetString(inTimeline ? "Default" : "Custom");
            break;
          case 11:
            if (id == "createdTime")
              return Utils.GetString("CreatedTime");
            break;
          case 12:
            if (id == "modifiedTime")
              return Utils.GetString("ModifiedTime");
            break;
        }
      }
      return Utils.GetString("none");
    }

    public static Geometry GetSortIconById(string id, bool inTimeline)
    {
      if (id != null)
      {
        switch (id.Length)
        {
          case 3:
            if (id == "tag")
              return Utils.GetIcon("IcSorByTag");
            break;
          case 5:
            if (id == "title")
              return Utils.GetIcon("IcSorByTitle");
            break;
          case 7:
            switch (id[0])
            {
              case 'd':
                if (id == "dueDate")
                  return Utils.GetIcon("IcSorByDuedate");
                break;
              case 'p':
                if (id == "project")
                  return Utils.GetIcon("IcSorByList");
                break;
            }
            break;
          case 8:
            switch (id[0])
            {
              case 'a':
                if (id == "assignee")
                  return Utils.GetIcon("IcSorByAssign");
                break;
              case 'p':
                if (id == "priority")
                  return Utils.GetIcon("IcSorByPriority");
                break;
            }
            break;
          case 9:
            if (id == "sortOrder")
              return Utils.GetIcon("IcSorByCustom");
            break;
          case 11:
            if (id == "createdTime")
              return Utils.GetIcon("IcSortByCreateTime");
            break;
          case 12:
            if (id == "modifiedTime")
              return Utils.GetIcon("IcSortByModifiedTime");
            break;
        }
      }
      return Utils.GetIcon("IcSorByDuedate");
    }

    public static Geometry GetGroupIconById(string id)
    {
      switch (id)
      {
        case "sortOrder":
          return Utils.GetIcon("IcGroupByCustom");
        case "project":
          return Utils.GetIcon("IcGroupByProject");
        case "dueDate":
          return Utils.GetIcon("IcDateTime");
        case "priority":
          return Utils.GetIcon("IcGroupByPriority");
        case "tag":
          return Utils.GetIcon("IcGroupByTag");
        case "assignee":
          return Utils.GetIcon("IcGroupByAssign");
        default:
          return Utils.GetIcon("IcGroupByNone");
      }
    }

    public bool CanGroup()
    {
      return this.Id != "title" && this.Id != "createdTime" && this.Id != "modifiedTime";
    }
  }
}
