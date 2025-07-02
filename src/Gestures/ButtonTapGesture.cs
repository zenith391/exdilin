using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class ButtonTapGesture : BaseGesture
{
	private readonly BuildPanel _buildPanel;

	private Touch _touch;

	private const int hitMargin = 0;

	public ButtonTapGesture(BuildPanel buildPanel)
	{
		_buildPanel = buildPanel;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (_touch == null)
		{
			if (allTouches.Count != 1 || !_buildPanel.goShopButton.activeSelf)
			{
				EnterState(GestureState.Failed);
			}
			else if (allTouches[0].Phase == TouchPhase.Began && Hit(allTouches[0].Position))
			{
				EnterState(GestureState.Active);
				_touch = allTouches[0];
			}
			else
			{
				EnterState(GestureState.Failed);
			}
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (_touch == null || _touch.Phase != TouchPhase.Ended)
		{
			return;
		}
		if (Hit(_touch.Position))
		{
			string name = _buildPanel.goShopButton.name;
			if (name != null && name == "Panel Shop Button")
			{
				Blocksworld.UI.Dialog.ShowGoToStoreConfirmationPrompt();
			}
		}
		_touch = null;
		EnterState(GestureState.Ended);
	}

	public override void Reset()
	{
		_touch = null;
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return $"ButtonTap({_buildPanel.goShopButton.name})";
	}

	private bool Hit(Vector3 v)
	{
		if (v.x >= _buildPanel.goShopButton.transform.position.x - _buildPanel.goShopButton.transform.localScale.x / 2f && v.x <= _buildPanel.goShopButton.transform.position.x + _buildPanel.goShopButton.transform.localScale.x / 2f && v.y >= _buildPanel.goShopButton.transform.position.y - _buildPanel.goShopButton.transform.localScale.y / 2f)
		{
			return v.y <= _buildPanel.goShopButton.transform.position.y + _buildPanel.goShopButton.transform.localScale.y / 2f;
		}
		return false;
	}
}
