using System;
using UnityEngine;

public class StereoCamera : MonoBehaviour
{
	private Camera cameraMain;

	private Camera cameraL;

	private Camera cameraR;

	private float eyeLFOVUpTan;

	private float eyeLFOVDownTan;

	private float eyeLFOVLeftTan;

	private float eyeLFOVRightTan;

	private float eyeRFOVUpTan;

	private float eyeRFOVDownTan;

	private float eyeRFOVLeftTan;

	private float eyeRFOVRightTan;

	private const float DEG2RAD = (float)Math.PI / 180f;

	private Transform myTransform;

	private Transform cameraLTransform;

	private Transform cameraRTransform;

	private Vector3 webVREuler;

	private Vector3 webVRPosition;

	private void Start()
	{
		myTransform = base.transform;
		cameraLTransform = cameraL.transform;
		cameraRTransform = cameraR.transform;
		eyeL_fovUpDegrees(53.094387f);
		eyeL_fovDownDegrees(53.094387f);
		eyeL_fovLeftDegrees(47.527695f);
		eyeL_fovRightDegrees(46.632095f);
		eyeR_fovUpDegrees(53.094387f);
		eyeR_fovDownDegrees(53.094387f);
		eyeR_fovLeftDegrees(47.527695f);
		eyeR_fovRightDegrees(46.632095f);
	}

	private Camera createCopyOfMainCamera(string name, Vector3 localPosition, Rect cameraRect)
	{
		GameObject obj = new GameObject(name);
		obj.tag = cameraMain.tag;
		obj.transform.parent = cameraMain.transform.parent;
		obj.transform.position = cameraMain.transform.position;
		obj.transform.rotation = cameraMain.transform.rotation;
		obj.transform.localPosition = localPosition;
		Camera camera = obj.AddComponent<Camera>();
		camera.rect = cameraRect;
		camera.cullingMask = cameraMain.cullingMask;
		camera.fov = cameraMain.fov;
		camera.nearClipPlane = cameraMain.nearClipPlane;
		camera.farClipPlane = cameraMain.farClipPlane;
		camera.clearFlags = cameraMain.clearFlags;
		camera.backgroundColor = cameraMain.backgroundColor;
		camera.depth = cameraMain.depth;
		return camera;
	}

	internal void SetupStereoCameras(Camera mainCamera)
	{
		cameraMain = mainCamera;
		cameraL = createCopyOfMainCamera("CameraL", new Vector3(-0.15f, 0f, 0f), new Rect(0f, 0f, 0.5f, 1f));
		cameraR = createCopyOfMainCamera("CameraR", new Vector3(0.15f, 0f, 0f), new Rect(0.5f, 0f, 0.5f, 1f));
	}

	internal void SetVREnabled(bool enabled)
	{
		if (enabled)
		{
			cameraMain.GetComponent<Camera>().enabled = false;
			cameraL.GetComponent<Camera>().enabled = true;
			cameraR.GetComponent<Camera>().enabled = true;
		}
		else
		{
			cameraMain.GetComponent<Camera>().enabled = true;
			cameraL.GetComponent<Camera>().enabled = false;
			cameraR.GetComponent<Camera>().enabled = false;
		}
	}

	internal Camera[] GetEyes()
	{
		return new Camera[2] { cameraL, cameraR };
	}

	private void Update()
	{
		Vector3 euler = ConvertWebVREulerToUnity(webVREuler);
		euler.x = 0f - euler.x;
		euler.z = 0f - euler.z;
		myTransform.localRotation = Quaternion.Euler(euler);
		Vector3 localPosition = webVRPosition * 10f;
		localPosition.z *= -1f;
		myTransform.localPosition = localPosition;
	}

	private void eyeL_translation(float x, float y, float z)
	{
		cameraLTransform.position.Set(x, y, z);
	}

	private void eyeR_translation(float x, float y, float z)
	{
		cameraRTransform.position.Set(x, y, z);
	}

	private void eyeL_translation_x(float val)
	{
		cameraLTransform.position.Set(val, 0f, 0f);
	}

	private void eyeR_translation_x(float val)
	{
		cameraRTransform.position.Set(val, 0f, 0f);
	}

	private void eyeL_fovUpDegrees(float val)
	{
		eyeLFOVUpTan = (float)Math.Tan(val * ((float)Math.PI / 180f)) * cameraMain.nearClipPlane;
	}

	private void eyeL_fovDownDegrees(float val)
	{
		eyeLFOVDownTan = (0f - (float)Math.Tan(val * ((float)Math.PI / 180f))) * cameraMain.nearClipPlane;
	}

	private void eyeL_fovLeftDegrees(float val)
	{
		eyeLFOVLeftTan = (0f - (float)Math.Tan(val * ((float)Math.PI / 180f))) * cameraMain.nearClipPlane;
	}

