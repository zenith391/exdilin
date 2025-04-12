using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000011 RID: 17
[AddComponentMenu("Cardboard/Cardboard")]
public class Cardboard : MonoBehaviour
{
	// Token: 0x1700000D RID: 13
	// (get) Token: 0x06000085 RID: 133 RVA: 0x00004D7C File Offset: 0x0000317C
	public static Cardboard SDK
	{
		get
		{
			if (Cardboard.sdk == null)
			{
				if (Application.isEditor && !Application.isPlaying)
				{
					Cardboard.sdk = UnityEngine.Object.FindObjectOfType<Cardboard>();
				}
				else
				{
					Debug.LogError("No Cardboard instance found.  Ensure one exists in the scene, or callCardboard.Create() at startup to generate one.\nIf one does exist but hasn't called Awake() yet, then this error is due to order-of-initialization.\nIn that case, consider moving your first reference to Cardboard.SDK to a later point in time.\nIf exiting the scene, this indicates that the Cardboard object has already been destroyed.");
				}
			}
			return Cardboard.sdk;
		}
	}

	// Token: 0x06000086 RID: 134 RVA: 0x00004DCC File Offset: 0x000031CC
	public static void Create()
	{
		if (Cardboard.sdk == null && UnityEngine.Object.FindObjectOfType<Cardboard>() == null)
		{
			Debug.Log("Creating Cardboard object");
			GameObject gameObject = new GameObject("Cardboard", new Type[]
			{
				typeof(Cardboard)
			});
			gameObject.transform.localPosition = Vector3.zero;
		}
	}

	// Token: 0x1700000E RID: 14
	// (get) Token: 0x06000087 RID: 135 RVA: 0x00004E34 File Offset: 0x00003234
	public static StereoController Controller
	{
		get
		{
			Camera main = Camera.main;
			if (main != Cardboard.currentMainCamera || Cardboard.currentController == null)
			{
				Cardboard.currentMainCamera = main;
				Cardboard.currentController = main.GetComponent<StereoController>();
			}
			return Cardboard.currentController;
		}
	}

	// Token: 0x1700000F RID: 15
	// (get) Token: 0x06000088 RID: 136 RVA: 0x00004E7D File Offset: 0x0000327D
	// (set) Token: 0x06000089 RID: 137 RVA: 0x00004E85 File Offset: 0x00003285
	public bool UILayerEnabled
	{
		get
		{
			return this.uiLayerEnabled;
		}
		private set
		{
			if (value != this.uiLayerEnabled && Cardboard.device != null)
			{
				Cardboard.device.SetUILayerEnabled(value);
			}
			this.uiLayerEnabled = value;
		}
	}

	// Token: 0x17000010 RID: 16
	// (get) Token: 0x0600008A RID: 138 RVA: 0x00004EAF File Offset: 0x000032AF
	// (set) Token: 0x0600008B RID: 139 RVA: 0x00004EB7 File Offset: 0x000032B7
	public bool VRModeEnabled
	{
		get
		{
			return this.vrModeEnabled;
		}
		set
		{
			if (value != this.vrModeEnabled && Cardboard.device != null)
			{
				Cardboard.device.SetVRModeEnabled(value);
			}
			this.vrModeEnabled = value;
		}
	}

	// Token: 0x17000011 RID: 17
	// (get) Token: 0x0600008C RID: 140 RVA: 0x00004EE1 File Offset: 0x000032E1
	// (set) Token: 0x0600008D RID: 141 RVA: 0x00004EEC File Offset: 0x000032EC
	public Cardboard.DistortionCorrectionMethod DistortionCorrection
	{
		get
		{
			return this.distortionCorrection;
		}
		set
		{
			if (value != this.distortionCorrection && Cardboard.device != null)
			{
				Cardboard.device.SetDistortionCorrectionEnabled(value == Cardboard.DistortionCorrectionMethod.Native && this.NativeDistortionCorrectionSupported);
				Cardboard.device.UpdateScreenData();
			}
			this.distortionCorrection = value;
		}
	}

