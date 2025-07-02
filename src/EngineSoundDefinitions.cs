using System.Collections.Generic;
using UnityEngine;

public class EngineSoundDefinitions : MonoBehaviour
{
	public EngineSoundDefinition[] definitions;

	private Dictionary<string, EngineSoundDefinition> lookup;

	private void Init()
	{
		lookup = new Dictionary<string, EngineSoundDefinition>();
		EngineSoundDefinition[] array = definitions;
		foreach (EngineSoundDefinition engineSoundDefinition in array)
		{
			lookup.Add(engineSoundDefinition.name, engineSoundDefinition);
		}
	}

	public EngineSoundDefinition GetEngineSoundDefinition(string name)
	{
		if (lookup == null)
		{
			Init();
		}
		EngineSoundDefinition value = null;
		if (!lookup.TryGetValue(name, out value))
		{
			value = definitions[0];
		}
		return value;
	}
}
