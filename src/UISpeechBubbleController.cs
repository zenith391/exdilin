using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blocks;
using UnityEngine;
using UnityEngine.UI;

public class UISpeechBubbleController : MonoBehaviour
{
	public UIButton buttonTemplate;

	public UISpeechBubble[] speechBubbleTemplates;

	public UISpeechBubble textWindowTemplate;

	public UISpeechBubble helpTextWindowTemplate;

	private Dictionary<Block, UISpeechBubble> allSpeechBubbles = new Dictionary<Block, UISpeechBubble>();

	private HashSet<Block> removeBlockSpeechBubble = new HashSet<Block>();

	private bool inWorldSpace;

	private Dictionary<Block, UISpeechBubble> allTextWindows = new Dictionary<Block, UISpeechBubble>();

	private HashSet<Block> removeBlockTextWindow = new HashSet<Block>();

	public void Init()
	{
		for (int i = 0; i < speechBubbleTemplates.Length; i++)
		{
			speechBubbleTemplates[i].gameObject.SetActive(value: false);
		}
		buttonTemplate.Hide();
		textWindowTemplate.gameObject.SetActive(value: false);
		helpTextWindowTemplate.gameObject.SetActive(value: false);
		Camera mainCamera = Blocksworld.mainCamera;
		GetComponent<Canvas>().worldCamera = mainCamera;
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
	}

	public void UpdateSpeechBubbles()
	{
		removeBlockSpeechBubble.Clear();
		foreach (KeyValuePair<Block, UISpeechBubble> allSpeechBubble in allSpeechBubbles)
		{
			UISpeechBubble value = allSpeechBubble.Value;
			Block key = allSpeechBubble.Key;
			if (!value.isConnected)
			{
				removeBlockSpeechBubble.Add(key);
				continue;
			}
			value.isConnected = false;
			if (inWorldSpace)
			{
				Vector3 speakerPos = GetSpeakerPos(key.go, key.GetBounds());
				value.UpdatePosition(speakerPos);
			}
			else
			{
				Vector3 speakerScreenPos = GetSpeakerScreenPos(key.go, key.GetBounds());
				value.UpdatePosition(speakerScreenPos, GetProtectedRects(value));
			}
			string text = value.ProcessButtons();
			if (!string.IsNullOrEmpty(text))
			{
				HandleButtonPress(text);
			}
		}
		foreach (Block item in removeBlockSpeechBubble)
		{
			DestroySpeechBubbleOnBlock(item);
		}
	}

	public int ActiveCount()
	{
		return allSpeechBubbles.Count;
	}

	public List<Rect> ActiveScreenRects()
	{
		List<Rect> list = new List<Rect>();
		foreach (UISpeechBubble value in allSpeechBubbles.Values)
		{
			Rect screenRect = value.GetScreenRect();
			list.Add(screenRect);
		}
		return list;
	}

	public void SetWorldSpaceMode(bool worldSpace)
	{
		inWorldSpace = worldSpace;
		if (inWorldSpace)
		{
			base.transform.localScale = Vector3.one;
			GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
			GetComponent<CanvasScaler>().enabled = false;
		}
		else
		{
			GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
			GetComponent<CanvasScaler>().enabled = true;
		}
		UpdateSpeechBubbles();
		ViewportSizeDidChange();
	}

	public void ClearAll()
	{
		removeBlockSpeechBubble.Clear();
		removeBlockSpeechBubble.UnionWith(allSpeechBubbles.Keys);
		foreach (Block item in removeBlockSpeechBubble)
		{
			DestroySpeechBubbleOnBlock(item);
		}
		removeBlockSpeechBubble.Clear();
		allSpeechBubbles.Clear();
	}

	public UISpeechBubble OnBlockSpeak(Block b, string text, int tailDirection)
	{
		if (!allSpeechBubbles.TryGetValue(b, out var value) || value.tailDirection != tailDirection)
		{
			if (value != null)
			{
				DestroySpeechBubbleOnBlock(b);
			}
			value = CreateSpeechBubbleOnBlock(b, tailDirection);
		}
		SetupSpeechBubble(value, text);
		value.isConnected = true;
		return value;
	}

