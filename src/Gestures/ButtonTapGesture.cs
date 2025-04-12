using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000179 RID: 377
	public class ButtonTapGesture : BaseGesture
	{
		// Token: 0x060015BC RID: 5564 RVA: 0x000972C0 File Offset: 0x000956C0
		public ButtonTapGesture(BuildPanel buildPanel)
		{
			this._buildPanel = buildPanel;
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x000972D0 File Offset: 0x000956D0
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (this._touch != null)
			{
				return;
			}
			if (allTouches.Count != 1 || !this._buildPanel.goShopButton.activeSelf)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches[0].Phase == TouchPhase.Began && this.Hit(allTouches[0].Position))
			{
				base.EnterState(GestureState.Active);
				this._touch = allTouches[0];
			}
			else
			{
				base.EnterState(GestureState.Failed);
			}
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x00097360 File Offset: 0x00095760
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (this._touch != null && this._touch.Phase == TouchPhase.Ended)
			{
				if (this.Hit(this._touch.Position))
				{
					string name = this._buildPanel.goShopButton.name;
					if (name != null)
					{
						if (name == "Panel Shop Button")
						{
							Blocksworld.UI.Dialog.ShowGoToStoreConfirmationPrompt();
						}
					}
				}
				this._touch = null;
				base.EnterState(GestureState.Ended);
			}
		}

		// Token: 0x060015BF RID: 5567 RVA: 0x000973F2 File Offset: 0x000957F2
		public override void Reset()
		{
			this._touch = null;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x060015C0 RID: 5568 RVA: 0x00097402 File Offset: 0x00095802
		public override string ToString()
		{
			return string.Format("ButtonTap({0})", this._buildPanel.goShopButton.name);
		}

		// Token: 0x060015C1 RID: 5569 RVA: 0x00097420 File Offset: 0x00095820
		private bool Hit(Vector3 v)
		{
			return v.x >= this._buildPanel.goShopButton.transform.position.x - this._buildPanel.goShopButton.transform.localScale.x / 2f && v.x <= this._buildPanel.goShopButton.transform.position.x + this._buildPanel.goShopButton.transform.localScale.x / 2f && v.y >= this._buildPanel.goShopButton.transform.position.y - this._buildPanel.goShopButton.transform.localScale.y / 2f && v.y <= this._buildPanel.goShopButton.transform.position.y + this._buildPanel.goShopButton.transform.localScale.y / 2f;
		}

		// Token: 0x040010E3 RID: 4323
		private readonly BuildPanel _buildPanel;

		// Token: 0x040010E4 RID: 4324
		private Touch _touch;

		// Token: 0x040010E5 RID: 4325
		private const int hitMargin = 0;
	}
}
