using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractHover : Block
{
	public Quaternion rotation = Quaternion.identity;

	protected List<Block> varyingMassBlocksChunk = new List<Block>();

	protected Dictionary<Rigidbody, List<Block>> varyingMassBlocksModel = new Dictionary<Rigidbody, List<Block>>();

	protected bool informAboutVaryingGravity = true;

	protected HashSet<Rigidbody> allRigidbodies;

	protected Rigidbody chunkRigidBody;

	protected float totalMassModel;

	protected float sfxLoopStrength;

	protected float currentVol;

	protected bool playLoop = true;

	private int sfxLoopUpdateCounter;

	private const int SFX_LOOP_UPDATE_INTERVAL = 5;

	protected HashSet<Block> chunkBlocksSet;

	public BlockAbstractHover(List<List<Tile>> tiles)
		: base(tiles)
	{
		sfxLoopUpdateCounter = Random.Range(0, 5);
	}

	public override void Play()
	{
		base.Play();
		currentVol = 0f;
		loopName = "Antigrav Hum";
		chunkBlocksSet = null;
	}

	protected void AddGravityForce(Rigidbody rb, float gravityMultiplier, float mass)
	{
		if (rb != null && !rb.isKinematic)
		{
			Vector3 force = Physics.gravity * mass * gravityMultiplier;
			rb.AddForce(force);
			sfxLoopStrength += Mathf.Abs(gravityMultiplier * 0.2f);
		}
	}

	protected void UpdateChunkRigidbody()
	{
		if (chunkRigidBody == null && !didFix)
		{
			chunkRigidBody = chunk.rb;
		}
	}

	private void GatherVaryingMassBlocks()
	{
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		allRigidbodies = new HashSet<Rigidbody>();
		totalMassModel = 0f;
		varyingMassBlocksChunk.Clear();
		varyingMassBlocksModel.Clear();
		foreach (Block item in list)
		{
			Rigidbody component = item.goT.parent.GetComponent<Rigidbody>();
			if (component != null)
			{
				if (item.CanChangeMass())
				{
					if (varyingMassBlocksModel.TryGetValue(component, out var value))
					{
						value.Add(item);
					}
					else
					{
						value = new List<Block>();
						varyingMassBlocksModel[component] = value;
						value.Add(item);
					}
					if (item.chunk == chunk)
					{
						varyingMassBlocksChunk.Add(item);
					}
				}
				else if (!allRigidbodies.Contains(component))
				{
					totalMassModel += component.mass;
					allRigidbodies.Add(component);
				}
			}
			if (informAboutVaryingGravity)
			{
				item.SetVaryingGravity(vg: true);
			}
		}
	}

	public override void Play2()
	{
		base.Play2();
		UpdateChunkRigidbody();
		GatherVaryingMassBlocks();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (broken)
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
		else if (!vanished)
		{
			UpdateChunkRigidbody();
		}
	}

	protected void UpdateSFXs()
	{
		bool flag = sfxLoopUpdateCounter % 5 == 0;
		if (Sound.sfxEnabled && playLoop && !vanished)
		{
			float num = Mathf.Clamp(sfxLoopStrength * 0.5f, 0f, 0.5f);
			float num2 = ((num >= currentVol) ? 0.01f : (-0.01f));
			currentVol = Mathf.Clamp(currentVol + num2, 0f, Mathf.Max(currentVol, num));
			if (flag)
			{
				float pitch = 0.9f + Mathf.Clamp(sfxLoopStrength * 0.05f, 0f, 0.2f);
				PlayLoopSound(currentVol > 0.01f, GetLoopClip(), currentVol, null, pitch);
				UpdateWithinWaterLPFilter();
			}
		}
		else if (flag)
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
		sfxLoopUpdateCounter++;
		sfxLoopStrength = 0f;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		PlayLoopSound(play: false, GetLoopClip());
		chunkBlocksSet = null;
	}

	public override void Pause()
	{
		PlayLoopSound(play: false, GetLoopClip());
	}

	protected float GetVaryingMassOffset(List<Block> list)
	{
		float num = 0f;
		foreach (Block item in list)
		{
			num += item.GetCurrentMassChange();
		}
		return num;
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return true;
	}
}
