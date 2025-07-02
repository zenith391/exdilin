using UnityEngine;

namespace Blocks;

public class BlockEmitterSystemInfo
{
	public GameObject emitterObject;

	private ParticleSystem particles;

	private ParticleSystem.Particle[] particleArr = new ParticleSystem.Particle[1000];

	private float particlesPerSecond;

	private float timeToNext = -1f;

	public BlockEmitterSystemInfo(string str)
	{
		emitterObject = Object.Instantiate(Resources.Load("Blocks/BlockEmitter " + str)) as GameObject;
		particles = emitterObject.GetComponent<ParticleSystem>();
		emitterObject.SetActive(value: false);
	}

	public void Emit(float particlesPerS)
	{
		particlesPerSecond += particlesPerS * (0.9f + 0.2f * Random.value);
	}

	public void Destroy()
	{
		Object.Destroy(emitterObject);
	}

	public void Activate()
	{
		particles.Clear();
		emitterObject.SetActive(value: true);
		particles.enableEmission = true;
	}

	public void Deactivate()
	{
		emitterObject.SetActive(value: false);
	}

	public void FixedUpdate()
	{
		if (timeToNext <= 0f && (double)particlesPerSecond > 0.01)
		{
			timeToNext = 1f / particlesPerSecond;
			particles.Emit(1);
		}
		else
		{
			timeToNext -= Blocksworld.fixedDeltaTime;
		}
		particlesPerSecond = 0f;
	}

	public bool AnyWithinBounds(Bounds bounds)
	{
		int num = particles.GetParticles(particleArr);
		for (int i = 0; i < num; i++)
		{
			ParticleSystem.Particle particle = particleArr[i];
			Vector3 position = particle.position;
			if (bounds.Contains(position))
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateTransform(GameObject go)
	{
		emitterObject.transform.position = go.transform.position;
		emitterObject.transform.rotation = go.transform.rotation;
		emitterObject.transform.Rotate(90f, 0f, 0f);
	}

	public void UpdateMaterial(Material mat, string texSym)
	{
		ParticleSystemRenderer particleSystemRenderer = (ParticleSystemRenderer)emitterObject.GetComponent<Renderer>();
		particleSystemRenderer.enabled = true;
		if (texSym == "Plain")
		{
			particleSystemRenderer.material.color = new Color(0f, 0f, 0f, 0f);
			particleSystemRenderer.enabled = false;
			particleSystemRenderer.material.mainTexture = null;
		}
		else
		{
			particleSystemRenderer.material.color = mat.color;
			particleSystemRenderer.material.mainTexture = mat.mainTexture;
		}
	}
}
