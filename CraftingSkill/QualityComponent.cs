using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedItemDataFramework;
using fastJSON;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static CraftingSkill.CraftingSkillsPlugin;
using Object = UnityEngine.Object;

namespace CraftingSkill
{
    public class QualityComponent : BaseExtendedItemComponent
    {
        public StackableQuality Quality;

        private static readonly JSONParameters _saveParams = new JSONParameters { UseExtensions = false };

        public QualityComponent(ExtendedItemData parent) 
            : base(typeof(QualityComponent).AssemblyQualifiedName, parent)
        {
        }

        public void SetQuality(StackableQuality quality)
        {
            this.Quality = quality;
            Save();
        }

        public override BaseExtendedItemComponent Clone()
        {
            // ZLog.LogWarning("Clone! ???, stack: ???, quality stack:" + Quality.Quantity);
            var clone = MemberwiseClone() as QualityComponent;
            clone.Quality = this.Quality.Clone();
            return clone as BaseExtendedItemComponent;
        }

        public override string Serialize()
        {
            // ZLog.LogError($"[{nameof(QualityComponent)}] Serialising NOW1!!! {JSON.ToJSON(this.Quality, _saveParams)}");
            // ZLog.LogError($"[{nameof(QualityComponent)}] ................!!! {this.GetTooltip(new CraftingConfig())}");
            return JSON.ToJSON(this.Quality, _saveParams);
        }

        public override void Deserialize(string data)
        {
            try
            {
                this.Quality = JSON.ToObject<StackableQuality>(data, _saveParams);
            }
            catch (Exception e)
            {
                ZLog.LogError($"[{nameof(QualityComponent)}] Error while deserialising Quality json data! ({ItemData?.m_shared?.m_name}): {e?.Message}");
                LegacyDeserialize(data);
            }
        }

        public void LegacyDeserialize(string data)
        {
            // Loading list of qualities failed, likely because data is stored as old style singular
            try
            {
                var quality = JSON.ToObject<Quality>(data, _saveParams);
                var stackable = new StackableQuality();
                stackable.Qualities.Add(quality);
                this.Quality = stackable;
            }
            catch (Exception e)
            {
                ZLog.LogError($"[{nameof(QualityComponent)}] Could not deserialize Quality json data! ({ItemData?.m_shared?.m_name}): {e?.Message}");
                throw;
            }
        }

        public string GetTooltip(CraftingConfig config)
        {
            // Non-formated right hand side of "Quality: XXX"
            return this.Quality.GetTooltip(config);
        }

        public static void OnLoadExtendedItemData(ExtendedItemData itemdata)
        {
            QualityComponent _qualityComponent = itemdata.GetComponent<QualityComponent>();
            if (CraftingSkillsPlugin.config.DebugTooltips)
            {
                if (_qualityComponent != null)
                {
                    ZLog.LogWarning("Load EID! " + itemdata.m_shared.m_name + ", stack:" + itemdata.m_stack + ", quality stack:" + _qualityComponent.Quality.Quantity);
                } else {
                    ZLog.LogWarning("Load EID! " + itemdata.m_shared.m_name + ", stack:" + itemdata.m_stack);
                }
            }
            OnExtendedItemData(itemdata);
        }
        
        public static void OnNewExtendedItemData(ExtendedItemData itemdata)
        {
            QualityComponent _qualityComponent = itemdata.GetComponent<QualityComponent>();

            if (CraftingSkillsPlugin.config.DebugTooltips)
            {
                if (_qualityComponent != null)
                {
                    ZLog.LogWarning("New EID! " + itemdata + ", stack:" + itemdata.m_stack + ", quality stack:" + _qualityComponent.Quality.Quantity);
                } else {
                    ZLog.LogWarning("New EID! " + itemdata + ", stack:" + itemdata.m_stack);
                }
            }
            OnExtendedItemData(itemdata);
        }

        public static void OnExtendedItemData(ExtendedItemData itemdata)
        {
            // This gets triggered on generated items and crafted items

            Recipe recipe = ObjectDB.instance.GetRecipe(itemdata);
            if (!CraftingSkillsPlugin.isCraftingRecipe(recipe))
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            { 
                return;
            }

            string crafterName = itemdata.GetCrafterName();
            string playerName = player.GetPlayerName();
            if (crafterName != playerName)
            {
                return;
            }

            // items are added on player load, in which case there won't be a current station, unless the no cost cheat active
            // we still cannot avoid setting quality on hammers, torches, etc.

            var currentStation = player.GetCurrentCraftingStation();
            var requiredStation = recipe.GetRequiredStation(itemdata.m_quality);
            if (!player.NoCostCheat() && requiredStation != null && currentStation?.m_name != requiredStation?.m_name) {
                return;
            }

            QualityComponent qualityComponent = itemdata.GetComponent<QualityComponent>();
            if (qualityComponent != null)
            {
                return;
            }

            // If we get to this point, the current player has just crafted a new item
            var skill = player.GetSkillFactor((Skills.SkillType)CraftingSkillsPlugin.CRAFTING_SKILL_ID);
            var quantity = itemdata.m_stack;

            CraftingStation station = player.GetCurrentCraftingStation();
            int stationLevel = station == null ? 0 : station.GetLevel();

            var quality = new Quality(skill, quantity, stationLevel);
            // if we are upgrading an item
            if (InventoryGuiPatcherDoCrafting.preUpgradeQuality != null) {
                var prevVariance = InventoryGuiPatcherDoCrafting.preUpgradeQuality.Variance;
                quality.Variance = Math.Max(quality.Variance, prevVariance);
            }

            itemdata.AddComponent<QualityComponent>().SetQuality(new StackableQuality(quality));

            // Quality may have changed durability on our new item, so fix it
            itemdata.m_durability = itemdata.GetMaxDurability();
        }
    }
}
