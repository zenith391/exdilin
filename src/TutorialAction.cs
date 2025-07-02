using System.Collections.Generic;
using Blocks;

public class TutorialAction
{
	public TutorialActionContext context;

	public bool stopProgressUntilDone;

	public bool done;

	public bool executing;

	public Block block;

	public List<Tile> tileRow;

	public Tile tileBefore;

	public Tile tileAfter;

	public virtual bool Step()
	{
		return false;
	}

	public virtual void EnterContext()
	{
		executing = true;
	}

	public virtual void LeaveContext()
	{
		executing = false;
		done = true;
	}

	public virtual void Execute()
	{
	}

	public override string ToString()
	{
		return string.Format("Tutorial action ctx: " + context);
	}

	public virtual bool CancelsAction(TutorialAction otherAction)
	{
		return false;
	}
}
