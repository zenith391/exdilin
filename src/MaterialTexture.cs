using System.Collections.Generic;
using UnityEngine;

public class MaterialTexture
{
	public static Dictionary<string, MaterialTextureDefinition> materialTextureDefinitions = new Dictionary<string, MaterialTextureDefinition>();

	public static bool CanMaterialTextureNonTerrain(string texture)
	{
		string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
		return GetPhysicMaterialDefinition(normalizedTexture)?.canApplyToNonTerrain ?? false;
	}

	public static PhysicMaterial GetPhysicMaterialTexture(string texture)
	{
		string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
		return GetPhysicMaterialDefinition(normalizedTexture)?.material;
	}

	public static MaterialTextureDefinition GetPhysicMaterialDefinition(string texture)
	{
		if (!materialTextureDefinitions.ContainsKey(texture))
		{
			MaterialTextureDefinitions component = Blocksworld.blocksworldDataContainer.GetComponent<MaterialTextureDefinitions>();
			MaterialTextureDefinition value = null;
			if (component != null)
			{
				MaterialTextureDefinition[] definitions = component.definitions;
				foreach (MaterialTextureDefinition materialTextureDefinition in definitions)
				{
					materialTextureDefinitions[materialTextureDefinition.name] = materialTextureDefinition;
					if (materialTextureDefinition.name == texture)
					{
						value = materialTextureDefinition;
					}
				}
			}
			materialTextureDefinitions[texture] = value;
		}
		return materialTextureDefinitions[texture];
	}

	public static bool IsPhysicMaterialTexture(string texture)
	{
		string normalizedTexture = Scarcity.GetNormalizedTexture(texture);
		MaterialTextureDefinition physicMaterialDefinition = GetPhysicMaterialDefinition(normalizedTexture);
		return physicMaterialDefinition != null;
	}
}
