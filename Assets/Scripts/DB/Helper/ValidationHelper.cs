//Author: Tamer Erdoğan

using System.Text.RegularExpressions;

namespace DB.Helper
{
    public class ValidationHelper
    {
        public static bool IsValidLength(string value, int min = 2, int max = 255)
        {
            return !(value.Length <= min || value.Length >= max);
        }

        public static bool IsValidEmail(string value)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(value, emailPattern, RegexOptions.IgnoreCase);
        }
    }
}
