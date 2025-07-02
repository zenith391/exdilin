using UnityEngine;
using UnityEngine.UI;

public class UIPopupEscapeMenu : UIPopup
{
	public Button exitButton;

	public Button exitWorldButton;

	public Button supportButton;

	public Button wikiButton;

	private void Awake()
	{
		exitButton.onClick.AddListener(delegate
		{
			Application.Quit();
		});
		supportButton.onClick.AddListener(delegate
		{
			Application.OpenURL("https://blocksworld-api.lindenlab.com/support/");
		});
		wikiButton.onClick.AddListener(delegate
		{
			Application.OpenURL("https://blocksworld.gamepedia.com/Blocksworld_Wiki");
		});
		if (!(exitWorldButton != null))
		{
			return;
		}
		exitWorldButton.onClick.AddListener(delegate
		{
			if (WorldSession.current != null)
			{
				Hide();
				Blocksworld.bw.ButtonExitWorldTapped();
			}
		});
	}
}
