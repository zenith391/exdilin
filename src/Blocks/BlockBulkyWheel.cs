using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockBulkyWheel : BlockAbstractWheel
{
	private bool axlesDirty = true;

	private bool hasAxles;

	private GameObject hideAxleN;

	private GameObject hideAxleP;

	public BlockBulkyWheel(List<List<Tile>> tiles, string xPrefix)
		: base(tiles, string.Empty, string.Empty)
	{
		Transform transform = goT.Find(xPrefix + " Wheel X N");
		Transform transform2 = goT.Find(xPrefix + " Wheel X P");
		axlesDirty = (hasAxles = transform != null && transform2 != null);
		if (transform != null)
		{
			hideAxleN = transform.gameObject;
			hideAxleN.SetActive(value: false);
		}
		if (transform2 != null)
		{
			hideAxleP = transform2.gameObject;
			hideAxleP.SetActive(value: false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (!axlesDirty)
		{
			return;
		}
		Vector3 position = goT.position;
		bool flag = false;
		bool flag2 = false;
		foreach (Block connection in connections)
		{
			Vector3 position2 = connection.goT.position;
			Vector3 lhs = position2 - position;
			float num = Vector3.Dot(lhs, goT.right);
			flag = flag || num < 0f;
			flag2 = flag2 || num > 0f;
		}
		hideAxleN.SetActive(flag);
		hideAxleP.SetActive(flag2);
		axlesDirty = false;
	}

	protected override void RegisterPaintChanged(int meshIndex, string paint, string oldPaint)
	{
		if (meshIndex < 3)
		{
			TextureAndPaintBlockRegistry.BlockPaintChanged(this, paint, oldPaint);
		}
		else if (hasAxles && ((meshIndex == 3 && hideAxleN.activeSelf) || (meshIndex == 4 && hideAxleP.activeSelf)))
		{
			TextureAndPaintBlockRegistry.BlockPaintChanged(this, paint, oldPaint);
		}
	}

	protected override void RegisterTextureChanged(int meshIndex, string texture, string oldTexture)
	{
		if (meshIndex < 3)
		{
			TextureAndPaintBlockRegistry.BlockTextureChanged(this, texture, oldTexture);
		}
		else if (hasAxles && ((meshIndex == 3 && hideAxleN.activeSelf) || (meshIndex == 4 && hideAxleP.activeSelf)))
		{
			TextureAndPaintBlockRegistry.BlockTextureChanged(this, texture, oldTexture);
		}
	}

	public override void ConnectionsChanged()
	{
		axlesDirty = hasAxles;
		base.ConnectionsChanged();
	}

	public override bool MoveTo(Vector3 pos)
	{
		axlesDirty = hasAxles;
		return base.MoveTo(pos);
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider, bool forceRescale)
	{
		axlesDirty = hasAxles;
		return base.ScaleTo(scale, recalculateCollider, forceRescale);
	}

	public override bool RotateTo(Quaternion rot)
	{
		axlesDirty = hasAxles;
		return base.RotateTo(rot);
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.Drive", (Block b) => ((BlockAbstractWheel)b).IsDrivingSensor, (Block b) => ((BlockAbstractWheel)b).Drive, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.Turn", (Block b) => ((BlockAbstractWheel)b).IsTurning, (Block b) => ((BlockAbstractWheel)b).Turn, new Type[1] { typeof(float) }, new string[1] { "Angle" });
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.TurnTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).TurnTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveTowardsTag", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveTowardsTagRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveTowardsTagRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.IsWheelTowardsTag", (Block b) => ((BlockAbstractWheel)b).IsWheelTowardsTag, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.IsDPadAlongWheel", (Block b) => ((BlockAbstractWheel)b).IsDPadAlongWheel, null, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.TurnAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).TurnAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveAlongDPad", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPad, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.DriveAlongDPadRaw", null, (Block b) => ((BlockAbstractWheel)b).DriveAlongDPadRaw, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBulkyWheel>("BulkyWheel.SetAsSpareTire", null, (Block b) => ((BlockAbstractWheel)b).SetAsSpareTire);
	}

	public override bool IgnorePaintToIndexInTutorial(int meshIndex)
	{
		if (meshIndex != 3)
		{
			return meshIndex == 4;
		}
		return true;
	}
}
