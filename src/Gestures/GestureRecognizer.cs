using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class GestureRecognizer
{
	private readonly List<Touch> _touches;

	private readonly List<BaseGesture> _gestures;

	private readonly Dictionary<BaseGesture, List<BaseGesture>> _cancellers;

	private readonly HashSet<BaseGesture> _preventStart;

	private bool prevMouseDown;

	private bool prevSimulateZoom;

	private bool prevSimulateTwoFingerSwipe;

	private bool _hasResetAll;

	public List<GestureCommand> gestureCommands = new List<GestureCommand>();

	private HashSet<Touch> currentPointsLeft = new HashSet<Touch>();

	private List<Touch> touchesLeft = new List<Touch>();

	private static HashSet<Touch> currentTouchPointsTouches = new HashSet<Touch>();

	private List<GestureCommand> tempGestureCommands = new List<GestureCommand>();

	public GestureRecognizer()
	{
		_touches = new List<Touch>();
		_gestures = new List<BaseGesture>();
		_cancellers = new Dictionary<BaseGesture, List<BaseGesture>>();
		_preventStart = new HashSet<BaseGesture>();
	}

	public void AddGesture(BaseGesture gesture)
	{
		_gestures.Add(gesture);
	}

	public void RemoveGesture(BaseGesture gesture)
	{
		_gestures.Remove(gesture);
	}

	public void Cancels(BaseGesture parent, BaseGesture child)
	{
		if (!_cancellers.ContainsKey(parent))
		{
			_cancellers[parent] = new List<BaseGesture>();
		}
		_cancellers[parent].Add(child);
	}

	public void CancelsAll(BaseGesture parent, IEnumerable<BaseGesture> children)
	{
		foreach (BaseGesture child in children)
		{
			Cancels(parent, child);
		}
	}

	public void AnyCancels(IEnumerable<BaseGesture> parents, BaseGesture child)
	{
		foreach (BaseGesture parent in parents)
		{
			Cancels(parent, child);
		}
	}

	public void AnyCancelsAll(IEnumerable<BaseGesture> parents, IEnumerable<BaseGesture> children)
	{
		foreach (BaseGesture child in children)
		{
			AnyCancels(parents, child);
		}
	}

	public void OnDebugGUI()
	{
		Color color = GUI.color;
		GUI.color = new Color(0f, 0f, 0f, 1f);
		int num = 40;
		int num2 = 10;
		foreach (Touch touch in _touches)
		{
			GUI.Label(new Rect(num, num2, 400f, 22f), $"({touch.Position.x}, {touch.Position.y}) {touch.Phase}");
			float num3 = NormalizedScreen.scale * touch.Position.x;
			float num4 = NormalizedScreen.scale * touch.Position.y;
			GUI.color = color;
			GUI.DrawTexture(new Rect(num3 - 32f, (float)Screen.height - num4 - 32f, 64f, 64f), (Texture)Resources.Load("GUI/Button Red", typeof(Texture)));
			GUI.color = new Color(0f, 0f, 0f, 1f);
			num2 += 20;
		}
		num2 = 40;
		foreach (object value in Enum.GetValues(typeof(GestureState)))
		{
			GestureState gestureState = (GestureState)value;
			GUI.color = new Color(0f, 0f, 0f, 1f);
			GUI.Label(new Rect(num, num2, 200f, 22f), gestureState.ToString());
			int num5 = num2 + 20;
			foreach (BaseGesture gesture in _gestures)
			{
				if (gesture.gestureState == gestureState)
				{
					GUI.color = ((!gesture.IsEnabled) ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0f, 0f, 0f, 1f));
					GUI.Label(new Rect(num, num5, 800f, 22f), string.Format("{0} {1}", gesture, (!_preventStart.Contains(gesture)) ? string.Empty : "(prevented)"));
					num5 += 20;
				}
			}
			num += 200;
			if (num >= 900)
			{
				num2 += 400;
				num = 320;
			}
		}
		GUI.color = color;
	}

	public void Update()
	{
		MatchTouchesToInput();
		RunGestures();
	}

	private void MatchTouchesToInput()
	{
		RemoveTouchesWithPhase(_touches, TouchPhase.Ended);
		HashSet<Touch> hashSet = CurrentTouchPoints();
		currentPointsLeft.Clear();
		foreach (Touch item in hashSet)
		{
			currentPointsLeft.Add(item);
		}
		touchesLeft.Clear();
		touchesLeft.AddRange(_touches);
		foreach (Touch item2 in hashSet)
		{
			if (item2.Phase != TouchPhase.Moved && item2.Phase != TouchPhase.Stationary && item2.Phase != TouchPhase.Ended)
			{
				continue;
			}
			Touch touch = null;
			float num = 100000000f;
			for (int i = 0; i < touchesLeft.Count; i++)
			{
				Touch touch2 = touchesLeft[i];
				float sqrMagnitude = (item2.Position - touch2.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					touch = touch2;
				}
			}
			if (touch != null)
			{
				touchesLeft.Remove(touch);
				currentPointsLeft.Remove(item2);
				touch.Moved(item2.Position);
			}
		}
		for (int j = 0; j < touchesLeft.Count; j++)
		{
			Touch touch3 = touchesLeft[j];
			touch3.End();
		}
		foreach (Touch item3 in currentPointsLeft)
		{
			_touches.Add(item3);
		}
	}

	private void RunGestures()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num = 0;
		for (int i = 0; i < _touches.Count; i++)
		{
			Touch touch = _touches[i];
			if (touch.Phase == TouchPhase.Began)
			{
				flag = true;
			}
			if (touch.Phase == TouchPhase.Moved)
			{
				flag2 = true;
				num = touch.moveFrameCount;
			}
			if (touch.Phase == TouchPhase.Stationary)
			{
				flag3 = true;
			}
			if (touch.Phase == TouchPhase.Ended)
			{
				flag4 = true;
			}
		}
		for (int j = 0; j < _gestures.Count; j++)
		{
			BaseGesture baseGesture = _gestures[j];
			if (!baseGesture.CanReceiveEvents || _preventStart.Contains(baseGesture))
			{
				continue;
			}
			List<Touch> list = null;
			_hasResetAll = false;
			bool isActive = baseGesture.IsActive;
			if (flag4 && baseGesture.IsStarted)
			{
				list = CopyIfNecessary(list, _touches);
				baseGesture.TouchesEnded(list);
			}
			bool flag5 = false;
			for (int k = 0; k < _touches.Count; k++)
			{
				if (_touches[k].Phase == TouchPhase.Ended)
				{
					flag5 = true;
					break;
				}
			}
			if (flag5)
			{
				list = CopyIfNecessary(list, _touches);
				RemoveTouchesWithPhase(list, TouchPhase.Ended);
			}
			if (flag)
			{
				list = CopyIfNecessary(list, _touches);
				baseGesture.TouchesBegan(list);
			}
			if (flag2)
			{
				if (baseGesture.IsStarted)
				{
					list = CopyIfNecessary(list, _touches);
					baseGesture.TouchesMoved(list);
				}
				else if ((float)num < baseGesture.touchBeginWindow)
				{
					list = CopyIfNecessary(list, _touches);
					baseGesture.TouchesBegan(list);
				}
			}
			if (flag3 && baseGesture.IsStarted)
			{
				list = CopyIfNecessary(list, _touches);
				baseGesture.TouchesStationary(list);
			}
			if (!isActive && baseGesture.IsActive)
			{
				CancelDependents(baseGesture);
			}
		}
		if (_hasResetAll || !AllTouchesEnded())
		{
			return;
		}
		_hasResetAll = true;
		for (int l = 0; l < _gestures.Count; l++)
		{
			BaseGesture baseGesture2 = _gestures[l];
			if (!baseGesture2.CanReceiveEvents)
			{
				baseGesture2.Reset();
			}
		}
		_preventStart.Clear();
	}

	private List<Touch> CopyIfNecessary(List<Touch> old, List<Touch> copyFrom)
	{
		if (old == null)
		{
			return new List<Touch>(copyFrom);
		}
		return old;
	}

	private bool AllTouchesEnded()
	{
		for (int i = 0; i < _touches.Count; i++)
		{
			Touch touch = _touches[i];
			if (touch.Phase != TouchPhase.Ended)
			{
				return false;
			}
		}
		return true;
	}

	private void RemoveTouchesWithPhase(List<Touch> touches, TouchPhase phase)
	{
		for (int num = touches.Count - 1; num >= 0; num--)
		{
			if (touches[num].Phase == phase)
			{
				touches.RemoveAt(num);
			}
		}
	}

	private void CancelDependents(BaseGesture gesture)
	{
		if (!_cancellers.ContainsKey(gesture))
		{
			return;
		}
		List<BaseGesture> list = _cancellers[gesture];
		for (int i = 0; i < list.Count; i++)
		{
			BaseGesture baseGesture = list[i];
			if (baseGesture.IsStarted)
			{
				baseGesture.Cancel();
			}
			else
			{
				_preventStart.Add(baseGesture);
			}
		}
	}

	private Touch CreateMouseTouch(TouchPhase phase)
	{
		float scale = NormalizedScreen.scale;
		return new Touch(Input.mousePosition / scale)
		{
			Phase = phase
		};
	}

	private Touch CreateMouseZoomTouch(TouchPhase phase)
	{
		float scale = NormalizedScreen.scale;
		Vector2 vector = Input.mousePosition / scale;
		Vector2 vector2 = new Vector2(NormalizedScreen.width / 2, NormalizedScreen.height / 2);
		return new Touch(vector + 2f * (vector2 - vector))
		{
			Phase = phase
		};
	}

	private Touch CreateTwoFingerSwipeTouch(TouchPhase phase)
	{
		float scale = NormalizedScreen.scale;
		Vector2 vector = Input.mousePosition / scale;
		return new Touch(vector + new Vector2(120f, 0f))
		{
			Phase = phase
		};
	}

	private HashSet<Touch> CurrentTouchPoints()
	{
		float scale = NormalizedScreen.scale;
		currentTouchPointsTouches.Clear();
		tempGestureCommands.Clear();
		tempGestureCommands.AddRange(gestureCommands);
		for (int i = 0; i < tempGestureCommands.Count; i++)
		{
			GestureCommand gestureCommand = tempGestureCommands[i];
			gestureCommand.Execute(currentTouchPointsTouches);
			if (gestureCommand.done)
			{
				gestureCommands.Remove(gestureCommand);
			}
		}
		if (BW.Options.useMouse())
		{
			bool mouseButton = Input.GetMouseButton(0);
			bool flag = false;
			bool flag2 = false;
			if (prevMouseDown && mouseButton)
			{
				currentTouchPointsTouches.Add(CreateMouseTouch(TouchPhase.Moved));
				flag = MappedInput.InputPressed(MappableInput.SIM_ZOOM);
				if (flag && !prevSimulateZoom)
				{
					currentTouchPointsTouches.Add(CreateMouseZoomTouch(TouchPhase.Began));
				}
				else if (flag && prevSimulateZoom)
				{
					currentTouchPointsTouches.Add(CreateMouseZoomTouch(TouchPhase.Moved));
				}
				else if (!flag && prevSimulateZoom)
				{
					currentTouchPointsTouches.Add(CreateMouseZoomTouch(TouchPhase.Ended));
				}
				else
				{
					flag2 = MappedInput.InputPressed(MappableInput.SIM_TWO_FINGER_SWIPE);
					if (flag2 && !prevSimulateTwoFingerSwipe)
					{
						currentTouchPointsTouches.Add(CreateTwoFingerSwipeTouch(TouchPhase.Began));
					}
					else if (flag2 && prevSimulateTwoFingerSwipe)
					{
						currentTouchPointsTouches.Add(CreateTwoFingerSwipeTouch(TouchPhase.Moved));
					}
					else if (!flag2 && prevSimulateTwoFingerSwipe)
					{
						currentTouchPointsTouches.Add(CreateTwoFingerSwipeTouch(TouchPhase.Ended));
					}
				}
			}
			else if (prevMouseDown && !mouseButton)
			{
				currentTouchPointsTouches.Add(CreateMouseTouch(TouchPhase.Ended));
				if (prevSimulateZoom)
				{
					currentTouchPointsTouches.Add(CreateMouseZoomTouch(TouchPhase.Ended));
				}
				if (prevSimulateTwoFingerSwipe)
				{
					currentTouchPointsTouches.Add(CreateTwoFingerSwipeTouch(TouchPhase.Ended));
				}
			}
			else if (!prevMouseDown && mouseButton)
			{
				currentTouchPointsTouches.Add(CreateMouseTouch(TouchPhase.Began));
			}
			prevMouseDown = mouseButton;
			prevSimulateZoom = flag;
			prevSimulateTwoFingerSwipe = flag2;
		}
		for (int j = 0; j < Input.touchCount; j++)
		{
			UnityEngine.Touch touch = Input.GetTouch(j);
			Touch touch2 = new Touch(touch.position / scale);
			touch2.Phase = touch.phase;
			currentTouchPointsTouches.Add(touch2);
		}
		return currentTouchPointsTouches;
	}
}
