using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000138 RID: 312
public class DetachData
{
	// Token: 0x0600143B RID: 5179 RVA: 0x0008D234 File Offset: 0x0008B634
	public void GatherColliders(Vector3 position, float maxRadius)
	{
		this.colliders = Physics.OverlapSphere(position, maxRadius);
		if (this.colliders.Length == 0)
		{
			this.done = true;
		}
		this.position = position;
	}

	// Token: 0x0600143C RID: 5180 RVA: 0x0008D260 File Offset: 0x0008B660
	public void GatherBlocks()
	{
		this.blocksWithinMaxRadius = new HashSet<Block>();
		this.detachBlocks = new HashSet<Block>();
		bool flag = !string.IsNullOrEmpty(this.onlyBlocksWithTag);
		for (int i = 0; i < this.colliders.Length; i++)
		{
			Collider collider = this.colliders[i];
			if (!(collider == null))
			{
				Block block = BWSceneManager.FindBlock(collider.gameObject, false);
				if (block != null)
				{
					if (!collider.isTrigger || block is BlockVolumeBlock)
					{
						if (this.onlyInclude == null || this.onlyInclude.Contains(block))
						{
							if (flag)
							{
								bool flag2 = false;
								if (TagManager.blockTags.ContainsKey(block))
								{
									flag2 = TagManager.blockTags[block].Contains(this.onlyBlocksWithTag);
								}
								if (!flag2 && TagManager.registeredBlocks.ContainsKey(this.onlyBlocksWithTag))
								{
									bool flag3 = block.chunk.blocks.Count < TagManager.registeredBlocks[this.onlyBlocksWithTag].Count;
									if (flag3)
									{
										int num = 0;
										while (num < block.chunk.blocks.Count && !flag2)
										{
											Block key = block.chunk.blocks[num];
											flag2 = (TagManager.blockTags.ContainsKey(key) && TagManager.blockTags[key].Contains(this.onlyBlocksWithTag));
											num++;
										}
									}
									else
									{
										List<Block> list = TagManager.registeredBlocks[this.onlyBlocksWithTag];
										int num2 = 0;
										while (num2 < list.Count && !flag2)
										{
											flag2 = (list[num2].modelBlock == block.modelBlock);
											num2++;
										}
									}
								}
								if (!flag2)
								{
									goto IL_251;
								}
							}
							if (block != null && !block.isTerrain)
							{
								if ((!block.broken || this.forceDetachBlock == block) && !this.blocksToExclude.Contains(block) && !Invincibility.IsInvincible(block) && this.detachForceGiver.DetachBlock(block))
								{
									this.detachBlocks.Add(block);
								}
								this.blocksWithinMaxRadius.Add(block);
							}
						}
					}
				}
			}
			IL_251:;
		}
		if (this.hitByExplosion != null && this.hitByExplosion.Count == 0)
		{
			this.hitByExplosion.UnionWith(this.blocksWithinMaxRadius);
		}
		if (this.blocksWithinMaxRadius.Count == 0)
		{
			this.done = true;
		}
	}

