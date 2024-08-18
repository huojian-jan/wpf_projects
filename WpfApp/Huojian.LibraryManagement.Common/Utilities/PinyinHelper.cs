using ShadowBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class PinyinHelper
    {
        private static string[] _pinyinList = new string[] { "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng",
                                                               "bi", "bian", "biao", "bie", "bin", "bing", "bo", "bu", "ca", "cai", "can", "cang",
                                                               "cao", "ce", "ceng", "cha", "chai", "chan", "chang", "chao", "che", "chen", "cheng",
                                                               "chi", "chong", "chou", "chu", "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci",
                                                               "cong", "cou", "cu", "cuan", "cui", "cun", "cuo", "da", "dai", "dan", "dang", "dao", "de",
                                                               "deng", "di", "dian", "diao", "die", "ding", "diu", "dong", "dou", "du", "duan", "dui",
                                                               "dun", "duo", "e", "en", "er", "fa", "fan", "fang", "fei", "fen", "feng", "fo", "fou",
                                                               "fu", "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong", "gou", "gu",
                                                               "gua", "guai", "guan", "guang", "gui", "gun", "guo", "ha", "hai", "han", "hang", "hao",
                                                               "he", "hei", "hen", "heng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui",
                                                               "hun", "huo", "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", "jiu",
                                                               "ju", "juan", "jue", "jun", "ka", "kai", "kan", "kang", "kao", "ke", "ken", "keng", "kong",
                                                               "kou" ,"ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan", "lang",
                                                               "lao", "le", "lei", "leng", "li", "lia", "lian", "liang", "liao", "lie", "lin", "ling", "liu",
                                                               "long", "lou", "lu", "lv", "luan", "lue", "lun", "luo", "ma", "mai", "man", "mang", "mao", "me",
                                                               "mei", "men", "meng", "mi", "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu", "na",
                                                               "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ni", "nian", "niang", "niao", "nie",
                                                               "nin", "ning", "niu", "nong", "nu", "nv", "nuan", "nue", "nuo", "o", "ou", "pa", "pai", "pan",
                                                               "pang", "pao", "pei", "pen", "peng", "pi", "pian", "piao", "pie", "pin", "ping", "po", "pu",
                                                               "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu", "qu", "quan",
                                                               "que", "qun", "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", "ruan",
                                                               "rui", "run", "ruo", "sa", "sai", "san", "sang", "sao", "se", "sen", "seng", "sha", "shai",
                                                               "shan", "shang", "shao", "she", "shen", "sheng", "shi", "shou", "shu", "shua", "shuai", "shuan",
                                                               "shuang", "shui", "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun", "suo",
                                                               "ta", "tai", "tan", "tang", "tao", "te", "teng", "ti", "tian", "tiao", "tie", "ting", "tong",
                                                               "tou", "tu", "tuan", "tui", "tun", "tuo", "wa", "wai", "wan", "wang", "wei", "wen", "weng",
                                                               "wo", "wu", "xi", "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu", "xu",
                                                               "xuan", "xue", "xun", "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", "yong",
                                                               "you", "yu", "yuan", "yue", "yun", "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng",
                                                               "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu",
                                                               "zhua", "zhuai", "zhuan", "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", "zuan",
                                                               "zui", "zun", "zuo" };
        private static Dictionary<char, List<string>> _pinyinDict;

        private const string BLANK = " ";
        private const string NON_CHAR_PATTERN = "[^A-Z|a-z]";
        private const int PINYIN_MAX_LENGHT = 6;
        private const string NON_SPECIAL_WORD =  "[a-zA-Z0-9\u4e00-\u9fa5]+";

        static PinyinHelper()
        {
            _pinyinDict = new Dictionary<char, List<string>>();
            foreach (var value in _pinyinList)
            {
                if (_pinyinDict.ContainsKey(value[0]))
                    _pinyinDict[value[0]].Add(value);
                else
                {
                    var values = new List<string> { value };
                    _pinyinDict[value[0]] = values;
                }
            }
        }

        public static string GetPinyin(string chinese)
        {
            var pinyinArray = string.Empty;
            if (string.IsNullOrWhiteSpace(chinese))
                return pinyinArray;

            var builder = new StringBuilder();
            foreach (var token in chinese)
            {
                if (!TinyPinyin.PinyinHelper.IsChinese(token))
                    builder.Append(token);
                else
                    builder.Append(" " + TinyPinyin.PinyinHelper.GetPinyin(token) + " ");
            }

            var array = builder.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            pinyinArray = string.Join(" ", array);
            return pinyinArray;
        }

        public static string GetPinyinInitials(string chinese)
        {
            try
            {
                var regex = new Regex(NON_SPECIAL_WORD);
                var result = regex.Matches(chinese);
                chinese = string.Empty;
                foreach (var item in result)
                    chinese += item;
                return TinyPinyin.PinyinHelper.GetPinyinInitials(chinese);
            }
            catch (Exception ex)
            {
                Logging.Warn(ex);
                return "";
            }
        }

        public static string PinyinCheck(string pinyin)
        {
            Regex rgx = new Regex(NON_CHAR_PATTERN);
            pinyin = rgx.Replace(pinyin, BLANK).Trim().ToLower();
            var pinyinArray = pinyin.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var pinyins = new List<string>();
            foreach (var value in pinyinArray)
                pinyins.AddRange(PinyinSplit(value));

            return string.Join(" ", pinyins);
        }

        public static Regex GetPinyinRegex(string pinyin)
        {
            var array = pinyin.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length > 0)
            {
                var pattern = string.Join(@"\s*", array);
                return new Regex($@"(\s+{pattern})|(^{pattern})", RegexOptions.IgnoreCase);
            }
            else
                return null;
        }

        private static List<string> PinyinSplit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var pinyins = new List<string>();
            do
            {
                var pinyin = value;
                if (value.Length > PINYIN_MAX_LENGHT)
                    pinyin = value.Substring(0, PINYIN_MAX_LENGHT);
                var temp = pinyin;
                int index = pinyin.Length;
                while (true)
                {
                    var key = pinyin[0];
                    if (!_pinyinDict.ContainsKey(key))
                        break;

                    var result = _pinyinDict[key].FirstOrDefault(m => m == pinyin);
                    if (result != null)
                    {
                        pinyins.Add(result);
                        break;
                    }

                    pinyin = pinyin.Substring(0, pinyin.Length - 1);
                    index = pinyin.Length;
                    if (string.IsNullOrEmpty(pinyin))
                    {
                        pinyins.Add(temp);
                        index = temp.Count();
                        break;
                    }
                }
                if (value.Length - 1 <= index)
                    break;
                value = value.Substring(index, value.Length - index);

            } while (true);
            return pinyins;
        }
    }
}
