using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace SubstatTiers
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 2;

        public bool ShowSubstatEffects { get; set; } = true;
        public bool ShowMateriaTiers { get; set; } = true;
        public bool ShowDamagePotency { get; set; } = true;
        public int LayoutType { get; set; } = 1;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
