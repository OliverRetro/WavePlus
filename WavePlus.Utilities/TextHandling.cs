using System.Globalization;

namespace Plus.Utilities
{
    public static class TextHandling
    {
        // Matches PlusEnvironment.CultureInfo (en-GB); kept local so this assembly stays a leaf.
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

        public static string GetString(double k)
        {
            return k.ToString(Culture);
        }
    }
}