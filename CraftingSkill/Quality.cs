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
            float factor = ScalingFactor(config);
            if (config.QuantisedQuality)
            {
                QualityTier tier = GetQualityTier(factor);
                factor = tier.GetFactor();
                return String.Format(
                    "{0} ({1}) #debug: {2}",
                    GetQualityTier(factor).GetTooltip(),
                    (factor * 100f).ToString("0"),
                    DebugInfo()
                );
            }
            return String.Format(
                "{0} / 100 #debug: {1}",
                (factor * 100f).ToString("0"),
                DebugInfo()
            );
        }

        public float ScalingFactor(CraftingConfig config)
        {
            var factor = this.Skill;

            if (config.StochasticVariance > 0)
            {
                // map 0,1 to -1,+1
                var variance = 2.0f * (this.Variance - 0.5f);
                // scale by config, add to factor
                factor += (config.StochasticVariance/100.0f) * variance;
                // clamp invalid values (level 0 and 100)
                factor = Mathf.Clamp(factor, 0.0f, 1.0f);
            }

            if (config.QuantisedQuality)
            {
                QualityTier tier = GetQualityTier(factor);
                factor = tier.GetFactor();
            }
            return factor;
        }
        public static QualityTier GetQualityTier(float factor)
        {
            //Debug.Log("GetQualityTier:   ", factor);
            foreach (QualityTier tier in (QualityTier[])Enum.GetValues(typeof(QualityTier)))
            {
                //Debug.Log("GetQualityTier -> ", tier.GetFactor(), " >= ", factor, tier.GetFactor() >= factor);
                if (tier.GetFactor() >= factor)
                {
                    return tier;
                }
            }
            return QualityTier.NORMAL;
        }
    }
}
