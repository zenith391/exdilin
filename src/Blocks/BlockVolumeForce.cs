using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockVolumeForce : BlockPosition
{
	private string collisionName = string.Empty;

	private float forceTime;

	public float windPower = 10f;

	protected const int MAX_SPLASH_SOUNDS = 5;

	protected const float FORCE_DELAY = 0.25f;

	protected Vector3 colliderSize = Vector3.one;

	private Bounds windBounds;

	public BlockVolumeForce(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockVolumeForce>("VolumeForce.SetWindPower", null, (Block b) => ((BlockVolumeForce)b).SetWindPower, new Type[1] { typeof(float) });
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.ThenTile(),
			new Tile(new GAF("Block.Fixed")),
			new Tile(new GAF("VolumeForce.SetWindPower", 10f))
		});
		list.Add(new List<Tile>
		{
			Block.ThenTile(),
			new Tile(new GAF("Block.PlayVfxDurational", "WindLines"))
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["Volume Block Force"] = value;
	}

	public override float GetEffectPower()
	{
		return windPower;
	}

	public TileResultCode SetWindPower(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 0f : ((float)args[0]));
		forceTime = 0f;
		windPower = num;
		return TileResultCode.True;
	}

	public override void Play()
	{
		base.Play();
		go.layer = 13;
		Collider component = go.GetComponent<Collider>();
		component.isTrigger = true;
		collisionName = component.name;
		colliderSize = component.bounds.size;
	}

	public override Vector3 GetEffectSize()
	{
		return colliderSize;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		go.GetComponent<Collider>().isTrigger = false;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		forceTime += Time.deltaTime;
		if (forceTime > 0.25f)
		{
			windPower = 0f;
		}
		if (go == null)
		{
			return;
		}
		Collider component = go.GetComponent<Collider>();
		if (component == null)
		{
			return;
		}
		windBounds = component.bounds;
		if (!CollisionManager.triggering.TryGetValue(collisionName, out var value) || value == null)
		{
			return;
		}
		foreach (GameObject item in value)
		{
			if (item == null || item == go)
			{
				continue;
			}
			Transform transform = item.transform;
			if (transform == null)
			{
				continue;
			}
			Rigidbody component2 = transform.GetComponent<Rigidbody>();
			if (component2 == null || component2.isKinematic)
			{
				continue;
			}
			foreach (Transform item2 in transform)
			{
				if (item2 == null)
				{
					continue;
				}
				GameObject gameObject = item2.gameObject;
				if (gameObject == null)
				{
					continue;
				}
				Block block = BWSceneManager.FindBlock(gameObject);
				if (block == null)
				{
					continue;
				}
				Collider component3 = gameObject.GetComponent<Collider>();
				if (component3 == null)
				{
					continue;
				}
				Bounds bounds = component3.bounds;
				if (bounds.Intersects(windBounds))
				{
					float mass = block.GetMass();
					Vector3 vector = block.Scale();
					float num = vector.x * vector.y * vector.z;
					if ((double)(mass / num) < 0.2501)
					{
						num *= 0.5f;
					}
					Bounds bounds2 = bounds;
					bounds2.SetMinMax(Vector3.Max(bounds.min, windBounds.min), Vector3.Min(bounds.max, windBounds.max));
					Vector3 force = go.transform.forward * windPower;
					component2.AddForceAtPosition(force, bounds2.center, ForceMode.Force);
				}
			}
		}
	}

	public override bool ColliderIsTriggerInPlayMode()
	{
		return true;
	}
}
