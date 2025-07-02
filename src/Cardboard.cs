using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Cardboard/Cardboard")]
public class Cardboard : MonoBehaviour
{
	public enum DistortionCorrectionMethod
	{
		None,
		Native,
		Unity
	}

	public enum BackButtonModes
	{
		Off,
		OnlyInVR,
		On
	}

	public delegate void StereoScreenChangeDelegate(RenderTexture newStereoScreen);

	public enum Eye
	{
		Left,
		Right,
		Center
	}

	public enum Distortion
	{
		Distorted,
		Undistorted
	}

	public const string CARDBOARD_SDK_VERSION = "0.6";

	private static Cardboard sdk;

	private static Camera currentMainCamera;

	private static StereoController currentController;

	private bool uiLayerEnabled;

	[SerializeField]
	private bool vrModeEnabled = true;

	[SerializeField]
	private DistortionCorrectionMethod distortionCorrection = DistortionCorrectionMethod.Unity;

	[SerializeField]
	private bool enableAlignmentMarker = true;

	[SerializeField]
	private bool enableSettingsButton = true;

	[SerializeField]
	private BackButtonModes backButtonMode = BackButtonModes.OnlyInVR;

	[SerializeField]
	private bool tapIsTrigger = true;

	[SerializeField]
	private float neckModelScale;

	[SerializeField]
	private bool autoDriftCorrection = true;

	[SerializeField]
	private bool electronicDisplayStabilization;

	private static BaseVRDevice device;

	[SerializeField]
	private float stereoScreenScale = 1f;

	private static RenderTexture stereoScreen;

	private readonly Vector2 defaultComfortableViewingRange = new Vector2(0.4f, 100000f);

	public Uri DefaultDeviceProfile;

	private int updatedToFrame;

	public static Cardboard SDK
	{
		get
		{
			if (sdk == null)
			{
				if (Application.isEditor && !Application.isPlaying)
				{
					sdk = UnityEngine.Object.FindObjectOfType<Cardboard>();
				}
				else
				{
					Debug.LogError("No Cardboard instance found.  Ensure one exists in the scene, or callCardboard.Create() at startup to generate one.\nIf one does exist but hasn't called Awake() yet, then this error is due to order-of-initialization.\nIn that case, consider moving your first reference to Cardboard.SDK to a later point in time.\nIf exiting the scene, this indicates that the Cardboard object has already been destroyed.");
				}
			}
			return sdk;
		}
	}

	public static StereoController Controller
	{
		get
		{
			Camera main = Camera.main;
			if (main != currentMainCamera || currentController == null)
			{
				currentMainCamera = main;
				currentController = main.GetComponent<StereoController>();
			}
			return currentController;
		}
	}

	public bool UILayerEnabled
	{
		get
		{
			return uiLayerEnabled;
		}
		private set
		{
			if (value != uiLayerEnabled && device != null)
			{
				device.SetUILayerEnabled(value);
			}
			uiLayerEnabled = value;
		}
	}

	public bool VRModeEnabled
	{
		get
		{
			return vrModeEnabled;
		}
		set
		{
			if (value != vrModeEnabled && device != null)
			{
				device.SetVRModeEnabled(value);
			}
			vrModeEnabled = value;
		}
	}

	public DistortionCorrectionMethod DistortionCorrection
	{
		get
		{
			return distortionCorrection;
		}
		set
		{
			if (value != distortionCorrection && device != null)
			{
				device.SetDistortionCorrectionEnabled(value == DistortionCorrectionMethod.Native && NativeDistortionCorrectionSupported);
				device.UpdateScreenData();
			}
			distortionCorrection = value;
		}
	}

	public bool EnableAlignmentMarker
	{
		get
		{
			return enableAlignmentMarker;
		}
		set
		{
			if (value != enableAlignmentMarker && device != null)
			{
				device.SetAlignmentMarkerEnabled(value);
			}
			enableAlignmentMarker = value;
		}
	}

	public bool EnableSettingsButton
	{
		get
		{
			return enableSettingsButton;
		}
		set
		{
			if (value != enableSettingsButton && device != null)
			{
				device.SetSettingsButtonEnabled(value);
			}
			enableSettingsButton = value;
		}
	}

	public BackButtonModes BackButtonMode
	{
		get
		{
			return backButtonMode;
		}
		set
		{
			if (value != backButtonMode && device != null)
			{
				device.SetVRBackButtonEnabled(value != BackButtonModes.Off);
				device.SetShowVrBackButtonOnlyInVR(value == BackButtonModes.OnlyInVR);
			}
			backButtonMode = value;
		}
	}

	public bool TapIsTrigger
	{
		get
		{
			return tapIsTrigger;
		}
		set
		{
			if (value != tapIsTrigger && device != null)
			{
				device.SetTapIsTrigger(value);
			}
			tapIsTrigger = value;
		}
	}

	public float NeckModelScale
	{
		get
		{
			return neckModelScale;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (!Mathf.Approximately(value, neckModelScale) && device != null)
			{
				device.SetNeckModelScale(value);
			}
			neckModelScale = value;
		}
	}

