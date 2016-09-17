using Discord;
using DiscordBot.Extensions;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Fun
{

    public class NoSuchFunModuleException : Exception { }

    public class FunManager
    {

        public static FunManager Instance { get; private set; }
        public Logger Logger { get; private set; }

        private List<IFunModule> _modules = new List<IFunModule>();

        public FunManager()
        {
            Instance = this;
            this.Logger = new Logger("FunManager");
            this.LoadModules();
        }

        public void LoadModules()
        {
            this.Logger.Log("Loading Fun Modules...");
            if (this._modules.Count > 0)
                this._modules.Clear();
            string[] blacklistedModuleClasses = new string[] { "FunModuleBase" };
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            Type[] _toInit = types.Where(a => (a.Name.StartsWith("FunModule") && !blacklistedModuleClasses.Contains(a.Name))).ToArray();
            this.Logger.Log("{0} module(s) to initialise", _toInit.Length);
            foreach (Type t in _toInit)
            {
                this.Logger.Log("Initialising module '{0}'", t.FullName);
                IFunModule c = (IFunModule)Activator.CreateInstance(t);
                this._modules.Add(c);
            }
        }

        public T getFunModuleWithName<T>(string name)
        {
            IFunModule m = getFunModuleWithName(name);
            return (T)m;
        }

        public IFunModule getFunModuleWithName(string name)
        {
            IFunModule fm = this._modules.First(m => m.ToString().EndsWithIgnoreCase(name));
            if (fm == null)
                throw new NoSuchFunModuleException();
            return fm;
        }

        public void onMessageReceived(MessageEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onMessageReceived(e);
        }

        public void onMessageUpdated(MessageUpdatedEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onMessageUpdated(e);
        }

        public void onDMReceived(MessageEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onDMReceived(e);
        }

        public void onServerJoined(ServerEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onServerJoined(e);
        }

        public void onServerLeft(ServerEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onServerLeft(e);
        }

        public void onUserUpdate(UserUpdatedEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onUserUpdate(e);
        }

        public void onUserLeft(UserEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onUserLeft(e);
        }

        public void onUserJoined(UserEventArgs e)
        {
            foreach (IFunModule f in this._modules)
                f.onUserJoined(e);
        }

        public void onBotTerminating()
        {
            foreach (IFunModule f in this._modules)
                f.onBotTerminating();
        }
    }
}
