using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class Projectile : AbstractProjectile
{
	protected bool addedHitEffect;

	protected Segment segment;

	private static GameObject segmentPrefab;

	private float segmentTime = 100f;

	public Projectile(BlockAbstractLaser sender)
		: base(sender)
	{
		travelThroughTransparent = false;
		canReflect = false;
		gravityInfluence = 0.25f;
		Vector3 fireDirectionUp = sender.GetFireDirectionUp();
		Vector3 fireDirectionForward = sender.GetFireDirectionForward();
		segment = new Segment(sender, GetSenderPosition() + extraSpeed * Blocksworld.fixedDeltaTime, fireDirectionUp, fireDirectionForward, kSpeed, extraSpeed, CreateSegmentGo());
		segment.ReceivingEnergy = true;
		segmentTime = sender.segmentTime;
	}

	public override void Destroy()
	{
		if (segment != null)
		{
			segment.Destroy();
		}
	}

	protected override GameObject GetSegmentPrefab()
	{
		if (segmentPrefab == null)
		{
			segmentPrefab = new GameObject(string.Empty);
			Mesh mesh = new Mesh();
			mesh.Clear();
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<int> list3 = new List<int>();
			float sizeMult = 1f;
			BlockAbstractLaser.DrawProjectileLine(-Vector3.forward * 0.5f, Vector3.forward * 0.5f, Vector3.up, list, list2, list3, sizeMult);
			mesh.vertices = list.ToArray();
			mesh.uv = list2.ToArray();
			mesh.triangles = list3.ToArray();
			mesh.name = "Projectile Mesh";
			MeshFilter meshFilter = segmentPrefab.AddComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			MeshRenderer meshRenderer = segmentPrefab.AddComponent<MeshRenderer>();
			meshRenderer.material = (Material)Resources.Load("Materials/Projectile");
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
		gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", laserColor);
		return gameObject;
	}

	public override bool ShouldBeDestroyed()
	{
		return segment == null;
	}

	protected override void StepSegments()
	{
		if (segment == null)
		{
			return;
		}
		segment.TravelledTime += Blocksworld.fixedDeltaTime;
		float num = Vector3.Dot(segment.ExtraVelocity, segment.Direction);
		if (gravityInfluence > 0f)
		{
			segment.ExtraVelocity += Physics.gravity * gravityInfluence * Blocksworld.fixedDeltaTime;
		}
		float num2 = segment.HeadSpeed + num;
		float num3 = segment.TailSpeed + num;
		segment.TravelledDist += num2 * Blocksworld.fixedDeltaTime;
		if (segment.ReceivingEnergy)
		{
			segment.Length += num2 * Blocksworld.fixedDeltaTime;
			segment.StartFraction = 1f - EnergyFraction;
		}
		segment.Length += (num2 - num3) * Blocksworld.fixedDeltaTime;
		segment.Origin += Util.ProjectOntoPlane(segment.ExtraVelocity, segment.Direction) * Blocksworld.fixedDeltaTime;
		Vector3 origin = segment.Origin + segment.Direction * (segment.TravelledDist - segment.Length);
		float num4 = segmentTime / kSpeed;
		RaycastHit hitInfo;
		if ((double)segment.Length < 0.0001 || segment.TravelledTime > num4)
		{
			segment.Destroy();
			segment = null;
		}
		else if (Physics.Raycast(origin, segment.Direction, out hitInfo, segment.Length))
		{
			Block block = BWSceneManager.FindBlock(hitInfo.collider.gameObject);
			if (block != null && block != _sender)
			{
				AddHit(block, segment.Direction, hitInfo.point, hitInfo.normal);
				segment.HeadSpeed = 0f;
				segment.ExtraVelocity = Vector3.zero;
			}
		}
	}

	public override void StopReceivingEnergy()
	{
		if (segment != null)
		{
			segment.ReceivingEnergy = false;
		}
	}

	public override void Update()
	{
		if (segment != null)
		{
			Vector3 vector = segment.Origin + segment.Direction * (segment.TravelledDist - segment.Length);
			Vector3 vector2 = segment.Origin + segment.Direction * segment.TravelledDist;
			Vector3 vector3 = vector2 - vector;
			float magnitude = vector3.magnitude;
			if (!(magnitude < 0.001f))
			{
				Transform goT = segment.goT;
				Transform cameraTransform = Blocksworld.cameraTransform;
				Vector3 position = cameraTransform.position;
				Vector3 rhs = vector - position;
				Vector3 normalized = Vector3.Cross(vector3, rhs).normalized;
				float projectileSizeMultiplier = _sender.projectileSizeMultiplier;
				goT.rotation = Quaternion.LookRotation(vector3 / magnitude, normalized);
				goT.position = 0.5f * (vector + vector2);
				goT.localScale = new Vector3(projectileSizeMultiplier, projectileSizeMultiplier, magnitude);
			}
		}
	}

	protected override void AddHit(Block block, Vector3 direction, Vector3 point, Vector3 normal)
	{
		BlockAbstractLaser.AddProjectileHit(_sender, block);
		Rigidbody rb = block.chunk.rb;
		if (rb != null && !rb.isKinematic)
		{
			rb.AddForceAtPosition(direction * _sender.laserMeta.projectileHitForce, point, ForceMode.Impulse);
		}
		float value = Random.value;
		Vector3 vector = normal + 0.2f * Random.insideUnitSphere;
		string texture = block.GetTexture();
		bool flag = Materials.TextureIsTransparent(texture);
		Color color = block.go.GetComponent<Renderer>().sharedMaterial.color;
		if (!block.isTerrain)
		{
			float r = color.r;
			if (r > 0f)
			{
				float g = color.g;
				float b = color.b;
				if ((g < 0.01f || r / g > 2f) && (b < 0.01f || r / b > 2f))
				{
					color.b = (color.g = (color.r = (r + b + g) / 3f));
				}
			}
		}
		Color color2 = color + 0.1f * Color.white;
		if (flag)
		{
			color.a = Random.Range(0.5f, 0.7f);
			color2.a = Random.Range(0.5f, 0.7f);
		}
		Color color3 = value * color + (1f - value) * color2;
		float num = Random.Range(0.7f, 1.1f);
		ParticleSystem.Particle particle = new ParticleSystem.Particle
		{
			remainingLifetime = num,
			color = color3,
			position = point,
			velocity = vector.normalized * Random.Range(3f, 8f),
			rotation = Random.Range(0f, 360f),
			startLifetime = num,
			size = Random.Range(0.6f, 0.9f),
			randomSeed = (uint)Random.Range(12, 21314)
		};
		BlockAbstractLaser.projectileHitParticleSystem.Emit(particle);
		block.PlayPositionedSound("Projectile Hit", 0.5f);
	}
}