	public bool AutoDriftCorrection
	{
		get
		{
			return autoDriftCorrection;
		}
		set
		{
			if (value != autoDriftCorrection && device != null)
			{
				device.SetAutoDriftCorrectionEnabled(value);
			}
			autoDriftCorrection = value;
		}
	}

	public bool ElectronicDisplayStabilization
	{
		get
		{
			return electronicDisplayStabilization;
		}
		set
		{
			if (value != electronicDisplayStabilization && device != null)
			{
				device.SetElectronicDisplayStabilizationEnabled(value);
			}
			electronicDisplayStabilization = value;
		}
	}

	public bool NativeDistortionCorrectionSupported { get; private set; }

	public bool NativeUILayerSupported { get; private set; }

	public float StereoScreenScale
	{
		get
		{
			return stereoScreenScale;
		}
		set
		{
			value = Mathf.Clamp(value, 0.1f, 10f);
			if (stereoScreenScale != value)
			{
				stereoScreenScale = value;
				StereoScreen = null;
			}
		}
	}

	public RenderTexture StereoScreen
	{
		get
		{
			if (distortionCorrection == DistortionCorrectionMethod.None || !vrModeEnabled)
			{
				return null;
			}
			if (stereoScreen == null)
			{
				StereoScreen = device.CreateStereoScreen();
			}
			return stereoScreen;
		}
		set
		{
			if (value == stereoScreen)
			{
				return;
			}
			if (!SystemInfo.supportsRenderTextures && value != null)
			{
				Debug.LogError("Can't set StereoScreen: RenderTextures are not supported.");
				return;
			}
			if (stereoScreen != null)
			{
				stereoScreen.Release();
			}
			stereoScreen = value;
			if (device != null)
			{
				if (stereoScreen != null)
				{
					stereoScreen.Create();
				}
				device.SetStereoScreen(stereoScreen);
			}
			if (this.OnStereoScreenChanged != null)
			{
				this.OnStereoScreenChanged(stereoScreen);
			}
		}
	}

	public CardboardProfile Profile => device.Profile;

	public Pose3D HeadPose => device.GetHeadPose();

	public Vector2 ComfortableViewingRange => defaultComfortableViewingRange;

	public bool Triggered { get; private set; }

	public bool Tilted { get; private set; }

	public bool ProfileChanged { get; private set; }

	public bool BackButtonPressed { get; private set; }

	[Obsolete("Use DistortionCorrection instead.")]
	public bool nativeDistortionCorrection
	{
		get
		{
			return DistortionCorrection == DistortionCorrectionMethod.Native;
		}
		set
		{
			DistortionCorrection = (value ? DistortionCorrectionMethod.Native : DistortionCorrectionMethod.None);
		}
	}

	[Obsolete("InCardboard is deprecated.")]
	public bool InCardboard => true;

	[Obsolete("Use Triggered instead.")]
	public bool CardboardTriggered => Triggered;

	[Obsolete("Use HeadPose instead.")]
	public Matrix4x4 HeadView => HeadPose.Matrix;

	[Obsolete("Use HeadPose instead.")]
	public Quaternion HeadRotation => HeadPose.Orientation;

	[Obsolete("Use HeadPose instead.")]
	public Vector3 HeadPosition => HeadPose.Position;

	[Obsolete("Use ComfortableViewingRange instead.")]
	public float MinimumComfortDistance => ComfortableViewingRange.x;

	[Obsolete("Use ComfortableViewingRange instead.")]
	public float MaximumComfortDistance => ComfortableViewingRange.y;

	public event StereoScreenChangeDelegate OnStereoScreenChanged;

	public event Action OnTrigger;

	public event Action OnTilt;

	public event Action OnProfileChange;

	public event Action OnBackButton;

	public static void Create()
	{
		if (sdk == null && UnityEngine.Object.FindObjectOfType<Cardboard>() == null)
		{
			Debug.Log("Creating Cardboard object");
			GameObject gameObject = new GameObject("Cardboard", typeof(Cardboard));
			gameObject.transform.localPosition = Vector3.zero;
		}
	}

	public Pose3D EyePose(Eye eye)
	{
		return device.GetEyePose(eye);
	}

	public Matrix4x4 Projection(Eye eye, Distortion distortion = Distortion.Distorted)
	{
		return device.GetProjection(eye, distortion);
	}

	public Rect Viewport(Eye eye, Distortion distortion = Distortion.Distorted)
	{
		return device.GetViewport(eye, distortion);
	}

