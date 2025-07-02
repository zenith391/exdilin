using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockPiston : Block
{
	private enum PistonState
	{
		None,
		Free,
		Move,
		Step
	}

	private PistonState _state;

	private GameObject _fakePlunger;

	private GameObject _plunger;

	private Transform _pistonBase;

	private Transform _pistonShaft;

	private Transform _pistonTop;

	private ConfigurableJoint _joint;

	private List<Collider> _shaftColliders = new List<Collider>();

	private const int PISTON_BASE_MESH_INDEX = 1;

	private const int PISTON_SHAFT_MESH_INDEX = 2;

	private const float PISTON_OFFSET_ERROR = 0.04f;

	public BlockPiston(List<List<Tile>> tiles)
		: base(tiles)
	{
		loopName = "Piston Move";
	}

	public new static void Register()
	{
		Predicate pred = PredicateRegistry.Add<BlockPiston>("Piston.Move", (Block b) => ((BlockPiston)b).IsMovedTo, (Block b) => ((BlockPiston)b).Move, new Type[1] { typeof(float) }, new string[1] { "Speed" });
		PredicateRegistry.Add<BlockPiston>("Piston.MoveReturn", null, (Block b) => ((BlockPiston)b).MoveReturn, new Type[1] { typeof(float) }, new string[1] { "Speed" });
		PredicateRegistry.Add<BlockPiston>("Piston.Step", null, (Block b) => ((BlockPiston)b).Step, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Units", "Seconds" });
		PredicateRegistry.Add<BlockPiston>("Piston.StepReturn", null, (Block b) => ((BlockPiston)b).StepReturn, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Units", "Seconds" });
		PredicateRegistry.Add<BlockPiston>("Piston.AtOffsetPosition", (Block b) => ((BlockPiston)b).AtOffsetPosition, null, new Type[1] { typeof(float) }, new string[1] { "Units" });
		PredicateRegistry.Add<BlockPiston>("Piston.SetLimit", null, (Block b) => ((BlockPiston)b).SetLimit, new Type[1] { typeof(float) }, new string[1] { "Units" });
		PredicateRegistry.Add<BlockPiston>("Piston.FreeMove", (Block b) => ((BlockPiston)b).IsFreeMove, (Block b) => ((BlockPiston)b).FreeMove);
		Block.defaultExtraTiles["Piston"] = new List<List<Tile>>
		{
			new List<Tile>
			{
				Block.ThenTile(),
				new Tile(pred, 1f),
				new Tile(pred, -1f)
			},
			Block.EmptyTileRow()
		};
	}

	protected override void FindSubMeshes()
	{
		if (_pistonBase == null)
		{
			_pistonBase = goT.Find("Piston Base");
			_pistonShaft = goT.Find("Piston Shaft");
			_pistonTop = goT.Find("Piston Top");
		}
		base.FindSubMeshes();
	}

	public override void UpdateSATVolumes()
	{
		base.UpdateSATVolumes();
		if (!skipUpdateSATVolumes)
		{
			Vector3 scale = GetScale();
			if (scale.y > 1f)
			{
				Vector3 offset = -goT.up * (scale.y - 1f) * 0.5f;
				CollisionVolumes.TranslateMeshes(glueMeshes, offset);
			}
		}
	}

	protected override Vector3 CollisionVolumesScale(CollisionMesh[] meshes)
	{
		if (meshes == glueMeshes)
		{
			return Vector3.one;
		}
		return base.CollisionVolumesScale(meshes);
	}

	private void DestroyJoint()
	{
		if (_joint != null)
		{
			UnityEngine.Object.Destroy(_joint);
			_joint = null;
			DestroyFakeRigidbodies();
		}
	}

	private void DestroyFakePlunger()
	{
		if (_fakePlunger != null)
		{
			UnityEngine.Object.Destroy(_fakePlunger);
			_fakePlunger = null;
		}
	}

	private void FindOrCreatePlungerRigidbody()
	{
		List<Block> list = ConnectionsOfType(2, directed: true);
		if (list.Count == 0)
		{
			_plunger = (_fakePlunger = new GameObject(go.name + " Fake Plunger"));
			_fakePlunger.transform.position = goT.position;
			Rigidbody rigidbody = _fakePlunger.AddComponent<Rigidbody>();
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigidbody.mass = 0.1f;
			BoxCollider boxCollider = _fakePlunger.AddComponent<BoxCollider>();
			boxCollider.size = Vector3.one;
			boxCollider.center = goT.up * 0.5f * (Scale().y - 1f);
		}
		else
		{
			_plunger = list[0].chunk.go;
		}
	}

	private void CreatePistonJoint()
	{
		if (chunk.go != _plunger)
		{
			_joint = chunk.go.AddComponent<ConfigurableJoint>();
			_joint.xMotion = ConfigurableJointMotion.Limited;
			_joint.yMotion = ConfigurableJointMotion.Locked;
			_joint.zMotion = ConfigurableJointMotion.Locked;
			_joint.angularXMotion = ConfigurableJointMotion.Locked;
			_joint.angularYMotion = ConfigurableJointMotion.Locked;
			_joint.angularZMotion = ConfigurableJointMotion.Locked;
			_joint.linearLimit = new SoftJointLimit
			{
				limit = 0.04f
			};
			_joint.anchor = goT.localPosition;
			_joint.axis = goT.up;
			_joint.connectedBody = _plunger.GetComponent<Rigidbody>();
			Vector3 connectedAnchor = _joint.connectedAnchor;
			_joint.autoConfigureConnectedAnchor = false;
			_joint.connectedAnchor = connectedAnchor;
		}
	}

	private void CreateColliders()
	{
		goT.GetComponent<BoxCollider>().enabled = false;
		_pistonBase.gameObject.AddComponent<BoxCollider>();
		_pistonTop.gameObject.AddComponent<BoxCollider>();
		BoxCollider boxCollider = _pistonShaft.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(boxCollider.size.x * 0.5f, boxCollider.size.y, boxCollider.size.z * 0.85f);
		_shaftColliders.Add(boxCollider);
		boxCollider = _pistonShaft.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(boxCollider.size.x * 0.7f, boxCollider.size.y, boxCollider.size.z * 0.7f);
		_shaftColliders.Add(boxCollider);
		boxCollider = _pistonShaft.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(boxCollider.size.x * 0.85f, boxCollider.size.y, boxCollider.size.z * 0.5f);
		_shaftColliders.Add(boxCollider);
	}

	private void DestroyColliders()
	{
		UnityEngine.Object.Destroy(_pistonBase.gameObject.GetComponent<BoxCollider>());
		UnityEngine.Object.Destroy(_pistonTop.gameObject.GetComponent<BoxCollider>());
		for (int i = 0; i < _shaftColliders.Count; i++)
		{
			UnityEngine.Object.Destroy(_shaftColliders[i]);
		}
		_shaftColliders.Clear();
		goT.GetComponent<BoxCollider>().enabled = true;
	}

	public override void Play()
	{
		base.Play();
		FindOrCreatePlungerRigidbody();
		CreatePistonJoint();
		CreateColliders();
	}

	public override void Play2()
	{
		base.Play2();
		CreateFakeRigidbodyBetweenJoints();
	}

	public override void Stop(bool resetBlock = true)
	{
		DestroyColliders();
		DestroyJoint();
		DestroyFakeRigidbodies();
		DestroyFakePlunger();
		PlayLoopSound(play: false, GetLoopClip());
		base.Stop(resetBlock);
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider, bool forceRescale)
	{
		bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
		float num = 0.5f * (scale.y - 1f);
		Vector3 vector = Vector3.down * 0.5f * (scale.y - 1f);
		_pistonBase.localPosition = Vector3.down * num;
		_pistonShaft.localPosition = Vector3.down * num;
		_pistonTop.localPosition = Vector3.up * num;
		float y = (scale.y - 0.5833334f) / (5f / 12f);
		_pistonShaft.localScale = new Vector3(1f, y, 1f);
		goT.GetComponent<BoxCollider>().size = scale;
		UpdateSATVolumes();
		return result;
	}

	protected override bool CanScaleMesh(int meshIndex)
	{
		return false;
	}

	public override TileResultCode IsPaintedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return boolToTileResult(GetPaint(1) == (string)args[0]);
	}

	public override TileResultCode PaintToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 1, 1);
		return PaintTo((string)args[0], permanent: false, intArg);
	}

	private TileResultCode IsMovedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode Move(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode MoveReturn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode Step(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode StepReturn(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode AtOffsetPosition(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode SetLimit(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode IsFreeMove(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}

	private TileResultCode FreeMove(ScriptRowExecutionInfo eInfo, object[] args)
	{
		return TileResultCode.True;
	}
}
