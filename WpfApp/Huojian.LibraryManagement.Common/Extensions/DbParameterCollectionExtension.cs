using System.Data.Common;
using System.Data.SQLite;

namespace Huojian.LibraryManagement.Common.Extensions
{
    public static class DbParameterCollectionExtension
    {
        public static DbParameter AddWithValue(this DbParameterCollection dbParameterCollection, string parameterName, object value)
        {
            if (dbParameterCollection is SQLiteParameterCollection sqLiteParameterCollection)
            {
                return sqLiteParameterCollection.AddWithValue(parameterName, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

    }
}