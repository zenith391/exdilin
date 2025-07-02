using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractRocket : Block
{
	private GameObject flame;

	private GameObject fireFlame;

	private ParticleSystem particles;

	private ParticleSystem flameParticles;

	private float mass;

	private bool isFiring;

	private bool isSmoking;

	private bool isFlaming;

	private bool playBurst;

	private float playBurstLevel;

	protected float smokeForce;

	private float fireForce;

	private int treatAsVehicleStatus = -1;

	private int sfxLoopUpdateCounter;

	private const int SFX_LOOP_UPDATE_INTERVAL = 5;

	private RocketMetaData rocketMeta;

	protected Color smokeColor = Color.white;

	protected bool setSmokeColor;

	protected int smokeColorMeshIndex;

	protected bool emitSmoke = true;

	private Vector3 exitOffset = Vector3.zero;

	private float smokeSizeMultiplier = 1f;

	private float smokeSpeedMultiplier = 1f;

	private float particlesPerSecond = 50f;

	public BlockAbstractRocket(List<List<Tile>> tiles, string smokePrefab = "Blocks/Rocket Flame", string flamePrefab = "")
		: base(tiles)
	{
		flame = Object.Instantiate(Resources.Load(smokePrefab)) as GameObject;
		if (!string.IsNullOrEmpty(flamePrefab))
		{
			fireFlame = Object.Instantiate(Resources.Load(flamePrefab)) as GameObject;
			flameParticles = fireFlame.GetComponent<ParticleSystem>();
			flameParticles.enableEmission = false;
			fireFlame.SetActive(value: false);
		}
		particles = flame.GetComponent<ParticleSystem>();
		particles.enableEmission = false;
		flame.SetActive(value: false);
		loopName = "Rocket Burst Loop";
		sfxLoopUpdateCounter = Random.Range(0, 5);
	}

	public override void Destroy()
	{
		Object.Destroy(flame);
		if (fireFlame != null)
		{
			Object.Destroy(fireFlame);
		}
		base.Destroy();
	}

	public override void Play()
	{
		base.Play();
		treatAsVehicleStatus = -1;
		flame.SetActive(value: true);
		if (fireFlame != null)
		{
			fireFlame.SetActive(value: true);
		}
		smokeForce = 0f;
		fireForce = 0f;
		rocketMeta = go.GetComponent<RocketMetaData>();
		if (rocketMeta != null)
		{
			exitOffset = rocketMeta.exitOffset;
			exitOffset.Scale(Scale());
			loopName = rocketMeta.loopSfx;
			smokeSizeMultiplier = rocketMeta.smokeSizeMultiplier;
			smokeSpeedMultiplier = rocketMeta.smokeSpeedMultiplier;
			particlesPerSecond = rocketMeta.particlesPerSecond;
		}
		else
		{
			BWLog.Info("Could not find rocket meta data component");
		}
	}

	public override void Play2()
	{
		mass = Bunch.GetModelMass(this);
		mass = Mathf.Min(10f, mass);
		Vector3 vector = Scale();
		if (vector != Vector3.one)
		{
			mass *= 1f + 0.1f * vector.x * vector.y * vector.z;
		}
		if (setSmokeColor)
		{
			UpdateSmokeColorPaint(GetPaint(smokeColorMeshIndex), smokeColorMeshIndex);
			UpdateSmokeColorTexture(GetTexture(smokeColorMeshIndex), smokeColorMeshIndex);
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		flame.SetActive(value: false);
		if (fireFlame != null)
		{
			fireFlame.SetActive(value: false);
		}
		base.Stop(resetBlock);
		playBurst = false;
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Pause()
	{
		particles.Pause();
		playBurst = false;
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Resume()
	{
		particles.Play();
	}

	public override void ResetFrame()
	{
		isFiring = false;
		isSmoking = false;
		isFlaming = false;
	}

	public TileResultCode IsFiring(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isFiring)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsSmoking(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isSmoking)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsFlaming(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isFlaming)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode FireRocket(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		smokeForce += num;
		num *= 12f;
		fireForce += num;
		isFiring = true;
		playBurst = true;
		return TileResultCode.True;
	}

	public TileResultCode Smoke(ScriptRowExecutionInfo eInfo, object[] args)
	{
		smokeForce += eInfo.floatArg;
		isSmoking = true;
		return TileResultCode.True;
	}

	public TileResultCode Flame(ScriptRowExecutionInfo eInfo, object[] args)
	{
		smokeForce += eInfo.floatArg;
		isFlaming = true;
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		Transform transform = goT;
		Vector3 position = transform.position + transform.TransformDirection(exitOffset);
		playBurst = isFiring;
		float num = Mathf.Min(size.x, size.z) * smokeSizeMultiplier;
		float num2 = Mathf.Sqrt(num);
		if (Sound.sfxEnabled && !vanished && go.activeInHierarchy)
		{
			float num3 = ((!playBurst) ? (-0.04f) : 0.04f);
			playBurstLevel = Mathf.Clamp(playBurstLevel + num3, 0f, Mathf.Clamp(0.5f * smokeForce, 0.1f, 1f));
			float value = (0.98f + Mathf.Min(0.1f, 0.02f * smokeForce)) / (0.5f + 0.5f * num2);
			value = Mathf.Clamp(value, 0.25f, 1.25f);
			if (sfxLoopUpdateCounter % 5 == 0)
			{
				PlayLoopSound(playBurst || playBurstLevel > 0.01f, GetLoopClip(), playBurstLevel, null, value);
				UpdateWithinWaterLPFilter();
			}
			sfxLoopUpdateCounter++;
		}
		else
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
		if (fireForce > 0f)
		{
			Transform parent = transform.parent;
			if (parent != null && parent.gameObject != null)
			{
				Rigidbody component = parent.gameObject.GetComponent<Rigidbody>();
				if (component != null && !component.isKinematic)
				{
					Vector3 vector = fireForce * transform.up;
					Vector3 force = mass * vector;
					component.AddForceAtPosition(force, transform.position);
					Vector3 vector2 = 2f * vector;
					float magnitude = vector2.magnitude;
					if (magnitude > 0.1f)
					{
						vector2 = vector2 / magnitude * Mathf.Clamp(magnitude, -60f, 60f);
						vector2 = Util.ProjectOntoPlane(vector2, Vector3.up);
						Blocksworld.blocksworldCamera.AddForceDirectionHint(chunk, vector2);
						BlockAccelerations.BlockAccelerates(this, vector);
					}
				}
			}
			fireForce = 0f;
		}
		if (smokeForce > 0f && emitSmoke && !vanished)
		{
			smokeForce = Mathf.Min(2.5f, smokeForce);
			float num4 = smokeForce * particlesPerSecond;
			float num5 = num4 * Blocksworld.fixedDeltaTime;
			int i = 0;
			Vector3 vector3 = Vector3.zero;
			if (chunk.go != null)
			{
				Rigidbody rb = chunk.rb;
				if (rb != null && !rb.isKinematic)
				{
					vector3 = rb.velocity;
				}
			}
			float num6 = smokeForce * smokeSpeedMultiplier;
			for (; i < 6; i++)
			{
				if (!(num5 > 0f))
				{
					break;
				}
				if (!(num5 >= 1f) && !(Random.value < num5))
				{
					break;
				}
				float num7 = num * (0.8f * Random.value + 0.5f);
				Vector3 velocity = (vector3 - transform.up * num * num6 * (9f + 2f * Random.value)) * Random.Range(0.75f, 1f);
				float num8 = 0.5f;
				ParticleSystem.Particle particle = new ParticleSystem.Particle
				{
					position = position,
					velocity = velocity,
					size = num7,
					remainingLifetime = num8,
					startLifetime = num8,
					rotation = Random.Range(-180, 180),
					color = smokeColor,
					randomSeed = (uint)Random.Range(17, 9999999)
				};
				if (isFlaming && flameParticles != null)
				{
					flameParticles.Emit(particle);
				}
				else
				{
					particles.Emit(particle);
				}
				num5 -= 1f;
			}
		}
		smokeForce = 0f;
	}

	private void UpdateSmokeColorTexture(string texture, int meshIndex)
	{
		if (setSmokeColor && meshIndex == smokeColorMeshIndex && subMeshGameObjects.Count > smokeColorMeshIndex)
		{
			emitSmoke = texture != "Glass";
		}
	}

	public static Color GetSmokeColor(string paint, Renderer renderer)
	{
		Color color = Color.white;
		if (paint == null || !(paint == "White"))
		{
			color = renderer.sharedMaterial.color;
		}
		if (Blocksworld.IsLuminousPaint(paint))
		{
			color += color;
		}
		return color;
	}

	private void UpdateSmokeColorPaint(string paint, int meshIndex)
	{
		if (setSmokeColor && meshIndex == smokeColorMeshIndex && subMeshGameObjects.Count > smokeColorMeshIndex)
		{
			Renderer renderer = ((meshIndex != 0) ? subMeshGameObjects[meshIndex - 1].GetComponent<Renderer>() : go.GetComponent<Renderer>());
			smokeColor = GetSmokeColor(paint, renderer);
		}
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
		UpdateSmokeColorTexture(texture, meshIndex);
		return result;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		UpdateSmokeColorPaint(paint, meshIndex);
		return result;
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}
}
