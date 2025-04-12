using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000434 RID: 1076
public class UISlideshow : UISceneElement
{
	// Token: 0x06002E49 RID: 11849 RVA: 0x00149BF6 File Offset: 0x00147FF6
	protected override void LoadContentFromDataSource()
	{
		base.StartCoroutine(this.LoadContent());
	}

	// Token: 0x06002E4A RID: 11850 RVA: 0x00149C08 File Offset: 0x00148008
	private IEnumerator LoadContent()
	{
		if (!this.dataSource.IsDataLoaded())
		{
			this.dataSource.Refresh();
		}
		while (!this.dataSource.IsDataLoaded())
		{
			yield return null;
		}
		this.fadeA = this.panelA.GetComponent<CanvasGroup>();
		this.fadeB = this.panelB.GetComponent<CanvasGroup>();
		if (this.fadeA == null)
		{
			this.fadeA = this.panelA.gameObject.AddComponent<CanvasGroup>();
		}
		if (this.fadeB == null)
		{
			this.fadeB = this.panelB.gameObject.AddComponent<CanvasGroup>();
		}
		int panelCount = Mathf.Min(this.maxPanelCount, this.dataSource.Keys.Count);
		this.dataIds = new string[panelCount];
		for (int i = 0; i < panelCount; i++)
		{
			this.dataIds[i] = this.dataSource.Keys[i];
		}
		this.LoadFirst();
		yield break;
	}

	// Token: 0x06002E4B RID: 11851 RVA: 0x00149C24 File Offset: 0x00148024
	public void LoadFirst()
	{
		this.fadeB.alpha = 0f;
		this.fadeB.interactable = false;
		if (this.dataIds.Length == 0)
		{
			this.fadeA.alpha = 0f;
			this.fadeA.interactable = false;
			return;
		}
		this.panelA.SetupPanel(this.dataSource, this.imageManager, this.dataIds[0]);
		this.fadeA.alpha = 1f;
		this.fadeA.interactable = true;
		this.currentFrameIndex = 0;
		if (this.dataIds.Length == 1)
		{
			return;
		}
		this.panelB.SetupPanel(this.dataSource, this.imageManager, this.dataIds[1]);
		base.StartCoroutine(this.SlideshowCoroutine());
	}

	// Token: 0x06002E4C RID: 11852 RVA: 0x00149CF4 File Offset: 0x001480F4
	private IEnumerator SlideshowCoroutine()
	{
		yield return new WaitForSeconds(this.autoPlayStartPause);
		while (this.autoPlay)
		{
			yield return new WaitForSeconds(this.autoPlayTimeOnEachFrame);
			bool even = this.currentFrameIndex % 2 == 0;
			UIPanelContents fromPanel = (!even) ? this.panelB : this.panelA;
			UIPanelContents uipanelContents = (!even) ? this.panelA : this.panelB;
			CanvasGroup fromFade = (!even) ? this.fadeB : this.fadeA;
			CanvasGroup toFade = (!even) ? this.fadeA : this.fadeB;
			fromFade.interactable = false;
			float fadeTime = this.autoPlayTransitionTime;
			while (fadeTime > 0f)
			{
				float t = fadeTime / this.autoPlayTransitionTime;
				fromFade.alpha = t;
				toFade.alpha = 1f - t;
				fadeTime -= Time.deltaTime;
				yield return null;
			}
			fromFade.alpha = 0f;
			toFade.alpha = 1f;
			toFade.interactable = true;
			int nextFrameIndex = this.currentFrameIndex + 1;
			if (nextFrameIndex >= this.dataIds.Length)
			{
				nextFrameIndex = 0;
			}
			fromPanel.SetupPanel(this.dataSource, this.imageManager, this.dataIds[nextFrameIndex]);
			this.currentFrameIndex = nextFrameIndex;
		}
		yield break;
	}

	// Token: 0x040026C1 RID: 9921
	public bool autoPlay = true;

	// Token: 0x040026C2 RID: 9922
	public float autoPlayStartPause = 1f;

	// Token: 0x040026C3 RID: 9923
	public float autoPlayTimeOnEachFrame = 2f;

	// Token: 0x040026C4 RID: 9924
	public float autoPlayTransitionTime = 0.5f;

	// Token: 0x040026C5 RID: 9925
	public int maxPanelCount = 10;

	// Token: 0x040026C6 RID: 9926
	public UIPanelContents panelA;

	// Token: 0x040026C7 RID: 9927
	public UIPanelContents panelB;

	// Token: 0x040026C8 RID: 9928
	private CanvasGroup fadeA;

	// Token: 0x040026C9 RID: 9929
	private CanvasGroup fadeB;

	// Token: 0x040026CA RID: 9930
	private string[] dataIds;

	// Token: 0x040026CB RID: 9931
	private int currentFrameIndex;
}
