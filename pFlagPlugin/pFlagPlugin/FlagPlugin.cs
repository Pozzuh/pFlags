using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Addon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pFlagPlugin
{
    public class FlagPlugin : CPlugin
    {
        public const int FLAG_MSG_GRACE = 12; // 12/4=3 seconds
        PluginClient[] PluginClients = new PluginClient[18];
        bool flagsEnabled = false;
        List<Flag> flags = new List<Flag>();
        /*int fx_flagbase_black;
        int fx_flagbase_gold;
        int fx_flagbase_red;
        int fx_flagbase_silver;*/

        public override void OnServerLoad()
        {
            ServerPrint("pFlags loaded. Author: Pozzuh. Version 1.2");
        }

        public override void OnMapChange()
        {
            setupFlagData();
            setupFlagIngame();
        }

        public override void OnFastRestart()
        {
            setupFlagData();
            setupFlagIngame();
        }

        public override void OnPreMapChange()
        {
            flagsEnabled = false;
            flags.Clear();
        }

        public override void OnPlayerConnect(ServerClient Client)
        {
            PluginClients[Client.ClientNum] = new PluginClient(Client.ClientNum);
        }

        public override void OnPlayerDisconnect(ServerClient Client)
        {
            PluginClients[Client.ClientNum] = null;
        }

        /*public override void OnPrecache()
        {
            fx_flagbase_black = Addon.Extensions.LoadFX(@"misc/ui_flagbase_black");
            fx_flagbase_gold = Addon.Extensions.LoadFX(@"misc/ui_flagbase_gold");
            fx_flagbase_red = Addon.Extensions.LoadFX(@"misc/ui_flagbase_red");
            fx_flagbase_silver = Addon.Extensions.LoadFX(@"misc/ui_flagbase_silver");
        }*/

        void setupFlagData()
        {
            flagsEnabled = false;
            flags.Clear();

            string mapname = GetDvar("mapname");
            if (!File.Exists(@"addon/pFlags/" + mapname + ".json"))
            {
                ServerPrint("pFlags: No flags for this map found.");
                return;
            }
           
            string sData;
            
            using(StreamReader sr = new StreamReader(@"addon/pFlags/" + mapname + ".json"))
            {
                sData = sr.ReadToEnd();
                sr.Close();
            }

            if (sData == null)
            {
                ServerPrint("pFlags: Flags for this map could not be loaded.");
                return;
            }

            JArray jsonData = JArray.Parse(sData);

            foreach (var item in jsonData.Children())
            {
                try
                {
                    int x1 = (int)item.SelectToken("position_in.x");
                    int y1 = (int)item.SelectToken("position_in.y");
                    int z1 = (int)item.SelectToken("position_in.z");

                    int x2 = (int)item.SelectToken("position_out.x");
                    int y2 = (int)item.SelectToken("position_out.y");
                    int z2 = (int)item.SelectToken("position_out.z");

                    bool bothWays = (bool)item.SelectToken("bothways");
                    bool hideOut = (bool)item.SelectToken("hide_out");
                    bool autoUseIn = (bool)item.SelectToken("auto_use_in");

                    string modelIn = (string)item.SelectToken("model_in");
                    string modelOut = (string)item.SelectToken("model_out");
                    Flag f = new Flag(x1, y1, z1, x2, y2, z2);
                    f.modelIn = modelIn;
                    f.modelOut = modelOut;
                    f.bothWays = bothWays;
                    f.hideOut = hideOut;
                    f.autoUseIn = autoUseIn;

                    flags.Add(f);
                    flagsEnabled = true;
                }
                catch
                {
                    ServerPrint("pFlags: something went wrong, not all flags were correctly loaded.");
                }
            }           
        }

        void setupFlagIngame()
        {
            if (!flagsEnabled)
                return;

            for(int i = 0; i < flags.Count; i++)
            {
                //prop_flag_delta, prop_flag_speznas, prop_flag_neutral
                Entity eIn = SpawnModel("script_brushmodel", flags[i].modelIn, flags[i].positionIn);
                Entity eOut = SpawnModel("script_brushmodel", flags[i].modelOut, flags[i].positionOut);

                if (flags[i].hideOut)
                    Addon.Extensions.Hide(eOut);

                Random random = new Random(); //WOW MAKES IT FEEL SO ALIVE
                Addon.Extensions.SetAngles(eIn, new Vector(0, random.Next(360), 0));
                Addon.Extensions.SetAngles(eOut, new Vector(0, random.Next(360), 0));

                flags[i].entityIn = eIn;
                flags[i].entityOut = eOut;
            }
        }

        public override void OnAddonFrame()
        {
            if (flagsEnabled)
            {
                if (GetClients() != null)
                {
                    foreach (ServerClient Client in GetClients())
                    {
                        if (Client == null)
                            continue;

                        if (Client.ConnectionState != ConnectionStates.Connected)
                            continue;

                        if (PluginClients[Client.ClientNum].msgGrace > 0)
                            PluginClients[Client.ClientNum].msgGrace--;

                        foreach (Flag f in flags)
                        {
                            
                            if (Util.Distance(Util.clientToVector(Client), f.positionIn) <= 100f)
                            {
                                if (PluginClients[Client.ClientNum].msgGrace <= 0 && !f.autoUseIn)
                                {
                                    PluginClients[Client.ClientNum].msgGrace = FLAG_MSG_GRACE;
                                    iPrintLnBold("Press ^3[{+activate}] ^7to teleport.", Client);
                                }

                                if ((Client.Other.ButtonPressed(Buttons.Activate) || f.autoUseIn))
                                    Util.moveTo(Client, f.positionOut);        

                            }
                            else if (Util.Distance(Util.clientToVector(Client), f.positionOut) <= 100f && f.bothWays)
                            {
                                if (PluginClients[Client.ClientNum].msgGrace <= 0)
                                {
                                    PluginClients[Client.ClientNum].msgGrace = FLAG_MSG_GRACE;
                                    iPrintLnBold("Press ^3[{+activate}] ^7to teleport.", Client);
                                }

                                if (Client.Other.ButtonPressed(Buttons.Activate))
                                    Util.moveTo(Client, f.positionIn);
                            }
                        }
                    }
                }
            }
            base.OnAddonFrame();
        }


        public override ChatType OnSay(string Message, ServerClient Client, bool teamChat)
        {
            string lowMsg = Message.ToLower();

            if (lowMsg.StartsWith("tp"))
            {
                iPrintLnBold(PluginClients[Client.ClientNum].msgGrace.ToString(), Client);
                return ChatType.ChatNone;
            }

            if (lowMsg.StartsWith("!coords"))
            {
                StringBuilder s = new StringBuilder();
                s.Append("Current coordinate: ");
                s.Append(((int)Client.OriginX).ToString());
                s.Append(",");
                s.Append(((int)Client.OriginY).ToString());
                s.Append(",");
                s.Append(((int)Client.OriginZ).ToString());
                iPrintLnBold(s.ToString(), Client);
                ServerLog(LogType.LogData, s.ToString());

                return ChatType.ChatNone;
            }

            return ChatType.ChatContinue;
        }
    }
}
