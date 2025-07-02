public class UnmuteAllBlocksCommand : DelayedCommand
{
	public UnmuteAllBlocksCommand()
		: base(2)
	{
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		Sound.UnmuteAllBlocks();
	}
}
