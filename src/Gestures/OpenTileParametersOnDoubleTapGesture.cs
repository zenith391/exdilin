using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class OpenTileParametersOnDoubleTapGesture : BaseGesture
{
	private Dictionary<Tile, float> activeTiles = new Dictionary<Tile, float>();

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count != 1 || Blocksworld.CurrentState != State.Build)
		{
			EnterState(GestureState.Failed);
			return;
		}
		if (Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
			return;
		}
		ScriptPanel scriptPanel = Blocksworld.scriptPanel;
		Touch touch = allTouches[0];
		Tile tile = scriptPanel.HitTile(touch.Position);
		if (tile != null)
		{
			float time = Time.time;
			if (activeTiles.ContainsKey(tile))
			{
				float num = time - activeTiles[tile];
				_ = 0.5f;
				activeTiles.Clear();
			}
			else
			{
				activeTiles[tile] = time;
			}
		}
	}

	public override void Reset()
	{
		EnterState(GestureState.Possible);
	}

	public override string ToString()
	{
		return "OpenTileParametersOnDoubleTapGesture()";
	}
}
