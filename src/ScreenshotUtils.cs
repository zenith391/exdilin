using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ScreenshotUtils
{
	public static readonly int iconSizeSD = 64;

	public static readonly int snapshotSizeSD = 384;

	private static bool busy;

	private static Shader metalShader = Shader.Find("Blocksworld/ScreenshotMetal");

	private static Cubemap lowMetalCubemap;

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
			metalShader
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

	public static bool IsBusy()
	{
		return busy;
	}

	public static void OnStopAllCoroutines()
	{
		busy = false;
	}

	public static void GenerateModelIconTexture(List<List<List<Tile>>> model, bool hd, Action<Texture2D> callback)
	{
		Blocksworld.bw.StartCoroutine(GenerateModelIcon(model, (!hd) ? iconSizeSD : (iconSizeSD * 2), withOutline: true, perspectiveCamera: false, callback));
	}

	public static void GenerateModelSnapshotTexture(List<List<List<Tile>>> model, bool hd, Action<Texture2D> callback)
	{
		Blocksworld.bw.StartCoroutine(GenerateModelIcon(model, (!hd) ? snapshotSizeSD : (snapshotSizeSD * 2), withOutline: true, perspectiveCamera: true, callback));
	}

	public static void GenerateModelIconTexture(List<List<List<Tile>>> model, int size, bool withOutline, bool perspectiveCamera, Action<Texture2D> callback)
	{
		Blocksworld.bw.StartCoroutine(GenerateModelIcon(model, size, withOutline, perspectiveCamera, callback));
	}

	private static IEnumerator GenerateModelIcon(List<List<List<Tile>>> model, int size, bool withOutline, bool perspectiveCamera, Action<Texture2D> callback)
	{
		while (busy)
		{
			yield return null;
		}
		busy = true;
		List<Block> volumeBlocks = new List<Block>();
		List<Block> invisibleBlocks = new List<Block>();
		List<Block> blocks = new List<Block>();
		for (int i = 0; i < model.Count; i++)
		{
			List<List<Tile>> tiles = model[i];
			List<List<Tile>> tiles2 = Util.CopyTiles(tiles);
			Block block = Block.NewBlock(tiles2);
			if (block is BlockPosition || (block is BlockAbstractParticles && (block as BlockAbstractParticles).HidingEmitter()))
			{
				volumeBlocks.Add(block);
				continue;
			}
			bool flag = false;
			for (int j = 0; j < ((block.childMeshes == null) ? 1 : (block.childMeshes.Count + 1)); j++)
			{
				string texture = block.GetTexture(j);
				if (!(texture == "Invisible"))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				block.MakeInvisibleVisible();
				if (block is BlockAnimatedCharacter)
				{
					(block as BlockAnimatedCharacter).PrepareForModelIconRender(Layer.IconGeneration);
				}
				block.go.SetLayer(Layer.IconGeneration, recursive: true);
				if (block is BlockAbstractMovingPlatform)
				{
					(block as BlockAbstractMovingPlatform).go.GetComponent<Renderer>().enabled = false;
					Vector3 scale = block.GetScale();
					BoxCollider boxCollider = (BoxCollider)block.go.GetComponent<Collider>();
					boxCollider.size = Vector3.one;
					boxCollider.center = -Vector3.forward * (scale.z - 1f) * 0.5f;
				}
				blocks.Add(block);
				if (i % 10 == 0)
				{
					yield return null;
				}
			}
			else
			{
				invisibleBlocks.Add(block);
			}
		}
		if (blocks.Count == 0 && volumeBlocks.Count == 0)
		{
			for (int i = 0; i < invisibleBlocks.Count; i++)
			{
				Block block2 = invisibleBlocks[i];
				if (block2 is BlockPosition)
				{
					volumeBlocks.Add(block2);
					continue;
				}
				block2.MakeInvisibleVisible();
				block2.go.SetLayer(Layer.IconGeneration, recursive: true);
				blocks.Add(block2);
				if (i % 10 == 0)
				{
					yield return null;
				}
			}
		}
		else
		{
			foreach (Block item in invisibleBlocks)
			{
				item.Destroy();
				if (item is BlockTerrain)
				{
					BWSceneManager.RemoveTerrainBlock((BlockTerrain)item);
				}
			}
		}
		invisibleBlocks.Clear();
		if (blocks.Count == 0)
		{
			withOutline = false;
			for (int k = 0; k < volumeBlocks.Count; k++)
			{
				volumeBlocks[k].go.SetLayer(Layer.IconGeneration, recursive: true);
				blocks.Add(volumeBlocks[k]);
			}
		}
		else
		{
			for (int l = 0; l < volumeBlocks.Count; l++)
			{
				volumeBlocks[l].Destroy();
			}
			volumeBlocks.Clear();
		}
		BlockGroups.GatherBlockGroups(blocks);
		foreach (Block item2 in blocks)
		{
			if (item2 is BlockGrouped)
			{
				item2.Update();
			}
		}
		yield return null;
		GameObject cameraGo = new GameObject();
		Camera cam = cameraGo.AddComponent<Camera>();
		cam.enabled = false;
		float num = 50f / 51f;
		Color backgroundColor = (cam.backgroundColor = new Color(1f, num, num, 1f));
		cam.clearFlags = CameraClearFlags.Color;
		Bounds targetBounds = default(Bounds);
		List<Vector3> toProject = new List<Vector3>();
		for (int m = 0; m < blocks.Count; m++)
		{
			Block block3 = blocks[m];
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
			if (m == 0)
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
			SetupModelScreenshotPerspectiveCamera(cam, targetBounds, toProject);
		}
		else
		{
			SetupModelScreenshotOrthographicCamera(cam, targetBounds, toProject);
			foreach (Block item3 in blocks)
			{
				MeshRenderer[] componentsInChildren = item3.go.GetComponentsInChildren<MeshRenderer>();
				MeshRenderer[] array = componentsInChildren;
				foreach (MeshRenderer meshRenderer in array)
				{
					Shader shader = meshRenderer.material.shader;
					if (!toonShaders.ContainsKey(shader))
					{
						continue;
					}
					meshRenderer.material.shader = toonShaders[shader];
					if (shader == metalShader)
					{
						if (lowMetalCubemap == null)
						{
							lowMetalCubemap = (Cubemap)Resources.Load("Cubemaps/Low Metal", typeof(Cubemap));
						}
						Material material = meshRenderer.material;
						if (material.HasProperty("_CubeMapTex"))
						{
							material.SetTexture("_CubeMapTex", lowMetalCubemap);
						}
					}
				}
			}
		}
		GameObject directionalLight = Blocksworld.directionalLight;
		Transform transform = directionalLight.transform;
		Light light = directionalLight.GetComponent<Light>();
		if (Blocksworld.renderingShadows)
		{
			light = Blocksworld.overheadLight;
			transform = light.transform;
			_ = light.gameObject;
		}
		Quaternion rotation = transform.rotation;
		Color color2 = light.color;
		float intensity = light.intensity;
		float shadowStrength = light.shadowStrength;
		float fogEnd = Blocksworld.fogEnd;
		float fogStart = Blocksworld.fogStart;
		float fogMultiplier = Blocksworld.fogMultiplier;
		Blocksworld.bw.SetFog(2000f, 3000f);
		Blocksworld.bw.SetFogMultiplier(1f);
		light.color = Color.white;
		light.intensity = 1f;
		light.shadowStrength = 0.25f;
		Quaternion quaternion = Quaternion.Euler(40f, 20f, 0f);
		transform.rotation = cam.transform.rotation * quaternion;
		Texture2D imageTexture = new Texture2D(size, size);
		Blocksworld.RenderScreenshot(imageTexture, cam, null, FilterMode.Point);
		cam.enabled = false;
		transform.rotation = rotation;
		light.color = color2;
		light.intensity = intensity;
		light.shadowStrength = shadowStrength;
		Blocksworld.bw.SetFog(fogStart, fogEnd);
		Blocksworld.bw.SetFogMultiplier(fogMultiplier);
		yield return null;
		foreach (Block item4 in blocks)
		{
			item4.Destroy();
			if (item4 is BlockTerrain)
			{
				BWSceneManager.RemoveTerrainBlock((BlockTerrain)item4);
			}
		}
		if (lowMetalCubemap != null)
		{
			lowMetalCubemap = null;
		}
		yield return null;
		imageTexture.Apply();
		Color[] array2 = imageTexture.GetPixels();
		float d = 0.01f;
		Color color3 = new Color(0.35f, 0.35f, 0.35f, 1f);
		for (int num2 = 0; num2 < array2.Length; num2++)
		{
			if (ColorsClose(array2[num2], backgroundColor, d))
			{
				array2[num2].a = 0f;
			}
		}
		backgroundColor.a = 0f;
		if (withOutline)
		{
			Color[] array3 = (Color[])array2.Clone();
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				if (ColorsClose(array2[num3], backgroundColor, d))
				{
					int x = num3 % size;
					int y = num3 / size;
					if (AddBorder(x, y, size, array2))
					{
						array3[num3] = color3;
					}
				}
			}
			Color[] array4 = array3;
			array3 = array2;
			array2 = array4;
			float[] array5 = new float[9] { 1f, 2f, 1f, 2f, 4f, 2f, 1f, 2f, 1f };
			float num4 = 0f;
			for (int num5 = 0; num5 < array5.Length; num5++)
			{
				num4 += array5[num5];
			}
			float m2 = 1f / num4;
			for (int num6 = 0; num6 < array2.Length; num6++)
			{
				int num7 = num6 % size;
				int num8 = num6 / size;
				Color color4 = array2[num6];
				if (color4.a < 0.05f && num7 > 0 && num7 < size - 1 && num8 > 0 && num8 < size - 1)
				{
					array3[num6] = Filter(num7, num8, size, array2, array5, m2);
				}
				else
				{
					array3[num6] = array2[num6];
				}
			}
			array2 = array3;
		}
		imageTexture.SetPixels(array2);
		imageTexture.Apply();
		UnityEngine.Object.Destroy(cameraGo);
		callback?.Invoke(imageTexture);
		busy = false;
	}

	private static Vector3 GetPixelCorrectLinesNormalizedOffset()
	{
		Vector3 vec = -Blocksworld.cameraTransform.forward;
		vec = Util.ProjectOntoPlane(vec, Vector3.up).normalized;
		Vector3[] array = new Vector3[4]
		{
			Vector3.forward,
			Vector3.back,
			Vector3.right,
			Vector3.left
		};
		float num = -1f;
		Vector3 vector = Vector3.forward;
		Quaternion quaternion = Quaternion.AngleAxis(45f, Vector3.up);
		Vector3[] array2 = array;
		foreach (Vector3 vector2 in array2)
		{
			Vector3 lhs = quaternion * vector2;
			float num2 = Vector3.Dot(lhs, vec);
			if (num2 > num)
			{
				num = num2;
				vector = lhs.normalized;
			}
		}
		float num3 = Mathf.Sqrt(3f) / 2f;
		return (num3 * vector + 0.5f * Vector3.up).normalized;
	}

	private static Vector3 FindModelForward(List<Block> blocks, Vector3 defaultIfNotFound)
	{
		Vector3 result = Vector3.forward;
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
					result = transform.forward;
					num = 0;
					flag = true;
				}
			}
			else
			{
				if (num <= 1)
				{
					continue;
				}
				if (block is BlockAbstractWheel)
				{
					Vector3 vector = block.Scale();
					if (Mathf.Abs(vector.y - vector.z) < 0.01f)
					{
						Transform transform2 = block.go.transform;
						if (Mathf.Abs(transform2.forward.y) < 0.01f)
						{
							result = transform2.forward;
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
						result = forward;
						num = 1;
						flag = true;
					}
				}
				if (num > 2 && block is BlockAbstractAntiGravity blockAbstractAntiGravity)
				{
					Vector3 vector2 = blockAbstractAntiGravity.rotation * blockAbstractAntiGravity.go.transform.forward;
					if (Mathf.Abs(vector2.y) < 0.01f)
					{
						result = vector2;
						num = 2;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			return result;
		}
		return defaultIfNotFound;
	}

	private static void SetupModelScreenshotPerspectiveCamera(Camera cam, Bounds targetBounds, List<Vector3> toProject)
	{
		Vector3 vector = -Blocksworld.cameraTransform.forward;
		float num = 0.01f;
		float a = 2000f;
		float fov = 45f;
		float aspect = 1f;
		float num2 = targetBounds.size.magnitude * 1.25f + num;
		Vector3 vector2 = vector * num2;
		cam.transform.position = targetBounds.center + vector2;
		cam.transform.LookAt(targetBounds.center);
		cam.projectionMatrix = Matrix4x4.Perspective(fov, aspect, num, Mathf.Max(a, num2));
		int height = Screen.height;
		int width = Screen.width;
		int num3 = 20;
		for (int i = 0; i < num3; i++)
		{
			float num4 = CalculateMinGap(cam, width, height, toProject);
			if (!(num4 <= 0.1f * (float)Mathf.Min(height, width)))
			{
				num2 *= 0.95f;
				vector2 = vector * num2;
				cam.transform.position = targetBounds.center + vector2;
				continue;
			}
			break;
		}
	}

	private static float CalculateMinGap(Camera cam, int sw, int sh, List<Vector3> toProject)
	{
		Vector2 vector = new Vector2(sw, sh);
		Vector2 vector2 = new Vector2(0f, 0f);
		foreach (Vector3 item in toProject)
		{
			Vector3 vector3 = cam.WorldToScreenPoint(item);
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

	private static void SetupModelScreenshotOrthographicCamera(Camera cam, Bounds targetBounds, List<Vector3> toProject)
	{
		Vector3 pixelCorrectLinesNormalizedOffset = GetPixelCorrectLinesNormalizedOffset();
		float num = Mathf.Sqrt(3f) * Util.MaxComponent(targetBounds.extents) + 2f;
		Vector3 vector = pixelCorrectLinesNormalizedOffset * num;
		cam.transform.position = targetBounds.center + vector;
		cam.transform.LookAt(targetBounds.center);
		float b = Mathf.Sqrt(3f) * Util.MaxComponent(targetBounds.extents);
		b = Mathf.Max(0.5f, b);
		cam.projectionMatrix = Matrix4x4.Ortho(0f - b, b, 0f - b, b, 1f, 1200f);
		int height = Screen.height;
		int width = Screen.width;
		int num2 = 20;
		for (int i = 0; i < num2; i++)
		{
			float num3 = CalculateMinGap(cam, width, height, toProject);
			if (!(num3 <= 0.1f * (float)Mathf.Min(height, width)))
			{
				b *= 0.95f;
				cam.projectionMatrix = Matrix4x4.Ortho(0f - b, b, 0f - b, b, 1f, 1200f);
				continue;
			}
			break;
		}
	}

	private static string GetModelIconFolder()
	{
		return Application.dataPath + "/../Icons";
	}

	private static string GetModelIconPath(int index)
	{
		return GetModelIconFolder() + "/ModelIcon_" + index + ".png";
	}

	private static bool AddBorder(int x, int y, int size, Color[] pixels)
	{
		if (x > 0 && x < size - 1 && y > 0 && y < size - 1 && pixels[Index(x, y, size)].a < 0.03f)
		{
			int num = 0;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (pixels[Index(x + i, y + j, size)].a > 0.04f)
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

	private static Color Filter(int x, int y, int size, Color[] pixels, float[] coeffs, float m)
	{
		Color result = new Color(0f, 0f, 0f, 0f);
		float num = 0f;
		float num2 = 0f;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Color color = pixels[Index(x + i, y + j, size)];
				if (color.a > 0.02f)
				{
					float num3 = m * coeffs[Index(i + 1, j + 1, 3)];
					num += num3;
					result += num3 * color;
					num2 += color.a * 0.25f;
				}
			}
		}
		if (num > 0f)
		{
			result /= num;
		}
		result.a = Mathf.Clamp(num2, 0f, 1f);
		return result;
	}

	private static int Index(int x, int y, int size)
	{
		return x + y * size;
	}

	private static bool FloatsClose(float f1, float f2, float d)
	{
		return Mathf.Abs(f1 - f2) <= d;
	}

	private static bool ColorsClose(Color pixelColor, Color backgroundColor, float d = 0.01f)
	{
		if (FloatsClose(pixelColor.r, backgroundColor.r, d) && FloatsClose(pixelColor.g, backgroundColor.g, d))
		{
			return FloatsClose(pixelColor.b, backgroundColor.b, d);
		}
		return false;
	}
}
