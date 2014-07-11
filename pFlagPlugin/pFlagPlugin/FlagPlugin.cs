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
        bool flagsEnabled = false;
        List<Flag> flags = new List<Flag>();

        public override void OnServerLoad()
        {
            ServerPrint("pFlags loaded. Author: Pozzuh. Version 1.0");
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
                int x1 = (int)item.SelectToken("position_in.x");
                int y1 = (int)item.SelectToken("position_in.y");
                int z1 = (int)item.SelectToken("position_in.z");

                int x2 = (int)item.SelectToken("position_out.x");
                int y2 = (int)item.SelectToken("position_out.y");
                int z2 = (int)item.SelectToken("position_out.z");

                bool bothWays = (bool)item.SelectToken("bothways");
                Flag f = new Flag(x1, y1, z1, x2, y2, z2);
                f.bothWays = bothWays;

                flags.Add(f);
                flagsEnabled = true;
            }           
        }

        void setupFlagIngame()
        {
            if (!flagsEnabled)
                return;

            for(int i = 0; i < flags.Count; i++)
            {
                Entity eIn = SpawnModel("script_brushmodel", "prop_flag_neutral", flags[i].positionIn);
                Entity eOut = SpawnModel("script_brushmodel", "prop_flag_neutral", flags[i].positionOut);

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
                        foreach (Flag f in flags)
                        {
                            if (Util.Distance(Util.clientToVector(Client), f.positionIn) <= 100f)
                            {
                                iPrintLnBold("Press ^3[{+activate}] ^7to teleport.", Client);
                                if (Client.Other.ButtonPressed(Buttons.Activate))
                                    Util.moveTo(Client, f.positionOut);
                            }
                            else if (Util.Distance(Util.clientToVector(Client), f.positionOut) <= 100f && f.bothWays)
                            {
                                iPrintLnBold("Press ^3[{+activate}] ^7to teleport.", Client);
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
