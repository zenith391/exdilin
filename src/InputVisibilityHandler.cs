using System.Collections.Generic;
using Blocks;

public class InputVisibilityHandler
{
	private HashSet<Block> inputBlocks = new HashSet<Block>();

	private bool[] usedAsSensor;

	private bool[] visible;

	private int controlCount;

	private List<UIInputControl.ControlType> allControls;

	public void Init()
	{
		allControls = UIInputControl.allButtonTypes;
		controlCount = allControls.Count;
		usedAsSensor = new bool[controlCount];
		visible = new bool[controlCount];
	}

	public void AddBlock(Block b)
	{
		inputBlocks.Add(b);
	}

	public void RemoveBlock(Block b)
	{
		inputBlocks.Remove(b);
	}

	public void ControlUsedAsSensor(UIInputControl.ControlType control)
	{
		usedAsSensor[(int)control] = true;
		visible[(int)control] = true;
	}

	public bool IsVisible(UIInputControl.ControlType control)
	{
		return visible[(int)control];
	}

	public void Reset()
	{
		for (int i = 0; i < controlCount; i++)
		{
			usedAsSensor[i] = false;
			visible[i] = false;
		}
		inputBlocks.Clear();
	}

	public void FixedUpdate()
	{
		for (int i = 0; i < controlCount; i++)
		{
			bool flag = true;
			if ((!usedAsSensor[i] || Blocksworld.lockInput) && visible[i])
			{
				if (!Blocksworld.lockInput)
				{
					foreach (Block inputBlock in inputBlocks)
					{
						int count = inputBlock.tiles.Count;
						for (int j = 0; j < count; j++)
						{
							ScriptRowExecutionInfo scriptRowExecutionInfo = inputBlock.executionInfos[j];
							if (scriptRowExecutionInfo.beforeThen && !scriptRowExecutionInfo.justFinishedExecutingRow)
							{
								continue;
							}
							for (int k = 0; k < inputBlock.tiles[j].Count; k++)
							{
								Tile tile = inputBlock.tiles[j][k];
								if (tile.gaf.Predicate == Block.predicateButton)
								{
									string key = (string)tile.gaf.Args[0];
									if (UIInputControl.controlTypeFromString[key] == (UIInputControl.ControlType)i)
									{
										flag = false;
									}
								}
							}
						}
					}
				}
				if (flag)
				{
					visible[i] = false;
				}
			}
			if (flag)
			{
				usedAsSensor[i] = false;
			}
		}
	}
}
