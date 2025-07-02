using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/CardboardPostRender")]
public class CardboardPostRender : MonoBehaviour
{
	private const int kMeshWidth = 40;

	private const int kMeshHeight = 40;

	private const bool kDistortVertices = true;

	private Mesh distortionMesh;

	private Material meshMaterial;

	private Material uiMaterial;

	private float centerWidthPx;

	private float buttonWidthPx;

	private float xScale;

	private float yScale;

	private Matrix4x4 xfm;

	private const float kAnglePerGearSection = 60f;

	private const float kOuterRimEndAngle = 12f;

	private const float kInnerRimBeginAngle = 20f;

	private const float kOuterRadius = 1f;

	private const float kMiddleRadius = 0.75f;

	private const float kInnerRadius = 0.3125f;

	private const float kCenterLineThicknessDp = 4f;

	private const int kButtonWidthDp = 28;

	private const float kTouchSlopFactor = 1.5f;

	private static readonly float[] Angles = new float[5] { 0f, 12f, 20f, 40f, 48f };

	public Camera cam { get; private set; }

	private void Reset()
	{
		cam.clearFlags = CameraClearFlags.Depth;
		cam.backgroundColor = Color.magenta;
		cam.orthographic = true;
		cam.orthographicSize = 0.5f;
		cam.cullingMask = 0;
		cam.useOcclusionCulling = false;
		cam.depth = 100f;
	}

	private void Awake()
	{
		cam = GetComponent<Camera>();
		Reset();
		meshMaterial = new Material(Shader.Find("Cardboard/UnlitTexture"));
		uiMaterial = new Material(Shader.Find("Cardboard/SolidColor"));
		uiMaterial.color = new Color(0.8f, 0.8f, 0.8f);
		if (!Application.isEditor)
		{
			ComputeUIMatrix();
		}
	}

