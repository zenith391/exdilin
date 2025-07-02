using UnityEngine;

namespace Blocks;

public class StabilizerNozzleInfo
{
	public StabilizerNozzleInfo otherNozzle;

	public Vector3 raycastHitPoint;

	public bool foundRaycastHit;

	private Rigidbody rigidBody;

	public BlockStabilizerMode mode;

	private BlockStabilizerMode oldMode;

	public BlockStabilizerMode lastMode;

	public float lastBurstAmount;

	public GameObject flame;

	private ParticleSystem particles;

	private BlockAbstractStabilizer parent;

	private GameObject parentGo;

	private Quaternion rotation;

	private float boostFactor;

	private float iAngleSum;

	private float iVelSum;

	private float angVelErrorSum;

	private float mass;

	private Vector3 worldOrigUp;

	private Vector3 worldOrigRight;

	private Vector3 worldDim;

	private Vector3 worldOrigDim;

	private Vector3 worldOrigPosition;

	private float prevAbsAngle;

	private float prevVelError;

	private float prevAngVelError;

	public int sign = 1;

	private float positionOffset;

	private float prevPositionOffset;

	private float angVelExternal;

	private bool hoverHeightIsSet;

	public float hoverHeight = 3f;

	private bool wentFromRaycastToPosition;

	private float extraForce;

	public bool stabilizing;

	public bool hovering;

	public bool canHover;

	private float hoverHeightMultiplier = 1f;

	public StabilizerNozzleInfo(BlockAbstractStabilizer parent, Quaternion rotation, int sign = 1)
	{
		this.sign = sign;
		this.parent = parent;
		parentGo = parent.go;
		this.rotation = rotation;
		flame = Object.Instantiate(Resources.Load("Blocks/Stabilize Flame")) as GameObject;
		particles = flame.GetComponent<ParticleSystem>();
		particles.enableEmission = false;
		UpdateTransform();
	}

	public void SetMass(float m)
	{
		mass = m;
	}

	public void Play()
	{
		UpdateTransform();
		hoverHeightIsSet = false;
		hoverHeight = 3f;
		hoverHeightMultiplier = 1f;
		wentFromRaycastToPosition = false;
		worldOrigUp = flame.transform.up;
		worldOrigRight = flame.transform.right;
		worldOrigDim = new Vector3(Mathf.Abs(worldOrigUp.x), Mathf.Abs(worldOrigUp.y), Mathf.Abs(worldOrigUp.z));
		worldDim = worldOrigDim;
		Rigidbody component = parentGo.transform.parent.GetComponent<Rigidbody>();
		if ((bool)component)
		{
			Vector3 vector = parentGo.transform.parent.TransformPoint(component.centerOfMass);
			worldOrigPosition = vector;
		}
		flame.SetActive(value: true);
		particles.enableEmission = false;
		mode = BlockStabilizerMode.OFF;
	}

	public void Destroy()
	{
		Object.Destroy(flame);
	}

	public void UpdateTransform()
	{
		Transform transform = flame.transform;
		Transform transform2 = parentGo.transform;
		transform.position = transform2.position;
		transform.rotation = transform2.rotation * rotation;
	}

	public void ResetFrame()
	{
		particles.enableEmission = false;
	}

