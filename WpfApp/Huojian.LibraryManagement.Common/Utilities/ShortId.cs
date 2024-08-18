using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class ShortId
    {
        private readonly Random _random;
        private readonly string _characters;

        public ShortId()
        {
            _random = new Random();
            _characters = "abcdefghjklmnopqrstuvwxyz0123456789";
        }

        public ShortId(IEnumerable<char> chars)
            : this()
        {
            _characters = new string(chars.ToArray());
        }

        public string Generate(int length)
        {
            var output = new char[length];
            for (var i = 0; i < length; i++)
            {
                var charIndex = _random.Next(0, _characters.Length);
                output[i] = _characters[charIndex];
            }
            return new string(output);
        }
    }

}
