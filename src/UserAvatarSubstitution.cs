using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class UserAvatarSubstitution : BlockSubstitution
{
	private Block placementCharacterBlock;

	private IBlockster placementBlockster;

	private List<Block> avatarBlocks;

	private Block avatarCharacter;

	private List<Vector3> avatarBlockOffsets;

	public UserAvatarSubstitution(BlockCharacter block)
	{
		placementCharacterBlock = block;
		placementBlockster = block;
	}

	public UserAvatarSubstitution(BlockAnimatedCharacter block)
	{
		placementCharacterBlock = block;
		placementBlockster = block;
	}

	public override void ApplySubstitute()
	{
		if (placementCharacterBlock == null)
		{
			return;
		}
		CreateAvatarBlocks();
		placementBlockster.IBlockster_FindAttachments();
		Block block = null;
		Block block2 = null;
		Block block3 = null;
		Block block4 = null;
		Block block5 = null;
		IBlockster blockster = (IBlockster)avatarCharacter;
		blockster.IBlockster_FindAttachments();
		block = blockster.IBlockster_BottomAttachment();
		if (BlockIsCharacter(block))
		{
			block = null;
		}
		block2 = blockster.IBlockster_FrontAttachment();
		if (BlockIsCharacter(block2))
		{
			block2 = null;
		}
		block3 = blockster.IBlockster_LeftHandAttachement();
		if (BlockIsCharacter(block3))
		{
			block3 = null;
		}
		block4 = blockster.IBlockster_RightHandAttachment();
		if (BlockIsCharacter(block4))
		{
			block4 = null;
		}
		block5 = blockster.IBlockster_BackAttachment();
		if (BlockIsCharacter(block5))
		{
			block5 = null;
		}
		Vector3 position = placementCharacterBlock.GetPosition();
		Quaternion quaternion = Quaternion.Inverse(avatarCharacter.GetRotation());
		for (int num = avatarBlocks.Count - 1; num >= 0; num--)
		{
			Block block6 = avatarBlocks[num];
			block6.RotateTo(placementCharacterBlock.GetRotation() * quaternion * block6.GetRotation());
			block6.MoveTo(placementCharacterBlock.goT.TransformPoint(avatarBlockOffsets[num]));
		}
		List<Block> list = avatarBlocks.FindAll((Block b) => b.ContainsGroupTile());
		list.ForEach(delegate(Block b)
		{
			b.OnBlockGroupReconstructed();
		});
		for (int num2 = avatarBlocks.Count - 1; num2 >= 0; num2--)
		{
			Block block7 = avatarBlocks[num2];
			bool flag = false;
			if (block7 != avatarCharacter)
			{
				Block block8 = placementBlockster.IBlockster_BottomAttachment();
				Block block9 = placementBlockster.IBlockster_LeftHandAttachement();
				Block block10 = placementBlockster.IBlockster_RightHandAttachment();
				Block block11 = placementBlockster.IBlockster_BackAttachment();
				flag = flag || (block7 == block && block8 != null);
				flag = flag || block7 == block2;
				flag = flag || (block7 == block3 && block9 != null);
				flag = flag || (block7 == block4 && block10 != null);
				flag = flag || (block7 == block5 && block11 != null);
				if (!flag)
				{
					foreach (Block connection in placementCharacterBlock.connections)
					{
						if (!(connection is BlockCharacter) && (CollisionTest.MultiMeshMeshTest(block7.glueMeshes, connection.glueMeshes, draw: true) || CollisionTest.MultiMeshMeshTest(block7.jointMeshes, connection.jointMeshes, draw: true)))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag && block7.IsColliding())
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
				avatarBlocks.RemoveAt(num2);
				block7.Deactivate();
				block7.Destroy();
			}
		}
		foreach (Block avatarBlock in avatarBlocks)
		{
			avatarBlock.tiles.RemoveRange(2, avatarBlock.tiles.Count - 2);
			avatarBlock.tiles[1] = Block.EmptyTileRow();
		}
		List<List<Tile>> list2 = Blocksworld.CloneBlockTiles(placementCharacterBlock, excludeFirstRow: true, ignoreLockedGroupTiles: true);
		if (avatarCharacter is BlockAnimatedCharacter)
		{
			ProfileBlocksterUtils.ConvertToAnimated(list2);
		}
		else
		{
			ProfileBlocksterUtils.ConvertToNonAnimated(list2);
		}
		avatarCharacter.tiles.AddRange(list2);
		placementCharacterBlock.Deactivate();
		BWSceneManager.RemoveBlock(placementCharacterBlock);
		Block.ClearConnectedCache();
		ConnectednessGraph.Update(avatarBlocks);
		List<Block> list3 = ConnectednessGraph.ConnectedComponent(avatarCharacter, 3);
		Util.AddGroupedTilesToBlockList(list3);
		for (int num3 = avatarBlocks.Count - 1; num3 >= 0; num3--)
		{
			Block block12 = avatarBlocks[num3];
			if (!list3.Contains(block12))
			{
				BWSceneManager.RemoveBlock(block12);
				avatarBlocks.RemoveAt(num3);
				block12.Destroy();
			}
		}
	}

	private static bool BlockIsCharacter(Block b)
	{
		if (b != null)
		{
			if (!(b is BlockCharacter))
			{
				return b is BlockAnimatedCharacter;
			}
			return true;
		}
		return false;
	}

	private static bool BlockIsUnique(Block b)
	{
		string key = (string)b.tiles[0][1].gaf.Args[0];
		return Blocksworld.GetUniqueBlockMap().ContainsKey(key);
	}

	public override void RestoreOriginal()
	{
		foreach (Block avatarBlock in avatarBlocks)
		{
			avatarBlock.Deactivate();
			BWSceneManager.RemoveBlock(avatarBlock);
			avatarBlock.Destroy();
		}
		placementCharacterBlock.Activate();
		BWSceneManager.AddBlock(placementCharacterBlock);
		avatarBlocks.Clear();
		avatarCharacter = null;
		avatarBlockOffsets.Clear();
		Block.ClearConnectedCache();
		ConnectednessGraph.Update(placementCharacterBlock);
	}

	private void CreateAvatarBlocks()
	{
		string currentUserAvatarSource = WorldSession.current.GetCurrentUserAvatarSource();
		avatarBlocks = LoadCharacterFromJson(currentUserAvatarSource);
		Vector3 vector = new Vector3(51f, 2.5f, 28f);
		float num = float.MaxValue;
		Block block = null;
		List<Block> list = new List<Block>();
		foreach (Block avatarBlock in avatarBlocks)
		{
			if (avatarBlock is BlockCharacter || avatarBlock is BlockAnimatedCharacter)
			{
				list.Add(avatarBlock);
				float sqrMagnitude = (avatarBlock.GetPosition() - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					block = avatarBlock;
				}
			}
		}
		avatarCharacter = block;
		foreach (Block item2 in list)
		{
			if (item2 != avatarCharacter)
			{
				avatarBlocks.Remove(item2);
				item2.Destroy();
			}
		}
		for (int num2 = avatarBlocks.Count - 1; num2 >= 0; num2--)
		{
			Block block2 = avatarBlocks[num2];
			if (BlockIsUnique(block2))
			{
				avatarBlocks.RemoveAt(num2);
				block2.Destroy();
			}
		}
		foreach (Block avatarBlock2 in avatarBlocks)
		{
			for (int num3 = avatarBlock2.connections.Count - 1; num3 >= 0; num3--)
			{
				if (!avatarBlocks.Contains(avatarBlock2.connections[num3]))
				{
					avatarBlock2.connections.RemoveAt(num3);
					avatarBlock2.connectionTypes.RemoveAt(num3);
				}
			}
		}
		List<Block> blocks = avatarBlocks.FindAll((Block b) => b.ContainsGroupTile());
		BlockGroups.GatherBlockGroups(blocks);
		List<Block> list2 = ConnectednessGraph.ConnectedComponent(avatarCharacter, 3);
		Util.AddGroupedTilesToBlockList(list2);
		for (int num4 = avatarBlocks.Count - 1; num4 >= 0; num4--)
		{
			Block block3 = avatarBlocks[num4];
			if (!list2.Contains(block3))
			{
				avatarBlocks.RemoveAt(num4);
				block3.Destroy();
			}
		}
		avatarBlockOffsets = new List<Vector3>();
		for (int num5 = 0; num5 < avatarBlocks.Count; num5++)
		{
			Block block4 = avatarBlocks[num5];
			Vector3 item = avatarCharacter.goT.InverseTransformPoint(block4.GetPosition());
			avatarBlockOffsets.Add(item);
		}
	}

	private List<Block> LoadCharacterFromJson(string avatarSourceJsonStr)
	{
		JObject jObject = JSONDecoder.Decode(avatarSourceJsonStr);
		if (jObject == null || !jObject.ContainsKey("avatar"))
		{
			Debug.Log("Failed to parse avatar json");
			return null;
		}
		List<JObject> arrayValue = jObject["avatar"].ArrayValue;
		List<Block> list = new List<Block>();
		foreach (JObject item2 in arrayValue)
		{
			List<List<Tile>> list2 = Blocksworld.bw.LoadJSONTiles(Blocksworld.GetTileRows(item2));
			string text = (string)list2[0][1].gaf.Args[0];
			if (text == "Character Avatar")
			{
				text = "Character Male";
				list2[0][1].gaf.Args[0] = text;
			}
			Block item = Block.NewBlock(list2);
			list.Add(item);
		}
		int num = 0;
		if (jObject.ContainsKey("connections"))
		{
			foreach (JObject item3 in jObject["connections"].ArrayValue)
			{
				foreach (JObject item4 in item3.ArrayValue)
				{
					list[num].connections.Add(list[(int)item4]);
				}
				num++;
			}
		}
		if (jObject.ContainsKey("connectionTypes"))
		{
			num = 0;
			foreach (JObject item5 in jObject["connectionTypes"].ArrayValue)
			{
				foreach (JObject item6 in item5.ArrayValue)
				{
					list[num].connectionTypes.Add((int)item6);
				}
				num++;
			}
		}
		return list;
	}
}
