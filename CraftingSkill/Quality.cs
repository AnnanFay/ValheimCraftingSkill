using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedItemDataFramework;
using UnityEngine;

namespace CraftingSkill
{
    [Serializable]
    public enum QualityTier
    {
        MIXED        = 0,
        AWFUL        = 1,
        POOR         = 2,
        NORMAL       = 3,
        FINE         = 4,
        SUPERIOR     = 5,
        EXCEPTIONAL  = 6,
        MASTERWORK   = 7,
        ARTIFACT     = 8
    }

    // Extension method to QualityTier enum
    public static class _QualityTier
    {
        public static string GetTooltip(this QualityTier tier) {
            switch ( tier ) {
                case QualityTier.MIXED:       return "Mixed";
                case QualityTier.AWFUL:       return "Awful";
                case QualityTier.POOR:        return "Poor";
                case QualityTier.NORMAL:      return "Normal";
                case QualityTier.FINE:        return "Fine";
                case QualityTier.SUPERIOR:    return "Superior";
                case QualityTier.EXCEPTIONAL: return "Exceptional";
                case QualityTier.MASTERWORK:  return "Masterwork";
                case QualityTier.ARTIFACT:    return "Artifact";
            }
            return "UNKNOWN";
        }
    }

    [Serializable]
    public class Quality
    {
        // Skill level when item is created
        public float Skill = 0;
        // Level of station used to create item
        public int StationLevel = 0;
        // Quantised item quality
        public QualityTier Tier = QualityTier.NORMAL;
        // So we can handle stack merges
        public int Quantity = 1;
        // For random outcomes
        public float Variance;

        public Quality()
        {
            Variance = UnityEngine.Random.value;
        }
        public string GetTooltip()
        {
            return String.Format(
                "{0} ({1})",
                this.Tier.GetTooltip(),
                (this.Skill * 100f).ToString("0")
            );
        }
    }
}
