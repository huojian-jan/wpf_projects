// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Filter.FilterRuleTest
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Filter
{
  public class FilterRuleTest
  {
    public const string TAG = "FilterRuleTest";

    public static void Test() => FilterRuleTest.Test3();

    public static void Test1()
    {
      PriorityRule priorityRule = new PriorityRule();
      priorityRule.and = new List<int>() { 0, 1, 3, 5 };
      TagRule tagRule = new TagRule();
      tagRule.and = new List<string>() { "aaa", "bbb" };
      Log.d(nameof (FilterRuleTest), JsonConvert.SerializeObject((object) priorityRule));
      Log.d(nameof (FilterRuleTest), JsonConvert.SerializeObject((object) tagRule));
    }

    public static void Test2()
    {
      NormalRuleModel normalRuleModel = new NormalRuleModel();
      PriorityRule priorityRule = new PriorityRule();
      priorityRule.or = new List<int>() { 1, 3, 5 };
      AssigneeRule assigneeRule = new AssigneeRule();
      assigneeRule.or = new List<string>()
      {
        "other",
        "noassignee"
      };
      normalRuleModel.and = new List<object>();
      normalRuleModel.and.Add((object) priorityRule);
      normalRuleModel.and.Add((object) assigneeRule);
      Log.d(nameof (FilterRuleTest), JsonConvert.SerializeObject((object) normalRuleModel));
    }

    public static void Test3()
    {
      NormalRuleModel normalRuleModel = new NormalRuleModel();
      normalRuleModel.and = new List<object>();
      ListOrGroupRule listOrGroupRule = new ListOrGroupRule();
      listOrGroupRule.or = new List<object>();
      ListRule listRule = new ListRule();
      listRule.or = new List<string>() { "inbox1010557619" };
      GroupRule groupRule = new GroupRule();
      groupRule.or = new List<string>()
      {
        "57b5519c7b854628f7e07622"
      };
      listOrGroupRule.or.Add((object) listRule);
      listOrGroupRule.or.Add((object) groupRule);
      normalRuleModel.and.Add((object) listOrGroupRule);
      Log.d(nameof (FilterRuleTest), JsonConvert.SerializeObject((object) normalRuleModel));
    }
  }
}