	// Token: 0x17000012 RID: 18
	// (get) Token: 0x0600008E RID: 142 RVA: 0x00004F3A File Offset: 0x0000333A
	// (set) Token: 0x0600008F RID: 143 RVA: 0x00004F42 File Offset: 0x00003342
	public bool EnableAlignmentMarker
	{
		get
		{
			return this.enableAlignmentMarker;
		}
		set
		{
			if (value != this.enableAlignmentMarker && Cardboard.device != null)
			{
				Cardboard.device.SetAlignmentMarkerEnabled(value);
			}
			this.enableAlignmentMarker = value;
		}
	}

	// Token: 0x17000013 RID: 19
	// (get) Token: 0x06000090 RID: 144 RVA: 0x00004F6C File Offset: 0x0000336C
	// (set) Token: 0x06000091 RID: 145 RVA: 0x00004F74 File Offset: 0x00003374
	public bool EnableSettingsButton
	{
		get
		{
			return this.enableSettingsButton;
		}
		set
		{
			if (value != this.enableSettingsButton && Cardboard.device != null)
			{
				Cardboard.device.SetSettingsButtonEnabled(value);
			}
			this.enableSettingsButton = value;
		}
	}

	// Token: 0x17000014 RID: 20
	// (get) Token: 0x06000092 RID: 146 RVA: 0x00004F9E File Offset: 0x0000339E
	// (set) Token: 0x06000093 RID: 147 RVA: 0x00004FA6 File Offset: 0x000033A6
	public Cardboard.BackButtonModes BackButtonMode
	{
		get
		{
			return this.backButtonMode;
		}
		set
		{
			if (value != this.backButtonMode && Cardboard.device != null)
			{
				Cardboard.device.SetVRBackButtonEnabled(value != Cardboard.BackButtonModes.Off);
				Cardboard.device.SetShowVrBackButtonOnlyInVR(value == Cardboard.BackButtonModes.OnlyInVR);
			}
			this.backButtonMode = value;
		}
	}

	// Token: 0x17000015 RID: 21
	// (get) Token: 0x06000094 RID: 148 RVA: 0x00004FE4 File Offset: 0x000033E4
	// (set) Token: 0x06000095 RID: 149 RVA: 0x00004FEC File Offset: 0x000033EC
	public bool TapIsTrigger
	{
		get
		{
			return this.tapIsTrigger;
		}
		set
		{
			if (value != this.tapIsTrigger && Cardboard.device != null)
			{
				Cardboard.device.SetTapIsTrigger(value);
			}
			this.tapIsTrigger = value;
		}
	}

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x06000096 RID: 150 RVA: 0x00005016 File Offset: 0x00003416
	// (set) Token: 0x06000097 RID: 151 RVA: 0x0000501E File Offset: 0x0000341E
	public float NeckModelScale
	{
		get
		{
			return this.neckModelScale;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (!Mathf.Approximately(value, this.neckModelScale) && Cardboard.device != null)
			{
				Cardboard.device.SetNeckModelScale(value);
			}
			this.neckModelScale = value;
		}
	}

	// Token: 0x17000017 RID: 23
	// (get) Token: 0x06000098 RID: 152 RVA: 0x00005055 File Offset: 0x00003455
	// (set) Token: 0x06000099 RID: 153 RVA: 0x0000505D File Offset: 0x0000345D
	public bool AutoDriftCorrection
	{
		get
		{
			return this.autoDriftCorrection;
		}
		set
		{
			if (value != this.autoDriftCorrection && Cardboard.device != null)
			{
				Cardboard.device.SetAutoDriftCorrectionEnabled(value);
			}
			this.autoDriftCorrection = value;
		}
	}

	// Token: 0x17000018 RID: 24
	// (get) Token: 0x0600009A RID: 154 RVA: 0x00005087 File Offset: 0x00003487
	// (set) Token: 0x0600009B RID: 155 RVA: 0x0000508F File Offset: 0x0000348F
	public bool ElectronicDisplayStabilization
	{
		get
		{
			return this.electronicDisplayStabilization;
		}
		set
		{
			if (value != this.electronicDisplayStabilization && Cardboard.device != null)
			{
				Cardboard.device.SetElectronicDisplayStabilizationEnabled(value);
			}
			this.electronicDisplayStabilization = value;
		}
	}

	// Token: 0x17000019 RID: 25
	// (get) Token: 0x0600009C RID: 156 RVA: 0x000050B9 File Offset: 0x000034B9
	// (set) Token: 0x0600009D RID: 157 RVA: 0x000050C1 File Offset: 0x000034C1
	public bool NativeDistortionCorrectionSupported { get; private set; }

