using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000189 RID: 393
	public class TapControlGesture : BaseGesture
	{
		// Token: 0x06001654 RID: 5716 RVA: 0x0009E0C0 File Offset: 0x0009C4C0
		public static bool HasWorldTapPos()
		{
			return TapControlGesture.hasWorldTapPos;
		}

		// Token: 0x06001655 RID: 5717 RVA: 0x0009E0C7 File Offset: 0x0009C4C7
		public static Vector3 GetWorldTapPos()
		{
			return TapControlGesture.worldTapPos;
		}

		// Token: 0x06001656 RID: 5718 RVA: 0x0009E0CE File Offset: 0x0009C4CE
		public static float GetWorldTapTime()
		{
			return TapControlGesture.worldTapTime;
		}

		// Token: 0x06001657 RID: 5719 RVA: 0x0009E0D8 File Offset: 0x0009C4D8
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count > 1)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches[0].Phase != TouchPhase.Began)
			{
				return;
			}
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
			bool flag = false;
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit, 10000f, 539))
			{
				TapControlGesture.startTouchPos = allTouches[0].Position;
				TapControlGesture.possibleWorldTapPos = raycastHit.point;
				TapControlGesture.startPos = allTouches[0].Position;
				flag = true;
			}
			if (flag)
			{
				base.EnterState(GestureState.Active);
			}
			else
			{
				base.EnterState(GestureState.Failed);
			}
		}

		// Token: 0x06001658 RID: 5720 RVA: 0x0009E198 File Offset: 0x0009C598
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Vector3 a = allTouches[0].Position;
			if (base.gestureState == GestureState.Tracking && (a - TapControlGesture.startPos).sqrMagnitude > 400f)
			{
				base.EnterState(GestureState.Cancelled);
				return;
			}
		}

		// Token: 0x06001659 RID: 5721 RVA: 0x0009E1E8 File Offset: 0x0009C5E8
		public override void TouchesStationary(List<Touch> allTouches)
		{
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x0009E1EC File Offset: 0x0009C5EC
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (allTouches[0].Phase != TouchPhase.Ended)
			{
				return;
			}
			if ((allTouches[0].Position - TapControlGesture.startTouchPos).sqrMagnitude > 5f)
			{
				return;
			}
			TapControlGesture.hasWorldTapPos = true;
			TapControlGesture.worldTapPos = TapControlGesture.possibleWorldTapPos;
			TapControlGesture.worldTapTime = Time.time;
			base.EnterState(GestureState.Ended);
		}

		// Token: 0x0600165B RID: 5723 RVA: 0x0009E256 File Offset: 0x0009C656
		public override void Cancel()
		{
			base.EnterState(GestureState.Cancelled);
		}

		// Token: 0x0600165C RID: 5724 RVA: 0x0009E25F File Offset: 0x0009C65F
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x0600165D RID: 5725 RVA: 0x0009E268 File Offset: 0x0009C668
		public override string ToString()
		{
			return "TapControlGesture()";
		}

		// Token: 0x04001169 RID: 4457
		private static Vector3 worldTapPos = default(Vector3);

		// Token: 0x0400116A RID: 4458
		private static Vector3 possibleWorldTapPos = default(Vector3);

		// Token: 0x0400116B RID: 4459
		private static bool hasWorldTapPos = false;

		// Token: 0x0400116C RID: 4460
		private static float worldTapTime = 0f;

		// Token: 0x0400116D RID: 4461
		private static Vector3 startPos = default(Vector3);

		// Token: 0x0400116E RID: 4462
		private static Vector2 startTouchPos = default(Vector2);
	}
}
