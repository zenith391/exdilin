using System;
using System.IO;
using System.Text;
using Blocks;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020001BB RID: 443
public static class MeshUtils
{
	// Token: 0x060017E8 RID: 6120 RVA: 0x000A8D80 File Offset: 0x000A7180
	public static MeshRenderer AddBWDefaultMeshRenderer(GameObject go)
	{
		MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
		MeshUtils.DisableUnusedProperties(meshRenderer);
		return meshRenderer;
	}

	// Token: 0x060017E9 RID: 6121 RVA: 0x000A8D9B File Offset: 0x000A719B
	public static void DisableUnusedProperties(MeshRenderer mr)
	{
		mr.receiveShadows = false;
		mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
		mr.shadowCastingMode = ShadowCastingMode.Off;
		mr.useLightProbes = false;
	}

	// Added by Exdilin
	public static void ExportGameObject(string path, GameObject go) {
		StringBuilder stringBuilder = new StringBuilder(32768);
		int firstIndex = 1;

		stringBuilder.Append("o blocksworld_mesh_" + Mathf.Round(UnityEngine.Random.value * 1000f) + "\n");
		go.transform.position = Vector3.zero;
		MeshRenderer component = go.transform.GetComponent<MeshRenderer>();
		if (component != null && component.enabled) {
			MeshFilter cp1 = go.GetComponent<MeshFilter>();
			SkinnedMeshRenderer cp2 = go.GetComponent<SkinnedMeshRenderer>();
			Mesh mesh = null;
			if (cp1 != null || cp2 != null) {
				if (cp2 != null) {
					cp2.sharedMesh = UnityEngine.Object.Instantiate<Mesh>(cp2.sharedMesh);
					mesh = cp2.sharedMesh;
				} else if (component != null) {
					mesh = cp1.mesh;
				}
			}
			if (mesh != null) {
				firstIndex = MeshUtils.AppendGOMeshOBJ(stringBuilder, mesh, go.transform, firstIndex);
			}
		}
		MeshRenderer[] componentsInChildren = go.transform.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren) {
			MeshFilter component2 = meshRenderer.GetComponent<MeshFilter>();
			SkinnedMeshRenderer component3 = meshRenderer.GetComponent<SkinnedMeshRenderer>();
			if (component2 != null) {
				if (component2.sharedMesh != null) {
					firstIndex = MeshUtils.AppendGOMeshOBJ(stringBuilder, component2.sharedMesh, meshRenderer.transform, firstIndex);
				}
			} else if (component3 != null) {
				if (component3.sharedMesh != null) {
					firstIndex = MeshUtils.AppendGOMeshOBJ(stringBuilder, component3.sharedMesh, meshRenderer.transform, firstIndex);
				}
			}
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	// Token: 0x060017EA RID: 6122 RVA: 0x000A8DBC File Offset: 0x000A71BC
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
				if (flag) {
					stringBuilder.Append("o blocksworld_mesh_" + Mathf.Round(UnityEngine.Random.value * 1000f) + "\n");
				}
				MeshRenderer component = block.go.transform.GetComponent<MeshRenderer>();
				if (component != null && component.enabled) {
					if (flag) {
						firstIndex = MeshUtils.AppendGOMeshOBJ(stringBuilder, block.mesh, block.go.transform, firstIndex);
					}
					if (flag2) {
						MeshUtils.AppendGOMeshSTL(stringBuilder, block.mesh, block.go.transform);
					}
				}
				MeshRenderer[] componentsInChildren = block.go.transform.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer meshRenderer in componentsInChildren) {
					MeshFilter component2 = meshRenderer.GetComponent<MeshFilter>();
					SkinnedMeshRenderer component3 = meshRenderer.GetComponent<SkinnedMeshRenderer>();
					if (component2 != null) {
						if (flag) {
							firstIndex = MeshUtils.AppendGOMeshOBJ(stringBuilder, component2.sharedMesh, meshRenderer.transform, firstIndex);
						}
						if (flag2) {
							MeshUtils.AppendGOMeshSTL(stringBuilder, component2.sharedMesh, meshRenderer.transform);
						}
					} else if (component3 != null) {
						if (flag) {
							firstIndex = MeshUtils.AppendGOMeshOBJ(stringBuilder, component3.sharedMesh, meshRenderer.transform, firstIndex);
						}
						if (flag2) {
							MeshUtils.AppendGOMeshSTL(stringBuilder, component3.sharedMesh, meshRenderer.transform);
						}
					}
				}
			}
		}
		File.WriteAllText(path, stringBuilder.ToString());
	}

	// Token: 0x060017EB RID: 6123 RVA: 0x000A8FD4 File Offset: 0x000A73D4
	private static int AppendGOMeshOBJ(StringBuilder sb, Mesh mesh, Transform bgoT, int firstIndex)
	{
		string text = bgoT.name;
		text = text.Replace(" ", "_");
		text = text + "_" + Mathf.Round(UnityEngine.Random.value * 1000f);
		sb.Append("g " + text + "\n");
		int num = mesh.vertices.Length;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = bgoT.TransformPoint(mesh.vertices[i]);
			sb.Append(string.Concat(new object[]
			{
				"v ",
				vector[0],
				" ",
				vector[1],
				" ",
				vector[2],
				"\n"
			}));
		}
		// UV mapping added by Exdilin
		for (int i = 0; i < mesh.uv.Length; i++) {
			Vector3 vector = mesh.uv[i];
			sb.Append("vt " + vector.x + " " + vector.y + "\n");
		}
		for (int j = 0; j < mesh.normals.Length; j++)
		{
			Vector3 vector2 = bgoT.TransformDirection(mesh.normals[j]);
			sb.Append(string.Concat(new object[]
			{
				"vn ",
				vector2[0],
				" ",
				vector2[1],
				" ",
				vector2[2],
				"\n"
			}));
		}
		for (int k = 0; k < mesh.triangles.Length; k += 3)
		{
			int num2 = mesh.triangles[k] + firstIndex;
			int num3 = mesh.triangles[k + 1] + firstIndex;
			int num4 = mesh.triangles[k + 2] + firstIndex;
			sb.Append(string.Concat(new object[]
			{
				"f ",
				num2,
				"/" + num2 + "/",
				num2,
				" ",
				num3,
				"/" + num3 + "/",
				num3,
				" ",
				num4,
				"/" + num4 + "/",
				num4,
				"\n"
			}));
		}
		return num + firstIndex;
	}

	// Token: 0x060017EC RID: 6124 RVA: 0x000A9238 File Offset: 0x000A7638
	private static void AppendGOMeshSTL(StringBuilder sb, Mesh mesh, Transform bgoT)
	{
		string text = bgoT.name;
		text = text.Replace(" ", "_");
		text = text + "_" + Mathf.Round(UnityEngine.Random.value * 1000f);
		sb.Append("solid " + text + "\n");
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			int num = mesh.triangles[i];
			int num2 = mesh.triangles[i + 1];
			int num3 = mesh.triangles[i + 2];
			Vector3 vector = bgoT.TransformPoint(mesh.vertices[num]);
			Vector3 vector2 = bgoT.TransformPoint(mesh.vertices[num2]);
			Vector3 vector3 = bgoT.TransformPoint(mesh.vertices[num3]);
			Vector3 vector4 = bgoT.TransformDirection((mesh.normals[num] + mesh.normals[num2] + mesh.normals[num3]) / 3f);
			sb.Append(string.Concat(new object[]
			{
				" facet normal ",
				vector4[0],
				" ",
				vector4[1],
				" ",
				vector4[2],
				"\n"
			}));
			sb.Append("  outer loop\n");
			sb.Append(string.Concat(new object[]
			{
				"    vertex ",
				vector[0],
				" ",
				vector[1],
				" ",
				vector[2],
				"\n"
			}));
			sb.Append(string.Concat(new object[]
			{
				"    vertex ",
				vector2[0],
				" ",
				vector2[1],
				" ",
				vector2[2],
				"\n"
			}));
			sb.Append(string.Concat(new object[]
			{
				"    vertex ",
				vector3[0],
				" ",
				vector3[1],
				" ",
				vector3[2],
				"\n"
			}));
			sb.Append("  endloop\n");
			sb.Append(" endfacet\n");
		}
		sb.Append("endsolid " + text + "\n");
	}
}
