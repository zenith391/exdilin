using UnityEngine;

public class Icon
{
	public GameObject go;

	public Icon(string textureName)
	{
		go = Object.Instantiate(Resources.Load("GUI/Prefab Icon")) as GameObject;
		Texture2D texture2D = Resources.Load("GUI/TabBar/" + textureName + ((!Blocksworld.hd) ? " SD" : " HD")) as Texture2D;
		if (!(texture2D == null))
		{
			go.GetComponent<MeshFilter>().mesh = GenerateIconMesh((float)texture2D.width / Blocksworld.screenScale, (float)texture2D.height / Blocksworld.screenScale);
			Material material = new Material(Resources.Load("GUI/Materials/TabBar/Material Icon") as Material)
			{
				mainTexture = texture2D
			};
			go.GetComponent<MeshRenderer>().material = material;
		}
	}

	public void SetAlpha(float alpha)
	{
		Material material = go.GetComponent<MeshRenderer>().material;
		material.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
	}

	private static Mesh GenerateIconMesh(float width, float height)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		float num = 0.5f * width;
		float num2 = 0.5f * height;
		array[0].x = num;
		array[0].y = 0f - num2;
		array[0].z = 0f;
		array[1].x = 0f - num;
		array[1].y = num2;
		array[1].z = 0f;
		array[2].x = num;
		array[2].y = num2;
		array[2].z = 0f;
		array[3].x = 0f - num;
		array[3].y = 0f - num2;
		array[3].z = 0f;
		array2[0].x = 1f;
		array2[0].y = 0f;
		array2[1].x = 0f;
		array2[1].y = 1f;
		array2[2].x = 1f;
		array2[2].y = 1f;
		array2[3].x = 0f;
		array2[3].y = 0f;
		int[] triangles = new int[6] { 0, 1, 2, 0, 3, 1 };
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateBounds();
		return mesh;
	}
}
