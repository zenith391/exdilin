using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000186 RID: 390
	public class SecretCommandGesture : BaseGesture
	{
		// Token: 0x0600163E RID: 5694 RVA: 0x0009CF98 File Offset: 0x0009B398
		public SecretCommandGesture()
		{
			float num = (float)NormalizedScreen.width;
			float num2 = (float)NormalizedScreen.height;
			float num3 = num * 0.1f;
			float num4 = num2 * 0.1f;
			Rect rect = new Rect(0f, num2 - num4, num, num4);
			Rect rect2 = new Rect(0f, 0f, num, num4);
			Rect rect3 = new Rect(0f, 0f, num3, num2);
			Rect rect4 = new Rect(num - num3, 0f, num3, num2);
			SecretCommandGesture.SecretCommand secretCommand = new SecretCommandGesture.SecretCommand();
			secretCommand.action = delegate()
			{
				if (!this.autoPlayer.IsActive())
				{
					this.autoPlayer.StartAutoPlay();
					BW.Analytics.SendAnalyticsEvent("world-autoplay", string.Empty);
					OnScreenLog.AddLogItem("Starting auto play. Tap anywhere to stop.", 5f, true);
				}
			};
			secretCommand.canExecute = (() => Blocksworld.CurrentState == State.Build && Tutorial.state != TutorialState.None);
			SecretCommandGesture.SecretCommand secretCommand2 = secretCommand;
			if (BW.isUnityEditor && Options.UseSimpleAutoPlayTrigger)
			{
				secretCommand2.rects = new Rect[]
				{
					rect,
					rect,
					rect,
					rect,
					rect
				};
			}
			else
			{
				secretCommand2.rects = new Rect[]
				{
					rect,
					rect2,
					rect3,
					rect4
				};
			}
			this.commands.Add(secretCommand2);
			secretCommand = new SecretCommandGesture.SecretCommand();
			secretCommand.action = delegate()
			{
				Texture[] array = Resources.FindObjectsOfTypeAll<Texture>();
				Mesh[] array2 = Resources.FindObjectsOfTypeAll<Mesh>();
				Material[] array3 = Resources.FindObjectsOfTypeAll<Material>();
				AudioClip[] array4 = Resources.FindObjectsOfTypeAll<AudioClip>();
				GameObject[] array5 = Resources.FindObjectsOfTypeAll<GameObject>();
				int num5 = 0;
				foreach (Texture texture in array)
				{
					num5 += texture.width * texture.height;
				}
				int num6 = 0;
				int num7 = 0;
				foreach (Mesh mesh in array2)
				{
					num6 += mesh.vertexCount;
					num7 += mesh.triangles.Length;
				}
				num7 /= 3;
				OnScreenLog.AddLogItem("Game Objects: " + array5.Length, 20f, true);
				OnScreenLog.AddLogItem("Audio Clips: " + array4.Length, 20f, true);
				OnScreenLog.AddLogItem("Material Count: " + array3.Length, 20f, true);
				OnScreenLog.AddLogItem("Mesh Triangle Count: " + num7, 20f, true);
				OnScreenLog.AddLogItem("Mesh Vertex Count: " + num6, 20f, true);
				OnScreenLog.AddLogItem("Mesh Count: " + array2.Length, 20f, true);
				OnScreenLog.AddLogItem("Texture Pixel Count: " + num5, 20f, true);
				OnScreenLog.AddLogItem("Texture Count: " + array.Length, 20f, true);
			};
			secretCommand.canExecute = (() => true);
			SecretCommandGesture.SecretCommand secretCommand3 = secretCommand;
			this.commands.Add(secretCommand3);
			if (BW.isUnityEditor && Options.UseSimpleAutoPlayTrigger)
			{
				secretCommand3.rects = new Rect[]
				{
					rect2,
					rect2,
					rect2,
					rect2,
					rect2
				};
			}
			else
			{
				secretCommand3.rects = new Rect[]
				{
					rect3,
					rect4,
					rect3,
					rect
				};
			}
		}

		// Token: 0x0600163F RID: 5695 RVA: 0x0009D238 File Offset: 0x0009B638
		public override void TouchesBegan(List<Touch> allTouches)
		{
			bool flag = true;
			for (int i = 0; i < allTouches.Count; i++)
			{
				Touch touch = allTouches[i];
				if (touch.Command == null)
				{
					flag = false;
					break;
				}
			}
			if (!flag && this.autoPlayer.IsActive())
			{
				this.autoPlayer.StopAutoPlay();
				return;
			}
			if (this.autoPlayer.IsActive())
			{
				return;
			}
			if (allTouches.Count > 1)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches[0].Phase != TouchPhase.Began)
			{
				return;
			}
			Touch touch2 = allTouches[0];
			float time = Time.time;
			for (int j = 0; j < this.commands.Count; j++)
			{
				SecretCommandGesture.SecretCommand secretCommand = this.commands[j];
				if (secretCommand.canExecute())
				{
					float num = time - secretCommand.lastHitTime;
					if (num > 0.5f)
					{
						secretCommand.currentIndex = 0;
					}
					Rect rect = secretCommand.rects[secretCommand.currentIndex];
					if (rect.Contains(touch2.Position))
					{
						secretCommand.currentIndex++;
						secretCommand.lastHitTime = time;
						if (secretCommand.currentIndex == secretCommand.rects.Length)
						{
							secretCommand.action();
							secretCommand.currentIndex = 0;
						}
					}
				}
			}
		}

		// Token: 0x06001640 RID: 5696 RVA: 0x0009D3B4 File Offset: 0x0009B7B4
		public override void Reset()
		{
			for (int i = 0; i < this.commands.Count; i++)
			{
				this.commands[i].currentIndex = 0;
			}
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x04001155 RID: 4437
		private List<SecretCommandGesture.SecretCommand> commands = new List<SecretCommandGesture.SecretCommand>();

		// Token: 0x04001156 RID: 4438
		private StepByStepAutoPlayer autoPlayer = new StepByStepAutoPlayer();

		// Token: 0x02000187 RID: 391
		private class SecretCommand
		{
			// Token: 0x0400115A RID: 4442
			public Rect[] rects;

			// Token: 0x0400115B RID: 4443
			public Action action;

			// Token: 0x0400115C RID: 4444
			public Func<bool> canExecute;

			// Token: 0x0400115D RID: 4445
			public float lastHitTime = -1f;

			// Token: 0x0400115E RID: 4446
			public int currentIndex;
		}
	}
}
