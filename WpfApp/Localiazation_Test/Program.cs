using System.Globalization;
using Localiazation_Test.LocalizationResources;
using Microsoft.VisualBasic;

namespace Localiazation_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var chineseCulture = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = chineseCulture;
            Thread.CurrentThread.CurrentCulture = chineseCulture;
            Print();

            //切成英文
            var englishCulture = CultureInfo.GetCultureInfo("en");
            Thread.CurrentThread.CurrentCulture = englishCulture;
            Thread.CurrentThread.CurrentUICulture = englishCulture;
            Print();
        }

        private static void Print()
        {
            var stu = new Student("xiaoming", 19);
            Console.WriteLine(stu.ToString());
        }
    }

    public class Student
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Student(string name,int age)
        {
            Name = name;    
            Age = age;
        }


        public override string ToString()
        {
            return LocalizationResources.Strings.Name+":"+Name+"\t"+
                   LocalizationResources.Strings.Age+":"+Age;
        }
    }
}
