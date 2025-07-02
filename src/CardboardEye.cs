using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/CardboardEye")]
public class CardboardEye : MonoBehaviour
{
	public Cardboard.Eye eye;

	[Tooltip("Culling mask layers that this eye should toggle relative to the parent camera.")]
	public LayerMask toggleCullingMask = 0;

	private StereoController controller;

	private StereoRenderEffect stereoEffect;

	private Camera monoCamera;

	private Matrix4x4 realProj;

	private float interpPosition = 1f;

	public StereoController Controller
	{
		get
		{
			if (base.transform.parent == null)
			{
				return null;
			}
			if ((Application.isEditor && !Application.isPlaying) || controller == null)
			{
				return base.transform.parent.GetComponentInParent<StereoController>();
			}
			return controller;
		}
	}

	public CardboardHead Head => GetComponentInParent<CardboardHead>();

	public Camera cam { get; private set; }

	private void Awake()
	{
		cam = GetComponent<Camera>();
	}

	private void Start()
	{
		StereoController stereoController = Controller;
		if (stereoController == null)
		{
			Debug.LogError("CardboardEye must be child of a StereoController.");
			base.enabled = false;
		}
		else
		{
			controller = stereoController;
			monoCamera = controller.GetComponent<Camera>();
			UpdateStereoValues();
		}
	}

	private void FixProjection(ref Matrix4x4 proj)
	{
		proj[0, 0] *= cam.rect.height / cam.rect.width / 2f;
		float nearClipPlane = monoCamera.nearClipPlane;
		float farClipPlane = monoCamera.farClipPlane;
		proj[2, 2] = (nearClipPlane + farClipPlane) / (nearClipPlane - farClipPlane);
		proj[2, 3] = 2f * nearClipPlane * farClipPlane / (nearClipPlane - farClipPlane);
	}

	private Rect FixViewport(Rect rect)
	{
		Rect rect2 = Cardboard.SDK.Viewport(eye);
		if (eye == Cardboard.Eye.Right)
		{
			rect.x -= 0.5f;
		}
		rect.width *= 2f * rect2.width;
		rect.x = rect2.x + 2f * rect.x * rect2.width;
		rect.height *= rect2.height;
		rect.y = rect2.y + rect.y * rect2.height;
		if (Application.isEditor)
		{
			float num = (float)Screen.width / (float)Screen.height;
			float num2 = Cardboard.SDK.Profile.screen.width / Cardboard.SDK.Profile.screen.height;
			float num3 = num2 / num;
			if (num3 < 1f)
			{
				rect.width *= num3;
				rect.x *= num3;
				rect.x += (1f - num3) / 2f;
			}
			else
			{
				rect.height /= num3;
				rect.y /= num3;
			}
		}
		return rect;
	}

	public void UpdateStereoValues()
	{
		Matrix4x4 proj = Cardboard.SDK.Projection(eye);
		realProj = Cardboard.SDK.Projection(eye, Cardboard.Distortion.Undistorted);
		CopyCameraAndMakeSideBySide(controller, proj[0, 2], proj[1, 2]);
		FixProjection(ref proj);
		FixProjection(ref realProj);
		float t = Mathf.Clamp01(controller.matchByZoom) * Mathf.Clamp01(controller.matchMonoFOV);
		float num = monoCamera.projectionMatrix[1, 1];
		float num2 = 1f / Mathf.Lerp(1f / proj[1, 1], 1f / num, t) / proj[1, 1];
		proj[0, 0] *= num2;
		proj[1, 1] *= num2;
		cam.projectionMatrix = proj;
		if (Application.isEditor)
		{
			cam.fieldOfView = 2f * Mathf.Atan(1f / proj[1, 1]) * 57.29578f;
		}
		cam.targetTexture = monoCamera.targetTexture ?? Cardboard.SDK.StereoScreen;
		if (cam.targetTexture == null)
		{
			cam.rect = FixViewport(cam.rect);
		}
	}

