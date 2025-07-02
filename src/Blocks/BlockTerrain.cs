using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockTerrain : Block
{
	private bool[] copiedRendererMaterial;

	public bool doubleTapToSelect = true;

	private List<GameObject> subObjects;

	public BlockTerrain(List<List<Tile>> tiles)
		: base(tiles)
	{
		switch (BlockType())
		{
		case "Terrain Cube":
		case "Terrain Wedge":
			Materials.CubicProjection(this);
			break;
		}
	}

	private List<GameObject> GetAllSubObjects()
	{
		if (subObjects == null)
		{
			subObjects = new List<GameObject>();
			subObjects.Add(go);
			subObjects.AddRange(subMeshGameObjects);
		}
		return subObjects;
	}

	protected override void FindSubMeshes()
	{
		base.FindSubMeshes();
		copiedRendererMaterial = new bool[subMeshGameObjects.Count + 1];
	}

	public override void Destroy()
	{
		if (copiedRendererMaterial != null)
		{
			List<GameObject> allSubObjects = GetAllSubObjects();
			for (int i = 0; i < allSubObjects.Count; i++)
			{
				if (copiedRendererMaterial[i])
				{
					Material sharedMaterial = allSubObjects[i].GetComponent<Renderer>().sharedMaterial;
					Materials.materialCachePaint.Remove(sharedMaterial);
					Materials.materialCacheTexture.Remove(sharedMaterial);
				}
			}
		}
		base.Destroy();
	}

	public override void OnCreate()
	{
		TextureTo(GetTexture(), GetTextureNormal(), permanent: true, 0, force: true);
	}

	public override void OnReconstructed()
	{
		TextureTo(GetTexture(), GetTextureNormal(), permanent: true, 0, force: true);
	}

	public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public void TweakVertexColors()
	{
		Color[] colors = mesh.colors;
		for (int i = 0; i < colors.Length; i++)
		{
			float num = colors[i].r + colors[i].g + colors[i].b;
			num = num / 4f + 0.2f;
			colors[i] = new Color(num, num, num);
		}
		mesh.colors = colors;
	}

	public void TweakVertexColors2()
	{
		Color[] colors = mesh.colors;
		for (int i = 0; i < colors.Length; i++)
		{
			float f = (colors[i].r + colors[i].g + colors[i].b) / 3f;
			f = Mathf.Pow(f, 4f);
			colors[i] = new Color(f, f, f);
		}
		mesh.colors = colors;
	}

	public void RandomizeVertexColors()
	{
		Vector3[] vertices = mesh.vertices;
		Color[] array = mesh.colors;
		if (array.Length != mesh.vertices.Length)
		{
			array = new Color[mesh.vertices.Length];
		}
		for (int i = 0; i < array.Length; i++)
		{
			Random.seed = (int)(goT.position + vertices[i]).magnitude;
			float num = 0.5f;
			float num2 = 0.5f;
			float value = Random.value;
			array[i].r = num + num2 * value;
			array[i].g = num + num2 * value;
			array[i].b = num + num2 * value;
		}
		mesh.colors = array;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		TileResultCode tileResultCode = base.PaintTo(paint, permanent, meshIndex);
		if (tileResultCode == TileResultCode.True && meshIndex < copiedRendererMaterial.Length)
		{
			Color terrainColor = GetTerrainColor(paint);
			if (terrainColor != Color.white)
			{
				Material material = GetIndexedGameObject(meshIndex).GetComponent<Renderer>().material;
				material.SetColor("_Color", terrainColor);
				Materials.materialCachePaint[material] = paint;
				copiedRendererMaterial[meshIndex] = true;
			}
		}
		return tileResultCode;
	}

	public static Color GetTerrainColor(string paint)
	{
		if (Blocksworld.colorDefinitions.TryGetValue("Terrain " + paint, out var value))
		{
			return value[0];
		}
		return Color.white;
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (texture == "Plain")
		{
			texture = GetDefaultTexture();
		}
		if (Materials.textureInfos.ContainsKey(texture))
		{
			ShaderType shader = Materials.textureInfos[texture].shader;
			bool flag = false;
			if (shader == ShaderType.Metal || shader == ShaderType.NormalGold || shader == ShaderType.PulsateGlow || shader == ShaderType.NormalTerrain || shader == ShaderType.Terrain)
			{
				flag = true;
			}
			if (!flag)
			{
				MaterialTextureDefinition physicMaterialDefinition = MaterialTexture.GetPhysicMaterialDefinition(texture);
				if (physicMaterialDefinition == null || physicMaterialDefinition.notOnTerrainBlocks)
				{
					return TileResultCode.False;
				}
			}
		}
		TileResultCode tileResultCode = base.TextureTo(texture, normal, permanent, meshIndex, force: true);
		if (tileResultCode == TileResultCode.True && meshIndex < copiedRendererMaterial.Length && copiedRendererMaterial[meshIndex])
		{
			texture = GetTexture(meshIndex);
			GameObject indexedGameObject = GetIndexedGameObject(meshIndex);
			Materials.materialCacheTexture[indexedGameObject.GetComponent<Renderer>().material] = texture;
		}
		return tileResultCode;
	}

	private GameObject GetIndexedGameObject(int meshIndex)
	{
		GameObject result = go;
		if (meshIndex > 0 && meshIndex - 1 < subMeshGameObjects.Count)
		{
			result = subMeshGameObjects[meshIndex - 1];
		}
		return result;
	}

	public virtual void SetFog(float start, float end)
	{
		for (int i = 0; i < copiedRendererMaterial.Length; i++)
		{
			GameObject gameObject = GetAllSubObjects()[i];
			if (copiedRendererMaterial[i])
			{
				gameObject.GetComponent<Renderer>().material.SetFloat("_FogStart", start);
				gameObject.GetComponent<Renderer>().material.SetFloat("_FogEnd", end);
			}
			else if (gameObject != null)
			{
				gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_FogStart", start);
				gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_FogEnd", end);
			}
		}
	}

	public virtual void UpdateFogColor(Color newFogColor)
	{
		for (int i = 0; i < copiedRendererMaterial.Length; i++)
		{
			GameObject gameObject = GetAllSubObjects()[i];
			if (copiedRendererMaterial[i])
			{
				gameObject.GetComponent<Renderer>().material.SetColor("_FogColor", newFogColor);
			}
			else if (gameObject != null)
			{
				gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_FogColor", newFogColor);
			}
		}
	}

	public override bool DoubleTapToSelect()
	{
		return doubleTapToSelect;
	}

	public virtual bool IsSolidTerrain()
	{
		return true;
	}
}
