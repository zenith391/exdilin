using System;
using UnityEngine;

// Token: 0x02000327 RID: 807
public class ViewportWatchdog : MonoBehaviour
{
	// Token: 0x14000011 RID: 17
	// (add) Token: 0x0600248A RID: 9354 RVA: 0x0010B0E8 File Offset: 0x001094E8
	// (remove) Token: 0x0600248B RID: 9355 RVA: 0x0010B120 File Offset: 0x00109520
	public event ViewportWatchdog.ViewportSizeChangedAction OnViewportSizeChanged;

	// Token: 0x0600248C RID: 9356 RVA: 0x0010B158 File Offset: 0x00109558
	public static void StartWatching()
	{
		if (BW.Options.fixedScreenSize())
		{
			return;
		}
		if (ViewportWatchdog.watchdog == null)
		{
			ViewportWatchdog.go = new GameObject("ViewportSizeWatchdog");
			ViewportWatchdog.watchdog = ViewportWatchdog.go.AddComponent<ViewportWatchdog>();
		}
		ViewportWatchdog.watchdog.enabled = true;
	}

	// Token: 0x0600248D RID: 9357 RVA: 0x0010B1AE File Offset: 0x001095AE
	public static void StopWatching()
	{
		if (BW.Options.fixedScreenSize())
		{
			return;
		}
		ViewportWatchdog.watchdog.enabled = false;
	}

	// Token: 0x0600248E RID: 9358 RVA: 0x0010B1CB File Offset: 0x001095CB
	public static void AddListener(ViewportWatchdog.ViewportSizeChangedAction action)
	{
		if (BW.Options.fixedScreenSize())
		{
			return;
		}
		if (ViewportWatchdog.watchdog == null)
		{
			ViewportWatchdog.StartWatching();
		}
		ViewportWatchdog.watchdog.OnViewportSizeChanged += action;
	}

	// Token: 0x0600248F RID: 9359 RVA: 0x0010B1FD File Offset: 0x001095FD
	public static void RemoveListener(ViewportWatchdog.ViewportSizeChangedAction action)
	{
		if (ViewportWatchdog.watchdog == null)
		{
			return;
		}
		ViewportWatchdog.watchdog.OnViewportSizeChanged -= action;
	}

	// Token: 0x06002490 RID: 9360 RVA: 0x0010B21C File Offset: 0x0010961C
	private void Update()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (width != this.currentWidth || height != this.currentHeight)
		{
			this.currentWidth = width;
			this.currentHeight = height;
			this.SendViewportSizeChangeEvent();
		}
	}

	// Token: 0x06002491 RID: 9361 RVA: 0x0010B261 File Offset: 0x00109661
	private void SendViewportSizeChangeEvent()
	{
		if (this.OnViewportSizeChanged != null)
		{
			this.OnViewportSizeChanged();
		}
	}

	// Token: 0x04001F6B RID: 8043
	private int currentWidth;

	// Token: 0x04001F6C RID: 8044
	private int currentHeight;

	// Token: 0x04001F6D RID: 8045
	private static GameObject go;

	// Token: 0x04001F6E RID: 8046
	private static ViewportWatchdog watchdog;

	// Token: 0x02000328 RID: 808
	// (Invoke) Token: 0x06002493 RID: 9363
	public delegate void ViewportSizeChangedAction();
}