	private void eyeL_fovRightDegrees(float val)
	{
		eyeLFOVRightTan = (float)Math.Tan(val * ((float)Math.PI / 180f)) * cameraMain.nearClipPlane;
		cameraL.projectionMatrix = PerspectiveOffCenter(eyeLFOVLeftTan, eyeLFOVRightTan, eyeLFOVDownTan, eyeLFOVUpTan, cameraMain.nearClipPlane, cameraMain.farClipPlane);
	}

	private void eyeR_fovUpDegrees(float val)
	{
		eyeRFOVUpTan = (float)Math.Tan(val * ((float)Math.PI / 180f)) * cameraMain.nearClipPlane;
	}

	private void eyeR_fovDownDegrees(float val)
	{
		eyeRFOVDownTan = (0f - (float)Math.Tan(val * ((float)Math.PI / 180f))) * cameraMain.nearClipPlane;
	}

	private void eyeR_fovLeftDegrees(float val)
	{
		eyeRFOVLeftTan = (0f - (float)Math.Tan(val * ((float)Math.PI / 180f))) * cameraMain.nearClipPlane;
	}

	private void eyeR_fovRightDegrees(float val)
	{
		try
		{
			eyeRFOVRightTan = (float)Math.Tan(val * ((float)Math.PI / 180f)) * cameraMain.nearClipPlane;
			Matrix4x4 projectionMatrix = PerspectiveOffCenter(eyeRFOVLeftTan, eyeRFOVRightTan, eyeRFOVDownTan, eyeRFOVUpTan, cameraMain.nearClipPlane, cameraMain.farClipPlane);
			cameraR.projectionMatrix = projectionMatrix;
		}
		catch (Exception ex)
		{
			Application.ExternalEval("console.log('" + ex.StackTrace + "')");
		}
	}

	private void euler_x(float val)
	{
		webVREuler.x = val;
	}

	private void euler_y(float val)
	{
		webVREuler.y = val;
	}

	private void euler_z(float val)
	{
		webVREuler.z = val;
	}

	private void position_x(float val)
	{
		webVRPosition.x = val;
	}

	private void position_y(float val)
	{
		webVRPosition.y = val;
	}

	private void position_z(float val)
	{
		webVRPosition.z = val;
	}

	private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
	{
		float value = 2f * near / (right - left);
		float value2 = 2f * near / (top - bottom);
		float value3 = (right + left) / (right - left);
		float value4 = (top + bottom) / (top - bottom);
		float value5 = (0f - (far + near)) / (far - near);
		float value6 = (0f - 2f * far * near) / (far - near);
		float value7 = -1f;
		return new Matrix4x4
		{
			[0, 0] = value,
			[0, 1] = 0f,
			[0, 2] = value3,
			[0, 3] = 0f,
			[1, 0] = 0f,
			[1, 1] = value2,
			[1, 2] = value4,
			[1, 3] = 0f,
			[2, 0] = 0f,
			[2, 1] = 0f,
			[2, 2] = value5,
			[2, 3] = value6,
			[3, 0] = 0f,
			[3, 1] = 0f,
			[3, 2] = value7,
			[3, 3] = 0f
		};
	}

	private Vector3 ConvertWebVREulerToUnity(Vector3 eulerThreejs)
	{
		eulerThreejs.x *= -1f;
		eulerThreejs.z *= -1f;
		Matrix4x4 matrix4x = CreateRotationalMatrixThreejs(ref eulerThreejs);
		Matrix4x4 matrix = matrix4x;
		matrix.m02 *= -1f;
		matrix.m12 *= -1f;
		matrix.m20 *= -1f;
		matrix.m21 *= -1f;
		return ExtractRotationFromMatrix(ref matrix).eulerAngles;
	}

	private Matrix4x4 CreateRotationalMatrixThreejs(ref Vector3 eulerThreejs)
	{
		float num = Mathf.Cos(eulerThreejs.x);
		float num2 = Mathf.Cos(eulerThreejs.y);
		float num3 = Mathf.Cos(eulerThreejs.z);
		float num4 = Mathf.Sin(eulerThreejs.x);
		float num5 = Mathf.Sin(eulerThreejs.y);
		float num6 = Mathf.Sin(eulerThreejs.z);
		return new Matrix4x4
		{
			m00 = num2 * num3,
			m01 = (0f - num2) * num6,
			m02 = num5,
			m10 = num * num6 + num3 * num4 * num5,
			m11 = num * num3 - num4 * num5 * num6,
			m12 = (0f - num2) * num4,
			m20 = num4 * num6 - num * num3 * num5,
			m21 = num3 * num4 + num * num5 * num6,
			m22 = num * num2,
			m33 = 1f
		};
	}

	private Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 forward = default(Vector3);
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;
		Vector3 upwards = default(Vector3);
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;
		return Quaternion.LookRotation(forward, upwards);
	}
}
