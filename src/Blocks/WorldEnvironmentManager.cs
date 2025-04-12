using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000361 RID: 865
	public static class WorldEnvironmentManager
	{
		// Token: 0x06002658 RID: 9816 RVA: 0x0011AE7F File Offset: 0x0011927F
		public static void ResetConfiguration()
		{
			WorldEnvironmentManager._config.Reset();
		}

		// Token: 0x06002659 RID: 9817 RVA: 0x0011AE8C File Offset: 0x0011928C
		private static void ApplySkyBox(int index)
		{
			SkyBoxDefinition skyBoxDef = WorldEnvironmentManager.GetSkyBoxDef(index);
			if (WorldEnvironmentManager._config.skyboxIndex != index || RenderSettings.skybox == null || !WorldEnvironmentManager._config.skyBoxAssigned)
			{
				Material material = Resources.Load<Material>(skyBoxDef.platformSpecificMaterialResourcePath);
				if (RenderSettings.skybox != null && WorldEnvironmentManager._currentAssignedSkyBoxInstanceID == RenderSettings.skybox.GetInstanceID())
				{
					UnityEngine.Object.Destroy(RenderSettings.skybox);
				}
				Material material2 = new Material(material);
				WorldEnvironmentManager._currentAssignedSkyBoxInstanceID = material2.GetInstanceID();
				RenderSettings.skybox = material2;
				Resources.UnloadAsset(material);
				WorldEnvironmentManager._config.skyboxDefaultTint = RenderSettings.skybox.GetColor("_Tint");
				WorldEnvironmentManager._config.skyboxIndex = index;
			}
			WorldEnvironmentManager._config.skyBoxAssigned = true;
			if (WorldEnvironmentManager._config.fogStartAssigned)
			{
				WorldEnvironmentManager.RevertToAssignedFogStart();
			}
			else
			{
				WorldEnvironmentManager.ApplyFogStart(skyBoxDef.fogStart);
			}
			if (WorldEnvironmentManager._config.fogDensityAssigned)
			{
				WorldEnvironmentManager.RevertToAssignedFogDensity();
			}
			else
			{
				WorldEnvironmentManager.ApplyFogDensity(skyBoxDef.fogDensity);
			}
			if (WorldEnvironmentManager._config.skyBoxRotationAssigned)
			{
				WorldEnvironmentManager.RevertToAssignedSkyBoxRotation();
			}
			else
			{
				WorldEnvironmentManager.ApplySkyBoxRotation(0f);
			}
			if (WorldEnvironmentManager._config.skyPaintAssigned)
			{
				WorldEnvironmentManager.RevertToAssignedSkyColor();
			}
			else
			{
				WorldEnvironmentManager.ApplySkyPaint("White");
			}
			if (WorldEnvironmentManager._config.fogPaintAssigned)
			{
				WorldEnvironmentManager.RevertToAssignedFogPaint();
			}
			else
			{
				WorldEnvironmentManager.AssignNewFogPaint(skyBoxDef.fogColor);
			}
		}

		// Token: 0x0600265A RID: 9818 RVA: 0x0011B008 File Offset: 0x00119408
		public static void ChangeSkyBoxTemporarily(int index)
		{
			WorldEnvironmentManager.ApplySkyBox(index);
		}

		// Token: 0x0600265B RID: 9819 RVA: 0x0011B010 File Offset: 0x00119410
		public static void ChangeSkyBoxPermanently(int index)
		{
			WorldEnvironmentManager.ResetConfiguration();
			WorldEnvironmentManager.ApplySkyBox(index);
			Blocksworld.worldSky.doNotApplyFogColorBackToEnvironmentManager = true;
			Blocksworld.worldSky.SetSkyBoxIndex(index);
			Blocksworld.worldSky.PaintTo(WorldEnvironmentManager._config.skyPaint, true, 0);
			Blocksworld.worldSky.SetFogColor(WorldEnvironmentManager._config.fogPaint);
			Blocksworld.worldSky.doNotApplyFogColorBackToEnvironmentManager = false;
		}

		// Token: 0x0600265C RID: 9820 RVA: 0x0011B074 File Offset: 0x00119474
		public static void RevertToPreviousSkyBox()
		{
			if (WorldEnvironmentManager._previousConfigStored)
			{
				WorldEnvironmentManager._previousConfigStored = false;
				WorldEnvironmentManager._config = WorldEnvironmentManager._previousConfig;
				WorldEnvironmentManager._config.skyBoxAssigned = false;
				WorldEnvironmentManager.ApplySkyBox(WorldEnvironmentManager._config.skyboxIndex);
			}
		}

		// Token: 0x0600265D RID: 9821 RVA: 0x0011B0AA File Offset: 0x001194AA
		public static void SaveSkyBoxHistory()
		{
			WorldEnvironmentManager._previousConfig = WorldEnvironmentManager._config;
			WorldEnvironmentManager._previousConfigStored = true;
		}

		// Token: 0x0600265E RID: 9822 RVA: 0x0011B0BC File Offset: 0x001194BC
		public static int SkyBoxIndex()
		{
			return WorldEnvironmentManager._config.skyboxIndex;
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x0011B0C8 File Offset: 0x001194C8
		private static void ApplySkyBoxRotation(float angle)
		{
			if (RenderSettings.skybox != null)
			{
				RenderSettings.skybox.SetFloat("_Rotation", angle);
			}
			if (WorldEnvironmentManager._skyboxDefinitions != null && Blocksworld.lightingRig != null)
			{
				SkyBoxDefinition skyBoxDefinition = WorldEnvironmentManager._skyboxDefinitions[WorldEnvironmentManager._config.skyboxIndex];
				Blocksworld.lightingRig.rotation = Quaternion.Euler(skyBoxDefinition.lightRotation.x, skyBoxDefinition.lightRotation.y - angle, skyBoxDefinition.lightRotation.z);
			}
		}

		// Token: 0x06002660 RID: 9824 RVA: 0x0011B152 File Offset: 0x00119552
		public static void AssignSkyBoxRotation(float angle)
		{
			WorldEnvironmentManager.ApplySkyBoxRotation(angle);
			WorldEnvironmentManager._config.skyboxRotation = angle;
			WorldEnvironmentManager._config.skyBoxRotationAssigned = true;
		}

		// Token: 0x06002661 RID: 9825 RVA: 0x0011B170 File Offset: 0x00119570
		public static void OverrideSkyBoxRotationTemporarily(float angle)
		{
			WorldEnvironmentManager.ApplySkyBoxRotation(angle);
		}

		// Token: 0x06002662 RID: 9826 RVA: 0x0011B178 File Offset: 0x00119578
		public static void RevertToAssignedSkyBoxRotation()
		{
			WorldEnvironmentManager.ApplySkyBoxRotation(WorldEnvironmentManager._config.skyboxRotation);
		}

		// Token: 0x06002663 RID: 9827 RVA: 0x0011B18C File Offset: 0x0011958C
		public static float SkyBoxRotation()
		{
			float result = 0f;
			if (RenderSettings.skybox != null)
			{
				result = RenderSettings.skybox.GetFloat("_Rotation");
			}
			return result;
		}

		// Token: 0x06002664 RID: 9828 RVA: 0x0011B1C0 File Offset: 0x001195C0
		private static void ApplyFogPaint(string paint)
		{
			Color[] array;
			if (Blocksworld.colorDefinitions.TryGetValue(paint, out array))
			{
				RenderSettings.fogColor = array[1];
				if (RenderSettings.skybox != null)
				{
					RenderSettings.skybox.SetColor("_FogTint", array[1]);
				}
				WorldEnvironmentManager._config.fogPaint = paint;
			}
		}

		// Token: 0x06002665 RID: 9829 RVA: 0x0011B226 File Offset: 0x00119626
		public static void AssignNewFogPaint(string paint)
		{
			WorldEnvironmentManager.ApplyFogPaint(paint);
			WorldEnvironmentManager._config.fogPaint = paint;
			WorldEnvironmentManager._config.fogPaintAssigned = true;
		}

		// Token: 0x06002666 RID: 9830 RVA: 0x0011B244 File Offset: 0x00119644
		public static void OverrideFogPaintTemporarily(string paint)
		{
			WorldEnvironmentManager.ApplyFogPaint(paint);
		}

		// Token: 0x06002667 RID: 9831 RVA: 0x0011B24C File Offset: 0x0011964C
		public static void RevertToAssignedFogPaint()
		{
			WorldEnvironmentManager.ApplyFogPaint(WorldEnvironmentManager._config.fogPaint);
		}

		// Token: 0x06002668 RID: 9832 RVA: 0x0011B25D File Offset: 0x0011965D
		public static string GetFogPaint()
		{
			return WorldEnvironmentManager._config.fogPaint;
		}

		// Token: 0x06002669 RID: 9833 RVA: 0x0011B269 File Offset: 0x00119669
		private static void ApplyFogDensity(float fogDensity)
		{
			RenderSettings.fogEndDistance = WorldEnvironmentManager.ComputeFogEnd(RenderSettings.fogStartDistance, fogDensity);
		}

		// Token: 0x0600266A RID: 9834 RVA: 0x0011B27B File Offset: 0x0011967B
		public static void AssignNewFogDensity(float fogDensity)
		{
			WorldEnvironmentManager.ApplyFogDensity(fogDensity);
			WorldEnvironmentManager._config.fogDensity = fogDensity;
			WorldEnvironmentManager._config.fogDensityAssigned = true;
		}

		// Token: 0x0600266B RID: 9835 RVA: 0x0011B299 File Offset: 0x00119699
		public static void OverrideFogDensityTemporarily(float fogDensity)
		{
			WorldEnvironmentManager.ApplyFogDensity(fogDensity);
		}

		// Token: 0x0600266C RID: 9836 RVA: 0x0011B2A1 File Offset: 0x001196A1
		public static void RevertToAssignedFogDensity()
		{
			WorldEnvironmentManager.ApplyFogDensity(WorldEnvironmentManager._config.fogDensity);
		}

		// Token: 0x0600266D RID: 9837 RVA: 0x0011B2B2 File Offset: 0x001196B2
		private static void ApplyFogStart(float fogStart)
		{
			RenderSettings.fogStartDistance = fogStart;
			RenderSettings.fogEndDistance = WorldEnvironmentManager.ComputeFogEnd(fogStart, WorldEnvironmentManager._config.fogDensity);
		}

		// Token: 0x0600266E RID: 9838 RVA: 0x0011B2CF File Offset: 0x001196CF
		public static void AssignNewFogStart(float fogStart)
		{
			WorldEnvironmentManager.ApplyFogStart(fogStart);
			WorldEnvironmentManager._config.fogStart = fogStart;
			WorldEnvironmentManager._config.fogStartAssigned = true;
		}

		// Token: 0x0600266F RID: 9839 RVA: 0x0011B2ED File Offset: 0x001196ED
		public static void OverrideFogStartTemporarily(float fogStart)
		{
			WorldEnvironmentManager.ApplyFogStart(fogStart);
		}

		// Token: 0x06002670 RID: 9840 RVA: 0x0011B2F5 File Offset: 0x001196F5
		public static void RevertToAssignedFogStart()
		{
			WorldEnvironmentManager.ApplyFogStart(WorldEnvironmentManager._config.fogStart);
		}

		// Token: 0x06002671 RID: 9841 RVA: 0x0011B308 File Offset: 0x00119708
		private static void ApplySkyPaint(string paint)
		{
			Color color;
			if (Blocksworld.skyBoxTintDefinitions.TryGetValue(paint, out color))
			{
				if (Blocksworld.overheadLight != null)
				{
					Blocksworld.overheadLight.color = 0.5f * (Color.white + color);
				}
				if (RenderSettings.skybox != null)
				{
					RenderSettings.skybox.SetColor("_Tint", color * WorldEnvironmentManager._config.skyboxDefaultTint);
				}
				Color[] array;
				if (Blocksworld.ambientLightGradientDefinitions.TryGetValue(paint, out array))
				{
					RenderSettings.ambientSkyColor = array[0];
					RenderSettings.ambientEquatorColor = array[1];
					RenderSettings.ambientGroundColor = array[2];
				}
				else
				{
					Blocksworld.ambientLightGradientDefinitions.TryGetValue("Base Ratio", out array);
					RenderSettings.ambientSkyColor = WorldEnvironmentManager.CombineSkyColors(color, array[0]);
					RenderSettings.ambientEquatorColor = WorldEnvironmentManager.CombineSkyColors(color, array[1]);
					RenderSettings.ambientGroundColor = WorldEnvironmentManager.CombineSkyColors(color, array[2]);
				}
				WorldEnvironmentManager._config.skyPaint = paint;
				if (!WorldEnvironmentManager._config.fogPaintAssigned)
				{
					WorldEnvironmentManager.ApplyFogPaint(paint);
				}
			}
		}

		// Token: 0x06002672 RID: 9842 RVA: 0x0011B446 File Offset: 0x00119846
		public static void AssignNewSkyPaint(string skyPaint)
		{
			WorldEnvironmentManager.ApplySkyPaint(skyPaint);
			WorldEnvironmentManager._config.skyPaint = skyPaint;
			WorldEnvironmentManager._config.skyPaintAssigned = true;
		}

		// Token: 0x06002673 RID: 9843 RVA: 0x0011B464 File Offset: 0x00119864
		public static void OverrideSkyColorTemporarily(string tempPaint)
		{
			WorldEnvironmentManager.ApplySkyPaint(tempPaint);
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x0011B46C File Offset: 0x0011986C
		public static void RevertToAssignedSkyColor()
		{
			WorldEnvironmentManager.ApplySkyPaint(WorldEnvironmentManager._config.skyPaint);
		}

		// Token: 0x06002675 RID: 9845 RVA: 0x0011B47D File Offset: 0x0011987D
		private static Color CombineSkyColors(Color c1, Color c2)
		{
			return c1 * c2;
		}

		// Token: 0x06002676 RID: 9846 RVA: 0x0011B488 File Offset: 0x00119888
		private static SkyBoxDefinition GetSkyBoxDef(int index)
		{
			if (WorldEnvironmentManager._skyboxDefinitions == null)
			{
				SkyBoxDefinitions skyBoxDefinitions = Resources.Load<SkyBoxDefinitions>("SkyBoxDefinitions");
				WorldEnvironmentManager._skyboxDefinitions = skyBoxDefinitions.definitions;
			}
			return WorldEnvironmentManager._skyboxDefinitions[index];
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x0011B4BC File Offset: 0x001198BC
		public static float ComputeFogEnd(float start, float density)
		{
			float result;
			if (density > 0f)
			{
				result = start + (3000f / Mathf.Sqrt(density) - 299.9f);
			}
			else
			{
				result = 2000f;
			}
			return result;
		}

		// Token: 0x04002197 RID: 8599
		private static WorldEnvironmentManager.Configuration _config = default(WorldEnvironmentManager.Configuration);

		// Token: 0x04002198 RID: 8600
		private static WorldEnvironmentManager.Configuration _previousConfig = default(WorldEnvironmentManager.Configuration);

		// Token: 0x04002199 RID: 8601
		private static bool _previousConfigStored = false;

		// Token: 0x0400219A RID: 8602
		private static SkyBoxDefinition[] _skyboxDefinitions = null;

		// Token: 0x0400219B RID: 8603
		private static int _currentAssignedSkyBoxInstanceID;

		// Token: 0x02000362 RID: 866
		public struct Configuration
		{
			// Token: 0x06002679 RID: 9849 RVA: 0x0011B530 File Offset: 0x00119930
			public void Reset()
			{
				this.fogDensityAssigned = false;
				this.fogPaintAssigned = false;
				this.fogStartAssigned = false;
				this.lightColorAssigned = false;
				this.lightIntensityAssigned = false;
				this.skyBoxAssigned = false;
				this.skyBoxRotationAssigned = false;
				this.skyPaintAssigned = false;
				this.skyboxIndex = 0;
				this.skyboxRotation = 0f;
				this.skyboxDefaultTint = Color.white;
				this.fogPaint = string.Empty;
				this.lightPaint = string.Empty;
				this.skyPaint = string.Empty;
				this.fogDensity = 50f;
				this.fogStart = 200f;
				this.lightIntensity = 1f;
			}

			// Token: 0x0400219C RID: 8604
			public bool fogDensityAssigned;

			// Token: 0x0400219D RID: 8605
			public bool fogPaintAssigned;

			// Token: 0x0400219E RID: 8606
			public bool fogStartAssigned;

			// Token: 0x0400219F RID: 8607
			public bool lightColorAssigned;

			// Token: 0x040021A0 RID: 8608
			public bool lightIntensityAssigned;

			// Token: 0x040021A1 RID: 8609
			public bool skyBoxAssigned;

			// Token: 0x040021A2 RID: 8610
			public bool skyBoxRotationAssigned;

			// Token: 0x040021A3 RID: 8611
			public bool skyPaintAssigned;

			// Token: 0x040021A4 RID: 8612
			public int skyboxIndex;

			// Token: 0x040021A5 RID: 8613
			public float skyboxRotation;

			// Token: 0x040021A6 RID: 8614
			public Color skyboxDefaultTint;

			// Token: 0x040021A7 RID: 8615
			public string fogPaint;

			// Token: 0x040021A8 RID: 8616
			public string lightPaint;

			// Token: 0x040021A9 RID: 8617
			public string skyPaint;

			// Token: 0x040021AA RID: 8618
			public float fogDensity;

			// Token: 0x040021AB RID: 8619
			public float fogStart;

			// Token: 0x040021AC RID: 8620
			public float lightIntensity;
		}
	}
}
