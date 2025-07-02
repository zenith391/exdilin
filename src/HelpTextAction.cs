using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class HelpTextAction : TutorialAction
{
	public string text = "Help text";

	public Vector3 position = new Vector3(100f, 200f, 0f);

	public float width = 400f;

	public string poseName = string.Empty;

	public string buttons = string.Empty;

	public string sfx = string.Empty;

	public string highlights = string.Empty;

	public string tiles = string.Empty;

	public float lifetime = float.MaxValue;

	private float startTime;

	private UISpeechBubble _bubble;

	private Vector3 camLookatPos = Vector3.zero;

	public List<HelpTextHighlighter> highlighters = new List<HelpTextHighlighter>();

	public override bool CancelsAction(TutorialAction otherAction)
	{
		if (_bubble != null && otherAction is HelpTextAction helpTextAction && helpTextAction._bubble != null)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format(string.Concat("Text action ctx: ", context, " stops: ", stopProgressUntilDone, " highlights: ", highlighters.Count));
	}

	protected void DestroyHighlighters()
	{
		foreach (HelpTextHighlighter highlighter in highlighters)
		{
			highlighter.Destroy();
		}
		highlighters.Clear();
	}

	public override void Execute()
	{
		if (!(_bubble != null))
		{
			return;
		}
		for (int i = 0; i < highlighters.Count; i++)
		{
			HelpTextHighlighter helpTextHighlighter = highlighters[i];
			if (!helpTextHighlighter.IsVisible())
			{
				helpTextHighlighter.Show();
			}
			helpTextHighlighter.Update();
		}
		float num = Time.time - startTime;
		if (num > lifetime)
		{
			LeaveContext();
		}
		if (Blocksworld.cameraPosesMap.TryGetValue(poseName, out var value))
		{
			Blocksworld.blocksworldCamera.SetTargetPosition(value.position + 15f * value.direction);
			Transform cameraTransform = Blocksworld.cameraTransform;
			float num2 = 0.95f;
			cameraTransform.position = num2 * Blocksworld.cameraPosition + (1f - num2) * value.position;
			camLookatPos = num2 * (Blocksworld.cameraPosition + Blocksworld.cameraForward * 15f) + (1f - num2) * (value.position + 15f * value.direction);
			cameraTransform.LookAt(camLookatPos);
		}
	}

	public override void LeaveContext()
	{
		base.LeaveContext();
		DestroyHighlighters();
		if (Tutorial.state != TutorialState.Play)
		{
			Tutorial.state = TutorialState.DetermineInstructions;
			Tutorial.stepOnNextUpdate = true;
		}
		if (_bubble != null)
		{
			Object.Destroy(_bubble.gameObject);
			_bubble = null;
		}
	}

	public override void EnterContext()
	{
		base.EnterContext();
		if (!(_bubble == null))
		{
			return;
		}
		startTime = Time.time;
		_bubble = Blocksworld.UI.SpeechBubble.ShowHelpTextWindow(this.text, position);
		Tutorial.stepOnNextUpdate = true;
		if (!string.IsNullOrEmpty(sfx))
		{
			Sound.PlayOneShotSound(sfx);
		}
		if (string.IsNullOrEmpty(highlights))
		{
			return;
		}
		DestroyHighlighters();
		string[] array = highlights.Split(',');
		string[] array2 = array;
		foreach (string text in array2)
		{
			bool flag = false;
			foreach (KeyValuePair<Block, string> blockName in Blocksworld.blockNames)
			{
				if (blockName.Value == text.Trim())
				{
					BlockHighlighter item = new BlockHighlighter(blockName.Key);
					highlighters.Add(item);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < 11; j++)
				{
					TILE_BUTTON tILE_BUTTON = (TILE_BUTTON)j;
					string text2 = tILE_BUTTON.ToString();
					if (text.ToLowerInvariant() == text2.ToLowerInvariant())
					{
						UIHighlighter item2 = new UIHighlighter((TILE_BUTTON)j);
						highlighters.Add(item2);
						flag = true;
					}
				}
			}
			if (!flag && UIInputControl.controlTypeFromString.TryGetValue(text, out var value))
			{
				UIHighlighter item3 = new UIHighlighter(value);
				highlighters.Add(item3);
				flag = true;
			}
			if (!flag)
			{
				BWLog.Info("Could not find a block or tile named '" + text + "'");
			}
		}
	}
}
