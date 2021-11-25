using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Token: 0x0200002D RID: 45
public abstract class BaseVRDevice
{
	// Token: 0x06000191 RID: 401 RVA: 0x000092ED File Offset: 0x000076ED
	protected BaseVRDevice()
	{
		this.Profile = CardboardProfile.Default.Clone();
	}

	// Token: 0x1700003A RID: 58
	// (get) Token: 0x06000192 RID: 402 RVA: 0x00009326 File Offset: 0x00007726
	// (set) Token: 0x06000193 RID: 403 RVA: 0x0000932E File Offset: 0x0000772E
	public CardboardProfile Profile { get; protected set; }

	// Token: 0x06000194 RID: 404
	public abstract void Init();

	// Token: 0x06000195 RID: 405
	public abstract void SetUILayerEnabled(bool enabled);

	// Token: 0x06000196 RID: 406
	public abstract void SetVRModeEnabled(bool enabled);

	// Token: 0x06000197 RID: 407
	public abstract void SetDistortionCorrectionEnabled(bool enabled);

	// Token: 0x06000198 RID: 408
	public abstract void SetStereoScreen(RenderTexture stereoScreen);

	// Token: 0x06000199 RID: 409
	public abstract void SetSettingsButtonEnabled(bool enabled);

	// Token: 0x0600019A RID: 410
	public abstract void SetAlignmentMarkerEnabled(bool enabled);

	// Token: 0x0600019B RID: 411
	public abstract void SetVRBackButtonEnabled(bool enabled);

	// Token: 0x0600019C RID: 412
	public abstract void SetShowVrBackButtonOnlyInVR(bool only);

	// Token: 0x0600019D RID: 413
	public abstract void SetTapIsTrigger(bool enabled);

	// Token: 0x0600019E RID: 414
	public abstract void SetNeckModelScale(float scale);

	// Token: 0x0600019F RID: 415
	public abstract void SetAutoDriftCorrectionEnabled(bool enabled);

	// Token: 0x060001A0 RID: 416
	public abstract void SetElectronicDisplayStabilizationEnabled(bool enabled);

