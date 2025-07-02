using UnityEngine;
using UnityEngine.UI;

public class UISidePanel : MonoBehaviour
{
	public UITabBar tabBarUI;

	public UIButton copyModelButton;

	public UIButton saveModelButton;

	public UIQuickSelect quickSelect;

	public RectTransform buildPanelProxy;

	public RectTransform scaler;

	public Text messageText;

	private CanvasScaler _canvasScaler;

	private float _buildPanelDefaultWidth;

	private bool _buildPanelGhosted;

	public void Init()
	{
		tabBarUI.Init();
		quickSelect.Init();
		copyModelButton.Init(alphaMode: true);
		saveModelButton.Init(alphaMode: true);
		copyModelButton.clickAction = delegate
		{
			Blocksworld.bw.ButtonCopyTapped();
		};
		saveModelButton.clickAction = delegate
		{
			Blocksworld.bw.ButtonSaveModelTapped();
		};
		Canvas component = GetComponent<Canvas>();
		component.worldCamera = Blocksworld.guiCamera;
		_canvasScaler = GetComponent<CanvasScaler>();
		_buildPanelDefaultWidth = buildPanelProxy.sizeDelta.x;
	}

	public float BuildPanelWidth()
	{
		return buildPanelProxy.sizeDelta.x * NormalizedScreen.pixelScale;
	}

	public Vector3 GetBuildPanelTopLeftOffset()
	{
		float pixelWidth = tabBarUI.GetPixelWidth();
		return new Vector3(1f - pixelWidth - _buildPanelDefaultWidth * NormalizedScreen.pixelScale, 0f, 0f);
	}

	public void Show()
	{
		tabBarUI.Show();
		quickSelect.Show();
		Blocksworld.buildPanel.Show(show: true);
		buildPanelProxy.gameObject.SetActive(value: true);
		GetComponent<Canvas>().enabled = true;
	}

	public void Hide()
	{
		tabBarUI.Hide();
		quickSelect.Hide();
		Blocksworld.buildPanel.Show(show: false);
		buildPanelProxy.gameObject.SetActive(value: false);
		GetComponent<Canvas>().enabled = false;
	}

	private void GhostBuildPanel(bool ghost)
	{
		if (ghost != _buildPanelGhosted)
		{
			_buildPanelGhosted = ghost;
			Blocksworld.buildPanel.GhostVisibleTiles(ghost);
		}
	}

	public void ShowSaveModelButton()
	{
		saveModelButton.Show();
	}

	public void HideSaveModelButton()
	{
		saveModelButton.Hide();
	}

	public void ShowCopyModelButton()
	{
		copyModelButton.Show();
	}

	public void HideCopyModelButton()
	{
		copyModelButton.Hide();
	}

	public void ShowPanelMessage(string messageStr)
	{
		messageText.gameObject.SetActive(value: true);
		messageText.text = messageStr;
	}

	public void HidePanelMessage()
	{
		messageText.gameObject.SetActive(value: false);
	}

	public CanvasScaler GetCanvasScaler()
	{
		return _canvasScaler;
	}

	public bool Hit(Vector3 pos)
	{
		if (!HitBuildPanel(pos) && !quickSelect.Hit(pos) && !tabBarUI.Hit(pos) && !copyModelButton.Hit(pos))
		{
			return saveModelButton.Hit(pos);
		}
		return true;
	}

	public bool HitBuildPanel(Vector3 pos)
	{
		if (!_buildPanelGhosted && buildPanelProxy.gameObject.activeInHierarchy && Util.RectTransformContains(buildPanelProxy, pos))
		{
			return !quickSelect.Hit(pos);
		}
		return false;
	}

	public bool HitCopyModelButton(Vector3 pos)
	{
		return copyModelButton.Hit(pos);
	}

	public bool HitSaveModelButton(Vector3 pos)
	{
		return saveModelButton.Hit(pos);
	}

	public void Layout()
	{
		Vector3 localScale = NormalizedScreen.pixelScale * Vector3.one;
		scaler.localScale = localScale;
		float pixelWidth = tabBarUI.GetPixelWidth();
		buildPanelProxy.anchoredPosition = new Vector2(0f - pixelWidth, 0f);
		buildPanelProxy.sizeDelta = new Vector2(_buildPanelDefaultWidth * NormalizedScreen.pixelScale, Screen.height);
		tabBarUI.Layout();
	}
}