	private void InitDevice()
	{
		if (device != null)
		{
			device.Destroy();
		}
		device = BaseVRDevice.GetDevice();
		device.Init();
		List<string> list = new List<string>();
		NativeDistortionCorrectionSupported = device.SupportsNativeDistortionCorrection(list);
		if (list.Count > 0)
		{
			Debug.LogWarning("Built-in distortion correction disabled. Causes: [" + string.Join("; ", list.ToArray()) + "]");
		}
		list.Clear();
		NativeUILayerSupported = device.SupportsNativeUILayer(list);
		if (list.Count > 0)
		{
			Debug.LogWarning("Built-in UI layer disabled. Causes: [" + string.Join("; ", list.ToArray()) + "]");
		}
		if (DefaultDeviceProfile != null)
		{
			device.SetDefaultDeviceProfile(DefaultDeviceProfile);
		}
		device.SetAlignmentMarkerEnabled(enableAlignmentMarker);
		device.SetSettingsButtonEnabled(enableSettingsButton);
		device.SetVRBackButtonEnabled(backButtonMode != BackButtonModes.Off);
		device.SetShowVrBackButtonOnlyInVR(backButtonMode == BackButtonModes.OnlyInVR);
		device.SetDistortionCorrectionEnabled(distortionCorrection == DistortionCorrectionMethod.Native && NativeDistortionCorrectionSupported);
		device.SetTapIsTrigger(tapIsTrigger);
		device.SetNeckModelScale(neckModelScale);
		device.SetAutoDriftCorrectionEnabled(autoDriftCorrection);
		device.SetElectronicDisplayStabilizationEnabled(electronicDisplayStabilization);
		device.SetVRModeEnabled(vrModeEnabled);
		device.UpdateScreenData();
	}

	private void Awake()
	{
		if (sdk == null)
		{
			sdk = this;
		}
		if (sdk != this)
		{
			Debug.LogError("There must be only one Cardboard object in a scene.");
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		Screen.sleepTimeout = -1;
		InitDevice();
		StereoScreen = null;
		AddCardboardCamera();
	}

	private void Start()
	{
		UILayerEnabled = true;
	}

	private void AddCardboardCamera()
	{
		CardboardPreRender cardboardPreRender = UnityEngine.Object.FindObjectOfType<CardboardPreRender>();
		if (cardboardPreRender == null)
		{
			GameObject gameObject = new GameObject("PreRender", typeof(CardboardPreRender));
			gameObject.SendMessage("Reset");
			gameObject.transform.parent = base.transform;
		}
		CardboardPostRender cardboardPostRender = UnityEngine.Object.FindObjectOfType<CardboardPostRender>();
		if (cardboardPostRender == null)
		{
			GameObject gameObject2 = new GameObject("PostRender", typeof(CardboardPostRender));
			gameObject2.SendMessage("Reset");
			gameObject2.transform.parent = base.transform;
		}
	}

	public void UpdateState()
	{
		if (updatedToFrame != Time.frameCount)
		{
			updatedToFrame = Time.frameCount;
			device.UpdateState();
			DispatchEvents();
		}
	}

	private void DispatchEvents()
	{
		Triggered = device.triggered || Input.GetMouseButtonDown(0);
		Tilted = device.tilted;
		ProfileChanged = device.profileChanged;
		BackButtonPressed = device.backButtonPressed || Input.GetKeyDown(KeyCode.Escape);
		device.triggered = false;
		device.tilted = false;
		device.profileChanged = false;
		device.backButtonPressed = false;
		if (Tilted && this.OnTilt != null)
		{
			this.OnTilt();
		}
		if (Triggered && this.OnTrigger != null)
		{
			this.OnTrigger();
		}
		if (ProfileChanged && this.OnProfileChange != null)
		{
			this.OnProfileChange();
		}
		if (BackButtonPressed && this.OnBackButton != null)
		{
			this.OnBackButton();
		}
	}

	public void PostRender()
	{
		if (NativeDistortionCorrectionSupported)
		{
			device.PostRender();
		}
	}

	public void Recenter()
	{
		device.Recenter();
	}

	public void SetTouchCoordinates(int x, int y)
	{
		device.SetTouchCoordinates(x, y);
	}

	public void ShowSettingsDialog()
	{
		device.ShowSettingsDialog();
	}

	private void OnEnable()
	{
		device.OnPause(pause: false);
	}

	private void OnDisable()
	{
		device.OnPause(pause: true);
	}

	private void OnApplicationPause(bool pause)
	{
		device.OnPause(pause);
	}

	private void OnApplicationFocus(bool focus)
	{
		device.OnFocus(focus);
	}

	private void OnLevelWasLoaded(int level)
	{
		device.OnLevelLoaded(level);
	}

	private void OnApplicationQuit()
	{
		device.OnApplicationQuit();
	}

	private void OnDestroy()
	{
		VRModeEnabled = false;
		UILayerEnabled = false;
		if (device != null)
		{
			device.Destroy();
		}
		if (sdk == this)
		{
			sdk = null;
		}
	}

	[Obsolete("Use EyePose() instead.")]
	public Matrix4x4 EyeView(Eye eye)
	{
		return EyePose(eye).Matrix;
	}

	[Obsolete("Use EyePose() instead.")]
	public Vector3 EyeOffset(Eye eye)
	{
		return EyePose(eye).Position;
	}

	[Obsolete("Use Projection() instead.")]
	public Matrix4x4 UndistortedProjection(Eye eye)
	{
		return Projection(eye, Distortion.Undistorted);
	}

	[Obsolete("Use Viewport() instead.")]
	public Rect EyeRect(Eye eye)
	{
		return Viewport(eye);
	}
}
