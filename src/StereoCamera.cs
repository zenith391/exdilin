using System;
using UnityEngine;

// Token: 0x02000468 RID: 1128
public class StereoCamera : MonoBehaviour
{
	// Token: 0x06002F83 RID: 12163 RVA: 0x0014F8E8 File Offset: 0x0014DCE8
	private void Start()
	{
		this.myTransform = base.transform;
		this.cameraLTransform = this.cameraL.transform;
		this.cameraRTransform = this.cameraR.transform;
		this.eyeL_fovUpDegrees(53.0943871f);
		this.eyeL_fovDownDegrees(53.0943871f);
		this.eyeL_fovLeftDegrees(47.5276947f);
		this.eyeL_fovRightDegrees(46.6320953f);
		this.eyeR_fovUpDegrees(53.0943871f);
		this.eyeR_fovDownDegrees(53.0943871f);
		this.eyeR_fovLeftDegrees(47.5276947f);
		this.eyeR_fovRightDegrees(46.6320953f);
	}

	// Token: 0x06002F84 RID: 12164 RVA: 0x0014F97C File Offset: 0x0014DD7C
	private Camera createCopyOfMainCamera(string name, Vector3 localPosition, Rect cameraRect)
	{
		Camera camera = new GameObject(name)
		{
			tag = this.cameraMain.tag,
			transform = 
			{
				parent = this.cameraMain.transform.parent,
				position = this.cameraMain.transform.position,
				rotation = this.cameraMain.transform.rotation,
				localPosition = localPosition
			}
		}.AddComponent<Camera>();
		camera.rect = cameraRect;
		camera.cullingMask = this.cameraMain.cullingMask;
		camera.fov = this.cameraMain.fov;
		camera.nearClipPlane = this.cameraMain.nearClipPlane;
		camera.farClipPlane = this.cameraMain.farClipPlane;
		camera.clearFlags = this.cameraMain.clearFlags;
		camera.backgroundColor = this.cameraMain.backgroundColor;
		camera.depth = this.cameraMain.depth;
		return camera;
	}

	// Token: 0x06002F85 RID: 12165 RVA: 0x0014FA84 File Offset: 0x0014DE84
	internal void SetupStereoCameras(Camera mainCamera)
	{
		this.cameraMain = mainCamera;
		this.cameraL = this.createCopyOfMainCamera("CameraL", new Vector3(-0.15f, 0f, 0f), new Rect(0f, 0f, 0.5f, 1f));
		this.cameraR = this.createCopyOfMainCamera("CameraR", new Vector3(0.15f, 0f, 0f), new Rect(0.5f, 0f, 0.5f, 1f));
	}

	// Token: 0x06002F86 RID: 12166 RVA: 0x0014FB14 File Offset: 0x0014DF14
	internal void SetVREnabled(bool enabled)
	{
		if (enabled)
		{
			this.cameraMain.GetComponent<Camera>().enabled = false;
			this.cameraL.GetComponent<Camera>().enabled = true;
			this.cameraR.GetComponent<Camera>().enabled = true;
		}
		else
		{
			this.cameraMain.GetComponent<Camera>().enabled = true;
			this.cameraL.GetComponent<Camera>().enabled = false;
			this.cameraR.GetComponent<Camera>().enabled = false;
		}
	}

	// Token: 0x06002F87 RID: 12167 RVA: 0x0014FB92 File Offset: 0x0014DF92
	internal Camera[] GetEyes()
	{
		return new Camera[]
		{
			this.cameraL,
			this.cameraR
		};
	}

	// Token: 0x06002F88 RID: 12168 RVA: 0x0014FBAC File Offset: 0x0014DFAC
	private void Update()
	{
		Vector3 euler = this.ConvertWebVREulerToUnity(this.webVREuler);
		euler.x = -euler.x;
		euler.z = -euler.z;
		this.myTransform.localRotation = Quaternion.Euler(euler);
		Vector3 localPosition = this.webVRPosition * 10f;
		localPosition.z *= -1f;
		this.myTransform.localPosition = localPosition;
	}

	// Token: 0x06002F89 RID: 12169 RVA: 0x0014FC28 File Offset: 0x0014E028
	private void eyeL_translation(float x, float y, float z)
	{
		this.cameraLTransform.position.Set(x, y, z);
	}

