// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.CalendarConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable
namespace ticktick_WPF
{
  public class CalendarConverter
  {
    public const string Hijri = "hijri";
    public const string KoreanLunar = "korean-lunar";
    public const string Hebcal = "hebcal";
    public const string Shaka = "shaka";
    public const string Lunar = "lunar";
    public static Dictionary<int, int> KoreanLunarLeapYearMonth = new Dictionary<int, int>()
    {
      {
        1971,
        6
      },
      {
        1974,
        5
      },
      {
        1976,
        9
      },
      {
        1979,
        7
      },
      {
        1982,
        5
      },
      {
        1984,
        11
      },
      {
        1987,
        7
      },
      {
        1990,
        6
      },
      {
        1993,
        4
      },
      {
        1995,
        9
      },
      {
        1998,
        6
      },
      {
        2001,
        5
      },
      {
        2004,
        3
      },
      {
        2006,
        8
      },
      {
        2009,
        6
      },
      {
        2012,
        4
      },
      {
        2014,
        10
      },
      {
        2017,
        6
      },
      {
        2020,
        5
      },
      {
        2023,
        3
      },
      {
        2025,
        7
      },
      {
        2028,
        6
      },
      {
        2031,
        4
      },
      {
        2033,
        12
      },
      {
        2036,
        7
      },
      {
        2039,
        6
      },
      {
        2042,
        3
      }
    };
    public static Dictionary<int, string> KoreanLunarLeapMonthNames = new Dictionary<int, string>()
    {
      {
        1,
        "윤정월"
      },
      {
        2,
        "윤이월"
      },
      {
        3,
        "윤삼월"
      },
      {
        4,
        "윤사월"
      },
      {
        5,
        "윤오월"
      },
      {
        6,
        "윤유월"
      }
    };
    public static Dictionary<string, string[]> CalendarMonthNamesEn = new Dictionary<string, string[]>()
    {
      {
        "hijri",
        new string[12]
        {
          "Mhrm.",
          "Safr.",
          "Rab.Ⅰ",
          "Rab.Ⅱ",
          "Jmd.Ⅰ",
          "Jmd.Ⅱ",
          "Rajb.",
          "Shbn.",
          "Rmdn.",
          "Shwl.",
          "Dhu'l-Q.",
          "Dhu'l-H."
        }
      },
      {
        "korean-lunar",
        new string[12]
        {
          "정월",
          "여월",
          "가월",
          "초월",
          "매월",
          "계하",
          "교월",
          "계월",
          "현월",
          "개동",
          "설한",
          "극월"
        }
      },
      {
        "hebcal",
        new string[12]
        {
          "Tis",
          "Hes",
          "Kis",
          "Tev",
          "She",
          "Ada",
          "Nis",
          "Iya",
          "Siv",
          "Tam",
          "Av",
          "Elu"
        }
      },
      {
        "shaka",
        new string[12]
        {
          "Chai",
          "Vai",
          "Jya",
          "Asad",
          "Sra",
          "Bha",
          "Asv",
          "Kar",
          "Agr",
          "Pau",
          "Mag",
          "Pha"
        }
      }
    };
    public static Dictionary<string, string[]> CalendarMonthNames = new Dictionary<string, string[]>()
    {
      {
        "hijri",
        new string[12]
        {
          "محرم",
          "صفر",
          "  دربيعا",
          "ربيع٢",
          "جما",
          "جما٢",
          "رجب",
          "شعبان",
          "رمضان",
          "شوال",
          "ذو. ق",
          "ذو. ح"
        }
      },
      {
        "korean-lunar",
        new string[12]
        {
          "정월",
          "여월",
          "가월",
          "초월",
          "매월",
          "계하",
          "교월",
          "계월",
          "현월",
          "개동",
          "설한",
          "극월"
        }
      },
      {
        "hebcal",
        new string[12]
        {
          "תִּשְׁרֵי",
          "חֶשְׁוָן",
          "כִּסְלֵו\u200E",
          "טֵבֵת",
          "שְׁבָט\u200E",
          "אֲדָר",
          "נִיסָן",
          "אִיָּיר",
          "סִיוָן",
          "תַּמּוּז",
          "אָב\u200E",
          "אֱלוּל"
        }
      },
      {
        "shaka",
        new string[12]
        {
          "Chai",
          "Vai",
          "Jya",
          "Asad",
          "Sra",
          "Bha",
          "Asv",
          "Kar",
          "Agr",
          "Pau",
          "Mag",
          "Pha"
        }
      }
    };
    public static Dictionary<string, string[]> CalendarDayNames = new Dictionary<string, string[]>()
    {
      {
        "hijri",
        new string[31]
        {
          "י",
          "ט",
          "ח",
          "ז",
          "ו",
          "ה",
          "ד",
          "ג",
          "ב",
          "א",
          "כ",
          "יט",
          "יח",
          "יז",
          "טז",
          "טו",
          "יד",
          "יג",
          "יב",
          "יא",
          "ל",
          "כט",
          "כח",
          "כז",
          "כו",
          "כה",
          "כד",
          "כג",
          "כב",
          "כא",
          "-"
        }
      },
      {
        "korean-lunar",
        new string[31]
        {
          "1일",
          "2일",
          "3일",
          "4일",
          "5일",
          "6일",
          "7일",
          "8일",
          "9일",
          "10일",
          "11일",
          "12일",
          "13일",
          "14일",
          "15일",
          "16일",
          "17일",
          "18일",
          "19일",
          "20일",
          "21일",
          "22일",
          "23일",
          "24일",
          "25일",
          "26일",
          "27일",
          "28일",
          "29일",
          "30일",
          "31일"
        }
      },
      {
        "hebcal",
        new string[31]
        {
          "١٠",
          "٩",
          "٨",
          "٧",
          "٦",
          "٥",
          "٤",
          "٣",
          "٢",
          "١",
          "٢٠",
          "١٩",
          "١٨",
          "١٧",
          "١٦",
          "١٥",
          "١٤",
          "١٣",
          "١٢",
          "١١",
          "٣٠",
          "٢٩",
          "٢٨",
          "٢٧",
          "٢٦",
          "٢٥",
          "٢٤",
          "٢٣",
          "٢٢",
          "٢١",
          "-"
        }
      }
    };

    public static bool IsValidCalendarType(string calendarType)
    {
      return calendarType.Equals("korean-lunar") || calendarType.Equals("hebcal") || calendarType.Equals("hijri") || calendarType.Equals("shaka") || calendarType.Equals("lunar");
    }

    public static Calendar GetCalendar(string calendarType)
    {
      switch (calendarType)
      {
        case "korean-lunar":
          return (Calendar) new KoreanLunisolarCalendar();
        case "hebcal":
          return (Calendar) new HebrewCalendar();
        case "hijri":
          return (Calendar) new HijriCalendar()
          {
            HijriAdjustment = -1
          };
        case "shaka":
          return (Calendar) new ThaiBuddhistCalendar();
        case "lunar":
          return (Calendar) new ChineseLunisolarCalendar();
        default:
          throw new ArgumentException("Invalid calendar type");
      }
    }

    public static string ConvertDate(DateTime date, Calendar calendar)
    {
      return calendar.ToDateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0).ToString();
    }
  }
}
