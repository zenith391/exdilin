using System;
using UnityEngine;

// Token: 0x02000017 RID: 23
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/CardboardEye")]
public class CardboardEye : MonoBehaviour
{
	// Token: 0x1700002C RID: 44
	// (get) Token: 0x060000E0 RID: 224 RVA: 0x00005A58 File Offset: 0x00003E58
	public StereoController Controller
	{
		get
		{
			if (base.transform.parent == null)
			{
				return null;
			}
			if ((Application.isEditor && !Application.isPlaying) || this.controller == null)
			{
				return base.transform.parent.GetComponentInParent<StereoController>();
			}
			return this.controller;
		}
	}

	// Token: 0x1700002D RID: 45
	// (get) Token: 0x060000E1 RID: 225 RVA: 0x00005AB9 File Offset: 0x00003EB9
	public CardboardHead Head
	{
		get
		{
			return base.GetComponentInParent<CardboardHead>();
		}
	}

	// Token: 0x1700002E RID: 46
	// (get) Token: 0x060000E2 RID: 226 RVA: 0x00005AC1 File Offset: 0x00003EC1
	// (set) Token: 0x060000E3 RID: 227 RVA: 0x00005AC9 File Offset: 0x00003EC9
	public Camera cam { get; private set; }

	// Token: 0x060000E4 RID: 228 RVA: 0x00005AD2 File Offset: 0x00003ED2
	private void Awake()
	{
		this.cam = base.GetComponent<Camera>();
	}

	// Token: 0x060000E5 RID: 229 RVA: 0x00005AE0 File Offset: 0x00003EE0
	private void Start()
	{
		StereoController x = this.Controller;
		if (x == null)
		{
			Debug.LogError("CardboardEye must be child of a StereoController.");
			base.enabled = false;
			return;
		}
		this.controller = x;
		this.monoCamera = this.controller.GetComponent<Camera>();
		this.UpdateStereoValues();
	}

	// Token: 0x060000E6 RID: 230 RVA: 0x00005B30 File Offset: 0x00003F30
	private void FixProjection(ref Matrix4x4 proj)
	{
		ref Matrix4x4 ptr = ref proj;
		proj[0, 0] = ptr[0, 0] * (this.cam.rect.height / this.cam.rect.width / 2f);
		float nearClipPlane = this.monoCamera.nearClipPlane;
		float farClipPlane = this.monoCamera.farClipPlane;
		proj[2, 2] = (nearClipPlane + farClipPlane) / (nearClipPlane - farClipPlane);
		proj[2, 3] = 2f * nearClipPlane * farClipPlane / (nearClipPlane - farClipPlane);
	}

	// Token: 0x060000E7 RID: 231 RVA: 0x00005BC0 File Offset: 0x00003FC0
	private Rect FixViewport(Rect rect)
	{
		Rect rect2 = Cardboard.SDK.Viewport(this.eye, Cardboard.Distortion.Distorted);
		if (this.eye == Cardboard.Eye.Right)
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

	// Token: 0x060000E8 RID: 232 RVA: 0x00005D20 File Offset: 0x00004120
	public void UpdateStereoValues()
	{
		Matrix4x4 projectionMatrix = Cardboard.SDK.Projection(this.eye, Cardboard.Distortion.Distorted);
		this.realProj = Cardboard.SDK.Projection(this.eye, Cardboard.Distortion.Undistorted);
		this.CopyCameraAndMakeSideBySide(this.controller, projectionMatrix[0, 2], projectionMatrix[1, 2]);
		this.FixProjection(ref projectionMatrix);
		this.FixProjection(ref this.realProj);
		float t = Mathf.Clamp01(this.controller.matchByZoom) * Mathf.Clamp01(this.controller.matchMonoFOV);
		float num = this.monoCamera.projectionMatrix[1, 1];
		float num2 = 1f / Mathf.Lerp(1f / projectionMatrix[1, 1], 1f / num, t) / projectionMatrix[1, 1];
		ref Matrix4x4 ptr = ref projectionMatrix;
		projectionMatrix[0, 0] = ptr[0, 0] * num2;
		ptr = ref projectionMatrix;
		projectionMatrix[1, 1] = ptr[1, 1] * num2;
		this.cam.projectionMatrix = projectionMatrix;
		if (Application.isEditor)
		{
			this.cam.fieldOfView = 2f * Mathf.Atan(1f / projectionMatrix[1, 1]) * 57.29578f;
		}
		this.cam.targetTexture = (this.monoCamera.targetTexture ?? Cardboard.SDK.StereoScreen);
		if (this.cam.targetTexture == null)
		{
			this.cam.rect = this.FixViewport(this.cam.rect);
		}
	}

	// Token: 0x060000E9 RID: 233 RVA: 0x00005EB8 File Offset: 0x000042B8
	private void SetupStereo()
	{
		Cardboard.SDK.UpdateState();
		bool flag = this.controller.centerOfInterest != null && this.controller.centerOfInterest.gameObject.activeInHierarchy;
		bool flag2 = flag || this.interpPosition < 1f;
		if (this.controller.keepStereoUpdated || Cardboard.SDK.ProfileChanged || (this.cam.targetTexture == null && Cardboard.SDK.StereoScreen != null))
		{
			this.UpdateStereoValues();
			flag2 = true;
		}
		if (flag2)
		{
			float proj = this.cam.projectionMatrix[1, 1];
			float z = base.transform.lossyScale.z;
			Vector3 b = this.controller.ComputeStereoEyePosition(this.eye, proj, z);
			this.interpPosition = ((!this.controller.keepStereoUpdated && !flag) ? 1f : (Time.deltaTime / (this.controller.stereoAdjustSmoothing + Time.deltaTime)));
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, b, this.interpPosition);
		}
		if (Cardboard.SDK.DistortionCorrection == Cardboard.DistortionCorrectionMethod.None)
		{
			Matrix4x4 mat = this.cam.cameraToWorldMatrix * Matrix4x4.Inverse(this.cam.projectionMatrix) * this.realProj;
			Shader.SetGlobalMatrix("_RealProjection", this.realProj);
			Shader.SetGlobalMatrix("_FixProjection", mat);
		}
		else
		{
			Shader.SetGlobalMatrix("_RealProjection", this.cam.projectionMatrix);
			Shader.SetGlobalMatrix("_FixProjection", this.cam.cameraToWorldMatrix);
		}
		Shader.SetGlobalFloat("_NearClip", this.cam.nearClipPlane);
	}

