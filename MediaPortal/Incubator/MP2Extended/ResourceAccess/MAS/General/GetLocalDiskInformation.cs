﻿using System.IO;
using System.Linq;
using HttpServer;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.TAS.Misc.BaseClasses;
using MediaPortal.Plugins.MP2Extended.Utils;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.General
{
  internal class GetLocalDiskInformation : BaseCard, IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      return DriveInfo.GetDrives().Select(x => DiskSpaceInformation.GetSpaceInformation(x.Name)).ToList();
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}