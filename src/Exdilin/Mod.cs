using System.Collections.Generic;
using System.Reflection;

namespace Exdilin;

public abstract class Mod
{
	public static Mod ExecutionMod;

	public List<Dependency> Dependencies = new List<Dependency>();

	public abstract string Name { get; }

	public abstract Version Version { get; }

	public abstract string Id { get; }

	public virtual bool IsImportant { get; }

	public string Directory { get; set; }

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

	public virtual void PreInit()
	{
	}

	public virtual void ApplyPatches(Assembly assembly)
	{
	}

	public virtual void Init()
	{
	}

	public virtual void Register(RegisterType registerType)
	{
	}
}
