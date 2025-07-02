using UnityEngine;

public static class NormalizedScreen
{
	private static readonly Vector2 defaultScreenSize = new Vector2(1024f, 768f);

	private static readonly float defaultDPI = 132f;

	private static Vector2 _referenceResolution = defaultScreenSize * scale;

	public static float devicePixelScale => BW.Options.GetScreenScale();

	public static float scale
	{
		get
		{
			if (!(devicePixelScale <= 1f))
			{
				return 2f;
			}
			return 1f;
		}
	}

	public static Vector2 referenceResolution => _referenceResolution;

	public static int width => (int)((float)Screen.width / scale);

	public static int height => (int)((float)Screen.height / scale);

	public static float aspectRatio
	{
		get
		{
			if (Screen.width == 0 || Screen.height == 0)
			{
				BWLog.Info("Invalid screen size, using default aspect ratio");
				return defaultScreenSize.x / defaultScreenSize.y;
			}
			return (float)Screen.width / (float)Screen.height;
		}
	}

	public static float DPI
	{
		get
		{
			float dpi = Screen.dpi;
			if (dpi == 0f)
			{
				dpi = defaultDPI;
			}
			return dpi * devicePixelScale;
		}
	}

	public static float physicalScreenWidth => (float)Screen.width / DPI;

	private static float defaultScreenWidth => defaultScreenSize.x / defaultDPI;

	public static float physicalScale => physicalScreenWidth / defaultScreenWidth;

	public static float pixelScale
	{
		get
		{
			if (!((float)Screen.width < referenceResolution.x))
			{
				return 1f;
			}
			return Mathf.Max(0.5f, (float)Screen.width / referenceResolution.x);
		}
	}

	public static float widthScaleRatio => (float)Screen.width / defaultScreenSize.x;

	public static float heightScaleRatio => (float)Screen.height / defaultScreenSize.y;
}
