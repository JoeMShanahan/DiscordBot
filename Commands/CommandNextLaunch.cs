using Discord;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

namespace DiscordBot.Commands
{
    public class CommandNextLaunch : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            e.Channel.SendIsTyping();
            Thread t = new Thread(new ThreadStart(() => runThread(e, pub)));
            t.Name = "LaunchBot NextLaunch Thread";
            t.IsBackground = true;
            t.Start();
        }

        private void runThread(MessageEventArgs e, bool pub)
        {
            try {
                string data = "";
                using (WebClient c = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                    ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
                    data = c.DownloadString("http://ipeer.auron.co.uk/launchschedule/api/1/launches?limit=1&omitapidata=1"); // This causes a fatal exception, and I can't fix it, so this entire project is dead. \o/
                }

                JObject _json = JObject.Parse(data);
                JToken json = _json["launches"][0];
                string vehicle = json["vehicle"].ToString();
                string payload = json["payload"].ToString();

                DateTime launch = DateTime.Parse(json["launchtime"].ToString())/*.ToUniversalTime()*/;
                TimeSpan span = (launch - DateTime.UtcNow);

                string time = String.Format("{0}:{1}:{2}", span.Hours.ToString("D2"), span.Minutes.ToString("D2"), span.Seconds.ToString("D2"));
                if (span.Days > 0) time = String.Format("{0} days, {1}", span.Days.ToString("D2"), time);

                string dateFormat = @"dd MMM yyyy \@ HH:mm:ss \U\T\C";

                string final = String.Format("{0}/{1} — {2} ({3})", vehicle, payload, launch.ToString(dateFormat), time);

                e.Channel.SendMessage(final);
            }
            catch (Exception _e)
            {
                e.Channel.SendMessage("Couldn't fetch data because an orror occurred. If it persists, please inform iPeer and show him this: `" + _e.ToString() + " // " + e.Message);
            }
        }
    }
}
