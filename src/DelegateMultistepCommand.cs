using System;

public class DelegateMultistepCommand : Command
{
	private Action action;

	private int steps = 1;

	public DelegateMultistepCommand(Action action, int steps)
	{
		this.action = action;
		this.steps = steps;
	}

	public void SetSteps(int steps)
	{
		this.steps = steps;
		done = false;
	}

	public override void Execute()
	{
		action();
		steps--;
		done = steps <= 0;
	}
}
