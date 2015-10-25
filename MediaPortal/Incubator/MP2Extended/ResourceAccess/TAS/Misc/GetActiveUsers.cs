﻿using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Exceptions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.MAS.General;
using MediaPortal.Plugins.MP2Extended.TAS;
using MediaPortal.Plugins.MP2Extended.TAS.Misc;
using MediaPortal.Plugins.SlimTv.Interfaces;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;
using MediaPortal.Plugins.SlimTv.Interfaces.UPnP.Items;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.TAS.Misc
{
  internal class GetActiveUsers : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      if (!ServiceRegistration.IsRegistered<ITvProvider>())
        throw new BadRequestException("GetActiveUsers: ITvProvider not found");

      ITunerInfo tunerInfo = ServiceRegistration.Get<ITvProvider>() as ITunerInfo;

      if (tunerInfo == null)
        throw new BadRequestException("GetActiveUsers: ITunerInfo not present");

      List<IVirtualCard> cards;
      tunerInfo.GetActiveVirtualCards(out cards);

      return cards.Select(card => new WebUser
      {
        ChannelId = card.User.IdChannel, 
        Name = card.User.Name, 
        CardId = card.User.CardId, 
        HeartBeat = card.User.HeartBeat, 
        IsAdmin = card.User.IsAdmin, 
        SubChannel = card.User.SubChannel, 
        TvStoppedReason = (int)card.User.TvStoppedReason,
      }).ToList();
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}