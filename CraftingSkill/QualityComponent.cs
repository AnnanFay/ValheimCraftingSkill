using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedItemDataFramework;
using fastJSON;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
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

        public override string Serialize()
        {
            return JSON.ToJSON(this.Quality, _saveParams);
        }

        public override void Deserialize(string data)
        {
            try
            {
                this.Quality = JSON.ToObject<StackableQuality>(data, _saveParams);
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                ZLog.LogError($"[{nameof(QualityComponent)}] Could not deserialize Quality json data! ({ItemData?.m_shared?.m_name})");
                throw;
            }
        }

        public override BaseExtendedItemComponent Clone()
        {
            return MemberwiseClone() as BaseExtendedItemComponent;
        }

        public string GetTooltip(CraftingConfig config)
        {
            // Non-formated right hand side of "Quality: XXX"
            return this.Quality.GetTooltip(config);
        }

        public static void OnNewExtendedItemData(ExtendedItemData itemdata)
        {
            // ZLog.LogError("OnNewExtendedItemData!");
            Recipe recipe = ObjectDB.instance.GetRecipe(itemdata);
            if (recipe == null)
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

            QualityComponent qualityComponent = itemdata.GetComponent<QualityComponent>();
            if (qualityComponent != null)
            {
                ZLog.LogWarning("qualityComponent is not null during ExtendedItemData creation");
                return;
            }

            var skill = player.GetSkillFactor((Skills.SkillType)CraftingSkillsPlugin.CRAFTING_SKILL_ID);
            var quantity = itemdata.m_stack;

            CraftingStation station = player.GetCurrentCraftingStation();
            int stationLevel = station == null ? 0 : station.GetLevel();

            var quality = new StackableQuality(skill, quantity, stationLevel);

            itemdata.AddComponent<QualityComponent>().SetQuality(quality);
        }

        public static void OnLoadExtendedItemData(ExtendedItemData itemdata)
        {
            // ZLog.LogError("OnLoadExtendedItemData!");
            // QualityComponent qualityComponent = itemdata.GetComponent<QualityComponent>();
            // if (qualityComponent != null)
            // {
            //     ZLog.LogError("qualityComponent! q.Skill=" + qualityComponent.Quality.Skill);
            // }
        }
    }
}
