using Blocks;

public class VanishModelForeverCommand : Command
{
	private Block block;

	private bool animate;

	private float delayPerBlock;

	private float timer;

	public VanishModelForeverCommand(Block block, bool animate, float delayPerBlock)
	{
		this.block = block;
		this.animate = animate;
		this.delayPerBlock = delayPerBlock;
	}

	public override void Execute()
	{
		bool flag = true;
		if (block.VanishModel(timer, animate, forever: true, delayPerBlock) == TileResultCode.True)
		{
			block.UpdateConnectedCache();
			bool flag2 = true;
			foreach (Block item in Block.connectedCache[block])
			{
				if (!BWSceneManager.playBlocksRemoved.Contains(item))
				{
					flag2 = false;
					flag = false;
					timer = 0f;
					break;
				}
			}
			done = flag2;
		}
		if (flag)
		{
			timer += Blocksworld.fixedDeltaTime;
		}
	}
}
