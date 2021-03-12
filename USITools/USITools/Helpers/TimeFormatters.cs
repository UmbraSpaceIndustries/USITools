using System;
using System.Text;

namespace USITools
{
    public static class TimeFormatters
    {
        public static string ToTimeSpanString(double seconds, bool alwaysShowSeconds = false)
        {
            var kspFormatter = KSPUtil.dateTimeFormatter;
            var builder = new StringBuilder();
            var remainder = seconds;
            if (remainder >= kspFormatter.Year)
            {
                var years = Math.Truncate(remainder / kspFormatter.Year);
                remainder -= years * kspFormatter.Year;
                builder.AppendFormat("{0:N0}y ", years);
            }
            if (remainder >= kspFormatter.Day)
            {
                var days = Math.Truncate(remainder / kspFormatter.Day);
                remainder -= days * kspFormatter.Day;
                builder.AppendFormat("{0:N0}d ", days);
            }
            else if (remainder < seconds)
            {
                builder.Append("0d ");
            }
            if (remainder >= kspFormatter.Hour)
            {
                var hours = Math.Truncate(remainder / kspFormatter.Hour);
                remainder -= hours * kspFormatter.Hour;
                builder.AppendFormat("{0:N0}h ", hours);
            }
            else if (remainder < seconds)
            {
                builder.Append("0h ");
            }
            if (remainder >= kspFormatter.Minute)
            {
                var minutes = Math.Truncate(remainder / kspFormatter.Minute);
                remainder -= minutes * kspFormatter.Minute;
                builder.AppendFormat("{0:00}m ", minutes);
            }
            else if (remainder < seconds)
            {
                builder.Append("0m ");
            }
            if (seconds < kspFormatter.Hour || alwaysShowSeconds)
            {
                builder.AppendFormat("{0:00}s", remainder);
            }
            return builder.ToString();
        }
    }
}
