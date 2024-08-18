using System.Globalization;

namespace ShadowBot.Common.Utilities
{
    public static class DateTimeHelper
    {
        private static readonly DateTime _baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static double DateTimeToJavaTimeStamp(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime() - _baseDateTime).TotalMilliseconds;
        }

        public static double DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime() - _baseDateTime).TotalSeconds;
        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            return _baseDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            return _baseDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public static string DateTimeToNormal(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception e)
            {
            }

            return dateTime.ToString();
        }

        public static string DateTimeNowString()
        {
            return DateTimeToNormal(DateTime.Now);
        }

    }

    public static class DateTimeExtensions
    {
        private readonly static string _justNow = $"{Strings.DateTimeHelper_Just}";
        public static String TimeAgo(this DateTime dateTime)
        {
            return dateTime.TimeAgo(CultureInfo.CurrentUICulture);
        }

        public static String TimeAgo(this DateTime dateTime, CultureInfo cultureInfo)
        {
            return dateTime.TimeAgo(DateTime.Now, cultureInfo);
        }

        public static String TimeAgo(this DateTime dateTime, DateTime relativeTo, CultureInfo cultureInfo)
        {
            var timeSpan = relativeTo - dateTime;
            var dateTimeFormatStrings = LanguageFormatStrings.SimplifiedChinese;
            if (timeSpan.Days > 365)
            {
                var years = (timeSpan.Days / 365);
                if (timeSpan.Days % 365 != 0)
                    years += 1;
                return String.Format(years == 1 ? dateTimeFormatStrings.YearAgo : dateTimeFormatStrings.YearsAgo, years);
            }
            if (timeSpan.Days > 30)
            {
                var months = (timeSpan.Days / 30);
                if (timeSpan.Days % 31 != 0)
                    months += 1;
                return String.Format(months == 1 ? dateTimeFormatStrings.MonthAgo : dateTimeFormatStrings.MonthsAgo, months);
            }
            if (timeSpan.Days > 0)
                return String.Format(timeSpan.Days == 1 ? dateTimeFormatStrings.DayAgo : dateTimeFormatStrings.DaysAgo, timeSpan.Days);
            if (timeSpan.Hours > 0)
                return String.Format(timeSpan.Hours == 1 ? dateTimeFormatStrings.HourAgo : dateTimeFormatStrings.HoursAgo, timeSpan.Hours);
            if (timeSpan.Minutes > 0)
                return String.Format(timeSpan.Minutes == 1 ? dateTimeFormatStrings.MinuteAgo : dateTimeFormatStrings.MinutesAgo, timeSpan.Minutes);
            if (timeSpan.Seconds >= 0)
                return _justNow;
            return "--";
        }
    }
    internal static class LanguageFormatStrings
    {
        internal class VerifiedByNativeSpeakerAttribute : Attribute { }
        private static DateTimeFormatStrings simplifiedChinese;
        [VerifiedByNativeSpeaker]
        public static DateTimeFormatStrings SimplifiedChinese
        {
            get
            {
                return simplifiedChinese ?? (simplifiedChinese = new DateTimeFormatStrings
                {
                    SecondAgo = $"{Strings.DateTimeHelper_SecondsAgo}",
                    SecondsAgo = $"{Strings.DateTimeHelper_SecondsAgo}",
                    MinuteAgo = $"{Strings.DateTimeHelper_MinutesAgo}",
                    MinutesAgo = $"{Strings.DateTimeHelper_MinutesAgo}",
                    HourAgo = $"{Strings.DateTimeHelper_HoursAgo}",
                    HoursAgo = $"{Strings.DateTimeHelper_HoursAgo}",
                    DayAgo = $"{Strings.DateTimeHelper_DaysAgo}",
                    DaysAgo = $"{Strings.DateTimeHelper_DaysAgo}",
                    MonthAgo = $"{Strings.DateTimeHelper_MonthsAgo}",
                    MonthsAgo = $"{Strings.DateTimeHelper_MonthsAgo}",
                    YearAgo = $"{Strings.DateTimeHelper_YearsAgo}",
                    YearsAgo = $"{Strings.DateTimeHelper_YearsAgo}",
                });
            }
        }
    }
    internal class DateTimeFormatStrings
    {
        public String SecondAgo;
        public String SecondsAgo;
        public String MinuteAgo;
        public String MinutesAgo;
        public String HourAgo;
        public String HoursAgo;
        public String DayAgo;
        public String DaysAgo;
        public String MonthAgo;
        public String MonthsAgo;
        public String YearAgo;
        public String YearsAgo;
    }

}