	// Token: 0x060001A1 RID: 417 RVA: 0x00009338 File Offset: 0x00007738
	public virtual BaseVRDevice.DisplayMetrics GetDisplayMetrics()
	{
		int width = Mathf.Max(Screen.width, Screen.height);
		int height = Mathf.Min(Screen.width, Screen.height);
		return new BaseVRDevice.DisplayMetrics
		{
			width = width,
			height = height,
			xdpi = Screen.dpi,
			ydpi = Screen.dpi
		};
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x00009398 File Offset: 0x00007798
	public virtual bool SupportsNativeDistortionCorrection(List<string> diagnostics)
	{
		bool result = true;
		if (!SystemInfo.supportsRenderTextures)
		{
			diagnostics.Add("RenderTexture (Unity Pro feature) is unavailable");
			result = false;
		}
		if (!this.SupportsUnityRenderEvent())
		{
			diagnostics.Add("Unity 4.5+ is needed for UnityRenderEvent");
			result = false;
		}
		return result;
	}

	// Token: 0x060001A3 RID: 419 RVA: 0x000093D7 File Offset: 0x000077D7
	public virtual bool SupportsNativeUILayer(List<string> diagnostics)
	{
		return true;
	}

	// Token: 0x060001A4 RID: 420 RVA: 0x000093DC File Offset: 0x000077DC
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

	// Token: 0x060001A5 RID: 421 RVA: 0x0000945C File Offset: 0x0000785C
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

	// Token: 0x060001A6 RID: 422 RVA: 0x000094B4 File Offset: 0x000078B4
	public virtual bool SetDefaultDeviceProfile(Uri uri)
	{
		return false;
	}

	// Token: 0x060001A7 RID: 423 RVA: 0x000094B7 File Offset: 0x000078B7
	public virtual void ShowSettingsDialog()
	{
	}

	// Token: 0x060001A8 RID: 424 RVA: 0x000094B9 File Offset: 0x000078B9
	public Pose3D GetHeadPose()
	{
		return this.headPose;
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x000094C1 File Offset: 0x000078C1
	public Pose3D GetEyePose(Cardboard.Eye eye)
	{
		if (eye == Cardboard.Eye.Left)
		{
			return this.leftEyePose;
		}
		if (eye != Cardboard.Eye.Right)
		{
			return null;
		}
		return this.rightEyePose;
	}

	// Token: 0x060001AA RID: 426 RVA: 0x000094E4 File Offset: 0x000078E4
	public Matrix4x4 GetProjection(Cardboard.Eye eye, Cardboard.Distortion distortion = Cardboard.Distortion.Distorted)
	{
		if (eye == Cardboard.Eye.Left)
		{
			return (distortion != Cardboard.Distortion.Distorted) ? this.leftEyeUndistortedProjection : this.leftEyeDistortedProjection;
		}
		if (eye != Cardboard.Eye.Right)
		{
			return Matrix4x4.identity;
		}
		return (distortion != Cardboard.Distortion.Distorted) ? this.rightEyeUndistortedProjection : this.rightEyeDistortedProjection;
	}

	// Token: 0x060001AB RID: 427 RVA: 0x00009538 File Offset: 0x00007938
	public Rect GetViewport(Cardboard.Eye eye, Cardboard.Distortion distortion = Cardboard.Distortion.Distorted)
	{
		if (eye == Cardboard.Eye.Left)
		{
			return (distortion != Cardboard.Distortion.Distorted) ? this.leftEyeUndistortedViewport : this.leftEyeDistortedViewport;
		}
		if (eye != Cardboard.Eye.Right)
		{
			return default(Rect);
		}
		return (distortion != Cardboard.Distortion.Distorted) ? this.rightEyeUndistortedViewport : this.rightEyeDistortedViewport;
	}

	// Token: 0x060001AC RID: 428
	public abstract void UpdateState();

	// Token: 0x060001AD RID: 429
	public abstract void UpdateScreenData();

	// Token: 0x060001AE RID: 430
	public abstract void Recenter();

	// Token: 0x060001AF RID: 431
	public abstract void PostRender();

	// Token: 0x060001B0 RID: 432 RVA: 0x00009590 File Offset: 0x00007990
	public virtual void SetTouchCoordinates(int x, int y)
	{
	}

	// Token: 0x060001B1 RID: 433 RVA: 0x00009592 File Offset: 0x00007992
	public virtual void OnPause(bool pause)
	{
		if (!pause)
		{
			this.UpdateScreenData();
		}
	}

	// Token: 0x060001B2 RID: 434 RVA: 0x000095A0 File Offset: 0x000079A0
	public virtual void OnFocus(bool focus)
	{
	}

	// Token: 0x060001B3 RID: 435 RVA: 0x000095A2 File Offset: 0x000079A2
	public virtual void OnLevelLoaded(int level)
	{
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x000095A4 File Offset: 0x000079A4
	public virtual void OnApplicationQuit()
	{
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x000095A6 File Offset: 0x000079A6
	public virtual void Destroy()
	{
		if (BaseVRDevice.device == this)
		{
			BaseVRDevice.device = null;
		}
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x000095BC File Offset: 0x000079BC
	protected void ComputeEyesFromProfile()
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity[0, 3] = -this.Profile.device.lenses.separation / 2f;
		this.leftEyePose.Set(identity);
		float[] array = new float[4];
		this.Profile.GetLeftEyeVisibleTanAngles(array);
		this.leftEyeDistortedProjection = BaseVRDevice.MakeProjection(array[0], array[1], array[2], array[3], 1f, 1000f);
		this.Profile.GetLeftEyeNoLensTanAngles(array);
		this.leftEyeUndistortedProjection = BaseVRDevice.MakeProjection(array[0], array[1], array[2], array[3], 1f, 1000f);
		this.leftEyeUndistortedViewport = this.Profile.GetLeftEyeVisibleScreenRect(array);
		this.leftEyeDistortedViewport = this.leftEyeUndistortedViewport;
		Matrix4x4 matrix = identity;
		ref Matrix4x4 ptr = ref matrix;
		matrix[0, 3] = ptr[0, 3] * -1f;
		this.rightEyePose.Set(matrix);
		this.rightEyeDistortedProjection = this.leftEyeDistortedProjection;
		ptr = ref this.rightEyeDistortedProjection;
		this.rightEyeDistortedProjection[0, 2] = ptr[0, 2] * -1f;
		this.rightEyeUndistortedProjection = this.leftEyeUndistortedProjection;
		ptr = ref this.rightEyeUndistortedProjection;
		this.rightEyeUndistortedProjection[0, 2] = ptr[0, 2] * -1f;
		this.rightEyeUndistortedViewport = this.leftEyeUndistortedViewport;
		this.rightEyeUndistortedViewport.x = 1f - this.rightEyeUndistortedViewport.xMax;
		this.rightEyeDistortedViewport = this.rightEyeUndistortedViewport;
		float x = (float)Screen.width * (this.leftEyeUndistortedViewport.width + this.rightEyeDistortedViewport.width);
		float y = (float)Screen.height * Mathf.Max(this.leftEyeUndistortedViewport.height, this.rightEyeUndistortedViewport.height);
		this.recommendedTextureSize = new Vector2(x, y);
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x00009780 File Offset: 0x00007B80
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

	// Token: 0x060001B8 RID: 440 RVA: 0x0000981A File Offset: 0x00007C1A
	public static BaseVRDevice GetDevice()
	{
		if (BaseVRDevice.device == null)
		{
			throw new InvalidOperationException("Unsupported device.");
		}
		return BaseVRDevice.device;
	}

	// Token: 0x040001AC RID: 428
	private static BaseVRDevice device;

	// Token: 0x040001AE RID: 430
	protected MutablePose3D headPose = new MutablePose3D();

	// Token: 0x040001AF RID: 431
	protected MutablePose3D leftEyePose = new MutablePose3D();

	// Token: 0x040001B0 RID: 432
	protected MutablePose3D rightEyePose = new MutablePose3D();

	// Token: 0x040001B1 RID: 433
	protected Matrix4x4 leftEyeDistortedProjection;

	// Token: 0x040001B2 RID: 434
	protected Matrix4x4 rightEyeDistortedProjection;

	// Token: 0x040001B3 RID: 435
	protected Matrix4x4 leftEyeUndistortedProjection;

	// Token: 0x040001B4 RID: 436
	protected Matrix4x4 rightEyeUndistortedProjection;

	// Token: 0x040001B5 RID: 437
	protected Rect leftEyeDistortedViewport;

	// Token: 0x040001B6 RID: 438
	protected Rect rightEyeDistortedViewport;

	// Token: 0x040001B7 RID: 439
	protected Rect leftEyeUndistortedViewport;

	// Token: 0x040001B8 RID: 440
	protected Rect rightEyeUndistortedViewport;

	// Token: 0x040001B9 RID: 441
	protected Vector2 recommendedTextureSize;

	// Token: 0x040001BA RID: 442
	public bool triggered;

	// Token: 0x040001BB RID: 443
	public bool tilted;

	// Token: 0x040001BC RID: 444
	public bool profileChanged;

	// Token: 0x040001BD RID: 445
	public bool backButtonPressed;

	// Token: 0x0200002E RID: 46
	public struct DisplayMetrics
	{
		// Token: 0x040001BE RID: 446
		public int width;

		// Token: 0x040001BF RID: 447
		public int height;

		// Token: 0x040001C0 RID: 448
		public float xdpi;

		// Token: 0x040001C1 RID: 449
		public float ydpi;
	}
}
