using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200007D RID: 125
	public class BlockBulkyWheel : BlockAbstractWheel
	{
		// Token: 0x06000AC2 RID: 2754 RVA: 0x0004D29C File Offset: 0x0004B69C
		public BlockBulkyWheel(List<List<Tile>> tiles, string xPrefix) : base(tiles, string.Empty, string.Empty)
		{
			Transform transform = this.goT.Find(xPrefix + " Wheel X N");
			Transform transform2 = this.goT.Find(xPrefix + " Wheel X P");
			this.axlesDirty = (this.hasAxles = (transform != null && transform2 != null));
			if (transform != null)
			{
				this.hideAxleN = transform.gameObject;
				this.hideAxleN.SetActive(false);
			}
			if (transform2 != null)
			{
				this.hideAxleP = transform2.gameObject;
				this.hideAxleP.SetActive(false);
			}
		}

		// Token: 0x06000AC3 RID: 2755 RVA: 0x0004D35C File Offset: 0x0004B75C
		public override void Update()
		{
			base.Update();
			if (this.axlesDirty)
			{
				Vector3 position = this.goT.position;
				bool flag = false;
				bool flag2 = false;
				foreach (Block block in this.connections)
				{
					Vector3 position2 = block.goT.position;
					Vector3 lhs = position2 - position;
					float num = Vector3.Dot(lhs, this.goT.right);
					flag = (flag || num < 0f);
					flag2 = (flag2 || num > 0f);
				}
				this.hideAxleN.SetActive(flag);
				this.hideAxleP.SetActive(flag2);
				this.axlesDirty = false;
			}
		}

		// Token: 0x06000AC4 RID: 2756 RVA: 0x0004D444 File Offset: 0x0004B844
		protected override void RegisterPaintChanged(int meshIndex, string paint, string oldPaint)
		{
			if (meshIndex < 3)
			{
				TextureAndPaintBlockRegistry.BlockPaintChanged(this, paint, oldPaint);
			}
			else if (this.hasAxles && ((meshIndex == 3 && this.hideAxleN.activeSelf) || (meshIndex == 4 && this.hideAxleP.activeSelf)))
			{
				TextureAndPaintBlockRegistry.BlockPaintChanged(this, paint, oldPaint);
			}
		}

		// Token: 0x06000AC5 RID: 2757 RVA: 0x0004D4A8 File Offset: 0x0004B8A8
		protected override void RegisterTextureChanged(int meshIndex, string texture, string oldTexture)
		{
			if (meshIndex < 3)
			{
				TextureAndPaintBlockRegistry.BlockTextureChanged(this, texture, oldTexture);
			}
			else if (this.hasAxles && ((meshIndex == 3 && this.hideAxleN.activeSelf) || (meshIndex == 4 && this.hideAxleP.activeSelf)))
			{
				TextureAndPaintBlockRegistry.BlockTextureChanged(this, texture, oldTexture);
			}
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x0004D50A File Offset: 0x0004B90A
		public override void ConnectionsChanged()
		{
			this.axlesDirty = this.hasAxles;
			base.ConnectionsChanged();
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x0004D51E File Offset: 0x0004B91E
		public override bool MoveTo(Vector3 pos)
		{
			this.axlesDirty = this.hasAxles;
			return base.MoveTo(pos);
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x0004D533 File Offset: 0x0004B933
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider, bool forceRescale)
		{
			this.axlesDirty = this.hasAxles;
			return base.ScaleTo(scale, recalculateCollider, forceRescale);
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x0004D54A File Offset: 0x0004B94A
		public override bool RotateTo(Quaternion rot)
		{
			this.axlesDirty = this.hasAxles;
			return base.RotateTo(rot);
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x0004D560 File Offset: 0x0004B960
		public new static void Register()
		{
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.Drive", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDrivingSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Drive), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsTurning), (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).Turn), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.TurnTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveTowardsTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveTowardsTagRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveTowardsTagRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.IsWheelTowardsTag", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsWheelTowardsTag), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.IsDPadAlongWheel", (Block b) => new PredicateSensorDelegate(((BlockAbstractWheel)b).IsDPadAlongWheel), null, new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.TurnAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).TurnAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveAlongDPad", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPad), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveAlongDPadRaw", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).DriveAlongDPadRaw), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.SetAsSpareTire", null, (Block b) => new PredicateActionDelegate(((BlockAbstractWheel)b).SetAsSpareTire), null, null, null);
		}

		// Token: 0x06000ACB RID: 2763 RVA: 0x0004D8BF File Offset: 0x0004BCBF
		public override bool IgnorePaintToIndexInTutorial(int meshIndex)
		{
			return meshIndex == 3 || meshIndex == 4;
		}

		// Token: 0x0400087D RID: 2173
		private bool axlesDirty = true;

		// Token: 0x0400087E RID: 2174
		private bool hasAxles;

		// Token: 0x0400087F RID: 2175
		private GameObject hideAxleN;

		// Token: 0x04000880 RID: 2176
		private GameObject hideAxleP;
	}
}
