using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Blocks;

public class BlockGravityGun : Block
{
	public static List<GameObject> blocksHit = new List<GameObject>();

	private float currentAttraction;

	private float influenceAngle = 20f;

	private float influenceRadius = 15f;

	private float selfForceMultiplier = 0.25f;

	private GameObject psObjectAttract;

	private GameObject psObjectRepel;

	private ParticleSystem particlesAttract;

	private ParticleSystem particlesRepel;

	[CompilerGenerated]
	private static PredicateSensorConstructorDelegate f__mg_cache0;

	public BlockGravityGun(List<List<Tile>> tiles)
		: base(tiles)
	{
		psObjectAttract = Object.Instantiate(Resources.Load("Blocks/BlockGravityGun Attract Particle System")) as GameObject;
		psObjectRepel = Object.Instantiate(Resources.Load("Blocks/BlockGravityGun Repel Particle System")) as GameObject;
		particlesAttract = psObjectAttract.GetComponent<ParticleSystem>();
		particlesRepel = psObjectRepel.GetComponent<ParticleSystem>();
		ResetParticles();
	}

	private void ResetParticles(bool clear = true)
	{
		particlesAttract.enableEmission = false;
		particlesRepel.enableEmission = false;
		if (clear)
		{
			particlesAttract.Clear();
			particlesRepel.Clear();
		}
	}

	public static PredicateSensorDelegate IsHitByGravityGun(Block block)
	{
		return (ScriptRowExecutionInfo eInfo, object[] args) => blocksHit.Contains(block.go) ? TileResultCode.True : TileResultCode.False;
	}

	public static void ClearHits()
	{
		blocksHit.Clear();
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockGravityGun>("GravityGun.Repel", null, (Block b) => ((BlockGravityGun)b).Repel);
		PredicateRegistry.Add<BlockGravityGun>("GravityGun.Attract", null, (Block b) => ((BlockGravityGun)b).Attract);
		string name = "GravityGun.HitBy";
		PredicateRegistry.Add<Block>(name, IsHitByGravityGun);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if ((double)Mathf.Abs(currentAttraction) > 0.01)
		{
			ParticleSystem particleSystem = particlesRepel;
			GameObject gameObject = psObjectRepel;
			if (currentAttraction > 0f)
			{
				particleSystem = particlesAttract;
				gameObject = psObjectAttract;
			}
			particleSystem.enableEmission = true;
			gameObject.transform.position = go.transform.position;
			gameObject.transform.rotation = go.transform.rotation;
			gameObject.transform.Rotate(-90f, 0f, 0f);
			Vector3 position = go.transform.position;
			float radius = influenceRadius;
			Collider[] array = Physics.OverlapSphere(position, radius);
			Vector3 up = go.transform.up;
			Vector3 position2 = go.transform.position;
			float num = 5f;
			Vector3 vector = position2 - up * num;
			Rigidbody component = go.transform.parent.GetComponent<Rigidbody>();
			Vector3 vector2 = default(Vector3);
			List<Rigidbody> list = new List<Rigidbody>();
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				Transform transform = collider.gameObject.transform;
				Transform parent = transform.parent;
				if (parent == null)
				{
					continue;
				}
				Rigidbody component2 = parent.GetComponent<Rigidbody>();
				if (!(component == component2) && component2 != null)
				{
					if (!list.Contains(component2))
					{
						list.Add(component2);
					}
					Bounds bounds = collider.bounds;
					Vector3 center = bounds.center;
					Vector3 vector3 = center - position2;
					Vector3 to = center - vector;
					float num2 = Vector3.Angle(up, to);
					float magnitude = vector3.magnitude;
					if (num2 < influenceAngle)
					{
						blocksHit.Add(collider.gameObject);
						Vector3 vector4 = vector3 * -1f;
						vector4.Normalize();
						Vector3 vector5 = bounds.size;
						float num3 = vector5.x * vector5.y * vector5.z;
						float num4 = num3 * 20f * currentAttraction;
						num4 *= (influenceAngle - num2) / influenceAngle;
						num4 *= (influenceRadius - magnitude) / influenceRadius;
						vector4 *= num4;
						component2.AddForceAtPosition(vector4, center);
						vector2 += vector4;
					}
				}
			}
			foreach (Rigidbody item in list)
			{
				float num5 = 1f;
				item.AddForce(num5 * -Physics.gravity);
			}
			Rigidbody component3 = go.transform.parent.GetComponent<Rigidbody>();
			if (component3 != null)
			{
				component3.AddForceAtPosition(vector2 * (0f - selfForceMultiplier), position2);
			}
		}
		else
		{
			ResetParticles(clear: false);
		}
		currentAttraction = 0f;
	}

	public override void Play()
	{
		base.Play();
		ResetParticles();
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		ResetParticles();
	}

	public TileResultCode Repel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		currentAttraction -= 1f;
		return TileResultCode.True;
	}

	public TileResultCode Attract(ScriptRowExecutionInfo eInfo, object[] args)
	{
		currentAttraction += 1f;
		return TileResultCode.True;
	}
}
