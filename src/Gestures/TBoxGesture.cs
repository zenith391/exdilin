using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class TBoxGesture : BaseGesture
{
	public bool tBox;

	private Touch touch;

	private Vector2 startTouchPos;

	private Vector3 startButtonPos;

	public bool raycastDragging;

	public bool didActuallyMove;

	private Vector3 startPos;

	private Vector2 touchStartPos;

	public static bool skipOneTap;

	private static HashSet<GAF> highlights;

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (highlights != null)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			result.UnionWith(highlights);
		}
		return result;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
		}
		else if (CharacterEditor.Instance.InEditMode())
		{
			EnterState(GestureState.Failed);
		}
		else if (allTouches.Count > 2 || TBox.selected == null)
		{
			if (base.gestureState == GestureState.Active)
			{
				Stop();
			}
			EnterState(GestureState.Failed);
		}
		else if (allTouches.Count > 1 && raycastDragging)
		{
			Stop();
			EnterState(GestureState.Failed);
		}
		else if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.go == null)
		{
			BWLog.Info("TBoxGesture trying to do something with a destroyed block");
			Stop();
			EnterState(GestureState.Failed);
		}
		else if (Blocksworld.mouseBlock != null && Blocksworld.mouseBlock.go == null)
		{
			BWLog.Info("mouseBlock is a destroyed block");
			Stop();
			EnterState(GestureState.Failed);
		}
		else if (touch == null)
		{
			Vector3 pos = allTouches[0].Position;
			if (TBox.HitRotate(pos))
			{
				touch = allTouches[0];
				tBox = false;
				startTouchPos = touch.Position;
				startButtonPos = TBox.tileButtonRotate.GetPosition();
				TBox.StartRotate(touch.Position, TBox.RotateMode.Button);
				TBox.tileButtonMove.Hide();
				TBox.tileButtonScale.Hide();
				TBox.tileLockedModelIcon.Hide();
				TBox.tileCharacterEditIcon.Hide();
				Blocksworld.UI.SidePanel.HideCopyModelButton();
				Blocksworld.UI.SidePanel.HideSaveModelButton();
				EnterState(GestureState.Active);
			}
			else if (TBox.HitMove(pos))
			{
				touch = allTouches[0];
				tBox = false;
				TBox.StartMove(touch.Position, TBox.MoveMode.Plane);
				TBox.tileButtonRotate.Hide();
				TBox.tileButtonScale.Hide();
				TBox.tileLockedModelIcon.Hide();
				TBox.tileCharacterEditIcon.Hide();
				Blocksworld.UI.SidePanel.HideCopyModelButton();
				Blocksworld.UI.SidePanel.HideSaveModelButton();
				EnterState(GestureState.Active);
			}
			else if (TBox.HitScale(pos) && Blocksworld.selectedBlock != null)
			{
				touch = allTouches[0];
				tBox = false;
				TBox.StartScale(touch.Position, TBox.ScaleMode.Plane);
				TBox.tileButtonRotate.Hide();
				TBox.tileButtonMove.Hide();
				TBox.tileLockedModelIcon.Hide();
				TBox.tileCharacterEditIcon.Hide();
				Blocksworld.UI.SidePanel.HideCopyModelButton();
				Blocksworld.UI.SidePanel.HideSaveModelButton();
				EnterState(GestureState.Active);
			}
			else
			{
				if (Blocksworld.mouseBlock == null)
				{
					return;
				}
				if (Tutorial.state == TutorialState.Rotation || Tutorial.state == TutorialState.Scale)
				{
					EnterState(GestureState.Failed);
					return;
				}
				Vector3 vector = Vector3.zero;
				bool flag = false;
				if (Blocksworld.mouseBlock == Blocksworld.selectedBlock)
				{
					flag = true;
					vector = Blocksworld.mouseBlock.GetPosition();
				}
				else if (Blocksworld.selectedBlock == null && Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.ContainsBlock(Blocksworld.mouseBlock))
				{
					flag = true;
					vector = Blocksworld.selectedBunch.GetPosition();
				}
				else if (Blocksworld.GetSelectedScriptBlock() == Blocksworld.mouseBlock && Blocksworld.selectedBlock != null)
				{
					flag = true;
					vector = Blocksworld.selectedBlock.GetPosition();
				}
				if (!flag)
				{
					EnterState(GestureState.Failed);
					return;
				}
				raycastDragging = true;
				touch = allTouches[0];
				touchStartPos = touch.Position;
				startPos = vector;
				TBox.StartMove(touch.Position, TBox.MoveMode.Raycast);
				TBox.tileButtonRotate.Hide();
				TBox.tileButtonMove.Hide();
				TBox.tileButtonScale.Hide();
				TBox.tileLockedModelIcon.Hide();
				TBox.tileCharacterEditIcon.Hide();
				Blocksworld.UI.SidePanel.HideCopyModelButton();
				Blocksworld.UI.SidePanel.HideSaveModelButton();
				EnterState(GestureState.Active);
				Tutorial.Step();
			}
		}
		else if (allTouches.Count == 2)
		{
			tBox = true;
			touch = allTouches[1];
			RestartGesture();
		}
	}

	public static void SetGafHighlights(List<GAF> gafs)
	{
		highlights = new HashSet<GAF>(gafs);
	}

	public void ActivateDrag(Touch t)
	{
		touch = t;
		raycastDragging = true;
		if (Blocksworld.mouseBlock == null)
		{
			startPos = t.Position;
		}
		else if (Blocksworld.mouseBlock == Blocksworld.selectedBlock)
		{
			startPos = Blocksworld.mouseBlock.GetPosition();
		}
		else if (Blocksworld.selectedBunch != null)
		{
			startPos = Blocksworld.selectedBunch.GetPosition();
		}
		else
		{
			startPos = t.Position;
		}
		TBox.StartMove(touch.Position, TBox.MoveMode.Raycast);
		TBox.tileButtonRotate.Hide();
		TBox.tileButtonMove.Hide();
		TBox.tileButtonScale.Hide();
		TBox.tileLockedModelIcon.Hide();
		Blocksworld.UI.SidePanel.HideCopyModelButton();
		Blocksworld.UI.SidePanel.HideSaveModelButton();
		EnterState(GestureState.Active);
	}

	private void RestartGesture()
	{
		if (touch != null && !(TBox.tileButtonRotate == null))
		{
			if (TBox.tileButtonRotate.IsShowing())
			{
				TBox.StopRotate();
				startTouchPos = touch.Position;
				TBox.StartRotate(touch.Position, TBox.RotateMode.Finger);
			}
			else if (TBox.tileButtonMove.IsShowing())
			{
				TBox.StopMove();
				TBox.StartMove(touch.Position, TBox.MoveMode.Up);
			}
			else if (TBox.tileButtonScale.IsShowing())
			{
				TBox.StopScale();
				TBox.StartScale(touch.Position, TBox.ScaleMode.Up);
			}
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (base.gestureState != GestureState.Active)
		{
			return;
		}
		if (TBox.selected == null)
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (TBox.tileButtonRotate.IsShowing())
		{
			if (!tBox)
			{
				if (Mathf.Abs(startTouchPos.x - touch.Position.x) >= Mathf.Abs(startTouchPos.y - touch.Position.y))
				{
					TBox.tileButtonRotate.MoveTo(touch.Position.x - 40f, startButtonPos.y);
				}
				else
				{
					TBox.tileButtonRotate.MoveTo(startButtonPos.x, touch.Position.y - 40f);
				}
			}
			TBox.ContinueRotate(startTouchPos, touch.Position);
			return;
		}
		if (TBox.tileButtonMove.IsShowing())
		{
			if (!tBox)
			{
				TBox.tileButtonMove.MoveTo(touch.Position.x - 40f, touch.Position.y - 40f);
			}
			TBox.ContinueMove(touch.Position);
			return;
		}
		if (TBox.tileButtonScale.IsShowing())
		{
			if (!tBox)
			{
				TBox.tileButtonScale.MoveTo(touch.Position.x - 40f, touch.Position.y - 40f);
			}
			TBox.ContinueScale(touch.Position);
			return;
		}
		if (Blocksworld.selectedBlock == null || !Blocksworld.selectedBlock.DisableBuildModeMove())
		{
			float magnitude = (touchStartPos - touch.Position).magnitude;
			if (magnitude > 10f)
			{
				TBox.ContinueMove(touch.Position);
			}
		}
		if (Blocksworld.selectedBlock != null && Blocksworld.selectedBlock.GetPosition() != startPos)
		{
			didActuallyMove = true;
		}
		if (Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.GetPosition() != startPos)
		{
			didActuallyMove = true;
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		TouchesMoved(allTouches);
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (touch.Phase == TouchPhase.Ended)
		{
			if (tBox && allTouches.Count == 2 && allTouches[0].Phase != TouchPhase.Ended)
			{
				touch = allTouches[0];
				tBox = false;
				RestartGesture();
			}
			else
			{
				Stop();
			}
			History.AddStateIfNecessary();
		}
		Blocksworld.UI.QuickSelect.UpdateModelIcon();
	}

	public void Stop()
	{
		bool flag = TBox.tileButtonRotate.IsShowing();
		if (flag)
		{
			TBox.StopRotate();
		}
		else if (TBox.tileButtonScale.IsShowing())
		{
			TBox.StopScale();
		}
		else if (TBox.tileButtonMove.IsShowing())
		{
			TBox.StopMove();
		}
		else
		{
			TBox.StopMove();
			if (!didActuallyMove && (Blocksworld.mouseBlock == null || !Blocksworld.mouseBlock.isTerrain))
			{
				if (skipOneTap)
				{
					skipOneTap = false;
				}
				else
				{
					Blocksworld.Select(Blocksworld.mouseBlock);
				}
			}
		}
		TBox.Show(show: true);
		if (Blocksworld.selectedBlock != null)
		{
			if (Blocksworld.selectedBlock.DisableBuildModeMove())
			{
				if (Blocksworld.selectedBlock != Blocksworld.mouseBlock && !flag)
				{
					Blocksworld.Deselect();
				}
				else
				{
					Blocksworld.ShowSelectedBlockPanel();
				}
			}
			else if (Blocksworld.selectedBlock.SelectableTerrain())
			{
				Blocksworld.ShowSelectedBlockPanel();
			}
		}
		if (Blocksworld.selectedBlock != null && Blocksworld.selectedBunch == null && raycastDragging)
		{
			Blocksworld.ScrollToFirstBlockSpecificTile(Blocksworld.selectedBlock);
		}
		EnterState(GestureState.Ended);
		Tutorial.Step();
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
		touch = null;
		raycastDragging = false;
		didActuallyMove = false;
		highlights = null;
	}

	public override string ToString()
	{
		return "TBoxGesture";
	}
}
