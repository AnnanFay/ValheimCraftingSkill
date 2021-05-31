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
        public Quality Quality;

        private static readonly JSONParameters _saveParams = new JSONParameters { UseExtensions = false };

        public QualityComponent(ExtendedItemData parent) 
            : base(typeof(QualityComponent).AssemblyQualifiedName, parent)
        {
        }

        public void SetQuality(Quality quality)
        {
            Quality = quality;
            Save();
        }

        public override string Serialize()
        {
            return JSON.ToJSON(Quality, _saveParams);
        }

        public override void Deserialize(string data)
        {
            try
            {
                Quality = JSON.ToObject<Quality>(data, _saveParams);
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

        public static void OnNewExtendedItemData(ExtendedItemData itemdata)
        {
            // ZLog.LogError("OnNewExtendedItemData!");

            if (itemdata.GetComponent<QualityComponent>() != null)
            {
                return;
            }

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
            if (crafterName != playerName) {
                return;
            }

            var quality = new Quality();
            quality.Skill = player.GetSkillFactor((Skills.SkillType)CraftingSkillsPlugin.CRAFTING_SKILL_ID);

            CraftingStation station = player.GetCurrentCraftingStation();
            if (station) {
                quality.StationLevel = station.GetLevel();
            }
            
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
