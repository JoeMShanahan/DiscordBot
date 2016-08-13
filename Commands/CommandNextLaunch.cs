using Discord;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;

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
            string data = "";
            using (WebClient c = new WebClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
                data = c.DownloadString("https://ipeer.auron.co.uk/launchschedule/api/1/launches?limit=1&omitapidata=1"); // This causes a fatal exception, and I can't fix it, so this entire project is dead. \o/
            }
            JObject json = JObject.Parse(data);
            string vehicle = json["launches"][0]["vehicle"].ToString();
            e.Channel.SendMessage(vehicle);
        }
    }
}
