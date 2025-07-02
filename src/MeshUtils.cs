using System.IO;
using System.Text;
using Blocks;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshUtils
{
	public static MeshRenderer AddBWDefaultMeshRenderer(GameObject go)
	{
		MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
		DisableUnusedProperties(meshRenderer);
		return meshRenderer;
	}

	public static void DisableUnusedProperties(MeshRenderer mr)
	{
		mr.receiveShadows = false;
		mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
		mr.shadowCastingMode = ShadowCastingMode.Off;
		mr.useLightProbes = false;
	}

	public static void ExportGameObject(string path, GameObject go)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		int firstIndex = 1;
		stringBuilder.Append("o blocksworld_mesh_" + Mathf.Round(Random.value * 1000f) + "\n");
		go.transform.position = Vector3.zero;
		MeshRenderer component = go.transform.GetComponent<MeshRenderer>();
		if (component != null && component.enabled)
		{
			MeshFilter component2 = go.GetComponent<MeshFilter>();
			SkinnedMeshRenderer component3 = go.GetComponent<SkinnedMeshRenderer>();
			Mesh mesh = null;
			if (component2 != null || component3 != null)
			{
				if (component3 != null)
				{
					component3.sharedMesh = Object.Instantiate(component3.sharedMesh);
					mesh = component3.sharedMesh;
				}
				else if (component != null)
				{
					mesh = component2.mesh;
				}
			}
			if (mesh != null)
			{
				firstIndex = AppendGOMeshOBJ(stringBuilder, mesh, go.transform, firstIndex);
			}
		}
		MeshRenderer[] componentsInChildren = go.transform.GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer meshRenderer in array)
		{
			MeshFilter component4 = meshRenderer.GetComponent<MeshFilter>();
			SkinnedMeshRenderer component5 = meshRenderer.GetComponent<SkinnedMeshRenderer>();
			if (component4 != null)
			{
				if (component4.sharedMesh != null)
				{
					firstIndex = AppendGOMeshOBJ(stringBuilder, component4.sharedMesh, meshRenderer.transform, firstIndex);
				}
			}
			else if (component5 != null && component5.sharedMesh != null)
			{
				firstIndex = AppendGOMeshOBJ(stringBuilder, component5.sharedMesh, meshRenderer.transform, firstIndex);
			}
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	public static void Export(string path, Bunch bunch)
	{
		bool flag = path.EndsWith(".obj");
		bool flag2 = path.EndsWith(".stl");
		StringBuilder stringBuilder = new StringBuilder(32768);
		if (bunch.blocks.Count > 0)
		{
			int firstIndex = 1;
			foreach (Block block in bunch.blocks)
			{
				if (flag)
				{
					stringBuilder.Append("o blocksworld_mesh_" + Mathf.Round(Random.value * 1000f) + "\n");
				}
				MeshRenderer component = block.go.transform.GetComponent<MeshRenderer>();
				if (component != null && component.enabled)
				{
					if (flag)
					{
						firstIndex = AppendGOMeshOBJ(stringBuilder, block.mesh, block.go.transform, firstIndex);
					}
					if (flag2)
					{
						AppendGOMeshSTL(stringBuilder, block.mesh, block.go.transform);
					}
				}
				MeshRenderer[] componentsInChildren = block.go.transform.GetComponentsInChildren<MeshRenderer>();
				MeshRenderer[] array = componentsInChildren;
				foreach (MeshRenderer meshRenderer in array)
				{
					MeshFilter component2 = meshRenderer.GetComponent<MeshFilter>();
					SkinnedMeshRenderer component3 = meshRenderer.GetComponent<SkinnedMeshRenderer>();
					if (component2 != null)
					{
						if (flag)
						{
							firstIndex = AppendGOMeshOBJ(stringBuilder, component2.sharedMesh, meshRenderer.transform, firstIndex);
						}
						if (flag2)
						{
							AppendGOMeshSTL(stringBuilder, component2.sharedMesh, meshRenderer.transform);
						}
					}
					else if (component3 != null)
					{
						if (flag)
						{
							firstIndex = AppendGOMeshOBJ(stringBuilder, component3.sharedMesh, meshRenderer.transform, firstIndex);
						}
						if (flag2)
						{
							AppendGOMeshSTL(stringBuilder, component3.sharedMesh, meshRenderer.transform);
						}
					}
				}
			}
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	private static int AppendGOMeshOBJ(StringBuilder sb, Mesh mesh, Transform bgoT, int firstIndex)
	{
		string name = bgoT.name;
		name = name.Replace(" ", "_");
		name = name + "_" + Mathf.Round(Random.value * 1000f);
		sb.Append("g " + name + "\n");
		int num = mesh.vertices.Length;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = bgoT.TransformPoint(mesh.vertices[i]);
			sb.Append("v " + vector[0] + " " + vector[1] + " " + vector[2] + "\n");
		}
		for (int j = 0; j < mesh.uv.Length; j++)
		{
			Vector3 vector2 = mesh.uv[j];
			sb.Append("vt " + vector2.x + " " + vector2.y + "\n");
		}
		for (int k = 0; k < mesh.normals.Length; k++)
		{
			Vector3 vector3 = bgoT.TransformDirection(mesh.normals[k]);
			sb.Append("vn " + vector3[0] + " " + vector3[1] + " " + vector3[2] + "\n");
		}
		for (int l = 0; l < mesh.triangles.Length; l += 3)
		{
			int num2 = mesh.triangles[l] + firstIndex;
			int num3 = mesh.triangles[l + 1] + firstIndex;
			int num4 = mesh.triangles[l + 2] + firstIndex;
			sb.Append(string.Concat("f ", num2, "/" + num2 + "/", num2, " ", num3, "/" + num3 + "/", num3, " ", num4, "/" + num4 + "/", num4, "\n"));
		}
		return num + firstIndex;
	}

	private static void AppendGOMeshSTL(StringBuilder sb, Mesh mesh, Transform bgoT)
	{
		string name = bgoT.name;
		name = name.Replace(" ", "_");
		name = name + "_" + Mathf.Round(Random.value * 1000f);
		sb.Append("solid " + name + "\n");
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			int num = mesh.triangles[i];
			int num2 = mesh.triangles[i + 1];
			int num3 = mesh.triangles[i + 2];
			Vector3 vector = bgoT.TransformPoint(mesh.vertices[num]);
			Vector3 vector2 = bgoT.TransformPoint(mesh.vertices[num2]);
			Vector3 vector3 = bgoT.TransformPoint(mesh.vertices[num3]);
			Vector3 vector4 = bgoT.TransformDirection((mesh.normals[num] + mesh.normals[num2] + mesh.normals[num3]) / 3f);
			sb.Append(" facet normal " + vector4[0] + " " + vector4[1] + " " + vector4[2] + "\n");
			sb.Append("  outer loop\n");
			sb.Append("    vertex " + vector[0] + " " + vector[1] + " " + vector[2] + "\n");
			sb.Append("    vertex " + vector2[0] + " " + vector2[1] + " " + vector2[2] + "\n");
			sb.Append("    vertex " + vector3[0] + " " + vector3[1] + " " + vector3[2] + "\n");
			sb.Append("  endloop\n");
			sb.Append(" endfacet\n");
		}
		sb.Append("endsolid " + name + "\n");
	}
}