	// Token: 0x06002F8A RID: 12170 RVA: 0x0014FC4C File Offset: 0x0014E04C
	private void eyeR_translation(float x, float y, float z)
	{
		this.cameraRTransform.position.Set(x, y, z);
	}

	// Token: 0x06002F8B RID: 12171 RVA: 0x0014FC70 File Offset: 0x0014E070
	private void eyeL_translation_x(float val)
	{
		this.cameraLTransform.position.Set(val, 0f, 0f);
	}

	// Token: 0x06002F8C RID: 12172 RVA: 0x0014FC9C File Offset: 0x0014E09C
	private void eyeR_translation_x(float val)
	{
		this.cameraRTransform.position.Set(val, 0f, 0f);
	}

	// Token: 0x06002F8D RID: 12173 RVA: 0x0014FCC7 File Offset: 0x0014E0C7
	private void eyeL_fovUpDegrees(float val)
	{
		this.eyeLFOVUpTan = (float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
	}

	// Token: 0x06002F8E RID: 12174 RVA: 0x0014FCE9 File Offset: 0x0014E0E9
	private void eyeL_fovDownDegrees(float val)
	{
		this.eyeLFOVDownTan = -(float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
	}

	// Token: 0x06002F8F RID: 12175 RVA: 0x0014FD0C File Offset: 0x0014E10C
	private void eyeL_fovLeftDegrees(float val)
	{
		this.eyeLFOVLeftTan = -(float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
	}

	// Token: 0x06002F90 RID: 12176 RVA: 0x0014FD30 File Offset: 0x0014E130
	private void eyeL_fovRightDegrees(float val)
	{
		this.eyeLFOVRightTan = (float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
		this.cameraL.projectionMatrix = StereoCamera.PerspectiveOffCenter(this.eyeLFOVLeftTan, this.eyeLFOVRightTan, this.eyeLFOVDownTan, this.eyeLFOVUpTan, this.cameraMain.nearClipPlane, this.cameraMain.farClipPlane);
	}

	// Token: 0x06002F91 RID: 12177 RVA: 0x0014FD9B File Offset: 0x0014E19B
	private void eyeR_fovUpDegrees(float val)
	{
		this.eyeRFOVUpTan = (float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
	}

	// Token: 0x06002F92 RID: 12178 RVA: 0x0014FDBD File Offset: 0x0014E1BD
	private void eyeR_fovDownDegrees(float val)
	{
		this.eyeRFOVDownTan = -(float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
	}

	// Token: 0x06002F93 RID: 12179 RVA: 0x0014FDE0 File Offset: 0x0014E1E0
	private void eyeR_fovLeftDegrees(float val)
	{
		this.eyeRFOVLeftTan = -(float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
	}

	// Token: 0x06002F94 RID: 12180 RVA: 0x0014FE04 File Offset: 0x0014E204
	private void eyeR_fovRightDegrees(float val)
	{
		try
		{
			this.eyeRFOVRightTan = (float)Math.Tan((double)(val * 0.0174532924f)) * this.cameraMain.nearClipPlane;
			Matrix4x4 projectionMatrix = StereoCamera.PerspectiveOffCenter(this.eyeRFOVLeftTan, this.eyeRFOVRightTan, this.eyeRFOVDownTan, this.eyeRFOVUpTan, this.cameraMain.nearClipPlane, this.cameraMain.farClipPlane);
			this.cameraR.projectionMatrix = projectionMatrix;
		}
		catch (Exception ex)
		{
			Application.ExternalEval("console.log('" + ex.StackTrace + "')");
		}
	}

	// Token: 0x06002F95 RID: 12181 RVA: 0x0014FEA8 File Offset: 0x0014E2A8
	private void euler_x(float val)
	{
		this.webVREuler.x = val;
	}

	// Token: 0x06002F96 RID: 12182 RVA: 0x0014FEB6 File Offset: 0x0014E2B6
	private void euler_y(float val)
	{
		this.webVREuler.y = val;
	}

	// Token: 0x06002F97 RID: 12183 RVA: 0x0014FEC4 File Offset: 0x0014E2C4
	private void euler_z(float val)
	{
		this.webVREuler.z = val;
	}

	// Token: 0x06002F98 RID: 12184 RVA: 0x0014FED2 File Offset: 0x0014E2D2
	private void position_x(float val)
	{
		this.webVRPosition.x = val;
	}

	// Token: 0x06002F99 RID: 12185 RVA: 0x0014FEE0 File Offset: 0x0014E2E0
	private void position_y(float val)
	{
		this.webVRPosition.y = val;
	}

	// Token: 0x06002F9A RID: 12186 RVA: 0x0014FEEE File Offset: 0x0014E2EE
	private void position_z(float val)
	{
		this.webVRPosition.z = val;
	}

	// Token: 0x06002F9B RID: 12187 RVA: 0x0014FEFC File Offset: 0x0014E2FC
	private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
	{
		float value = 2f * near / (right - left);
		float value2 = 2f * near / (top - bottom);
		float value3 = (right + left) / (right - left);
		float value4 = (top + bottom) / (top - bottom);
		float value5 = -(far + near) / (far - near);
		float value6 = -(2f * far * near) / (far - near);
		float value7 = -1f;
		Matrix4x4 result = default(Matrix4x4);
		result[0, 0] = value;
		result[0, 1] = 0f;
		result[0, 2] = value3;
		result[0, 3] = 0f;
		result[1, 0] = 0f;
		result[1, 1] = value2;
		result[1, 2] = value4;
		result[1, 3] = 0f;
		result[2, 0] = 0f;
		result[2, 1] = 0f;
		result[2, 2] = value5;
		result[2, 3] = value6;
		result[3, 0] = 0f;
		result[3, 1] = 0f;
		result[3, 2] = value7;
		result[3, 3] = 0f;
		return result;
	}

	// Token: 0x06002F9C RID: 12188 RVA: 0x00150030 File Offset: 0x0014E430
	private Vector3 ConvertWebVREulerToUnity(Vector3 eulerThreejs)
	{
		eulerThreejs.x *= -1f;
		eulerThreejs.z *= -1f;
		Matrix4x4 matrix4x = this.CreateRotationalMatrixThreejs(ref eulerThreejs);
		Matrix4x4 matrix4x2 = matrix4x;
		matrix4x2.m02 *= -1f;
		matrix4x2.m12 *= -1f;
		matrix4x2.m20 *= -1f;
		matrix4x2.m21 *= -1f;
		return this.ExtractRotationFromMatrix(ref matrix4x2).eulerAngles;
	}

	// Token: 0x06002F9D RID: 12189 RVA: 0x001500CC File Offset: 0x0014E4CC
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
			m01 = -num2 * num6,
			m02 = num5,
			m10 = num * num6 + num3 * num4 * num5,
			m11 = num * num3 - num4 * num5 * num6,
			m12 = -num2 * num4,
			m20 = num4 * num6 - num * num3 * num5,
			m21 = num3 * num4 + num * num5 * num6,
			m22 = num * num2,
			m33 = 1f
		};
	}

	// Token: 0x06002F9E RID: 12190 RVA: 0x001501B8 File Offset: 0x0014E5B8
	private Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 forward;
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;
		Vector3 upwards;
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;
		return Quaternion.LookRotation(forward, upwards);
	}

	// Token: 0x040027CD RID: 10189
	private Camera cameraMain;

	// Token: 0x040027CE RID: 10190
	private Camera cameraL;

	// Token: 0x040027CF RID: 10191
	private Camera cameraR;

	// Token: 0x040027D0 RID: 10192
	private float eyeLFOVUpTan;

	// Token: 0x040027D1 RID: 10193
	private float eyeLFOVDownTan;

	// Token: 0x040027D2 RID: 10194
	private float eyeLFOVLeftTan;

	// Token: 0x040027D3 RID: 10195
	private float eyeLFOVRightTan;

	// Token: 0x040027D4 RID: 10196
	private float eyeRFOVUpTan;

	// Token: 0x040027D5 RID: 10197
	private float eyeRFOVDownTan;

	// Token: 0x040027D6 RID: 10198
	private float eyeRFOVLeftTan;

	// Token: 0x040027D7 RID: 10199
	private float eyeRFOVRightTan;

	// Token: 0x040027D8 RID: 10200
	private const float DEG2RAD = 0.0174532924f;

	// Token: 0x040027D9 RID: 10201
	private Transform myTransform;

	// Token: 0x040027DA RID: 10202
	private Transform cameraLTransform;

	// Token: 0x040027DB RID: 10203
	private Transform cameraRTransform;

	// Token: 0x040027DC RID: 10204
	private Vector3 webVREuler = default(Vector3);

	// Token: 0x040027DD RID: 10205
	private Vector3 webVRPosition = default(Vector3);
}
