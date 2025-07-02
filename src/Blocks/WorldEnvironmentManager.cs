using UnityEngine;

namespace Blocks;

public static class WorldEnvironmentManager
{
	public struct Configuration
	{
		public bool fogDensityAssigned;

		public bool fogPaintAssigned;

		public bool fogStartAssigned;

		public bool lightColorAssigned;

		public bool lightIntensityAssigned;

		public bool skyBoxAssigned;

		public bool skyBoxRotationAssigned;

		public bool skyPaintAssigned;

		public int skyboxIndex;

		public float skyboxRotation;

		public Color skyboxDefaultTint;

		public string fogPaint;

		public string lightPaint;

		public string skyPaint;

		public float fogDensity;

		public float fogStart;

		public float lightIntensity;

		public void Reset()
		{
			fogDensityAssigned = false;
			fogPaintAssigned = false;
			fogStartAssigned = false;
			lightColorAssigned = false;
			lightIntensityAssigned = false;
			skyBoxAssigned = false;
			skyBoxRotationAssigned = false;
			skyPaintAssigned = false;
			skyboxIndex = 0;
			skyboxRotation = 0f;
			skyboxDefaultTint = Color.white;
			fogPaint = string.Empty;
			lightPaint = string.Empty;
			skyPaint = string.Empty;
			fogDensity = 50f;
			fogStart = 200f;
			lightIntensity = 1f;
		}
	}

	private static Configuration _config;

	private static Configuration _previousConfig;

	private static bool _previousConfigStored;

	private static SkyBoxDefinition[] _skyboxDefinitions;

	private static int _currentAssignedSkyBoxInstanceID;

	public static void ResetConfiguration()
	{
		_config.Reset();
	}

	private static void ApplySkyBox(int index)
	{
		SkyBoxDefinition skyBoxDef = GetSkyBoxDef(index);
		if (_config.skyboxIndex != index || RenderSettings.skybox == null || !_config.skyBoxAssigned)
		{
			Material material = Resources.Load<Material>(skyBoxDef.platformSpecificMaterialResourcePath);
			if (RenderSettings.skybox != null && _currentAssignedSkyBoxInstanceID == RenderSettings.skybox.GetInstanceID())
			{
				Object.Destroy(RenderSettings.skybox);
			}
			Material material2 = new Material(material);
			_currentAssignedSkyBoxInstanceID = material2.GetInstanceID();
			RenderSettings.skybox = material2;
			Resources.UnloadAsset(material);
			_config.skyboxDefaultTint = RenderSettings.skybox.GetColor("_Tint");
			_config.skyboxIndex = index;
		}
		_config.skyBoxAssigned = true;
		if (_config.fogStartAssigned)
		{
			RevertToAssignedFogStart();
		}
		else
		{
			ApplyFogStart(skyBoxDef.fogStart);
		}
		if (_config.fogDensityAssigned)
		{
			RevertToAssignedFogDensity();
		}
		else
		{
			ApplyFogDensity(skyBoxDef.fogDensity);
		}
		if (_config.skyBoxRotationAssigned)
		{
			RevertToAssignedSkyBoxRotation();
		}
		else
		{
			ApplySkyBoxRotation(0f);
		}
		if (_config.skyPaintAssigned)
		{
			RevertToAssignedSkyColor();
		}
		else
		{
			ApplySkyPaint("White");
		}
		if (_config.fogPaintAssigned)
		{
			RevertToAssignedFogPaint();
		}
		else
		{
			AssignNewFogPaint(skyBoxDef.fogColor);
		}
	}

	public static void ChangeSkyBoxTemporarily(int index)
	{
		ApplySkyBox(index);
	}

	public static void ChangeSkyBoxPermanently(int index)
	{
		ResetConfiguration();
		ApplySkyBox(index);
		Blocksworld.worldSky.doNotApplyFogColorBackToEnvironmentManager = true;
		Blocksworld.worldSky.SetSkyBoxIndex(index);
		Blocksworld.worldSky.PaintTo(_config.skyPaint, permanent: true);
		Blocksworld.worldSky.SetFogColor(_config.fogPaint);
		Blocksworld.worldSky.doNotApplyFogColorBackToEnvironmentManager = false;
	}

	public static void RevertToPreviousSkyBox()
	{
		if (_previousConfigStored)
		{
			_previousConfigStored = false;
			_config = _previousConfig;
			_config.skyBoxAssigned = false;
			ApplySkyBox(_config.skyboxIndex);
		}
	}

	public static void SaveSkyBoxHistory()
	{
		_previousConfig = _config;
		_previousConfigStored = true;
	}

	public static int SkyBoxIndex()
	{
		return _config.skyboxIndex;
	}

	private static void ApplySkyBoxRotation(float angle)
	{
		if (RenderSettings.skybox != null)
		{
			RenderSettings.skybox.SetFloat("_Rotation", angle);
		}
		if (_skyboxDefinitions != null && Blocksworld.lightingRig != null)
		{
			SkyBoxDefinition skyBoxDefinition = _skyboxDefinitions[_config.skyboxIndex];
			Blocksworld.lightingRig.rotation = Quaternion.Euler(skyBoxDefinition.lightRotation.x, skyBoxDefinition.lightRotation.y - angle, skyBoxDefinition.lightRotation.z);
		}
	}

