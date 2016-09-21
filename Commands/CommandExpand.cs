using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;
using Newtonsoft.Json;
using DiscordBot.Logging;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{

    public class ExpandEntry
    {
        public string Abbr { get; private set; }
        public List<string> Values { get; private set; }
        public ExpandEntry(string abbr, List<string> values)
        {
            this.Abbr = abbr;
            this.Values = values;
        }
    }

    public class CommandExpand : CommandBase, ICommand
    {

        private List<ExpandEntry> definitions = new List<ExpandEntry>();
        private Logger Logger;

        public override void initialise()
        {
            this.Logger = CommandManager.Instance.Logger.createSubLogger("CommandExpand");
            string[] jsonURLs = new string[] { "http://www.decronym.xyz/acronyms/SpaceX.json", "http://www.decronym.xyz/acronyms/ULA.json", "http://www.decronym.xyz/acronyms/Spaceflight.json", "http://www.decronym.xyz/acronyms/Space.json" };
            foreach (string url in jsonURLs)
            {
                this.Logger.Log("Loading definitions from {0}...", url);
                string json = Utils.getWebPage(url);
                Dictionary<string, List<string>> d = new Dictionary<string, List<string>>(JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json));
                foreach(KeyValuePair<string, List<string>> kvp in d)
                {
                    if (this.definitions.Any(a => a.Abbr.EqualsIgnoreCase(kvp.Key))) { continue; }
                    ExpandEntry e = new ExpandEntry(kvp.Key, kvp.Value);
                    this.definitions.Add(e);
                }
            }
            this.Logger.Log("Loaded {0} definitions from {1} URLs", this.definitions.Count, jsonURLs.Length);
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            try
            {
                string abbr = e.Message.Text.FromNthDeliminator(1, ' ');
                List<ExpandEntry> valid = this.definitions.FindAll(a => a.Abbr.EqualsIgnoreCase(abbr));
                StringBuilder sb = new StringBuilder();
                if (valid.Count == 0)
                    sb.AppendFormattedLine("{0}, I didn't find any matches for '{1}'", e.User.Mention, abbr);
                else if (valid.Count > 1)
                {
                    int x = 0;
                    StringBuilder rsb = new StringBuilder();
                    int results = 0;
                    foreach (ExpandEntry ee in valid)
                    {
                        if (x++ > 1)
                            rsb.AppendLine();
                        foreach (string s in ee.Values)
                        {
                            results++;
                            string str = s;
                            bool markup = s.StartsWith("[");
                            if (markup)
                                str = s.Substring(1).Replace("](", " (");
                            rsb.AppendFormattedLine("\t\t{0}", str);
                        }
                    }
                    sb.AppendFormattedLine("{0}, I found {1} enties with {3} {4} for '**{2}**':", e.User.Mention, valid.Count, abbr, results, results == 1 ? "result" : "results");
                    sb.Append(rsb);
                    //sb.Append("```");
                }
                else
                {
                    sb.AppendFormattedLine("{0}, I found 1 entry with {3} {4} for '**{2}**':", e.User.Mention, valid.Count, abbr, valid.First().Values.Count, valid.First().Values.Count == 1 ? "result" : "results");
                    foreach (string s in valid.First().Values)
                    {
                        string str = s;
                        bool markup = s.StartsWith("[");
                        if (markup)
                            str = s.Substring(1).Replace("](", " (");
                        sb.AppendFormattedLine("\t\t{0}", str);
                    }
                    //sb.Append("```");
                }
                e.Channel.SendMessage(sb.ToString());
            }
            catch (IndexOutOfRangeException) { e.Channel.SendMessageFormatted("{0}, you need to provide an abbreviation to look up.", e.User.Mention); }
        }
    }
}
