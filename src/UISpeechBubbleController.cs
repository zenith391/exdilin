using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blocks;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200031A RID: 794
public class UISpeechBubbleController : MonoBehaviour
{
	// Token: 0x060023F2 RID: 9202 RVA: 0x00108040 File Offset: 0x00106440
	public void Init()
	{
		for (int i = 0; i < this.speechBubbleTemplates.Length; i++)
		{
			this.speechBubbleTemplates[i].gameObject.SetActive(false);
		}
		this.buttonTemplate.Hide();
		this.textWindowTemplate.gameObject.SetActive(false);
		this.helpTextWindowTemplate.gameObject.SetActive(false);
		Camera mainCamera = Blocksworld.mainCamera;
		base.GetComponent<Canvas>().worldCamera = mainCamera;
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
	}

	// Token: 0x060023F3 RID: 9203 RVA: 0x001080CC File Offset: 0x001064CC
	public void UpdateSpeechBubbles()
	{
		this.removeBlockSpeechBubble.Clear();
		foreach (KeyValuePair<Block, UISpeechBubble> keyValuePair in this.allSpeechBubbles)
		{
			UISpeechBubble value = keyValuePair.Value;
			Block key = keyValuePair.Key;
			if (!value.isConnected)
			{
				this.removeBlockSpeechBubble.Add(key);
			}
			else
			{
				value.isConnected = false;
				if (this.inWorldSpace)
				{
					Vector3 speakerPos = this.GetSpeakerPos(key.go, key.GetBounds());
					value.UpdatePosition(speakerPos);
				}
				else
				{
					Vector3 speakerScreenPos = this.GetSpeakerScreenPos(key.go, key.GetBounds());
					value.UpdatePosition(speakerScreenPos, this.GetProtectedRects(value));
				}
				string text = value.ProcessButtons();
				if (!string.IsNullOrEmpty(text))
				{
					this.HandleButtonPress(text);
				}
			}
		}
		foreach (Block b in this.removeBlockSpeechBubble)
		{
			this.DestroySpeechBubbleOnBlock(b);
		}
	}

	// Token: 0x060023F4 RID: 9204 RVA: 0x0010821C File Offset: 0x0010661C
	public int ActiveCount()
	{
		return this.allSpeechBubbles.Count;
	}

	// Token: 0x060023F5 RID: 9205 RVA: 0x0010822C File Offset: 0x0010662C
	public List<Rect> ActiveScreenRects()
	{
		List<Rect> list = new List<Rect>();
		foreach (UISpeechBubble uispeechBubble in this.allSpeechBubbles.Values)
		{
			Rect screenRect = uispeechBubble.GetScreenRect();
			list.Add(screenRect);
		}
		return list;
	}

	// Token: 0x060023F6 RID: 9206 RVA: 0x0010829C File Offset: 0x0010669C
	public void SetWorldSpaceMode(bool worldSpace)
	{
		this.inWorldSpace = worldSpace;
		if (this.inWorldSpace)
		{
			base.transform.localScale = Vector3.one;
			base.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
			base.GetComponent<CanvasScaler>().enabled = false;
		}
		else
		{
			base.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
			base.GetComponent<CanvasScaler>().enabled = true;
		}
		this.UpdateSpeechBubbles();
		this.ViewportSizeDidChange();
	}

	// Token: 0x060023F7 RID: 9207 RVA: 0x0010830C File Offset: 0x0010670C
	public void ClearAll()
	{
		this.removeBlockSpeechBubble.Clear();
		this.removeBlockSpeechBubble.UnionWith(this.allSpeechBubbles.Keys);
		foreach (Block b in this.removeBlockSpeechBubble)
		{
			this.DestroySpeechBubbleOnBlock(b);
		}
		this.removeBlockSpeechBubble.Clear();
		this.allSpeechBubbles.Clear();
	}

	// Token: 0x060023F8 RID: 9208 RVA: 0x001083A0 File Offset: 0x001067A0
	public UISpeechBubble OnBlockSpeak(Block b, string text, int tailDirection)
	{
		UISpeechBubble uispeechBubble;
		if (!this.allSpeechBubbles.TryGetValue(b, out uispeechBubble) || uispeechBubble.tailDirection != tailDirection)
		{
			if (uispeechBubble != null)
			{
				this.DestroySpeechBubbleOnBlock(b);
			}
			uispeechBubble = this.CreateSpeechBubbleOnBlock(b, tailDirection);
		}
		this.SetupSpeechBubble(uispeechBubble, text);
		uispeechBubble.isConnected = true;
		return uispeechBubble;
	}