	public static void AssignSkyBoxRotation(float angle)
	{
		ApplySkyBoxRotation(angle);
		_config.skyboxRotation = angle;
		_config.skyBoxRotationAssigned = true;
	}

	public static void OverrideSkyBoxRotationTemporarily(float angle)
	{
		ApplySkyBoxRotation(angle);
	}

	public static void RevertToAssignedSkyBoxRotation()
	{
		ApplySkyBoxRotation(_config.skyboxRotation);
	}

	public static float SkyBoxRotation()
	{
		float result = 0f;
		if (RenderSettings.skybox != null)
		{
			result = RenderSettings.skybox.GetFloat("_Rotation");
		}
		return result;
	}

	private static void ApplyFogPaint(string paint)
	{
		if (Blocksworld.colorDefinitions.TryGetValue(paint, out var value))
		{
			RenderSettings.fogColor = value[1];
			if (RenderSettings.skybox != null)
			{
				RenderSettings.skybox.SetColor("_FogTint", value[1]);
			}
			_config.fogPaint = paint;
		}
	}

	public static void AssignNewFogPaint(string paint)
	{
		ApplyFogPaint(paint);
		_config.fogPaint = paint;
		_config.fogPaintAssigned = true;
	}

	public static void OverrideFogPaintTemporarily(string paint)
	{
		ApplyFogPaint(paint);
	}

	public static void RevertToAssignedFogPaint()
	{
		ApplyFogPaint(_config.fogPaint);
	}

	public static string GetFogPaint()
	{
		return _config.fogPaint;
	}

	private static void ApplyFogDensity(float fogDensity)
	{
		RenderSettings.fogEndDistance = ComputeFogEnd(RenderSettings.fogStartDistance, fogDensity);
	}

	public static void AssignNewFogDensity(float fogDensity)
	{
		ApplyFogDensity(fogDensity);
		_config.fogDensity = fogDensity;
		_config.fogDensityAssigned = true;
	}

	public static void OverrideFogDensityTemporarily(float fogDensity)
	{
		ApplyFogDensity(fogDensity);
	}

	public static void RevertToAssignedFogDensity()
	{
		ApplyFogDensity(_config.fogDensity);
	}

	private static void ApplyFogStart(float fogStart)
	{
		RenderSettings.fogStartDistance = fogStart;
		RenderSettings.fogEndDistance = ComputeFogEnd(fogStart, _config.fogDensity);
	}

	public static void AssignNewFogStart(float fogStart)
	{
		ApplyFogStart(fogStart);
		_config.fogStart = fogStart;
		_config.fogStartAssigned = true;
	}

	public static void OverrideFogStartTemporarily(float fogStart)
	{
		ApplyFogStart(fogStart);
	}

	public static void RevertToAssignedFogStart()
	{
		ApplyFogStart(_config.fogStart);
	}

	private static void ApplySkyPaint(string paint)
	{
		if (Blocksworld.skyBoxTintDefinitions.TryGetValue(paint, out var value))
		{
			if (Blocksworld.overheadLight != null)
			{
				Blocksworld.overheadLight.color = 0.5f * (Color.white + value);
			}
			if (RenderSettings.skybox != null)
			{
				RenderSettings.skybox.SetColor("_Tint", value * _config.skyboxDefaultTint);
			}
			if (Blocksworld.ambientLightGradientDefinitions.TryGetValue(paint, out var value2))
			{
				RenderSettings.ambientSkyColor = value2[0];
				RenderSettings.ambientEquatorColor = value2[1];
				RenderSettings.ambientGroundColor = value2[2];
			}
			else
			{
				Blocksworld.ambientLightGradientDefinitions.TryGetValue("Base Ratio", out value2);
				RenderSettings.ambientSkyColor = CombineSkyColors(value, value2[0]);
				RenderSettings.ambientEquatorColor = CombineSkyColors(value, value2[1]);
				RenderSettings.ambientGroundColor = CombineSkyColors(value, value2[2]);
			}
			_config.skyPaint = paint;
			if (!_config.fogPaintAssigned)
			{
				ApplyFogPaint(paint);
			}
		}
	}

	public static void AssignNewSkyPaint(string skyPaint)
	{
		ApplySkyPaint(skyPaint);
		_config.skyPaint = skyPaint;
		_config.skyPaintAssigned = true;
	}

	public static void OverrideSkyColorTemporarily(string tempPaint)
	{
		ApplySkyPaint(tempPaint);
	}

	public static void RevertToAssignedSkyColor()
	{
		ApplySkyPaint(_config.skyPaint);
	}

	private static Color CombineSkyColors(Color c1, Color c2)
	{
		return c1 * c2;
	}

	private static SkyBoxDefinition GetSkyBoxDef(int index)
	{
		if (_skyboxDefinitions == null)
		{
			SkyBoxDefinitions skyBoxDefinitions = Resources.Load<SkyBoxDefinitions>("SkyBoxDefinitions");
			_skyboxDefinitions = skyBoxDefinitions.definitions;
		}
		return _skyboxDefinitions[index];
	}

	public static float ComputeFogEnd(float start, float density)
	{
		if (density > 0f)
		{
			return start + (3000f / Mathf.Sqrt(density) - 299.9f);
		}
		return 2000f;
	}
}
