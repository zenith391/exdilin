using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200003C RID: 60
	public class BWWater : MonoBehaviour
	{
		// Token: 0x06000205 RID: 517 RVA: 0x0000B934 File Offset: 0x00009D34
		public void OnWillRenderObject()
		{
			if (!base.enabled || !base.GetComponent<Renderer>() || !base.GetComponent<Renderer>().sharedMaterial || !base.GetComponent<Renderer>().enabled)
			{
				return;
			}
			Camera current = Camera.current;
			if (!current)
			{
				return;
			}
			if (BWWater.s_InsideWater)
			{
				return;
			}
			BWWater.s_InsideWater = true;
			this.m_HardwareWaterSupport = this.FindHardwareWaterSupport();
			BWWater.WaterMode waterMode = this.GetWaterMode();
			Camera camera;
			Camera camera2;
			this.CreateWaterObjects(current, out camera, out camera2);
			Vector3 position = base.transform.position;
			Vector3 up = base.transform.up;
			int pixelLightCount = QualitySettings.pixelLightCount;
			if (this.disablePixelLights)
			{
				QualitySettings.pixelLightCount = 0;
			}
			this.UpdateCameraModes(current, camera);
			this.UpdateCameraModes(current, camera2);
			if (waterMode >= BWWater.WaterMode.Reflective)
			{
				float w = -Vector3.Dot(up, position) - this.clipPlaneOffset;
				Vector4 plane = new Vector4(up.x, up.y, up.z, w);
				Matrix4x4 zero = Matrix4x4.zero;
				BWWater.CalculateReflectionMatrix(ref zero, plane);
				Vector3 position2 = current.transform.position;
				Vector3 position3 = zero.MultiplyPoint(position2);
				camera.worldToCameraMatrix = current.worldToCameraMatrix * zero;
				Vector4 clipPlane = this.CameraSpacePlane(camera, position, up, 1f);
				camera.projectionMatrix = current.CalculateObliqueMatrix(clipPlane);
				camera.cullingMatrix = current.projectionMatrix * current.worldToCameraMatrix;
				camera.cullingMask = this.reflectLayers.value;
				camera.targetTexture = this.m_ReflectionTexture;
				bool invertCulling = GL.invertCulling;
				GL.invertCulling = !invertCulling;
				camera.transform.position = position3;
				Vector3 eulerAngles = current.transform.eulerAngles;
				camera.transform.eulerAngles = new Vector3(-eulerAngles.x, eulerAngles.y, eulerAngles.z);
				camera.Render();
				camera.transform.position = position2;
				GL.invertCulling = invertCulling;
				base.GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", this.m_ReflectionTexture);
			}
			if (waterMode >= BWWater.WaterMode.Refractive)
			{
				camera2.worldToCameraMatrix = current.worldToCameraMatrix;
				Vector4 clipPlane2 = this.CameraSpacePlane(camera2, position, up, -1f);
				camera2.projectionMatrix = current.CalculateObliqueMatrix(clipPlane2);
				camera2.cullingMatrix = current.projectionMatrix * current.worldToCameraMatrix;
				camera2.cullingMask = this.refractLayers.value;
				camera2.targetTexture = this.m_RefractionTexture;
				camera2.transform.position = current.transform.position;
				camera2.transform.rotation = current.transform.rotation;
				camera2.Render();
				base.GetComponent<Renderer>().sharedMaterial.SetTexture("_RefractionTex", this.m_RefractionTexture);
			}
			if (this.disablePixelLights)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			if (waterMode != BWWater.WaterMode.Simple)
			{
				if (waterMode != BWWater.WaterMode.Reflective)
				{
					if (waterMode == BWWater.WaterMode.Refractive)
					{
						Shader.DisableKeyword("WATER_SIMPLE");
						Shader.DisableKeyword("WATER_REFLECTIVE");
						Shader.EnableKeyword("WATER_REFRACTIVE");
					}
				}
				else
				{
					Shader.DisableKeyword("WATER_SIMPLE");
					Shader.EnableKeyword("WATER_REFLECTIVE");
					Shader.DisableKeyword("WATER_REFRACTIVE");
				}
			}
			else
			{
				Shader.EnableKeyword("WATER_SIMPLE");
				Shader.DisableKeyword("WATER_REFLECTIVE");
				Shader.DisableKeyword("WATER_REFRACTIVE");
			}
			BWWater.s_InsideWater = false;
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000BCA0 File Offset: 0x0000A0A0
		private void OnDisable()
		{
			if (this.m_ReflectionTexture)
			{
				UnityEngine.Object.DestroyImmediate(this.m_ReflectionTexture);
				this.m_ReflectionTexture = null;
			}
			if (this.m_RefractionTexture)
			{
				UnityEngine.Object.DestroyImmediate(this.m_RefractionTexture);
				this.m_RefractionTexture = null;
			}
			foreach (KeyValuePair<Camera, Camera> keyValuePair in this.m_ReflectionCameras)
			{
				UnityEngine.Object.DestroyImmediate(keyValuePair.Value.gameObject);
			}
			this.m_ReflectionCameras.Clear();
			foreach (KeyValuePair<Camera, Camera> keyValuePair2 in this.m_RefractionCameras)
			{
				UnityEngine.Object.DestroyImmediate(keyValuePair2.Value.gameObject);
			}
			this.m_RefractionCameras.Clear();
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0000BDB8 File Offset: 0x0000A1B8
		private void Update()
		{
			if (!base.GetComponent<Renderer>())
			{
				return;
			}
			Material sharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
			if (!sharedMaterial)
			{
				return;
			}
			Vector4 vector = sharedMaterial.GetVector("WaveSpeed");
			float @float = sharedMaterial.GetFloat("_WaveScale");
			Vector4 vector2 = new Vector4(@float, @float, @float * 0.4f, @float * 0.45f);
			double num = (double)Time.timeSinceLevelLoad / 20.0;
			Vector4 vector3 = new Vector4((float)Math.IEEERemainder((double)(vector.x * vector2.x) * num, 1.0), (float)Math.IEEERemainder((double)(vector.y * vector2.y) * num, 1.0), (float)Math.IEEERemainder((double)(vector.z * vector2.z) * num, 1.0), (float)Math.IEEERemainder((double)(vector.w * vector2.w) * num, 1.0));
			sharedMaterial.SetVector("_WaveOffset", vector3);
			sharedMaterial.SetVector("_WaveScale4", vector2);
			if (Blocksworld.worldOceanBlock != null)
			{
				Vector3 position = Blocksworld.cameraTransform.position;
				float num2 = 0.25f + Blocksworld.worldOceanBlock.WaterLevelOffset(position);
				this.clipPlaneOffset = Blocksworld.worldOceanBlock.GetScale().y * 0.5f + num2;
			}
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000BF24 File Offset: 0x0000A324
		private void UpdateCameraModes(Camera src, Camera dest)
		{
			if (dest == null)
			{
				return;
			}
			dest.clearFlags = src.clearFlags;
			dest.backgroundColor = src.backgroundColor;
			if (src.clearFlags == CameraClearFlags.Skybox)
			{
				Skybox component = src.GetComponent<Skybox>();
				Skybox component2 = dest.GetComponent<Skybox>();
				if (!component || !component.material)
				{
					component2.enabled = false;
				}
				else
				{
					component2.enabled = true;
					component2.material = component.material;
				}
			}
			dest.farClipPlane = src.farClipPlane;
			dest.nearClipPlane = src.nearClipPlane;
			dest.orthographic = src.orthographic;
			dest.fieldOfView = src.fieldOfView;
			dest.aspect = src.aspect;
			dest.orthographicSize = src.orthographicSize;
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000BFF4 File Offset: 0x0000A3F4
		private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
		{
			BWWater.WaterMode waterMode = this.GetWaterMode();
			reflectionCamera = null;
			refractionCamera = null;
			if (waterMode >= BWWater.WaterMode.Reflective)
			{
				if (!this.m_ReflectionTexture || this.m_OldReflectionTextureSize != this.textureSize)
				{
					if (this.m_ReflectionTexture)
					{
						UnityEngine.Object.DestroyImmediate(this.m_ReflectionTexture);
					}
					this.m_ReflectionTexture = new RenderTexture(this.textureSize, this.textureSize, 16);
					this.m_ReflectionTexture.name = "__WaterReflection" + base.GetInstanceID();
					this.m_ReflectionTexture.isPowerOfTwo = true;
					this.m_ReflectionTexture.hideFlags = HideFlags.DontSave;
					this.m_OldReflectionTextureSize = this.textureSize;
				}
				this.m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
				if (!reflectionCamera)
				{
					GameObject gameObject = new GameObject(string.Concat(new object[]
					{
						"Water Refl Camera id",
						base.GetInstanceID(),
						" for ",
						currentCamera.GetInstanceID()
					}), new Type[]
					{
						typeof(Camera),
						typeof(Skybox)
					});
					reflectionCamera = gameObject.GetComponent<Camera>();
					reflectionCamera.enabled = false;
					reflectionCamera.transform.position = base.transform.position;
					reflectionCamera.transform.rotation = base.transform.rotation;
					reflectionCamera.gameObject.AddComponent<FlareLayer>();
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					this.m_ReflectionCameras[currentCamera] = reflectionCamera;
				}
			}
			if (waterMode >= BWWater.WaterMode.Refractive)
			{
				if (!this.m_RefractionTexture || this.m_OldRefractionTextureSize != this.textureSize)
				{
					if (this.m_RefractionTexture)
					{
						UnityEngine.Object.DestroyImmediate(this.m_RefractionTexture);
					}
					this.m_RefractionTexture = new RenderTexture(this.textureSize, this.textureSize, 16);
					this.m_RefractionTexture.name = "__WaterRefraction" + base.GetInstanceID();
					this.m_RefractionTexture.isPowerOfTwo = true;
					this.m_RefractionTexture.hideFlags = HideFlags.DontSave;
					this.m_OldRefractionTextureSize = this.textureSize;
				}
				this.m_RefractionCameras.TryGetValue(currentCamera, out refractionCamera);
				if (!refractionCamera)
				{
					GameObject gameObject2 = new GameObject(string.Concat(new object[]
					{
						"Water Refr Camera id",
						base.GetInstanceID(),
						" for ",
						currentCamera.GetInstanceID()
					}), new Type[]
					{
						typeof(Camera),
						typeof(Skybox)
					});
					refractionCamera = gameObject2.GetComponent<Camera>();
					refractionCamera.enabled = false;
					refractionCamera.transform.position = base.transform.position;
					refractionCamera.transform.rotation = base.transform.rotation;
					refractionCamera.gameObject.AddComponent<FlareLayer>();
					gameObject2.hideFlags = HideFlags.HideAndDontSave;
					this.m_RefractionCameras[currentCamera] = refractionCamera;
				}
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000C300 File Offset: 0x0000A700
		private BWWater.WaterMode GetWaterMode()
		{
			if (this.m_HardwareWaterSupport < this.waterMode)
			{
				return this.m_HardwareWaterSupport;
			}
			return this.waterMode;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000C320 File Offset: 0x0000A720
		private BWWater.WaterMode FindHardwareWaterSupport()
		{
			if (!base.GetComponent<Renderer>())
			{
				return BWWater.WaterMode.Simple;
			}
			Material sharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
			if (!sharedMaterial)
			{
				return BWWater.WaterMode.Simple;
			}
			string tag = sharedMaterial.GetTag("WATERMODE", false);
			if (tag == "Refractive")
			{
				return BWWater.WaterMode.Refractive;
			}
			if (tag == "Reflective")
			{
				return BWWater.WaterMode.Reflective;
			}
			return BWWater.WaterMode.Simple;
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000C38C File Offset: 0x0000A78C
		private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 v = pos + normal * this.clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(v);
			Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(rhs.x, rhs.y, rhs.z, -Vector3.Dot(lhs, rhs));
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000C3F8 File Offset: 0x0000A7F8
		private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
			reflectionMat.m01 = -2f * plane[0] * plane[1];
			reflectionMat.m02 = -2f * plane[0] * plane[2];
			reflectionMat.m03 = -2f * plane[3] * plane[0];
			reflectionMat.m10 = -2f * plane[1] * plane[0];
			reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
			reflectionMat.m12 = -2f * plane[1] * plane[2];
			reflectionMat.m13 = -2f * plane[3] * plane[1];
			reflectionMat.m20 = -2f * plane[2] * plane[0];
			reflectionMat.m21 = -2f * plane[2] * plane[1];
			reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
			reflectionMat.m23 = -2f * plane[3] * plane[2];
			reflectionMat.m30 = 0f;
			reflectionMat.m31 = 0f;
			reflectionMat.m32 = 0f;
			reflectionMat.m33 = 1f;
		}

		// Token: 0x040001EF RID: 495
		public BWWater.WaterMode waterMode = BWWater.WaterMode.Refractive;

		// Token: 0x040001F0 RID: 496
		public bool disablePixelLights = true;

		// Token: 0x040001F1 RID: 497
		public int textureSize = 256;

		// Token: 0x040001F2 RID: 498
		private float clipPlaneOffset = 512f;

		// Token: 0x040001F3 RID: 499
		public LayerMask reflectLayers = -1;

		// Token: 0x040001F4 RID: 500
		public LayerMask refractLayers = -1;

		// Token: 0x040001F5 RID: 501
		private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();

		// Token: 0x040001F6 RID: 502
		private Dictionary<Camera, Camera> m_RefractionCameras = new Dictionary<Camera, Camera>();

		// Token: 0x040001F7 RID: 503
		private RenderTexture m_ReflectionTexture;

		// Token: 0x040001F8 RID: 504
		private RenderTexture m_RefractionTexture;

		// Token: 0x040001F9 RID: 505
		private BWWater.WaterMode m_HardwareWaterSupport = BWWater.WaterMode.Refractive;

		// Token: 0x040001FA RID: 506
		private int m_OldReflectionTextureSize;

		// Token: 0x040001FB RID: 507
		private int m_OldRefractionTextureSize;

		// Token: 0x040001FC RID: 508
		private static bool s_InsideWater;

		// Token: 0x0200003D RID: 61
		public enum WaterMode
		{
			// Token: 0x040001FE RID: 510
			Simple,
			// Token: 0x040001FF RID: 511
			Reflective,
			// Token: 0x04000200 RID: 512
			Refractive
		}
	}
}
