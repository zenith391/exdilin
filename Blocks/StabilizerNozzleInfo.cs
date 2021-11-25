using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200006C RID: 108
	public class StabilizerNozzleInfo
	{
		// Token: 0x060008AF RID: 2223 RVA: 0x0003C6B0 File Offset: 0x0003AAB0
		public StabilizerNozzleInfo(BlockAbstractStabilizer parent, Quaternion rotation, int sign = 1)
		{
			this.sign = sign;
			this.parent = parent;
			this.parentGo = parent.go;
			this.rotation = rotation;
			this.flame = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/Stabilize Flame")) as GameObject);
			this.particles = this.flame.GetComponent<ParticleSystem>();
			this.particles.enableEmission = false;
			this.UpdateTransform();
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x0003C73E File Offset: 0x0003AB3E
		public void SetMass(float m)
		{
			this.mass = m;
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x0003C748 File Offset: 0x0003AB48
		public void Play()
		{
			this.UpdateTransform();
			this.hoverHeightIsSet = false;
			this.hoverHeight = 3f;
			this.hoverHeightMultiplier = 1f;
			this.wentFromRaycastToPosition = false;
			this.worldOrigUp = this.flame.transform.up;
			this.worldOrigRight = this.flame.transform.right;
			this.worldOrigDim = new Vector3(Mathf.Abs(this.worldOrigUp.x), Mathf.Abs(this.worldOrigUp.y), Mathf.Abs(this.worldOrigUp.z));
			this.worldDim = this.worldOrigDim;
			Rigidbody component = this.parentGo.transform.parent.GetComponent<Rigidbody>();
			if (component)
			{
				Vector3 vector = this.parentGo.transform.parent.TransformPoint(component.centerOfMass);
				this.worldOrigPosition = vector;
			}
			this.flame.SetActive(true);
			this.particles.enableEmission = false;
			this.mode = BlockStabilizerMode.OFF;
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x0003C855 File Offset: 0x0003AC55
		public void Destroy()
		{
			UnityEngine.Object.Destroy(this.flame);
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x0003C864 File Offset: 0x0003AC64
		public void UpdateTransform()
		{
			Transform transform = this.flame.transform;
			Transform transform2 = this.parentGo.transform;
			transform.position = transform2.position;
			transform.rotation = transform2.rotation * this.rotation;
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x0003C8AC File Offset: 0x0003ACAC
		public void ResetFrame()
		{
			this.particles.enableEmission = false;
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x0003C8BC File Offset: 0x0003ACBC
		public Vector3 FixedUpdate(bool broken = false, bool vanished = false)
		{
			Vector3 vector = default(Vector3);
			if (this.parentGo == null)
			{
				return vector;
			}
			Transform transform = this.parentGo.transform.parent;
			if (transform == null)
			{
				return vector;
			}
			if (this.rigidBody == null)
			{
				this.rigidBody = transform.GetComponent<Rigidbody>();
			}
			if (this.rigidBody != null)
			{
				Transform transform2 = this.flame.transform;
				Vector3 up = transform2.up;
				this.hovering = false;
				this.canHover = false;
				float num = Blocksworld.fixedDeltaTime / 0.02f;
				Vector3 worldCenterOfMass = this.rigidBody.worldCenterOfMass;
				Vector3 position = transform2.position;
				Vector3 vector2 = position - worldCenterOfMass;
				float sqrMagnitude = vector2.sqrMagnitude;
				bool flag = (double)sqrMagnitude > 0.0001;
				if (this.mode == BlockStabilizerMode.ANG_VEL && flag)
				{
					Vector3 vector3 = Vector3.Cross(vector2, up);
					Vector3 angularVelocity = this.rigidBody.angularVelocity;
					float magnitude = angularVelocity.magnitude;
					if ((double)magnitude > 0.01)
					{
						Vector3 normalized = angularVelocity.normalized;
						Vector3 normalized2 = vector3.normalized;
						float num2 = Util.FastAngle(normalized, -normalized2);
						float num3 = Vector3.Dot(angularVelocity, -normalized2);
						this.angVelErrorSum = Mathf.Clamp(this.angVelErrorSum + num * num3, -3f, 3f);
						float num4 = (num3 - this.prevAngVelError) / num;
						this.prevAngVelError = num3;
						float num5 = 40f;
						if (num2 < num5)
						{
							float num6 = (num5 - num2) / num5;
							float num7 = 10f;
							float num8 = 3f;
							float num9 = 0.01f;
							float max = 2f * this.mass * this.boostFactor;
							float d = Mathf.Clamp(num6 * this.mass * this.boostFactor * (num7 * num3 + num8 * num4 + num9 * this.angVelErrorSum), 0f, max);
							vector += up * d;
						}
					}
				}
				else if (this.mode == BlockStabilizerMode.STABILIZE)
				{
					this.stabilizing = false;
					Vector3 right = transform2.right;
					Vector3 vector4 = transform2.forward;
					float d2 = Mathf.Sign(Vector3.Dot(right, vector2));
					vector4 *= d2;
					float num10 = 0f;
					Vector3 lhs = Vector3.up;
					if (Mathf.Abs(this.worldOrigUp.y) > 0.1f)
					{
						num10 = Util.FastAngle(up, this.worldOrigUp);
						lhs = Vector3.Cross(up, this.worldOrigUp);
					}
					else if (Mathf.Abs(this.worldOrigUp.x) > 0.1f || Mathf.Abs(this.worldOrigUp.z) > 0.1f)
					{
						num10 = Util.FastAngle(right, this.worldOrigRight);
						lhs = Vector3.Cross(right, this.worldOrigRight);
					}
					if ((double)lhs.sqrMagnitude < 0.001)
					{
						lhs = Vector3.zero;
					}
					else
					{
						lhs.Normalize();
					}
					float num11 = Vector3.Dot(lhs, vector4);
					if (num11 < 0f)
					{
						num10 = 0f;
					}
					else
					{
						this.stabilizing = (num10 < 30f);
						num10 *= num11;
					}
					float num12 = (num10 - this.prevAbsAngle) / num;
					num12 = Mathf.Clamp(num12, -5f, 5f);
					this.prevAbsAngle = num10;
					float num13 = num10 / 180f;
					float num14 = 5f;
					this.iAngleSum = Mathf.Clamp(this.iAngleSum + num * num13, -num14, num14);
					float num15 = 20f;
					float num16 = 4f;
					float num17 = 0.1f;
					float b = 4f * this.mass * this.boostFactor;
					float d3 = Mathf.Max(0f, this.boostFactor * Mathf.Min(this.mass * (num15 * num13 + num12 * num16 + this.iAngleSum * num17), b));
					Vector3 b2 = d3 * up;
					vector += b2;
				}
				else if (this.mode == BlockStabilizerMode.POSITION || this.mode == BlockStabilizerMode.POSITION_RAYCAST || (this.mode == BlockStabilizerMode.STABILIZE_PLANE && flag))
				{
					float num18 = Util.FastAngle(this.worldOrigUp, up);
					float num19 = (num18 - this.prevAbsAngle) / num;
					num19 = Mathf.Clamp(num19, -5f, 5f);
					this.prevAbsAngle = num18;
					float num20 = num18 / 180f;
					float num21 = 10f;
					this.iAngleSum = Mathf.Clamp(this.iAngleSum + num * num20, -num21, num21);
					Vector3 value = Vector3.Cross(vector2, up);
					Vector3 lhs2 = Vector3.Normalize(value);
					float f = Vector3.Dot(lhs2, this.worldOrigUp);
					float num22 = Mathf.Abs(f);
					float num23 = 20f;
					float num24 = 4f;
					float num25 = 0.1f;
					float b3 = 4f * this.mass * this.boostFactor;
					float d4 = Mathf.Max(0f, this.boostFactor * Mathf.Min(this.mass * (num23 * num20 + num19 * num24 + this.iAngleSum * num25), b3));
					Vector3 vector5 = d4 * up;
					Vector3 lhs3 = Vector3.Cross(lhs2, up);
					float num26 = Vector3.Dot(lhs3, this.worldOrigUp);
					float num27 = 0.1f;
					if (num22 > num27)
					{
						vector5 *= 1f + num27 - num22;
					}
					if (num26 > 0f)
					{
						vector += vector5;
					}
					Vector3 vector6 = worldCenterOfMass;
					float num28 = 1f;
					if (num18 < 40f && this.mode == BlockStabilizerMode.POSITION_RAYCAST)
					{
						this.canHover = true;
						Vector3 vector7 = up;
						bool flag2 = vector7.y <= 0f;
						if (flag2)
						{
							if (!this.parent.varyingGravity)
							{
								num28 = 0f;
							}
						}
						else
						{
							vector7 *= -1f;
						}
						bool flag3 = false;
						Vector3 a = default(Vector3);
						if (this.sign == -1)
						{
							RaycastHit[] array = Physics.RaycastAll(position, vector7, 100f);
							Util.SmartSort(array, position);
							foreach (RaycastHit raycastHit in array)
							{
								Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
								if (block != null && block.modelBlock != this.parent.modelBlock)
								{
									flag3 = true;
									this.foundRaycastHit = flag3;
								}
								if (flag3)
								{
									a = raycastHit.point;
									this.raycastHitPoint = a;
									break;
								}
							}
						}
						else
						{
							flag3 = this.otherNozzle.foundRaycastHit;
							if (flag3)
							{
								a = this.otherNozzle.raycastHitPoint;
							}
						}
						if (flag3)
						{
							float magnitude2 = (a - position).magnitude;
							this.wentFromRaycastToPosition = false;
							float num29 = this.hoverHeight * this.hoverHeightMultiplier;
							if (!this.hoverHeightIsSet)
							{
								this.hoverHeight = magnitude2 + 2f;
								this.hoverHeightIsSet = true;
							}
							this.hoverHeight = Mathf.Max(0.1f, this.hoverHeight + num * 0.4f * this.positionOffset);
							if (magnitude2 <= num29)
							{
								this.hovering = true;
							}
							this.positionOffset = 0f;
							if (flag2 && magnitude2 > num29 && magnitude2 < num29 + 1f)
							{
								this.worldOrigPosition = a - vector7 * magnitude2;
							}
							else
							{
								this.worldOrigPosition = a - vector7 * num29;
							}
							this.mode = BlockStabilizerMode.POSITION;
						}
						else
						{
							if (!this.wentFromRaycastToPosition)
							{
								this.worldOrigPosition = worldCenterOfMass;
								this.wentFromRaycastToPosition = true;
							}
							this.mode = BlockStabilizerMode.POSITION;
						}
						vector6 = this.worldOrigPosition;
					}
					if (this.mode == BlockStabilizerMode.POSITION && num18 < 40f && num28 > 0.001f)
					{
						bool flag4 = (double)Mathf.Abs(this.positionOffset) > 0.001;
						bool flag5 = (double)Mathf.Abs(this.prevPositionOffset) > 0.001;
						if (!flag4 && flag5)
						{
							this.worldOrigPosition = vector6;
						}
						if (num18 < 40f)
						{
							float num30 = 1f - num18 / 40f;
							Vector3 lhs4 = this.worldOrigPosition - worldCenterOfMass;
							Vector3 vector8 = this.worldDim * Vector3.Dot(lhs4, this.worldDim);
							if ((double)vector8.sqrMagnitude > 0.01 || flag4)
							{
								float num31 = 1f;
								if (vector8.magnitude < 1.5f)
								{
									num31 *= vector8.magnitude / 1.5f;
								}
								Vector3 a2 = (!flag4) ? (vector8.normalized * 4f * this.boostFactor * num31) : (this.positionOffset * this.worldOrigUp * (float)(-(float)this.sign) * 35f);
								Vector3 velocity = this.rigidBody.velocity;
								Vector3 lhs5 = a2 - velocity;
								float num32 = Vector3.Dot(lhs5.normalized, up);
								if (num32 > 0f)
								{
									float num33 = Vector3.Dot(lhs5, up);
									float num34 = 2f;
									this.iVelSum = Mathf.Clamp(this.iVelSum + num * num33, -num34, num34);
									float num35 = (num33 - this.prevVelError) / num;
									num35 = Mathf.Clamp(num35, -5f, 5f);
									this.prevVelError = num33;
									float num36 = 4f;
									float num37 = 2f;
									float num38 = 0f;
									float max2 = 6f * this.mass * this.boostFactor;
									float d5 = num28 * num30 * this.boostFactor * Mathf.Clamp(this.mass * (num36 * num33 + num35 * num37 + this.iVelSum * num38), 0f, max2);
									vector += d5 * up;
								}
								else
								{
									this.hovering = false;
								}
							}
							else
							{
								this.hovering = false;
							}
						}
						else
						{
							this.hovering = false;
						}
					}
				}
				if (this.mode == BlockStabilizerMode.BURST)
				{
					vector += this.extraForce * this.mass * up;
				}
				this.rigidBody.AddForceAtPosition(vector, position);
				float magnitude3 = vector.magnitude;
				float num39 = num * (magnitude3 / this.mass);
				if (!vanished && num39 > 0.2f && UnityEngine.Random.value < num39 * 0.2f)
				{
					float value2 = UnityEngine.Random.value;
					this.particles.Emit(this.parentGo.transform.position, this.rigidBody.velocity - vector * (1.5f + 1.5f * value2) / this.mass, 0.75f, 0.1f, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
				this.lastBurstAmount = num39 * 0.2f;
			}
			this.boostFactor = 0f;
			this.prevPositionOffset = this.positionOffset;
			this.positionOffset = 0f;
			this.angVelExternal = 0f;
			this.extraForce = 0f;
			this.lastMode = this.mode;
			this.mode = BlockStabilizerMode.OFF;
			return vector;
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x0003D45C File Offset: 0x0003B85C
		public void DrawPlane(Vector3 pos, Vector3 normal, Vector3 right, float size)
		{
			Vector3 a = Vector3.Cross(normal, right);
			Debug.DrawLine(pos - a * size, pos + a * size);
			Debug.DrawLine(pos - right * size, pos + right * size);
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x0003D4B3 File Offset: 0x0003B8B3
		public void Stop()
		{
			this.flame.SetActive(false);
			this.particles.Clear();
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x0003D4CC File Offset: 0x0003B8CC
		public void Pause()
		{
			this.particles.Pause();
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x0003D4D9 File Offset: 0x0003B8D9
		public void Resume()
		{
			this.particles.Play();
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x0003D4E6 File Offset: 0x0003B8E6
		public void BoostStabilizer()
		{
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x0003D4E8 File Offset: 0x0003B8E8
		public void IncreaseBoost(float boostIncrement = 1f)
		{
			if (this.mode != this.oldMode)
			{
				this.boostFactor = 0f;
			}
			if ((double)this.boostFactor > 0.9 * (double)boostIncrement)
			{
				this.boostFactor *= 1.5f * boostIncrement;
			}
			else
			{
				this.boostFactor = 1f * boostIncrement;
			}
			this.oldMode = this.mode;
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x0003D55B File Offset: 0x0003B95B
		public void Stabilize(float analogInput)
		{
			this.mode = BlockStabilizerMode.STABILIZE;
			this.IncreaseBoost(analogInput);
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x0003D56B File Offset: 0x0003B96B
		public void StabilizePlane(float analogInput)
		{
			this.mode = BlockStabilizerMode.STABILIZE_PLANE;
			this.IncreaseBoost(analogInput);
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x0003D57B File Offset: 0x0003B97B
		public void ControlZeroAngVel(float analogInput)
		{
			this.mode = BlockStabilizerMode.ANG_VEL;
			this.IncreaseBoost(analogInput);
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x0003D58C File Offset: 0x0003B98C
		public void ControlPosition(float analogInput, float argument = 1f)
		{
			this.mode = BlockStabilizerMode.POSITION;
			if ((double)this.worldOrigDim.y > 0.01)
			{
				this.mode = BlockStabilizerMode.POSITION_RAYCAST;
				this.hoverHeightMultiplier = Mathf.Min(1f, analogInput);
				this.IncreaseBoost(argument);
			}
			else
			{
				this.IncreaseBoost(analogInput * argument);
			}
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0003D5E8 File Offset: 0x0003B9E8
		public void ChangePosition(float amount)
		{
			this.positionOffset += amount;
			this.mode = BlockStabilizerMode.POSITION;
			if ((double)this.worldOrigDim.y > 0.01)
			{
				this.mode = BlockStabilizerMode.POSITION_RAYCAST;
			}
			this.boostFactor = Mathf.Max(1f, this.boostFactor);
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x0003D641 File Offset: 0x0003BA41
		public void ChangeAngVel(float amount)
		{
			this.angVelExternal += amount;
			this.mode = BlockStabilizerMode.ANG_VEL;
			this.boostFactor = Mathf.Max(1f, this.boostFactor);
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x0003D66E File Offset: 0x0003BA6E
		public void IncreaseForce(float analogInput)
		{
			this.mode = BlockStabilizerMode.BURST;
			this.extraForce += analogInput * 2f;
			this.boostFactor = Mathf.Max(1f, this.boostFactor);
		}

		// Token: 0x040006A8 RID: 1704
		public StabilizerNozzleInfo otherNozzle;

		// Token: 0x040006A9 RID: 1705
		public Vector3 raycastHitPoint;

		// Token: 0x040006AA RID: 1706
		public bool foundRaycastHit;

		// Token: 0x040006AB RID: 1707
		private Rigidbody rigidBody;

		// Token: 0x040006AC RID: 1708
		public BlockStabilizerMode mode;

		// Token: 0x040006AD RID: 1709
		private BlockStabilizerMode oldMode;

		// Token: 0x040006AE RID: 1710
		public BlockStabilizerMode lastMode;

		// Token: 0x040006AF RID: 1711
		public float lastBurstAmount;

		// Token: 0x040006B0 RID: 1712
		public GameObject flame;

		// Token: 0x040006B1 RID: 1713
		private ParticleSystem particles;

		// Token: 0x040006B2 RID: 1714
		private BlockAbstractStabilizer parent;

		// Token: 0x040006B3 RID: 1715
		private GameObject parentGo;

		// Token: 0x040006B4 RID: 1716
		private Quaternion rotation;

		// Token: 0x040006B5 RID: 1717
		private float boostFactor;

		// Token: 0x040006B6 RID: 1718
		private float iAngleSum;

		// Token: 0x040006B7 RID: 1719
		private float iVelSum;

		// Token: 0x040006B8 RID: 1720
		private float angVelErrorSum;

		// Token: 0x040006B9 RID: 1721
		private float mass;

		// Token: 0x040006BA RID: 1722
		private Vector3 worldOrigUp;

		// Token: 0x040006BB RID: 1723
		private Vector3 worldOrigRight;

		// Token: 0x040006BC RID: 1724
		private Vector3 worldDim;

		// Token: 0x040006BD RID: 1725
		private Vector3 worldOrigDim;

		// Token: 0x040006BE RID: 1726
		private Vector3 worldOrigPosition;

		// Token: 0x040006BF RID: 1727
		private float prevAbsAngle;

		// Token: 0x040006C0 RID: 1728
		private float prevVelError;

		// Token: 0x040006C1 RID: 1729
		private float prevAngVelError;

		// Token: 0x040006C2 RID: 1730
		public int sign = 1;

		// Token: 0x040006C3 RID: 1731
		private float positionOffset;

		// Token: 0x040006C4 RID: 1732
		private float prevPositionOffset;

		// Token: 0x040006C5 RID: 1733
		private float angVelExternal;

		// Token: 0x040006C6 RID: 1734
		private bool hoverHeightIsSet;

		// Token: 0x040006C7 RID: 1735
		public float hoverHeight = 3f;

		// Token: 0x040006C8 RID: 1736
		private bool wentFromRaycastToPosition;

		// Token: 0x040006C9 RID: 1737
		private float extraForce;

		// Token: 0x040006CA RID: 1738
		public bool stabilizing;

		// Token: 0x040006CB RID: 1739
		public bool hovering;

		// Token: 0x040006CC RID: 1740
		public bool canHover;

		// Token: 0x040006CD RID: 1741
		private float hoverHeightMultiplier = 1f;
	}
}
