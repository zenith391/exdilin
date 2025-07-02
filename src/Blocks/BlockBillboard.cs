using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockBillboard : Block
{
	private Vector3 offsetCamera;

	private GameObject mirrorGo;

	private Mesh mirrorMesh;

	private GameObject lensFlareGo;

	private LensFlare lensFlare;

	private Renderer lensFlareRenderer;

	private Material mirrorMaterial;

	private BillboardParameters parameters;

	private static GameObject lensFlarePrefab;

	private float scaleY;

	public BlockBillboard(List<List<Tile>> tiles)
		: base(tiles)
	{
		offsetCamera = new Vector3(0.4f, 1f, -4f).normalized * 300f;
		scaleY = 1f;
		parameters = GetBillboardParameters();
		if (parameters.showLensflare)
		{
			if (lensFlarePrefab == null)
			{
				lensFlarePrefab = Resources.Load("Lens flares/Lens Flare") as GameObject;
			}
			lensFlareGo = Object.Instantiate(lensFlarePrefab);
			lensFlare = lensFlareGo.GetComponent<LensFlare>();
			lensFlare.enabled = true;
			lensFlare.color = parameters.lensFlareColor;
			lensFlare.brightness = parameters.lensFlareBrightness;
		}
		Material material = go.GetComponent<Renderer>().material;
		if (null != material)
		{
			if (parameters.ignoreFog)
			{
				material.SetFloat("_FogInfluence", 0f);
			}
			if (!parameters.mirrorInWater)
			{
				material.SetFloat("_WaterLevel", 0.5f);
			}
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		go.GetComponent<Collider>().enabled = false;
	}

	public override void Play()
	{
		base.Play();
		go.GetComponent<Collider>().enabled = false;
	}

	private void CreateMesh()
	{
		if (mirrorGo == null)
		{
			mirrorGo = new GameObject(go.name + " Mirror");
			MeshFilter meshFilter = mirrorGo.AddComponent<MeshFilter>();
			mirrorMesh = new Mesh();
			meshFilter.mesh = mirrorMesh;
			MeshRenderer meshRenderer = mirrorGo.AddComponent<MeshRenderer>();
			mirrorMaterial = go.GetComponent<MeshRenderer>().sharedMaterial;
			meshRenderer.sharedMaterial = mirrorMaterial;
		}
		mirrorMesh.Clear();
		Vector3 vector = Scale();
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		List<Vector3> list4 = new List<Vector3>();
		float num = 0.5f * vector.x;
		float num2 = 0.5f * vector.y;
		int num3 = 7;
		int num4 = 7;
		Vector3 up = Vector3.up;
		Vector3 right = Vector3.right;
		Vector3 forward = Vector3.forward;
		float num5 = vector.x / (float)num3;
		float num6 = vector.y / (float)num4;
		for (int i = 0; i <= num3; i++)
		{
			float num7 = (float)i * num5 - num;
			for (int j = 0; j <= num4; j++)
			{
				float num8 = (0f - num6) * (float)j + num2;
				Vector3 item = right * num7 + up * num8;
				Vector3 vector2 = new Vector2((float)i / (float)num3, (float)j / (float)num4);
				list.Add(item);
				list2.Add(vector2);
				list4.Add(forward);
			}
		}
		int num9 = num3 + 1;
		for (int k = 0; k < num3; k++)
		{
			for (int l = 0; l < num4; l++)
			{
				list3.AddRange(new int[6]
				{
					k + l * num9,
					k + 1 + l * num9,
					k + 1 + (l + 1) * num9,
					k + l * num9,
					k + 1 + (l + 1) * num9,
					k + (l + 1) * num9
				});
			}
		}
		mirrorMesh.vertices = list.ToArray();
		mirrorMesh.uv = list2.ToArray();
		mirrorMesh.triangles = list3.ToArray();
		mirrorMesh.normals = list4.ToArray();
	}

	public override void Destroy()
	{
		base.Destroy();
		if (mirrorMesh != null)
		{
			Object.Destroy(mirrorMesh);
		}
		if (mirrorGo != null)
		{
			Object.Destroy(mirrorGo);
		}
		if (lensFlareGo != null)
		{
			Object.Destroy(lensFlareGo);
		}
	}

	public override bool MoveTo(Vector3 pos)
	{
		bool result = base.MoveTo(pos);
		BillboardParameters billboardParameters = GetBillboardParameters();
		float num = 300f;
		if (billboardParameters != null)
		{
			num = billboardParameters.realDistance;
		}
		offsetCamera = pos.normalized * num;
		return result;
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
		CreateMesh();
		scaleY = scale.y;
		return result;
	}

	private void UpdateMirror(bool showMirror)
	{
		if (showMirror)
		{
			Transform transform = goT;
			Vector3 position = transform.position;
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			Bounds waterBounds = worldOceanBlock.GetWaterBounds();
			float num = waterBounds.max.y + worldOceanBlock.WaterLevelOffset(position);
			float num2 = position.y - num;
			Vector3 vector = position - Vector3.up * num2 * 2f;
			float magnitude = offsetCamera.magnitude;
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Vector3 normalized = (vector - cameraPosition).normalized;
			vector = cameraPosition + normalized * magnitude;
			if (vector.y > num && new Plane(Vector3.up, waterBounds.max).Raycast(new Ray(cameraPosition, normalized), out var enter))
			{
				vector = cameraPosition + normalized * enter;
			}
			Transform transform2 = mirrorGo.transform;
			transform2.position = vector;
			mirrorMaterial.SetFloat("_WaterLevel", num);
		}
	}

	public override void Update()
	{
		base.Update();
		Transform transform = goT;
		bool flag = BlockAbstractWater.CameraWithinAnyWater();
		bool enabled = !flag;
		bool flag2 = parameters.mirrorInWater && !flag && Blocksworld.worldOcean != null && Blocksworld.worldOceanBlock.isReflective;
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		transform.LookAt(cameraPosition, Vector3.up);
		if (flag2)
		{
			mirrorGo.transform.LookAt(cameraPosition, Vector3.up);
		}
		Vector3 position = cameraPosition + offsetCamera;
		if (parameters.snapHorizon)
		{
			float num = 0f;
			if (Blocksworld.worldOcean != null)
			{
				num = Blocksworld.worldOcean.transform.position.y + 0.5f;
			}
			position.y = num + scaleY * 0.5f;
		}
		if (parameters.mirrorInWater && position.y < scaleY * 0.5f)
		{
			position.y = scaleY * 0.5f;
		}
		transform.position = position;
		if (parameters.showLensflare)
		{
			Transform transform2 = lensFlareGo.transform;
			transform2.position = cameraPosition + offsetCamera * 0.9f;
			Color color = Blocksworld.dynamicLightColor * parameters.lensFlareColor;
			lensFlare.color = color;
			lensFlare.enabled = enabled;
		}
		UpdateMirror(flag2);
		mirrorGo.GetComponent<Renderer>().enabled = flag2;
	}

	public BillboardParameters GetBillboardParameters()
	{
		BillboardParameters component = go.GetComponent<BillboardParameters>();
		if (component == null)
		{
			BWLog.Info("Every billboard block needs a BillboardParameters script component");
		}
		return component;
	}
}
