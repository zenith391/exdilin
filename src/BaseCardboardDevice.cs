using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

public abstract class BaseCardboardDevice : BaseVRDevice
{
	private delegate void VREventCallback(int eventID);

	private const int kCardboardRenderEvent = 1196770114;

	private const int kTriggered = 1;

	private const int kTilted = 2;

	private const int kProfileChanged = 3;

	private const int kVRBackButton = 4;

	private float[] headData = new float[16];

	private float[] viewData = new float[106];

	private float[] profileData = new float[13];

	private Matrix4x4 headView;

	private Matrix4x4 leftEyeView;

	private Matrix4x4 rightEyeView;

	private Queue<int> eventQueue = new Queue<int>();

	protected bool debugDisableNativeProjections;

	protected bool debugDisableNativeDistortion;

	protected bool debugDisableNativeUILayer;

	private int[] events = new int[4];

	private const string dllName = "vrunity";

	[CompilerGenerated]
	private static VREventCallback f__mg_cache0;

	public override bool SupportsNativeDistortionCorrection(List<string> diagnostics)
	{
		bool result = base.SupportsNativeDistortionCorrection(diagnostics);
		if (debugDisableNativeDistortion)
		{
			result = false;
			diagnostics.Add("Debug override");
		}
		return result;
	}

	public override void SetDistortionCorrectionEnabled(bool enabled)
	{
		EnableDistortionCorrection(enabled);
	}

	public override void SetNeckModelScale(float scale)
	{
		SetNeckModelFactor(scale);
	}

	public override void SetAutoDriftCorrectionEnabled(bool enabled)
	{
		EnableAutoDriftCorrection(enabled);
	}

	public override void SetElectronicDisplayStabilizationEnabled(bool enabled)
	{
		EnableElectronicDisplayStabilization(enabled);
	}

