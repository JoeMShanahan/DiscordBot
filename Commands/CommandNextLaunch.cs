using Discord;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using DiscordBot.Utilities;
using DiscordBot.Extensions;
using System.Text;
using System.Collections.Generic;

namespace DiscordBot.Commands
{
    public class CommandNextLaunch : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            e.Channel.SendIsTyping();
            Thread t = new Thread(new ThreadStart(() => runThread(e, pub, false, fromPhrase)));
            t.Name = "LaunchBot NextLaunch Thread";
            t.IsBackground = true;
            t.Start();
        }

        private void runThread(MessageEventArgs e, bool pub, bool lastLaunch = false, bool fromPhrase = false)
        {

            bool search = false;
            string searchText = string.Empty;

            if (e.Message.Text.Split(' ').Length > 1 && !fromPhrase)
            {
                search = true;
                searchText = e.Message.Text.Substring(e.Message.Text.Split(' ')[0].Length + 1);
            }

            try {

                string url = String.Format("https://ipeer.auron.co.uk/launchschedule/api/1/launches?cutoff="+Utils.getEpochTime()+"&limit=1&omitapidata=1{0}", (lastLaunch ? "&history=1" : ""));
                if (search)
                    url = String.Format("https://ipeer.auron.co.uk/launchschedule/api/1/launches?cutoff="+Utils.getEpochTime()+"&omitapidata=1{0}&noreturnlimit=1", (lastLaunch ? "&history=1" : ""));
                JObject _json = JObject.Parse(Utils.getWebPage(url));

                StringBuilder b = new StringBuilder();

                JToken json = null;
                if (search)
                {
                    // In what world does a third-party library not have predicate search functions?
                    JToken _tmp = _json["launches"];
                    foreach (JToken j in _tmp)
                    {
                        if (j["vehicle"].ToString().ToLower().ContainsIgnoreCase(searchText) || (j["vehicle"].ToString().ToLower().ContainsIgnoreCase("super strypi") && searchText.EqualsIgnoreCase("oops")) /* Everyone loves a good easter egg */)
                        {
                            json = j;
                            break;
                        }
                    }
					
                    if (json == null)
                    {
                        e.Channel.SendMessage(String.Format("Sorry {0}, I couldn't find any rockets that matched the search text '{1}'.", e.User.Mention, searchText));
                        return;
                    }
                    else
                    {
                        b.AppendFormattedLine(String.Format("{0}, the {1} launch match for '{2}' is:", e.User.Mention, lastLaunch ? "last" : "next", searchText));
                    }
                }
                else
                {
                    json = _json["launches"][0];
                }
                string vehicle = json["vehicle"].ToString();
                string payload = json["payload"].ToString();

                string location = json["location"].ToString();

                string date;
                string timeStamp = LaunchUtils.getFormattedTime(json, out date);
                string final = String.Format("{0}/{1} — {2} {3}", vehicle, payload, date, timeStamp);
                b.AppendFormattedLine((search ? "\t" : "") + final);
                if (search)
                    b.Append("\t");
                if (json["hasTags"].ToObject<bool>())
                {
                    b.Append("Tags: ");
                    List<Dictionary<string, string>> tags = new List<Dictionary<string, string>>(json["tags"].ToObject<List<Dictionary<string, string>>>());
                    int tIn = 0;
                    foreach (Dictionary<string, string> dic in tags)
                    {
                        if (tIn++ >= 1)
                            b.Append(", ");
                        b.AppendFormat("**{0}**", dic["text"]);
                    }
                    b.AppendLine();
                    if (search)
                        b.Append("\t");
                }
                if ((!json["delayed"].ToObject<bool>() && !json["monthonlyeta"].ToObject<bool>()) && json["windowcloses"] != null && json["windowopens"] != json["windowcloses"])
                {
                    string windowData = LaunchUtils.getWindowString(json);
                    b.AppendFormat("{0}\t\t", windowData);
                }
                b.AppendFormat("From {0}", location);

                e.Channel.SendMessage(b.ToString());
            }
            catch (Exception _e)
            {
                e.Channel.SendMessage("Couldn't fetch data because an error occurred. If it persists, please inform iPeer and show him this: `" + _e.ToString() + " // " + e.Message + "`");
            }
        }

        public void customInvoke(MessageEventArgs e, bool pub, bool fromPhrase = false) // Writing an essential duplicate of this command just to do the last launch (instead of next) wasn't really a good idea, so I'm hacking it in like any sensible programmer
        {

            e.Channel.SendIsTyping();
            Thread t = new Thread(new ThreadStart(() => runThread(e, pub, true, fromPhrase)));
            t.Name = "LaunchBot NextLaunch Thread";
            t.IsBackground = true;
            t.Start();

        }

        public override string triggerPattern()
        {
            return @"%me%,? (when|what)('s| is) the next (rocket )?launch\??";
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "nl" };
        }

        public override string helpText()
        {
            return "If _search_ is not provided this command will return information on the next rocket launch. If _search_ is provided, it will return information on the first upcoming launch (if any) that matches _search_";
        }

        public override string usageText()
        {
            return "%c% [search]";
        }
    }
}
