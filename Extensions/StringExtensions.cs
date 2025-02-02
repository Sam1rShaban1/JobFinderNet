using System;

namespace JobFinderNet.Extensions
{
    public static class StringExtensions
    {
        public static string Shorten(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;

            return str.Substring(0, maxLength) + "...";
        }
    }
} 