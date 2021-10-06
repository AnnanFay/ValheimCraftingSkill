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

        public StackableQuality()
        {
            this.Qualities = new List<Quality>();
        }

        public StackableQuality(float skill, int quantity, int stationLevel)
        {
            this.Qualities = new List<Quality>();

            var quality = new Quality();
            quality.Skill = skill;
            quality.Quantity = quantity;
            quality.StationLevel = stationLevel;

            Qualities.Add(quality);
        }

        public int Quantity {
            get {
                return Qualities.Sum(q => q.Quantity);
            }
        }
        public float Skill
        {
            get
            {
                //return Qualities.Average(q => q.Skill);
                return Qualities.First().Skill;
            }
        }
        public int StationLevel {
            get
            {
                //return (int)Qualities.Average(q => q.StationLevel);
                return (int)Qualities.First().StationLevel;
            }
        }
        public float Variance {
            get
            {
                //return Qualities.Average(q => q.Variance);
                return Qualities.First().Variance;
            }
        }
        public string GetTooltip(CraftingConfig config)
        {
            if (this.Qualities.Count == 1)
            {
                return this.Qualities[0].GetTooltip(config);
            }
            else
            {
                return "Mixed!";
            }
        }
        public float ScalingFactor(CraftingConfig config)
        {
            return this.Qualities.First().ScalingFactor(config);
        }
    }
}
