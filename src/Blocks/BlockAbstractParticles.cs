using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractParticles : Block
{
	private GameObject emitterGO;

	private bool isFiring;

	private bool gotSpread;

	private int meshIndexToColor;

	private float particleSpeedModifier = 1f;

	private float particleAngleModifier;

	private float particleSpreadModifier;

	private Vector3 baseSize = Vector3.zero;

	private Renderer storeRender;

	private bool hideEmitter;

	private bool shouldEmit = true;

	private ParticleSystem[] particles;

	private Color particleTint = Color.white;

	private List<float> particleEmitTimes = new List<float>();

	private static Dictionary<ParticleSystem, ParticleParameters> particleParameters = new Dictionary<ParticleSystem, ParticleParameters>();

	public BlockAbstractParticles(List<List<Tile>> tiles, string particlePathName = "Particles/Water Stream", bool shouldHide = false, int meshIndex = 0, string audioName = "815 Water Jet")
		: base(tiles)
	{
		meshIndexToColor = meshIndex;
		emitterGO = Object.Instantiate(Resources.Load(particlePathName)) as GameObject;
		particles = emitterGO.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].playOnAwake = false;
			ParticleParameters value = new ParticleParameters
			{
				storeSize = particles[i].startSize,
				storeSpeed = particles[i].startSpeed,
				storeLife = particles[i].startLifetime,
				storeEmission = particles[i].emissionRate,
				storeEmitTimes = particles[i].emissionRate / 60f,
				storeColor = particles[i].startColor,
				storeOriginalColor = particles[i].startColor,
				storeMaxParts = particles[i].maxParticles
			};
			particleParameters.Add(particles[i], value);
			particleEmitTimes.Add(0f);
		}
		if (go.GetComponent<Renderer>() != null)
		{
			string paint = GetPaint(meshIndexToColor);
			particleTint = GetParticleColor(paint);
		}
		hideEmitter = shouldHide;
		loopName = audioName;
		UpdateParticleColor();
		emitterGO.SetActive(value: false);
	}

	public override void Destroy()
	{
		Object.Destroy(emitterGO);
		base.Destroy();
	}

	public override void Play()
	{
		base.Play();
		for (int i = 0; i < connections.Count; i++)
		{
			connections[i].go.SetLayer(Layer.MeshEmitters, recursive: true);
		}
		go.SetLayer(Layer.MeshEmitters, recursive: true);
		storeRender = go.GetComponent<Renderer>();
		if (storeRender != null)
		{
			Bounds bounds = storeRender.bounds;
			Mesh mesh = go.GetComponent<MeshFilter>().mesh;
			if (mesh != null)
			{
				bounds = mesh.bounds;
			}
			baseSize = new Vector3(bounds.size.x * 0.5f, bounds.size.y * 0.5f, bounds.size.z * 0.5f);
		}
		isFiring = true;
		shouldEmit = true;
		ShowHideEmitter(hide: false);
		emitterGO.SetActive(value: true);
		UpdateEmitTimes();
	}

	public override void Stop(bool resetBlock = true)
	{
		for (int i = 0; i < connections.Count; i++)
		{
			connections[i].go.SetLayer(Layer.Default, recursive: true);
		}
		go.SetLayer(Layer.Default, recursive: true);
		base.Stop(resetBlock);
		for (int j = 0; j < particles.Length; j++)
		{
			particles[j].Stop();
			particles[j].Clear();
		}
		PlayLoopSound(play: false, GetLoopClip());
		ShowHideEmitter(hide: true);
		emitterGO.SetActive(value: false);
	}

	public bool HidingEmitter()
	{
		return hideEmitter;
	}

	private void ShowHideEmitter(bool hide)
	{
		if (!hideEmitter)
		{
			return;
		}
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = hide;
		}
		List<Collider> colliders = GetColliders();
		foreach (Collider item in colliders)
		{
			item.enabled = hide;
		}
	}

	public override void Pause()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Pause();
		}
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Resume()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Play();
		}
	}

	public override void ResetFrame()
	{
		isFiring = false;
		particleAngleModifier = 0f;
		if (gotSpread)
		{
			gotSpread = false;
		}
		else if (particleSpreadModifier != 0f)
		{
			particleSpreadModifier = 0f;
			UpdateEmitTimes();
		}
	}

	public TileResultCode IsFiring(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!BlockWater.BlockWithinWater(this))
		{
			float num = ((args.Length == 0) ? 1f : ((float)args[0]));
			float num2 = eInfo.floatArg * num;
			if (num2 != particleSpeedModifier)
			{
				particleSpeedModifier = num2;
				UpdateEmitTimes();
			}
			isFiring = true;
		}
		return TileResultCode.True;
	}

	public TileResultCode ParticleSpread(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 0f : ((float)args[0]));
		float num2 = eInfo.floatArg * num;
		gotSpread = true;
		if (num2 != particleSpreadModifier)
		{
			particleSpreadModifier = num2;
			UpdateEmitTimes();
		}
		return TileResultCode.True;
	}

	public TileResultCode ParticleAngle(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 1f : ((float)args[0]));
		float num2 = eInfo.floatArg * num;
		if (num2 != particleAngleModifier)
		{
			particleAngleModifier = num2;
		}
		return TileResultCode.True;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		paint = paint.Replace("Luminous ", string.Empty);
		particleTint = GetParticleColor(paint);
		if (meshIndex == meshIndexToColor && particles != null)
		{
			UpdateParticleColor();
		}
		if (hideEmitter && Blocksworld.CurrentState == State.Play)
		{
			ShowHideEmitter(hide: false);
		}
		return result;
	}

	public override TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		shouldEmit = false;
		return base.ExplodeLocal(eInfo, args);
	}

	public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		shouldEmit = false;
		return base.Explode(eInfo, args);
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (hideEmitter)
		{
			if (Blocksworld.CurrentState == State.Play)
			{
				ShaderType value = ShaderType.Normal;
				if (!Materials.shaders.TryGetValue(texture, out value))
				{
					ResourceLoader.LoadTexture(texture);
				}
				string paint = GetPaint();
				Material material = Materials.GetMaterial(paint, texture, value);
				if (material != null)
				{
					go.GetComponent<Renderer>().sharedMaterial = material;
				}
				return TileResultCode.True;
			}
			texture = "Volume";
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force);
	}

	public override TileResultCode AppearModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		shouldEmit = true;
		if (hideEmitter)
		{
			return TileResultCode.True;
		}
		return base.AppearModel(eInfo, args);
	}

	public override TileResultCode VanishModel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		shouldEmit = false;
		if (hideEmitter)
		{
			return TileResultCode.True;
		}
		return base.VanishModel(eInfo, args);
	}

	private void UpdateEmitTimes()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			if (BlockAbstractParticles.particleParameters.ContainsKey(particles[i]))
			{
				ParticleParameters particleParameters = BlockAbstractParticles.particleParameters[particles[i]];
				float num = Mathf.Clamp(particleSpeedModifier, 1f, 100f);
				float num2 = Mathf.Clamp(particleSpreadModifier * 10f, 1f, 10f);
				float storeEmitTimes = 1f / (particleParameters.storeEmission * num * num2);
				particleParameters.storeEmitTimes = storeEmitTimes;
			}
		}
	}

	private void UpdateParticleColor()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			float num = 0.5f;
			if (particleParameters.ContainsKey(particles[i]))
			{
				Color storeOriginalColor = particleParameters[particles[i]].storeOriginalColor;
				Color color = particleTint + storeOriginalColor * num;
				if (storeOriginalColor != Color.white)
				{
					particleParameters[particles[i]].storeColor = color;
				}
				if (particles[i].name.Contains("SubEmitter"))
				{
					particles[i].startColor = color;
				}
			}
		}
	}

	public static Color GetParticleColor(string paint)
	{
		Color white = Color.white;
		return Blocksworld.getColor(paint);
	}

	private void UpdateParticlePosition()
	{
		if (emitterGO != null)
		{
			emitterGO.transform.position = goT.position;
			emitterGO.transform.rotation = goT.rotation;
			emitterGO.transform.rotation *= Quaternion.Euler(0f - particleAngleModifier, 0f, 0f);
		}
	}

	private void EmitParticle(ParticleSystem ourSystem)
	{
		ParticleParameters particleParameters = BlockAbstractParticles.particleParameters[ourSystem];
		Transform transform = ourSystem.transform;
		Vector3 vector = new Vector3(Random.Range(0f - particleSpreadModifier, particleSpreadModifier), Random.Range(0f - particleSpreadModifier, particleSpreadModifier), 0f);
		Vector3 velocity = transform.TransformDirection(Vector3.forward + vector).normalized * particleParameters.storeSpeed * particleSpeedModifier;
		float num = Mathf.Clamp(particleParameters.storeLife * (particleSpeedModifier * 0.75f), 1f, 5f);
		float num2 = Mathf.Clamp(particleSpreadModifier * 100f / 20f, 1f, 1.5f);
		float num3 = particleParameters.storeSize * particleSpeedModifier * num2;
		Vector3 position = goT.position + goT.TransformDirection(new Vector3(Random.Range(0f - baseSize.x, baseSize.x), Random.Range(0f - baseSize.y, baseSize.y), 0.1f));
		ParticleSystem.Particle particle = new ParticleSystem.Particle
		{
			position = position,
			velocity = velocity,
			size = num3,
			remainingLifetime = num,
			startLifetime = num,
			color = particleParameters.storeColor,
			randomSeed = (uint)Random.Range(17, 9999999)
		};
		ourSystem.Emit(particle);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (particles.Length < 1)
		{
			return;
		}
		UpdateParticlePosition();
		bool flag = TreasureHandler.IsPartOfPickedUpTreasureModel(this);
		if (isFiring && shouldEmit && !flag)
		{
			if (Sound.sfxEnabled)
			{
				PlayLoopSound(play: true, GetLoopClip());
			}
			for (int i = 0; i < Mathf.Min(particles.Length, particleEmitTimes.Count); i++)
			{
				particleEmitTimes[i] += Time.deltaTime;
				if (particleParameters.ContainsKey(particles[i]))
				{
					float storeEmitTimes = particleParameters[particles[i]].storeEmitTimes;
					if (particleEmitTimes[i] > storeEmitTimes)
					{
						particles[i].maxParticles = (int)((float)particleParameters[particles[i]].storeMaxParts * Mathf.Max(particleSpeedModifier * particleSpeedModifier, 1f, 1.5f));
						EmitParticle(particles[i]);
						particleEmitTimes[i] = 0f;
					}
				}
			}
		}
		else
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
	}
}