	private UISpeechBubble CreateSpeechBubbleOnBlock(Block b, int tailDirection)
	{
		UISpeechBubble uISpeechBubble = UnityEngine.Object.Instantiate(speechBubbleTemplates[tailDirection]);
		uISpeechBubble.Init();
		uISpeechBubble.transform.SetParent(base.transform, worldPositionStays: false);
		uISpeechBubble.gameObject.SetActive(value: true);
		uISpeechBubble.tailDirection = tailDirection;
		allSpeechBubbles[b] = uISpeechBubble;
		return uISpeechBubble;
	}

	private void DestroySpeechBubbleOnBlock(Block b)
	{
		if (allSpeechBubbles.TryGetValue(b, out var _))
		{
			UnityEngine.Object.Destroy(allSpeechBubbles[b].gameObject);
			allSpeechBubbles.Remove(b);
		}
	}

	private void SetupSpeechBubble(UISpeechBubble speechBubble, string s)
	{
		if (speechBubble.rawText != s)
		{
			speechBubble.rawText = s;
			speechBubble.Reset();
			List<string> buttonTexts = new List<string>();
			List<string> buttonCommands = new List<string>();
			string text = ParseText(s, out buttonTexts, out buttonCommands);
			speechBubble.SetText(text);
			for (int i = 0; i < buttonTexts.Count; i++)
			{
				speechBubble.AddButton(UnityEngine.Object.Instantiate(buttonTemplate), buttonTexts[i], buttonCommands[i]);
			}
			speechBubble.Layout();
		}
	}

	private string ParseText(string s, out List<string> buttonTexts, out List<string> buttonCommands)
	{
		buttonTexts = new List<string>();
		buttonCommands = new List<string>();
		MatchCollection matchCollection = Regex.Matches(s, "\\[([^]]*)]");
		foreach (object item in matchCollection)
		{
			Match match = (Match)item;
			string text = match.Groups[1].Value;
			string text2 = text;
			if (text2.Contains("|"))
			{
				string[] array = text2.Split('|');
				text = array[0];
				text2 = array[1];
			}
			text = text.Replace("/name/", WorldSession.current.config.currentUserUsername);
			text = text.Replace("/coins/", WorldSession.current.config.currentUserCoins.ToString());
			foreach (string key in Blocksworld.customIntVariables.Keys)
			{
				text = text.Replace("<" + key + ">", Blocksworld.customIntVariables[key].ToString());
			}
			buttonTexts.Add(text);
			buttonCommands.Add(text2);
		}
		if (BW.isUnityEditor)
		{
			s = Regex.Replace(s, "\\\\n\\\\n\\[", "\\n[");
		}
		s = Regex.Replace(s, "\\[[^]]*]", string.Empty);
		s = Regex.Replace(s, "{[^}]*}", string.Empty);
		s = s.Replace("\\n", "\n");
		foreach (string key2 in Blocksworld.customIntVariables.Keys)
		{
			s = s.Replace("<" + key2 + ">", Blocksworld.customIntVariables[key2].ToString());
		}
		s = s.Replace("/name/", WorldSession.current.config.currentUserUsername);
		s = s.Replace("/coins/", WorldSession.current.config.currentUserCoins.ToString());
		return s;
	}

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

