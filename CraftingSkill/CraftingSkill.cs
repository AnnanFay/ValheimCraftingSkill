using BepInEx;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using UnityEngine;
using UnityEngine.UI;
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
        public const string MOD_VERSION = "0.0.3";
        public const int CRAFTING_SKILL_ID = 1605;

        Harmony harmony;

        public static CraftingConfig config = new CraftingConfig();
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        public CraftingSkillsPlugin() {
            LoadEmbeddedAssembly("fastJSON.dll");
        }

        void Awake()
        {
            config.InitConfig(MOD_ID, Config);

            harmony = new Harmony(MOD_ID);
            harmony.PatchAll();

            SkillInjector.RegisterNewSkill(CRAFTING_SKILL_ID, "Crafting", "Craft higher quality items as you level", 1.0f, LoadIconTexture(), Skills.SkillType.Unarmed);

            ExtendedItemData.LoadExtendedItemData += QualityComponent.OnLoadExtendedItemData;
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
            //  RandyKnapp https://github.com/RandyKnapp/ValheimMods/blob/719e1a6dc419f9075c46b3eb4e3eb53285b7ffda/EpicLoot-Addon-Helheim/Helheim.cs
            var assembly = Assembly.GetCallingAssembly();
            var fullname = assembly.GetManifestResourceNames().SingleOrDefault(x => x.EndsWith(filename));
            if (!string.IsNullOrEmpty(fullname))
            {
                return assembly.GetManifestResourceStream(fullname);
            }

            return null;
        }


        public static void DropHeadFix(ItemDrop.ItemData item)
        {
            // if item stack is less than quality stack remove from front until they match
            // this happens after using arrows, consuming food, etc.

            QualityComponent comp = item.Extended()?.GetComponent<QualityComponent>();
            // item will be deleted if required, so no need to handle stack 0 case
            if (comp != null && item.m_stack > 0 && item.m_stack < comp.Quality.Quantity) {
                int toRemove = comp.Quality.Quantity - item.m_stack;
                if (config.DebugTooltips)
                {
                    ZLog.Log("DropHeadFix APPLIED #" + item.Extended()?.GetUniqueId() + " | " + item.m_stack + " < " + comp.Quality.Quantity);
                }
                comp.Quality.Shift(toRemove);
                comp.Save();
            }
        }
        
        public static void DropTailFix(ItemDrop.ItemData item)
        {
            // called in handlers explicitly when new item stacks are created
            // discards fron back of stack until counts match

            QualityComponent comp = item.Extended()?.GetComponent<QualityComponent>();
            if (comp == null) return;

            if (config.DebugTooltips)
            {
                ZLog.Log("DropTailFix? #" + item.Extended()?.GetUniqueId() + " | " + item.m_stack + " < " + comp.Quality.Quantity);
            }
            // item will be deleted if required, so no need to handle stack 0 case
            if (item.m_stack > 0 && item.m_stack < comp.Quality.Quantity) {
                int toRemove = comp.Quality.Quantity - item.m_stack;
                comp.Quality.Pop(toRemove);
            }
            comp.Save();
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
                // Display craft quality if item has it
                // Then display crafting experience if there is a crafting recipe for the item
                QualityComponent qualityComp = item.Extended()?.GetComponent<QualityComponent>();
                if(qualityComp != null) {
                    var qcTooltip = qualityComp.GetTooltip(config);
                    __result += String.Format(
                        "\n{0}: <color=orange>{1}</color>",
                        CraftQualityLabel,
                        qcTooltip
                    );
                }
                Recipe recipe = ObjectDB.instance.GetRecipe(item);
                if (isCraftingRecipe(recipe)) {
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
                return min + (max - min) * qualityComp.Quality.ScalingFactor(config);
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
                DropHeadFix(__instance);
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
                DropHeadFix(__instance);
                return __result * GetItemQualityScalingFactor(__instance, config.MaxDurabilityStart, config.MaxDurabilityStop);
            }

            // public float GetBaseBlockPower(int quality){}
            [HarmonyPostfix]
            [HarmonyPatch("GetBaseBlockPower", typeof(int))]
            public static float GetBaseBlockPower(float __result, ItemDrop.ItemData __instance)
            {
                DropHeadFix(__instance);
                return __result * GetItemQualityScalingFactor(__instance, config.BaseBlockPowerStart, config.BaseBlockPowerStop);
            }

            // public float GetDeflectionForce(int quality){}
            [HarmonyPostfix]
            [HarmonyPatch("GetDeflectionForce", typeof(int))]
            public static float GetDeflectionForce(float __result, ItemDrop.ItemData __instance)
            {
                DropHeadFix(__instance);
                return __result * GetItemQualityScalingFactor(__instance, config.DeflectionForceStart, config.DeflectionForceStop);
            }

            // public HitData.DamageTypes GetDamage(int quality)
            [HarmonyPostfix]
            [HarmonyPatch("GetDamage", typeof(int))]
            public static void GetDamage(ref HitData.DamageTypes __result, ItemDrop.ItemData __instance)
            {
                DropHeadFix(__instance);
                float scalingFactor = GetItemQualityScalingFactor(__instance, config.DamageStart, config.DamageStop);
                __result.Modify(scalingFactor);
            }
        }

        // Below code handles gaining experience when crafting

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

        public static bool isCraftingRecipe(Recipe recipe)
        {
            // # Full list of stations used in recipes as of 0.147.3:
            // # - identifier: `$piece_forge` in game name: Forge
            // # - identifier: `$piece_workbench` in game name: Workbench
            // # - identifier: `$piece_cauldron` in game name: Cauldron
            // # - identifier: `$piece_stonecutter` in game name: Stonecutter
            // See also (added at some point after above list):
            //  - $piece_artisanstation
            //  - $piece_oven
            if (recipe == null) return false;

            string craftingStationName = recipe?.m_craftingStation?.m_name;
            bool isNoStation         = craftingStationName == null;
            bool isForgeRecipe       = craftingStationName == "$piece_forge";
            bool isWorkbenchRecipe   = craftingStationName == "$piece_workbench";
            bool isStonecutterRecipe = craftingStationName == "$piece_stonecutter";
            bool isArtisanRecipe     = craftingStationName == "$piece_artisanstation";
            return isWorkbenchRecipe || isForgeRecipe || isNoStation || isStonecutterRecipe || isArtisanRecipe;
        }

        [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
        public static class InventoryGuiPatcherDoCrafting
        {
            static int m_crafts;
            static int craftLevel;

            // Before upgrading an item we store quality here 
            // so we can access it when the new item is created!
            // Item is created between prefix and postfix in the original DoCrafting
            // In postfix this field is nullified
            public static StackableQuality preUpgradeQuality;

            // private void DoCrafting(Player player)
            static void Prefix(InventoryGui __instance, Player player,
                // private fields
                ItemDrop.ItemData ___m_craftUpgradeItem,
                int ___m_craftVariant
            )
            {
                // ZLog.LogError($"[{nameof(InventoryGui)}] pre crafting hook");
                if (___m_craftUpgradeItem != null) {
                    craftLevel = ___m_craftUpgradeItem.m_quality + 1;
                    QualityComponent comp = ___m_craftUpgradeItem.Extended()?.GetComponent<QualityComponent>();
                    if (comp != null) {
                        preUpgradeQuality = comp.Quality;
                    }
                } else {
                    craftLevel = 1;
                }
                m_crafts = Game.instance.GetPlayerProfile().m_playerStats.m_crafts;

            }

            static void Postfix(InventoryGui __instance, Player player,
                // private fields
                Recipe ___m_craftRecipe
            )
            {
                // this MUST be nullified or new items will be bugged
                preUpgradeQuality = null;

                // ZLog.LogError($"[{nameof(InventoryGui)}] post crafting hook");
                int new_m_crafts = Game.instance.GetPlayerProfile().m_playerStats.m_crafts;

                // craft failed
                if (m_crafts >= new_m_crafts) {
                    // ZLog.LogError($"Craft Failed? {___m_craftRecipe.m_item.m_itemData.m_shared.m_name} {craftLevel} {___m_craftRecipe.m_craftingStation?.m_name}");
                    return;
                }
                // ZLog.LogError($"Craft Succeeded? {___m_craftRecipe.m_item.m_itemData.m_shared.m_name} {craftLevel} {___m_craftRecipe.m_craftingStation?.m_name}");
                if (isCraftingRecipe(___m_craftRecipe)) {
                    float craftExperience = GetCraftExperience(___m_craftRecipe, craftLevel);
                    player.RaiseSkill((Skills.SkillType)CRAFTING_SKILL_ID, craftExperience);
                    // float SkillLevel = player.GetSkillFactor((Skills.SkillType)CRAFTING_SKILL_ID);
                    // ZLog.LogError($"Craft Succeeded => exp={craftExperience}, {SkillLevel}");
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "RepairOneItem")]
        public static class InventoryGuiPatcherRepairOneItem
        {
            static int REPAIR_EXPERIENCE = 1;

            // private void RepairOneItem()
            static void Prefix(InventoryGui __instance,
                // private fields
                ItemDrop.ItemData ___m_craftUpgradeItem,
                int ___m_craftVariant,
                List<ItemDrop.ItemData> ___m_tempWornItems
            )
            {
                if (Player.m_localPlayer == null)
                {
                    return;
                }
                CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
                if ((currentCraftingStation == null && !Player.m_localPlayer.NoCostCheat()) 
                    || ((bool)currentCraftingStation && !currentCraftingStation.CheckUsable(Player.m_localPlayer, showMessage: false)))
                {
                    return;
                }
                ___m_tempWornItems.Clear();
                Player.m_localPlayer.GetInventory().GetWornItems(___m_tempWornItems);
                var CanRepair = __instance.GetType().GetMethod("CanRepair", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (ItemDrop.ItemData tempWornItem in ___m_tempWornItems)
                {
                    if ((bool)CanRepair.Invoke(__instance, new object[] { tempWornItem }))
                    {
                        // tempWornItem.m_durability = tempWornItem.GetMaxDurability();
                        Player.m_localPlayer.RaiseSkill((Skills.SkillType)CRAFTING_SKILL_ID, REPAIR_EXPERIENCE);
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateCharacterStats")]
        public static class InventoryGuiPatcherUpdateCharacterStats
        {
            // private void UpdateCharacterStats(Player player)
            static void Postfix(InventoryGui __instance,
                Player player,
                // private fields
                Text ___m_armor
            )
            {
                float bodyArmor = player.GetBodyArmor();
                ___m_armor.text = $"{bodyArmor:0.00}";
            }
        }

        [HarmonyPatch]
        static class ItemDataClonePatches
        {
            [HarmonyPatch(typeof(DropTable), "AddItemToList")]
            static void Postfix(List<ItemDrop.ItemData> toDrop, DropTable.DropData data) {
                DropTailFix(toDrop.Last());
            }

            [HarmonyPatch(typeof(Inventory), "AddItem", new Type[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int)})]
            static void Postfix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y) {
                if (config.DebugTooltips)
                {
                    ZLog.Log("AddItem(amount:" + amount + ", x:"+x+", y:"+y+")");
                }

                var target = __instance.GetItemAt(x, y);

                QualityComponent comp = item.Extended()?.GetComponent<QualityComponent>();
                if (config.DebugTooltips)
                {
                    ZLog.Log("... AddItem, source #" + item.Extended()?.GetUniqueId() + " | " + comp);
                }
                if (comp == null) return;

                QualityComponent targetComp = target.Extended()?.GetComponent<QualityComponent>();
                if (config.DebugTooltips)
                {
                    ZLog.Log("... AddItem, target = #" + target.Extended()?.GetUniqueId() + " | " + targetComp + " " + (targetComp == null ? "NULL" : (target.m_stack + " > " + targetComp.Quality.Quantity)));
                }
                // existing item, needs extra quality data
                if (targetComp != null && target.m_stack > targetComp.Quality.Quantity) {
                    targetComp.Quality.MergeInto(comp.Quality);
                    targetComp.Save();
                }
                DropTailFix(target);
            }


            [HarmonyPatch(typeof(Inventory), "AddItem", new Type[] { typeof(ItemDrop.ItemData) })]
            static void Postfix(Inventory __instance, ItemDrop.ItemData item, List<ItemDrop.ItemData> ___m_inventory) {
                // We need to recover from an item stack being automatically spread across multiple stacks in target inventory
                // Example: Have stacks of 99, 99 and 99 arrows in your inventory. Crafting 20 arrows will deposit 1 arrow
                // in each stack and 17 in a new stack.

                // item is a reference to item (maybe also in inventory) which we take qualities from as needed
                // it will likely have more qualities than stacksize, while targets for fixing have more stack than qualities

                QualityComponent comp = item.Extended()?.GetComponent<QualityComponent>();
                
                if (comp == null)
                {
                    return;
                }

                var sourceQuality = comp.Quality;
                if (item.m_shared.m_maxStackSize > 1)
                {
                    foreach (ItemDrop.ItemData other in ___m_inventory)
                    {
                        if (item.m_shared.m_name != other.m_shared.m_name && item.m_quality != other.m_quality)
                        {
                            continue;
                        }

                        QualityComponent targetComp = other.Extended()?.GetComponent<QualityComponent>();
                        if (targetComp == null || other.m_stack <= targetComp.Quality.Quantity)
                        {
                            continue;
                        }
                        var needed = other.m_stack - targetComp.Quality.Quantity;
                        var quals = sourceQuality.Shift(needed);
                        targetComp.Quality.Qualities.AddRange(quals);
                        targetComp.Save();

                        Debug.Assert(other.m_stack == targetComp.Quality.Quantity);
                        // DropTailFix(target);
                    }
                    if (sourceQuality.Quantity > 0) {
                        comp.Save();
                    }
                }
            }

            [HarmonyPatch(typeof(ItemDrop), "DropItem")]
            static void Postfix(ref ItemDrop __result, ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation) {
                DropTailFix(__result.m_itemData);
            }

            [HarmonyPatch(typeof(ItemStand), "UpdateAttach")]
            static void Postfix(ItemStand __instance, ItemDrop.ItemData ___m_queuedItem) {
                // Drop on the origin item as we can't easily access ZDO stored item
                DropTailFix(___m_queuedItem);
            }
        }
    }
}
