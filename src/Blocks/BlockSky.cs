using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockSky : Block
{
	public class EnvEffectInfo
	{
		public WeatherEffect effect;

		public string effectName;

		public HashSet<string> textures;

		public string mainTexture;

		public bool changeSkyTexture;
	}

	private bool weatherLocked;

	public bool lockY;

	public float yLock;

	public static Predicate predicateEnvEffect;

	public static Predicate predicateSkyBoxTo;

	private Dictionary<string, LightChanger> lightChangers = new Dictionary<string, LightChanger>();

	private SkyParameters skyParameters;

	private static Color _skyboxDefaultTint;

	private static string _skyBoxPaint;

	private int _currentSkyBoxIndex;

	private float _targetSkyBoxRotation;

	private Tile _setFogTile;

	private Tile _skyBoxTile;

	private float _sunIntensity = 2f;

	private bool copiedRendererMaterial;

	private float _lightIntensityMultiplier = 2f;

	private Color oldColor = Color.black;

	private Color oldSpecColor = Color.black;

	private Color oldEmissiveColor = Color.black;

	private float oldLightIntensity;

	private Vector3 oldGravity = Vector3.up;

	private float oldDrag = -1f;

	private float oldAngularDrag = -1f;

	private const string CLEAR = "Clear";

	private const string RAIN = "Rain";

	private const string METEORS = "Meteors";

	private const string METEORS_2 = "Meteors 2";

	private const string SNOW = "Snow";

	private const string ASH = "Ash";

	private const string LEAVES = "Leaves";

	private const string LEAVES_GREEN = "Leaves Green";

	private const string SAND = "Sand";

	private const string SPACE_DUST = "Space Dust";

	private Color lightColor = Color.white;

	private static List<EnvEffectInfo> envEffectInfos;

	private static Dictionary<string, EnvEffectInfo> effectNameToEnvEffectInfos;

	private static Dictionary<string, EnvEffectInfo> textureToEnvEffectInfos;

	private static Dictionary<WeatherEffect, EnvEffectInfo> effectToEnvEffectInfos;

	private bool inColorTransition;

	private SkyColorState transitionStart;

	private SkyColorState transitionEnd;

	public bool doNotApplyFogColorBackToEnvironmentManager;

	public BlockSky(string skyType, List<List<Tile>> tiles)
		: base(tiles)
	{
		skyParameters = GetSkyParameters();
		go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
		for (int i = 0; i < base.tiles[0].Count; i++)
		{
			Tile tile = base.tiles[0][i];
			if (tile.gaf.Predicate == predicateSkyBoxTo)
			{
				_skyBoxTile = tile;
			}
			else if (tile.gaf.Predicate == Block.predicateSetFog)
			{
				if (tile.gaf.Args.Length < 3)
				{
					tile.gaf = new GAF(Block.predicateSetFog, tile.gaf.Args[0], tile.gaf.Args[1], "White");
				}
				_setFogTile = tile;
			}
		}
		bool flag = skyType == "Sky Space Asteroid";
		int num = (flag ? 9 : 0);
		if (_skyBoxTile == null)
		{
			_skyBoxTile = new Tile(predicateSkyBoxTo, num);
			base.tiles[0].Add(_skyBoxTile);
		}
		else if (flag && Util.GetIntArg(_skyBoxTile.gaf.Args, 0, 0) == 0)
		{
			_skyBoxTile.gaf.Args[0] = num;
		}
		WorldEnvironmentManager.ChangeSkyBoxTemporarily((int)_skyBoxTile.gaf.Args[0]);
		_currentSkyBoxIndex = (int)_skyBoxTile.gaf.Args[0];
		if (skyType == "Sky Oz")
		{
			lockY = false;
			yLock = 0f;
		}
	}

	public new static void Register()
	{
		predicateEnvEffect = PredicateRegistry.Add<BlockSky>("Sky.EnvEffect", null, (Block b) => ((BlockSky)b).SetEnvEffect, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockSky>("Sky.AddSphereLightChanger", null, (Block b) => ((BlockSky)b).AddSphereLightChanger, new Type[5]
		{
			typeof(string),
			typeof(Vector3),
			typeof(Vector3),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockSky>("Sky.SetGravity", null, (Block b) => ((BlockSky)b).SetGravity, new Type[1] { typeof(Vector3) });
		PredicateRegistry.Add<BlockSky>("Sky.SetAtmosphere", null, (Block b) => ((BlockSky)b).SetAtmosphere, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		predicateSkyBoxTo = PredicateRegistry.Add<BlockSky>("Sky.SkyBoxTo", (Block b) => ((BlockSky)b).IsSkyBox, (Block b) => ((BlockSky)b).SetSkyBox, new Type[1] { typeof(int) });
	}

	private Material GetRendererMaterial()
	{
		if (copiedRendererMaterial)
		{
			return go.GetComponent<Renderer>().sharedMaterial;
		}
		copiedRendererMaterial = true;
		return go.GetComponent<Renderer>().material;
	}

	public TileResultCode AddSphereLightChanger(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = ((args.Length == 0) ? string.Empty : ((string)args[0]));
		Vector3 vector = ((args.Length <= 1) ? Vector3.one : ((Vector3)args[1]));
		Vector3 position = ((args.Length <= 2) ? Vector3.zero : ((Vector3)args[2]));
		float radius = ((args.Length <= 3) ? 50f : ((float)args[3]));
		float innerRadius = ((args.Length <= 4) ? 20f : ((float)args[4]));
		if (text != string.Empty)
		{
			SphereLightChanger sphereLightChanger = new SphereLightChanger();
			sphereLightChanger.color = new Color(vector.x, vector.y, vector.z, 1f);
			sphereLightChanger.position = position;
			sphereLightChanger.radius = radius;
			sphereLightChanger.innerRadius = innerRadius;
			lightChangers[text] = sphereLightChanger;
		}
		return TileResultCode.True;
	}

	public TileResultCode SetGravity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Vector3 vector3Arg = Util.GetVector3Arg(args, 0, Vector3.zero);
		if (oldGravity != vector3Arg)
		{
			Physics.gravity = vector3Arg;
			foreach (Block item in BWSceneManager.AllBlocks())
			{
				item.SetVaryingGravity(vg: true);
			}
			oldGravity = vector3Arg;
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAtmosphere(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0f);
		float floatArg2 = Util.GetFloatArg(args, 1, 0f);
		if (oldDrag != floatArg)
		{
			Blocksworld.dragMultiplier = floatArg;
			Blocksworld.UpdateDrag();
			oldDrag = floatArg;
		}
		if (oldAngularDrag != floatArg2)
		{
			Blocksworld.angularDragMultiplier = floatArg2;
			Blocksworld.UpdateAngularDrag();
			oldAngularDrag = floatArg2;
		}
		return TileResultCode.True;
	}

	public override bool HasDynamicalLight()
	{
		return true;
	}

	public override Color GetDynamicalLightTint()
	{
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		Color white = Color.white;
		foreach (KeyValuePair<string, LightChanger> lightChanger in lightChangers)
		{
			white *= lightChanger.Value.GetLightTint(cameraPosition);
		}
		return white;
	}

	public TileResultCode SetEnvEffect(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string envEffect = ((args.Length == 0) ? "Clear" : ((string)args[0]));
		SetEnvEffect(envEffect);
		return TileResultCode.True;
	}

	private void SetEnvEffect(string effect)
	{
		UpdateWeather(GetWeatherEffectByName(effect));
	}

	public override Color GetLightTint()
	{
		return lightColor;
	}

	public static List<string> GetAllEnvEffects()
	{
		List<string> list = new List<string>();
		CreateEffectInfos();
		foreach (EnvEffectInfo envEffectInfo in envEffectInfos)
		{
			list.Add(envEffectInfo.effectName);
		}
		return list;
	}

	private static void CreateEffectInfos()
	{
		if (envEffectInfos != null)
		{
			return;
		}
		envEffectInfos = new List<EnvEffectInfo>
		{
			new EnvEffectInfo
			{
				effect = WeatherEffect.clear,
				effectName = "Clear",
				textures = new HashSet<string> { "Plain" },
				mainTexture = "Plain",
				changeSkyTexture = true
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.snow,
				effectName = "Snow",
				textures = new HashSet<string> { "Ice Material", "Ice Material_Sky" },
				mainTexture = "Ice Material",
				changeSkyTexture = true
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.rain,
				effectName = "Rain",
				textures = new HashSet<string> { "Texture Water Stream" },
				mainTexture = "Texture Water Stream"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.sandStorm,
				effectName = "Sand",
				textures = new HashSet<string> { "Sand", "Sand_Sky" },
				mainTexture = "Sand"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.greenLeaves,
				effectName = "Leaves Green",
				textures = new HashSet<string> { "Grass", "Grass Castle", "Grass_Sky", "Grass Castle_Sky" },
				mainTexture = "Grass"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.autumnLeaves,
				effectName = "Leaves",
				textures = new HashSet<string> { "Grass 2" },
				mainTexture = "Grass 2"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.ash,
				effectName = "Ash",
				textures = new HashSet<string> { "Texture Lava" },
				mainTexture = "Texture Lava"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.meteors,
				effectName = "Meteors",
				textures = new HashSet<string> { "Rock" },
				mainTexture = "Rock"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.meteors2,
				effectName = "Meteors 2",
				textures = new HashSet<string> { "Texture Crater" },
				mainTexture = "Texture Crater"
			},
			new EnvEffectInfo
			{
				effect = WeatherEffect.spaceDust,
				effectName = "Space Dust",
				textures = new HashSet<string> { "Dust" },
				mainTexture = "Dust"
			}
		};
		foreach (EnvEffectInfo envEffectInfo in envEffectInfos)
		{
			effectNameToEnvEffectInfos[envEffectInfo.effectName] = envEffectInfo;
			foreach (string texture in envEffectInfo.textures)
			{
				textureToEnvEffectInfos[texture] = envEffectInfo;
			}
			effectToEnvEffectInfos[envEffectInfo.effect] = envEffectInfo;
		}
	}

	public static WeatherEffect GetWeatherEffectByName(string name)
	{
		CreateEffectInfos();
		if (effectNameToEnvEffectInfos.TryGetValue(name, out var value))
		{
			return value.effect;
		}
		return WeatherEffect.clear;
	}

	public static WeatherEffect GetWeatherEffect(string texture)
	{
		CreateEffectInfos();
		if (textureToEnvEffectInfos.TryGetValue(texture, out var value))
		{
			return value.effect;
		}
		return WeatherEffect.clear;
	}

	public static string EnvEffectToTexture(string effect)
	{
		CreateEffectInfos();
		if (effectNameToEnvEffectInfos.TryGetValue(effect, out var value))
		{
			return value.mainTexture;
		}
		return "Plain";
	}

	public static string SkyTextureToEnvEffect(string texture)
	{
		CreateEffectInfos();
		if (textureToEnvEffectInfos.TryGetValue(texture, out var value))
		{
			return value.effectName;
		}
		return "Clear";
	}

	public static bool IsSkyTexture(string texture)
	{
		CreateEffectInfos();
		return textureToEnvEffectInfos.ContainsKey(texture);
	}

	public static bool ChangeSkyTexture(string texture)
	{
		CreateEffectInfos();
		if (textureToEnvEffectInfos.TryGetValue(texture, out var value))
		{
			return value.changeSkyTexture;
		}
		return false;
	}

	public int GetSkyBoxIndex()
	{
		return _currentSkyBoxIndex;
	}

	public void SetSkyBoxIndex(int sbIndex)
	{
		_currentSkyBoxIndex = sbIndex;
		_skyBoxTile.gaf.Args[0] = sbIndex;
	}

	public TileResultCode IsSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		return boolToTileResult(intArg == _currentSkyBoxIndex);
	}

	public TileResultCode SetSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
	{
		WorldEnvironmentManager.ChangeSkyBoxTemporarily(Util.GetIntArg(args, 0, 0));
		return TileResultCode.True;
	}

	public float GetSkyBoxRotation()
	{
		return WorldEnvironmentManager.SkyBoxRotation();
	}

	public void SetSkyBoxRotation(float angle, bool immediate)
	{
		_targetSkyBoxRotation = angle;
		if (Blocksworld.isFirstFrame || immediate)
		{
			WorldEnvironmentManager.OverrideSkyBoxRotationTemporarily(_targetSkyBoxRotation);
		}
	}

	public float GetSunIntensity()
	{
		return _sunIntensity;
	}

	public void SetSunIntensity(float intensity)
	{
		_sunIntensity = intensity;
		Blocksworld.UpdateSunIntensity(_sunIntensity);
	}

	public override void Reset(bool forceRescale = false)
	{
		weatherLocked = true;
		base.Reset(forceRescale);
		weatherLocked = false;
		copiedRendererMaterial = false;
	}

	public TileResultCode TransitionPaintTo(string paint, int meshIndex, float duration, float timer)
	{
		Material rendererMaterial = GetRendererMaterial();
		if (timer == 0f)
		{
			if (inColorTransition)
			{
				return TileResultCode.False;
			}
			transitionStart = new SkyColorState();
			transitionStart.color = rendererMaterial.GetColor("_Color");
			transitionStart.emissiveColor = rendererMaterial.GetColor("_Emission");
			transitionStart.specColor = rendererMaterial.GetColor("_SpecColor");
			transitionStart.lightColor = lightColor;
			transitionStart.lightIntensity = Blocksworld.directionalLight.GetComponent<Light>().intensity;
			string texture = GetTexture();
			if (!Materials.shaders.TryGetValue(texture, out var value))
			{
				BWLog.Info("Could not find shader for texture " + texture);
				return TileResultCode.True;
			}
			Material material = Materials.GetMaterial(paint, texture, value);
			if (!(material != null))
			{
				BWLog.Info("Failed to find material for " + paint + " " + texture + " " + value);
				return TileResultCode.True;
			}
			transitionEnd = new SkyColorState();
			transitionEnd.color = material.GetColor("_Color");
			transitionEnd.specColor = material.GetColor("_SpecColor");
			transitionEnd.lightColor = Color.white;
			transitionEnd.lightIntensity = 1f;
			GetSkyColor(paint, ref transitionEnd.color, ref transitionEnd.specColor, ref transitionEnd.lightColor, ref transitionEnd.lightIntensity);
			transitionEnd.emissiveColor = material.GetColor("_Emission");
			WorldEnvironmentManager.AssignNewSkyPaint(paint);
			SetFogColor(paint);
		}
		else
		{
			if (timer >= duration)
			{
				inColorTransition = false;
				PaintTo(paint, permanent: false, meshIndex);
				return TileResultCode.True;
			}
			float num = timer / duration;
			float num2 = 1f - num;
			Color color = Color.Lerp(transitionStart.color, transitionEnd.color, num);
			Color emissiveColor = Color.Lerp(transitionStart.emissiveColor, transitionEnd.emissiveColor, num);
			Color specColor = Color.Lerp(transitionStart.specColor, transitionEnd.specColor, num);
			lightColor = Color.Lerp(transitionStart.lightColor, transitionEnd.lightColor, num);
			float lightIntensity = num2 * transitionStart.lightIntensity + num * transitionEnd.lightIntensity;
			WorldEnvironmentManager.AssignNewSkyPaint(paint);
			SetFogColor(paint);
			UpdateColorAndLightState(rendererMaterial, color, emissiveColor, specColor, lightIntensity);
		}
		return TileResultCode.Delayed;
	}

	public void SetFogColor(string paint)
	{
		if (Blocksworld.colorDefinitions.TryGetValue(paint, out var value))
		{
			Blocksworld.UpdateFogColor(value[1]);
		}
		if (_setFogTile != null)
		{
			_setFogTile.gaf.Args[2] = paint;
		}
		if (!doNotApplyFogColorBackToEnvironmentManager)
		{
			WorldEnvironmentManager.AssignNewFogPaint(paint);
		}
	}

	public static void GetSkyColor(string paint, ref Color color, ref Color specColor, ref Color lightColor, ref float lightIntensity)
	{
		if (Blocksworld.colorDefinitions.TryGetValue(paint, out var value))
		{
			color = value[1];
		}
		if (Blocksworld.colorDefinitions.TryGetValue("Sky " + paint, out var value2))
		{
			color = value2[0];
		}
		specColor = color;
		if (Blocksworld.colorDefinitions.TryGetValue("Sky Spec " + paint, out var value3))
		{
			specColor = value3[0];
		}
		bool flag = false;
		if (Blocksworld.colorDefinitions.TryGetValue("Sky Light Color " + paint, out var value4))
		{
			lightColor = value4[0];
			flag = true;
		}
		if (Blocksworld.IsLuminousPaint(paint))
		{
			lightIntensity = 1.1f;
			if (!flag)
			{
				lightColor = 0.5f * Color.white + 0.5f * color;
			}
		}
		else if (paint.StartsWith("Darker"))
		{
			lightIntensity = 0.4f;
		}
		else if (paint.StartsWith("Dark") || paint.StartsWith("Earth") || paint == "Black")
		{
			lightIntensity = 0.55f;
		}
		else if (paint.StartsWith("Light") || paint == "White")
		{
			lightIntensity = 0.9f;
		}
		else if (paint == "Super Black")
		{
			lightIntensity = 0.2f;
		}
		else
		{
			lightIntensity = 0.7f;
		}
		if (!Blocksworld.renderingShadows)
		{
			lightIntensity = (lightIntensity + 2f) * 0.33f;
		}
	}

	private static Color CombineSkyColors(Color c1, Color c2)
	{
		return c1 * c2;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		base.PaintTo(paint, permanent, meshIndex);
		if (permanent)
		{
			for (int i = 0; i < tiles[0].Count; i++)
			{
				Tile tile = tiles[0][i];
				if (tile.gaf.Predicate.Name == "Block.SetFog" && tile.gaf.Args.Length > 2)
				{
					tile.gaf.Args[2] = paint;
					break;
				}
			}
		}
		Material rendererMaterial = GetRendererMaterial();
		Materials.materialCachePaint[rendererMaterial] = paint;
		lightColor = Color.white;
		Color color = rendererMaterial.GetColor("_Color");
		Color color2 = rendererMaterial.GetColor("_Emission");
		Color specColor = color;
		oldColor = color;
		float lightIntensity = 1f;
		GetSkyColor(paint, ref color, ref specColor, ref lightColor, ref lightIntensity);
		WorldEnvironmentManager.AssignNewSkyPaint(paint);
		SetFogColor(paint);
		UpdateColorAndLightState(rendererMaterial, color, color2, specColor, lightIntensity);
		return TileResultCode.True;
	}

	private bool colorDiff(Color c1, Color c2)
	{
		return new Vector4(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b, c1.a - c2.a).magnitude > 0.01f;
	}

	private void UpdateColorAndLightState(Material m, Color color, Color emissiveColor, Color specColor, float lightIntensity)
	{
		if (colorDiff(color, oldColor) || colorDiff(emissiveColor, oldEmissiveColor) || colorDiff(specColor, oldSpecColor) || Mathf.Abs(lightIntensity - oldLightIntensity) > 0.01f)
		{
			oldEmissiveColor = emissiveColor;
			oldSpecColor = specColor;
			oldLightIntensity = lightIntensity;
			m.SetColor("_SpecColor", specColor);
			m.SetColor("_Color", color);
			m.SetColor("_Emission", emissiveColor);
			_lightIntensityMultiplier = lightIntensity;
			WorldEnvironmentManager.RevertToAssignedSkyColor();
			WorldEnvironmentManager.RevertToAssignedFogPaint();
			Blocksworld.UpdateLightColor(updateFog: false);
			Blocksworld.UpdateDynamicalLights(updateFog: false, forceUpdate: true);
			Blocksworld.UpdateFogColor(GetFogColor());
			Blocksworld.UpdateSunLight(color, emissiveColor, _lightIntensityMultiplier, _sunIntensity);
		}
	}

	public override float GetLightIntensityMultiplier()
	{
		return _lightIntensityMultiplier;
	}

	public static Color GetFogColor()
	{
		if (Blocksworld.renderingSkybox)
		{
			return RenderSettings.fogColor;
		}
		if (Blocksworld.worldSky != null && Blocksworld.worldSky.go != null)
		{
			Material sharedMaterial = Blocksworld.worldSky.renderer.sharedMaterial;
			Color result = 1.5f * sharedMaterial.GetColor("_Color");
			Color color = Blocksworld.directionalLight.GetComponent<Light>().color;
			result.r *= color.r;
			result.g *= color.g;
			result.b *= color.b;
			return result;
		}
		return Color.white;
	}

	public override void OnCreate()
	{
		base.OnCreate();
		weatherLocked = true;
		TextureTo(GetTexture(), GetTextureNormal(), permanent: true);
		weatherLocked = false;
		UpdateWeatherFromEnvEffectTile();
		UpdateFarplane();
	}

	private void UpdateWeatherFromEnvEffectTile()
	{
		GAF simpleInitGAF = GetSimpleInitGAF("Sky.EnvEffect");
		if (simpleInitGAF == null)
		{
			UpdateWeatherCurrent();
		}
		else
		{
			SetEnvEffect((string)simpleInitGAF.Args[0]);
		}
	}

	public override void FixedUpdate()
	{
		if (Blocksworld.isFirstFrame)
		{
			_sunIntensity = 1f;
			_targetSkyBoxRotation = 0f;
		}
		base.FixedUpdate();
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.worldOceanBlock != null && Blocksworld.worldOceanBlock.go != null && Blocksworld.worldOceanBlock.go.GetComponent<Collider>() != null)
		{
			Material rendererMaterial = GetRendererMaterial();
			if (rendererMaterial != null)
			{
				rendererMaterial.SetFloat("_WaterLevel", Blocksworld.worldOceanBlock.GetWaterBounds().max.y);
				rendererMaterial.SetVector("_OverlayCosSinAngles", new Vector4(Mathf.Cos(0.01f * Time.time), Mathf.Sin(0.01f * Time.time)));
			}
		}
		float skyBoxRotation = GetSkyBoxRotation();
		if (!Util.FloatsClose(_targetSkyBoxRotation, skyBoxRotation))
		{
			WorldEnvironmentManager.OverrideSkyBoxRotationTemporarily(Mathf.LerpAngle(skyBoxRotation, _targetSkyBoxRotation, 0.1f));
		}
	}

	public override void OnReconstructed()
	{
		base.OnReconstructed();
		UpdateWeatherFromEnvEffectTile();
	}

	public void UpdateWeatherCurrent()
	{
		string texture = GetTexture();
		texture = Scarcity.GetNormalizedTexture(texture);
		UpdateWeather(texture);
	}

	private void UpdateWeather(string texture)
	{
		WeatherEffect weatherEffect = GetWeatherEffect(texture);
		UpdateWeather(weatherEffect);
	}

	public static string GetEnvEffectName(WeatherEffect newWeather)
	{
		if (newWeather == WeatherEffect.snow)
		{
			return "Snow";
		}
		if (newWeather == WeatherEffect.ash)
		{
			return "Ash";
		}
		if (newWeather == WeatherEffect.sandStorm)
		{
			return "Sand";
		}
		if (newWeather == WeatherEffect.rain)
		{
			return "Rain";
		}
		if (newWeather == WeatherEffect.meteors)
		{
			return "Meteors";
		}
		if (newWeather == WeatherEffect.autumnLeaves)
		{
			return "Leaves";
		}
		if (newWeather == WeatherEffect.greenLeaves)
		{
			return "Leaves Green";
		}
		if (newWeather == WeatherEffect.spaceDust)
		{
			return "Space Dust";
		}
		return "Clear";
	}

	public void UpdateWeather(WeatherEffect newWeather, bool permanent = true)
	{
		if (Blocksworld.weather != newWeather)
		{
			Blocksworld.weather.Stop();
			Blocksworld.weather = newWeather;
		}
		newWeather.Start();
		newWeather.IntensityMultiplier = 1f;
		if (permanent)
		{
			SetSimpleInitTile(new GAF("Sky.EnvEffect", GetEnvEffectName(newWeather)));
		}
		Blocksworld.bw.SetFogMultiplier(newWeather.GetFogMultiplier());
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		texture = Scarcity.GetNormalizedTexture(texture);
		if (!weatherLocked)
		{
			UpdateWeather(texture);
		}
		bool flag = IsSkyTexture(texture);
		bool flag2 = ChangeSkyTexture(texture);
		if (texture == "Plain")
		{
			string defaultTexture = GetDefaultTexture(meshIndex);
			flag = IsSkyTexture(defaultTexture);
			texture = defaultTexture;
			force = true;
		}
		if (flag)
		{
			if (flag2)
			{
				texture += "_Sky";
			}
			else
			{
				texture = GetDefaultTexture(meshIndex);
				if (ChangeSkyTexture(texture))
				{
					texture += "_Sky";
				}
			}
		}
		if (skyParameters != null && !skyParameters.canChangeTexture)
		{
			return TileResultCode.True;
		}
		if (flag)
		{
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, flag || force);
			if (copiedRendererMaterial)
			{
				Materials.materialCacheTexture[GetRendererMaterial()] = GetTexture();
			}
			return result;
		}
		return TileResultCode.True;
	}

	public override void Play()
	{
		oldGravity = Vector3.up;
		base.Play();
		go.GetComponent<Collider>().enabled = false;
		go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		go.GetComponent<Collider>().enabled = true;
		go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
	}

	public SkyParameters GetSkyParameters()
	{
		return go.GetComponent<SkyParameters>();
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		goT.localScale = scale;
		UpdateFarplane();
		return true;
	}

	private void UpdateFarplane()
	{
		MeshRenderer component = go.GetComponent<MeshRenderer>();
		if (component != null)
		{
			float farClipPlane = Mathf.Max(0.51f * component.bounds.size.magnitude, 400f);
			Blocksworld.mainCamera.farClipPlane = farClipPlane;
		}
	}

	public override void Destroy()
	{
		if (copiedRendererMaterial)
		{
			Material rendererMaterial = GetRendererMaterial();
			Materials.materialCachePaint.Remove(rendererMaterial);
			Materials.materialCacheTexture.Remove(rendererMaterial);
			UnityEngine.Object.Destroy(rendererMaterial);
			copiedRendererMaterial = false;
		}
		base.Destroy();
	}

	static BlockSky()
	{
		_skyBoxPaint = "White";
		envEffectInfos = null;
		effectNameToEnvEffectInfos = new Dictionary<string, EnvEffectInfo>();
		textureToEnvEffectInfos = new Dictionary<string, EnvEffectInfo>();
		effectToEnvEffectInfos = new Dictionary<WeatherEffect, EnvEffectInfo>();
	}
}
