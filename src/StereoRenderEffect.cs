using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/StereoRenderEffect")]
public class StereoRenderEffect : MonoBehaviour
{
	private Material material;

	private Camera camera;

	private void Awake()
	{
		camera = GetComponent<Camera>();
	}

	private void Start()
	{
		material = new Material(Shader.Find("Cardboard/SkyboxMesh"));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		GL.PushMatrix();
		int num = ((!dest) ? Screen.width : dest.width);
		int num2 = ((!dest) ? Screen.height : dest.height);
		GL.LoadPixelMatrix(0f, num, num2, 0f);
		Rect pixelRect = camera.pixelRect;
		pixelRect.y = (float)num2 - pixelRect.height - pixelRect.y;
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = dest;
		Graphics.DrawTexture(pixelRect, source, material);
		RenderTexture.active = active;
		GL.PopMatrix();
	}
}
