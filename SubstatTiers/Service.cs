using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;

#pragma warning disable 8618

namespace SubstatTiers
{
    internal class Service
    {
        internal static void Initialize(DalamudPluginInterface pluginInterface) => pluginInterface.Create<Service>();

        [PluginService]
        [RequiredVersion("1.0")]
        internal static DataManager DataManager { get; private set; }
    }
}

#pragma warning restore 8618
