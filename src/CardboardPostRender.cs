using System;
using UnityEngine;

// Token: 0x0200001A RID: 26
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/CardboardPostRender")]
public class CardboardPostRender : MonoBehaviour
{
	// Token: 0x17000030 RID: 48
	// (get) Token: 0x060000F9 RID: 249 RVA: 0x00006608 File Offset: 0x00004A08
	// (set) Token: 0x060000FA RID: 250 RVA: 0x00006610 File Offset: 0x00004A10
	public Camera cam { get; private set; }

	// Token: 0x060000FB RID: 251 RVA: 0x0000661C File Offset: 0x00004A1C
	private void Reset()
	{
		this.cam.clearFlags = CameraClearFlags.Depth;
		this.cam.backgroundColor = Color.magenta;
		this.cam.orthographic = true;
		this.cam.orthographicSize = 0.5f;
		this.cam.cullingMask = 0;
		this.cam.useOcclusionCulling = false;
		this.cam.depth = 100f;
	}

	// Token: 0x060000FC RID: 252 RVA: 0x0000668C File Offset: 0x00004A8C
	private void Awake()
	{
		this.cam = base.GetComponent<Camera>();
		this.Reset();
		this.meshMaterial = new Material(Shader.Find("Cardboard/UnlitTexture"));
		this.uiMaterial = new Material(Shader.Find("Cardboard/SolidColor"));
		this.uiMaterial.color = new Color(0.8f, 0.8f, 0.8f);
		if (!Application.isEditor)
		{
			this.ComputeUIMatrix();
		}
	}

	// Token: 0x060000FD RID: 253 RVA: 0x00006704 File Offset: 0x00004B04
	private void OnRenderObject()
	{
		if (Camera.current != this.cam)
		{
			return;
		}
		Cardboard.SDK.UpdateState();
		Cardboard.DistortionCorrectionMethod distortionCorrection = Cardboard.SDK.DistortionCorrection;
		RenderTexture stereoScreen = Cardboard.SDK.StereoScreen;
		if (stereoScreen == null || distortionCorrection == Cardboard.DistortionCorrectionMethod.None)
		{
			return;
		}
		if (distortionCorrection == Cardboard.DistortionCorrectionMethod.Native && Cardboard.SDK.NativeDistortionCorrectionSupported)
		{
			Cardboard.SDK.PostRender();
		}
		else
		{
			if (this.distortionMesh == null || Cardboard.SDK.ProfileChanged)
			{
				this.RebuildDistortionMesh();
			}
			this.meshMaterial.mainTexture = stereoScreen;
			this.meshMaterial.SetPass(0);
			Graphics.DrawMeshNow(this.distortionMesh, base.transform.position, base.transform.rotation);
		}
		stereoScreen.DiscardContents();
		if (!Cardboard.SDK.NativeUILayerSupported && Cardboard.SDK.UILayerEnabled)
		{
			this.DrawUILayer();
		}
	}

	// Token: 0x060000FE RID: 254 RVA: 0x0000680C File Offset: 0x00004C0C
	private void RebuildDistortionMesh()
	{
		this.distortionMesh = new Mesh();
		Vector3[] vertices;
		Vector2[] array;
		CardboardPostRender.ComputeMeshPoints(40, 40, true, out vertices, out array);
		int[] array2 = CardboardPostRender.ComputeMeshIndices(40, 40, true);
		Color[] colors = CardboardPostRender.ComputeMeshColors(40, 40, array, array2, true);
		this.distortionMesh.vertices = vertices;
		this.distortionMesh.uv = array;
		this.distortionMesh.colors = colors;
		this.distortionMesh.triangles = array2;
		this.distortionMesh.UploadMeshData(true);
	}

