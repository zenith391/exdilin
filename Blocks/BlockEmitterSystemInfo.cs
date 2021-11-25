using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000089 RID: 137
	public class BlockEmitterSystemInfo
	{
		// Token: 0x06000B97 RID: 2967 RVA: 0x00053974 File Offset: 0x00051D74
		public BlockEmitterSystemInfo(string str)
		{
			this.emitterObject = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockEmitter " + str)) as GameObject);
			this.particles = this.emitterObject.GetComponent<ParticleSystem>();
			this.emitterObject.SetActive(false);
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x000539DF File Offset: 0x00051DDF
		public void Emit(float particlesPerS)
		{
			this.particlesPerSecond += particlesPerS * (0.9f + 0.2f * UnityEngine.Random.value);
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x00053A01 File Offset: 0x00051E01
		public void Destroy()
		{
			UnityEngine.Object.Destroy(this.emitterObject);
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x00053A0E File Offset: 0x00051E0E
		public void Activate()
		{
			this.particles.Clear();
			this.emitterObject.SetActive(true);
			this.particles.enableEmission = true;
		}

		// Token: 0x06000B9B RID: 2971 RVA: 0x00053A33 File Offset: 0x00051E33
		public void Deactivate()
		{
			this.emitterObject.SetActive(false);
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x00053A44 File Offset: 0x00051E44
		public void FixedUpdate()
		{
			if (this.timeToNext <= 0f && (double)this.particlesPerSecond > 0.01)
			{
				this.timeToNext = 1f / this.particlesPerSecond;
				this.particles.Emit(1);
			}
			else
			{
				this.timeToNext -= Blocksworld.fixedDeltaTime;
			}
			this.particlesPerSecond = 0f;
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x00053AB8 File Offset: 0x00051EB8
		public bool AnyWithinBounds(Bounds bounds)
		{
			int num = this.particles.GetParticles(this.particleArr);
			for (int i = 0; i < num; i++)
			{
				ParticleSystem.Particle particle = this.particleArr[i];
				Vector3 position = particle.position;
				if (bounds.Contains(position))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x00053B14 File Offset: 0x00051F14
		public void UpdateTransform(GameObject go)
		{
			this.emitterObject.transform.position = go.transform.position;
			this.emitterObject.transform.rotation = go.transform.rotation;
			this.emitterObject.transform.Rotate(90f, 0f, 0f);
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x00053B78 File Offset: 0x00051F78
		public void UpdateMaterial(Material mat, string texSym)
		{
			ParticleSystemRenderer particleSystemRenderer = (ParticleSystemRenderer)this.emitterObject.GetComponent<Renderer>();
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

		// Token: 0x04000934 RID: 2356
		public GameObject emitterObject;

		// Token: 0x04000935 RID: 2357
		private ParticleSystem particles;

		// Token: 0x04000936 RID: 2358
		private ParticleSystem.Particle[] particleArr = new ParticleSystem.Particle[1000];

		// Token: 0x04000937 RID: 2359
		private float particlesPerSecond;

		// Token: 0x04000938 RID: 2360
		private float timeToNext = -1f;
	}
}
