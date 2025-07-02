using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class TutorialActions
{
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

	private static HashSet<TutorialActionContext> scriptRowDependentContexts = new HashSet<TutorialActionContext> { TutorialActionContext.DuringScriptThisRow };

	private static HashSet<TutorialActionContext> scriptTileDependentContexts = new HashSet<TutorialActionContext> { TutorialActionContext.DuringScriptNextTile };

	private static HashSet<TutorialActionContext> contextDependentContexts = new HashSet<TutorialActionContext>
	{
		TutorialActionContext.AutoAddBlockWait,
		TutorialActionContext.RemoveBlock,
		TutorialActionContext.Paint,
		TutorialActionContext.Texture,
		TutorialActionContext.EnterPlay,
		TutorialActionContext.BeginScripting
	};

	public static Dictionary<TutorialActionContext, List<TutorialAction>> tutorialActions = new Dictionary<TutorialActionContext, List<TutorialAction>>();

	public static List<TutorialAction> executingTutorialActions = new List<TutorialAction>();

	public static HashSet<TutorialActionContext> activeContexts = new HashSet<TutorialActionContext>();

	public static bool StepActions()
	{
		for (int i = 0; i < executingTutorialActions.Count; i++)
		{
			TutorialAction tutorialAction = executingTutorialActions[i];
			if (tutorialAction.Step())
			{
				return true;
			}
		}
		return false;
	}

	public static void ExecuteAndUpdateActions()
	{
		for (int i = 0; i < executingTutorialActions.Count; i++)
		{
			TutorialAction tutorialAction = executingTutorialActions[i];
			tutorialAction.Execute();
			if (tutorialAction.stopProgressUntilDone)
			{
				break;
			}
		}
		for (int num = executingTutorialActions.Count - 1; num >= 0; num--)
		{
			if (executingTutorialActions[num].done)
			{
				executingTutorialActions.RemoveAt(num);
			}
		}
	}

	public static bool AnyActionBlocksProgress()
	{
		foreach (TutorialAction executingTutorialAction in executingTutorialActions)
		{
			if (executingTutorialAction.stopProgressUntilDone && executingTutorialAction.executing && !executingTutorialAction.done)
			{
				return true;
			}
		}
		return false;
	}

	public static void StopFirstBlockingAction()
	{
		TutorialAction tutorialAction = null;
		foreach (TutorialAction executingTutorialAction in executingTutorialActions)
		{
			if (executingTutorialAction.stopProgressUntilDone && executingTutorialAction.executing && !executingTutorialAction.done)
			{
				tutorialAction = executingTutorialAction;
			}
		}
		if (tutorialAction != null)
		{
			tutorialAction.LeaveContext();
			Tutorial.state = TutorialState.DetermineInstructions;
			Tutorial.stepOnNextUpdate = true;
		}
	}

	public static void Clear()
	{
		tutorialActions.Clear();
		foreach (TutorialAction executingTutorialAction in executingTutorialActions)
		{
			executingTutorialAction.LeaveContext();
		}
		executingTutorialActions.Clear();
		activeContexts.Clear();
		Tutorial.playModeHelpMover = false;
	}

	private static bool TileRemovedAtTutorialStart(Tile t)
	{
		Predicate predicate = t.gaf.Predicate;
		if (predicate != Block.predicateTutorialCreateBlockHint && predicate != Block.predicateTutorialRemoveBlockHint && predicate != Block.predicateUnlocked && predicate != Block.predicateTutorialPaintExistingBlock && predicate != Block.predicateTutorialTextureExistingBlock && predicate != Block.predicateTutorialRotateExistingBlock && predicate != Block.predicateTutorialHelpTextAction)
		{
			return predicate == Block.predicateTutorialOperationPose;
		}
		return true;
	}

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
			else if (!TileRemovedAtTutorialStart(tile))
			{
				return tile;
			}
		}
		return null;
	}

	private static Tile GetTileBefore(int c, List<Tile> row)
	{
		for (int num = c - 1; num >= 0; num--)
		{
			Tile tile = row[num];
			Tile tile2 = ((num <= 0) ? null : row[num - 1]);
			if (tile2 != null)
			{
				Predicate predicate = tile2.gaf.Predicate;
				if (predicate == Block.predicateHideNextTile)
				{
					num--;
					continue;
				}
			}
			Predicate predicate2 = tile.gaf.Predicate;
			if (predicate2 == Block.predicateHideTileRow)
			{
				BWLog.Error("Previous tile is undefined on rows that contains a 'hide tile row' tile");
			}
			else if (!TileRemovedAtTutorialStart(tile))
			{
				return tile;
			}
		}
		return null;
	}

	public static void ParseActions(List<Block> blocks)
	{
		Clear();
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
						TutorialActionContext tutorialActionContext = (tutorialAction.context = (TutorialActionContext)Util.GetEnumArg(args, 0, "TutorialStart", typeof(TutorialActionContext)));
						tutorialAction.block = block;
						tutorialAction.tileRow = list;
						tutorialAction.tileAfter = GetTileAfter(j, list);
						tutorialAction.tileBefore = GetTileBefore(j, list);
						Tutorial.playModeHelpMover |= tutorialActionContext == TutorialActionContext.BeforePlayMoverUse;
						List<TutorialAction> list2;
						if (!tutorialActions.ContainsKey(tutorialActionContext))
						{
							list2 = new List<TutorialAction>();
							tutorialActions[tutorialActionContext] = list2;
						}
						else
						{
							list2 = tutorialActions[tutorialActionContext];
						}
						list2.Add(tutorialAction);
					}
				}
			}
		}
	}

	private static bool BoolLog(string str, bool result = true)
	{
		BWLog.Info(str);
		return result;
	}

	public static bool EnterContext(TutorialActionContext context, Block block = null, List<Tile> tileRow = null, Tile tileBefore = null, Tile tileAfter = null)
	{
		bool result = false;
		activeContexts.Clear();
		activeContexts.Add(context);
		foreach (TutorialAction executingTutorialAction in executingTutorialActions)
		{
			bool flag = false || (blockDependentContexts.Contains(executingTutorialAction.context) && blockDependentContexts.Contains(context) && executingTutorialAction.block != null && executingTutorialAction.block != block) || (scriptRowDependentContexts.Contains(executingTutorialAction.context) && scriptRowDependentContexts.Contains(context) && executingTutorialAction.tileRow != tileRow) || (scriptTileDependentContexts.Contains(executingTutorialAction.context) && scriptTileDependentContexts.Contains(context) && (executingTutorialAction.tileBefore != tileBefore || executingTutorialAction.tileAfter != tileAfter)) || (executingTutorialAction.context != context && contextDependentContexts.Contains(executingTutorialAction.context)) || (context == TutorialActionContext.BeforeThisBlockCreate && executingTutorialAction.context != TutorialActionContext.BeforeThisBlockCreate && blockDependentContexts.Contains(executingTutorialAction.context) && executingTutorialAction.block == block);
			if (!flag && context == TutorialActionContext.AfterThisBlockCreate && executingTutorialAction.context == TutorialActionContext.BeforeThisBlockCreate && executingTutorialAction.block == block)
			{
				flag = true;
			}
			if (flag)
			{
				executingTutorialAction.LeaveContext();
			}
			else
			{
				activeContexts.Add(executingTutorialAction.context);
			}
		}
		List<TutorialAction> value;
		if (context == TutorialActionContext.BeforeThisBlockCreate)
		{
			foreach (TutorialActionContext blockDependentContext in blockDependentContexts)
			{
				if (!tutorialActions.TryGetValue(blockDependentContext, out value))
				{
					continue;
				}
				foreach (TutorialAction item in value)
				{
					if (!item.executing && item.done && item.block == block && item.context != TutorialActionContext.BeforeThisBlockCreate && !executingTutorialActions.Contains(item))
					{
						item.done = false;
					}
				}
			}
		}
		if (tutorialActions.TryGetValue(context, out value))
		{
			foreach (TutorialAction item2 in value)
			{
				if (item2.executing || item2.done || (blockDependentContexts.Contains(context) && item2.block != block) || (scriptRowDependentContexts.Contains(context) && item2.tileRow != tileRow) || (scriptTileDependentContexts.Contains(context) && ((tileBefore != null && item2.tileBefore != tileBefore) || (tileAfter != null && item2.tileAfter != tileAfter))) || executingTutorialActions.Contains(item2))
				{
					continue;
				}
				result = true;
				item2.EnterContext();
				activeContexts.Clear();
				activeContexts.Add(context);
				foreach (TutorialAction executingTutorialAction2 in executingTutorialActions)
				{
					if (item2.CancelsAction(executingTutorialAction2) && !executingTutorialAction2.stopProgressUntilDone)
					{
						executingTutorialAction2.LeaveContext();
					}
					else
					{
						activeContexts.Add(executingTutorialAction2.context);
					}
				}
				executingTutorialActions.Add(item2);
			}
		}
		return result;
	}
}
