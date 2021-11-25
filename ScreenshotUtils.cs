using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200027F RID: 639
public class ScreenshotUtils
{
	// Token: 0x06001DF0 RID: 7664 RVA: 0x000D5EF0 File Offset: 0x000D42F0
	public static bool IsBusy()
	{
		return ScreenshotUtils.busy;
	}

	// Token: 0x06001DF1 RID: 7665 RVA: 0x000D5EF7 File Offset: 0x000D42F7
	public static void OnStopAllCoroutines()
	{
		ScreenshotUtils.busy = false;
	}

	// Token: 0x06001DF2 RID: 7666 RVA: 0x000D5EFF File Offset: 0x000D42FF
	public static void GenerateModelIconTexture(List<List<List<Tile>>> model, bool hd, Action<Texture2D> callback)
	{
		Blocksworld.bw.StartCoroutine(ScreenshotUtils.GenerateModelIcon(model, (!hd) ? ScreenshotUtils.iconSizeSD : (ScreenshotUtils.iconSizeSD * 2), true, false, callback));
	}

	// Token: 0x06001DF3 RID: 7667 RVA: 0x000D5F2C File Offset: 0x000D432C
	public static void GenerateModelSnapshotTexture(List<List<List<Tile>>> model, bool hd, Action<Texture2D> callback)
	{
		Blocksworld.bw.StartCoroutine(ScreenshotUtils.GenerateModelIcon(model, (!hd) ? ScreenshotUtils.snapshotSizeSD : (ScreenshotUtils.snapshotSizeSD * 2), true, true, callback));
	}

	// Token: 0x06001DF4 RID: 7668 RVA: 0x000D5F59 File Offset: 0x000D4359
	public static void GenerateModelIconTexture(List<List<List<Tile>>> model, int size, bool withOutline, bool perspectiveCamera, Action<Texture2D> callback)
	{
		Blocksworld.bw.StartCoroutine(ScreenshotUtils.GenerateModelIcon(model, size, withOutline, perspectiveCamera, callback));
	}

