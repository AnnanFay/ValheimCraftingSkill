using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

// This code and associated ConfigVariable is largely copied from the sailing_skill mod
// It allows us to have the modconfigenforcer mod as a soft dependency
// https://github.com/gaijinx/valheim_mods/tree/main/sailing_skill

namespace CraftingSkill
{
    public class CraftingConfig
    {
        private ConfigVariable<bool> __QuantisedQuality       = new ConfigVariable<bool>("General", "QuantisedQuality", false);
        private ConfigVariable<float> __StochasticVariance    = new ConfigVariable<float>("General", "StochasticVariance", 0.0f);

        // Experience for crafting
        private ConfigVariable<float> __ExpScapeTier          = new ConfigVariable<float>("ExpGain", "ExpScapeTier", 1.0f);
        private ConfigVariable<float> __ExpScapePower         = new ConfigVariable<float>("ExpGain", "ExpScapePower", 2.0f);
        private ConfigVariable<float> __ExpScapeLinear        = new ConfigVariable<float>("ExpGain", "ExpScapeLinear", 0.16f);

        // Effects of quality
        private ConfigVariable<float> __ArmorStart            = new ConfigVariable<float>("Attributes", "ArmorStart", 0.8f);
        private ConfigVariable<float> __ArmorStop             = new ConfigVariable<float>("Attributes", "ArmorStop", 1.3f);
        private ConfigVariable<float> __WeightStart           = new ConfigVariable<float>("Attributes", "WeightStart", 1.1f);
        private ConfigVariable<float> __WeightStop            = new ConfigVariable<float>("Attributes", "WeightStop", 0.7f);
        private ConfigVariable<float> __MaxDurabilityStart    = new ConfigVariable<float>("Attributes", "MaxDurabilityStart", 0.8f);
        private ConfigVariable<float> __MaxDurabilityStop     = new ConfigVariable<float>("Attributes", "MaxDurabilityStop", 1.6f);
        private ConfigVariable<float> __BaseBlockPowerStart   = new ConfigVariable<float>("Attributes", "BaseBlockPowerStart", 0.8f);
        private ConfigVariable<float> __BaseBlockPowerStop    = new ConfigVariable<float>("Attributes", "BaseBlockPowerStop", 1.3f);
        private ConfigVariable<float> __DeflectionForceStart  = new ConfigVariable<float>("Attributes", "DeflectionForceStart", 0.8f);
        private ConfigVariable<float> __DeflectionForceStop   = new ConfigVariable<float>("Attributes", "DeflectionForceStop", 1.3f);
        private ConfigVariable<float> __DamageStart           = new ConfigVariable<float>("Attributes", "DamageStart", 0.8f);
        private ConfigVariable<float> __DamageStop            = new ConfigVariable<float>("Attributes", "DamageStop", 1.3f);

        // Cauldron/Oven: It's not crafting so no experience, but if we did
        // Stonecutter: should probably be 2 or 3, but only thing right now is shitty sharpening stone
        // Artisan: is used by Epic Loot?, will likely have vanilla items in future
        // Guessing at 2 for now
        private ConfigVariable<int> __TierModifierInventory   = new ConfigVariable<int>("StationBalancing", "TierModifierInventory", 0);
        private ConfigVariable<int> __TierModifierWorkbench   = new ConfigVariable<int>("StationBalancing", "TierModifierWorkbench", 0);
        private ConfigVariable<int> __TierModifierForge       = new ConfigVariable<int>("StationBalancing", "TierModifierForge", 2);
        private ConfigVariable<int> __TierModifierCauldron    = new ConfigVariable<int>("StationBalancing", "TierModifierCauldron", 3);
        private ConfigVariable<int> __TierModifierOven        = new ConfigVariable<int>("StationBalancing", "TierModifierOven", 3);
        private ConfigVariable<int> __TierModifierStonecutter = new ConfigVariable<int>("StationBalancing", "TierModifierStonecutter", 1);
        private ConfigVariable<int> __TierModifierArtisan     = new ConfigVariable<int>("StationBalancing", "TierModifierArtisan", 2);
        private ConfigVariable<int> __TierModifierDefault     = new ConfigVariable<int>("StationBalancing", "TierModifierDefault", 0);

