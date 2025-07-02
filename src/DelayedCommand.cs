public class DelayedCommand : Command
{
	protected int counter;

	protected int delay;

	public DelayedCommand(int delay)
	{
		this.delay = delay;
	}

	public override void Reset()
	{
		base.Reset();
		counter = 0;
	}

	public override void Execute()
	{
		counter++;
		if (counter > delay)
		{
			DelayedExecute();
		}
	}

	protected virtual void DelayedExecute()
	{
		done = true;
	}
}
