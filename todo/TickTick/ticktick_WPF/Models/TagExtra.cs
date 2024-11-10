// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TagExtra
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TagExtra
  {
    private Dictionary<string, List<string>> _data = new Dictionary<string, List<string>>();

    public TagExtra()
    {
    }

    public TagExtra(string taskId, List<string> tagList) => this.AddTaskTagsPair(taskId, tagList);

    public void AddTaskTagsPair(string id, List<string> tags)
    {
      if (tags != null)
      {
        List<string> stringList = new List<string>((IEnumerable<string>) tags);
        this._data[id] = stringList;
      }
      else
        this._data[id] = new List<string>();
    }

    public bool ContainsId(string id) => !string.IsNullOrEmpty(id) && this._data.ContainsKey(id);

    public List<string> GetIds()
    {
      return this._data.Any<KeyValuePair<string, List<string>>>() ? this._data.Keys.ToList<string>() : new List<string>();
    }

    public List<string> GetTags(string id)
    {
      return this._data.Any<KeyValuePair<string, List<string>>>() && this._data.ContainsKey(id) ? this._data[id] : new List<string>();
    }

    public bool Any() => this._data.Any<KeyValuePair<string, List<string>>>();
  }
}
