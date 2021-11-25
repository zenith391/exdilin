using System;
using UnityEngine;

// Token: 0x0200024B RID: 587
public class VisualizeTapsMB : MonoBehaviour
{
	// Token: 0x06001B08 RID: 6920 RVA: 0x000C5F70 File Offset: 0x000C4370
	public void Setup(GameObject masterObject, Texture2D tapTexture)
	{
		this.tapObjects = new GameObject[5];
		this.renderers = new Renderer[5];
		Rect r = new Rect((float)(-(float)tapTexture.width / 2), (float)(tapTexture.height / 2), (float)tapTexture.width, (float)(-(float)tapTexture.height));
		r = HudMeshUtils.NormalizedRect(r);
		Color color = new Color(1f, 1f, 1f, 0.5f);
		Shader shader = Resources.Load("Shaders/UnlitAlpha") as Shader;
		for (int i = 0; i < 5; i++)
		{
			this.tapObjects[i] = HudMeshUtils.CreateMeshObject("Tap " + i.ToString(), tapTexture);
			this.tapObjects[i].transform.parent = masterObject.transform;
			this.renderers[i] = this.tapObjects[i].GetComponent<Renderer>();
			Material material = new Material(shader);
			material.SetTexture("_MainTex", tapTexture);
			material.SetColor("_Tint", color);
			this.renderers[i].sharedMaterial = material;
			this.renderers[i].enabled = false;
			Mesh mesh = HudMeshUtils.GetMesh(this.tapObjects[i]);
			HudMeshUtils.UpdateVertPositions(mesh, r, 5f, false);
		}
	}

	// Token: 0x06001B09 RID: 6921 RVA: 0x000C60B0 File Offset: 0x000C44B0
	private void FixedUpdate()
	{
		int num = Input.touchCount;
		if (Application.isEditor)
		{
			num = ((!Input.GetMouseButton(0)) ? 0 : 1);
		}
		if (num == 0 && this.allOff)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (i >= num)
			{
				this.renderers[i].enabled = false;
			}
			else
			{
				if (Application.isEditor)
				{
					this.tapObjects[i].transform.position = Blocksworld.guiCamera.ScreenToWorldPoint(Input.mousePosition);
				}
				else
				{
					this.tapObjects[i].transform.position = Blocksworld.guiCamera.ScreenToWorldPoint(Input.GetTouch(i).position);
				}
				this.renderers[i].enabled = true;
			}
		}
		this.allOff = (num == 0);
	}

	// Token: 0x04001709 RID: 5897
	private GameObject[] tapObjects;

	// Token: 0x0400170A RID: 5898
	private Renderer[] renderers;

	// Token: 0x0400170B RID: 5899
	private bool allOff = true;

	// Token: 0x0400170C RID: 5900
	private const int maxFingers = 5;
}