	// Token: 0x0600143D RID: 5181 RVA: 0x0008D514 File Offset: 0x0008B914
	public void ComputeChunks()
	{
		this.detachChunks = new HashSet<Chunk>();
		this.chunkBlockRemoves = new Dictionary<Chunk, List<Block>>();
		this.forceChunks = new HashSet<Chunk>();
		foreach (Block block in this.detachBlocks)
		{
			Chunk chunk = block.chunk;
			if ((!block.broken || block == this.forceDetachBlock) && block.BreakByDetachExplosion() && chunk.go != null)
			{
				this.detachChunks.Add(chunk);
				List<Block> list;
				if (!this.chunkBlockRemoves.TryGetValue(chunk, out list))
				{
					list = new List<Block>();
					this.chunkBlockRemoves[chunk] = list;
				}
				list.Add(block);
			}
		}
		if (this.blocksWithinMaxRadius != null && !this.detachWithoutForce)
		{
			foreach (Block block2 in this.blocksWithinMaxRadius)
			{
				Chunk chunk2 = block2.chunk;
				if (!this.detachChunks.Contains(chunk2) && !this.forceChunks.Contains(chunk2) && chunk2.go != null)
				{
					Rigidbody rb = chunk2.rb;
					if (rb != null && !rb.isKinematic)
					{
						this.forceChunks.Add(chunk2);
					}
				}
			}
		}
		List<Block> list2 = new List<Block>();
		List<List<Block>> list3 = new List<List<Block>>();
		List<Vector3> list4 = new List<Vector3>();
		List<Vector3> list5 = new List<Vector3>();
		List<Vector3> list6 = new List<Vector3>();
		Dictionary<Block, Chunk> dictionary = new Dictionary<Block, Chunk>();
		HashSet<Chunk> hashSet = new HashSet<Chunk>();
		foreach (KeyValuePair<Chunk, List<Block>> keyValuePair in this.chunkBlockRemoves)
		{
			Chunk key = keyValuePair.Key;
			List<Block> value = keyValuePair.Value;
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
				list2.AddRange(value);
			}
			int count = list3.Count;
			if (key.blocks.Count == value.Count)
			{
				foreach (Block item4 in key.blocks)
				{
					list3.Add(new List<Block>
					{
						item4
					});
				}
			}
			else
			{
				foreach (Block block3 in value)
				{
					key.RemoveBlock(block3);
					list3.Add(new List<Block>
					{
						block3
					});
				}
			}
			if (key.blocks.Count > 0)
			{
				HashSet<Block> hashSet2 = new HashSet<Block>(key.blocks);
				while (hashSet2.Count > 0)
				{
					Block block4 = null;
					List<Block> list7 = new List<Block>(hashSet2);
					foreach (Block block5 in list7)
					{
						if (!this.detachBlocks.Contains(block5))
						{
							block4 = block5;
							break;
						}
						hashSet2.Remove(block5);
					}
					if (block4 != null)
					{
						HashSet<Block> hashSet3 = new HashSet<Block>();
						List<Block> list8 = new List<Block>();
						list8.Add(block4);
						while (list8.Count > 0)
						{
							int index = list8.Count - 1;
							Block block6 = list8[index];
							list8.RemoveAt(index);
							hashSet3.Add(block6);
							hashSet2.Remove(block6);
							for (int i = 0; i < block6.connections.Count; i++)
							{
								int num = block6.connectionTypes[i];
								if (num == 1)
								{
									Block block7 = block6.connections[i];
									if (!hashSet3.Contains(block7) && !this.detachBlocks.Contains(block7))
									{
										Chunk chunk3 = block7.chunk;
										if (chunk3 == key)
										{
											list8.Add(block7);
										}
									}
								}
							}
						}
						list3.Add(new List<Block>(hashSet3));
					}
				}
			}
			int num2 = list3.Count - count;
			for (int j = 0; j < num2; j++)
			{
				list4.Add(item);
				list6.Add(item2);
				list5.Add(item3);
			}
			for (int k = 0; k < key.blocks.Count; k++)
			{
				dictionary[key.blocks[k]] = key;
			}
			hashSet.Add(key);
		}
		HashSet<List<Block>> hashSet4 = new HashSet<List<Block>>();
		foreach (Chunk chunk4 in hashSet)
		{
			if (chunk4.blocks.Count != 0)
			{
				List<Block> item5;
				if (Block.connectedCache.TryGetValue(chunk4.blocks[0], out item5))
				{
					hashSet4.Add(item5);
					Blocksworld.blocksworldCamera.Unfollow(chunk4);
					Blocksworld.chunks.Remove(chunk4);
					chunk4.Destroy(false);
					Blocksworld.blocksworldCamera.ChunkDirty(chunk4);
				}
			}
		}
		List<Chunk> list9 = new List<Chunk>();
		Dictionary<Chunk, Chunk> dictionary2 = new Dictionary<Chunk, Chunk>();
		Dictionary<Chunk, Chunk> dictionary3 = new Dictionary<Chunk, Chunk>();
		for (int l = 0; l < list3.Count; l++)
		{
			List<Block> list10 = list3[l];
			foreach (Block key2 in list10)
			{
				Block.connectedChunks.Remove(key2);
			}
			if (!this.detachWithoutBreak && list10.Count == 1 && this.detachBlocks.Contains(list10[0]))
			{
				Block block8 = list10[0];
				if (block8.BreakByDetachExplosion())
				{
					block8.Break(list5[l], list4[l], list6[l]);
				}
			}
			Quaternion rotation = list10[0].chunk.go.transform.rotation;
			Chunk chunk5 = new Chunk(list10, rotation, false);
			if (chunk5.rb != null)
			{
				chunk5.UpdateCenterOfMass(true);
				chunk5.rb.velocity = list4[l];
				chunk5.rb.angularVelocity = list6[l];
				if (list10.Count > 0 && list10[0].didFix && !list10[0].broken && !this.detachBlocks.Contains(list10[0]))
				{
					chunk5.rb.isKinematic = true;
				}
			}
			Blocksworld.chunks.Add(chunk5);
			list9.Add(chunk5);
			this.forceChunks.Add(chunk5);
			if (list10.Count > 1)
			{
				for (int m = 0; m < list10.Count; m++)
				{
					Block key3 = list10[m];
					Chunk chunk6;
					if (dictionary.TryGetValue(key3, out chunk6))
					{
						dictionary2[chunk6] = chunk5;
						dictionary3[chunk5] = chunk6;
					}
				}
			}
		}
		foreach (List<Block> list11 in hashSet4)
		{
			HashSet<Chunk> hashSet5 = new HashSet<Chunk>();
			Dictionary<GameObject, Chunk> dictionary4 = new Dictionary<GameObject, Chunk>();
			Dictionary<Joint, Joint> dictionary5 = new Dictionary<Joint, Joint>();
			for (int n = 0; n < list11.Count; n++)
			{
				Chunk chunk7 = list11[n].chunk;
				hashSet5.Add(chunk7);
				dictionary4[chunk7.go] = chunk7;
				Chunk chunk8;
				if (dictionary3.TryGetValue(chunk7, out chunk8))
				{
					dictionary4[chunk8.go] = chunk8;
				}
			}
			foreach (Chunk chunk9 in hashSet5)
			{
				if (dictionary3.ContainsKey(chunk9))
				{
					Chunk chunk10 = dictionary3[chunk9];
					foreach (Joint joint in chunk10.go.GetComponents<Joint>())
					{
						ConfigurableJoint configurableJoint = joint as ConfigurableJoint;
						GameObject gameObject = joint.connectedBody.gameObject;
						Chunk chunk11;
						if (dictionary4.TryGetValue(gameObject, out chunk11))
						{
							if (!dictionary3.ContainsKey(chunk11))
							{
								if (configurableJoint != null && chunk11.rb != null)
								{
									Joint value2 = this.ReconstructJoint(chunk10.go, chunk9.go, chunk11.go, configurableJoint);
									dictionary5[configurableJoint] = value2;
								}
							}
						}
						else if (configurableJoint != null)
						{
							Joint value3 = this.ReconstructJoint(chunk10.go, chunk9.go, gameObject, configurableJoint);
							dictionary5[configurableJoint] = value3;
						}
					}
				}
				else if (chunk9.go != null)
				{
					foreach (Joint joint2 in chunk9.go.GetComponents<Joint>())
					{
						GameObject gameObject2 = joint2.connectedBody.gameObject;
						Chunk key4;
						if (dictionary4.TryGetValue(gameObject2, out key4) && dictionary2.ContainsKey(key4))
						{
							Chunk chunk12 = dictionary2[key4];
							Rigidbody component = chunk12.go.GetComponent<Rigidbody>();
							if (component != null)
							{
								joint2.connectedBody = component;
								Vector3 vector = gameObject2.transform.TransformPoint(joint2.connectedAnchor);
								joint2.connectedAnchor = chunk12.go.transform.InverseTransformPoint(vector);
							}
						}
					}
				}
				chunk9.ChunksAndJointsModified(dictionary5, dictionary2, dictionary3);
			}
		}
		for (int num5 = 0; num5 < list3.Count; num5++)
		{
			List<Block> list12 = list3[num5];
			foreach (Block block9 in list12)
			{
				Block.connectedChunks[block9] = new HashSet<Chunk>(list9);
				block9.ReassignedToChunk(block9.chunk);
			}
		}
		if (Blocksworld.worldOceanBlock != null)
		{
			foreach (Block b in list2)
			{
				Blocksworld.worldOceanBlock.AddBlockToSimulation(b);
			}
		}
		if (this.informExploded)
		{
			foreach (Block block10 in this.detachBlocks)
			{
				block10.Exploded();
			}
		}
		Blocksworld.blocksworldCamera.UpdateChunkSpeeds();
	}

	// Token: 0x0600143E RID: 5182 RVA: 0x0008E25C File Offset: 0x0008C65C
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

	// Token: 0x0600143F RID: 5183 RVA: 0x0008E418 File Offset: 0x0008C818
	public void ApplyForces()
	{
		this.explosionTime = (float)this.forceCounter * Blocksworld.fixedDeltaTime;
		if (this.detachForceGiver != null && this.explosionTime < this.forceDuration && !this.detachWithoutForce)
		{
			foreach (Chunk chunk in this.forceChunks)
			{
				if (chunk.go != null)
				{
					Rigidbody rb = chunk.rb;
					if (rb != null)
					{
						Vector3 force = this.detachForceGiver.GetForce(rb.worldCenterOfMass, this.explosionTime);
						rb.AddForce(force, ForceMode.Impulse);
					}
				}
			}
		}
		this.forceCounter++;
		this.done = (this.forceCounter > 3);
	}

	// Token: 0x04000FE4 RID: 4068
	public HashSet<Block> blocksToExclude = new HashSet<Block>();

	// Token: 0x04000FE5 RID: 4069
	public HashSet<Block> onlyInclude;

	// Token: 0x04000FE6 RID: 4070
	public HashSet<Block> hitByExplosion;

	// Token: 0x04000FE7 RID: 4071
	public Collider[] colliders;

	// Token: 0x04000FE8 RID: 4072
	public HashSet<Block> detachBlocks;

	// Token: 0x04000FE9 RID: 4073
	public HashSet<Block> blocksWithinMaxRadius;

	// Token: 0x04000FEA RID: 4074
	public HashSet<Chunk> detachChunks;

	// Token: 0x04000FEB RID: 4075
	public HashSet<Chunk> forceChunks;

	// Token: 0x04000FEC RID: 4076
	public Dictionary<Chunk, List<Block>> chunkBlockRemoves;

	// Token: 0x04000FED RID: 4077
	public string onlyBlocksWithTag;

	// Token: 0x04000FEE RID: 4078
	public bool informExploded;

	// Token: 0x04000FEF RID: 4079
	public bool detachWithoutBreak;

	// Token: 0x04000FF0 RID: 4080
	public bool detachWithoutForce;

	// Token: 0x04000FF1 RID: 4081
	public bool done;

	// Token: 0x04000FF2 RID: 4082
	public AbstractDetachCommand detachForceGiver;

	// Token: 0x04000FF3 RID: 4083
	public int forceCounter;

	// Token: 0x04000FF4 RID: 4084
	public float forceDuration = 1f;

	// Token: 0x04000FF5 RID: 4085
	public float explosionTime;

	// Token: 0x04000FF6 RID: 4086
	public Vector3 position;

	// Token: 0x04000FF7 RID: 4087
	public Block forceDetachBlock;
}
