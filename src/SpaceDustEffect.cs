using System;
using UnityEngine;

// Token: 0x02000356 RID: 854
public class SpaceDustEffect : WeatherEffect
{
	// Token: 0x0600261A RID: 9754 RVA: 0x0011A3A8 File Offset: 0x001187A8
	private void CreateParticleSystem()
	{
		if (this.spaceDustGo == null)
		{
			this.spaceDustGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Space Dust Particle System")) as GameObject);
			this.spaceDustPs = this.spaceDustGo.GetComponent<ParticleSystem>();
		}
		this.ResetSpaceDust();
		this.Stop();
	}

	// Token: 0x0600261B RID: 9755 RVA: 0x0011A3FD File Offset: 0x001187FD
	public override void Pause()
	{
		base.Pause();
		if (this.spaceDustPs != null)
		{
			this.spaceDustPs.Pause();
		}
	}

	// Token: 0x0600261C RID: 9756 RVA: 0x0011A421 File Offset: 0x00118821
	public override void Resume()
	{
		base.Resume();
		if (this.spaceDustPs != null)
		{
			this.spaceDustPs.Play();
		}
	}

	// Token: 0x0600261D RID: 9757 RVA: 0x0011A448 File Offset: 0x00118848
	public override void Update()
	{
		base.Update();
		if (null != this.spaceDustGo && !this.paused)
		{
			Transform cameraTransform = Blocksworld.cameraTransform;
			this.spaceDustGo.transform.rotation = cameraTransform.rotation;
			Vector3 camVelocity = BlocksworldCamera.GetCamVelocity();
			float z = cameraTransform.InverseTransformDirection(camVelocity).z;
			Vector3 cameraForward = Blocksworld.cameraForward;
			Vector3 b = cameraForward * (z * 40f);
			this.spaceDustGo.transform.position = cameraTransform.position + b;
			float d = Mathf.Clamp(z, 1f, 4f);
			Vector3 localScale = Vector3.one + Vector3.forward * d;
			this.spaceDustGo.transform.localScale = localScale;
		}
	}

	// Token: 0x0600261E RID: 9758 RVA: 0x0011A519 File Offset: 0x00118919
	public override void Start()
	{
		base.Start();
		this.CreateParticleSystem();
		this.spaceDustPs.Play();
	}

	// Token: 0x0600261F RID: 9759 RVA: 0x0011A532 File Offset: 0x00118932
	public override void Stop()
	{
		if (this.spaceDustPs != null)
		{
			this.spaceDustPs.Stop();
			this.spaceDustPs.startSpeed = 0f;
		}
	}

	// Token: 0x06002620 RID: 9760 RVA: 0x0011A560 File Offset: 0x00118960
	private void ResetSpaceDust()
	{
		if (this.spaceDustPs != null)
		{
			this.spaceDustPs.startSpeed = 0f;
		}
		if (this.spaceDustGo != null)
		{
			this.spaceDustGo.transform.rotation = Quaternion.identity;
		}
	}

	// Token: 0x06002621 RID: 9761 RVA: 0x0011A5B4 File Offset: 0x001189B4
	public override void Reset()
	{
		base.Reset();
		if (this.spaceDustGo != null)
		{
			UnityEngine.Object.Destroy(this.spaceDustGo);
		}
		this.spaceDustGo = null;
		this.spaceDustPs = null;
	}

	// Token: 0x04002122 RID: 8482
	private GameObject spaceDustGo;

	// Token: 0x04002123 RID: 8483
	private ParticleSystem spaceDustPs;
}
