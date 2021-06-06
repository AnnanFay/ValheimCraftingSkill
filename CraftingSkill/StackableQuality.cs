using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedItemDataFramework;
using UnityEngine;

namespace CraftingSkill
{
    [Serializable]
    public class StackableQuality
    {
        public List<Quality> Qualities;

        // Defaults return weighted average over stack

        public int Quantity {
            get {
                return Qualities.Sum(q => q.Quantity);
            }
        }
        public float Skill {
            get {
                return Qualities.Average(q => q.Skill);
            }
        }
        public int StationLevel {
            get {
                return (int)Qualities.Average(q => q.StationLevel);
            }
        }
        public float Variance {
            get {
                return Qualities.Average(q => q.Variance);
            }
        }
        public QualityTier Tier {
            get {
                if (Qualities.Count > 0) {
                    return QualityTier.MIXED;
                } else {
                    return Qualities[0].Tier;
                }
            }
        }
        public string GetTooltip()
        {
            if (this.Qualities.Count == 1) {
                return this.Qualities[0].GetTooltip();
            } else {
                return "Mixed!";
            }
        }
    }
}
