using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001B4 RID: 436
public class MaterialTexture
{
	// Token: 0x060017BE RID: 6078 RVA: 0x000A6E30 File Offset: 0x000A5230
	public static bool CanMaterialTextureNonTerrain(string texture)
	{
		string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
		MaterialTextureDefinition physicMaterialDefinition = MaterialTexture.GetPhysicMaterialDefinition(normalizedTexture);
		return physicMaterialDefinition != null && physicMaterialDefinition.canApplyToNonTerrain;
	}

	// Token: 0x060017BF RID: 6079 RVA: 0x000A6E5C File Offset: 0x000A525C
	public static PhysicMaterial GetPhysicMaterialTexture(string texture)
	{
		string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
		MaterialTextureDefinition physicMaterialDefinition = MaterialTexture.GetPhysicMaterialDefinition(normalizedTexture);
		if (physicMaterialDefinition != null)
		{
			return physicMaterialDefinition.material;
		}
		return null;
	}

	// Token: 0x060017C0 RID: 6080 RVA: 0x000A6E88 File Offset: 0x000A5288
	public static MaterialTextureDefinition GetPhysicMaterialDefinition(string texture)
	{
		if (!MaterialTexture.materialTextureDefinitions.ContainsKey(texture))
		{
			MaterialTextureDefinitions component = Blocksworld.blocksworldDataContainer.GetComponent<MaterialTextureDefinitions>();
			MaterialTextureDefinition value = null;
			if (component != null)
			{
				foreach (MaterialTextureDefinition materialTextureDefinition in component.definitions)
				{
					MaterialTexture.materialTextureDefinitions[materialTextureDefinition.name] = materialTextureDefinition;
					if (materialTextureDefinition.name == texture)
					{
						value = materialTextureDefinition;
					}
				}
			}
			MaterialTexture.materialTextureDefinitions[texture] = value;
		}
		return MaterialTexture.materialTextureDefinitions[texture];
	}

	// Token: 0x060017C1 RID: 6081 RVA: 0x000A6F20 File Offset: 0x000A5320
	public static bool IsPhysicMaterialTexture(string texture)
	{
		string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
		MaterialTextureDefinition physicMaterialDefinition = MaterialTexture.GetPhysicMaterialDefinition(normalizedTexture);
		return physicMaterialDefinition != null;
	}

	// Token: 0x0400129F RID: 4767
	public static Dictionary<string, MaterialTextureDefinition> materialTextureDefinitions = new Dictionary<string, MaterialTextureDefinition>();
}
