using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractMovingPlatform : BlockAbstractPlatform
{
	protected Vector3[] positions = new Vector3[2];

	protected float positionOffset;

	protected float lastPositionOffset;

	protected int targetSteps;

	protected int lastTargetSteps;

	protected float maxSpeed;

	protected const float MIN_SPEED = 0.001f;

	protected bool slideFree;

	protected bool didSlideFree;

	public BlockAbstractMovingPlatform(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public override void Play()
	{
		base.Play();
		positionOffset = 0f;
		lastPositionOffset = 0f;
		targetSteps = 0;
		lastTargetSteps = 0;
		Vector3 scale = GetScale();
		BoxCollider boxCollider = (BoxCollider)go.GetComponent<Collider>();
		boxCollider.size = Vector3.one;
		boxCollider.center = -Vector3.forward * (scale.z - 1f) * 0.5f;
	}

	public override void Play2()
	{
		base.Play2();
		if (chunkRigidBody != null)
		{
			targetPosition = GetPlatformPosition();
			Vector3 scale = GetScale();
			positions[0] = targetPosition;
			positions[1] = targetPosition + goT.forward * (scale.z - 1f);
		}
		go.GetComponent<Renderer>().enabled = false;
		slideFree = false;
		didSlideFree = false;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		go.GetComponent<Renderer>().enabled = true;
		Vector3 scale = GetScale();
		BoxCollider boxCollider = (BoxCollider)go.GetComponent<Collider>();
		boxCollider.size = scale;
		boxCollider.center = Vector3.zero;
	}

	protected override Vector3 GetPlatformPosition()
	{
		return subMeshGameObjects[0].transform.position;
	}

	protected override Vector3 GetTargetPositionOffset()
	{
		Vector3 vector = positions[1] - positions[0];
		Vector3 vector2 = Vector3.zero;
		if (vector.sqrMagnitude > 0.0001f)
		{
			vector2 = vector.normalized;
		}
		return vector2 * positionOffset;
	}

	protected override bool CanScaleMesh(int meshIndex)
	{
		return meshIndex == 0;
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider = false, bool forceRescale = false)
	{
		bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
		Vector3 scale2 = GetScale();
		for (int i = 0; i < subMeshGameObjects.Count; i++)
		{
			GameObject gameObject = subMeshGameObjects[i];
			gameObject.transform.localPosition = new Vector3(0f, 0f, (0f - (scale2.z - 1f)) * 0.5f);
		}
		UpdateSATVolumes();
		return result;
	}

	public override void UpdateSATVolumes()
	{
		base.UpdateSATVolumes();
		Vector3 scale = GetScale();
		if (scale.z > 1f)
		{
			TranslateSATVolumes(-goT.forward * (scale.z - 1f) * 0.5f);
		}
	}

	protected override Vector3 CollisionVolumesScale(CollisionMesh[] meshes)
	{
		return Vector3.one;
	}

	public override Vector3 GetPlayModeCenter()
	{
		return GetPlatformPosition();
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (!permanent && Blocksworld.CurrentState == State.Play)
		{
			meshIndex = ((meshIndex == 0) ? 1 : meshIndex);
		}
		return base.PaintTo(paint, permanent, meshIndex);
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (!permanent && Blocksworld.CurrentState == State.Play)
		{
			meshIndex = ((meshIndex == 0) ? 1 : meshIndex);
		}
		if (meshIndex == 0 && texture != "Glass")
		{
			return TileResultCode.False;
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force);
	}

	protected override void UpdateRuntimeInvisible()
	{
		bool flag = false;
		for (int i = 1; i < ((childMeshes == null) ? 1 : (childMeshes.Count + 1)); i++)
		{
			string texture = GetTexture(i);
			if (texture != "Invisible")
			{
				flag = true;
				break;
			}
		}
		isRuntimeInvisible = !flag;
		go.layer = (int)(isTransparent ? Layer.TransparentFX : ((!isRuntimePhantom) ? goLayerAssignment : Layer.Phantom));
	}

	public override TileResultCode IsPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GetPaint(1) == (string)args[0])
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public override TileResultCode IsTexturedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (GetTexture(1) == (string)args[0])
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}
}
