using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractWater : Block
{
	protected bool bubbleSoundWithin = true;

	protected bool playLoopAboveWater;

	protected GameObject bubblePsObject;

	public ParticleSystem bubblePs;

	protected GameObject bubbleSfxObject;

	protected AudioSource bubbleSfxAudio;

	public static HashSet<Block> blocksWithWaterSensors;

	protected Dictionary<int, WaterSplashInfo> splashInfos = new Dictionary<int, WaterSplashInfo>();

	protected List<WaterSplashInfo> playingSplashInfos = new List<WaterSplashInfo>();

	protected static List<GameObject> splashAudioSources = new List<GameObject>();

	protected const float MAX_WATER_DENSITY = 100f;

	protected const float MAX_WATER_FRICTION = 100f;

	public const float DEFAULT_WATER_DENSITY = 0.7f;

	protected float waterDensity = 0.7f;

	protected const float LAVA_DENSITY = 0.9f;

	protected const float LAVA_FRICTION = 3f;

	public const float DEFAULT_WATER_FRICTION = 1f;

	protected float waterFriction = 1f;

	protected Vector3 streamVelocity;

	protected static bool cameraWithinOcean = false;

	public static List<BlockWaterCube> waterCubes = new List<BlockWaterCube>();

	protected bool hasSlowWaves;

	public bool isSolid;

	protected bool isLava;

	protected bool maxPositiveWaterLevelOffsetSet;

	protected bool maxNegativeWaterLevelOffsetSet;

	protected float maxPositiveWaterLevelOffset = 3f;

	protected float maxNegativeWaterLevelOffset = 3f;

	protected float waterLevelOffset;

	protected AudioClip withinClip;

	protected AudioClip aboveClip;

	private const string LAVA = "Lava";

	private const string JELLO = "Jello";

	private const string MUD = "Mud";

	private const string WATER = "Water";

	private const string INTANGIBLE = "Intangible";

	private const string QUICKSAND = "Quicksand";

	public bool cameraWasWithinWater;

	protected Color lightColor = Color.white;

	protected Color emissiveLightColor = Block.transparent;

	public BlockAbstractWater(List<List<Tile>> tiles)
		: base(tiles)
	{
		bubblePsObject = Object.Instantiate(Resources.Load("Blocks/BlockWater Particle System")) as GameObject;
		bubblePs = bubblePsObject.GetComponent<ParticleSystem>();
		bubblePs.enableEmission = false;
		bubbleSfxObject = new GameObject(go.name + " bubbles");
		bubbleSfxAudio = bubbleSfxObject.AddComponent<AudioSource>();
		bubbleSfxAudio.dopplerLevel = 0f;
		bubbleSfxAudio.playOnAwake = false;
		bubbleSfxAudio.volume = 0f;
		bubbleSfxAudio.loop = true;
	}

	public override void Play()
	{
		base.Play();
		maxPositiveWaterLevelOffset = 3f;
		maxNegativeWaterLevelOffset = 3f;
		maxPositiveWaterLevelOffsetSet = false;
		maxNegativeWaterLevelOffsetSet = false;
		waterLevelOffset = 0f;
		SetLiquidProperties((!isLava) ? "Water" : "Lava");
		GetBubbleClip();
		withinClip = bubbleSfxAudio.clip;
		aboveClip = null;
		playLoopAboveWater = false;
		string texture = GetTexture();
		if (texture == "Texture Water Stream")
		{
			aboveClip = Sound.GetSfx("Ocean Loop");
			playLoopAboveWater = true;
		}
	}

	protected static void GatherAllBlocksWithWaterSensors()
	{
		if (blocksWithWaterSensors != null)
		{
			return;
		}
		HashSet<Predicate> fewPreds = new HashSet<Predicate>
		{
			Block.predicateWithinWater,
			Block.predicateModelWithinWater,
			Block.predicateWithinTaggedWater,
			Block.predicateModelWithinTaggedWater
		};
		HashSet<Predicate> fewPreds2 = new HashSet<Predicate>
		{
			Block.predicateModelWithinWater,
			Block.predicateModelWithinTaggedWater
		};
		blocksWithWaterSensors = new HashSet<Block>();
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			if (item.ContainsTileWithAnyPredicateInPlayMode(fewPreds))
			{
				blocksWithWaterSensors.Add(item);
				if (item.ContainsTileWithAnyPredicateInPlayMode(fewPreds2))
				{
					item.UpdateConnectedCache();
					List<Block> other = Block.connectedCache[item];
					blocksWithWaterSensors.UnionWith(other);
				}
			}
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		waterLevelOffset = 0f;
		IncreaseWaterLevel(0f);
		blocksWithWaterSensors = null;
	}

	public static bool CameraWithinAnyWater()
	{
		bool flag = cameraWithinOcean;
		if (!flag)
		{
			for (int i = 0; i < waterCubes.Count; i++)
			{
				BlockWaterCube blockWaterCube = waterCubes[i];
				if (blockWaterCube.cameraWasWithinWater)
				{
					return true;
				}
			}
		}
		return flag;
	}

	protected void GetBubbleClip()
	{
		string name = "Bubbles Loop";
		string texture = GetTexture();
		if (texture == "Texture Lava")
		{
			name = "Lava Bubbles Loop";
		}
		bubbleSfxAudio.clip = Sound.GetSfx(name);
	}

	public override void Destroy()
	{
		Object.Destroy(bubblePsObject);
		Object.Destroy(bubbleSfxObject);
		base.Destroy();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		GetBubbleClip();
	}

	public virtual Bounds GetWaterBounds()
	{
		return go.GetComponent<Collider>().bounds;
	}

	public TileResultCode SetLiquidProperties(ScriptRowExecutionInfo eInfo, object[] args)
	{
		SetLiquidProperties(Util.GetStringArg(args, 0, "Water"));
		return TileResultCode.True;
	}

	public void SetLiquidProperties(string type)
	{
		float density = 0.7f;
		float friction = 1f;
		switch (type)
		{
		case "Quicksand":
			density = 0.25f;
			friction = 20f;
			break;
		case "Mud":
			density = 1f;
			friction = 6f;
			break;
		case "Intangible":
			density = 0f;
			friction = 0f;
			break;
		case "Jello":
			density = 20f;
			friction = 3f;
			break;
		case "Lava":
			density = 0.9f;
			friction = 3f;
			break;
		}
		SetDensity(density);
		SetFriction(friction);
	}

	public TileResultCode SetMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		SetMaxPositiveWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
		return TileResultCode.True;
	}

	public TileResultCode SetMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		SetMaxNegativeWaterLevelOffset(eInfo.floatArg * Util.GetFloatArg(args, 0, 10f));
		return TileResultCode.True;
	}

	public TileResultCode AtMaxPositiveWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (AtMaxPositiveWaterLevelOffset())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode AtMaxNegativeWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (AtMaxNegativeWaterLevelOffset())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public bool AtMaxPositiveWaterLevelOffset()
	{
		if (maxPositiveWaterLevelOffsetSet)
		{
			return Mathf.Abs(waterLevelOffset - maxPositiveWaterLevelOffset) < 0.05f;
		}
		return false;
	}

	public bool AtMaxNegativeWaterLevelOffset()
	{
		if (maxNegativeWaterLevelOffsetSet)
		{
			return Mathf.Abs(waterLevelOffset + maxNegativeWaterLevelOffset) < 0.05f;
		}
		return false;
	}

	public void SetMaxPositiveWaterLevelOffset(float offset)
	{
		maxPositiveWaterLevelOffsetSet = true;
		maxPositiveWaterLevelOffset = offset;
		if (waterLevelOffset > maxPositiveWaterLevelOffset)
		{
			SetWaterLevelOffset(maxPositiveWaterLevelOffset);
		}
	}

	public void SetMaxNegativeWaterLevelOffset(float offset)
	{
		maxNegativeWaterLevelOffsetSet = true;
		maxNegativeWaterLevelOffset = offset;
		if (waterLevelOffset < 0f - maxNegativeWaterLevelOffset)
		{
			SetWaterLevelOffset(0f - maxNegativeWaterLevelOffset);
		}
	}

	public void IncreaseWaterLevel(float inc)
	{
		if (!isSolid)
		{
			float num = waterLevelOffset + inc;
			SetWaterLevelOffset(num);
		}
	}

	protected void UpdateSounds(bool within)
	{
		if (Sound.sfxEnabled && Blocksworld.CurrentState == State.Play)
		{
			float num = 0.1f;
			if (!within && !playLoopAboveWater)
			{
				num *= -1f;
			}
			float max = 1f;
			if (!within && playLoopAboveWater && aboveClip != null && bubbleSfxAudio.clip != aboveClip)
			{
				bubbleSfxAudio.clip = aboveClip;
			}
			else if (within && withinClip != null && bubbleSfxAudio.clip != withinClip)
			{
				bubbleSfxAudio.clip = withinClip;
			}
			if (!within && playLoopAboveWater)
			{
				float num2 = Mathf.Abs(Blocksworld.cameraPosition.y - GetWaterBounds().max.y);
				max = 0.1f / (0.1f + num2 * 0.02f);
			}
			float volume = bubbleSfxAudio.volume;
			volume = Mathf.Clamp(volume + num, 0f, max);
			bubbleSfxAudio.volume = volume;
			if (volume > 0.01f && !bubbleSfxAudio.isPlaying && num > 0f)
			{
				bubbleSfxAudio.Play();
			}
			else if (volume < 0.01f && num < 0f && bubbleSfxAudio.isPlaying)
			{
				bubbleSfxAudio.Stop();
			}
		}
		else
		{
			bubbleSfxAudio.volume = 0f;
			if (bubbleSfxAudio.isPlaying)
			{
				bubbleSfxAudio.Stop();
			}
		}
	}

	public override void Pause()
	{
		bubblePs.Pause();
	}

	public override void Resume()
	{
		bubblePs.Play();
	}

	protected void FindSplashAudioSources()
	{
		playingSplashInfos.Clear();
		for (int i = splashAudioSources.Count; i < 3; i++)
		{
			GameObject gameObject = new GameObject();
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			Sound.SetWorldAudioSourceParams(audioSource);
			splashAudioSources.Add(gameObject);
		}
		for (int j = 0; j < 3; j++)
		{
			AudioSource component = splashAudioSources[j].GetComponent<AudioSource>();
			component.clip = Sound.GetSfx("Water Splash Medium " + Random.Range(1, 4));
		}
	}

	protected void UpdateUnderwaterLightColors(bool w)
	{
		if (Blocksworld.renderingWater)
		{
			return;
		}
		if (w)
		{
			Color color = (lightColor = go.GetComponent<Renderer>().sharedMaterial.GetColor("_Color"));
			if (Blocksworld.IsLuminousPaint(GetPaint()))
			{
				emissiveLightColor = new Color(color.r, color.g, color.b, 0f);
				lightColor = Color.white;
			}
			else if (GetTexture() == "Texture Lava")
			{
				lightColor = Color.white;
			}
			else
			{
				emissiveLightColor = Block.transparent;
			}
		}
		else
		{
			lightColor = Color.white;
			emissiveLightColor = Block.transparent;
		}
		Blocksworld.UpdateLightColor();
	}

	public override Color GetEmissiveLightTint()
	{
		return emissiveLightColor;
	}

	public override Color GetLightTint()
	{
		return lightColor;
	}

	public void SetDensity(float dens)
	{
		waterDensity = Mathf.Clamp(dens, 0f, 100f);
	}

	public void SetFriction(float frict)
	{
		waterFriction = Mathf.Clamp(frict, 0f, 100f);
	}

	public TileResultCode SetDensity(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float density = eInfo.floatArg * Util.GetFloatArg(args, 0, 0.7f);
		SetDensity(density);
		return TileResultCode.True;
	}

	public new TileResultCode SetFriction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float friction = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		SetFriction(friction);
		return TileResultCode.True;
	}

	public float WaterLevelOffset(Vector3 pos)
	{
		if (hasSlowWaves)
		{
			return 0.25f + 0.2f * Mathf.Sin(0.1f * pos.x + Time.time);
		}
		return 0f;
	}

	protected virtual void SetWaterLevelOffset(float newOffset)
	{
		waterLevelOffset = ClampWaterLevelOffset(newOffset);
	}

	protected float ClampWaterLevelOffset(float offset)
	{
		if (offset > 0f && maxPositiveWaterLevelOffsetSet)
		{
			offset = Mathf.Min(offset, maxPositiveWaterLevelOffset);
		}
		if (offset < 0f && maxNegativeWaterLevelOffsetSet)
		{
			offset = Mathf.Max(offset, 0f - maxNegativeWaterLevelOffset);
		}
		return offset;
	}

	public override TileResultCode Freeze(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}
}
