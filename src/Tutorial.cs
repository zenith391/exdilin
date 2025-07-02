using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

public class Tutorial
{
	public static string text = null;

	public static string button = null;

	public static Rect rectText;

	public static Rect rectButton;

	public static TutorialMode mode = TutorialMode.StepByStep;

	private static TutorialState _state = TutorialState.None;

	public static TutorialMode modeBeforePlay = TutorialMode.StepByStep;

	public static TutorialState stateBeforePlay = TutorialState.None;

	public static TutorialSafeState safeState = null;

	private static string targetSignalName = "Signal";

	private static Predicate targetSignalPredicate;

	private static string action;

	public static int step = -1;

	public static int savedStep;

	public static int numGivenBlocks;

	public static bool progressBlocked = false;

	public static bool includeDefaultBlockTiles = false;

	public static List<List<Tile>> savedProgressTiles;

	public static List<Block> blocks = new List<Block>();

	public static Block goalBlock = null;

	public static Block oldGoalBlock = null;

	private static List<List<Tile>> goalBlockTilesCopy;

	private static Dictionary<Tile, Tile> originalToCopyTiles;

	private static Dictionary<Block, List<TutorialOperationPose>> operationPoses = new Dictionary<Block, List<TutorialOperationPose>>();

	private static Block existingBlock = null;

	private static List<string> tileRowHints = new List<string>();

	private static string tileRowHint;

	private static bool lastTileWasToggle;

	private static float waitTimeEnd;

	private static TutorialState stateAfterWait;

	private static HashSet<Tile> okParameterTiles = new HashSet<Tile>();

	private static bool autoAddBlock;

	private static float autoAddBlockWaitTime;

	private static Tile manualGoalHint;

	private static Tile moveBlockTile;

	private static Tile moveModelTile;

	private static Tile removeBlockTile;

	private static Dictionary<Block, Tile> groupedManualGoalHints = new Dictionary<Block, Tile>();

	public static List<Tile> manualPaintOrTexture = new List<Tile>();

	public static List<Tile> completedManualPaintOrTexture = new List<Tile>();

	public static List<Tile> manualRemoveBlock = new List<Tile>();

	public static GameObject placementHelper;

	public static Arrow arrow1;

	public static Arrow arrow2;

	public static Target target1;

	public static Target target2;

	public static Hand hand1;

	public static Hand hand2;

	public static bool usingQuickSelectColorIcon = false;

	public static bool usingQuickSelectTextureIcon = false;

	public static bool playModeHelpMover = false;

	private const float defaultAutoCameraDist = 15f;

	private static float autoCameraDist = 15f;

	private static Vector3 autoCameraAngle;

	public static float autoCameraDelay = 0.5f;

	private static float autoCameraTimer = 0f;

	public static float autoCameraTweenTimeMultiplier = 1f;

	private static List<Block> hiddenBlocks = new List<Block>();

	private const int SNAP_THRESHOLD = 12;

	private static HashSet<Block> raycastDisabledBlocks = new HashSet<Block>();

	public static Vector3 autoCameraEndPos;

	private static Vector3 autoCameraStartPos;

	private static Vector3 autoTargetStartPos;

	private static Tween autoCameraTween = null;

	private static bool rotateBeforeScale = false;

	private static bool paintBeforePose = false;

	private static bool textureBeforePose = false;

	private static Vector3 autoAnglesFilter = Vector3.zero;

	private static bool useTwoFingerScale = false;

	private static bool useTwoFingerMove = false;

	private static Vector3 gluePointBlock = Vector3.zero;

	private static Vector3 gluePointWorld = Vector3.zero;

	private static Vector3 glueNormal = Vector3.zero;

	private static Vector3 helpPositionTarget = Vector3.zero;

	public static int forceMeshIndex = 0;

	public static bool cheatNextStep = false;

	private static HashSet<GAF> hiddenGafs = new HashSet<GAF>();

	private static HashSet<GAF> neverHideGafs = new HashSet<GAF>();

	private static bool hideGraphics = false;

	public static bool stepOnNextUpdate = false;

	public static Tile scrollToTile;

	private static HudMeshLabel hintLabel;

	private static HashSet<string> neverHideInStepByStepBlocks;

	private static HashSet<string> neverHideInStepByStepPaints;

	private static HashSet<string> neverHideInStepByStepTextures;

	private static HashSet<string> neverHideInStepByStepSFXs;

	private static TutorialSettings tutorialSettings;

	private static Dictionary<Predicate, int> predicateAccumIndices = null;

	private static Vector3[] rots = new Vector3[4]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	private static Vector3[] best = new Vector3[4]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	private static int bestN = 0;

	private static Vector3 dragYRotationAxis;

	private static Vector3[] rots2D = new Vector3[4]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	private static Vector3[] best2D = new Vector3[4]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	private static int bestN2D = 0;

	private static string paths;

