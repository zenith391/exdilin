using System;
using Blocks;

// Token: 0x0200014B RID: 331
public class VanishModelForeverCommand : Command
{
	// Token: 0x06001485 RID: 5253 RVA: 0x00090476 File Offset: 0x0008E876
	public VanishModelForeverCommand(Block block, bool animate, float delayPerBlock)
	{
		this.block = block;
		this.animate = animate;
		this.delayPerBlock = delayPerBlock;
	}

	// Token: 0x06001486 RID: 5254 RVA: 0x00090494 File Offset: 0x0008E894
	public override void Execute()
	{
		bool flag = true;
		if (this.block.VanishModel(this.timer, this.animate, true, this.delayPerBlock) == TileResultCode.True)
		{
			this.block.UpdateConnectedCache();
			bool done = true;
			foreach (Block item in Block.connectedCache[this.block])
			{
				if (!BWSceneManager.playBlocksRemoved.Contains(item))
				{
					done = false;
					flag = false;
					this.timer = 0f;
					break;
				}
			}
			this.done = done;
		}
		if (flag)
		{
			this.timer += Blocksworld.fixedDeltaTime;
		}
	}

	// Token: 0x0400103B RID: 4155
	private Block block;

	// Token: 0x0400103C RID: 4156
	private bool animate;

	// Token: 0x0400103D RID: 4157
	private float delayPerBlock;

	// Token: 0x0400103E RID: 4158
	private float timer;
}
