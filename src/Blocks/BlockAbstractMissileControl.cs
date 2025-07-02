using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractMissileControl : Block
{
	public const int MISSILE_LABEL_COUNT = 6;

	private HashSet<IMissileLauncher> missileLaunchers = new HashSet<IMissileLauncher>();

	private float lastFireSequenceTime = float.MinValue;

	private int fireSequenceLabel;

	private bool firingSequence;

	public static Predicate predicateFireSequence;

	public static Predicate predicateAllMissilesGone;

	public static Predicate predicateReloadAll;

	public BlockAbstractMissileControl(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateFireSequence = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.FireSequence", null, (Block b) => ((BlockAbstractMissileControl)b).FireSequence, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Delay", "Force" });
		predicateAllMissilesGone = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.AllMissilesGone", (Block b) => ((BlockAbstractMissileControl)b).AllMissilesGone);
		PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.FireLabel", null, (Block b) => ((BlockAbstractMissileControl)b).FireLabel, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "Missile Label", "Force" });
		predicateReloadAll = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.ReloadAll", null, (Block b) => ((BlockAbstractMissileControl)b).ReloadAll);
		PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.ReloadLabel", null, (Block b) => ((BlockAbstractMissileControl)b).ReloadLabel, new Type[1] { typeof(int) }, new string[1] { "Missile Label" });
		PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.TargetTag", null, (Block b) => ((BlockAbstractMissileControl)b).TargetTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Tag Name", "Lock delay" });
		PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.TargetTagLabel", null, (Block b) => ((BlockAbstractMissileControl)b).TargetTagLabel, new Type[2]
		{
			typeof(string),
			typeof(int)
		}, new string[2] { "Tag Name", "Missile Label" });
		Predicate predicate = PredicateRegistry.Add<BlockAbstractMissileControl>("MissileControl.SetBurstTime", null, (Block b) => ((BlockAbstractMissileControl)b).SetBurstTime, new Type[1] { typeof(float) });
		predicate.updatesIconOnArgumentChange = true;
		string[] array = new string[3] { "Missile Control", "Missile Control Model", "GIJ Global Missile Control" };
		string[] array2 = array;
		foreach (string key in array2)
		{
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					FireSequenceTile(),
					Block.WaitTile(1f)
				},
				new List<Tile>
				{
					AllGoneTile(),
					Block.ThenTile(),
					ReloadAllTile()
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles[key] = value;
		}
	}

	public static Tile FireSequenceTile()
	{
		return new Tile(predicateFireSequence, 0.5f, 1f);
	}

	public static Tile AllGoneTile()
	{
		return new Tile(predicateAllMissilesGone);
	}

	public static Tile ReloadAllTile()
	{
		return new Tile(predicateReloadAll);
	}

	public override void Play()
	{
		base.Play();
		lastFireSequenceTime = float.MinValue;
		fireSequenceLabel = 0;
		firingSequence = false;
	}

	public override void Stop(bool resetBlock)
	{
		base.Stop(resetBlock);
		missileLaunchers.Clear();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		firingSequence = false;
	}

	public TileResultCode FireSequence(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 0.5f);
		float floatArg2 = Util.GetFloatArg(args, 1, 1f);
		if (firingSequence)
		{
			return TileResultCode.True;
		}
		firingSequence = true;
		float fixedTime = Time.fixedTime;
		if (fixedTime > lastFireSequenceTime + floatArg)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = fireSequenceLabel; i <= fireSequenceLabel + 6; i++)
			{
				int num = i % 6;
				foreach (IMissileLauncher missileLauncher in missileLaunchers)
				{
					if (missileLauncher.HasLabel(num + 1))
					{
						fireSequenceLabel = num;
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
				foreach (IMissileLauncher missileLauncher2 in missileLaunchers)
				{
					if (missileLauncher2.CanLaunch() && missileLauncher2.HasLabel(fireSequenceLabel + 1))
					{
						missileLauncher2.LaunchMissile(floatArg2);
						flag = true;
					}
				}
				if (flag)
				{
					lastFireSequenceTime = fixedTime;
				}
				fireSequenceLabel = (fireSequenceLabel + 1) % 6;
			}
			else
			{
				fireSequenceLabel = 0;
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode FireLabel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		float floatArg = Util.GetFloatArg(args, 1, 1f);
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
		{
			if (missileLauncher.CanLaunch() && missileLauncher.HasLabel(intArg))
			{
				missileLauncher.LaunchMissile(floatArg);
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode AllMissilesGone(ScriptRowExecutionInfo eInfo, object[] args)
	{
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
		{
			if (!missileLauncher.MissileGone())
			{
				return TileResultCode.False;
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode ReloadAll(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = true;
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
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
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode ReloadLabel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		bool flag = true;
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
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
		if (flag)
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	public TileResultCode TargetTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		float floatArg = Util.GetFloatArg(args, 1, 0.5f);
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
		{
			missileLauncher.AddControllerTargetTag(stringArg, floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode TargetTagLabel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		int intArg = Util.GetIntArg(args, 1, 0);
		float floatArg = Util.GetFloatArg(args, 2, 0.5f);
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
		{
			if (missileLauncher.HasLabel(intArg))
			{
				missileLauncher.AddControllerTargetTag(stringArg, floatArg);
			}
		}
		return TileResultCode.True;
	}

	public TileResultCode SetBurstTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 10f);
		foreach (IMissileLauncher missileLauncher in missileLaunchers)
		{
			missileLauncher.SetGlobalBurstTime(floatArg);
		}
		return TileResultCode.True;
	}

	public void RegisterMissileLauncher(IMissileLauncher ml)
	{
		missileLaunchers.Add(ml);
	}
}
