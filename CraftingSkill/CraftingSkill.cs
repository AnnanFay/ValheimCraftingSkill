using BepInEx;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using UnityEngine;
using ExtendedItemDataFramework;
using System.IO;
using System.Reflection;

// using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace CraftingSkill
{

    [BepInPlugin("annanfay.mod.crafting_skill", CraftingSkillsPlugin.MOD_NAME, CraftingSkillsPlugin.MOD_VERSION)]
    [BepInDependency("pfhoenix.modconfigenforcer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    [BepInDependency("randyknapp.mods.extendeditemdataframework")]
    public class CraftingSkillsPlugin : BaseUnityPlugin
    {
        public const String MOD_ID = "annanfay.mod.crafting_skill";
        public const string MOD_NAME = "CraftingSkill";
        public const string MOD_VERSION = "0.0.2";
        public const int CRAFTING_SKILL_ID = 1605;

        Harmony harmony;

        private static CraftingConfig config = new CraftingConfig();

        public CraftingSkillsPlugin() {
            LoadEmbeddedAssembly("fastJSON.dll");
        }

        private static void LoadEmbeddedAssembly(string assemblyName)
        {
            //  RandyKnapp https://github.com/RandyKnapp/ValheimMods/blob/719e1a6dc419f9075c46b3eb4e3eb53285b7ffda/EpicLoot-Addon-Helheim/Helheim.cs#L58
            var stream = GetManifestResourceStream(assemblyName);
            if (stream == null)
            {
                Debug.LogError($"Could not load embedded assembly ({assemblyName})!");
                return;
            }

            using (stream)
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                Assembly.Load(data);
            }
        }

        public static Stream GetManifestResourceStream(string filename)
        {
            var assembly = Assembly.GetCallingAssembly();
            var fullname = assembly.GetManifestResourceNames().SingleOrDefault(x => x.EndsWith(filename));
            if (!string.IsNullOrEmpty(fullname))
            {
                return assembly.GetManifestResourceStream(fullname);
            }

            return null;
        }

        void Awake()
        {
            config.InitConfig(MOD_ID, Config);

            harmony = new Harmony(MOD_ID);
            harmony.PatchAll();

            SkillInjector.RegisterNewSkill(CRAFTING_SKILL_ID, "Crafting", "Craft higher quality items as you level", 1.0f, LoadIconTexture(), Skills.SkillType.Unarmed);

            ExtendedItemData.LoadExtendedItemData += QualityComponent.OnNewExtendedItemData;
            ExtendedItemData.NewExtendedItemData += QualityComponent.OnNewExtendedItemData;
        }

        private static Sprite LoadIconTexture()
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(directoryName, "CraftingSkillIcon.png");
            if (File.Exists(filepath))
            {
                Texture2D texture2D = LoadTexture(filepath);
                return Sprite.Create(texture2D, new Rect(0f, 0f, 64f, 64f), Vector2.zero);
            } 
            else
            {
                Debug.LogError("Unable to load skill icon! filepath:" + filepath);
                return null;
            }
        }
        
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        private static Texture2D LoadTexture(string filepath)
        {
            if (cachedTextures.ContainsKey(filepath))
            {
                return cachedTextures[filepath];
            }
            Texture2D texture2D = new Texture2D(0, 0);
            ImageConversion.LoadImage(texture2D, File.ReadAllBytes(filepath));
            return texture2D;
        }

        [HarmonyPatch(typeof(ItemDrop.ItemData))]
        public static class ItemDataPatcher
        {
            static String CraftQualityLabel = "Craft Quality";
            static String CraftExperienceLabel = "Craft Exp.";
            // public static string GetTooltip(ItemData item, int qualityLevel, bool crafting)
            [HarmonyPostfix]
            [HarmonyPatch("GetTooltip", typeof(ItemDrop.ItemData), typeof(int), typeof(bool))]
            static void GetTooltip(ItemDrop.ItemData item, int qualityLevel, bool crafting, ref string __result)
            {
                QualityComponent qualityComp = item.Extended()?.GetComponent<QualityComponent>();
                if(qualityComp != null) {
                    var qcTooltip = qualityComp.GetTooltip();
                    __result += String.Format(
                        "\n{0}: <color=orange>{1}</color>",
                        CraftQualityLabel,
                        qcTooltip
                    );
                }
                Recipe recipe = ObjectDB.instance.GetRecipe(item);
                if (recipe != null) {
                    __result += String.Format(
                        "\n{0}: <color=orange>{1}</color>",
                        CraftExperienceLabel,
                        GetCraftExperience(recipe, qualityLevel).ToString("0")
                    );
                }
            }

            public static float GetItemQualityScalingFactor(ItemDrop.ItemData item, float min, float max)
            {
                QualityComponent qualityComp = item.Extended()?.GetComponent<QualityComponent>();
                if (qualityComp == null) {
                    return 1.0f;
                }
                float scalingFactor = min + (max - min) * qualityComp.Quality.Skill;
                return scalingFactor;
            }

            // First override the simple methods which returns floats
                // public float GetArmor(int quality){}
                // public float GetWeight(){}
                // public float GetMaxDurability(int quality){}
                // public float GetBaseBlockPower(int quality){}
                // public float GetDeflectionForce(int quality){}
            // public float GetArmor(int quality){}
            [HarmonyPostfix]
            [HarmonyPatch("GetArmor", typeof(int))]
            public static float GetArmor(float __result, ItemDrop.ItemData __instance)
            {
                return __result * GetItemQualityScalingFactor(__instance, config.ArmorStart, config.ArmorStop);
            }

            // public float GetWeight()
            [HarmonyPostfix]
            [HarmonyPatch("GetWeight")]
            public static float GetWeight(float __result, ItemDrop.ItemData __instance)
            {
                return __result * GetItemQualityScalingFactor(__instance, config.WeightStart, config.WeightStop);
            }

            // public float GetMaxDurability(int quality)
            [HarmonyPostfix]
            [HarmonyPatch("GetMaxDurability", typeof(int))]
            public static float GetMaxDurability(float __result, ItemDrop.ItemData __instance)
            {
                return __result * GetItemQualityScalingFactor(__instance, config.MaxDurabilityStart, config.MaxDurabilityStop);
            }

            // public float GetBaseBlockPower(int quality){}
            [HarmonyPostfix]
            [HarmonyPatch("GetBaseBlockPower", typeof(int))]
            public static float GetBaseBlockPower(float __result, ItemDrop.ItemData __instance)
            {
                return __result * GetItemQualityScalingFactor(__instance, config.BaseBlockPowerStart, config.BaseBlockPowerStop);
            }

            // public float GetDeflectionForce(int quality){}
            [HarmonyPostfix]
            [HarmonyPatch("GetDeflectionForce", typeof(int))]
            public static float GetDeflectionForce(float __result, ItemDrop.ItemData __instance)
            {
                return __result * GetItemQualityScalingFactor(__instance, config.DeflectionForceStart, config.DeflectionForceStop);
            }

            // public HitData.DamageTypes GetDamage(int quality)
            [HarmonyPostfix]
            [HarmonyPatch("GetDamage", typeof(int))]
            public static void GetDamage(ref HitData.DamageTypes __result, ItemDrop.ItemData __instance)
            {
                float scalingFactor = GetItemQualityScalingFactor(__instance, config.DamageStart, config.DamageStop);
                __result.Modify(scalingFactor);
            }
        }

        public static float GetCraftTierMod(Recipe recipe)
        {
            switch (recipe.m_craftingStation?.m_name)
            {
                case null: // Crafting from inventory
                    return config.TierModifierInventory;
                case "$piece_forge":
                    return config.TierModifierForge;
                case "$piece_workbench":
                    return config.TierModifierWorkbench;
                case "$piece_cauldron":
                    return config.TierModifierCauldron;
                case "$piece_oven":
                    return config.TierModifierOven;
                case "$piece_stonecutter":
                    return config.TierModifierStonecutter;
                case "$piece_artisanstation":
                    return config.TierModifierArtisan;
                default:
                    return config.TierModifierDefault;
            }
        }

        public static int CountIngredients(Recipe recipe, int craftLevel)
        {
            int count = 0;
            foreach (Piece.Requirement req in recipe.m_resources)
            {
                int amount = req.GetAmount(craftLevel);
                count += amount;
            }
            return count;
        }

        public static float GetCraftExperience(Recipe recipe, int craftLevel)
        {
            int ingredientCount = CountIngredients(recipe, craftLevel);
            float craftTypeTierMod = GetCraftTierMod(recipe);
            float craftTier = recipe.m_minStationLevel + craftLevel;
            float effectiveCraftTier = craftTier + craftTypeTierMod;
            float exp = ingredientCount * config.ExpScapeLinear * (float)Math.Pow(config.ExpScapeTier * effectiveCraftTier, config.ExpScapePower);
            return exp;
        }

        [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
        public static class InventoryGuiPatcher
        {
            static int m_crafts;
            static int craftLevel;

            // private void DoCrafting(Player player)
            static void Prefix(InventoryGui __instance, Player player,
                // private fields
                ItemDrop.ItemData ___m_craftUpgradeItem,
                int ___m_craftVariant
            )
            {
                // ZLog.LogError($"[{nameof(InventoryGui)}] pre crafting hook");
                craftLevel = ((___m_craftUpgradeItem == null) ? 1 : (___m_craftUpgradeItem.m_quality + 1));
                m_crafts = Game.instance.GetPlayerProfile().m_playerStats.m_crafts;
            }

            static void Postfix(InventoryGui __instance, Player player,
                // private fields
                Recipe ___m_craftRecipe
            )
            {

                // ZLog.LogError($"[{nameof(InventoryGui)}] post crafting hook");
                int new_m_crafts = Game.instance.GetPlayerProfile().m_playerStats.m_crafts;

                // craft failed
                if (m_crafts >= new_m_crafts) {
                    // ZLog.LogError($"Craft Failed? {___m_craftRecipe.m_item.m_itemData.m_shared.m_name} {craftLevel} {___m_craftRecipe.m_craftingStation?.m_name}");
                    return;
                }
                // ZLog.LogError($"Craft Succeeded? {___m_craftRecipe.m_item.m_itemData.m_shared.m_name} {craftLevel} {___m_craftRecipe.m_craftingStation?.m_name}");

                // # Full list of stations used in recipes as of 0.147.3:
                // # - identifier: `$piece_forge` in game name: Forge
                // # - identifier: `$piece_workbench` in game name: Workbench
                // # - identifier: `$piece_cauldron` in game name: Cauldron
                // # - identifier: `$piece_stonecutter` in game name: Stonecutter
                // See also (added at some point after above list):
                //  - $piece_artisanstation
                //  - $piece_oven

                string craftingStationName = ___m_craftRecipe.m_craftingStation?.m_name;
                bool isNoStation         = craftingStationName == null;
                bool isForgeRecipe       = craftingStationName == "$piece_forge";
                bool isWorkbenchRecipe   = craftingStationName == "$piece_workbench";
                bool isStonecutterRecipe = craftingStationName == "$piece_stonecutter";
                bool isArtisanRecipe     = craftingStationName == "$piece_artisanstation";
                if (isWorkbenchRecipe || isForgeRecipe || isNoStation || isStonecutterRecipe || isArtisanRecipe) {
                    float craftExperience = GetCraftExperience(___m_craftRecipe, craftLevel);
                    player.RaiseSkill((Skills.SkillType)CRAFTING_SKILL_ID, craftExperience);
                    float SkillLevel = player.GetSkillFactor((Skills.SkillType)CRAFTING_SKILL_ID);
                    // ZLog.LogError($"Craft Succeeded => exp={craftExperience}, {SkillLevel}");
                }
            }
        }
    }
}
