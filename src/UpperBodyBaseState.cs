public class UpperBodyBaseState
{
	public virtual void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
	}

	public virtual void Exit(UpperBodyStateHandler parent)
	{
	}

	public virtual bool Update(UpperBodyStateHandler parent)
	{
		return true;
	}
}