	private Vector3 GetSpeakerScreenPos(GameObject goSpeaker, Bounds speakerBounds)
	{
		Camera mainCamera = Blocksworld.mainCamera;
		Vector3 speakerPos = GetSpeakerPos(goSpeaker, speakerBounds);
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
				result.x = Screen.width;
			}
			if (Vector3.Dot(lhs, transform.up) < 0f)
			{
				result.y = 0f;
			}
			else
			{
				result.y = Screen.height;
			}
		}
		return result;
	}

	private void HandleButtonPress(string action)
	{
		if (WorldSession.isProfileBuildSession())
		{
			return;
		}
		switch (action)
		{
		case "Done":
		case "Back":
		case "Stop":
			if (WorldSession.isNormalBuildAndPlaySession() || WorldSession.isConstructionChallenge())
			{
				Blocksworld.EnableBuildMode();
				Blocksworld.bw.Stop();
			}
			else
			{
				WorldSession.Quit();
			}
			return;
		case "Restart":
			Blocksworld.bw.ButtonRestartTapped();
			return;
		case "GameDone":
			if (Blocksworld.bw.forcePlayMode)
			{
				WorldSession.Quit();
			}
			else
			{
				Blocksworld.bw.Stop();
			}
			return;
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

	public UISpeechBubble ShowTextWindow(Block block, string text, Vector2 pos, float width, string buttons)
	{
		UISpeechBubble uISpeechBubble = GetBlockTextWindow(block);
		if (uISpeechBubble == null)
		{
			uISpeechBubble = UnityEngine.Object.Instantiate(textWindowTemplate);
			uISpeechBubble.Init();
			uISpeechBubble.transform.SetParent(base.transform, worldPositionStays: false);
			uISpeechBubble.gameObject.SetActive(value: true);
			allTextWindows[block] = uISpeechBubble;
		}
		uISpeechBubble.isConnected = true;
		SetupTextWindow(uISpeechBubble, pos, width, text, buttons);
		return uISpeechBubble;
	}

	public UISpeechBubble GetBlockTextWindow(Block block)
	{
		UISpeechBubble value = null;
		allTextWindows.TryGetValue(block, out value);
		return value;
	}

	public UISpeechBubble ShowHelpTextWindow(string text, Vector2 pos)
	{
		UISpeechBubble uISpeechBubble = UnityEngine.Object.Instantiate(helpTextWindowTemplate);
		uISpeechBubble.Init();
		uISpeechBubble.transform.SetParent(base.transform, worldPositionStays: false);
		uISpeechBubble.gameObject.SetActive(value: true);
		uISpeechBubble.SetText(text);
		uISpeechBubble.Layout();
		uISpeechBubble.UpdatePosition(pos);
		return uISpeechBubble;
	}

	public void UpdateTextWindows()
	{
		removeBlockTextWindow.Clear();
		foreach (KeyValuePair<Block, UISpeechBubble> allTextWindow in allTextWindows)
		{
			UISpeechBubble value = allTextWindow.Value;
			Block key = allTextWindow.Key;
			if (!value.isConnected)
			{
				removeBlockTextWindow.Add(key);
				continue;
			}
			value.isConnected = false;
			string text = value.ProcessButtons();
			if (!string.IsNullOrEmpty(text))
			{
				HandleButtonPress(text);
			}
		}
		foreach (Block item in removeBlockTextWindow)
		{
			DestroyTextWindowOnBlock(item);
		}
	}

	private void SetupTextWindow(UISpeechBubble window, Vector3 position, float width, string text, string buttonText)
	{
		if (window.rawText != buttonText)
		{
			window.Reset();
			window.rawText = buttonText;
			string[] array = buttonText.Split(';');
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					string[] array3 = text2.Split('|');
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
				window.AddButton(UnityEngine.Object.Instantiate(buttonTemplate), list[j], list2[j]);
			}
		}
		window.SetWidth(width);
		window.SetText(text);
		window.Layout();
	}

	private void DestroyTextWindowOnBlock(Block b)
	{
		if (allTextWindows.TryGetValue(b, out var value))
		{
			UnityEngine.Object.Destroy(value.gameObject);
			allTextWindows.Remove(b);
		}
	}

	private void ViewportSizeDidChange()
	{
		foreach (UISpeechBubble value in allSpeechBubbles.Values)
		{
			value.UpdateScale();
		}
		foreach (UISpeechBubble value2 in allTextWindows.Values)
		{
			value2.UpdateScale();
		}
	}

	private List<Rect> GetProtectedRects(UISpeechBubble ignore)
	{
		List<Rect> list = new List<Rect>();
		foreach (UISpeechBubble value in allSpeechBubbles.Values)
		{
			if (!(value == ignore))
			{
				list.Add(value.GetWorldRect());
			}
		}
		Blocksworld.UI.GetProtectedRects(list);
		return list;
	}

	public int GetSpeechBubbleCount()
	{
		if (allSpeechBubbles == null)
		{
			return 0;
		}
		return allSpeechBubbles.Count;
	}
}