	// Token: 0x060000FF RID: 255 RVA: 0x00006888 File Offset: 0x00004C88
	private static void ComputeMeshPoints(int width, int height, bool distortVertices, out Vector3[] vertices, out Vector2[] tex)
	{
		float[] array = new float[4];
		float[] array2 = new float[4];
		CardboardProfile profile = Cardboard.SDK.Profile;
		profile.GetLeftEyeVisibleTanAngles(array);
		profile.GetLeftEyeNoLensTanAngles(array2);
		Rect leftEyeVisibleScreenRect = profile.GetLeftEyeVisibleScreenRect(array2);
		vertices = new Vector3[2 * width * height];
		tex = new Vector2[2 * width * height];
		int i = 0;
		int num = 0;
		while (i < 2)
		{
			for (int j = 0; j < height; j++)
			{
				int k = 0;
				while (k < width)
				{
					float num2 = (float)k / (float)(width - 1);
					float num3 = (float)j / (float)(height - 1);
					float num4;
					float y;
					if (distortVertices)
					{
						num4 = num2;
						y = num3;
						float num5 = Mathf.Lerp(array[0], array[2], num2);
						float num6 = Mathf.Lerp(array[3], array[1], num3);
						float num7 = Mathf.Sqrt(num5 * num5 + num6 * num6);
						float num8 = profile.device.distortion.distortInv(num7);
						float num9 = num5 * num8 / num7;
						float num10 = num6 * num8 / num7;
						num2 = (num9 - array2[0]) / (array2[2] - array2[0]);
						num3 = (num10 - array2[3]) / (array2[1] - array2[3]);
					}
					else
					{
						float num11 = Mathf.Lerp(array2[0], array2[2], num2);
						float num12 = Mathf.Lerp(array2[3], array2[1], num3);
						float num13 = Mathf.Sqrt(num11 * num11 + num12 * num12);
						float num14 = profile.device.distortion.distort(num13);
						float num15 = num11 * num14 / num13;
						float num16 = num12 * num14 / num13;
						num4 = Mathf.Clamp01((num15 - array[0]) / (array[2] - array[0]));
						y = Mathf.Clamp01((num16 - array[3]) / (array[1] - array[3]));
					}
					float num17 = profile.screen.width / profile.screen.height;
					num2 = (leftEyeVisibleScreenRect.x + num2 * leftEyeVisibleScreenRect.width - 0.5f) * num17;
					num3 = leftEyeVisibleScreenRect.y + num3 * leftEyeVisibleScreenRect.height - 0.5f;
					vertices[num] = new Vector3(num2, num3, 1f);
					num4 = (num4 + (float)i) / 2f;
					tex[num] = new Vector2(num4, y);
					k++;
					num++;
				}
			}
			float num18 = array[2] - array[0];
			array[0] = -(num18 + array[0]);
			array[2] = num18 - array[2];
			num18 = array2[2] - array2[0];
			array2[0] = -(num18 + array2[0]);
			array2[2] = num18 - array2[2];
			leftEyeVisibleScreenRect.x = 1f - (leftEyeVisibleScreenRect.x + leftEyeVisibleScreenRect.width);
			i++;
		}
	}

	// Token: 0x06000100 RID: 256 RVA: 0x00006B30 File Offset: 0x00004F30
	private static Color[] ComputeMeshColors(int width, int height, Vector2[] tex, int[] indices, bool distortVertices)
	{
		Color[] array = new Color[2 * width * height];
		int i = 0;
		int num = 0;
		while (i < 2)
		{
			for (int j = 0; j < height; j++)
			{
				int k = 0;
				while (k < width)
				{
					array[num] = Color.white;
					if (distortVertices)
					{
						if (k == 0 || j == 0 || k == width - 1 || j == height - 1)
						{
							array[num] = Color.black;
						}
					}
					else
					{
						Vector2 vector = tex[num];
						vector.x = Mathf.Abs(vector.x * 2f - 1f);
						if (vector.x <= 0f || vector.y <= 0f || vector.x >= 1f || vector.y >= 1f)
						{
							array[num] = Color.black;
						}
					}
					k++;
					num++;
				}
			}
			i++;
		}
		return array;
	}

	// Token: 0x06000101 RID: 257 RVA: 0x00006C5C File Offset: 0x0000505C
	private static int[] ComputeMeshIndices(int width, int height, bool distortVertices)
	{
		int[] array = new int[2 * (width - 1) * (height - 1) * 6];
		int num = width / 2;
		int num2 = height / 2;
		int i = 0;
		int num3 = 0;
		int num4 = 0;
		while (i < 2)
		{
			for (int j = 0; j < height; j++)
			{
				int k = 0;
				while (k < width)
				{
					if (k != 0 && j != 0)
					{
						if (k <= num == j <= num2)
						{
							array[num4++] = num3;
							array[num4++] = num3 - width;
							array[num4++] = num3 - width - 1;
							array[num4++] = num3 - width - 1;
							array[num4++] = num3 - 1;
							array[num4++] = num3;
						}
						else
						{
							array[num4++] = num3 - 1;
							array[num4++] = num3;
							array[num4++] = num3 - width;
							array[num4++] = num3 - width;
							array[num4++] = num3 - width - 1;
							array[num4++] = num3 - 1;
						}
					}
					k++;
					num3++;
				}
			}
			i++;
		}
		return array;
	}

	// Token: 0x06000102 RID: 258 RVA: 0x00006D98 File Offset: 0x00005198
	private void DrawUILayer()
	{
		bool vrmodeEnabled = Cardboard.SDK.VRModeEnabled;
		if (Application.isEditor)
		{
			this.ComputeUIMatrix();
		}
		this.uiMaterial.SetPass(0);
		if (vrmodeEnabled && Cardboard.SDK.EnableSettingsButton)
		{
			this.DrawSettingsButton();
		}
		if (vrmodeEnabled && Cardboard.SDK.EnableAlignmentMarker)
		{
			this.DrawAlignmentMarker();
		}
		if (Cardboard.SDK.BackButtonMode == Cardboard.BackButtonModes.On || (vrmodeEnabled && Cardboard.SDK.BackButtonMode == Cardboard.BackButtonModes.OnlyInVR))
		{
			this.DrawVRBackButton();
		}
	}

