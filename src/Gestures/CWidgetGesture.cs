using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class CWidgetGesture : BaseGesture
{
	private const float sqrDistDrag = 64f;

	private Touch touch1;

	private Touch touch2;

	private Vector2 posStart;

	private Vector2 posLast;

	private float distStart;

	private float distLast;

	private float dragDist;

	private const int LAYER_MASK = 234011;

	private bool tBoxWasShowing;

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count > 2 || allTouches.Count == 0 || Blocksworld.lockInput || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (Blocksworld.UI.SidePanel.Hit(allTouches[0].Position))
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (allTouches.Count == 1)
		{
			touch1 = allTouches[0];
			posStart = touch1.Position;
			if (Blocksworld.CurrentState == State.Play)
			{
				Ray ray = Blocksworld.mainCamera.ScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
				if (Physics.Raycast(ray, out var hitInfo, 10000f, 234011))
				{
					Rigidbody rigidbody = null;
					if (hitInfo.rigidbody != null)
					{
						rigidbody = hitInfo.rigidbody;
					}
					if (hitInfo.transform.parent != null && hitInfo.transform.parent.GetComponent<Rigidbody>() != null)
					{
						rigidbody = hitInfo.transform.parent.GetComponent<Rigidbody>();
					}
					if (rigidbody != null && !rigidbody.isKinematic)
					{
						EnterState(GestureState.Tracking);
						return;
					}
				}
			}
			else if (!CharacterEditor.Instance.InEditMode() && TBox.selected != null)
			{
				if (Blocksworld.tBoxGesture.IsActive && (TBox.tileButtonMove.IsShowing() || TBox.tileButtonRotate.IsShowing() || TBox.tileButtonScale.IsShowing()))
				{
					EnterState(GestureState.Failed);
					return;
				}
				if ((Blocksworld.selectedBlock == null || !Blocksworld.selectedBlock.DisableBuildModeMove()) && TBox.selected.ContainsBlock(Blocksworld.mouseBlock))
				{
					EnterState(GestureState.Tracking);
					return;
				}
			}
		}
		else
		{
			touch1 = allTouches[0];
			touch2 = allTouches[1];
			dragDist = 64f;
			if (TBox.IsShowing())
			{
				tBoxWasShowing = true;
				TBox.Show(show: false);
			}
			posStart = (touch1.Position + touch2.Position) / 2f;
			distStart = (touch1.Position - touch2.Position).magnitude;
		}
		posLast = posStart;
		distLast = distStart;
		if (CharacterEditor.Instance.InEditMode())
		{
			TBox.tileCharacterEditExitIcon.Show(show: false);
		}
		EnterState(GestureState.Active);
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (Blocksworld.lockInput || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
		}
		else
		{
			if (base.gestureState == GestureState.Tracking)
			{
				return;
			}
			Vector2 vector = touch1.Position;
			if (allTouches.Count == 2)
			{
				if (touch2 == null)
				{
					touch2 = allTouches[1];
				}
				vector = (touch1.Position + touch2.Position) / 2f;
			}
			Vector2 vector2 = posLast - vector;
			if (dragDist < 64f)
			{
				dragDist = (vector - posStart).sqrMagnitude;
				if (dragDist < 64f)
				{
					return;
				}
				tBoxWasShowing = TBox.IsShowing();
				if (tBoxWasShowing)
				{
					TBox.Show(show: false);
				}
			}
			bool flag = Blocksworld.tBoxGesture.gestureState == GestureState.Active;
			bool flag2 = Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.DisableBuildModeMove();
			if (!flag || flag2)
			{
				if (allTouches.Count == 2)
				{
					Blocksworld.blocksworldCamera.SetCameraStill(still: false);
					float magnitude = (touch1.Position - touch2.Position).magnitude;
					float diff = magnitude - distLast;
					Blocksworld.blocksworldCamera.ZoomBy(diff, vector2.magnitude);
					distLast = magnitude;
					Blocksworld.blocksworldCamera.PanBy(vector2);
				}
				if (allTouches.Count == 1)
				{
					Blocksworld.blocksworldCamera.SetCameraStill(still: false);
					Blocksworld.blocksworldCamera.OrbitBy(vector2);
				}
			}
			posLast = vector;
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (allTouches.Count == 2 && base.IsActive)
		{
			if (allTouches[0].Phase == TouchPhase.Ended && allTouches[1].Phase == TouchPhase.Ended)
			{
				EnterState(GestureState.Ended);
			}
			else
			{
				if (allTouches[0].Phase == TouchPhase.Ended)
				{
					touch1 = allTouches[1];
				}
				posStart = touch1.Position;
				posLast = posStart;
			}
		}
		if (allTouches.Count == 1)
		{
			EnterState(GestureState.Ended);
		}
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
		touch1 = (touch2 = null);
		dragDist = 0f;
		if (CharacterEditor.Instance.InEditMode())
		{
			TBox.tileCharacterEditExitIcon.Show(show: true);
		}
		else if (tBoxWasShowing && TBox.selected != null)
		{
			TBox.Show(show: true);
		}
		tBoxWasShowing = false;
		Tutorial.Step();
	}

	public override string ToString()
	{
		return "CWidget";
	}
}