	// Token: 0x1700001A RID: 26
	// (get) Token: 0x0600009E RID: 158 RVA: 0x000050CA File Offset: 0x000034CA
	// (set) Token: 0x0600009F RID: 159 RVA: 0x000050D2 File Offset: 0x000034D2
	public bool NativeUILayerSupported { get; private set; }

	// Token: 0x1700001B RID: 27
	// (get) Token: 0x060000A0 RID: 160 RVA: 0x000050DB File Offset: 0x000034DB
	// (set) Token: 0x060000A1 RID: 161 RVA: 0x000050E3 File Offset: 0x000034E3
	public float StereoScreenScale
	{
		get
		{
			return this.stereoScreenScale;
		}
		set
		{
			value = Mathf.Clamp(value, 0.1f, 10f);
			if (this.stereoScreenScale != value)
			{
				this.stereoScreenScale = value;
				this.StereoScreen = null;
			}
		}
	}

	// Token: 0x1700001C RID: 28
	// (get) Token: 0x060000A2 RID: 162 RVA: 0x00005111 File Offset: 0x00003511
	// (set) Token: 0x060000A3 RID: 163 RVA: 0x00005150 File Offset: 0x00003550
	public RenderTexture StereoScreen
	{
		get
		{
			if (this.distortionCorrection == Cardboard.DistortionCorrectionMethod.None || !this.vrModeEnabled)
			{
				return null;
			}
			if (Cardboard.stereoScreen == null)
			{
				this.StereoScreen = Cardboard.device.CreateStereoScreen();
			}
			return Cardboard.stereoScreen;
		}
		set
		{
			if (value == Cardboard.stereoScreen)
			{
				return;
			}
			if (!SystemInfo.supportsRenderTextures && value != null)
			{
				Debug.LogError("Can't set StereoScreen: RenderTextures are not supported.");
				return;
			}
			if (Cardboard.stereoScreen != null)
			{
				Cardboard.stereoScreen.Release();
			}
			Cardboard.stereoScreen = value;
			if (Cardboard.device != null)
			{
				if (Cardboard.stereoScreen != null)
				{
					Cardboard.stereoScreen.Create();
				}
				Cardboard.device.SetStereoScreen(Cardboard.stereoScreen);
			}
			if (this.OnStereoScreenChanged != null)
			{
				this.OnStereoScreenChanged(Cardboard.stereoScreen);
			}
		}
	}

	// Token: 0x14000001 RID: 1
	// (add) Token: 0x060000A4 RID: 164 RVA: 0x00005200 File Offset: 0x00003600
	// (remove) Token: 0x060000A5 RID: 165 RVA: 0x00005238 File Offset: 0x00003638
	public event Cardboard.StereoScreenChangeDelegate OnStereoScreenChanged;

