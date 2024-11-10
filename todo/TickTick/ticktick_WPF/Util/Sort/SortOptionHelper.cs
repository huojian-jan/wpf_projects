// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sort.SortOptionHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Util.Sort
{
  public static class SortOptionHelper
  {
    private static readonly Utils.TupleList<string, string, int> SortTypeDictNormalProject = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.sortOrder.ToString(),
        Utils.GetString("Custom"),
        0
      },
      {
        Constants.SortType.dueDate.ToString(),
        Utils.GetString("Time"),
        1
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        2
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        3
      },
      {
        Constants.SortType.priority.ToString(),
        Utils.GetString("priority"),
        4
      }
    };
    private static readonly Utils.TupleList<string, string, int> SortTypeDicInGroup = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.project.ToString(),
        Utils.GetString("List"),
        0
      },
      {
        Constants.SortType.dueDate.ToString(),
        Utils.GetString("Time"),
        1
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        2
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        3
      },
      {
        Constants.SortType.priority.ToString(),
        Utils.GetString("priority"),
        4
      }
    };
    private static readonly Utils.TupleList<string, string, int> SortTypeDictWithProject = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.project.ToString(),
        Utils.GetString("List"),
        0
      },
      {
        Constants.SortType.dueDate.ToString(),
        Utils.GetString("Time"),
        1
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        2
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        3
      },
      {
        Constants.SortType.priority.ToString(),
        Utils.GetString("priority"),
        4
      }
    };
    private static readonly Utils.TupleList<string, string, int> SortTypeDictWithAssign = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.sortOrder.ToString(),
        Utils.GetString("Custom"),
        0
      },
      {
        Constants.SortType.dueDate.ToString(),
        Utils.GetString("Time"),
        1
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        2
      },
      {
        Constants.SortType.priority.ToString(),
        Utils.GetString("priority"),
        3
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        4
      },
      {
        Constants.SortType.assignee.ToString(),
        Utils.GetString("assignee"),
        5
      }
    };
    private static readonly Utils.TupleList<string, string, int> NoteSortTypeDictWithAssign = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.sortOrder.ToString(),
        Utils.GetString("Custom"),
        0
      },
      {
        Constants.SortType.modifiedTime.ToString(),
        Utils.GetString("ModifiedTime"),
        1
      },
      {
        Constants.SortType.createdTime.ToString(),
        Utils.GetString("CreatedTime"),
        2
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        3
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        4
      },
      {
        Constants.SortType.assignee.ToString(),
        Utils.GetString("assignee"),
        5
      }
    };
    private static readonly Utils.TupleList<string, string, int> NoteSortTypeDict = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.sortOrder.ToString(),
        Utils.GetString("Custom"),
        0
      },
      {
        Constants.SortType.modifiedTime.ToString(),
        Utils.GetString("ModifiedTime"),
        1
      },
      {
        Constants.SortType.createdTime.ToString(),
        Utils.GetString("CreatedTime"),
        2
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        3
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        4
      }
    };
    private static readonly Utils.TupleList<string, string, int> NoteSortTypeDictWithProject = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.project.ToString(),
        Utils.GetString("List"),
        0
      },
      {
        Constants.SortType.modifiedTime.ToString(),
        Utils.GetString("ModifiedTime"),
        1
      },
      {
        Constants.SortType.createdTime.ToString(),
        Utils.GetString("CreatedTime"),
        2
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        3
      },
      {
        Constants.SortType.tag.ToString(),
        Utils.GetString("tag"),
        4
      }
    };
    private static readonly Utils.TupleList<string, string, int> SortTypeTagProjectDict = new Utils.TupleList<string, string, int>()
    {
      {
        Constants.SortType.project.ToString(),
        Utils.GetString("List"),
        0
      },
      {
        Constants.SortType.dueDate.ToString(),
        Utils.GetString("Time"),
        1
      },
      {
        Constants.SortType.title.ToString(),
        Utils.GetString("Title"),
        2
      },
      {
        Constants.SortType.priority.ToString(),
        Utils.GetString("priority"),
        3
      }
    };

    private static Dictionary<string, SortTypeViewModel> GetSortTypeDictionary(bool isTimeline = false)
    {
      Dictionary<string, SortTypeViewModel> sortTypeDictionary = new Dictionary<string, SortTypeViewModel>();
      Constants.SortType sortType1 = Constants.SortType.sortOrder;
      string key1 = sortType1.ToString();
      sortType1 = Constants.SortType.sortOrder;
      SortTypeViewModel sortTypeViewModel1 = new SortTypeViewModel(sortType1.ToString(), isTimeline);
      sortTypeDictionary.Add(key1, sortTypeViewModel1);
      Constants.SortType sortType2 = Constants.SortType.project;
      string key2 = sortType2.ToString();
      sortType2 = Constants.SortType.project;
      SortTypeViewModel sortTypeViewModel2 = new SortTypeViewModel(sortType2.ToString());
      sortTypeDictionary.Add(key2, sortTypeViewModel2);
      Constants.SortType sortType3 = Constants.SortType.dueDate;
      string key3 = sortType3.ToString();
      sortType3 = Constants.SortType.dueDate;
      SortTypeViewModel sortTypeViewModel3 = new SortTypeViewModel(sortType3.ToString());
      sortTypeDictionary.Add(key3, sortTypeViewModel3);
      Constants.SortType sortType4 = Constants.SortType.title;
      string key4 = sortType4.ToString();
      sortType4 = Constants.SortType.title;
      SortTypeViewModel sortTypeViewModel4 = new SortTypeViewModel(sortType4.ToString());
      sortTypeDictionary.Add(key4, sortTypeViewModel4);
      Constants.SortType sortType5 = Constants.SortType.priority;
      string key5 = sortType5.ToString();
      sortType5 = Constants.SortType.priority;
      SortTypeViewModel sortTypeViewModel5 = new SortTypeViewModel(sortType5.ToString());
      sortTypeDictionary.Add(key5, sortTypeViewModel5);
      Constants.SortType sortType6 = Constants.SortType.tag;
      string key6 = sortType6.ToString();
      sortType6 = Constants.SortType.tag;
      SortTypeViewModel sortTypeViewModel6 = new SortTypeViewModel(sortType6.ToString());
      sortTypeDictionary.Add(key6, sortTypeViewModel6);
      Constants.SortType sortType7 = Constants.SortType.assignee;
      string key7 = sortType7.ToString();
      sortType7 = Constants.SortType.assignee;
      SortTypeViewModel sortTypeViewModel7 = new SortTypeViewModel(sortType7.ToString());
      sortTypeDictionary.Add(key7, sortTypeViewModel7);
      Constants.SortType sortType8 = Constants.SortType.createdTime;
      string key8 = sortType8.ToString();
      sortType8 = Constants.SortType.createdTime;
      SortTypeViewModel sortTypeViewModel8 = new SortTypeViewModel(sortType8.ToString());
      sortTypeDictionary.Add(key8, sortTypeViewModel8);
      Constants.SortType sortType9 = Constants.SortType.modifiedTime;
      string key9 = sortType9.ToString();
      sortType9 = Constants.SortType.modifiedTime;
      SortTypeViewModel sortTypeViewModel9 = new SortTypeViewModel(sortType9.ToString());
      sortTypeDictionary.Add(key9, sortTypeViewModel9);
      return sortTypeDictionary;
    }

    public static Utils.TupleList<string, string, int> GetSortOptionDataInGroup(TaskType kind = TaskType.Task)
    {
      Utils.TupleList<string, string, int> sortTypeDicInGroup = SortOptionHelper.SortTypeDicInGroup;
      Utils.TupleList<string, string, int> optionDataInGroup = new Utils.TupleList<string, string, int>();
      foreach (Tuple<string, string, int> tuple in (List<Tuple<string, string, int>>) sortTypeDicInGroup)
      {
        string str1 = tuple.Item1;
        Constants.SortType sortType = Constants.SortType.dueDate;
        string str2 = sortType.ToString();
        if (!(str1 == str2))
        {
          string str3 = tuple.Item1;
          sortType = Constants.SortType.priority;
          string str4 = sortType.ToString();
          if (!(str3 == str4))
            goto label_5;
        }
        if (kind == TaskType.Note)
          continue;
label_5:
        optionDataInGroup.Add(tuple);
      }
      if (kind != TaskType.Task)
      {
        Tuple<string, string, int> tuple1 = new Tuple<string, string, int>(Constants.SortType.createdTime.ToString(), Utils.GetString("CreatedTime"), 5);
        Tuple<string, string, int> tuple2 = new Tuple<string, string, int>(Constants.SortType.modifiedTime.ToString(), Utils.GetString("ModifiedTime"), 6);
        optionDataInGroup.Insert(kind == TaskType.Note ? 1 : 5, tuple1);
        optionDataInGroup.Insert(kind == TaskType.Note ? 2 : 6, tuple2);
      }
      return optionDataInGroup;
    }

    public static Utils.TupleList<string, string, int> GetSortOptionData(
      bool normalProject,
      bool withShare,
      bool isTag = false,
      bool isTagParent = false,
      TaskType kind = TaskType.Task)
    {
      switch (kind)
      {
        case TaskType.Task:
          if (isTag)
            return !isTagParent ? SortOptionHelper.SortTypeTagProjectDict : SortOptionHelper.SortTypeDictWithProject;
          if (!normalProject)
            return SortOptionHelper.SortTypeDictWithProject;
          return !withShare ? SortOptionHelper.SortTypeDictNormalProject : SortOptionHelper.SortTypeDictWithAssign;
        case TaskType.Note:
          if (!normalProject)
            return SortOptionHelper.NoteSortTypeDictWithProject;
          return !withShare ? SortOptionHelper.NoteSortTypeDict : SortOptionHelper.NoteSortTypeDictWithAssign;
        case TaskType.TaskAndNote:
          return SortOptionHelper.SortTypeDictWithProject;
        default:
          return SortOptionHelper.SortTypeDictWithProject;
      }
    }

    public static List<SortTypeViewModel> GetNormalProjectSortTypeModels(
      bool isShare = false,
      bool isNote = false,
      bool isTimeline = false)
    {
      Dictionary<string, SortTypeViewModel> sortTypeDictionary = SortOptionHelper.GetSortTypeDictionary(isTimeline);
      Constants.SortType sortType1;
      List<SortTypeViewModel> projectSortTypeModels;
      if (isTimeline)
      {
        List<SortTypeViewModel> sortTypeViewModelList = new List<SortTypeViewModel>();
        Dictionary<string, SortTypeViewModel> dictionary1 = sortTypeDictionary;
        sortType1 = Constants.SortType.sortOrder;
        string key1 = sortType1.ToString();
        sortTypeViewModelList.Add(dictionary1[key1]);
        Dictionary<string, SortTypeViewModel> dictionary2 = sortTypeDictionary;
        sortType1 = Constants.SortType.dueDate;
        string key2 = sortType1.ToString();
        sortTypeViewModelList.Add(dictionary2[key2]);
        Dictionary<string, SortTypeViewModel> dictionary3 = sortTypeDictionary;
        sortType1 = Constants.SortType.tag;
        string key3 = sortType1.ToString();
        sortTypeViewModelList.Add(dictionary3[key3]);
        Dictionary<string, SortTypeViewModel> dictionary4 = sortTypeDictionary;
        sortType1 = Constants.SortType.priority;
        string key4 = sortType1.ToString();
        sortTypeViewModelList.Add(dictionary4[key4]);
        projectSortTypeModels = sortTypeViewModelList;
      }
      else if (isNote)
      {
        List<SortTypeViewModel> sortTypeViewModelList = new List<SortTypeViewModel>();
        Dictionary<string, SortTypeViewModel> dictionary5 = sortTypeDictionary;
        Constants.SortType sortType2 = Constants.SortType.sortOrder;
        string key5 = sortType2.ToString();
        sortTypeViewModelList.Add(dictionary5[key5]);
        Dictionary<string, SortTypeViewModel> dictionary6 = sortTypeDictionary;
        sortType2 = Constants.SortType.createdTime;
        string key6 = sortType2.ToString();
        sortTypeViewModelList.Add(dictionary6[key6]);
        Dictionary<string, SortTypeViewModel> dictionary7 = sortTypeDictionary;
        sortType2 = Constants.SortType.modifiedTime;
        string key7 = sortType2.ToString();
        sortTypeViewModelList.Add(dictionary7[key7]);
        Dictionary<string, SortTypeViewModel> dictionary8 = sortTypeDictionary;
        sortType2 = Constants.SortType.title;
        string key8 = sortType2.ToString();
        sortTypeViewModelList.Add(dictionary8[key8]);
        Dictionary<string, SortTypeViewModel> dictionary9 = sortTypeDictionary;
        sortType2 = Constants.SortType.tag;
        string key9 = sortType2.ToString();
        sortTypeViewModelList.Add(dictionary9[key9]);
        projectSortTypeModels = sortTypeViewModelList;
      }
      else
      {
        List<SortTypeViewModel> sortTypeViewModelList = new List<SortTypeViewModel>();
        Dictionary<string, SortTypeViewModel> dictionary10 = sortTypeDictionary;
        Constants.SortType sortType3 = Constants.SortType.sortOrder;
        string key10 = sortType3.ToString();
        sortTypeViewModelList.Add(dictionary10[key10]);
        Dictionary<string, SortTypeViewModel> dictionary11 = sortTypeDictionary;
        sortType3 = Constants.SortType.dueDate;
        string key11 = sortType3.ToString();
        sortTypeViewModelList.Add(dictionary11[key11]);
        Dictionary<string, SortTypeViewModel> dictionary12 = sortTypeDictionary;
        sortType3 = Constants.SortType.title;
        string key12 = sortType3.ToString();
        sortTypeViewModelList.Add(dictionary12[key12]);
        Dictionary<string, SortTypeViewModel> dictionary13 = sortTypeDictionary;
        sortType3 = Constants.SortType.tag;
        string key13 = sortType3.ToString();
        sortTypeViewModelList.Add(dictionary13[key13]);
        Dictionary<string, SortTypeViewModel> dictionary14 = sortTypeDictionary;
        sortType3 = Constants.SortType.priority;
        string key14 = sortType3.ToString();
        sortTypeViewModelList.Add(dictionary14[key14]);
        projectSortTypeModels = sortTypeViewModelList;
      }
      if (isShare)
      {
        List<SortTypeViewModel> sortTypeViewModelList = projectSortTypeModels;
        Dictionary<string, SortTypeViewModel> dictionary = sortTypeDictionary;
        sortType1 = Constants.SortType.assignee;
        string key = sortType1.ToString();
        SortTypeViewModel sortTypeViewModel = dictionary[key];
        sortTypeViewModelList.Add(sortTypeViewModel);
      }
      return projectSortTypeModels;
    }

    public static List<SortTypeViewModel> GetGroupTimelineSortTypeModels(bool isShare = false)
    {
      Dictionary<string, SortTypeViewModel> sortTypeDictionary = SortOptionHelper.GetSortTypeDictionary();
      if (isShare)
      {
        List<SortTypeViewModel> timelineSortTypeModels = new List<SortTypeViewModel>();
        Dictionary<string, SortTypeViewModel> dictionary1 = sortTypeDictionary;
        Constants.SortType sortType = Constants.SortType.project;
        string key1 = sortType.ToString();
        timelineSortTypeModels.Add(dictionary1[key1]);
        Dictionary<string, SortTypeViewModel> dictionary2 = sortTypeDictionary;
        sortType = Constants.SortType.dueDate;
        string key2 = sortType.ToString();
        timelineSortTypeModels.Add(dictionary2[key2]);
        Dictionary<string, SortTypeViewModel> dictionary3 = sortTypeDictionary;
        sortType = Constants.SortType.tag;
        string key3 = sortType.ToString();
        timelineSortTypeModels.Add(dictionary3[key3]);
        Dictionary<string, SortTypeViewModel> dictionary4 = sortTypeDictionary;
        sortType = Constants.SortType.priority;
        string key4 = sortType.ToString();
        timelineSortTypeModels.Add(dictionary4[key4]);
        Dictionary<string, SortTypeViewModel> dictionary5 = sortTypeDictionary;
        sortType = Constants.SortType.assignee;
        string key5 = sortType.ToString();
        timelineSortTypeModels.Add(dictionary5[key5]);
        return timelineSortTypeModels;
      }
      List<SortTypeViewModel> timelineSortTypeModels1 = new List<SortTypeViewModel>();
      Dictionary<string, SortTypeViewModel> dictionary6 = sortTypeDictionary;
      Constants.SortType sortType1 = Constants.SortType.project;
      string key6 = sortType1.ToString();
      timelineSortTypeModels1.Add(dictionary6[key6]);
      Dictionary<string, SortTypeViewModel> dictionary7 = sortTypeDictionary;
      sortType1 = Constants.SortType.dueDate;
      string key7 = sortType1.ToString();
      timelineSortTypeModels1.Add(dictionary7[key7]);
      Dictionary<string, SortTypeViewModel> dictionary8 = sortTypeDictionary;
      sortType1 = Constants.SortType.tag;
      string key8 = sortType1.ToString();
      timelineSortTypeModels1.Add(dictionary8[key8]);
      Dictionary<string, SortTypeViewModel> dictionary9 = sortTypeDictionary;
      sortType1 = Constants.SortType.priority;
      string key9 = sortType1.ToString();
      timelineSortTypeModels1.Add(dictionary9[key9]);
      return timelineSortTypeModels1;
    }

    public static List<SortTypeViewModel> GetSmartProjectSortTypeModels(
      bool isNote = false,
      bool inTimeline = false)
    {
      Dictionary<string, SortTypeViewModel> sortTypeDictionary = SortOptionHelper.GetSortTypeDictionary();
      if (inTimeline)
      {
        List<SortTypeViewModel> projectSortTypeModels = new List<SortTypeViewModel>();
        Dictionary<string, SortTypeViewModel> dictionary1 = sortTypeDictionary;
        Constants.SortType sortType = Constants.SortType.project;
        string key1 = sortType.ToString();
        projectSortTypeModels.Add(dictionary1[key1]);
        Dictionary<string, SortTypeViewModel> dictionary2 = sortTypeDictionary;
        sortType = Constants.SortType.dueDate;
        string key2 = sortType.ToString();
        projectSortTypeModels.Add(dictionary2[key2]);
        Dictionary<string, SortTypeViewModel> dictionary3 = sortTypeDictionary;
        sortType = Constants.SortType.tag;
        string key3 = sortType.ToString();
        projectSortTypeModels.Add(dictionary3[key3]);
        Dictionary<string, SortTypeViewModel> dictionary4 = sortTypeDictionary;
        sortType = Constants.SortType.priority;
        string key4 = sortType.ToString();
        projectSortTypeModels.Add(dictionary4[key4]);
        return projectSortTypeModels;
      }
      if (isNote)
      {
        List<SortTypeViewModel> projectSortTypeModels = new List<SortTypeViewModel>();
        Dictionary<string, SortTypeViewModel> dictionary5 = sortTypeDictionary;
        Constants.SortType sortType = Constants.SortType.createdTime;
        string key5 = sortType.ToString();
        projectSortTypeModels.Add(dictionary5[key5]);
        Dictionary<string, SortTypeViewModel> dictionary6 = sortTypeDictionary;
        sortType = Constants.SortType.modifiedTime;
        string key6 = sortType.ToString();
        projectSortTypeModels.Add(dictionary6[key6]);
        Dictionary<string, SortTypeViewModel> dictionary7 = sortTypeDictionary;
        sortType = Constants.SortType.project;
        string key7 = sortType.ToString();
        projectSortTypeModels.Add(dictionary7[key7]);
        Dictionary<string, SortTypeViewModel> dictionary8 = sortTypeDictionary;
        sortType = Constants.SortType.title;
        string key8 = sortType.ToString();
        projectSortTypeModels.Add(dictionary8[key8]);
        Dictionary<string, SortTypeViewModel> dictionary9 = sortTypeDictionary;
        sortType = Constants.SortType.tag;
        string key9 = sortType.ToString();
        projectSortTypeModels.Add(dictionary9[key9]);
        return projectSortTypeModels;
      }
      List<SortTypeViewModel> projectSortTypeModels1 = new List<SortTypeViewModel>();
      Dictionary<string, SortTypeViewModel> dictionary10 = sortTypeDictionary;
      Constants.SortType sortType1 = Constants.SortType.project;
      string key10 = sortType1.ToString();
      projectSortTypeModels1.Add(dictionary10[key10]);
      Dictionary<string, SortTypeViewModel> dictionary11 = sortTypeDictionary;
      sortType1 = Constants.SortType.dueDate;
      string key11 = sortType1.ToString();
      projectSortTypeModels1.Add(dictionary11[key11]);
      Dictionary<string, SortTypeViewModel> dictionary12 = sortTypeDictionary;
      sortType1 = Constants.SortType.title;
      string key12 = sortType1.ToString();
      projectSortTypeModels1.Add(dictionary12[key12]);
      Dictionary<string, SortTypeViewModel> dictionary13 = sortTypeDictionary;
      sortType1 = Constants.SortType.tag;
      string key13 = sortType1.ToString();
      projectSortTypeModels1.Add(dictionary13[key13]);
      Dictionary<string, SortTypeViewModel> dictionary14 = sortTypeDictionary;
      sortType1 = Constants.SortType.priority;
      string key14 = sortType1.ToString();
      projectSortTypeModels1.Add(dictionary14[key14]);
      return projectSortTypeModels1;
    }

    public static List<SortTypeViewModel> GetTagProjectSortTypeModels(bool showByTag)
    {
      Dictionary<string, SortTypeViewModel> sortTypeDictionary = SortOptionHelper.GetSortTypeDictionary();
      List<SortTypeViewModel> projectSortTypeModels = new List<SortTypeViewModel>()
      {
        sortTypeDictionary[Constants.SortType.project.ToString()],
        sortTypeDictionary[Constants.SortType.dueDate.ToString()],
        sortTypeDictionary[Constants.SortType.title.ToString()],
        sortTypeDictionary[Constants.SortType.priority.ToString()]
      };
      if (showByTag)
        projectSortTypeModels.Insert(3, sortTypeDictionary[Constants.SortType.tag.ToString()]);
      return projectSortTypeModels;
    }

    public static List<SortTypeViewModel> GetGroupProjectSortTypeModels(
      int displayKind,
      bool isShare = false)
    {
      Dictionary<string, SortTypeViewModel> sortTypeDictionary = SortOptionHelper.GetSortTypeDictionary();
      Constants.SortType sortType;
      List<SortTypeViewModel> projectSortTypeModels;
      switch (displayKind)
      {
        case 1:
          List<SortTypeViewModel> sortTypeViewModelList1 = new List<SortTypeViewModel>();
          Dictionary<string, SortTypeViewModel> dictionary1 = sortTypeDictionary;
          sortType = Constants.SortType.project;
          string key1 = sortType.ToString();
          sortTypeViewModelList1.Add(dictionary1[key1]);
          Dictionary<string, SortTypeViewModel> dictionary2 = sortTypeDictionary;
          sortType = Constants.SortType.createdTime;
          string key2 = sortType.ToString();
          sortTypeViewModelList1.Add(dictionary2[key2]);
          Dictionary<string, SortTypeViewModel> dictionary3 = sortTypeDictionary;
          sortType = Constants.SortType.modifiedTime;
          string key3 = sortType.ToString();
          sortTypeViewModelList1.Add(dictionary3[key3]);
          Dictionary<string, SortTypeViewModel> dictionary4 = sortTypeDictionary;
          sortType = Constants.SortType.title;
          string key4 = sortType.ToString();
          sortTypeViewModelList1.Add(dictionary4[key4]);
          Dictionary<string, SortTypeViewModel> dictionary5 = sortTypeDictionary;
          sortType = Constants.SortType.tag;
          string key5 = sortType.ToString();
          sortTypeViewModelList1.Add(dictionary5[key5]);
          projectSortTypeModels = sortTypeViewModelList1;
          break;
        case 2:
          List<SortTypeViewModel> sortTypeViewModelList2 = new List<SortTypeViewModel>();
          Dictionary<string, SortTypeViewModel> dictionary6 = sortTypeDictionary;
          sortType = Constants.SortType.project;
          string key6 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary6[key6]);
          Dictionary<string, SortTypeViewModel> dictionary7 = sortTypeDictionary;
          sortType = Constants.SortType.dueDate;
          string key7 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary7[key7]);
          Dictionary<string, SortTypeViewModel> dictionary8 = sortTypeDictionary;
          sortType = Constants.SortType.title;
          string key8 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary8[key8]);
          Dictionary<string, SortTypeViewModel> dictionary9 = sortTypeDictionary;
          sortType = Constants.SortType.tag;
          string key9 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary9[key9]);
          Dictionary<string, SortTypeViewModel> dictionary10 = sortTypeDictionary;
          sortType = Constants.SortType.priority;
          string key10 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary10[key10]);
          Dictionary<string, SortTypeViewModel> dictionary11 = sortTypeDictionary;
          sortType = Constants.SortType.createdTime;
          string key11 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary11[key11]);
          Dictionary<string, SortTypeViewModel> dictionary12 = sortTypeDictionary;
          sortType = Constants.SortType.modifiedTime;
          string key12 = sortType.ToString();
          sortTypeViewModelList2.Add(dictionary12[key12]);
          projectSortTypeModels = sortTypeViewModelList2;
          break;
        default:
          List<SortTypeViewModel> sortTypeViewModelList3 = new List<SortTypeViewModel>();
          Dictionary<string, SortTypeViewModel> dictionary13 = sortTypeDictionary;
          sortType = Constants.SortType.project;
          string key13 = sortType.ToString();
          sortTypeViewModelList3.Add(dictionary13[key13]);
          Dictionary<string, SortTypeViewModel> dictionary14 = sortTypeDictionary;
          sortType = Constants.SortType.dueDate;
          string key14 = sortType.ToString();
          sortTypeViewModelList3.Add(dictionary14[key14]);
          Dictionary<string, SortTypeViewModel> dictionary15 = sortTypeDictionary;
          sortType = Constants.SortType.title;
          string key15 = sortType.ToString();
          sortTypeViewModelList3.Add(dictionary15[key15]);
          Dictionary<string, SortTypeViewModel> dictionary16 = sortTypeDictionary;
          sortType = Constants.SortType.tag;
          string key16 = sortType.ToString();
          sortTypeViewModelList3.Add(dictionary16[key16]);
          Dictionary<string, SortTypeViewModel> dictionary17 = sortTypeDictionary;
          sortType = Constants.SortType.priority;
          string key17 = sortType.ToString();
          sortTypeViewModelList3.Add(dictionary17[key17]);
          projectSortTypeModels = sortTypeViewModelList3;
          break;
      }
      if (isShare)
      {
        List<SortTypeViewModel> sortTypeViewModelList4 = projectSortTypeModels;
        Dictionary<string, SortTypeViewModel> dictionary18 = sortTypeDictionary;
        sortType = Constants.SortType.assignee;
        string key18 = sortType.ToString();
        SortTypeViewModel sortTypeViewModel = dictionary18[key18];
        sortTypeViewModelList4.Add(sortTypeViewModel);
      }
      return projectSortTypeModels;
    }

    public static DrawingImage GetSortTypeImage(Constants.SortType sortType, bool isTimeline = false)
    {
      switch (sortType)
      {
        case Constants.SortType.sortOrder:
          return Utils.GetImageSource(isTimeline ? "TimelineDefaultSortOrderDrawingImage" : "customDrawingImage");
        case Constants.SortType.project:
          return Utils.GetImageSource("SortByListDrawingImage");
        case Constants.SortType.dueDate:
          return Utils.GetImageSource("SortByDateDrawingImage");
        case Constants.SortType.title:
          return Utils.GetImageSource("SortByTitleDrawingImage");
        case Constants.SortType.priority:
          return Utils.GetImageSource("SortByPriorityDrawingImage");
        case Constants.SortType.assignee:
          return Utils.GetImageSource("SortByAssigneeDrawingImage");
        case Constants.SortType.tag:
          return Utils.GetImageSource("SortByTagDrawingImage");
        case Constants.SortType.createdTime:
          return Utils.GetImageSource("SortByCreateTimeDrawingImage");
        case Constants.SortType.modifiedTime:
          return Utils.GetImageSource("SortByModifiedTimeDrawingImage");
        default:
          return (DrawingImage) null;
      }
    }
  }
}
