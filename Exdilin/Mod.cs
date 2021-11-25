using System.Reflection;
using System.Collections.Generic;
using System;

namespace Exdilin
{

    public class Dependency : IEquatable<Dependency>
    {
        public string Id;
        public Version MinimumVersion;
		public Version MaximumVersion;

        public Dependency(string id, Version minimum)
        {
            Id = id;
            MinimumVersion = minimum;
			MaximumVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue);
        }

		public static bool operator==(Dependency a, Dependency b) {
			return a.Id == b.Id && a.MinimumVersion == b.MinimumVersion && a.MaximumVersion == b.MaximumVersion;
		}

		public static bool operator !=(Dependency a, Dependency b) {
			return a.Id != b.Id || a.MinimumVersion != b.MinimumVersion || a.MaximumVersion != b.MaximumVersion;
		}

		public bool Equals(Dependency obj) {
			return this == obj;
		}

		public override bool Equals(object obj) {
			if (obj is Dependency) {
				return this == (obj as Dependency);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			int hashCode = -375925812;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
			hashCode = hashCode * -1521134295 + EqualityComparer<Version>.Default.GetHashCode(MinimumVersion);
			hashCode = hashCode * -1521134295 + EqualityComparer<Version>.Default.GetHashCode(MaximumVersion);
			return hashCode;
		}
	}

	public abstract class Mod
    {

        /*
         * The mod currently being executed.
         */
        public static Mod ExecutionMod;

        public static Mod GetById(string id)
        {
            foreach (Mod mod in ModLoader.mods)
            {
                if (mod.Id == id)
                {
                    return mod;
                }
            }
            return null;
        }

        public abstract string Name { get; }
        public abstract Version Version { get; }
        public abstract string Id { get; }
        public List<Dependency> Dependencies = new List<Dependency>();
		/// <summary>
		/// True if the mod changes gameplay of a world. A mod
		/// that doesn't change gameplay could be a mod that replaces
		/// game textures (a texture pack) in a way that doesn't
		/// affect the world gameplay, but a mod that adds any content
		/// or would limit worlds playability if non-present changes gameplay.
		/// </summary>
		/// <value><c>true</c> if changes gameplay; otherwise, <c>false</c>.</value>
		public virtual bool IsImportant { get; } = false;
		public string Directory { get; set; }

        public virtual void PreInit() { }
        public virtual void ApplyPatches(Assembly assembly) { }
        public virtual void Init() { }
        public virtual void Register(RegisterType registerType) { }
    }
}
