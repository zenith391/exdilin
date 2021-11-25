using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000002 RID: 2
[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	// Token: 0x06000002 RID: 2 RVA: 0x0000212C File Offset: 0x0000052C
	private bool CheckParamsChanged()
	{
		return this.prevScreenWidth != this.m_camera.pixelWidth || this.prevScreenHeight != this.m_camera.pixelHeight || this.prevHDR != this.m_camera.hdr || this.prevApplyMethod != this.ApplyMethod || this.prevSampleCount != this.SampleCount || this.prevPerPixelNormals != this.PerPixelNormals || this.prevCacheAware != this.CacheAware || this.prevDownscale != this.Downsample || this.prevFadeEnabled != this.FadeEnabled || this.prevFadeToIntensity != this.FadeToIntensity || this.prevFadeToRadius != this.FadeToRadius || this.prevFadeToPowerExponent != this.FadeToPowerExponent || this.prevFadeStart != this.FadeStart || this.prevFadeLength != this.FadeLength || this.prevBlurEnabled != this.BlurEnabled || this.prevBlurRadius != this.BlurRadius || this.prevBlurPasses != this.BlurPasses;
	}

	// Token: 0x06000003 RID: 3 RVA: 0x00002270 File Offset: 0x00000670
	private void UpdateParams()
	{
		this.prevScreenWidth = this.m_camera.pixelWidth;
		this.prevScreenHeight = this.m_camera.pixelHeight;
		this.prevHDR = this.m_camera.hdr;
		this.prevApplyMethod = this.ApplyMethod;
		this.prevSampleCount = this.SampleCount;
		this.prevPerPixelNormals = this.PerPixelNormals;
		this.prevCacheAware = this.CacheAware;
		this.prevDownscale = this.Downsample;
		this.prevFadeEnabled = this.FadeEnabled;
		this.prevFadeToIntensity = this.FadeToIntensity;
		this.prevFadeToRadius = this.FadeToRadius;
		this.prevFadeToPowerExponent = this.FadeToPowerExponent;
		this.prevFadeStart = this.FadeStart;
		this.prevFadeLength = this.FadeLength;
		this.prevBlurEnabled = this.BlurEnabled;
		this.prevBlurRadius = this.BlurRadius;
		this.prevBlurPasses = this.BlurPasses;
	}

	// Token: 0x06000004 RID: 4 RVA: 0x00002358 File Offset: 0x00000758
	private void Warmup()
	{
		this.CheckMaterial();
		this.CheckRandomData();
		this.m_depthLayerRT = new int[16];
		this.m_normalLayerRT = new int[16];
		this.m_occlusionLayerRT = new int[16];
		this.m_mrtCount = Mathf.Min(SystemInfo.supportedRenderTargetCount, 4);
		this.m_layerOffsetNames = new string[this.m_mrtCount];
		this.m_layerRandomNames = new string[this.m_mrtCount];
		for (int i = 0; i < this.m_mrtCount; i++)
		{
			this.m_layerOffsetNames[i] = "_AO_LayerOffset" + i;
			this.m_layerRandomNames[i] = "_AO_LayerRandom" + i;
		}
		this.m_layerDepthNames = new string[16];
		this.m_layerNormalNames = new string[16];
		this.m_layerOcclusionNames = new string[16];
		for (int j = 0; j < 16; j++)
		{
			this.m_layerDepthNames[j] = "_AO_DepthLayer" + j;
			this.m_layerNormalNames[j] = "_AO_NormalLayer" + j;
			this.m_layerOcclusionNames[j] = "_AO_OcclusionLayer" + j;
		}
		this.m_depthTargets = new RenderTargetIdentifier[this.m_mrtCount];
		this.m_normalTargets = new RenderTargetIdentifier[this.m_mrtCount];
		int mrtCount = this.m_mrtCount;
		if (mrtCount != 4)
		{
			this.m_deinterleaveDepthPass = 5;
			this.m_deinterleaveNormalPass = 6;
		}
		else
		{
			this.m_deinterleaveDepthPass = 10;
			this.m_deinterleaveNormalPass = 11;
		}
		this.m_applyDeferredTargets = new RenderTargetIdentifier[2];
		if (this.m_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_blitMesh);
		}
		this.m_blitMesh = new Mesh();
		this.m_blitMesh.vertices = new Vector3[]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, 0f, 0f)
		};
		this.m_blitMesh.uv = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		this.m_blitMesh.triangles = new int[]
		{
			0,
			1,
			2,
			0,
			2,
			3
		};
	}

	// Token: 0x06000005 RID: 5 RVA: 0x00002650 File Offset: 0x00000A50
	private void Shutdown()
	{
		this.CommandBuffer_UnregisterAll();
		this.SafeReleaseRT(ref this.m_occlusionRT);
		if (this.m_occlusionMat != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_occlusionMat);
		}
		if (this.m_blurMat != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_blurMat);
		}
		if (this.m_copyMat != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_copyMat);
		}
		if (this.m_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_blitMesh);
		}
	}

	// Token: 0x06000006 RID: 6 RVA: 0x000026E0 File Offset: 0x00000AE0
	private bool CheckRenderTextureFormats()
	{
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			this.m_depthRTFormat = RenderTextureFormat.RFloat;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_depthRTFormat))
			{
				this.m_depthRTFormat = RenderTextureFormat.RHalf;
				if (!SystemInfo.SupportsRenderTextureFormat(this.m_depthRTFormat))
				{
					this.m_depthRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			this.m_normalRTFormat = RenderTextureFormat.ARGB2101010;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_normalRTFormat))
			{
				this.m_normalRTFormat = RenderTextureFormat.ARGB32;
			}
			this.m_occlusionRTFormat = RenderTextureFormat.RGHalf;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_occlusionRTFormat))
			{
				this.m_occlusionRTFormat = RenderTextureFormat.RGFloat;
				if (!SystemInfo.SupportsRenderTextureFormat(this.m_occlusionRTFormat))
				{
					this.m_occlusionRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000007 RID: 7 RVA: 0x00002792 File Offset: 0x00000B92
	private void OnEnable()
	{
		if (!this.CheckRenderTextureFormats())
		{
			Debug.LogError("[AmplifyOcclusion] Target platform does not meet the minimum requirements for this effect to work properly.");
			base.enabled = false;
			return;
		}
		this.m_camera = base.GetComponent<Camera>();
		this.Warmup();
		this.CommandBuffer_UnregisterAll();
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000027C9 File Offset: 0x00000BC9
	private void OnDisable()
	{
		this.Shutdown();
	}

	// Token: 0x06000009 RID: 9 RVA: 0x000027D1 File Offset: 0x00000BD1
	private void OnDestroy()
	{
		this.Shutdown();
	}

	// Token: 0x0600000A RID: 10 RVA: 0x000027DC File Offset: 0x00000BDC
	private void Update()
	{
		if (this.m_camera.actualRenderingPath != RenderingPath.DeferredShading)
		{
			if (this.PerPixelNormals != AmplifyOcclusionBase.PerPixelNormalSource.None && this.PerPixelNormals != AmplifyOcclusionBase.PerPixelNormalSource.Camera)
			{
				this.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.Camera;
				Debug.LogWarning("[AmplifyOcclusion] GBuffer Normals only available in Camera Deferred Shading mode. Switched to Camera source.");
			}
			if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred)
			{
				this.ApplyMethod = AmplifyOcclusionBase.ApplicationMethod.PostEffect;
				Debug.LogWarning("[AmplifyOcclusion] Deferred Method requires a Deferred Shading path. Switching to Post Effect Method.");
			}
		}
		if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred && this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.Camera)
		{
			this.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.GBuffer;
			Debug.LogWarning("[AmplifyOcclusion] Camera Normals not supported for Deferred Method. Switching to GBuffer Normals.");
		}
		if ((this.m_camera.depthTextureMode & DepthTextureMode.Depth) == DepthTextureMode.None)
		{
			this.m_camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.Camera && (this.m_camera.depthTextureMode & DepthTextureMode.DepthNormals) == DepthTextureMode.None)
		{
			this.m_camera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		this.CheckMaterial();
		this.CheckRandomData();
	}

	// Token: 0x0600000B RID: 11 RVA: 0x000028CC File Offset: 0x00000CCC
	private void CheckMaterial()
	{
		if (this.m_occlusionMat == null)
		{
			this.m_occlusionMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Occlusion"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		if (this.m_blurMat == null)
		{
			this.m_blurMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Blur"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		if (this.m_copyMat == null)
		{
			this.m_copyMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Copy"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
	}

	// Token: 0x0600000C RID: 12 RVA: 0x00002969 File Offset: 0x00000D69
	private void CheckRandomData()
	{
		if (this.m_randomData == null)
		{
			this.m_randomData = AmplifyOcclusionBase.GenerateRandomizationData();
		}
	}

	// Token: 0x0600000D RID: 13 RVA: 0x00002984 File Offset: 0x00000D84
	public static Color[] GenerateRandomizationData()
	{
		Color[] array = new Color[16];
		int i = 0;
		int num = 0;
		while (i < 16)
		{
			float num2 = RandomTable.Values[num++];
			float b = RandomTable.Values[num++];
			float f = 6.28318548f * num2 / 8f;
			array[i].r = Mathf.Cos(f);
			array[i].g = Mathf.Sin(f);
			array[i].b = b;
			array[i].a = 0f;
			i++;
		}
		return array;
	}

	// Token: 0x0600000E RID: 14 RVA: 0x00002A1C File Offset: 0x00000E1C
	public static Texture2D GenerateRandomizationTexture(Color[] randomPixels)
	{
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.ARGB32, false, true)
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

	// Token: 0x0600000F RID: 15 RVA: 0x00002A68 File Offset: 0x00000E68
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

	// Token: 0x06000010 RID: 16 RVA: 0x00002ABB File Offset: 0x00000EBB
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

	// Token: 0x06000011 RID: 17 RVA: 0x00002AE4 File Offset: 0x00000EE4
	private int SafeAllocateTemporaryRT(CommandBuffer cb, string propertyName, int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Point)
	{
		int num = Shader.PropertyToID(propertyName);
		cb.GetTemporaryRT(num, width, height, 0, filterMode, format, readWrite);
		return num;
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00002B0A File Offset: 0x00000F0A
	private void SafeReleaseTemporaryRT(CommandBuffer cb, int id)
	{
		cb.ReleaseTemporaryRT(id);
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00002B14 File Offset: 0x00000F14
	private void SetBlitTarget(CommandBuffer cb, RenderTargetIdentifier[] targets, int targetWidth, int targetHeight)
	{
		cb.SetGlobalVector("_AO_Target_TexelSize", new Vector4(1f / (float)targetWidth, 1f / (float)targetHeight, (float)targetWidth, (float)targetHeight));
		cb.SetGlobalVector("_AO_Target_Position", Vector2.zero);
		cb.SetRenderTarget(targets, targets[0]);
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00002B6F File Offset: 0x00000F6F
	private void SetBlitTarget(CommandBuffer cb, RenderTargetIdentifier target, int targetWidth, int targetHeight)
	{
		cb.SetGlobalVector("_AO_Target_TexelSize", new Vector4(1f / (float)targetWidth, 1f / (float)targetHeight, (float)targetWidth, (float)targetHeight));
		cb.SetRenderTarget(target);
	}

	// Token: 0x06000015 RID: 21 RVA: 0x00002B9E File Offset: 0x00000F9E
	private void PerformBlit(CommandBuffer cb, Material mat, int pass)
	{
		cb.DrawMesh(this.m_blitMesh, Matrix4x4.identity, mat, 0, pass);
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002BB4 File Offset: 0x00000FB4
	private void PerformBlit(CommandBuffer cb, Material mat, int pass, int x, int y)
	{
		cb.SetGlobalVector("_AO_Target_Position", new Vector2((float)x, (float)y));
		this.PerformBlit(cb, mat, pass);
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002BDA File Offset: 0x00000FDA
	private void PerformBlit(CommandBuffer cb, RenderTargetIdentifier source, int sourceWidth, int sourceHeight, Material mat, int pass)
	{
		cb.SetGlobalTexture("_AO_Source", source);
		cb.SetGlobalVector("_AO_Source_TexelSize", new Vector4(1f / (float)sourceWidth, 1f / (float)sourceHeight, (float)sourceWidth, (float)sourceHeight));
		this.PerformBlit(cb, mat, pass);
	}

	// Token: 0x06000018 RID: 24 RVA: 0x00002C19 File Offset: 0x00001019
	private void PerformBlit(CommandBuffer cb, RenderTargetIdentifier source, int sourceWidth, int sourceHeight, Material mat, int pass, int x, int y)
	{
		cb.SetGlobalVector("_AO_Target_Position", new Vector2((float)x, (float)y));
		this.PerformBlit(cb, source, sourceWidth, sourceHeight, mat, pass);
	}

	// Token: 0x06000019 RID: 25 RVA: 0x00002C48 File Offset: 0x00001048
	private CommandBuffer CommandBuffer_Allocate(string name)
	{
		return new CommandBuffer
		{
			name = name
		};
	}

	// Token: 0x0600001A RID: 26 RVA: 0x00002C63 File Offset: 0x00001063
	private void CommandBuffer_Register(CameraEvent cameraEvent, CommandBuffer commandBuffer)
	{
		this.m_camera.AddCommandBuffer(cameraEvent, commandBuffer);
		this.m_registeredCommandBuffers.Add(cameraEvent, commandBuffer);
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00002C80 File Offset: 0x00001080
	private void CommandBuffer_Unregister(CameraEvent cameraEvent, CommandBuffer commandBuffer)
	{
		if (this.m_camera != null)
		{
			CommandBuffer[] commandBuffers = this.m_camera.GetCommandBuffers(cameraEvent);
			foreach (CommandBuffer commandBuffer2 in commandBuffers)
			{
				if (commandBuffer2.name == commandBuffer.name)
				{
					this.m_camera.RemoveCommandBuffer(cameraEvent, commandBuffer2);
				}
			}
		}
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002CE8 File Offset: 0x000010E8
	private CommandBuffer CommandBuffer_AllocateRegister(CameraEvent cameraEvent)
	{
		string name = string.Empty;
		if (cameraEvent == CameraEvent.BeforeReflections)
		{
			name = "AO-BeforeRefl";
		}
		else if (cameraEvent == CameraEvent.AfterLighting)
		{
			name = "AO-AfterLighting";
		}
		else if (cameraEvent == CameraEvent.BeforeImageEffectsOpaque)
		{
			name = "AO-BeforePostOpaque";
		}
		else
		{
			Debug.LogError("[AmplifyOcclusion] Unsupported CameraEvent. Please contact support.");
		}
		CommandBuffer commandBuffer = this.CommandBuffer_Allocate(name);
		this.CommandBuffer_Register(cameraEvent, commandBuffer);
		return commandBuffer;
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00002D50 File Offset: 0x00001150
	private void CommandBuffer_UnregisterAll()
	{
		foreach (KeyValuePair<CameraEvent, CommandBuffer> keyValuePair in this.m_registeredCommandBuffers)
		{
			this.CommandBuffer_Unregister(keyValuePair.Key, keyValuePair.Value);
		}
		this.m_registeredCommandBuffers.Clear();
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00002DC4 File Offset: 0x000011C4
	private void UpdateGlobalShaderConstants(AmplifyOcclusionBase.TargetDesc target)
	{
		float num = this.m_camera.fieldOfView * 0.0174532924f;
		Vector2 vector = new Vector2(1f / Mathf.Tan(num * 0.5f) * ((float)target.height / (float)target.width), 1f / Mathf.Tan(num * 0.5f));
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		float num2;
		if (this.m_camera.orthographic)
		{
			num2 = (float)target.height / this.m_camera.orthographicSize;
		}
		else
		{
			num2 = (float)target.height / (Mathf.Tan(num * 0.5f) * 2f);
		}
		float num3 = Mathf.Clamp(this.Bias, 0f, 1f);
		this.FadeStart = Mathf.Max(0f, this.FadeStart);
		this.FadeLength = Mathf.Max(0.01f, this.FadeLength);
		float y = (!this.FadeEnabled) ? 0f : (1f / this.FadeLength);
		Shader.SetGlobalMatrix("_AO_CameraProj", GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(0f, 1f, 0f, 1f, -1f, 100f), false));
		Shader.SetGlobalMatrix("_AO_CameraView", this.m_camera.worldToCameraMatrix);
		Shader.SetGlobalVector("_AO_UVToView", new Vector4(2f * vector2.x, -2f * vector2.y, -1f * vector2.x, 1f * vector2.y));
		Shader.SetGlobalFloat("_AO_HalfProjScale", 0.5f * num2);
		Shader.SetGlobalFloat("_AO_Radius", this.Radius);
		Shader.SetGlobalFloat("_AO_PowExponent", this.PowerExponent);
		Shader.SetGlobalFloat("_AO_Bias", num3);
		Shader.SetGlobalFloat("_AO_Multiplier", 1f / (1f - num3));
		Shader.SetGlobalFloat("_AO_BlurSharpness", this.BlurSharpness);
		Shader.SetGlobalColor("_AO_Levels", new Color(this.Tint.r, this.Tint.g, this.Tint.b, this.Intensity));
		Shader.SetGlobalVector("_AO_FadeParams", new Vector2(this.FadeStart, y));
		Shader.SetGlobalVector("_AO_FadeValues", new Vector3(this.FadeToIntensity, this.FadeToRadius, this.FadeToPowerExponent));
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00003058 File Offset: 0x00001458
	private void CommandBuffer_FillComputeOcclusion(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target)
	{
		this.CheckMaterial();
		this.CheckRandomData();
		cb.SetGlobalVector("_AO_Buffer_PadScale", new Vector4(target.padRatioWidth, target.padRatioHeight, 1f / target.padRatioWidth, 1f / target.padRatioHeight));
		cb.SetGlobalVector("_AO_Buffer_TexelSize", new Vector4(1f / (float)target.width, 1f / (float)target.height, (float)target.width, (float)target.height));
		cb.SetGlobalVector("_AO_QuarterBuffer_TexelSize", new Vector4(1f / (float)target.quarterWidth, 1f / (float)target.quarterHeight, (float)target.quarterWidth, (float)target.quarterHeight));
		cb.SetGlobalFloat("_AO_MaxRadiusPixels", (float)Mathf.Min(target.width, target.height));
		if (this.m_occlusionRT == null || this.m_occlusionRT.width != target.width || this.m_occlusionRT.height != target.height || !this.m_occlusionRT.IsCreated())
		{
			this.SafeReleaseRT(ref this.m_occlusionRT);
			this.m_occlusionRT = this.SafeAllocateRT("_AO_OcclusionTexture", target.width, target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear);
		}
		int num = -1;
		if (this.Downsample)
		{
			num = this.SafeAllocateTemporaryRT(cb, "_AO_SmallOcclusionTexture", target.width / 2, target.height / 2, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Bilinear);
		}
		if (this.CacheAware && !this.Downsample)
		{
			int num2 = this.SafeAllocateTemporaryRT(cb, "_AO_OcclusionAtlas", target.width, target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
			for (int i = 0; i < 16; i++)
			{
				this.m_depthLayerRT[i] = this.SafeAllocateTemporaryRT(cb, this.m_layerDepthNames[i], target.quarterWidth, target.quarterHeight, this.m_depthRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
				this.m_normalLayerRT[i] = this.SafeAllocateTemporaryRT(cb, this.m_layerNormalNames[i], target.quarterWidth, target.quarterHeight, this.m_normalRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
				this.m_occlusionLayerRT[i] = this.SafeAllocateTemporaryRT(cb, this.m_layerOcclusionNames[i], target.quarterWidth, target.quarterHeight, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
			}
			for (int j = 0; j < 16; j += this.m_mrtCount)
			{
				for (int k = 0; k < this.m_mrtCount; k++)
				{
					int num3 = k + j;
					int num4 = num3 & 3;
					int num5 = num3 >> 2;
					cb.SetGlobalVector(this.m_layerOffsetNames[k], new Vector2((float)num4 + 0.5f, (float)num5 + 0.5f));
					this.m_depthTargets[k] = this.m_depthLayerRT[num3];
					this.m_normalTargets[k] = this.m_normalLayerRT[num3];
				}
				this.SetBlitTarget(cb, this.m_depthTargets, target.quarterWidth, target.quarterHeight);
				this.PerformBlit(cb, this.m_occlusionMat, this.m_deinterleaveDepthPass);
				this.SetBlitTarget(cb, this.m_normalTargets, target.quarterWidth, target.quarterHeight);
				this.PerformBlit(cb, this.m_occlusionMat, (int)(this.m_deinterleaveNormalPass + this.PerPixelNormals));
			}
			for (int l = 0; l < 16; l++)
			{
				cb.SetGlobalVector("_AO_LayerOffset", new Vector2((float)(l & 3) + 0.5f, (float)(l >> 2) + 0.5f));
				cb.SetGlobalVector("_AO_LayerRandom", this.m_randomData[l]);
				cb.SetGlobalTexture("_AO_NormalTexture", this.m_normalLayerRT[l]);
				cb.SetGlobalTexture("_AO_DepthTexture", this.m_depthLayerRT[l]);
				this.SetBlitTarget(cb, this.m_occlusionLayerRT[l], target.quarterWidth, target.quarterHeight);
				this.PerformBlit(cb, this.m_occlusionMat, (int)(15 + this.SampleCount));
			}
			this.SetBlitTarget(cb, num2, target.width, target.height);
			for (int m = 0; m < 16; m++)
			{
				int x = (m & 3) * target.quarterWidth;
				int y = (m >> 2) * target.quarterHeight;
				this.PerformBlit(cb, this.m_occlusionLayerRT[m], target.quarterWidth, target.quarterHeight, this.m_copyMat, 0, x, y);
			}
			cb.SetGlobalTexture("_AO_OcclusionAtlas", num2);
			this.SetBlitTarget(cb, this.m_occlusionRT, target.width, target.height);
			this.PerformBlit(cb, this.m_occlusionMat, 19);
			for (int n = 0; n < 16; n++)
			{
				this.SafeReleaseTemporaryRT(cb, this.m_occlusionLayerRT[n]);
				this.SafeReleaseTemporaryRT(cb, this.m_normalLayerRT[n]);
				this.SafeReleaseTemporaryRT(cb, this.m_depthLayerRT[n]);
			}
			this.SafeReleaseTemporaryRT(cb, num2);
		}
		else
		{
			int pass = (int)(20 + (int)this.SampleCount * 4 + (int)this.PerPixelNormals);
			if (this.Downsample)
			{
				cb.Blit(null, new RenderTargetIdentifier(num), this.m_occlusionMat, pass);
				this.SetBlitTarget(cb, this.m_occlusionRT, target.width, target.height);
				this.PerformBlit(cb, num, target.width / 2, target.height / 2, this.m_occlusionMat, 41);
			}
			else
			{
				cb.Blit(null, this.m_occlusionRT, this.m_occlusionMat, pass);
			}
		}
		if (this.BlurEnabled)
		{
			int num6 = this.SafeAllocateTemporaryRT(cb, "_AO_TEMP", target.width, target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
			for (int num7 = 0; num7 < this.BlurPasses; num7++)
			{
				this.SetBlitTarget(cb, num6, target.width, target.height);
				this.PerformBlit(cb, this.m_occlusionRT, target.width, target.height, this.m_blurMat, (this.BlurRadius - 1) * 2);
				this.SetBlitTarget(cb, this.m_occlusionRT, target.width, target.height);
				this.PerformBlit(cb, num6, target.width, target.height, this.m_blurMat, 1 + (this.BlurRadius - 1) * 2);
			}
			this.SafeReleaseTemporaryRT(cb, num6);
		}
		if (this.Downsample && num >= 0)
		{
			this.SafeReleaseTemporaryRT(cb, num);
		}

        cb.SetRenderTarget(new RenderTargetIdentifier());
    }

	// Token: 0x06000020 RID: 32 RVA: 0x00003768 File Offset: 0x00001B68
	private void CommandBuffer_FillApplyDeferred(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target, bool logTarget)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", this.m_occlusionRT);
		this.m_applyDeferredTargets[0] = BuiltinRenderTextureType.GBuffer0;
		this.m_applyDeferredTargets[1] = ((!logTarget) ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3);
		if (!logTarget)
		{
			this.SetBlitTarget(cb, this.m_applyDeferredTargets, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 37);
		}
		else
		{
			int num = this.SafeAllocateTemporaryRT(cb, "_AO_GBufferAlbedo", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			int num2 = this.SafeAllocateTemporaryRT(cb, "_AO_GBufferEmission", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			cb.Blit(this.m_applyDeferredTargets[0], num);
			cb.Blit(this.m_applyDeferredTargets[1], num2);
			cb.SetGlobalTexture("_AO_GBufferAlbedo", num);
			cb.SetGlobalTexture("_AO_GBufferEmission", num2);
			this.SetBlitTarget(cb, this.m_applyDeferredTargets, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 38);
			this.SafeReleaseTemporaryRT(cb, num);
			this.SafeReleaseTemporaryRT(cb, num2);
		}
		cb.SetRenderTarget(new RenderTargetIdentifier());
	}

	// Token: 0x06000021 RID: 33 RVA: 0x000038DC File Offset: 0x00001CDC
	private void CommandBuffer_FillApplyPostEffect(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target, bool logTarget)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", this.m_occlusionRT);
		if (!logTarget)
		{
			this.SetBlitTarget(cb, BuiltinRenderTextureType.CameraTarget, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 39);
		}
		else
		{
			int num = this.SafeAllocateTemporaryRT(cb, "_AO_GBufferEmission", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			cb.Blit(BuiltinRenderTextureType.GBuffer3, num);
			cb.SetGlobalTexture("_AO_GBufferEmission", num);
			this.SetBlitTarget(cb, BuiltinRenderTextureType.GBuffer3, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 40);
			this.SafeReleaseTemporaryRT(cb, num);
		}
        cb.SetRenderTarget(new RenderTargetIdentifier());
    }

	// Token: 0x06000022 RID: 34 RVA: 0x000039B8 File Offset: 0x00001DB8
	private void CommandBuffer_FillApplyDebug(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", this.m_occlusionRT);
		this.SetBlitTarget(cb, BuiltinRenderTextureType.CameraTarget, target.fullWidth, target.fullHeight);
		this.PerformBlit(cb, this.m_occlusionMat, 36);
        cb.SetRenderTarget(new RenderTargetIdentifier());
    }

	// Token: 0x06000023 RID: 35 RVA: 0x00003A14 File Offset: 0x00001E14
	private void CommandBuffer_Rebuild(AmplifyOcclusionBase.TargetDesc target)
	{
		bool flag = this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.GBuffer || this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.GBufferOctaEncoded;
		CameraEvent cameraEvent = (!flag) ? CameraEvent.BeforeImageEffectsOpaque : CameraEvent.AfterLighting;
		if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Debug)
		{
			CommandBuffer cb = this.CommandBuffer_AllocateRegister(cameraEvent);
			this.CommandBuffer_FillComputeOcclusion(cb, target);
			this.CommandBuffer_FillApplyDebug(cb, target);
		}
		else
		{
			bool logTarget = !this.m_camera.hdr && flag;
			cameraEvent = ((this.ApplyMethod != AmplifyOcclusionBase.ApplicationMethod.Deferred) ? cameraEvent : CameraEvent.BeforeReflections);
			CommandBuffer cb = this.CommandBuffer_AllocateRegister(cameraEvent);
			this.CommandBuffer_FillComputeOcclusion(cb, target);
			if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.PostEffect)
			{
				this.CommandBuffer_FillApplyPostEffect(cb, target, logTarget);
			}
			else if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred)
			{
				this.CommandBuffer_FillApplyDeferred(cb, target, logTarget);
			}
		}
	}

	// Token: 0x06000024 RID: 36 RVA: 0x00003AE4 File Offset: 0x00001EE4
	private void OnPreRender()
	{
		bool hdr = this.m_camera.hdr;
		this.m_target.fullWidth = this.m_camera.pixelWidth;
		this.m_target.fullHeight = this.m_camera.pixelHeight;
		this.m_target.format = ((!hdr) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.ARGBHalf);
		this.m_target.width = ((!this.CacheAware) ? this.m_target.fullWidth : (this.m_target.fullWidth + 3 & -4));
		this.m_target.height = ((!this.CacheAware) ? this.m_target.fullHeight : (this.m_target.fullHeight + 3 & -4));
		this.m_target.quarterWidth = this.m_target.width / 4;
		this.m_target.quarterHeight = this.m_target.height / 4;
		this.m_target.padRatioWidth = (float)this.m_target.width / (float)this.m_target.fullWidth;
		this.m_target.padRatioHeight = (float)this.m_target.height / (float)this.m_target.fullHeight;
		this.UpdateGlobalShaderConstants(this.m_target);
		if (this.CheckParamsChanged() || this.m_registeredCommandBuffers.Count == 0)
		{
			this.CommandBuffer_UnregisterAll();
			this.CommandBuffer_Rebuild(this.m_target);
			this.UpdateParams();
		}
	}

	// Token: 0x06000025 RID: 37 RVA: 0x00003C64 File Offset: 0x00002064
	private void OnPostRender()
	{
		this.m_occlusionRT.MarkRestoreExpected();
	}

	// Token: 0x04000001 RID: 1
	[Header("Ambient Occlusion")]
	public AmplifyOcclusionBase.ApplicationMethod ApplyMethod;

	// Token: 0x04000002 RID: 2
	public AmplifyOcclusionBase.SampleCountLevel SampleCount = AmplifyOcclusionBase.SampleCountLevel.Medium;

	// Token: 0x04000003 RID: 3
	public AmplifyOcclusionBase.PerPixelNormalSource PerPixelNormals;

	// Token: 0x04000004 RID: 4
	[Range(0f, 1f)]
	public float Intensity = 1f;

	// Token: 0x04000005 RID: 5
	public Color Tint = Color.black;

	// Token: 0x04000006 RID: 6
	[Range(0f, 16f)]
	public float Radius = 1f;

	// Token: 0x04000007 RID: 7
	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	// Token: 0x04000008 RID: 8
	[Range(0f, 0.99f)]
	public float Bias = 0.05f;

	// Token: 0x04000009 RID: 9
	public bool CacheAware;

	// Token: 0x0400000A RID: 10
	public bool Downsample;

	// Token: 0x0400000B RID: 11
	[Header("Distance Fade")]
	public bool FadeEnabled;

	// Token: 0x0400000C RID: 12
	public float FadeStart = 100f;

	// Token: 0x0400000D RID: 13
	public float FadeLength = 50f;

	// Token: 0x0400000E RID: 14
	[Range(0f, 1f)]
	public float FadeToIntensity = 1f;

	// Token: 0x0400000F RID: 15
	[Range(0f, 16f)]
	public float FadeToRadius = 1f;

	// Token: 0x04000010 RID: 16
	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	// Token: 0x04000011 RID: 17
	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	// Token: 0x04000012 RID: 18
	[Range(1f, 4f)]
	public int BlurRadius = 2;

	// Token: 0x04000013 RID: 19
	[Range(1f, 4f)]
	public int BlurPasses = 1;

	// Token: 0x04000014 RID: 20
	[Range(0f, 20f)]
	public float BlurSharpness = 10f;

	// Token: 0x04000015 RID: 21
	private const int PerPixelNormalSourceCount = 4;

	// Token: 0x04000016 RID: 22
	private int prevScreenWidth;

	// Token: 0x04000017 RID: 23
	private int prevScreenHeight;

	// Token: 0x04000018 RID: 24
	private bool prevHDR;

	// Token: 0x04000019 RID: 25
	private AmplifyOcclusionBase.ApplicationMethod prevApplyMethod;

	// Token: 0x0400001A RID: 26
	private AmplifyOcclusionBase.SampleCountLevel prevSampleCount;

	// Token: 0x0400001B RID: 27
	private AmplifyOcclusionBase.PerPixelNormalSource prevPerPixelNormals;

	// Token: 0x0400001C RID: 28
	private bool prevCacheAware;

	// Token: 0x0400001D RID: 29
	private bool prevDownscale;

	// Token: 0x0400001E RID: 30
	private bool prevFadeEnabled;

	// Token: 0x0400001F RID: 31
	private float prevFadeToIntensity;

	// Token: 0x04000020 RID: 32
	private float prevFadeToRadius;

	// Token: 0x04000021 RID: 33
	private float prevFadeToPowerExponent;

	// Token: 0x04000022 RID: 34
	private float prevFadeStart;

	// Token: 0x04000023 RID: 35
	private float prevFadeLength;

	// Token: 0x04000024 RID: 36
	private bool prevBlurEnabled;

	// Token: 0x04000025 RID: 37
	private int prevBlurRadius;

	// Token: 0x04000026 RID: 38
	private int prevBlurPasses;

	// Token: 0x04000027 RID: 39
	private Camera m_camera;

	// Token: 0x04000028 RID: 40
	private Material m_occlusionMat;

	// Token: 0x04000029 RID: 41
	private Material m_blurMat;

	// Token: 0x0400002A RID: 42
	private Material m_copyMat;

	// Token: 0x0400002B RID: 43
	private const int RandomSize = 4;

	// Token: 0x0400002C RID: 44
	private const int DirectionCount = 8;

	// Token: 0x0400002D RID: 45
	private Color[] m_randomData;

	// Token: 0x0400002E RID: 46
	private string[] m_layerOffsetNames;

	// Token: 0x0400002F RID: 47
	private string[] m_layerRandomNames;

	// Token: 0x04000030 RID: 48
	private string[] m_layerDepthNames;

	// Token: 0x04000031 RID: 49
	private string[] m_layerNormalNames;

	// Token: 0x04000032 RID: 50
	private string[] m_layerOcclusionNames;

	// Token: 0x04000033 RID: 51
	private RenderTextureFormat m_depthRTFormat = RenderTextureFormat.RFloat;

	// Token: 0x04000034 RID: 52
	private RenderTextureFormat m_normalRTFormat = RenderTextureFormat.ARGB2101010;

	// Token: 0x04000035 RID: 53
	private RenderTextureFormat m_occlusionRTFormat = RenderTextureFormat.RGHalf;

	// Token: 0x04000036 RID: 54
	private RenderTexture m_occlusionRT;

	// Token: 0x04000037 RID: 55
	private int[] m_depthLayerRT;

	// Token: 0x04000038 RID: 56
	private int[] m_normalLayerRT;

	// Token: 0x04000039 RID: 57
	private int[] m_occlusionLayerRT;

	// Token: 0x0400003A RID: 58
	private int m_mrtCount;

	// Token: 0x0400003B RID: 59
	private RenderTargetIdentifier[] m_depthTargets;

	// Token: 0x0400003C RID: 60
	private RenderTargetIdentifier[] m_normalTargets;

	// Token: 0x0400003D RID: 61
	private int m_deinterleaveDepthPass;

	// Token: 0x0400003E RID: 62
	private int m_deinterleaveNormalPass;

	// Token: 0x0400003F RID: 63
	private RenderTargetIdentifier[] m_applyDeferredTargets;

	// Token: 0x04000040 RID: 64
	private Mesh m_blitMesh;

	// Token: 0x04000041 RID: 65
	private AmplifyOcclusionBase.TargetDesc m_target = default(AmplifyOcclusionBase.TargetDesc);

	// Token: 0x04000042 RID: 66
	private Dictionary<CameraEvent, CommandBuffer> m_registeredCommandBuffers = new Dictionary<CameraEvent, CommandBuffer>();

	// Token: 0x02000003 RID: 3
	public enum ApplicationMethod
	{
		// Token: 0x04000044 RID: 68
		PostEffect,
		// Token: 0x04000045 RID: 69
		Deferred,
		// Token: 0x04000046 RID: 70
		Debug
	}

	// Token: 0x02000004 RID: 4
	public enum PerPixelNormalSource
	{
		// Token: 0x04000048 RID: 72
		None,
		// Token: 0x04000049 RID: 73
		Camera,
		// Token: 0x0400004A RID: 74
		GBuffer,
		// Token: 0x0400004B RID: 75
		GBufferOctaEncoded
	}

	// Token: 0x02000005 RID: 5
	public enum SampleCountLevel
	{
		// Token: 0x0400004D RID: 77
		Low,
		// Token: 0x0400004E RID: 78
		Medium,
		// Token: 0x0400004F RID: 79
		High,
		// Token: 0x04000050 RID: 80
		VeryHigh
	}

	// Token: 0x02000006 RID: 6
	private static class ShaderPass
	{
		// Token: 0x04000051 RID: 81
		public const int FullDepth = 0;

		// Token: 0x04000052 RID: 82
		public const int FullNormal_None = 1;

		// Token: 0x04000053 RID: 83
		public const int FullNormal_Camera = 2;

		// Token: 0x04000054 RID: 84
		public const int FullNormal_GBuffer = 3;

		// Token: 0x04000055 RID: 85
		public const int FullNormal_GBufferOctaEncoded = 4;

		// Token: 0x04000056 RID: 86
		public const int DeinterleaveDepth1 = 5;

		// Token: 0x04000057 RID: 87
		public const int DeinterleaveNormal1_None = 6;

		// Token: 0x04000058 RID: 88
		public const int DeinterleaveNormal1_Camera = 7;

		// Token: 0x04000059 RID: 89
		public const int DeinterleaveNormal1_GBuffer = 8;

		// Token: 0x0400005A RID: 90
		public const int DeinterleaveNormal1_GBufferOctaEncoded = 9;

		// Token: 0x0400005B RID: 91
		public const int DeinterleaveDepth4 = 10;

		// Token: 0x0400005C RID: 92
		public const int DeinterleaveNormal4_None = 11;

		// Token: 0x0400005D RID: 93
		public const int DeinterleaveNormal4_Camera = 12;

		// Token: 0x0400005E RID: 94
		public const int DeinterleaveNormal4_GBuffer = 13;

		// Token: 0x0400005F RID: 95
		public const int DeinterleaveNormal4_GBufferOctaEncoded = 14;

		// Token: 0x04000060 RID: 96
		public const int OcclusionCache_Low = 15;

		// Token: 0x04000061 RID: 97
		public const int OcclusionCache_Medium = 16;

		// Token: 0x04000062 RID: 98
		public const int OcclusionCache_High = 17;

		// Token: 0x04000063 RID: 99
		public const int OcclusionCache_VeryHigh = 18;

		// Token: 0x04000064 RID: 100
		public const int Reinterleave = 19;

		// Token: 0x04000065 RID: 101
		public const int OcclusionLow_None = 20;

		// Token: 0x04000066 RID: 102
		public const int OcclusionLow_Camera = 21;

		// Token: 0x04000067 RID: 103
		public const int OcclusionLow_GBuffer = 22;

		// Token: 0x04000068 RID: 104
		public const int OcclusionLow_GBufferOctaEncoded = 23;

		// Token: 0x04000069 RID: 105
		public const int OcclusionMedium_None = 24;

		// Token: 0x0400006A RID: 106
		public const int OcclusionMedium_Camera = 25;

		// Token: 0x0400006B RID: 107
		public const int OcclusionMedium_GBuffer = 26;

		// Token: 0x0400006C RID: 108
		public const int OcclusionMedium_GBufferOctaEncoded = 27;

		// Token: 0x0400006D RID: 109
		public const int OcclusionHigh_None = 28;

		// Token: 0x0400006E RID: 110
		public const int OcclusionHigh_Camera = 29;

		// Token: 0x0400006F RID: 111
		public const int OcclusionHigh_GBuffer = 30;

		// Token: 0x04000070 RID: 112
		public const int OcclusionHigh_GBufferOctaEncoded = 31;

		// Token: 0x04000071 RID: 113
		public const int OcclusionVeryHigh_None = 32;

		// Token: 0x04000072 RID: 114
		public const int OcclusionVeryHigh_Camera = 33;

		// Token: 0x04000073 RID: 115
		public const int OcclusionVeryHigh_GBuffer = 34;

		// Token: 0x04000074 RID: 116
		public const int OcclusionVeryHigh_GBufferNormalEncoded = 35;

		// Token: 0x04000075 RID: 117
		public const int ApplyDebug = 36;

		// Token: 0x04000076 RID: 118
		public const int ApplyDeferred = 37;

		// Token: 0x04000077 RID: 119
		public const int ApplyDeferredLog = 38;

		// Token: 0x04000078 RID: 120
		public const int ApplyPostEffect = 39;

		// Token: 0x04000079 RID: 121
		public const int ApplyPostEffectLog = 40;

		// Token: 0x0400007A RID: 122
		public const int CombineDownsampledOcclusionDepth = 41;

		// Token: 0x0400007B RID: 123
		public const int BlurHorizontal1 = 0;

		// Token: 0x0400007C RID: 124
		public const int BlurVertical1 = 1;

		// Token: 0x0400007D RID: 125
		public const int BlurHorizontal2 = 2;

		// Token: 0x0400007E RID: 126
		public const int BlurVertical2 = 3;

		// Token: 0x0400007F RID: 127
		public const int BlurHorizontal3 = 4;

		// Token: 0x04000080 RID: 128
		public const int BlurVertical3 = 5;

		// Token: 0x04000081 RID: 129
		public const int BlurHorizontal4 = 6;

		// Token: 0x04000082 RID: 130
		public const int BlurVertical4 = 7;

		// Token: 0x04000083 RID: 131
		public const int Copy = 0;
	}

	// Token: 0x02000007 RID: 7
	private struct TargetDesc
	{
		// Token: 0x04000084 RID: 132
		public int fullWidth;

		// Token: 0x04000085 RID: 133
		public int fullHeight;

		// Token: 0x04000086 RID: 134
		public RenderTextureFormat format;

		// Token: 0x04000087 RID: 135
		public int width;

		// Token: 0x04000088 RID: 136
		public int height;

		// Token: 0x04000089 RID: 137
		public int quarterWidth;

		// Token: 0x0400008A RID: 138
		public int quarterHeight;

		// Token: 0x0400008B RID: 139
		public float padRatioWidth;

		// Token: 0x0400008C RID: 140
		public float padRatioHeight;
	}
}
