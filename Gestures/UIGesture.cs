using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200018F RID: 399
	public class UIGesture : BaseGesture
	{
		// Token: 0x06001687 RID: 5767 RVA: 0x000A0AF8 File Offset: 0x0009EEF8
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count != 1 || Blocksworld.bw == null || Blocksworld.UI == null)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Vector2 position = allTouches[0].Position;
			if (Blocksworld.UI.IsBlocking(position))
			{
				base.EnterState(GestureState.Active);
			}
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x000A0B62 File Offset: 0x0009EF62
		public override void TouchesEnded(List<Touch> allTouches)
		{
			base.EnterState(GestureState.Ended);
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x000A0B6B File Offset: 0x0009EF6B
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
		}
	}
}
