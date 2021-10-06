using System;

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
        public static string GetTooltip(this QualityTier tier)
        {
            switch (tier)
            {
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

        public static float GetFactor(this QualityTier tier)
        {
            switch (tier)
            {
                // numbers are the tops of bins
                // So Awful is from 0 to 6
                // Poor from 7 22
                // Artifact is 100 only
                case QualityTier.AWFUL:       return 0.06f;
                case QualityTier.POOR:        return 0.22f;
                case QualityTier.NORMAL:      return 0.38f;
                case QualityTier.FINE:        return 0.54f;
                case QualityTier.SUPERIOR:    return 0.70f;
                case QualityTier.EXCEPTIONAL: return 0.86f;
                case QualityTier.MASTERWORK:  return 0.99f;
                case QualityTier.ARTIFACT:    return 1.00f;
            }
            return 0;
        }
    }
}
