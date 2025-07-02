using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class UIGesture : BaseGesture
{
	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count != 1 || Blocksworld.bw == null || Blocksworld.UI == null)
		{
			EnterState(GestureState.Failed);
			return;
		}
		Vector2 position = allTouches[0].Position;
		if (Blocksworld.UI.IsBlocking(position))
		{
			EnterState(GestureState.Active);
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		EnterState(GestureState.Ended);
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
	}
}
