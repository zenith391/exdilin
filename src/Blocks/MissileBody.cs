using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class MissileBody : IMissile
{
	public BlockMissile block;

	public Chunk chunk;

	public Quaternion oldLocalRotation;

	public Vector3 oldLocalPosition;

	public bool oldRbWasKinematic;

	public float burstMultiplier = 1f;

	public float lifetime = 10f;

	public int counter;

	public bool canExplode;

	public bool exploded;

	public bool expired;

	public bool bursting = true;

	public bool broken;

	private const float MIN_EXPLODE_TIME = 1f;

	private const float PROJECTION_TIME = 0.5f;

	private const float PROJECTION_TIME_SQ = 0.25f;

	public HashSet<int> GetLabels()
	{
		return block.labels;
	}

	public void Break()
	{
		broken = true;
	}

	public bool IsBroken()
	{
		return broken;
	}

	public bool IsBursting()
	{
		return bursting;
	}

	public bool CanExplode()
	{
		return canExplode;
	}

	private float CalculateMinDistTime(Vector3 posDiff, Vector3 velDiff)
	{
		return (0f - Vector3.Dot(posDiff, velDiff)) / Vector3.Dot(velDiff, velDiff);
	}

	private float CalculatePenalty(Vector3 posDiff, Vector3 velDiff, float t)
	{
		return (posDiff + velDiff * t).magnitude;
	}

	public void FixedUpdate()
	{
		if (exploded)
		{
			return;
		}
		GameObject go = chunk.go;
		if (go == null)
		{
			return;
		}
		float num = (float)counter * Blocksworld.fixedDeltaTime;
		bool flag = !broken;
		if (!broken)
		{
			if (this.block.globalBurstTimeSet)
			{
				flag = num < this.block.globalBurstTime;
			}
			else if (this.block.burstTimeSet)
			{
				flag = num < this.block.burstTime;
			}
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		GameObject go2 = this.block.go;
		Rigidbody component = go.GetComponent<Rigidbody>();
		float mass = component.mass;
		float gravityFraction = this.block.gravityFraction;
		if (gravityFraction < 1f && !broken)
		{
			zero -= Physics.gravity * mass * (1f - gravityFraction);
		}
		Transform transform = go2.transform;
		Vector3 up = transform.up;
		float magnitude = component.inertiaTensor.magnitude;
		bursting = flag;
		if (flag)
		{
			Vector3 position = transform.position;
			float num2 = 20f * burstMultiplier;
			float num3 = mass * num2;
			Vector3 vector = up * num3;
			if (burstMultiplier > 0f && !this.block.vanished && !expired)
			{
				this.block.EmitSmoke(position - up * this.block.smokeOffset, component.velocity - vector.normalized * 6f * burstMultiplier * this.block.smokeSize);
			}
			float num4 = ((this.block.localTargetTag != null) ? this.block.localLockDelay : this.block.controllerLockDelay);
			if (num > num4)
			{
				string text = ((this.block.localTargetTag != null) ? this.block.localTargetTag : this.block.controllerTargetTag);
				if (text != null)
				{
					Vector3 velocity = component.velocity;
					Vector3 pos = position + velocity * 0.5f;
					if (TagManager.TryGetClosestBlockWithTag(text, pos, out var block))
					{
						Vector3 position2 = block.goT.position;
						Vector3 zero3 = Vector3.zero;
						Vector3 posDiff = position2 - position;
						Vector3 vector2 = zero3 - velocity;
						float num5 = Mathf.Min(posDiff.magnitude / 3f, 10f);
						Vector3 vector3 = position2 + vector2 * num5;
						Vector3 normalized = (vector3 - position).normalized;
						float num6 = CalculateMinDistTime(posDiff, vector2);
						bool flag2 = num6 < 0f;
						if (!flag2)
						{
							float num7 = CalculatePenalty(posDiff, vector2, num6);
							Vector3 vector4 = Vector3.zero;
							float num8 = 0.01f;
							for (int i = 0; i < 3; i++)
							{
								Vector3 zero4 = Vector3.zero;
								zero4[i] = num8;
								vector2 = zero3 - (velocity + zero4);
								float num9 = CalculateMinDistTime(posDiff, vector2);
								flag2 = flag2 || num9 < 0f;
								float num10 = CalculatePenalty(posDiff, vector2, num9);
								vector4[i] = (num10 - num7) / num8;
							}
							float magnitude2 = vector4.magnitude;
							if (!flag2 && magnitude2 > 0f)
							{
								float num11 = 2f;
								if (magnitude2 > num11)
								{
									vector4 = vector4.normalized * num11;
								}
								normalized = (up - vector4 * 20f).normalized;
								Vector3 vector5 = (0f - mass) * vector4 * 5f * burstMultiplier;
								zero += vector5;
							}
						}
						normalized = (normalized - Physics.gravity * gravityFraction * 0.05f).normalized;
						float num12 = Vector3.Dot(up, normalized);
						if (num12 < 0.5f)
						{
							float num13 = (num12 + 1f) / 1.5f;
							vector *= num13;
						}
						float a = Vector3.Angle(up, normalized);
						Vector3 vec = Vector3.Cross(up, normalized).normalized * Mathf.Min(a, 70f) * magnitude * 0.07f;
						vec = Util.ProjectOntoPlane(vec, up);
						vec -= component.angularVelocity * magnitude * 0.1f;
						zero2 += vec;
					}
				}
			}
			zero += vector;
			component.AddForce(zero);
			component.AddTorque(zero2);
		}
		else
		{
			Vector3 velocity2 = component.velocity;
			float magnitude3 = velocity2.magnitude;
			if (magnitude3 > 0.01f)
			{
				Vector3 vector6 = velocity2 / magnitude3;
				float a2 = Vector3.Angle(up, vector6);
				Vector3 torque = Vector3.Cross(up, vector6).normalized * Mathf.Min(a2, 70f) * magnitude * magnitude3 * 0.003f;
				component.AddTorque(torque);
			}
		}
		counter++;
		canExplode = num > 1f;
		if (num >= lifetime)
		{
			expired = true;
		}
	}

	public void Explode(float radius)
	{
		exploded = true;
		Vector3 vel = Vector3.zero;
		if (chunk.go != null)
		{
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				vel = rb.velocity;
			}
		}
		block.AddLocalExplosion(block.goT.position, vel, radius);
	}

	public bool HasExploded()
	{
		return exploded;
	}

	public bool HasExpired()
	{
		return expired;
	}

	public void SetExpired()
	{
		counter = int.MaxValue;
		expired = true;
	}

	public void Update()
	{
	}

	public float GetLifetime()
	{
		return lifetime;
	}

	public void SetLifetime(float newLifetime)
	{
		lifetime = newLifetime;
	}

	public float GetInFlightTime()
	{
		return (float)counter * Blocksworld.fixedDeltaTime;
	}

	public void Deactivate()
	{
		block.go.SetActive(value: false);
	}

	public void Destroy()
	{
		this.chunk.Destroy();
		Blocksworld.chunks.Remove(this.chunk);
		Blocksworld.blocksworldCamera.ChunkDirty(this.chunk);
		bool flag = false;
		if (block.reloadConnections.Count == 0)
		{
			block.goT.position = oldLocalPosition;
			block.goT.rotation = oldLocalRotation;
			block.playChunk.AddBlock(block);
			if (block.playChunk.rb != null)
			{
				block.playChunk.rb.isKinematic = oldRbWasKinematic;
			}
		}
		else if (block.reloadConnections[0] != null && !block.reloadConnections[0].broken)
		{
			Chunk chunk = block.reloadConnections[0].chunk;
			if (chunk != null && chunk.go != null && block.playChunk != null && block.playChunk.go != null)
			{
				Vector3 centerOfMass = Vector3.zero;
				if (block.playChunk.rb != null)
				{
					centerOfMass = block.playChunk.rb.centerOfMass;
				}
				block.goT.position = block.reloadConnections[0].goT.TransformPoint(oldLocalPosition);
				block.goT.rotation = block.reloadConnections[0].goT.rotation * oldLocalRotation;
				block.playChunk.AddBlock(block);
				if (block.playChunk.rb != null)
				{
					block.playChunk.rb.centerOfMass = centerOfMass;
				}
			}
		}
		else
		{
			flag = true;
		}
		block.go.SetActive(value: false);
		if (block.goShadow != null)
		{
			block.goShadow.SetActive(value: false);
		}
		Blocksworld.blocksworldCamera.SetSingleton(block, s: false);
		if (flag)
		{
			BWSceneManager.RemovePlayBlock(block);
		}
	}
}
