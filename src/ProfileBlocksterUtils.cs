using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public static class ProfileBlocksterUtils
{
	public const string PROFILE_GENDER_MALE = "male";

	public const string PROFILE_GENDER_FEMALE = "female";

	public const string PROFILE_GENDER_UNKNOWN = "unknown";

	public const string PROFILE_BLOCK_TYPE_DEFAULT_MALE = "Character Profile";

	public const string PROFILE_BLOCK_TYPE_DEFAULT_FEMALE = "Character Female Profile";

	public const string PROFILE_BLOCK_TYPE_DEFAULT_SKELETON = "Character Skeleton Profile";

	public const string PROFILE_BLOCK_TYPE_ANIM_MALE = "Anim Character Male Profile";

	public const string PROFILE_BLOCK_TYPE_ANIM_FEMALE = "Anim Character Female Profile";

	public const string PROFILE_BLOCK_TYPE_ANIM_SKELETON = "Anim Character Skeleton Profile";

	public const string PROFILE_BLOCK_TYPE_MINI_MALE = "Character Mini Profile";

	public const string PROFILE_BLOCK_TYPE_MINI_FEMALE = "Character Mini Female Profile";

	public const string PROFILE_BLOCK_TYPE_HEADLESS = "Character Headless Profile";

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

	public static IEnumerable<ProfileType> AllProfileTypes()
	{
		return profileTypeInfoDict.Keys;
	}

	public static bool IsProfileBlockType(string typeStr)
	{
		foreach (ProfileTypeInfo value in profileTypeInfoDict.Values)
		{
			if (value.blockTypeStr == typeStr)
			{
				return true;
			}
		}
		return false;
	}

	public static Block GetProfileCharacterBlock()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		for (int i = 0; i < list.Count; i++)
		{
			Block block = list[i];
			if (IsProfileBlockType(block.BlockType()))
			{
				return block;
			}
		}
		return null;
	}

	public static ProfileType GetProfileCharacterBlockType()
	{
		Block profileCharacterBlock = GetProfileCharacterBlock();
		return GetProfileCharacterType(profileCharacterBlock);
	}

	public static ProfileType GetProfileCharacterType(Block profileBlock)
	{
		string profileGender = WorldSession.current.config.profileGender;
		bool flag = profileGender.ToLowerInvariant() == "female";
		string text = profileBlock.BlockType();
		if (text != null && text == "Character Profile")
		{
			if (flag)
			{
				return ProfileType.DEFAULT_FEMALE;
			}
			return ProfileType.DEFAULT_MALE;
		}
		foreach (ProfileTypeInfo value in profileTypeInfoDict.Values)
		{
			if (value.blockTypeStr == text)
			{
				return value.profileType;
			}
		}
		if (flag)
		{
			return ProfileType.DEFAULT_FEMALE;
		}
		return ProfileType.DEFAULT_MALE;
	}

	public static string BlockItemIdentifierForProfileType(ProfileType type)
	{
		if (profileTypeInfoDict.ContainsKey(type))
		{
			return profileTypeInfoDict[type].blockItemIdentifier;
		}
		return "Block Character Male";
	}

	public static string GenderForProfileType(ProfileType type)
	{
		if (profileTypeInfoDict.ContainsKey(type) && !string.IsNullOrEmpty(profileTypeInfoDict[type].profileGender))
		{
			return profileTypeInfoDict[type].profileGender;
		}
		return "unknown";
	}

	public static string GetNonProfileBlockType(string profileBlockType)
	{
		foreach (ProfileTypeInfo value in profileTypeInfoDict.Values)
		{
			if (value.blockTypeStr == profileBlockType)
			{
				return value.nonProfileBlockType;
			}
		}
		return null;
	}

	public static Block ReplaceProfileCharacter(ProfileType newType)
	{
		if (!WorldSession.isProfileBuildSession())
		{
			return null;
		}
		if (!profileTypeInfoDict.ContainsKey(newType))
		{
			return null;
		}
		ProfileTypeInfo profileTypeInfo = profileTypeInfoDict[newType];
		Block block = null;
		ProfileTypeInfo profileTypeInfo2 = null;
		foreach (ProfileTypeInfo value in profileTypeInfoDict.Values)
		{
			Block block2 = BWSceneManager.FindBlockOfType(value.blockTypeStr);
			if (block2 != null)
			{
				profileTypeInfo2 = value;
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
			float num = ((newType != ProfileType.HEADLESS) ? 1f : (-1f));
			List<Block> headAttachments = GetHeadAttachments(block);
			foreach (Block item in headAttachments)
			{
				BlocksterGearType blocksterGearType = CharacterEditor.GetBlocksterGearType(item);
				if (blocksterGearType != BlocksterGearType.Head)
				{
					item.MoveTo(item.GetPosition() + num * Vector3.up);
				}
			}
		}
		string profileCharacterGender = GenderForProfileType(newType);
		WorldSession.current.profileCharacterGender = profileCharacterGender;
		List<List<Tile>> list = Blocksworld.CloneBlockTiles(block.tiles);
		Tile tile = list[0][1];
		tile.gaf = new GAF(Block.predicateCreate, profileTypeInfo.blockTypeStr);
		if (!profileTypeInfo2.isAnimated && profileTypeInfo.isAnimated)
		{
			ConvertToAnimated(list);
		}
		else if (profileTypeInfo2.isAnimated && !profileTypeInfo.isAnimated)
		{
			ConvertToNonAnimated(list);
		}
		Block block3 = Block.NewBlock(list, defaultColors: true, defaultTiles: true);
		if (Blocksworld.IsSelectionLocked(block))
		{
			Blocksworld.SelectionLock(block3);
		}
		BWSceneManager.RemoveBlock(block);
		block.Destroy();
		BWSceneManager.AddBlock(block3);
		ConnectednessGraph.Update(block3);
		if (block3 is BlockAnimatedCharacter)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = block3 as BlockAnimatedCharacter;
			blockAnimatedCharacter.SetLimbsToDefaults();
			blockAnimatedCharacter.DetermineAttachments();
		}
		return block3;
	}

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

	public static Block RestoreProfileCharacter(List<List<Tile>> tiles)
	{
		if (!WorldSession.isProfileBuildSession())
		{
			return null;
		}
		Block profileCharacterBlock = GetProfileCharacterBlock();
		if (profileCharacterBlock != null)
		{
			BWSceneManager.RemoveBlock(profileCharacterBlock);
			profileCharacterBlock.Destroy();
		}
		Block block = Block.NewBlock(tiles);
		BWSceneManager.AddBlock(block);
		ConnectednessGraph.Update(block);
		Blocksworld.SelectionLock(block);
		if (block is BlockAnimatedCharacter)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = block as BlockAnimatedCharacter;
			blockAnimatedCharacter.DetermineAttachments();
		}
		ProfileType profileCharacterType = GetProfileCharacterType(block);
		string profileCharacterGender = GenderForProfileType(profileCharacterType);
		WorldSession.current.profileCharacterGender = profileCharacterGender;
		return block;
	}

	public static void ConvertToAnimated(List<List<Tile>> tiles)
	{
		foreach (List<Tile> tile2 in tiles)
		{
			for (int num = tile2.Count - 1; num >= 0; num--)
			{
				Tile tile = tile2[num];
				if (!ConvertPredicatesForType(tile, typeof(BlockCharacter), typeof(BlockAnimatedCharacter)))
				{
					tile2.RemoveAt(num);
				}
				else if (!ConvertPredicatesForType(tile, typeof(BlockAbstractLegs), typeof(BlockWalkable)))
				{
					tile2.RemoveAt(num);
				}
			}
		}
	}

	public static void ConvertToNonAnimated(List<List<Tile>> tiles)
	{
		foreach (List<Tile> tile2 in tiles)
		{
			for (int num = tile2.Count - 1; num >= 0; num--)
			{
				Tile tile = tile2[num];
				if (!ConvertPredicatesForType(tile, typeof(BlockAnimatedCharacter), typeof(BlockCharacter)))
				{
					tile2.RemoveAt(num);
				}
				else if (!ConvertPredicatesForType(tile, typeof(BlockWalkable), typeof(BlockAbstractLegs)))
				{
					tile2.RemoveAt(num);
				}
			}
		}
	}

	private static bool ConvertPredicatesForType(Tile tile, Type fromType, Type toType)
	{
		Predicate predicate = tile.gaf.Predicate;
		if (PredicateRegistry.GetTypeForPredicate(predicate) == fromType)
		{
			HashSet<Predicate> equivalentPredicates = PredicateRegistry.GetEquivalentPredicates(predicate);
			if (equivalentPredicates != null)
			{
				List<Predicate> list = PredicateRegistry.ForType(toType, includeBaseTypes: false);
				foreach (Predicate item in list)
				{
					if (equivalentPredicates.Contains(item))
					{
						object[] args = PredicateRegistry.ConvertEquivalentPredicateArguments(predicate, item, tile.gaf.Args);
						GAF gAF = new GAF(item, args, dummy: true);
						BWLog.Info(string.Concat("Converted ", tile.gaf, " to ", gAF));
						tile.gaf = gAF;
						return true;
					}
				}
			}
			BWLog.Info("skipping non-compatible tile " + tile.gaf);
			return false;
		}
		return true;
	}
}
