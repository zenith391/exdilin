using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

// Token: 0x0200002B RID: 43
public abstract class BaseCardboardDevice : BaseVRDevice
{
	// Token: 0x0600016A RID: 362 RVA: 0x000098B8 File Offset: 0x00007CB8
	public override bool SupportsNativeDistortionCorrection(List<string> diagnostics)
	{
		bool result = base.SupportsNativeDistortionCorrection(diagnostics);
		if (this.debugDisableNativeDistortion)
		{
			result = false;
			diagnostics.Add("Debug override");
		}
		return result;
	}

	// Token: 0x0600016B RID: 363 RVA: 0x000098E6 File Offset: 0x00007CE6
	public override void SetDistortionCorrectionEnabled(bool enabled)
	{
		BaseCardboardDevice.EnableDistortionCorrection(enabled);
	}

	// Token: 0x0600016C RID: 364 RVA: 0x000098EE File Offset: 0x00007CEE
	public override void SetNeckModelScale(float scale)
	{
		BaseCardboardDevice.SetNeckModelFactor(scale);
	}

	// Token: 0x0600016D RID: 365 RVA: 0x000098F6 File Offset: 0x00007CF6
	public override void SetAutoDriftCorrectionEnabled(bool enabled)
	{
		BaseCardboardDevice.EnableAutoDriftCorrection(enabled);
	}

	// Token: 0x0600016E RID: 366 RVA: 0x000098FE File Offset: 0x00007CFE
	public override void SetElectronicDisplayStabilizationEnabled(bool enabled)
	{
		BaseCardboardDevice.EnableElectronicDisplayStabilization(enabled);
	}

