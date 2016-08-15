using Discord;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using DiscordBot.Utilities;
using DiscordBot.Extensions;

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

        private void runThread(MessageEventArgs e, bool pub, bool lastLaunch = false)
        {

            bool search = false;
            string searchText = string.Empty;

            if (e.Message.Text.Split(' ').Length > 1)
            {
                search = true;
                searchText = e.Message.Text.Substring(e.Message.Text.Split(' ')[0].Length + 1);
            }

            try {

                string url = String.Format("https://ipeer.auron.co.uk/launchschedule/api/1/launches?limit=1&omitapidata=1{0}", (lastLaunch ? "&history=1" : ""));
                if (search)
                    url = String.Format("https://ipeer.auron.co.uk/launchschedule/api/1/launches?omitapidata=1{0}&noreturnlimit=1", (lastLaunch ? "&history=1" : ""));
                JObject _json = JObject.Parse(Utils.getWebPage(url));


                JToken json = null;
                if (search)
                {
                    // In what world does a third-party library not have predicate search functions?
                    JToken _tmp = _json["launches"];
                    foreach (JToken j in _tmp)
                    {
                        if (j["vehicle"].ToString().ToLower().Contains(searchText) || (j["vehicle"].ToString().ToLower().Contains("super strypi") && searchText.EqualsIgnoreCase("oops")) /* Everyone loves a good easter egg */)
                        {
                            json = j;
                            break;
                        }
                    }
					
					// TODO: Put this command reply into one line to ease server admin's jobs if they want to remove it.
					
                    if (json == null)
                    {
                        e.Channel.SendMessage(String.Format("Sorry {0}, I couldn't find any rockets that matched the search text '{1}'.", e.User.Mention, searchText));
                        return;
                    }
                    else
                    {
                        e.Channel.SendMessage(String.Format("{0}, the {1} launch match for '{2}' is:", e.User.Mention, lastLaunch ? "last" : "next", searchText));
                    }
                }
                else
                {
                    json = _json["launches"][0];
                }
                string vehicle = json["vehicle"].ToString();
                string payload = json["payload"].ToString();

                string date;
                string timeStamp = LaunchUtils.getFormattedTime(json, out date);
                string final = String.Format("{0}/{1} — {2} {3}", vehicle, payload, date, timeStamp);

                e.Channel.SendMessage((search?"\t":"")+final);
            }
            catch (Exception _e)
            {
                e.Channel.SendMessage("Couldn't fetch data because an orror occurred. If it persists, please inform iPeer and show him this: `" + _e.ToString() + " // " + e.Message);
            }
        }

        public void customInvoke(MessageEventArgs e, bool pub) // Writing an essential duplicate of this command just to do the last launch (instead of next) wasn't really a good idea, so I'm hacking it in like any sensible programmer
        {

            e.Channel.SendIsTyping();
            Thread t = new Thread(new ThreadStart(() => runThread(e, pub, true)));
            t.Name = "LaunchBot NextLaunch Thread";
            t.IsBackground = true;
            t.Start();

        }
    }
}
