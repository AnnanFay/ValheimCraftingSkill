using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CraftingSkill
{
    class ConfigVariable <T>
    {
        object backingStore;

        private string configSection;
        private string varName;
        private T defaultValue;
        private string configDescription;
        private bool localOnly;

        public T Value
        {
            get
            {
                return Traverse.Create(backingStore).Property("Value").GetValue<T>();
            }
        }

        public ConfigVariable(string configSection, string varName, T defaultValue, string configDescription = "", bool localOnly = false)
        {
            this.configSection = configSection;
            this.varName = varName;
            this.defaultValue = defaultValue;
            this.configDescription = configDescription;
            this.localOnly = localOnly;
        }

        public void init(Assembly assembly, ConfigFile config, string id) {
            if (assembly != null)
            {
                var method = assembly.GetType("ModConfigEnforcer.ConfigManager")
                    .GetMethods()
                    .First(x => x.Name == "RegisterModConfigVariable" && x.IsGenericMethod)
                    .MakeGenericMethod(typeof(T));
                backingStore = method.Invoke(null, new object[] { id, varName, defaultValue, configSection, configDescription, localOnly });
            }
            else
            {
                backingStore = config.Bind(configSection, varName, defaultValue, configDescription);
            }
        }
    }
}
