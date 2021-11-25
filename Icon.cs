using System;
using UnityEngine;

// Token: 0x020001A0 RID: 416
public class Icon
{
	// Token: 0x06001759 RID: 5977 RVA: 0x000A4BE0 File Offset: 0x000A2FE0
	public Icon(string textureName)
	{
		this.go = (UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Icon")) as GameObject);
		string path = "GUI/TabBar/" + textureName + ((!Blocksworld.hd) ? " SD" : " HD");
		Texture2D texture2D = Resources.Load(path) as Texture2D;
		if (texture2D == null)
		{
			return;
		}
		this.go.GetComponent<MeshFilter>().mesh = Icon.GenerateIconMesh((float)texture2D.width / Blocksworld.screenScale, (float)texture2D.height / Blocksworld.screenScale);
		Material material = new Material(Resources.Load("GUI/Materials/TabBar/Material Icon") as Material);
		material.mainTexture = texture2D;
		this.go.GetComponent<MeshRenderer>().material = material;
	}

	// Token: 0x0600175A RID: 5978 RVA: 0x000A4CA8 File Offset: 0x000A30A8
	public void SetAlpha(float alpha)
	{
		Material material = this.go.GetComponent<MeshRenderer>().material;
		material.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
	}

	// Token: 0x0600175B RID: 5979 RVA: 0x000A4CE8 File Offset: 0x000A30E8
	private static Mesh GenerateIconMesh(float width, float height)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		float num = 0.5f * width;
		float num2 = 0.5f * height;
		array[0].x = num;
		array[0].y = -num2;
		array[0].z = 0f;
		array[1].x = -num;
		array[1].y = num2;
		array[1].z = 0f;
		array[2].x = num;
		array[2].y = num2;
		array[2].z = 0f;
		array[3].x = -num;
		array[3].y = -num2;
		array[3].z = 0f;
		array2[0].x = 1f;
		array2[0].y = 0f;
		array2[1].x = 0f;
		array2[1].y = 1f;
		array2[2].x = 1f;
		array2[2].y = 1f;
		array2[3].x = 0f;
		array2[3].y = 0f;
		int[] triangles = new int[]
		{
			0,
			1,
			2,
			0,
			3,
			1
		};
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateBounds();
		return mesh;
	}

	// Token: 0x040011E9 RID: 4585
	public GameObject go;
}
