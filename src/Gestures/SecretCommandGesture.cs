using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gestures;

public class SecretCommandGesture : BaseGesture
{
	private class SecretCommand
	{
		public Rect[] rects;

		public Action action;

		public Func<bool> canExecute;

		public float lastHitTime = -1f;

		public int currentIndex;
	}

	private List<SecretCommand> commands = new List<SecretCommand>();

	private StepByStepAutoPlayer autoPlayer = new StepByStepAutoPlayer();

	public SecretCommandGesture()
	{
		float num = NormalizedScreen.width;
		float num2 = NormalizedScreen.height;
		float num3 = num * 0.1f;
		float num4 = num2 * 0.1f;
		Rect rect = new Rect(0f, num2 - num4, num, num4);
		Rect rect2 = new Rect(0f, 0f, num, num4);
		Rect rect3 = new Rect(0f, 0f, num3, num2);
		Rect rect4 = new Rect(num - num3, 0f, num3, num2);
		SecretCommand secretCommand = new SecretCommand
		{
			action = delegate
			{
				if (!autoPlayer.IsActive())
				{
					autoPlayer.StartAutoPlay();
					BW.Analytics.SendAnalyticsEvent("world-autoplay", string.Empty);
					OnScreenLog.AddLogItem("Starting auto play. Tap anywhere to stop.", 5f, log: true);
				}
			},
			canExecute = () => Blocksworld.CurrentState == State.Build && Tutorial.state != TutorialState.None
		};
		if (BW.isUnityEditor && Options.UseSimpleAutoPlayTrigger)
		{
			secretCommand.rects = new Rect[5] { rect, rect, rect, rect, rect };
		}
		else
		{
			secretCommand.rects = new Rect[4] { rect, rect2, rect3, rect4 };
		}
		commands.Add(secretCommand);
		SecretCommand secretCommand2 = new SecretCommand
		{
			action = delegate
			{
				Texture[] array = Resources.FindObjectsOfTypeAll<Texture>();
				Mesh[] array2 = Resources.FindObjectsOfTypeAll<Mesh>();
				Material[] array3 = Resources.FindObjectsOfTypeAll<Material>();
				AudioClip[] array4 = Resources.FindObjectsOfTypeAll<AudioClip>();
				GameObject[] array5 = Resources.FindObjectsOfTypeAll<GameObject>();
				int num5 = 0;
				Texture[] array6 = array;
				foreach (Texture texture in array6)
				{
					num5 += texture.width * texture.height;
				}
				int num6 = 0;
				int num7 = 0;
				Mesh[] array7 = array2;
				foreach (Mesh mesh in array7)
				{
					num6 += mesh.vertexCount;
					num7 += mesh.triangles.Length;
				}
				num7 /= 3;
				OnScreenLog.AddLogItem("Game Objects: " + array5.Length, 20f, log: true);
				OnScreenLog.AddLogItem("Audio Clips: " + array4.Length, 20f, log: true);
				OnScreenLog.AddLogItem("Material Count: " + array3.Length, 20f, log: true);
				OnScreenLog.AddLogItem("Mesh Triangle Count: " + num7, 20f, log: true);
				OnScreenLog.AddLogItem("Mesh Vertex Count: " + num6, 20f, log: true);
				OnScreenLog.AddLogItem("Mesh Count: " + array2.Length, 20f, log: true);
				OnScreenLog.AddLogItem("Texture Pixel Count: " + num5, 20f, log: true);
				OnScreenLog.AddLogItem("Texture Count: " + array.Length, 20f, log: true);
			},
			canExecute = () => true
		};
		commands.Add(secretCommand2);
		if (BW.isUnityEditor && Options.UseSimpleAutoPlayTrigger)
		{
			secretCommand2.rects = new Rect[5] { rect2, rect2, rect2, rect2, rect2 };
		}
		else
		{
			secretCommand2.rects = new Rect[4] { rect3, rect4, rect3, rect };
		}
	}

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
		if (!flag && autoPlayer.IsActive())
		{
			autoPlayer.StopAutoPlay();
		}
		else
		{
			if (autoPlayer.IsActive())
			{
				return;
			}
			if (allTouches.Count > 1)
			{
				EnterState(GestureState.Failed);
			}
			else
			{
				if (allTouches[0].Phase != TouchPhase.Began)
				{
					return;
				}
				Touch touch2 = allTouches[0];
				float time = Time.time;
				for (int j = 0; j < commands.Count; j++)
				{
					SecretCommand secretCommand = commands[j];
					if (!secretCommand.canExecute())
					{
						continue;
					}
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
	}

	public override void Reset()
	{
		for (int i = 0; i < commands.Count; i++)
		{
			commands[i].currentIndex = 0;
		}
		EnterState(GestureState.Possible);
	}
}
