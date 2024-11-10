// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CardViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class CardViewModel : BaseViewModel
  {
    private string _content;
    private CardType _type;
    public int Version = 2;

    public LogicType LogicType { get; set; } = LogicType.Or;

    public int Index { get; set; }

    public CardType Type
    {
      get => this._type;
      set
      {
        this._type = value;
        this.OnPropertyChanged(nameof (Type));
      }
    }

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public string ConditionName { get; set; } = string.Empty;

    public bool ShowDropdown { get; set; }

    public virtual ConditionEditDialog GenerateEditDialog() => (ConditionEditDialog) null;

    public virtual string ToCardText() => string.Empty;
  }
}
