using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/StereoController")]
public class StereoController : MonoBehaviour
{
	[Tooltip("Whether to draw directly to the output window (true), or to an offscreen buffer first and then blit (false).  Image  Effects and Deferred Lighting may only work if set to false.")]
	public bool directRender = true;

	[Tooltip("When enabled, UpdateStereoValues() is called every frame to keep the stereo cameras completely synchronized with both the mono camera and the device profile. It is better for performance to leave this option disabled whenever possible.")]
	public bool keepStereoUpdated;

	[Tooltip("Set the stereo level for this camera.")]
	[Range(0f, 1f)]
	public float stereoMultiplier = 1f;

	[Tooltip("How much to adjust the stereo field of view to match this camera.")]
	[Range(0f, 1f)]
	public float matchMonoFOV;

	[Tooltip("Whether to adjust FOV by moving the eyes (0) or simply zooming (1).")]
	[Range(0f, 1f)]
	public float matchByZoom;

	[Tooltip("Object or point where field of view matching is done.")]
	public Transform centerOfInterest;

	[Tooltip("If COI is an object, its approximate size.")]
	public float radiusOfInterest;

	[Tooltip("Adjust stereo level when COI gets too close or too far.")]
	public bool checkStereoComfort = true;

	[Tooltip("Smoothing factor to use when adjusting stereo for COI and comfort.")]
	[Range(0f, 1f)]
	public float stereoAdjustSmoothing = 0.1f;

	[Tooltip("Adjust the virtual depth of this camera's window (picture-in-picture only).")]
	[Range(0f, 1f)]
	public float screenParallax;

	[Tooltip("Move the camera window horizontally towards the center of the screen (PIP only).")]
	[Range(0f, 1f)]
	public float stereoPaddingX;

	[Tooltip("Move the camera window vertically towards the center of the screen (PIP only).")]
	[Range(0f, 1f)]
	public float stereoPaddingY;

	private bool renderedStereo;

	private CardboardEye[] eyes;

	private CardboardHead head;

	public CardboardEye[] Eyes
	{
		get
		{
			if (eyes == null)
			{
				eyes = (from eye in GetComponentsInChildren<CardboardEye>(includeInactive: true)
					where eye.Controller == this
					select eye).ToArray();
			}
			return eyes;
		}
	}

	public CardboardHead Head
	{
		get
		{
			if (head == null)
			{
				head = Eyes.Select((CardboardEye eye) => eye.Head).FirstOrDefault();
			}
			return head;
		}
	}

	public Camera cam { get; private set; }

	public void InvalidateEyes()
	{
		eyes = null;
		head = null;
	}

	public void UpdateStereoValues()
	{
		CardboardEye[] array = Eyes;
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i].UpdateStereoValues();
		}
	}

	private void Awake()
	{
		Cardboard.Create();
		cam = GetComponent<Camera>();
		AddStereoRig();
	}

	public void AddStereoRig()
	{
		IEnumerable<CardboardEye> source = from eye in GetComponentsInChildren<CardboardEye>(includeInactive: true)
			where eye.Controller == this
			select eye;
		if (!source.Any())
		{
			CreateEye(Cardboard.Eye.Left);
			CreateEye(Cardboard.Eye.Right);
			if (Head == null)
			{
				CardboardHead cardboardHead = base.gameObject.AddComponent<CardboardHead>();
				cardboardHead.trackPosition = false;
			}
		}
	}

	private void CreateEye(Cardboard.Eye eye)
	{
		string text = base.name + ((eye != Cardboard.Eye.Left) ? " Right" : " Left");
		GameObject gameObject = new GameObject(text);
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject.AddComponent<Camera>().enabled = false;
		CardboardEye cardboardEye = gameObject.AddComponent<CardboardEye>();
		cardboardEye.eye = eye;
		cardboardEye.CopyCameraAndMakeSideBySide(this);
	}

	public Vector3 ComputeStereoEyePosition(Cardboard.Eye eye, float proj11, float zScale)
	{
		if (centerOfInterest == null || !centerOfInterest.gameObject.activeInHierarchy)
		{
			return Cardboard.SDK.EyePose(eye).Position * stereoMultiplier;
		}
		float num = ((!(centerOfInterest != null)) ? 0f : (centerOfInterest.position - base.transform.position).magnitude);
		float num2 = Mathf.Clamp(radiusOfInterest, 0f, num);
		float num3 = proj11 / cam.projectionMatrix[1, 1];
		float num4 = Mathf.Sqrt(num2 * num2 + (num * num - num2 * num2) * num3 * num3);
		float num5 = (num - num4) * Mathf.Clamp01(matchMonoFOV) / zScale;
		float num6 = stereoMultiplier;
		if (checkStereoComfort)
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

	private void OnEnable()
	{
		StartCoroutine("EndOfFrame");
	}

	private void OnDisable()
	{
		StopCoroutine("EndOfFrame");
	}

	private void OnPreCull()
	{
		if (Cardboard.SDK.VRModeEnabled)
		{
			CardboardEye[] array = Eyes;
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				array[i].cam.enabled = true;
			}
			cam.enabled = false;
			renderedStereo = true;
		}
		else
		{
			Cardboard.SDK.UpdateState();
			Shader.SetGlobalMatrix("_RealProjection", cam.projectionMatrix);
			Shader.SetGlobalMatrix("_FixProjection", cam.cameraToWorldMatrix);
			Shader.SetGlobalFloat("_NearClip", cam.nearClipPlane);
		}
	}

	private IEnumerator EndOfFrame()
	{
		while (true)
		{
			if (renderedStereo)
			{
				cam.enabled = true;
				renderedStereo = false;
			}
			yield return new WaitForEndOfFrame();
		}
	}
}
