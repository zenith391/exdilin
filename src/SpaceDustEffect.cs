using UnityEngine;

public class SpaceDustEffect : WeatherEffect
{
	private GameObject spaceDustGo;

	private ParticleSystem spaceDustPs;

	private void CreateParticleSystem()
	{
		if (spaceDustGo == null)
		{
			spaceDustGo = Object.Instantiate(Resources.Load("Env Effect/Space Dust Particle System")) as GameObject;
			spaceDustPs = spaceDustGo.GetComponent<ParticleSystem>();
		}
		ResetSpaceDust();
		Stop();
	}

	public override void Pause()
	{
		base.Pause();
		if (spaceDustPs != null)
		{
			spaceDustPs.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (spaceDustPs != null)
		{
			spaceDustPs.Play();
		}
	}

	public override void Update()
	{
		base.Update();
		if (null != spaceDustGo && !paused)
		{
			Transform cameraTransform = Blocksworld.cameraTransform;
			spaceDustGo.transform.rotation = cameraTransform.rotation;
			Vector3 camVelocity = BlocksworldCamera.GetCamVelocity();
			float z = cameraTransform.InverseTransformDirection(camVelocity).z;
			Vector3 cameraForward = Blocksworld.cameraForward;
			Vector3 vector = cameraForward * (z * 40f);
			spaceDustGo.transform.position = cameraTransform.position + vector;
			float num = Mathf.Clamp(z, 1f, 4f);
			Vector3 localScale = Vector3.one + Vector3.forward * num;
			spaceDustGo.transform.localScale = localScale;
		}
	}

	public override void Start()
	{
		base.Start();
		CreateParticleSystem();
		spaceDustPs.Play();
	}

	public override void Stop()
	{
		if (spaceDustPs != null)
		{
			spaceDustPs.Stop();
			spaceDustPs.startSpeed = 0f;
		}
	}

	private void ResetSpaceDust()
	{
		if (spaceDustPs != null)
		{
			spaceDustPs.startSpeed = 0f;
		}
		if (spaceDustGo != null)
		{
			spaceDustGo.transform.rotation = Quaternion.identity;
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (spaceDustGo != null)
		{
			Object.Destroy(spaceDustGo);
		}
		spaceDustGo = null;
		spaceDustPs = null;
	}
}
