using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class LaserPulse : AbstractProjectile
{
	protected List<Segment> _segments = new List<Segment>();

	private float segmentTime = 100f;

	private static GameObject segmentPrefab;

	private List<Segment> toRemove = new List<Segment>();

	public LaserPulse(BlockAbstractLaser sender)
		: base(sender)
	{
		Vector3 fireDirectionUp = sender.GetFireDirectionUp();
		Vector3 fireDirectionForward = sender.GetFireDirectionForward();
		_segments.Add(new Segment(sender, GetSenderPosition() + extraSpeed * Blocksworld.fixedDeltaTime, fireDirectionForward, fireDirectionUp, kSpeed, extraSpeed, CreateSegmentGo()));
		_segments[0].ReceivingEnergy = true;
		segmentTime = sender.segmentTime;
	}

	public override void Destroy()
	{
		foreach (Segment segment in _segments)
		{
			segment.Destroy();
		}
	}

	protected override GameObject GetSegmentPrefab()
	{
		if (segmentPrefab == null)
		{
			segmentPrefab = new GameObject("Pulse");
			Mesh mesh = new Mesh();
			mesh.Clear();
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<int> list3 = new List<int>();
			float sizeMult = 1f;
			BlockAbstractLaser.DrawLaserLine(-Vector3.forward * 0.5f, Vector3.forward * 0.5f, Vector3.up, list, list2, list3, sizeMult);
			mesh.vertices = list.ToArray();
			mesh.uv = list2.ToArray();
			mesh.triangles = list3.ToArray();
			mesh.name = "Pulse Mesh";
			MeshFilter meshFilter = segmentPrefab.AddComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			MeshRenderer meshRenderer = segmentPrefab.AddComponent<MeshRenderer>();
			meshRenderer.material = (Material)Resources.Load("Materials/Laser2");
			segmentPrefab.SetActive(value: false);
		}
		return segmentPrefab;
	}

	protected override GameObject CreateSegmentGo()
	{
		GameObject original = GetSegmentPrefab();
		GameObject gameObject = Object.Instantiate(original);
		gameObject.SetActive(value: true);
		Color laserColor = _sender.GetLaserColor();
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		component.material.SetColor("_Color", laserColor);
		return gameObject;
	}

	public override bool ShouldBeDestroyed()
	{
		return _segments.Count == 0;
	}

	public override void StopReceivingEnergy()
	{
		for (int i = 0; i < _segments.Count; i++)
		{
			Segment segment = _segments[i];
			segment.ReceivingEnergy = false;
			segment.StartFraction = 0f;
		}
	}

	protected override void StepSegments()
	{
		float num = 0f;
		toRemove.Clear();
		for (int i = 0; i < _segments.Count; i++)
		{
			Segment segment = _segments[i];
			segment.TravelledTime += Blocksworld.fixedDeltaTime;
			float num2 = Vector3.Dot(segment.ExtraVelocity, segment.Direction);
			if (gravityInfluence > 0f)
			{
				segment.ExtraVelocity += Physics.gravity * gravityInfluence * Blocksworld.fixedDeltaTime;
			}
			float num3 = segment.HeadSpeed + num2;
			float num4 = segment.TailSpeed + num2;
			segment.TravelledDist += num3 * Blocksworld.fixedDeltaTime;
			if (segment.ReceivingEnergy)
			{
				segment.Length += num3 * Blocksworld.fixedDeltaTime;
				segment.StartFraction = 1f - EnergyFraction;
			}
			segment.Length += num;
			float length = segment.Length;
			segment.Length += (num3 - num4) * Blocksworld.fixedDeltaTime;
			num = length - segment.Length;
			if (segment.Reflected && segment.ReceivingEnergy && num3 == 0f)
			{
				segment.Length = length;
			}
			segment.Origin += Util.ProjectOntoPlane(segment.ExtraVelocity, segment.Direction) * Blocksworld.fixedDeltaTime;
			Vector3 origin = segment.Origin + segment.Direction * (segment.TravelledDist - segment.Length);
			float num5 = segmentTime / kSpeed;
			RaycastHit hitInfo;
			if ((double)segment.Length < 0.0001 || segment.TravelledTime > num5)
			{
				toRemove.Add(segment);
			}
			else if (Physics.Raycast(origin, segment.Direction, out hitInfo, segment.Length))
			{
				Block block = BWSceneManager.FindBlock(hitInfo.collider.gameObject);
				if (block != null && (block != _sender || segment.IsReflection))
				{
					AddHit(block, segment.Direction, hitInfo.point, hitInfo.normal);
				}
			}
		}
		for (int j = 0; j < toRemove.Count; j++)
		{
			Segment segment2 = toRemove[j];
			segment2.Destroy();
			_segments.Remove(segment2);
		}
		if (_segments.Count == 0)
		{
			return;
		}
		Segment segment3 = _segments[_segments.Count - 1];
		Vector3 vector = segment3.Origin + segment3.Direction * segment3.TravelledDist;
		Vector3 vector2 = vector - segment3.Direction * segment3.Length;
		int num6 = 0;
		RaycastHit hitInfo2;
		while (Physics.Raycast(vector2, segment3.Direction, out hitInfo2, segment3.Length))
		{
			if (num6++ > 10)
			{
				Debug.LogWarning("Too many raycasts for pulsed laser projectile! Exiting loop");
				break;
			}
			Block block2 = BWSceneManager.FindBlock(hitInfo2.collider.gameObject);
			if (block2 == null || block2 is BlockSky)
			{
				break;
			}
			if (!travelThroughTransparent || !BlockAbstractLaser.IsTransparent(block2.GetTexture()))
			{
				segment3.HeadSpeed = 0f;
				segment3.ExtraVelocity = Vector3.zero;
				segment3.Reflected = false;
				if (canReflect && BlockAbstractLaser.IsReflective(block2.GetPaint()))
				{
					Vector3 point = hitInfo2.point;
					Vector3 vector3 = Vector3.Reflect(segment3.Direction, hitInfo2.normal);
					Vector3 up = Vector3.Reflect(segment3.Up, hitInfo2.normal);
					segment3.TravelledDist = (segment3.Origin - point).magnitude;
					segment3.Reflected = true;
					Vector3 vector4 = GetExtraSpeed(block2, vector3, point);
					Segment segment4 = new Segment(null, point + vector4 * Blocksworld.fixedDeltaTime, vector3, up, kSpeed, vector4, CreateSegmentGo());
					segment4.IsReflection = true;
					segment4.TravelledTime = segment3.TravelledTime;
					_segments.Add(segment4);
				}
				else
				{
					UpdateHitEffect(hitInfo2, segment3);
				}
				break;
			}
			float num7 = (hitInfo2.point - vector2).magnitude;
			if (num7 < 0.0001f)
			{
				num7 = 0.0001f;
				vector2 = vector - segment3.Direction * segment3.Length;
			}
			else
			{
				vector2 = hitInfo2.point;
			}
			segment3.Length -= num7;
			if (segment3.Length <= 0.0001f)
			{
				break;
			}
		}
	}

	public override void Update()
	{
		float projectileSizeMultiplier = _sender.projectileSizeMultiplier;
		for (int i = 0; i < _segments.Count; i++)
		{
			Segment segment = _segments[i];
			Vector3 direction = segment.Direction;
			Vector3 vector = segment.Origin + direction * (segment.TravelledDist - segment.Length);
			Vector3 vector2 = vector + direction * segment.Length;
			float magnitude = (vector2 - vector).magnitude;
			if (magnitude >= 0.001f)
			{
				Transform goT = segment.goT;
				goT.localScale = new Vector3(projectileSizeMultiplier, projectileSizeMultiplier, magnitude);
				goT.position = 0.5f * (vector + vector2);
			}
		}
	}
}
