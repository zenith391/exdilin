using UnityEngine;

public class AbstractSnowEffect : WeatherEffect
{
	protected string prefabName = "Env Effect/Snow Particle System";

	private GameObject snowGo;

	private ParticleSystem ps;

	private void CreateParticleSystem()
	{
		if (ps == null)
		{
			if (snowGo == null)
			{
				snowGo = Object.Instantiate(Resources.Load(prefabName)) as GameObject;
			}
			ps = snowGo.GetComponent<ParticleSystem>();
		}
		Stop();
	}

	public override void Pause()
	{
		base.Pause();
		if (ps != null)
		{
			ps.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (ps != null)
		{
			ps.Play();
		}
	}

	public override void Update()
	{
		base.Update();
		if (!(snowGo != null) || paused)
		{
			return;
		}
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		Vector3 vector = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
		float magnitude = vector.magnitude;
		if (magnitude > 0.01f)
		{
			Vector3 vector2 = vector / magnitude;
			float num = Mathf.Min(magnitude * 0.5f, 30f);
			Vector3 position = cameraPosition + vector2 * num;
			GameObject worldOcean = Blocksworld.worldOcean;
			if (worldOcean != null && worldOcean.GetComponent<Collider>() != null)
			{
				Bounds bounds = worldOcean.GetComponent<Collider>().bounds;
				position.y = Mathf.Max(position.y, bounds.max.y + 10f);
			}
			snowGo.transform.position = position;
		}
		ps.emissionRate = intensityMultiplier * 100f;
	}

	public override void Start()
	{
		base.Start();
		CreateParticleSystem();
		ps.Play();
	}

	public override void Stop()
	{
		if (ps != null)
		{
			ps.Stop();
			ps.Clear();
		}
	}

	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		if (snowGo != null)
		{
			snowGo.transform.rotation = Quaternion.Euler(0f, angle, 0f);
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (snowGo != null)
		{
			Object.Destroy(snowGo);
		}
		snowGo = null;
		ps = null;
	}
}
