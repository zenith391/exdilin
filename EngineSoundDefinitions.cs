using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000200 RID: 512
public class EngineSoundDefinitions : MonoBehaviour
{
	// Token: 0x06001A24 RID: 6692 RVA: 0x000C1558 File Offset: 0x000BF958
	private void Init()
	{
		this.lookup = new Dictionary<string, EngineSoundDefinition>();
		foreach (EngineSoundDefinition engineSoundDefinition in this.definitions)
		{
			this.lookup.Add(engineSoundDefinition.name, engineSoundDefinition);
		}
	}

	// Token: 0x06001A25 RID: 6693 RVA: 0x000C15A4 File Offset: 0x000BF9A4
	public EngineSoundDefinition GetEngineSoundDefinition(string name)
	{
		if (this.lookup == null)
		{
			this.Init();
		}
		EngineSoundDefinition result = null;
		if (!this.lookup.TryGetValue(name, out result))
		{
			result = this.definitions[0];
		}
		return result;
	}

	// Token: 0x040015B3 RID: 5555
	public EngineSoundDefinition[] definitions;

	// Token: 0x040015B4 RID: 5556
	private Dictionary<string, EngineSoundDefinition> lookup;
}
