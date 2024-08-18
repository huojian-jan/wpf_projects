using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Huojian.LibraryManagement.Common.Extensions
{
    public static class CommandLineExtension
    {
        //让Environment.GetCommandLineArgs()及Environment.CommandLine的返回值和在.Net Framework中一样
        static bool called_Args0UseExeSuffixLikeInDotNetFramework;
        public static void Args0UseExeSuffixLikeInDotNetFramework()
        {
            if (!called_Args0UseExeSuffixLikeInDotNetFramework)
            {
                called_Args0UseExeSuffixLikeInDotNetFramework = true;

                Type environment = typeof(Environment);
                MethodInfo Environment_SetCommandLineArgs = environment.GetMethod("SetCommandLineArgs", BindingFlags.Static | BindingFlags.NonPublic);
                Environment_SetCommandLineArgs.Invoke(null, new object[] { null });
            }
        }

        public static ParserResult<T> ThrowOnParseError<T>(this ParserResult<T> result)
        {
            // https://github.com/commandlineparser/commandline/issues/513#issuecomment-576585612
            if (result.Tag == ParserResultType.NotParsed)
            {
                var builder = SentenceBuilder.Create();
                var parserError = HelpText.RenderParsingErrorsText(result,
                    builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
                throw new ArgumentException(parserError);
            }
            else
            {
                return result;
            }
        }
    }
}