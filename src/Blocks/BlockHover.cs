using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockHover : Block
{
	private float hoverHeight;

	private float defaultHoverHeight = 5f;

	private float prevError;

	private List<Rigidbody> allRigidbodies;

	public BlockHover(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockHover>("Hover.IncreaseHeight", null, (Block b) => ((BlockHover)b).IncreaseHeight);
		PredicateRegistry.Add<BlockHover>("Hover.DecreaseHeight", null, (Block b) => ((BlockHover)b).DecreaseHeight);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if ((double)Math.Abs(hoverHeight) > 0.01)
		{
			RaycastHit[] array = Physics.RaycastAll(goT.position, Vector3.down, hoverHeight + 5f);
			Array.Sort(array, new RaycastDistanceComparer(goT.position));
			Vector3 vector = default(Vector3);
			bool flag = false;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
				if (block != null)
				{
					Rigidbody component = block.goT.parent.GetComponent<Rigidbody>();
					if (component != null)
					{
						if (!allRigidbodies.Contains(component))
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
				}
				if (flag)
				{
					vector = raycastHit.point;
					break;
				}
			}
			if (flag)
			{
				float magnitude = (vector - goT.position).magnitude;
				Debug.DrawLine(goT.position, vector);
				float num = magnitude - hoverHeight;
				float num2 = num - prevError;
				prevError = num;
				float num3 = 0.5f;
				float num4 = 8f;
				float value = num3 * num + num4 * num2;
				value = -1f + Mathf.Clamp(value, -1f, 1f);
				foreach (Rigidbody allRigidbody in allRigidbodies)
				{
					allRigidbody.AddForce(Physics.gravity * allRigidbody.mass * value);
				}
			}
		}
		hoverHeight = defaultHoverHeight;
	}

	public override void Play()
	{
		base.Play();
		UpdateRigidBodyList();
	}

	private void UpdateRigidBodyList()
	{
		List<Block> list = ConnectednessGraph.ConnectedComponent(this, 3);
		allRigidbodies = new List<Rigidbody>();
		foreach (Block item in list)
		{
			Rigidbody component = item.goT.parent.GetComponent<Rigidbody>();
			if (component != null && !allRigidbodies.Contains(component))
			{
				allRigidbodies.Add(component);
			}
		}
	}

	public TileResultCode IncreaseHeight(ScriptRowExecutionInfo eInfo, object[] args)
	{
		hoverHeight += 1f;
		return TileResultCode.True;
	}

	public TileResultCode DecreaseHeight(ScriptRowExecutionInfo eInfo, object[] args)
	{
		hoverHeight -= 1f;
		return TileResultCode.True;
	}
}
