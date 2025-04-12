using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000064 RID: 100
	public class BlockAbstractMovingPlatform : BlockAbstractPlatform
	{
		// Token: 0x06000828 RID: 2088 RVA: 0x00039AE8 File Offset: 0x00037EE8
		public BlockAbstractMovingPlatform(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x00039B00 File Offset: 0x00037F00
		public override void Play()
		{
			base.Play();
			this.positionOffset = 0f;
			this.lastPositionOffset = 0f;
			this.targetSteps = 0;
			this.lastTargetSteps = 0;
			Vector3 scale = base.GetScale();
			BoxCollider boxCollider = (BoxCollider)this.go.GetComponent<Collider>();
			boxCollider.size = Vector3.one;
			boxCollider.center = -Vector3.forward * (scale.z - 1f) * 0.5f;
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x00039B88 File Offset: 0x00037F88
		public override void Play2()
		{
			base.Play2();
			if (this.chunkRigidBody != null)
			{
				this.targetPosition = this.GetPlatformPosition();
				Vector3 scale = base.GetScale();
				this.positions[0] = this.targetPosition;
				this.positions[1] = this.targetPosition + this.goT.forward * (scale.z - 1f);
			}
			this.go.GetComponent<Renderer>().enabled = false;
			this.slideFree = false;
			this.didSlideFree = false;
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x00039C30 File Offset: 0x00038030
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.go.GetComponent<Renderer>().enabled = true;
			Vector3 scale = base.GetScale();
			BoxCollider boxCollider = (BoxCollider)this.go.GetComponent<Collider>();
			boxCollider.size = scale;
			boxCollider.center = Vector3.zero;
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x00039C7F File Offset: 0x0003807F
		protected override Vector3 GetPlatformPosition()
		{
			return this.subMeshGameObjects[0].transform.position;
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x00039C98 File Offset: 0x00038098
		protected override Vector3 GetTargetPositionOffset()
		{
			Vector3 vector = this.positions[1] - this.positions[0];
			Vector3 a = Vector3.zero;
			if (vector.sqrMagnitude > 0.0001f)
			{
				a = vector.normalized;
			}
			return a * this.positionOffset;
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x00039CF8 File Offset: 0x000380F8
		protected override bool CanScaleMesh(int meshIndex)
		{
			return meshIndex == 0;
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x00039D0C File Offset: 0x0003810C
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider = false, bool forceRescale = false)
		{
			bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
			Vector3 scale2 = base.GetScale();
			for (int i = 0; i < this.subMeshGameObjects.Count; i++)
			{
				GameObject gameObject = this.subMeshGameObjects[i];
				gameObject.transform.localPosition = new Vector3(0f, 0f, -(scale2.z - 1f) * 0.5f);
			}
			this.UpdateSATVolumes();
			return result;
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x00039D88 File Offset: 0x00038188
		public override void UpdateSATVolumes()
		{
			base.UpdateSATVolumes();
			Vector3 scale = base.GetScale();
			if (scale.z > 1f)
			{
				this.TranslateSATVolumes(-this.goT.forward * (scale.z - 1f) * 0.5f);
			}
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x00039DE5 File Offset: 0x000381E5
		protected override Vector3 CollisionVolumesScale(CollisionMesh[] meshes)
		{
			return Vector3.one;
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x00039DEC File Offset: 0x000381EC
		public override Vector3 GetPlayModeCenter()
		{
			return this.GetPlatformPosition();
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x00039E01 File Offset: 0x00038201
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			if (!permanent && Blocksworld.CurrentState == State.Play)
			{
				meshIndex = ((meshIndex != 0) ? meshIndex : 1);
			}
			return base.PaintTo(paint, permanent, meshIndex);
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x00039E2C File Offset: 0x0003822C
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (!permanent && Blocksworld.CurrentState == State.Play)
			{
				meshIndex = ((meshIndex != 0) ? meshIndex : 1);
			}
			if (meshIndex == 0 && texture != "Glass")
			{
				return TileResultCode.False;
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, force);
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x00039E84 File Offset: 0x00038284
		protected override void UpdateRuntimeInvisible()
		{
			bool flag = false;
			for (int i = 1; i < ((this.childMeshes != null) ? (this.childMeshes.Count + 1) : 1); i++)
			{
				string texture = base.GetTexture(i);
				if (texture != "Invisible")
				{
					flag = true;
					break;
				}
			}
			this.isRuntimeInvisible = !flag;
			this.go.layer = (int)((!this.isTransparent) ? ((!this.isRuntimePhantom) ? this.goLayerAssignment : Layer.Phantom) : Layer.TransparentFX);
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x00039F1F File Offset: 0x0003831F
		public override TileResultCode IsPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!(this.GetPaint(1) == (string)args[0])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x00039F41 File Offset: 0x00038341
		public override TileResultCode IsTexturedTo(ScriptRowExecutionInfo eInfo, object[] args)
		{
			return (!(base.GetTexture(1) == (string)args[0])) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x04000643 RID: 1603
		protected Vector3[] positions = new Vector3[2];

		// Token: 0x04000644 RID: 1604
		protected float positionOffset;

		// Token: 0x04000645 RID: 1605
		protected float lastPositionOffset;

		// Token: 0x04000646 RID: 1606
		protected int targetSteps;

		// Token: 0x04000647 RID: 1607
		protected int lastTargetSteps;

		// Token: 0x04000648 RID: 1608
		protected float maxSpeed;

		// Token: 0x04000649 RID: 1609
		protected const float MIN_SPEED = 0.001f;

		// Token: 0x0400064A RID: 1610
		protected bool slideFree;

		// Token: 0x0400064B RID: 1611
		protected bool didSlideFree;
	}
}
