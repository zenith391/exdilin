using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000BC RID: 188
	public class BlockPiston : Block
	{
		// Token: 0x06000E82 RID: 3714 RVA: 0x00062140 File Offset: 0x00060540
		public BlockPiston(List<List<Tile>> tiles) : base(tiles)
		{
			this.loopName = "Piston Move";
		}

		// Token: 0x06000E83 RID: 3715 RVA: 0x00062160 File Offset: 0x00060560
		public new static void Register()
		{
			Predicate pred = PredicateRegistry.Add<BlockPiston>("Piston.Move", (Block b) => new PredicateSensorDelegate(((BlockPiston)b).IsMovedTo), (Block b) => new PredicateActionDelegate(((BlockPiston)b).Move), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockPiston>("Piston.MoveReturn", null, (Block b) => new PredicateActionDelegate(((BlockPiston)b).MoveReturn), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockPiston>("Piston.Step", null, (Block b) => new PredicateActionDelegate(((BlockPiston)b).Step), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Units",
				"Seconds"
			}, null);
			PredicateRegistry.Add<BlockPiston>("Piston.StepReturn", null, (Block b) => new PredicateActionDelegate(((BlockPiston)b).StepReturn), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Units",
				"Seconds"
			}, null);
			PredicateRegistry.Add<BlockPiston>("Piston.AtOffsetPosition", (Block b) => new PredicateSensorDelegate(((BlockPiston)b).AtOffsetPosition), null, new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Units"
			}, null);
			PredicateRegistry.Add<BlockPiston>("Piston.SetLimit", null, (Block b) => new PredicateActionDelegate(((BlockPiston)b).SetLimit), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Units"
			}, null);
			PredicateRegistry.Add<BlockPiston>("Piston.FreeMove", (Block b) => new PredicateSensorDelegate(((BlockPiston)b).IsFreeMove), (Block b) => new PredicateActionDelegate(((BlockPiston)b).FreeMove), null, null, null);
			Block.defaultExtraTiles["Piston"] = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(pred, new object[]
					{
						1f
					}),
					new Tile(pred, new object[]
					{
						-1f
					})
				},
				Block.EmptyTileRow()
			};
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x00062434 File Offset: 0x00060834
		protected override void FindSubMeshes()
		{
			if (this._pistonBase == null)
			{
				this._pistonBase = this.goT.Find("Piston Base");
				this._pistonShaft = this.goT.Find("Piston Shaft");
				this._pistonTop = this.goT.Find("Piston Top");
			}
			base.FindSubMeshes();
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x0006249C File Offset: 0x0006089C
		public override void UpdateSATVolumes()
		{
			base.UpdateSATVolumes();
			if (!this.skipUpdateSATVolumes)
			{
				Vector3 scale = base.GetScale();
				if (scale.y > 1f)
				{
					Vector3 offset = -this.goT.up * (scale.y - 1f) * 0.5f;
					CollisionVolumes.TranslateMeshes(this.glueMeshes, offset);
				}
			}
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x0006250B File Offset: 0x0006090B
		protected override Vector3 CollisionVolumesScale(CollisionMesh[] meshes)
		{
			if (meshes == this.glueMeshes)
			{
				return Vector3.one;
			}
			return base.CollisionVolumesScale(meshes);
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x00062526 File Offset: 0x00060926
		private void DestroyJoint()
		{
			if (this._joint != null)
			{
				UnityEngine.Object.Destroy(this._joint);
				this._joint = null;
				base.DestroyFakeRigidbodies();
			}
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x00062551 File Offset: 0x00060951
		private void DestroyFakePlunger()
		{
			if (this._fakePlunger != null)
			{
				UnityEngine.Object.Destroy(this._fakePlunger);
				this._fakePlunger = null;
			}
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x00062578 File Offset: 0x00060978
		private void FindOrCreatePlungerRigidbody()
		{
			List<Block> list = base.ConnectionsOfType(2, true);
			if (list.Count == 0)
			{
				this._plunger = (this._fakePlunger = new GameObject(this.go.name + " Fake Plunger"));
				this._fakePlunger.transform.position = this.goT.position;
				Rigidbody rigidbody = this._fakePlunger.AddComponent<Rigidbody>();
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				}
				rigidbody.mass = 0.1f;
				BoxCollider boxCollider = this._fakePlunger.AddComponent<BoxCollider>();
				boxCollider.size = Vector3.one;
				boxCollider.center = this.goT.up * 0.5f * (base.Scale().y - 1f);
			}
			else
			{
				this._plunger = list[0].chunk.go;
			}
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x00062670 File Offset: 0x00060A70
		private void CreatePistonJoint()
		{
			if (this.chunk.go != this._plunger)
			{
				this._joint = this.chunk.go.AddComponent<ConfigurableJoint>();
				this._joint.xMotion = ConfigurableJointMotion.Limited;
				this._joint.yMotion = ConfigurableJointMotion.Locked;
				this._joint.zMotion = ConfigurableJointMotion.Locked;
				this._joint.angularXMotion = ConfigurableJointMotion.Locked;
				this._joint.angularYMotion = ConfigurableJointMotion.Locked;
				this._joint.angularZMotion = ConfigurableJointMotion.Locked;
				this._joint.linearLimit = new SoftJointLimit
				{
					limit = 0.04f
				};
				this._joint.anchor = this.goT.localPosition;
				this._joint.axis = this.goT.up;
				this._joint.connectedBody = this._plunger.GetComponent<Rigidbody>();
				Vector3 connectedAnchor = this._joint.connectedAnchor;
				this._joint.autoConfigureConnectedAnchor = false;
				this._joint.connectedAnchor = connectedAnchor;
			}
		}

		// Token: 0x06000E8B RID: 3723 RVA: 0x0006277C File Offset: 0x00060B7C
		private void CreateColliders()
		{
			this.goT.GetComponent<BoxCollider>().enabled = false;
			this._pistonBase.gameObject.AddComponent<BoxCollider>();
			this._pistonTop.gameObject.AddComponent<BoxCollider>();
			BoxCollider boxCollider = this._pistonShaft.gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(boxCollider.size.x * 0.5f, boxCollider.size.y, boxCollider.size.z * 0.85f);
			this._shaftColliders.Add(boxCollider);
			boxCollider = this._pistonShaft.gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(boxCollider.size.x * 0.7f, boxCollider.size.y, boxCollider.size.z * 0.7f);
			this._shaftColliders.Add(boxCollider);
			boxCollider = this._pistonShaft.gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(boxCollider.size.x * 0.85f, boxCollider.size.y, boxCollider.size.z * 0.5f);
			this._shaftColliders.Add(boxCollider);
		}

		// Token: 0x06000E8C RID: 3724 RVA: 0x000628DC File Offset: 0x00060CDC
		private void DestroyColliders()
		{
			UnityEngine.Object.Destroy(this._pistonBase.gameObject.GetComponent<BoxCollider>());
			UnityEngine.Object.Destroy(this._pistonTop.gameObject.GetComponent<BoxCollider>());
			for (int i = 0; i < this._shaftColliders.Count; i++)
			{
				UnityEngine.Object.Destroy(this._shaftColliders[i]);
			}
			this._shaftColliders.Clear();
			this.goT.GetComponent<BoxCollider>().enabled = true;
		}

		// Token: 0x06000E8D RID: 3725 RVA: 0x0006295C File Offset: 0x00060D5C
		public override void Play()
		{
			base.Play();
			this.FindOrCreatePlungerRigidbody();
			this.CreatePistonJoint();
			this.CreateColliders();
		}

		// Token: 0x06000E8E RID: 3726 RVA: 0x00062976 File Offset: 0x00060D76
		public override void Play2()
		{
			base.Play2();
			base.CreateFakeRigidbodyBetweenJoints();
		}

		// Token: 0x06000E8F RID: 3727 RVA: 0x00062984 File Offset: 0x00060D84
		public override void Stop(bool resetBlock = true)
		{
			this.DestroyColliders();
			this.DestroyJoint();
			base.DestroyFakeRigidbodies();
			this.DestroyFakePlunger();
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			base.Stop(resetBlock);
		}

		// Token: 0x06000E90 RID: 3728 RVA: 0x000629C0 File Offset: 0x00060DC0
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider, bool forceRescale)
		{
			bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
			float d = 0.5f * (scale.y - 1f);
			Vector3 vector = Vector3.down * 0.5f * (scale.y - 1f);
			this._pistonBase.localPosition = Vector3.down * d;
			this._pistonShaft.localPosition = Vector3.down * d;
			this._pistonTop.localPosition = Vector3.up * d;
			float y = (scale.y - 0.5833334f) / 0.416666657f;
			this._pistonShaft.localScale = new Vector3(1f, y, 1f);
			this.goT.GetComponent<BoxCollider>().size = scale;
			this.UpdateSATVolumes();
			return result;
		}

		// Token: 0x06000E91 RID: 3729 RVA: 0x00062A96 File Offset: 0x00060E96
		protected override bool CanScaleMesh(int meshIndex)
		{
			return false;
		}

		// Token: 0x06000E92 RID: 3730 RVA: 0x00062A99 File Offset: 0x00060E99
		public override TileResultCode IsPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return base.boolToTileResult(this.GetPaint(1) == (string)args[0]);
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x00062AB8 File Offset: 0x00060EB8
		public override TileResultCode PaintToAction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 1, 1);
			return this.PaintTo((string)args[0], false, intArg);
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x00062ADE File Offset: 0x00060EDE
		private TileResultCode IsMovedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E95 RID: 3733 RVA: 0x00062AE1 File Offset: 0x00060EE1
		private TileResultCode Move(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x00062AE4 File Offset: 0x00060EE4
		private TileResultCode MoveReturn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x00062AE7 File Offset: 0x00060EE7
		private TileResultCode Step(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x00062AEA File Offset: 0x00060EEA
		private TileResultCode StepReturn(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E99 RID: 3737 RVA: 0x00062AED File Offset: 0x00060EED
		private TileResultCode AtOffsetPosition(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E9A RID: 3738 RVA: 0x00062AF0 File Offset: 0x00060EF0
		private TileResultCode SetLimit(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E9B RID: 3739 RVA: 0x00062AF3 File Offset: 0x00060EF3
		private TileResultCode IsFreeMove(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x00062AF6 File Offset: 0x00060EF6
		private TileResultCode FreeMove(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return TileResultCode.True;
		}

		// Token: 0x04000B45 RID: 2885
		private BlockPiston.PistonState _state;

		// Token: 0x04000B46 RID: 2886
		private GameObject _fakePlunger;

		// Token: 0x04000B47 RID: 2887
		private GameObject _plunger;

		// Token: 0x04000B48 RID: 2888
		private Transform _pistonBase;

		// Token: 0x04000B49 RID: 2889
		private Transform _pistonShaft;

		// Token: 0x04000B4A RID: 2890
		private Transform _pistonTop;

		// Token: 0x04000B4B RID: 2891
		private ConfigurableJoint _joint;

		// Token: 0x04000B4C RID: 2892
		private List<Collider> _shaftColliders = new List<Collider>();

		// Token: 0x04000B4D RID: 2893
		private const int PISTON_BASE_MESH_INDEX = 1;

		// Token: 0x04000B4E RID: 2894
		private const int PISTON_SHAFT_MESH_INDEX = 2;

		// Token: 0x04000B4F RID: 2895
		private const float PISTON_OFFSET_ERROR = 0.04f;

		// Token: 0x020000BD RID: 189
		private enum PistonState
		{
			// Token: 0x04000B5A RID: 2906
			None,
			// Token: 0x04000B5B RID: 2907
			Free,
			// Token: 0x04000B5C RID: 2908
			Move,
			// Token: 0x04000B5D RID: 2909
			Step
		}
	}
}
