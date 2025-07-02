using UnityEngine;

public class ViewportWatchdog : MonoBehaviour
{
	public delegate void ViewportSizeChangedAction();

	private int currentWidth;

	private int currentHeight;

	private static GameObject go;

	private static ViewportWatchdog watchdog;

	public event ViewportSizeChangedAction OnViewportSizeChanged;

	public static void StartWatching()
	{
		if (!BW.Options.fixedScreenSize())
		{
			if (watchdog == null)
			{
				go = new GameObject("ViewportSizeWatchdog");
				watchdog = go.AddComponent<ViewportWatchdog>();
			}
			watchdog.enabled = true;
		}
	}

	public static void StopWatching()
	{
		if (!BW.Options.fixedScreenSize())
		{
			watchdog.enabled = false;
		}
	}

	public static void AddListener(ViewportSizeChangedAction action)
	{
		if (!BW.Options.fixedScreenSize())
		{
			if (watchdog == null)
			{
				StartWatching();
			}
			watchdog.OnViewportSizeChanged += action;
		}
	}

	public static void RemoveListener(ViewportSizeChangedAction action)
	{
		if (!(watchdog == null))
		{
			watchdog.OnViewportSizeChanged -= action;
		}
	}

	private void Update()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (width != currentWidth || height != currentHeight)
		{
			currentWidth = width;
			currentHeight = height;
			SendViewportSizeChangeEvent();
		}
	}

	private void SendViewportSizeChangeEvent()
	{
		if (this.OnViewportSizeChanged != null)
		{
			this.OnViewportSizeChanged();
		}
	}
}
