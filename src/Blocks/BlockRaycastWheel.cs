using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000C2 RID: 194
	public class BlockRaycastWheel : Block
	{
		// Token: 0x06000EF6 RID: 3830 RVA: 0x00065203 File Offset: 0x00063603
		public BlockRaycastWheel(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x00065214 File Offset: 0x00063614
		public new static void Register()
		{
			PredicateRegistry.Add<BlockRaycastWheel>("RaycastWheel.Drive", null, (Block b) => new PredicateActionDelegate(((BlockRaycastWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockRaycastWheel>("RaycastWheel.Turn", null, (Block b) => new PredicateActionDelegate(((BlockRaycastWheel)b).Turn), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("RaycastWheel.Drive", new object[]
			{
				20f
			}), new string[]
			{
				"Raycast Wheel"
			});
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x000652E8 File Offset: 0x000636E8
		public override void Play()
		{
			base.Play();
			this.treatAsVehicleStatus = -1;
			this.speedTarget = 0f;
			this.angleTarget = 0f;
			this.chunkMass = this.chunk.rb.mass;
			float mass = base.GetMass();
			this.origCollider = this.go.GetComponent<Collider>();
			this.origCollider.enabled = false;
			this.wco = new GameObject(this.go.name + " Wheel Collider GO");
			this.wco.transform.position = this.goT.position;
			this.wco.transform.rotation = this.goT.rotation;
			Rigidbody rigidbody = this.wco.AddComponent<Rigidbody>();
			rigidbody.mass = 1f;
			this.wheelCollider = this.wco.AddComponent<WheelCollider>();
			FixedJoint fixedJoint = this.wco.AddComponent<FixedJoint>();
			fixedJoint.connectedBody = this.chunk.rb;
			WheelFrictionCurve sidewaysFriction = this.wheelCollider.sidewaysFriction;
			sidewaysFriction.stiffness = this.chunkMass / 20f;
			this.wheelCollider.sidewaysFriction = sidewaysFriction;
			WheelFrictionCurve forwardFriction = this.wheelCollider.forwardFriction;
			forwardFriction.stiffness = this.chunkMass / 5f;
			this.wheelCollider.forwardFriction = forwardFriction;
			this.wheelCollider.enabled = true;
			this.wheelCollider.suspensionSpring = new JointSpring
			{
				spring = 20f * this.chunkMass,
				damper = 5f * this.chunkMass
			};
			this.wheelCollider.mass = mass;
			float radius = this.GetRadius();
			this.wheelCollider.radius = radius;
			CapsuleCollider capsuleCollider = this.wco.AddComponent<CapsuleCollider>();
			capsuleCollider.direction = 2;
			capsuleCollider.radius = Mathf.Min(radius, 0.3f);
			capsuleCollider.height = capsuleCollider.radius * 0.5f;
			Vector3 center = Vector3.down * 0.3f;
			capsuleCollider.center = center;
			capsuleCollider.material = new PhysicMaterial
			{
				dynamicFriction = 0f,
				staticFriction = 0f
			};
			BWSceneManager.AddBlockMap(this.wco, this);
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x00065537 File Offset: 0x00063937
		public override void Stop(bool resetBlock = true)
		{
			if (this.wco != null)
			{
				BWSceneManager.RemoveBlockMap(this.wco);
				UnityEngine.Object.Destroy(this.wco);
				this.wco = null;
			}
			base.Stop(resetBlock);
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x00065570 File Offset: 0x00063970
		public float GetRadius()
		{
			Vector3 scale = base.GetScale();
			return 0.25f * (scale.y + scale.z);
		}

		// Token: 0x06000EFB RID: 3835 RVA: 0x0006559B File Offset: 0x0006399B
		public override void ResetFrame()
		{
			this.speedTarget = 0f;
			this.angleTarget = 0f;
		}

		// Token: 0x06000EFC RID: 3836 RVA: 0x000655B3 File Offset: 0x000639B3
		public void Drive(float f)
		{
			this.speedTarget += f;
		}

		// Token: 0x06000EFD RID: 3837 RVA: 0x000655C4 File Offset: 0x000639C4
		public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float f = (float)args[0] * eInfo.floatArg;
			this.Drive(f);
			return TileResultCode.True;
		}

		// Token: 0x06000EFE RID: 3838 RVA: 0x000655E9 File Offset: 0x000639E9
		public void Turn(float angleOffset)
		{
			this.angleTarget += angleOffset;
		}

		// Token: 0x06000EFF RID: 3839 RVA: 0x000655FC File Offset: 0x000639FC
		public TileResultCode Turn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0] * Mathf.Min(1f, eInfo.floatArg);
			this.angleTarget += num;
			return TileResultCode.True;
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x00065634 File Offset: 0x00063A34
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.isTreasure)
			{
				return;
			}
			if (this.broken || this.vanished)
			{
				return;
			}
			this.filteredAngle += (this.angleTarget - this.filteredAngle) / 10f;
			this.wheelCollider.motorTorque = 0.1f * this.speedTarget * this.chunkMass;
			this.wheelCollider.steerAngle = this.filteredAngle;
			Vector3 position;
			Quaternion rotation;
			this.wheelCollider.GetWorldPose(out position, out rotation);
			this.goT.position = position;
			this.goT.rotation = rotation;
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x000656E0 File Offset: 0x00063AE0
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x04000B9E RID: 2974
		private int treatAsVehicleStatus = -1;

		// Token: 0x04000B9F RID: 2975
		private Collider origCollider;

		// Token: 0x04000BA0 RID: 2976
		private WheelCollider wheelCollider;

		// Token: 0x04000BA1 RID: 2977
		private float chunkMass;

		// Token: 0x04000BA2 RID: 2978
		private GameObject wco;

		// Token: 0x04000BA3 RID: 2979
		private float filteredAngle;

		// Token: 0x04000BA4 RID: 2980
		private float speedTarget;

		// Token: 0x04000BA5 RID: 2981
		private float angleTarget;

		// Token: 0x04000BA6 RID: 2982
		public GameObject axle;
	}
}