	public static TutorialState state
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
		}
	}

	public static void Init()
	{
		arrow1 = new Arrow();
		arrow2 = new Arrow();
		target1 = new Target();
		target2 = new Target();
		hand1 = new Hand();
		hand2 = new Hand(left: true);
	}

	public static void ResetState()
	{
		state = TutorialState.None;
		stateBeforePlay = state;
		mode = TutorialMode.StepByStep;
		modeBeforePlay = mode;
		TutorialActions.Clear();
		safeState = null;
	}

	private static TutorialSettings GetTutorialSettings()
	{
		if (tutorialSettings == null)
		{
			tutorialSettings = Blocksworld.blocksworldDataContainer.GetComponent<TutorialSettings>();
		}
		return tutorialSettings;
	}

	private static bool NeverHideInStepByStep(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate)
		{
			if (neverHideInStepByStepBlocks == null)
			{
				neverHideInStepByStepBlocks = new HashSet<string>(GetTutorialSettings().neverHideStepByStepBlocks);
			}
			return neverHideInStepByStepBlocks.Contains((string)gaf.Args[0]);
		}
		if (gaf.Predicate == Block.predicateTextureTo)
		{
			if (neverHideInStepByStepTextures == null)
			{
				neverHideInStepByStepTextures = new HashSet<string>(GetTutorialSettings().neverHideStepByStepTextures);
			}
			return neverHideInStepByStepTextures.Contains((string)gaf.Args[0]);
		}
		if (gaf.Predicate == Block.predicatePaintTo)
		{
			if (neverHideInStepByStepPaints == null)
			{
				neverHideInStepByStepPaints = new HashSet<string>(GetTutorialSettings().neverHideStepByStepPaints);
			}
			return neverHideInStepByStepPaints.Contains((string)gaf.Args[0]);
		}
		if (gaf.Predicate == Block.predicatePlaySoundDurational)
		{
			if (neverHideInStepByStepSFXs == null)
			{
				neverHideInStepByStepSFXs = new HashSet<string>(GetTutorialSettings().neverHideStepByStepSFXs);
			}
			return neverHideInStepByStepSFXs.Contains((string)gaf.Args[0]);
		}
		return false;
	}

	private static void ProcessHiddenGaf(GAF gaf, HashSet<string> blocksToKeep, HashSet<string> texturesToKeep, HashSet<string> paintsToKeep, HashSet<string> sfxsToKeep, HashSet<Predicate> allPlaceableBlockPredicates)
	{
		Predicate predicate = gaf.Predicate;
		if (predicate == Block.predicateCreate)
		{
			blocksToKeep.Add((string)gaf.Args[0]);
		}
		else if (predicate == Block.predicateTextureTo)
		{
			texturesToKeep.Add((string)gaf.Args[0]);
		}
		else if (predicate == Block.predicatePaintTo)
		{
			paintsToKeep.Add((string)gaf.Args[0]);
		}
		else if (predicate == Block.predicatePlaySoundDurational)
		{
			sfxsToKeep.Add((string)gaf.Args[0]);
		}
		else if (predicate == Block.predicateTutorialTextureExistingBlock)
		{
			texturesToKeep.Add((string)gaf.Args[1]);
		}
		else if (predicate == Block.predicateTutorialPaintExistingBlock)
		{
			paintsToKeep.Add((string)gaf.Args[1]);
		}
		else
		{
			allPlaceableBlockPredicates.Add(predicate);
		}
	}

	private static void SetHiddenGafs(bool addNeverHideGafs = true)
	{
		HashSet<GAF> hashSet = new HashSet<GAF>();
		HashSet<Predicate> taggedPredicates = Blocksworld.GetTaggedPredicates();
		foreach (Block block in blocks)
		{
			if (block is BlockGrouped blockGrouped)
			{
				neverHideGafs.Add(blockGrouped.GetIconGaf());
			}
			foreach (List<Tile> tile2 in block.tiles)
			{
				foreach (Tile item2 in tile2)
				{
					GAF gaf = item2.gaf;
					GAF primitiveGafFor = TileToggleChain.GetPrimitiveGafFor(gaf);
					if (primitiveGafFor != null)
					{
						hashSet.Add(primitiveGafFor);
						neverHideGafs.Add(primitiveGafFor);
						if (primitiveGafFor.Predicate == Block.predicateSendCustomSignal || primitiveGafFor.Predicate == Block.predicateSendCustomSignalModel)
						{
							neverHideGafs.Add(Scarcity.GetNormalizedGaf(primitiveGafFor));
						}
					}
					else if (taggedPredicates.Contains(gaf.Predicate))
					{
						EditableTileParameter editableParameter = gaf.Predicate.EditableParameter;
						if (editableParameter == null)
						{
							neverHideGafs.Add(gaf);
							continue;
						}
						GAF gAF = gaf.Clone();
						gAF.Args[editableParameter.parameterIndex] = null;
						Tile tile = Blocksworld.buildPanel.FindTileMatching(gAF);
						if (tile != null)
						{
							neverHideGafs.Add(tile.gaf);
							continue;
						}
						GAF gAF2 = gaf.Clone();
						GAF item = new GAF(gAF2.Predicate, gAF2.Predicate.ExtendArguments(gAF2.Args, overwrite: true));
						neverHideGafs.Add(item);
					}
					else if (gaf.Predicate == BlockMaster.predicateSetEnvEffect)
					{
						neverHideGafs.Add(Scarcity.GetNormalizedGaf(new GAF(Block.predicateTextureTo, BlockSky.EnvEffectToTexture((string)gaf.Args[0]))));
					}
					else if (gaf.Predicate == BlockMaster.predicatePaintSkyTo)
					{
						neverHideGafs.Add(Scarcity.GetNormalizedGaf(new GAF(Block.predicatePaintTo, (string)gaf.Args[0])));
					}
				}
			}
		}
		HashSet<Predicate> hashSet2 = new HashSet<Predicate>();
		HashSet<Predicate> hashSet3 = new HashSet<Predicate>
		{
			Block.predicateCreate,
			Block.predicateTextureTo,
			Block.predicatePaintTo,
			Block.predicatePlaySoundDurational
		};
		HashSet<string> hashSet4 = new HashSet<string>();
		HashSet<string> hashSet5 = new HashSet<string>();
		HashSet<string> hashSet6 = new HashSet<string>();
		HashSet<string> hashSet7 = new HashSet<string>();
		foreach (Tile item3 in manualPaintOrTexture)
		{
			GAF gaf2 = item3.gaf;
			if (gaf2.Predicate == Block.predicateTutorialTextureExistingBlock)
			{
				hashSet5.Add((string)gaf2.Args[1]);
			}
			else if (gaf2.Predicate == Block.predicateTutorialPaintExistingBlock)
			{
				hashSet6.Add((string)gaf2.Args[1]);
			}
		}
		foreach (Block block2 in blocks)
		{
			foreach (List<Tile> tile3 in block2.tiles)
			{
				foreach (Tile item4 in tile3)
				{
					GAF gaf3 = item4.gaf;
					ProcessHiddenGaf(gaf3, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
				}
			}
		}
		ProcessHiddenIfNotNull(Scarcity.puzzleGAFUsage, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		ProcessHiddenIfNotNull(Scarcity.puzzleInventory, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		ProcessHiddenIfNotNull(Scarcity.puzzleGAFUsageAfterStepByStep, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		ProcessHiddenIfNotNull(Scarcity.puzzleInventoryAfterStepByStep, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		foreach (GAF unlockedGAF in Blocksworld.unlockedGAFs)
		{
			if (((!hashSet3.Contains(unlockedGAF.Predicate) && !hashSet2.Contains(unlockedGAF.Predicate)) || ((!addNeverHideGafs || !NeverHideInStepByStep(unlockedGAF)) && ((unlockedGAF.Predicate == Block.predicateCreate && !hashSet4.Contains((string)unlockedGAF.Args[0])) || (unlockedGAF.Predicate == Block.predicatePaintTo && !hashSet6.Contains((string)unlockedGAF.Args[0])) || (unlockedGAF.Predicate == Block.predicatePlaySoundDurational && !hashSet7.Contains((string)unlockedGAF.Args[0])) || (unlockedGAF.Predicate == Block.predicateTextureTo && !hashSet5.Contains((string)unlockedGAF.Args[0]))))) && !hashSet.Contains(unlockedGAF))
			{
				hiddenGafs.Add(unlockedGAF);
			}
		}
	}

	private static void ProcessHiddenIfNotNull(Dictionary<GAF, int> dict, HashSet<string> blocksToKeep, HashSet<string> texturesToKeep, HashSet<string> paintsToKeep, HashSet<string> sfxsToKeep, HashSet<Predicate> allPlaceableBlockPredicates)
	{
		if (dict == null)
		{
			return;
		}
		foreach (GAF key in dict.Keys)
		{
			ProcessHiddenGaf(key, blocksToKeep, texturesToKeep, paintsToKeep, sfxsToKeep, allPlaceableBlockPredicates);
		}
	}

	private static void UpdatePredicateAccums()
	{
		if (predicateAccumIndices == null)
		{
			predicateAccumIndices = new Dictionary<Predicate, int>
			{
				{
					BlockCharacter.predicateCharacterMover,
					1
				},
				{
					BlockCharacter.predicateCharacterChaseTag,
					1
				},
				{
					BlockCharacter.predicateCharacterGotoTag,
					1
				},
				{
					Block.predicateWaitTime,
					0
				},
				{
					BlockCharacter.predicateCharacterJump,
					0
				},
				{
					BlockAbstractWheel.predicateWheelDrive,
					0
				},
				{
					BlockAbstractWheel.predicateWheelTurn,
					0
				},
				{
					BlockSpokedWheel.predicateSpokedWheelDrive,
					0
				},
				{
					BlockSpokedWheel.predicateSpokedWheelTurn,
					0
				},
				{
					BlockRocket.predicateRocketFire,
					0
				},
				{
					BlockRocketOctagonal.predicateRocketOctagonalFire,
					0
				},
				{
					BlockRocketSquare.predicateRocketSquareFire,
					0
				},
				{
					BlockStabilizer.predicateStabilizerHold,
					0
				},
				{
					BlockStabilizer.predicateStabilizerStabilize,
					0
				},
				{
					BlockStabilizer.predicateStabilizerBurst,
					0
				},
				{
					BlockSquareStabilizer.predicateSquareStabilizerHold,
					0
				},
				{
					BlockSquareStabilizer.predicateSquareStabilizerStabilize,
					0
				},
				{
					BlockSquareStabilizer.predicateSquareStabilizerBurst,
					0
				},
				{
					BlockAntiGravity.predicateAntigravityAlign,
					0
				},
				{
					BlockAntiGravity.predicateAntigravityAlignTerrain,
					0
				},
				{
					BlockAntiGravity.predicateAntigravityLevitate,
					0
				},
				{
					BlockAntiGravity.predicateAntigravityStay,
					0
				},
				{
					BlockAntiGravityColumn.predicateAntigravityColumnAlign,
					0
				},
				{
					BlockAntiGravityColumn.predicateAntigravityColumnAlignTerrain,
					0
				},
				{
					BlockAntiGravityColumn.predicateAntigravityColumnLevitate,
					0
				},
				{
					BlockAntiGravityColumn.predicateAntigravityColumnStay,
					0
				}
			};
		}
	}

	private static bool CanAccumulateArguments(Tile tile)
	{
		UpdatePredicateAccums();
		return predicateAccumIndices.ContainsKey(tile.gaf.Predicate);
	}

	private static void Accumulate(Tile accumTile, Tile tile)
	{
		UpdatePredicateAccums();
		Predicate predicate = tile.gaf.Predicate;
		if (predicateAccumIndices.TryGetValue(predicate, out var value))
		{
			float num = (float)accumTile.gaf.Args[value];
			num += (float)tile.gaf.Args[value];
			accumTile.gaf.Args[value] = num;
		}
	}

	private static bool GoalBlockNeedsFiltering()
	{
		Predicate predicateHideTileRow = Block.predicateHideTileRow;
		Predicate predicateHideNextTile = Block.predicateHideNextTile;
		for (int num = goalBlock.tiles.Count - 1; num >= 0; num--)
		{
			List<Tile> list = goalBlock.tiles[num];
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Tile tile = list[num2];
				Predicate predicate = tile.gaf.Predicate;
				if (predicate == predicateHideTileRow || predicate == predicateHideNextTile)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void FilterGoalBlockTiles()
	{
		Predicate predicateHideTileRow = Block.predicateHideTileRow;
		Predicate predicateHideNextTile = Block.predicateHideNextTile;
		for (int num = goalBlock.tiles.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			List<Tile> list = goalBlock.tiles[num];
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Tile tile = list[num2];
				Predicate predicate = tile.gaf.Predicate;
				if (predicate == predicateHideTileRow)
				{
					flag = true;
				}
				else if (predicate == predicateHideNextTile || predicate == Block.predicateTutorialHelpTextAction)
				{
					list.RemoveAt(num2);
					if (num2 < list.Count)
					{
						Tile tile2 = list[num2];
						if (tile2.gaf.Predicate != Block.predicateThen)
						{
							list.RemoveAt(num2);
						}
					}
				}
			}
			HashSet<Tile> hashSet = new HashSet<Tile>();
			Tile tile3 = null;
			for (int i = 0; i < list.Count; i++)
			{
				Tile tile4 = list[i];
				bool flag2 = CanAccumulateArguments(tile4);
				if (tile3 == null || tile4.gaf.Predicate != tile3.gaf.Predicate)
				{
					tile3 = ((!flag2) ? null : tile4);
					continue;
				}
				Accumulate(tile3, tile4);
				hashSet.Add(tile4);
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				Tile item = list[num3];
				if (hashSet.Contains(item))
				{
					list.RemoveAt(num3);
				}
			}
			if (flag)
			{
				goalBlock.tiles.RemoveAt(num);
			}
		}
		Predicate predicate2 = PredicateRegistry.ByName("Meta.TileRowHint");
		List<List<Tile>> tiles = goalBlock.tiles;
		for (int j = 0; j < tiles.Count; j++)
		{
			string item2 = string.Empty;
			List<Tile> list2 = goalBlock.tiles[j];
			for (int num4 = list2.Count - 1; num4 >= 0; num4--)
			{
				Tile tile5 = list2[num4];
				Predicate predicate3 = tile5.gaf.Predicate;
				if (predicate3 == predicate2)
				{
					item2 = (string)tile5.gaf.Args[0];
					list2.RemoveAt(num4);
				}
			}
			tileRowHints.Add(item2);
		}
	}

	private static void EnterPuzzleMode(Dictionary<GAF, int> puzzleInventory, Dictionary<GAF, int> puzzleGAFUsage)
	{
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>(puzzleGAFUsage);
		Scarcity.inventory = new Dictionary<GAF, int>(puzzleInventory);
		Scarcity.UpdateInventory();
		foreach (KeyValuePair<GAF, int> item in puzzleGAFUsage)
		{
			GAF key = item.Key;
			if (Scarcity.inventory.ContainsKey(key) && Scarcity.inventory[key] != -1)
			{
				Scarcity.inventory[key] -= item.Value;
			}
		}
		Blocksworld.enabledGAFs = null;
		Blocksworld.UpdateTiles();
		mode = TutorialMode.Puzzle;
		state = TutorialState.Puzzle;
		SetHiddenGafs(addNeverHideGafs: false);
		Blocksworld.UpdateTiles();
		WorldSession.current.OnCompleteBuild();
	}

	public static void Start()
	{
		Start(skipBuild: false);
	}

	public static void Start(bool skipBuild)
	{
		Blocksworld.buildPanel.SetTutorialMode(tutorialActive: true);
		TBox.ResetTargetSnapping();
		DestroyPlacementHelper();
		usingQuickSelectColorIcon = false;
		usingQuickSelectTextureIcon = false;
		rotateBeforeScale = false;
		paintBeforePose = false;
		textureBeforePose = false;
		autoAnglesFilter = Vector3.zero;
		useTwoFingerMove = false;
		useTwoFingerScale = false;
		includeDefaultBlockTiles = false;
		groupedManualGoalHints.Clear();
		existingBlock = null;
		oldGoalBlock = null;
		safeState = null;
		hiddenGafs = new HashSet<GAF>();
		neverHideGafs = new HashSet<GAF>();
		okParameterTiles.Clear();
		operationPoses.Clear();
		Blocksworld.UpdateCameraPosesMap();
		TutorialActions.ParseActions(BWSceneManager.AllBlocks());
		HashSet<Block> hashSet = new HashSet<Block>();
		hiddenBlocks.Clear();
		raycastDisabledBlocks.Clear();
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			bool flag = item.ContainsTileWithPredicate(Block.predicateLocked);
			bool flag2 = item.ContainsTileWithPredicate(Block.predicateTutorialRemoveBlockHint);
			if (item is BlockGrouped { group: not null } blockGrouped)
			{
				if (hashSet.Contains(item))
				{
					flag = true;
				}
				else
				{
					Block mainBlockInGroup = blockGrouped.GetMainBlockInGroup();
					if (mainBlockInGroup.ContainsTileWithPredicate(Block.predicateLocked))
					{
						flag = true;
						hashSet.UnionWith(blockGrouped.group.GetBlocks());
					}
				}
			}
			if (flag || item.isTerrain)
			{
				if (!flag && item.IsUnlocked() && !flag2)
				{
					blocks.Add(item);
					continue;
				}
				if (!skipBuild && item.ContainsTileWithPredicate(Block.predicateTutorialHideInBuildMode))
				{
					hiddenBlocks.Add(item);
					item.go.SetActive(value: false);
					if (item.goShadow != null)
					{
						item.goShadow.SetActive(value: false);
					}
				}
				Blocksworld.locked.Add(item);
			}
			else
			{
				blocks.Add(item);
			}
		}
		tileRowHints.Clear();
		if (Scarcity.puzzleInventory != null)
		{
			EnterPuzzleMode(Scarcity.puzzleInventory, Scarcity.puzzleGAFUsage);
			return;
		}
		SetHiddenGafs();
		if (!skipBuild)
		{
			foreach (Block block3 in blocks)
			{
				block3.go.SetActive(value: false);
				if (block3.goShadow != null)
				{
					block3.goShadow.SetActive(value: false);
				}
				BWSceneManager.RemoveBlock(block3);
			}
		}
		else
		{
			foreach (Block block4 in blocks)
			{
				ConnectednessGraph.Update(block4);
			}
		}
		Scarcity.inventory = new Dictionary<GAF, int>();
		Blocksworld.Deselect();
		Blocksworld.stars.Play();
		Blocksworld.scriptPanel.PositionReset();
		numGivenBlocks = BWSceneManager.BlockCount();
		step = -1;
		if (!skipBuild)
		{
			state = TutorialState.GetNextBlock;
		}
		autoCameraAngle = new Vector3(30f, 0f, 0f);
		if (!skipBuild)
		{
			_ = savedProgressTiles;
		}
		for (int num = manualPaintOrTexture.Count - 1; num >= 0; num--)
		{
			if (blocks.Count > 0)
			{
				Block block = blocks[0];
				block.tiles.Insert(1, new List<Tile>
				{
					new Tile(new GAF("Meta.Then")),
					manualPaintOrTexture[num]
				});
			}
		}
		manualPaintOrTexture.Clear();
		for (int i = 0; i < completedManualPaintOrTexture.Count; i++)
		{
			GAF gaf = completedManualPaintOrTexture[i].gaf;
			Block block2 = NthBlock((int)gaf.Args[0]);
			object obj = gaf.Args[1];
			if (gaf.Predicate == Block.predicateTutorialPaintExistingBlock)
			{
				block2.PaintTo((string)obj, permanent: true);
			}
			else if (gaf.Predicate == Block.predicateTutorialTextureExistingBlock)
			{
				block2.TextureTo((string)obj, Vector3.zero, permanent: true);
			}
			else if (gaf.Predicate.Name.Equals("Block.TutorialRotateExistingBlock"))
			{
				block2.RotateTo(Quaternion.Euler((Vector3)obj));
			}
		}
		completedManualPaintOrTexture.Clear();
		Blocksworld.UpdateTiles();
		HardCodedTweaksAtStart(Blocksworld.currentWorldId);
		if (skipBuild)
		{
			ResetState();
			action = string.Empty;
			SetWinLoseBlocksToTutorialMode();
		}
		else
		{
			CreateTutorialSafeState();
			TutorialActions.EnterContext(TutorialActionContext.TutorialStart);
			Step();
		}
	}

	public static bool IsLastBlockIncomplete()
	{
		return BWSceneManager.BlockCount() - numGivenBlocks >= step + 1;
	}

	public static void CheatCreateBlock()
	{
		TutorialState tutorialState = state;
		if (tutorialState != TutorialState.Play && tutorialState != TutorialState.Puzzle && tutorialState != TutorialState.None && Options.Cowlorded)
		{
			cheatNextStep = true;
			stepOnNextUpdate = true;
		}
	}

	public static void Stop(bool forReal = false)
	{
		step = -1;
		safeState = null;
		Scarcity.ResetInventory();
		Blocksworld.enabledGAFs = null;
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.stars.Stop();
		arrow1.Show(show: false);
		arrow2.Show(show: false);
		target1.Show(show: false);
		target2.Show(show: false);
		hand1.Hide();
		hand2.Hide();
		TBox.tileButtonMove.Show(show: true);
		TBox.tileButtonRotate.Show(show: true);
		TBox.tileButtonScale.Show(show: true);
		TBox.tileButtonMove.Enable(enable: true);
		TBox.tileButtonRotate.Enable(enable: true);
		TBox.tileButtonScale.Enable(enable: true);
		TBox.tileButtonMove.Show(show: false);
		TBox.tileButtonRotate.Show(show: false);
		TBox.tileButtonScale.Show(show: false);
		state = stateBeforePlay;
		mode = modeBeforePlay;
		stateBeforePlay = TutorialState.None;
		modeBeforePlay = TutorialMode.StepByStep;
		if (forReal)
		{
			state = TutorialState.None;
			mode = TutorialMode.StepByStep;
		}
		Blocksworld.buildPanel.SetTutorialMode(tutorialActive: false);
		tileRowHint = null;
		tileRowHints.Clear();
		DestroyPlacementHelper();
		TutorialActions.Clear();
	}

	public static void StartPlay()
	{
		if (state != TutorialState.None)
		{
			target1.Show(show: false);
			target2.Show(show: false);
			hand1.Hide();
			hand2.Hide();
			modeBeforePlay = mode;
			stateBeforePlay = state;
			state = TutorialState.Play;
			TutorialActions.EnterContext(TutorialActionContext.EnterPlay);
			foreach (Block hiddenBlock in hiddenBlocks)
			{
				if (hiddenBlock.go != null)
				{
					hiddenBlock.go.SetActive(value: true);
					if (hiddenBlock.goShadow != null)
					{
						hiddenBlock.goShadow.SetActive(value: true);
					}
				}
				ConnectednessGraph.Update(hiddenBlock);
			}
			hiddenBlocks.Clear();
			DestroyPlacementHelper();
			if (playModeHelpMover)
			{
				TutorialActions.EnterContext(TutorialActionContext.BeforePlayMoverUse);
				target1.TargetMoverControl();
				hand1.TapTarget(target1);
			}
		}
		UpdateCheatButton();
	}

	public static void StopPlay()
	{
		if (mode == TutorialMode.Puzzle)
		{
			state = TutorialState.Puzzle;
		}
		if (Blocksworld.currentWorldId == "rock_fence")
		{
			arrow1.Show(show: false);
			arrow2.Show(show: false);
			target1.Show(show: false);
			target2.Show(show: false);
			hand1.Hide();
			hand2.Hide();
			Blocksworld.counters["3"] = 0;
		}
		state = stateBeforePlay;
		mode = modeBeforePlay;
		safeState = null;
		Blocksworld.UpdateTiles();
		TutorialActions.Clear();
	}

	public static void TurnOffMoverHelp()
	{
		playModeHelpMover = false;
		target1.Show(show: false);
		hand1.Hide();
		TutorialActions.Clear();
	}

	public static void BeforeSave()
	{
		if (hiddenBlocks == null || hiddenBlocks.Count == 0)
		{
			return;
		}
		foreach (Block hiddenBlock in hiddenBlocks)
		{
			if (hiddenBlock.go != null)
			{
				hiddenBlock.go.SetActive(value: true);
				if (hiddenBlock.goShadow != null)
				{
					hiddenBlock.goShadow.SetActive(value: true);
				}
			}
			ConnectednessGraph.Update(hiddenBlock);
		}
	}

	public static void AfterSave()
	{
		if (hiddenBlocks == null || hiddenBlocks.Count == 0)
		{
			return;
		}
		foreach (Block hiddenBlock in hiddenBlocks)
		{
			if (hiddenBlock.go != null)
			{
				hiddenBlock.go.SetActive(value: false);
				if (hiddenBlock.goShadow != null)
				{
					hiddenBlock.goShadow.SetActive(value: false);
				}
			}
		}
		hiddenBlocks.Clear();
	}

	private static void HardCodedTweaksAtStart(string id)
	{
		if (id != null && id == "rock_fence")
		{
			Blocksworld.locked.Add(HardCodeBlock("Cube", new Vector3(-45f, 2f, 58f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
			Blocksworld.locked.Add(HardCodeBlock("Cube", new Vector3(-43f, 2f, 53f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
			Blocksworld.locked.Add(HardCodeBlock("Cube", new Vector3(-41f, 2f, 57f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
			Blocksworld.locked.Add(HardCodeBlock("Wedge", new Vector3(-43f, 2f, 57f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
			Blocksworld.locked.Add(HardCodeBlock("Wedge", new Vector3(-45f, 2f, 53f), new Vector3(0f, 180f, 0f), Vector3.one, "Grey", "Bricks"));
		}
	}

	private static Block HardCodeBlock(string type, Vector3 pos, Vector3 rot, Vector3 scale, string paint, string texture)
	{
		Block block = Blocksworld.bw.AddNewBlock(new Tile(new GAF("Block.Create", type)));
		block.RotateTo(Quaternion.Euler(rot));
		block.MoveTo(pos);
		block.ScaleTo(scale);
		block.PaintTo(paint, permanent: true);
		block.TextureTo(texture, Vector3.zero, permanent: true);
		ConnectednessGraph.Update(block);
		return block;
	}

	public static bool Hit(Vector3 mouse)
	{
		if (state == TutorialState.None)
		{
			return false;
		}
		Vector3 point = new Vector3(mouse.x, (float)NormalizedScreen.height - mouse.y, 0f);
		return rectText.Contains(point);
	}

	public static bool Close(Vector3 v1, Vector3 v2)
	{
		return (v2 - v1).sqrMagnitude < 0.02f;
	}

	private static bool Close(float f1, float f2)
	{
		return Mathf.Abs(f2 - f1) < 1f;
	}

	private static bool Rotation(float f1, float f2)
	{
		return Mathf.Abs(f2 - f1) < 1f;
	}

	private static bool CanFakeSameRotation(Block b1, Quaternion r1, Block b2, Quaternion r2)
	{
		string text = b1.BlockType();
		if (text != null && text == "Cube")
		{
			return b1.Scale() == Vector3.one;
		}
		return false;
	}

	private static bool SameRotation(Block b1, Quaternion r1, Block b2, Quaternion r2, bool fakeEnabled = true)
	{
		if (fakeEnabled)
		{
			switch (b1.BlockType())
			{
			case "Wedge":
				if (!(Quaternion.Angle(r1, r2) < 5f))
				{
					if (b1.Scale() == Vector3.one)
					{
						return Quaternion.Angle(r1 * Quaternion.Euler(-90f, 180f, 0f), r2) < 5f;
					}
					return false;
				}
				return true;
			case "Rocket":
				if (Materials.GetMapping(b2.GetTexture()) == Mapping.AllSidesTo1x1 && b1.Scale().x == 1f && b1.Scale().z == 1f)
				{
					if (!(Quaternion.Angle(r1, r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(0f, 90f, 0f), r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(0f, 180f, 0f), r2) < 5f))
					{
						return Quaternion.Angle(r1 * Quaternion.Euler(0f, 270f, 0f), r2) < 5f;
					}
					return true;
				}
				break;
			case "Cylinder":
				if (Materials.GetMapping(b2.GetTexture()) == Mapping.AllSidesTo1x1)
				{
					if (!(Quaternion.Angle(r1, r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(0f, 90f, 0f), r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(0f, 180f, 0f), r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(0f, 270f, 0f), r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(180f, 0f, 0f), r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(180f, 90f, 0f), r2) < 5f) && !(Quaternion.Angle(r1 * Quaternion.Euler(180f, 180f, 0f), r2) < 5f))
					{
						return Quaternion.Angle(r1 * Quaternion.Euler(180f, 270f, 0f), r2) < 5f;
					}
					return true;
				}
				break;
			case "Cube":
				if (Materials.GetMapping(b2.GetTexture()) == Mapping.AllSidesTo1x1 && b1.Scale() == Vector3.one)
				{
					return true;
				}
				break;
			}
		}
		return Quaternion.Angle(r1, r2) < 5f;
	}

	public static void Update()
	{
		if (MappedInput.InputDown(MappableInput.CHEAT_CREATE_BLOCK))
		{
			CheatCreateBlock();
		}
		TutorialActions.ExecuteAndUpdateActions();
		switch (state)
		{
		case TutorialState.Waiting:
			if (Time.time > waitTimeEnd)
			{
				state = stateAfterWait;
				Step();
			}
			break;
		default:
			arrow1.Update();
			arrow2.Update();
			target1.Update();
			target2.Update();
			hand1.Update();
			hand2.Update();
			AutoCamera();
			if (state == TutorialState.Position || state == TutorialState.CreateBlock)
			{
				Block selectedBlock = Blocksworld.selectedBlock;
				if (selectedBlock == null || selectedBlock.go == null)
				{
					arrow1.Show(show: true);
					target1.Show(show: true);
					target2.Show(show: true);
				}
				else
				{
					Vector3 position = selectedBlock.go.transform.position;
					if (TutorialSnap(position, helpPositionTarget))
					{
						selectedBlock.MoveTo(helpPositionTarget);
						arrow1.Show(show: false);
						target2.Show(show: false);
						Blocksworld.tBoxGesture.Stop();
						bool enabled = selectedBlock.go.GetComponent<Collider>().enabled;
						selectedBlock.EnableCollider(value: true);
						ConnectednessGraph.Update(selectedBlock);
						selectedBlock.EnableCollider(enabled);
						stepOnNextUpdate = true;
					}
					else if (TutorialSnapXZ(position, helpPositionTarget))
					{
						Vector3 pos = new Vector3(helpPositionTarget.x, position.y, helpPositionTarget.z);
						selectedBlock.MoveTo(pos);
					}
					else if (TutorialSnapY(position, helpPositionTarget))
					{
						Vector3 pos2 = new Vector3(position.x, helpPositionTarget.y, position.z);
						selectedBlock.MoveTo(pos2);
					}
					else
					{
						arrow1.Show(show: true);
						target1.Show(show: true);
						target2.Show(show: true);
					}
				}
			}
			else
			{
				target2.Show(show: false);
			}
			if (stepOnNextUpdate)
			{
				stepOnNextUpdate = false;
				Step();
			}
			break;
		case TutorialState.Play:
			if (playModeHelpMover)
			{
				target1.Update();
				hand1.Update();
			}
			break;
		case TutorialState.None:
			break;
		}
		if (hideGraphics)
		{
			arrow1.Show(show: false);
			arrow2.Show(show: false);
			target1.Show(show: false);
			target2.Show(show: false);
			hand1.Hide();
			hand2.Hide();
		}
	}

	private static Vector3 NearestCameraAngleSnap(float x, float y)
	{
		return new Vector3(x, Mathf.Round((y + 45f) / 90f) * 90f - 45f, 0f);
	}

	public static void HelpAutoCamera()
	{
		autoCameraStartPos = Blocksworld.cameraTransform.position;
		autoTargetStartPos = Blocksworld.cameraTransform.position + autoCameraDist * Blocksworld.cameraTransform.forward;
		autoCameraTimer = 0f;
		state = TutorialState.Orbit;
	}

	private static bool AutoCameraEnabled()
	{
		return !Options.TutorialDisableAutoCamera;
	}

	private static void AutoCamera()
	{
		if (state != TutorialState.Orbit || !AutoCameraEnabled() || TutorialActions.AnyActionBlocksProgress())
		{
			return;
		}
		if (Blocksworld.cWidgetGesture.gestureState == GestureState.Active || Blocksworld.tileDragGesture.gestureState == GestureState.Active || Blocksworld.createTileDragGesture.gestureState == GestureState.Active || Blocksworld.tBoxGesture.gestureState == GestureState.Active)
		{
			autoCameraTimer = 0f;
			autoCameraTween = null;
			return;
		}
		if (autoCameraTween != null && autoCameraTween.IsFinished())
		{
			autoCameraTween = null;
			Util.ClearDraw();
			Blocksworld.blocksworldCamera.SetCameraPosition(autoCameraEndPos);
			Blocksworld.cameraTransform.LookAt(Blocksworld.blocksworldCamera.GetTargetPosition());
			stepOnNextUpdate = true;
			return;
		}
		autoCameraTimer += Time.deltaTime;
		if (!(autoCameraTimer < autoCameraDelay))
		{
			if (autoCameraTween == null)
			{
				float magnitude = (autoCameraEndPos - autoCameraStartPos).magnitude;
				float duration = autoCameraTweenTimeMultiplier * Mathf.Clamp(0.15f * Mathf.Sqrt(magnitude), 0.25f, 3f);
				autoCameraTween = new Tween();
				autoCameraTween.Start(duration);
			}
			Vector3 vector = autoCameraEndPos - autoCameraStartPos;
			Blocksworld.blocksworldCamera.SetCameraPosition(autoCameraStartPos + autoCameraTween.Value() * vector);
			Vector3 vector2 = Blocksworld.blocksworldCamera.GetTargetPosition() - autoTargetStartPos;
			Blocksworld.cameraTransform.LookAt(autoTargetStartPos + autoCameraTween.Value() * vector2);
		}
	}

	private static void CreateTutorialSafeState()
	{
		safeState = new TutorialSafeState();
		safeState.ExtractState();
	}

	public static void UpdateCheatButton()
	{
		bool flag = Options.Cowlorded;
		State currentState = Blocksworld.CurrentState;
		if (currentState == State.Play)
		{
			flag = false;
		}
		TutorialState tutorialState = state;
		if (tutorialState == TutorialState.None || tutorialState == TutorialState.BuildingCompleted || tutorialState == TutorialState.Play)
		{
			flag = false;
		}
		if (flag)
		{
			Blocksworld.UI.ShowCheatButton();
			if (BW.isUnityEditor)
			{
				Blocksworld.UI.MakeCheatButtonInvisible();
			}
		}
		else
		{
			Blocksworld.UI.HideCheatButton();
		}
	}

	private static bool SameTutorialNormals(string blockType, Vector3 n1, Vector3 n2)
	{
		if (blockType != null && blockType == "Slice")
		{
			if (n1.z > 0.01f)
			{
				return n2.z > 0.01f;
			}
			return false;
		}
		return Close(n1, n2);
	}

	public static bool GAFsEqualInTutorial(GAF g1, GAF g2)
	{
		return g1.Equals(g2);
	}

	private static void SetWinLoseBlocksToTutorialMode()
	{
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			for (int i = 0; i < item.tiles.Count; i++)
			{
				List<Tile> list = item.tiles[i];
				foreach (Tile item2 in list)
				{
					GAF gaf = item2.gaf;
					if (gaf.Predicate == Block.predicateGameWin || gaf.Predicate == Block.predicateGameLose)
					{
						gaf.Args[1] = 4f;
					}
				}
			}
		}
	}

	public static Block GetLastBlock()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		if (list.Count == 0)
		{
			return null;
		}
		return list[list.Count - 1];
	}

	public static Block NthBlock(int index)
	{
		List<Block> list = BWSceneManager.AllBlocks();
		return list[index];
	}

	public static void Step()
	{
		progressBlocked = false;
		if (TutorialActions.StepActions() || TutorialActions.AnyActionBlocksProgress())
		{
			progressBlocked = true;
			return;
		}
		UpdateCheatButton();
		TBox.ResetTargetSnapping();
		DisablePlacementHelper();
		hideGraphics = BW.isUnityEditor && Options.HideTutorialGraphics;
		scrollToTile = null;
		switch (state)
		{
		case TutorialState.BuildingCompleted:
			HelpPlay();
			return;
		case TutorialState.Puzzle:
		{
			bool flag = Blocksworld.selectedBunch != null || (Blocksworld.selectedBlock != null && Blocksworld.locked.Contains(Blocksworld.selectedBlock));
			TBox.tileButtonMove.Enable(!flag);
			TBox.tileButtonRotate.Enable(!flag);
			TBox.tileButtonScale.Enable(!flag);
			return;
		}
		case TutorialState.GetNextBlock:
			step++;
			if (step == blocks.Count)
			{
				state = TutorialState.BuildingCompleted;
				SetWinLoseBlocksToTutorialMode();
				BW.Analytics.SendAnalyticsEvent("world-puzzle-built");
				if (Scarcity.puzzleInventoryAfterStepByStep != null)
				{
					EnterPuzzleMode(Scarcity.puzzleInventoryAfterStepByStep, Scarcity.puzzleGAFUsageAfterStepByStep);
				}
				else
				{
					WorldSession.current.OnCompleteBuild();
				}
			}
			else
			{
				BW.Analytics.SendAnalyticsEvent("world-puzzle-step", Convert.ToString(step + savedStep));
				oldGoalBlock = goalBlock;
				goalBlock = blocks[step];
				bool flag2 = goalBlock != null && goalBlock.HasAnyGroup();
				bool flag3 = oldGoalBlock != null && oldGoalBlock.HasAnyGroup();
				bool flag4 = false;
				if (flag2)
				{
					if (!flag3 || goalBlock.GetGroupOfType("tank-treads") != oldGoalBlock.GetGroupOfType("tank-treads"))
					{
						flag4 = true;
					}
				}
				else if (GoalBlockNeedsFiltering())
				{
					flag4 = true;
				}
				if (flag4)
				{
					CreateTutorialSafeState();
				}
				else
				{
					safeState = null;
				}
				manualGoalHint = null;
				moveBlockTile = null;
				removeBlockTile = null;
				moveModelTile = null;
				autoAddBlock = false;
				autoAddBlockWaitTime = 1f;
				manualRemoveBlock.Clear();
				manualPaintOrTexture.Clear();
				completedManualPaintOrTexture.Clear();
				bool flag5 = true;
				while (flag5)
				{
					flag5 = false;
					if (goalBlock.tiles.Count <= 1 || goalBlock.tiles[1].Count <= 1)
					{
						continue;
					}
					Tile tile = goalBlock.tiles[1][1];
					GAF gaf = tile.gaf;
					Predicate predicate = gaf.Predicate;
					if (predicate == Block.predicateTutorialCreateBlockHint)
					{
						manualGoalHint = tile;
						if (flag2)
						{
							groupedManualGoalHints[goalBlock.GetMainBlockInGroup("tank-treads")] = manualGoalHint;
						}
						flag5 = true;
					}
					else if (predicate == Block.predicateTutorialAutoAddBlock)
					{
						autoAddBlock = true;
						autoAddBlockWaitTime = Util.GetFloatArg(gaf.Args, 0, 1f);
						flag5 = true;
					}
					else if (predicate == Block.predicateTutorialOperationPose)
					{
						if (!operationPoses.ContainsKey(goalBlock))
						{
							operationPoses[goalBlock] = new List<TutorialOperationPose>();
						}
						List<TutorialOperationPose> list = operationPoses[goalBlock];
						TutorialState tutorialState = (TutorialState)Util.GetEnumArg(gaf.Args, 0, "Texture", typeof(TutorialState));
						int intArg = Util.GetIntArg(gaf.Args, 1, 0);
						string stringArg = Util.GetStringArg(gaf.Args, 2, string.Empty);
						list.Add(new TutorialOperationPose
						{
							state = tutorialState,
							poseName = stringArg,
							meshIndex = intArg
						});
						flag5 = true;
					}
					else if (predicate == Block.predicateTutorialPaintExistingBlock || predicate == Block.predicateTutorialTextureExistingBlock || predicate == Block.predicateTutorialRotateExistingBlock)
					{
						manualPaintOrTexture.Add(tile);
						flag5 = true;
					}
					else if (predicate == Block.predicateTutorialRemoveBlockHint)
					{
						manualRemoveBlock.Add(tile);
						removeBlockTile = tile;
						flag5 = true;
					}
					else if (predicate == Block.predicateUnlocked || predicate == Block.predicateTutorialMoveBlock || predicate == Block.predicateTutorialMoveModel)
					{
						flag5 = true;
						if (predicate == Block.predicateTutorialMoveBlock)
						{
							moveBlockTile = tile;
						}
						else if (predicate == Block.predicateTutorialMoveModel)
						{
							moveModelTile = tile;
						}
					}
					if (flag5)
					{
						goalBlock.tiles[1].RemoveAt(1);
						if (goalBlock.tiles[1].Count == 1)
						{
							goalBlock.tiles.RemoveAt(1);
						}
					}
				}
				originalToCopyTiles = new Dictionary<Tile, Tile>();
				goalBlockTilesCopy = Blocksworld.CloneBlockTiles(goalBlock.tiles, originalToCopyTiles);
				FilterGoalBlockTiles();
				state = TutorialState.DetermineInstructions;
			}
			stepOnNextUpdate = true;
			return;
		case TutorialState.None:
			if (mode == TutorialMode.Puzzle)
			{
				state = TutorialState.Puzzle;
				stepOnNextUpdate = true;
			}
			return;
		case TutorialState.Waiting:
		case TutorialState.Play:
			return;
		}
		arrow1.Show(show: false);
		arrow2.Show(show: false);
		target1.Show(show: false);
		target2.Show(show: false);
		hand1.Hide();
		hand2.Hide();
		TBox.tileButtonMove.Enable(enable: false);
		TBox.tileButtonRotate.Enable(enable: false);
		TBox.tileButtonScale.Enable(enable: false);
		tileRowHint = null;
		usingQuickSelectColorIcon = false;
		usingQuickSelectTextureIcon = false;
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		quickSelect.UpdateTextureIcon();
		quickSelect.UpdateColorIcon();
		autoCameraAngle = NearestCameraAngleSnap(autoCameraAngle.x, Blocksworld.cameraTransform.eulerAngles.y);
		if (BWSceneManager.BlockCount() > numGivenBlocks + blocks.Count)
		{
			Block lastBlock = GetLastBlock();
			Blocksworld.blocksworldCamera.Follow(lastBlock);
			HelpDestroyBlock(lastBlock);
			return;
		}
		if (step == blocks.Count)
		{
			Sound.PlaySound("Tutorial Complete", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
			state = TutorialState.BuildingCompleted;
			safeState = null;
			stepOnNextUpdate = true;
			Blocksworld.blocksworldCamera.Follow(GetLastBlock());
			return;
		}
		existingBlock = null;
		if (manualPaintOrTexture.Count > 0 && manualPaintOrTexture[0].gaf.Predicate.Name.Equals("Block.TutorialRotateExistingBlock"))
		{
			GAF gaf2 = manualPaintOrTexture[0].gaf;
			Block block = NthBlock((int)gaf2.Args[0]);
			Vector3 vector = (Vector3)gaf2.Args[1];
			if (!Close((Vector3)block.tiles[0][3].gaf.Args[0], vector))
			{
				Blocksworld.locked.Remove(block);
				existingBlock = block;
				HelpRotation(block, Quaternion.Euler(vector));
			}
			else
			{
				completedManualPaintOrTexture.Add(manualPaintOrTexture[0]);
				manualPaintOrTexture.RemoveAt(0);
				Blocksworld.Deselect();
				Blocksworld.locked.Add(block);
				stepOnNextUpdate = true;
			}
			return;
		}
		if (manualPaintOrTexture.Count > 0 && manualPaintOrTexture[0].gaf.Predicate == Block.predicateTutorialPaintExistingBlock)
		{
			GAF gaf3 = manualPaintOrTexture[0].gaf;
			Block block2 = NthBlock((int)gaf3.Args[0]);
			string text = (string)gaf3.Args[1];
			Vector3 cameraPos = (Vector3)gaf3.Args[2];
			Vector3 cameraTarget = (Vector3)gaf3.Args[3];
			if (!((string)block2.tiles[0][4].gaf.Args[0]).Equals(text))
			{
				Blocksworld.locked.Remove(block2);
				existingBlock = block2;
				HelpColor(block2, new GAF("Block.PaintTo", text), cameraPos, cameraTarget);
			}
			else
			{
				completedManualPaintOrTexture.Add(manualPaintOrTexture[0]);
				manualPaintOrTexture.RemoveAt(0);
				Blocksworld.locked.Add(block2);
				stepOnNextUpdate = true;
			}
			return;
		}
		if (manualPaintOrTexture.Count > 0 && manualPaintOrTexture[0].gaf.Predicate == Block.predicateTutorialTextureExistingBlock)
		{
			GAF gaf4 = manualPaintOrTexture[0].gaf;
			Block block3 = NthBlock((int)gaf4.Args[0]);
			string text2 = (string)gaf4.Args[1];
			Vector3 cameraPos2 = (Vector3)gaf4.Args[2];
			Vector3 cameraTarget2 = (Vector3)gaf4.Args[3];
			if (!Scarcity.GetNormalizedTexture((string)block3.tiles[0][5].gaf.Args[0]).Equals(text2))
			{
				Blocksworld.locked.Remove(block3);
				existingBlock = block3;
				HelpTexture(block3, new GAF("Block.TextureTo", text2, Vector3.zero), cameraPos2, cameraTarget2);
			}
			else
			{
				completedManualPaintOrTexture.Add(manualPaintOrTexture[0]);
				manualPaintOrTexture.RemoveAt(0);
				Blocksworld.locked.Add(block3);
				stepOnNextUpdate = true;
			}
			return;
		}
		if (manualRemoveBlock.Count > 0)
		{
			GAF gaf5 = manualRemoveBlock[0].gaf;
			int index = (int)gaf5.Args[0];
			Block block4 = NthBlock(index);
			Vector3 vector2 = (Vector3)gaf5.Args[1];
			Vector3 targetPosition = (Vector3)gaf5.Args[2];
			if (!(block4 is BlockPosition))
			{
				BlockPosition blockPosition = (BlockPosition)Blocksworld.bw.AddNewBlock(new Tile(new GAF(Block.predicateCreate, "Position No Glue")), addToBlocks: false);
				blockPosition.MoveTo(blockPosition.GetPosition() + Vector3.one * 9999f);
				blockPosition.tiles.Insert(1, Block.LockedTileRow());
				blockPosition.go.SetActive(value: false);
				int index2 = Blocksworld.locked.IndexOf(block4);
				List<Block> list2 = BWSceneManager.AllBlocks();
				list2[index] = blockPosition;
				Blocksworld.locked[index2] = blockPosition;
				if (block4.IsLocked())
				{
					block4.tiles[1].RemoveAt(1);
					if (block4.tiles[1].Count == 1 && block4.tiles.Count > 2)
					{
						block4.tiles.RemoveAt(1);
					}
				}
				list2.Add(block4);
				autoCameraEndPos = vector2;
				Blocksworld.blocksworldCamera.Unfollow();
				Blocksworld.blocksworldCamera.SetTargetPosition(targetPosition);
				HelpAutoCamera();
				stepOnNextUpdate = true;
			}
			else
			{
				manualRemoveBlock.RemoveAt(0);
				stepOnNextUpdate = true;
			}
			return;
		}
		bool flag6 = goalBlock != null && goalBlock.HasGroup("tank-treads");
		if (manualGoalHint == null && flag6)
		{
			Block mainBlockInGroup = goalBlock.GetMainBlockInGroup("tank-treads");
			if (groupedManualGoalHints.ContainsKey(mainBlockInGroup))
			{
				manualGoalHint = groupedManualGoalHints[mainBlockInGroup];
			}
		}
		if (BWSceneManager.BlockCount() - numGivenBlocks < step + 1)
		{
			Blocksworld.blocksworldCamera.Follow(goalBlock);
			HelpCreateBlock();
			return;
		}
		if (state == TutorialState.CreateBlock)
		{
			TutorialActions.EnterContext(TutorialActionContext.AfterThisBlockCreate, goalBlock);
		}
		Block block5 = NthBlock(numGivenBlocks + step);
		Blocksworld.blocksworldCamera.Follow(block5);
		if (goalBlock == null)
		{
			state = TutorialState.GetNextBlock;
			stepOnNextUpdate = true;
			return;
		}
		if (block5.BlockType() != goalBlock.BlockType())
		{
			if (goalBlock.GetGroupOfType("locked-model") != null)
			{
				Blocksworld.locked.Add(block5);
				state = TutorialState.GetNextBlock;
				stepOnNextUpdate = true;
			}
			else
			{
				HelpDestroyBlock(block5);
			}
			return;
		}
		if ((paintBeforePose && !CheckPaint(block5, goalBlock)) || (textureBeforePose && !CheckTexture(block5, goalBlock)))
		{
			return;
		}
		if (rotateBeforeScale)
		{
			if (!CheckRotation(block5, fakeEnabled: false) || !CheckScale(block5))
			{
				return;
			}
		}
		else if (!CheckScale(block5) || !CheckRotation(block5))
		{
			return;
		}
		Vector3 vector3 = goalBlock.go.transform.position + Vector3.one * 10000.25f;
		if (moveBlockTile != null || moveModelTile != null)
		{
			Tile tile2 = ((moveBlockTile != null) ? moveBlockTile : moveModelTile);
			vector3 = (Vector3)tile2.gaf.Args[0];
		}
		if (!Close(block5.go.transform.position, goalBlock.go.transform.position) && !Close(goalBlock.go.transform.position, vector3))
		{
			HelpPosition(block5);
			return;
		}
		DisableOccludingConnectedColliders(block5);
		if ((!paintBeforePose && !CheckPaint(block5, goalBlock)) || (!textureBeforePose && !CheckTexture(block5, goalBlock)))
		{
			return;
		}
		EnableOccludingConnectedColliders();
		TutorialActions.EnterContext(TutorialActionContext.BeginScripting, goalBlock);
		Dictionary<Tile, Tile> dictionary = new Dictionary<Tile, Tile>();
		Predicate predicate2 = PredicateRegistry.ByName("Meta.Then");
		for (int i = 1; i < block5.tiles.Count; i++)
		{
			for (int j = 0; j < block5.tiles[i].Count; j++)
			{
				Tile tile3 = block5.tiles[i][j];
				bool flag7 = i < goalBlock.tiles.Count && j < goalBlock.tiles[i].Count;
				Tile tile4 = ((!flag7) ? null : goalBlock.tiles[i][j]);
				if (tile3 != null && tile3.gaf.Predicate == predicate2)
				{
					tile3 = null;
				}
				if (tile4 != null && tile4.gaf.Predicate == predicate2)
				{
					tile4 = null;
				}
				if (tile3 != null && tile4 != null)
				{
					if (TileToggleChain.InSameChain(tile3.gaf.BlockItemId, tile4.gaf.BlockItemId))
					{
						continue;
					}
					if (!tile3.gaf.EqualsInTutorial(tile4.gaf))
					{
						EditableTileParameter editableParameter = tile3.gaf.Predicate.EditableParameter;
						if (editableParameter != null)
						{
							GAF gAF = tile3.gaf.Clone();
							int parameterIndex = editableParameter.parameterIndex;
							gAF.Args[parameterIndex] = null;
							if (gAF.Matches(tile4.gaf))
							{
								if (okParameterTiles.Contains(tile4))
								{
									dictionary[tile4] = tile3;
									continue;
								}
								GAF gaf6 = tile4.gaf;
								GAF gaf7 = tile3.gaf;
								if (OkOverwriteParameterValue(editableParameter, gaf6.Predicate, gaf7.Args[parameterIndex], gaf6.Args[parameterIndex]))
								{
									gaf7.Args[parameterIndex] = gaf6.Args[parameterIndex];
								}
								else if (!SnapParameterValue(gaf7, gaf6))
								{
									state = TutorialState.SetParameter;
									editableParameter.HelpSetParameterValueInTutorial(block5, tile3, tile4);
									UpdateTileRowHint(i);
									return;
								}
								continue;
							}
						}
						if (tile3.gaf.Predicate == Block.predicatePlaySoundDurational)
						{
							GAF gaf8 = tile4.gaf;
							GAF gaf9 = tile3.gaf;
							GAF gAF2 = gaf9.Clone();
							for (int k = 1; k < gAF2.Args.Length; k++)
							{
								gAF2.Args[k] = null;
							}
							if (gAF2.Matches(gaf8))
							{
								for (int l = 1; l < gaf8.Args.Length; l++)
								{
									gaf9.Args[l] = gaf8.Args[l];
								}
								return;
							}
						}
					}
				}
				Tile tile5 = block5.tiles[i][j];
				if (!tile5.IsThen() && flag7)
				{
					Tile tile6 = goalBlock.tiles[i][j];
					if (!tile6.gaf.EqualsInTutorial(tile5.gaf))
					{
						for (int m = -j; m < goalBlock.tiles[i].Count - j + 1; m++)
						{
							if (m != 0 && block5.tiles[i].Count > j + m && goalBlock.tiles[i].Count > j + m && block5.tiles[i][j].gaf.EqualsInTutorial(goalBlock.tiles[i][j + m].gaf))
							{
								HelpSwapTiles(block5, block5.tiles[i][j], block5.tiles[i][j + m], Mathf.Sign(m));
								UpdateTileRowHint(i);
								return;
							}
						}
					}
				}
				if (tile3 != null && (tile4 == null || !tile4.gaf.EqualsInTutorial(tile3.gaf)))
				{
					HelpRemoveTile(block5, block5.tiles[i][j]);
					UpdateTileRowHint(i);
					return;
				}
			}
		}
		for (int n = 1; n < Mathf.Max(block5.tiles.Count, goalBlock.tiles.Count); n++)
		{
			if (n >= goalBlock.tiles.Count || n >= block5.tiles.Count)
			{
				continue;
			}
			for (int num = 0; num < Mathf.Max(block5.tiles[n].Count, goalBlock.tiles[n].Count); num++)
			{
				Tile tile7 = ((block5.tiles.Count <= n || block5.tiles[n].Count <= num) ? null : block5.tiles[n][num]);
				Tile tile8 = ((goalBlock.tiles.Count <= n || goalBlock.tiles[n].Count <= num) ? null : goalBlock.tiles[n][num]);
				if (tile7 != null && tile7.gaf.Predicate == predicate2)
				{
					tile7 = null;
				}
				if (tile8 != null && tile8.gaf.Predicate == predicate2)
				{
					tile8 = null;
				}
				if (!okParameterTiles.Contains(tile8))
				{
					if (tile8 != null && tile7 != null && !tile8.gaf.EqualsInTutorial(tile7.gaf) && TileToggleChain.InSameChain(tile8.gaf.BlockItemId, tile7.gaf.BlockItemId))
					{
						HelpToggleTile(block5, tile7);
						UpdateTileRowHint(n);
						lastTileWasToggle = true;
						return;
					}
					if (tile8 != null && (tile7 == null || !tile8.gaf.EqualsInTutorial(tile7.gaf)))
					{
						HelpAddTile(block5, goalBlock.tiles[n][num].gaf, n, num);
						UpdateTileRowHint(n);
						lastTileWasToggle = false;
						return;
					}
					if (tile7 != null && tile8 == null)
					{
						HelpRemoveTile(block5, block5.tiles[n][num]);
						UpdateTileRowHint(n);
						lastTileWasToggle = false;
						return;
					}
				}
			}
		}
		if (lastTileWasToggle)
		{
			lastTileWasToggle = false;
			stateAfterWait = state;
			state = TutorialState.Waiting;
			waitTimeEnd = Time.time + 1f;
			return;
		}
		if (TutorialActions.EnterContext(TutorialActionContext.BlockDone, goalBlock))
		{
			stateAfterWait = state;
			state = TutorialState.Waiting;
			waitTimeEnd = Time.time + 2f;
			return;
		}
		if (moveBlockTile != null || moveModelTile != null)
		{
			Vector3 position = goalBlock.go.transform.position;
			if ((position - vector3).magnitude > 0.001f)
			{
				goalBlock.go.transform.position = vector3;
				goalBlockTilesCopy[0][2].gaf.Args[0] = vector3;
			}
			if (!Close(block5.go.transform.position, goalBlock.go.transform.position))
			{
				HelpPosition(block5, moveModelTile != null);
				return;
			}
		}
		if (originalToCopyTiles != null)
		{
			foreach (KeyValuePair<Tile, Tile> item in dictionary)
			{
				if (originalToCopyTiles.TryGetValue(item.Key, out var value))
				{
					value.gaf = item.Value.gaf.Clone();
				}
			}
			originalToCopyTiles = null;
			dictionary.Clear();
		}
		okParameterTiles.Clear();
		block5.tiles = goalBlockTilesCopy;
		manualGoalHint = null;
		goalBlock.Destroy();
		groupedManualGoalHints.Remove(goalBlock);
		Blocksworld.scriptPanel.PositionReset();
		Blocksworld.locked.Add(block5);
		Blocksworld.Deselect();
		Blocksworld.buildPanel.PositionReset();
		if (!hideGraphics)
		{
			Blocksworld.stars.transform.position = block5.go.transform.position;
			Blocksworld.stars.Emit(30);
		}
		Sound.PlaySound("Tutorial Step", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		bool flag8 = autoAddBlock;
		autoAddBlock = false;
		autoCameraAngle = new Vector3(30f, autoCameraAngle.y, 0f);
		autoCameraTimer = 0f;
		if (flag8)
		{
			TutorialActions.EnterContext(TutorialActionContext.AutoAddBlockWait);
			state = TutorialState.Waiting;
			waitTimeEnd = Time.time + autoAddBlockWaitTime;
			stateAfterWait = TutorialState.GetNextBlock;
		}
		else
		{
			state = TutorialState.GetNextBlock;
			stepOnNextUpdate = true;
		}
	}

	private static bool CheckPaint(Block thisBlock, Block goalBlock)
	{
		int num = 0;
		for (int i = 4; i < goalBlock.tiles[0].Count; i++)
		{
			GAF gaf = goalBlock.tiles[0][i].gaf;
			bool flag = gaf.Predicate == Block.predicatePaintTo;
			if (!goalBlock.IgnorePaintToIndexInTutorial(num) && flag && (thisBlock.tiles[0].Count <= i || !thisBlock.tiles[0][i].gaf.Equals(gaf)))
			{
				TutorialActions.EnterContext(TutorialActionContext.Paint, goalBlock);
				HelpColor(thisBlock, gaf, Util.nullVector3, Util.nullVector3);
				return false;
			}
			if (flag)
			{
				num++;
			}
		}
		return true;
	}

	private static bool CheckTexture(Block thisBlock, Block goalBlock)
	{
		int num = 0;
		for (int i = 5; i < goalBlock.tiles[0].Count; i++)
		{
			GAF gaf = goalBlock.tiles[0][i].gaf;
			string name = gaf.Predicate.Name;
			bool flag = name == "Block.TextureTo";
			if (!goalBlock.IgnoreTextureToIndexInTutorial(num) && flag)
			{
				GAF gaf2 = thisBlock.tiles[0][i].gaf;
				Mapping mapping = Materials.GetMapping((string)gaf.Args[0]);
				if (thisBlock.tiles[0].Count <= i || !flag || !gaf2.Args[0].Equals(gaf.Args[0]) || gaf2.Args.Length != gaf.Args.Length || (gaf2.Args.Length == 3 && !gaf2.Args[2].Equals(gaf.Args[2])) || ((mapping == Mapping.OneSideTo1x1 || mapping == Mapping.OneSideWrapTo1x1 || mapping == Mapping.TwoSidesTo1x1 || mapping == Mapping.TwoSidesWrapTo1x1) && !SameTutorialNormals(thisBlock.BlockType(), (Vector3)gaf2.Args[1], (Vector3)gaf.Args[1])))
				{
					TutorialActions.EnterContext(TutorialActionContext.Texture, goalBlock);
					HelpTexture(thisBlock, gaf, Util.nullVector3, Util.nullVector3);
					return false;
				}
			}
			if (flag)
			{
				num++;
			}
		}
		return true;
	}

	public static bool DistanceOK(Vector3 point)
	{
		if (state == TutorialState.None)
		{
			return true;
		}
		if (mode == TutorialMode.Puzzle)
		{
			return true;
		}
		if (goalBlock == null || goalBlock.go == null)
		{
			return false;
		}
		float num = Util.MaxComponent(goalBlock.size);
		float magnitude = (point - goalBlock.go.transform.position).magnitude;
		return magnitude < 9f + num;
	}

	public static bool RaycastTargetBlockOK(Block bl)
	{
		if (state == TutorialState.None)
		{
			return true;
		}
		if (mode == TutorialMode.Puzzle)
		{
			return true;
		}
		if (bl == null)
		{
			return true;
		}
		if (goalBlock == null || goalBlock.go == null)
		{
			return true;
		}
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (bl == worldOceanBlock)
		{
			Bounds waterBounds = worldOceanBlock.GetWaterBounds();
			Vector3 position = goalBlock.GetPosition();
			if (waterBounds.Contains(position))
			{
				return false;
			}
		}
		return true;
	}

	private static void HelpTapBlock(Block block, bool allowBunchSelect = false)
	{
		if (Blocksworld.selectedBlock != null && !allowBunchSelect)
		{
			Blocksworld.Deselect();
		}
		block.EnableCollider(value: false);
		Vector3 position = block.go.transform.position;
		if (AutoCameraEnabled() && FindCameraAngleForPoint(position, ignoreCameraIsLevel: false, autoCameraDist * 0.5f, autoCameraDist * 2f) && !Close(Blocksworld.cameraTransform.position, autoCameraEndPos))
		{
			Blocksworld.blocksworldCamera.SetTargetPosition(position);
			HelpAutoCamera();
			block.EnableCollider(value: true);
			return;
		}
		block.EnableCollider(value: true);
		state = ((!allowBunchSelect) ? TutorialState.TapBlock : TutorialState.SelectBunch);
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		target1.TargetBlock(block);
		hand1.TapTarget(target1);
	}

	private static bool IsOutsideScreen(Tile tile)
	{
		return IsOutsideScreen(tile.GetPosition());
	}

	private static bool IsOutsideScreen(Vector3 position)
	{
		float num = 80f * NormalizedScreen.pixelScale;
		if (!(position.x < 0f) && !(position.x > (float)Blocksworld.screenWidth - num) && !(position.y < 0f))
		{
			return position.y > (float)Blocksworld.screenHeight - num * 1.5f;
		}
		return true;
	}

	private static bool BlockInsideScreen(Block b)
	{
		Vector3 vector = Util.WorldToScreenPoint(b.go.transform.position, z: false);
		if (vector.x > 0f && vector.x < (float)Blocksworld.screenWidth && vector.y > 0f)
		{
			return vector.y < (float)Blocksworld.screenHeight;
		}
		return false;
	}

	private static bool PointInsideScreen(Vector3 s, float margin = 50f)
	{
		if (s.x > margin && s.x < (float)Blocksworld.screenWidth - margin && s.y > margin)
		{
			return s.y < (float)Blocksworld.screenHeight - margin;
		}
		return false;
	}

	private static void HelpScrollSidePanel(Tile tile)
	{
		state = TutorialState.Scroll;
		Vector3 position = tile.GetPosition();
		float num = ((position.y >= 0f) ? (-150) : 150);
		float num2 = 13f;
		num2 *= NormalizedScreen.pixelScale;
		num *= NormalizedScreen.pixelScale;
		arrow1.state = TrackingState.Screen2Screen;
		arrow1.screen = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, (float)(Blocksworld.screenHeight / 2) - num + num, 0f);
		arrow1.screen2 = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, (float)(Blocksworld.screenHeight / 2) + num + num, 0f);
		arrow1.Show(show: true, 3);
		hand1.DragArrow(arrow1);
		arrow2.state = TrackingState.Screen2ScreenBounce;
		float num3 = ((position.y >= 0f) ? Blocksworld.screenHeight : 0);
		arrow2.bounce = ((!(position.y >= 0f)) ? 1 : (-1));
		arrow2.screen = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, num3 + num, 0f);
		arrow2.screen2 = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, num3, 0f);
		arrow2.Show(show: true, 1);
	}

	private static void HelpDragScriptPanel(Tile tile)
	{
		state = TutorialState.DragBlockPanel;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		ScriptPanel scriptPanel = Blocksworld.scriptPanel;
		Vector3 targetPos = new Vector3((float)Blocksworld.screenWidth * 0.35f, (float)Blocksworld.screenHeight * 0.5f, 0f);
		float num = 20f;
		float x = scriptPanel.width * 0.5f;
		float y = scriptPanel.height * 0.5f;
		List<Vector3> list = new List<Vector3>
		{
			scriptPanel.position + new Vector3(scriptPanel.width - num, y),
			scriptPanel.position + new Vector3(num, y),
			scriptPanel.position + new Vector3(x, scriptPanel.height - num),
			scriptPanel.position + new Vector3(x, num)
		};
		list.Sort((Vector3 a, Vector3 b) => (a - targetPos).sqrMagnitude.CompareTo((b - targetPos).sqrMagnitude));
		Vector3 screen = list[0];
		arrow1.state = TrackingState.Screen2Screen;
		arrow1.screen = screen;
		arrow1.screen2 = targetPos;
		arrow1.Show(show: true);
		hand1.DragArrow(arrow1);
	}

	private static bool CanCreateBlock(Block goalBlock, out List<string> errors)
	{
		errors = new List<string>();
		Tile tile = goalBlock.tiles[0][1];
		GAF gaf = tile.gaf;
		if (Blocksworld.buildPanel.FindTileMatching(gaf) == null)
		{
			errors.Add("Could not find tile for gaf " + gaf);
		}
		return errors.Count == 0;
	}

	private static bool HelpTapTab(GAF gaf)
	{
		int selectedTab = (int)Blocksworld.buildPanel.GetTabBar().SelectedTab;
		int tabIndexForGafSmart = PanelSlots.GetTabIndexForGafSmart(gaf);
		if (selectedTab != tabIndexForGafSmart)
		{
			HelpTapTab(tabIndexForGafSmart);
			return true;
		}
		return false;
	}

	private static void HelpTapTab(int tabIndex)
	{
		float tabHeight = Blocksworld.UI.TabBar.GetTabHeight();
		Vector3 screen = new Vector3((float)NormalizedScreen.width - 0.5f * TabBar.pixelWidth, (float)NormalizedScreen.height - tabHeight * ((float)tabIndex + 0.5f) / NormalizedScreen.scale, 0f);
		target1.TargetScreen(screen);
		hand1.TapTarget(target1);
		state = TutorialState.TapTab;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
	}

	private static void HelpCreateBlock()
	{
		if (goalBlock == null || goalBlock.go == null)
		{
			return;
		}
		TutorialActions.EnterContext(TutorialActionContext.BeforeThisBlockCreate, goalBlock);
		bool flag = goalBlock.HasAnyGroup();
		Tile tile = goalBlock.tiles[0][1];
		if (cheatNextStep || autoAddBlock)
		{
			cheatNextStep = false;
			if (!flag)
			{
				Block block = Blocksworld.CloneBlock(goalBlock);
				ConnectednessGraph.Update(block);
				if (!autoAddBlock)
				{
					Blocksworld.Select(block);
				}
				stepOnNextUpdate = true;
			}
			return;
		}
		GAF gAF = tile.gaf;
		if (flag)
		{
			if (goalBlock.HasGroup("locked-model"))
			{
				if (Blocksworld.modelCollection != null)
				{
					BlockGroup groupOfType = goalBlock.GetGroupOfType("locked-model");
					ModelData modelData = Blocksworld.modelCollection.FindSimilarModel(groupOfType.GetBlockList());
					if (modelData == null)
					{
						BWLog.Warning("Could not find a similar model to place");
					}
					else
					{
						gAF = modelData.tile.gaf;
					}
				}
				else
				{
					BWLog.Warning("Model collection was null");
				}
			}
			else
			{
				gAF = goalBlock.GetIconGaf();
			}
		}
		if (HelpTapTab(gAF))
		{
			return;
		}
		Blocksworld.enabledGAFs = new List<GAF> { gAF };
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tile tile2 = Blocksworld.buildPanel.FindTileMatching(gAF);
		if (tile2 == null)
		{
			BWLog.Warning("Could not find tile for gaf " + gAF);
			Blocksworld.UpdateTiles();
			stepOnNextUpdate = true;
			return;
		}
		bool flag2 = AutoCameraEnabled();
		rotateBeforeScale = false;
		paintBeforePose = false;
		textureBeforePose = false;
		autoAnglesFilter = Vector3.zero;
		useTwoFingerScale = false;
		useTwoFingerMove = false;
		includeDefaultBlockTiles = false;
		if (HelpOperationPose(TutorialState.CreateBlock))
		{
			return;
		}
		if (manualGoalHint != null)
		{
			object[] args = manualGoalHint.gaf.Args;
			gluePointBlock = (Vector3)args[0];
			glueNormal = (Vector3)args[1];
			Vector3 euler = (Vector3)args[2];
			autoCameraDist = (float)args[3];
			autoCameraEndPos = gluePointBlock - autoCameraDist * (Quaternion.Euler(euler) * Vector3.forward);
			rotateBeforeScale = args.Length > 4 && (int)args[4] != 0;
			paintBeforePose = Util.GetIntBooleanArg(args, 5, defaultValue: false);
			textureBeforePose = Util.GetIntBooleanArg(args, 6, defaultValue: false);
			autoAnglesFilter = Util.GetVector3Arg(args, 7, Vector3.zero);
			useTwoFingerScale = Util.GetIntBooleanArg(args, 8, defaultValue: false);
			useTwoFingerMove = Util.GetIntBooleanArg(args, 9, defaultValue: false);
			includeDefaultBlockTiles = Util.GetIntBooleanArg(args, 10, defaultValue: false);
			if (flag2 && !Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				Blocksworld.blocksworldCamera.Unfollow();
				Blocksworld.blocksworldCamera.SetTargetPosition(gluePointBlock);
				HelpAutoCamera();
				return;
			}
		}
		else
		{
			autoCameraDist = 15f + 2f * (Util.MaxComponent(goalBlock.Scale()) - 1f);
			FindGluePoint(goalBlock);
			if (flag2 && FindCameraAngleForPoint(gluePointBlock, ignoreCameraIsLevel: false, autoCameraDist * 0.5f, autoCameraDist * 2f) && !Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				HelpAutoCamera();
				return;
			}
		}
		if (goalBlock != null && goalBlock.go != null)
		{
			Vector3 vector = Blocksworld.mainCamera.WorldToScreenPoint(goalBlock.go.transform.position);
			float num = 0.25f * (float)Screen.width;
			float num2 = 0.6f * (float)Screen.width;
			float num3 = 0.1f * (float)Screen.height;
			float num4 = 0.9f * (float)Screen.height;
			if (!new Rect(num, num3, num2 - num, num4 - num3).Contains(new Vector2(vector.x, vector.y)))
			{
				Blocksworld.blocksworldCamera.Focus();
			}
		}
		if (IsOutsideScreen(tile2))
		{
			HelpScrollSidePanel(tile2);
			scrollToTile = tile2;
			return;
		}
		state = TutorialState.CreateBlock;
		helpPositionTarget = gluePointBlock;
		arrow1.state = TrackingState.TileWorld;
		arrow1.tile = tile2.tileObject;
		arrow1.world = helpPositionTarget;
		arrow1.Show(show: true);
		target1.TargetTile(tile2.tileObject);
		target2.TargetSurface(helpPositionTarget, glueNormal);
		target2.Show(show: true);
		hand1.DragArrow(arrow1);
	}

	private static Vector3 GetHelpOrbitScreenPoint()
	{
		return new Vector3(((float)NormalizedScreen.width - Blocksworld.buildPanel.width) / 2f, 80f, 0f);
	}

	private static void ShowHelpOrbit(int leftOf)
	{
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		autoCameraAngle = NearestCameraAngleSnap(30f, Blocksworld.cameraTransform.eulerAngles.y + (float)(90 * leftOf));
	}

	private static void HelpOrbit(Vector3? maybeNormal = null)
	{
		Vector3 valueOrDefault = maybeNormal.GetValueOrDefault(glueNormal);
		state = TutorialState.Orbit;
		int num = Util.LeftOf(valueOrDefault, Blocksworld.cameraForward, Vector3.up);
		if (num == 0)
		{
			num = 1;
		}
		ShowHelpOrbit(num);
	}

	private static bool CameraIsLevel()
	{
		TBox.Update();
		if (TBox.tileButtonMove != null)
		{
			return TBox.IsMoveUp();
		}
		return false;
	}

	private static void HelpDestroyBlock(Block b)
	{
		TutorialActions.EnterContext(TutorialActionContext.RemoveBlock);
		Blocksworld.locked.Remove(b);
		bool flag = AutoCameraEnabled();
		if (flag && removeBlockTile != null)
		{
			autoCameraEndPos = (Vector3)removeBlockTile.gaf.Args[1];
			if (!Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				HelpAutoCamera();
				return;
			}
		}
		if (Blocksworld.selectedBlock != b)
		{
			HelpTapBlock(b);
			return;
		}
		b.EnableCollider(value: false);
		if (flag && (removeBlockTile != null || FindCameraAngleForPoint(b.GetPosition() + Vector3.up, ignoreCameraIsLevel: true)) && !Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
		{
			HelpAutoCamera();
		}
		else
		{
			state = TutorialState.DestroyBlock;
			Blocksworld.enabledGAFs = new List<GAF>();
			Blocksworld.enabledPanelBlock = null;
			Blocksworld.UpdateTiles();
			arrow1.state = TrackingState.Block2Screen;
			arrow1.block = b;
			arrow1.screen = new Vector3((float)NormalizedScreen.width - Blocksworld.buildPanel.width / 2f, Util.WorldToScreenPoint(b.go.transform.position, z: false).y, 0f);
			arrow1.Show(show: true);
			hand1.DragArrow(arrow1);
			target1.TargetBlock(b);
		}
		b.EnableCollider(value: true);
	}

	private static Vector3 FindSidePointForBlock(Block newBlock)
	{
		Vector3 position = newBlock.go.transform.position;
		float num = Mathf.Max(autoCameraDist, 5f + 2.5f * Util.MaxComponent(Util.Abs(newBlock.Scale())));
		Vector3 vector = -(num * (Quaternion.Euler(5f, Blocksworld.cameraTransform.eulerAngles.y, 0f) * Vector3.forward));
		int num2 = 16;
		float num3 = float.MinValue;
		Vector3 vector2 = vector;
		for (int i = 0; i < num2; i++)
		{
			float num4 = 0f;
			float angle = 360f * ((float)i / (float)num2);
			Vector3 vector3 = Quaternion.AngleAxis(angle, Vector3.up) * vector;
			RaycastHit[] array = Physics.RaycastAll(position, vector3.normalized, autoCameraDist);
			float num5 = autoCameraDist;
			RaycastHit[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				RaycastHit raycastHit = array2[j];
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, checkChildGos: true);
				if (block != null && block != newBlock)
				{
					num5 = Mathf.Min((raycastHit.point - position).magnitude, num5);
				}
			}
			num4 -= autoCameraDist - num5;
			num4 -= (vector - vector3).magnitude * 0.01f;
			if (num4 > num3)
			{
				num3 = num4;
				vector2 = vector3;
			}
		}
		return position + vector2;
	}

	private static void CheatRotateScaleMove(Block newBlock, Block goalBlock)
	{
		newBlock.RotateTo(goalBlock.GetRotation());
		newBlock.ScaleTo(goalBlock.Scale(), recalculateCollider: true, forceRescale: true);
		newBlock.MoveTo(goalBlock.GetPosition());
		cheatNextStep = false;
		stepOnNextUpdate = true;
		TBox.FitToSelected();
		ConnectednessGraph.Update(newBlock);
	}

	private static void HelpScale(Block newBlock)
	{
		if (cheatNextStep)
		{
			CheatRotateScaleMove(newBlock, goalBlock);
			return;
		}
		if (Blocksworld.selectedBlock != newBlock)
		{
			HelpTapBlock(newBlock);
			return;
		}
		state = TutorialState.Scale;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		Vector3 vector = goalBlock.Scale();
		Vector3 vector2 = newBlock.Scale();
		if (newBlock is BlockAbstractWheel && vector.y == vector.z && vector2.y == vector2.z)
		{
			vector2.y = vector.y;
		}
		Quaternion rotation = newBlock.go.transform.rotation;
		Vector3 v = rotation * vector;
		Vector3 v2 = rotation * vector2;
		Vector3 vector3 = Util.Abs(v) - Util.Abs(v2);
		vector3.x = ((Mathf.Abs(vector3.x) >= 0.01f) ? vector3.x : 0f);
		vector3.y = ((Mathf.Abs(vector3.y) >= 0.01f) ? vector3.y : 0f);
		vector3.z = ((Mathf.Abs(vector3.z) >= 0.01f) ? vector3.z : 0f);
		bool flag = AutoCameraEnabled();
		bool flag2 = Mathf.Abs(vector3.y) > 0.01f;
		Vector3 position = Blocksworld.cameraTransform.position;
		if (flag2)
		{
			if (flag && !CameraIsLevel() && !useTwoFingerScale)
			{
				autoCameraEndPos = FindSidePointForBlock(newBlock);
				if (!Close(autoCameraEndPos, position))
				{
					HelpAutoCamera();
					return;
				}
			}
		}
		else if (flag)
		{
			float num = Mathf.Max(autoCameraDist, 5f + 2.5f * Util.MaxComponent(Util.Abs(vector)));
			autoCameraEndPos = newBlock.go.transform.position - num * (Quaternion.Euler(30f, Blocksworld.cameraTransform.eulerAngles.y, 0f) * Vector3.forward);
			if (!Close(autoCameraEndPos, position))
			{
				HelpAutoCamera();
				return;
			}
		}
		if ((vector3.x < 0f && Vector3.Dot(new Vector3(vector3.x, 0f, 0f), Blocksworld.cameraForward) < 0f) || (vector3.x > 0f && Vector3.Dot(new Vector3(vector3.x, 0f, 0f), Blocksworld.cameraForward) > 0f))
		{
			vector3.x = 0f - vector3.x;
		}
		if ((vector3.z < 0f && Vector3.Dot(new Vector3(0f, 0f, vector3.z), Blocksworld.cameraForward) < 0f) || (vector3.z > 0f && Vector3.Dot(new Vector3(0f, 0f, vector3.z), Blocksworld.cameraForward) > 0f))
		{
			vector3.z = 0f - vector3.z;
		}
		if (CameraIsLevel() || flag2)
		{
			vector3.x = (vector3.z = 0f);
		}
		TBox.tileButtonScale.Show(show: true);
		TBox.tileButtonScale.Enable(enable: true);
		TBox.UpdateButtons();
		Vector3 scaleButtonInWorldSpace = TBox.GetScaleButtonInWorldSpace();
		arrow1.state = TrackingState.Button2World;
		arrow1.tile = TBox.tileButtonScale;
		arrow1.world = scaleButtonInWorldSpace + vector3;
		Hand hand = hand1;
		arrow1.Show(show: true);
		if (flag2 && Mathf.Abs(vector3.x) < 0.01f && Mathf.Abs(vector3.z) < 0.01f && useTwoFingerScale && !CameraIsLevel())
		{
			hand = hand2;
		}
		target1.TargetTile(TBox.tileButtonScale);
		hand.screenOffset = Vector3.zero;
		if (hand == hand2)
		{
			float num2 = 80f;
			hand.screenOffset = Vector3.right * 25f;
			arrow2.state = TrackingState.Screen2Screen;
			Vector3 vector4 = Vector3.right * num2;
			arrow2.screen = arrow1.tile.GetPosition() + new Vector3(40f, 40f, 0f) + vector4;
			arrow2.screen2 = Util.WorldToScreenPoint(arrow1.world, z: false) + vector4;
			arrow2.Show(show: true);
			target2.TargetScreen(arrow2.screen);
		}
		hand.DragArrow(arrow1);
		if (Util.MaxComponent(vector) > 12f)
		{
			TBox.targetSnapScale = vector;
			TBox.targetSnapScaleMaxDistance = Util.Round(vector * 0.1f);
		}
	}

	private static void FindRotations(Quaternion from, Quaternion to, int n)
	{
		if (Quaternion.Angle(from, to) < 5f)
		{
			bestN = n;
			for (int i = 0; i < n; i++)
			{
				best[i] = rots[i];
			}
		}
		else if (n < bestN)
		{
			rots[n].x = 0f;
			rots[n].y = 90f;
			rots[n].z = 0f;
			FindRotations(Quaternion.Euler(rots[n]) * from, to, n + 1);
			rots[n].x = 0f;
			rots[n].y = 180f;
			rots[n].z = 0f;
			FindRotations(Quaternion.Euler(rots[n]) * from, to, n + 1);
			rots[n].x = 0f;
			rots[n].y = -90f;
			rots[n].z = 0f;
			FindRotations(Quaternion.Euler(rots[n]) * from, to, n + 1);
			rots[n] = 90f * dragYRotationAxis;
			FindRotations(Quaternion.Euler(rots[n]) * from, to, n + 1);
			rots[n] = 180f * dragYRotationAxis;
			FindRotations(Quaternion.Euler(rots[n]) * from, to, n + 1);
			rots[n] = -90f * dragYRotationAxis;
			FindRotations(Quaternion.Euler(rots[n]) * from, to, n + 1);
		}
	}

	private static void HelpRotation(Block newBlock, Quaternion rotation)
	{
		bool flag = newBlock.HasGroup("locked-model");
		if (cheatNextStep && !flag)
		{
			CheatRotateScaleMove(newBlock, goalBlock);
			return;
		}
		if (Blocksworld.selectedBlock != newBlock && !flag)
		{
			HelpTapBlock(newBlock);
			return;
		}
		state = TutorialState.Rotation;
		int num = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
		dragYRotationAxis = -Vector3.right;
		switch (num)
		{
		case 1:
			dragYRotationAxis = Vector3.forward;
			break;
		case 2:
			dragYRotationAxis = Vector3.right;
			break;
		case 3:
			dragYRotationAxis = -Vector3.forward;
			break;
		}
		bestN = 4;
		FindRotations(newBlock.go.transform.rotation, rotation, 0);
		Vector3 off = new Vector3(0f - best[0].y, 0f, 0f);
		switch (num)
		{
		case 3:
			off.y = best[0].z;
			break;
		case 2:
			off.y = 0f - best[0].x;
			break;
		case 1:
			off.y = 0f - best[0].z;
			break;
		default:
			off.y = best[0].x;
			break;
		}
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		TBox.tileButtonRotate.Show(show: true);
		TBox.tileButtonRotate.Enable(enable: true);
		TBox.UpdateButtons();
		arrow1.state = TrackingState.Tile2Screen;
		arrow1.tile = TBox.tileButtonRotate;
		arrow1.screen = TBox.tileButtonRotate.GetCenterPosition() + off;
		if (AutoCameraEnabled() && TBox.tileButtonRotate != null && FindCameraAngleForScreenPoints(delegate
		{
			TBox.UpdateButtons();
			return TBox.tileButtonRotate.GetPosition();
		}, () => TBox.tileButtonRotate.GetPosition() + off))
		{
			HelpAutoCamera();
			return;
		}
		arrow1.Show(show: true);
		hand1.DragArrow(arrow1);
		target1.TargetTile(TBox.tileButtonRotate);
	}

	private static void FindRotations2D(Quaternion view, Quaternion from, Quaternion to, int n)
	{
		if (Quaternion.Angle(from, to) < 5f)
		{
			bestN2D = n;
			for (int i = 0; i < n; i++)
			{
				best2D[i] = rots2D[i];
			}
		}
		else if (n < bestN2D)
		{
			if (n == 0 || rots2D[n - 1].z == 0f)
			{
				rots2D[n].x = 0f;
				rots2D[n].y = 0f;
				rots2D[n].z = 90f;
				FindRotations2D(view, Quaternion.AngleAxis(90f, view * Vector3.forward) * from, to, n + 1);
				rots2D[n].x = 0f;
				rots2D[n].y = 0f;
				rots2D[n].z = 180f;
				FindRotations2D(view, Quaternion.AngleAxis(180f, view * Vector3.forward) * from, to, n + 1);
				rots2D[n].x = 0f;
				rots2D[n].y = 0f;
				rots2D[n].z = 270f;
				FindRotations2D(view, Quaternion.AngleAxis(270f, view * Vector3.forward) * from, to, n + 1);
			}
			rots2D[n].x = (((view * Vector3.forward).y != 0f) ? (-90) : 90);
			rots2D[n].y = 0f;
			rots2D[n].z = 0f;
			FindRotations2D(view * Quaternion.Euler(rots2D[n]), from, to, n + 1);
			rots2D[n].x = 0f;
			rots2D[n].y = 90f;
			rots2D[n].z = 0f;
			FindRotations2D(view * Quaternion.Euler(rots2D[n]), from, to, n + 1);
			rots2D[n].x = 0f;
			rots2D[n].y = -90f;
			rots2D[n].z = 0f;
			FindRotations2D(view * Quaternion.Euler(rots2D[n]), from, to, n + 1);
		}
	}

	private static void FindTexturePoint(Block block, Vector3 normal)
	{
		Vector3 origin = block.go.transform.position + block.go.GetComponent<Collider>().bounds.extents.magnitude * normal;
		if (Physics.Raycast(new Ray(origin, -normal), out var hitInfo, 10f))
		{
			glueNormal = hitInfo.normal;
			gluePointWorld = hitInfo.point;
			gluePointBlock = gluePointWorld;
		}
		else
		{
			BWLog.Info("Couldn't find normal of face to apply texture to (shouldn't happen)");
			glueNormal = Vector3.up;
			gluePointWorld = block.go.transform.position;
			gluePointBlock = gluePointWorld;
		}
	}

	private static void FindGluePoint(Block block, Block ignore = null)
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Block> list3 = new List<Block>();
		ConnectednessGraph.GlueBonds(block, ignore, list, list2, list3);
		Vector3 vector = Util.Abs(block.GetRotation() * block.GetScale());
		Vector3 vector2 = block.go.transform.position - 0.5f * vector.y * Vector3.up;
		bool flag = Util.PointWithinTerrain(vector2 - 0.5f * Vector3.up);
		bool flag2 = list2.Count == 0 || flag;
		if (!flag2)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				gluePointBlock = list[i];
				gluePointWorld = list2[i];
				glueNormal = Vector3.zero;
				Block block2 = list3[i];
				Vector3 vector3 = block2.go.transform.position - gluePointWorld;
				if (vector3 != Vector3.zero && block2.go.GetComponent<Collider>().Raycast(new Ray(gluePointWorld - vector3, vector3), out var hitInfo, 10f))
				{
					glueNormal = hitInfo.normal;
					if (!(glueNormal.y >= -0.01f))
					{
						flag2 = true;
						continue;
					}
				}
				if (TestVisibility(gluePointBlock, Blocksworld.cameraTransform.position))
				{
					return;
				}
			}
		}
		if (!flag2)
		{
			return;
		}
		if (!flag)
		{
			if (Physics.Raycast(vector2 + Vector3.up * 0.1f, -Vector3.up, out var hitInfo2))
			{
				float magnitude = (vector2 - hitInfo2.point).magnitude;
				Block block3 = BWSceneManager.FindBlock(hitInfo2.collider.gameObject, checkChildGos: true);
				float num = Util.MaxComponent(block.size);
				if (magnitude >= num + 9f || block3 == null || block3 is BlockSky)
				{
					SetPlacementHelper(vector2);
				}
				else
				{
					vector2 = hitInfo2.point;
				}
			}
			else
			{
				SetPlacementHelper(vector2);
			}
		}
		glueNormal = Vector3.up;
		gluePointWorld = vector2;
		gluePointBlock = vector2;
	}

	private static void DisablePlacementHelper()
	{
		if (placementHelper != null)
		{
			BoxCollider component = placementHelper.GetComponent<BoxCollider>();
			component.enabled = false;
		}
	}

	public static void DestroyPlacementHelper()
	{
		if (placementHelper != null)
		{
			UnityEngine.Object.Destroy(placementHelper);
		}
	}

	private static GameObject SetPlacementHelper(Vector3 top)
	{
		if (placementHelper == null)
		{
			placementHelper = new GameObject("Block Placement Helper");
			placementHelper.AddComponent<BoxCollider>();
		}
		BoxCollider component = placementHelper.GetComponent<BoxCollider>();
		component.enabled = true;
		float num = 0.1f;
		component.size = new Vector3(3f, num, 3f);
		placementHelper.transform.position = top - Vector3.up * num * 0.5f;
		return placementHelper;
	}

	private static bool FindCameraAngleForPoint(Vector3 targetPos, bool ignoreCameraIsLevel = false, float minDistance = 0f, float maxDistance = float.MaxValue)
	{
		float magnitude = (targetPos - Blocksworld.cameraTransform.position).magnitude;
		if ((ignoreCameraIsLevel || !CameraIsLevel()) && magnitude >= minDistance && magnitude <= maxDistance && TestCameraVisibility(targetPos))
		{
			return false;
		}
		int num = 8;
		int num2 = int.MinValue;
		Vector3[] array = new Vector3[4]
		{
			0.5f * Vector3.forward,
			-0.5f * Vector3.forward,
			0.5f * Vector3.right,
			-0.5f * Vector3.right
		};
		float[] array2 = new float[3] { -30f, -45f, -60f };
		Vector3 position = Blocksworld.cameraTransform.position;
		List<Vector3> list = new List<Vector3>();
		list.Add(position);
		list.Add(targetPos + (position - targetPos).normalized * autoCameraDist);
		float[] array3 = array2;
		foreach (float x in array3)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 item = targetPos + autoCameraDist * (Quaternion.Euler(x, j * (360 / num) + 45, 0f) * Vector3.forward);
				list.Add(item);
			}
		}
		bool result = false;
		for (int k = 0; k < list.Count; k++)
		{
			Vector3 vector = list[k];
			if (!TestVisibility(vector, targetPos, 0.6f))
			{
				continue;
			}
			Vector3 vector2 = vector - position;
			float magnitude2 = (vector - targetPos).magnitude;
			int num3 = -Mathf.RoundToInt(Mathf.Min(autoCameraDist, vector2.magnitude) * 5f);
			if (magnitude2 >= minDistance && magnitude2 <= maxDistance)
			{
				num3 += 1000;
			}
			Vector3[] array4 = array;
			foreach (Vector3 vector3 in array4)
			{
				if (TestCameraVisibility(targetPos + vector3, vector))
				{
					num3 += 10000;
				}
			}
			if (num3 > num2)
			{
				num2 = num3;
				autoCameraEndPos = vector;
				result = true;
			}
		}
		return result;
	}

	private static bool FindCameraAngleForScreenPoints(Func<Vector3> p1, Func<Vector3> p2)
	{
		if (!PointInsideScreen(p1()) || !PointInsideScreen(p2()))
		{
			Vector3 position = Blocksworld.cameraTransform.position;
			for (int i = 0; i < 10; i++)
			{
				Blocksworld.blocksworldCamera.SetCameraPosition(position - i * 5 * Blocksworld.cameraForward);
				if (PointInsideScreen(p1()) && PointInsideScreen(p2()))
				{
					if (i > 0)
					{
						autoCameraEndPos = Blocksworld.cameraTransform.position;
					}
					Blocksworld.blocksworldCamera.SetCameraPosition(position);
					return true;
				}
			}
			Blocksworld.blocksworldCamera.SetCameraPosition(position);
			return false;
		}
		return false;
	}

	private static bool TestVisibility(Vector3 p1, Vector3 p2, float stopShort = 0.1f)
	{
		return !Physics.Raycast(p1, p2 - p1, (p2 - p1).magnitude - stopShort);
	}

	private static bool TestCameraVisibility(Vector3 targetPos, Vector3? camPos = null)
	{
		float num = 0.1f;
		Vector3 vector = ((!camPos.HasValue) ? Blocksworld.cameraTransform.position : camPos.Value);
		if (GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(targetPos, Vector3.one * 2f)))
		{
			return !Physics.Raycast(vector, (targetPos - vector).normalized, (targetPos - vector).magnitude - num);
		}
		return false;
	}

	private static void HelpPosition(Block newBlock, bool moveModel = false)
	{
		Blocksworld.locked.Remove(newBlock);
		if (cheatNextStep && !moveModel)
		{
			CheatRotateScaleMove(newBlock, goalBlock);
			return;
		}
		state = TutorialState.Position;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		bool flag = moveModel && (Blocksworld.selectedBunch == null || !Blocksworld.selectedBunch.blocks.Contains(newBlock));
		bool flag2 = goalBlock.HasGroup("locked-model");
		if ((!moveModel && !flag2 && Blocksworld.selectedBlock != newBlock) || flag)
		{
			HelpTapBlock(newBlock, flag);
			return;
		}
		Vector3 position = goalBlock.go.transform.position;
		Vector3 vector = newBlock.go.transform.position;
		Vector3 vector2 = position - vector;
		helpPositionTarget = position;
		if (!moveModel && !flag2)
		{
			if (TutorialSnap(vector, position))
			{
				vector = position;
				newBlock.MoveTo(vector);
				TBox.FitToSelected();
				stepOnNextUpdate = true;
				state = TutorialState.DetermineInstructions;
				ConnectednessGraph.Update(newBlock);
				return;
			}
			if (TutorialSnapXZ(vector, position))
			{
				vector = new Vector3(position.x, vector.y, position.z);
				newBlock.MoveTo(vector);
				TBox.FitToSelected();
			}
			else if (TutorialSnapY(vector, position))
			{
				vector = new Vector3(vector.x, position.y, vector.z);
				newBlock.MoveTo(vector);
				TBox.FitToSelected();
			}
		}
		if (Mathf.Abs(vector2.y) < Mathf.Epsilon && (Blocksworld.selectedBlock == newBlock || moveModel || flag2))
		{
			TBox.tileButtonMove.Show(show: true);
			TBox.tileButtonMove.Enable(enable: true);
			TBox.UpdateButtons();
		}
		if (moveModel || flag2)
		{
			HelpPositionMove(newBlock, vector, position);
			return;
		}
		List<Vector3> resultPos = new List<Vector3>();
		List<Vector3> resultPos2 = new List<Vector3>();
		List<Block> resultBlock = new List<Block>();
		ConnectednessGraph.GlueBonds(newBlock, null, resultPos, resultPos2, resultBlock);
		HelpPositionMove(newBlock, vector, position);
	}

	private static void HelpPositionMove(Block newBlock, Vector3 fromPos, Vector3 toPos)
	{
		bool flag = AutoCameraEnabled();
		Vector3 vector = toPos - fromPos;
		bool flag2 = Mathf.Abs(vector.x) < 0.01f && Mathf.Abs(vector.z) < 0.01f && Mathf.Abs(vector.y) > 0.01f;
		if (!useTwoFingerMove && flag && flag2 && !CameraIsLevel())
		{
			autoCameraEndPos = FindSidePointForBlock(newBlock);
			if (!Close(Blocksworld.cameraTransform.position, autoCameraEndPos))
			{
				HelpAutoCamera();
				return;
			}
		}
		else if (flag && (vector.x != 0f || vector.z != 0f))
		{
			float num = Mathf.Max(autoCameraDist, 5f + 2.5f * Mathf.Max(Util.MaxComponent(Util.Abs(newBlock.Scale())), Util.MaxComponent(Util.Abs(vector))));
			autoCameraEndPos = newBlock.go.transform.position - num * (Quaternion.Euler(30f, Blocksworld.cameraTransform.eulerAngles.y, 0f) * Vector3.forward);
			if (!Close(Blocksworld.cameraTransform.position, autoCameraEndPos))
			{
				HelpAutoCamera();
				return;
			}
		}
		TBox.tileButtonMove.Show(show: true);
		TBox.tileButtonMove.Enable(enable: true);
		TBox.UpdateButtons();
		Hand hand = hand1;
		if (flag2 && useTwoFingerMove)
		{
			if (!CameraIsLevel())
			{
				hand = hand2;
			}
		}
		else if (!CameraIsLevel())
		{
			vector.y = 0f;
		}
		if (TBox.tileButtonMove.IsShowing() && !TBox.IsRaycastDragging())
		{
			arrow1.state = TrackingState.MoveButtonHelper;
			Vector3 world = ((!flag2) ? new Vector3(toPos.x, fromPos.y, toPos.z) : toPos);
			arrow1.world = world;
			float num2 = Util.ScreenToWorldScale(fromPos) / Util.ScreenToWorldScale(toPos);
			arrow1.offset = num2 * (TBox.tileButtonMove.GetCenterPosition() - Util.WorldToScreenPointSafe(fromPos));
			target1.TargetTile(TBox.tileButtonMove);
		}
		else
		{
			arrow1.state = TrackingState.Block2World;
			arrow1.block = newBlock;
			arrow1.world = toPos;
		}
		arrow1.Show(show: true);
		hand.DragArrow(arrow1);
		Vector3 vector2 = newBlock.Scale();
		if (Util.MaxComponent(vector2) > 12f)
		{
			Transform transform = newBlock.go.transform;
			Vector3 vector3 = Quaternion.Inverse(transform.rotation) * vector2;
			TBox.targetSnapPosition = transform.position + vector;
			TBox.targetSnapPositionMaxDistance = Util.Abs(Util.Round(vector3 * 0.1f));
		}
		hand.screenOffset = Vector3.zero;
		if (hand == hand2)
		{
			float num3 = 80f;
			hand.screenOffset = Vector3.right * 25f;
			arrow2.state = TrackingState.Screen2Screen;
			Vector3 vector4 = Vector3.right * num3;
			arrow2.screen = arrow1.tile.GetPosition() + new Vector3(40f, 40f, 0f) + vector4;
			arrow2.screen2 = Util.WorldToScreenPoint(arrow1.world, z: false) + vector4;
			arrow2.Show(show: true);
			target1.TargetScreen(arrow2.screen);
		}
		FindGluePoint(goalBlock);
		target2.TargetSurface(gluePointBlock, glueNormal);
	}

	private static bool TutorialSnap(Vector3 p1, Vector3 p2)
	{
		if ((p1 - p2).magnitude < 1.25f)
		{
			return Mathf.Abs(p1.y - p2.y) < 0.5f;
		}
		return false;
	}

	private static bool TutorialSnapXZ(Vector3 p1, Vector3 p2)
	{
		Vector3 vector = p1 - p2;
		float num = 0.2f;
		bool flag = false;
		bool flag2 = false;
		if (Mathf.Abs(vector.x) < num)
		{
			flag = true;
		}
		if (Mathf.Abs(vector.z) < num)
		{
			flag2 = true;
		}
		return flag && flag2;
	}

	private static bool TutorialSnapY(Vector3 p1, Vector3 p2)
	{
		Vector3 vector = p1 - p2;
		float num = 0.2f;
		bool result = false;
		if (Mathf.Abs(vector.y) < num)
		{
			result = true;
		}
		return result;
	}

	private static void HelpPositionMoveTo(Block newBlock)
	{
		bool flag = AutoCameraEnabled();
		FindGluePoint(goalBlock);
		if (flag && FindCameraAngleForPoint(gluePointBlock, ignoreCameraIsLevel: true) && !Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
		{
			HelpAutoCamera();
			return;
		}
		arrow1.state = TrackingState.BlockOffsetWorld;
		arrow1.block = newBlock;
		arrow1.offset = Vector3.zero;
		arrow1.world = gluePointBlock;
		target1.TargetSurface(gluePointBlock, -glueNormal);
		target2.TargetSurface(gluePointBlock, glueNormal);
		arrow1.Show(show: true);
		hand1.DragArrow(arrow1);
	}

	private static void HelpColor(Block block, GAF gafPaintTo, Vector3 cameraPos, Vector3 cameraTarget)
	{
		string text = (string)gafPaintTo.Args[0];
		forceMeshIndex = ((gafPaintTo.Args.Length > 1) ? ((int)gafPaintTo.Args[1]) : 0);
		if (cheatNextStep)
		{
			block.PaintTo(text, permanent: true, forceMeshIndex);
			stepOnNextUpdate = true;
			cheatNextStep = false;
			return;
		}
		GAF lastPaintedColorGAF = Blocksworld.clipboard.GetLastPaintedColorGAF();
		usingQuickSelectColorIcon = text == (string)lastPaintedColorGAF.Args[0];
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		quickSelect.UpdateColorIcon();
		Vector2 vector = quickSelect.PaintRectCenter();
		if (!usingQuickSelectColorIcon && HelpTapTab(gafPaintTo))
		{
			return;
		}
		GAF gAF = new GAF("Block.PaintTo", text);
		Blocksworld.enabledGAFs = new List<GAF> { gAF };
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tile tile = Blocksworld.buildPanel.FindTileMatching(gAF);
		if (tile == null)
		{
			BWLog.Error("Could not find tile for gaf " + new GAF("Block.PaintTo", text));
		}
		bool flag = AutoCameraEnabled();
		if (HelpOperationPose(TutorialState.Color, forceMeshIndex))
		{
			return;
		}
		block.EnableCollider(value: false);
		Vector3 position = Blocksworld.cameraTransform.position;
		if (flag && cameraPos != Util.nullVector3 && (!Close(autoCameraEndPos, position) || !Close((cameraTarget - cameraPos).normalized, Blocksworld.cameraForward)))
		{
			autoCameraEndPos = cameraPos;
			Blocksworld.blocksworldCamera.Unfollow();
			Blocksworld.blocksworldCamera.SetTargetPosition(cameraTarget);
			HelpAutoCamera();
		}
		else if (flag && Util.IsNullVector3(cameraPos) && FindCameraAngleForPoint(block.go.transform.position, ignoreCameraIsLevel: false, autoCameraDist * 0.5f, autoCameraDist * 1.5f) && !Close(autoCameraEndPos, position))
		{
			HelpAutoCamera();
		}
		else if (!usingQuickSelectColorIcon && IsOutsideScreen(tile))
		{
			HelpScrollSidePanel(tile);
			scrollToTile = tile;
		}
		else
		{
			bool flag2 = true;
			state = TutorialState.Color;
			if (!Util.IsNullVector3(cameraPos))
			{
				if (usingQuickSelectColorIcon)
				{
					target1.TargetScreen(vector);
					arrow1.state = TrackingState.Screen2Screen;
					arrow1.screen = vector;
					arrow1.screen2 = CenterOfScreen();
				}
				else
				{
					target1.TargetTile(tile.tileObject);
					arrow1.state = TrackingState.Tile2Screen;
					arrow1.tile = tile.tileObject;
					arrow1.screen = CenterOfScreen();
				}
				target2.TargetScreen(CenterOfScreen());
			}
			else if (forceMeshIndex != 0)
			{
				Vector3 center = block.GetSubMesh(forceMeshIndex).AABB.center;
				if (flag && !TestCameraVisibility(center) && FindCameraAngleForPoint(center) && !Close(autoCameraEndPos, position))
				{
					Blocksworld.blocksworldCamera.Unfollow();
					Blocksworld.blocksworldCamera.SetTargetPosition(center);
					HelpAutoCamera();
					flag2 = false;
				}
				else
				{
					if (usingQuickSelectColorIcon)
					{
						target1.TargetScreen(vector);
						arrow1.state = TrackingState.Screen2World;
						arrow1.screen = vector;
					}
					else
					{
						target1.TargetTile(tile.tileObject);
						arrow1.state = TrackingState.TileWorld;
						arrow1.tile = tile.tileObject;
					}
					arrow1.world = center;
					target2.TargetWorld(arrow1.world);
				}
			}
			else
			{
				Vector3 position2 = block.go.transform.position;
				if (flag && !TestCameraVisibility(position2) && FindCameraAngleForPoint(position2) && !Close(autoCameraEndPos, position))
				{
					Blocksworld.blocksworldCamera.Unfollow();
					Blocksworld.blocksworldCamera.SetTargetPosition(position2);
					HelpAutoCamera();
					flag2 = false;
				}
				else
				{
					if (usingQuickSelectColorIcon)
					{
						target1.TargetScreen(vector);
						arrow1.state = TrackingState.Screen2Block;
						arrow1.screen = vector;
					}
					else
					{
						target1.TargetTile(tile.tileObject);
						arrow1.state = TrackingState.TileBlock;
						arrow1.tile = tile.tileObject;
					}
					arrow1.block = block;
					target2.TargetBlock(block);
				}
			}
			if (flag2)
			{
				arrow1.Show(show: true);
				hand1.DragArrow(arrow1);
			}
		}
		block.EnableCollider(value: true);
	}

	private static void HelpTexture(Block newBlock, GAF gafTextureTo, Vector3 cameraPos, Vector3 cameraTarget)
	{
		string text = (string)gafTextureTo.Args[0];
		Vector3 vector = (Vector3)gafTextureTo.Args[1];
		forceMeshIndex = ((gafTextureTo.Args.Length > 2) ? ((int)gafTextureTo.Args[2]) : 0);
		if (cheatNextStep)
		{
			newBlock.TextureTo(text, vector, permanent: true, forceMeshIndex);
			stepOnNextUpdate = true;
			cheatNextStep = false;
			return;
		}
		vector = goalBlock.go.transform.rotation * vector;
		GAF lastTextureGAF = Blocksworld.clipboard.GetLastTextureGAF();
		usingQuickSelectTextureIcon = text == (string)lastTextureGAF.Args[0];
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		quickSelect.UpdateTextureIcon();
		Vector2 vector2 = quickSelect.TextureRectCenter();
		if (!usingQuickSelectTextureIcon && HelpTapTab(gafTextureTo))
		{
			return;
		}
		GAF item = new GAF("Block.TextureTo", text, Vector3.zero);
		Blocksworld.enabledGAFs = new List<GAF> { item };
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		BuildPanel buildPanel = Blocksworld.buildPanel;
		string predicateName = "Block.TextureTo";
		Tile tile = buildPanel.FindTileMatching(new GAF(predicateName, text, null));
		if (tile == null)
		{
			object obj = "Could not find tile for gaf ";
			string predicateName2 = "Block.TextureTo";
			BWLog.Info(string.Concat(str1: new GAF(predicateName2, text, null).ToString(), str0: obj?.ToString()));
			if (newBlock != null)
			{
				BWLog.Info("Falling back to auto texturing the block instead...");
				newBlock.TextureTo(text, vector, permanent: true, forceMeshIndex);
				stepOnNextUpdate = true;
				return;
			}
		}
		if (HelpOperationPose(TutorialState.Texture, forceMeshIndex))
		{
			return;
		}
		bool flag = AutoCameraEnabled();
		Mapping mapping = Materials.GetMapping(text);
		if (Util.IsNullVector3(cameraPos) && (mapping == Mapping.OneSideTo1x1 || mapping == Mapping.TwoSidesTo1x1 || mapping == Mapping.TwoSidesWrapTo1x1 || mapping == Mapping.OneSideWrapTo1x1))
		{
			FindTexturePoint(newBlock, vector);
			if (flag && FindCameraAngleForPoint(gluePointBlock, ignoreCameraIsLevel: false, autoCameraDist * 0.5f, autoCameraDist * 1.5f) && !Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				HelpAutoCamera();
				return;
			}
			if (!usingQuickSelectTextureIcon && IsOutsideScreen(tile))
			{
				HelpScrollSidePanel(tile);
				scrollToTile = tile;
				return;
			}
			state = TutorialState.Texture;
			if (usingQuickSelectTextureIcon)
			{
				arrow1.state = TrackingState.Screen2World;
				arrow1.screen = vector2;
				target1.TargetScreen(vector2);
			}
			else
			{
				arrow1.state = TrackingState.TileWorld;
				arrow1.tile = tile.tileObject;
				target1.TargetTile(tile.tileObject);
			}
			arrow1.world = gluePointWorld;
			target2.TargetSurface(gluePointWorld, glueNormal);
			arrow1.Show(show: true);
			hand1.DragArrow(arrow1);
			return;
		}
		newBlock.EnableCollider(value: false);
		if (flag && !Util.IsNullVector3(cameraPos) && (!Close(autoCameraEndPos, Blocksworld.cameraTransform.position) || !Close((cameraTarget - cameraPos).normalized, Blocksworld.cameraForward)))
		{
			autoCameraEndPos = cameraPos;
			Blocksworld.blocksworldCamera.Unfollow();
			Blocksworld.blocksworldCamera.SetTargetPosition(cameraTarget);
			HelpAutoCamera();
		}
		else if (flag && Util.IsNullVector3(cameraPos) && FindCameraAngleForPoint(newBlock.go.transform.position) && !Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
		{
			HelpAutoCamera();
		}
		else if (!usingQuickSelectTextureIcon && IsOutsideScreen(tile))
		{
			HelpScrollSidePanel(tile);
			scrollToTile = tile;
		}
		else
		{
			state = TutorialState.Texture;
			if (usingQuickSelectTextureIcon)
			{
				target1.TargetScreen(vector2);
			}
			else
			{
				target1.TargetTile(tile.tileObject);
			}
			if (!Util.IsNullVector3(cameraPos))
			{
				if (usingQuickSelectTextureIcon)
				{
					arrow1.state = TrackingState.Screen2Screen;
					arrow1.screen = vector2;
					arrow1.screen2 = CenterOfScreen();
					target1.TargetScreen(vector2);
				}
				else
				{
					arrow1.state = TrackingState.Tile2Screen;
					arrow1.tile = tile.tileObject;
					arrow1.screen = CenterOfScreen();
				}
				target2.TargetScreen(CenterOfScreen());
			}
			else if (forceMeshIndex != 0)
			{
				if (usingQuickSelectTextureIcon)
				{
					arrow1.state = TrackingState.Screen2World;
					arrow1.screen = vector2;
					target1.TargetScreen(vector2);
				}
				else
				{
					arrow1.state = TrackingState.TileWorld;
					arrow1.tile = tile.tileObject;
				}
				arrow1.world = goalBlock.GetSubMesh(forceMeshIndex).AABB.center;
				target2.TargetWorld(arrow1.world);
			}
			else
			{
				if (usingQuickSelectTextureIcon)
				{
					arrow1.state = TrackingState.Screen2Block;
					arrow1.screen = vector2;
					target1.TargetScreen(vector2);
				}
				else
				{
					arrow1.state = TrackingState.TileBlock;
					arrow1.tile = tile.tileObject;
				}
				arrow1.block = goalBlock;
				target2.TargetBlock(goalBlock);
			}
			arrow1.Show(show: true);
			hand1.DragArrow(arrow1);
		}
		newBlock.EnableCollider(value: true);
	}

	private static void HelpRemoveTile(Block newBlock, Tile tile)
	{
		if (Blocksworld.selectedBlock != newBlock)
		{
			HelpTapBlock(newBlock);
			return;
		}
		if (IsOutsideScreen(tile))
		{
			HelpDragScriptPanel(tile);
			return;
		}
		state = TutorialState.RemoveTile;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		arrow1.state = TrackingState.Tile2Screen;
		arrow1.tile = tile.tileObject;
		Vector3[] array = new Vector3[4]
		{
			new Vector3((float)NormalizedScreen.width - Blocksworld.buildPanel.width / 2f, tile.GetPosition().y, 0f),
			new Vector3((float)NormalizedScreen.width * 0.5f, 40f, 0f),
			new Vector3((float)NormalizedScreen.width * 0.5f, (float)NormalizedScreen.height - 40f, 0f),
			new Vector3(40f, (float)NormalizedScreen.height * 0.5f)
		};
		Vector3[] array2 = array;
		foreach (Vector3 vector in array2)
		{
			if (!Blocksworld.scriptPanel.Hit(vector))
			{
				arrow1.screen = vector;
				break;
			}
		}
		arrow1.Show(show: true);
		hand1.DragArrow(arrow1);
		target1.TargetTile(tile.tileObject);
	}

	private static void HelpSwapTiles(Block newBlock, Tile tile1, Tile tile2, float offsetSign = 1f)
	{
		if (Blocksworld.selectedBlock != newBlock)
		{
			HelpTapBlock(newBlock);
			return;
		}
		if (IsOutsideScreen(tile1))
		{
			HelpDragScriptPanel(tile1);
			return;
		}
		state = TutorialState.SwapTiles;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		arrow1.state = TrackingState.Tile2Tile;
		arrow1.tile = tile1.tileObject;
		arrow1.tile2 = tile2.tileObject;
		arrow1.offset = offsetSign * Vector3.right * 80f * 0.5f * NormalizedScreen.pixelScale;
		target1.TargetTile(tile1.tileObject);
		arrow1.Show(show: true);
		hand1.DragArrow(arrow1);
	}

	public static void HelpToggleTile(Block newBlock, Tile tile)
	{
		if (Blocksworld.selectedBlock != newBlock)
		{
			HelpTapBlock(newBlock);
			return;
		}
		if (IsOutsideScreen(tile))
		{
			HelpDragScriptPanel(tile);
			return;
		}
		state = TutorialState.TapTile;
		target1.TargetTile(tile.tileObject);
		hand1.TapTarget(target1);
	}

	private static void HelpAddTile(Block newBlock, GAF gaf, int row, int col)
	{
		if (cheatNextStep)
		{
			stepOnNextUpdate = true;
			cheatNextStep = false;
			newBlock.tiles = Blocksworld.CloneBlockTiles(goalBlock.tiles);
			return;
		}
		if (Blocksworld.selectedBlock != newBlock)
		{
			HelpTapBlock(newBlock);
			return;
		}
		List<List<Tile>> tiles = Blocksworld.scriptPanel.tiles;
		Tile tile = tiles[1][0];
		int num = row;
		int num2 = col;
		if (num >= tiles.Count)
		{
			num--;
		}
		if (num > 0 && num < tiles.Count)
		{
			List<Tile> list = tiles[num];
			if (num2 >= list.Count)
			{
				num2 = list.Count - 1;
			}
		}
		if (num >= 0 && num2 >= 0 && num < tiles.Count && num2 < tiles[num].Count)
		{
			tile = tiles[num][num2];
		}
		if (IsOutsideScreen(tile) || !tile.IsShowing())
		{
			HelpDragScriptPanel(tile);
			return;
		}
		ScriptPanel scriptPanel = Blocksworld.scriptPanel;
		Vector3 position = scriptPanel.position;
		bool flag = position.x < 0f;
		bool flag2 = scriptPanel.width + 80f > Blocksworld.buildPanel.position.x;
		if (position.x + scriptPanel.width > Blocksworld.buildPanel.position.x && !flag2)
		{
			HelpDragScriptPanel(tile);
			return;
		}
		if (flag && !flag2)
		{
			HelpDragScriptPanel(tile);
			return;
		}
		List<Tile> list2 = ((row >= goalBlock.tiles.Count) ? null : goalBlock.tiles[row]);
		TutorialActions.EnterContext(TutorialActionContext.DuringScriptThisRow, goalBlock, list2);
		Vector3 screen = Vector3.zero;
		if (gaf.Predicate == Block.predicatePaintTo)
		{
			GAF lastPaintedColorGAF = Blocksworld.clipboard.GetLastPaintedColorGAF();
			usingQuickSelectColorIcon = (string)gaf.Args[0] == (string)lastPaintedColorGAF.Args[0];
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			quickSelect.UpdateColorIcon();
			Vector2 vector = quickSelect.PaintRectCenter();
			screen = vector;
		}
		if (!usingQuickSelectColorIcon && HelpTapTab(gaf))
		{
			return;
		}
		Tile tile2 = null;
		if (TileToggleChain.IsToggleGaf(gaf))
		{
			GAF primitiveGafFor = TileToggleChain.GetPrimitiveGafFor(gaf);
			if (primitiveGafFor != null)
			{
				tile2 = Blocksworld.buildPanel.FindTileMatching(primitiveGafFor);
				if (tile2 == null)
				{
					BWLog.Info("Could not find toggle panel icon for " + primitiveGafFor);
				}
			}
			else
			{
				BWLog.Info("Could not find toggle gaf for " + gaf);
			}
		}
		if (tile2 == null)
		{
			GAF gAF = gaf.Clone();
			EditableTileParameter editableParameter = gAF.Predicate.EditableParameter;
			if (editableParameter != null)
			{
				object obj = null;
				if (editableParameter is NumericHandleTileParameter numericHandleTileParameter)
				{
					object obj2 = gaf.Args[numericHandleTileParameter.parameterIndex];
					if (obj2 is int)
					{
						float num3 = (int)obj2;
						obj = num3;
					}
					else if (obj2 is float num4)
					{
						obj = num4;
					}
					else
					{
						BWLog.Info("Unsupported numeric in tutorial: " + obj2.GetType().Name);
					}
				}
				gAF.Args[editableParameter.parameterIndex] = obj;
				if (Blocksworld.buildPanel.FindTileMatching(gAF) == null)
				{
					for (int i = editableParameter.parameterIndex; i < gAF.Args.Length; i++)
					{
						gAF.Args[i] = null;
					}
				}
			}
			tile2 = Blocksworld.buildPanel.FindTileMatching(gAF);
		}
		if (tile2 == null && gaf.Predicate == Block.predicatePlaySoundDurational)
		{
			GAF gAF2 = gaf.Clone();
			for (int j = 1; j < gAF2.Args.Length; j++)
			{
				gAF2.Args[j] = null;
			}
			tile2 = Blocksworld.buildPanel.FindTileMatching(gAF2);
		}
		if (gaf.Predicate == BlockMaster.predicateSetEnvEffect)
		{
			string text = BlockSky.EnvEffectToTexture((string)gaf.Args[0]);
			GAF patternGaf = new GAF(Block.predicateTextureTo, text);
			tile2 = Blocksworld.buildPanel.FindTileMatching(patternGaf);
		}
		if (gaf.Predicate == BlockMaster.predicatePaintSkyTo)
		{
			GAF patternGaf2 = new GAF(Block.predicatePaintTo, (string)gaf.Args[0]);
			tile2 = Blocksworld.buildPanel.FindTileMatching(patternGaf2);
		}
		if (gaf.Predicate == Block.predicateSendCustomSignal || gaf.Predicate == Block.predicateSendCustomSignalModel)
		{
			targetSignalName = (string)gaf.Args[0];
			targetSignalPredicate = gaf.Predicate;
			GAF patternGaf3 = new GAF(Block.predicateSendCustomSignal, "*");
			tile2 = Blocksworld.buildPanel.FindTileMatching(patternGaf3);
		}
		if (tile2 == null)
		{
			BWLog.Error("Could not find tile for gaf " + gaf);
			return;
		}
		Blocksworld.enabledGAFs = new List<GAF> { tile2.gaf };
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		if (!usingQuickSelectColorIcon && IsOutsideScreen(tile2))
		{
			HelpScrollSidePanel(tile2);
			scrollToTile = tile2;
			return;
		}
		if (list2 != null)
		{
			int num5 = col - 1;
			Tile tileBefore = ((num5 <= 0 || num5 >= list2.Count) ? null : list2[num5]);
			Tile tileAfter = ((num5 + 1 >= list2.Count) ? null : list2[num5 + 1]);
			TutorialActions.EnterContext(TutorialActionContext.DuringScriptNextTile, goalBlock, list2, tileBefore, tileAfter);
		}
		state = TutorialState.AddTile;
		arrow1.state = TrackingState.TilePanelOffset;
		arrow1.tile = tile2.tileObject;
		arrow1.panel = Blocksworld.scriptPanel;
		List<Tile> list3 = tiles[row];
		bool flag3 = false;
		for (int k = col; k < list3.Count; k++)
		{
			if (list3[k].gaf.Predicate == Block.predicateThen)
			{
				flag3 = true;
				break;
			}
		}
		if (flag3)
		{
			arrow1.offset = tiles[row][col].GetCenterPosition() - new Vector3(1f, 0f, 0f) * 75f * NormalizedScreen.pixelScale - Blocksworld.scriptPanel.position;
		}
		else
		{
			arrow1.offset = tiles[row][col - 1].GetCenterPosition() + new Vector3(0.5f, 0f, 0f) * 75f * NormalizedScreen.pixelScale - Blocksworld.scriptPanel.position;
		}
		arrow1.Show(show: true);
		target1.TargetTile(tile2.tileObject);
		if (usingQuickSelectColorIcon)
		{
			target1.TargetScreen(screen);
			arrow1.state = TrackingState.ScreenPanelOffset;
			arrow1.screen = screen;
		}
		target2.TargetPanelOffset(arrow1.panel, arrow1.offset);
		hand1.DragArrow(arrow1);
	}

	private static void HelpPlay()
	{
		TutorialActions.EnterContext(TutorialActionContext.BeforeEnterPlay);
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Blocksworld.UI.Tapedeck.Ghost(ghost: false);
		target1.TargetButton(TILE_BUTTON.PLAY);
		hand1.TapTarget(target1);
	}

	private static void HelpStop()
	{
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Blocksworld.UI.Tapedeck.Ghost(ghost: false);
		target1.TargetButton(TILE_BUTTON.TOOLS);
		hand1.TapTarget(target1);
	}

	private static bool CheckScale(Block thisBlock)
	{
		if (!Close(thisBlock.Scale(), goalBlock.Scale()))
		{
			HelpScale(thisBlock);
			return false;
		}
		return true;
	}

	private static bool CheckRotation(Block thisBlock, bool fakeEnabled = true)
	{
		bool flag = thisBlock.HasGroup("locked-model");
		fakeEnabled = fakeEnabled && !flag;
		if (SameRotation(thisBlock, thisBlock.go.transform.rotation, goalBlock, goalBlock.go.transform.rotation, fakeEnabled))
		{
			return true;
		}
		if (fakeEnabled && CanFakeSameRotation(thisBlock, thisBlock.go.transform.rotation, goalBlock, goalBlock.go.transform.rotation))
		{
			thisBlock.RotateTo(goalBlock.go.transform.rotation);
			return true;
		}
		HelpRotation(thisBlock, goalBlock.go.transform.rotation);
		return false;
	}

	private static Vector3 CenterOfScreen()
	{
		return new Vector3(((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width) * 0.5f, (float)Blocksworld.screenHeight * 0.5f, 0f);
	}

	public static bool FilterOutMouseBlock()
	{
		if (state != TutorialState.Color)
		{
			return state == TutorialState.Texture;
		}
		return true;
	}

	public static bool BlockCanBeMouseBlock(Block b)
	{
		if (!(existingBlock is BlockWater))
		{
			if (FilterOutMouseBlock())
			{
				return !(b is BlockWater);
			}
			return true;
		}
		return true;
	}

	public static HashSet<GAF> GetHiddenGafs()
	{
		if (state != TutorialState.None)
		{
			return hiddenGafs;
		}
		return new HashSet<GAF>();
	}

	public static HashSet<GAF> GetNeverHideGafs()
	{
		if (state != TutorialState.None)
		{
			return neverHideGafs;
		}
		return new HashSet<GAF>();
	}

	private static void UpdateTileRowHint(int row)
	{
		tileRowHint = null;
		if (row < tileRowHints.Count)
		{
			tileRowHint = tileRowHints[row];
		}
	}

	public static void OnHudMesh()
	{
		if (state != TutorialState.None)
		{
			float y = Screen.height - 100;
			if (tileRowHint != null && tileRowHint.Length > 0)
			{
				HudMeshOnGUI.Label(ref hintLabel, new Rect(0f, y, Screen.width, 100f), tileRowHint);
			}
		}
	}

	public static void AddOkParameterTile(Tile tile)
	{
		okParameterTiles.Add(tile);
	}

	public static bool SnapParameterValue(GAF thisGaf, GAF goalGaf)
	{
		EditableTileParameter editableParameter = thisGaf.Predicate.EditableParameter;
		if (editableParameter is TimeTileParameter)
		{
			TimeTileParameter timeTileParameter = (TimeTileParameter)editableParameter;
			int[] array = timeTileParameter.CalculateTimeComponents(goalGaf);
			int[] array2 = timeTileParameter.CalculateTimeComponents(thisGaf);
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				int num = array[i];
				int num2 = array2[i];
				int num3 = Mathf.Abs(num - num2);
				if (num3 > 0 && num > 0)
				{
					float num4 = (float)num3 / (float)num;
					if (num4 < 0.1f)
					{
						array2[i] = num;
						flag = true;
					}
				}
			}
			if (flag)
			{
				float num5 = (float)array2[0] * 0.01f + (float)array2[1] + (float)array2[2] * 60f;
				thisGaf.Args[editableParameter.parameterIndex] = num5;
				float num6 = (float)goalGaf.Args[editableParameter.parameterIndex];
				if ((double)Mathf.Abs(num5 - num6) < 0.01)
				{
					return true;
				}
			}
		}
		else if (editableParameter is FloatTileParameter)
		{
			float num7 = (float)goalGaf.Args[editableParameter.parameterIndex];
			float num8 = (float)thisGaf.Args[editableParameter.parameterIndex];
			float num9 = Mathf.Abs(num7 - num8);
			if (Mathf.Abs(num7) > 0.01f)
			{
				float f = num9 / num7;
				if (Mathf.Abs(f) < 0.1f)
				{
					thisGaf.Args[editableParameter.parameterIndex] = goalGaf.Args[editableParameter.parameterIndex];
					return true;
				}
			}
		}
		return false;
	}

	public static bool OkOverwriteParameterValue(EditableTileParameter editableParam, Predicate predicate, object thisValue, object goalValue)
	{
		if (predicate == Block.predicateWaitTime)
		{
			float num = (float)goalValue;
			if (Mathf.Abs(num - 0.24f) < 0.02f)
			{
				return true;
			}
		}
		if (editableParam is IntTileParameter)
		{
			IntTileParameter intTileParameter = (IntTileParameter)editableParam;
			BidiIntFloatConverter converter = intTileParameter.converter;
			if (converter != null)
			{
				float arg = (float)thisValue;
				float arg2 = (float)goalValue;
				int num2 = converter.floatToInt(arg);
				int num3 = converter.floatToInt(arg2);
				if (num2 == num3)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void AutoRotateBlock(Block b)
	{
		if (goalBlock == null || !(goalBlock.go != null))
		{
			return;
		}
		BlockMetaData blockMetaData = b.GetBlockMetaData();
		if (blockMetaData != null)
		{
			Vector3 autoAnglesTutorial = blockMetaData.autoAnglesTutorial;
			Vector3 eulerAngles = b.GetRotation().eulerAngles;
			Vector3 eulerAngles2 = goalBlock.GetRotation().eulerAngles;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				float num = Mathf.Max(autoAnglesTutorial[i], autoAnglesFilter[i]);
				zero[i] = eulerAngles[i] * (1f - num) + eulerAngles2[i] * num;
			}
			b.RotateTo(Quaternion.Euler(zero));
		}
	}

	private static bool HelpOperationPose(TutorialState tutState, int meshIndex = -1)
	{
		if (!AutoCameraEnabled())
		{
			return false;
		}
		if (operationPoses.TryGetValue(goalBlock, out var value))
		{
			TutorialOperationPose tutorialOperationPose = null;
			NamedPose namedPose = null;
			foreach (TutorialOperationPose item in value)
			{
				if (item.state == tutState && (item.meshIndex == meshIndex || meshIndex < 0))
				{
					if (Blocksworld.cameraPosesMap.TryGetValue(item.poseName, out var value2))
					{
						tutorialOperationPose = item;
						namedPose = value2;
						break;
					}
					BWLog.Warning("Could not find a pose named: '" + item.poseName + "'");
				}
			}
			if (tutorialOperationPose != null && namedPose != null)
			{
				autoCameraEndPos = namedPose.position;
				Blocksworld.blocksworldCamera.SetTargetPosition(namedPose.position + namedPose.direction * 15f);
				if (Close(autoCameraEndPos, Blocksworld.cameraTransform.position))
				{
					return false;
				}
				if (state != TutorialState.Orbit)
				{
					stepOnNextUpdate = true;
				}
				HelpAutoCamera();
				return true;
			}
		}
		return false;
	}

	public static bool InTutorialOrPuzzle()
	{
		if (state == TutorialState.None)
		{
			return mode == TutorialMode.Puzzle;
		}
		return true;
	}

	public static bool InTutorial()
	{
		return state != TutorialState.None;
	}

	public static string GetTargetSignalName()
	{
		return targetSignalName;
	}

	public static Predicate GetTargetSignalPredicate()
	{
		return targetSignalPredicate;
	}

	private static void DisableOccludingConnectedColliders(Block block)
	{
		if (raycastDisabledBlocks.Count > 0)
		{
			EnableOccludingConnectedColliders();
		}
		Vector3 position = block.go.transform.position;
		for (int i = 0; i < block.connections.Count; i++)
		{
			Block block2 = block.connections[i];
			bool flag = block2 is BlockPosition;
			if (!flag)
			{
				Collider[] componentsInChildren = block2.go.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					if (collider != null && collider.enabled && collider.bounds.Contains(position))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				raycastDisabledBlocks.Add(block2);
				block2.IgnoreRaycasts(value: true);
			}
		}
	}

	private static void EnableOccludingConnectedColliders()
	{
		foreach (Block raycastDisabledBlock in raycastDisabledBlocks)
		{
			raycastDisabledBlock.IgnoreRaycasts(value: false);
		}
		raycastDisabledBlocks.Clear();
	}
}
