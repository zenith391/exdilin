using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x0200027E RID: 638
public class UserAvatarSubstitution : BlockSubstitution
{
	// Token: 0x06001DE4 RID: 7652 RVA: 0x000D5301 File Offset: 0x000D3701
	public UserAvatarSubstitution(BlockCharacter block)
	{
		this.placementCharacterBlock = block;
		this.placementBlockster = block;
	}

	// Token: 0x06001DE5 RID: 7653 RVA: 0x000D5317 File Offset: 0x000D3717
	public UserAvatarSubstitution(BlockAnimatedCharacter block)
	{
		this.placementCharacterBlock = block;
		this.placementBlockster = block;
	}

	// Token: 0x06001DE6 RID: 7654 RVA: 0x000D5330 File Offset: 0x000D3730
	public override void ApplySubstitute()
	{
		if (this.placementCharacterBlock == null)
		{
			return;
		}
		this.CreateAvatarBlocks();
		this.placementBlockster.IBlockster_FindAttachments();
		Block block = null;
		Block block2 = null;
		Block block3 = null;
		Block block4 = null;
		Block block5 = null;
		IBlockster blockster = (IBlockster)this.avatarCharacter;
		blockster.IBlockster_FindAttachments();
		block = blockster.IBlockster_BottomAttachment();
		if (UserAvatarSubstitution.BlockIsCharacter(block))
		{
			block = null;
		}
		block2 = blockster.IBlockster_FrontAttachment();
		if (UserAvatarSubstitution.BlockIsCharacter(block2))
		{
			block2 = null;
		}
		block3 = blockster.IBlockster_LeftHandAttachement();
		if (UserAvatarSubstitution.BlockIsCharacter(block3))
		{
			block3 = null;
		}
		block4 = blockster.IBlockster_RightHandAttachment();
		if (UserAvatarSubstitution.BlockIsCharacter(block4))
		{
			block4 = null;
		}
		block5 = blockster.IBlockster_BackAttachment();
		if (UserAvatarSubstitution.BlockIsCharacter(block5))
		{
			block5 = null;
		}
		Vector3 position = this.placementCharacterBlock.GetPosition();
		Quaternion rhs = Quaternion.Inverse(this.avatarCharacter.GetRotation());
		for (int i = this.avatarBlocks.Count - 1; i >= 0; i--)
		{
			Block block6 = this.avatarBlocks[i];
			block6.RotateTo(this.placementCharacterBlock.GetRotation() * rhs * block6.GetRotation());
			block6.MoveTo(this.placementCharacterBlock.goT.TransformPoint(this.avatarBlockOffsets[i]));
		}
		List<Block> list = this.avatarBlocks.FindAll((Block b) => b.ContainsGroupTile());
		list.ForEach(delegate(Block b)
		{
			b.OnBlockGroupReconstructed();
		});
		for (int j = this.avatarBlocks.Count - 1; j >= 0; j--)
		{
			Block block7 = this.avatarBlocks[j];
			bool flag = false;
			if (block7 != this.avatarCharacter)
			{
				Block block8 = this.placementBlockster.IBlockster_BottomAttachment();
				Block block9 = this.placementBlockster.IBlockster_LeftHandAttachement();
				Block block10 = this.placementBlockster.IBlockster_RightHandAttachment();
				Block block11 = this.placementBlockster.IBlockster_BackAttachment();
				flag |= (block7 == block && block8 != null);
				flag |= (block7 == block2);
				flag |= (block7 == block3 && block9 != null);
				flag |= (block7 == block4 && block10 != null);
				flag |= (block7 == block5 && block11 != null);
				if (!flag)
				{
					foreach (Block block12 in this.placementCharacterBlock.connections)
					{
						if (!(block12 is BlockCharacter))
						{
							if (CollisionTest.MultiMeshMeshTest(block7.glueMeshes, block12.glueMeshes, true) || CollisionTest.MultiMeshMeshTest(block7.jointMeshes, block12.jointMeshes, true))
							{
								flag = true;
								break;
							}
						}
					}
				}
				if (!flag && block7.IsColliding(0f, null))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				BWSceneManager.AddBlock(block7);
			}
			else
			{
				this.avatarBlocks.RemoveAt(j);
				block7.Deactivate();
				block7.Destroy();
			}
		}
		foreach (Block block13 in this.avatarBlocks)
		{
			block13.tiles.RemoveRange(2, block13.tiles.Count - 2);
			block13.tiles[1] = Block.EmptyTileRow();
		}
		List<List<Tile>> list2 = Blocksworld.CloneBlockTiles(this.placementCharacterBlock, true, true);
		if (this.avatarCharacter is BlockAnimatedCharacter)
		{
			ProfileBlocksterUtils.ConvertToAnimated(list2);
		}
		else
		{
			ProfileBlocksterUtils.ConvertToNonAnimated(list2);
		}
		this.avatarCharacter.tiles.AddRange(list2);
		this.placementCharacterBlock.Deactivate();
		BWSceneManager.RemoveBlock(this.placementCharacterBlock);
		Block.ClearConnectedCache();
		ConnectednessGraph.Update(this.avatarBlocks);
		List<Block> list3 = ConnectednessGraph.ConnectedComponent(this.avatarCharacter, 3, null, true);
		Util.AddGroupedTilesToBlockList(list3);
		for (int k = this.avatarBlocks.Count - 1; k >= 0; k--)
		{
			Block block14 = this.avatarBlocks[k];
			if (!list3.Contains(block14))
			{
				BWSceneManager.RemoveBlock(block14);
				this.avatarBlocks.RemoveAt(k);
				block14.Destroy();
			}
		}
	}

