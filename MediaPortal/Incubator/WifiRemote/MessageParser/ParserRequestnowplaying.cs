﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Deusty.Net;
using MediaPortal.Plugins.WifiRemote.Messages;
using MediaPortal.Plugins.WifiRemote.SendMessages;
using Newtonsoft.Json.Linq;

namespace MediaPortal.Plugins.WifiRemote.MessageParser
{
  internal class ParserRequestnowplaying
  {
    public static bool Parse(JObject message, SocketServer server, AsyncSocket sender)
    {
      SendMessageToAllClients.Send(new MessageNowPlaying(), ref SocketServer.Instance.connectedSockets);

      return true;
    }
  }
}
