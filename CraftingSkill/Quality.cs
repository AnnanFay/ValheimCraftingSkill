using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedItemDataFramework;
using UnityEngine;

namespace CraftingSkill
{

    [Serializable]
    public class Quality
    {
        // Skill level when item is created
        public float Skill = 0;
        // So we can handle stack merges
        public int Quantity = 1;
        // Level of station used to create item
        public int StationLevel = 0;
        // For random outcomes
        public float Variance;

        public Quality()
        {
            Variance = UnityEngine.Random.value;
        }
        public Quality Clone()
        {
            return MemberwiseClone() as Quality;
        }

        public string DebugInfo() { 
            return String.Format(
                   "x{3} x{0} ~{1} L{2}",
                   Skill,
                   Variance,
                   StationLevel,
                   Quantity
               );
        }

        public string GetTooltip(CraftingConfig config)
        {
            string debugInfo = config.DebugTooltips ? " #debug: " + DebugInfo() : "";

            float factor = ScalingFactor(config.StochasticVariance, config.QuantisedQuality);
            if (config.QuantisedQuality)
            {
                QualityTier tier = GetQualityTier(factor);
                float preQuantisation = ScalingFactor(config.StochasticVariance, false);
                return String.Format(
                    "{0} ({1} [{2}]){3}",
                    (preQuantisation * 100f).ToString("0"),
                    tier.GetTooltip(),
                    (tier.GetFactor() * 100f).ToString("0"),
                    debugInfo
                );
            }
            return String.Format(
                "{0} / 100 ({1}){2}",
                (factor * 100f).ToString("0"),
                GetQualityTier(factor).GetTooltip(),
                debugInfo
            );
        }

        public float ScalingFactor(float StochasticVariance, bool QuantisedQuality)
        {
            var factor = this.Skill;

            if (StochasticVariance > 0)
            {
                // map 0,1 to -1,+1
                var variance = 2.0f * (this.Variance - 0.5f);
                // scale by config, add to factor
                factor += (StochasticVariance / 100.0f) * variance;
                // clamp invalid values (level 0 and 100)
                factor = Mathf.Clamp(factor, 0.0f, 1.0f);
            }

            if (QuantisedQuality)
            {
                QualityTier tier = GetQualityTier(factor);
                factor = tier.GetFactor();
            }
            return factor;
        }

        public static QualityTier GetQualityTier(float factor)
        {
            //Debug.Log("GetQualityTier:   ", factor);
            var tiers = (QualityTier[])Enum.GetValues(typeof(QualityTier));
            foreach (QualityTier tier in tiers.OrderByDescending(x => x))
            {
                //Debug.Log("GetQualityTier -> ", tier.GetFactor(), " >= ", factor, tier.GetFactor() >= factor);
                if (tier.GetFactor() <= factor)
                {
                    return tier;
                }
            }
            return QualityTier.NORMAL;
        }
    }
}