	// Token: 0x06001DE7 RID: 7655 RVA: 0x000D57E8 File Offset: 0x000D3BE8
	private static bool BlockIsCharacter(Block b)
	{
		return b != null && (b is BlockCharacter || b is BlockAnimatedCharacter);
	}

	// Token: 0x06001DE8 RID: 7656 RVA: 0x000D580C File Offset: 0x000D3C0C
	private static bool BlockIsUnique(Block b)
	{
		string key = (string)b.tiles[0][1].gaf.Args[0];
		return Blocksworld.GetUniqueBlockMap().ContainsKey(key);
	}

	// Token: 0x06001DE9 RID: 7657 RVA: 0x000D5848 File Offset: 0x000D3C48
	public override void RestoreOriginal()
	{
		foreach (Block block in this.avatarBlocks)
		{
			block.Deactivate();
			BWSceneManager.RemoveBlock(block);
			block.Destroy();
		}
		this.placementCharacterBlock.Activate();
		BWSceneManager.AddBlock(this.placementCharacterBlock);
		this.avatarBlocks.Clear();
		this.avatarCharacter = null;
		this.avatarBlockOffsets.Clear();
		Block.ClearConnectedCache();
		ConnectednessGraph.Update(this.placementCharacterBlock);
	}

	// Token: 0x06001DEA RID: 7658 RVA: 0x000D58F4 File Offset: 0x000D3CF4
	private void CreateAvatarBlocks()
	{
		string currentUserAvatarSource = WorldSession.current.GetCurrentUserAvatarSource();
		this.avatarBlocks = this.LoadCharacterFromJson(currentUserAvatarSource);
		Vector3 b2 = new Vector3(51f, 2.5f, 28f);
		float num = float.MaxValue;
		Block block = null;
		List<Block> list = new List<Block>();
		foreach (Block block2 in this.avatarBlocks)
		{
			if (block2 is BlockCharacter || block2 is BlockAnimatedCharacter)
			{
				list.Add(block2);
				float sqrMagnitude = (block2.GetPosition() - b2).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					block = block2;
				}
			}
		}
		this.avatarCharacter = block;
		foreach (Block block3 in list)
		{
			if (block3 != this.avatarCharacter)
			{
				this.avatarBlocks.Remove(block3);
				block3.Destroy();
			}
		}
		for (int i = this.avatarBlocks.Count - 1; i >= 0; i--)
		{
			Block block4 = this.avatarBlocks[i];
			if (UserAvatarSubstitution.BlockIsUnique(block4))
			{
				this.avatarBlocks.RemoveAt(i);
				block4.Destroy();
			}
		}
		foreach (Block block5 in this.avatarBlocks)
		{
			for (int j = block5.connections.Count - 1; j >= 0; j--)
			{
				if (!this.avatarBlocks.Contains(block5.connections[j]))
				{
					block5.connections.RemoveAt(j);
					block5.connectionTypes.RemoveAt(j);
				}
			}
		}
		List<Block> blocks = this.avatarBlocks.FindAll((Block b) => b.ContainsGroupTile());
		BlockGroups.GatherBlockGroups(blocks);
		List<Block> list2 = ConnectednessGraph.ConnectedComponent(this.avatarCharacter, 3, null, true);
		Util.AddGroupedTilesToBlockList(list2);
		for (int k = this.avatarBlocks.Count - 1; k >= 0; k--)
		{
			Block block6 = this.avatarBlocks[k];
			if (!list2.Contains(block6))
			{
				this.avatarBlocks.RemoveAt(k);
				block6.Destroy();
			}
		}
		this.avatarBlockOffsets = new List<Vector3>();
		for (int l = 0; l < this.avatarBlocks.Count; l++)
		{
			Block block7 = this.avatarBlocks[l];
			Vector3 item = this.avatarCharacter.goT.InverseTransformPoint(block7.GetPosition());
			this.avatarBlockOffsets.Add(item);
		}
	}

	// Token: 0x06001DEB RID: 7659 RVA: 0x000D5C2C File Offset: 0x000D402C
	private List<Block> LoadCharacterFromJson(string avatarSourceJsonStr)
	{
		JObject jobject = JSONDecoder.Decode(avatarSourceJsonStr);
		if (jobject == null || !jobject.ContainsKey("avatar"))
		{
			Debug.Log("Failed to parse avatar json");
			return null;
		}
		List<JObject> arrayValue = jobject["avatar"].ArrayValue;
		List<Block> list = new List<Block>();
		foreach (JObject obj in arrayValue)
		{
			List<List<Tile>> list2 = Blocksworld.bw.LoadJSONTiles(Blocksworld.GetTileRows(obj));
			string text = (string)list2[0][1].gaf.Args[0];
			if (text == "Character Avatar")
			{
				text = "Character Male";
				list2[0][1].gaf.Args[0] = text;
			}
			Block item = Block.NewBlock(list2, false, false);
			list.Add(item);
		}
		int num = 0;
		if (jobject.ContainsKey("connections"))
		{
			foreach (JObject jobject2 in jobject["connections"].ArrayValue)
			{
				foreach (JObject obj2 in jobject2.ArrayValue)
				{
					list[num].connections.Add(list[(int)obj2]);
				}
				num++;
			}
		}
		if (jobject.ContainsKey("connectionTypes"))
		{
			num = 0;
			foreach (JObject jobject3 in jobject["connectionTypes"].ArrayValue)
			{
				foreach (JObject obj3 in jobject3.ArrayValue)
				{
					list[num].connectionTypes.Add((int)obj3);
				}
				num++;
			}
		}
		return list;
	}

	// Token: 0x0400183C RID: 6204
	private Block placementCharacterBlock;

	// Token: 0x0400183D RID: 6205
	private IBlockster placementBlockster;

	// Token: 0x0400183E RID: 6206
	private List<Block> avatarBlocks;

	// Token: 0x0400183F RID: 6207
	private Block avatarCharacter;

	// Token: 0x04001840 RID: 6208
	private List<Vector3> avatarBlockOffsets;
}
