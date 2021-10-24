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
                // numbers are the bottoms of bins
                // So Awful is from 0 to 6
                // Poor from 7 22
                // Artifact is 100 only
                case QualityTier.AWFUL:       return 0.00f;
                case QualityTier.POOR:        return 0.07f;
                case QualityTier.NORMAL:      return 0.23f;
                case QualityTier.FINE:        return 0.39f;
                case QualityTier.SUPERIOR:    return 0.55f;
                case QualityTier.EXCEPTIONAL: return 0.71f;
                case QualityTier.MASTERWORK:  return 0.87f;
                case QualityTier.ARTIFACT:    return 1.00f;
            }
            return 0;
        }
    }
}