	private void OnRenderObject()
	{
		if (Camera.current != cam)
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
			if (distortionMesh == null || Cardboard.SDK.ProfileChanged)
			{
				RebuildDistortionMesh();
			}
			meshMaterial.mainTexture = stereoScreen;
			meshMaterial.SetPass(0);
			Graphics.DrawMeshNow(distortionMesh, base.transform.position, base.transform.rotation);
		}
		stereoScreen.DiscardContents();
		if (!Cardboard.SDK.NativeUILayerSupported && Cardboard.SDK.UILayerEnabled)
		{
			DrawUILayer();
		}
	}

	private void RebuildDistortionMesh()
	{
		distortionMesh = new Mesh();
		ComputeMeshPoints(40, 40, distortVertices: true, out var vertices, out var tex);
		int[] array = ComputeMeshIndices(40, 40, distortVertices: true);
		Color[] colors = ComputeMeshColors(40, 40, tex, array, distortVertices: true);
		distortionMesh.vertices = vertices;
		distortionMesh.uv = tex;
		distortionMesh.colors = colors;
		distortionMesh.triangles = array;
		distortionMesh.UploadMeshData(markNoLogerReadable: true);
	}

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
		for (; i < 2; i++)
		{
			for (int j = 0; j < height; j++)
			{
				int num2 = 0;
				while (num2 < width)
				{
					float num3 = (float)num2 / (float)(width - 1);
					float num4 = (float)j / (float)(height - 1);
					float y;
					float num5;
					if (distortVertices)
					{
						num5 = num3;
						y = num4;
						float num6 = Mathf.Lerp(array[0], array[2], num3);
						float num7 = Mathf.Lerp(array[3], array[1], num4);
						float num8 = Mathf.Sqrt(num6 * num6 + num7 * num7);
						float num9 = profile.device.distortion.distortInv(num8);
						float num10 = num6 * num9 / num8;
						float num11 = num7 * num9 / num8;
						num3 = (num10 - array2[0]) / (array2[2] - array2[0]);
						num4 = (num11 - array2[3]) / (array2[1] - array2[3]);
					}
					else
					{
						float num12 = Mathf.Lerp(array2[0], array2[2], num3);
						float num13 = Mathf.Lerp(array2[3], array2[1], num4);
						float num14 = Mathf.Sqrt(num12 * num12 + num13 * num13);
						float num15 = profile.device.distortion.distort(num14);
						float num16 = num12 * num15 / num14;
						float num17 = num13 * num15 / num14;
						num5 = Mathf.Clamp01((num16 - array[0]) / (array[2] - array[0]));
						y = Mathf.Clamp01((num17 - array[3]) / (array[1] - array[3]));
					}
					float num18 = profile.screen.width / profile.screen.height;
					num3 = (leftEyeVisibleScreenRect.x + num3 * leftEyeVisibleScreenRect.width - 0.5f) * num18;
					num4 = leftEyeVisibleScreenRect.y + num4 * leftEyeVisibleScreenRect.height - 0.5f;
					vertices[num] = new Vector3(num3, num4, 1f);
					num5 = (num5 + (float)i) / 2f;
					tex[num] = new Vector2(num5, y);
					num2++;
					num++;
				}
			}
			float num19 = array[2] - array[0];
			array[0] = 0f - (num19 + array[0]);
			array[2] = num19 - array[2];
			num19 = array2[2] - array2[0];
			array2[0] = 0f - (num19 + array2[0]);
			array2[2] = num19 - array2[2];
			leftEyeVisibleScreenRect.x = 1f - (leftEyeVisibleScreenRect.x + leftEyeVisibleScreenRect.width);
		}
	}

	private static Color[] ComputeMeshColors(int width, int height, Vector2[] tex, int[] indices, bool distortVertices)
	{
		Color[] array = new Color[2 * width * height];
		int i = 0;
		int num = 0;
		for (; i < 2; i++)
		{
			for (int j = 0; j < height; j++)
			{
				int num2 = 0;
				while (num2 < width)
				{
					array[num] = Color.white;
					if (distortVertices)
					{
						if (num2 == 0 || j == 0 || num2 == width - 1 || j == height - 1)
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
					num2++;
					num++;
				}
			}
		}
		return array;
	}

	private static int[] ComputeMeshIndices(int width, int height, bool distortVertices)
	{
		int[] array = new int[2 * (width - 1) * (height - 1) * 6];
		int num = width / 2;
		int num2 = height / 2;
		int i = 0;
		int num3 = 0;
		int num4 = 0;
		for (; i < 2; i++)
		{
			for (int j = 0; j < height; j++)
			{
				int num5 = 0;
				while (num5 < width)
				{
					if (num5 != 0 && j != 0)
					{
						if (num5 <= num == j <= num2)
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
					num5++;
					num3++;
				}
			}
		}
		return array;
	}

	private void DrawUILayer()
	{
		bool vRModeEnabled = Cardboard.SDK.VRModeEnabled;
		if (Application.isEditor)
		{
			ComputeUIMatrix();
		}
		uiMaterial.SetPass(0);
		if (vRModeEnabled && Cardboard.SDK.EnableSettingsButton)
		{
			DrawSettingsButton();
		}
		if (vRModeEnabled && Cardboard.SDK.EnableAlignmentMarker)
		{
			DrawAlignmentMarker();
		}
		if (Cardboard.SDK.BackButtonMode == Cardboard.BackButtonModes.On || (vRModeEnabled && Cardboard.SDK.BackButtonMode == Cardboard.BackButtonModes.OnlyInVR))
		{
			DrawVRBackButton();
		}
	}

	private void ComputeUIMatrix()
	{
		centerWidthPx = 0.025f * Screen.dpi / 2f;
		buttonWidthPx = 0.175f * Screen.dpi / 2f;
		xScale = buttonWidthPx / (float)Screen.width;
		yScale = buttonWidthPx / (float)Screen.height;
		xfm = Matrix4x4.TRS(new Vector3(0.5f, yScale, 0f), Quaternion.identity, new Vector3(xScale, yScale, 1f));
	}

	private void DrawSettingsButton()
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.MultMatrix(xfm);
		GL.Begin(5);
		int i = 0;
		for (int num = Angles.Length * 6; i <= num; i++)
		{
			float num2 = (float)(i / Angles.Length) * 60f + Angles[i % Angles.Length];
			float f = (90f - num2) * ((float)Math.PI / 180f);
			float num3 = Mathf.Cos(f);
			float num4 = Mathf.Sin(f);
			float num5 = Mathf.PingPong(num2, 30f);
			float t = (num5 - 12f) / 8f;
			float num6 = Mathf.Lerp(1f, 0.75f, t);
			GL.Vertex3(0.3125f * num3, 0.3125f * num4, 0f);
			GL.Vertex3(num6 * num3, num6 * num4, 0f);
		}
		GL.End();
		GL.PopMatrix();
	}

	private void DrawAlignmentMarker()
	{
		int num = Screen.width / 2;
		int num2 = (int)centerWidthPx;
		int num3 = (int)(3f * buttonWidthPx);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, Screen.width, 0f, Screen.height);
		GL.Begin(7);
		GL.Vertex3(num - num2, num3, 0f);
		GL.Vertex3(num - num2, Screen.height - num3, 0f);
		GL.Vertex3(num + num2, Screen.height - num3, 0f);
		GL.Vertex3(num + num2, num3, 0f);
		GL.End();
		GL.PopMatrix();
	}

	private void DrawVRBackButton()
	{
	}
}
