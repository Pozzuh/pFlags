using System;
using System.Collections.Generic;
using System.Text;
using Addon;

namespace pFlagPlugin
{
    class PluginClient
    {
        private int _clientNum = -1;
        public int msgGrace = 0;
        public int timesTeleported = 0;
        //public int tpCooldown = 0;

        public PluginClient(int clientNum)
        {
            _clientNum = clientNum;
        }

        public PluginClient()
        {
        }

        /*public ~PluginClient()
        {
            _clientNum = -1;
            tpCooldown = -1;
        }*/

        public int ClientNum 
        { 
            get { return _clientNum; } 
        }
    }
}
