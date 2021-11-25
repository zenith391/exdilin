using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200026F RID: 623
public static class ProfileBlocksterUtils
{
	// Token: 0x06001D16 RID: 7446 RVA: 0x000CD3EA File Offset: 0x000CB7EA
	public static IEnumerable<ProfileType> AllProfileTypes()
	{
		return ProfileBlocksterUtils.profileTypeInfoDict.Keys;
	}

	// Token: 0x06001D17 RID: 7447 RVA: 0x000CD3F8 File Offset: 0x000CB7F8
	public static bool IsProfileBlockType(string typeStr)
	{
		foreach (ProfileTypeInfo profileTypeInfo in ProfileBlocksterUtils.profileTypeInfoDict.Values)
		{
			if (profileTypeInfo.blockTypeStr == typeStr)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001D18 RID: 7448 RVA: 0x000CD46C File Offset: 0x000CB86C
	public static Block GetProfileCharacterBlock()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (ProfileBlocksterUtils.IsProfileBlockType(block.BlockType()))
			{
				return block;
			}
		}
		return null;
	}

	// Token: 0x06001D19 RID: 7449 RVA: 0x000CD4B4 File Offset: 0x000CB8B4
	public static ProfileType GetProfileCharacterBlockType()
	{
		Block profileCharacterBlock = ProfileBlocksterUtils.GetProfileCharacterBlock();
		return ProfileBlocksterUtils.GetProfileCharacterType(profileCharacterBlock);
	}

	// Token: 0x06001D1A RID: 7450 RVA: 0x000CD4D0 File Offset: 0x000CB8D0
	public static ProfileType GetProfileCharacterType(Block profileBlock)
	{
		string profileGender = WorldSession.current.config.profileGender;
		bool flag = profileGender.ToLowerInvariant() == "female";
		string text = profileBlock.BlockType();
		if (text != null)
		{
			if (text == "Character Profile")
			{
				if (flag)
				{
					return ProfileType.DEFAULT_FEMALE;
				}
				return ProfileType.DEFAULT_MALE;
			}
		}
		foreach (ProfileTypeInfo profileTypeInfo in ProfileBlocksterUtils.profileTypeInfoDict.Values)
		{
			if (profileTypeInfo.blockTypeStr == text)
			{
				return profileTypeInfo.profileType;
			}
		}
		return (!flag) ? ProfileType.DEFAULT_MALE : ProfileType.DEFAULT_FEMALE;
	}

	// Token: 0x06001D1B RID: 7451 RVA: 0x000CD5AC File Offset: 0x000CB9AC
	public static string BlockItemIdentifierForProfileType(ProfileType type)
	{
		if (ProfileBlocksterUtils.profileTypeInfoDict.ContainsKey(type))
		{
			return ProfileBlocksterUtils.profileTypeInfoDict[type].blockItemIdentifier;
		}
		return "Block Character Male";
	}

	// Token: 0x06001D1C RID: 7452 RVA: 0x000CD5D4 File Offset: 0x000CB9D4
	public static string GenderForProfileType(ProfileType type)
	{
		if (ProfileBlocksterUtils.profileTypeInfoDict.ContainsKey(type) && !string.IsNullOrEmpty(ProfileBlocksterUtils.profileTypeInfoDict[type].profileGender))
		{
			return ProfileBlocksterUtils.profileTypeInfoDict[type].profileGender;
		}
		return "unknown";
	}

	// Token: 0x06001D1D RID: 7453 RVA: 0x000CD624 File Offset: 0x000CBA24
	public static string GetNonProfileBlockType(string profileBlockType)
	{
		foreach (ProfileTypeInfo profileTypeInfo in ProfileBlocksterUtils.profileTypeInfoDict.Values)
		{
			if (profileTypeInfo.blockTypeStr == profileBlockType)
			{
				return profileTypeInfo.nonProfileBlockType;
			}
		}
		return null;
	}

