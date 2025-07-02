using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockVolcano : BlockTerrain
{
	private GameObject particleGo;

	private ParticleSystem smokePs;

	private ParticleSystem firePs;

	private float eruptIntensity;

	private float cumulativeEruptIntensity;

	private float pitchRnd;

	private float smokeToEmit;

	private float fireToEmit;

	private float sizeMultiplier = 1f;

	private VolcanoSettings settings;

	private Quaternion origPsRotation;

	private static Dictionary<BlockVolcano, float> particleCounts = new Dictionary<BlockVolcano, float>();

	private const int MAX_PARTICLES = 200;

	private int updateLoopCounter;

	private float eruptPitch = 1f;

	private float eruptVolume;

	private float targetEruptVolume;

	public BlockVolcano(List<List<Tile>> tiles)
		: base(tiles)
	{
		particleGo = UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Volcano Particle System")) as GameObject;
		smokePs = particleGo.transform.Find("Smoke").gameObject.GetComponent<ParticleSystem>();
		firePs = particleGo.transform.Find("Fire").gameObject.GetComponent<ParticleSystem>();
		origPsRotation = particleGo.transform.rotation;
		smokePs.enableEmission = false;
		firePs.enableEmission = false;
		SetFog(Blocksworld.fogStart, Blocksworld.fogEnd);
		UpdateFogColor(Blocksworld.fogColor);
		settings = go.GetComponent<VolcanoSettings>();
		eruptIntensity = 0f;
		cumulativeEruptIntensity = 0f;
		UpdateParticlePosition();
		loopName = "Rocket Burst 2 Loop";
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockVolcano>("Volcano.Erupt", null, (Block b) => ((BlockVolcano)b).Erupt, new Type[1] { typeof(float) }, new string[1] { "Intensity" });
		Block.AddSimpleDefaultTiles(new GAF("Volcano.Erupt", 2f), "Volcano");
	}

	public override void Play()
	{
		base.Play();
		eruptPitch = 1f;
		eruptIntensity = 0f;
		cumulativeEruptIntensity = 0f;
		updateLoopCounter = UnityEngine.Random.Range(1, 10);
		pitchRnd = UnityEngine.Random.Range(0f, 1000f);
		particleCounts.Clear();
		eruptVolume = 0f;
		targetEruptVolume = 0f;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		eruptIntensity = 0f;
		cumulativeEruptIntensity = 0f;
		smokePs.Clear();
		firePs.Clear();
		particleCounts.Clear();
		PlayLoopSound(play: false, GetLoopClip());
	}

	public TileResultCode Erupt(ScriptRowExecutionInfo eInfo, object[] args)
	{
		cumulativeEruptIntensity += eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
		return TileResultCode.True;
	}

	public override void SetFog(float start, float end)
	{
		base.SetFog(start, end);
		smokePs.GetComponent<Renderer>().material.SetFloat("_FogStart", start);
		smokePs.GetComponent<Renderer>().material.SetFloat("_FogEnd", end);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		eruptIntensity = cumulativeEruptIntensity;
		updateLoopCounter++;
		bool flag = eruptIntensity > 0.01f && Sound.sfxEnabled && !vanished;
		if (updateLoopCounter % 5 == 0)
		{
			if (flag)
			{
				targetEruptVolume = Mathf.Min(eruptIntensity * 0.5f, sizeMultiplier * 0.05f);
				float num = 0.1f;
				eruptVolume = Mathf.Clamp(eruptVolume + Mathf.Clamp(targetEruptVolume - eruptVolume, 0f - num, num), 0f, 1f);
				float num2 = Mathf.Max(3f - sizeMultiplier * 0.3f, 0.5f);
				eruptPitch = num2 + 0.1f * SimplexNoise.Noise(0.25f * Time.time, pitchRnd);
			}
			else
			{
				targetEruptVolume = 0f;
			}
		}
		if (flag)
		{
			PlaySound("Rocket Burst 2 Loop", "Block", "Loop", eruptVolume, eruptPitch);
		}
		else
		{
			eruptVolume = 0f;
		}
		cumulativeEruptIntensity = 0f;
	}

	public override void UpdateFogColor(Color newFogColor)
	{
		base.UpdateFogColor(newFogColor);
		smokePs.GetComponent<Renderer>().material.SetColor("_FogColor", newFogColor);
	}

	private void UpdateParticlePosition()
	{
		if (particleGo != null)
		{
			Vector3 vector = Scale();
			Vector3 relativeOffset = settings.relativeOffset;
			Vector3 direction = new Vector3(vector.x * relativeOffset.x, vector.y * relativeOffset.y, vector.z * relativeOffset.z);
			Vector3 vector2 = goT.TransformDirection(direction);
			Vector3 position = GetPosition();
			particleGo.transform.position = position + vector2;
			particleGo.transform.rotation = goT.rotation * origPsRotation;
			sizeMultiplier = Mathf.Abs((vector.x + vector.y + vector.z) / 3f);
		}
	}

	public override void Pause()
	{
		base.Pause();
		smokePs.Pause();
		firePs.Pause();
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Resume()
	{
		base.Resume();
		smokePs.Play();
		firePs.Play();
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState != State.Play || vanished)
		{
			return;
		}
		Transform transform = goT;
		Vector3 position = transform.position;
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		float num = Blocksworld.fogEnd * 1.2f + Util.MaxComponent(size);
		Vector3 vector = position - cameraPosition;
		float magnitude = vector.magnitude;
		Vector3 rhs = vector / magnitude;
		particleCounts[this] = smokePs.particleCount + firePs.particleCount;
		float num2 = 0f;
		foreach (KeyValuePair<BlockVolcano, float> particleCount in particleCounts)
		{
			num2 += particleCount.Value;
		}
		float num3 = Mathf.Clamp(1f - num2 / 200f, 0f, 1f);
		if (magnitude < num && (!(Vector3.Dot(Blocksworld.cameraForward, rhs) <= 0f) || !(magnitude > Util.MaxComponent(size) * 4f)))
		{
			smokeToEmit += num3 * eruptIntensity * settings.smokePerFrame;
			Vector3 position2 = particleGo.transform.position;
			int num4 = 0;
			for (int i = 0; (float)i < smokeToEmit; i++)
			{
				float num5 = sizeMultiplier * UnityEngine.Random.Range(settings.smokeSizeRandomFrom, settings.smokeSizeRandomTo) + settings.smokeSizeBias;
				float num6 = sizeMultiplier * (UnityEngine.Random.Range(0.06f, 0.11f) * eruptIntensity + 0.02f);
				float num7 = Mathf.Clamp(sizeMultiplier * UnityEngine.Random.Range(settings.smokeLifetimeRandomFrom, settings.smokeLifetimeRandomTo), 1f, 3f);
				Vector3 vector2 = transform.TransformDirection(Vector3.up) * num6;
				float num8 = 5f * eruptIntensity;
				vector2 = Quaternion.Euler(UnityEngine.Random.Range(0f - num8, num8), UnityEngine.Random.Range(0f - num8, num8), UnityEngine.Random.Range(0f - num8, num8)) * vector2;
				ParticleSystem.Particle particle = new ParticleSystem.Particle
				{
					position = position2,
					startLifetime = num7,
					remainingLifetime = num7,
					velocity = vector2,
					angularVelocity = UnityEngine.Random.Range(settings.smokeAngularVelocityRandomFrom, settings.smokeAngularVelocityRandomTo),
					size = num5,
					color = Color.white,
					rotation = UnityEngine.Random.Range(settings.smokeRotationRandomFrom, settings.smokeRotationRandomTo),
					randomSeed = (uint)UnityEngine.Random.Range(1, 999999)
				};
				smokePs.Emit(particle);
				num4++;
			}
			smokeToEmit -= num4;
			fireToEmit += num3 * eruptIntensity * settings.firePerFrame;
			int num9 = 0;
			for (int j = 0; (float)j < fireToEmit; j++)
			{
				firePs.startSize = sizeMultiplier * UnityEngine.Random.Range(settings.fireSizeRandomFrom, settings.fireSizeRandomTo);
				firePs.startSpeed = sizeMultiplier * (UnityEngine.Random.Range(0f, 0.3f) * eruptIntensity + 0.5f) + 2f;
				firePs.startLifetime = Mathf.Clamp(sizeMultiplier * UnityEngine.Random.Range(settings.fireLifetimeRandomFrom, settings.fireLifetimeRandomTo), 1.5f, 3f);
				firePs.Emit(1);
				num9++;
			}
			fireToEmit -= num9;
		}
	}

	public override bool MoveTo(Vector3 pos)
	{
		bool result = base.MoveTo(pos);
		UpdateParticlePosition();
		return result;
	}

	public override bool RotateTo(Quaternion rot)
	{
		bool result = base.RotateTo(rot);
		UpdateParticlePosition();
		return result;
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
		UpdateParticlePosition();
		return result;
	}

	public override void Destroy()
	{
		base.Destroy();
		if (particleGo != null)
		{
			UnityEngine.Object.Destroy(particleGo);
			particleGo = null;
		}
	}
}
