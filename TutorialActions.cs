using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002DF RID: 735
public class TutorialActions
{
	// Token: 0x060021D0 RID: 8656 RVA: 0x000FD5CC File Offset: 0x000FB9CC
	public static bool StepActions()
	{
		for (int i = 0; i < TutorialActions.executingTutorialActions.Count; i++)
		{
			TutorialAction tutorialAction = TutorialActions.executingTutorialActions[i];
			if (tutorialAction.Step())
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060021D1 RID: 8657 RVA: 0x000FD610 File Offset: 0x000FBA10
	public static void ExecuteAndUpdateActions()
	{
		for (int i = 0; i < TutorialActions.executingTutorialActions.Count; i++)
		{
			TutorialAction tutorialAction = TutorialActions.executingTutorialActions[i];
			tutorialAction.Execute();
			if (tutorialAction.stopProgressUntilDone)
			{
				break;
			}
		}
		for (int j = TutorialActions.executingTutorialActions.Count - 1; j >= 0; j--)
		{
			if (TutorialActions.executingTutorialActions[j].done)
			{
				TutorialActions.executingTutorialActions.RemoveAt(j);
			}
		}
	}

	// Token: 0x060021D2 RID: 8658 RVA: 0x000FD698 File Offset: 0x000FBA98
	public static bool AnyActionBlocksProgress()
	{
		foreach (TutorialAction tutorialAction in TutorialActions.executingTutorialActions)
		{
			if (tutorialAction.stopProgressUntilDone && tutorialAction.executing && !tutorialAction.done)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060021D3 RID: 8659 RVA: 0x000FD718 File Offset: 0x000FBB18
	public static void StopFirstBlockingAction()
	{
		TutorialAction tutorialAction = null;
		foreach (TutorialAction tutorialAction2 in TutorialActions.executingTutorialActions)
		{
			if (tutorialAction2.stopProgressUntilDone && tutorialAction2.executing && !tutorialAction2.done)
			{
				tutorialAction = tutorialAction2;
			}
		}
		if (tutorialAction != null)
		{
			tutorialAction.LeaveContext();
			Tutorial.state = TutorialState.DetermineInstructions;
			Tutorial.stepOnNextUpdate = true;
		}
	}

	// Token: 0x060021D4 RID: 8660 RVA: 0x000FD7AC File Offset: 0x000FBBAC
	public static void Clear()
	{
		TutorialActions.tutorialActions.Clear();
		foreach (TutorialAction tutorialAction in TutorialActions.executingTutorialActions)
		{
			tutorialAction.LeaveContext();
		}
		TutorialActions.executingTutorialActions.Clear();
		TutorialActions.activeContexts.Clear();
		Tutorial.playModeHelpMover = false;
	}

	// Token: 0x060021D5 RID: 8661 RVA: 0x000FD82C File Offset: 0x000FBC2C
	private static bool TileRemovedAtTutorialStart(Tile t)
	{
		Predicate predicate = t.gaf.Predicate;
		return predicate == Block.predicateTutorialCreateBlockHint || predicate == Block.predicateTutorialRemoveBlockHint || predicate == Block.predicateUnlocked || predicate == Block.predicateTutorialPaintExistingBlock || predicate == Block.predicateTutorialTextureExistingBlock || predicate == Block.predicateTutorialRotateExistingBlock || predicate == Block.predicateTutorialHelpTextAction || predicate == Block.predicateTutorialOperationPose;
	}

	// Token: 0x060021D6 RID: 8662 RVA: 0x000FD8A0 File Offset: 0x000FBCA0
	private static Tile GetTileAfter(int c, List<Tile> row)
	{
		for (int i = c + 1; i < row.Count; i++)
		{
			Tile tile = row[i];
			Predicate predicate = tile.gaf.Predicate;
			if (predicate == Block.predicateHideNextTile)
			{
				i++;
			}
			else if (predicate == Block.predicateHideTileRow)
			{
				BWLog.Error("Next tile is undefined on rows that contains a 'hide tile row' tile");
			}
			else if (!TutorialActions.TileRemovedAtTutorialStart(tile))
			{
				return tile;
			}
		}
		return null;
	}

	// Token: 0x060021D7 RID: 8663 RVA: 0x000FD918 File Offset: 0x000FBD18
	private static Tile GetTileBefore(int c, List<Tile> row)
	{
		int i = c - 1;
		while (i >= 0)
		{
			Tile tile = row[i];
			Tile tile2 = (i <= 0) ? null : row[i - 1];
			if (tile2 == null)
			{
				goto IL_4E;
			}
			Predicate predicate = tile2.gaf.Predicate;
			if (predicate != Block.predicateHideNextTile)
			{
				goto IL_4E;
			}
			i--;
			IL_83:
			i--;
			continue;
			IL_4E:
			Predicate predicate2 = tile.gaf.Predicate;
			if (predicate2 == Block.predicateHideTileRow)
			{
				BWLog.Error("Previous tile is undefined on rows that contains a 'hide tile row' tile");
				goto IL_83;
			}
			if (!TutorialActions.TileRemovedAtTutorialStart(tile))
			{
				return tile;
			}
			goto IL_83;
		}
		return null;
	}

	// Token: 0x060021D8 RID: 8664 RVA: 0x000FD9B4 File Offset: 0x000FBDB4
	public static void ParseActions(List<Block> blocks)
	{
		TutorialActions.Clear();
		foreach (Block block in blocks)
		{
			for (int i = 0; i < block.tiles.Count; i++)
			{
				List<Tile> list = block.tiles[i];
				for (int j = 0; j < list.Count; j++)
				{
					Tile tile = list[j];
					GAF gaf = tile.gaf;
					object[] args = gaf.Args;
					Predicate predicate = gaf.Predicate;
					TutorialAction tutorialAction = null;
					if (predicate == Block.predicateTutorialHelpTextAction)
					{
						tutorialAction = new HelpTextAction
						{
							text = Util.GetStringArg(args, 1, "Help text"),
							position = Util.GetVector3Arg(args, 2, new Vector3(100f, 200f, 0f)),
							width = Util.GetFloatArg(args, 3, 400f),
							poseName = Util.GetStringArg(args, 4, string.Empty),
							buttons = Util.GetStringArg(args, 5, "Ok"),
							sfx = Util.GetStringArg(args, 6, string.Empty),
							highlights = Util.GetStringArg(args, 7, string.Empty),
							tiles = Util.GetStringArg(args, 8, string.Empty),
							lifetime = Util.GetFloatArg(args, 9, float.MaxValue)
						};
					}
					if (tutorialAction != null)
					{
						TutorialActionContext tutorialActionContext = (TutorialActionContext)Util.GetEnumArg(args, 0, "TutorialStart", typeof(TutorialActionContext));
						tutorialAction.context = tutorialActionContext;
						tutorialAction.block = block;
						tutorialAction.tileRow = list;
						tutorialAction.tileAfter = TutorialActions.GetTileAfter(j, list);
						tutorialAction.tileBefore = TutorialActions.GetTileBefore(j, list);
						Tutorial.playModeHelpMover |= (tutorialActionContext == TutorialActionContext.BeforePlayMoverUse);
						List<TutorialAction> list2;
						if (!TutorialActions.tutorialActions.ContainsKey(tutorialActionContext))
						{
							list2 = new List<TutorialAction>();
							TutorialActions.tutorialActions[tutorialActionContext] = list2;
						}
						else
						{
							list2 = TutorialActions.tutorialActions[tutorialActionContext];
						}
						list2.Add(tutorialAction);
					}
				}
			}
		}
	}

	// Token: 0x060021D9 RID: 8665 RVA: 0x000FDC0C File Offset: 0x000FC00C
	private static bool BoolLog(string str, bool result = true)
	{
		BWLog.Info(str);
		return result;
	}

	// Token: 0x060021DA RID: 8666 RVA: 0x000FDC18 File Offset: 0x000FC018
	public static bool EnterContext(TutorialActionContext context, Block block = null, List<Tile> tileRow = null, Tile tileBefore = null, Tile tileAfter = null)
	{
		bool result = false;
		TutorialActions.activeContexts.Clear();
		TutorialActions.activeContexts.Add(context);
		foreach (TutorialAction tutorialAction in TutorialActions.executingTutorialActions)
		{
			bool flag = false;
			flag = (flag || (TutorialActions.blockDependentContexts.Contains(tutorialAction.context) && TutorialActions.blockDependentContexts.Contains(context) && tutorialAction.block != null && tutorialAction.block != block));
			flag = (flag || (TutorialActions.scriptRowDependentContexts.Contains(tutorialAction.context) && TutorialActions.scriptRowDependentContexts.Contains(context) && tutorialAction.tileRow != tileRow));
			flag = (flag || (TutorialActions.scriptTileDependentContexts.Contains(tutorialAction.context) && TutorialActions.scriptTileDependentContexts.Contains(context) && (tutorialAction.tileBefore != tileBefore || tutorialAction.tileAfter != tileAfter)));
			flag = (flag || (tutorialAction.context != context && TutorialActions.contextDependentContexts.Contains(tutorialAction.context)));
			flag = (flag || (context == TutorialActionContext.BeforeThisBlockCreate && tutorialAction.context != TutorialActionContext.BeforeThisBlockCreate && TutorialActions.blockDependentContexts.Contains(tutorialAction.context) && tutorialAction.block == block));
			if (!flag)
			{
				if (context == TutorialActionContext.AfterThisBlockCreate)
				{
					if (tutorialAction.context == TutorialActionContext.BeforeThisBlockCreate && tutorialAction.block == block)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				tutorialAction.LeaveContext();
			}
			else
			{
				TutorialActions.activeContexts.Add(tutorialAction.context);
			}
		}
		List<TutorialAction> list;
		if (context == TutorialActionContext.BeforeThisBlockCreate)
		{
			foreach (TutorialActionContext key in TutorialActions.blockDependentContexts)
			{
				if (TutorialActions.tutorialActions.TryGetValue(key, out list))
				{
					foreach (TutorialAction tutorialAction2 in list)
					{
						if (!tutorialAction2.executing && tutorialAction2.done && tutorialAction2.block == block && tutorialAction2.context != TutorialActionContext.BeforeThisBlockCreate && !TutorialActions.executingTutorialActions.Contains(tutorialAction2))
						{
							tutorialAction2.done = false;
						}
					}
				}
			}
		}
		if (TutorialActions.tutorialActions.TryGetValue(context, out list))
		{
			foreach (TutorialAction tutorialAction3 in list)
			{
				if (!tutorialAction3.executing)
				{
					if (!tutorialAction3.done)
					{
						if (!TutorialActions.blockDependentContexts.Contains(context) || tutorialAction3.block == block)
						{
							if (!TutorialActions.scriptRowDependentContexts.Contains(context) || tutorialAction3.tileRow == tileRow)
							{
								if (TutorialActions.scriptTileDependentContexts.Contains(context))
								{
									if (tileBefore != null && tutorialAction3.tileBefore != tileBefore)
									{
										continue;
									}
									if (tileAfter != null && tutorialAction3.tileAfter != tileAfter)
									{
										continue;
									}
								}
								if (!TutorialActions.executingTutorialActions.Contains(tutorialAction3))
								{
									result = true;
									tutorialAction3.EnterContext();
									TutorialActions.activeContexts.Clear();
									TutorialActions.activeContexts.Add(context);
									foreach (TutorialAction tutorialAction4 in TutorialActions.executingTutorialActions)
									{
										if (tutorialAction3.CancelsAction(tutorialAction4) && !tutorialAction4.stopProgressUntilDone)
										{
											tutorialAction4.LeaveContext();
										}
										else
										{
											TutorialActions.activeContexts.Add(tutorialAction4.context);
										}
									}
									TutorialActions.executingTutorialActions.Add(tutorialAction3);
								}
							}
						}
					}
				}
			}
		}
		return result;
	}

	// Token: 0x04001CC5 RID: 7365
	private static HashSet<TutorialActionContext> blockDependentContexts = new HashSet<TutorialActionContext>
	{
		TutorialActionContext.AfterThisBlockCreate,
		TutorialActionContext.BeforeThisBlockCreate,
		TutorialActionContext.DuringScriptNextTile,
		TutorialActionContext.DuringScriptThisRow,
		TutorialActionContext.BlockDone,
		TutorialActionContext.Texture,
		TutorialActionContext.Paint,
		TutorialActionContext.BeginScripting
	};

	// Token: 0x04001CC6 RID: 7366
	private static HashSet<TutorialActionContext> scriptRowDependentContexts = new HashSet<TutorialActionContext>
	{
		TutorialActionContext.DuringScriptThisRow
	};

	// Token: 0x04001CC7 RID: 7367
	private static HashSet<TutorialActionContext> scriptTileDependentContexts = new HashSet<TutorialActionContext>
	{
		TutorialActionContext.DuringScriptNextTile
	};

	// Token: 0x04001CC8 RID: 7368
	private static HashSet<TutorialActionContext> contextDependentContexts = new HashSet<TutorialActionContext>
	{
		TutorialActionContext.AutoAddBlockWait,
		TutorialActionContext.RemoveBlock,
		TutorialActionContext.Paint,
		TutorialActionContext.Texture,
		TutorialActionContext.EnterPlay,
		TutorialActionContext.BeginScripting
	};

	// Token: 0x04001CC9 RID: 7369
	public static Dictionary<TutorialActionContext, List<TutorialAction>> tutorialActions = new Dictionary<TutorialActionContext, List<TutorialAction>>();

	// Token: 0x04001CCA RID: 7370
	public static List<TutorialAction> executingTutorialActions = new List<TutorialAction>();

	// Token: 0x04001CCB RID: 7371
	public static HashSet<TutorialActionContext> activeContexts = new HashSet<TutorialActionContext>();
}
