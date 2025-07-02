public class UnlockDynamicLockPullCommand : Command
{
	public override void Execute()
	{
		base.Execute();
		Blocksworld.dynamicLockPull = false;
	}
}
