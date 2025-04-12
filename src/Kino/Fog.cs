using System;
using UnityEngine;

namespace Kino
{
	// Token: 0x0200002F RID: 47
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Kino Image Effects/Fog")]
	public class Fog : MonoBehaviour
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060001BB RID: 443 RVA: 0x00009EAB File Offset: 0x000082AB
		// (set) Token: 0x060001BC RID: 444 RVA: 0x00009EB3 File Offset: 0x000082B3
		public float startDistance
		{
			get
			{
				return this._startDistance;
			}
			set
			{
				this._startDistance = value;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060001BD RID: 445 RVA: 0x00009EBC File Offset: 0x000082BC
		// (set) Token: 0x060001BE RID: 446 RVA: 0x00009EC4 File Offset: 0x000082C4
		public bool useRadialDistance
		{
			get
			{
				return this._useRadialDistance;
			}
			set
			{
				this._useRadialDistance = value;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00009ECD File Offset: 0x000082CD
		// (set) Token: 0x060001C0 RID: 448 RVA: 0x00009ED5 File Offset: 0x000082D5
		public bool fadeToSkybox
		{
			get
			{
				return this._fadeToSkybox;
			}
			set
			{
				this._fadeToSkybox = value;
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00009EDE File Offset: 0x000082DE
		private void OnEnable()
		{
			base.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00009EF4 File Offset: 0x000082F4
		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (this._material == null)
			{
				this._material = new Material(this._shader);
				this._material.hideFlags = HideFlags.DontSave;
			}
			this._startDistance = Mathf.Max(this._startDistance, 0f);
			this._material.SetFloat("_DistanceOffset", this._startDistance);
			FogMode fogMode = RenderSettings.fogMode;
			if (fogMode == FogMode.Linear)
			{
				float fogStartDistance = RenderSettings.fogStartDistance;
				float fogEndDistance = RenderSettings.fogEndDistance;
				float num = 1f / Mathf.Max(fogEndDistance - fogStartDistance, 1E-06f);
				this._material.SetFloat("_LinearGrad", -num);
				this._material.SetFloat("_LinearOffs", fogEndDistance * num);
				this._material.DisableKeyword("FOG_EXP");
				this._material.DisableKeyword("FOG_EXP2");
			}
			else if (fogMode == FogMode.Exponential)
			{
				float fogDensity = RenderSettings.fogDensity;
				this._material.SetFloat("_Density", 1.442695f * fogDensity);
				this._material.EnableKeyword("FOG_EXP");
				this._material.DisableKeyword("FOG_EXP2");
			}
			else
			{
				float fogDensity2 = RenderSettings.fogDensity;
				this._material.SetFloat("_Density", 1.2011224f * fogDensity2);
				this._material.DisableKeyword("FOG_EXP");
				this._material.EnableKeyword("FOG_EXP2");
			}
			if (this._useRadialDistance)
			{
				this._material.EnableKeyword("RADIAL_DIST");
			}
			else
			{
				this._material.DisableKeyword("RADIAL_DIST");
			}
			if (this._fadeToSkybox)
			{
				this._material.EnableKeyword("USE_SKYBOX");
				Material skybox = RenderSettings.skybox;
				BWLog.Info("Skybox material: " + skybox);
				this._material.SetTexture("_SkyCubemap", skybox.GetTexture("_Tex"));
				this._material.SetColor("_SkyTint", skybox.GetColor("_Tint"));
				this._material.SetFloat("_SkyExposure", skybox.GetFloat("_Exposure"));
				this._material.SetFloat("_SkyRotation", skybox.GetFloat("_Rotation"));
			}
			else
			{
				this._material.DisableKeyword("USE_SKYBOX");
				this._material.SetColor("_FogColor", RenderSettings.fogColor);
			}
			Camera component = base.GetComponent<Camera>();
			Transform transform = component.transform;
			float nearClipPlane = component.nearClipPlane;
			float farClipPlane = component.farClipPlane;
			float d = Mathf.Tan(component.fieldOfView * 0.0174532924f / 2f);
			Vector3 b = transform.right * nearClipPlane * d * component.aspect;
			Vector3 b2 = transform.up * nearClipPlane * d;
			Vector3 vector = transform.forward * nearClipPlane - b + b2;
			Vector3 vector2 = transform.forward * nearClipPlane + b + b2;
			Vector3 vector3 = transform.forward * nearClipPlane + b - b2;
			Vector3 vector4 = transform.forward * nearClipPlane - b - b2;
			float d2 = vector.magnitude * farClipPlane / nearClipPlane;
			RenderTexture.active = destination;
			this._material.SetTexture("_MainTex", source);
			this._material.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Begin(7);
			GL.MultiTexCoord2(0, 0f, 0f);
			GL.MultiTexCoord(1, vector4.normalized * d2);
			GL.Vertex3(0f, 0f, 0.1f);
			GL.MultiTexCoord2(0, 1f, 0f);
			GL.MultiTexCoord(1, vector3.normalized * d2);
			GL.Vertex3(1f, 0f, 0.1f);
			GL.MultiTexCoord2(0, 1f, 1f);
			GL.MultiTexCoord(1, vector2.normalized * d2);
			GL.Vertex3(1f, 1f, 0.1f);
			GL.MultiTexCoord2(0, 0f, 1f);
			GL.MultiTexCoord(1, vector.normalized * d2);
			GL.Vertex3(0f, 1f, 0.1f);
			GL.End();
			GL.PopMatrix();
		}

		// Token: 0x040001C2 RID: 450
		[SerializeField]
		private float _startDistance = 1f;

		// Token: 0x040001C3 RID: 451
		[SerializeField]
		private bool _useRadialDistance;

		// Token: 0x040001C4 RID: 452
		[SerializeField]
		private bool _fadeToSkybox;

		// Token: 0x040001C5 RID: 453
		[SerializeField]
		private Shader _shader;

		// Token: 0x040001C6 RID: 454
		private Material _material;
	}
}
