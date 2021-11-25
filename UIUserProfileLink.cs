using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000325 RID: 805
public class UIUserProfileLink : MonoBehaviour
{
	// Token: 0x0600247A RID: 9338 RVA: 0x0010AB2C File Offset: 0x00108F2C
	public void SetupForUser(string nameStr, int id, bool highlighted)
	{
		this.usernameStr = ((!string.IsNullOrEmpty(nameStr)) ? nameStr : "Unnamed Blockster");
		if (this.usernameStr.Length > 20)
		{
			this.usernameStr = this.usernameStr.Substring(0, 20) + "...";
		}
		this.userId = id;
		this.username.text = this.usernameStr;
		ColorBlock colors = this.button.colors;
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
		this.button.colors = colors;
		this.username.color = colors.normalColor;
		base.gameObject.SetActive(true);
		this.profileImage.enabled = false;
		this.button.onClick.AddListener(new UnityAction(this.ButtonPressed));
	}

	// Token: 0x0600247B RID: 9339 RVA: 0x0010AC4C File Offset: 0x0010904C
	public void LoadProfileImage(string imageUrl)
	{
		if (!string.IsNullOrEmpty(imageUrl))
		{
			this.userProfileUrl = imageUrl;
		}
	}

	// Token: 0x0600247C RID: 9340 RVA: 0x0010AC60 File Offset: 0x00109060
	private IEnumerator CoroutineLoadProfileImage(string imageUrl)
	{
		this.userProfileTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
		this.loadingProfileImage = true;
		WWW loader = new WWW(imageUrl);
		while (!loader.isDone)
		{
			yield return null;
		}
		if (string.IsNullOrEmpty(loader.error))
		{
			loader.LoadImageIntoTexture(this.userProfileTexture);
			this.profileImage.texture = this.userProfileTexture;
			this.profileImage.enabled = true;
		}
		else
		{
			this.profileImage.enabled = false;
		}
		yield break;
	}

	// Token: 0x0600247D RID: 9341 RVA: 0x0010AC84 File Offset: 0x00109084
	public void ButtonPressed()
	{
		UIDialog dialog = Blocksworld.UI.Dialog;
		string mainText = string.Format("Visit {0}'s page?", this.usernameStr);
		string[] buttonLabels = new string[]
		{
			"Ok",
			"No thanks"
		};
		Action[] array = new Action[2];
		array[0] = delegate()
		{
			WorldSession.QuitWithDeepLink(string.Format("deep-link/profile/{0}", this.userId.ToString()));
		};
		array[1] = delegate()
		{
			Blocksworld.UI.Dialog.CloseActiveDialog();
		};
		dialog.ShowGenericDialog(mainText, buttonLabels, array);
	}

	// Token: 0x0600247E RID: 9342 RVA: 0x0010ACFB File Offset: 0x001090FB
	private void Update()
	{
		if (!string.IsNullOrEmpty(this.userProfileUrl) && !this.loadingProfileImage)
		{
			base.StartCoroutine(this.CoroutineLoadProfileImage(this.userProfileUrl));
		}
	}

	// Token: 0x0600247F RID: 9343 RVA: 0x0010AD2B File Offset: 0x0010912B
	private void OnDestroy()
	{
		this.profileImage.texture = null;
		if (this.userProfileTexture != null)
		{
			UnityEngine.Object.Destroy(this.userProfileTexture);
		}
	}

	// Token: 0x04001F59 RID: 8025
	[SerializeField]
	private Button button;

	// Token: 0x04001F5A RID: 8026
	[SerializeField]
	private Text username;

	// Token: 0x04001F5B RID: 8027
	[SerializeField]
	private RawImage profileImage;

	// Token: 0x04001F5C RID: 8028
	private int userId;

	// Token: 0x04001F5D RID: 8029
	private string usernameStr;

	// Token: 0x04001F5E RID: 8030
	private string userProfileUrl;

	// Token: 0x04001F5F RID: 8031
	private bool loadingProfileImage;

	// Token: 0x04001F60 RID: 8032
	private Texture2D userProfileTexture;
}
