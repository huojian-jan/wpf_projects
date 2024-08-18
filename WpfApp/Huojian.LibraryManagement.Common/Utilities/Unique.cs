using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public static class Unique
    {
        public static string NewName(string prefix, IEnumerable<string> exclude, bool alwaysWithSuffix = false)
        {
            int max = 0;
            foreach (var name in exclude)
            {
                if (name == null)
                    continue;

                if (name == prefix)
                {
                    if (max == 0)
                        max = 1;
                }
                else
                {
                    if (!name.StartsWith(prefix))
                        continue;
                    var subfix = name.Substring(prefix.Length);
                    if (!int.TryParse(subfix, out int num))
                        continue;
                    if (num > max)
                        max = num;
                }
            }

            if (max == 0 && !alwaysWithSuffix)
                return prefix;
            else
                return prefix + (max + 1);
        }

        public static string Rename(string originalName, IEnumerable<string> existingNames)
        {
            if (!existingNames.Contains(originalName))
            {
                return originalName;
            }

            int index = 1;
            string newName;

            do
            {
                newName = string.Concat(originalName, "_", index);
                index++;
            } while (existingNames.Contains(newName));

            return newName;
        }
    }
}
