using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000E0 RID: 224
	public class BlockTerrain : Block
	{
		// Token: 0x060010DB RID: 4315 RVA: 0x000509E8 File Offset: 0x0004EDE8
		public BlockTerrain(List<List<Tile>> tiles) : base(tiles)
		{
			string text = base.BlockType();
			if (text != null)
			{
				if (text == "Terrain Cube" || text == "Terrain Wedge")
				{
					Materials.CubicProjection(this);
				}
			}
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x00050A40 File Offset: 0x0004EE40
		private List<GameObject> GetAllSubObjects()
		{
			if (this.subObjects == null)
			{
				this.subObjects = new List<GameObject>();
				this.subObjects.Add(this.go);
				this.subObjects.AddRange(this.subMeshGameObjects);
			}
			return this.subObjects;
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x00050A80 File Offset: 0x0004EE80
		protected override void FindSubMeshes()
		{
			base.FindSubMeshes();
			this.copiedRendererMaterial = new bool[this.subMeshGameObjects.Count + 1];
		}

		// Token: 0x060010DE RID: 4318 RVA: 0x00050AA0 File Offset: 0x0004EEA0
		public override void Destroy()
		{
			if (this.copiedRendererMaterial != null)
			{
				List<GameObject> allSubObjects = this.GetAllSubObjects();
				for (int i = 0; i < allSubObjects.Count; i++)
				{
					if (this.copiedRendererMaterial[i])
					{
						Material sharedMaterial = allSubObjects[i].GetComponent<Renderer>().sharedMaterial;
						Materials.materialCachePaint.Remove(sharedMaterial);
						Materials.materialCacheTexture.Remove(sharedMaterial);
					}
				}
			}
			base.Destroy();
		}

		// Token: 0x060010DF RID: 4319 RVA: 0x00050B13 File Offset: 0x0004EF13
		public override void OnCreate()
		{
			this.TextureTo(base.GetTexture(0), base.GetTextureNormal(), true, 0, true);
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x00050B2C File Offset: 0x0004EF2C
		public override void OnReconstructed()
		{
			this.TextureTo(base.GetTexture(0), base.GetTextureNormal(), true, 0, true);
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x00050B45 File Offset: 0x0004EF45
		public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x00050B48 File Offset: 0x0004EF48
		public void TweakVertexColors()
		{
			Color[] colors = this.mesh.colors;
			for (int i = 0; i < colors.Length; i++)
			{
				float num = colors[i].r + colors[i].g + colors[i].b;
				num = num / 4f + 0.2f;
				colors[i] = new Color(num, num, num);
			}
			this.mesh.colors = colors;
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x00050BCC File Offset: 0x0004EFCC
		public void TweakVertexColors2()
		{
			Color[] colors = this.mesh.colors;
			for (int i = 0; i < colors.Length; i++)
			{
				float num = (colors[i].r + colors[i].g + colors[i].b) / 3f;
				num = Mathf.Pow(num, 4f);
				colors[i] = new Color(num, num, num);
			}
			this.mesh.colors = colors;
		}

		// Token: 0x060010E4 RID: 4324 RVA: 0x00050C54 File Offset: 0x0004F054
		public void RandomizeVertexColors()
		{
			Vector3[] vertices = this.mesh.vertices;
			Color[] array = this.mesh.colors;
			if (array.Length != this.mesh.vertices.Length)
			{
				array = new Color[this.mesh.vertices.Length];
			}
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Random.seed = (int)(this.goT.position + vertices[i]).magnitude;
				float num = 0.5f;
				float num2 = 0.5f;
				float value = UnityEngine.Random.value;
				array[i].r = num + num2 * value;
				array[i].g = num + num2 * value;
				array[i].b = num + num2 * value;
			}
			this.mesh.colors = array;
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x00050D3C File Offset: 0x0004F13C
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			TileResultCode tileResultCode = base.PaintTo(paint, permanent, meshIndex);
			if (tileResultCode == TileResultCode.True && meshIndex < this.copiedRendererMaterial.Length)
			{
				Color terrainColor = BlockTerrain.GetTerrainColor(paint);
				if (terrainColor != Color.white)
				{
					Material material = this.GetIndexedGameObject(meshIndex).GetComponent<Renderer>().material;
					material.SetColor("_Color", terrainColor);
					Materials.materialCachePaint[material] = paint;
					this.copiedRendererMaterial[meshIndex] = true;
				}
			}
			return tileResultCode;
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x00050DB4 File Offset: 0x0004F1B4
		public static Color GetTerrainColor(string paint)
		{
			Color[] array;
			if (Blocksworld.colorDefinitions.TryGetValue("Terrain " + paint, out array))
			{
				return array[0];
			}
			return Color.white;
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x00050DF0 File Offset: 0x0004F1F0
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (texture == "Plain")
			{
				texture = base.GetDefaultTexture(0);
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
			TileResultCode tileResultCode = base.TextureTo(texture, normal, permanent, meshIndex, true);
			if (tileResultCode == TileResultCode.True && meshIndex < this.copiedRendererMaterial.Length && this.copiedRendererMaterial[meshIndex])
			{
				texture = base.GetTexture(meshIndex);
				GameObject indexedGameObject = this.GetIndexedGameObject(meshIndex);
				Materials.materialCacheTexture[indexedGameObject.GetComponent<Renderer>().material] = texture;
			}
			return tileResultCode;
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x00050EE8 File Offset: 0x0004F2E8
		private GameObject GetIndexedGameObject(int meshIndex)
		{
			GameObject result = this.go;
			if (meshIndex > 0 && meshIndex - 1 < this.subMeshGameObjects.Count)
			{
				result = this.subMeshGameObjects[meshIndex - 1];
			}
			return result;
		}

		// Token: 0x060010E9 RID: 4329 RVA: 0x00050F28 File Offset: 0x0004F328
		public virtual void SetFog(float start, float end)
		{
			for (int i = 0; i < this.copiedRendererMaterial.Length; i++)
			{
				GameObject gameObject = this.GetAllSubObjects()[i];
				if (this.copiedRendererMaterial[i])
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

		// Token: 0x060010EA RID: 4330 RVA: 0x00050FD4 File Offset: 0x0004F3D4
		public virtual void UpdateFogColor(Color newFogColor)
		{
			for (int i = 0; i < this.copiedRendererMaterial.Length; i++)
			{
				GameObject gameObject = this.GetAllSubObjects()[i];
				if (this.copiedRendererMaterial[i])
				{
					gameObject.GetComponent<Renderer>().material.SetColor("_FogColor", newFogColor);
				}
				else if (gameObject != null)
				{
					gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_FogColor", newFogColor);
				}
			}
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x00051051 File Offset: 0x0004F451
		public override bool DoubleTapToSelect()
		{
			return this.doubleTapToSelect;
		}

		// Token: 0x060010EC RID: 4332 RVA: 0x00051059 File Offset: 0x0004F459
		public virtual bool IsSolidTerrain()
		{
			return true;
		}

		// Token: 0x04000D45 RID: 3397
		private bool[] copiedRendererMaterial;

		// Token: 0x04000D46 RID: 3398
		public bool doubleTapToSelect = true;

		// Token: 0x04000D47 RID: 3399
		private List<GameObject> subObjects;
	}
}
