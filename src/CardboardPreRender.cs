using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/CardboardPreRender")]
public class CardboardPreRender : MonoBehaviour
{
	public Camera cam { get; private set; }

	private void Awake()
	{
		cam = GetComponent<Camera>();
	}

	private void Reset()
	{
		cam.clearFlags = CameraClearFlags.Color;
		cam.backgroundColor = Color.black;
		cam.cullingMask = 0;
		cam.useOcclusionCulling = false;
		cam.depth = -100f;
	}

	private void OnPreCull()
	{
		Cardboard.SDK.UpdateState();
		if (Cardboard.SDK.ProfileChanged)
		{
			SetShaderGlobals();
		}
		cam.clearFlags = ((!Cardboard.SDK.VRModeEnabled) ? CameraClearFlags.Nothing : CameraClearFlags.Color);
		RenderTexture stereoScreen = Cardboard.SDK.StereoScreen;
		if (stereoScreen != null && !stereoScreen.IsCreated())
		{
			stereoScreen.Create();
		}
	}

	private void SetShaderGlobals()
	{
		CardboardProfile profile = Cardboard.SDK.Profile;
		Shader.SetGlobalVector("_Undistortion", new Vector4(profile.device.inverse.k1, profile.device.inverse.k2));
		Shader.SetGlobalVector("_Distortion", new Vector4(profile.device.distortion.k1, profile.device.distortion.k2));
		float[] array = new float[4];
		profile.GetLeftEyeVisibleTanAngles(array);
		float maxRadius = CardboardProfile.GetMaxRadius(array);
		Shader.SetGlobalFloat("_MaxRadSq", maxRadius * maxRadius);
	}
}
