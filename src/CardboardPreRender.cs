using System;
using UnityEngine;

// Token: 0x0200001B RID: 27
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/CardboardPreRender")]
public class CardboardPreRender : MonoBehaviour
{
	// Token: 0x17000031 RID: 49
	// (get) Token: 0x06000109 RID: 265 RVA: 0x00007081 File Offset: 0x00005481
	// (set) Token: 0x0600010A RID: 266 RVA: 0x00007089 File Offset: 0x00005489
	public Camera cam { get; private set; }

	// Token: 0x0600010B RID: 267 RVA: 0x00007092 File Offset: 0x00005492
	private void Awake()
	{
		this.cam = base.GetComponent<Camera>();
	}

	// Token: 0x0600010C RID: 268 RVA: 0x000070A0 File Offset: 0x000054A0
	private void Reset()
	{
		this.cam.clearFlags = CameraClearFlags.Color;
		this.cam.backgroundColor = Color.black;
		this.cam.cullingMask = 0;
		this.cam.useOcclusionCulling = false;
		this.cam.depth = -100f;
	}

	// Token: 0x0600010D RID: 269 RVA: 0x000070F4 File Offset: 0x000054F4
	private void OnPreCull()
	{
		Cardboard.SDK.UpdateState();
		if (Cardboard.SDK.ProfileChanged)
		{
			this.SetShaderGlobals();
		}
		this.cam.clearFlags = ((!Cardboard.SDK.VRModeEnabled) ? CameraClearFlags.Nothing : CameraClearFlags.Color);
		RenderTexture stereoScreen = Cardboard.SDK.StereoScreen;
		if (stereoScreen != null && !stereoScreen.IsCreated())
		{
			stereoScreen.Create();
		}
	}

	// Token: 0x0600010E RID: 270 RVA: 0x0000716C File Offset: 0x0000556C
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