	// Token: 0x06000103 RID: 259 RVA: 0x00006E30 File Offset: 0x00005230
	private void ComputeUIMatrix()
	{
		this.centerWidthPx = 0.025f * Screen.dpi / 2f;
		this.buttonWidthPx = 0.175f * Screen.dpi / 2f;
		this.xScale = this.buttonWidthPx / (float)Screen.width;
		this.yScale = this.buttonWidthPx / (float)Screen.height;
		this.xfm = Matrix4x4.TRS(new Vector3(0.5f, this.yScale, 0f), Quaternion.identity, new Vector3(this.xScale, this.yScale, 1f));
	}

	// Token: 0x06000104 RID: 260 RVA: 0x00006ECC File Offset: 0x000052CC
	private void DrawSettingsButton()
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.MultMatrix(this.xfm);
		GL.Begin(5);
		int i = 0;
		int num = CardboardPostRender.Angles.Length * 6;
		while (i <= num)
		{
			float num2 = (float)(i / CardboardPostRender.Angles.Length) * 60f + CardboardPostRender.Angles[i % CardboardPostRender.Angles.Length];
			float f = (90f - num2) * 0.0174532924f;
			float num3 = Mathf.Cos(f);
			float num4 = Mathf.Sin(f);
			float num5 = Mathf.PingPong(num2, 30f);
			float t = (num5 - 12f) / 8f;
			float num6 = Mathf.Lerp(1f, 0.75f, t);
			GL.Vertex3(0.3125f * num3, 0.3125f * num4, 0f);
			GL.Vertex3(num6 * num3, num6 * num4, 0f);
			i++;
		}
		GL.End();
		GL.PopMatrix();
	}

	// Token: 0x06000105 RID: 261 RVA: 0x00006FB8 File Offset: 0x000053B8
	private void DrawAlignmentMarker()
	{
		int num = Screen.width / 2;
		int num2 = (int)this.centerWidthPx;
		int num3 = (int)(3f * this.buttonWidthPx);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, (float)Screen.width, 0f, (float)Screen.height);
		GL.Begin(7);
		GL.Vertex3((float)(num - num2), (float)num3, 0f);
		GL.Vertex3((float)(num - num2), (float)(Screen.height - num3), 0f);
		GL.Vertex3((float)(num + num2), (float)(Screen.height - num3), 0f);
		GL.Vertex3((float)(num + num2), (float)num3, 0f);
		GL.End();
		GL.PopMatrix();
	}

	// Token: 0x06000106 RID: 262 RVA: 0x0000705F File Offset: 0x0000545F
	private void DrawVRBackButton()
	{
	}

	// Token: 0x04000127 RID: 295
	private const int kMeshWidth = 40;

	// Token: 0x04000128 RID: 296
	private const int kMeshHeight = 40;

	// Token: 0x04000129 RID: 297
	private const bool kDistortVertices = true;

	// Token: 0x0400012A RID: 298
	private Mesh distortionMesh;

	// Token: 0x0400012B RID: 299
	private Material meshMaterial;

	// Token: 0x0400012C RID: 300
	private Material uiMaterial;

	// Token: 0x0400012D RID: 301
	private float centerWidthPx;

	// Token: 0x0400012E RID: 302
	private float buttonWidthPx;

	// Token: 0x0400012F RID: 303
	private float xScale;

	// Token: 0x04000130 RID: 304
	private float yScale;

	// Token: 0x04000131 RID: 305
	private Matrix4x4 xfm;

	// Token: 0x04000132 RID: 306
	private const float kAnglePerGearSection = 60f;

	// Token: 0x04000133 RID: 307
	private const float kOuterRimEndAngle = 12f;

	// Token: 0x04000134 RID: 308
	private const float kInnerRimBeginAngle = 20f;

	// Token: 0x04000135 RID: 309
	private const float kOuterRadius = 1f;

	// Token: 0x04000136 RID: 310
	private const float kMiddleRadius = 0.75f;

	// Token: 0x04000137 RID: 311
	private const float kInnerRadius = 0.3125f;

	// Token: 0x04000138 RID: 312
	private const float kCenterLineThicknessDp = 4f;

	// Token: 0x04000139 RID: 313
	private const int kButtonWidthDp = 28;

	// Token: 0x0400013A RID: 314
	private const float kTouchSlopFactor = 1.5f;

	// Token: 0x0400013B RID: 315
	private static readonly float[] Angles = new float[]
	{
		0f,
		12f,
		20f,
		40f,
		48f
	};
}
