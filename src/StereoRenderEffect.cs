using System;
using UnityEngine;

// Token: 0x02000028 RID: 40
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cardboard/StereoRenderEffect")]
public class StereoRenderEffect : MonoBehaviour
{
	// Token: 0x06000150 RID: 336 RVA: 0x00008E9C File Offset: 0x0000729C
	private void Awake()
	{
		this.camera = base.GetComponent<Camera>();
	}

	// Token: 0x06000151 RID: 337 RVA: 0x00008EAA File Offset: 0x000072AA
	private void Start()
	{
		this.material = new Material(Shader.Find("Cardboard/SkyboxMesh"));
	}

	// Token: 0x06000152 RID: 338 RVA: 0x00008EC4 File Offset: 0x000072C4
	private void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		GL.PushMatrix();
		int num = (!dest) ? Screen.width : dest.width;
		int num2 = (!dest) ? Screen.height : dest.height;
		GL.LoadPixelMatrix(0f, (float)num, (float)num2, 0f);
		Rect pixelRect = this.camera.pixelRect;
		pixelRect.y = (float)num2 - pixelRect.height - pixelRect.y;
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = dest;
		Graphics.DrawTexture(pixelRect, source, this.material);
		RenderTexture.active = active;
		GL.PopMatrix();
	}

	// Token: 0x0400018A RID: 394
	private Material material;

	// Token: 0x0400018B RID: 395
	private Camera camera;
}
