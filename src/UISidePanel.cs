using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000318 RID: 792
public class UISidePanel : MonoBehaviour
{
	// Token: 0x060023C8 RID: 9160 RVA: 0x001073B8 File Offset: 0x001057B8
	public void Init()
	{
		this.tabBarUI.Init();
		this.quickSelect.Init();
		this.copyModelButton.Init(true);
		this.saveModelButton.Init(true);
		this.copyModelButton.clickAction = delegate()
		{
			Blocksworld.bw.ButtonCopyTapped();
		};
		this.saveModelButton.clickAction = delegate()
		{
			Blocksworld.bw.ButtonSaveModelTapped();
		};
		Canvas component = base.GetComponent<Canvas>();
		component.worldCamera = Blocksworld.guiCamera;
		this._canvasScaler = base.GetComponent<CanvasScaler>();
		this._buildPanelDefaultWidth = this.buildPanelProxy.sizeDelta.x;
	}

	// Token: 0x060023C9 RID: 9161 RVA: 0x0010747C File Offset: 0x0010587C
	public float BuildPanelWidth()
	{
		return this.buildPanelProxy.sizeDelta.x * NormalizedScreen.pixelScale;
	}

	// Token: 0x060023CA RID: 9162 RVA: 0x001074A4 File Offset: 0x001058A4
	public Vector3 GetBuildPanelTopLeftOffset()
	{
		float pixelWidth = this.tabBarUI.GetPixelWidth();
		Vector3 result = new Vector3(1f - pixelWidth - this._buildPanelDefaultWidth * NormalizedScreen.pixelScale, 0f, 0f);
		return result;
	}

	// Token: 0x060023CB RID: 9163 RVA: 0x001074E3 File Offset: 0x001058E3
	public void Show()
	{
		this.tabBarUI.Show();
		this.quickSelect.Show();
		Blocksworld.buildPanel.Show(true);
		this.buildPanelProxy.gameObject.SetActive(true);
		base.GetComponent<Canvas>().enabled = true;
	}

	// Token: 0x060023CC RID: 9164 RVA: 0x00107523 File Offset: 0x00105923
	public void Hide()
	{
		this.tabBarUI.Hide();
		this.quickSelect.Hide();
		Blocksworld.buildPanel.Show(false);
		this.buildPanelProxy.gameObject.SetActive(false);
		base.GetComponent<Canvas>().enabled = false;
	}

	// Token: 0x060023CD RID: 9165 RVA: 0x00107563 File Offset: 0x00105963
	private void GhostBuildPanel(bool ghost)
	{
		if (ghost == this._buildPanelGhosted)
		{
			return;
		}
		this._buildPanelGhosted = ghost;
		Blocksworld.buildPanel.GhostVisibleTiles(ghost);
	}

	// Token: 0x060023CE RID: 9166 RVA: 0x00107584 File Offset: 0x00105984
	public void ShowSaveModelButton()
	{
		this.saveModelButton.Show();
	}

	// Token: 0x060023CF RID: 9167 RVA: 0x00107591 File Offset: 0x00105991
	public void HideSaveModelButton()
	{
		this.saveModelButton.Hide();
	}

	// Token: 0x060023D0 RID: 9168 RVA: 0x0010759E File Offset: 0x0010599E
	public void ShowCopyModelButton()
	{
		this.copyModelButton.Show();
	}

	// Token: 0x060023D1 RID: 9169 RVA: 0x001075AB File Offset: 0x001059AB
	public void HideCopyModelButton()
	{
		this.copyModelButton.Hide();
	}

	// Token: 0x060023D2 RID: 9170 RVA: 0x001075B8 File Offset: 0x001059B8
	public void ShowPanelMessage(string messageStr)
	{
		this.messageText.gameObject.SetActive(true);
		this.messageText.text = messageStr;
	}

	// Token: 0x060023D3 RID: 9171 RVA: 0x001075D7 File Offset: 0x001059D7
	public void HidePanelMessage()
	{
		this.messageText.gameObject.SetActive(false);
	}

	// Token: 0x060023D4 RID: 9172 RVA: 0x001075EA File Offset: 0x001059EA
	public CanvasScaler GetCanvasScaler()
	{
		return this._canvasScaler;
	}

	// Token: 0x060023D5 RID: 9173 RVA: 0x001075F4 File Offset: 0x001059F4
	public bool Hit(Vector3 pos)
	{
		return this.HitBuildPanel(pos) || this.quickSelect.Hit(pos) || this.tabBarUI.Hit(pos) || this.copyModelButton.Hit(pos) || this.saveModelButton.Hit(pos);
	}

	// Token: 0x060023D6 RID: 9174 RVA: 0x00107650 File Offset: 0x00105A50
	public bool HitBuildPanel(Vector3 pos)
	{
		return !this._buildPanelGhosted && this.buildPanelProxy.gameObject.activeInHierarchy && Util.RectTransformContains(this.buildPanelProxy, pos) && !this.quickSelect.Hit(pos);
	}

	// Token: 0x060023D7 RID: 9175 RVA: 0x001076A2 File Offset: 0x00105AA2
	public bool HitCopyModelButton(Vector3 pos)
	{
		return this.copyModelButton.Hit(pos);
	}

	// Token: 0x060023D8 RID: 9176 RVA: 0x001076B0 File Offset: 0x00105AB0
	public bool HitSaveModelButton(Vector3 pos)
	{
		return this.saveModelButton.Hit(pos);
	}

	// Token: 0x060023D9 RID: 9177 RVA: 0x001076C0 File Offset: 0x00105AC0
	public void Layout()
	{
		Vector3 localScale = NormalizedScreen.pixelScale * Vector3.one;
		this.scaler.localScale = localScale;
		float pixelWidth = this.tabBarUI.GetPixelWidth();
		this.buildPanelProxy.anchoredPosition = new Vector2(-pixelWidth, 0f);
		this.buildPanelProxy.sizeDelta = new Vector2(this._buildPanelDefaultWidth * NormalizedScreen.pixelScale, (float)Screen.height);
		this.tabBarUI.Layout();
	}

	// Token: 0x04001EEB RID: 7915
	public UITabBar tabBarUI;

	// Token: 0x04001EEC RID: 7916
	public UIButton copyModelButton;

	// Token: 0x04001EED RID: 7917
	public UIButton saveModelButton;

	// Token: 0x04001EEE RID: 7918
	public UIQuickSelect quickSelect;

	// Token: 0x04001EEF RID: 7919
	public RectTransform buildPanelProxy;

	// Token: 0x04001EF0 RID: 7920
	public RectTransform scaler;

	// Token: 0x04001EF1 RID: 7921
	public Text messageText;

	// Token: 0x04001EF2 RID: 7922
	private CanvasScaler _canvasScaler;

	// Token: 0x04001EF3 RID: 7923
	private float _buildPanelDefaultWidth;

	// Token: 0x04001EF4 RID: 7924
	private bool _buildPanelGhosted;
}
