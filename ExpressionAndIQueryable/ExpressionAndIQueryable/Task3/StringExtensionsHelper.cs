namespace Sample03
{
    public static class StringExtensionsHelper
    {
        public static string EndsWith(this string source, string condition)
        {
            return $"*{condition}";
        }

        public static string StartsWith(this string source, string condition)
        {
            return $"{condition}*";
        }

        public static string Contains(this string source, string condition)
        {
            return $"*{condition}*";
        }
    }
}

