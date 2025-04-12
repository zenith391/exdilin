using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000AC RID: 172
	public class MissileBody : IMissile
	{
		// Token: 0x06000DB3 RID: 3507 RVA: 0x0005E189 File Offset: 0x0005C589
		public HashSet<int> GetLabels()
		{
			return this.block.labels;
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x0005E196 File Offset: 0x0005C596
		public void Break()
		{
			this.broken = true;
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x0005E19F File Offset: 0x0005C59F
		public bool IsBroken()
		{
			return this.broken;
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x0005E1A7 File Offset: 0x0005C5A7
		public bool IsBursting()
		{
			return this.bursting;
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x0005E1AF File Offset: 0x0005C5AF
		public bool CanExplode()
		{
			return this.canExplode;
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x0005E1B7 File Offset: 0x0005C5B7
		private float CalculateMinDistTime(Vector3 posDiff, Vector3 velDiff)
		{
			return -Vector3.Dot(posDiff, velDiff) / Vector3.Dot(velDiff, velDiff);
		}

		// Token: 0x06000DB9 RID: 3513 RVA: 0x0005E1CC File Offset: 0x0005C5CC
		private float CalculatePenalty(Vector3 posDiff, Vector3 velDiff, float t)
		{
			return (posDiff + velDiff * t).magnitude;
		}

		// Token: 0x06000DBA RID: 3514 RVA: 0x0005E1F0 File Offset: 0x0005C5F0
		public void FixedUpdate()
		{
			if (this.exploded)
			{
				return;
			}
			GameObject go = this.chunk.go;
			if (go == null)
			{
				return;
			}
			float num = (float)this.counter * Blocksworld.fixedDeltaTime;
			bool flag = !this.broken;
			if (!this.broken)
			{
				if (this.block.globalBurstTimeSet)
				{
					flag = (num < this.block.globalBurstTime);
				}
				else if (this.block.burstTimeSet)
				{
					flag = (num < this.block.burstTime);
				}
			}
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			GameObject go2 = this.block.go;
			Rigidbody component = go.GetComponent<Rigidbody>();
			float mass = component.mass;
			float gravityFraction = this.block.gravityFraction;
			if (gravityFraction < 1f && !this.broken)
			{
				vector -= Physics.gravity * mass * (1f - gravityFraction);
			}
			Transform transform = go2.transform;
			Vector3 up = transform.up;
			float magnitude = component.inertiaTensor.magnitude;
			this.bursting = flag;
			if (flag)
			{
				Vector3 position = transform.position;
				float num2 = 20f * this.burstMultiplier;
				float d = mass * num2;
				Vector3 vector3 = up * d;
				if (this.burstMultiplier > 0f && !this.block.vanished && !this.expired)
				{
					this.block.EmitSmoke(position - up * this.block.smokeOffset, component.velocity - vector3.normalized * 6f * this.burstMultiplier * this.block.smokeSize);
				}
				float num3 = (this.block.localTargetTag != null) ? this.block.localLockDelay : this.block.controllerLockDelay;
				bool flag2 = num > num3;
				if (flag2)
				{
					string text = (this.block.localTargetTag != null) ? this.block.localTargetTag : this.block.controllerTargetTag;
					if (text != null)
					{
						Vector3 velocity = component.velocity;
						Vector3 pos = position + velocity * 0.5f;
						Block block;
						if (TagManager.TryGetClosestBlockWithTag(text, pos, out block, null))
						{
							Vector3 position2 = block.goT.position;
							Vector3 zero = Vector3.zero;
							Vector3 posDiff = position2 - position;
							Vector3 vector4 = zero - velocity;
							float d2 = Mathf.Min(posDiff.magnitude / 3f, 10f);
							Vector3 a = position2 + vector4 * d2;
							Vector3 normalized = (a - position).normalized;
							float num4 = this.CalculateMinDistTime(posDiff, vector4);
							bool flag3 = num4 < 0f;
							if (!flag3)
							{
								float num5 = this.CalculatePenalty(posDiff, vector4, num4);
								Vector3 a2 = Vector3.zero;
								float num6 = 0.01f;
								for (int i = 0; i < 3; i++)
								{
									Vector3 zero2 = Vector3.zero;
									zero2[i] = num6;
									vector4 = zero - (velocity + zero2);
									float num7 = this.CalculateMinDistTime(posDiff, vector4);
									flag3 = (flag3 || num7 < 0f);
									float num8 = this.CalculatePenalty(posDiff, vector4, num7);
									a2[i] = (num8 - num5) / num6;
								}
								float magnitude2 = a2.magnitude;
								if (!flag3 && magnitude2 > 0f)
								{
									float num9 = 2f;
									if (magnitude2 > num9)
									{
										a2 = a2.normalized * num9;
									}
									normalized = (up - a2 * 20f).normalized;
									Vector3 b = -mass * a2 * 5f * this.burstMultiplier;
									vector += b;
								}
							}
							normalized = (normalized - Physics.gravity * gravityFraction * 0.05f).normalized;
							float num10 = Vector3.Dot(up, normalized);
							if (num10 < 0.5f)
							{
								float d3 = (num10 + 1f) / 1.5f;
								vector3 *= d3;
							}
							float a3 = Vector3.Angle(up, normalized);
							Vector3 vector5 = Vector3.Cross(up, normalized).normalized * Mathf.Min(a3, 70f) * magnitude * 0.07f;
							vector5 = Util.ProjectOntoPlane(vector5, up);
							vector5 -= component.angularVelocity * magnitude * 0.1f;
							vector2 += vector5;
						}
					}
				}
				vector += vector3;
				component.AddForce(vector);
				component.AddTorque(vector2);
			}
			else
			{
				Vector3 velocity2 = component.velocity;
				float magnitude3 = velocity2.magnitude;
				if (magnitude3 > 0.01f)
				{
					Vector3 vector6 = velocity2 / magnitude3;
					float a4 = Vector3.Angle(up, vector6);
					Vector3 torque = Vector3.Cross(up, vector6).normalized * Mathf.Min(a4, 70f) * magnitude * magnitude3 * 0.003f;
					component.AddTorque(torque);
				}
			}
			this.counter++;
			this.canExplode = (num > 1f);
			if (num >= this.lifetime)
			{
				this.expired = true;
			}
		}

		// Token: 0x06000DBB RID: 3515 RVA: 0x0005E7B8 File Offset: 0x0005CBB8
		public void Explode(float radius)
		{
			this.exploded = true;
			Vector3 vel = Vector3.zero;
			if (this.chunk.go != null)
			{
				Rigidbody rb = this.chunk.rb;
				if (rb != null)
				{
					vel = rb.velocity;
				}
			}
			this.block.AddLocalExplosion(this.block.goT.position, vel, radius);
		}

		// Token: 0x06000DBC RID: 3516 RVA: 0x0005E824 File Offset: 0x0005CC24
		public bool HasExploded()
		{
			return this.exploded;
		}

		// Token: 0x06000DBD RID: 3517 RVA: 0x0005E82C File Offset: 0x0005CC2C
		public bool HasExpired()
		{
			return this.expired;
		}

		// Token: 0x06000DBE RID: 3518 RVA: 0x0005E834 File Offset: 0x0005CC34
		public void SetExpired()
		{
			this.counter = int.MaxValue;
			this.expired = true;
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x0005E848 File Offset: 0x0005CC48
		public void Update()
		{
		}

		// Token: 0x06000DC0 RID: 3520 RVA: 0x0005E84A File Offset: 0x0005CC4A
		public float GetLifetime()
		{
			return this.lifetime;
		}

		// Token: 0x06000DC1 RID: 3521 RVA: 0x0005E852 File Offset: 0x0005CC52
		public void SetLifetime(float newLifetime)
		{
			this.lifetime = newLifetime;
		}

		// Token: 0x06000DC2 RID: 3522 RVA: 0x0005E85B File Offset: 0x0005CC5B
		public float GetInFlightTime()
		{
			return (float)this.counter * Blocksworld.fixedDeltaTime;
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x0005E86A File Offset: 0x0005CC6A
		public void Deactivate()
		{
			this.block.go.SetActive(false);
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x0005E880 File Offset: 0x0005CC80
		public void Destroy()
		{
			this.chunk.Destroy(false);
			Blocksworld.chunks.Remove(this.chunk);
			Blocksworld.blocksworldCamera.ChunkDirty(this.chunk);
			bool flag = false;
			if (this.block.reloadConnections.Count == 0)
			{
				this.block.goT.position = this.oldLocalPosition;
				this.block.goT.rotation = this.oldLocalRotation;
				this.block.playChunk.AddBlock(this.block);
				if (this.block.playChunk.rb != null)
				{
					this.block.playChunk.rb.isKinematic = this.oldRbWasKinematic;
				}
			}
			else if (this.block.reloadConnections[0] != null && !this.block.reloadConnections[0].broken)
			{
				Chunk chunk = this.block.reloadConnections[0].chunk;
				if (chunk != null && chunk.go != null && this.block.playChunk != null && this.block.playChunk.go != null)
				{
					Vector3 centerOfMass = Vector3.zero;
					if (this.block.playChunk.rb != null)
					{
						centerOfMass = this.block.playChunk.rb.centerOfMass;
					}
					this.block.goT.position = this.block.reloadConnections[0].goT.TransformPoint(this.oldLocalPosition);
					this.block.goT.rotation = this.block.reloadConnections[0].goT.rotation * this.oldLocalRotation;
					this.block.playChunk.AddBlock(this.block);
					if (this.block.playChunk.rb != null)
					{
						this.block.playChunk.rb.centerOfMass = centerOfMass;
					}
				}
			}
			else
			{
				flag = true;
			}
			this.block.go.SetActive(false);
			if (this.block.goShadow != null)
			{
				this.block.goShadow.SetActive(false);
			}
			Blocksworld.blocksworldCamera.SetSingleton(this.block, false);
			if (flag)
			{
				BWSceneManager.RemovePlayBlock(this.block);
			}
		}

		// Token: 0x04000ACA RID: 2762
		public BlockMissile block;

		// Token: 0x04000ACB RID: 2763
		public Chunk chunk;

		// Token: 0x04000ACC RID: 2764
		public Quaternion oldLocalRotation;

		// Token: 0x04000ACD RID: 2765
		public Vector3 oldLocalPosition;

		// Token: 0x04000ACE RID: 2766
		public bool oldRbWasKinematic;

		// Token: 0x04000ACF RID: 2767
		public float burstMultiplier = 1f;

		// Token: 0x04000AD0 RID: 2768
		public float lifetime = 10f;

		// Token: 0x04000AD1 RID: 2769
		public int counter;

		// Token: 0x04000AD2 RID: 2770
		public bool canExplode;

		// Token: 0x04000AD3 RID: 2771
		public bool exploded;

		// Token: 0x04000AD4 RID: 2772
		public bool expired;

		// Token: 0x04000AD5 RID: 2773
		public bool bursting = true;

		// Token: 0x04000AD6 RID: 2774
		public bool broken;

		// Token: 0x04000AD7 RID: 2775
		private const float MIN_EXPLODE_TIME = 1f;

		// Token: 0x04000AD8 RID: 2776
		private const float PROJECTION_TIME = 0.5f;

		// Token: 0x04000AD9 RID: 2777
		private const float PROJECTION_TIME_SQ = 0.25f;
	}
}
