using System;

namespace Exdilin
{
    public class Version
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }

        public Version(int Major, int Minor, int Patch)
        {
            this.Major = Major;
            this.Minor = Minor;
            this.Patch = Patch;
        }

        public Version(int Major, int Minor) : this(Major, Minor, 0) { }

		public Version(string version) {
			string[] split = version.Split('.');
			this.Major = int.Parse(split[0]);
			this.Minor = int.Parse(split[1]);
			if (split.Length > 2) {
				this.Patch = int.Parse(split[2]);
			} else {
				this.Patch = 0;
			}
		}

		public override string ToString()
        {
            return Major + "." + Minor + "." + Patch;
        }

        public override int GetHashCode()
        {
            return Major.GetHashCode() + Minor.GetHashCode() + Patch.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj is Version)
            {
                return this == (obj as Version);
            }
            return false;
        }
        // Operators

        public static bool operator==(Version a, Version b)
        {
            return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch;
        }

        public static bool operator!=(Version a, Version b)
        {
            return a.Major != b.Major || a.Minor != b.Minor || a.Patch != b.Patch;
        }

        public static bool operator>=(Version a, Version b)
        {
            return a > b || a == b;
        }

        public static bool operator <=(Version a, Version b)
        {
            return a < b || a == b;
        }

		public static bool operator <(Version a, Version b) {
			if (a.Major < b.Major) {
				return true;
			}
			if (a.Major == b.Major) {
				if (a.Minor < b.Minor) {
					return true;
				}
				if (a.Minor == b.Minor) {
					if (a.Patch < b.Patch) {
						return true;
					}
				}
			}
			return false;
		}

        public static bool operator>(Version a, Version b)
        {
			if (a.Major > b.Major) {
				return true;
			}
			if (a.Major == b.Major) {
				if (a.Minor > b.Minor) {
					return true;
				}
				if (a.Minor == b.Minor) {
					if (a.Patch > b.Patch) {
						return true;
					}
				}
			}
			return false;
        }
    }
}
