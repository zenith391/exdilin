using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

// Token: 0x020002DC RID: 732
public class Tutorial
{
	// Token: 0x1700015C RID: 348
	// (get) Token: 0x06002153 RID: 8531 RVA: 0x000F4460 File Offset: 0x000F2860
	// (set) Token: 0x06002154 RID: 8532 RVA: 0x000F4467 File Offset: 0x000F2867
	public static TutorialState state
	{
		get
		{
			return Tutorial._state;
		}
		set
		{
			Tutorial._state = value;
		}
	}

	// Token: 0x06002155 RID: 8533 RVA: 0x000F446F File Offset: 0x000F286F
	public static void Init()
	{
		Tutorial.arrow1 = new Arrow();
		Tutorial.arrow2 = new Arrow();
		Tutorial.target1 = new Target();
		Tutorial.target2 = new Target();
		Tutorial.hand1 = new Hand(false);
		Tutorial.hand2 = new Hand(true);
	}

	// Token: 0x06002156 RID: 8534 RVA: 0x000F44AF File Offset: 0x000F28AF
	public static void ResetState()
	{
		Tutorial.state = TutorialState.None;
		Tutorial.stateBeforePlay = Tutorial.state;
		Tutorial.mode = TutorialMode.StepByStep;
		Tutorial.modeBeforePlay = Tutorial.mode;
		TutorialActions.Clear();
		Tutorial.safeState = null;
	}

	// Token: 0x06002157 RID: 8535 RVA: 0x000F44DC File Offset: 0x000F28DC
	private static TutorialSettings GetTutorialSettings()
	{
		if (Tutorial.tutorialSettings == null)
		{
			Tutorial.tutorialSettings = Blocksworld.blocksworldDataContainer.GetComponent<TutorialSettings>();
		}
		return Tutorial.tutorialSettings;
	}

