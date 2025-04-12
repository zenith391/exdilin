using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200007C RID: 124
	public class BlockBow : BlockAbstractBow
	{
		// Token: 0x06000AB4 RID: 2740 RVA: 0x0004D0CC File Offset: 0x0004B4CC
		public BlockBow(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x0004D145 File Offset: 0x0004B545
		protected override float GetMinTimeBetweenShots()
		{
			return 0.33f;
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x0004D14C File Offset: 0x0004B54C
		protected override Vector3 GetAmmoPositionOffset()
		{
			return this._ammoPositionOffset;
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x0004D154 File Offset: 0x0004B554
		protected override string GetAmmoPrefabName()
		{
			return this._ammoPrefabName;
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x0004D15C File Offset: 0x0004B55C
		protected override int GetAmmoSubmeshColorMapping(int i)
		{
			return this._ammoSubmeshColorMapping[i];
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x0004D166 File Offset: 0x0004B566
		protected override string GetSFXForBlocked()
		{
			return this._blockedSFX;
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x0004D16E File Offset: 0x0004B56E
		protected override string GetSFXForHit()
		{
			return this._hitSFX;
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x0004D176 File Offset: 0x0004B576
		protected override string GetSFXForLoad()
		{
			return this._loadSFX;
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x0004D17E File Offset: 0x0004B57E
		protected override string GetSFXForShoot()
		{
			return this._shootSFX;
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x0004D188 File Offset: 0x0004B588
		public new static void Register()
		{
			PredicateRegistry.Add<BlockBow>("Bow.Loosed", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			PredicateRegistry.Add<BlockBow>("Bow.Nock", (Block b) => new PredicateSensorDelegate(((BlockAbstractBow)b).IsLoaded), (Block b) => new PredicateActionDelegate(((BlockAbstractBow)b).Load), null, null, null);
			PredicateRegistry.Add<BlockBow>("Bow.Lose", null, (Block b) => new PredicateActionDelegate(((BlockAbstractBow)b).Shoot), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
		}

		// Token: 0x04000871 RID: 2161
		private const float _minTimeBetweenShots = 0.33f;

		// Token: 0x04000872 RID: 2162
		private readonly Vector3 _ammoPositionOffset = new Vector3(-0.555f, 0.018f, -0.332f);

		// Token: 0x04000873 RID: 2163
		private readonly string _ammoPrefabName = "Blocks/Prefab SAM Arrow";

		// Token: 0x04000874 RID: 2164
		private readonly int[] _ammoSubmeshColorMapping = new int[]
		{
			0,
			1,
			2
		};

		// Token: 0x04000875 RID: 2165
		private readonly string _blockedSFX = "Arrow Blocked";

		// Token: 0x04000876 RID: 2166
		private readonly string _hitSFX = "Arrow Hit";

		// Token: 0x04000877 RID: 2167
		private readonly string _loadSFX = "Arrow Nock";

		// Token: 0x04000878 RID: 2168
		private readonly string _shootSFX = "Arrow Loose";
	}
}
