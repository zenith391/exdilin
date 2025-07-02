using System;
using System.Collections.Generic;

namespace Exdilin;

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

	public static bool operator ==(Dependency a, Dependency b)
	{
		if (a.Id == b.Id && a.MinimumVersion == b.MinimumVersion)
		{
			return a.MaximumVersion == b.MaximumVersion;
		}
		return false;
	}

	public static bool operator !=(Dependency a, Dependency b)
	{
		if (!(a.Id != b.Id) && !(a.MinimumVersion != b.MinimumVersion))
		{
			return a.MaximumVersion != b.MaximumVersion;
		}
		return true;
	}

	public bool Equals(Dependency obj)
	{
		return this == obj;
	}

	public override bool Equals(object obj)
	{
		if (obj is Dependency)
		{
			return this == obj as Dependency;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = -375925812;
		num = num * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
		num = num * -1521134295 + EqualityComparer<Version>.Default.GetHashCode(MinimumVersion);
		return num * -1521134295 + EqualityComparer<Version>.Default.GetHashCode(MaximumVersion);
	}
}
