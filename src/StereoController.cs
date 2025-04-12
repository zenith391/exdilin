using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000027 RID: 39
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/StereoController")]
public class StereoController : MonoBehaviour
{
	// Token: 0x17000037 RID: 55
	// (get) Token: 0x0600013E RID: 318 RVA: 0x00008994 File Offset: 0x00006D94
	public CardboardEye[] Eyes
	{
		get
		{
			if (this.eyes == null)
			{
				this.eyes = (from eye in base.GetComponentsInChildren<CardboardEye>(true)
				where eye.Controller == this
				select eye).ToArray<CardboardEye>();
			}
			return this.eyes;
		}
	}

	// Token: 0x17000038 RID: 56
	// (get) Token: 0x0600013F RID: 319 RVA: 0x000089CC File Offset: 0x00006DCC
	public CardboardHead Head
	{
		get
		{
			if (this.head == null)
			{
				this.head = (from eye in this.Eyes
				select eye.Head).FirstOrDefault<CardboardHead>();
			}
			return this.head;
		}
	}

	// Token: 0x06000140 RID: 320 RVA: 0x00008A23 File Offset: 0x00006E23
	public void InvalidateEyes()
	{
		this.eyes = null;
		this.head = null;
	}

	// Token: 0x06000141 RID: 321 RVA: 0x00008A34 File Offset: 0x00006E34
	public void UpdateStereoValues()
	{
		CardboardEye[] array = this.Eyes;
		int i = 0;
		int num = array.Length;
		while (i < num)
		{
			array[i].UpdateStereoValues();
			i++;
		}
	}

	// Token: 0x17000039 RID: 57
	// (get) Token: 0x06000142 RID: 322 RVA: 0x00008A66 File Offset: 0x00006E66
	// (set) Token: 0x06000143 RID: 323 RVA: 0x00008A6E File Offset: 0x00006E6E
	public Camera cam { get; private set; }

	// Token: 0x06000144 RID: 324 RVA: 0x00008A77 File Offset: 0x00006E77
	private void Awake()
	{
		Cardboard.Create();
		this.cam = base.GetComponent<Camera>();
		this.AddStereoRig();
	}

	// Token: 0x06000145 RID: 325 RVA: 0x00008A90 File Offset: 0x00006E90
	public void AddStereoRig()
	{
		IEnumerable<CardboardEye> source = from eye in base.GetComponentsInChildren<CardboardEye>(true)
		where eye.Controller == this
		select eye;
		if (source.Any<CardboardEye>())
		{
			return;
		}
		this.CreateEye(Cardboard.Eye.Left);
		this.CreateEye(Cardboard.Eye.Right);
		if (this.Head == null)
		{
			CardboardHead cardboardHead = base.gameObject.AddComponent<CardboardHead>();
			cardboardHead.trackPosition = false;
		}
	}

	// Token: 0x06000146 RID: 326 RVA: 0x00008AF4 File Offset: 0x00006EF4
	private void CreateEye(Cardboard.Eye eye)
	{
		string name = base.name + ((eye != Cardboard.Eye.Left) ? " Right" : " Left");
		GameObject gameObject = new GameObject(name);
		gameObject.transform.SetParent(base.transform, false);
		gameObject.AddComponent<Camera>().enabled = false;
		CardboardEye cardboardEye = gameObject.AddComponent<CardboardEye>();
		cardboardEye.eye = eye;
		cardboardEye.CopyCameraAndMakeSideBySide(this, 0f, 0f);
	}

	// Token: 0x06000147 RID: 327 RVA: 0x00008B68 File Offset: 0x00006F68
	public Vector3 ComputeStereoEyePosition(Cardboard.Eye eye, float proj11, float zScale)
	{
		if (this.centerOfInterest == null || !this.centerOfInterest.gameObject.activeInHierarchy)
		{
			return Cardboard.SDK.EyePose(eye).Position * this.stereoMultiplier;
		}
		float num = (!(this.centerOfInterest != null)) ? 0f : (this.centerOfInterest.position - base.transform.position).magnitude;
		float num2 = Mathf.Clamp(this.radiusOfInterest, 0f, num);
		float num3 = proj11 / this.cam.projectionMatrix[1, 1];
		float num4 = Mathf.Sqrt(num2 * num2 + (num * num - num2 * num2) * num3 * num3);
		float num5 = (num - num4) * Mathf.Clamp01(this.matchMonoFOV) / zScale;
		float num6 = this.stereoMultiplier;
		if (this.checkStereoComfort)
		{
			float x = Cardboard.SDK.ComfortableViewingRange.x;
			float y = Cardboard.SDK.ComfortableViewingRange.y;
			if (x < y)
			{
				float num7 = (num - num2) / zScale - num5;
				num6 *= num7 / Mathf.Clamp(num7, x, y);
			}
		}
		return num6 * Cardboard.SDK.EyePose(eye).Position + num5 * Vector3.forward;
	}

