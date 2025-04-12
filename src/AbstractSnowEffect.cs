using System;
using UnityEngine;

// Token: 0x0200034C RID: 844
public class AbstractSnowEffect : WeatherEffect
{
	// Token: 0x060025DE RID: 9694 RVA: 0x0011769C File Offset: 0x00115A9C
	private void CreateParticleSystem()
	{
		if (this.ps == null)
		{
			if (this.snowGo == null)
			{
				this.snowGo = (UnityEngine.Object.Instantiate(Resources.Load(this.prefabName)) as GameObject);
			}
			this.ps = this.snowGo.GetComponent<ParticleSystem>();
		}
		this.Stop();
	}

	// Token: 0x060025DF RID: 9695 RVA: 0x001176FD File Offset: 0x00115AFD
	public override void Pause()
	{
		base.Pause();
		if (this.ps != null)
		{
			this.ps.Pause();
		}
	}

	// Token: 0x060025E0 RID: 9696 RVA: 0x00117721 File Offset: 0x00115B21
	public override void Resume()
	{
		base.Resume();
		if (this.ps != null)
		{
			this.ps.Play();
		}
	}

	// Token: 0x060025E1 RID: 9697 RVA: 0x00117748 File Offset: 0x00115B48
	public override void Update()
	{
		base.Update();
		if (this.snowGo != null && !this.paused)
		{
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Vector3 a = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
			float magnitude = a.magnitude;
			if (magnitude > 0.01f)
			{
				Vector3 a2 = a / magnitude;
				float d = Mathf.Min(magnitude * 0.5f, 30f);
				Vector3 position = cameraPosition + a2 * d;
				GameObject worldOcean = Blocksworld.worldOcean;
				if (worldOcean != null && worldOcean.GetComponent<Collider>() != null)
				{
					Bounds bounds = worldOcean.GetComponent<Collider>().bounds;
					position.y = Mathf.Max(position.y, bounds.max.y + 10f);
				}
				this.snowGo.transform.position = position;
			}
			this.ps.emissionRate = this.intensityMultiplier * 100f;
		}
	}

	// Token: 0x060025E2 RID: 9698 RVA: 0x00117852 File Offset: 0x00115C52
	public override void Start()
	{
		base.Start();
		this.CreateParticleSystem();
		this.ps.Play();
	}

	// Token: 0x060025E3 RID: 9699 RVA: 0x0011786B File Offset: 0x00115C6B
	public override void Stop()
	{
		if (this.ps != null)
		{
			this.ps.Stop();
			this.ps.Clear();
		}
	}

	// Token: 0x060025E4 RID: 9700 RVA: 0x00117894 File Offset: 0x00115C94
	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		if (this.snowGo != null)
		{
			this.snowGo.transform.rotation = Quaternion.Euler(0f, angle, 0f);
		}
	}

	// Token: 0x060025E5 RID: 9701 RVA: 0x001178CE File Offset: 0x00115CCE
	public override void Reset()
	{
		base.Reset();
		if (this.snowGo != null)
		{
			UnityEngine.Object.Destroy(this.snowGo);
		}
		this.snowGo = null;
		this.ps = null;
	}

	// Token: 0x040020E2 RID: 8418
	protected string prefabName = "Env Effect/Snow Particle System";

	// Token: 0x040020E3 RID: 8419
	private GameObject snowGo;

	// Token: 0x040020E4 RID: 8420
	private ParticleSystem ps;
}
