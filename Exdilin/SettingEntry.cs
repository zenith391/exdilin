using System;

namespace Exdilin
{
    public class SettingEntry
    {
        public Mod mod;

        public string value;
        public string defaultValue = String.Empty;

        public string id = String.Empty;
        public string label = String.Empty;

        // Those value should not be edited by the mod
        public string _old = null;
        public string _valedit = null;

        public SettingEntry(string id, string label, string defaultValue)
        {
            this.id = id;
            this.label = label;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }
    }
}
