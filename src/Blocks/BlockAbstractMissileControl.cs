using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000061 RID: 97
	public class BlockAbstractMissileControl : Block
	{
		// Token: 0x060007E7 RID: 2023 RVA: 0x0003742D File Offset: 0x0003582D
		public BlockAbstractMissileControl(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x0003744C File Offset: 0x0003584C
		public new static void Register()
		{
			BlockAbstractMissileControl.predicateFireSequence = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.FireSequence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).FireSequence), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Delay",
				"Force"
			}, null);
			BlockAbstractMissileControl.predicateAllMissilesGone = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.AllMissilesGone", (Block b) => new PredicateSensorDelegate(((BlockAbstractMissileControl)b).AllMissilesGone), null, null, null, null);
			PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.FireLabel", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).FireLabel), new Type[]
			{
				typeof(int),
				typeof(float)
			}, new string[]
			{
				"Missile Label",
				"Force"
			}, null);
			BlockAbstractMissileControl.predicateReloadAll = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.ReloadAll", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).ReloadAll), null, null, null);
			PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.ReloadLabel", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).ReloadLabel), new Type[]
			{
				typeof(int)
			}, new string[]
			{
				"Missile Label"
			}, null);
			PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.TargetTag", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).TargetTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, new string[]
			{
				"Tag Name",
				"Lock delay"
			}, null);
			PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.TargetTagLabel", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).TargetTagLabel), new Type[]
			{
				typeof(string),
				typeof(int)
			}, new string[]
			{
				"Tag Name",
				"Missile Label"
			}, null);
			Predicate predicate = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.SetBurstTime", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMissileControl)b).SetBurstTime), new Type[]
			{
				typeof(float)
			}, null, null);
			predicate.updatesIconOnArgumentChange = true;
			string[] array = new string[]
			{
				"Missile Control",
				"Missile Control Model",
				"GIJ Global Missile Control"
			};
			foreach (string key in array)
			{
				List<List<Tile>> value = new List<List<Tile>>
				{
					new List<Tile>
					{
						Block.ThenTile(),
						BlockAbstractMissileControl.FireSequenceTile(),
						Block.WaitTile(1f)
					},
					new List<Tile>
					{
						BlockAbstractMissileControl.AllGoneTile(),
						Block.ThenTile(),
						BlockAbstractMissileControl.ReloadAllTile()
					},
					Block.EmptyTileRow()
				};
				Block.defaultExtraTiles[key] = value;
			}
		}

		// Token: 0x060007E9 RID: 2025 RVA: 0x0003779C File Offset: 0x00035B9C
		public static Tile FireSequenceTile()
		{
			return new Tile(BlockAbstractMissileControl.predicateFireSequence, new object[]
			{
				0.5f,
				1f
			});
		}

		// Token: 0x060007EA RID: 2026 RVA: 0x000377C8 File Offset: 0x00035BC8
		public static Tile AllGoneTile()
		{
			return new Tile(BlockAbstractMissileControl.predicateAllMissilesGone, new object[0]);
		}

		// Token: 0x060007EB RID: 2027 RVA: 0x000377DA File Offset: 0x00035BDA
		public static Tile ReloadAllTile()
		{
			return new Tile(BlockAbstractMissileControl.predicateReloadAll, new object[0]);
		}

		// Token: 0x060007EC RID: 2028 RVA: 0x000377EC File Offset: 0x00035BEC
		public override void Play()
		{
			base.Play();
			this.lastFireSequenceTime = float.MinValue;
			this.fireSequenceLabel = 0;
			this.firingSequence = false;
		}

		// Token: 0x060007ED RID: 2029 RVA: 0x0003780D File Offset: 0x00035C0D
		public override void Stop(bool resetBlock)
		{
			base.Stop(resetBlock);
			this.missileLaunchers.Clear();
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x00037821 File Offset: 0x00035C21
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.firingSequence = false;
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x00037830 File Offset: 0x00035C30
		public TileResultCode FireSequence(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0.5f);
			float floatArg2 = Util.GetFloatArg(args, 1, 1f);
			if (this.firingSequence)
			{
				return TileResultCode.True;
			}
			this.firingSequence = true;
			float fixedTime = Time.fixedTime;
			if (fixedTime > this.lastFireSequenceTime + floatArg)
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = this.fireSequenceLabel; i <= this.fireSequenceLabel + 6; i++)
				{
					int num = i % 6;
					foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
					{
						if (missileLauncher.HasLabel(num + 1))
						{
							this.fireSequenceLabel = num;
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
				if (flag2)
				{
					foreach (IMissileLauncher missileLauncher2 in this.missileLaunchers)
					{
						if (missileLauncher2.CanLaunch() && missileLauncher2.HasLabel(this.fireSequenceLabel + 1))
						{
							missileLauncher2.LaunchMissile(floatArg2);
							flag = true;
						}
					}
					if (flag)
					{
						this.lastFireSequenceTime = fixedTime;
					}
					this.fireSequenceLabel = (this.fireSequenceLabel + 1) % 6;
				}
				else
				{
					this.fireSequenceLabel = 0;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x060007F0 RID: 2032 RVA: 0x000379C4 File Offset: 0x00035DC4
		public TileResultCode FireLabel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			float floatArg = Util.GetFloatArg(args, 1, 1f);
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				if (missileLauncher.CanLaunch() && missileLauncher.HasLabel(intArg))
				{
					missileLauncher.LaunchMissile(floatArg);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x00037A50 File Offset: 0x00035E50
		public TileResultCode AllMissilesGone(ScriptRowExecutionInfo eInfo, object[] args)
		{
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				if (!missileLauncher.MissileGone())
				{
					return TileResultCode.False;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x00037ABC File Offset: 0x00035EBC
		public TileResultCode ReloadAll(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = true;
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				if (missileLauncher.CanReload())
				{
					missileLauncher.Reload();
				}
				if (!missileLauncher.IsLoaded())
				{
					flag = false;
				}
			}
			return (!flag) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x060007F3 RID: 2035 RVA: 0x00037B40 File Offset: 0x00035F40
		public TileResultCode ReloadLabel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			bool flag = true;
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				if (missileLauncher.HasLabel(intArg))
				{
					if (missileLauncher.CanReload())
					{
						missileLauncher.Reload();
					}
					if (!missileLauncher.IsLoaded())
					{
						flag = false;
					}
				}
			}
			return (!flag) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x00037BD8 File Offset: 0x00035FD8
		public TileResultCode TargetTag(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			float floatArg = Util.GetFloatArg(args, 1, 0.5f);
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				missileLauncher.AddControllerTargetTag(stringArg, floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x00037C50 File Offset: 0x00036050
		public TileResultCode TargetTagLabel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			int intArg = Util.GetIntArg(args, 1, 0);
			float floatArg = Util.GetFloatArg(args, 2, 0.5f);
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				if (missileLauncher.HasLabel(intArg))
				{
					missileLauncher.AddControllerTargetTag(stringArg, floatArg);
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x00037CE0 File Offset: 0x000360E0
		public TileResultCode SetBurstTime(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 10f);
			foreach (IMissileLauncher missileLauncher in this.missileLaunchers)
			{
				missileLauncher.SetGlobalBurstTime(floatArg);
			}
			return TileResultCode.True;
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x00037D4C File Offset: 0x0003614C
		public void RegisterMissileLauncher(IMissileLauncher ml)
		{
			this.missileLaunchers.Add(ml);
		}

		// Token: 0x0400060A RID: 1546
		public const int MISSILE_LABEL_COUNT = 6;

		// Token: 0x0400060B RID: 1547
		private HashSet<IMissileLauncher> missileLaunchers = new HashSet<IMissileLauncher>();

		// Token: 0x0400060C RID: 1548
		private float lastFireSequenceTime = float.MinValue;

		// Token: 0x0400060D RID: 1549
		private int fireSequenceLabel;

		// Token: 0x0400060E RID: 1550
		private bool firingSequence;

		// Token: 0x0400060F RID: 1551
		public static Predicate predicateFireSequence;

		// Token: 0x04000610 RID: 1552
		public static Predicate predicateAllMissilesGone;

		// Token: 0x04000611 RID: 1553
		public static Predicate predicateReloadAll;
	}
}
