using System.Collections;
using UnityEngine;

public class UISlideshow : UISceneElement
{
	public bool autoPlay = true;

	public float autoPlayStartPause = 1f;

	public float autoPlayTimeOnEachFrame = 2f;

	public float autoPlayTransitionTime = 0.5f;

	public int maxPanelCount = 10;

	public UIPanelContents panelA;

	public UIPanelContents panelB;

	private CanvasGroup fadeA;

	private CanvasGroup fadeB;

	private string[] dataIds;

	private int currentFrameIndex;

	protected override void LoadContentFromDataSource()
	{
		StartCoroutine(LoadContent());
	}

	private IEnumerator LoadContent()
	{
		if (!dataSource.IsDataLoaded())
		{
			dataSource.Refresh();
		}
		while (!dataSource.IsDataLoaded())
		{
			yield return null;
		}
		fadeA = panelA.GetComponent<CanvasGroup>();
		fadeB = panelB.GetComponent<CanvasGroup>();
		if (fadeA == null)
		{
			fadeA = panelA.gameObject.AddComponent<CanvasGroup>();
		}
		if (fadeB == null)
		{
			fadeB = panelB.gameObject.AddComponent<CanvasGroup>();
		}
		int num = Mathf.Min(maxPanelCount, dataSource.Keys.Count);
		dataIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			dataIds[i] = dataSource.Keys[i];
		}
		LoadFirst();
	}

	public void LoadFirst()
	{
		fadeB.alpha = 0f;
		fadeB.interactable = false;
		if (dataIds.Length == 0)
		{
			fadeA.alpha = 0f;
			fadeA.interactable = false;
			return;
		}
		panelA.SetupPanel(dataSource, imageManager, dataIds[0]);
		fadeA.alpha = 1f;
		fadeA.interactable = true;
		currentFrameIndex = 0;
		if (dataIds.Length != 1)
		{
			panelB.SetupPanel(dataSource, imageManager, dataIds[1]);
			StartCoroutine(SlideshowCoroutine());
		}
	}

	private IEnumerator SlideshowCoroutine()
	{
		yield return new WaitForSeconds(autoPlayStartPause);
		while (autoPlay)
		{
			yield return new WaitForSeconds(autoPlayTimeOnEachFrame);
			bool flag = currentFrameIndex % 2 == 0;
			UIPanelContents fromPanel = ((!flag) ? panelB : panelA);
			if (flag)
			{
				_ = panelB;
			}
			else
			{
				_ = panelA;
			}
			CanvasGroup fromFade = ((!flag) ? fadeB : fadeA);
			CanvasGroup toFade = ((!flag) ? fadeA : fadeB);
			fromFade.interactable = false;
			float fadeTime = autoPlayTransitionTime;
			while (fadeTime > 0f)
			{
				float num = (fromFade.alpha = fadeTime / autoPlayTransitionTime);
				toFade.alpha = 1f - num;
				fadeTime -= Time.deltaTime;
				yield return null;
			}
			fromFade.alpha = 0f;
			toFade.alpha = 1f;
			toFade.interactable = true;
			int num3 = currentFrameIndex + 1;
			if (num3 >= dataIds.Length)
			{
				num3 = 0;
			}
			fromPanel.SetupPanel(dataSource, imageManager, dataIds[num3]);
			currentFrameIndex = num3;
		}
	}
}
