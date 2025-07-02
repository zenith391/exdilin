using System;
using UnityEngine;

namespace Kino;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Kino Image Effects/Fog")]
public class Fog : MonoBehaviour
{
	[SerializeField]
	private float _startDistance = 1f;

	[SerializeField]
	private bool _useRadialDistance;

	[SerializeField]
	private bool _fadeToSkybox;

	[SerializeField]
	private Shader _shader;

	private Material _material;

	public float startDistance
	{
		get
		{
			return _startDistance;
		}
		set
		{
			_startDistance = value;
		}
	}

	public bool useRadialDistance
	{
		get
		{
			return _useRadialDistance;
		}
		set
		{
			_useRadialDistance = value;
		}
	}

	public bool fadeToSkybox
	{
		get
		{
			return _fadeToSkybox;
		}
		set
		{
			_fadeToSkybox = value;
		}
	}

	private void OnEnable()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_material == null)
		{
			_material = new Material(_shader);
			_material.hideFlags = HideFlags.DontSave;
		}
		_startDistance = Mathf.Max(_startDistance, 0f);
		_material.SetFloat("_DistanceOffset", _startDistance);
		switch (RenderSettings.fogMode)
		{
		case FogMode.Linear:
		{
			float fogStartDistance = RenderSettings.fogStartDistance;
			float fogEndDistance = RenderSettings.fogEndDistance;
			float num = 1f / Mathf.Max(fogEndDistance - fogStartDistance, 1E-06f);
			_material.SetFloat("_LinearGrad", 0f - num);
			_material.SetFloat("_LinearOffs", fogEndDistance * num);
			_material.DisableKeyword("FOG_EXP");
			_material.DisableKeyword("FOG_EXP2");
			break;
		}
		case FogMode.Exponential:
		{
			float fogDensity2 = RenderSettings.fogDensity;
			_material.SetFloat("_Density", 1.442695f * fogDensity2);
			_material.EnableKeyword("FOG_EXP");
			_material.DisableKeyword("FOG_EXP2");
			break;
		}
		default:
		{
			float fogDensity = RenderSettings.fogDensity;
			_material.SetFloat("_Density", 1.2011224f * fogDensity);
			_material.DisableKeyword("FOG_EXP");
			_material.EnableKeyword("FOG_EXP2");
			break;
		}
		}
		if (_useRadialDistance)
		{
			_material.EnableKeyword("RADIAL_DIST");
		}
		else
		{
			_material.DisableKeyword("RADIAL_DIST");
		}
		if (_fadeToSkybox)
		{
			_material.EnableKeyword("USE_SKYBOX");
			Material skybox = RenderSettings.skybox;
			BWLog.Info("Skybox material: " + skybox);
			_material.SetTexture("_SkyCubemap", skybox.GetTexture("_Tex"));
			_material.SetColor("_SkyTint", skybox.GetColor("_Tint"));
			_material.SetFloat("_SkyExposure", skybox.GetFloat("_Exposure"));
			_material.SetFloat("_SkyRotation", skybox.GetFloat("_Rotation"));
		}
		else
		{
			_material.DisableKeyword("USE_SKYBOX");
			_material.SetColor("_FogColor", RenderSettings.fogColor);
		}
		Camera component = GetComponent<Camera>();
		Transform transform = component.transform;
		float nearClipPlane = component.nearClipPlane;
		float farClipPlane = component.farClipPlane;
		float num2 = Mathf.Tan(component.fieldOfView * ((float)Math.PI / 180f) / 2f);
		Vector3 vector = transform.right * nearClipPlane * num2 * component.aspect;
		Vector3 vector2 = transform.up * nearClipPlane * num2;
		Vector3 vector3 = transform.forward * nearClipPlane - vector + vector2;
		Vector3 vector4 = transform.forward * nearClipPlane + vector + vector2;
		Vector3 vector5 = transform.forward * nearClipPlane + vector - vector2;
		Vector3 vector6 = transform.forward * nearClipPlane - vector - vector2;
		float num3 = vector3.magnitude * farClipPlane / nearClipPlane;
		RenderTexture.active = destination;
		_material.SetTexture("_MainTex", source);
		_material.SetPass(0);
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.MultiTexCoord(1, vector6.normalized * num3);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.MultiTexCoord(1, vector5.normalized * num3);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.MultiTexCoord(1, vector4.normalized * num3);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.MultiTexCoord(1, vector3.normalized * num3);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}
}
