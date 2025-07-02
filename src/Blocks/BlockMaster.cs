using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockMaster : BlockAbstractUI
{
	private bool informedBlocksAboutVaryingGravity;

	private Dictionary<string, Block> terrainBlocks;

	public static Predicate predicatePaintSkyTo;

	public static Predicate predicateSkyBoxTo;

	public static Predicate predicateSetEnvEffect;

	public static Predicate predicateSetLiquidProperties;

	public static Predicate predicateIncreaseWaterLevel;

	public static Predicate predicateStepIncreaseWaterLevel;

	public static Predicate predicateSetMaxPositiveWaterLevelOffset;

	public static Predicate predicateSetMaxNegativeWaterLevelOffset;

	private float earthQuakeStrength;

	private float cameraShakeStrength;

	private float prolongedEarthQuakeStrength;

	private float prolongedCameraShakeStrength;

	private float earthQuakeStartTime;

	private float cameraShakeStartTime;

	private string toEnvEffect;

	private string fromEnvEffect;

	private float fromEnvEffectIntensity = 1f;

	private string _fogPaint = "White";

	private float _fogDensity = 10f;

	private float _fogStart = 100f;

	private bool tiltGravity;

	private bool trackingTilt;

	private bool tiltGravityFlat;

	private float tiltGravityStrength = 1f;

	internal static RigidbodyConstraints masterRBConstraints;

	private static bool masterRBConstraintsUpdated;

	private Dictionary<Block, int> earthquakeBlocks = new Dictionary<Block, int>();

	private const float DEFAULT_GRAVITY = 9.82f;

	public static GravityDefinitions gravDefs;

	public static AtmosphereDefinitions atmosDefs;

	public BlockMaster(List<List<Tile>> tiles)
		: base(tiles, 0)
	{
	}

	public override void Play()
	{
		base.Play();
		informedBlocksAboutVaryingGravity = false;
		ClearTerrainBlocks();
		earthQuakeStrength = 0f;
		cameraShakeStrength = 0f;
		prolongedEarthQuakeStrength = 0f;
		prolongedCameraShakeStrength = 0f;
		cameraShakeStartTime = -10f;
		earthQuakeStartTime = -10f;
		toEnvEffect = null;
		fromEnvEffect = null;
		fromEnvEffectIntensity = 1f;
		masterRBConstraints = RigidbodyConstraints.None;
		masterRBConstraintsUpdated = false;
	}

	public override void Stop(bool resetBlock = true)
	{
		masterRBConstraints = RigidbodyConstraints.None;
		base.Stop(resetBlock);
		ClearTerrainBlocks();
		earthquakeBlocks.Clear();
	}

	public static GAF ReplaceGaf(GAF gaf)
	{
		Predicate predicate = gaf.Predicate;
		object[] args = gaf.Args;
		if (predicate == Block.predicatePaintTo)
		{
			return new GAF(predicatePaintSkyTo, args[0], 0, 5f);
		}
		if (predicate == Block.predicateTextureTo)
		{
			string texture = (string)args[0];
			if (BlockSky.IsSkyTexture(texture))
			{
				string text = BlockSky.SkyTextureToEnvEffect(texture);
				return new GAF(predicateSetEnvEffect, text, 1f, 10f);
			}
		}
		return gaf;
	}

	private void ClearTerrainBlocks()
	{
		if (terrainBlocks != null)
		{
			terrainBlocks.Clear();
			terrainBlocks = null;
		}
	}

	public new static void Register()
	{
		predicatePaintSkyTo = PredicateRegistry.Add<BlockMaster>("Master.PaintSkyTo", (Block b) => b.IsSkyPaintedTo, (Block b) => b.PaintSkyToAction, new Type[3]
		{
			typeof(string),
			typeof(int),
			typeof(float)
		}, new string[3] { "Paint", "Mesh index", "Duration" });
		PredicateRegistry.Add<BlockMaster>("Master.RotateSkyTo", (Block b) => ((BlockMaster)b).IsSkyRotatedTo, (Block b) => ((BlockMaster)b).SkyRotateToAction, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		PredicateRegistry.Add<BlockMaster>("Master.SetSunIntensity", (Block b) => ((BlockMaster)b).IsSunIntensity, (Block b) => ((BlockMaster)b).SetSunIntensity, new Type[1] { typeof(float) }, new string[1] { "Intensity" });
		predicateSkyBoxTo = PredicateRegistry.Add<BlockMaster>("Master.SkyBoxTo", (Block b) => ((BlockMaster)b).IsSkyBox, (Block b) => ((BlockMaster)b).SetSkyBox, new Type[1] { typeof(int) });
		predicateSetEnvEffect = PredicateRegistry.Add<BlockMaster>("Master.SetEnvEffect", (Block b) => ((BlockMaster)b).IsEnvEffect, (Block b) => ((BlockMaster)b).SetEnvEffect, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3] { "Effect name", "Intensity", "Fade Time" });
		PredicateRegistry.Add<BlockMaster>("Master.SetGravity", null, (Block b) => ((BlockMaster)b).SetGravity, new Type[1] { typeof(Vector3) }, new string[1] { "Gravity field" });
		PredicateRegistry.Add<BlockMaster>("Master.SetWorldGravity", null, (Block b) => ((BlockMaster)b).SetWorldGravity, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.SetDragMult", null, (Block b) => ((BlockMaster)b).SetDragMultiplier, new Type[1] { typeof(float) }, new string[1] { "Multiplier" });
		PredicateRegistry.Add<BlockMaster>("Master.SetAngularDragMult", null, (Block b) => ((BlockMaster)b).SetAngularDragMultiplier, new Type[1] { typeof(float) }, new string[1] { "Multiplier" });
		PredicateRegistry.Add<BlockMaster>("Master.SetAtmosphere", null, (Block b) => ((BlockMaster)b).SetAtmosphere, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.ShowUI", null, (Block b) => ((BlockAbstractUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.ShowPhysical", null, (Block b) => ((BlockAbstractUI)b).ShowPhysical, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.SetBackgroundMusic", null, (Block b) => b.SetBackgroundMusic, new Type[1] { typeof(string) }, new string[1] { "Music name" });
		PredicateRegistry.Add<BlockMaster>("Master.PaintTerrainTo", null, (Block b) => ((BlockMaster)b).PaintTerrainTo, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Terrain block", "Paint" });
		PredicateRegistry.Add<BlockMaster>("Master.TextureTerrainTo", null, (Block b) => ((BlockMaster)b).TextureTerrainTo, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, new string[2] { "Terrain block", "Texture" });
		PredicateRegistry.Add<BlockMaster>("Master.SetWaterBuoyancyMultiplier", null, (Block b) => ((BlockMaster)b).SetWaterBuoyancyMultiplier, new Type[1] { typeof(float) }, new string[1] { "Multiplier" });
		PredicateRegistry.Add<BlockMaster>("Master.PullLock", null, (Block b) => ((BlockMaster)b).PullLockGlobal);
		PredicateRegistry.Add<BlockMaster>("Master.DisableAutoFollow", null, (Block b) => ((BlockMaster)b).DisableAutoFollow);
		PredicateRegistry.Add<BlockMaster>("Master.Earthquake", null, (Block b) => ((BlockMaster)b).Earthquake, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMaster>("Master.CameraShake", null, (Block b) => ((BlockMaster)b).CameraShake, new Type[1] { typeof(float) });
		predicateIncreaseWaterLevel = PredicateRegistry.Add<BlockMaster>("Master.IncreaseWaterLevel", null, (Block b) => ((BlockMaster)b).IncreaseWaterLevel, new Type[1] { typeof(float) });
		predicateStepIncreaseWaterLevel = PredicateRegistry.Add<BlockMaster>("Master.StepIncreaseWaterLevel", null, (Block b) => ((BlockMaster)b).StepIncreaseWaterLevel, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Units", "Duration" });
		predicateSetLiquidProperties = PredicateRegistry.Add<BlockMaster>("Master.SetLiquidProperties", null, (Block b) => ((BlockMaster)b).SetLiquidProperties, new Type[1] { typeof(string) });
		predicateSetMaxPositiveWaterLevelOffset = PredicateRegistry.Add<BlockMaster>("Master.SetMaxPositiveWaterLevelOffset", (Block b) => ((BlockMaster)b).AtMaxPositiveWaterLevelOffset, (Block b) => ((BlockMaster)b).SetMaxPositiveWaterLevelOffset, new Type[1] { typeof(float) });
		predicateSetMaxNegativeWaterLevelOffset = PredicateRegistry.Add<BlockMaster>("Master.SetMaxNegativeWaterLevelOffset", (Block b) => ((BlockMaster)b).AtMaxNegativeWaterLevelOffset, (Block b) => ((BlockMaster)b).SetMaxNegativeWaterLevelOffset, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMaster>("Master.SetEnvEffectAngle", null, (Block b) => ((BlockMaster)b).SetEnvEffectAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMaster>("Master.FogColorTo", (Block b) => ((BlockMaster)b).IsFogColor, (Block b) => ((BlockMaster)b).SetFogColor, new Type[2]
		{
			typeof(int),
			typeof(string)
		});
		PredicateRegistry.Add<BlockMaster>("Master.FogDensityTo", (Block b) => ((BlockMaster)b).IsFogDensity, (Block b) => ((BlockMaster)b).SetFogDensity, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMaster>("Master.FogStartTo", (Block b) => ((BlockMaster)b).IsFogStart, (Block b) => ((BlockMaster)b).SetFogStart, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMaster>("Master.TiltGravity", null, (Block b) => ((BlockMaster)b).TiltGravity, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		PredicateRegistry.Add<BlockMaster>("Master.ConstrainTranslation", (Block b) => ((BlockMaster)b).IsConstrainTranslationMaster, (Block b) => ((BlockMaster)b).ConstrainTranslationMaster, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.FreeTranslation", (Block b) => ((BlockMaster)b).IsFreeTranslationMaster, (Block b) => ((BlockMaster)b).FreeTranslationMaster, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.ConstrainRotation", (Block b) => ((BlockMaster)b).IsConstrainRotationMaster, (Block b) => ((BlockMaster)b).ConstrainRotationMaster, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("Master.FreeRotation", (Block b) => ((BlockMaster)b).IsFreeRotationMaster, (Block b) => ((BlockMaster)b).FreeRotationMaster, new Type[1] { typeof(int) });
		SetupGravityAndAtmosphereDefinitions();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (masterRBConstraintsUpdated)
		{
			for (int i = 0; i < Blocksworld.chunks.Count; i++)
			{
				Blocksworld.chunks[i].UpdateRBConstraints();
			}
			masterRBConstraintsUpdated = false;
		}
		float magnitude = Physics.gravity.magnitude;
		if (tiltGravity && TiltManager.Instance.IsMonitoring())
		{
			Vector3 vector = ((!tiltGravityFlat) ? TiltManager.Instance.GetRelativeGravityVector() : TiltManager.Instance.GetGravityVector());
			Vector3 vec = ((!((Blocksworld.cameraForward - Vector3.down).sqrMagnitude < 0.1f)) ? Blocksworld.cameraForward : Blocksworld.cameraUp);
			Vector3 vector2 = Util.ProjectOntoPlane(vec, Vector3.up);
			Vector3 vector3 = Util.ProjectOntoPlane(Blocksworld.cameraRight, Vector3.up);
			vector2.Normalize();
			vector3.Normalize();
			Vector3 normalized = (vector.x * vector3 + vector.y * vector2 + vector.z * Vector3.up).normalized;
			Physics.gravity = normalized * magnitude;
		}
		else
		{
			Physics.gravity = magnitude * Vector3.down;
		}
		tiltGravity = false;
		if (Sound.sfxEnabled && (earthQuakeStrength > 0f || prolongedEarthQuakeStrength > 0f))
		{
			float num = Mathf.Max(earthQuakeStrength, prolongedEarthQuakeStrength);
			PlaySound("Earthquake Loop", "Block", "Loop", Mathf.Min(num * 0.5f, 1f), 1f);
		}
		float fixedTime = Time.fixedTime;
		if (earthQuakeStrength > 0f || prolongedEarthQuakeStrength > 0f)
		{
			foreach (Block item in CollisionManager.blocksOnGround)
			{
				earthquakeBlocks[item] = 0;
			}
			CollisionManager.blocksOnGround.Clear();
			float num2 = Mathf.Max(earthQuakeStrength, prolongedEarthQuakeStrength);
			foreach (Block item2 in new List<Block>(earthquakeBlocks.Keys))
			{
				int num3 = earthquakeBlocks[item2];
				if (num3 < 4)
				{
					num3++;
					earthquakeBlocks[item2] = num3;
					GameObject gameObject = item2.go;
					if (gameObject != null)
					{
						Chunk chunk = item2.chunk;
						Rigidbody rb = chunk.rb;
						if (rb != null && !rb.isKinematic)
						{
							Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
							insideUnitSphere.y = Mathf.Abs(insideUnitSphere.y);
							Vector3 force = insideUnitSphere * num2 * rb.mass;
							Vector3 position = gameObject.transform.position;
							rb.AddForceAtPosition(force, position, ForceMode.Impulse);
						}
					}
				}
				else
				{
					earthquakeBlocks.Remove(item2);
				}
			}
			if (earthQuakeStrength > 0f)
			{
				prolongedEarthQuakeStrength = earthQuakeStrength;
			}
			else if (fixedTime > earthQuakeStartTime + 1f)
			{
				prolongedEarthQuakeStrength = 0f;
			}
			earthQuakeStrength = 0f;
		}
		if (cameraShakeStrength > 0f || prolongedCameraShakeStrength > 0f)
		{
			float strength = Mathf.Max(cameraShakeStrength, prolongedCameraShakeStrength);
			CameraShake(strength);
			if (cameraShakeStrength > 0f)
			{
				prolongedCameraShakeStrength = cameraShakeStrength;
			}
			else if (fixedTime > cameraShakeStartTime + 1f)
			{
				prolongedCameraShakeStrength = 0f;
			}
			cameraShakeStrength = 0f;
		}
	}

	public static void CameraShake(float strength)
	{
		float num = strength * 2f * (Mathf.PerlinNoise(Time.time * 20f, 0f) - 0.5f);
		float angle = 360f * (Mathf.PerlinNoise(Time.time * 2f + 232.2f, 0f) - 0.5f);
		Vector3 offset = num * (Quaternion.AngleAxis(angle, Blocksworld.cameraForward) * Blocksworld.cameraUp);
		Blocksworld.blocksworldCamera.AddImmediateOffset(offset);
	}

	public TileResultCode TiltGravity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		tiltGravity = true;
		tiltGravityStrength = 0.25f * Util.GetFloatArg(args, 0, 4f);
		tiltGravityFlat = Util.GetIntArg(args, 1, 0) > 0;
		Blocksworld.UI.Controls.UpdateTiltPrompt();
		return TileResultCode.True;
	}

	public TileResultCode SetEnvEffectAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0f);
		Blocksworld.weather.SetEffectAngle(floatArg);
		return TileResultCode.True;
	}

	public TileResultCode SetLiquidProperties(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.worldOceanBlock?.SetLiquidProperties(Util.GetStringArg(args, 0, "Water"));
		return TileResultCode.True;
	}

	public TileResultCode AtMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (worldOceanBlock != null)
		{
			if (worldOceanBlock.AtMaxPositiveWaterLevelOffset())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode AtMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (worldOceanBlock != null)
		{
			if (worldOceanBlock.AtMaxNegativeWaterLevelOffset())
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.worldOceanBlock?.SetMaxPositiveWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
		return TileResultCode.True;
	}

	public TileResultCode SetMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.worldOceanBlock?.SetMaxNegativeWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
		return TileResultCode.True;
	}

	public TileResultCode IncreaseWaterLevel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (worldOceanBlock != null)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 0f);
			worldOceanBlock.IncreaseWaterLevel(num * Blocksworld.fixedDeltaTime);
		}
		return TileResultCode.True;
	}

	public TileResultCode StepIncreaseWaterLevel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (worldOceanBlock != null)
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			float num = Util.GetFloatArg(args, 1, 1f);
			if (num < 0.01f)
			{
				num = 1f;
			}
			float num2 = eInfo.floatArg * floatArg / num;
			worldOceanBlock.IncreaseWaterLevel(num2 * Blocksworld.fixedDeltaTime);
			if (!(eInfo.timer < num))
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode Earthquake(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		if (prolongedEarthQuakeStrength == 0f)
		{
			earthQuakeStartTime = Time.fixedTime;
		}
		if (prolongedCameraShakeStrength == 0f)
		{
			cameraShakeStartTime = Time.fixedTime;
		}
		earthQuakeStrength += num;
		cameraShakeStrength += 0.5f * num;
		return TileResultCode.True;
	}

	public TileResultCode CameraShake(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		if (prolongedCameraShakeStrength == 0f)
		{
			cameraShakeStartTime = Time.fixedTime;
		}
		cameraShakeStrength += num;
		return TileResultCode.True;
	}

	public TileResultCode PullLockGlobal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.dynamicLockPull = true;
		Blocksworld.AddResetStateCommand(new UnlockDynamicLockPullCommand());
		return TileResultCode.True;
	}

	public TileResultCode DisableAutoFollow(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Blocksworld.blocksworldCamera.EnableAutoFollow(e: false);
		Blocksworld.AddResetStateCommand(new DelegateCommand(delegate
		{
			Blocksworld.blocksworldCamera.EnableAutoFollow(e: true);
		}));
		return TileResultCode.True;
	}

	public TileResultCode SetWaterBuoyancyMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Blocksworld.worldOceanBlock != null)
		{
			Blocksworld.worldOceanBlock.buoyancyMultiplier = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		}
		return TileResultCode.True;
	}

	private Block GetTerrainBlock(string name)
	{
		if (terrainBlocks == null)
		{
			terrainBlocks = new Dictionary<string, Block>();
			foreach (Block item in BWSceneManager.AllBlocks())
			{
				if (item.isTerrain)
				{
					terrainBlocks[item.BlockType()] = item;
				}
			}
		}
		if (terrainBlocks.TryGetValue(name, out var value))
		{
			return value;
		}
		BWLog.Info("Could not find terrain block '" + name + "'");
		BWLog.Info("Choose from one of the following:");
		foreach (string key in terrainBlocks.Keys)
		{
			BWLog.Info(key);
		}
		terrainBlocks[name] = null;
		return null;
	}

	public TileResultCode PaintTerrainTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string name = (string)args[0];
		string paint = (string)args[1];
		return GetTerrainBlock(name)?.PaintTo(paint, permanent: false) ?? TileResultCode.True;
	}

	public TileResultCode TextureTerrainTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string name = (string)args[0];
		string texture = (string)args[1];
		return GetTerrainBlock(name)?.TextureTo(texture, Vector3.up, permanent: false) ?? TileResultCode.True;
	}

	public TileResultCode IsEnvEffect(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockSky worldSky = Blocksworld.worldSky;
		if (worldSky != null)
		{
			if (worldSky.GetPaint() == (string)args[0])
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetEnvEffect(ScriptRowExecutionInfo eInfo, object[] args)
	{
		BlockSky worldSky = Blocksworld.worldSky;
		if (worldSky == null)
		{
			return TileResultCode.True;
		}
		string stringArg = Util.GetStringArg(args, 0, "Clear");
		float num = eInfo.floatArg * Util.GetFloatArg(args, 1, 1f);
		float num2 = Mathf.Max(0.1f, Util.GetFloatArg(args, 2, 1f));
		if (eInfo.timer == 0f)
		{
			if (toEnvEffect != null)
			{
				return TileResultCode.True;
			}
			fromEnvEffect = BlockSky.GetEnvEffectName(Blocksworld.weather);
			toEnvEffect = stringArg;
			if (fromEnvEffect == "Clear" && toEnvEffect != "Clear")
			{
				worldSky.UpdateWeather(BlockSky.GetWeatherEffectByName(stringArg), permanent: false);
			}
			fromEnvEffectIntensity = Blocksworld.weather.IntensityMultiplier;
		}
		if (fromEnvEffect == toEnvEffect)
		{
			if (eInfo.timer >= num2)
			{
				Blocksworld.weather.IntensityMultiplier = num;
				toEnvEffect = null;
				fromEnvEffect = null;
				return TileResultCode.True;
			}
			float num3 = eInfo.timer / num2;
			Blocksworld.weather.IntensityMultiplier = num3 * num + (1f - num3) * fromEnvEffectIntensity;
			return TileResultCode.Delayed;
		}
		if (eInfo.timer >= num2)
		{
			if (toEnvEffect == "Clear")
			{
				worldSky.UpdateWeather(BlockSky.GetWeatherEffectByName(toEnvEffect), permanent: false);
			}
			Blocksworld.weather.IntensityMultiplier = num;
			toEnvEffect = null;
			fromEnvEffect = null;
			return TileResultCode.True;
		}
		if (fromEnvEffect == "Clear")
		{
			Blocksworld.weather.IntensityMultiplier = num * (eInfo.timer / num2);
		}
		else if (toEnvEffect == "Clear")
		{
			Blocksworld.weather.IntensityMultiplier = fromEnvEffectIntensity * (1f - eInfo.timer / num2);
		}
		else if (eInfo.timer < num2 * 0.5f)
		{
			Blocksworld.weather.IntensityMultiplier = fromEnvEffectIntensity * (1f - eInfo.timer / (num2 * 0.5f));
		}
		else
		{
			if (Blocksworld.weather != BlockSky.GetWeatherEffectByName(stringArg))
			{
				worldSky.UpdateWeather(BlockSky.GetWeatherEffectByName(stringArg), permanent: false);
			}
			Blocksworld.weather.IntensityMultiplier = num * (eInfo.timer - num2 * 0.5f) / (num2 * 0.5f);
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode SetGravity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		Vector3 field = eInfo.floatArg * 9.82f * Util.GetVector3Arg(args, 0, -Vector3.up);
		ApplyGravity(field);
		return TileResultCode.True;
	}

	public TileResultCode SetWorldGravity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (intArg < gravDefs.definitions.Length)
		{
			ApplyGravity(gravDefs.definitions[intArg].amount);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetAtmosphere(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		if (intArg < atmosDefs.definitions.Length)
		{
			ApplyDrag(atmosDefs.definitions[intArg].drag);
			ApplyAngularDrag(atmosDefs.definitions[intArg].angularDrag);
		}
		return TileResultCode.True;
	}

	public static void SetupGravityAndAtmosphereDefinitions()
	{
		gravDefs = Resources.Load<GravityDefinitions>("GravityDefinitions");
		atmosDefs = Resources.Load<AtmosphereDefinitions>("AtmosphereDefinitions");
	}

	private void ApplyGravity(Vector3 field)
	{
		Physics.gravity = field;
		if (informedBlocksAboutVaryingGravity)
		{
			return;
		}
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			item.SetVaryingGravity(vg: true);
		}
		informedBlocksAboutVaryingGravity = true;
	}

	public TileResultCode SetDragMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float dragMult = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		ApplyDrag(dragMult);
		return TileResultCode.True;
	}

	private void ApplyDrag(float dragMult)
	{
		Blocksworld.dragMultiplier = dragMult;
		Blocksworld.UpdateDrag();
	}

	public TileResultCode SetAngularDragMultiplier(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float angDragMult = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		ApplyAngularDrag(angDragMult);
		return TileResultCode.True;
	}

	private void ApplyAngularDrag(float angDragMult)
	{
		Blocksworld.angularDragMultiplier = angDragMult;
		Blocksworld.UpdateAngularDrag();
	}

	public TileResultCode IsFogColor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, "White");
		return boolToTileResult(_fogPaint == stringArg);
	}

	public TileResultCode SetFogColor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_fogPaint = Util.GetStringArg(args, 1, "White");
		WorldEnvironmentManager.OverrideFogPaintTemporarily(_fogPaint);
		Blocksworld.worldSky.SetFogColor(_fogPaint);
		return TileResultCode.True;
	}

	public TileResultCode IsFogDensity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 10f);
		return boolToTileResult(Util.FloatsClose(floatArg, _fogDensity));
	}

	public TileResultCode SetFogDensity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_fogDensity = Util.GetFloatArg(args, 0, 10f);
		WorldEnvironmentManager.OverrideFogDensityTemporarily(_fogDensity);
		UpdateFog();
		return TileResultCode.True;
	}

	public TileResultCode IsFogStart(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float f = Mathf.Max(0.1f, Util.GetFloatArg(args, 0, 100f));
		return boolToTileResult(Util.FloatsClose(f, _fogStart));
	}

	public TileResultCode SetFogStart(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_fogStart = Mathf.Max(0.1f, Util.GetFloatArg(args, 0, 100f));
		WorldEnvironmentManager.OverrideFogStartTemporarily(_fogStart);
		UpdateFog();
		return TileResultCode.True;
	}

	private void UpdateFog()
	{
		float end = WorldEnvironmentManager.ComputeFogEnd(_fogStart, _fogDensity);
		Blocksworld.bw.SetFog(_fogStart, end);
	}

	internal static RigidbodyConstraints GetGlobalRigidbodyConstraints()
	{
		return masterRBConstraints;
	}

	public TileResultCode IsConstrainTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		return tileResultFromRBConstraintInclusion((int)masterRBConstraints, rBConstraintsArgAsInt);
	}

	public TileResultCode ConstrainTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		rBConstraintsArgAsInt = (int)masterRBConstraints | rBConstraintsArgAsInt;
		masterRBConstraintsUpdated |= rBConstraintsArgAsInt != (int)masterRBConstraints;
		masterRBConstraints = (RigidbodyConstraints)rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsFreeTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		return tileResultFromRBConstraintExclusion((int)masterRBConstraints, rBConstraintsArgAsInt);
	}

	public TileResultCode FreeTranslationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: true);
		rBConstraintsArgAsInt = (int)masterRBConstraints & ~rBConstraintsArgAsInt;
		masterRBConstraintsUpdated |= rBConstraintsArgAsInt != (int)masterRBConstraints;
		masterRBConstraints = (RigidbodyConstraints)rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsConstrainRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		return tileResultFromRBConstraintInclusion((int)masterRBConstraints, rBConstraintsArgAsInt);
	}

	public TileResultCode ConstrainRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		rBConstraintsArgAsInt = (int)masterRBConstraints | rBConstraintsArgAsInt;
		masterRBConstraintsUpdated |= rBConstraintsArgAsInt != (int)masterRBConstraints;
		masterRBConstraints = (RigidbodyConstraints)rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsFreeRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		return tileResultFromRBConstraintExclusion((int)masterRBConstraints, rBConstraintsArgAsInt);
	}

	public TileResultCode FreeRotationMaster(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int rBConstraintsArgAsInt = GetRBConstraintsArgAsInt(args, 0, translation: false);
		rBConstraintsArgAsInt = (int)masterRBConstraints & ~rBConstraintsArgAsInt;
		masterRBConstraintsUpdated |= rBConstraintsArgAsInt != (int)masterRBConstraints;
		masterRBConstraints = (RigidbodyConstraints)rBConstraintsArgAsInt;
		return TileResultCode.True;
	}

	public TileResultCode IsSkyRotatedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0f);
		return boolToTileResult(Util.FloatsClose(floatArg, Blocksworld.worldSky.GetSkyBoxRotation()));
	}

	public TileResultCode SkyRotateToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float angle = Util.GetFloatArg(args, 0, 0f) * eInfo.floatArg;
		Blocksworld.worldSky.SetSkyBoxRotation(angle, immediate: false);
		return TileResultCode.True;
	}

	public TileResultCode IsSunIntensity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float f = Util.GetFloatArg(args, 0, 1f) * 0.01f;
		return boolToTileResult(Util.FloatsClose(f, Blocksworld.worldSky.GetSunIntensity()));
	}

	public TileResultCode SetSunIntensity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float sunIntensity = Util.GetFloatArg(args, 0, 1f) * eInfo.floatArg * 0.01f;
		Blocksworld.worldSky.SetSunIntensity(sunIntensity);
		return TileResultCode.True;
	}

	public TileResultCode IsSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return Blocksworld.worldSky.IsSkyBox(eInfo, args);
	}

	public TileResultCode SetSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return Blocksworld.worldSky.SetSkyBox(eInfo, args);
	}
}
