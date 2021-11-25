using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000086 RID: 134
	public class BlockCrossbow : BlockAbstractBow
	{
		// Token: 0x06000B66 RID: 2918 RVA: 0x00052A18 File Offset: 0x00050E18
		public BlockCrossbow(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000B67 RID: 2919 RVA: 0x00052A91 File Offset: 0x00050E91
		protected override float GetMinTimeBetweenShots()
		{
			return 0.15f;
		}

		// Token: 0x06000B68 RID: 2920 RVA: 0x00052A98 File Offset: 0x00050E98
		protected override Vector3 GetAmmoPositionOffset()
		{
			return this._ammoPositionOffset;
		}

		// Token: 0x06000B69 RID: 2921 RVA: 0x00052AA0 File Offset: 0x00050EA0
		protected override string GetAmmoPrefabName()
		{
			return this._ammoPrefabName;
		}

		// Token: 0x06000B6A RID: 2922 RVA: 0x00052AA8 File Offset: 0x00050EA8
		protected override int GetAmmoSubmeshColorMapping(int i)
		{
			return this._ammoSubmeshColorMapping[i];
		}

		// Token: 0x06000B6B RID: 2923 RVA: 0x00052AB2 File Offset: 0x00050EB2
		protected override string GetSFXForBlocked()
		{
			return this._blockedSFX;
		}

		// Token: 0x06000B6C RID: 2924 RVA: 0x00052ABA File Offset: 0x00050EBA
		protected override string GetSFXForHit()
		{
			return this._hitSFX;
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x00052AC2 File Offset: 0x00050EC2
		protected override string GetSFXForLoad()
		{
			return this._loadSFX;
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x00052ACA File Offset: 0x00050ECA
		protected override string GetSFXForShoot()
		{
			return this._shootSFX;
		}

		// Token: 0x06000B6F RID: 2927 RVA: 0x00052AD4 File Offset: 0x00050ED4
		public new static void Register()
		{
			PredicateRegistry.Add<BlockCrossbow>("Crossbow.Triggered", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Predicate pred = PredicateRegistry.Add<BlockCrossbow>("Crossbow.Load", (Block b) => new PredicateSensorDelegate(((BlockAbstractBow)b).IsLoaded), (Block b) => new PredicateActionDelegate(((BlockAbstractBow)b).Load), null, null, null);
			PredicateRegistry.Add<BlockCrossbow>("Crossbow.Shoot", null, (Block b) => new PredicateActionDelegate(((BlockAbstractBow)b).Shoot), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					new Tile(Block.predicateFirstFrame, new object[0]),
					Block.ThenTile(),
					new Tile(pred, new object[0])
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["RAR Cross Bow"] = value;
		}

		// Token: 0x0400090F RID: 2319
		private const float _minTimeBetweenShots = 0.15f;

		// Token: 0x04000910 RID: 2320
		private readonly Vector3 _ammoPositionOffset = new Vector3(-0.793f, 0.196f, -0.3343f);

		// Token: 0x04000911 RID: 2321
		private readonly string _ammoPrefabName = "Blocks/Prefab RAR Cross Bow Bolt";

		// Token: 0x04000912 RID: 2322
		private readonly int[] _ammoSubmeshColorMapping = new int[]
		{
			1,
			0,
			2
		};

		// Token: 0x04000913 RID: 2323
		private readonly string _blockedSFX = "Arrow Blocked";

		// Token: 0x04000914 RID: 2324
		private readonly string _hitSFX = "Arrow Hit";

		// Token: 0x04000915 RID: 2325
		private readonly string _loadSFX = "Arrow Nock";

		// Token: 0x04000916 RID: 2326
		private readonly string _shootSFX = "Arrow Loose";
	}
}
