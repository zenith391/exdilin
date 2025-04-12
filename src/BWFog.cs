using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

// Token: 0x0200003A RID: 58
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BWFog : PostEffectsBase
{
	// Token: 0x060001FD RID: 509 RVA: 0x0000B475 File Offset: 0x00009875
	public override bool CheckResources()
	{
		base.CheckSupport(true);
		this.fogMaterial = base.CheckShaderAndCreateMaterial(this.fogShader, this.fogMaterial);
		if (!this.isSupported)
		{
			base.ReportAutoDisable();
		}
		return this.isSupported;
	}

	// Token: 0x060001FE RID: 510 RVA: 0x0000B4B0 File Offset: 0x000098B0
	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!this.CheckResources() || (!this.distanceFog && !this.heightFog))
		{
			Graphics.Blit(source, destination);
			return;
		}
		Camera component = base.GetComponent<Camera>();
		Transform transform = component.transform;
		Vector3[] array = new Vector3[4];
		component.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), component.farClipPlane, component.stereoActiveEye, array);
		Vector3 v = transform.TransformVector(array[0]);
		Vector3 v2 = transform.TransformVector(array[1]);
		Vector3 v3 = transform.TransformVector(array[2]);
		Vector3 v4 = transform.TransformVector(array[3]);
		Matrix4x4 identity = Matrix4x4.identity;
		identity.SetRow(0, v);
		identity.SetRow(1, v4);
		identity.SetRow(2, v2);
		identity.SetRow(3, v3);
		Vector3 position = transform.position;
		float num = position.y - this.height;
		float z = (num > 0f) ? 0f : 1f;
		float y = (!this.excludeFarPixels) ? 2f : 1f;
		this.fogMaterial.SetMatrix("_FrustumCornersWS", identity);
		this.fogMaterial.SetVector("_CameraWS", position);
		this.fogMaterial.SetVector("_HeightParams", new Vector4(this.height, num, z, this.heightDensity * 0.5f));
		this.fogMaterial.SetVector("_DistanceParams", new Vector4(-Mathf.Max(this.startDistance, 0f), y, 0f, 0f));
		FogMode fogMode = RenderSettings.fogMode;
		float fogDensity = RenderSettings.fogDensity;
		float fogStartDistance = RenderSettings.fogStartDistance;
		float fogEndDistance = RenderSettings.fogEndDistance;
		bool flag = fogMode == FogMode.Linear;
		float num2 = (!flag) ? 0f : (fogEndDistance - fogStartDistance);
		float num3 = (Mathf.Abs(num2) <= 0.0001f) ? 0f : (1f / num2);
		Vector4 vector;
		vector.x = fogDensity * 1.2011224f;
		vector.y = fogDensity * 1.442695f;
		vector.z = ((!flag) ? 0f : (-num3));
		vector.w = ((!flag) ? 0f : (fogEndDistance * num3));
		this.fogMaterial.SetVector("_SceneFogParams", vector);
		this.fogMaterial.SetVector("_SceneFogMode", new Vector4((float)fogMode, (float)((!this.useRadialDistance) ? 0 : 1), 0f, 0f));
		float value;
		if (Blocksworld.worldOceanBlock != null && !Blocksworld.worldOceanBlock.isSolid)
		{
			value = Blocksworld.worldOceanBlock.GetWaterBounds().max.y - Mathf.Clamp(position.y / 200f, 0f, 5f);
		}
		else
		{
			value = -1E+07f;
		}
		this.fogMaterial.SetFloat("_WaterLevel", value);
		int pass;
		if (this.distanceFog && this.heightFog)
		{
			pass = 0;
		}
		else if (this.distanceFog)
		{
			pass = 1;
		}
		else
		{
			pass = 2;
		}
		Graphics.Blit(source, destination, this.fogMaterial, pass);
	}

	// Token: 0x040001E2 RID: 482
	[Tooltip("Apply distance-based fog?")]
	public bool distanceFog = true;

	// Token: 0x040001E3 RID: 483
	[Tooltip("Exclude far plane pixels from distance-based fog? (Skybox or clear color)")]
	public bool excludeFarPixels = true;

	// Token: 0x040001E4 RID: 484
	[Tooltip("Distance fog is based on radial distance from camera when checked")]
	public bool useRadialDistance;

	// Token: 0x040001E5 RID: 485
	[Tooltip("Apply height-based fog?")]
	public bool heightFog;

	// Token: 0x040001E6 RID: 486
	[Tooltip("Fog top Y coordinate")]
	public float height = 1f;

	// Token: 0x040001E7 RID: 487
	[Range(0.001f, 10f)]
	public float heightDensity = 2f;

	// Token: 0x040001E8 RID: 488
	[Tooltip("Push fog away from the camera by this amount")]
	public float startDistance;

	// Token: 0x040001E9 RID: 489
	public Shader fogShader;

	// Token: 0x040001EA RID: 490
	private Material fogMaterial;
}
