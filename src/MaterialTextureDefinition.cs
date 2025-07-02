using System;
using UnityEngine;

[Serializable]
public class MaterialTextureDefinition
{
	public string name;

	public PhysicMaterial material;

	public bool canApplyToNonTerrain;

	public string terrainTextureSuffix = string.Empty;

	public string blockTextureSuffix = string.Empty;

	public bool notOnTerrainBlocks;
}