        // Utility getters
        public bool QuantisedQuality             {get => __QuantisedQuality.Value; }
        public float StochasticVariance          {get => __StochasticVariance.Value; }
        public float ExpScapeTier                {get => __ExpScapeTier.Value; }
        public float ExpScapePower               {get => __ExpScapePower.Value; }
        public float ExpScapeLinear              {get => __ExpScapeLinear.Value; }
        public float ArmorStart                  {get => __ArmorStart.Value; }
        public float ArmorStop                   {get => __ArmorStop.Value; }
        public float WeightStart                 {get => __WeightStart.Value; }
        public float WeightStop                  {get => __WeightStop.Value; }
        public float MaxDurabilityStart          {get => __MaxDurabilityStart.Value; }
        public float MaxDurabilityStop           {get => __MaxDurabilityStop.Value; }
        public float BaseBlockPowerStart         {get => __BaseBlockPowerStart.Value; }
        public float BaseBlockPowerStop          {get => __BaseBlockPowerStop.Value; }
        public float DeflectionForceStart        {get => __DeflectionForceStart.Value; }
        public float DeflectionForceStop         {get => __DeflectionForceStop.Value; }
        public float DamageStart                 {get => __DamageStart.Value; }
        public float DamageStop                  {get => __DamageStop.Value; }
        public int TierModifierInventory         {get => __TierModifierInventory.Value; }
        public int TierModifierWorkbench         {get => __TierModifierWorkbench.Value; }
        public int TierModifierForge             {get => __TierModifierForge.Value; }
        public int TierModifierCauldron          {get => __TierModifierCauldron.Value; }
        public int TierModifierOven              {get => __TierModifierOven.Value; }
        public int TierModifierStonecutter       {get => __TierModifierStonecutter.Value; }
        public int TierModifierArtisan           {get => __TierModifierArtisan.Value; }
        public int TierModifierDefault           {get => __TierModifierDefault.Value; }

        public CraftingConfig()
        {

        }

        public void InitConfig(string mod_id, ConfigFile config)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ModConfigEnforcer");
            if (assembly != null)
            {
                try
                {
                    // Try to register using MCE
                    Debug.Log("[CraftingSkill] Mod Config Enforcer detected, registering mod...");
                    var configManagerType = assembly.GetType("ModConfigEnforcer.ConfigManager");
                    Debug.Log("configManagerType: " + configManagerType.ToString());
                    var traverse = Traverse.Create(configManagerType);
                    var serverConfigReceivedDelegateType = (Type)traverse.Type("ServerConfigReceivedDelegate").GetValue();
                    Type[] paramTypes = { typeof(string), typeof(ConfigFile), serverConfigReceivedDelegateType };
                    traverse.Method("RegisterMod", paramTypes).GetValue(mod_id, config, null);
                } catch (Exception) {
                    // registering mod failed, API may have changed
                    // pretend MCE doesn't exist
                    assembly = null;
                }
            }
            else
            {
                Debug.Log("Mod Config Enforcer not detected.");
            }

            __QuantisedQuality.init(assembly, config, mod_id);
            __StochasticVariance.init(assembly, config, mod_id);
            __ExpScapeTier.init(assembly, config, mod_id);
            __ExpScapePower.init(assembly, config, mod_id);
            __ExpScapeLinear.init(assembly, config, mod_id);
            __ArmorStart.init(assembly, config, mod_id);
            __ArmorStop.init(assembly, config, mod_id);
            __WeightStart.init(assembly, config, mod_id);
            __WeightStop.init(assembly, config, mod_id);
            __MaxDurabilityStart.init(assembly, config, mod_id);
            __MaxDurabilityStop.init(assembly, config, mod_id);
            __BaseBlockPowerStart.init(assembly, config, mod_id);
            __BaseBlockPowerStop.init(assembly, config, mod_id);
            __DeflectionForceStart.init(assembly, config, mod_id);
            __DeflectionForceStop.init(assembly, config, mod_id);
            __DamageStart.init(assembly, config, mod_id);
            __DamageStop.init(assembly, config, mod_id);
            __TierModifierInventory.init(assembly, config, mod_id);
            __TierModifierWorkbench.init(assembly, config, mod_id);
            __TierModifierForge.init(assembly, config, mod_id);
            __TierModifierCauldron.init(assembly, config, mod_id);
            __TierModifierOven.init(assembly, config, mod_id);
            __TierModifierStonecutter.init(assembly, config, mod_id);
            __TierModifierArtisan.init(assembly, config, mod_id);
            __TierModifierDefault.init(assembly, config, mod_id);

        }
    }
}
