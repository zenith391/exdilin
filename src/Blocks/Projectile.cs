using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200005A RID: 90
	public class Projectile : AbstractProjectile
	{
		// Token: 0x06000744 RID: 1860 RVA: 0x0003179C File Offset: 0x0002FB9C
		public Projectile(BlockAbstractLaser sender) : base(sender)
		{
			this.travelThroughTransparent = false;
			this.canReflect = false;
			this.gravityInfluence = 0.25f;
			Vector3 fireDirectionUp = sender.GetFireDirectionUp();
			Vector3 fireDirectionForward = sender.GetFireDirectionForward();
			this.segment = new AbstractProjectile.Segment(sender, base.GetSenderPosition() + this.extraSpeed * Blocksworld.fixedDeltaTime, fireDirectionUp, fireDirectionForward, this.kSpeed, this.extraSpeed, this.CreateSegmentGo());
			this.segment.ReceivingEnergy = true;
			this.segmentTime = sender.segmentTime;
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x00031835 File Offset: 0x0002FC35
		public override void Destroy()
		{
			if (this.segment != null)
			{
				this.segment.Destroy();
			}
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00031850 File Offset: 0x0002FC50
		protected override GameObject GetSegmentPrefab()
		{
			if (Projectile.segmentPrefab == null)
			{
				Projectile.segmentPrefab = new GameObject(string.Empty);
				Mesh mesh = new Mesh();
				mesh.Clear();
				List<Vector3> list = new List<Vector3>();
				List<Vector2> list2 = new List<Vector2>();
				List<int> list3 = new List<int>();
				float sizeMult = 1f;
				BlockAbstractLaser.DrawProjectileLine(-Vector3.forward * 0.5f, Vector3.forward * 0.5f, Vector3.up, list, list2, list3, sizeMult, 0f);
				mesh.vertices = list.ToArray();
				mesh.uv = list2.ToArray();
				mesh.triangles = list3.ToArray();
				mesh.name = "Projectile Mesh";
				MeshFilter meshFilter = Projectile.segmentPrefab.AddComponent<MeshFilter>();
				meshFilter.mesh = mesh;
				MeshRenderer meshRenderer = Projectile.segmentPrefab.AddComponent<MeshRenderer>();
				meshRenderer.material = (Material)Resources.Load("Materials/Projectile");
				Projectile.segmentPrefab.SetActive(false);
			}
			return Projectile.segmentPrefab;
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x00031950 File Offset: 0x0002FD50
		protected override GameObject CreateSegmentGo()
		{
			GameObject original = this.GetSegmentPrefab();
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
			gameObject.SetActive(true);
			Color laserColor = this._sender.GetLaserColor();
			gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", laserColor);
			return gameObject;
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x00031995 File Offset: 0x0002FD95
		public override bool ShouldBeDestroyed()
		{
			return this.segment == null;
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x000319A0 File Offset: 0x0002FDA0
		protected override void StepSegments()
		{
			if (this.segment == null)
			{
				return;
			}
			this.segment.TravelledTime += Blocksworld.fixedDeltaTime;
			float num = Vector3.Dot(this.segment.ExtraVelocity, this.segment.Direction);
			if (this.gravityInfluence > 0f)
			{
				this.segment.ExtraVelocity += Physics.gravity * this.gravityInfluence * Blocksworld.fixedDeltaTime;
			}
			float num2 = this.segment.HeadSpeed + num;
			float num3 = this.segment.TailSpeed + num;
			this.segment.TravelledDist += num2 * Blocksworld.fixedDeltaTime;
			if (this.segment.ReceivingEnergy)
			{
				this.segment.Length += num2 * Blocksworld.fixedDeltaTime;
				this.segment.StartFraction = 1f - this.EnergyFraction;
			}
			this.segment.Length += (num2 - num3) * Blocksworld.fixedDeltaTime;
			this.segment.Origin += Util.ProjectOntoPlane(this.segment.ExtraVelocity, this.segment.Direction) * Blocksworld.fixedDeltaTime;
			Vector3 origin = this.segment.Origin + this.segment.Direction * (this.segment.TravelledDist - this.segment.Length);
			float num4 = this.segmentTime / this.kSpeed;
			RaycastHit raycastHit;
			if ((double)this.segment.Length < 0.0001 || this.segment.TravelledTime > num4)
			{
				this.segment.Destroy();
				this.segment = null;
			}
			else if (Physics.Raycast(origin, this.segment.Direction, out raycastHit, this.segment.Length))
			{
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
				if (block != null && block != this._sender)
				{
					this.AddHit(block, this.segment.Direction, raycastHit.point, raycastHit.normal);
					this.segment.HeadSpeed = 0f;
					this.segment.ExtraVelocity = Vector3.zero;
				}
			}
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x00031C0D File Offset: 0x0003000D
		public override void StopReceivingEnergy()
		{
			if (this.segment != null)
			{
				this.segment.ReceivingEnergy = false;
			}
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00031C28 File Offset: 0x00030028
		public override void Update()
		{
			if (this.segment == null)
			{
				return;
			}
			Vector3 vector = this.segment.Origin + this.segment.Direction * (this.segment.TravelledDist - this.segment.Length);
			Vector3 vector2 = this.segment.Origin + this.segment.Direction * this.segment.TravelledDist;
			Vector3 vector3 = vector2 - vector;
			float magnitude = vector3.magnitude;
			if (magnitude < 0.001f)
			{
				return;
			}
			Transform goT = this.segment.goT;
			Transform cameraTransform = Blocksworld.cameraTransform;
			Vector3 position = cameraTransform.position;
			Vector3 rhs = vector - position;
			Vector3 normalized = Vector3.Cross(vector3, rhs).normalized;
			float projectileSizeMultiplier = this._sender.projectileSizeMultiplier;
			goT.rotation = Quaternion.LookRotation(vector3 / magnitude, normalized);
			goT.position = 0.5f * (vector + vector2);
			goT.localScale = new Vector3(projectileSizeMultiplier, projectileSizeMultiplier, magnitude);
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00031D48 File Offset: 0x00030148
		protected override void AddHit(Block block, Vector3 direction, Vector3 point, Vector3 normal)
		{
			BlockAbstractLaser.AddProjectileHit(this._sender, block);
			Rigidbody rb = block.chunk.rb;
			if (rb != null && !rb.isKinematic)
			{
				rb.AddForceAtPosition(direction * this._sender.laserMeta.projectileHitForce, point, ForceMode.Impulse);
			}
			float value = UnityEngine.Random.value;
			Vector3 vector = normal + 0.2f * UnityEngine.Random.insideUnitSphere;
			string texture = block.GetTexture(0);
			bool flag = Materials.TextureIsTransparent(texture);
			Color color = block.go.GetComponent<Renderer>().sharedMaterial.color;
			if (!block.isTerrain)
			{
				float r = color.r;
				if (r > 0f)
				{
					float g = color.g;
					float b = color.b;
					if ((g < 0.01f || r / g > 2f) && (b < 0.01f || r / b > 2f))
					{
						float num = (r + b + g) / 3f;
						color.r = num;
						color.g = num;
						color.b = num;
					}
				}
			}
			Color a = color + 0.1f * Color.white;
			if (flag)
			{
				color.a = UnityEngine.Random.Range(0.5f, 0.7f);
				a.a = UnityEngine.Random.Range(0.5f, 0.7f);
			}
			Color c = value * color + (1f - value) * a;
			float num2 = UnityEngine.Random.Range(0.7f, 1.1f);
			ParticleSystem.Particle particle = new ParticleSystem.Particle
			{
				remainingLifetime = num2,
				color = c,
				position = point,
				velocity = vector.normalized * UnityEngine.Random.Range(3f, 8f),
				rotation = UnityEngine.Random.Range(0f, 360f),
				startLifetime = num2,
				size = UnityEngine.Random.Range(0.6f, 0.9f),
				randomSeed = (uint)UnityEngine.Random.Range(12, 21314)
			};
			BlockAbstractLaser.projectileHitParticleSystem.Emit(particle);
			block.PlayPositionedSound("Projectile Hit", 0.5f, 1f);
		}

		// Token: 0x04000582 RID: 1410
		protected bool addedHitEffect;

		// Token: 0x04000583 RID: 1411
		protected AbstractProjectile.Segment segment;

		// Token: 0x04000584 RID: 1412
		private static GameObject segmentPrefab;

		// Token: 0x04000585 RID: 1413
		private float segmentTime = 100f;
	}
}
