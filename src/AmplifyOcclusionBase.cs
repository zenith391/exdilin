using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	public enum ApplicationMethod
	{
		PostEffect,
		Deferred,
		Debug
	}

	public enum PerPixelNormalSource
	{
		None,
		Camera,
		GBuffer,
		GBufferOctaEncoded
	}

	public enum SampleCountLevel
	{
		Low,
		Medium,
		High,
		VeryHigh
	}

	private static class ShaderPass
	{
		public const int FullDepth = 0;

		public const int FullNormal_None = 1;

		public const int FullNormal_Camera = 2;

		public const int FullNormal_GBuffer = 3;

		public const int FullNormal_GBufferOctaEncoded = 4;

		public const int DeinterleaveDepth1 = 5;

		public const int DeinterleaveNormal1_None = 6;

		public const int DeinterleaveNormal1_Camera = 7;

		public const int DeinterleaveNormal1_GBuffer = 8;

		public const int DeinterleaveNormal1_GBufferOctaEncoded = 9;

		public const int DeinterleaveDepth4 = 10;

		public const int DeinterleaveNormal4_None = 11;

		public const int DeinterleaveNormal4_Camera = 12;

		public const int DeinterleaveNormal4_GBuffer = 13;

		public const int DeinterleaveNormal4_GBufferOctaEncoded = 14;

		public const int OcclusionCache_Low = 15;

		public const int OcclusionCache_Medium = 16;

		public const int OcclusionCache_High = 17;

		public const int OcclusionCache_VeryHigh = 18;

		public const int Reinterleave = 19;

		public const int OcclusionLow_None = 20;

		public const int OcclusionLow_Camera = 21;

		public const int OcclusionLow_GBuffer = 22;

		public const int OcclusionLow_GBufferOctaEncoded = 23;

		public const int OcclusionMedium_None = 24;

		public const int OcclusionMedium_Camera = 25;

		public const int OcclusionMedium_GBuffer = 26;

		public const int OcclusionMedium_GBufferOctaEncoded = 27;

		public const int OcclusionHigh_None = 28;

		public const int OcclusionHigh_Camera = 29;

		public const int OcclusionHigh_GBuffer = 30;

		public const int OcclusionHigh_GBufferOctaEncoded = 31;

		public const int OcclusionVeryHigh_None = 32;

		public const int OcclusionVeryHigh_Camera = 33;

		public const int OcclusionVeryHigh_GBuffer = 34;

		public const int OcclusionVeryHigh_GBufferNormalEncoded = 35;

		public const int ApplyDebug = 36;

		public const int ApplyDeferred = 37;

		public const int ApplyDeferredLog = 38;

		public const int ApplyPostEffect = 39;

		public const int ApplyPostEffectLog = 40;

		public const int CombineDownsampledOcclusionDepth = 41;

		public const int BlurHorizontal1 = 0;

		public const int BlurVertical1 = 1;

		public const int BlurHorizontal2 = 2;

		public const int BlurVertical2 = 3;

		public const int BlurHorizontal3 = 4;

		public const int BlurVertical3 = 5;

		public const int BlurHorizontal4 = 6;

		public const int BlurVertical4 = 7;

		public const int Copy = 0;
	}

	private struct TargetDesc
	{
		public int fullWidth;

		public int fullHeight;

		public RenderTextureFormat format;

		public int width;

		public int height;

		public int quarterWidth;

		public int quarterHeight;

		public float padRatioWidth;

		public float padRatioHeight;
	}

	[Header("Ambient Occlusion")]
	public ApplicationMethod ApplyMethod;

	public SampleCountLevel SampleCount = SampleCountLevel.Medium;

	public PerPixelNormalSource PerPixelNormals;

	[Range(0f, 1f)]
	public float Intensity = 1f;

	public Color Tint = Color.black;

	[Range(0f, 16f)]
	public float Radius = 1f;

	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	[Range(0f, 0.99f)]
	public float Bias = 0.05f;

	public bool CacheAware;

	public bool Downsample;

	[Header("Distance Fade")]
	public bool FadeEnabled;

	public float FadeStart = 100f;

	public float FadeLength = 50f;

	[Range(0f, 1f)]
	public float FadeToIntensity = 1f;

	[Range(0f, 16f)]
	public float FadeToRadius = 1f;

	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	[Range(1f, 4f)]
	public int BlurRadius = 2;

	[Range(1f, 4f)]
	public int BlurPasses = 1;

	[Range(0f, 20f)]
	public float BlurSharpness = 10f;

	private const int PerPixelNormalSourceCount = 4;

	private int prevScreenWidth;

	private int prevScreenHeight;

	private bool prevHDR;

	private ApplicationMethod prevApplyMethod;

	private SampleCountLevel prevSampleCount;

	private PerPixelNormalSource prevPerPixelNormals;

	private bool prevCacheAware;

	private bool prevDownscale;

	private bool prevFadeEnabled;

	private float prevFadeToIntensity;

	private float prevFadeToRadius;

	private float prevFadeToPowerExponent;

	private float prevFadeStart;

	private float prevFadeLength;

	private bool prevBlurEnabled;

	private int prevBlurRadius;

	private int prevBlurPasses;

	private Camera m_camera;

	private Material m_occlusionMat;

	private Material m_blurMat;

	private Material m_copyMat;

	private const int RandomSize = 4;

	private const int DirectionCount = 8;

	private Color[] m_randomData;

	private string[] m_layerOffsetNames;

	private string[] m_layerRandomNames;

	private string[] m_layerDepthNames;

	private string[] m_layerNormalNames;

	private string[] m_layerOcclusionNames;

	private RenderTextureFormat m_depthRTFormat = RenderTextureFormat.RFloat;

	private RenderTextureFormat m_normalRTFormat = RenderTextureFormat.ARGB2101010;

	private RenderTextureFormat m_occlusionRTFormat = RenderTextureFormat.RGHalf;

	private RenderTexture m_occlusionRT;

	private int[] m_depthLayerRT;

	private int[] m_normalLayerRT;

	private int[] m_occlusionLayerRT;

	private int m_mrtCount;

	private RenderTargetIdentifier[] m_depthTargets;

	private RenderTargetIdentifier[] m_normalTargets;

	private int m_deinterleaveDepthPass;

	private int m_deinterleaveNormalPass;

	private RenderTargetIdentifier[] m_applyDeferredTargets;

	private Mesh m_blitMesh;

	private TargetDesc m_target;

	private Dictionary<CameraEvent, CommandBuffer> m_registeredCommandBuffers = new Dictionary<CameraEvent, CommandBuffer>();

	private bool CheckParamsChanged()
	{
		if (prevScreenWidth == m_camera.pixelWidth && prevScreenHeight == m_camera.pixelHeight && prevHDR == m_camera.hdr && prevApplyMethod == ApplyMethod && prevSampleCount == SampleCount && prevPerPixelNormals == PerPixelNormals && prevCacheAware == CacheAware && prevDownscale == Downsample && prevFadeEnabled == FadeEnabled && prevFadeToIntensity == FadeToIntensity && prevFadeToRadius == FadeToRadius && prevFadeToPowerExponent == FadeToPowerExponent && prevFadeStart == FadeStart && prevFadeLength == FadeLength && prevBlurEnabled == BlurEnabled && prevBlurRadius == BlurRadius)
		{
			return prevBlurPasses != BlurPasses;
		}
		return true;
	}

	private void UpdateParams()
	{
		prevScreenWidth = m_camera.pixelWidth;
		prevScreenHeight = m_camera.pixelHeight;
		prevHDR = m_camera.hdr;
		prevApplyMethod = ApplyMethod;
		prevSampleCount = SampleCount;
		prevPerPixelNormals = PerPixelNormals;
		prevCacheAware = CacheAware;
		prevDownscale = Downsample;
		prevFadeEnabled = FadeEnabled;
		prevFadeToIntensity = FadeToIntensity;
		prevFadeToRadius = FadeToRadius;
		prevFadeToPowerExponent = FadeToPowerExponent;
		prevFadeStart = FadeStart;
		prevFadeLength = FadeLength;
		prevBlurEnabled = BlurEnabled;
		prevBlurRadius = BlurRadius;
		prevBlurPasses = BlurPasses;
	}

	private void Warmup()
	{
		CheckMaterial();
		CheckRandomData();
		m_depthLayerRT = new int[16];
		m_normalLayerRT = new int[16];
		m_occlusionLayerRT = new int[16];
		m_mrtCount = Mathf.Min(SystemInfo.supportedRenderTargetCount, 4);
		m_layerOffsetNames = new string[m_mrtCount];
		m_layerRandomNames = new string[m_mrtCount];
		for (int i = 0; i < m_mrtCount; i++)
		{
			m_layerOffsetNames[i] = "_AO_LayerOffset" + i;
			m_layerRandomNames[i] = "_AO_LayerRandom" + i;
		}
		m_layerDepthNames = new string[16];
		m_layerNormalNames = new string[16];
		m_layerOcclusionNames = new string[16];
		for (int j = 0; j < 16; j++)
		{
			m_layerDepthNames[j] = "_AO_DepthLayer" + j;
			m_layerNormalNames[j] = "_AO_NormalLayer" + j;
			m_layerOcclusionNames[j] = "_AO_OcclusionLayer" + j;
		}
		m_depthTargets = new RenderTargetIdentifier[m_mrtCount];
		m_normalTargets = new RenderTargetIdentifier[m_mrtCount];
		int mrtCount = m_mrtCount;
		if (mrtCount != 4)
		{
			m_deinterleaveDepthPass = 5;
			m_deinterleaveNormalPass = 6;
		}
		else
		{
			m_deinterleaveDepthPass = 10;
			m_deinterleaveNormalPass = 11;
		}
		m_applyDeferredTargets = new RenderTargetIdentifier[2];
		if (m_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(m_blitMesh);
		}
		m_blitMesh = new Mesh();
		m_blitMesh.vertices = new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, 0f, 0f)
		};
		m_blitMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		m_blitMesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
	}

	private void Shutdown()
	{
		CommandBuffer_UnregisterAll();
		SafeReleaseRT(ref m_occlusionRT);
		if (m_occlusionMat != null)
		{
			UnityEngine.Object.DestroyImmediate(m_occlusionMat);
		}
		if (m_blurMat != null)
		{
			UnityEngine.Object.DestroyImmediate(m_blurMat);
		}
		if (m_copyMat != null)
		{
			UnityEngine.Object.DestroyImmediate(m_copyMat);
		}
		if (m_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(m_blitMesh);
		}
	}

	private bool CheckRenderTextureFormats()
	{
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			m_depthRTFormat = RenderTextureFormat.RFloat;
			if (!SystemInfo.SupportsRenderTextureFormat(m_depthRTFormat))
			{
				m_depthRTFormat = RenderTextureFormat.RHalf;
				if (!SystemInfo.SupportsRenderTextureFormat(m_depthRTFormat))
				{
					m_depthRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			m_normalRTFormat = RenderTextureFormat.ARGB2101010;
			if (!SystemInfo.SupportsRenderTextureFormat(m_normalRTFormat))
			{
				m_normalRTFormat = RenderTextureFormat.ARGB32;
			}
			m_occlusionRTFormat = RenderTextureFormat.RGHalf;
			if (!SystemInfo.SupportsRenderTextureFormat(m_occlusionRTFormat))
			{
				m_occlusionRTFormat = RenderTextureFormat.RGFloat;
				if (!SystemInfo.SupportsRenderTextureFormat(m_occlusionRTFormat))
				{
					m_occlusionRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			return true;
		}
		return false;
	}

	private void OnEnable()
	{
		if (!CheckRenderTextureFormats())
		{
			Debug.LogError("[AmplifyOcclusion] Target platform does not meet the minimum requirements for this effect to work properly.");
			base.enabled = false;
		}
		else
		{
			m_camera = GetComponent<Camera>();
			Warmup();
			CommandBuffer_UnregisterAll();
		}
	}

	private void OnDisable()
	{
		Shutdown();
	}

	private void OnDestroy()
	{
		Shutdown();
	}

	private void Update()
	{
		if (m_camera.actualRenderingPath != RenderingPath.DeferredShading)
		{
			if (PerPixelNormals != PerPixelNormalSource.None && PerPixelNormals != PerPixelNormalSource.Camera)
			{
				PerPixelNormals = PerPixelNormalSource.Camera;
				Debug.LogWarning("[AmplifyOcclusion] GBuffer Normals only available in Camera Deferred Shading mode. Switched to Camera source.");
			}
			if (ApplyMethod == ApplicationMethod.Deferred)
			{
				ApplyMethod = ApplicationMethod.PostEffect;
				Debug.LogWarning("[AmplifyOcclusion] Deferred Method requires a Deferred Shading path. Switching to Post Effect Method.");
			}
		}
		if (ApplyMethod == ApplicationMethod.Deferred && PerPixelNormals == PerPixelNormalSource.Camera)
		{
			PerPixelNormals = PerPixelNormalSource.GBuffer;
			Debug.LogWarning("[AmplifyOcclusion] Camera Normals not supported for Deferred Method. Switching to GBuffer Normals.");
		}
		if ((m_camera.depthTextureMode & DepthTextureMode.Depth) == 0)
		{
			m_camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (PerPixelNormals == PerPixelNormalSource.Camera && (m_camera.depthTextureMode & DepthTextureMode.DepthNormals) == 0)
		{
			m_camera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		CheckMaterial();
		CheckRandomData();
	}

	private void CheckMaterial()
	{
		if (m_occlusionMat == null)
		{
			m_occlusionMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Occlusion"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		if (m_blurMat == null)
		{
			m_blurMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Blur"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		if (m_copyMat == null)
		{
			m_copyMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Copy"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
	}

	private void CheckRandomData()
	{
		if (m_randomData == null)
		{
			m_randomData = GenerateRandomizationData();
		}
	}

	public static Color[] GenerateRandomizationData()
	{
		Color[] array = new Color[16];
		int i = 0;
		int num = 0;
		for (; i < 16; i++)
		{
			float num2 = RandomTable.Values[num++];
			float b = RandomTable.Values[num++];
			float f = (float)Math.PI * 2f * num2 / 8f;
			array[i].r = Mathf.Cos(f);
			array[i].g = Mathf.Sin(f);
			array[i].b = b;
			array[i].a = 0f;
		}
		return array;
	}

	public static Texture2D GenerateRandomizationTexture(Color[] randomPixels)
	{
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.ARGB32, mipmap: false, linear: true)
		{
			hideFlags = HideFlags.DontSave
		};
		texture2D.name = "RandomTexture";
		texture2D.filterMode = FilterMode.Point;
		texture2D.wrapMode = TextureWrapMode.Repeat;
		texture2D.SetPixels(randomPixels);
		texture2D.Apply();
		return texture2D;
	}

	private RenderTexture SafeAllocateRT(string name, int width, int height, RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		width = Mathf.Max(width, 1);
		height = Mathf.Max(height, 1);
		RenderTexture renderTexture = new RenderTexture(width, height, 0, format, readWrite)
		{
			hideFlags = HideFlags.DontSave
		};
		renderTexture.name = name;
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.Create();
		return renderTexture;
	}

	private void SafeReleaseRT(ref RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.active = null;
			rt.Release();
			UnityEngine.Object.DestroyImmediate(rt);
			rt = null;
		}
	}

	private int SafeAllocateTemporaryRT(CommandBuffer cb, string propertyName, int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Point)
	{
		int num = Shader.PropertyToID(propertyName);
		cb.GetTemporaryRT(num, width, height, 0, filterMode, format, readWrite);
		return num;
	}

	private void SafeReleaseTemporaryRT(CommandBuffer cb, int id)
	{
		cb.ReleaseTemporaryRT(id);
	}

	private void SetBlitTarget(CommandBuffer cb, RenderTargetIdentifier[] targets, int targetWidth, int targetHeight)
	{
		cb.SetGlobalVector("_AO_Target_TexelSize", new Vector4(1f / (float)targetWidth, 1f / (float)targetHeight, targetWidth, targetHeight));
		cb.SetGlobalVector("_AO_Target_Position", Vector2.zero);
		cb.SetRenderTarget(targets, targets[0]);
	}

	private void SetBlitTarget(CommandBuffer cb, RenderTargetIdentifier target, int targetWidth, int targetHeight)
	{
		cb.SetGlobalVector("_AO_Target_TexelSize", new Vector4(1f / (float)targetWidth, 1f / (float)targetHeight, targetWidth, targetHeight));
		cb.SetRenderTarget(target);
	}

	private void PerformBlit(CommandBuffer cb, Material mat, int pass)
	{
		cb.DrawMesh(m_blitMesh, Matrix4x4.identity, mat, 0, pass);
	}

	private void PerformBlit(CommandBuffer cb, Material mat, int pass, int x, int y)
	{
		cb.SetGlobalVector("_AO_Target_Position", new Vector2(x, y));
		PerformBlit(cb, mat, pass);
	}

	private void PerformBlit(CommandBuffer cb, RenderTargetIdentifier source, int sourceWidth, int sourceHeight, Material mat, int pass)
	{
		cb.SetGlobalTexture("_AO_Source", source);
		cb.SetGlobalVector("_AO_Source_TexelSize", new Vector4(1f / (float)sourceWidth, 1f / (float)sourceHeight, sourceWidth, sourceHeight));
		PerformBlit(cb, mat, pass);
	}

	private void PerformBlit(CommandBuffer cb, RenderTargetIdentifier source, int sourceWidth, int sourceHeight, Material mat, int pass, int x, int y)
	{
		cb.SetGlobalVector("_AO_Target_Position", new Vector2(x, y));
		PerformBlit(cb, source, sourceWidth, sourceHeight, mat, pass);
	}

	private CommandBuffer CommandBuffer_Allocate(string name)
	{
		return new CommandBuffer
		{
			name = name
		};
	}

	private void CommandBuffer_Register(CameraEvent cameraEvent, CommandBuffer commandBuffer)
	{
		m_camera.AddCommandBuffer(cameraEvent, commandBuffer);
		m_registeredCommandBuffers.Add(cameraEvent, commandBuffer);
	}

	private void CommandBuffer_Unregister(CameraEvent cameraEvent, CommandBuffer commandBuffer)
	{
		if (!(m_camera != null))
		{
			return;
		}
		CommandBuffer[] commandBuffers = m_camera.GetCommandBuffers(cameraEvent);
		CommandBuffer[] array = commandBuffers;
		foreach (CommandBuffer commandBuffer2 in array)
		{
			if (commandBuffer2.name == commandBuffer.name)
			{
				m_camera.RemoveCommandBuffer(cameraEvent, commandBuffer2);
			}
		}
	}

	private CommandBuffer CommandBuffer_AllocateRegister(CameraEvent cameraEvent)
	{
		string text = string.Empty;
		switch (cameraEvent)
		{
		case CameraEvent.BeforeReflections:
			text = "AO-BeforeRefl";
			break;
		case CameraEvent.AfterLighting:
			text = "AO-AfterLighting";
			break;
		case CameraEvent.BeforeImageEffectsOpaque:
			text = "AO-BeforePostOpaque";
			break;
		default:
			Debug.LogError("[AmplifyOcclusion] Unsupported CameraEvent. Please contact support.");
			break;
		}
		CommandBuffer commandBuffer = CommandBuffer_Allocate(text);
		CommandBuffer_Register(cameraEvent, commandBuffer);
		return commandBuffer;
	}

	private void CommandBuffer_UnregisterAll()
	{
		foreach (KeyValuePair<CameraEvent, CommandBuffer> registeredCommandBuffer in m_registeredCommandBuffers)
		{
			CommandBuffer_Unregister(registeredCommandBuffer.Key, registeredCommandBuffer.Value);
		}
		m_registeredCommandBuffers.Clear();
	}

	private void UpdateGlobalShaderConstants(TargetDesc target)
	{
		float num = m_camera.fieldOfView * ((float)Math.PI / 180f);
		Vector2 vector = new Vector2(1f / Mathf.Tan(num * 0.5f) * ((float)target.height / (float)target.width), 1f / Mathf.Tan(num * 0.5f));
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		float num2 = ((!m_camera.orthographic) ? ((float)target.height / (Mathf.Tan(num * 0.5f) * 2f)) : ((float)target.height / m_camera.orthographicSize));
		float num3 = Mathf.Clamp(Bias, 0f, 1f);
		FadeStart = Mathf.Max(0f, FadeStart);
		FadeLength = Mathf.Max(0.01f, FadeLength);
		float y = ((!FadeEnabled) ? 0f : (1f / FadeLength));
		Shader.SetGlobalMatrix("_AO_CameraProj", GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(0f, 1f, 0f, 1f, -1f, 100f), renderIntoTexture: false));
		Shader.SetGlobalMatrix("_AO_CameraView", m_camera.worldToCameraMatrix);
		Shader.SetGlobalVector("_AO_UVToView", new Vector4(2f * vector2.x, -2f * vector2.y, -1f * vector2.x, 1f * vector2.y));
		Shader.SetGlobalFloat("_AO_HalfProjScale", 0.5f * num2);
		Shader.SetGlobalFloat("_AO_Radius", Radius);
		Shader.SetGlobalFloat("_AO_PowExponent", PowerExponent);
		Shader.SetGlobalFloat("_AO_Bias", num3);
		Shader.SetGlobalFloat("_AO_Multiplier", 1f / (1f - num3));
		Shader.SetGlobalFloat("_AO_BlurSharpness", BlurSharpness);
		Shader.SetGlobalColor("_AO_Levels", new Color(Tint.r, Tint.g, Tint.b, Intensity));
		Shader.SetGlobalVector("_AO_FadeParams", new Vector2(FadeStart, y));
		Shader.SetGlobalVector("_AO_FadeValues", new Vector3(FadeToIntensity, FadeToRadius, FadeToPowerExponent));
	}

	private void CommandBuffer_FillComputeOcclusion(CommandBuffer cb, TargetDesc target)
	{
		CheckMaterial();
		CheckRandomData();
		cb.SetGlobalVector("_AO_Buffer_PadScale", new Vector4(target.padRatioWidth, target.padRatioHeight, 1f / target.padRatioWidth, 1f / target.padRatioHeight));
		cb.SetGlobalVector("_AO_Buffer_TexelSize", new Vector4(1f / (float)target.width, 1f / (float)target.height, target.width, target.height));
		cb.SetGlobalVector("_AO_QuarterBuffer_TexelSize", new Vector4(1f / (float)target.quarterWidth, 1f / (float)target.quarterHeight, target.quarterWidth, target.quarterHeight));
		cb.SetGlobalFloat("_AO_MaxRadiusPixels", Mathf.Min(target.width, target.height));
		if (m_occlusionRT == null || m_occlusionRT.width != target.width || m_occlusionRT.height != target.height || !m_occlusionRT.IsCreated())
		{
			SafeReleaseRT(ref m_occlusionRT);
			m_occlusionRT = SafeAllocateRT("_AO_OcclusionTexture", target.width, target.height, m_occlusionRTFormat, RenderTextureReadWrite.Linear);
		}
		int num = -1;
		if (Downsample)
		{
			num = SafeAllocateTemporaryRT(cb, "_AO_SmallOcclusionTexture", target.width / 2, target.height / 2, m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Bilinear);
		}
		if (CacheAware && !Downsample)
		{
			int num2 = SafeAllocateTemporaryRT(cb, "_AO_OcclusionAtlas", target.width, target.height, m_occlusionRTFormat, RenderTextureReadWrite.Linear);
			for (int i = 0; i < 16; i++)
			{
				m_depthLayerRT[i] = SafeAllocateTemporaryRT(cb, m_layerDepthNames[i], target.quarterWidth, target.quarterHeight, m_depthRTFormat, RenderTextureReadWrite.Linear);
				m_normalLayerRT[i] = SafeAllocateTemporaryRT(cb, m_layerNormalNames[i], target.quarterWidth, target.quarterHeight, m_normalRTFormat, RenderTextureReadWrite.Linear);
				m_occlusionLayerRT[i] = SafeAllocateTemporaryRT(cb, m_layerOcclusionNames[i], target.quarterWidth, target.quarterHeight, m_occlusionRTFormat, RenderTextureReadWrite.Linear);
			}
			for (int j = 0; j < 16; j += m_mrtCount)
			{
				for (int k = 0; k < m_mrtCount; k++)
				{
					int num3 = k + j;
					int num4 = num3 & 3;
					int num5 = num3 >> 2;
					cb.SetGlobalVector(m_layerOffsetNames[k], new Vector2((float)num4 + 0.5f, (float)num5 + 0.5f));
					m_depthTargets[k] = m_depthLayerRT[num3];
					m_normalTargets[k] = m_normalLayerRT[num3];
				}
				SetBlitTarget(cb, m_depthTargets, target.quarterWidth, target.quarterHeight);
				PerformBlit(cb, m_occlusionMat, m_deinterleaveDepthPass);
				SetBlitTarget(cb, m_normalTargets, target.quarterWidth, target.quarterHeight);
				PerformBlit(cb, m_occlusionMat, (int)(m_deinterleaveNormalPass + PerPixelNormals));
			}
			for (int l = 0; l < 16; l++)
			{
				cb.SetGlobalVector("_AO_LayerOffset", new Vector2((float)(l & 3) + 0.5f, (float)(l >> 2) + 0.5f));
				cb.SetGlobalVector("_AO_LayerRandom", m_randomData[l]);
				cb.SetGlobalTexture("_AO_NormalTexture", m_normalLayerRT[l]);
				cb.SetGlobalTexture("_AO_DepthTexture", m_depthLayerRT[l]);
				SetBlitTarget(cb, m_occlusionLayerRT[l], target.quarterWidth, target.quarterHeight);
				PerformBlit(cb, m_occlusionMat, (int)(15 + SampleCount));
			}
			SetBlitTarget(cb, num2, target.width, target.height);
			for (int m = 0; m < 16; m++)
			{
				int x = (m & 3) * target.quarterWidth;
				int y = (m >> 2) * target.quarterHeight;
				PerformBlit(cb, m_occlusionLayerRT[m], target.quarterWidth, target.quarterHeight, m_copyMat, 0, x, y);
			}
			cb.SetGlobalTexture("_AO_OcclusionAtlas", num2);
			SetBlitTarget(cb, m_occlusionRT, target.width, target.height);
			PerformBlit(cb, m_occlusionMat, 19);
			for (int n = 0; n < 16; n++)
			{
				SafeReleaseTemporaryRT(cb, m_occlusionLayerRT[n]);
				SafeReleaseTemporaryRT(cb, m_normalLayerRT[n]);
				SafeReleaseTemporaryRT(cb, m_depthLayerRT[n]);
			}
			SafeReleaseTemporaryRT(cb, num2);
		}
		else
		{
			int pass = (int)(20 + (int)SampleCount * 4 + PerPixelNormals);
			if (Downsample)
			{
				cb.Blit(null, new RenderTargetIdentifier(num), m_occlusionMat, pass);
				SetBlitTarget(cb, m_occlusionRT, target.width, target.height);
				PerformBlit(cb, num, target.width / 2, target.height / 2, m_occlusionMat, 41);
			}
			else
			{
				cb.Blit(null, m_occlusionRT, m_occlusionMat, pass);
			}
		}
		if (BlurEnabled)
		{
			int num6 = SafeAllocateTemporaryRT(cb, "_AO_TEMP", target.width, target.height, m_occlusionRTFormat, RenderTextureReadWrite.Linear);
			for (int num7 = 0; num7 < BlurPasses; num7++)
			{
				SetBlitTarget(cb, num6, target.width, target.height);
				PerformBlit(cb, m_occlusionRT, target.width, target.height, m_blurMat, (BlurRadius - 1) * 2);
				SetBlitTarget(cb, m_occlusionRT, target.width, target.height);
				PerformBlit(cb, num6, target.width, target.height, m_blurMat, 1 + (BlurRadius - 1) * 2);
			}
			SafeReleaseTemporaryRT(cb, num6);
		}
		if (Downsample && num >= 0)
		{
			SafeReleaseTemporaryRT(cb, num);
		}
		cb.SetRenderTarget(default(RenderTargetIdentifier));
	}

	private void CommandBuffer_FillApplyDeferred(CommandBuffer cb, TargetDesc target, bool logTarget)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", m_occlusionRT);
		m_applyDeferredTargets[0] = BuiltinRenderTextureType.GBuffer0;
		m_applyDeferredTargets[1] = ((!logTarget) ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3);
		if (!logTarget)
		{
			SetBlitTarget(cb, m_applyDeferredTargets, target.fullWidth, target.fullHeight);
			PerformBlit(cb, m_occlusionMat, 37);
		}
		else
		{
			int num = SafeAllocateTemporaryRT(cb, "_AO_GBufferAlbedo", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32);
			int num2 = SafeAllocateTemporaryRT(cb, "_AO_GBufferEmission", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32);
			cb.Blit(m_applyDeferredTargets[0], num);
			cb.Blit(m_applyDeferredTargets[1], num2);
			cb.SetGlobalTexture("_AO_GBufferAlbedo", num);
			cb.SetGlobalTexture("_AO_GBufferEmission", num2);
			SetBlitTarget(cb, m_applyDeferredTargets, target.fullWidth, target.fullHeight);
			PerformBlit(cb, m_occlusionMat, 38);
			SafeReleaseTemporaryRT(cb, num);
			SafeReleaseTemporaryRT(cb, num2);
		}
		cb.SetRenderTarget(default(RenderTargetIdentifier));
	}

	private void CommandBuffer_FillApplyPostEffect(CommandBuffer cb, TargetDesc target, bool logTarget)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", m_occlusionRT);
		if (!logTarget)
		{
			SetBlitTarget(cb, BuiltinRenderTextureType.CameraTarget, target.fullWidth, target.fullHeight);
			PerformBlit(cb, m_occlusionMat, 39);
		}
		else
		{
			int num = SafeAllocateTemporaryRT(cb, "_AO_GBufferEmission", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32);
			cb.Blit(BuiltinRenderTextureType.GBuffer3, num);
			cb.SetGlobalTexture("_AO_GBufferEmission", num);
			SetBlitTarget(cb, BuiltinRenderTextureType.GBuffer3, target.fullWidth, target.fullHeight);
			PerformBlit(cb, m_occlusionMat, 40);
			SafeReleaseTemporaryRT(cb, num);
		}
		cb.SetRenderTarget(default(RenderTargetIdentifier));
	}

	private void CommandBuffer_FillApplyDebug(CommandBuffer cb, TargetDesc target)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", m_occlusionRT);
		SetBlitTarget(cb, BuiltinRenderTextureType.CameraTarget, target.fullWidth, target.fullHeight);
		PerformBlit(cb, m_occlusionMat, 36);
		cb.SetRenderTarget(default(RenderTargetIdentifier));
	}

	private void CommandBuffer_Rebuild(TargetDesc target)
	{
		bool flag = PerPixelNormals == PerPixelNormalSource.GBuffer || PerPixelNormals == PerPixelNormalSource.GBufferOctaEncoded;
		CameraEvent cameraEvent = ((!flag) ? CameraEvent.BeforeImageEffectsOpaque : CameraEvent.AfterLighting);
		if (ApplyMethod == ApplicationMethod.Debug)
		{
			CommandBuffer cb = CommandBuffer_AllocateRegister(cameraEvent);
			CommandBuffer_FillComputeOcclusion(cb, target);
			CommandBuffer_FillApplyDebug(cb, target);
			return;
		}
		bool logTarget = !m_camera.hdr && flag;
		cameraEvent = ((ApplyMethod != ApplicationMethod.Deferred) ? cameraEvent : CameraEvent.BeforeReflections);
		CommandBuffer cb2 = CommandBuffer_AllocateRegister(cameraEvent);
		CommandBuffer_FillComputeOcclusion(cb2, target);
		if (ApplyMethod == ApplicationMethod.PostEffect)
		{
			CommandBuffer_FillApplyPostEffect(cb2, target, logTarget);
		}
		else if (ApplyMethod == ApplicationMethod.Deferred)
		{
			CommandBuffer_FillApplyDeferred(cb2, target, logTarget);
		}
	}

	private void OnPreRender()
	{
		bool hdr = m_camera.hdr;
		m_target.fullWidth = m_camera.pixelWidth;
		m_target.fullHeight = m_camera.pixelHeight;
		m_target.format = (hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
		m_target.width = ((!CacheAware) ? m_target.fullWidth : ((m_target.fullWidth + 3) & -4));
		m_target.height = ((!CacheAware) ? m_target.fullHeight : ((m_target.fullHeight + 3) & -4));
		m_target.quarterWidth = m_target.width / 4;
		m_target.quarterHeight = m_target.height / 4;
		m_target.padRatioWidth = (float)m_target.width / (float)m_target.fullWidth;
		m_target.padRatioHeight = (float)m_target.height / (float)m_target.fullHeight;
		UpdateGlobalShaderConstants(m_target);
		if (CheckParamsChanged() || m_registeredCommandBuffers.Count == 0)
		{
			CommandBuffer_UnregisterAll();
			CommandBuffer_Rebuild(m_target);
			UpdateParams();
		}
	}

	private void OnPostRender()
	{
		m_occlusionRT.MarkRestoreExpected();
	}
}
