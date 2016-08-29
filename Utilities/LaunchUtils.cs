using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Utilities
{
    public static class LaunchUtils
    {

        public static string getFormattedTime(JToken json, out string date)
        {

            DateTime launch = DateTime.Parse(json["launchtime"].ToString())/*.ToUniversalTime()*/;

            if (Convert.ToBoolean(json["monthonlyeta"]))
            {
                date = launch.ToString("MMMM");
                return "";
            }
            else if (Convert.ToBoolean(json["delayed"]))
            {
                date = String.Format("**No Earlier Than** {0}", launch.ToString("MMM dd, yyyy"));
                return "";
            }
            else {

                TimeSpan span = (launch - DateTime.UtcNow);
                bool past = false;
                if (span.Ticks < 0)
                {
                    past = true;
                    span = span.Duration();
                }

                string time = String.Format("{0}:{1}:{2}", span.Hours.ToString("D2"), span.Minutes.ToString("D2"), span.Seconds.ToString("D2"));
                if (span.Days > 0) time = String.Format("{0} days, {1}", span.Days, time);

                string dateFormat = @"dd MMM yyyy \@ HH:mm:ss \U\T\C";

                date = launch.ToString(dateFormat);
                return String.Format("({0}{1})", past ? "T+" : "L-", time);
            }
        }

        public static string getWindowString(JToken json)
        {

            DateTime winClose = DateTime.Parse(json["windowcloses"].ToString());
            DateTime winOpen = DateTime.Parse(json["windowopens"].ToString());

            string close = getHMSString(winClose);
            string open = getHMSString(winOpen);

            TimeSpan dif = winClose - winOpen;
            string winDif = getHMSString(dif);

            return String.Format("Window: {0}—{1} UTC ({2})\t\t", open, close, winDif);

        }

        public static string getHMSString(TimeSpan t)
        {
            String @string = String.Format("{0:00}:{1:00}:{2:00}", t.TotalHours, t.Minutes, t.Seconds);
            if (@string.EndsWith(":00"))
                @string = @string.Substring(0, @string.Length - 3);
            return @string;
        }

        public static string getHMSString(DateTime dt)
        {
            String @string = String.Format("{0:00}:{1:00}:{2:00}", dt.Hour, dt.Minute, dt.Second);
            if (@string.EndsWith(":00"))
                @string = @string.Substring(0, @string.Length - 3);
            return @string;
        }

    }
}
