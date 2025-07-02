using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class LaserBeam
{
	private BlockAbstractLaser _sender;

	private GameObject _go;

	private Mesh _mesh;

	private const int kMaxBounces = 10;

	private const float kNoHitLength = 1000f;

	public float sizeMult = 1f;

	private List<Vector3> lastVertices = new List<Vector3>();

	private List<Vector2> lastUvs = new List<Vector2>();

	private List<int> lastTriangles = new List<int>();

	private Vector3 lastOrigin;

	private Vector3[] lastVerticesArr;

	private Vector2[] lastUvsArr;

	private int[] lastTrianglesArr;

	private MeshRenderer renderer;

	public LaserBeam(BlockAbstractLaser sender)
	{
		_sender = sender;
		_go = new GameObject(sender.go.name + " beam");
		_mesh = new Mesh();
		_mesh.name = "Beam Mesh";
		MeshFilter meshFilter = _go.AddComponent<MeshFilter>();
		meshFilter.mesh = _mesh;
		renderer = _go.AddComponent<MeshRenderer>();
		renderer.material = (Material)Resources.Load("Materials/Laser2");
		Color laserColor = sender.GetLaserColor();
		renderer.material.SetColor("_Color", laserColor);
	}

	public void Destroy()
	{
		Object.Destroy(_go);
		Object.Destroy(_mesh);
	}

	public void Update(bool paused = false)
	{
		Color laserColor = _sender.GetLaserColor();
		renderer.material.SetColor("_Color", laserColor);
		FixedUpdate(fromUpdate: true, paused);
		if (lastVertices.Count <= 0)
		{
			return;
		}
		_mesh.Clear();
		Vector3 vector = lastOrigin - _sender.goT.position;
		for (int i = 0; i < lastVertices.Count; i++)
		{
			lastVertices[i] += vector;
		}
		if (lastVerticesArr == null || lastVerticesArr.Length != lastVertices.Count)
		{
			lastVerticesArr = lastVertices.ToArray();
		}
		else
		{
			int count = lastVertices.Count;
			for (int j = 0; j < count; j++)
			{
				lastVerticesArr[j] = lastVertices[j];
			}
		}
		_mesh.vertices = lastVerticesArr;
		if (lastUvsArr == null || lastUvsArr.Length != lastUvs.Count)
		{
			lastUvsArr = lastUvs.ToArray();
		}
		else
		{
			int count2 = lastUvs.Count;
			for (int k = 0; k < count2; k++)
			{
				lastUvsArr[k] = lastUvs[k];
			}
		}
		_mesh.uv = lastUvsArr;
		if (lastTrianglesArr == null || lastTrianglesArr.Length != lastTriangles.Count)
		{
			lastTrianglesArr = lastTriangles.ToArray();
		}
		else
		{
			int count3 = lastTriangles.Count;
			for (int l = 0; l < count3; l++)
			{
				lastTrianglesArr[l] = lastTriangles[l];
			}
		}
		_mesh.triangles = lastTrianglesArr;
	}

	public void FixedUpdate(bool fromUpdate = false, bool paused = false)
	{
		Transform goT = _sender.goT;
		lastVertices.Clear();
		lastUvs.Clear();
		lastTriangles.Clear();
		int num = 0;
		Vector3 laserExitOffset = _sender.laserExitOffset;
		Vector3 right = goT.right;
		Vector3 up = goT.up;
		Vector3 forward = goT.forward;
		Vector3 vector = laserExitOffset.x * right + laserExitOffset.y * up + laserExitOffset.z * forward;
		Vector3 vector2 = goT.position + vector;
		Vector3 vector3 = _sender.GetFireDirectionForward();
		Vector3 vector4 = _sender.GetFireDirectionUp();
		bool flag;
		do
		{
			Vector3 to = vector2 + vector3 * 1000f;
			flag = false;
			RaycastHit[] array = Physics.RaycastAll(vector2, vector3);
			RaycastHit raycastHit = default(RaycastHit);
			RaycastHit raycastHit2 = default(RaycastHit);
			bool flag2 = false;
			for (int i = 0; i < array.Length; i++)
			{
				float num2 = float.MaxValue;
				int num3 = i;
				for (int j = i; j < array.Length; j++)
				{
					RaycastHit raycastHit3 = array[j];
					float sqrMagnitude = (vector2 - raycastHit3.point).sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						num2 = sqrMagnitude;
						num3 = j;
					}
				}
				if (!flag2)
				{
					RaycastHit raycastHit4 = array[num3];
					Block block = BWSceneManager.FindBlock(raycastHit4.collider.gameObject);
					if (block != null && !(block is BlockSky))
					{
						if (!fromUpdate)
						{
							BlockAbstractLaser.AddHit(_sender, block);
						}
						string texture = block.GetTexture();
						if (!BlockAbstractLaser.IsTransparent(texture))
						{
							to = raycastHit4.point;
							flag2 = true;
							raycastHit2 = raycastHit4;
							if (BlockAbstractLaser.IsReflective(block.GetPaint()))
							{
								num++;
								flag = true;
								raycastHit = raycastHit4;
							}
						}
					}
				}
				if (flag2)
				{
					break;
				}
				RaycastHit raycastHit5 = array[num3];
				array[num3] = array[i];
				array[i] = raycastHit5;
			}
			if (fromUpdate)
			{
				float num4 = _sender.beamStrength * _sender.beamSizeMultiplier;
				BlockAbstractLaser.DrawLaserLine(vector2, to, vector4, lastVertices, lastUvs, lastTriangles, sizeMult * num4);
			}
			if (flag2)
			{
				_sender.UpdateLaserHitParticles(raycastHit2.point, raycastHit2.normal, vector3, emitSparks: false, !flag);
			}
			if (flag)
			{
				vector2 = raycastHit.point;
				vector3 = Vector3.Reflect(vector3, raycastHit.normal);
				vector2 -= vector3 * 0.05f;
				vector4 = Vector3.Reflect(vector4, raycastHit.normal);
			}
		}
		while (flag && num < 10);
		lastOrigin = goT.position;
		if (!paused)
		{
			sizeMult = 1f + 0.15f * Mathf.Sin(Time.time * 20f);
		}
	}
}
