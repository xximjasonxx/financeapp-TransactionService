
using System.Text.RegularExpressions;

namespace TransactionService.Extensions
{
    public static class StringExtensions
    {
        public static string AsJwtToken(this string rawToken)
        {
            Regex regex = new Regex(@"^Bearer ", RegexOptions.Compiled);
            return regex.Replace(rawToken, string.Empty);
        }
    }
}