	// Token: 0x06001DF5 RID: 7669 RVA: 0x000D5F74 File Offset: 0x000D4374
	private static IEnumerator GenerateModelIcon(List<List<List<Tile>>> model, int size, bool withOutline, bool perspectiveCamera, Action<Texture2D> callback)
	{
		while (ScreenshotUtils.busy)
		{
			yield return null;
		}
		ScreenshotUtils.busy = true;
		List<Block> volumeBlocks = new List<Block>();
		List<Block> invisibleBlocks = new List<Block>();
		List<Block> blocks = new List<Block>();
		for (int i = 0; i < model.Count; i++)
		{
			List<List<Tile>> info = model[i];
			List<List<Tile>> infoCopy = Util.CopyTiles(info);
			Block b = Block.NewBlock(infoCopy, false, false);
			if (b is BlockPosition || (b is BlockAbstractParticles && (b as BlockAbstractParticles).HidingEmitter()))
			{
				volumeBlocks.Add(b);
			}
			else
			{
				bool bVisible = false;
				for (int k = 0; k < ((b.childMeshes != null) ? (b.childMeshes.Count + 1) : 1); k++)
				{
					string texture = b.GetTexture(k);
					if (!(texture == "Invisible"))
					{
						bVisible = true;
						break;
					}
				}
				if (bVisible)
				{
					b.MakeInvisibleVisible();
					if (b is BlockAnimatedCharacter)
					{
						(b as BlockAnimatedCharacter).PrepareForModelIconRender(Layer.IconGeneration);
					}
					b.go.SetLayer(Layer.IconGeneration, true);
					if (b is BlockAbstractMovingPlatform)
					{
						(b as BlockAbstractMovingPlatform).go.GetComponent<Renderer>().enabled = false;
						Vector3 scale = b.GetScale();
						BoxCollider boxCollider = (BoxCollider)b.go.GetComponent<Collider>();
						boxCollider.size = Vector3.one;
						boxCollider.center = -Vector3.forward * (scale.z - 1f) * 0.5f;
					}
					blocks.Add(b);
					if (i % 10 == 0)
					{
						yield return null;
					}
				}
				else
				{
					invisibleBlocks.Add(b);
				}
			}
		}
		if (blocks.Count == 0 && volumeBlocks.Count == 0)
		{
			for (int j = 0; j < invisibleBlocks.Count; j++)
			{
				Block b2 = invisibleBlocks[j];
				if (b2 is BlockPosition)
				{
					volumeBlocks.Add(b2);
				}
				else
				{
					b2.MakeInvisibleVisible();
					b2.go.SetLayer(Layer.IconGeneration, true);
					blocks.Add(b2);
					if (j % 10 == 0)
					{
						yield return null;
					}
				}
			}
		}
		else
		{
			foreach (Block block in invisibleBlocks)
			{
				block.Destroy();
				if (block is BlockTerrain)
				{
					BWSceneManager.RemoveTerrainBlock((BlockTerrain)block);
				}
			}
		}
		invisibleBlocks.Clear();
		bool onlyVolumes = blocks.Count == 0;
		if (onlyVolumes)
		{
			withOutline = false;
			for (int l = 0; l < volumeBlocks.Count; l++)
			{
				volumeBlocks[l].go.SetLayer(Layer.IconGeneration, true);
				blocks.Add(volumeBlocks[l]);
			}
		}
		else
		{
			for (int m = 0; m < volumeBlocks.Count; m++)
			{
				volumeBlocks[m].Destroy();
			}
			volumeBlocks.Clear();
		}
		BlockGroups.GatherBlockGroups(blocks);
		foreach (Block block2 in blocks)
		{
			if (block2 is BlockGrouped)
			{
				block2.Update();
			}
		}
		yield return null;
		GameObject cameraGo = new GameObject();
		Camera cam = cameraGo.AddComponent<Camera>();
		cam.enabled = false;
		float bright = 0.980392158f;
		Color backgroundColor = new Color(1f, bright, bright, 1f);
		cam.backgroundColor = backgroundColor;
		cam.clearFlags = CameraClearFlags.Color;
		Bounds targetBounds = default(Bounds);
		List<Vector3> toProject = new List<Vector3>();
		for (int n = 0; n < blocks.Count; n++)
		{
			Block block3 = blocks[n];
			Bounds bounds = block3.go.GetComponent<Collider>().bounds;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			toProject.AddRange(new List<Vector3>
			{
				min,
				max,
				new Vector3(min.x, min.y, max.z),
				new Vector3(min.x, max.y, min.z),
				new Vector3(min.x, max.y, max.z),
				new Vector3(max.x, min.y, min.z),
				new Vector3(max.x, min.y, max.z),
				new Vector3(max.x, max.y, min.z)
			});
			if (n == 0)
			{
				targetBounds = bounds;
			}
			else
			{
				targetBounds.Encapsulate(bounds);
			}
		}
		yield return null;
		cam.enabled = true;
		cam.cullingMask = 16384;
		if (perspectiveCamera)
		{
			ScreenshotUtils.SetupModelScreenshotPerspectiveCamera(cam, targetBounds, toProject);
		}
		else
		{
			ScreenshotUtils.SetupModelScreenshotOrthographicCamera(cam, targetBounds, toProject);
			foreach (Block block4 in blocks)
			{
				MeshRenderer[] componentsInChildren = block4.go.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer meshRenderer in componentsInChildren)
				{
					Shader shader = meshRenderer.material.shader;
					if (ScreenshotUtils.toonShaders.ContainsKey(shader))
					{
						meshRenderer.material.shader = ScreenshotUtils.toonShaders[shader];
						if (shader == ScreenshotUtils.metalShader)
						{
							if (ScreenshotUtils.lowMetalCubemap == null)
							{
								ScreenshotUtils.lowMetalCubemap = (Cubemap)Resources.Load("Cubemaps/Low Metal", typeof(Cubemap));
							}
							Material material = meshRenderer.material;
							if (material.HasProperty("_CubeMapTex"))
							{
								material.SetTexture("_CubeMapTex", ScreenshotUtils.lowMetalCubemap);
							}
						}
					}
				}
			}
		}
		GameObject lightGO = Blocksworld.directionalLight;
		Transform lightT = lightGO.transform;
		Light light = lightGO.GetComponent<Light>();
		if (Blocksworld.renderingShadows)
		{
			light = Blocksworld.overheadLight;
			lightT = light.transform;
			lightGO = light.gameObject;
		}
		Quaternion oldLightRotation = lightT.rotation;
		Color oldLightColor = light.color;
		float oldLightIntensity = light.intensity;
		float oldLightShadowStrength = light.shadowStrength;
		float oldFogEnd = Blocksworld.fogEnd;
		float oldFogStart = Blocksworld.fogStart;
		float oldFogMultiplier = Blocksworld.fogMultiplier;
		Blocksworld.bw.SetFog(2000f, 3000f);
		Blocksworld.bw.SetFogMultiplier(1f);
		light.color = Color.white;
		light.intensity = 1f;
		light.shadowStrength = 0.25f;
		Quaternion lightRotation = Quaternion.Euler(40f, 20f, 0f);
		lightT.rotation = cam.transform.rotation * lightRotation;
		Texture2D imageTexture = new Texture2D(size, size);
		Blocksworld.RenderScreenshot(imageTexture, cam, null, FilterMode.Point);
		cam.enabled = false;
		lightT.rotation = oldLightRotation;
		light.color = oldLightColor;
		light.intensity = oldLightIntensity;
		light.shadowStrength = oldLightShadowStrength;
		Blocksworld.bw.SetFog(oldFogStart, oldFogEnd);
		Blocksworld.bw.SetFogMultiplier(oldFogMultiplier);
		yield return null;
		foreach (Block block5 in blocks)
		{
			block5.Destroy();
			if (block5 is BlockTerrain)
			{
				BWSceneManager.RemoveTerrainBlock((BlockTerrain)block5);
			}
		}
		if (ScreenshotUtils.lowMetalCubemap != null)
		{
			ScreenshotUtils.lowMetalCubemap = null;
		}
		yield return null;
		imageTexture.Apply();
		Color[] pixels = imageTexture.GetPixels();
		float colorDistThreshold = 0.01f;
		Color borderColor = new Color(0.35f, 0.35f, 0.35f, 1f);
		for (int num2 = 0; num2 < pixels.Length; num2++)
		{
			if (ScreenshotUtils.ColorsClose(pixels[num2], backgroundColor, colorDistThreshold))
			{
				pixels[num2].a = 0f;
			}
		}
		backgroundColor.a = 0f;
		if (withOutline)
		{
			Color[] array2 = (Color[])pixels.Clone();
			for (int num3 = 0; num3 < pixels.Length; num3++)
			{
				if (ScreenshotUtils.ColorsClose(pixels[num3], backgroundColor, colorDistThreshold))
				{
					int x = num3 % size;
					int y = num3 / size;
					if (ScreenshotUtils.AddBorder(x, y, size, pixels))
					{
						array2[num3] = borderColor;
					}
				}
			}
			Color[] array3 = array2;
			array2 = pixels;
			pixels = array3;
			float[] array4 = new float[]
			{
				1f,
				2f,
				1f,
				2f,
				4f,
				2f,
				1f,
				2f,
				1f
			};
			float num4 = 0f;
			for (int num5 = 0; num5 < array4.Length; num5++)
			{
				num4 += array4[num5];
			}
			float m2 = 1f / num4;
			for (int num6 = 0; num6 < pixels.Length; num6++)
			{
				int num7 = num6 % size;
				int num8 = num6 / size;
				Color color = pixels[num6];
				if (color.a < 0.05f && num7 > 0 && num7 < size - 1 && num8 > 0 && num8 < size - 1)
				{
					array2[num6] = ScreenshotUtils.Filter(num7, num8, size, pixels, array4, m2);
				}
				else
				{
					array2[num6] = pixels[num6];
				}
			}
			pixels = array2;
		}
		imageTexture.SetPixels(pixels);
		imageTexture.Apply();
		UnityEngine.Object.Destroy(cameraGo);
		if (callback != null)
		{
			callback(imageTexture);
		}
		ScreenshotUtils.busy = false;
		yield break;
	}

	// Token: 0x06001DF6 RID: 7670 RVA: 0x000D5FAC File Offset: 0x000D43AC
	private static Vector3 GetPixelCorrectLinesNormalizedOffset()
	{
		Vector3 vector = -Blocksworld.cameraTransform.forward;
		vector = Util.ProjectOntoPlane(vector, Vector3.up).normalized;
		Vector3[] array = new Vector3[]
		{
			Vector3.forward,
			Vector3.back,
			Vector3.right,
			Vector3.left
		};
		float num = -1f;
		Vector3 a = Vector3.forward;
		Quaternion rotation = Quaternion.AngleAxis(45f, Vector3.up);
		foreach (Vector3 point in array)
		{
			Vector3 lhs = rotation * point;
			float num2 = Vector3.Dot(lhs, vector);
			if (num2 > num)
			{
				num = num2;
				a = lhs.normalized;
			}
		}
		float d = Mathf.Sqrt(3f) / 2f;
		return (d * a + 0.5f * Vector3.up).normalized;
	}

	// Token: 0x06001DF7 RID: 7671 RVA: 0x000D60D8 File Offset: 0x000D44D8
	private static Vector3 FindModelForward(List<Block> blocks, Vector3 defaultIfNotFound)
	{
		Vector3 vector = Vector3.forward;
		int num = int.MaxValue;
		bool flag = false;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block is BlockCharacter)
			{
				Transform transform = block.go.transform;
				if (Mathf.Abs(transform.forward.y) < 0.01f)
				{
					vector = transform.forward;
					num = 0;
					flag = true;
				}
			}
			else if (num > 1)
			{
				if (block is BlockAbstractWheel)
				{
					Vector3 vector2 = block.Scale();
					if (Mathf.Abs(vector2.y - vector2.z) < 0.01f)
					{
						Transform transform2 = block.go.transform;
						if (Mathf.Abs(transform2.forward.y) < 0.01f)
						{
							vector = transform2.forward;
							num = 1;
							flag = true;
						}
					}
				}
				else if (block is BlockTankTreadsWheel)
				{
					Vector3 forward = block.go.transform.forward;
					if (Mathf.Abs(forward.y) < 0.01f)
					{
						vector = forward;
						num = 1;
						flag = true;
					}
				}
				if (num > 2)
				{
					BlockAbstractAntiGravity blockAbstractAntiGravity = block as BlockAbstractAntiGravity;
					if (blockAbstractAntiGravity != null)
					{
						Vector3 vector3 = blockAbstractAntiGravity.rotation * blockAbstractAntiGravity.go.transform.forward;
						if (Mathf.Abs(vector3.y) < 0.01f)
						{
							vector = vector3;
							num = 2;
							flag = true;
						}
					}
				}
			}
		}
		return (!flag) ? defaultIfNotFound : vector;
	}

	// Token: 0x06001DF8 RID: 7672 RVA: 0x000D6274 File Offset: 0x000D4674
	private static void SetupModelScreenshotPerspectiveCamera(Camera cam, Bounds targetBounds, List<Vector3> toProject)
	{
		Vector3 a = -Blocksworld.cameraTransform.forward;
		float num = 0.01f;
		float a2 = 2000f;
		float fov = 45f;
		float aspect = 1f;
		float num2 = targetBounds.size.magnitude * 1.25f + num;
		Vector3 b = a * num2;
		cam.transform.position = targetBounds.center + b;
		cam.transform.LookAt(targetBounds.center);
		cam.projectionMatrix = Matrix4x4.Perspective(fov, aspect, num, Mathf.Max(a2, num2));
		int height = Screen.height;
		int width = Screen.width;
		int num3 = 20;
		for (int i = 0; i < num3; i++)
		{
			float num4 = ScreenshotUtils.CalculateMinGap(cam, width, height, toProject);
			if (num4 <= 0.1f * (float)Mathf.Min(height, width))
			{
				break;
			}
			num2 *= 0.95f;
			b = a * num2;
			cam.transform.position = targetBounds.center + b;
		}
	}

	// Token: 0x06001DF9 RID: 7673 RVA: 0x000D6394 File Offset: 0x000D4794
	private static float CalculateMinGap(Camera cam, int sw, int sh, List<Vector3> toProject)
	{
		Vector2 vector = new Vector2((float)sw, (float)sh);
		Vector2 vector2 = new Vector2(0f, 0f);
		foreach (Vector3 position in toProject)
		{
			Vector3 vector3 = cam.WorldToScreenPoint(position);
			vector.x = Mathf.Min(vector3.x, vector.x);
			vector.y = Mathf.Min(vector3.y, vector.y);
			vector2.x = Mathf.Max(vector3.x, vector2.x);
			vector2.y = Mathf.Max(vector3.y, vector2.y);
		}
		float x = vector.x;
		float b = (float)sw - vector2.x;
		float y = vector.y;
		float b2 = (float)sh - vector2.y;
		return Mathf.Min(Mathf.Min(x, b), Mathf.Min(y, b2));
	}

	// Token: 0x06001DFA RID: 7674 RVA: 0x000D64B8 File Offset: 0x000D48B8
	private static void SetupModelScreenshotOrthographicCamera(Camera cam, Bounds targetBounds, List<Vector3> toProject)
	{
		Vector3 pixelCorrectLinesNormalizedOffset = ScreenshotUtils.GetPixelCorrectLinesNormalizedOffset();
		float d = Mathf.Sqrt(3f) * Util.MaxComponent(targetBounds.extents) + 2f;
		Vector3 b = pixelCorrectLinesNormalizedOffset * d;
		cam.transform.position = targetBounds.center + b;
		cam.transform.LookAt(targetBounds.center);
		float num = Mathf.Sqrt(3f) * Util.MaxComponent(targetBounds.extents);
		num = Mathf.Max(0.5f, num);
		cam.projectionMatrix = Matrix4x4.Ortho(-num, num, -num, num, 1f, 1200f);
		int height = Screen.height;
		int width = Screen.width;
		int num2 = 20;
		for (int i = 0; i < num2; i++)
		{
			float num3 = ScreenshotUtils.CalculateMinGap(cam, width, height, toProject);
			if (num3 <= 0.1f * (float)Mathf.Min(height, width))
			{
				break;
			}
			num *= 0.95f;
			cam.projectionMatrix = Matrix4x4.Ortho(-num, num, -num, num, 1f, 1200f);
		}
	}

	// Token: 0x06001DFB RID: 7675 RVA: 0x000D65D4 File Offset: 0x000D49D4
	private static string GetModelIconFolder()
	{
		return Application.dataPath + "/../Icons";
	}

	// Token: 0x06001DFC RID: 7676 RVA: 0x000D65E5 File Offset: 0x000D49E5
	private static string GetModelIconPath(int index)
	{
		return string.Concat(new object[]
		{
			ScreenshotUtils.GetModelIconFolder(),
			"/ModelIcon_",
			index,
			".png"
		});
	}

	// Token: 0x06001DFD RID: 7677 RVA: 0x000D6614 File Offset: 0x000D4A14
	private static bool AddBorder(int x, int y, int size, Color[] pixels)
	{
		if (x > 0 && x < size - 1 && y > 0 && y < size - 1 && pixels[ScreenshotUtils.Index(x, y, size)].a < 0.03f)
		{
			int num = 0;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (pixels[ScreenshotUtils.Index(x + i, y + j, size)].a > 0.04f)
					{
						num++;
						if (num > 1)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	// Token: 0x06001DFE RID: 7678 RVA: 0x000D66B4 File Offset: 0x000D4AB4
	private static Color Filter(int x, int y, int size, Color[] pixels, float[] coeffs, float m)
	{
		Color color = new Color(0f, 0f, 0f, 0f);
		float num = 0f;
		float num2 = 0f;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Color a = pixels[ScreenshotUtils.Index(x + i, y + j, size)];
				if (a.a > 0.02f)
				{
					float num3 = m * coeffs[ScreenshotUtils.Index(i + 1, j + 1, 3)];
					num += num3;
					color += num3 * a;
					num2 += a.a * 0.25f;
				}
			}
		}
		if (num > 0f)
		{
			color /= num;
		}
		color.a = Mathf.Clamp(num2, 0f, 1f);
		return color;
	}

	// Token: 0x06001DFF RID: 7679 RVA: 0x000D67A0 File Offset: 0x000D4BA0
	private static int Index(int x, int y, int size)
	{
		return x + y * size;
	}

	// Token: 0x06001E00 RID: 7680 RVA: 0x000D67A7 File Offset: 0x000D4BA7
	private static bool FloatsClose(float f1, float f2, float d)
	{
		return Mathf.Abs(f1 - f2) <= d;
	}

	// Token: 0x06001E01 RID: 7681 RVA: 0x000D67B8 File Offset: 0x000D4BB8
	private static bool ColorsClose(Color pixelColor, Color backgroundColor, float d = 0.01f)
	{
		return ScreenshotUtils.FloatsClose(pixelColor.r, backgroundColor.r, d) && ScreenshotUtils.FloatsClose(pixelColor.g, backgroundColor.g, d) && ScreenshotUtils.FloatsClose(pixelColor.b, backgroundColor.b, d);
	}

	// Token: 0x04001844 RID: 6212
	public static readonly int iconSizeSD = 64;

	// Token: 0x04001845 RID: 6213
	public static readonly int snapshotSizeSD = 384;

	// Token: 0x04001846 RID: 6214
	private static bool busy;

	// Token: 0x04001847 RID: 6215
	private static Shader metalShader = Shader.Find("Blocksworld/ScreenshotMetal");

	// Token: 0x04001848 RID: 6216
	private static Cubemap lowMetalCubemap;

	// Token: 0x04001849 RID: 6217
	private static Dictionary<Shader, Shader> toonShaders = new Dictionary<Shader, Shader>
	{
		{
			Shader.Find("Blocksworld/Normal"),
			Shader.Find("Blocksworld/ScreenshotNormal")
		},
		{
			Shader.Find("Blocksworld/Multiply"),
			Shader.Find("Blocksworld/ScreenshotMultiply")
		},
		{
			Shader.Find("Blocksworld/Terrain"),
			Shader.Find("Blocksworld/ScreenshotTerrain")
		},
		{
			Shader.Find("Blocksworld/Metal"),
			ScreenshotUtils.metalShader
		},
		{
			Shader.Find("Blocksworld/PulsateGlow"),
			Shader.Find("Blocksworld/PulsateGlow")
		},
		{
			Shader.Find("Blocksworld/Glass"),
			Shader.Find("Blocksworld/Glass")
		}
	};
}
