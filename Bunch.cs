using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000101 RID: 257
public class Bunch : ITBox
{
	// Token: 0x06001296 RID: 4758 RVA: 0x00081DA8 File Offset: 0x000801A8
	public Bunch()
	{
		this.blocks = new List<Block>();
	}

	// Token: 0x06001297 RID: 4759 RVA: 0x00081DC6 File Offset: 0x000801C6
	public void Add(Block b)
	{
		this.blocks.Add(b);
		Bunch.blockBunch[b] = this;
	}

	// Token: 0x06001298 RID: 4760 RVA: 0x00081DE0 File Offset: 0x000801E0
	public void TBoxSnap()
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
			Vector3 position = block.GetPosition();
			Vector3 scale = block.go.transform.rotation * block.Scale();
			if (!TBox.OkScalePositionCombination(scale, position, null))
			{
				Vector3 pos = new Vector3(Mathf.Round(position.x * 2f) * 0.5f, Mathf.Round(position.y * 2f) * 0.5f, Mathf.Round(position.z * 2f) * 0.5f);
				block.MoveTo(pos);
			}
		}
	}

	// Token: 0x06001299 RID: 4761 RVA: 0x00081E9E File Offset: 0x0008029E
	public void Remove(Block b)
	{
		this.blocks.Remove(b);
		Bunch.blockBunch.Remove(b);
	}

	// Token: 0x0600129A RID: 4762 RVA: 0x00081EBC File Offset: 0x000802BC
	public override string ToString()
	{
		string str = "Bunch[";
		for (int i = 0; i < this.blocks.Count; i++)
		{
			str += this.blocks[i].go.name;
			if (i < this.blocks.Count - 1)
			{
				str += ",";
			}
		}
		return str + "]";
	}

	// Token: 0x0600129B RID: 4763 RVA: 0x00081F32 File Offset: 0x00080332
	public Vector3 GetPosition()
	{
		return Util.ComputeCenter(this.blocks, false);
	}

	// Token: 0x0600129C RID: 4764 RVA: 0x00081F40 File Offset: 0x00080340
	public Quaternion GetRotation()
	{
		return this.rotation;
	}

	// Token: 0x0600129D RID: 4765 RVA: 0x00081F48 File Offset: 0x00080348
	public Vector3 GetScale()
	{
		Bounds bounds = Util.ComputeBounds(this.blocks);
		return Quaternion.Inverse(this.rotation) * bounds.size;
	}

	// Token: 0x0600129E RID: 4766 RVA: 0x00081F78 File Offset: 0x00080378
	public Bounds GetBounds()
	{
		return Util.ComputeBounds(this.blocks);
	}

	// Token: 0x0600129F RID: 4767 RVA: 0x00081F92 File Offset: 0x00080392
	public Vector3 CanScale()
	{
		return Vector3.zero;
	}

	// Token: 0x060012A0 RID: 4768 RVA: 0x00081F99 File Offset: 0x00080399
	public bool TBoxMoveTo(Vector3 pos, bool forced = false)
	{
		return this.MoveTo(pos);
	}

	// Token: 0x060012A1 RID: 4769 RVA: 0x00081FA4 File Offset: 0x000803A4
	public bool MoveTo(Vector3 pos)
	{
		if (Util.IsNullVector3(pos))
		{
			return true;
		}
		Vector3 b = pos - this.GetPosition();
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
			block.MoveTo(block.GetPosition() + b);
		}
		HashSet<BlockGrouped> hashSet = new HashSet<BlockGrouped>();
		for (int j = 0; j < this.blocks.Count; j++)
		{
			Block block2 = this.blocks[j];
			BlockGrouped blockGrouped = block2 as BlockGrouped;
			if (blockGrouped != null)
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
		foreach (BlockGrouped blockGrouped2 in hashSet)
		{
			blockGrouped2.BunchMoved();
		}
		return true;
	}

	// Token: 0x060012A2 RID: 4770 RVA: 0x000820D4 File Offset: 0x000804D4
	public bool TBoxRotateTo(Quaternion rot)
	{
		return this.RotateTo(rot);
	}

	// Token: 0x060012A3 RID: 4771 RVA: 0x000820E0 File Offset: 0x000804E0
	public bool RotateTo(Quaternion rot)
	{
		if (Quaternion.Angle(rot, this.rotation) > 5f)
		{
			Quaternion lhs = rot * Quaternion.Inverse(this.rotation);
			for (int i = 0; i < this.blocks.Count; i++)
			{
				Block block = this.blocks[i];
				block.RotateTo(lhs * block.GetRotation());
			}
			Vector3 position = this.GetPosition();
			for (int j = 0; j < this.blocks.Count; j++)
			{
				Block block2 = this.blocks[j];
				Vector3 point = block2.GetPosition() - position;
				block2.MoveTo(lhs * point);
			}
			this.rotation = rot;
			for (int k = 0; k < this.blocks.Count; k++)
			{
				Block block3 = this.blocks[k];
				block3.BunchRotated();
			}
		}
		return true;
	}

	// Token: 0x060012A4 RID: 4772 RVA: 0x000821E4 File Offset: 0x000805E4
	public void EnableCollider(bool value)
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
			block.EnableCollider(value);
		}
	}

	// Token: 0x060012A5 RID: 4773 RVA: 0x00082221 File Offset: 0x00080621
	public bool IsColliderEnabled()
	{
		return this.blocks.Count >= 1 && this.blocks[0].IsColliderEnabled();
	}

	// Token: 0x060012A6 RID: 4774 RVA: 0x00082248 File Offset: 0x00080648
	public bool IsColliderHit(Collider other)
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
			if (block.go.GetComponent<Collider>() == other)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060012A7 RID: 4775 RVA: 0x00082298 File Offset: 0x00080698
	public bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null)
	{
		if (exclude == null && this.blocks.Count > 1)
		{
			exclude = new HashSet<Block>(this.blocks);
		}
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
			if (block.IsColliding(terrainOffset, exclude))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060012A8 RID: 4776 RVA: 0x00082304 File Offset: 0x00080704
	public bool ContainsBlock(Block block)
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block2 = this.blocks[i];
			if (block2.ContainsBlock(block))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060012A9 RID: 4777 RVA: 0x0008234C File Offset: 0x0008074C
	public void IgnoreRaycasts(bool value)
	{
		for (int i = 0; i < this.blocks.Count; i++)
		{
			Block block = this.blocks[i];
			block.IgnoreRaycasts(value);
		}
	}

	// Token: 0x060012AA RID: 4778 RVA: 0x0008238C File Offset: 0x0008078C
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

	// Token: 0x060012AB RID: 4779 RVA: 0x000823DC File Offset: 0x000807DC
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

	// Token: 0x04000F03 RID: 3843
	public static Dictionary<Block, Bunch> blockBunch = new Dictionary<Block, Bunch>();

	// Token: 0x04000F04 RID: 3844
	public List<Block> blocks;

	// Token: 0x04000F05 RID: 3845
	private Quaternion rotation = Quaternion.identity;
}
