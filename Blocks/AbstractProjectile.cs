using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000058 RID: 88
	public abstract class AbstractProjectile
	{
		// Token: 0x06000734 RID: 1844 RVA: 0x000314C0 File Offset: 0x0002F8C0
		public AbstractProjectile(BlockAbstractLaser sender)
		{
			this._sender = sender;
			this.kSpeed *= sender.pulseSpeedMultiplier;
			Transform goT = sender.goT;
			Vector3 up = goT.up;
			this.extraSpeed = this.GetExtraSpeed(sender, up, sender.goT.position);
		}

		// Token: 0x06000735 RID: 1845
		protected abstract GameObject GetSegmentPrefab();

		// Token: 0x06000736 RID: 1846
		protected abstract GameObject CreateSegmentGo();

		// Token: 0x06000737 RID: 1847 RVA: 0x00031530 File Offset: 0x0002F930
		protected Vector3 GetSenderPosition()
		{
			Transform goT = this._sender.goT;
			Vector3 exitOffset = this.GetExitOffset();
			Vector3 right = goT.right;
			Vector3 up = goT.up;
			Vector3 forward = goT.forward;
			Vector3 b = exitOffset.x * right + exitOffset.y * up + exitOffset.z * forward;
			return goT.position + b;
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x000315A7 File Offset: 0x0002F9A7
		protected virtual Vector3 GetExitOffset()
		{
			return this._sender.laserExitOffset;
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x000315B4 File Offset: 0x0002F9B4
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

		// Token: 0x0600073A RID: 1850 RVA: 0x0003162C File Offset: 0x0002FA2C
		public virtual bool ShouldBeDestroyed()
		{
			return false;
		}

		// Token: 0x0600073B RID: 1851
		public abstract void Destroy();

		// Token: 0x0600073C RID: 1852 RVA: 0x0003162F File Offset: 0x0002FA2F
		public virtual void StopReceivingEnergy()
		{
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x00031631 File Offset: 0x0002FA31
		public void FixedUpdate()
		{
			this.StepSegments();
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x00031639 File Offset: 0x0002FA39
		public virtual void Update()
		{
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x0003163B File Offset: 0x0002FA3B
		protected virtual void StepSegments()
		{
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x0003163D File Offset: 0x0002FA3D
		protected virtual void AddHit(Block block, Vector3 direction, Vector3 point, Vector3 normal)
		{
			BlockAbstractLaser.AddHit(this._sender, block);
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x0003164B File Offset: 0x0002FA4B
		protected virtual void UpdateHitEffect(RaycastHit hit, AbstractProjectile.Segment lastSegment)
		{
			this._sender.UpdateLaserHitParticles(hit.point, hit.normal, lastSegment.Direction, false, true);
		}

		// Token: 0x0400056C RID: 1388
		protected BlockAbstractLaser _sender;

		// Token: 0x0400056D RID: 1389
		protected float kSpeed = 16f;

		// Token: 0x0400056E RID: 1390
		protected bool canReflect = true;

		// Token: 0x0400056F RID: 1391
		protected bool travelThroughTransparent = true;

		// Token: 0x04000570 RID: 1392
		public float EnergyFraction;

		// Token: 0x04000571 RID: 1393
		protected float gravityInfluence;

		// Token: 0x04000572 RID: 1394
		protected Vector3 extraSpeed;

		// Token: 0x02000059 RID: 89
		protected class Segment
		{
			// Token: 0x06000742 RID: 1858 RVA: 0x00031670 File Offset: 0x0002FA70
			public Segment(BlockAbstractLaser sender, Vector3 origin, Vector3 direction, Vector3 up, float speed, Vector3 extraSpeed, GameObject go)
			{
				this.Origin = origin;
				this.Direction = direction;
				if (sender != null)
				{
					Vector3 aimAdjustTarget = Blocksworld.blocksworldCamera.GetAimAdjustTarget(sender);
					if (Vector3.zero != aimAdjustTarget)
					{
						this.Direction = aimAdjustTarget - this.Origin;
						this.Direction.Normalize();
					}
				}
				this.Up = up;
				this.HeadSpeed = speed;
				this.TailSpeed = speed;
				this.ExtraVelocity = extraSpeed;
				this.go = go;
				this.goT = go.transform;
				this.goT.rotation = Quaternion.LookRotation(this.Direction, this.Up);
				this.goT.position = origin;
				this.goT.localScale = Vector3.zero;
				this.TravelledTime = 0f;
			}

			// Token: 0x06000743 RID: 1859 RVA: 0x00031748 File Offset: 0x0002FB48
			public void Destroy()
			{
				if (this.go != null)
				{
					MeshFilter component = this.go.GetComponent<MeshFilter>();
					if (component != null)
					{
						UnityEngine.Object.Destroy(component.mesh);
					}
					UnityEngine.Object.Destroy(this.go);
					this.go = null;
				}
			}

			// Token: 0x04000573 RID: 1395
			public Vector3 Direction;

			// Token: 0x04000574 RID: 1396
			public Vector3 Origin;

			// Token: 0x04000575 RID: 1397
			public Vector3 Up;

			// Token: 0x04000576 RID: 1398
			public float TravelledDist;

			// Token: 0x04000577 RID: 1399
			public float Length;

			// Token: 0x04000578 RID: 1400
			public float HeadSpeed;

			// Token: 0x04000579 RID: 1401
			public float TailSpeed;

			// Token: 0x0400057A RID: 1402
			public Vector3 ExtraVelocity;

			// Token: 0x0400057B RID: 1403
			public bool ReceivingEnergy;

			// Token: 0x0400057C RID: 1404
			public bool Reflected;

			// Token: 0x0400057D RID: 1405
			public float TravelledTime;

			// Token: 0x0400057E RID: 1406
			public bool IsReflection;

			// Token: 0x0400057F RID: 1407
			public float StartFraction;

			// Token: 0x04000580 RID: 1408
			public GameObject go;

			// Token: 0x04000581 RID: 1409
			public Transform goT;
		}
	}
}
