using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002DA RID: 730
public class HelpTextAction : TutorialAction
{
	// Token: 0x06002147 RID: 8519 RVA: 0x000F3F9C File Offset: 0x000F239C
	public override bool CancelsAction(TutorialAction otherAction)
	{
		if (this._bubble != null)
		{
			HelpTextAction helpTextAction = otherAction as HelpTextAction;
			if (helpTextAction != null && helpTextAction._bubble != null)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002148 RID: 8520 RVA: 0x000F3FDC File Offset: 0x000F23DC
	public override string ToString()
	{
		return string.Format(string.Concat(new object[]
		{
			"Text action ctx: ",
			this.context,
			" stops: ",
			this.stopProgressUntilDone,
			" highlights: ",
			this.highlighters.Count
		}), new object[0]);
	}

	// Token: 0x06002149 RID: 8521 RVA: 0x000F4048 File Offset: 0x000F2448
	protected void DestroyHighlighters()
	{
		foreach (HelpTextHighlighter helpTextHighlighter in this.highlighters)
		{
			helpTextHighlighter.Destroy();
		}
		this.highlighters.Clear();
	}

	// Token: 0x0600214A RID: 8522 RVA: 0x000F40B0 File Offset: 0x000F24B0
	public override void Execute()
	{
		if (this._bubble != null)
		{
			for (int i = 0; i < this.highlighters.Count; i++)
			{
				HelpTextHighlighter helpTextHighlighter = this.highlighters[i];
				if (!helpTextHighlighter.IsVisible())
				{
					helpTextHighlighter.Show();
				}
				helpTextHighlighter.Update();
			}
			float num = Time.time - this.startTime;
			if (num > this.lifetime)
			{
				this.LeaveContext();
			}
			NamedPose namedPose;
			if (Blocksworld.cameraPosesMap.TryGetValue(this.poseName, out namedPose))
			{
				Blocksworld.blocksworldCamera.SetTargetPosition(namedPose.position + 15f * namedPose.direction);
				Transform cameraTransform = Blocksworld.cameraTransform;
				float num2 = 0.95f;
				cameraTransform.position = num2 * Blocksworld.cameraPosition + (1f - num2) * namedPose.position;
				this.camLookatPos = num2 * (Blocksworld.cameraPosition + Blocksworld.cameraForward * 15f) + (1f - num2) * (namedPose.position + 15f * namedPose.direction);
				cameraTransform.LookAt(this.camLookatPos);
			}
		}
	}

	// Token: 0x0600214B RID: 8523 RVA: 0x000F4204 File Offset: 0x000F2604
	public override void LeaveContext()
	{
		base.LeaveContext();
		this.DestroyHighlighters();
		if (Tutorial.state != TutorialState.Play)
		{
			Tutorial.state = TutorialState.DetermineInstructions;
			Tutorial.stepOnNextUpdate = true;
		}
		if (this._bubble != null)
		{
			UnityEngine.Object.Destroy(this._bubble.gameObject);
			this._bubble = null;
		}
	}

	// Token: 0x0600214C RID: 8524 RVA: 0x000F4260 File Offset: 0x000F2660
	public override void EnterContext()
	{
		base.EnterContext();
		if (this._bubble == null)
		{
			this.startTime = Time.time;
			this._bubble = Blocksworld.UI.SpeechBubble.ShowHelpTextWindow(this.text, this.position);
			Tutorial.stepOnNextUpdate = true;
			if (!string.IsNullOrEmpty(this.sfx))
			{
				Sound.PlayOneShotSound(this.sfx, 1f);
			}
			if (!string.IsNullOrEmpty(this.highlights))
			{
				this.DestroyHighlighters();
				string[] array = this.highlights.Split(new char[]
				{
					','
				});
				foreach (string text in array)
				{
					bool flag = false;
					foreach (KeyValuePair<Block, string> keyValuePair in Blocksworld.blockNames)
					{
						if (keyValuePair.Value == text.Trim())
						{
							BlockHighlighter item = new BlockHighlighter(keyValuePair.Key);
							this.highlighters.Add(item);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						for (int j = 0; j < 11; j++)
						{
							TILE_BUTTON tile_BUTTON = (TILE_BUTTON)j;
							string text2 = tile_BUTTON.ToString();
							if (text.ToLowerInvariant() == text2.ToLowerInvariant())
							{
								UIHighlighter item2 = new UIHighlighter((TILE_BUTTON)j);
								this.highlighters.Add(item2);
								flag = true;
							}
						}
					}
					UIInputControl.ControlType control;
					if (!flag && UIInputControl.controlTypeFromString.TryGetValue(text, out control))
					{
						UIHighlighter item3 = new UIHighlighter(control);
						this.highlighters.Add(item3);
						flag = true;
					}
					if (!flag)
					{
						BWLog.Info("Could not find a block or tile named '" + text + "'");
					}
				}
			}
		}
	}

	// Token: 0x04001C3F RID: 7231
	public string text = "Help text";

	// Token: 0x04001C40 RID: 7232
	public Vector3 position = new Vector3(100f, 200f, 0f);

	// Token: 0x04001C41 RID: 7233
	public float width = 400f;

	// Token: 0x04001C42 RID: 7234
	public string poseName = string.Empty;

	// Token: 0x04001C43 RID: 7235
	public string buttons = string.Empty;

	// Token: 0x04001C44 RID: 7236
	public string sfx = string.Empty;

	// Token: 0x04001C45 RID: 7237
	public string highlights = string.Empty;

	// Token: 0x04001C46 RID: 7238
	public string tiles = string.Empty;

	// Token: 0x04001C47 RID: 7239
	public float lifetime = float.MaxValue;

	// Token: 0x04001C48 RID: 7240
	private float startTime;

	// Token: 0x04001C49 RID: 7241
	private UISpeechBubble _bubble;

	// Token: 0x04001C4A RID: 7242
	private Vector3 camLookatPos = Vector3.zero;

	// Token: 0x04001C4B RID: 7243
	public List<HelpTextHighlighter> highlighters = new List<HelpTextHighlighter>();
}
