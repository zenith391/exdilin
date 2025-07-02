using System.Collections.Generic;
using Gestures;

public class GestureCommand
{
	public bool done;

	public virtual void Execute(HashSet<Touch> touches)
	{
		done = true;
	}
}
