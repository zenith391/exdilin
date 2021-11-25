using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200017F RID: 383
	public class OpenTileParametersOnDoubleTapGesture : BaseGesture
	{
		// Token: 0x06001602 RID: 5634 RVA: 0x0009B1D0 File Offset: 0x000995D0
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count != 1 || Blocksworld.CurrentState != State.Build)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			ScriptPanel scriptPanel = Blocksworld.scriptPanel;
			Touch touch = allTouches[0];
			Tile tile = scriptPanel.HitTile(touch.Position, false);
			if (tile != null)
			{
				float time = Time.time;
				if (this.activeTiles.ContainsKey(tile))
				{
					float num = time - this.activeTiles[tile];
					if (num < 0.5f)
					{
					}
					this.activeTiles.Clear();
				}
				else
				{
					this.activeTiles[tile] = time;
				}
			}
		}

		// Token: 0x06001603 RID: 5635 RVA: 0x0009B284 File Offset: 0x00099684
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x06001604 RID: 5636 RVA: 0x0009B28D File Offset: 0x0009968D
		public override string ToString()
		{
			return "OpenTileParametersOnDoubleTapGesture()";
		}

		// Token: 0x04001126 RID: 4390
		private Dictionary<Tile, float> activeTiles = new Dictionary<Tile, float>();
	}
}
