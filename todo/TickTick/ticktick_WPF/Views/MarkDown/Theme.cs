// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Theme
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class Theme : INotifyPropertyChanged
  {
    private double _codeHeight = 0.875;
    private string _editorBackground = "#404040";
    private string _editorForeground = "#ccc";
    private double _header1Height = 1.28;
    private double _header2Height = 1.14;
    private double _header3Height = 1.0;
    private double _header4Height = 1.0;
    private double _header5Height = 1.0;
    private double _header6Height = 1.0;
    private Highlight _highlightBlockCode = new Highlight()
    {
      Name = "BlockCode"
    };
    private Highlight _highlightBlockQuote = new Highlight()
    {
      Name = "BlockQuote"
    };
    private Highlight _highlightCheckedItem = new Highlight()
    {
      Name = "CheckedItem",
      Strikethrough = true
    };
    private Highlight _highlightEmphasis = new Highlight()
    {
      Name = "Emphasis",
      FontStyle = "italic"
    };
    private Highlight _highlightFlag = new Highlight()
    {
      Name = "Flag"
    };
    private Highlight _highlightFlagExtra = new Highlight()
    {
      Name = "FlagExtra"
    };
    private Highlight _highlightHeading = new Highlight()
    {
      Name = "transparent",
      FontWeight = "bold"
    };
    private Highlight _highlightHighlight = new Highlight()
    {
      Name = "Highlight"
    };
    private Highlight _highlightInlineCode = new Highlight()
    {
      Name = "InlineCode"
    };
    private Highlight _highlightLink = new Highlight()
    {
      Name = "Link"
    };
    private Highlight _highlightNormal = new Highlight()
    {
      Name = "Normal"
    };
    private Highlight _highlightStrikethrough = new Highlight()
    {
      Name = "Strikethrough",
      Strikethrough = true
    };
    private Highlight _highlightStrongEmphasis = new Highlight()
    {
      Name = "StrongEmphasis",
      FontWeight = "bold"
    };
    private Highlight _highlightTransparent = new Highlight()
    {
      Name = "Highlight"
    };
    private Highlight _highlightUnderLine = new Highlight()
    {
      Name = "UnderLine",
      Underline = true
    };
    private string _name = "Zenburn";

    public Theme(FrameworkElement element = null)
    {
      this._highlightFlag.Foreground = ThemeUtil.GetColor("BaseColorOpacity20", element);
      this._highlightFlagExtra.Foreground = ThemeUtil.GetColor("BaseColorOpacity20", element);
      this._highlightFlagExtra.Background = new SolidColorBrush(Colors.Transparent);
      this._highlightHighlight.Foreground = ThemeUtil.GetColor("MarkdownHighlightFontColor", element);
      this._highlightHighlight.Background = new SolidColorBrush(Colors.Transparent);
      this._highlightTransparent.Foreground = new SolidColorBrush(Colors.Transparent);
      this._highlightLink.Foreground = ThemeUtil.GetPrimaryColor(1.0);
      this._highlightInlineCode.Foreground = ThemeUtil.GetColor("BaseColorOpacity60", element);
      this._highlightInlineCode.Background = ThemeUtil.GetColor("CodeBackground", element);
      this._highlightInlineCode.FontType = Constants.DefaultMonoFont;
      this._highlightBlockCode.Foreground = ThemeUtil.GetColor("BaseColorOpacity100", element);
      this._highlightBlockCode.Background = ThemeUtil.GetColor("CodeBackground", element);
      this._highlightBlockCode.FontType = Constants.DefaultMonoFont;
      this._highlightBlockQuote.Foreground = ThemeUtil.GetColor("BaseSolidColorOpacity60", element);
      this._highlightStrikethrough.Foreground = ThemeUtil.GetColor("BaseColorOpacity60", element);
      this._highlightCheckedItem.Foreground = ThemeUtil.GetColor("BaseColorOpacity40", element);
    }

    public string Name
    {
      get => this._name;
      set => this.Set<string>(ref this._name, value, nameof (Name));
    }

    public double Header1Height
    {
      get => this._header1Height;
      set => this.Set<double>(ref this._header1Height, value, nameof (Header1Height));
    }

    public double Header2Height
    {
      get => this._header2Height;
      set => this.Set<double>(ref this._header2Height, value, nameof (Header2Height));
    }

    public double Header3Height
    {
      get => this._header3Height;
      set => this.Set<double>(ref this._header3Height, value, nameof (Header3Height));
    }

    public double Header4Height
    {
      get => this._header4Height;
      set => this.Set<double>(ref this._header4Height, value, nameof (Header4Height));
    }

    public double Header5Height
    {
      get => this._header5Height;
      set => this.Set<double>(ref this._header5Height, value, nameof (Header5Height));
    }

    public double Header6Height
    {
      get => this._header6Height;
      set => this.Set<double>(ref this._header6Height, value, nameof (Header6Height));
    }

    public Highlight HighlightHeading
    {
      get => this._highlightHeading;
      set => this.Set<Highlight>(ref this._highlightHeading, value, nameof (HighlightHeading));
    }

    public Highlight HighlightEmphasis
    {
      get => this._highlightEmphasis;
      set => this.Set<Highlight>(ref this._highlightEmphasis, value, nameof (HighlightEmphasis));
    }

    public Highlight HighlightStrongEmphasis
    {
      get => this._highlightStrongEmphasis;
      set
      {
        this.Set<Highlight>(ref this._highlightStrongEmphasis, value, nameof (HighlightStrongEmphasis));
      }
    }

    public Highlight HighlightInlineCode
    {
      get => this._highlightInlineCode;
      set
      {
        this.Set<Highlight>(ref this._highlightInlineCode, value, nameof (HighlightInlineCode));
      }
    }

    public Highlight HighlightBlockCode
    {
      get => this._highlightBlockCode;
      set => this.Set<Highlight>(ref this._highlightBlockCode, value, nameof (HighlightBlockCode));
    }

    public Highlight HighlightBlockQuote
    {
      get => this._highlightBlockQuote;
      set
      {
        this.Set<Highlight>(ref this._highlightBlockQuote, value, nameof (HighlightBlockQuote));
      }
    }

    public Highlight HighlightLink
    {
      get => this._highlightLink;
      set => this.Set<Highlight>(ref this._highlightLink, value, nameof (HighlightLink));
    }

    public Highlight HighlightFlagExtra
    {
      get => this._highlightFlagExtra;
      set => this.Set<Highlight>(ref this._highlightFlagExtra, value, nameof (HighlightFlagExtra));
    }

    public Highlight HighlightFlag
    {
      get => this._highlightFlag;
      set => this.Set<Highlight>(ref this._highlightFlag, value, nameof (HighlightFlag));
    }

    public Highlight HighlightStrikethrough
    {
      get => this._highlightStrikethrough;
      set
      {
        this.Set<Highlight>(ref this._highlightStrikethrough, value, nameof (HighlightStrikethrough));
      }
    }

    public Highlight HighlightUnderLine
    {
      get => this._highlightUnderLine;
      set => this.Set<Highlight>(ref this._highlightUnderLine, value, nameof (HighlightUnderLine));
    }

    public Highlight HighlightHighlight
    {
      get => this._highlightHighlight;
      set => this.Set<Highlight>(ref this._highlightHighlight, value, nameof (HighlightHighlight));
    }

    public Highlight HighlightTransparent
    {
      get => this._highlightTransparent;
      set
      {
        this.Set<Highlight>(ref this._highlightTransparent, value, nameof (HighlightTransparent));
      }
    }

    public Highlight HighlightNormal
    {
      get => this._highlightNormal;
      set => this.Set<Highlight>(ref this._highlightNormal, value, nameof (HighlightNormal));
    }

    public Highlight HighlightCheckedItem
    {
      get => this._highlightCheckedItem;
      set
      {
        this.Set<Highlight>(ref this._highlightCheckedItem, value, nameof (HighlightCheckedItem));
      }
    }

    public double CodeHeight
    {
      get => this._codeHeight;
      set => this.Set<double>(ref this._codeHeight, value, nameof (CodeHeight));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void Set<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(property, value))
        return;
      property = value;
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
