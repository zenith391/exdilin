using UnityEngine;

public class UITitleBar : MonoBehaviour
{
	public UIEditableText title;

	public UIEditableText subtitle;

	public GameObject coinBalance;

	public UIEditableText coinBalanceValueText;

	private RectTransform _rectTransform;

	public float Height
	{
		get
		{
			if (base.gameObject.activeInHierarchy)
			{
				return _rectTransform.sizeDelta.y;
			}
			return 0f;
		}
	}

	public void Init()
	{
		_rectTransform = (RectTransform)base.transform.GetChild(0);
		title.Init();
		subtitle.Init();
		coinBalanceValueText.Init();
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Layout();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetTitleText(string text)
	{
		title.Set(text);
	}

	public void SetSubtitleText(string text)
	{
		subtitle.Set(text);
	}

	public void SetSubtext(string str)
	{
		SetSubtitleText(str);
		if (string.IsNullOrEmpty(str))
		{
			HideSubtitle();
		}
		else
		{
			ShowSubtitle();
		}
	}

	public void ShowTitle()
	{
		title.Show();
	}

	public void HideTitle()
	{
		title.Hide();
	}

	public void ShowSubtitle()
	{
		subtitle.Show();
	}

	public void HideSubtitle()
	{
		subtitle.Hide();
	}

	public void ShowCoinBalance()
	{
		coinBalance.SetActive(value: true);
	}

	public void HideCoinBalance()
	{
		coinBalance.SetActive(value: false);
	}

	public void SetCoinBalance(int coins)
	{
		string str = coins.ToString("N0");
		coinBalanceValueText.Set(str);
	}

	private void Layout()
	{
	}

	private void ViewportSizeDidChange()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Layout();
		}
	}
}
