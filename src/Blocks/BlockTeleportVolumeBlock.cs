using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockTeleportVolumeBlock : BlockGrouped
{
	private BlockTeleportVolumeBlock _otherVolume;

	private bool _teleportActive;

	private bool _isOutlet;

	private HashSet<string> _teleportActiveTags = new HashSet<string>();

	public BlockTeleportVolumeBlock(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockTeleportVolumeBlock>("TeleportVolume.Outlet", null, (Block b) => ((BlockTeleportVolumeBlock)b).SetOutlet, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockTeleportVolumeBlock>("TeleportVolume.Teleport", null, (Block b) => ((BlockTeleportVolumeBlock)b).DoTeleport);
		PredicateRegistry.Add<BlockTeleportVolumeBlock>("TeleportVolume.TeleportTag", null, (Block b) => ((BlockTeleportVolumeBlock)b).DoTeleportTag, new Type[1] { typeof(string) });
	}

	public override void SetBlockGroup(BlockGroup blockGroup)
	{
		base.SetBlockGroup(blockGroup);
		if (blockGroup is TeleportVolumeBlockGroup)
		{
			group = blockGroup;
		}
	}

	public override bool BlockUsesDefaultPaintsAndTextures()
	{
		return false;
	}

	public override bool GroupHasIndividualSripting()
	{
		return true;
	}

	public override bool GroupRotateMainBlockOnPlacement()
	{
		return false;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (Blocksworld.CurrentState == State.Play || Blocksworld.resetting)
		{
			return base.PaintTo(paint, permanent, meshIndex);
		}
		return TileResultCode.True;
	}

	public override void Play()
	{
		base.Play();
		_isOutlet = false;
		_teleportActive = false;
		_teleportActiveTags.Clear();
		Block[] blocks = group.GetBlocks();
		for (int i = 0; i < blocks.Length; i++)
		{
			Block block = blocks[i];
			if (this != block)
			{
				_otherVolume = (BlockTeleportVolumeBlock)blocks[i];
			}
		}
		HideAndMakeTrigger();
		IgnoreRaycasts(value: true);
	}

	public override void Pause()
	{
	}

	public override void Resume()
	{
	}

	public override void ResetFrame()
	{
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		UpdateStopVisibility();
		_isOutlet = false;
		_teleportActive = false;
		_teleportActiveTags.Clear();
		_otherVolume = null;
	}

	public override void OnCreate()
	{
		base.OnCreate();
		UpdateStopVisibility();
		TextureTo("Volume", GetTextureNormal(), permanent: true, 0, force: true);
	}

	public TileResultCode SetOutlet(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_isOutlet = Util.GetIntArg(args, 0, 1) != 0;
		return TileResultCode.True;
	}

	public TileResultCode DoTeleport(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_teleportActive = true;
		return TileResultCode.True;
	}

	public TileResultCode DoTeleportTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		_teleportActiveTags.Add(Util.GetStringArg(args, 0, string.Empty));
		return TileResultCode.True;
	}

	private static Vector3 ComputePositionFromNormalizedPosition(BoxCollider collider, Vector3 normalizedPosition)
	{
		Vector3 vector = Vector3.Scale(Vector3.Scale(collider.transform.localScale, collider.size), normalizedPosition);
		vector = collider.transform.rotation * vector;
		return collider.transform.position + vector;
	}

	private static Vector3 GetNormalizedPositionFromVolume(BoxCollider collider, Vector3 worldPosition)
	{
		Vector3 vector = worldPosition - collider.transform.position;
		vector = Quaternion.Inverse(collider.transform.rotation) * vector;
		Vector3 vector2 = Vector3.Scale(collider.transform.localScale, collider.size);
		return Vector3.Scale(vector, new Vector3(1f / vector2.x, 1f / vector2.y, 1f / vector2.z));
	}

	private static Quaternion GetRotationToOtherVolume(BoxCollider fromCollider, BoxCollider toCollider)
	{
		Quaternion identity = Quaternion.identity;
		identity.SetFromToRotation(-fromCollider.transform.forward, toCollider.transform.forward);
		return identity;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if ((_teleportActive || _teleportActiveTags.Count > 0) && _otherVolume._isOutlet)
		{
			HashSet<GameObject> triggeringBlocks = CollisionManager.GetTriggeringBlocks(this);
			if (triggeringBlocks != null)
			{
				Collider component = go.GetComponent<Collider>();
				Bounds bounds = component.bounds;
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				foreach (GameObject item in triggeringBlocks)
				{
					if (item == null)
					{
						continue;
					}
					if (!_teleportActive)
					{
						bool flag = false;
						for (int i = 0; i < item.transform.childCount; i++)
						{
							Block block = BWSceneManager.FindBlock(item.transform.GetChild(i).gameObject);
							if (block != null && TagManager.blockTags.ContainsKey(block))
							{
								flag |= _teleportActiveTags.Overlaps(TagManager.blockTags[block]);
							}
						}
						if (!flag)
						{
							continue;
						}
					}
					Rigidbody component2 = item.GetComponent<Rigidbody>();
					if (item.transform.childCount > 0)
					{
						Block block2 = BWSceneManager.FindBlock(item.transform.GetChild(0).gameObject);
						if (block2 != null && (!(block2 is BlockMissile blockMissile) || blockMissile.GetLaunchedMissile() == null) && Block.connectedChunks[block2].Count > 1)
						{
							continue;
						}
					}
					BoxCollider boxCollider = component as BoxCollider;
					BoxCollider component3 = _otherVolume.go.GetComponent<BoxCollider>();
					Quaternion rotationToOtherVolume = GetRotationToOtherVolume(boxCollider, component3);
					Vector3 normalizedPositionFromVolume = GetNormalizedPositionFromVolume(boxCollider, component2.worldCenterOfMass);
					Vector3 position = ComputePositionFromNormalizedPosition(component3, normalizedPositionFromVolume);
					component2.position = position;
					component2.rotation = rotationToOtherVolume * component2.rotation;
					component2.velocity = rotationToOtherVolume * component2.velocity;
					component2.angularVelocity = rotationToOtherVolume * component2.angularVelocity;
					hashSet.Add(item);
					_otherVolume.PlayPositionedSound("Teleport_Quick");
				}
				foreach (GameObject item2 in hashSet)
				{
					triggeringBlocks.Remove(item2);
					for (int j = 0; j < item2.transform.childCount; j++)
					{
						Block block3 = BWSceneManager.FindBlock(item2.transform.GetChild(j).gameObject);
						if (block3 != null)
						{
							block3.Teleported();
							if (Blocksworld.worldOceanBlock != null && !Blocksworld.worldOceanBlock.SimulatesBlock(block3))
							{
								Blocksworld.worldOceanBlock.AddBlockToSimulation(block3);
							}
							Blocksworld.blocksworldCamera.HandleTeleport(block3);
						}
					}
				}
			}
		}
		_teleportActive = false;
		_teleportActiveTags.Clear();
	}

	public void HideAndMakeTrigger()
	{
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().isTrigger = true;
			go.GetComponent<Collider>().enabled = true;
		}
		go.GetComponent<Renderer>().enabled = false;
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = false;
		}
	}

	public void ShowAndRemoveIsTrigger()
	{
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().isTrigger = false;
		}
		if (go.GetComponent<Renderer>() != null)
		{
			go.GetComponent<Renderer>().enabled = true;
		}
		if (goShadow != null)
		{
			goShadow.GetComponent<Renderer>().enabled = true;
		}
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (Blocksworld.CurrentState != State.Play)
		{
			texture = "Volume";
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force: true);
	}

	public void UpdateStopVisibility()
	{
		if (Tutorial.state == TutorialState.None)
		{
			ShowAndRemoveIsTrigger();
			IgnoreRaycasts(value: false);
		}
		else
		{
			HideAndMakeTrigger();
		}
	}

	public override bool VisibleInPlayMode()
	{
		return false;
	}

	public override bool ColliderIsTriggerInPlayMode()
	{
		return true;
	}

	protected override void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
	{
	}

	public override bool IsRuntimeInvisible()
	{
		return true;
	}
}
