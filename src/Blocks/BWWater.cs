using UnityEngine;

namespace Blocks;

public class BWWater : MonoBehaviour
{
	public int textureSize = 256;

	public LayerMask reflectLayers;

	public LayerMask refractLayers;

	public RenderTexture reflectionTexture;

	public RenderTexture refractionTexture;

	private Camera reflectionCamera;

	private Camera refractionCamera;

	private void OnWillRenderObject()
	{
		if (base.enabled && (bool)GetComponent<Renderer>() && (bool)GetComponent<Renderer>().sharedMaterial)
		{
			Camera current = Camera.current;
			if ((bool)current)
			{
				Vector3 position = base.transform.position;
				Vector3 up = base.transform.up;
				CreateWaterObjects(current);
				RenderReflection(current, position, up);
				RenderRefraction(current, position, up);
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)reflectionTexture)
		{
			Object.DestroyImmediate(reflectionTexture);
		}
		if ((bool)refractionTexture)
		{
			Object.DestroyImmediate(refractionTexture);
		}
		if ((bool)reflectionCamera)
		{
			Object.DestroyImmediate(reflectionCamera.gameObject);
		}
		if ((bool)refractionCamera)
		{
			Object.DestroyImmediate(refractionCamera.gameObject);
		}
	}

	private void Update()
	{
	}

	private void CreateWaterObjects(Camera currentCamera)
	{
		if (!reflectionCamera)
		{
			GameObject gameObject = new GameObject("Reflection Camera", typeof(Camera));
			reflectionCamera = gameObject.GetComponent<Camera>();
			reflectionCamera.enabled = false;
		}
		if (!refractionCamera)
		{
			GameObject gameObject2 = new GameObject("Refraction Camera", typeof(Camera));
			refractionCamera = gameObject2.GetComponent<Camera>();
			refractionCamera.enabled = false;
		}
		reflectionCamera.transform.position = base.transform.position;
		reflectionCamera.transform.rotation = base.transform.rotation;
		refractionCamera.transform.position = currentCamera.transform.position;
		refractionCamera.transform.rotation = currentCamera.transform.rotation;
	}

	private void RenderReflection(Camera currentCamera, Vector3 position, Vector3 up)
	{
		if (reflectionTexture == null)
		{
			CreateTexture(reflectionTexture);
		}
		Vector4 plane = new Vector4(up.x, up.y, up.z, 0f - Vector3.Dot(up, position));
		Matrix4x4 matrix4x = CalculateReflectionMatrix(plane);
		reflectionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix * matrix4x;
		reflectionCamera.cullingMask = reflectLayers;
		reflectionCamera.targetTexture = reflectionTexture;
		reflectionCamera.transform.position = position + Vector3.up * 0.1f;
		reflectionCamera.Render();
		GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", reflectionTexture);
	}

	private void RenderRefraction(Camera currentCamera, Vector3 position, Vector3 up)
	{
		if (refractionTexture == null)
		{
			CreateTexture(refractionTexture);
		}
		refractionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix;
		refractionCamera.cullingMask = refractLayers;
		refractionCamera.targetTexture = refractionTexture;
		refractionCamera.transform.position = currentCamera.transform.position;
		refractionCamera.Render();
		GetComponent<Renderer>().sharedMaterial.SetTexture("_RefractionTex", refractionTexture);
	}

	private void CreateTexture(RenderTexture texture)
	{
		if (texture != null)
		{
			Object.DestroyImmediate(texture);
		}
		texture = new RenderTexture(textureSize, textureSize, 16);
		texture.isPowerOfTwo = true;
		texture.hideFlags = HideFlags.DontSave;
	}

	private Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
	{
		Matrix4x4 zero = Matrix4x4.zero;
		float num = -2f * plane.w;
		zero.m00 = 1f - 2f * plane.x * plane.x;
		zero.m01 = -2f * plane.x * plane.y;
		zero.m02 = -2f * plane.x * plane.z;
		zero.m03 = num * plane.x;
		zero.m10 = -2f * plane.x * plane.y;
		zero.m11 = 1f - 2f * plane.y * plane.y;
		zero.m12 = -2f * plane.y * plane.z;
		zero.m13 = num * plane.y;
		zero.m20 = -2f * plane.x * plane.z;
		zero.m21 = -2f * plane.y * plane.z;
		zero.m22 = 1f - 2f * plane.z * plane.z;
		zero.m23 = num * plane.z;
		zero.m30 = 0f;
		zero.m31 = 0f;
		zero.m32 = 0f;
		zero.m33 = 1f;
		return zero;
	}
}