	public Vector3 FixedUpdate(bool broken = false, bool vanished = false)
	{
		Vector3 vector = default(Vector3);
		if (parentGo == null)
		{
			return vector;
		}
		Transform transform = parentGo.transform.parent;
		if (transform == null)
		{
			return vector;
		}
		if (rigidBody == null)
		{
			rigidBody = transform.GetComponent<Rigidbody>();
		}
		if (rigidBody != null)
		{
			Transform transform2 = flame.transform;
			Vector3 up = transform2.up;
			hovering = false;
			canHover = false;
			float num = Blocksworld.fixedDeltaTime / 0.02f;
			Vector3 worldCenterOfMass = rigidBody.worldCenterOfMass;
			Vector3 position = transform2.position;
			Vector3 vector2 = position - worldCenterOfMass;
			float sqrMagnitude = vector2.sqrMagnitude;
			bool flag = (double)sqrMagnitude > 0.0001;
			if (mode == BlockStabilizerMode.ANG_VEL && flag)
			{
				Vector3 vector3 = Vector3.Cross(vector2, up);
				Vector3 angularVelocity = rigidBody.angularVelocity;
				float magnitude = angularVelocity.magnitude;
				if ((double)magnitude > 0.01)
				{
					Vector3 normalized = angularVelocity.normalized;
					Vector3 normalized2 = vector3.normalized;
					float num2 = Util.FastAngle(normalized, -normalized2);
					float num3 = Vector3.Dot(angularVelocity, -normalized2);
					angVelErrorSum = Mathf.Clamp(angVelErrorSum + num * num3, -3f, 3f);
					float num4 = (num3 - prevAngVelError) / num;
					prevAngVelError = num3;
					float num5 = 40f;
					if (num2 < num5)
					{
						float num6 = (num5 - num2) / num5;
						float num7 = 10f;
						float num8 = 3f;
						float num9 = 0.01f;
						float max = 2f * mass * boostFactor;
						float num10 = Mathf.Clamp(num6 * mass * boostFactor * (num7 * num3 + num8 * num4 + num9 * angVelErrorSum), 0f, max);
						vector += up * num10;
					}
				}
			}
			else if (mode == BlockStabilizerMode.STABILIZE)
			{
				stabilizing = false;
				Vector3 right = transform2.right;
				Vector3 forward = transform2.forward;
				float num11 = Mathf.Sign(Vector3.Dot(right, vector2));
				forward *= num11;
				float num12 = 0f;
				Vector3 lhs = Vector3.up;
				if (Mathf.Abs(worldOrigUp.y) > 0.1f)
				{
					num12 = Util.FastAngle(up, worldOrigUp);
					lhs = Vector3.Cross(up, worldOrigUp);
				}
				else if (Mathf.Abs(worldOrigUp.x) > 0.1f || Mathf.Abs(worldOrigUp.z) > 0.1f)
				{
					num12 = Util.FastAngle(right, worldOrigRight);
					lhs = Vector3.Cross(right, worldOrigRight);
				}
				if ((double)lhs.sqrMagnitude < 0.001)
				{
					lhs = Vector3.zero;
				}
				else
				{
					lhs.Normalize();
				}
				float num13 = Vector3.Dot(lhs, forward);
				if (num13 < 0f)
				{
					num12 = 0f;
				}
				else
				{
					stabilizing = num12 < 30f;
					num12 *= num13;
				}
				float value = (num12 - prevAbsAngle) / num;
				value = Mathf.Clamp(value, -5f, 5f);
				prevAbsAngle = num12;
				float num14 = num12 / 180f;
				float num15 = 5f;
				iAngleSum = Mathf.Clamp(iAngleSum + num * num14, 0f - num15, num15);
				float num16 = 20f;
				float num17 = 4f;
				float num18 = 0.1f;
				float b = 4f * mass * boostFactor;
				float num19 = Mathf.Max(0f, boostFactor * Mathf.Min(mass * (num16 * num14 + value * num17 + iAngleSum * num18), b));
				Vector3 vector4 = num19 * up;
				vector += vector4;
			}
			else if (mode == BlockStabilizerMode.POSITION || mode == BlockStabilizerMode.POSITION_RAYCAST || (mode == BlockStabilizerMode.STABILIZE_PLANE && flag))
			{
				float num20 = Util.FastAngle(worldOrigUp, up);
				float value2 = (num20 - prevAbsAngle) / num;
				value2 = Mathf.Clamp(value2, -5f, 5f);
				prevAbsAngle = num20;
				float num21 = num20 / 180f;
				float num22 = 10f;
				iAngleSum = Mathf.Clamp(iAngleSum + num * num21, 0f - num22, num22);
				Vector3 value3 = Vector3.Cross(vector2, up);
				Vector3 lhs2 = Vector3.Normalize(value3);
				float f = Vector3.Dot(lhs2, worldOrigUp);
				float num23 = Mathf.Abs(f);
				float num24 = 20f;
				float num25 = 4f;
				float num26 = 0.1f;
				float b2 = 4f * mass * boostFactor;
				float num27 = Mathf.Max(0f, boostFactor * Mathf.Min(mass * (num24 * num21 + value2 * num25 + iAngleSum * num26), b2));
				Vector3 vector5 = num27 * up;
				Vector3 lhs3 = Vector3.Cross(lhs2, up);
				float num28 = Vector3.Dot(lhs3, worldOrigUp);
				float num29 = 0.1f;
				if (num23 > num29)
				{
					vector5 *= 1f + num29 - num23;
				}
				if (num28 > 0f)
				{
					vector += vector5;
				}
				Vector3 vector6 = worldCenterOfMass;
				float num30 = 1f;
				if (num20 < 40f && mode == BlockStabilizerMode.POSITION_RAYCAST)
				{
					canHover = true;
					Vector3 vector7 = up;
					bool flag2 = vector7.y <= 0f;
					if (flag2)
					{
						if (!parent.varyingGravity)
						{
							num30 = 0f;
						}
					}
					else
					{
						vector7 *= -1f;
					}
					bool flag3 = false;
					Vector3 vector8 = default(Vector3);
					if (sign == -1)
					{
						RaycastHit[] array = Physics.RaycastAll(position, vector7, 100f);
						Util.SmartSort(array, position);
						RaycastHit[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							RaycastHit raycastHit = array2[i];
							Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
							if (block != null && block.modelBlock != parent.modelBlock)
							{
								flag3 = (foundRaycastHit = true);
							}
							if (flag3)
							{
								vector8 = (raycastHitPoint = raycastHit.point);
								break;
							}
						}
					}
					else
					{
						flag3 = otherNozzle.foundRaycastHit;
						if (flag3)
						{
							vector8 = otherNozzle.raycastHitPoint;
						}
					}
					if (flag3)
					{
						float magnitude2 = (vector8 - position).magnitude;
						wentFromRaycastToPosition = false;
						float num31 = hoverHeight * hoverHeightMultiplier;
						if (!hoverHeightIsSet)
						{
							hoverHeight = magnitude2 + 2f;
							hoverHeightIsSet = true;
						}
						hoverHeight = Mathf.Max(0.1f, hoverHeight + num * 0.4f * positionOffset);
						if (magnitude2 <= num31)
						{
							hovering = true;
						}
						positionOffset = 0f;
						if (flag2 && magnitude2 > num31 && magnitude2 < num31 + 1f)
						{
							worldOrigPosition = vector8 - vector7 * magnitude2;
						}
						else
						{
							worldOrigPosition = vector8 - vector7 * num31;
						}
						mode = BlockStabilizerMode.POSITION;
					}
					else
					{
						if (!wentFromRaycastToPosition)
						{
							worldOrigPosition = worldCenterOfMass;
							wentFromRaycastToPosition = true;
						}
						mode = BlockStabilizerMode.POSITION;
					}
					vector6 = worldOrigPosition;
				}
				if (mode == BlockStabilizerMode.POSITION && num20 < 40f && num30 > 0.001f)
				{
					bool flag4 = (double)Mathf.Abs(positionOffset) > 0.001;
					bool flag5 = (double)Mathf.Abs(prevPositionOffset) > 0.001;
					if (!flag4 && flag5)
					{
						worldOrigPosition = vector6;
					}
					if (num20 < 40f)
					{
						float num32 = 1f - num20 / 40f;
						Vector3 lhs4 = worldOrigPosition - worldCenterOfMass;
						Vector3 vector9 = worldDim * Vector3.Dot(lhs4, worldDim);
						if ((double)vector9.sqrMagnitude > 0.01 || flag4)
						{
							float num33 = 1f;
							if (vector9.magnitude < 1.5f)
							{
								num33 *= vector9.magnitude / 1.5f;
							}
							Vector3 vector10 = ((!flag4) ? (vector9.normalized * 4f * boostFactor * num33) : (positionOffset * worldOrigUp * (0f - (float)sign) * 35f));
							Vector3 velocity = rigidBody.velocity;
							Vector3 lhs5 = vector10 - velocity;
							float num34 = Vector3.Dot(lhs5.normalized, up);
							if (num34 > 0f)
							{
								float num35 = Vector3.Dot(lhs5, up);
								float num36 = 2f;
								iVelSum = Mathf.Clamp(iVelSum + num * num35, 0f - num36, num36);
								float value4 = (num35 - prevVelError) / num;
								value4 = Mathf.Clamp(value4, -5f, 5f);
								prevVelError = num35;
								float num37 = 4f;
								float num38 = 2f;
								float num39 = 0f;
								float max2 = 6f * mass * boostFactor;
								float num40 = num30 * num32 * boostFactor * Mathf.Clamp(mass * (num37 * num35 + value4 * num38 + iVelSum * num39), 0f, max2);
								vector += num40 * up;
							}
							else
							{
								hovering = false;
							}
						}
						else
						{
							hovering = false;
						}
					}
					else
					{
						hovering = false;
					}
				}
			}
			if (mode == BlockStabilizerMode.BURST)
			{
				vector += extraForce * mass * up;
			}
			rigidBody.AddForceAtPosition(vector, position);
			float magnitude3 = vector.magnitude;
			float num41 = num * (magnitude3 / mass);
			if (!vanished && num41 > 0.2f && Random.value < num41 * 0.2f)
			{
				float value5 = Random.value;
				particles.Emit(parentGo.transform.position, rigidBody.velocity - vector * (1.5f + 1.5f * value5) / mass, 0.75f, 0.1f, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			}
			lastBurstAmount = num41 * 0.2f;
		}
		boostFactor = 0f;
		prevPositionOffset = positionOffset;
		positionOffset = 0f;
		angVelExternal = 0f;
		extraForce = 0f;
		lastMode = mode;
		mode = BlockStabilizerMode.OFF;
		return vector;
	}

	public void DrawPlane(Vector3 pos, Vector3 normal, Vector3 right, float size)
	{
		Vector3 vector = Vector3.Cross(normal, right);
		Debug.DrawLine(pos - vector * size, pos + vector * size);
		Debug.DrawLine(pos - right * size, pos + right * size);
	}

	public void Stop()
	{
		flame.SetActive(value: false);
		particles.Clear();
	}

	public void Pause()
	{
		particles.Pause();
	}

	public void Resume()
	{
		particles.Play();
	}

	public void BoostStabilizer()
	{
	}

	public void IncreaseBoost(float boostIncrement = 1f)
	{
		if (mode != oldMode)
		{
			boostFactor = 0f;
		}
		if ((double)boostFactor > 0.9 * (double)boostIncrement)
		{
			boostFactor *= 1.5f * boostIncrement;
		}
		else
		{
			boostFactor = 1f * boostIncrement;
		}
		oldMode = mode;
	}

	public void Stabilize(float analogInput)
	{
		mode = BlockStabilizerMode.STABILIZE;
		IncreaseBoost(analogInput);
	}

	public void StabilizePlane(float analogInput)
	{
		mode = BlockStabilizerMode.STABILIZE_PLANE;
		IncreaseBoost(analogInput);
	}

	public void ControlZeroAngVel(float analogInput)
	{
		mode = BlockStabilizerMode.ANG_VEL;
		IncreaseBoost(analogInput);
	}

	public void ControlPosition(float analogInput, float argument = 1f)
	{
		mode = BlockStabilizerMode.POSITION;
		if ((double)worldOrigDim.y > 0.01)
		{
			mode = BlockStabilizerMode.POSITION_RAYCAST;
			hoverHeightMultiplier = Mathf.Min(1f, analogInput);
			IncreaseBoost(argument);
		}
		else
		{
			IncreaseBoost(analogInput * argument);
		}
	}

	public void ChangePosition(float amount)
	{
		positionOffset += amount;
		mode = BlockStabilizerMode.POSITION;
		if ((double)worldOrigDim.y > 0.01)
		{
			mode = BlockStabilizerMode.POSITION_RAYCAST;
		}
		boostFactor = Mathf.Max(1f, boostFactor);
	}

	public void ChangeAngVel(float amount)
	{
		angVelExternal += amount;
		mode = BlockStabilizerMode.ANG_VEL;
		boostFactor = Mathf.Max(1f, boostFactor);
	}

	public void IncreaseForce(float analogInput)
	{
		mode = BlockStabilizerMode.BURST;
		extraForce += analogInput * 2f;
		boostFactor = Mathf.Max(1f, boostFactor);
	}
}
