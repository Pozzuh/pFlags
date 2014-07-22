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
        public bool TP_LIMIT_ENABLED = false;
        public int MAX_TELEPORTS = 0;
        public int TP_LIMIT_TEAM = 0; //0=neither,1=allies,2=axis,3=both
        PluginClient[] PluginClients = new PluginClient[18];
        bool flagsEnabled = false;
        List<Flag> flags = new List<Flag>();
        /*int fx_flagbase_black;
        int fx_flagbase_gold;
        int fx_flagbase_red;
        int fx_flagbase_silver;*/

        public override void OnServerLoad()
        {
            ServerPrint("pFlags loaded. Author: Pozzuh. Version 1.3");

            setupTPLimit();
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

        void setupTPLimit()
        {
            if (GetServerCFG("pFlags", "Max_Teleports", "-1") == "-1")
                SetServerCFG("pFlags", "Max_Teleports", "0");

            if(GetServerCFG("pFlags", "Limit_for_team", "-1") == "-1")
                SetServerCFG("pFlags", "Limit_for_team", "both");


            string maxTeleports = GetServerCFG("pFlags", "Max_Teleports", "0"); // <1 = unlimited
            Int32.TryParse(maxTeleports, out MAX_TELEPORTS);

            if (MAX_TELEPORTS >= 1)
                TP_LIMIT_ENABLED = true;

            string limitTPteam = GetServerCFG("pFlags", "Limit_for_team", "both");
            switch (limitTPteam.ToLower() )
            {
                case "allies":
                    TP_LIMIT_TEAM = 1;
                    break;
                case "axis":
                    TP_LIMIT_TEAM = 2;
                    break;
                case "both":
                    TP_LIMIT_TEAM = 3;
                    break;
                default:
                    TP_LIMIT_TEAM = 3;
                    break;
            }
        }

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

        bool teamHasLimit(Teams team)
        {
            if (!TP_LIMIT_ENABLED)
                return false;
            if (TP_LIMIT_TEAM == 3)
                return true;

            if (team == Teams.Allies)
            {
                if (TP_LIMIT_TEAM == 1)
                    return true;
            }
            else if(team == Teams.Axis)
            {
                if(TP_LIMIT_TEAM == 2)
                    return true;
            }

            return false;
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
                                    if(!TP_LIMIT_ENABLED 
                                        || (TP_LIMIT_ENABLED && teamHasLimit(Client.Team) && PluginClients[Client.ClientNum].timesTeleported < MAX_TELEPORTS) 
                                        || (TP_LIMIT_ENABLED && !teamHasLimit(Client.Team)))
                                    {
                                        PluginClients[Client.ClientNum].msgGrace = FLAG_MSG_GRACE;
                                        iPrintLnBold("Press ^3[{+activate}] ^7to teleport.", Client);
                                    }
                                }

                                if ((Client.Other.ButtonPressed(Buttons.Activate) || f.autoUseIn))
                                {
                                    if (!TP_LIMIT_ENABLED)
                                    {
                                        Util.moveTo(Client, f.positionOut);
                                    }
                                    else if (TP_LIMIT_ENABLED && (teamHasLimit(Client.Team) && PluginClients[Client.ClientNum].timesTeleported < MAX_TELEPORTS))
                                    {
                                        Util.moveTo(Client, f.positionOut);
                                        PluginClients[Client.ClientNum].timesTeleported++;
                                        iPrintLnBold(String.Format("You have {0} teleport(s) remaining this match.", MAX_TELEPORTS - PluginClients[Client.ClientNum].timesTeleported), Client);
                                    }
                                    else if(TP_LIMIT_ENABLED && !teamHasLimit(Client.Team))
                                    {
                                        Util.moveTo(Client, f.positionOut);
                                    }
                                    else if (PluginClients[Client.ClientNum].msgGrace <= 0)
                                    {
                                        PluginClients[Client.ClientNum].msgGrace = FLAG_MSG_GRACE;
                                        iPrintLnBold("You can't teleport anymore in this match.", Client);
                                    }
                                }
                            }
                            else if (Util.Distance(Util.clientToVector(Client), f.positionOut) <= 100f && f.bothWays )
                            {
                                if (PluginClients[Client.ClientNum].msgGrace <= 0)
                                {
                                    if (!TP_LIMIT_ENABLED
                                        || (TP_LIMIT_ENABLED && teamHasLimit(Client.Team) && PluginClients[Client.ClientNum].timesTeleported < MAX_TELEPORTS)
                                        || (TP_LIMIT_ENABLED && !teamHasLimit(Client.Team)))
                                    {
                                        PluginClients[Client.ClientNum].msgGrace = FLAG_MSG_GRACE;
                                        iPrintLnBold("Press ^3[{+activate}] ^7to teleport.", Client);
                                    }
                                    
                                }

                                if (Client.Other.ButtonPressed(Buttons.Activate))
                                {
                                    if (!TP_LIMIT_ENABLED)
                                    {
                                        Util.moveTo(Client, f.positionIn);
                                    }
                                    else if (TP_LIMIT_ENABLED && (teamHasLimit(Client.Team) && PluginClients[Client.ClientNum].timesTeleported < MAX_TELEPORTS))
                                    {
                                        Util.moveTo(Client, f.positionIn);
                                        PluginClients[Client.ClientNum].timesTeleported++;
                                        iPrintLnBold(String.Format("You have {0} teleport(s) remaining this match.", MAX_TELEPORTS - PluginClients[Client.ClientNum].timesTeleported), Client);
                                    }
                                    else if (TP_LIMIT_ENABLED && !teamHasLimit(Client.Team))
                                    {
                                        Util.moveTo(Client, f.positionIn);
                                    }
                                    else if (PluginClients[Client.ClientNum].msgGrace <= 0)
                                    {
                                        PluginClients[Client.ClientNum].msgGrace = FLAG_MSG_GRACE;
                                        iPrintLnBold("You can't teleport anymore in this match.", Client);
                                    }
                                }
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