	public override bool SetDefaultDeviceProfile(Uri uri)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(uri.ToString());
		return SetDefaultProfile(bytes, bytes.Length);
	}

	public override void Init()
	{
		DisplayMetrics displayMetrics = GetDisplayMetrics();
		byte[] bytes = Encoding.UTF8.GetBytes(Application.unityVersion);
		SetUnityVersion(bytes, bytes.Length);
		Start(displayMetrics.width, displayMetrics.height, displayMetrics.xdpi, displayMetrics.ydpi);
		SetEventCallback(OnVREvent);
	}

	public override void SetStereoScreen(RenderTexture stereoScreen)
	{
		SetTextureId((stereoScreen != null) ? ((int)stereoScreen.GetNativeTexturePtr()) : 0);
	}

	public override void UpdateState()
	{
		ProcessEvents();
		GetHeadPose(headData);
		ExtractMatrix(ref headView, headData);
		headPose.SetRightHanded(headView.inverse);
	}

	public override void UpdateScreenData()
	{
		UpdateProfile();
		if (debugDisableNativeProjections)
		{
			ComputeEyesFromProfile();
		}
		else
		{
			UpdateView();
		}
		profileChanged = true;
	}

	public override void Recenter()
	{
		ResetHeadTracker();
	}

	public override void PostRender()
	{
		GL.IssuePluginEvent(1196770114);
	}

	public override void OnPause(bool pause)
	{
		if (pause)
		{
			Pause();
		}
		else
		{
			Resume();
		}
	}

	public override void OnApplicationQuit()
	{
		Stop();
		base.OnApplicationQuit();
	}

	private void UpdateView()
	{
		GetViewParameters(viewData);
		int i = 0;
		i = ExtractMatrix(ref leftEyeView, viewData, i);
		i = ExtractMatrix(ref rightEyeView, viewData, i);
		leftEyePose.SetRightHanded(leftEyeView.inverse);
		rightEyePose.SetRightHanded(rightEyeView.inverse);
		i = ExtractMatrix(ref leftEyeDistortedProjection, viewData, i);
		i = ExtractMatrix(ref rightEyeDistortedProjection, viewData, i);
		i = ExtractMatrix(ref leftEyeUndistortedProjection, viewData, i);
		i = ExtractMatrix(ref rightEyeUndistortedProjection, viewData, i);
		leftEyeUndistortedViewport.Set(viewData[i], viewData[i + 1], viewData[i + 2], viewData[i + 3]);
		leftEyeDistortedViewport = leftEyeUndistortedViewport;
		i += 4;
		rightEyeUndistortedViewport.Set(viewData[i], viewData[i + 1], viewData[i + 2], viewData[i + 3]);
		rightEyeDistortedViewport = rightEyeUndistortedViewport;
		i += 4;
		recommendedTextureSize = new Vector2(viewData[i], viewData[i + 1]);
		i += 2;
	}

	private void UpdateProfile()
	{
		GetProfile(profileData);
		CardboardProfile.Device device = default(CardboardProfile.Device);
		CardboardProfile.Screen screen = default(CardboardProfile.Screen);
		device.maxFOV.outer = profileData[0];
		device.maxFOV.upper = profileData[1];
		device.maxFOV.inner = profileData[2];
		device.maxFOV.lower = profileData[3];
		screen.width = profileData[4];
		screen.height = profileData[5];
		screen.border = profileData[6];
		device.lenses.separation = profileData[7];
		device.lenses.offset = profileData[8];
		device.lenses.screenDistance = profileData[9];
		device.lenses.alignment = (int)profileData[10];
		device.distortion.k1 = profileData[11];
		device.distortion.k2 = profileData[12];
		device.inverse = CardboardProfile.ApproximateInverse(device.distortion);
		base.Profile.screen = screen;
		base.Profile.device = device;
	}

	private static int ExtractMatrix(ref Matrix4x4 mat, float[] data, int i = 0)
	{
		for (int j = 0; j < 4; j++)
		{
			int num = 0;
			while (num < 4)
			{
				mat[j, num] = data[i];
				num++;
				i++;
			}
		}
		return i;
	}

	protected virtual void ProcessEvents()
	{
		int num = 0;
		object obj = eventQueue;
		lock (obj)
		{
			num = eventQueue.Count;
			if (num == 0)
			{
				return;
			}
			if (num > events.Length)
			{
				events = new int[num];
			}
			eventQueue.CopyTo(events, 0);
			eventQueue.Clear();
		}
		for (int i = 0; i < num; i++)
		{
			switch (events[i])
			{
			case 1:
				triggered = true;
				break;
			case 2:
				tilted = true;
				break;
			case 3:
				UpdateScreenData();
				break;
			case 4:
				backButtonPressed = true;
				break;
			}
		}
	}

	[MonoPInvokeCallback(typeof(VREventCallback))]
	private static void OnVREvent(int eventID)
	{
		BaseCardboardDevice baseCardboardDevice = BaseVRDevice.GetDevice() as BaseCardboardDevice;
		object obj = baseCardboardDevice.eventQueue;
		lock (obj)
		{
			baseCardboardDevice.eventQueue.Enqueue(eventID);
		}
	}

	[DllImport("vrunity")]
	private static extern void Start(int width, int height, float xdpi, float ydpi);

	[DllImport("vrunity")]
	private static extern void SetEventCallback(VREventCallback callback);

	[DllImport("vrunity")]
	private static extern void SetTextureId(int id);

	[DllImport("vrunity")]
	private static extern bool SetDefaultProfile(byte[] uri, int size);

	[DllImport("vrunity")]
	private static extern void SetUnityVersion(byte[] version_str, int version_length);

	[DllImport("vrunity")]
	private static extern void EnableDistortionCorrection(bool enable);

	[DllImport("vrunity")]
	private static extern void EnableAutoDriftCorrection(bool enable);

	[DllImport("vrunity")]
	private static extern void EnableElectronicDisplayStabilization(bool enable);

	[DllImport("vrunity")]
	private static extern void SetNeckModelFactor(float factor);

	[DllImport("vrunity")]
	private static extern void ResetHeadTracker();

	[DllImport("vrunity")]
	private static extern void GetProfile(float[] profile);

	[DllImport("vrunity")]
	private static extern void GetHeadPose(float[] pose);

	[DllImport("vrunity")]
	private static extern void GetViewParameters(float[] viewParams);

	[DllImport("vrunity")]
	private static extern void Pause();

	[DllImport("vrunity")]
	private static extern void Resume();

	[DllImport("vrunity")]
	private static extern void Stop();
}
