// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickAddToken
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickAddToken
  {
    public TokenType TokenType { get; set; }

    public string Value { get; set; }

    public bool Exclusive { get; set; }

    public static QuickAddToken BuildTag(string tag)
    {
      return new QuickAddToken()
      {
        TokenType = TokenType.Tag,
        Exclusive = false,
        Value = tag
      };
    }

    public static QuickAddToken BuildProject(string projectTitle)
    {
      return new QuickAddToken()
      {
        TokenType = TokenType.Project,
        Exclusive = true,
        Value = projectTitle
      };
    }

    public static QuickAddToken BuildPriority(string priority)
    {
      return new QuickAddToken()
      {
        TokenType = TokenType.Priority,
        Exclusive = true,
        Value = priority
      };
    }

    public static QuickAddToken BuildQuickDate(string date)
    {
      return new QuickAddToken()
      {
        TokenType = TokenType.QuickDate,
        Exclusive = true,
        Value = date
      };
    }

    public static QuickAddToken BuildAssignee(string name)
    {
      return new QuickAddToken()
      {
        TokenType = TokenType.Assignee,
        Exclusive = true,
        Value = name
      };
    }

    private bool Equals(QuickAddToken other)
    {
      return other != null && this.TokenType == other.TokenType && this.Value == other.Value;
    }

    public override bool Equals(object obj) => this.Equals(obj as QuickAddToken);
  }
}
