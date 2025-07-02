using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class TapControlGesture : BaseGesture
{
	private static Vector3 worldTapPos;

	private static Vector3 possibleWorldTapPos;

	private static bool hasWorldTapPos;

	private static float worldTapTime;

	private static Vector3 startPos;

	private static Vector2 startTouchPos;

	public static bool HasWorldTapPos()
	{
		return hasWorldTapPos;
	}

	public static Vector3 GetWorldTapPos()
	{
		return worldTapPos;
	}

	public static float GetWorldTapTime()
	{
		return worldTapTime;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count > 1)
		{
			EnterState(GestureState.Failed);
		}
		else if (allTouches[0].Phase == TouchPhase.Began)
		{
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
			bool flag = false;
			if (Physics.Raycast(ray, out var hitInfo, 10000f, 539))
			{
				startTouchPos = allTouches[0].Position;
				possibleWorldTapPos = hitInfo.point;
				startPos = allTouches[0].Position;
				flag = true;
			}
			if (flag)
			{
				EnterState(GestureState.Active);
			}
			else
			{
				EnterState(GestureState.Failed);
			}
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Vector3 vector = allTouches[0].Position;
		if (base.gestureState == GestureState.Tracking && (vector - startPos).sqrMagnitude > 400f)
		{
			EnterState(GestureState.Cancelled);
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (allTouches[0].Phase == TouchPhase.Ended && !((allTouches[0].Position - startTouchPos).sqrMagnitude > 5f))
		{
			hasWorldTapPos = true;
			worldTapPos = possibleWorldTapPos;
			worldTapTime = Time.time;
			EnterState(GestureState.Ended);
		}
	}

	public override void Cancel()
	{
		EnterState(GestureState.Cancelled);
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return "TapControlGesture()";
	}
}