	// Token: 0x060023F9 RID: 9209 RVA: 0x001083F8 File Offset: 0x001067F8
	private UISpeechBubble CreateSpeechBubbleOnBlock(Block b, int tailDirection)
	{
		UISpeechBubble uispeechBubble = UnityEngine.Object.Instantiate<UISpeechBubble>(this.speechBubbleTemplates[tailDirection]);
		uispeechBubble.Init();
		uispeechBubble.transform.SetParent(base.transform, false);
		uispeechBubble.gameObject.SetActive(true);
		uispeechBubble.tailDirection = tailDirection;
		this.allSpeechBubbles[b] = uispeechBubble;
		return uispeechBubble;
	}

	// Token: 0x060023FA RID: 9210 RVA: 0x0010844C File Offset: 0x0010684C
	private void DestroySpeechBubbleOnBlock(Block b)
	{
		UISpeechBubble uispeechBubble;
		if (this.allSpeechBubbles.TryGetValue(b, out uispeechBubble))
		{
			UnityEngine.Object.Destroy(this.allSpeechBubbles[b].gameObject);
			this.allSpeechBubbles.Remove(b);
		}
	}

	// Token: 0x060023FB RID: 9211 RVA: 0x00108490 File Offset: 0x00106890
	private void SetupSpeechBubble(UISpeechBubble speechBubble, string s)
	{
		if (speechBubble.rawText != s)
		{
			speechBubble.rawText = s;
			speechBubble.Reset();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			string text = this.ParseText(s, out list, out list2);
			speechBubble.SetText(text);
			for (int i = 0; i < list.Count; i++)
			{
				speechBubble.AddButton(UnityEngine.Object.Instantiate<UIButton>(this.buttonTemplate), list[i], list2[i]);
			}
			speechBubble.Layout();
		}
	}

	// Token: 0x060023FC RID: 9212 RVA: 0x00108518 File Offset: 0x00106918
	private string ParseText(string s, out List<string> buttonTexts, out List<string> buttonCommands)
	{
		buttonTexts = new List<string>();
		buttonCommands = new List<string>();
		MatchCollection matchCollection = Regex.Matches(s, "\\[([^]]*)]");
		IEnumerator enumerator = matchCollection.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Match match = (Match)obj;
				string text = match.Groups[1].Value;
				string text2 = text;
				if (text2.Contains("|"))
				{
					string[] array = text2.Split(new char[]
					{
						'|'
					});
					text = array[0];
					text2 = array[1];
				}
				text = text.Replace("/name/", WorldSession.current.config.currentUserUsername);
                text = text.Replace("/coins/", WorldSession.current.config.currentUserCoins.ToString());
				foreach (string text3 in Blocksworld.customIntVariables.Keys)
				{
					text = text.Replace("<" + text3 + ">", Blocksworld.customIntVariables[text3].ToString());
				}
				buttonTexts.Add(text);
				buttonCommands.Add(text2);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		if (BW.isUnityEditor)
		{
			s = Regex.Replace(s, "\\\\n\\\\n\\[", "\\n[");
		}
		s = Regex.Replace(s, "\\[[^]]*]", string.Empty);
		s = Regex.Replace(s, "{[^}]*}", string.Empty);
		s = s.Replace("\\n", "\n");
		foreach (string text4 in Blocksworld.customIntVariables.Keys)
		{
			s = s.Replace("<" + text4 + ">", Blocksworld.customIntVariables[text4].ToString());
		}
		s = s.Replace("/name/", WorldSession.current.config.currentUserUsername);
        s = s.Replace("/coins/", WorldSession.current.config.currentUserCoins.ToString());
        return s;
	}

	// Token: 0x060023FD RID: 9213 RVA: 0x0010878C File Offset: 0x00106B8C
	private Vector3 GetSpeakerPos(GameObject goSpeaker, Bounds speakerBounds)
	{
		Camera mainCamera = Blocksworld.mainCamera;
		Vector3 result = speakerBounds.center + Mathf.Clamp(speakerBounds.extents.y, 0.5f, 2f) * mainCamera.transform.up - 1f * mainCamera.transform.forward;
		if (Blocksworld.blocksworldCamera.firstPersonBlock != null && Blocksworld.blocksworldCamera.firstPersonBlock.go == goSpeaker)
		{
			result = mainCamera.transform.position + mainCamera.transform.forward + 0.75f * mainCamera.transform.right + 0.5f * mainCamera.transform.up;
		}
		return result;
	}

