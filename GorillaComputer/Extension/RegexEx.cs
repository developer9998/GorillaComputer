using System.Text.RegularExpressions;

namespace GorillaComputer.Extension
{
    public static class RegexEx
    {
        public static string ToPascalResolveCase(this string name)
        {
            return Regex.Replace(name, "([A-Z])([a-z]*)", " $1$2").TrimStart(' ');
        }

        public static string ToSentenceCase(this string name)
        {
            return Regex.Replace(name.ToLower(), @"(^[a-z])|[?!.:,;]\s+(.)", match => match.Value.ToUpper(), RegexOptions.ExplicitCapture);
        }
    }
}
