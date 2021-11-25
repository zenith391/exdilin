using System;
using UnityEngine;

// Token: 0x02000259 RID: 601
public static class NormalizedScreen
{
	// Token: 0x17000083 RID: 131
	// (get) Token: 0x06001B42 RID: 6978 RVA: 0x000C6821 File Offset: 0x000C4C21
	public static float devicePixelScale
	{
		get
		{
			return BW.Options.GetScreenScale();
		}
	}

	// Token: 0x17000084 RID: 132
	// (get) Token: 0x06001B43 RID: 6979 RVA: 0x000C682D File Offset: 0x000C4C2D
	public static float scale
	{
		get
		{
			return (NormalizedScreen.devicePixelScale <= 1f) ? 1f : 2f;
		}
	}

	// Token: 0x17000085 RID: 133
	// (get) Token: 0x06001B44 RID: 6980 RVA: 0x000C684D File Offset: 0x000C4C4D
	public static Vector2 referenceResolution
	{
		get
		{
			return NormalizedScreen._referenceResolution;
		}
	}

	// Token: 0x17000086 RID: 134
	// (get) Token: 0x06001B45 RID: 6981 RVA: 0x000C6854 File Offset: 0x000C4C54
	public static int width
	{
		get
		{
			return (int)((float)Screen.width / NormalizedScreen.scale);
		}
	}

	// Token: 0x17000087 RID: 135
	// (get) Token: 0x06001B46 RID: 6982 RVA: 0x000C6863 File Offset: 0x000C4C63
	public static int height
	{
		get
		{
			return (int)((float)Screen.height / NormalizedScreen.scale);
		}
	}

	// Token: 0x17000088 RID: 136
	// (get) Token: 0x06001B47 RID: 6983 RVA: 0x000C6874 File Offset: 0x000C4C74
	public static float aspectRatio
	{
		get
		{
			if (Screen.width == 0 || Screen.height == 0)
			{
				BWLog.Info("Invalid screen size, using default aspect ratio");
				return NormalizedScreen.defaultScreenSize.x / NormalizedScreen.defaultScreenSize.y;
			}
			return (float)Screen.width / (float)Screen.height;
		}
	}

	// Token: 0x17000089 RID: 137
	// (get) Token: 0x06001B48 RID: 6984 RVA: 0x000C68CC File Offset: 0x000C4CCC
	public static float DPI
	{
		get
		{
			float dpi = Screen.dpi;
			if (dpi == 0f)
			{
				dpi = NormalizedScreen.defaultDPI;
			}
			return dpi * NormalizedScreen.devicePixelScale;
		}
	}

	// Token: 0x1700008A RID: 138
	// (get) Token: 0x06001B49 RID: 6985 RVA: 0x000C68F7 File Offset: 0x000C4CF7
	public static float physicalScreenWidth
	{
		get
		{
			return (float)Screen.width / NormalizedScreen.DPI;
		}
	}

	// Token: 0x1700008B RID: 139
	// (get) Token: 0x06001B4A RID: 6986 RVA: 0x000C6908 File Offset: 0x000C4D08
	private static float defaultScreenWidth
	{
		get
		{
			return NormalizedScreen.defaultScreenSize.x / NormalizedScreen.defaultDPI;
		}
	}

	// Token: 0x1700008C RID: 140
	// (get) Token: 0x06001B4B RID: 6987 RVA: 0x000C6928 File Offset: 0x000C4D28
	public static float physicalScale
	{
		get
		{
			return NormalizedScreen.physicalScreenWidth / NormalizedScreen.defaultScreenWidth;
		}
	}

	// Token: 0x1700008D RID: 141
	// (get) Token: 0x06001B4C RID: 6988 RVA: 0x000C6938 File Offset: 0x000C4D38
	public static float pixelScale
	{
		get
		{
			return ((float)Screen.width < NormalizedScreen.referenceResolution.x) ? Mathf.Max(0.5f, (float)Screen.width / NormalizedScreen.referenceResolution.x) : 1f;
		}
	}

	// Token: 0x1700008E RID: 142
	// (get) Token: 0x06001B4D RID: 6989 RVA: 0x000C6988 File Offset: 0x000C4D88
	public static float widthScaleRatio
	{
		get
		{
			return (float)Screen.width / NormalizedScreen.defaultScreenSize.x;
		}
	}

	// Token: 0x1700008F RID: 143
	// (get) Token: 0x06001B4E RID: 6990 RVA: 0x000C69AC File Offset: 0x000C4DAC
	public static float heightScaleRatio
	{
		get
		{
			return (float)Screen.height / NormalizedScreen.defaultScreenSize.y;
		}
	}

	// Token: 0x0400172F RID: 5935
	private static readonly Vector2 defaultScreenSize = new Vector2(1024f, 768f);

	// Token: 0x04001730 RID: 5936
	private static readonly float defaultDPI = 132f;

	// Token: 0x04001731 RID: 5937
	private static Vector2 _referenceResolution = NormalizedScreen.defaultScreenSize * NormalizedScreen.scale;
}