	// Token: 0x1700001D RID: 29
	// (get) Token: 0x060000A6 RID: 166 RVA: 0x0000526E File Offset: 0x0000366E
	public CardboardProfile Profile
	{
		get
		{
			return Cardboard.device.Profile;
		}
	}

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x060000A7 RID: 167 RVA: 0x0000527A File Offset: 0x0000367A
	public Pose3D HeadPose
	{
		get
		{
			return Cardboard.device.GetHeadPose();
		}
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00005286 File Offset: 0x00003686
	public Pose3D EyePose(Cardboard.Eye eye)
	{
		return Cardboard.device.GetEyePose(eye);
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00005293 File Offset: 0x00003693
	public Matrix4x4 Projection(Cardboard.Eye eye, Cardboard.Distortion distortion = Cardboard.Distortion.Distorted)
	{
		return Cardboard.device.GetProjection(eye, distortion);
	}

	// Token: 0x060000AA RID: 170 RVA: 0x000052A1 File Offset: 0x000036A1
	public Rect Viewport(Cardboard.Eye eye, Cardboard.Distortion distortion = Cardboard.Distortion.Distorted)
	{
		return Cardboard.device.GetViewport(eye, distortion);
	}

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x060000AB RID: 171 RVA: 0x000052AF File Offset: 0x000036AF
	public Vector2 ComfortableViewingRange
	{
		get
		{
			return this.defaultComfortableViewingRange;
		}
	}

	// Token: 0x060000AC RID: 172 RVA: 0x000052B8 File Offset: 0x000036B8
	private void InitDevice()
	{
		if (Cardboard.device != null)
		{
			Cardboard.device.Destroy();
		}
		Cardboard.device = BaseVRDevice.GetDevice();
		Cardboard.device.Init();
		List<string> list = new List<string>();
		this.NativeDistortionCorrectionSupported = Cardboard.device.SupportsNativeDistortionCorrection(list);
		if (list.Count > 0)
		{
			Debug.LogWarning("Built-in distortion correction disabled. Causes: [" + string.Join("; ", list.ToArray()) + "]");
		}
		list.Clear();
		this.NativeUILayerSupported = Cardboard.device.SupportsNativeUILayer(list);
		if (list.Count > 0)
		{
			Debug.LogWarning("Built-in UI layer disabled. Causes: [" + string.Join("; ", list.ToArray()) + "]");
		}
		if (this.DefaultDeviceProfile != null)
		{
			Cardboard.device.SetDefaultDeviceProfile(this.DefaultDeviceProfile);
		}
		Cardboard.device.SetAlignmentMarkerEnabled(this.enableAlignmentMarker);
		Cardboard.device.SetSettingsButtonEnabled(this.enableSettingsButton);
		Cardboard.device.SetVRBackButtonEnabled(this.backButtonMode != Cardboard.BackButtonModes.Off);
		Cardboard.device.SetShowVrBackButtonOnlyInVR(this.backButtonMode == Cardboard.BackButtonModes.OnlyInVR);
		Cardboard.device.SetDistortionCorrectionEnabled(this.distortionCorrection == Cardboard.DistortionCorrectionMethod.Native && this.NativeDistortionCorrectionSupported);
		Cardboard.device.SetTapIsTrigger(this.tapIsTrigger);
		Cardboard.device.SetNeckModelScale(this.neckModelScale);
		Cardboard.device.SetAutoDriftCorrectionEnabled(this.autoDriftCorrection);
		Cardboard.device.SetElectronicDisplayStabilizationEnabled(this.electronicDisplayStabilization);
		Cardboard.device.SetVRModeEnabled(this.vrModeEnabled);
		Cardboard.device.UpdateScreenData();
	}

	// Token: 0x060000AD RID: 173 RVA: 0x00005460 File Offset: 0x00003860
	private void Awake()
	{
		if (Cardboard.sdk == null)
		{
			Cardboard.sdk = this;
		}
		if (Cardboard.sdk != this)
		{
			Debug.LogError("There must be only one Cardboard object in a scene.");
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		Screen.sleepTimeout = -1;
		this.InitDevice();
		this.StereoScreen = null;
		this.AddCardboardCamera();
	}

	// Token: 0x060000AE RID: 174 RVA: 0x000054BD File Offset: 0x000038BD
	private void Start()
	{
		this.UILayerEnabled = true;
	}

	// Token: 0x060000AF RID: 175 RVA: 0x000054C8 File Offset: 0x000038C8
	private void AddCardboardCamera()
	{
		CardboardPreRender x = UnityEngine.Object.FindObjectOfType<CardboardPreRender>();
		if (x == null)
		{
			GameObject gameObject = new GameObject("PreRender", new Type[]
			{
				typeof(CardboardPreRender)
			});
			gameObject.SendMessage("Reset");
			gameObject.transform.parent = base.transform;
		}
		CardboardPostRender x2 = UnityEngine.Object.FindObjectOfType<CardboardPostRender>();
		if (x2 == null)
		{
			GameObject gameObject2 = new GameObject("PostRender", new Type[]
			{
				typeof(CardboardPostRender)
			});
			gameObject2.SendMessage("Reset");
			gameObject2.transform.parent = base.transform;
		}
	}

	// Token: 0x14000002 RID: 2
	// (add) Token: 0x060000B0 RID: 176 RVA: 0x00005570 File Offset: 0x00003970
	// (remove) Token: 0x060000B1 RID: 177 RVA: 0x000055A8 File Offset: 0x000039A8
	public event Action OnTrigger;

	// Token: 0x14000003 RID: 3
	// (add) Token: 0x060000B2 RID: 178 RVA: 0x000055E0 File Offset: 0x000039E0
	// (remove) Token: 0x060000B3 RID: 179 RVA: 0x00005618 File Offset: 0x00003A18
	public event Action OnTilt;

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x060000B4 RID: 180 RVA: 0x00005650 File Offset: 0x00003A50
	// (remove) Token: 0x060000B5 RID: 181 RVA: 0x00005688 File Offset: 0x00003A88
	public event Action OnProfileChange;

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x060000B6 RID: 182 RVA: 0x000056C0 File Offset: 0x00003AC0
	// (remove) Token: 0x060000B7 RID: 183 RVA: 0x000056F8 File Offset: 0x00003AF8
	public event Action OnBackButton;

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x060000B8 RID: 184 RVA: 0x0000572E File Offset: 0x00003B2E
	// (set) Token: 0x060000B9 RID: 185 RVA: 0x00005736 File Offset: 0x00003B36
	public bool Triggered { get; private set; }

	// Token: 0x17000021 RID: 33
	// (get) Token: 0x060000BA RID: 186 RVA: 0x0000573F File Offset: 0x00003B3F
	// (set) Token: 0x060000BB RID: 187 RVA: 0x00005747 File Offset: 0x00003B47
	public bool Tilted { get; private set; }

	// Token: 0x17000022 RID: 34
	// (get) Token: 0x060000BC RID: 188 RVA: 0x00005750 File Offset: 0x00003B50
	// (set) Token: 0x060000BD RID: 189 RVA: 0x00005758 File Offset: 0x00003B58
	public bool ProfileChanged { get; private set; }

	// Token: 0x17000023 RID: 35
	// (get) Token: 0x060000BE RID: 190 RVA: 0x00005761 File Offset: 0x00003B61
	// (set) Token: 0x060000BF RID: 191 RVA: 0x00005769 File Offset: 0x00003B69
	public bool BackButtonPressed { get; private set; }

	// Token: 0x060000C0 RID: 192 RVA: 0x00005772 File Offset: 0x00003B72
	public void UpdateState()
	{
		if (this.updatedToFrame != Time.frameCount)
		{
			this.updatedToFrame = Time.frameCount;
			Cardboard.device.UpdateState();
			this.DispatchEvents();
		}
	}

	// Token: 0x060000C1 RID: 193 RVA: 0x000057A0 File Offset: 0x00003BA0
	private void DispatchEvents()
	{
		this.Triggered = (Cardboard.device.triggered || Input.GetMouseButtonDown(0));
		this.Tilted = Cardboard.device.tilted;
		this.ProfileChanged = Cardboard.device.profileChanged;
		this.BackButtonPressed = (Cardboard.device.backButtonPressed || Input.GetKeyDown(KeyCode.Escape));
		Cardboard.device.triggered = false;
		Cardboard.device.tilted = false;
		Cardboard.device.profileChanged = false;
		Cardboard.device.backButtonPressed = false;
		if (this.Tilted && this.OnTilt != null)
		{
			this.OnTilt();
		}
		if (this.Triggered && this.OnTrigger != null)
		{
			this.OnTrigger();
		}
		if (this.ProfileChanged && this.OnProfileChange != null)
		{
			this.OnProfileChange();
		}
		if (this.BackButtonPressed && this.OnBackButton != null)
		{
			this.OnBackButton();
		}
	}

	// Token: 0x060000C2 RID: 194 RVA: 0x000058BA File Offset: 0x00003CBA
	public void PostRender()
	{
		if (this.NativeDistortionCorrectionSupported)
		{
			Cardboard.device.PostRender();
		}
	}

	// Token: 0x060000C3 RID: 195 RVA: 0x000058D1 File Offset: 0x00003CD1
	public void Recenter()
	{
		Cardboard.device.Recenter();
	}

	// Token: 0x060000C4 RID: 196 RVA: 0x000058DD File Offset: 0x00003CDD
	public void SetTouchCoordinates(int x, int y)
	{
		Cardboard.device.SetTouchCoordinates(x, y);
	}

	// Token: 0x060000C5 RID: 197 RVA: 0x000058EB File Offset: 0x00003CEB
	public void ShowSettingsDialog()
	{
		Cardboard.device.ShowSettingsDialog();
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x000058F7 File Offset: 0x00003CF7
	private void OnEnable()
	{
		Cardboard.device.OnPause(false);
	}

	// Token: 0x060000C7 RID: 199 RVA: 0x00005904 File Offset: 0x00003D04
	private void OnDisable()
	{
		Cardboard.device.OnPause(true);
	}

	// Token: 0x060000C8 RID: 200 RVA: 0x00005911 File Offset: 0x00003D11
	private void OnApplicationPause(bool pause)
	{
		Cardboard.device.OnPause(pause);
	}

	// Token: 0x060000C9 RID: 201 RVA: 0x0000591E File Offset: 0x00003D1E
	private void OnApplicationFocus(bool focus)
	{
		Cardboard.device.OnFocus(focus);
	}

	// Token: 0x060000CA RID: 202 RVA: 0x0000592B File Offset: 0x00003D2B
	private void OnLevelWasLoaded(int level)
	{
		Cardboard.device.OnLevelLoaded(level);
	}

	// Token: 0x060000CB RID: 203 RVA: 0x00005938 File Offset: 0x00003D38
	private void OnApplicationQuit()
	{
		Cardboard.device.OnApplicationQuit();
	}

	// Token: 0x060000CC RID: 204 RVA: 0x00005944 File Offset: 0x00003D44
	private void OnDestroy()
	{
		this.VRModeEnabled = false;
		this.UILayerEnabled = false;
		if (Cardboard.device != null)
		{
			Cardboard.device.Destroy();
		}
		if (Cardboard.sdk == this)
		{
			Cardboard.sdk = null;
		}
	}

	// Token: 0x17000024 RID: 36
	// (get) Token: 0x060000CD RID: 205 RVA: 0x0000597E File Offset: 0x00003D7E
	// (set) Token: 0x060000CE RID: 206 RVA: 0x00005989 File Offset: 0x00003D89
	[Obsolete("Use DistortionCorrection instead.")]
	public bool nativeDistortionCorrection
	{
		get
		{
			return this.DistortionCorrection == Cardboard.DistortionCorrectionMethod.Native;
		}
		set
		{
			this.DistortionCorrection = ((!value) ? Cardboard.DistortionCorrectionMethod.None : Cardboard.DistortionCorrectionMethod.Native);
		}
	}

	// Token: 0x17000025 RID: 37
	// (get) Token: 0x060000CF RID: 207 RVA: 0x0000599E File Offset: 0x00003D9E
	[Obsolete("InCardboard is deprecated.")]
	public bool InCardboard
	{
		get
		{
			return true;
		}
	}

	// Token: 0x17000026 RID: 38
	// (get) Token: 0x060000D0 RID: 208 RVA: 0x000059A1 File Offset: 0x00003DA1
	[Obsolete("Use Triggered instead.")]
	public bool CardboardTriggered
	{
		get
		{
			return this.Triggered;
		}
	}

	// Token: 0x17000027 RID: 39
	// (get) Token: 0x060000D1 RID: 209 RVA: 0x000059A9 File Offset: 0x00003DA9
	[Obsolete("Use HeadPose instead.")]
	public Matrix4x4 HeadView
	{
		get
		{
			return this.HeadPose.Matrix;
		}
	}

	// Token: 0x17000028 RID: 40
	// (get) Token: 0x060000D2 RID: 210 RVA: 0x000059B6 File Offset: 0x00003DB6
	[Obsolete("Use HeadPose instead.")]
	public Quaternion HeadRotation
	{
		get
		{
			return this.HeadPose.Orientation;
		}
	}

	// Token: 0x17000029 RID: 41
	// (get) Token: 0x060000D3 RID: 211 RVA: 0x000059C3 File Offset: 0x00003DC3
	[Obsolete("Use HeadPose instead.")]
	public Vector3 HeadPosition
	{
		get
		{
			return this.HeadPose.Position;
		}
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x000059D0 File Offset: 0x00003DD0
	[Obsolete("Use EyePose() instead.")]
	public Matrix4x4 EyeView(Cardboard.Eye eye)
	{
		return this.EyePose(eye).Matrix;
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x000059DE File Offset: 0x00003DDE
	[Obsolete("Use EyePose() instead.")]
	public Vector3 EyeOffset(Cardboard.Eye eye)
	{
		return this.EyePose(eye).Position;
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x000059EC File Offset: 0x00003DEC
	[Obsolete("Use Projection() instead.")]
	public Matrix4x4 UndistortedProjection(Cardboard.Eye eye)
	{
		return this.Projection(eye, Cardboard.Distortion.Undistorted);
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x000059F6 File Offset: 0x00003DF6
	[Obsolete("Use Viewport() instead.")]
	public Rect EyeRect(Cardboard.Eye eye)
	{
		return this.Viewport(eye, Cardboard.Distortion.Distorted);
	}

	// Token: 0x1700002A RID: 42
	// (get) Token: 0x060000D8 RID: 216 RVA: 0x00005A00 File Offset: 0x00003E00
	[Obsolete("Use ComfortableViewingRange instead.")]
	public float MinimumComfortDistance
	{
		get
		{
			return this.ComfortableViewingRange.x;
		}
	}

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x060000D9 RID: 217 RVA: 0x00005A1C File Offset: 0x00003E1C
	[Obsolete("Use ComfortableViewingRange instead.")]
	public float MaximumComfortDistance
	{
		get
		{
			return this.ComfortableViewingRange.y;
		}
	}

	// Token: 0x040000EA RID: 234
	public const string CARDBOARD_SDK_VERSION = "0.6";

	// Token: 0x040000EB RID: 235
	private static Cardboard sdk;

	// Token: 0x040000EC RID: 236
	private static Camera currentMainCamera;

	// Token: 0x040000ED RID: 237
	private static StereoController currentController;

	// Token: 0x040000EE RID: 238
	private bool uiLayerEnabled;

	// Token: 0x040000EF RID: 239
	[SerializeField]
	private bool vrModeEnabled = true;

	// Token: 0x040000F0 RID: 240
	[SerializeField]
	private Cardboard.DistortionCorrectionMethod distortionCorrection = Cardboard.DistortionCorrectionMethod.Unity;

	// Token: 0x040000F1 RID: 241
	[SerializeField]
	private bool enableAlignmentMarker = true;

	// Token: 0x040000F2 RID: 242
	[SerializeField]
	private bool enableSettingsButton = true;

	// Token: 0x040000F3 RID: 243
	[SerializeField]
	private Cardboard.BackButtonModes backButtonMode = Cardboard.BackButtonModes.OnlyInVR;

	// Token: 0x040000F4 RID: 244
	[SerializeField]
	private bool tapIsTrigger = true;

	// Token: 0x040000F5 RID: 245
	[SerializeField]
	private float neckModelScale;

	// Token: 0x040000F6 RID: 246
	[SerializeField]
	private bool autoDriftCorrection = true;

	// Token: 0x040000F7 RID: 247
	[SerializeField]
	private bool electronicDisplayStabilization;

	// Token: 0x040000F8 RID: 248
	private static BaseVRDevice device;

	// Token: 0x040000FB RID: 251
	[SerializeField]
	private float stereoScreenScale = 1f;

	// Token: 0x040000FC RID: 252
	private static RenderTexture stereoScreen;

	// Token: 0x040000FE RID: 254
	private readonly Vector2 defaultComfortableViewingRange = new Vector2(0.4f, 100000f);

	// Token: 0x040000FF RID: 255
	public Uri DefaultDeviceProfile;

	// Token: 0x04000108 RID: 264
	private int updatedToFrame;

	// Token: 0x02000012 RID: 18
	public enum DistortionCorrectionMethod
	{
		// Token: 0x0400010A RID: 266
		None,
		// Token: 0x0400010B RID: 267
		Native,
		// Token: 0x0400010C RID: 268
		Unity
	}

	// Token: 0x02000013 RID: 19
	public enum BackButtonModes
	{
		// Token: 0x0400010E RID: 270
		Off,
		// Token: 0x0400010F RID: 271
		OnlyInVR,
		// Token: 0x04000110 RID: 272
		On
	}

	// Token: 0x02000014 RID: 20
	// (Invoke) Token: 0x060000DC RID: 220
	public delegate void StereoScreenChangeDelegate(RenderTexture newStereoScreen);

	// Token: 0x02000015 RID: 21
	public enum Eye
	{
		// Token: 0x04000112 RID: 274
		Left,
		// Token: 0x04000113 RID: 275
		Right,
		// Token: 0x04000114 RID: 276
		Center
	}

	// Token: 0x02000016 RID: 22
	public enum Distortion
	{
		// Token: 0x04000116 RID: 278
		Distorted,
		// Token: 0x04000117 RID: 279
		Undistorted
	}
}