	// Token: 0x06001D1E RID: 7454 RVA: 0x000CD6A0 File Offset: 0x000CBAA0
	public static Block ReplaceProfileCharacter(ProfileType newType)
	{
		if (!WorldSession.isProfileBuildSession())
		{
			return null;
		}
		if (!ProfileBlocksterUtils.profileTypeInfoDict.ContainsKey(newType))
		{
			return null;
		}
		ProfileTypeInfo profileTypeInfo = ProfileBlocksterUtils.profileTypeInfoDict[newType];
		Block block = null;
		ProfileTypeInfo profileTypeInfo2 = null;
		foreach (ProfileTypeInfo profileTypeInfo3 in ProfileBlocksterUtils.profileTypeInfoDict.Values)
		{
			Block block2 = BWSceneManager.FindBlockOfType(profileTypeInfo3.blockTypeStr);
			if (block2 != null)
			{
				profileTypeInfo2 = profileTypeInfo3;
				block = block2;
				break;
			}
		}
		if (block == null)
		{
			return null;
		}
		if (profileTypeInfo2.profileType == ProfileType.HEADLESS)
		{
			float d = (newType != ProfileType.HEADLESS) ? 1f : -1f;
			List<Block> headAttachments = ProfileBlocksterUtils.GetHeadAttachments(block);
			foreach (Block block3 in headAttachments)
			{
				BlocksterGearType blocksterGearType = CharacterEditor.GetBlocksterGearType(block3);
				if (blocksterGearType != BlocksterGearType.Head)
				{
					block3.MoveTo(block3.GetPosition() + d * Vector3.up);
				}
			}
		}
		string profileCharacterGender = ProfileBlocksterUtils.GenderForProfileType(newType);
		WorldSession.current.profileCharacterGender = profileCharacterGender;
		List<List<Tile>> list = Blocksworld.CloneBlockTiles(block.tiles, null, false, false);
		Tile tile = list[0][1];
		tile.gaf = new GAF(Block.predicateCreate, new object[]
		{
			profileTypeInfo.blockTypeStr
		});
		if (!profileTypeInfo2.isAnimated && profileTypeInfo.isAnimated)
		{
			ProfileBlocksterUtils.ConvertToAnimated(list);
		}
		else if (profileTypeInfo2.isAnimated && !profileTypeInfo.isAnimated)
		{
			ProfileBlocksterUtils.ConvertToNonAnimated(list);
		}
		Block block4 = Block.NewBlock(list, true, true);
		bool flag = Blocksworld.IsSelectionLocked(block);
		if (flag)
		{
			Blocksworld.SelectionLock(block4);
		}
		BWSceneManager.RemoveBlock(block);
		block.Destroy();
		BWSceneManager.AddBlock(block4);
		ConnectednessGraph.Update(block4);
		if (block4 is BlockAnimatedCharacter)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = block4 as BlockAnimatedCharacter;
			blockAnimatedCharacter.SetLimbsToDefaults();
			blockAnimatedCharacter.DetermineAttachments();
		}
		return block4;
	}

	// Token: 0x06001D1F RID: 7455 RVA: 0x000CD8E0 File Offset: 0x000CBCE0
	private static List<Block> GetHeadAttachments(Block profileCharacterBlock)
	{
		List<Block> list = new List<Block>();
		if (profileCharacterBlock is BlockAnimatedCharacter)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = profileCharacterBlock as BlockAnimatedCharacter;
			blockAnimatedCharacter.DetermineAttachments();
			list.AddRange(blockAnimatedCharacter.attachedHeadBlocks);
		}
		else if (profileCharacterBlock is BlockCharacter)
		{
			BlockCharacter blockCharacter = profileCharacterBlock as BlockCharacter;
			blockCharacter.FindAttachements();
			list.AddRange(blockCharacter.headAttachments);
		}
		return list;
	}

	// Token: 0x06001D20 RID: 7456 RVA: 0x000CD944 File Offset: 0x000CBD44
	public static Block RestoreProfileCharacter(List<List<Tile>> tiles)
	{
		if (!WorldSession.isProfileBuildSession())
		{
			return null;
		}
		Block profileCharacterBlock = ProfileBlocksterUtils.GetProfileCharacterBlock();
		if (profileCharacterBlock != null)
		{
			BWSceneManager.RemoveBlock(profileCharacterBlock);
			profileCharacterBlock.Destroy();
		}
		Block block = Block.NewBlock(tiles, false, false);
		BWSceneManager.AddBlock(block);
		ConnectednessGraph.Update(block);
		Blocksworld.SelectionLock(block);
		if (block is BlockAnimatedCharacter)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = block as BlockAnimatedCharacter;
			blockAnimatedCharacter.DetermineAttachments();
		}
		ProfileType profileCharacterType = ProfileBlocksterUtils.GetProfileCharacterType(block);
		string profileCharacterGender = ProfileBlocksterUtils.GenderForProfileType(profileCharacterType);
		WorldSession.current.profileCharacterGender = profileCharacterGender;
		return block;
	}

	// Token: 0x06001D21 RID: 7457 RVA: 0x000CD9C4 File Offset: 0x000CBDC4
	public static void ConvertToAnimated(List<List<Tile>> tiles)
	{
		foreach (List<Tile> list in tiles)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				Tile tile = list[i];
				if (!ProfileBlocksterUtils.ConvertPredicatesForType(tile, typeof(BlockCharacter), typeof(BlockAnimatedCharacter)))
				{
					list.RemoveAt(i);
				}
				else if (!ProfileBlocksterUtils.ConvertPredicatesForType(tile, typeof(BlockAbstractLegs), typeof(BlockWalkable)))
				{
					list.RemoveAt(i);
				}
			}
		}
	}

	// Token: 0x06001D22 RID: 7458 RVA: 0x000CDA8C File Offset: 0x000CBE8C
	public static void ConvertToNonAnimated(List<List<Tile>> tiles)
	{
		foreach (List<Tile> list in tiles)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				Tile tile = list[i];
				if (!ProfileBlocksterUtils.ConvertPredicatesForType(tile, typeof(BlockAnimatedCharacter), typeof(BlockCharacter)))
				{
					list.RemoveAt(i);
				}
				else if (!ProfileBlocksterUtils.ConvertPredicatesForType(tile, typeof(BlockWalkable), typeof(BlockAbstractLegs)))
				{
					list.RemoveAt(i);
				}
			}
		}
	}

	// Token: 0x06001D23 RID: 7459 RVA: 0x000CDB54 File Offset: 0x000CBF54
	private static bool ConvertPredicatesForType(Tile tile, Type fromType, Type toType)
	{
		Predicate predicate = tile.gaf.Predicate;
		bool flag = PredicateRegistry.GetTypeForPredicate(predicate) == fromType;
		if (flag)
		{
			HashSet<Predicate> equivalentPredicates = PredicateRegistry.GetEquivalentPredicates(predicate);
			if (equivalentPredicates != null)
			{
				List<Predicate> list = PredicateRegistry.ForType(toType, false);
				foreach (Predicate predicate2 in list)
				{
					if (equivalentPredicates.Contains(predicate2))
					{
						object[] args = PredicateRegistry.ConvertEquivalentPredicateArguments(predicate, predicate2, tile.gaf.Args);
						GAF gaf = new GAF(predicate2, args, true);
						BWLog.Info(string.Concat(new object[]
						{
							"Converted ",
							tile.gaf,
							" to ",
							gaf
						}));
						tile.gaf = gaf;
						return true;
					}
				}
			}
			BWLog.Info("skipping non-compatible tile " + tile.gaf);
			return false;
		}
		return true;
	}

	// Token: 0x040017BF RID: 6079
	public const string PROFILE_GENDER_MALE = "male";

	// Token: 0x040017C0 RID: 6080
	public const string PROFILE_GENDER_FEMALE = "female";

	// Token: 0x040017C1 RID: 6081
	public const string PROFILE_GENDER_UNKNOWN = "unknown";

	// Token: 0x040017C2 RID: 6082
	public const string PROFILE_BLOCK_TYPE_DEFAULT_MALE = "Character Profile";

	// Token: 0x040017C3 RID: 6083
	public const string PROFILE_BLOCK_TYPE_DEFAULT_FEMALE = "Character Female Profile";

	// Token: 0x040017C4 RID: 6084
	public const string PROFILE_BLOCK_TYPE_DEFAULT_SKELETON = "Character Skeleton Profile";

	// Token: 0x040017C5 RID: 6085
	public const string PROFILE_BLOCK_TYPE_ANIM_MALE = "Anim Character Male Profile";

	// Token: 0x040017C6 RID: 6086
	public const string PROFILE_BLOCK_TYPE_ANIM_FEMALE = "Anim Character Female Profile";

	// Token: 0x040017C7 RID: 6087
	public const string PROFILE_BLOCK_TYPE_ANIM_SKELETON = "Anim Character Skeleton Profile";

	// Token: 0x040017C8 RID: 6088
	public const string PROFILE_BLOCK_TYPE_MINI_MALE = "Character Mini Profile";

	// Token: 0x040017C9 RID: 6089
	public const string PROFILE_BLOCK_TYPE_MINI_FEMALE = "Character Mini Female Profile";

	// Token: 0x040017CA RID: 6090
	public const string PROFILE_BLOCK_TYPE_HEADLESS = "Character Headless Profile";

	// Token: 0x040017CB RID: 6091
	private static Dictionary<ProfileType, ProfileTypeInfo> profileTypeInfoDict = new Dictionary<ProfileType, ProfileTypeInfo>
	{
		{
			ProfileType.ANIMATED_MALE,
			new ProfileTypeInfo
			{
				profileType = ProfileType.ANIMATED_MALE,
				blockTypeStr = "Anim Character Male Profile",
				nonProfileBlockType = "Anim Character Male",
				blockItemIdentifier = "Block Anim Character Male",
				profileGender = "male",
				isAnimated = true
			}
		},
		{
			ProfileType.ANIMATED_FEMALE,
			new ProfileTypeInfo
			{
				profileType = ProfileType.ANIMATED_FEMALE,
				blockTypeStr = "Anim Character Female Profile",
				nonProfileBlockType = "Anim Character Female",
				blockItemIdentifier = "Block Anim Character Female",
				profileGender = "female",
				isAnimated = true
			}
		},
		{
			ProfileType.ANIMATED_SKELETON,
			new ProfileTypeInfo
			{
				profileType = ProfileType.ANIMATED_SKELETON,
				blockTypeStr = "Anim Character Skeleton Profile",
				nonProfileBlockType = "Anim Character Skeleton",
				blockItemIdentifier = "Block Anim Character Skeleton",
				profileGender = "unknown",
				isAnimated = true
			}
		},
		{
			ProfileType.DEFAULT_MALE,
			new ProfileTypeInfo
			{
				profileType = ProfileType.DEFAULT_MALE,
				blockTypeStr = "Character Profile",
				nonProfileBlockType = "Character Male",
				blockItemIdentifier = "Block Character Male",
				profileGender = "male",
				isAnimated = false
			}
		},
		{
			ProfileType.DEFAULT_FEMALE,
			new ProfileTypeInfo
			{
				profileType = ProfileType.DEFAULT_FEMALE,
				blockTypeStr = "Character Female Profile",
				nonProfileBlockType = "Character Female",
				blockItemIdentifier = "Block Character Female",
				profileGender = "female",
				isAnimated = false
			}
		},
		{
			ProfileType.DEFAULT_SKELETON,
			new ProfileTypeInfo
			{
				profileType = ProfileType.DEFAULT_SKELETON,
				blockTypeStr = "Character Skeleton Profile",
				nonProfileBlockType = "Character Skeleton",
				blockItemIdentifier = "Block Character Skeleton",
				profileGender = "unknown",
				isAnimated = false
			}
		},
		{
			ProfileType.MINI_MALE,
			new ProfileTypeInfo
			{
				profileType = ProfileType.MINI_MALE,
				blockTypeStr = "Character Mini Profile",
				nonProfileBlockType = "Character Mini",
				blockItemIdentifier = "Block Character Mini",
				profileGender = "male",
				isAnimated = false
			}
		},
		{
			ProfileType.MINI_FEMALE,
			new ProfileTypeInfo
			{
				profileType = ProfileType.MINI_FEMALE,
				blockTypeStr = "Character Mini Female Profile",
				nonProfileBlockType = "Character Mini Female",
				blockItemIdentifier = "Block Character Mini Female",
				profileGender = "female",
				isAnimated = false
			}
		},
		{
			ProfileType.HEADLESS,
			new ProfileTypeInfo
			{
				profileType = ProfileType.HEADLESS,
				blockTypeStr = "Character Headless Profile",
				nonProfileBlockType = "Character Headless",
				blockItemIdentifier = "Block Character Headless",
				profileGender = "unknown",
				isAnimated = false
			}
		}
	};
}