	// Token: 0x06000148 RID: 328 RVA: 0x00008CD6 File Offset: 0x000070D6
	private void OnEnable()
	{
		base.StartCoroutine("EndOfFrame");
	}

	// Token: 0x06000149 RID: 329 RVA: 0x00008CE4 File Offset: 0x000070E4
	private void OnDisable()
	{
		base.StopCoroutine("EndOfFrame");
	}

	// Token: 0x0600014A RID: 330 RVA: 0x00008CF4 File Offset: 0x000070F4
	private void OnPreCull()
	{
		if (Cardboard.SDK.VRModeEnabled)
		{
			CardboardEye[] array = this.Eyes;
			int i = 0;
			int num = array.Length;
			while (i < num)
			{
				array[i].cam.enabled = true;
				i++;
			}
			this.cam.enabled = false;
			this.renderedStereo = true;
		}
		else
		{
			Cardboard.SDK.UpdateState();
			Shader.SetGlobalMatrix("_RealProjection", this.cam.projectionMatrix);
			Shader.SetGlobalMatrix("_FixProjection", this.cam.cameraToWorldMatrix);
			Shader.SetGlobalFloat("_NearClip", this.cam.nearClipPlane);
		}
	}

	// Token: 0x0600014B RID: 331 RVA: 0x00008D9C File Offset: 0x0000719C
	private IEnumerator EndOfFrame()
	{
		for (;;)
		{
			if (this.renderedStereo)
			{
				this.cam.enabled = true;
				this.renderedStereo = false;
			}
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	// Token: 0x04000179 RID: 377
	[Tooltip("Whether to draw directly to the output window (true), or to an offscreen buffer first and then blit (false).  Image  Effects and Deferred Lighting may only work if set to false.")]
	public bool directRender = true;

	// Token: 0x0400017A RID: 378
	[Tooltip("When enabled, UpdateStereoValues() is called every frame to keep the stereo cameras completely synchronized with both the mono camera and the device profile. It is better for performance to leave this option disabled whenever possible.")]
	public bool keepStereoUpdated;

	// Token: 0x0400017B RID: 379
	[Tooltip("Set the stereo level for this camera.")]
	[Range(0f, 1f)]
	public float stereoMultiplier = 1f;

	// Token: 0x0400017C RID: 380
	[Tooltip("How much to adjust the stereo field of view to match this camera.")]
	[Range(0f, 1f)]
	public float matchMonoFOV;

	// Token: 0x0400017D RID: 381
	[Tooltip("Whether to adjust FOV by moving the eyes (0) or simply zooming (1).")]
	[Range(0f, 1f)]
	public float matchByZoom;

	// Token: 0x0400017E RID: 382
	[Tooltip("Object or point where field of view matching is done.")]
	public Transform centerOfInterest;

	// Token: 0x0400017F RID: 383
	[Tooltip("If COI is an object, its approximate size.")]
	public float radiusOfInterest;

	// Token: 0x04000180 RID: 384
	[Tooltip("Adjust stereo level when COI gets too close or too far.")]
	public bool checkStereoComfort = true;

	// Token: 0x04000181 RID: 385
	[Tooltip("Smoothing factor to use when adjusting stereo for COI and comfort.")]
	[Range(0f, 1f)]
	public float stereoAdjustSmoothing = 0.1f;

	// Token: 0x04000182 RID: 386
	[Tooltip("Adjust the virtual depth of this camera's window (picture-in-picture only).")]
	[Range(0f, 1f)]
	public float screenParallax;

	// Token: 0x04000183 RID: 387
	[Tooltip("Move the camera window horizontally towards the center of the screen (PIP only).")]
	[Range(0f, 1f)]
	public float stereoPaddingX;

	// Token: 0x04000184 RID: 388
	[Tooltip("Move the camera window vertically towards the center of the screen (PIP only).")]
	[Range(0f, 1f)]
	public float stereoPaddingY;

	// Token: 0x04000185 RID: 389
	private bool renderedStereo;

	// Token: 0x04000186 RID: 390
	private CardboardEye[] eyes;

	// Token: 0x04000187 RID: 391
	private CardboardHead head;
}
