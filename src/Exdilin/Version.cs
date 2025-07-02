namespace Exdilin;

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

	public Version(int Major, int Minor)
		: this(Major, Minor, 0)
	{
	}

	public Version(string version)
	{
		string[] array = version.Split('.');
		Major = int.Parse(array[0]);
		Minor = int.Parse(array[1]);
		if (array.Length > 2)
		{
			Patch = int.Parse(array[2]);
		}
		else
		{
			Patch = 0;
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

	public override bool Equals(object obj)
	{
		if (obj is Version)
		{
			return this == obj as Version;
		}
		return false;
	}

	public static bool operator ==(Version a, Version b)
	{
		if (a.Major == b.Major && a.Minor == b.Minor)
		{
			return a.Patch == b.Patch;
		}
		return false;
	}

	public static bool operator !=(Version a, Version b)
	{
		if (a.Major == b.Major && a.Minor == b.Minor)
		{
			return a.Patch != b.Patch;
		}
		return true;
	}

	public static bool operator >=(Version a, Version b)
	{
		if (!(a > b))
		{
			return a == b;
		}
		return true;
	}

	public static bool operator <=(Version a, Version b)
	{
		if (!(a < b))
		{
			return a == b;
		}
		return true;
	}

	public static bool operator <(Version a, Version b)
	{
		if (a.Major < b.Major)
		{
			return true;
		}
		if (a.Major == b.Major)
		{
			if (a.Minor < b.Minor)
			{
				return true;
			}
			if (a.Minor == b.Minor && a.Patch < b.Patch)
			{
				return true;
			}
		}
		return false;
	}

	public static bool operator >(Version a, Version b)
	{
		if (a.Major > b.Major)
		{
			return true;
		}
		if (a.Major == b.Major)
		{
			if (a.Minor > b.Minor)
			{
				return true;
			}
			if (a.Minor == b.Minor && a.Patch > b.Patch)
			{
				return true;
			}
		}
		return false;
	}
}
