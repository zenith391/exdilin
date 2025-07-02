using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public abstract class BlockAbstractJetpack : BlockAbstractAntiGravity
{
	private ParticleSystem particles;

	private Vector3[] localOffsets = new Vector3[2]
	{
		new Vector3(-0.325f, -0.4f, -0.8f),
		new Vector3(0.325f, -0.4f, -0.8f)
	};

	private float particleSpeed = -3.5f;

	private float particleLifetime = 1f;

	private const int MAX_PARTICLES = 100;

	private bool enabled;

	private bool setParticleColor;

	private bool paused;

	private bool emitParticle = true;

	private int colorMeshIndex;

	private Color particleColor = Color.white;

	private Transform transform;

	private Rigidbody chunkRB;

	private JetpackMetaData jetpackMeta;

	private string particleName = "Jetpack Smoke";

	public BlockAbstractJetpack(List<List<Tile>> tiles)
		: base(tiles)
	{
		playLoop = false;
		informAboutVaryingGravity = false;
		transform = goT;
		jetpackMeta = go.GetComponent<JetpackMetaData>();
		if (jetpackMeta != null)
		{
			localOffsets = jetpackMeta.effectOffsets;
			particleName = jetpackMeta.particleName;
			setParticleColor = jetpackMeta.inheritColor;
		}
	}

	public override void Play()
	{
		chunkRB = chunk.rb;
		base.Play();
		if (setParticleColor)
		{
			UpdateParticleColorPaint(GetPaint(colorMeshIndex), colorMeshIndex);
			UpdateParticleColorTexture(GetTexture(colorMeshIndex), colorMeshIndex);
		}
	}

	public static Color GetParticleColor(string paint, Renderer renderer)
	{
		Color color = renderer.sharedMaterial.color * 1.3f;
		if (Blocksworld.IsLuminousPaint(paint))
		{
			color += color;
		}
		return color;
	}

	private void UpdateParticleColorPaint(string paint, int meshIndex)
	{
		if (setParticleColor && meshIndex == colorMeshIndex && subMeshGameObjects.Count > colorMeshIndex)
		{
			Renderer renderer = ((meshIndex != 0) ? subMeshGameObjects[meshIndex - 1].GetComponent<Renderer>() : go.GetComponent<Renderer>());
			particleColor = GetParticleColor(paint, renderer);
		}
	}

	private void UpdateParticleColorTexture(string texture, int meshIndex)
	{
		if (setParticleColor && meshIndex == colorMeshIndex && subMeshGameObjects.Count > colorMeshIndex)
		{
			emitParticle = texture != "Glass";
		}
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
		UpdateParticleColorTexture(texture, meshIndex);
		return result;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		UpdateParticleColorPaint(paint, meshIndex);
		return result;
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		if (particles != null)
		{
			if (particles.isPlaying)
			{
				particles.Stop();
			}
			particles.Clear();
			Object.Destroy(particles.gameObject);
		}
	}

	public override void Pause()
	{
		base.Pause();
		if (particles != null && particles.isPlaying)
		{
			particles.Pause();
		}
	}

	public override void ResetFrame()
	{
		base.ResetFrame();
		enabled = false;
	}

	public override void Resume()
	{
		base.Resume();
	}

	public TileResultCode EmitSmoke(ScriptRowExecutionInfo eInfo, object[] args)
	{
		enabled = true;
		return TileResultCode.True;
	}

	private ParticleSystem SpawnParticle()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("VFX/" + particleName)) as GameObject;
		if (gameObject != null)
		{
			ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
			if (component != null)
			{
				particleSpeed = component.startSpeed;
				particleLifetime = component.startLifetime;
				return component;
			}
		}
		return null;
	}

	public override void Update()
	{
		base.Update();
		_ = particles != null;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (paused)
		{
			return;
		}
		if (particles == null)
		{
			particles = SpawnParticle();
			return;
		}
		if (transform == null || !enabled || !emitParticle)
		{
			particles.Stop();
			return;
		}
		int particleCount = particles.particleCount;
		if (particleCount >= 100)
		{
			return;
		}
		float num = 1f - (float)particleCount / 100f;
		float num2 = num;
		Vector3 vector = ((!(chunkRB != null)) ? Vector3.zero : chunk.rb.velocity);
		while (num2 > 0f)
		{
			num2 -= 1f;
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Vector3 position = transform.position;
			float magnitude = (position - cameraPosition).magnitude;
			if (magnitude <= Blocksworld.fogEnd && (magnitude <= 15f || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(position, Vector3.one))))
			{
				for (int i = 0; i < localOffsets.Length; i++)
				{
					Vector3 direction = localOffsets[i];
					Vector3 vector2 = transform.TransformDirection(direction);
					position = transform.position + vector2;
					float num3 = Random.Range(particleLifetime - 0.05f, particleLifetime + 0.05f);
					float num4 = Random.Range(1f, 1.25f);
					float y = Random.Range(particleSpeed + 0.25f, particleSpeed - 0.25f);
					Vector3 velocity = transform.TransformDirection(new Vector3(GetRandomXZ(), y, GetRandomXZ())) + vector;
					ParticleSystem.Particle particle = new ParticleSystem.Particle
					{
						remainingLifetime = num3,
						color = particleColor,
						position = position,
						velocity = velocity,
						rotation = Random.Range(0f, 360f),
						startLifetime = num3,
						size = num4,
						randomSeed = (uint)Random.Range(12, 21314)
					};
					particles.Emit(particle);
				}
			}
		}
	}

	private float GetRandomXZ()
	{
		return Random.Range(0.1f, -0.1f);
	}
}
