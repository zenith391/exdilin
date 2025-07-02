using UnityEngine;

public class VisualizeTapsMB : MonoBehaviour
{
	private GameObject[] tapObjects;

	private Renderer[] renderers;

	private bool allOff = true;

	private const int maxFingers = 5;

	public void Setup(GameObject masterObject, Texture2D tapTexture)
	{
		tapObjects = new GameObject[5];
		renderers = new Renderer[5];
		Rect r = new Rect((0f - (float)tapTexture.width) / 2f, tapTexture.height / 2, tapTexture.width, 0f - (float)tapTexture.height);
		r = HudMeshUtils.NormalizedRect(r);
		Color color = new Color(1f, 1f, 1f, 0.5f);
		Shader shader = Resources.Load("Shaders/UnlitAlpha") as Shader;
		for (int i = 0; i < 5; i++)
		{
			tapObjects[i] = HudMeshUtils.CreateMeshObject("Tap " + i, tapTexture);
			tapObjects[i].transform.parent = masterObject.transform;
			renderers[i] = tapObjects[i].GetComponent<Renderer>();
			Material material = new Material(shader);
			material.SetTexture("_MainTex", tapTexture);
			material.SetColor("_Tint", color);
			renderers[i].sharedMaterial = material;
			renderers[i].enabled = false;
			Mesh mesh = HudMeshUtils.GetMesh(tapObjects[i]);
			HudMeshUtils.UpdateVertPositions(mesh, r, 5f, invertY: false);
		}
	}

	private void FixedUpdate()
	{
		int num = Input.touchCount;
		if (Application.isEditor)
		{
			num = (Input.GetMouseButton(0) ? 1 : 0);
		}
		if (num == 0 && allOff)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (i >= num)
			{
				renderers[i].enabled = false;
				continue;
			}
			if (Application.isEditor)
			{
				tapObjects[i].transform.position = Blocksworld.guiCamera.ScreenToWorldPoint(Input.mousePosition);
			}
			else
			{
				tapObjects[i].transform.position = Blocksworld.guiCamera.ScreenToWorldPoint(Input.GetTouch(i).position);
			}
			renderers[i].enabled = true;
		}
		allOff = num == 0;
	}
}
