using System.Data.Common;

namespace Huojian.LibraryManagement.Common.Extensions
{
    public static class DbDataReaderExtension
    {
        public static string ReadString(this DbDataReader reader, int ordinal, string defaultValue = default)
        {
            if (reader.IsDBNull(ordinal))
                return defaultValue;
            else
            {
                var value = reader.GetValue(ordinal);
                return value == null ? default : value.ToString();
            }
        }

        public static Int64 ReadInt64(this DbDataReader reader, int ordinal, Int64 defaultValue = default)
        {
            if (reader.IsDBNull(ordinal))
                return defaultValue;
            else
                return reader.GetInt64(ordinal);
        }

        public static int ReadInt32(this DbDataReader reader, int ordinal, int defaultValue = default)
        {
            if (reader.IsDBNull(ordinal))
                return defaultValue;
            else
                return reader.GetInt32(ordinal);
        }

        public static bool ReadBoolean(this DbDataReader reader, int ordinal, bool defaultValue = default)
        {
            if (reader.IsDBNull(ordinal))
                return defaultValue;
            else
                return reader.GetBoolean(ordinal);
        }

        public static DateTime ReadDateTime(this DbDataReader reader, int ordinal, DateTime defaultValue = default)
        {
            if (reader.IsDBNull(ordinal))
                return defaultValue;
            else
            {
                var timeString = reader.GetString(ordinal);
                if (DateTime.TryParse(timeString, out DateTime result))
                    return result;
                else
                    return defaultValue;
            }
        }

        public static TEnum ReadEnum<TEnum>(this DbDataReader reader, int ordinal, TEnum defaultValue = default)
            where TEnum : struct
        {
            if (reader.IsDBNull(ordinal))
                return defaultValue;
            else
            {
                var value = reader.GetInt32(ordinal);
                if (Enum.IsDefined(typeof(TEnum), value))
                    return (TEnum)Enum.ToObject(typeof(TEnum), value);
                else
                    return defaultValue;
            }
        }
    }
}
