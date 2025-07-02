using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class DetachData
{
	public HashSet<Block> blocksToExclude = new HashSet<Block>();

	public HashSet<Block> onlyInclude;

	public HashSet<Block> hitByExplosion;

	public Collider[] colliders;

	public HashSet<Block> detachBlocks;

	public HashSet<Block> blocksWithinMaxRadius;

	public HashSet<Chunk> detachChunks;

	public HashSet<Chunk> forceChunks;

	public Dictionary<Chunk, List<Block>> chunkBlockRemoves;

	public string onlyBlocksWithTag;

	public bool informExploded;

	public bool detachWithoutBreak;

	public bool detachWithoutForce;

	public bool done;

	public AbstractDetachCommand detachForceGiver;

	public int forceCounter;

	public float forceDuration = 1f;

	public float explosionTime;

	public Vector3 position;

	public Block forceDetachBlock;

	public void GatherColliders(Vector3 position, float maxRadius)
	{
		colliders = Physics.OverlapSphere(position, maxRadius);
		if (colliders.Length == 0)
		{
			done = true;
		}
		this.position = position;
	}

	public void GatherBlocks()
	{
		blocksWithinMaxRadius = new HashSet<Block>();
		detachBlocks = new HashSet<Block>();
		bool flag = !string.IsNullOrEmpty(onlyBlocksWithTag);
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider collider = colliders[i];
			if (collider == null)
			{
				continue;
			}
			Block block = BWSceneManager.FindBlock(collider.gameObject);
			if (block == null || (collider.isTrigger && !(block is BlockVolumeBlock)) || (onlyInclude != null && !onlyInclude.Contains(block)))
			{
				continue;
			}
			if (flag)
			{
				bool flag2 = false;
				if (TagManager.blockTags.ContainsKey(block))
				{
					flag2 = TagManager.blockTags[block].Contains(onlyBlocksWithTag);
				}
				if (!flag2 && TagManager.registeredBlocks.ContainsKey(onlyBlocksWithTag))
				{
					if (block.chunk.blocks.Count < TagManager.registeredBlocks[onlyBlocksWithTag].Count)
					{
						for (int j = 0; j < block.chunk.blocks.Count; j++)
						{
							if (flag2)
							{
								break;
							}
							Block key = block.chunk.blocks[j];
							flag2 = TagManager.blockTags.ContainsKey(key) && TagManager.blockTags[key].Contains(onlyBlocksWithTag);
						}
					}
					else
					{
						List<Block> list = TagManager.registeredBlocks[onlyBlocksWithTag];
						for (int k = 0; k < list.Count; k++)
						{
							if (flag2)
							{
								break;
							}
							flag2 = list[k].modelBlock == block.modelBlock;
						}
					}
				}
				if (!flag2)
				{
					continue;
				}
			}
			if (block != null && !block.isTerrain)
			{
				if ((!block.broken || forceDetachBlock == block) && !blocksToExclude.Contains(block) && !Invincibility.IsInvincible(block) && detachForceGiver.DetachBlock(block))
				{
					detachBlocks.Add(block);
				}
				blocksWithinMaxRadius.Add(block);
			}
		}
		if (hitByExplosion != null && hitByExplosion.Count == 0)
		{
			hitByExplosion.UnionWith(blocksWithinMaxRadius);
		}
		if (blocksWithinMaxRadius.Count == 0)
		{
			done = true;
		}
	}

	public void ComputeChunks()
	{
		detachChunks = new HashSet<Chunk>();
		chunkBlockRemoves = new Dictionary<Chunk, List<Block>>();
		forceChunks = new HashSet<Chunk>();
		foreach (Block detachBlock in detachBlocks)
		{
			Chunk chunk = detachBlock.chunk;
			if ((!detachBlock.broken || detachBlock == forceDetachBlock) && detachBlock.BreakByDetachExplosion() && chunk.go != null)
			{
				detachChunks.Add(chunk);
				if (!chunkBlockRemoves.TryGetValue(chunk, out var value))
				{
					value = new List<Block>();
					chunkBlockRemoves[chunk] = value;
				}
				value.Add(detachBlock);
			}
		}
		if (blocksWithinMaxRadius != null && !detachWithoutForce)
		{
			foreach (Block item4 in blocksWithinMaxRadius)
			{
				Chunk chunk2 = item4.chunk;
				if (!detachChunks.Contains(chunk2) && !forceChunks.Contains(chunk2) && chunk2.go != null)
				{
					Rigidbody rb = chunk2.rb;
					if (rb != null && !rb.isKinematic)
					{
						forceChunks.Add(chunk2);
					}
				}
			}
		}
		List<Block> list = new List<Block>();
		List<List<Block>> list2 = new List<List<Block>>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector3> list4 = new List<Vector3>();
		List<Vector3> list5 = new List<Vector3>();
		Dictionary<Block, Chunk> dictionary = new Dictionary<Block, Chunk>();
		HashSet<Chunk> hashSet = new HashSet<Chunk>();
		foreach (KeyValuePair<Chunk, List<Block>> chunkBlockRemove in chunkBlockRemoves)
		{
			Chunk key = chunkBlockRemove.Key;
			List<Block> value2 = chunkBlockRemove.Value;
			Vector3 item = Vector3.zero;
			Vector3 item2 = Vector3.zero;
			Vector3 item3 = key.go.transform.position;
			Rigidbody rb2 = key.rb;
			if (rb2 != null)
			{
				item = rb2.velocity;
				item2 = rb2.angularVelocity;
			}
			else
			{
				list.AddRange(value2);
			}
			int count = list2.Count;
			if (key.blocks.Count == value2.Count)
			{
				foreach (Block block5 in key.blocks)
				{
					list2.Add(new List<Block> { block5 });
				}
			}
			else
			{
				foreach (Block item5 in value2)
				{
					key.RemoveBlock(item5);
					list2.Add(new List<Block> { item5 });
				}
			}
			if (key.blocks.Count > 0)
			{
				HashSet<Block> hashSet2 = new HashSet<Block>(key.blocks);
				while (hashSet2.Count > 0)
				{
					Block block = null;
					List<Block> list6 = new List<Block>(hashSet2);
					foreach (Block item6 in list6)
					{
						if (!detachBlocks.Contains(item6))
						{
							block = item6;
							break;
						}
						hashSet2.Remove(item6);
					}
					if (block == null)
					{
						continue;
					}
					HashSet<Block> hashSet3 = new HashSet<Block>();
					List<Block> list7 = new List<Block>();
					list7.Add(block);
					while (list7.Count > 0)
					{
						int index = list7.Count - 1;
						Block block2 = list7[index];
						list7.RemoveAt(index);
						hashSet3.Add(block2);
						hashSet2.Remove(block2);
						for (int i = 0; i < block2.connections.Count; i++)
						{
							int num = block2.connectionTypes[i];
							if (num != 1)
							{
								continue;
							}
							Block block3 = block2.connections[i];
							if (!hashSet3.Contains(block3) && !detachBlocks.Contains(block3))
							{
								Chunk chunk3 = block3.chunk;
								if (chunk3 == key)
								{
									list7.Add(block3);
								}
							}
						}
					}
					list2.Add(new List<Block>(hashSet3));
				}
			}
			int num2 = list2.Count - count;
			for (int j = 0; j < num2; j++)
			{
				list3.Add(item);
				list5.Add(item2);
				list4.Add(item3);
			}
			for (int k = 0; k < key.blocks.Count; k++)
			{
				dictionary[key.blocks[k]] = key;
			}
			hashSet.Add(key);
		}
		HashSet<List<Block>> hashSet4 = new HashSet<List<Block>>();
		foreach (Chunk item7 in hashSet)
		{
			if (item7.blocks.Count != 0 && Block.connectedCache.TryGetValue(item7.blocks[0], out var value3))
			{
				hashSet4.Add(value3);
				Blocksworld.blocksworldCamera.Unfollow(item7);
				Blocksworld.chunks.Remove(item7);
				item7.Destroy();
				Blocksworld.blocksworldCamera.ChunkDirty(item7);
			}
		}
		List<Chunk> list8 = new List<Chunk>();
		Dictionary<Chunk, Chunk> dictionary2 = new Dictionary<Chunk, Chunk>();
		Dictionary<Chunk, Chunk> dictionary3 = new Dictionary<Chunk, Chunk>();
		for (int l = 0; l < list2.Count; l++)
		{
			List<Block> list9 = list2[l];
			foreach (Block item8 in list9)
			{
				Block.connectedChunks.Remove(item8);
			}
			if (!detachWithoutBreak && list9.Count == 1 && detachBlocks.Contains(list9[0]))
			{
				Block block4 = list9[0];
				if (block4.BreakByDetachExplosion())
				{
					block4.Break(list4[l], list3[l], list5[l]);
				}
			}
			Quaternion rotation = list9[0].chunk.go.transform.rotation;
			Chunk chunk4 = new Chunk(list9, rotation);
			if (chunk4.rb != null)
			{
				chunk4.UpdateCenterOfMass();
				chunk4.rb.velocity = list3[l];
				chunk4.rb.angularVelocity = list5[l];
				if (list9.Count > 0 && list9[0].didFix && !list9[0].broken && !detachBlocks.Contains(list9[0]))
				{
					chunk4.rb.isKinematic = true;
				}
			}
			Blocksworld.chunks.Add(chunk4);
			list8.Add(chunk4);
			forceChunks.Add(chunk4);
			if (list9.Count <= 1)
			{
				continue;
			}
			for (int m = 0; m < list9.Count; m++)
			{
				Block key2 = list9[m];
				if (dictionary.TryGetValue(key2, out var value4))
				{
					dictionary2[value4] = chunk4;
					dictionary3[chunk4] = value4;
				}
			}
		}
		foreach (List<Block> item9 in hashSet4)
		{
			HashSet<Chunk> hashSet5 = new HashSet<Chunk>();
			Dictionary<GameObject, Chunk> dictionary4 = new Dictionary<GameObject, Chunk>();
			Dictionary<Joint, Joint> dictionary5 = new Dictionary<Joint, Joint>();
			for (int n = 0; n < item9.Count; n++)
			{
				Chunk chunk5 = item9[n].chunk;
				hashSet5.Add(chunk5);
				dictionary4[chunk5.go] = chunk5;
				if (dictionary3.TryGetValue(chunk5, out var value5))
				{
					dictionary4[value5.go] = value5;
				}
			}
			foreach (Chunk item10 in hashSet5)
			{
				if (dictionary3.ContainsKey(item10))
				{
					Chunk chunk6 = dictionary3[item10];
					Joint[] components = chunk6.go.GetComponents<Joint>();
					foreach (Joint joint in components)
					{
						ConfigurableJoint configurableJoint = joint as ConfigurableJoint;
						GameObject gameObject = joint.connectedBody.gameObject;
						if (dictionary4.TryGetValue(gameObject, out var value6))
						{
							if (!dictionary3.ContainsKey(value6) && configurableJoint != null && value6.rb != null)
							{
								Joint value7 = ReconstructJoint(chunk6.go, item10.go, value6.go, configurableJoint);
								dictionary5[configurableJoint] = value7;
							}
						}
						else if (configurableJoint != null)
						{
							Joint value8 = ReconstructJoint(chunk6.go, item10.go, gameObject, configurableJoint);
							dictionary5[configurableJoint] = value8;
						}
					}
				}
				else if (item10.go != null)
				{
					Joint[] components2 = item10.go.GetComponents<Joint>();
					foreach (Joint joint2 in components2)
					{
						GameObject gameObject2 = joint2.connectedBody.gameObject;
						if (dictionary4.TryGetValue(gameObject2, out var value9) && dictionary2.ContainsKey(value9))
						{
							Chunk chunk7 = dictionary2[value9];
							Rigidbody component = chunk7.go.GetComponent<Rigidbody>();
							if (component != null)
							{
								joint2.connectedBody = component;
								Vector3 vector = gameObject2.transform.TransformPoint(joint2.connectedAnchor);
								joint2.connectedAnchor = chunk7.go.transform.InverseTransformPoint(vector);
							}
						}
					}
				}
				item10.ChunksAndJointsModified(dictionary5, dictionary2, dictionary3);
			}
		}
		for (int num5 = 0; num5 < list2.Count; num5++)
		{
			List<Block> list10 = list2[num5];
			foreach (Block item11 in list10)
			{
				Block.connectedChunks[item11] = new HashSet<Chunk>(list8);
				item11.ReassignedToChunk(item11.chunk);
			}
		}
		if (Blocksworld.worldOceanBlock != null)
		{
			foreach (Block item12 in list)
			{
				Blocksworld.worldOceanBlock.AddBlockToSimulation(item12);
			}
		}
		if (informExploded)
		{
			foreach (Block detachBlock2 in detachBlocks)
			{
				detachBlock2.Exploded();
			}
		}
		Blocksworld.blocksworldCamera.UpdateChunkSpeeds();
	}

	private Joint ReconstructJoint(GameObject oldFrom, GameObject from, GameObject to, ConfigurableJoint cfgJoint)
	{
		ConfigurableJoint configurableJoint = from.AddComponent<ConfigurableJoint>();
		configurableJoint.autoConfigureConnectedAnchor = false;
		configurableJoint.connectedBody = to.GetComponent<Rigidbody>();
		configurableJoint.angularXDrive = cfgJoint.angularXDrive;
		configurableJoint.angularXLimitSpring = cfgJoint.angularXLimitSpring;
		configurableJoint.angularXMotion = cfgJoint.angularXMotion;
		configurableJoint.angularYLimit = cfgJoint.angularYLimit;
		configurableJoint.angularYMotion = cfgJoint.angularYMotion;
		configurableJoint.angularYZDrive = cfgJoint.angularYZDrive;
		configurableJoint.angularYZLimitSpring = cfgJoint.angularYZLimitSpring;
		configurableJoint.angularZLimit = cfgJoint.angularZLimit;
		configurableJoint.angularZMotion = cfgJoint.angularZMotion;
		configurableJoint.axis = cfgJoint.axis;
		Vector3 vector = oldFrom.transform.TransformPoint(cfgJoint.anchor);
		configurableJoint.anchor = from.transform.InverseTransformPoint(vector);
		configurableJoint.connectedAnchor = cfgJoint.connectedAnchor;
		configurableJoint.enablePreprocessing = cfgJoint.enablePreprocessing;
		configurableJoint.linearLimit = cfgJoint.linearLimit;
		configurableJoint.linearLimitSpring = cfgJoint.linearLimitSpring;
		configurableJoint.lowAngularXLimit = cfgJoint.lowAngularXLimit;
		configurableJoint.rotationDriveMode = cfgJoint.rotationDriveMode;
		configurableJoint.secondaryAxis = cfgJoint.secondaryAxis;
		configurableJoint.slerpDrive = cfgJoint.slerpDrive;
		configurableJoint.targetAngularVelocity = cfgJoint.targetAngularVelocity;
		configurableJoint.targetPosition = cfgJoint.targetPosition;
		configurableJoint.targetRotation = cfgJoint.targetRotation;
		configurableJoint.targetVelocity = cfgJoint.targetVelocity;
		configurableJoint.xDrive = cfgJoint.xDrive;
		configurableJoint.xMotion = cfgJoint.xMotion;
		configurableJoint.yDrive = cfgJoint.yDrive;
		configurableJoint.yMotion = cfgJoint.yMotion;
		configurableJoint.zDrive = cfgJoint.zDrive;
		configurableJoint.zMotion = cfgJoint.zMotion;
		return configurableJoint;
	}

	public void ApplyForces()
	{
		explosionTime = (float)forceCounter * Blocksworld.fixedDeltaTime;
		if (detachForceGiver != null && explosionTime < forceDuration && !detachWithoutForce)
		{
			foreach (Chunk forceChunk in forceChunks)
			{
				if (forceChunk.go != null)
				{
					Rigidbody rb = forceChunk.rb;
					if (rb != null)
					{
						Vector3 force = detachForceGiver.GetForce(rb.worldCenterOfMass, explosionTime);
						rb.AddForce(force, ForceMode.Impulse);
					}
				}
			}
		}
		forceCounter++;
		done = forceCounter > 3;
	}
}
