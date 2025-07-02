using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class Bunch : ITBox
{
	public static Dictionary<Block, Bunch> blockBunch = new Dictionary<Block, Bunch>();

	public List<Block> blocks;

	private Quaternion rotation = Quaternion.identity;

	public Bunch()
	{
		blocks = new List<Block>();
	}

	public void Add(Block b)
	{
		blocks.Add(b);
		blockBunch[b] = this;
	}

	public void TBoxSnap()
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			Vector3 position = block.GetPosition();
			Vector3 scale = block.go.transform.rotation * block.Scale();
			if (!TBox.OkScalePositionCombination(scale, position))
			{
				Vector3 pos = new Vector3(Mathf.Round(position.x * 2f) * 0.5f, Mathf.Round(position.y * 2f) * 0.5f, Mathf.Round(position.z * 2f) * 0.5f);
				block.MoveTo(pos);
			}
		}
	}

	public void Remove(Block b)
	{
		blocks.Remove(b);
		blockBunch.Remove(b);
	}

	public override string ToString()
	{
		string text = "Bunch[";
		for (int i = 0; i < blocks.Count; i++)
		{
			text += blocks[i].go.name;
			if (i < blocks.Count - 1)
			{
				text += ",";
			}
		}
		return text + "]";
	}

	public Vector3 GetPosition()
	{
		return Util.ComputeCenter(blocks);
	}

	public Quaternion GetRotation()
	{
		return rotation;
	}

	public Vector3 GetScale()
	{
		Bounds bounds = Util.ComputeBounds(blocks);
		return Quaternion.Inverse(rotation) * bounds.size;
	}

	public Bounds GetBounds()
	{
		return Util.ComputeBounds(blocks);
	}

	public Vector3 CanScale()
	{
		return Vector3.zero;
	}

	public bool TBoxMoveTo(Vector3 pos, bool forced = false)
	{
		return MoveTo(pos);
	}

	public bool MoveTo(Vector3 pos)
	{
		if (Util.IsNullVector3(pos))
		{
			return true;
		}
		Vector3 vector = pos - GetPosition();
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			block.MoveTo(block.GetPosition() + vector);
		}
		HashSet<BlockGrouped> hashSet = new HashSet<BlockGrouped>();
		for (int j = 0; j < blocks.Count; j++)
		{
			Block block2 = blocks[j];
			if (block2 is BlockGrouped blockGrouped)
			{
				if (blockGrouped.group != null)
				{
					hashSet.Add(blockGrouped.GetMainBlockInGroup() as BlockGrouped);
				}
				else
				{
					BWLog.Warning("Bunch is trying to move a grouped block that doesn't have a set group");
				}
			}
			else
			{
				block2.BunchMoved();
			}
		}
		foreach (BlockGrouped item in hashSet)
		{
			item.BunchMoved();
		}
		return true;
	}

	public bool TBoxRotateTo(Quaternion rot)
	{
		return RotateTo(rot);
	}

	public bool RotateTo(Quaternion rot)
	{
		if (Quaternion.Angle(rot, rotation) > 5f)
		{
			Quaternion quaternion = rot * Quaternion.Inverse(rotation);
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				block.RotateTo(quaternion * block.GetRotation());
			}
			Vector3 position = GetPosition();
			for (int j = 0; j < blocks.Count; j++)
			{
				Block block2 = blocks[j];
				Vector3 vector = block2.GetPosition() - position;
				block2.MoveTo(quaternion * vector);
			}
			rotation = rot;
			for (int k = 0; k < blocks.Count; k++)
			{
				Block block3 = blocks[k];
				block3.BunchRotated();
			}
		}
		return true;
	}

	public void EnableCollider(bool value)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			block.EnableCollider(value);
		}
	}

	public bool IsColliderEnabled()
	{
		if (blocks.Count >= 1)
		{
			return blocks[0].IsColliderEnabled();
		}
		return false;
	}

	public bool IsColliderHit(Collider other)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.go.GetComponent<Collider>() == other)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null)
	{
		if (exclude == null && blocks.Count > 1)
		{
			exclude = new HashSet<Block>(blocks);
		}
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if (block.IsColliding(terrainOffset, exclude))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsBlock(Block block)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block2 = blocks[i];
			if (block2.ContainsBlock(block))
			{
				return true;
			}
		}
		return false;
	}

	public void IgnoreRaycasts(bool value)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			block.IgnoreRaycasts(value);
		}
	}

	public static float GetModelMass(Block block)
	{
		block.UpdateConnectedCache();
		List<Block> list = Block.connectedCache[block];
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			Block block2 = list[i];
			num += block2.GetMass();
		}
		return num;
	}

	public static float GetModelMassPerType<T>(Block block)
	{
		block.UpdateConnectedCache();
		List<Block> list = Block.connectedCache[block];
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			Block block2 = list[i];
			num += block2.GetMass();
			if (block2 is T)
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			return 0f;
		}
		return num / (float)num2;
	}
}
