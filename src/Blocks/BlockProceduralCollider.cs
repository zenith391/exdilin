using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockProceduralCollider : Block
{
	private List<GameObject> goColliders = new List<GameObject>();

	private List<Mesh> goMeshes = new List<Mesh>();

	private static Dictionary<string, List<GameObject>> reusableMeshes = new Dictionary<string, List<GameObject>>();

	private GameObject goColliderParent;

	protected int maxSplits = 10;

	protected float scaleFactorZ = 0.25f;

	protected float scaleFactorY = 0.25f;

	public BlockProceduralCollider(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public static void ClearReuseableMeshes()
	{
		reusableMeshes.Clear();
	}

	private List<Vector2> GetInitCoordList()
	{
		List<Vector2> list = new List<Vector2>();
		int num = 4;
		for (int i = 0; i < num; i++)
		{
			float x = (float)i / ((float)num - 1f);
			float y = Evaluate(x);
			list.Add(new Vector2(x, y));
		}
		return list;
	}

	private float GetAbsError(Vector2 left, Vector2 right)
	{
		float x = 0.5f * (right.x + left.x);
		float num = Evaluate(x);
		float num2 = 0.5f * (right.y + left.y);
		return Mathf.Abs(num2 - num);
	}

	private bool Split(List<Vector2> list, float errorLimit)
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < list.Count - 1; i++)
		{
			Vector2 left = list[i];
			Vector2 right = list[i + 1];
			float num3 = Mathf.Abs(left.x - right.x);
			if (num3 > 0.05f)
			{
				float absError = GetAbsError(left, right);
				if (absError > num)
				{
					num2 = i;
					num = absError;
				}
			}
		}
		if (num > errorLimit)
		{
			float x = (list[num2].x + list[num2 + 1].x) * 0.5f;
			Vector2 item = new Vector2(x, Evaluate(x));
			list.Insert(num2 + 1, item);
			return true;
		}
		return false;
	}

	public override void Play()
	{
		base.Play();
		go.GetComponent<Collider>().enabled = false;
		Vector3 vector = Scale();
		string key = BlockType() + vector.ToString();
		List<GameObject> list = null;
		if (reusableMeshes.ContainsKey(key))
		{
			list = reusableMeshes[key];
		}
		goColliderParent = new GameObject(go.name + " Concave collider parent");
		goColliderParent.transform.position = goT.position;
		goColliderParent.transform.rotation = goT.rotation;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				GameObject gameObject = Object.Instantiate(list[i]);
				gameObject.transform.parent = goColliderParent.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				MeshCollider component = gameObject.GetComponent<MeshCollider>();
				goColliders.Add(gameObject);
				goMeshes.Add(component.sharedMesh);
				BWSceneManager.AddBlockMap(gameObject, this);
			}
		}
		else
		{
			float x = vector.x;
			float y = vector.y;
			float z = vector.z;
			int num = Mathf.RoundToInt(Mathf.Min(Mathf.Max(scaleFactorZ * vector.z, scaleFactorY * vector.y), maxSplits));
			List<Vector2> initCoordList = GetInitCoordList();
			for (int j = 0; j < num; j++)
			{
				if (!Split(initCoordList, 0.05f))
				{
					break;
				}
			}
			for (int k = 0; k < initCoordList.Count - 1; k++)
			{
				List<Vector3> list2 = new List<Vector3>();
				List<int> list3 = new List<int>();
				Mesh mesh = new Mesh();
				mesh.Clear();
				GameObject gameObject2 = new GameObject(go.name + " Concave Collider Part");
				gameObject2.transform.parent = goColliderParent.transform;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				MeshCollider meshCollider = gameObject2.AddComponent<MeshCollider>();
				Vector2 vector2 = initCoordList[k];
				Vector2 vector3 = initCoordList[k + 1];
				float x2 = vector2.x;
				float y2 = vector2.y;
				float x3 = vector3.x;
				float y3 = vector3.y;
				x2 -= 0.5f;
				x3 -= 0.5f;
				y2 -= 0.5f;
				y3 -= 0.5f;
				int count = list2.Count;
				list2.AddRange(new Vector3[8]
				{
					new Vector3(-0.5f, y2, 0f - x2),
					new Vector3(0.5f, y2, 0f - x2),
					new Vector3(0.5f, y3, 0f - x3),
					new Vector3(-0.5f, y3, 0f - x3),
					new Vector3(-0.5f, -0.5f, 0f - x2),
					new Vector3(0.5f, -0.5f, 0f - x2),
					new Vector3(0.5f, -0.5f, 0f - x3),
					new Vector3(-0.5f, -0.5f, 0f - x3)
				});
				for (int l = 0; l < list2.Count; l++)
				{
					Vector3 value = list2[l];
					value.Scale(new Vector3(x, y, z));
					list2[l] = value;
				}
				AddQuad(list3, count, count + 1, count + 2, count + 3);
				AddQuad(list3, count, count + 3, count + 7, count + 4);
				AddQuad(list3, count + 2, count + 1, count + 3, count + 6);
				list3.Reverse();
				mesh.vertices = list2.ToArray();
				mesh.triangles = list3.ToArray();
				meshCollider.convex = true;
				meshCollider.sharedMesh = mesh;
				goColliders.Add(gameObject2);
				goMeshes.Add(mesh);
				BWSceneManager.AddBlockMap(gameObject2, this);
			}
			reusableMeshes[key] = goColliders;
		}
		goColliderParent.transform.parent = goT;
	}

	private void AddQuad(List<int> triangles, int i1, int i2, int i3, int i4)
	{
		triangles.AddRange(new int[3] { i1, i2, i3 });
		triangles.AddRange(new int[3] { i1, i3, i4 });
	}

	protected virtual float Evaluate(float x)
	{
		return 1f;
	}

	public override void Stop(bool resetBlock = true)
	{
		DestroyColliders();
		base.Stop(resetBlock);
	}

	public override void Destroy()
	{
		DestroyColliders();
		base.Destroy();
	}

	private void DestroyColliders()
	{
		reusableMeshes.Clear();
		for (int i = 0; i < goColliders.Count; i++)
		{
			GameObject obj = goColliders[i];
			Mesh mesh = goMeshes[i];
			if (mesh != null)
			{
				Object.Destroy(mesh);
			}
			BWSceneManager.RemoveBlockMap(obj);
			Object.Destroy(obj);
		}
		Object.Destroy(goColliderParent);
		goMeshes.Clear();
		goColliders.Clear();
		if (go != null && go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().enabled = true;
		}
	}

	public void SetLayer(Layer layerEnum)
	{
		for (int i = 0; i < goColliders.Count; i++)
		{
			GameObject gameObject = goColliders[i];
			gameObject.layer = (int)layerEnum;
		}
		if (goColliderParent != null)
		{
			goColliderParent.layer = (int)layerEnum;
		}
		go.layer = (int)layerEnum;
	}
}
