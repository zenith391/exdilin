using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class BaseVRDevice
{
	public struct DisplayMetrics
	{
		public int width;

		public int height;

		public float xdpi;

		public float ydpi;
	}

	private static BaseVRDevice device;

	protected MutablePose3D headPose = new MutablePose3D();

	protected MutablePose3D leftEyePose = new MutablePose3D();

	protected MutablePose3D rightEyePose = new MutablePose3D();

	protected Matrix4x4 leftEyeDistortedProjection;

	protected Matrix4x4 rightEyeDistortedProjection;

	protected Matrix4x4 leftEyeUndistortedProjection;

	protected Matrix4x4 rightEyeUndistortedProjection;

	protected Rect leftEyeDistortedViewport;

	protected Rect rightEyeDistortedViewport;

	protected Rect leftEyeUndistortedViewport;

	protected Rect rightEyeUndistortedViewport;

	protected Vector2 recommendedTextureSize;

	public bool triggered;

	public bool tilted;

	public bool profileChanged;

	public bool backButtonPressed;

	public CardboardProfile Profile { get; protected set; }

	protected BaseVRDevice()
	{
		Profile = CardboardProfile.Default.Clone();
	}

	public abstract void Init();

	public abstract void SetUILayerEnabled(bool enabled);

	public abstract void SetVRModeEnabled(bool enabled);

	public abstract void SetDistortionCorrectionEnabled(bool enabled);

	public abstract void SetStereoScreen(RenderTexture stereoScreen);

	public abstract void SetSettingsButtonEnabled(bool enabled);

	public abstract void SetAlignmentMarkerEnabled(bool enabled);

	public abstract void SetVRBackButtonEnabled(bool enabled);

	public abstract void SetShowVrBackButtonOnlyInVR(bool only);

	public abstract void SetTapIsTrigger(bool enabled);

	public abstract void SetNeckModelScale(float scale);

	public abstract void SetAutoDriftCorrectionEnabled(bool enabled);

	public abstract void SetElectronicDisplayStabilizationEnabled(bool enabled);

	public virtual DisplayMetrics GetDisplayMetrics()
	{
		int width = Mathf.Max(Screen.width, Screen.height);
		int height = Mathf.Min(Screen.width, Screen.height);
		return new DisplayMetrics
		{
			width = width,
			height = height,
			xdpi = Screen.dpi,
			ydpi = Screen.dpi
		};
	}

	public virtual bool SupportsNativeDistortionCorrection(List<string> diagnostics)
	{
		bool result = true;
		if (!SystemInfo.supportsRenderTextures)
		{
			diagnostics.Add("RenderTexture (Unity Pro feature) is unavailable");
			result = false;
		}
		if (!SupportsUnityRenderEvent())
		{
			diagnostics.Add("Unity 4.5+ is needed for UnityRenderEvent");
			result = false;
		}
		return result;
	}

	public virtual bool SupportsNativeUILayer(List<string> diagnostics)
	{
		return true;
	}

	public bool SupportsUnityRenderEvent()
	{
		bool result = true;
		if (Application.isMobilePlatform)
		{
			try
			{
				string version = new Regex("(\\d+\\.\\d+)\\..*").Replace(Application.unityVersion, "$1");
				if (new Version(version) < new Version("4.5"))
				{
					result = false;
				}
			}
			catch
			{
				Debug.LogWarning("Unable to determine Unity version from: " + Application.unityVersion);
			}
		}
		return result;
	}

	public virtual RenderTexture CreateStereoScreen()
	{
		float stereoScreenScale = Cardboard.SDK.StereoScreenScale;
		int width = Mathf.RoundToInt((float)Screen.width * stereoScreenScale);
		int height = Mathf.RoundToInt((float)Screen.height * stereoScreenScale);
		return new RenderTexture(width, height, 24, RenderTextureFormat.Default)
		{
			anisoLevel = 0,
			antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 1)
		};
	}

	public virtual bool SetDefaultDeviceProfile(Uri uri)
	{
		return false;
	}

	public virtual void ShowSettingsDialog()
	{
	}

	public Pose3D GetHeadPose()
	{
		return headPose;
	}

	public Pose3D GetEyePose(Cardboard.Eye eye)
	{
		return eye switch
		{
			Cardboard.Eye.Left => leftEyePose, 
			Cardboard.Eye.Right => rightEyePose, 
			_ => null, 
		};
	}

	public Matrix4x4 GetProjection(Cardboard.Eye eye, Cardboard.Distortion distortion = Cardboard.Distortion.Distorted)
	{
		switch (eye)
		{
		case Cardboard.Eye.Left:
			if (distortion == Cardboard.Distortion.Distorted)
			{
				return leftEyeDistortedProjection;
			}
			return leftEyeUndistortedProjection;
		default:
			return Matrix4x4.identity;
		case Cardboard.Eye.Right:
			if (distortion == Cardboard.Distortion.Distorted)
			{
				return rightEyeDistortedProjection;
			}
			return rightEyeUndistortedProjection;
		}
	}

	public Rect GetViewport(Cardboard.Eye eye, Cardboard.Distortion distortion = Cardboard.Distortion.Distorted)
	{
		switch (eye)
		{
		case Cardboard.Eye.Left:
			if (distortion == Cardboard.Distortion.Distorted)
			{
				return leftEyeDistortedViewport;
			}
			return leftEyeUndistortedViewport;
		default:
			return default(Rect);
		case Cardboard.Eye.Right:
			if (distortion == Cardboard.Distortion.Distorted)
			{
				return rightEyeDistortedViewport;
			}
			return rightEyeUndistortedViewport;
		}
	}

	public abstract void UpdateState();

	public abstract void UpdateScreenData();

	public abstract void Recenter();

	public abstract void PostRender();

	public virtual void SetTouchCoordinates(int x, int y)
	{
	}

	public virtual void OnPause(bool pause)
	{
		if (!pause)
		{
			UpdateScreenData();
		}
	}

	public virtual void OnFocus(bool focus)
	{
	}

	public virtual void OnLevelLoaded(int level)
	{
	}

	public virtual void OnApplicationQuit()
	{
	}

	public virtual void Destroy()
	{
		if (device == this)
		{
			device = null;
		}
	}

	protected void ComputeEyesFromProfile()
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity[0, 3] = (0f - Profile.device.lenses.separation) / 2f;
		leftEyePose.Set(identity);
		float[] array = new float[4];
		Profile.GetLeftEyeVisibleTanAngles(array);
		leftEyeDistortedProjection = MakeProjection(array[0], array[1], array[2], array[3], 1f, 1000f);
		Profile.GetLeftEyeNoLensTanAngles(array);
		leftEyeUndistortedProjection = MakeProjection(array[0], array[1], array[2], array[3], 1f, 1000f);
		leftEyeUndistortedViewport = Profile.GetLeftEyeVisibleScreenRect(array);
		leftEyeDistortedViewport = leftEyeUndistortedViewport;
		Matrix4x4 matrix = identity;
		matrix[0, 3] *= -1f;
		rightEyePose.Set(matrix);
		rightEyeDistortedProjection = leftEyeDistortedProjection;
		ref Matrix4x4 reference = ref rightEyeDistortedProjection;
		rightEyeDistortedProjection[0, 2] = reference[0, 2] * -1f;
		rightEyeUndistortedProjection = leftEyeUndistortedProjection;
		reference = ref rightEyeUndistortedProjection;
		rightEyeUndistortedProjection[0, 2] = reference[0, 2] * -1f;
		rightEyeUndistortedViewport = leftEyeUndistortedViewport;
		rightEyeUndistortedViewport.x = 1f - rightEyeUndistortedViewport.xMax;
		rightEyeDistortedViewport = rightEyeUndistortedViewport;
		float x = (float)Screen.width * (leftEyeUndistortedViewport.width + rightEyeDistortedViewport.width);
		float y = (float)Screen.height * Mathf.Max(leftEyeUndistortedViewport.height, rightEyeUndistortedViewport.height);
		recommendedTextureSize = new Vector2(x, y);
	}

	private static Matrix4x4 MakeProjection(float l, float t, float r, float b, float n, float f)
	{
		Matrix4x4 zero = Matrix4x4.zero;
		zero[0, 0] = 2f * n / (r - l);
		zero[1, 1] = 2f * n / (t - b);
		zero[0, 2] = (r + l) / (r - l);
		zero[1, 2] = (t + b) / (t - b);
		zero[2, 2] = (n + f) / (n - f);
		zero[2, 3] = 2f * n * f / (n - f);
		zero[3, 2] = -1f;
		return zero;
	}

	public static BaseVRDevice GetDevice()
	{
		if (device == null)
		{
			throw new InvalidOperationException("Unsupported device.");
		}
		return device;
	}
}
