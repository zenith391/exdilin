using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000A2 RID: 162
	public class BlockLauncher : Block
	{
		// Token: 0x06000C9F RID: 3231 RVA: 0x00058901 File Offset: 0x00056D01
		public BlockLauncher(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000CA0 RID: 3232 RVA: 0x0005890C File Offset: 0x00056D0C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLauncher>("Launcher.Launch", (Block b) => new PredicateSensorDelegate(((BlockLauncher)b).IsLaunched), (Block b) => new PredicateActionDelegate(((BlockLauncher)b).Launch), new Type[]
			{
				typeof(float)
			}, null, null);
		}

		// Token: 0x06000CA1 RID: 3233 RVA: 0x00058973 File Offset: 0x00056D73
		public override void Play2()
		{
			base.Play2();
			base.CreateFakeRigidbodyBetweenJoints();
		}

		// Token: 0x06000CA2 RID: 3234 RVA: 0x00058984 File Offset: 0x00056D84
		public override void Play()
		{
			base.Play();
			List<Block> list = base.ConnectionsOfType(2, true);
			this.joint = null;
			if (list.Count > 0)
			{
				BWLog.Info("Created joint");
				GameObject go = list[0].chunk.go;
				GameObject gameObject = new GameObject(this.go.name + " Middle");
				gameObject.transform.position = this.go.transform.position + this.go.transform.up;
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				rigidbody.mass = 1f;
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				}
				this.joint = this.chunk.go.AddComponent<ConfigurableJoint>();
				this.joint.anchor = this.go.transform.localPosition;
				this.joint.axis = this.go.transform.up;
				this.joint.xMotion = ConfigurableJointMotion.Locked;
				this.joint.yMotion = ConfigurableJointMotion.Locked;
				this.joint.zMotion = ConfigurableJointMotion.Locked;
				this.joint.angularXMotion = ConfigurableJointMotion.Locked;
				this.joint.angularYMotion = ConfigurableJointMotion.Locked;
				this.joint.angularZMotion = ConfigurableJointMotion.Locked;
				this.joint.connectedBody = rigidbody;
				ConfigurableJoint configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
				configurableJoint.anchor = this.go.transform.localPosition;
				configurableJoint.axis = this.go.transform.up;
				configurableJoint.xMotion = ConfigurableJointMotion.Locked;
				configurableJoint.yMotion = ConfigurableJointMotion.Locked;
				configurableJoint.zMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
				configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
				configurableJoint.connectedBody = go.GetComponent<Rigidbody>();
			}
			this.isLaunched = false;
			this.wasLaunched = false;
			this.launchForce = 0f;
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x00058B69 File Offset: 0x00056F69
		public override void Stop(bool resetBlock = true)
		{
			if (this.joint != null)
			{
				UnityEngine.Object.Destroy(this.joint);
				this.joint = null;
				base.DestroyFakeRigidbodies();
			}
			base.Stop(resetBlock);
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x00058B9C File Offset: 0x00056F9C
		public TileResultCode Launch(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (float)args[0];
			if (this.joint != null && !this.wasLaunched)
			{
				this.joint.xMotion = ConfigurableJointMotion.Free;
				this.joint.yMotion = ConfigurableJointMotion.Free;
				this.joint.zMotion = ConfigurableJointMotion.Free;
				this.joint.angularXMotion = ConfigurableJointMotion.Free;
				this.joint.angularYMotion = ConfigurableJointMotion.Free;
				this.joint.angularZMotion = ConfigurableJointMotion.Free;
				this.launchForce += num;
				this.isLaunched = true;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x00058C2C File Offset: 0x0005702C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.isLaunched && !this.wasLaunched)
			{
				Vector3 vector = this.go.transform.up * this.launchForce;
				Vector3 position = this.go.transform.position + this.go.transform.up * 0.5f;
				this.joint.connectedBody.AddForceAtPosition(vector, position, ForceMode.Force);
				this.go.transform.parent.gameObject.GetComponent<Rigidbody>().AddForceAtPosition(-vector, position, ForceMode.Force);
				if (vector.magnitude > 0.5f)
				{
					base.PlayPositionedSound("Explode", 1f, 1f);
				}
			}
			this.wasLaunched = this.isLaunched;
		}

		// Token: 0x06000CA6 RID: 3238 RVA: 0x00058D0D File Offset: 0x0005710D
		public TileResultCode IsLaunched(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!this.isLaunched) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x040009F7 RID: 2551
		private ConfigurableJoint joint;

		// Token: 0x040009F8 RID: 2552
		private bool isLaunched;

		// Token: 0x040009F9 RID: 2553
		private bool wasLaunched;

		// Token: 0x040009FA RID: 2554
		private float launchForce;
	}
}
