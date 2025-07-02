using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIUserProfileLink : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Text username;

	[SerializeField]
	private RawImage profileImage;

	private int userId;

	private string usernameStr;

	private string userProfileUrl;

	private bool loadingProfileImage;

	private Texture2D userProfileTexture;

	public void SetupForUser(string nameStr, int id, bool highlighted)
	{
		usernameStr = ((!string.IsNullOrEmpty(nameStr)) ? nameStr : "Unnamed Blockster");
		if (usernameStr.Length > 20)
		{
			usernameStr = usernameStr.Substring(0, 20) + "...";
		}
		userId = id;
		username.text = usernameStr;
		ColorBlock colors = button.colors;
		if (highlighted)
		{
			colors.normalColor = Color.white;
			colors.highlightedColor = Color.white;
		}
		else
		{
			colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
			colors.highlightedColor = colors.normalColor;
		}
		button.colors = colors;
		username.color = colors.normalColor;
		base.gameObject.SetActive(value: true);
		profileImage.enabled = false;
		button.onClick.AddListener(ButtonPressed);
	}

	public void LoadProfileImage(string imageUrl)
	{
		if (!string.IsNullOrEmpty(imageUrl))
		{
			userProfileUrl = imageUrl;
		}
	}

	private IEnumerator CoroutineLoadProfileImage(string imageUrl)
	{
		userProfileTexture = new Texture2D(4, 4, TextureFormat.RGBA32, mipmap: false);
		loadingProfileImage = true;
		WWW loader = new WWW(imageUrl);
		while (!loader.isDone)
		{
			yield return null;
		}
		if (string.IsNullOrEmpty(loader.error))
		{
			loader.LoadImageIntoTexture(userProfileTexture);
			profileImage.texture = userProfileTexture;
			profileImage.enabled = true;
		}
		else
		{
			profileImage.enabled = false;
		}
	}

	public void ButtonPressed()
	{
		UIDialog dialog = Blocksworld.UI.Dialog;
		string mainText = $"Visit {usernameStr}'s page?";
		string[] buttonLabels = new string[2] { "Ok", "No thanks" };
		dialog.ShowGenericDialog(mainText, buttonLabels, new Action[2]
		{
			delegate
			{
				WorldSession.QuitWithDeepLink($"deep-link/profile/{userId.ToString()}");
			},
			delegate
			{
				Blocksworld.UI.Dialog.CloseActiveDialog();
			}
		});
	}

	private void Update()
	{
		if (!string.IsNullOrEmpty(userProfileUrl) && !loadingProfileImage)
		{
			StartCoroutine(CoroutineLoadProfileImage(userProfileUrl));
		}
	}

	private void OnDestroy()
	{
		profileImage.texture = null;
		if (userProfileTexture != null)
		{
			UnityEngine.Object.Destroy(userProfileTexture);
		}
	}
}