	// Token: 0x060023FE RID: 9214 RVA: 0x00108870 File Offset: 0x00106C70
	private Vector3 GetSpeakerScreenPos(GameObject goSpeaker, Bounds speakerBounds)
	{
		Camera mainCamera = Blocksworld.mainCamera;
		Vector3 speakerPos = this.GetSpeakerPos(goSpeaker, speakerBounds);
		Vector3 result = Vector3.zero;
		if (GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(speakerPos, Vector3.one * 0.1f)))
		{
			result = mainCamera.WorldToScreenPoint(speakerPos);
		}
		else
		{
			Transform transform = mainCamera.transform;
			Vector3 lhs = speakerPos - transform.position;
			if (Vector3.Dot(lhs, transform.right) < 0f)
			{
				result.x = 0f;
			}
			else
			{
				result.x = (float)Screen.width;
			}
			if (Vector3.Dot(lhs, transform.up) < 0f)
			{
				result.y = 0f;
			}
			else
			{
				result.y = (float)Screen.height;
			}
		}
		return result;
	}

	// Token: 0x060023FF RID: 9215 RVA: 0x00108944 File Offset: 0x00106D44
	private void HandleButtonPress(string action)
	{
		if (!WorldSession.isProfileBuildSession())
		{
			if (action != null)
			{
				if (action == "Done" || action == "Back" || action == "Stop")
				{
					if (WorldSession.isNormalBuildAndPlaySession() || WorldSession.isConstructionChallenge())
					{
						Blocksworld.EnableBuildMode();
						Blocksworld.bw.Stop(false, true);
					}
					else
					{
						WorldSession.Quit();
					}
					return;
				}
				if (action == "Restart")
				{
					Blocksworld.bw.ButtonRestartTapped();
					return;
				}
				if (action == "GameDone")
				{
					if (Blocksworld.bw.forcePlayMode)
					{
						WorldSession.Quit();
					}
					else
					{
						Blocksworld.bw.Stop(false, true);
					}
					return;
				}
			}
			if (action.Length == 1)
			{
				int num = Convert.ToInt32(action[0]) - 65;
				if (num >= 0 && num < 26)
				{
					Blocksworld.sending[num] = true;
					Blocksworld.sendingValues[num] = 1f;
				}
			}
		}
	}

	// Token: 0x06002400 RID: 9216 RVA: 0x00108A68 File Offset: 0x00106E68
	public UISpeechBubble ShowTextWindow(Block block, string text, Vector2 pos, float width, string buttons)
	{
		UISpeechBubble uispeechBubble = this.GetBlockTextWindow(block);
		if (uispeechBubble == null)
		{
			uispeechBubble = UnityEngine.Object.Instantiate<UISpeechBubble>(this.textWindowTemplate);
			uispeechBubble.Init();
			uispeechBubble.transform.SetParent(base.transform, false);
			uispeechBubble.gameObject.SetActive(true);
			this.allTextWindows[block] = uispeechBubble;
		}
		uispeechBubble.isConnected = true;
		this.SetupTextWindow(uispeechBubble, pos, width, text, buttons);
		return uispeechBubble;
	}

	// Token: 0x06002401 RID: 9217 RVA: 0x00108AE0 File Offset: 0x00106EE0
	public UISpeechBubble GetBlockTextWindow(Block block)
	{
		UISpeechBubble result = null;
		this.allTextWindows.TryGetValue(block, out result);
		return result;
	}

	// Token: 0x06002402 RID: 9218 RVA: 0x00108B00 File Offset: 0x00106F00
	public UISpeechBubble ShowHelpTextWindow(string text, Vector2 pos)
	{
		UISpeechBubble uispeechBubble = UnityEngine.Object.Instantiate<UISpeechBubble>(this.helpTextWindowTemplate);
		uispeechBubble.Init();
		uispeechBubble.transform.SetParent(base.transform, false);
		uispeechBubble.gameObject.SetActive(true);
		uispeechBubble.SetText(text);
		uispeechBubble.Layout();
		uispeechBubble.UpdatePosition(pos);
		return uispeechBubble;
	}

	// Token: 0x06002403 RID: 9219 RVA: 0x00108B54 File Offset: 0x00106F54
	public void UpdateTextWindows()
	{
		this.removeBlockTextWindow.Clear();
		foreach (KeyValuePair<Block, UISpeechBubble> keyValuePair in this.allTextWindows)
		{
			UISpeechBubble value = keyValuePair.Value;
			Block key = keyValuePair.Key;
			if (!value.isConnected)
			{
				this.removeBlockTextWindow.Add(key);
			}
			else
			{
				value.isConnected = false;
				string text = value.ProcessButtons();
				if (!string.IsNullOrEmpty(text))
				{
					this.HandleButtonPress(text);
				}
			}
		}
		foreach (Block b in this.removeBlockTextWindow)
		{
			this.DestroyTextWindowOnBlock(b);
		}
	}

	// Token: 0x06002404 RID: 9220 RVA: 0x00108C54 File Offset: 0x00107054
	private void SetupTextWindow(UISpeechBubble window, Vector3 position, float width, string text, string buttonText)
	{
		if (window.rawText != buttonText)
		{
			window.Reset();
			window.rawText = buttonText;
			string[] array = buttonText.Split(new char[]
			{
				';'
			});
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (string text2 in array)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					string[] array3 = text2.Split(new char[]
					{
						'|'
					});
					list.Add(array3[0]);
					if (array3.Length == 2)
					{
						list2.Add(array3[1]);
					}
					else
					{
						list2.Add(array3[0]);
					}
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				window.AddButton(UnityEngine.Object.Instantiate<UIButton>(this.buttonTemplate), list[j], list2[j]);
			}
		}
		window.SetWidth(width);
		window.SetText(text);
		window.Layout();
	}

	// Token: 0x06002405 RID: 9221 RVA: 0x00108D60 File Offset: 0x00107160
	private void DestroyTextWindowOnBlock(Block b)
	{
		UISpeechBubble uispeechBubble;
		if (this.allTextWindows.TryGetValue(b, out uispeechBubble))
		{
			UnityEngine.Object.Destroy(uispeechBubble.gameObject);
			this.allTextWindows.Remove(b);
		}
	}

	// Token: 0x06002406 RID: 9222 RVA: 0x00108D98 File Offset: 0x00107198
	private void ViewportSizeDidChange()
	{
		foreach (UISpeechBubble uispeechBubble in this.allSpeechBubbles.Values)
		{
			uispeechBubble.UpdateScale();
		}
		foreach (UISpeechBubble uispeechBubble2 in this.allTextWindows.Values)
		{
			uispeechBubble2.UpdateScale();
		}
	}

	// Token: 0x06002407 RID: 9223 RVA: 0x00108E48 File Offset: 0x00107248
	private List<Rect> GetProtectedRects(UISpeechBubble ignore)
	{
		List<Rect> list = new List<Rect>();
		foreach (UISpeechBubble uispeechBubble in this.allSpeechBubbles.Values)
		{
			if (!(uispeechBubble == ignore))
			{
				list.Add(uispeechBubble.GetWorldRect());
			}
		}
		Blocksworld.UI.GetProtectedRects(list);
		return list;
	}

	// Token: 0x06002408 RID: 9224 RVA: 0x00108ED4 File Offset: 0x001072D4
	public int GetSpeechBubbleCount()
	{
		if (this.allSpeechBubbles == null)
		{
			return 0;
		}
		return this.allSpeechBubbles.Count;
	}

	// Token: 0x04001F09 RID: 7945
	public UIButton buttonTemplate;

	// Token: 0x04001F0A RID: 7946
	public UISpeechBubble[] speechBubbleTemplates;

	// Token: 0x04001F0B RID: 7947
	public UISpeechBubble textWindowTemplate;

	// Token: 0x04001F0C RID: 7948
	public UISpeechBubble helpTextWindowTemplate;

	// Token: 0x04001F0D RID: 7949
	private Dictionary<Block, UISpeechBubble> allSpeechBubbles = new Dictionary<Block, UISpeechBubble>();

	// Token: 0x04001F0E RID: 7950
	private HashSet<Block> removeBlockSpeechBubble = new HashSet<Block>();

	// Token: 0x04001F0F RID: 7951
	private bool inWorldSpace;

	// Token: 0x04001F10 RID: 7952
	private Dictionary<Block, UISpeechBubble> allTextWindows = new Dictionary<Block, UISpeechBubble>();

	// Token: 0x04001F11 RID: 7953
	private HashSet<Block> removeBlockTextWindow = new HashSet<Block>();
}
