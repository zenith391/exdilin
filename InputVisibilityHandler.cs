using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020002F5 RID: 757
public class InputVisibilityHandler
{
	// Token: 0x0600223A RID: 8762 RVA: 0x000FF51D File Offset: 0x000FD91D
	public void Init()
	{
		this.allControls = UIInputControl.allButtonTypes;
		this.controlCount = this.allControls.Count;
		this.usedAsSensor = new bool[this.controlCount];
		this.visible = new bool[this.controlCount];
	}

	// Token: 0x0600223B RID: 8763 RVA: 0x000FF55D File Offset: 0x000FD95D
	public void AddBlock(Block b)
	{
		this.inputBlocks.Add(b);
	}

	// Token: 0x0600223C RID: 8764 RVA: 0x000FF56C File Offset: 0x000FD96C
	public void RemoveBlock(Block b)
	{
		this.inputBlocks.Remove(b);
	}

	// Token: 0x0600223D RID: 8765 RVA: 0x000FF57B File Offset: 0x000FD97B
	public void ControlUsedAsSensor(UIInputControl.ControlType control)
	{
		this.usedAsSensor[(int)control] = true;
		this.visible[(int)control] = true;
	}

	// Token: 0x0600223E RID: 8766 RVA: 0x000FF58F File Offset: 0x000FD98F
	public bool IsVisible(UIInputControl.ControlType control)
	{
		return this.visible[(int)control];
	}

	// Token: 0x0600223F RID: 8767 RVA: 0x000FF59C File Offset: 0x000FD99C
	public void Reset()
	{
		for (int i = 0; i < this.controlCount; i++)
		{
			this.usedAsSensor[i] = false;
			this.visible[i] = false;
		}
		this.inputBlocks.Clear();
	}

	// Token: 0x06002240 RID: 8768 RVA: 0x000FF5E0 File Offset: 0x000FD9E0
	public void FixedUpdate()
	{
		for (int i = 0; i < this.controlCount; i++)
		{
			bool flag = true;
			if ((!this.usedAsSensor[i] || Blocksworld.lockInput) && this.visible[i])
			{
				if (!Blocksworld.lockInput)
				{
					foreach (Block block in this.inputBlocks)
					{
						int count = block.tiles.Count;
						for (int j = 0; j < count; j++)
						{
							ScriptRowExecutionInfo scriptRowExecutionInfo = block.executionInfos[j];
							if (!scriptRowExecutionInfo.beforeThen || scriptRowExecutionInfo.justFinishedExecutingRow)
							{
								for (int k = 0; k < block.tiles[j].Count; k++)
								{
									Tile tile = block.tiles[j][k];
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
				}
				if (flag)
				{
					this.visible[i] = false;
				}
			}
			if (flag)
			{
				this.usedAsSensor[i] = false;
			}
		}
	}

	// Token: 0x04001D34 RID: 7476
	private HashSet<Block> inputBlocks = new HashSet<Block>();

	// Token: 0x04001D35 RID: 7477
	private bool[] usedAsSensor;

	// Token: 0x04001D36 RID: 7478
	private bool[] visible;

	// Token: 0x04001D37 RID: 7479
	private int controlCount;

	// Token: 0x04001D38 RID: 7480
	private List<UIInputControl.ControlType> allControls;
}