	// Token: 0x06002158 RID: 8536 RVA: 0x000F4504 File Offset: 0x000F2904
	private static bool NeverHideInStepByStep(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate)
		{
			if (Tutorial.neverHideInStepByStepBlocks == null)
			{
				Tutorial.neverHideInStepByStepBlocks = new HashSet<string>(Tutorial.GetTutorialSettings().neverHideStepByStepBlocks);
			}
			return Tutorial.neverHideInStepByStepBlocks.Contains((string)gaf.Args[0]);
		}
		if (gaf.Predicate == Block.predicateTextureTo)
		{
			if (Tutorial.neverHideInStepByStepTextures == null)
			{
				Tutorial.neverHideInStepByStepTextures = new HashSet<string>(Tutorial.GetTutorialSettings().neverHideStepByStepTextures);
			}
			return Tutorial.neverHideInStepByStepTextures.Contains((string)gaf.Args[0]);
		}
		if (gaf.Predicate == Block.predicatePaintTo)
		{
			if (Tutorial.neverHideInStepByStepPaints == null)
			{
				Tutorial.neverHideInStepByStepPaints = new HashSet<string>(Tutorial.GetTutorialSettings().neverHideStepByStepPaints);
			}
			return Tutorial.neverHideInStepByStepPaints.Contains((string)gaf.Args[0]);
		}
		if (gaf.Predicate == Block.predicatePlaySoundDurational)
		{
			if (Tutorial.neverHideInStepByStepSFXs == null)
			{
				Tutorial.neverHideInStepByStepSFXs = new HashSet<string>(Tutorial.GetTutorialSettings().neverHideStepByStepSFXs);
			}
			return Tutorial.neverHideInStepByStepSFXs.Contains((string)gaf.Args[0]);
		}
		return false;
	}

	// Token: 0x06002159 RID: 8537 RVA: 0x000F462C File Offset: 0x000F2A2C
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

	// Token: 0x0600215A RID: 8538 RVA: 0x000F4724 File Offset: 0x000F2B24
	private static void SetHiddenGafs(bool addNeverHideGafs = true)
	{
		HashSet<GAF> hashSet = new HashSet<GAF>();
		HashSet<Predicate> taggedPredicates = Blocksworld.GetTaggedPredicates();
		foreach (Block block in Tutorial.blocks)
		{
			BlockGrouped blockGrouped = block as BlockGrouped;
			if (blockGrouped != null)
			{
				Tutorial.neverHideGafs.Add(blockGrouped.GetIconGaf());
			}
			foreach (List<Tile> list in block.tiles)
			{
				foreach (Tile tile in list)
				{
					GAF gaf = tile.gaf;
					GAF primitiveGafFor = TileToggleChain.GetPrimitiveGafFor(gaf);
					if (primitiveGafFor != null)
					{
						hashSet.Add(primitiveGafFor);
						Tutorial.neverHideGafs.Add(primitiveGafFor);
						if (primitiveGafFor.Predicate == Block.predicateSendCustomSignal || primitiveGafFor.Predicate == Block.predicateSendCustomSignalModel)
						{
							Tutorial.neverHideGafs.Add(Scarcity.GetNormalizedGaf(primitiveGafFor, false));
						}
					}
					else if (taggedPredicates.Contains(gaf.Predicate))
					{
						EditableTileParameter editableParameter = gaf.Predicate.EditableParameter;
						if (editableParameter == null)
						{
							Tutorial.neverHideGafs.Add(gaf);
						}
						else
						{
							GAF gaf2 = gaf.Clone();
							gaf2.Args[editableParameter.parameterIndex] = null;
							Tile tile2 = Blocksworld.buildPanel.FindTileMatching(gaf2);
							if (tile2 != null)
							{
								Tutorial.neverHideGafs.Add(tile2.gaf);
							}
							else
							{
								GAF gaf3 = gaf.Clone();
								GAF item = new GAF(gaf3.Predicate, gaf3.Predicate.ExtendArguments(gaf3.Args, true));
								Tutorial.neverHideGafs.Add(item);
							}
						}
					}
					else if (gaf.Predicate == BlockMaster.predicateSetEnvEffect)
					{
						Tutorial.neverHideGafs.Add(Scarcity.GetNormalizedGaf(new GAF(Block.predicateTextureTo, new object[]
						{
							BlockSky.EnvEffectToTexture((string)gaf.Args[0])
						}), false));
					}
					else if (gaf.Predicate == BlockMaster.predicatePaintSkyTo)
					{
						Tutorial.neverHideGafs.Add(Scarcity.GetNormalizedGaf(new GAF(Block.predicatePaintTo, new object[]
						{
							(string)gaf.Args[0]
						}), false));
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
		foreach (Tile tile3 in Tutorial.manualPaintOrTexture)
		{
			GAF gaf4 = tile3.gaf;
			if (gaf4.Predicate == Block.predicateTutorialTextureExistingBlock)
			{
				hashSet5.Add((string)gaf4.Args[1]);
			}
			else if (gaf4.Predicate == Block.predicateTutorialPaintExistingBlock)
			{
				hashSet6.Add((string)gaf4.Args[1]);
			}
		}
		foreach (Block block2 in Tutorial.blocks)
		{
			foreach (List<Tile> list2 in block2.tiles)
			{
				foreach (Tile tile4 in list2)
				{
					GAF gaf5 = tile4.gaf;
					Tutorial.ProcessHiddenGaf(gaf5, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
				}
			}
		}
		Tutorial.ProcessHiddenIfNotNull(Scarcity.puzzleGAFUsage, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		Tutorial.ProcessHiddenIfNotNull(Scarcity.puzzleInventory, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		Tutorial.ProcessHiddenIfNotNull(Scarcity.puzzleGAFUsageAfterStepByStep, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		Tutorial.ProcessHiddenIfNotNull(Scarcity.puzzleInventoryAfterStepByStep, hashSet4, hashSet5, hashSet6, hashSet7, hashSet2);
		foreach (GAF gaf6 in Blocksworld.unlockedGAFs)
		{
			if (((!hashSet3.Contains(gaf6.Predicate) && !hashSet2.Contains(gaf6.Predicate)) || ((!addNeverHideGafs || !Tutorial.NeverHideInStepByStep(gaf6)) && ((gaf6.Predicate == Block.predicateCreate && !hashSet4.Contains((string)gaf6.Args[0])) || (gaf6.Predicate == Block.predicatePaintTo && !hashSet6.Contains((string)gaf6.Args[0])) || (gaf6.Predicate == Block.predicatePlaySoundDurational && !hashSet7.Contains((string)gaf6.Args[0])) || (gaf6.Predicate == Block.predicateTextureTo && !hashSet5.Contains((string)gaf6.Args[0]))))) && !hashSet.Contains(gaf6))
			{
				Tutorial.hiddenGafs.Add(gaf6);
			}
		}
	}

	// Token: 0x0600215B RID: 8539 RVA: 0x000F4DC0 File Offset: 0x000F31C0
	private static void ProcessHiddenIfNotNull(Dictionary<GAF, int> dict, HashSet<string> blocksToKeep, HashSet<string> texturesToKeep, HashSet<string> paintsToKeep, HashSet<string> sfxsToKeep, HashSet<Predicate> allPlaceableBlockPredicates)
	{
		if (dict != null)
		{
			foreach (GAF gaf in dict.Keys)
			{
				Tutorial.ProcessHiddenGaf(gaf, blocksToKeep, texturesToKeep, paintsToKeep, sfxsToKeep, allPlaceableBlockPredicates);
			}
		}
	}

	// Token: 0x0600215C RID: 8540 RVA: 0x000F4E28 File Offset: 0x000F3228
	private static void UpdatePredicateAccums()
	{
		if (Tutorial.predicateAccumIndices == null)
		{
			Tutorial.predicateAccumIndices = new Dictionary<Predicate, int>
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

	// Token: 0x0600215D RID: 8541 RVA: 0x000F4F83 File Offset: 0x000F3383
	private static bool CanAccumulateArguments(Tile tile)
	{
		Tutorial.UpdatePredicateAccums();
		return Tutorial.predicateAccumIndices.ContainsKey(tile.gaf.Predicate);
	}

	// Token: 0x0600215E RID: 8542 RVA: 0x000F4FA0 File Offset: 0x000F33A0
	private static void Accumulate(Tile accumTile, Tile tile)
	{
		Tutorial.UpdatePredicateAccums();
		Predicate predicate = tile.gaf.Predicate;
		int num;
		if (Tutorial.predicateAccumIndices.TryGetValue(predicate, out num))
		{
			float num2 = (float)accumTile.gaf.Args[num];
			num2 += (float)tile.gaf.Args[num];
			accumTile.gaf.Args[num] = num2;
		}
	}

	// Token: 0x0600215F RID: 8543 RVA: 0x000F500C File Offset: 0x000F340C
	private static bool GoalBlockNeedsFiltering()
	{
		Predicate predicateHideTileRow = Block.predicateHideTileRow;
		Predicate predicateHideNextTile = Block.predicateHideNextTile;
		for (int i = Tutorial.goalBlock.tiles.Count - 1; i >= 0; i--)
		{
			List<Tile> list = Tutorial.goalBlock.tiles[i];
			for (int j = list.Count - 1; j >= 0; j--)
			{
				Tile tile = list[j];
				Predicate predicate = tile.gaf.Predicate;
				if (predicate == predicateHideTileRow || predicate == predicateHideNextTile)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06002160 RID: 8544 RVA: 0x000F50A0 File Offset: 0x000F34A0
	private static void FilterGoalBlockTiles()
	{
		Predicate predicateHideTileRow = Block.predicateHideTileRow;
		Predicate predicateHideNextTile = Block.predicateHideNextTile;
		for (int i = Tutorial.goalBlock.tiles.Count - 1; i >= 0; i--)
		{
			bool flag = false;
			List<Tile> list = Tutorial.goalBlock.tiles[i];
			for (int j = list.Count - 1; j >= 0; j--)
			{
				Tile tile = list[j];
				Predicate predicate = tile.gaf.Predicate;
				if (predicate == predicateHideTileRow)
				{
					flag = true;
				}
				else if (predicate == predicateHideNextTile || predicate == Block.predicateTutorialHelpTextAction)
				{
					list.RemoveAt(j);
					if (j < list.Count)
					{
						Tile tile2 = list[j];
						if (tile2.gaf.Predicate != Block.predicateThen)
						{
							list.RemoveAt(j);
						}
					}
				}
			}
			HashSet<Tile> hashSet = new HashSet<Tile>();
			Tile tile3 = null;
			for (int k = 0; k < list.Count; k++)
			{
				Tile tile4 = list[k];
				bool flag2 = Tutorial.CanAccumulateArguments(tile4);
				if (tile3 != null && tile4.gaf.Predicate == tile3.gaf.Predicate)
				{
					Tutorial.Accumulate(tile3, tile4);
					hashSet.Add(tile4);
				}
				else if (flag2)
				{
					tile3 = tile4;
				}
				else
				{
					tile3 = null;
				}
			}
			for (int l = list.Count - 1; l >= 0; l--)
			{
				Tile item = list[l];
				if (hashSet.Contains(item))
				{
					list.RemoveAt(l);
				}
			}
			if (flag)
			{
				Tutorial.goalBlock.tiles.RemoveAt(i);
			}
		}
		Predicate predicate2 = PredicateRegistry.ByName("Meta.TileRowHint", true);
		List<List<Tile>> tiles = Tutorial.goalBlock.tiles;
		for (int m = 0; m < tiles.Count; m++)
		{
			string item2 = string.Empty;
			List<Tile> list2 = Tutorial.goalBlock.tiles[m];
			for (int n = list2.Count - 1; n >= 0; n--)
			{
				Tile tile5 = list2[n];
				Predicate predicate3 = tile5.gaf.Predicate;
				if (predicate3 == predicate2)
				{
					item2 = (string)tile5.gaf.Args[0];
					list2.RemoveAt(n);
				}
			}
			Tutorial.tileRowHints.Add(item2);
		}
	}

	// Token: 0x06002161 RID: 8545 RVA: 0x000F5324 File Offset: 0x000F3724
	private static void EnterPuzzleMode(Dictionary<GAF, int> puzzleInventory, Dictionary<GAF, int> puzzleGAFUsage)
	{
		Scarcity.worldGAFUsage = new Dictionary<GAF, int>(puzzleGAFUsage);
		Scarcity.inventory = new Dictionary<GAF, int>(puzzleInventory);
		Scarcity.UpdateInventory(true, null);
		foreach (KeyValuePair<GAF, int> keyValuePair in puzzleGAFUsage)
		{
			GAF key = keyValuePair.Key;
			if (Scarcity.inventory.ContainsKey(key) && Scarcity.inventory[key] != -1)
			{
				Dictionary<GAF, int> inventory;
				GAF key2;
				(inventory = Scarcity.inventory)[key2 = key] = inventory[key2] - keyValuePair.Value;
			}
		}
		Blocksworld.enabledGAFs = null;
		Blocksworld.UpdateTiles();
		Tutorial.mode = TutorialMode.Puzzle;
		Tutorial.state = TutorialState.Puzzle;
		Tutorial.SetHiddenGafs(false);
		Blocksworld.UpdateTiles();
		WorldSession.current.OnCompleteBuild();
	}

	// Token: 0x06002162 RID: 8546 RVA: 0x000F540C File Offset: 0x000F380C
	public static void Start()
	{
		Tutorial.Start(false);
	}

	// Token: 0x06002163 RID: 8547 RVA: 0x000F5414 File Offset: 0x000F3814
	public static void Start(bool skipBuild)
	{
		Blocksworld.buildPanel.SetTutorialMode(true);
		TBox.ResetTargetSnapping();
		Tutorial.DestroyPlacementHelper();
		Tutorial.usingQuickSelectColorIcon = false;
		Tutorial.usingQuickSelectTextureIcon = false;
		Tutorial.rotateBeforeScale = false;
		Tutorial.paintBeforePose = false;
		Tutorial.textureBeforePose = false;
		Tutorial.autoAnglesFilter = Vector3.zero;
		Tutorial.useTwoFingerMove = false;
		Tutorial.useTwoFingerScale = false;
		Tutorial.includeDefaultBlockTiles = false;
		Tutorial.groupedManualGoalHints.Clear();
		Tutorial.existingBlock = null;
		Tutorial.oldGoalBlock = null;
		Tutorial.safeState = null;
		Tutorial.hiddenGafs = new HashSet<GAF>();
		Tutorial.neverHideGafs = new HashSet<GAF>();
		Tutorial.okParameterTiles.Clear();
		Tutorial.operationPoses.Clear();
		Blocksworld.UpdateCameraPosesMap();
		TutorialActions.ParseActions(BWSceneManager.AllBlocks());
		HashSet<Block> hashSet = new HashSet<Block>();
		Tutorial.hiddenBlocks.Clear();
		Tutorial.raycastDisabledBlocks.Clear();
		foreach (Block block in BWSceneManager.AllBlocks())
		{
			bool flag = block.ContainsTileWithPredicate(Block.predicateLocked);
			bool flag2 = block.ContainsTileWithPredicate(Block.predicateTutorialRemoveBlockHint);
			BlockGrouped blockGrouped = block as BlockGrouped;
			if (blockGrouped != null && blockGrouped.group != null)
			{
				if (hashSet.Contains(block))
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
			if (flag || block.isTerrain)
			{
				if (!flag && block.IsUnlocked() && !flag2)
				{
					Tutorial.blocks.Add(block);
				}
				else
				{
					if (!skipBuild && block.ContainsTileWithPredicate(Block.predicateTutorialHideInBuildMode))
					{
						Tutorial.hiddenBlocks.Add(block);
						block.go.SetActive(false);
						if (block.goShadow != null)
						{
							block.goShadow.SetActive(false);
						}
					}
					Blocksworld.locked.Add(block);
				}
			}
			else
			{
				Tutorial.blocks.Add(block);
			}
		}
		Tutorial.tileRowHints.Clear();
		if (Scarcity.puzzleInventory != null)
		{
			Tutorial.EnterPuzzleMode(Scarcity.puzzleInventory, Scarcity.puzzleGAFUsage);
			return;
		}
		Tutorial.SetHiddenGafs(true);
		if (!skipBuild)
		{
			foreach (Block block2 in Tutorial.blocks)
			{
				block2.go.SetActive(false);
				if (block2.goShadow != null)
				{
					block2.goShadow.SetActive(false);
				}
				BWSceneManager.RemoveBlock(block2);
			}
		}
		else
		{
			foreach (Block block3 in Tutorial.blocks)
			{
				ConnectednessGraph.Update(block3);
			}
		}
		Scarcity.inventory = new Dictionary<GAF, int>();
		Blocksworld.Deselect(false, true);
		Blocksworld.stars.Play();
		Blocksworld.scriptPanel.PositionReset();
		Tutorial.numGivenBlocks = BWSceneManager.BlockCount();
		Tutorial.step = -1;
		if (!skipBuild)
		{
			Tutorial.state = TutorialState.GetNextBlock;
		}
		Tutorial.autoCameraAngle = new Vector3(30f, 0f, 0f);
		if (skipBuild || Tutorial.savedProgressTiles != null)
		{
		}
		for (int i = Tutorial.manualPaintOrTexture.Count - 1; i >= 0; i--)
		{
			if (Tutorial.blocks.Count > 0)
			{
				Block block4 = Tutorial.blocks[0];
				block4.tiles.Insert(1, new List<Tile>
				{
					new Tile(new GAF("Meta.Then", new object[0])),
					Tutorial.manualPaintOrTexture[i]
				});
			}
		}
		Tutorial.manualPaintOrTexture.Clear();
		for (int j = 0; j < Tutorial.completedManualPaintOrTexture.Count; j++)
		{
			GAF gaf = Tutorial.completedManualPaintOrTexture[j].gaf;
			Block block5 = Tutorial.NthBlock((int)gaf.Args[0]);
			object obj = gaf.Args[1];
			if (gaf.Predicate == Block.predicateTutorialPaintExistingBlock)
			{
				block5.PaintTo((string)obj, true, 0);
			}
			else if (gaf.Predicate == Block.predicateTutorialTextureExistingBlock)
			{
				block5.TextureTo((string)obj, Vector3.zero, true, 0, false);
			}
			else if (gaf.Predicate.Name.Equals("Block.TutorialRotateExistingBlock"))
			{
				block5.RotateTo(Quaternion.Euler((Vector3)obj));
			}
		}
		Tutorial.completedManualPaintOrTexture.Clear();
		Blocksworld.UpdateTiles();
		Tutorial.HardCodedTweaksAtStart(Blocksworld.currentWorldId);
		if (skipBuild)
		{
			Tutorial.ResetState();
			Tutorial.action = string.Empty;
			Tutorial.SetWinLoseBlocksToTutorialMode();
		}
		else
		{
			Tutorial.CreateTutorialSafeState();
			TutorialActions.EnterContext(TutorialActionContext.TutorialStart, null, null, null, null);
			Tutorial.Step();
		}
	}

	// Token: 0x06002164 RID: 8548 RVA: 0x000F5970 File Offset: 0x000F3D70
	public static bool IsLastBlockIncomplete()
	{
		return BWSceneManager.BlockCount() - Tutorial.numGivenBlocks >= Tutorial.step + 1;
	}

	// Token: 0x06002165 RID: 8549 RVA: 0x000F598C File Offset: 0x000F3D8C
	public static void CheatCreateBlock()
	{
		TutorialState state = Tutorial.state;
		if (state != TutorialState.Play && state != TutorialState.Puzzle && state != TutorialState.None)
		{
			if (Options.Cowlorded)
			{
				Tutorial.cheatNextStep = true;
				Tutorial.stepOnNextUpdate = true;
			}
		}
	}

	// Token: 0x06002166 RID: 8550 RVA: 0x000F59DC File Offset: 0x000F3DDC
	public static void Stop(bool forReal = false)
	{
		Tutorial.step = -1;
		Tutorial.safeState = null;
		Scarcity.ResetInventory();
		Blocksworld.enabledGAFs = null;
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.stars.Stop();
		Tutorial.arrow1.Show(false, 0);
		Tutorial.arrow2.Show(false, 0);
		Tutorial.target1.Show(false);
		Tutorial.target2.Show(false);
		Tutorial.hand1.Hide();
		Tutorial.hand2.Hide();
		TBox.tileButtonMove.Show(true);
		TBox.tileButtonRotate.Show(true);
		TBox.tileButtonScale.Show(true);
		TBox.tileButtonMove.Enable(true);
		TBox.tileButtonRotate.Enable(true);
		TBox.tileButtonScale.Enable(true);
		TBox.tileButtonMove.Show(false);
		TBox.tileButtonRotate.Show(false);
		TBox.tileButtonScale.Show(false);
		Tutorial.state = Tutorial.stateBeforePlay;
		Tutorial.mode = Tutorial.modeBeforePlay;
		Tutorial.stateBeforePlay = TutorialState.None;
		Tutorial.modeBeforePlay = TutorialMode.StepByStep;
		if (forReal)
		{
			Tutorial.state = TutorialState.None;
			Tutorial.mode = TutorialMode.StepByStep;
		}
		Blocksworld.buildPanel.SetTutorialMode(false);
		Tutorial.tileRowHint = null;
		Tutorial.tileRowHints.Clear();
		Tutorial.DestroyPlacementHelper();
		TutorialActions.Clear();
	}

	// Token: 0x06002167 RID: 8551 RVA: 0x000F5B0C File Offset: 0x000F3F0C
	public static void StartPlay()
	{
		if (Tutorial.state != TutorialState.None)
		{
			Tutorial.target1.Show(false);
			Tutorial.target2.Show(false);
			Tutorial.hand1.Hide();
			Tutorial.hand2.Hide();
			Tutorial.modeBeforePlay = Tutorial.mode;
			Tutorial.stateBeforePlay = Tutorial.state;
			Tutorial.state = TutorialState.Play;
			TutorialActions.EnterContext(TutorialActionContext.EnterPlay, null, null, null, null);
			foreach (Block block in Tutorial.hiddenBlocks)
			{
				if (block.go != null)
				{
					block.go.SetActive(true);
					if (block.goShadow != null)
					{
						block.goShadow.SetActive(true);
					}
				}
				ConnectednessGraph.Update(block);
			}
			Tutorial.hiddenBlocks.Clear();
			Tutorial.DestroyPlacementHelper();
			if (Tutorial.playModeHelpMover)
			{
				TutorialActions.EnterContext(TutorialActionContext.BeforePlayMoverUse, null, null, null, null);
				Tutorial.target1.TargetMoverControl();
				Tutorial.hand1.TapTarget(Tutorial.target1, 0.25f);
			}
		}
		Tutorial.UpdateCheatButton();
	}

	// Token: 0x06002168 RID: 8552 RVA: 0x000F5C44 File Offset: 0x000F4044
	public static void StopPlay()
	{
		if (Tutorial.mode == TutorialMode.Puzzle)
		{
			Tutorial.state = TutorialState.Puzzle;
		}
		if (Blocksworld.currentWorldId == "rock_fence")
		{
			Tutorial.arrow1.Show(false, 0);
			Tutorial.arrow2.Show(false, 0);
			Tutorial.target1.Show(false);
			Tutorial.target2.Show(false);
			Tutorial.hand1.Hide();
			Tutorial.hand2.Hide();
			Blocksworld.counters["3"] = 0;
		}
		Tutorial.state = Tutorial.stateBeforePlay;
		Tutorial.mode = Tutorial.modeBeforePlay;
		Tutorial.safeState = null;
		Blocksworld.UpdateTiles();
		TutorialActions.Clear();
	}

	// Token: 0x06002169 RID: 8553 RVA: 0x000F5CED File Offset: 0x000F40ED
	public static void TurnOffMoverHelp()
	{
		Tutorial.playModeHelpMover = false;
		Tutorial.target1.Show(false);
		Tutorial.hand1.Hide();
		TutorialActions.Clear();
	}

	// Token: 0x0600216A RID: 8554 RVA: 0x000F5D10 File Offset: 0x000F4110
	public static void BeforeSave()
	{
		if (Tutorial.hiddenBlocks == null || Tutorial.hiddenBlocks.Count == 0)
		{
			return;
		}
		foreach (Block block in Tutorial.hiddenBlocks)
		{
			if (block.go != null)
			{
				block.go.SetActive(true);
				if (block.goShadow != null)
				{
					block.goShadow.SetActive(true);
				}
			}
			ConnectednessGraph.Update(block);
		}
	}

	// Token: 0x0600216B RID: 8555 RVA: 0x000F5DC0 File Offset: 0x000F41C0
	public static void AfterSave()
	{
		if (Tutorial.hiddenBlocks == null || Tutorial.hiddenBlocks.Count == 0)
		{
			return;
		}
		foreach (Block block in Tutorial.hiddenBlocks)
		{
			if (block.go != null)
			{
				block.go.SetActive(false);
				if (block.goShadow != null)
				{
					block.goShadow.SetActive(false);
				}
			}
		}
		Tutorial.hiddenBlocks.Clear();
	}

	// Token: 0x0600216C RID: 8556 RVA: 0x000F5E74 File Offset: 0x000F4274
	private static void HardCodedTweaksAtStart(string id)
	{
		if (id != null)
		{
			if (id == "rock_fence")
			{
				Blocksworld.locked.Add(Tutorial.HardCodeBlock("Cube", new Vector3(-45f, 2f, 58f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
				Blocksworld.locked.Add(Tutorial.HardCodeBlock("Cube", new Vector3(-43f, 2f, 53f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
				Blocksworld.locked.Add(Tutorial.HardCodeBlock("Cube", new Vector3(-41f, 2f, 57f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
				Blocksworld.locked.Add(Tutorial.HardCodeBlock("Wedge", new Vector3(-43f, 2f, 57f), new Vector3(0f, 90f, 0f), Vector3.one, "Grey", "Bricks"));
				Blocksworld.locked.Add(Tutorial.HardCodeBlock("Wedge", new Vector3(-45f, 2f, 53f), new Vector3(0f, 180f, 0f), Vector3.one, "Grey", "Bricks"));
			}
		}
	}

	// Token: 0x0600216D RID: 8557 RVA: 0x000F6018 File Offset: 0x000F4418
	private static Block HardCodeBlock(string type, Vector3 pos, Vector3 rot, Vector3 scale, string paint, string texture)
	{
		Block block = Blocksworld.bw.AddNewBlock(new Tile(new GAF("Block.Create", new object[]
		{
			type
		})), true, null, true);
		block.RotateTo(Quaternion.Euler(rot));
		block.MoveTo(pos);
		block.ScaleTo(scale, true, false);
		block.PaintTo(paint, true, 0);
		block.TextureTo(texture, Vector3.zero, true, 0, false);
		ConnectednessGraph.Update(block);
		return block;
	}

	// Token: 0x0600216E RID: 8558 RVA: 0x000F6090 File Offset: 0x000F4490
	public static bool Hit(Vector3 mouse)
	{
		if (Tutorial.state == TutorialState.None)
		{
			return false;
		}
		Vector3 point = new Vector3(mouse.x, (float)NormalizedScreen.height - mouse.y, 0f);
		return Tutorial.rectText.Contains(point);
	}

	// Token: 0x0600216F RID: 8559 RVA: 0x000F60D8 File Offset: 0x000F44D8
	public static bool Close(Vector3 v1, Vector3 v2)
	{
		return (v2 - v1).sqrMagnitude < 0.02f;
	}

	// Token: 0x06002170 RID: 8560 RVA: 0x000F60FB File Offset: 0x000F44FB
	private static bool Close(float f1, float f2)
	{
		return Mathf.Abs(f2 - f1) < 1f;
	}

	// Token: 0x06002171 RID: 8561 RVA: 0x000F610C File Offset: 0x000F450C
	private static bool Rotation(float f1, float f2)
	{
		return Mathf.Abs(f2 - f1) < 1f;
	}

	// Token: 0x06002172 RID: 8562 RVA: 0x000F6120 File Offset: 0x000F4520
	private static bool CanFakeSameRotation(Block b1, Quaternion r1, Block b2, Quaternion r2)
	{
		string text = b1.BlockType();
		if (text != null)
		{
			if (text == "Cube")
			{
				return b1.Scale() == Vector3.one;
			}
		}
		return false;
	}

	// Token: 0x06002173 RID: 8563 RVA: 0x000F6164 File Offset: 0x000F4564
	private static bool SameRotation(Block b1, Quaternion r1, Block b2, Quaternion r2, bool fakeEnabled = true)
	{
		if (fakeEnabled)
		{
			string text = b1.BlockType();
			if (text != null)
			{
				if (!(text == "Cube"))
				{
					if (text == "Wedge")
					{
						return Quaternion.Angle(r1, r2) < 5f || (b1.Scale() == Vector3.one && Quaternion.Angle(r1 * Quaternion.Euler(-90f, 180f, 0f), r2) < 5f);
					}
					if (!(text == "Cylinder"))
					{
						if (text == "Rocket")
						{
							if (Materials.GetMapping(b2.GetTexture(0)) == Mapping.AllSidesTo1x1 && b1.Scale().x == 1f && b1.Scale().z == 1f)
							{
								return Quaternion.Angle(r1, r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(0f, 90f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(0f, 180f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(0f, 270f, 0f), r2) < 5f;
							}
						}
					}
					else if (Materials.GetMapping(b2.GetTexture(0)) == Mapping.AllSidesTo1x1)
					{
						return Quaternion.Angle(r1, r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(0f, 90f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(0f, 180f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(0f, 270f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(180f, 0f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(180f, 90f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(180f, 180f, 0f), r2) < 5f || Quaternion.Angle(r1 * Quaternion.Euler(180f, 270f, 0f), r2) < 5f;
					}
				}
				else if (Materials.GetMapping(b2.GetTexture(0)) == Mapping.AllSidesTo1x1 && b1.Scale() == Vector3.one)
				{
					return true;
				}
			}
		}
		return Quaternion.Angle(r1, r2) < 5f;
	}

	// Token: 0x06002174 RID: 8564 RVA: 0x000F6480 File Offset: 0x000F4880
	public static void Update()
	{
		if (MappedInput.InputDown(MappableInput.CHEAT_CREATE_BLOCK))
		{
			Tutorial.CheatCreateBlock();
		}
		TutorialActions.ExecuteAndUpdateActions();
		TutorialState state = Tutorial.state;
		switch (state)
		{
		case TutorialState.Waiting:
			if (Time.time > Tutorial.waitTimeEnd)
			{
				Tutorial.state = Tutorial.stateAfterWait;
				Tutorial.Step();
			}
			break;
		default:
			if (state != TutorialState.None)
			{
				Tutorial.arrow1.Update();
				Tutorial.arrow2.Update();
				Tutorial.target1.Update();
				Tutorial.target2.Update();
				Tutorial.hand1.Update();
				Tutorial.hand2.Update();
				Tutorial.AutoCamera();
				if (Tutorial.state == TutorialState.Position || Tutorial.state == TutorialState.CreateBlock)
				{
					Block selectedBlock = Blocksworld.selectedBlock;
					if (selectedBlock == null || selectedBlock.go == null)
					{
						Tutorial.arrow1.Show(true, 0);
						Tutorial.target1.Show(true);
						Tutorial.target2.Show(true);
					}
					else
					{
						Vector3 position = selectedBlock.go.transform.position;
						if (Tutorial.TutorialSnap(position, Tutorial.helpPositionTarget))
						{
							selectedBlock.MoveTo(Tutorial.helpPositionTarget);
							Tutorial.arrow1.Show(false, 0);
							Tutorial.target2.Show(false);
							Blocksworld.tBoxGesture.Stop();
							bool enabled = selectedBlock.go.GetComponent<Collider>().enabled;
							selectedBlock.EnableCollider(true);
							ConnectednessGraph.Update(selectedBlock);
							selectedBlock.EnableCollider(enabled);
							Tutorial.stepOnNextUpdate = true;
						}
						else if (Tutorial.TutorialSnapXZ(position, Tutorial.helpPositionTarget))
						{
							Vector3 pos = new Vector3(Tutorial.helpPositionTarget.x, position.y, Tutorial.helpPositionTarget.z);
							selectedBlock.MoveTo(pos);
						}
						else if (Tutorial.TutorialSnapY(position, Tutorial.helpPositionTarget))
						{
							Vector3 pos2 = new Vector3(position.x, Tutorial.helpPositionTarget.y, position.z);
							selectedBlock.MoveTo(pos2);
						}
						else
						{
							Tutorial.arrow1.Show(true, 0);
							Tutorial.target1.Show(true);
							Tutorial.target2.Show(true);
						}
					}
				}
				else
				{
					Tutorial.target2.Show(false);
				}
				if (Tutorial.stepOnNextUpdate)
				{
					Tutorial.stepOnNextUpdate = false;
					Tutorial.Step();
				}
			}
			break;
		case TutorialState.Play:
			if (Tutorial.playModeHelpMover)
			{
				Tutorial.target1.Update();
				Tutorial.hand1.Update();
			}
			break;
		}
		if (Tutorial.hideGraphics)
		{
			Tutorial.arrow1.Show(false, 0);
			Tutorial.arrow2.Show(false, 0);
			Tutorial.target1.Show(false);
			Tutorial.target2.Show(false);
			Tutorial.hand1.Hide();
			Tutorial.hand2.Hide();
		}
	}

	// Token: 0x06002175 RID: 8565 RVA: 0x000F6744 File Offset: 0x000F4B44
	private static Vector3 NearestCameraAngleSnap(float x, float y)
	{
		return new Vector3(x, Mathf.Round((y + 45f) / 90f) * 90f - 45f, 0f);
	}

	// Token: 0x06002176 RID: 8566 RVA: 0x000F6770 File Offset: 0x000F4B70
	public static void HelpAutoCamera()
	{
		Tutorial.autoCameraStartPos = Blocksworld.cameraTransform.position;
		Tutorial.autoTargetStartPos = Blocksworld.cameraTransform.position + Tutorial.autoCameraDist * Blocksworld.cameraTransform.forward;
		Tutorial.autoCameraTimer = 0f;
		Tutorial.state = TutorialState.Orbit;
	}

	// Token: 0x06002177 RID: 8567 RVA: 0x000F67C4 File Offset: 0x000F4BC4
	private static bool AutoCameraEnabled()
	{
		return !Options.TutorialDisableAutoCamera;
	}

	// Token: 0x06002178 RID: 8568 RVA: 0x000F67D0 File Offset: 0x000F4BD0
	private static void AutoCamera()
	{
		if (Tutorial.state != TutorialState.Orbit || !Tutorial.AutoCameraEnabled() || TutorialActions.AnyActionBlocksProgress())
		{
			return;
		}
		if (Blocksworld.cWidgetGesture.gestureState == GestureState.Active || Blocksworld.tileDragGesture.gestureState == GestureState.Active || Blocksworld.createTileDragGesture.gestureState == GestureState.Active || Blocksworld.tBoxGesture.gestureState == GestureState.Active)
		{
			Tutorial.autoCameraTimer = 0f;
			Tutorial.autoCameraTween = null;
			return;
		}
		if (Tutorial.autoCameraTween != null && Tutorial.autoCameraTween.IsFinished())
		{
			Tutorial.autoCameraTween = null;
			Util.ClearDraw();
			Blocksworld.blocksworldCamera.SetCameraPosition(Tutorial.autoCameraEndPos);
			Blocksworld.cameraTransform.LookAt(Blocksworld.blocksworldCamera.GetTargetPosition());
			Tutorial.stepOnNextUpdate = true;
			return;
		}
		Tutorial.autoCameraTimer += Time.deltaTime;
		if (Tutorial.autoCameraTimer < Tutorial.autoCameraDelay)
		{
			return;
		}
		if (Tutorial.autoCameraTween == null)
		{
			float magnitude = (Tutorial.autoCameraEndPos - Tutorial.autoCameraStartPos).magnitude;
			float duration = Tutorial.autoCameraTweenTimeMultiplier * Mathf.Clamp(0.15f * Mathf.Sqrt(magnitude), 0.25f, 3f);
			Tutorial.autoCameraTween = new Tween();
			Tutorial.autoCameraTween.Start(duration, 0f, 1f);
		}
		Vector3 a = Tutorial.autoCameraEndPos - Tutorial.autoCameraStartPos;
		Blocksworld.blocksworldCamera.SetCameraPosition(Tutorial.autoCameraStartPos + Tutorial.autoCameraTween.Value() * a);
		Vector3 a2 = Blocksworld.blocksworldCamera.GetTargetPosition() - Tutorial.autoTargetStartPos;
		Blocksworld.cameraTransform.LookAt(Tutorial.autoTargetStartPos + Tutorial.autoCameraTween.Value() * a2);
	}

	// Token: 0x06002179 RID: 8569 RVA: 0x000F698E File Offset: 0x000F4D8E
	private static void CreateTutorialSafeState()
	{
		Tutorial.safeState = new TutorialSafeState();
		Tutorial.safeState.ExtractState();
	}

	// Token: 0x0600217A RID: 8570 RVA: 0x000F69A4 File Offset: 0x000F4DA4
	public static void UpdateCheatButton()
	{
		bool flag = Options.Cowlorded;
		State currentState = Blocksworld.CurrentState;
		if (currentState == State.Play)
		{
			flag = false;
		}
		TutorialState state = Tutorial.state;
		if (state == TutorialState.None || state == TutorialState.BuildingCompleted || state == TutorialState.Play)
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

	// Token: 0x0600217B RID: 8571 RVA: 0x000F6A2C File Offset: 0x000F4E2C
	private static bool SameTutorialNormals(string blockType, Vector3 n1, Vector3 n2)
	{
		if (blockType != null)
		{
			if (blockType == "Slice")
			{
				return n1.z > 0.01f && n2.z > 0.01f;
			}
		}
		return Tutorial.Close(n1, n2);
	}

	// Token: 0x0600217C RID: 8572 RVA: 0x000F6A7E File Offset: 0x000F4E7E
	public static bool GAFsEqualInTutorial(GAF g1, GAF g2)
	{
		return g1.Equals(g2);
	}

	// Token: 0x0600217D RID: 8573 RVA: 0x000F6A88 File Offset: 0x000F4E88
	private static void SetWinLoseBlocksToTutorialMode()
	{
		foreach (Block block in BWSceneManager.AllBlocks())
		{
			for (int i = 0; i < block.tiles.Count; i++)
			{
				List<Tile> list = block.tiles[i];
				foreach (Tile tile in list)
				{
					GAF gaf = tile.gaf;
					if (gaf.Predicate == Block.predicateGameWin || gaf.Predicate == Block.predicateGameLose)
					{
						gaf.Args[1] = 4f;
					}
				}
			}
		}
	}

	// Token: 0x0600217E RID: 8574 RVA: 0x000F6B84 File Offset: 0x000F4F84
	public static Block GetLastBlock()
	{
		List<Block> list = BWSceneManager.AllBlocks();
		if (list.Count == 0)
		{
			return null;
		}
		return list[list.Count - 1];
	}

	// Token: 0x0600217F RID: 8575 RVA: 0x000F6BB4 File Offset: 0x000F4FB4
	public static Block NthBlock(int index)
	{
		List<Block> list = BWSceneManager.AllBlocks();
		return list[index];
	}

	// Token: 0x06002180 RID: 8576 RVA: 0x000F6BD0 File Offset: 0x000F4FD0
	public static void Step()
	{
		Tutorial.progressBlocked = false;
		if (TutorialActions.StepActions() || TutorialActions.AnyActionBlocksProgress())
		{
			Tutorial.progressBlocked = true;
			return;
		}
		Tutorial.UpdateCheatButton();
		TBox.ResetTargetSnapping();
		Tutorial.DisablePlacementHelper();
		Tutorial.hideGraphics = (BW.isUnityEditor && Options.HideTutorialGraphics);
		Tutorial.scrollToTile = null;
		TutorialState state = Tutorial.state;
		switch (state)
		{
		case TutorialState.Waiting:
			break;
		case TutorialState.BuildingCompleted:
			Tutorial.HelpPlay();
			break;
		case TutorialState.Play:
			break;
		case TutorialState.Puzzle:
		{
			bool flag = Blocksworld.selectedBunch != null || (Blocksworld.selectedBlock != null && Blocksworld.locked.Contains(Blocksworld.selectedBlock));
			TBox.tileButtonMove.Enable(!flag);
			TBox.tileButtonRotate.Enable(!flag);
			TBox.tileButtonScale.Enable(!flag);
			break;
		}
		default:
			if (state != TutorialState.None)
			{
				if (state == TutorialState.GetNextBlock)
				{
					Tutorial.step++;
					if (Tutorial.step == Tutorial.blocks.Count)
					{
						Tutorial.state = TutorialState.BuildingCompleted;
						Tutorial.SetWinLoseBlocksToTutorialMode();
						BW.Analytics.SendAnalyticsEvent("world-puzzle-built");
						if (Scarcity.puzzleInventoryAfterStepByStep != null)
						{
							Tutorial.EnterPuzzleMode(Scarcity.puzzleInventoryAfterStepByStep, Scarcity.puzzleGAFUsageAfterStepByStep);
						}
						else
						{
							WorldSession.current.OnCompleteBuild();
						}
					}
					else
					{
						BW.Analytics.SendAnalyticsEvent("world-puzzle-step", Convert.ToString(Tutorial.step + Tutorial.savedStep));
						Tutorial.oldGoalBlock = Tutorial.goalBlock;
						Tutorial.goalBlock = Tutorial.blocks[Tutorial.step];
						bool flag2 = Tutorial.goalBlock != null && Tutorial.goalBlock.HasAnyGroup();
						bool flag3 = Tutorial.oldGoalBlock != null && Tutorial.oldGoalBlock.HasAnyGroup();
						bool flag4 = false;
						if (flag2)
						{
							if (!flag3 || Tutorial.goalBlock.GetGroupOfType("tank-treads") != Tutorial.oldGoalBlock.GetGroupOfType("tank-treads"))
							{
								flag4 = true;
							}
						}
						else if (Tutorial.GoalBlockNeedsFiltering())
						{
							flag4 = true;
						}
						if (flag4)
						{
							Tutorial.CreateTutorialSafeState();
						}
						else
						{
							Tutorial.safeState = null;
						}
						Tutorial.manualGoalHint = null;
						Tutorial.moveBlockTile = null;
						Tutorial.removeBlockTile = null;
						Tutorial.moveModelTile = null;
						Tutorial.autoAddBlock = false;
						Tutorial.autoAddBlockWaitTime = 1f;
						Tutorial.manualRemoveBlock.Clear();
						Tutorial.manualPaintOrTexture.Clear();
						Tutorial.completedManualPaintOrTexture.Clear();
						bool flag5 = true;
						while (flag5)
						{
							flag5 = false;
							if (Tutorial.goalBlock.tiles.Count > 1 && Tutorial.goalBlock.tiles[1].Count > 1)
							{
								Tile tile = Tutorial.goalBlock.tiles[1][1];
								GAF gaf = tile.gaf;
								Predicate predicate = gaf.Predicate;
								if (predicate == Block.predicateTutorialCreateBlockHint)
								{
									Tutorial.manualGoalHint = tile;
									if (flag2)
									{
										Tutorial.groupedManualGoalHints[Tutorial.goalBlock.GetMainBlockInGroup("tank-treads")] = Tutorial.manualGoalHint;
									}
									flag5 = true;
								}
								else if (predicate == Block.predicateTutorialAutoAddBlock)
								{
									Tutorial.autoAddBlock = true;
									Tutorial.autoAddBlockWaitTime = Util.GetFloatArg(gaf.Args, 0, 1f);
									flag5 = true;
								}
								else if (predicate == Block.predicateTutorialOperationPose)
								{
									if (!Tutorial.operationPoses.ContainsKey(Tutorial.goalBlock))
									{
										Tutorial.operationPoses[Tutorial.goalBlock] = new List<TutorialOperationPose>();
									}
									List<TutorialOperationPose> list = Tutorial.operationPoses[Tutorial.goalBlock];
									TutorialState state2 = (TutorialState)Util.GetEnumArg(gaf.Args, 0, "Texture", typeof(TutorialState));
									int intArg = Util.GetIntArg(gaf.Args, 1, 0);
									string stringArg = Util.GetStringArg(gaf.Args, 2, string.Empty);
									list.Add(new TutorialOperationPose
									{
										state = state2,
										poseName = stringArg,
										meshIndex = intArg
									});
									flag5 = true;
								}
								else if (predicate == Block.predicateTutorialPaintExistingBlock || predicate == Block.predicateTutorialTextureExistingBlock || predicate == Block.predicateTutorialRotateExistingBlock)
								{
									Tutorial.manualPaintOrTexture.Add(tile);
									flag5 = true;
								}
								else if (predicate == Block.predicateTutorialRemoveBlockHint)
								{
									Tutorial.manualRemoveBlock.Add(tile);
									Tutorial.removeBlockTile = tile;
									flag5 = true;
								}
								else if (predicate == Block.predicateUnlocked || predicate == Block.predicateTutorialMoveBlock || predicate == Block.predicateTutorialMoveModel)
								{
									flag5 = true;
									if (predicate == Block.predicateTutorialMoveBlock)
									{
										Tutorial.moveBlockTile = tile;
									}
									else if (predicate == Block.predicateTutorialMoveModel)
									{
										Tutorial.moveModelTile = tile;
									}
								}
								if (flag5)
								{
									Tutorial.goalBlock.tiles[1].RemoveAt(1);
									if (Tutorial.goalBlock.tiles[1].Count == 1)
									{
										Tutorial.goalBlock.tiles.RemoveAt(1);
									}
								}
							}
						}
						Tutorial.originalToCopyTiles = new Dictionary<Tile, Tile>();
						Tutorial.goalBlockTilesCopy = Blocksworld.CloneBlockTiles(Tutorial.goalBlock.tiles, Tutorial.originalToCopyTiles, false, false);
						Tutorial.FilterGoalBlockTiles();
						Tutorial.state = TutorialState.DetermineInstructions;
					}
					Tutorial.stepOnNextUpdate = true;
					return;
				}
				Tutorial.arrow1.Show(false, 0);
				Tutorial.arrow2.Show(false, 0);
				Tutorial.target1.Show(false);
				Tutorial.target2.Show(false);
				Tutorial.hand1.Hide();
				Tutorial.hand2.Hide();
				TBox.tileButtonMove.Enable(false);
				TBox.tileButtonRotate.Enable(false);
				TBox.tileButtonScale.Enable(false);
				Tutorial.tileRowHint = null;
				Tutorial.usingQuickSelectColorIcon = false;
				Tutorial.usingQuickSelectTextureIcon = false;
				UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
				quickSelect.UpdateTextureIcon();
				quickSelect.UpdateColorIcon();
				Tutorial.autoCameraAngle = Tutorial.NearestCameraAngleSnap(Tutorial.autoCameraAngle.x, Blocksworld.cameraTransform.eulerAngles.y);
				if (BWSceneManager.BlockCount() > Tutorial.numGivenBlocks + Tutorial.blocks.Count)
				{
					Block lastBlock = Tutorial.GetLastBlock();
					Blocksworld.blocksworldCamera.Follow(lastBlock);
					Tutorial.HelpDestroyBlock(lastBlock);
					return;
				}
				if (Tutorial.step == Tutorial.blocks.Count)
				{
					Sound.PlaySound("Tutorial Complete", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
					Tutorial.state = TutorialState.BuildingCompleted;
					Tutorial.safeState = null;
					Tutorial.stepOnNextUpdate = true;
					Blocksworld.blocksworldCamera.Follow(Tutorial.GetLastBlock());
					return;
				}
				Tutorial.existingBlock = null;
				if (Tutorial.manualPaintOrTexture.Count > 0 && Tutorial.manualPaintOrTexture[0].gaf.Predicate.Name.Equals("Block.TutorialRotateExistingBlock"))
				{
					GAF gaf2 = Tutorial.manualPaintOrTexture[0].gaf;
					Block block = Tutorial.NthBlock((int)gaf2.Args[0]);
					Vector3 vector = (Vector3)gaf2.Args[1];
					if (!Tutorial.Close((Vector3)block.tiles[0][3].gaf.Args[0], vector))
					{
						Blocksworld.locked.Remove(block);
						Tutorial.existingBlock = block;
						Tutorial.HelpRotation(block, Quaternion.Euler(vector));
						return;
					}
					Tutorial.completedManualPaintOrTexture.Add(Tutorial.manualPaintOrTexture[0]);
					Tutorial.manualPaintOrTexture.RemoveAt(0);
					Blocksworld.Deselect(false, true);
					Blocksworld.locked.Add(block);
					Tutorial.stepOnNextUpdate = true;
					return;
				}
				else if (Tutorial.manualPaintOrTexture.Count > 0 && Tutorial.manualPaintOrTexture[0].gaf.Predicate == Block.predicateTutorialPaintExistingBlock)
				{
					GAF gaf3 = Tutorial.manualPaintOrTexture[0].gaf;
					Block block2 = Tutorial.NthBlock((int)gaf3.Args[0]);
					string text = (string)gaf3.Args[1];
					Vector3 cameraPos = (Vector3)gaf3.Args[2];
					Vector3 cameraTarget = (Vector3)gaf3.Args[3];
					if (!((string)block2.tiles[0][4].gaf.Args[0]).Equals(text))
					{
						Blocksworld.locked.Remove(block2);
						Tutorial.existingBlock = block2;
						Tutorial.HelpColor(block2, new GAF("Block.PaintTo", new object[]
						{
							text
						}), cameraPos, cameraTarget);
						return;
					}
					Tutorial.completedManualPaintOrTexture.Add(Tutorial.manualPaintOrTexture[0]);
					Tutorial.manualPaintOrTexture.RemoveAt(0);
					Blocksworld.locked.Add(block2);
					Tutorial.stepOnNextUpdate = true;
					return;
				}
				else if (Tutorial.manualPaintOrTexture.Count > 0 && Tutorial.manualPaintOrTexture[0].gaf.Predicate == Block.predicateTutorialTextureExistingBlock)
				{
					GAF gaf4 = Tutorial.manualPaintOrTexture[0].gaf;
					Block block3 = Tutorial.NthBlock((int)gaf4.Args[0]);
					string text2 = (string)gaf4.Args[1];
					Vector3 cameraPos2 = (Vector3)gaf4.Args[2];
					Vector3 cameraTarget2 = (Vector3)gaf4.Args[3];
					if (!Scarcity.GetNormalizedTexture((string)block3.tiles[0][5].gaf.Args[0]).Equals(text2))
					{
						Blocksworld.locked.Remove(block3);
						Tutorial.existingBlock = block3;
						Tutorial.HelpTexture(block3, new GAF("Block.TextureTo", new object[]
						{
							text2,
							Vector3.zero
						}), cameraPos2, cameraTarget2);
						return;
					}
					Tutorial.completedManualPaintOrTexture.Add(Tutorial.manualPaintOrTexture[0]);
					Tutorial.manualPaintOrTexture.RemoveAt(0);
					Blocksworld.locked.Add(block3);
					Tutorial.stepOnNextUpdate = true;
					return;
				}
				else if (Tutorial.manualRemoveBlock.Count > 0)
				{
					GAF gaf5 = Tutorial.manualRemoveBlock[0].gaf;
					int index = (int)gaf5.Args[0];
					Block block4 = Tutorial.NthBlock(index);
					Vector3 vector2 = (Vector3)gaf5.Args[1];
					Vector3 targetPosition = (Vector3)gaf5.Args[2];
					if (!(block4 is BlockPosition))
					{
						BlockPosition blockPosition = (BlockPosition)Blocksworld.bw.AddNewBlock(new Tile(new GAF(Block.predicateCreate, new object[]
						{
							"Position No Glue"
						})), false, null, true);
						blockPosition.MoveTo(blockPosition.GetPosition() + Vector3.one * 9999f);
						blockPosition.tiles.Insert(1, Block.LockedTileRow());
						blockPosition.go.SetActive(false);
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
						Tutorial.autoCameraEndPos = vector2;
						Blocksworld.blocksworldCamera.Unfollow();
						Blocksworld.blocksworldCamera.SetTargetPosition(targetPosition);
						Tutorial.HelpAutoCamera();
						Tutorial.stepOnNextUpdate = true;
						return;
					}
					Tutorial.manualRemoveBlock.RemoveAt(0);
					Tutorial.stepOnNextUpdate = true;
					return;
				}
				else
				{
					bool flag6 = Tutorial.goalBlock != null && Tutorial.goalBlock.HasGroup("tank-treads");
					if (Tutorial.manualGoalHint == null && flag6)
					{
						Block mainBlockInGroup = Tutorial.goalBlock.GetMainBlockInGroup("tank-treads");
						if (Tutorial.groupedManualGoalHints.ContainsKey(mainBlockInGroup))
						{
							Tutorial.manualGoalHint = Tutorial.groupedManualGoalHints[mainBlockInGroup];
						}
					}
					if (BWSceneManager.BlockCount() - Tutorial.numGivenBlocks < Tutorial.step + 1)
					{
						Blocksworld.blocksworldCamera.Follow(Tutorial.goalBlock);
						Tutorial.HelpCreateBlock();
						return;
					}
					if (Tutorial.state == TutorialState.CreateBlock)
					{
						TutorialActions.EnterContext(TutorialActionContext.AfterThisBlockCreate, Tutorial.goalBlock, null, null, null);
					}
					Block block5 = Tutorial.NthBlock(Tutorial.numGivenBlocks + Tutorial.step);
					Blocksworld.blocksworldCamera.Follow(block5);
					if (Tutorial.goalBlock == null)
					{
						Tutorial.state = TutorialState.GetNextBlock;
						Tutorial.stepOnNextUpdate = true;
						return;
					}
					if (block5.BlockType() != Tutorial.goalBlock.BlockType())
					{
						if (Tutorial.goalBlock.GetGroupOfType("locked-model") != null)
						{
							Blocksworld.locked.Add(block5);
							Tutorial.state = TutorialState.GetNextBlock;
							Tutorial.stepOnNextUpdate = true;
						}
						else
						{
							Tutorial.HelpDestroyBlock(block5);
						}
						return;
					}
					if (Tutorial.paintBeforePose && !Tutorial.CheckPaint(block5, Tutorial.goalBlock))
					{
						return;
					}
					if (Tutorial.textureBeforePose && !Tutorial.CheckTexture(block5, Tutorial.goalBlock))
					{
						return;
					}
					if (Tutorial.rotateBeforeScale)
					{
						if (!Tutorial.CheckRotation(block5, false))
						{
							return;
						}
						if (!Tutorial.CheckScale(block5))
						{
							return;
						}
					}
					else
					{
						if (!Tutorial.CheckScale(block5))
						{
							return;
						}
						if (!Tutorial.CheckRotation(block5, true))
						{
							return;
						}
					}
					Vector3 vector3 = Tutorial.goalBlock.go.transform.position + Vector3.one * 10000.25f;
					if (Tutorial.moveBlockTile != null || Tutorial.moveModelTile != null)
					{
						Tile tile2 = (Tutorial.moveBlockTile != null) ? Tutorial.moveBlockTile : Tutorial.moveModelTile;
						vector3 = (Vector3)tile2.gaf.Args[0];
					}
					if (!Tutorial.Close(block5.go.transform.position, Tutorial.goalBlock.go.transform.position) && !Tutorial.Close(Tutorial.goalBlock.go.transform.position, vector3))
					{
						Tutorial.HelpPosition(block5, false);
						return;
					}
					Tutorial.DisableOccludingConnectedColliders(block5);
					if (!Tutorial.paintBeforePose && !Tutorial.CheckPaint(block5, Tutorial.goalBlock))
					{
						return;
					}
					if (!Tutorial.textureBeforePose && !Tutorial.CheckTexture(block5, Tutorial.goalBlock))
					{
						return;
					}
					Tutorial.EnableOccludingConnectedColliders();
					TutorialActions.EnterContext(TutorialActionContext.BeginScripting, Tutorial.goalBlock, null, null, null);
					Dictionary<Tile, Tile> dictionary = new Dictionary<Tile, Tile>();
					Predicate predicate2 = PredicateRegistry.ByName("Meta.Then", true);
					for (int i = 1; i < block5.tiles.Count; i++)
					{
						int j = 0;
						while (j < block5.tiles[i].Count)
						{
							Tile tile3 = block5.tiles[i][j];
							bool flag7 = i < Tutorial.goalBlock.tiles.Count && j < Tutorial.goalBlock.tiles[i].Count;
							Tile tile4 = (!flag7) ? null : Tutorial.goalBlock.tiles[i][j];
							if (tile3 != null && tile3.gaf.Predicate == predicate2)
							{
								tile3 = null;
							}
							if (tile4 != null && tile4.gaf.Predicate == predicate2)
							{
								tile4 = null;
							}
							if (tile3 == null || tile4 == null)
							{
								goto IL_10F4;
							}
							if (!TileToggleChain.InSameChain(tile3.gaf.BlockItemId, tile4.gaf.BlockItemId))
							{
								if (tile3.gaf.EqualsInTutorial(tile4.gaf))
								{
									goto IL_10F4;
								}
								EditableTileParameter editableParameter = tile3.gaf.Predicate.EditableParameter;
								if (editableParameter != null)
								{
									GAF gaf6 = tile3.gaf.Clone();
									int parameterIndex = editableParameter.parameterIndex;
									gaf6.Args[parameterIndex] = null;
									if (gaf6.Matches(tile4.gaf))
									{
										if (Tutorial.okParameterTiles.Contains(tile4))
										{
											dictionary[tile4] = tile3;
											goto IL_12A6;
										}
										GAF gaf7 = tile4.gaf;
										GAF gaf8 = tile3.gaf;
										if (Tutorial.OkOverwriteParameterValue(editableParameter, gaf7.Predicate, gaf8.Args[parameterIndex], gaf7.Args[parameterIndex]))
										{
											gaf8.Args[parameterIndex] = gaf7.Args[parameterIndex];
											goto IL_12A6;
										}
										if (Tutorial.SnapParameterValue(gaf8, gaf7))
										{
											goto IL_12A6;
										}
										Tutorial.state = TutorialState.SetParameter;
										editableParameter.HelpSetParameterValueInTutorial(block5, tile3, tile4);
										Tutorial.UpdateTileRowHint(i);
										return;
									}
								}
								if (tile3.gaf.Predicate != Block.predicatePlaySoundDurational)
								{
									goto IL_10F4;
								}
								GAF gaf9 = tile4.gaf;
								GAF gaf10 = tile3.gaf;
								GAF gaf11 = gaf10.Clone();
								for (int k = 1; k < gaf11.Args.Length; k++)
								{
									gaf11.Args[k] = null;
								}
								if (gaf11.Matches(gaf9))
								{
									for (int l = 1; l < gaf9.Args.Length; l++)
									{
										gaf10.Args[l] = gaf9.Args[l];
									}
									return;
								}
								goto IL_10F4;
							}
							IL_12A6:
							j++;
							continue;
							IL_10F4:
							Tile tile5 = block5.tiles[i][j];
							if (!tile5.IsThen() && flag7)
							{
								Tile tile6 = Tutorial.goalBlock.tiles[i][j];
								if (!tile6.gaf.EqualsInTutorial(tile5.gaf))
								{
									for (int m = -j; m < Tutorial.goalBlock.tiles[i].Count - j + 1; m++)
									{
										if (m != 0)
										{
											bool flag8 = block5.tiles[i].Count > j + m && Tutorial.goalBlock.tiles[i].Count > j + m;
											if (flag8 && block5.tiles[i][j].gaf.EqualsInTutorial(Tutorial.goalBlock.tiles[i][j + m].gaf))
											{
												Tutorial.HelpSwapTiles(block5, block5.tiles[i][j], block5.tiles[i][j + m], Mathf.Sign((float)m));
												Tutorial.UpdateTileRowHint(i);
												return;
											}
										}
									}
								}
							}
							if (tile3 != null && (tile4 == null || !tile4.gaf.EqualsInTutorial(tile3.gaf)))
							{
								Tutorial.HelpRemoveTile(block5, block5.tiles[i][j]);
								Tutorial.UpdateTileRowHint(i);
								return;
							}
							goto IL_12A6;
						}
					}
					for (int n = 1; n < Mathf.Max(block5.tiles.Count, Tutorial.goalBlock.tiles.Count); n++)
					{
						if (n < Tutorial.goalBlock.tiles.Count && n < block5.tiles.Count)
						{
							for (int num = 0; num < Mathf.Max(block5.tiles[n].Count, Tutorial.goalBlock.tiles[n].Count); num++)
							{
								Tile tile7 = (block5.tiles.Count <= n || block5.tiles[n].Count <= num) ? null : block5.tiles[n][num];
								Tile tile8 = (Tutorial.goalBlock.tiles.Count <= n || Tutorial.goalBlock.tiles[n].Count <= num) ? null : Tutorial.goalBlock.tiles[n][num];
								if (tile7 != null && tile7.gaf.Predicate == predicate2)
								{
									tile7 = null;
								}
								if (tile8 != null && tile8.gaf.Predicate == predicate2)
								{
									tile8 = null;
								}
								if (!Tutorial.okParameterTiles.Contains(tile8))
								{
									if (tile8 != null && tile7 != null && !tile8.gaf.EqualsInTutorial(tile7.gaf) && TileToggleChain.InSameChain(tile8.gaf.BlockItemId, tile7.gaf.BlockItemId))
									{
										Tutorial.HelpToggleTile(block5, tile7);
										Tutorial.UpdateTileRowHint(n);
										Tutorial.lastTileWasToggle = true;
										return;
									}
									if (tile8 != null && (tile7 == null || !tile8.gaf.EqualsInTutorial(tile7.gaf)))
									{
										Tutorial.HelpAddTile(block5, Tutorial.goalBlock.tiles[n][num].gaf, n, num);
										Tutorial.UpdateTileRowHint(n);
										Tutorial.lastTileWasToggle = false;
										return;
									}
									if (tile7 != null && tile8 == null)
									{
										Tutorial.HelpRemoveTile(block5, block5.tiles[n][num]);
										Tutorial.UpdateTileRowHint(n);
										Tutorial.lastTileWasToggle = false;
										return;
									}
								}
							}
						}
					}
					if (Tutorial.lastTileWasToggle)
					{
						Tutorial.lastTileWasToggle = false;
						Tutorial.stateAfterWait = Tutorial.state;
						Tutorial.state = TutorialState.Waiting;
						Tutorial.waitTimeEnd = Time.time + 1f;
						return;
					}
					if (TutorialActions.EnterContext(TutorialActionContext.BlockDone, Tutorial.goalBlock, null, null, null))
					{
						Tutorial.stateAfterWait = Tutorial.state;
						Tutorial.state = TutorialState.Waiting;
						Tutorial.waitTimeEnd = Time.time + 2f;
						return;
					}
					if (Tutorial.moveBlockTile != null || Tutorial.moveModelTile != null)
					{
						Vector3 position = Tutorial.goalBlock.go.transform.position;
						if ((position - vector3).magnitude > 0.001f)
						{
							Tutorial.goalBlock.go.transform.position = vector3;
							Tutorial.goalBlockTilesCopy[0][2].gaf.Args[0] = vector3;
						}
						if (!Tutorial.Close(block5.go.transform.position, Tutorial.goalBlock.go.transform.position))
						{
							Tutorial.HelpPosition(block5, Tutorial.moveModelTile != null);
							return;
						}
					}
					if (Tutorial.originalToCopyTiles != null)
					{
						foreach (KeyValuePair<Tile, Tile> keyValuePair in dictionary)
						{
							Tile tile9;
							if (Tutorial.originalToCopyTiles.TryGetValue(keyValuePair.Key, out tile9))
							{
								tile9.gaf = keyValuePair.Value.gaf.Clone();
							}
						}
						Tutorial.originalToCopyTiles = null;
						dictionary.Clear();
					}
					Tutorial.okParameterTiles.Clear();
					block5.tiles = Tutorial.goalBlockTilesCopy;
					Tutorial.manualGoalHint = null;
					Tutorial.goalBlock.Destroy();
					Tutorial.groupedManualGoalHints.Remove(Tutorial.goalBlock);
					Blocksworld.scriptPanel.PositionReset();
					Blocksworld.locked.Add(block5);
					Blocksworld.Deselect(false, true);
					Blocksworld.buildPanel.PositionReset(false);
					if (!Tutorial.hideGraphics)
					{
						Blocksworld.stars.transform.position = block5.go.transform.position;
						Blocksworld.stars.Emit(30);
					}
					Sound.PlaySound("Tutorial Step", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
					bool flag9 = Tutorial.autoAddBlock;
					Tutorial.autoAddBlock = false;
					Tutorial.autoCameraAngle = new Vector3(30f, Tutorial.autoCameraAngle.y, 0f);
					Tutorial.autoCameraTimer = 0f;
					if (flag9)
					{
						TutorialActions.EnterContext(TutorialActionContext.AutoAddBlockWait, null, null, null, null);
						Tutorial.state = TutorialState.Waiting;
						Tutorial.waitTimeEnd = Time.time + Tutorial.autoAddBlockWaitTime;
						Tutorial.stateAfterWait = TutorialState.GetNextBlock;
						return;
					}
					Tutorial.state = TutorialState.GetNextBlock;
					Tutorial.stepOnNextUpdate = true;
				}
			}
			else if (Tutorial.mode == TutorialMode.Puzzle)
			{
				Tutorial.state = TutorialState.Puzzle;
				Tutorial.stepOnNextUpdate = true;
				return;
			}
			break;
		}
	}

	// Token: 0x06002181 RID: 8577 RVA: 0x000F8420 File Offset: 0x000F6820
	private static bool CheckPaint(Block thisBlock, Block goalBlock)
	{
		int num = 0;
		for (int i = 4; i < goalBlock.tiles[0].Count; i++)
		{
			GAF gaf = goalBlock.tiles[0][i].gaf;
			bool flag = gaf.Predicate == Block.predicatePaintTo;
			if (!goalBlock.IgnorePaintToIndexInTutorial(num) && flag && (thisBlock.tiles[0].Count <= i || !thisBlock.tiles[0][i].gaf.Equals(gaf)))
			{
				TutorialActions.EnterContext(TutorialActionContext.Paint, goalBlock, null, null, null);
				Tutorial.HelpColor(thisBlock, gaf, Util.nullVector3, Util.nullVector3);
				return false;
			}
			if (flag)
			{
				num++;
			}
		}
		return true;
	}

	// Token: 0x06002182 RID: 8578 RVA: 0x000F84EC File Offset: 0x000F68EC
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
				if (thisBlock.tiles[0].Count <= i || !flag || !gaf2.Args[0].Equals(gaf.Args[0]) || gaf2.Args.Length != gaf.Args.Length || (gaf2.Args.Length == 3 && !gaf2.Args[2].Equals(gaf.Args[2])) || ((mapping == Mapping.OneSideTo1x1 || mapping == Mapping.OneSideWrapTo1x1 || mapping == Mapping.TwoSidesTo1x1 || mapping == Mapping.TwoSidesWrapTo1x1) && !Tutorial.SameTutorialNormals(thisBlock.BlockType(), (Vector3)gaf2.Args[1], (Vector3)gaf.Args[1])))
				{
					TutorialActions.EnterContext(TutorialActionContext.Texture, goalBlock, null, null, null);
					Tutorial.HelpTexture(thisBlock, gaf, Util.nullVector3, Util.nullVector3);
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

	// Token: 0x06002183 RID: 8579 RVA: 0x000F8680 File Offset: 0x000F6A80
	public static bool DistanceOK(Vector3 point)
	{
		if (Tutorial.state == TutorialState.None)
		{
			return true;
		}
		if (Tutorial.mode == TutorialMode.Puzzle)
		{
			return true;
		}
		if (Tutorial.goalBlock == null || Tutorial.goalBlock.go == null)
		{
			return false;
		}
		float num = Util.MaxComponent(Tutorial.goalBlock.size);
		float magnitude = (point - Tutorial.goalBlock.go.transform.position).magnitude;
		return magnitude < 9f + num;
	}

	// Token: 0x06002184 RID: 8580 RVA: 0x000F8704 File Offset: 0x000F6B04
	public static bool RaycastTargetBlockOK(Block bl)
	{
		if (Tutorial.state == TutorialState.None)
		{
			return true;
		}
		if (Tutorial.mode == TutorialMode.Puzzle)
		{
			return true;
		}
		if (bl == null)
		{
			return true;
		}
		if (Tutorial.goalBlock == null || Tutorial.goalBlock.go == null)
		{
			return true;
		}
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (bl == worldOceanBlock)
		{
			Bounds waterBounds = worldOceanBlock.GetWaterBounds();
			Vector3 position = Tutorial.goalBlock.GetPosition();
			if (waterBounds.Contains(position))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06002185 RID: 8581 RVA: 0x000F8784 File Offset: 0x000F6B84
	private static void HelpTapBlock(Block block, bool allowBunchSelect = false)
	{
		if (Blocksworld.selectedBlock != null && !allowBunchSelect)
		{
			Blocksworld.Deselect(false, true);
		}
		block.EnableCollider(false);
		Vector3 position = block.go.transform.position;
		if (Tutorial.AutoCameraEnabled() && Tutorial.FindCameraAngleForPoint(position, false, Tutorial.autoCameraDist * 0.5f, Tutorial.autoCameraDist * 2f) && !Tutorial.Close(Blocksworld.cameraTransform.position, Tutorial.autoCameraEndPos))
		{
			Blocksworld.blocksworldCamera.SetTargetPosition(position);
			Tutorial.HelpAutoCamera();
			block.EnableCollider(true);
			return;
		}
		block.EnableCollider(true);
		Tutorial.state = ((!allowBunchSelect) ? TutorialState.TapBlock : TutorialState.SelectBunch);
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		Tutorial.target1.TargetBlock(block);
		Tutorial.hand1.TapTarget(Tutorial.target1, 0.25f);
	}

	// Token: 0x06002186 RID: 8582 RVA: 0x000F8865 File Offset: 0x000F6C65
	private static bool IsOutsideScreen(Tile tile)
	{
		return Tutorial.IsOutsideScreen(tile.GetPosition());
	}

	// Token: 0x06002187 RID: 8583 RVA: 0x000F8874 File Offset: 0x000F6C74
	private static bool IsOutsideScreen(Vector3 position)
	{
		float num = 80f * NormalizedScreen.pixelScale;
		return position.x < 0f || position.x > (float)Blocksworld.screenWidth - num || position.y < 0f || position.y > (float)Blocksworld.screenHeight - num * 1.5f;
	}

	// Token: 0x06002188 RID: 8584 RVA: 0x000F88E0 File Offset: 0x000F6CE0
	private static bool BlockInsideScreen(Block b)
	{
		Vector3 vector = Util.WorldToScreenPoint(b.go.transform.position, false);
		return vector.x > 0f && vector.x < (float)Blocksworld.screenWidth && vector.y > 0f && vector.y < (float)Blocksworld.screenHeight;
	}

	// Token: 0x06002189 RID: 8585 RVA: 0x000F894C File Offset: 0x000F6D4C
	private static bool PointInsideScreen(Vector3 s, float margin = 50f)
	{
		return s.x > margin && s.x < (float)Blocksworld.screenWidth - margin && s.y > margin && s.y < (float)Blocksworld.screenHeight - margin;
	}

	// Token: 0x0600218A RID: 8586 RVA: 0x000F899C File Offset: 0x000F6D9C
	private static void HelpScrollSidePanel(Tile tile)
	{
		Tutorial.state = TutorialState.Scroll;
		Vector3 position = tile.GetPosition();
		float num = (float)((position.y >= 0f) ? -150 : 150);
		float num2 = 13f;
		num2 *= NormalizedScreen.pixelScale;
		num *= NormalizedScreen.pixelScale;
		Tutorial.arrow1.state = TrackingState.Screen2Screen;
		Tutorial.arrow1.screen = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, (float)(Blocksworld.screenHeight / 2) - num + num, 0f);
		Tutorial.arrow1.screen2 = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, (float)(Blocksworld.screenHeight / 2) + num + num, 0f);
		Tutorial.arrow1.Show(true, 3);
		Tutorial.hand1.DragArrow(Tutorial.arrow1);
		Tutorial.arrow2.state = TrackingState.Screen2ScreenBounce;
		float num3 = (float)((position.y >= 0f) ? Blocksworld.screenHeight : 0);
		Tutorial.arrow2.bounce = (float)((position.y >= 0f) ? -1 : 1);
		Tutorial.arrow2.screen = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, num3 + num, 0f);
		Tutorial.arrow2.screen2 = new Vector3((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width / 2f + num2, num3, 0f);
		Tutorial.arrow2.Show(true, 1);
	}

	// Token: 0x0600218B RID: 8587 RVA: 0x000F8B40 File Offset: 0x000F6F40
	private static void HelpDragScriptPanel(Tile tile)
	{
		Tutorial.state = TutorialState.DragBlockPanel;
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
		Tutorial.arrow1.state = TrackingState.Screen2Screen;
		Tutorial.arrow1.screen = screen;
		Tutorial.arrow1.screen2 = targetPos;
		Tutorial.arrow1.Show(true, 0);
		Tutorial.hand1.DragArrow(Tutorial.arrow1);
	}

	// Token: 0x0600218C RID: 8588 RVA: 0x000F8C98 File Offset: 0x000F7098
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

	// Token: 0x0600218D RID: 8589 RVA: 0x000F8CF4 File Offset: 0x000F70F4
	private static bool HelpTapTab(GAF gaf)
	{
		int selectedTab = (int)Blocksworld.buildPanel.GetTabBar().SelectedTab;
		int tabIndexForGafSmart = PanelSlots.GetTabIndexForGafSmart(gaf);
		if (selectedTab != tabIndexForGafSmart)
		{
			Tutorial.HelpTapTab(tabIndexForGafSmart);
			return true;
		}
		return false;
	}

	// Token: 0x0600218E RID: 8590 RVA: 0x000F8D28 File Offset: 0x000F7128
	private static void HelpTapTab(int tabIndex)
	{
		float tabHeight = Blocksworld.UI.TabBar.GetTabHeight();
		Vector3 screen = new Vector3((float)NormalizedScreen.width - 0.5f * TabBar.pixelWidth, (float)NormalizedScreen.height - tabHeight * ((float)tabIndex + 0.5f) / NormalizedScreen.scale, 0f);
		Tutorial.target1.TargetScreen(screen);
		Tutorial.hand1.TapTarget(Tutorial.target1, 0.25f);
		Tutorial.state = TutorialState.TapTab;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
	}

	// Token: 0x0600218F RID: 8591 RVA: 0x000F8DB8 File Offset: 0x000F71B8
	private static void HelpCreateBlock()
	{
		if (Tutorial.goalBlock == null || Tutorial.goalBlock.go == null)
		{
			return;
		}
		TutorialActions.EnterContext(TutorialActionContext.BeforeThisBlockCreate, Tutorial.goalBlock, null, null, null);
		bool flag = Tutorial.goalBlock.HasAnyGroup();
		Tile tile = Tutorial.goalBlock.tiles[0][1];
		if (Tutorial.cheatNextStep || Tutorial.autoAddBlock)
		{
			Tutorial.cheatNextStep = false;
			if (!flag)
			{
				Block block = Blocksworld.CloneBlock(Tutorial.goalBlock);
				ConnectednessGraph.Update(block);
				if (!Tutorial.autoAddBlock)
				{
					Blocksworld.Select(block, false, true);
				}
				Tutorial.stepOnNextUpdate = true;
			}
			return;
		}
		GAF gaf = tile.gaf;
		if (flag)
		{
			if (Tutorial.goalBlock.HasGroup("locked-model"))
			{
				if (Blocksworld.modelCollection != null)
				{
					BlockGroup groupOfType = Tutorial.goalBlock.GetGroupOfType("locked-model");
					ModelData modelData = Blocksworld.modelCollection.FindSimilarModel(groupOfType.GetBlockList());
					if (modelData == null)
					{
						BWLog.Warning("Could not find a similar model to place");
					}
					else
					{
						gaf = modelData.tile.gaf;
					}
				}
				else
				{
					BWLog.Warning("Model collection was null");
				}
			}
			else
			{
				gaf = Tutorial.goalBlock.GetIconGaf();
			}
		}
		if (Tutorial.HelpTapTab(gaf))
		{
			return;
		}
		Blocksworld.enabledGAFs = new List<GAF>
		{
			gaf
		};
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tile tile2 = Blocksworld.buildPanel.FindTileMatching(gaf);
		if (tile2 == null)
		{
			BWLog.Warning("Could not find tile for gaf " + gaf);
			Blocksworld.UpdateTiles();
			Tutorial.stepOnNextUpdate = true;
			return;
		}
		bool flag2 = Tutorial.AutoCameraEnabled();
		Tutorial.rotateBeforeScale = false;
		Tutorial.paintBeforePose = false;
		Tutorial.textureBeforePose = false;
		Tutorial.autoAnglesFilter = Vector3.zero;
		Tutorial.useTwoFingerScale = false;
		Tutorial.useTwoFingerMove = false;
		Tutorial.includeDefaultBlockTiles = false;
		if (Tutorial.HelpOperationPose(TutorialState.CreateBlock, -1))
		{
			return;
		}
		if (Tutorial.manualGoalHint != null)
		{
			object[] args = Tutorial.manualGoalHint.gaf.Args;
			Tutorial.gluePointBlock = (Vector3)args[0];
			Tutorial.glueNormal = (Vector3)args[1];
			Vector3 euler = (Vector3)args[2];
			Tutorial.autoCameraDist = (float)args[3];
			Tutorial.autoCameraEndPos = Tutorial.gluePointBlock - Tutorial.autoCameraDist * (Quaternion.Euler(euler) * Vector3.forward);
			Tutorial.rotateBeforeScale = (args.Length > 4 && (int)args[4] != 0);
			Tutorial.paintBeforePose = Util.GetIntBooleanArg(args, 5, false);
			Tutorial.textureBeforePose = Util.GetIntBooleanArg(args, 6, false);
			Tutorial.autoAnglesFilter = Util.GetVector3Arg(args, 7, Vector3.zero);
			Tutorial.useTwoFingerScale = Util.GetIntBooleanArg(args, 8, false);
			Tutorial.useTwoFingerMove = Util.GetIntBooleanArg(args, 9, false);
			Tutorial.includeDefaultBlockTiles = Util.GetIntBooleanArg(args, 10, false);
			if (flag2 && !Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				Blocksworld.blocksworldCamera.Unfollow();
				Blocksworld.blocksworldCamera.SetTargetPosition(Tutorial.gluePointBlock);
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		else
		{
			Tutorial.autoCameraDist = 15f + 2f * (Util.MaxComponent(Tutorial.goalBlock.Scale()) - 1f);
			Tutorial.FindGluePoint(Tutorial.goalBlock, null);
			bool flag3 = flag2 && Tutorial.FindCameraAngleForPoint(Tutorial.gluePointBlock, false, Tutorial.autoCameraDist * 0.5f, Tutorial.autoCameraDist * 2f) && !Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position);
			if (flag3)
			{
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		if (Tutorial.goalBlock != null && Tutorial.goalBlock.go != null)
		{
			Vector3 vector = Blocksworld.mainCamera.WorldToScreenPoint(Tutorial.goalBlock.go.transform.position);
			float num = 0.25f * (float)Screen.width;
			float num2 = 0.6f * (float)Screen.width;
			float num3 = 0.1f * (float)Screen.height;
			float num4 = 0.9f * (float)Screen.height;
			Rect rect = new Rect(num, num3, num2 - num, num4 - num3);
			if (!rect.Contains(new Vector2(vector.x, vector.y)))
			{
				Blocksworld.blocksworldCamera.Focus();
			}
		}
		if (Tutorial.IsOutsideScreen(tile2))
		{
			Tutorial.HelpScrollSidePanel(tile2);
			Tutorial.scrollToTile = tile2;
		}
		else
		{
			Tutorial.state = TutorialState.CreateBlock;
			Tutorial.helpPositionTarget = Tutorial.gluePointBlock;
			Tutorial.arrow1.state = TrackingState.TileWorld;
			Tutorial.arrow1.tile = tile2.tileObject;
			Tutorial.arrow1.world = Tutorial.helpPositionTarget;
			Tutorial.arrow1.Show(true, 0);
			Tutorial.target1.TargetTile(tile2.tileObject);
			Tutorial.target2.TargetSurface(Tutorial.helpPositionTarget, Tutorial.glueNormal);
			Tutorial.target2.Show(true);
			Tutorial.hand1.DragArrow(Tutorial.arrow1);
		}
	}

	// Token: 0x06002190 RID: 8592 RVA: 0x000F92AF File Offset: 0x000F76AF
	private static Vector3 GetHelpOrbitScreenPoint()
	{
		return new Vector3(((float)NormalizedScreen.width - Blocksworld.buildPanel.width) / 2f, 80f, 0f);
	}

	// Token: 0x06002191 RID: 8593 RVA: 0x000F92D8 File Offset: 0x000F76D8
	private static void ShowHelpOrbit(int leftOf)
	{
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tutorial.autoCameraAngle = Tutorial.NearestCameraAngleSnap(30f, Blocksworld.cameraTransform.eulerAngles.y + (float)(90 * leftOf));
	}

	// Token: 0x06002192 RID: 8594 RVA: 0x000F9324 File Offset: 0x000F7724
	private static void HelpOrbit(Vector3? maybeNormal = null)
	{
		Vector3 valueOrDefault = maybeNormal.GetValueOrDefault(Tutorial.glueNormal);
		Tutorial.state = TutorialState.Orbit;
		int num = Util.LeftOf(valueOrDefault, Blocksworld.cameraForward, Vector3.up);
		if (num == 0)
		{
			num = 1;
		}
		Tutorial.ShowHelpOrbit(num);
	}

	// Token: 0x06002193 RID: 8595 RVA: 0x000F9363 File Offset: 0x000F7763
	private static bool CameraIsLevel()
	{
		TBox.Update();
		return TBox.tileButtonMove != null && TBox.IsMoveUp();
	}

	// Token: 0x06002194 RID: 8596 RVA: 0x000F9384 File Offset: 0x000F7784
	private static void HelpDestroyBlock(Block b)
	{
		TutorialActions.EnterContext(TutorialActionContext.RemoveBlock, null, null, null, null);
		Blocksworld.locked.Remove(b);
		bool flag = Tutorial.AutoCameraEnabled();
		if (flag && Tutorial.removeBlockTile != null)
		{
			Tutorial.autoCameraEndPos = (Vector3)Tutorial.removeBlockTile.gaf.Args[1];
			if (!Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		if (Blocksworld.selectedBlock != b)
		{
			Tutorial.HelpTapBlock(b, false);
			return;
		}
		b.EnableCollider(false);
		if (flag && (Tutorial.removeBlockTile != null || Tutorial.FindCameraAngleForPoint(b.GetPosition() + Vector3.up, true, 0f, 3.40282347E+38f)) && !Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
		{
			Tutorial.HelpAutoCamera();
		}
		else
		{
			Tutorial.state = TutorialState.DestroyBlock;
			Blocksworld.enabledGAFs = new List<GAF>();
			Blocksworld.enabledPanelBlock = null;
			Blocksworld.UpdateTiles();
			Tutorial.arrow1.state = TrackingState.Block2Screen;
			Tutorial.arrow1.block = b;
			Tutorial.arrow1.screen = new Vector3((float)NormalizedScreen.width - Blocksworld.buildPanel.width / 2f, Util.WorldToScreenPoint(b.go.transform.position, false).y, 0f);
			Tutorial.arrow1.Show(true, 0);
			Tutorial.hand1.DragArrow(Tutorial.arrow1);
			Tutorial.target1.TargetBlock(b);
		}
		b.EnableCollider(true);
	}

	// Token: 0x06002195 RID: 8597 RVA: 0x000F9514 File Offset: 0x000F7914
	private static Vector3 FindSidePointForBlock(Block newBlock)
	{
		Vector3 position = newBlock.go.transform.position;
		float d = Mathf.Max(Tutorial.autoCameraDist, 5f + 2.5f * Util.MaxComponent(Util.Abs(newBlock.Scale())));
		Vector3 vector = -(d * (Quaternion.Euler(5f, Blocksworld.cameraTransform.eulerAngles.y, 0f) * Vector3.forward));
		int num = 16;
		float num2 = float.MinValue;
		Vector3 b = vector;
		for (int i = 0; i < num; i++)
		{
			float num3 = 0f;
			float angle = 360f * ((float)i / (float)num);
			Vector3 vector2 = Quaternion.AngleAxis(angle, Vector3.up) * vector;
			RaycastHit[] array = Physics.RaycastAll(position, vector2.normalized, Tutorial.autoCameraDist);
			float num4 = Tutorial.autoCameraDist;
			foreach (RaycastHit raycastHit in array)
			{
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, true);
				if (block != null && block != newBlock)
				{
					num4 = Mathf.Min((raycastHit.point - position).magnitude, num4);
				}
			}
			num3 -= Tutorial.autoCameraDist - num4;
			num3 -= (vector - vector2).magnitude * 0.01f;
			if (num3 > num2)
			{
				num2 = num3;
				b = vector2;
			}
		}
		return position + b;
	}

	// Token: 0x06002196 RID: 8598 RVA: 0x000F96AC File Offset: 0x000F7AAC
	private static void CheatRotateScaleMove(Block newBlock, Block goalBlock)
	{
		newBlock.RotateTo(goalBlock.GetRotation());
		newBlock.ScaleTo(goalBlock.Scale(), true, true);
		newBlock.MoveTo(goalBlock.GetPosition());
		Tutorial.cheatNextStep = false;
		Tutorial.stepOnNextUpdate = true;
		TBox.FitToSelected();
		ConnectednessGraph.Update(newBlock);
	}

	// Token: 0x06002197 RID: 8599 RVA: 0x000F96FC File Offset: 0x000F7AFC
	private static void HelpScale(Block newBlock)
	{
		if (Tutorial.cheatNextStep)
		{
			Tutorial.CheatRotateScaleMove(newBlock, Tutorial.goalBlock);
			return;
		}
		if (Blocksworld.selectedBlock != newBlock)
		{
			Tutorial.HelpTapBlock(newBlock, false);
			return;
		}
		Tutorial.state = TutorialState.Scale;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		Vector3 vector = Tutorial.goalBlock.Scale();
		Vector3 point = newBlock.Scale();
		bool flag = newBlock is BlockAbstractWheel;
		if (flag && vector.y == vector.z && point.y == point.z)
		{
			point.y = vector.y;
		}
		Quaternion rotation = newBlock.go.transform.rotation;
		Vector3 v = rotation * vector;
		Vector3 v2 = rotation * point;
		Vector3 b = Util.Abs(v) - Util.Abs(v2);
		b.x = ((Mathf.Abs(b.x) >= 0.01f) ? b.x : 0f);
		b.y = ((Mathf.Abs(b.y) >= 0.01f) ? b.y : 0f);
		b.z = ((Mathf.Abs(b.z) >= 0.01f) ? b.z : 0f);
		bool flag2 = Tutorial.AutoCameraEnabled();
		bool flag3 = Mathf.Abs(b.y) > 0.01f;
		Vector3 position = Blocksworld.cameraTransform.position;
		if (flag3)
		{
			if (flag2 && !Tutorial.CameraIsLevel() && !Tutorial.useTwoFingerScale)
			{
				Tutorial.autoCameraEndPos = Tutorial.FindSidePointForBlock(newBlock);
				if (!Tutorial.Close(Tutorial.autoCameraEndPos, position))
				{
					Tutorial.HelpAutoCamera();
					return;
				}
			}
		}
		else if (flag2)
		{
			float d = Mathf.Max(Tutorial.autoCameraDist, 5f + 2.5f * Util.MaxComponent(Util.Abs(vector)));
			Tutorial.autoCameraEndPos = newBlock.go.transform.position - d * (Quaternion.Euler(30f, Blocksworld.cameraTransform.eulerAngles.y, 0f) * Vector3.forward);
			if (!Tutorial.Close(Tutorial.autoCameraEndPos, position))
			{
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		if ((b.x < 0f && Vector3.Dot(new Vector3(b.x, 0f, 0f), Blocksworld.cameraForward) < 0f) || (b.x > 0f && Vector3.Dot(new Vector3(b.x, 0f, 0f), Blocksworld.cameraForward) > 0f))
		{
			b.x = -b.x;
		}
		if ((b.z < 0f && Vector3.Dot(new Vector3(0f, 0f, b.z), Blocksworld.cameraForward) < 0f) || (b.z > 0f && Vector3.Dot(new Vector3(0f, 0f, b.z), Blocksworld.cameraForward) > 0f))
		{
			b.z = -b.z;
		}
		if (Tutorial.CameraIsLevel() || flag3)
		{
			b.x = (b.z = 0f);
		}
		TBox.tileButtonScale.Show(true);
		TBox.tileButtonScale.Enable(true);
		TBox.UpdateButtons();
		Vector3 scaleButtonInWorldSpace = TBox.GetScaleButtonInWorldSpace();
		Tutorial.arrow1.state = TrackingState.Button2World;
		Tutorial.arrow1.tile = TBox.tileButtonScale;
		Tutorial.arrow1.world = scaleButtonInWorldSpace + b;
		Hand hand = Tutorial.hand1;
		Tutorial.arrow1.Show(true, 0);
		if (flag3 && Mathf.Abs(b.x) < 0.01f && Mathf.Abs(b.z) < 0.01f && Tutorial.useTwoFingerScale && !Tutorial.CameraIsLevel())
		{
			hand = Tutorial.hand2;
		}
		Tutorial.target1.TargetTile(TBox.tileButtonScale);
		hand.screenOffset = Vector3.zero;
		if (hand == Tutorial.hand2)
		{
			float d2 = 80f;
			hand.screenOffset = Vector3.right * 25f;
			Tutorial.arrow2.state = TrackingState.Screen2Screen;
			Vector3 b2 = Vector3.right * d2;
			Tutorial.arrow2.screen = Tutorial.arrow1.tile.GetPosition() + new Vector3(40f, 40f, 0f) + b2;
			Tutorial.arrow2.screen2 = Util.WorldToScreenPoint(Tutorial.arrow1.world, false) + b2;
			Tutorial.arrow2.Show(true, 0);
			Tutorial.target2.TargetScreen(Tutorial.arrow2.screen);
		}
		hand.DragArrow(Tutorial.arrow1);
		if (Util.MaxComponent(vector) > 12f)
		{
			TBox.targetSnapScale = vector;
			TBox.targetSnapScaleMaxDistance = Util.Round(vector * 0.1f);
		}
	}

	// Token: 0x06002198 RID: 8600 RVA: 0x000F9C54 File Offset: 0x000F8054
	private static void FindRotations(Quaternion from, Quaternion to, int n)
	{
		if (Quaternion.Angle(from, to) < 5f)
		{
			Tutorial.bestN = n;
			for (int i = 0; i < n; i++)
			{
				Tutorial.best[i] = Tutorial.rots[i];
			}
		}
		else if (n < Tutorial.bestN)
		{
			Tutorial.rots[n].x = 0f;
			Tutorial.rots[n].y = 90f;
			Tutorial.rots[n].z = 0f;
			Tutorial.FindRotations(Quaternion.Euler(Tutorial.rots[n]) * from, to, n + 1);
			Tutorial.rots[n].x = 0f;
			Tutorial.rots[n].y = 180f;
			Tutorial.rots[n].z = 0f;
			Tutorial.FindRotations(Quaternion.Euler(Tutorial.rots[n]) * from, to, n + 1);
			Tutorial.rots[n].x = 0f;
			Tutorial.rots[n].y = -90f;
			Tutorial.rots[n].z = 0f;
			Tutorial.FindRotations(Quaternion.Euler(Tutorial.rots[n]) * from, to, n + 1);
			Tutorial.rots[n] = 90f * Tutorial.dragYRotationAxis;
			Tutorial.FindRotations(Quaternion.Euler(Tutorial.rots[n]) * from, to, n + 1);
			Tutorial.rots[n] = 180f * Tutorial.dragYRotationAxis;
			Tutorial.FindRotations(Quaternion.Euler(Tutorial.rots[n]) * from, to, n + 1);
			Tutorial.rots[n] = -90f * Tutorial.dragYRotationAxis;
			Tutorial.FindRotations(Quaternion.Euler(Tutorial.rots[n]) * from, to, n + 1);
		}
	}

	// Token: 0x06002199 RID: 8601 RVA: 0x000F9EAC File Offset: 0x000F82AC
	private static void HelpRotation(Block newBlock, Quaternion rotation)
	{
		bool flag = newBlock.HasGroup("locked-model");
		if (Tutorial.cheatNextStep && !flag)
		{
			Tutorial.CheatRotateScaleMove(newBlock, Tutorial.goalBlock);
			return;
		}
		if (Blocksworld.selectedBlock != newBlock && !flag)
		{
			Tutorial.HelpTapBlock(newBlock, false);
			return;
		}
		Tutorial.state = TutorialState.Rotation;
		int num = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
		Tutorial.dragYRotationAxis = -Vector3.right;
		if (num == 1)
		{
			Tutorial.dragYRotationAxis = Vector3.forward;
		}
		else if (num == 2)
		{
			Tutorial.dragYRotationAxis = Vector3.right;
		}
		else if (num == 3)
		{
			Tutorial.dragYRotationAxis = -Vector3.forward;
		}
		Tutorial.bestN = 4;
		Tutorial.FindRotations(newBlock.go.transform.rotation, rotation, 0);
		Vector3 off = new Vector3(-Tutorial.best[0].y, 0f, 0f);
		if (num == 3)
		{
			off.y = Tutorial.best[0].z;
		}
		else if (num == 2)
		{
			off.y = -Tutorial.best[0].x;
		}
		else if (num == 1)
		{
			off.y = -Tutorial.best[0].z;
		}
		else
		{
			off.y = Tutorial.best[0].x;
		}
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		TBox.tileButtonRotate.Show(true);
		TBox.tileButtonRotate.Enable(true);
		TBox.UpdateButtons();
		Tutorial.arrow1.state = TrackingState.Tile2Screen;
		Tutorial.arrow1.tile = TBox.tileButtonRotate;
		Tutorial.arrow1.screen = TBox.tileButtonRotate.GetCenterPosition() + off;
		bool flag2 = Tutorial.AutoCameraEnabled();
		if (flag2 && TBox.tileButtonRotate != null)
		{
			if (Tutorial.FindCameraAngleForScreenPoints(delegate
			{
				TBox.UpdateButtons();
				return TBox.tileButtonRotate.GetPosition();
			}, () => TBox.tileButtonRotate.GetPosition() + off))
			{
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		Tutorial.arrow1.Show(true, 0);
		Tutorial.hand1.DragArrow(Tutorial.arrow1);
		Tutorial.target1.TargetTile(TBox.tileButtonRotate);
	}

	// Token: 0x0600219A RID: 8602 RVA: 0x000FA130 File Offset: 0x000F8530
	private static void FindRotations2D(Quaternion view, Quaternion from, Quaternion to, int n)
	{
		if (Quaternion.Angle(from, to) < 5f)
		{
			Tutorial.bestN2D = n;
			for (int i = 0; i < n; i++)
			{
				Tutorial.best2D[i] = Tutorial.rots2D[i];
			}
		}
		else if (n < Tutorial.bestN2D)
		{
			if (n == 0 || Tutorial.rots2D[n - 1].z == 0f)
			{
				Tutorial.rots2D[n].x = 0f;
				Tutorial.rots2D[n].y = 0f;
				Tutorial.rots2D[n].z = 90f;
				Tutorial.FindRotations2D(view, Quaternion.AngleAxis(90f, view * Vector3.forward) * from, to, n + 1);
				Tutorial.rots2D[n].x = 0f;
				Tutorial.rots2D[n].y = 0f;
				Tutorial.rots2D[n].z = 180f;
				Tutorial.FindRotations2D(view, Quaternion.AngleAxis(180f, view * Vector3.forward) * from, to, n + 1);
				Tutorial.rots2D[n].x = 0f;
				Tutorial.rots2D[n].y = 0f;
				Tutorial.rots2D[n].z = 270f;
				Tutorial.FindRotations2D(view, Quaternion.AngleAxis(270f, view * Vector3.forward) * from, to, n + 1);
			}
			Tutorial.rots2D[n].x = (float)(((view * Vector3.forward).y != 0f) ? -90 : 90);
			Tutorial.rots2D[n].y = 0f;
			Tutorial.rots2D[n].z = 0f;
			Tutorial.FindRotations2D(view * Quaternion.Euler(Tutorial.rots2D[n]), from, to, n + 1);
			Tutorial.rots2D[n].x = 0f;
			Tutorial.rots2D[n].y = 90f;
			Tutorial.rots2D[n].z = 0f;
			Tutorial.FindRotations2D(view * Quaternion.Euler(Tutorial.rots2D[n]), from, to, n + 1);
			Tutorial.rots2D[n].x = 0f;
			Tutorial.rots2D[n].y = -90f;
			Tutorial.rots2D[n].z = 0f;
			Tutorial.FindRotations2D(view * Quaternion.Euler(Tutorial.rots2D[n]), from, to, n + 1);
		}
	}

	// Token: 0x0600219B RID: 8603 RVA: 0x000FA434 File Offset: 0x000F8834
	private static void FindTexturePoint(Block block, Vector3 normal)
	{
		Vector3 origin = block.go.transform.position + block.go.GetComponent<Collider>().bounds.extents.magnitude * normal;
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(origin, -normal), out raycastHit, 10f))
		{
			Tutorial.glueNormal = raycastHit.normal;
			Tutorial.gluePointWorld = raycastHit.point;
			Tutorial.gluePointBlock = Tutorial.gluePointWorld;
		}
		else
		{
			BWLog.Info("Couldn't find normal of face to apply texture to (shouldn't happen)");
			Tutorial.glueNormal = Vector3.up;
			Tutorial.gluePointWorld = block.go.transform.position;
			Tutorial.gluePointBlock = Tutorial.gluePointWorld;
		}
	}

	// Token: 0x0600219C RID: 8604 RVA: 0x000FA4F4 File Offset: 0x000F88F4
	private static void FindGluePoint(Block block, Block ignore = null)
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Block> list3 = new List<Block>();
		ConnectednessGraph.GlueBonds(block, ignore, list, list2, list3);
		Vector3 vector = Util.Abs(block.GetRotation() * block.GetScale());
		Vector3 a = block.go.transform.position - 0.5f * vector.y * Vector3.up;
		bool flag = Util.PointWithinTerrain(a - 0.5f * Vector3.up, false);
		bool flag2 = list2.Count == 0 || flag;
		if (!flag2)
		{
			int i = 0;
			while (i < list2.Count)
			{
				Tutorial.gluePointBlock = list[i];
				Tutorial.gluePointWorld = list2[i];
				Tutorial.glueNormal = Vector3.zero;
				Block block2 = list3[i];
				Vector3 vector2 = block2.go.transform.position - Tutorial.gluePointWorld;
				RaycastHit raycastHit;
				if (!(vector2 != Vector3.zero) || !block2.go.GetComponent<Collider>().Raycast(new Ray(Tutorial.gluePointWorld - vector2, vector2), out raycastHit, 10f))
				{
					goto IL_154;
				}
				Tutorial.glueNormal = raycastHit.normal;
				if (Tutorial.glueNormal.y >= -0.01f)
				{
					goto IL_154;
				}
				flag2 = true;
				IL_173:
				i++;
				continue;
				IL_154:
				if (Tutorial.TestVisibility(Tutorial.gluePointBlock, Blocksworld.cameraTransform.position, 0.1f))
				{
					return;
				}
				goto IL_173;
			}
		}
		if (flag2)
		{
			if (!flag)
			{
				RaycastHit raycastHit2;
				if (Physics.Raycast(a + Vector3.up * 0.1f, -Vector3.up, out raycastHit2))
				{
					float magnitude = (a - raycastHit2.point).magnitude;
					Block block3 = BWSceneManager.FindBlock(raycastHit2.collider.gameObject, true);
					float num = Util.MaxComponent(block.size);
					if (magnitude >= num + 9f || block3 == null || block3 is BlockSky)
					{
						Tutorial.SetPlacementHelper(a);
					}
					else
					{
						a = raycastHit2.point;
					}
				}
				else
				{
					Tutorial.SetPlacementHelper(a);
				}
			}
			Tutorial.glueNormal = Vector3.up;
			Tutorial.gluePointWorld = a;
			Tutorial.gluePointBlock = a;
			return;
		}
	}

	// Token: 0x0600219D RID: 8605 RVA: 0x000FA75C File Offset: 0x000F8B5C
	private static void DisablePlacementHelper()
	{
		if (Tutorial.placementHelper != null)
		{
			BoxCollider component = Tutorial.placementHelper.GetComponent<BoxCollider>();
			component.enabled = false;
		}
	}

	// Token: 0x0600219E RID: 8606 RVA: 0x000FA78B File Offset: 0x000F8B8B
	public static void DestroyPlacementHelper()
	{
		if (Tutorial.placementHelper != null)
		{
			UnityEngine.Object.Destroy(Tutorial.placementHelper);
		}
	}

	// Token: 0x0600219F RID: 8607 RVA: 0x000FA7A8 File Offset: 0x000F8BA8
	private static GameObject SetPlacementHelper(Vector3 top)
	{
		if (Tutorial.placementHelper == null)
		{
			Tutorial.placementHelper = new GameObject("Block Placement Helper");
			Tutorial.placementHelper.AddComponent<BoxCollider>();
		}
		BoxCollider component = Tutorial.placementHelper.GetComponent<BoxCollider>();
		component.enabled = true;
		float num = 0.1f;
		component.size = new Vector3(3f, num, 3f);
		Tutorial.placementHelper.transform.position = top - Vector3.up * num * 0.5f;
		return Tutorial.placementHelper;
	}

	// Token: 0x060021A0 RID: 8608 RVA: 0x000FA83C File Offset: 0x000F8C3C
	private static bool FindCameraAngleForPoint(Vector3 targetPos, bool ignoreCameraIsLevel = false, float minDistance = 0f, float maxDistance = 3.40282347E+38f)
	{
		float magnitude = (targetPos - Blocksworld.cameraTransform.position).magnitude;
		if ((ignoreCameraIsLevel || !Tutorial.CameraIsLevel()) && magnitude >= minDistance && magnitude <= maxDistance && Tutorial.TestCameraVisibility(targetPos, null))
		{
			return false;
		}
		int num = 8;
		int num2 = int.MinValue;
		Vector3[] array = new Vector3[]
		{
			0.5f * Vector3.forward,
			-0.5f * Vector3.forward,
			0.5f * Vector3.right,
			-0.5f * Vector3.right
		};
		float[] array2 = new float[]
		{
			-30f,
			-45f,
			-60f
		};
		Vector3 position = Blocksworld.cameraTransform.position;
		List<Vector3> list = new List<Vector3>();
		list.Add(position);
		list.Add(targetPos + (position - targetPos).normalized * Tutorial.autoCameraDist);
		foreach (float x in array2)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 item = targetPos + Tutorial.autoCameraDist * (Quaternion.Euler(x, (float)(j * (360 / num) + 45), 0f) * Vector3.forward);
				list.Add(item);
			}
		}
		bool result = false;
		for (int k = 0; k < list.Count; k++)
		{
			Vector3 vector = list[k];
			if (Tutorial.TestVisibility(vector, targetPos, 0.6f))
			{
				Vector3 vector2 = vector - position;
				float magnitude2 = (vector - targetPos).magnitude;
				int num3 = -Mathf.RoundToInt(Mathf.Min(Tutorial.autoCameraDist, vector2.magnitude) * 5f);
				if (magnitude2 >= minDistance && magnitude2 <= maxDistance)
				{
					num3 += 1000;
				}
				foreach (Vector3 b in array)
				{
					if (Tutorial.TestCameraVisibility(targetPos + b, new Vector3?(vector)))
					{
						num3 += 10000;
					}
				}
				if (num3 > num2)
				{
					num2 = num3;
					Tutorial.autoCameraEndPos = vector;
					result = true;
				}
			}
		}
		return result;
	}

	// Token: 0x060021A1 RID: 8609 RVA: 0x000FAADC File Offset: 0x000F8EDC
	private static bool FindCameraAngleForScreenPoints(Func<Vector3> p1, Func<Vector3> p2)
	{
		if (!Tutorial.PointInsideScreen(p1(), 50f) || !Tutorial.PointInsideScreen(p2(), 50f))
		{
			Vector3 position = Blocksworld.cameraTransform.position;
			for (int i = 0; i < 10; i++)
			{
				Blocksworld.blocksworldCamera.SetCameraPosition(position - (float)(i * 5) * Blocksworld.cameraForward);
				if (Tutorial.PointInsideScreen(p1(), 50f) && Tutorial.PointInsideScreen(p2(), 50f))
				{
					if (i > 0)
					{
						Tutorial.autoCameraEndPos = Blocksworld.cameraTransform.position;
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

	// Token: 0x060021A2 RID: 8610 RVA: 0x000FABAC File Offset: 0x000F8FAC
	private static bool TestVisibility(Vector3 p1, Vector3 p2, float stopShort = 0.1f)
	{
		return !Physics.Raycast(p1, p2 - p1, (p2 - p1).magnitude - stopShort);
	}

	// Token: 0x060021A3 RID: 8611 RVA: 0x000FABDC File Offset: 0x000F8FDC
	private static bool TestCameraVisibility(Vector3 targetPos, Vector3? camPos = null)
	{
		float num = 0.1f;
		Vector3 vector = (camPos == null) ? Blocksworld.cameraTransform.position : camPos.Value;
		return GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(targetPos, Vector3.one * 2f)) && !Physics.Raycast(vector, (targetPos - vector).normalized, (targetPos - vector).magnitude - num);
	}

	// Token: 0x060021A4 RID: 8612 RVA: 0x000FAC68 File Offset: 0x000F9068
	private static void HelpPosition(Block newBlock, bool moveModel = false)
	{
		Blocksworld.locked.Remove(newBlock);
		if (Tutorial.cheatNextStep && !moveModel)
		{
			Tutorial.CheatRotateScaleMove(newBlock, Tutorial.goalBlock);
			return;
		}
		Tutorial.state = TutorialState.Position;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.UpdateTiles();
		bool flag = moveModel && (Blocksworld.selectedBunch == null || !Blocksworld.selectedBunch.blocks.Contains(newBlock));
		bool flag2 = Tutorial.goalBlock.HasGroup("locked-model");
		if ((!moveModel && !flag2 && Blocksworld.selectedBlock != newBlock) || flag)
		{
			Tutorial.HelpTapBlock(newBlock, flag);
			return;
		}
		Vector3 position = Tutorial.goalBlock.go.transform.position;
		Vector3 vector = newBlock.go.transform.position;
		Vector3 vector2 = position - vector;
		Tutorial.helpPositionTarget = position;
		if (!moveModel && !flag2)
		{
			if (Tutorial.TutorialSnap(vector, position))
			{
				vector = position;
				newBlock.MoveTo(vector);
				TBox.FitToSelected();
				Tutorial.stepOnNextUpdate = true;
				Tutorial.state = TutorialState.DetermineInstructions;
				ConnectednessGraph.Update(newBlock);
				return;
			}
			if (Tutorial.TutorialSnapXZ(vector, position))
			{
				vector = new Vector3(position.x, vector.y, position.z);
				newBlock.MoveTo(vector);
				TBox.FitToSelected();
			}
			else if (Tutorial.TutorialSnapY(vector, position))
			{
				vector = new Vector3(vector.x, position.y, vector.z);
				newBlock.MoveTo(vector);
				TBox.FitToSelected();
			}
		}
		if (Mathf.Abs(vector2.y) < Mathf.Epsilon && (Blocksworld.selectedBlock == newBlock || moveModel || flag2))
		{
			TBox.tileButtonMove.Show(true);
			TBox.tileButtonMove.Enable(true);
			TBox.UpdateButtons();
		}
		if (moveModel || flag2)
		{
			Tutorial.HelpPositionMove(newBlock, vector, position);
			return;
		}
		List<Vector3> resultPos = new List<Vector3>();
		List<Vector3> resultPos2 = new List<Vector3>();
		List<Block> resultBlock = new List<Block>();
		ConnectednessGraph.GlueBonds(newBlock, null, resultPos, resultPos2, resultBlock);
		Tutorial.HelpPositionMove(newBlock, vector, position);
	}

	// Token: 0x060021A5 RID: 8613 RVA: 0x000FAE78 File Offset: 0x000F9278
	private static void HelpPositionMove(Block newBlock, Vector3 fromPos, Vector3 toPos)
	{
		bool flag = Tutorial.AutoCameraEnabled();
		Vector3 vector = toPos - fromPos;
		bool flag2 = Mathf.Abs(vector.x) < 0.01f && Mathf.Abs(vector.z) < 0.01f && Mathf.Abs(vector.y) > 0.01f;
		if (!Tutorial.useTwoFingerMove && flag && flag2 && !Tutorial.CameraIsLevel())
		{
			Tutorial.autoCameraEndPos = Tutorial.FindSidePointForBlock(newBlock);
			if (!Tutorial.Close(Blocksworld.cameraTransform.position, Tutorial.autoCameraEndPos))
			{
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		else if (flag && (vector.x != 0f || vector.z != 0f))
		{
			float d = Mathf.Max(Tutorial.autoCameraDist, 5f + 2.5f * Mathf.Max(Util.MaxComponent(Util.Abs(newBlock.Scale())), Util.MaxComponent(Util.Abs(vector))));
			Tutorial.autoCameraEndPos = newBlock.go.transform.position - d * (Quaternion.Euler(30f, Blocksworld.cameraTransform.eulerAngles.y, 0f) * Vector3.forward);
			if (!Tutorial.Close(Blocksworld.cameraTransform.position, Tutorial.autoCameraEndPos))
			{
				Tutorial.HelpAutoCamera();
				return;
			}
		}
		TBox.tileButtonMove.Show(true);
		TBox.tileButtonMove.Enable(true);
		TBox.UpdateButtons();
		Hand hand = Tutorial.hand1;
		if (flag2 && Tutorial.useTwoFingerMove)
		{
			if (!Tutorial.CameraIsLevel())
			{
				hand = Tutorial.hand2;
			}
		}
		else if (!Tutorial.CameraIsLevel())
		{
			vector.y = 0f;
		}
		if (TBox.tileButtonMove.IsShowing() && !TBox.IsRaycastDragging())
		{
			Tutorial.arrow1.state = TrackingState.MoveButtonHelper;
			Vector3 world = (!flag2) ? new Vector3(toPos.x, fromPos.y, toPos.z) : toPos;
			Tutorial.arrow1.world = world;
			float d2 = Util.ScreenToWorldScale(fromPos) / Util.ScreenToWorldScale(toPos);
			Tutorial.arrow1.offset = d2 * (TBox.tileButtonMove.GetCenterPosition() - Util.WorldToScreenPointSafe(fromPos));
			Tutorial.target1.TargetTile(TBox.tileButtonMove);
		}
		else
		{
			Tutorial.arrow1.state = TrackingState.Block2World;
			Tutorial.arrow1.block = newBlock;
			Tutorial.arrow1.world = toPos;
		}
		Tutorial.arrow1.Show(true, 0);
		hand.DragArrow(Tutorial.arrow1);
		Vector3 vector2 = newBlock.Scale();
		if (Util.MaxComponent(vector2) > 12f)
		{
			Transform transform = newBlock.go.transform;
			Vector3 a = Quaternion.Inverse(transform.rotation) * vector2;
			TBox.targetSnapPosition = transform.position + vector;
			TBox.targetSnapPositionMaxDistance = Util.Abs(Util.Round(a * 0.1f));
		}
		hand.screenOffset = Vector3.zero;
		if (hand == Tutorial.hand2)
		{
			float d3 = 80f;
			hand.screenOffset = Vector3.right * 25f;
			Tutorial.arrow2.state = TrackingState.Screen2Screen;
			Vector3 b = Vector3.right * d3;
			Tutorial.arrow2.screen = Tutorial.arrow1.tile.GetPosition() + new Vector3(40f, 40f, 0f) + b;
			Tutorial.arrow2.screen2 = Util.WorldToScreenPoint(Tutorial.arrow1.world, false) + b;
			Tutorial.arrow2.Show(true, 0);
			Tutorial.target1.TargetScreen(Tutorial.arrow2.screen);
		}
		Tutorial.FindGluePoint(Tutorial.goalBlock, null);
		Tutorial.target2.TargetSurface(Tutorial.gluePointBlock, Tutorial.glueNormal);
	}

	// Token: 0x060021A6 RID: 8614 RVA: 0x000FB27C File Offset: 0x000F967C
	private static bool TutorialSnap(Vector3 p1, Vector3 p2)
	{
		return (p1 - p2).magnitude < 1.25f && Mathf.Abs(p1.y - p2.y) < 0.5f;
	}

	// Token: 0x060021A7 RID: 8615 RVA: 0x000FB2C0 File Offset: 0x000F96C0
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

	// Token: 0x060021A8 RID: 8616 RVA: 0x000FB314 File Offset: 0x000F9714
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

	// Token: 0x060021A9 RID: 8617 RVA: 0x000FB348 File Offset: 0x000F9748
	private static void HelpPositionMoveTo(Block newBlock)
	{
		bool flag = Tutorial.AutoCameraEnabled();
		Tutorial.FindGluePoint(Tutorial.goalBlock, null);
		if (flag && Tutorial.FindCameraAngleForPoint(Tutorial.gluePointBlock, true, 0f, 3.40282347E+38f) && !Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
		{
			Tutorial.HelpAutoCamera();
		}
		else
		{
			Tutorial.arrow1.state = TrackingState.BlockOffsetWorld;
			Tutorial.arrow1.block = newBlock;
			Tutorial.arrow1.offset = Vector3.zero;
			Tutorial.arrow1.world = Tutorial.gluePointBlock;
			Tutorial.target1.TargetSurface(Tutorial.gluePointBlock, -Tutorial.glueNormal);
			Tutorial.target2.TargetSurface(Tutorial.gluePointBlock, Tutorial.glueNormal);
			Tutorial.arrow1.Show(true, 0);
			Tutorial.hand1.DragArrow(Tutorial.arrow1);
		}
	}

	// Token: 0x060021AA RID: 8618 RVA: 0x000FB428 File Offset: 0x000F9828
	private static void HelpColor(Block block, GAF gafPaintTo, Vector3 cameraPos, Vector3 cameraTarget)
	{
		string text = (string)gafPaintTo.Args[0];
		Tutorial.forceMeshIndex = ((gafPaintTo.Args.Length <= 1) ? 0 : ((int)gafPaintTo.Args[1]));
		if (Tutorial.cheatNextStep)
		{
			block.PaintTo(text, true, Tutorial.forceMeshIndex);
			Tutorial.stepOnNextUpdate = true;
			Tutorial.cheatNextStep = false;
			return;
		}
		GAF lastPaintedColorGAF = Blocksworld.clipboard.GetLastPaintedColorGAF();
		Tutorial.usingQuickSelectColorIcon = (text == (string)lastPaintedColorGAF.Args[0]);
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		quickSelect.UpdateColorIcon();
		Vector2 v = quickSelect.PaintRectCenter();
		if (!Tutorial.usingQuickSelectColorIcon && Tutorial.HelpTapTab(gafPaintTo))
		{
			return;
		}
		GAF gaf = new GAF("Block.PaintTo", new object[]
		{
			text
		});
		Blocksworld.enabledGAFs = new List<GAF>
		{
			gaf
		};
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tile tile = Blocksworld.buildPanel.FindTileMatching(gaf);
		if (tile == null)
		{
			BWLog.Error("Could not find tile for gaf " + new GAF("Block.PaintTo", new object[]
			{
				text
			}));
		}
		bool flag = Tutorial.AutoCameraEnabled();
		if (Tutorial.HelpOperationPose(TutorialState.Color, Tutorial.forceMeshIndex))
		{
			return;
		}
		block.EnableCollider(false);
		Vector3 position = Blocksworld.cameraTransform.position;
		if (flag && cameraPos != Util.nullVector3 && (!Tutorial.Close(Tutorial.autoCameraEndPos, position) || !Tutorial.Close((cameraTarget - cameraPos).normalized, Blocksworld.cameraForward)))
		{
			Tutorial.autoCameraEndPos = cameraPos;
			Blocksworld.blocksworldCamera.Unfollow();
			Blocksworld.blocksworldCamera.SetTargetPosition(cameraTarget);
			Tutorial.HelpAutoCamera();
		}
		else if (flag && Util.IsNullVector3(cameraPos) && Tutorial.FindCameraAngleForPoint(block.go.transform.position, false, Tutorial.autoCameraDist * 0.5f, Tutorial.autoCameraDist * 1.5f) && !Tutorial.Close(Tutorial.autoCameraEndPos, position))
		{
			Tutorial.HelpAutoCamera();
		}
		else if (!Tutorial.usingQuickSelectColorIcon && Tutorial.IsOutsideScreen(tile))
		{
			Tutorial.HelpScrollSidePanel(tile);
			Tutorial.scrollToTile = tile;
		}
		else
		{
			bool flag2 = true;
			Tutorial.state = TutorialState.Color;
			if (!Util.IsNullVector3(cameraPos))
			{
				if (Tutorial.usingQuickSelectColorIcon)
				{
					Tutorial.target1.TargetScreen(v);
					Tutorial.arrow1.state = TrackingState.Screen2Screen;
					Tutorial.arrow1.screen = v;
					Tutorial.arrow1.screen2 = Tutorial.CenterOfScreen();
				}
				else
				{
					Tutorial.target1.TargetTile(tile.tileObject);
					Tutorial.arrow1.state = TrackingState.Tile2Screen;
					Tutorial.arrow1.tile = tile.tileObject;
					Tutorial.arrow1.screen = Tutorial.CenterOfScreen();
				}
				Tutorial.target2.TargetScreen(Tutorial.CenterOfScreen());
			}
			else if (Tutorial.forceMeshIndex != 0)
			{
				Vector3 center = block.GetSubMesh(Tutorial.forceMeshIndex).AABB.center;
				if (flag && !Tutorial.TestCameraVisibility(center, null) && Tutorial.FindCameraAngleForPoint(center, false, 0f, 3.40282347E+38f) && !Tutorial.Close(Tutorial.autoCameraEndPos, position))
				{
					Blocksworld.blocksworldCamera.Unfollow();
					Blocksworld.blocksworldCamera.SetTargetPosition(center);
					Tutorial.HelpAutoCamera();
					flag2 = false;
				}
				else
				{
					if (Tutorial.usingQuickSelectColorIcon)
					{
						Tutorial.target1.TargetScreen(v);
						Tutorial.arrow1.state = TrackingState.Screen2World;
						Tutorial.arrow1.screen = v;
					}
					else
					{
						Tutorial.target1.TargetTile(tile.tileObject);
						Tutorial.arrow1.state = TrackingState.TileWorld;
						Tutorial.arrow1.tile = tile.tileObject;
					}
					Tutorial.arrow1.world = center;
					Tutorial.target2.TargetWorld(Tutorial.arrow1.world);
				}
			}
			else
			{
				Vector3 position2 = block.go.transform.position;
				if (flag && !Tutorial.TestCameraVisibility(position2, null) && Tutorial.FindCameraAngleForPoint(position2, false, 0f, 3.40282347E+38f) && !Tutorial.Close(Tutorial.autoCameraEndPos, position))
				{
					Blocksworld.blocksworldCamera.Unfollow();
					Blocksworld.blocksworldCamera.SetTargetPosition(position2);
					Tutorial.HelpAutoCamera();
					flag2 = false;
				}
				else
				{
					if (Tutorial.usingQuickSelectColorIcon)
					{
						Tutorial.target1.TargetScreen(v);
						Tutorial.arrow1.state = TrackingState.Screen2Block;
						Tutorial.arrow1.screen = v;
					}
					else
					{
						Tutorial.target1.TargetTile(tile.tileObject);
						Tutorial.arrow1.state = TrackingState.TileBlock;
						Tutorial.arrow1.tile = tile.tileObject;
					}
					Tutorial.arrow1.block = block;
					Tutorial.target2.TargetBlock(block);
				}
			}
			if (flag2)
			{
				Tutorial.arrow1.Show(true, 0);
				Tutorial.hand1.DragArrow(Tutorial.arrow1);
			}
		}
		block.EnableCollider(true);
	}

	// Token: 0x060021AB RID: 8619 RVA: 0x000FB954 File Offset: 0x000F9D54
	private static void HelpTexture(Block newBlock, GAF gafTextureTo, Vector3 cameraPos, Vector3 cameraTarget)
	{
		string text = (string)gafTextureTo.Args[0];
		Vector3 vector = (Vector3)gafTextureTo.Args[1];
		Tutorial.forceMeshIndex = ((gafTextureTo.Args.Length <= 2) ? 0 : ((int)gafTextureTo.Args[2]));
		if (Tutorial.cheatNextStep)
		{
			newBlock.TextureTo(text, vector, true, Tutorial.forceMeshIndex, false);
			Tutorial.stepOnNextUpdate = true;
			Tutorial.cheatNextStep = false;
			return;
		}
		vector = Tutorial.goalBlock.go.transform.rotation * vector;
		GAF lastTextureGAF = Blocksworld.clipboard.GetLastTextureGAF();
		Tutorial.usingQuickSelectTextureIcon = (text == (string)lastTextureGAF.Args[0]);
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		quickSelect.UpdateTextureIcon();
		Vector2 v = quickSelect.TextureRectCenter();
		if (!Tutorial.usingQuickSelectTextureIcon && Tutorial.HelpTapTab(gafTextureTo))
		{
			return;
		}
		GAF item = new GAF("Block.TextureTo", new object[]
		{
			text,
			Vector3.zero
		});
		Blocksworld.enabledGAFs = new List<GAF>
		{
			item
		};
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		BuildPanel buildPanel = Blocksworld.buildPanel;
		string predicateName = "Block.TextureTo";
		object[] array = new object[2];
		array[0] = text;
		Tile tile = buildPanel.FindTileMatching(new GAF(predicateName, array));
		if (tile == null)
		{
			object arg = "Could not find tile for gaf ";
			string predicateName2 = "Block.TextureTo";
			object[] array2 = new object[2];
			array2[0] = text;
			BWLog.Info(arg + new GAF(predicateName2, array2).ToString());
			if (newBlock != null)
			{
				BWLog.Info("Falling back to auto texturing the block instead...");
				newBlock.TextureTo(text, vector, true, Tutorial.forceMeshIndex, false);
				Tutorial.stepOnNextUpdate = true;
				return;
			}
		}
		if (Tutorial.HelpOperationPose(TutorialState.Texture, Tutorial.forceMeshIndex))
		{
			return;
		}
		bool flag = Tutorial.AutoCameraEnabled();
		Mapping mapping = Materials.GetMapping(text);
		if (Util.IsNullVector3(cameraPos) && (mapping == Mapping.OneSideTo1x1 || mapping == Mapping.TwoSidesTo1x1 || mapping == Mapping.TwoSidesWrapTo1x1 || mapping == Mapping.OneSideWrapTo1x1))
		{
			Tutorial.FindTexturePoint(newBlock, vector);
			if (flag && Tutorial.FindCameraAngleForPoint(Tutorial.gluePointBlock, false, Tutorial.autoCameraDist * 0.5f, Tutorial.autoCameraDist * 1.5f) && !Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				Tutorial.HelpAutoCamera();
			}
			else if (!Tutorial.usingQuickSelectTextureIcon && Tutorial.IsOutsideScreen(tile))
			{
				Tutorial.HelpScrollSidePanel(tile);
				Tutorial.scrollToTile = tile;
			}
			else
			{
				Tutorial.state = TutorialState.Texture;
				if (Tutorial.usingQuickSelectTextureIcon)
				{
					Tutorial.arrow1.state = TrackingState.Screen2World;
					Tutorial.arrow1.screen = v;
					Tutorial.target1.TargetScreen(v);
				}
				else
				{
					Tutorial.arrow1.state = TrackingState.TileWorld;
					Tutorial.arrow1.tile = tile.tileObject;
					Tutorial.target1.TargetTile(tile.tileObject);
				}
				Tutorial.arrow1.world = Tutorial.gluePointWorld;
				Tutorial.target2.TargetSurface(Tutorial.gluePointWorld, Tutorial.glueNormal);
				Tutorial.arrow1.Show(true, 0);
				Tutorial.hand1.DragArrow(Tutorial.arrow1);
			}
		}
		else
		{
			newBlock.EnableCollider(false);
			if (flag && !Util.IsNullVector3(cameraPos) && (!Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position) || !Tutorial.Close((cameraTarget - cameraPos).normalized, Blocksworld.cameraForward)))
			{
				Tutorial.autoCameraEndPos = cameraPos;
				Blocksworld.blocksworldCamera.Unfollow();
				Blocksworld.blocksworldCamera.SetTargetPosition(cameraTarget);
				Tutorial.HelpAutoCamera();
			}
			else if (flag && Util.IsNullVector3(cameraPos) && Tutorial.FindCameraAngleForPoint(newBlock.go.transform.position, false, 0f, 3.40282347E+38f) && !Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
			{
				Tutorial.HelpAutoCamera();
			}
			else if (!Tutorial.usingQuickSelectTextureIcon && Tutorial.IsOutsideScreen(tile))
			{
				Tutorial.HelpScrollSidePanel(tile);
				Tutorial.scrollToTile = tile;
			}
			else
			{
				Tutorial.state = TutorialState.Texture;
				if (Tutorial.usingQuickSelectTextureIcon)
				{
					Tutorial.target1.TargetScreen(v);
				}
				else
				{
					Tutorial.target1.TargetTile(tile.tileObject);
				}
				if (!Util.IsNullVector3(cameraPos))
				{
					if (Tutorial.usingQuickSelectTextureIcon)
					{
						Tutorial.arrow1.state = TrackingState.Screen2Screen;
						Tutorial.arrow1.screen = v;
						Tutorial.arrow1.screen2 = Tutorial.CenterOfScreen();
						Tutorial.target1.TargetScreen(v);
					}
					else
					{
						Tutorial.arrow1.state = TrackingState.Tile2Screen;
						Tutorial.arrow1.tile = tile.tileObject;
						Tutorial.arrow1.screen = Tutorial.CenterOfScreen();
					}
					Tutorial.target2.TargetScreen(Tutorial.CenterOfScreen());
				}
				else if (Tutorial.forceMeshIndex != 0)
				{
					if (Tutorial.usingQuickSelectTextureIcon)
					{
						Tutorial.arrow1.state = TrackingState.Screen2World;
						Tutorial.arrow1.screen = v;
						Tutorial.target1.TargetScreen(v);
					}
					else
					{
						Tutorial.arrow1.state = TrackingState.TileWorld;
						Tutorial.arrow1.tile = tile.tileObject;
					}
					Tutorial.arrow1.world = Tutorial.goalBlock.GetSubMesh(Tutorial.forceMeshIndex).AABB.center;
					Tutorial.target2.TargetWorld(Tutorial.arrow1.world);
				}
				else
				{
					if (Tutorial.usingQuickSelectTextureIcon)
					{
						Tutorial.arrow1.state = TrackingState.Screen2Block;
						Tutorial.arrow1.screen = v;
						Tutorial.target1.TargetScreen(v);
					}
					else
					{
						Tutorial.arrow1.state = TrackingState.TileBlock;
						Tutorial.arrow1.tile = tile.tileObject;
					}
					Tutorial.arrow1.block = Tutorial.goalBlock;
					Tutorial.target2.TargetBlock(Tutorial.goalBlock);
				}
				Tutorial.arrow1.Show(true, 0);
				Tutorial.hand1.DragArrow(Tutorial.arrow1);
			}
			newBlock.EnableCollider(true);
		}
	}

	// Token: 0x060021AC RID: 8620 RVA: 0x000FBF6C File Offset: 0x000FA36C
	private static void HelpRemoveTile(Block newBlock, Tile tile)
	{
		if (Blocksworld.selectedBlock != newBlock)
		{
			Tutorial.HelpTapBlock(newBlock, false);
			return;
		}
		if (Tutorial.IsOutsideScreen(tile))
		{
			Tutorial.HelpDragScriptPanel(tile);
			return;
		}
		Tutorial.state = TutorialState.RemoveTile;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tutorial.arrow1.state = TrackingState.Tile2Screen;
		Tutorial.arrow1.tile = tile.tileObject;
		Vector3[] array = new Vector3[]
		{
			new Vector3((float)NormalizedScreen.width - Blocksworld.buildPanel.width / 2f, tile.GetPosition().y, 0f),
			new Vector3((float)NormalizedScreen.width * 0.5f, 40f, 0f),
			new Vector3((float)NormalizedScreen.width * 0.5f, (float)NormalizedScreen.height - 40f, 0f),
			new Vector3(40f, (float)NormalizedScreen.height * 0.5f)
		};
		foreach (Vector3 vector in array)
		{
			if (!Blocksworld.scriptPanel.Hit(vector))
			{
				Tutorial.arrow1.screen = vector;
				break;
			}
		}
		Tutorial.arrow1.Show(true, 0);
		Tutorial.hand1.DragArrow(Tutorial.arrow1);
		Tutorial.target1.TargetTile(tile.tileObject);
	}

	// Token: 0x060021AD RID: 8621 RVA: 0x000FC104 File Offset: 0x000FA504
	private static void HelpSwapTiles(Block newBlock, Tile tile1, Tile tile2, float offsetSign = 1f)
	{
		if (Blocksworld.selectedBlock != newBlock)
		{
			Tutorial.HelpTapBlock(newBlock, false);
			return;
		}
		if (Tutorial.IsOutsideScreen(tile1))
		{
			Tutorial.HelpDragScriptPanel(tile1);
			return;
		}
		Tutorial.state = TutorialState.SwapTiles;
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Tutorial.arrow1.state = TrackingState.Tile2Tile;
		Tutorial.arrow1.tile = tile1.tileObject;
		Tutorial.arrow1.tile2 = tile2.tileObject;
		Tutorial.arrow1.offset = offsetSign * Vector3.right * 80f * 0.5f * NormalizedScreen.pixelScale;
		Tutorial.target1.TargetTile(tile1.tileObject);
		Tutorial.arrow1.Show(true, 0);
		Tutorial.hand1.DragArrow(Tutorial.arrow1);
	}

	// Token: 0x060021AE RID: 8622 RVA: 0x000FC1DC File Offset: 0x000FA5DC
	public static void HelpToggleTile(Block newBlock, Tile tile)
	{
		if (Blocksworld.selectedBlock != newBlock)
		{
			Tutorial.HelpTapBlock(newBlock, false);
			return;
		}
		if (Tutorial.IsOutsideScreen(tile))
		{
			Tutorial.HelpDragScriptPanel(tile);
			return;
		}
		Tutorial.state = TutorialState.TapTile;
		Tutorial.target1.TargetTile(tile.tileObject);
		Tutorial.hand1.TapTarget(Tutorial.target1, 0.25f);
	}

	// Token: 0x060021AF RID: 8623 RVA: 0x000FC238 File Offset: 0x000FA638
	private static void HelpAddTile(Block newBlock, GAF gaf, int row, int col)
	{
		if (Tutorial.cheatNextStep)
		{
			Tutorial.stepOnNextUpdate = true;
			Tutorial.cheatNextStep = false;
			newBlock.tiles = Blocksworld.CloneBlockTiles(Tutorial.goalBlock.tiles, null, false, false);
			return;
		}
		if (Blocksworld.selectedBlock != newBlock)
		{
			Tutorial.HelpTapBlock(newBlock, false);
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
		if (Tutorial.IsOutsideScreen(tile) || !tile.IsShowing())
		{
			Tutorial.HelpDragScriptPanel(tile);
			return;
		}
		ScriptPanel scriptPanel = Blocksworld.scriptPanel;
		Vector3 position = scriptPanel.position;
		bool flag = position.x < 0f;
		bool flag2 = scriptPanel.width + 80f > Blocksworld.buildPanel.position.x;
		bool flag3 = position.x + scriptPanel.width > Blocksworld.buildPanel.position.x;
		if (flag3 && !flag2)
		{
			Tutorial.HelpDragScriptPanel(tile);
			return;
		}
		if (flag && !flag2)
		{
			Tutorial.HelpDragScriptPanel(tile);
			return;
		}
		List<Tile> list2 = (row >= Tutorial.goalBlock.tiles.Count) ? null : Tutorial.goalBlock.tiles[row];
		TutorialActions.EnterContext(TutorialActionContext.DuringScriptThisRow, Tutorial.goalBlock, list2, null, null);
		Vector3 screen = Vector3.zero;
		if (gaf.Predicate == Block.predicatePaintTo)
		{
			GAF lastPaintedColorGAF = Blocksworld.clipboard.GetLastPaintedColorGAF();
			Tutorial.usingQuickSelectColorIcon = ((string)gaf.Args[0] == (string)lastPaintedColorGAF.Args[0]);
			UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
			quickSelect.UpdateColorIcon();
			Vector2 v = quickSelect.PaintRectCenter();
			screen = v;
		}
		if (!Tutorial.usingQuickSelectColorIcon && Tutorial.HelpTapTab(gaf))
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
			GAF gaf2 = gaf.Clone();
			EditableTileParameter editableParameter = gaf2.Predicate.EditableParameter;
			if (editableParameter != null)
			{
				object obj = null;
				NumericHandleTileParameter numericHandleTileParameter = editableParameter as NumericHandleTileParameter;
				if (numericHandleTileParameter != null)
				{
					object obj2 = gaf.Args[numericHandleTileParameter.parameterIndex];
					if (obj2 is int)
					{
						float num3 = (float)((int)obj2);
						obj = num3;
					}
					else if (obj2 is float)
					{
						float num3 = (float)obj2;
						obj = num3;
					}
					else
					{
						BWLog.Info("Unsupported numeric in tutorial: " + obj2.GetType().Name);
					}
				}
				gaf2.Args[editableParameter.parameterIndex] = obj;
				if (Blocksworld.buildPanel.FindTileMatching(gaf2) == null)
				{
					for (int i = editableParameter.parameterIndex; i < gaf2.Args.Length; i++)
					{
						gaf2.Args[i] = null;
					}
				}
			}
			tile2 = Blocksworld.buildPanel.FindTileMatching(gaf2);
		}
		if (tile2 == null && gaf.Predicate == Block.predicatePlaySoundDurational)
		{
			GAF gaf3 = gaf.Clone();
			for (int j = 1; j < gaf3.Args.Length; j++)
			{
				gaf3.Args[j] = null;
			}
			tile2 = Blocksworld.buildPanel.FindTileMatching(gaf3);
		}
		if (gaf.Predicate == BlockMaster.predicateSetEnvEffect)
		{
			string text = BlockSky.EnvEffectToTexture((string)gaf.Args[0]);
			GAF patternGaf = new GAF(Block.predicateTextureTo, new object[]
			{
				text
			});
			tile2 = Blocksworld.buildPanel.FindTileMatching(patternGaf);
		}
		if (gaf.Predicate == BlockMaster.predicatePaintSkyTo)
		{
			GAF patternGaf2 = new GAF(Block.predicatePaintTo, new object[]
			{
				(string)gaf.Args[0]
			});
			tile2 = Blocksworld.buildPanel.FindTileMatching(patternGaf2);
		}
		if (gaf.Predicate == Block.predicateSendCustomSignal || gaf.Predicate == Block.predicateSendCustomSignalModel)
		{
			Tutorial.targetSignalName = (string)gaf.Args[0];
			Tutorial.targetSignalPredicate = gaf.Predicate;
			GAF patternGaf3 = new GAF(Block.predicateSendCustomSignal, new object[]
			{
				"*"
			});
			tile2 = Blocksworld.buildPanel.FindTileMatching(patternGaf3);
		}
		if (tile2 == null)
		{
			BWLog.Error("Could not find tile for gaf " + gaf);
			return;
		}
		Blocksworld.enabledGAFs = new List<GAF>
		{
			tile2.gaf
		};
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		if (!Tutorial.usingQuickSelectColorIcon && Tutorial.IsOutsideScreen(tile2))
		{
			Tutorial.HelpScrollSidePanel(tile2);
			Tutorial.scrollToTile = tile2;
		}
		else
		{
			if (list2 != null)
			{
				int num4 = col - 1;
				Tile tileBefore = (num4 <= 0 || num4 >= list2.Count) ? null : list2[num4];
				Tile tileAfter = (num4 + 1 >= list2.Count) ? null : list2[num4 + 1];
				TutorialActions.EnterContext(TutorialActionContext.DuringScriptNextTile, Tutorial.goalBlock, list2, tileBefore, tileAfter);
			}
			Tutorial.state = TutorialState.AddTile;
			Tutorial.arrow1.state = TrackingState.TilePanelOffset;
			Tutorial.arrow1.tile = tile2.tileObject;
			Tutorial.arrow1.panel = Blocksworld.scriptPanel;
			List<Tile> list3 = tiles[row];
			bool flag4 = false;
			for (int k = col; k < list3.Count; k++)
			{
				if (list3[k].gaf.Predicate == Block.predicateThen)
				{
					flag4 = true;
					break;
				}
			}
			if (flag4)
			{
				Tutorial.arrow1.offset = tiles[row][col].GetCenterPosition() - new Vector3(1f, 0f, 0f) * 75f * NormalizedScreen.pixelScale - Blocksworld.scriptPanel.position;
			}
			else
			{
				Tutorial.arrow1.offset = tiles[row][col - 1].GetCenterPosition() + new Vector3(0.5f, 0f, 0f) * 75f * NormalizedScreen.pixelScale - Blocksworld.scriptPanel.position;
			}
			Tutorial.arrow1.Show(true, 0);
			Tutorial.target1.TargetTile(tile2.tileObject);
			if (Tutorial.usingQuickSelectColorIcon)
			{
				Tutorial.target1.TargetScreen(screen);
				Tutorial.arrow1.state = TrackingState.ScreenPanelOffset;
				Tutorial.arrow1.screen = screen;
			}
			Tutorial.target2.TargetPanelOffset(Tutorial.arrow1.panel, Tutorial.arrow1.offset);
			Tutorial.hand1.DragArrow(Tutorial.arrow1);
		}
	}

	// Token: 0x060021B0 RID: 8624 RVA: 0x000FC9C8 File Offset: 0x000FADC8
	private static void HelpPlay()
	{
		TutorialActions.EnterContext(TutorialActionContext.BeforeEnterPlay, null, null, null, null);
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Blocksworld.UI.Tapedeck.Ghost(false);
		Tutorial.target1.TargetButton(TILE_BUTTON.PLAY);
		Tutorial.hand1.TapTarget(Tutorial.target1, 0.25f);
	}

	// Token: 0x060021B1 RID: 8625 RVA: 0x000FCA28 File Offset: 0x000FAE28
	private static void HelpStop()
	{
		Blocksworld.enabledGAFs = new List<GAF>();
		Blocksworld.enabledPanelBlock = null;
		Blocksworld.UpdateTiles();
		Blocksworld.UI.Tapedeck.Ghost(false);
		Tutorial.target1.TargetButton(TILE_BUTTON.TOOLS);
		Tutorial.hand1.TapTarget(Tutorial.target1, 0.25f);
	}

	// Token: 0x060021B2 RID: 8626 RVA: 0x000FCA79 File Offset: 0x000FAE79
	private static bool CheckScale(Block thisBlock)
	{
		if (!Tutorial.Close(thisBlock.Scale(), Tutorial.goalBlock.Scale()))
		{
			Tutorial.HelpScale(thisBlock);
			return false;
		}
		return true;
	}

	// Token: 0x060021B3 RID: 8627 RVA: 0x000FCAA0 File Offset: 0x000FAEA0
	private static bool CheckRotation(Block thisBlock, bool fakeEnabled = true)
	{
		bool flag = thisBlock.HasGroup("locked-model");
		fakeEnabled = (fakeEnabled && !flag);
		if (Tutorial.SameRotation(thisBlock, thisBlock.go.transform.rotation, Tutorial.goalBlock, Tutorial.goalBlock.go.transform.rotation, fakeEnabled))
		{
			return true;
		}
		if (fakeEnabled && Tutorial.CanFakeSameRotation(thisBlock, thisBlock.go.transform.rotation, Tutorial.goalBlock, Tutorial.goalBlock.go.transform.rotation))
		{
			thisBlock.RotateTo(Tutorial.goalBlock.go.transform.rotation);
			return true;
		}
		Tutorial.HelpRotation(thisBlock, Tutorial.goalBlock.go.transform.rotation);
		return false;
	}

	// Token: 0x060021B4 RID: 8628 RVA: 0x000FCB71 File Offset: 0x000FAF71
	private static Vector3 CenterOfScreen()
	{
		return new Vector3(((float)Blocksworld.screenWidth - Blocksworld.buildPanel.width) * 0.5f, (float)Blocksworld.screenHeight * 0.5f, 0f);
	}

	// Token: 0x060021B5 RID: 8629 RVA: 0x000FCBA0 File Offset: 0x000FAFA0
	public static bool FilterOutMouseBlock()
	{
		return Tutorial.state == TutorialState.Color || Tutorial.state == TutorialState.Texture;
	}

	// Token: 0x060021B6 RID: 8630 RVA: 0x000FCBBA File Offset: 0x000FAFBA
	public static bool BlockCanBeMouseBlock(Block b)
	{
		return Tutorial.existingBlock is BlockWater || (!Tutorial.FilterOutMouseBlock() || !(b is BlockWater));
	}

	// Token: 0x060021B7 RID: 8631 RVA: 0x000FCBE7 File Offset: 0x000FAFE7
	public static HashSet<GAF> GetHiddenGafs()
	{
		if (Tutorial.state != TutorialState.None)
		{
			return Tutorial.hiddenGafs;
		}
		return new HashSet<GAF>();
	}

	// Token: 0x060021B8 RID: 8632 RVA: 0x000FCBFE File Offset: 0x000FAFFE
	public static HashSet<GAF> GetNeverHideGafs()
	{
		if (Tutorial.state != TutorialState.None)
		{
			return Tutorial.neverHideGafs;
		}
		return new HashSet<GAF>();
	}

	// Token: 0x060021B9 RID: 8633 RVA: 0x000FCC15 File Offset: 0x000FB015
	private static void UpdateTileRowHint(int row)
	{
		Tutorial.tileRowHint = null;
		if (row < Tutorial.tileRowHints.Count)
		{
			Tutorial.tileRowHint = Tutorial.tileRowHints[row];
		}
	}

	// Token: 0x060021BA RID: 8634 RVA: 0x000FCC40 File Offset: 0x000FB040
	public static void OnHudMesh()
	{
		if (Tutorial.state != TutorialState.None)
		{
			float y = (float)(Screen.height - 100);
			if (Tutorial.tileRowHint != null && Tutorial.tileRowHint.Length > 0)
			{
				HudMeshOnGUI.Label(ref Tutorial.hintLabel, new Rect(0f, y, (float)Screen.width, 100f), Tutorial.tileRowHint, null, 0f);
			}
		}
	}

	// Token: 0x060021BB RID: 8635 RVA: 0x000FCCA6 File Offset: 0x000FB0A6
	public static void AddOkParameterTile(Tile tile)
	{
		Tutorial.okParameterTiles.Add(tile);
	}

	// Token: 0x060021BC RID: 8636 RVA: 0x000FCCB4 File Offset: 0x000FB0B4
	public static bool SnapParameterValue(GAF thisGaf, GAF goalGaf)
	{
		EditableTileParameter editableParameter = thisGaf.Predicate.EditableParameter;
		if (editableParameter is TimeTileParameter)
		{
			TimeTileParameter timeTileParameter = (TimeTileParameter)editableParameter;
			int[] array = timeTileParameter.CalculateTimeComponents(goalGaf, null);
			int[] array2 = timeTileParameter.CalculateTimeComponents(thisGaf, null);
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

	// Token: 0x060021BD RID: 8637 RVA: 0x000FCE44 File Offset: 0x000FB244
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

	// Token: 0x060021BE RID: 8638 RVA: 0x000FCED4 File Offset: 0x000FB2D4
	public static void AutoRotateBlock(Block b)
	{
		if (Tutorial.goalBlock != null && Tutorial.goalBlock.go != null)
		{
			BlockMetaData blockMetaData = b.GetBlockMetaData();
			if (blockMetaData != null)
			{
				Vector3 autoAnglesTutorial = blockMetaData.autoAnglesTutorial;
				Vector3 eulerAngles = b.GetRotation().eulerAngles;
				Vector3 eulerAngles2 = Tutorial.goalBlock.GetRotation().eulerAngles;
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < 3; i++)
				{
					float num = Mathf.Max(autoAnglesTutorial[i], Tutorial.autoAnglesFilter[i]);
					zero[i] = eulerAngles[i] * (1f - num) + eulerAngles2[i] * num;
				}
				b.RotateTo(Quaternion.Euler(zero));
			}
		}
	}

	// Token: 0x060021BF RID: 8639 RVA: 0x000FCFB0 File Offset: 0x000FB3B0
	private static bool HelpOperationPose(TutorialState tutState, int meshIndex = -1)
	{
		if (!Tutorial.AutoCameraEnabled())
		{
			return false;
		}
		List<TutorialOperationPose> list;
		if (Tutorial.operationPoses.TryGetValue(Tutorial.goalBlock, out list))
		{
			TutorialOperationPose tutorialOperationPose = null;
			NamedPose namedPose = null;
			foreach (TutorialOperationPose tutorialOperationPose2 in list)
			{
				if (tutorialOperationPose2.state == tutState && (tutorialOperationPose2.meshIndex == meshIndex || meshIndex < 0))
				{
					NamedPose namedPose2;
					if (Blocksworld.cameraPosesMap.TryGetValue(tutorialOperationPose2.poseName, out namedPose2))
					{
						tutorialOperationPose = tutorialOperationPose2;
						namedPose = namedPose2;
						break;
					}
					BWLog.Warning("Could not find a pose named: '" + tutorialOperationPose2.poseName + "'");
				}
			}
			if (tutorialOperationPose != null && namedPose != null)
			{
				Tutorial.autoCameraEndPos = namedPose.position;
				Blocksworld.blocksworldCamera.SetTargetPosition(namedPose.position + namedPose.direction * 15f);
				if (Tutorial.Close(Tutorial.autoCameraEndPos, Blocksworld.cameraTransform.position))
				{
					return false;
				}
				if (Tutorial.state != TutorialState.Orbit)
				{
					Tutorial.stepOnNextUpdate = true;
				}
				Tutorial.HelpAutoCamera();
				return true;
			}
		}
		return false;
	}

	// Token: 0x060021C0 RID: 8640 RVA: 0x000FD0FC File Offset: 0x000FB4FC
	public static bool InTutorialOrPuzzle()
	{
		return Tutorial.state != TutorialState.None || Tutorial.mode == TutorialMode.Puzzle;
	}

	// Token: 0x060021C1 RID: 8641 RVA: 0x000FD113 File Offset: 0x000FB513
	public static bool InTutorial()
	{
		return Tutorial.state != TutorialState.None;
	}

	// Token: 0x060021C2 RID: 8642 RVA: 0x000FD120 File Offset: 0x000FB520
	public static string GetTargetSignalName()
	{
		return Tutorial.targetSignalName;
	}

	// Token: 0x060021C3 RID: 8643 RVA: 0x000FD127 File Offset: 0x000FB527
	public static Predicate GetTargetSignalPredicate()
	{
		return Tutorial.targetSignalPredicate;
	}

	// Token: 0x060021C4 RID: 8644 RVA: 0x000FD130 File Offset: 0x000FB530
	private static void DisableOccludingConnectedColliders(Block block)
	{
		if (Tutorial.raycastDisabledBlocks.Count > 0)
		{
			Tutorial.EnableOccludingConnectedColliders();
		}
		Vector3 position = block.go.transform.position;
		for (int i = 0; i < block.connections.Count; i++)
		{
			Block block2 = block.connections[i];
			bool flag = block2 is BlockPosition;
			if (!flag)
			{
				foreach (Collider collider in block2.go.GetComponentsInChildren<Collider>())
				{
					if (collider != null && collider.enabled && collider.bounds.Contains(position))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				Tutorial.raycastDisabledBlocks.Add(block2);
				block2.IgnoreRaycasts(true);
			}
		}
	}

	// Token: 0x060021C5 RID: 8645 RVA: 0x000FD214 File Offset: 0x000FB614
	private static void EnableOccludingConnectedColliders()
	{
		foreach (Block block in Tutorial.raycastDisabledBlocks)
		{
			block.IgnoreRaycasts(false);
		}
		Tutorial.raycastDisabledBlocks.Clear();
	}

	// Token: 0x04001C4D RID: 7245
	public static string text = null;

	// Token: 0x04001C4E RID: 7246
	public static string button = null;

	// Token: 0x04001C4F RID: 7247
	public static Rect rectText;

	// Token: 0x04001C50 RID: 7248
	public static Rect rectButton;

	// Token: 0x04001C51 RID: 7249
	public static TutorialMode mode = TutorialMode.StepByStep;

	// Token: 0x04001C52 RID: 7250
	private static TutorialState _state = TutorialState.None;

	// Token: 0x04001C53 RID: 7251
	public static TutorialMode modeBeforePlay = TutorialMode.StepByStep;

	// Token: 0x04001C54 RID: 7252
	public static TutorialState stateBeforePlay = TutorialState.None;

	// Token: 0x04001C55 RID: 7253
	public static TutorialSafeState safeState = null;

	// Token: 0x04001C56 RID: 7254
	private static string targetSignalName = "Signal";

	// Token: 0x04001C57 RID: 7255
	private static Predicate targetSignalPredicate;

	// Token: 0x04001C58 RID: 7256
	private static string action;

	// Token: 0x04001C59 RID: 7257
	public static int step = -1;

	// Token: 0x04001C5A RID: 7258
	public static int savedStep;

	// Token: 0x04001C5B RID: 7259
	public static int numGivenBlocks;

	// Token: 0x04001C5C RID: 7260
	public static bool progressBlocked = false;

	// Token: 0x04001C5D RID: 7261
	public static bool includeDefaultBlockTiles = false;

	// Token: 0x04001C5E RID: 7262
	public static List<List<Tile>> savedProgressTiles;

	// Token: 0x04001C5F RID: 7263
	public static List<Block> blocks = new List<Block>();

	// Token: 0x04001C60 RID: 7264
	public static Block goalBlock = null;

	// Token: 0x04001C61 RID: 7265
	public static Block oldGoalBlock = null;

	// Token: 0x04001C62 RID: 7266
	private static List<List<Tile>> goalBlockTilesCopy;

	// Token: 0x04001C63 RID: 7267
	private static Dictionary<Tile, Tile> originalToCopyTiles;

	// Token: 0x04001C64 RID: 7268
	private static Dictionary<Block, List<TutorialOperationPose>> operationPoses = new Dictionary<Block, List<TutorialOperationPose>>();

	// Token: 0x04001C65 RID: 7269
	private static Block existingBlock = null;

	// Token: 0x04001C66 RID: 7270
	private static List<string> tileRowHints = new List<string>();

	// Token: 0x04001C67 RID: 7271
	private static string tileRowHint;

	// Token: 0x04001C68 RID: 7272
	private static bool lastTileWasToggle;

	// Token: 0x04001C69 RID: 7273
	private static float waitTimeEnd;

	// Token: 0x04001C6A RID: 7274
	private static TutorialState stateAfterWait;

	// Token: 0x04001C6B RID: 7275
	private static HashSet<Tile> okParameterTiles = new HashSet<Tile>();

	// Token: 0x04001C6C RID: 7276
	private static bool autoAddBlock;

	// Token: 0x04001C6D RID: 7277
	private static float autoAddBlockWaitTime;

	// Token: 0x04001C6E RID: 7278
	private static Tile manualGoalHint;

	// Token: 0x04001C6F RID: 7279
	private static Tile moveBlockTile;

	// Token: 0x04001C70 RID: 7280
	private static Tile moveModelTile;

	// Token: 0x04001C71 RID: 7281
	private static Tile removeBlockTile;

	// Token: 0x04001C72 RID: 7282
	private static Dictionary<Block, Tile> groupedManualGoalHints = new Dictionary<Block, Tile>();

	// Token: 0x04001C73 RID: 7283
	public static List<Tile> manualPaintOrTexture = new List<Tile>();

	// Token: 0x04001C74 RID: 7284
	public static List<Tile> completedManualPaintOrTexture = new List<Tile>();

	// Token: 0x04001C75 RID: 7285
	public static List<Tile> manualRemoveBlock = new List<Tile>();

	// Token: 0x04001C76 RID: 7286
	public static GameObject placementHelper;

	// Token: 0x04001C77 RID: 7287
	public static Arrow arrow1;

	// Token: 0x04001C78 RID: 7288
	public static Arrow arrow2;

	// Token: 0x04001C79 RID: 7289
	public static Target target1;

	// Token: 0x04001C7A RID: 7290
	public static Target target2;

	// Token: 0x04001C7B RID: 7291
	public static Hand hand1;

	// Token: 0x04001C7C RID: 7292
	public static Hand hand2;

	// Token: 0x04001C7D RID: 7293
	public static bool usingQuickSelectColorIcon = false;

	// Token: 0x04001C7E RID: 7294
	public static bool usingQuickSelectTextureIcon = false;

	// Token: 0x04001C7F RID: 7295
	public static bool playModeHelpMover = false;

	// Token: 0x04001C80 RID: 7296
	private const float defaultAutoCameraDist = 15f;

	// Token: 0x04001C81 RID: 7297
	private static float autoCameraDist = 15f;

	// Token: 0x04001C82 RID: 7298
	private static Vector3 autoCameraAngle;

	// Token: 0x04001C83 RID: 7299
	public static float autoCameraDelay = 0.5f;

	// Token: 0x04001C84 RID: 7300
	private static float autoCameraTimer = 0f;

	// Token: 0x04001C85 RID: 7301
	public static float autoCameraTweenTimeMultiplier = 1f;

	// Token: 0x04001C86 RID: 7302
	private static List<Block> hiddenBlocks = new List<Block>();

	// Token: 0x04001C87 RID: 7303
	private const int SNAP_THRESHOLD = 12;

	// Token: 0x04001C88 RID: 7304
	private static HashSet<Block> raycastDisabledBlocks = new HashSet<Block>();

	// Token: 0x04001C89 RID: 7305
	public static Vector3 autoCameraEndPos;

	// Token: 0x04001C8A RID: 7306
	private static Vector3 autoCameraStartPos;

	// Token: 0x04001C8B RID: 7307
	private static Vector3 autoTargetStartPos;

	// Token: 0x04001C8C RID: 7308
	private static Tween autoCameraTween = null;

	// Token: 0x04001C8D RID: 7309
	private static bool rotateBeforeScale = false;

	// Token: 0x04001C8E RID: 7310
	private static bool paintBeforePose = false;

	// Token: 0x04001C8F RID: 7311
	private static bool textureBeforePose = false;

	// Token: 0x04001C90 RID: 7312
	private static Vector3 autoAnglesFilter = Vector3.zero;

	// Token: 0x04001C91 RID: 7313
	private static bool useTwoFingerScale = false;

	// Token: 0x04001C92 RID: 7314
	private static bool useTwoFingerMove = false;

	// Token: 0x04001C93 RID: 7315
	private static Vector3 gluePointBlock = Vector3.zero;

	// Token: 0x04001C94 RID: 7316
	private static Vector3 gluePointWorld = Vector3.zero;

	// Token: 0x04001C95 RID: 7317
	private static Vector3 glueNormal = Vector3.zero;

	// Token: 0x04001C96 RID: 7318
	private static Vector3 helpPositionTarget = Vector3.zero;

	// Token: 0x04001C97 RID: 7319
	public static int forceMeshIndex = 0;

	// Token: 0x04001C98 RID: 7320
	public static bool cheatNextStep = false;

	// Token: 0x04001C99 RID: 7321
	private static HashSet<GAF> hiddenGafs = new HashSet<GAF>();

	// Token: 0x04001C9A RID: 7322
	private static HashSet<GAF> neverHideGafs = new HashSet<GAF>();

	// Token: 0x04001C9B RID: 7323
	private static bool hideGraphics = false;

	// Token: 0x04001C9C RID: 7324
	public static bool stepOnNextUpdate = false;

	// Token: 0x04001C9D RID: 7325
	public static Tile scrollToTile;

	// Token: 0x04001C9E RID: 7326
	private static HudMeshLabel hintLabel;

	// Token: 0x04001C9F RID: 7327
	private static HashSet<string> neverHideInStepByStepBlocks;

	// Token: 0x04001CA0 RID: 7328
	private static HashSet<string> neverHideInStepByStepPaints;

	// Token: 0x04001CA1 RID: 7329
	private static HashSet<string> neverHideInStepByStepTextures;

	// Token: 0x04001CA2 RID: 7330
	private static HashSet<string> neverHideInStepByStepSFXs;

	// Token: 0x04001CA3 RID: 7331
	private static TutorialSettings tutorialSettings;

	// Token: 0x04001CA4 RID: 7332
	private static Dictionary<Predicate, int> predicateAccumIndices = null;

	// Token: 0x04001CA5 RID: 7333
	private static Vector3[] rots = new Vector3[]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	// Token: 0x04001CA6 RID: 7334
	private static Vector3[] best = new Vector3[]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	// Token: 0x04001CA7 RID: 7335
	private static int bestN = 0;

	// Token: 0x04001CA8 RID: 7336
	private static Vector3 dragYRotationAxis;

	// Token: 0x04001CA9 RID: 7337
	private static Vector3[] rots2D = new Vector3[]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	// Token: 0x04001CAA RID: 7338
	private static Vector3[] best2D = new Vector3[]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	// Token: 0x04001CAB RID: 7339
	private static int bestN2D = 0;

	// Token: 0x04001CAC RID: 7340
	private static string paths;
}