	// Token: 0x0600016F RID: 367 RVA: 0x00009908 File Offset: 0x00007D08
	public override bool SetDefaultDeviceProfile(Uri uri)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(uri.ToString());
		return BaseCardboardDevice.SetDefaultProfile(bytes, bytes.Length);
	}

	// Token: 0x06000170 RID: 368 RVA: 0x00009930 File Offset: 0x00007D30
	public override void Init()
	{
		BaseVRDevice.DisplayMetrics displayMetrics = this.GetDisplayMetrics();
		byte[] bytes = Encoding.UTF8.GetBytes(Application.unityVersion);
		BaseCardboardDevice.SetUnityVersion(bytes, bytes.Length);
		BaseCardboardDevice.Start(displayMetrics.width, displayMetrics.height, displayMetrics.xdpi, displayMetrics.ydpi);
		if (BaseCardboardDevice.f__mg_cache0 == null)
		{
			BaseCardboardDevice.f__mg_cache0 = new BaseCardboardDevice.VREventCallback(BaseCardboardDevice.OnVREvent);
		}
		BaseCardboardDevice.SetEventCallback(BaseCardboardDevice.f__mg_cache0);
	}

	// Token: 0x06000171 RID: 369 RVA: 0x000099A0 File Offset: 0x00007DA0
	public override void SetStereoScreen(RenderTexture stereoScreen)
	{
		BaseCardboardDevice.SetTextureId((!(stereoScreen != null)) ? 0 : ((int)stereoScreen.GetNativeTexturePtr()));
	}

	// Token: 0x06000172 RID: 370 RVA: 0x000099C4 File Offset: 0x00007DC4
	public override void UpdateState()
	{
		this.ProcessEvents();
		BaseCardboardDevice.GetHeadPose(this.headData);
		BaseCardboardDevice.ExtractMatrix(ref this.headView, this.headData, 0);
		this.headPose.SetRightHanded(this.headView.inverse);
	}

	// Token: 0x06000173 RID: 371 RVA: 0x00009A00 File Offset: 0x00007E00
	public override void UpdateScreenData()
	{
		this.UpdateProfile();
		if (this.debugDisableNativeProjections)
		{
			base.ComputeEyesFromProfile();
		}
		else
		{
			this.UpdateView();
		}
		this.profileChanged = true;
	}

	// Token: 0x06000174 RID: 372 RVA: 0x00009A2B File Offset: 0x00007E2B
	public override void Recenter()
	{
		BaseCardboardDevice.ResetHeadTracker();
	}

	// Token: 0x06000175 RID: 373 RVA: 0x00009A32 File Offset: 0x00007E32
	public override void PostRender()
	{
		GL.IssuePluginEvent(1196770114);
	}

	// Token: 0x06000176 RID: 374 RVA: 0x00009A3E File Offset: 0x00007E3E
	public override void OnPause(bool pause)
	{
		if (pause)
		{
			BaseCardboardDevice.Pause();
		}
		else
		{
			BaseCardboardDevice.Resume();
		}
	}

	// Token: 0x06000177 RID: 375 RVA: 0x00009A55 File Offset: 0x00007E55
	public override void OnApplicationQuit()
	{
		BaseCardboardDevice.Stop();
		base.OnApplicationQuit();
	}

	// Token: 0x06000178 RID: 376 RVA: 0x00009A64 File Offset: 0x00007E64
	private void UpdateView()
	{
		BaseCardboardDevice.GetViewParameters(this.viewData);
		int num = 0;
		num = BaseCardboardDevice.ExtractMatrix(ref this.leftEyeView, this.viewData, num);
		num = BaseCardboardDevice.ExtractMatrix(ref this.rightEyeView, this.viewData, num);
		this.leftEyePose.SetRightHanded(this.leftEyeView.inverse);
		this.rightEyePose.SetRightHanded(this.rightEyeView.inverse);
		num = BaseCardboardDevice.ExtractMatrix(ref this.leftEyeDistortedProjection, this.viewData, num);
		num = BaseCardboardDevice.ExtractMatrix(ref this.rightEyeDistortedProjection, this.viewData, num);
		num = BaseCardboardDevice.ExtractMatrix(ref this.leftEyeUndistortedProjection, this.viewData, num);
		num = BaseCardboardDevice.ExtractMatrix(ref this.rightEyeUndistortedProjection, this.viewData, num);
		this.leftEyeUndistortedViewport.Set(this.viewData[num], this.viewData[num + 1], this.viewData[num + 2], this.viewData[num + 3]);
		this.leftEyeDistortedViewport = this.leftEyeUndistortedViewport;
		num += 4;
		this.rightEyeUndistortedViewport.Set(this.viewData[num], this.viewData[num + 1], this.viewData[num + 2], this.viewData[num + 3]);
		this.rightEyeDistortedViewport = this.rightEyeUndistortedViewport;
		num += 4;
		this.recommendedTextureSize = new Vector2(this.viewData[num], this.viewData[num + 1]);
		num += 2;
	}

	// Token: 0x06000179 RID: 377 RVA: 0x00009BC0 File Offset: 0x00007FC0
	private void UpdateProfile()
	{
		BaseCardboardDevice.GetProfile(this.profileData);
		CardboardProfile.Device device = default(CardboardProfile.Device);
		CardboardProfile.Screen screen = default(CardboardProfile.Screen);
		device.maxFOV.outer = this.profileData[0];
		device.maxFOV.upper = this.profileData[1];
		device.maxFOV.inner = this.profileData[2];
		device.maxFOV.lower = this.profileData[3];
		screen.width = this.profileData[4];
		screen.height = this.profileData[5];
		screen.border = this.profileData[6];
		device.lenses.separation = this.profileData[7];
		device.lenses.offset = this.profileData[8];
		device.lenses.screenDistance = this.profileData[9];
		device.lenses.alignment = (int)this.profileData[10];
		device.distortion.k1 = this.profileData[11];
		device.distortion.k2 = this.profileData[12];
		device.inverse = CardboardProfile.ApproximateInverse(device.distortion, 1f, 10);
		base.Profile.screen = screen;
		base.Profile.device = device;
	}

	// Token: 0x0600017A RID: 378 RVA: 0x00009D14 File Offset: 0x00008114
	private static int ExtractMatrix(ref Matrix4x4 mat, float[] data, int i = 0)
	{
		for (int j = 0; j < 4; j++)
		{
			int k = 0;
			while (k < 4)
			{
				mat[j, k] = data[i];
				k++;
				i++;
			}
		}
		return i;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x00009D58 File Offset: 0x00008158
	protected virtual void ProcessEvents()
	{
		int num = 0;
		object obj = this.eventQueue;
		lock (obj)
		{
			num = this.eventQueue.Count;
			if (num == 0)
			{
				return;
			}
			if (num > this.events.Length)
			{
				this.events = new int[num];
			}
			this.eventQueue.CopyTo(this.events, 0);
			this.eventQueue.Clear();
		}
		for (int i = 0; i < num; i++)
		{
			switch (this.events[i])
			{
			case 1:
				this.triggered = true;
				break;
			case 2:
				this.tilted = true;
				break;
			case 3:
				this.UpdateScreenData();
				break;
			case 4:
				this.backButtonPressed = true;
				break;
			}
		}
	}

	// Token: 0x0600017C RID: 380 RVA: 0x00009E48 File Offset: 0x00008248
	[MonoPInvokeCallback(typeof(BaseCardboardDevice.VREventCallback))]
	private static void OnVREvent(int eventID)
	{
		BaseCardboardDevice baseCardboardDevice = BaseVRDevice.GetDevice() as BaseCardboardDevice;
		object obj = baseCardboardDevice.eventQueue;
		lock (obj)
		{
			baseCardboardDevice.eventQueue.Enqueue(eventID);
		}
	}

	// Token: 0x0600017D RID: 381
	[DllImport("vrunity")]
	private static extern void Start(int width, int height, float xdpi, float ydpi);

	// Token: 0x0600017E RID: 382
	[DllImport("vrunity")]
	private static extern void SetEventCallback(BaseCardboardDevice.VREventCallback callback);

	// Token: 0x0600017F RID: 383
	[DllImport("vrunity")]
	private static extern void SetTextureId(int id);

	// Token: 0x06000180 RID: 384
	[DllImport("vrunity")]
	private static extern bool SetDefaultProfile(byte[] uri, int size);

	// Token: 0x06000181 RID: 385
	[DllImport("vrunity")]
	private static extern void SetUnityVersion(byte[] version_str, int version_length);

	// Token: 0x06000182 RID: 386
	[DllImport("vrunity")]
	private static extern void EnableDistortionCorrection(bool enable);

	// Token: 0x06000183 RID: 387
	[DllImport("vrunity")]
	private static extern void EnableAutoDriftCorrection(bool enable);

	// Token: 0x06000184 RID: 388
	[DllImport("vrunity")]
	private static extern void EnableElectronicDisplayStabilization(bool enable);

	// Token: 0x06000185 RID: 389
	[DllImport("vrunity")]
	private static extern void SetNeckModelFactor(float factor);

	// Token: 0x06000186 RID: 390
	[DllImport("vrunity")]
	private static extern void ResetHeadTracker();

	// Token: 0x06000187 RID: 391
	[DllImport("vrunity")]
	private static extern void GetProfile(float[] profile);

	// Token: 0x06000188 RID: 392
	[DllImport("vrunity")]
	private static extern void GetHeadPose(float[] pose);

	// Token: 0x06000189 RID: 393
	[DllImport("vrunity")]
	private static extern void GetViewParameters(float[] viewParams);

	// Token: 0x0600018A RID: 394
	[DllImport("vrunity")]
	private static extern void Pause();

	// Token: 0x0600018B RID: 395
	[DllImport("vrunity")]
	private static extern void Resume();

	// Token: 0x0600018C RID: 396
	[DllImport("vrunity")]
	private static extern void Stop();

	// Token: 0x0400019A RID: 410
	private const int kCardboardRenderEvent = 1196770114;

	// Token: 0x0400019B RID: 411
	private const int kTriggered = 1;

	// Token: 0x0400019C RID: 412
	private const int kTilted = 2;

	// Token: 0x0400019D RID: 413
	private const int kProfileChanged = 3;

	// Token: 0x0400019E RID: 414
	private const int kVRBackButton = 4;

	// Token: 0x0400019F RID: 415
	private float[] headData = new float[16];

	// Token: 0x040001A0 RID: 416
	private float[] viewData = new float[106];

	// Token: 0x040001A1 RID: 417
	private float[] profileData = new float[13];

	// Token: 0x040001A2 RID: 418
	private Matrix4x4 headView = default(Matrix4x4);

	// Token: 0x040001A3 RID: 419
	private Matrix4x4 leftEyeView = default(Matrix4x4);

	// Token: 0x040001A4 RID: 420
	private Matrix4x4 rightEyeView = default(Matrix4x4);

	// Token: 0x040001A5 RID: 421
	private Queue<int> eventQueue = new Queue<int>();

	// Token: 0x040001A6 RID: 422
	protected bool debugDisableNativeProjections;

	// Token: 0x040001A7 RID: 423
	protected bool debugDisableNativeDistortion;

	// Token: 0x040001A8 RID: 424
	protected bool debugDisableNativeUILayer;

	// Token: 0x040001A9 RID: 425
	private int[] events = new int[4];

	// Token: 0x040001AA RID: 426
	private const string dllName = "vrunity";

	// Token: 0x040001AB RID: 427
	[CompilerGenerated]
	private static BaseCardboardDevice.VREventCallback f__mg_cache0;

	// Token: 0x0200002C RID: 44
	// (Invoke) Token: 0x0600018E RID: 398
	private delegate void VREventCallback(int eventID);
}