	// Token: 0x060000EA RID: 234 RVA: 0x000060AC File Offset: 0x000044AC
	private void OnPreCull()
	{
		if (!Cardboard.SDK.VRModeEnabled || !this.monoCamera.enabled)
		{
			this.cam.enabled = false;
			return;
		}
		this.SetupStereo();
		if (!this.controller.directRender && Cardboard.SDK.StereoScreen != null)
		{
			this.stereoEffect = base.GetComponent<StereoRenderEffect>();
			if (this.stereoEffect == null)
			{
				this.stereoEffect = base.gameObject.AddComponent<StereoRenderEffect>();
			}
			this.stereoEffect.enabled = true;
		}
		else if (this.stereoEffect != null)
		{
			this.stereoEffect.enabled = false;
		}
	}

	// Token: 0x060000EB RID: 235 RVA: 0x0000616C File Offset: 0x0000456C
	public void CopyCameraAndMakeSideBySide(StereoController controller, float parx = 0f, float pary = 0f)
	{
		Camera camera = (!(controller == this.controller)) ? controller.GetComponent<Camera>() : this.monoCamera;
		float num = CardboardProfile.Default.device.lenses.separation * controller.stereoMultiplier;
		Vector3 localPosition = (!Application.isPlaying) ? (((this.eye != Cardboard.Eye.Left) ? (num / 2f) : (-num / 2f)) * Vector3.right) : base.transform.localPosition;
		this.cam.CopyFrom(camera);
		this.cam.cullingMask ^= this.toggleCullingMask.value;
		this.cam.depth = camera.depth;
		base.transform.localPosition = localPosition;
		base.transform.localRotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
		Skybox component = camera.GetComponent<Skybox>();
		Skybox skybox = base.GetComponent<Skybox>();
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
			UnityEngine.Object.Destroy(skybox);
		}
		Rect rect = this.cam.rect;
		Vector2 center = rect.center;
		center.x = Mathf.Lerp(center.x, 0.5f, Mathf.Clamp01(controller.stereoPaddingX));
		center.y = Mathf.Lerp(center.y, 0.5f, Mathf.Clamp01(controller.stereoPaddingY));
		rect.center = center;
		float num2 = Mathf.SmoothStep(-0.5f, 0.5f, (rect.width + 1f) / 2f);
		rect.x += (rect.width - num2) / 2f;
		rect.width = num2;
		rect.x *= (0.5f - rect.width) / (1f - rect.width);
		if (this.eye == Cardboard.Eye.Right)
		{
			rect.x += 0.5f;
		}
		float num3 = Mathf.Clamp01(controller.screenParallax);
		if (camera.rect.width < 1f && num3 > 0f)
		{
			rect.x -= parx / 4f * num3;
			rect.y -= pary / 2f * num3;
		}
		this.cam.rect = rect;
	}

	// Token: 0x04000118 RID: 280
	public Cardboard.Eye eye;

	// Token: 0x04000119 RID: 281
	[Tooltip("Culling mask layers that this eye should toggle relative to the parent camera.")]
	public LayerMask toggleCullingMask = 0;

	// Token: 0x0400011A RID: 282
	private StereoController controller;

	// Token: 0x0400011B RID: 283
	private StereoRenderEffect stereoEffect;

	// Token: 0x0400011C RID: 284
	private Camera monoCamera;

	// Token: 0x0400011D RID: 285
	private Matrix4x4 realProj;

	// Token: 0x0400011E RID: 286
	private float interpPosition = 1f;
}
