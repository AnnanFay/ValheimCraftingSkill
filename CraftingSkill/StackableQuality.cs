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

        public StackableQuality Clone()
        {
            var clone = MemberwiseClone() as StackableQuality;
            clone.Qualities = new List<Quality>(this.Qualities.Count);
            this.Qualities.ForEach((item) => {
                clone.Qualities.Add(item.Clone());
            });
            return clone;
        }

        public int Quantity
        {
            get {
                if (Qualities.Count == 0)
                {
                    return 0;
                }
                return Qualities.Sum(q => q.Quantity);
            }
        }
        public float Skill
        {
            get
            {
                if (Qualities.Count == 0)
                {
                    return 0;
                }
                //return Qualities.Average(q => q.Skill);
                return Qualities.First().Skill;
            }
        }
        public int StationLevel
        {
            get
            {
                if (Qualities.Count == 0)
                {
                    return 0;
                }
                //return (int)Qualities.Average(q => q.StationLevel);
                return (int)Qualities.First().StationLevel;
            }
        }
        public float Variance
        {
            get
            {
                if (Qualities.Count == 0)
                {
                    return 0;
                }
                //return Qualities.Average(q => q.Variance);
                return Qualities.First().Variance;
            }
        }
        public float ScalingFactor(CraftingConfig config)
        {
                if (Qualities.Count == 0)
                {
                    return 0;
                }
            return this.Qualities.First().ScalingFactor(config);
        }


        public string DebugInfo()
        {
            return " Stack{\n  " + (
                    String.Join(
                        "\n  ",
                        this.Qualities.Select(x => x.DebugInfo())
                    )
                ) + "\n}";
        }

        public string GetTooltip(CraftingConfig config)
        {
            if (this.Qualities.Count == 0)
            {
                return "CORRUPT QUALITY!" + DebugInfo();
            }
            else if (this.Qualities.Count == 1)
            {
                return this.Qualities[0].GetTooltip(config) + DebugInfo();
            }
            else
            {
                return "Mixed!" + DebugInfo();
            }
        }

        internal void MergeInto(StackableQuality other)
        {
            other.Qualities.ForEach(q => {
                // if functionally the same as tail we merge into
                // otherwise we preserve order and stack on end
                // (prevents a bit of bloat)
                var tail = this.Qualities.Last();
                if (tail.Variance == q.Variance && tail.Skill == q.Skill && tail.Variance == q.Variance && tail.StationLevel == q.StationLevel) {
                    tail.Quantity += q.Quantity;
                } else {
                    this.Qualities.Add(q.Clone());
                }
            });
        }

        internal List<Quality> Shift(int n)
        {
            // remove and return n from start
            var shifted = new List<Quality>();

            if (n > this.Quantity)
            {
                n = this.Quantity;
            }

            int needed = n;
            while (needed > 0)
            {
                var first = this.Qualities.First();
                Quality toAdd;
                if (first.Quantity <= needed)
                {
                    this.Qualities.Remove(first);
                    toAdd = first;
                }
                else
                {
                    first.Quantity -= needed;
                    toAdd = first.Clone();
                    toAdd.Quantity = needed;
                }
                shifted.Add(toAdd);
                needed -= toAdd.Quantity;
            }

            // Avoid killing ourself on corrupt data by always preserving one item
            if (this.Qualities.Count == 0)
            {
                this.Qualities.Add(shifted.Last().Clone());
            }
            return shifted;
        }

        internal List<Quality> Pop(int n)
        {
            // remove and return n from end
            // (Bad implemention here)
            this.Qualities.Reverse();
            var popped = this.Shift(n);
            this.Qualities.Reverse();
            return popped;
        }
    }
}
