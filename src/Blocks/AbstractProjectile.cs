using UnityEngine;

namespace Blocks;

public abstract class AbstractProjectile
{
	protected class Segment
	{
		public Vector3 Direction;

		public Vector3 Origin;

		public Vector3 Up;

		public float TravelledDist;

		public float Length;

		public float HeadSpeed;

		public float TailSpeed;

		public Vector3 ExtraVelocity;

		public bool ReceivingEnergy;

		public bool Reflected;

		public float TravelledTime;

		public bool IsReflection;

		public float StartFraction;

		public GameObject go;

		public Transform goT;

		public Segment(BlockAbstractLaser sender, Vector3 origin, Vector3 direction, Vector3 up, float speed, Vector3 extraSpeed, GameObject go)
		{
			Origin = origin;
			Direction = direction;
			if (sender != null)
			{
				Vector3 aimAdjustTarget = Blocksworld.blocksworldCamera.GetAimAdjustTarget(sender);
				if (Vector3.zero != aimAdjustTarget)
				{
					Direction = aimAdjustTarget - Origin;
					Direction.Normalize();
				}
			}
			Up = up;
			HeadSpeed = speed;
			TailSpeed = speed;
			ExtraVelocity = extraSpeed;
			this.go = go;
			goT = go.transform;
			goT.rotation = Quaternion.LookRotation(Direction, Up);
			goT.position = origin;
			goT.localScale = Vector3.zero;
			TravelledTime = 0f;
		}

		public void Destroy()
		{
			if (go != null)
			{
				MeshFilter component = go.GetComponent<MeshFilter>();
				if (component != null)
				{
					Object.Destroy(component.mesh);
				}
				Object.Destroy(go);
				go = null;
			}
		}
	}

	protected BlockAbstractLaser _sender;

	protected float kSpeed = 16f;

	protected bool canReflect = true;

	protected bool travelThroughTransparent = true;

	public float EnergyFraction;

	protected float gravityInfluence;

	protected Vector3 extraSpeed;

	public AbstractProjectile(BlockAbstractLaser sender)
	{
		_sender = sender;
		kSpeed *= sender.pulseSpeedMultiplier;
		Transform goT = sender.goT;
		Vector3 up = goT.up;
		extraSpeed = GetExtraSpeed(sender, up, sender.goT.position);
	}

	protected abstract GameObject GetSegmentPrefab();

	protected abstract GameObject CreateSegmentGo();

	protected Vector3 GetSenderPosition()
	{
		Transform goT = _sender.goT;
		Vector3 exitOffset = GetExitOffset();
		Vector3 right = goT.right;
		Vector3 up = goT.up;
		Vector3 forward = goT.forward;
		Vector3 vector = exitOffset.x * right + exitOffset.y * up + exitOffset.z * forward;
		return goT.position + vector;
	}

	protected virtual Vector3 GetExitOffset()
	{
		return _sender.laserExitOffset;
	}

	protected Vector3 GetExtraSpeed(Block sender, Vector3 dir, Vector3 startPos)
	{
		Vector3 result = Vector3.zero;
		Transform parent = sender.goT.parent;
		if (parent != null)
		{
			Rigidbody component = parent.gameObject.GetComponent<Rigidbody>();
			if (component != null && !component.isKinematic)
			{
				Vector3 rhs = startPos - component.worldCenterOfMass;
				Vector3 vector = component.velocity + Vector3.Cross(component.angularVelocity, rhs);
				result = vector;
			}
		}
		return result;
	}

	public virtual bool ShouldBeDestroyed()
	{
		return false;
	}

	public abstract void Destroy();

	public virtual void StopReceivingEnergy()
	{
	}

	public void FixedUpdate()
	{
		StepSegments();
	}

	public virtual void Update()
	{
	}

	protected virtual void StepSegments()
	{
	}

	protected virtual void AddHit(Block block, Vector3 direction, Vector3 point, Vector3 normal)
	{
		BlockAbstractLaser.AddHit(_sender, block);
	}

	protected virtual void UpdateHitEffect(RaycastHit hit, Segment lastSegment)
	{
		_sender.UpdateLaserHitParticles(hit.point, hit.normal, lastSegment.Direction, emitSparks: false);
	}
}