	private void SetupStereo()
	{
		Cardboard.SDK.UpdateState();
		bool flag = controller.centerOfInterest != null && controller.centerOfInterest.gameObject.activeInHierarchy;
		bool flag2 = flag || interpPosition < 1f;
		if (controller.keepStereoUpdated || Cardboard.SDK.ProfileChanged || (cam.targetTexture == null && Cardboard.SDK.StereoScreen != null))
		{
			UpdateStereoValues();
			flag2 = true;
		}
		if (flag2)
		{
			float proj = cam.projectionMatrix[1, 1];
			float z = base.transform.lossyScale.z;
			Vector3 b = controller.ComputeStereoEyePosition(eye, proj, z);
			interpPosition = ((!controller.keepStereoUpdated && !flag) ? 1f : (Time.deltaTime / (controller.stereoAdjustSmoothing + Time.deltaTime)));
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, b, interpPosition);
		}
		if (Cardboard.SDK.DistortionCorrection == Cardboard.DistortionCorrectionMethod.None)
		{
			Matrix4x4 mat = cam.cameraToWorldMatrix * Matrix4x4.Inverse(cam.projectionMatrix) * realProj;
			Shader.SetGlobalMatrix("_RealProjection", realProj);
			Shader.SetGlobalMatrix("_FixProjection", mat);
		}
		else
		{
			Shader.SetGlobalMatrix("_RealProjection", cam.projectionMatrix);
			Shader.SetGlobalMatrix("_FixProjection", cam.cameraToWorldMatrix);
		}
		Shader.SetGlobalFloat("_NearClip", cam.nearClipPlane);
	}

	private void OnPreCull()
	{
		if (!Cardboard.SDK.VRModeEnabled || !monoCamera.enabled)
		{
			cam.enabled = false;
			return;
		}
		SetupStereo();
		if (!controller.directRender && Cardboard.SDK.StereoScreen != null)
		{
			stereoEffect = GetComponent<StereoRenderEffect>();
			if (stereoEffect == null)
			{
				stereoEffect = base.gameObject.AddComponent<StereoRenderEffect>();
			}
			stereoEffect.enabled = true;
		}
		else if (stereoEffect != null)
		{
			stereoEffect.enabled = false;
		}
	}

	public void CopyCameraAndMakeSideBySide(StereoController controller, float parx = 0f, float pary = 0f)
	{
		Camera camera = ((!(controller == this.controller)) ? controller.GetComponent<Camera>() : monoCamera);
		float num = CardboardProfile.Default.device.lenses.separation * controller.stereoMultiplier;
		Vector3 localPosition = ((!Application.isPlaying) ? (((eye != Cardboard.Eye.Left) ? (num / 2f) : ((0f - num) / 2f)) * Vector3.right) : base.transform.localPosition);
		cam.CopyFrom(camera);
		cam.cullingMask ^= toggleCullingMask.value;
		cam.depth = camera.depth;
		base.transform.localPosition = localPosition;
		base.transform.localRotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
		Skybox component = camera.GetComponent<Skybox>();
		Skybox skybox = GetComponent<Skybox>();
		if (component != null)
		{
			if (skybox == null)
			{
				skybox = base.gameObject.AddComponent<Skybox>();
			}
			skybox.material = component.material;
		}
		else if (skybox != null)
		{
			Object.Destroy(skybox);
		}
		Rect rect = cam.rect;
		Vector2 center = rect.center;
		center.x = Mathf.Lerp(center.x, 0.5f, Mathf.Clamp01(controller.stereoPaddingX));
		center.y = Mathf.Lerp(center.y, 0.5f, Mathf.Clamp01(controller.stereoPaddingY));
		rect.center = center;
		float num2 = Mathf.SmoothStep(-0.5f, 0.5f, (rect.width + 1f) / 2f);
		rect.x += (rect.width - num2) / 2f;
		rect.width = num2;
		rect.x *= (0.5f - rect.width) / (1f - rect.width);
		if (eye == Cardboard.Eye.Right)
		{
			rect.x += 0.5f;
		}
		float num3 = Mathf.Clamp01(controller.screenParallax);
		if (camera.rect.width < 1f && num3 > 0f)
		{
			rect.x -= parx / 4f * num3;
			rect.y -= pary / 2f * num3;
		}
		cam.rect = rect;
	}
}
