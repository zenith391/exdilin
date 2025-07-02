using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Blocks;

public class BlockTankTreadsWheel : BlockGrouped, INoCollisionSound, IHelpForceGiver
{
	public class TankTreadsSupportWheel
	{
		public GameObject go;

		public ConfigurableJoint joint;

		public Vector3 pausedVelocity;

		public Vector3 pausedAngularVelocity;

		public SupportWheelHelpForceBehaviour helpBehaviour;

		public void DestroyJoint()
		{
			if (joint != null)
			{
				UnityEngine.Object.Destroy(joint);
				joint = null;
			}
		}

		public void Destroy()
		{
			if (go != null)
			{
				UnityEngine.Object.Destroy(go);
				go = null;
			}
			DestroyJoint();
		}
	}

	public class TreadLink : IHelpForceGiver
	{
		public Vector3 fromCoord;

		public Vector3 toCoord;

		public Transform fromTransform;

		public Transform toTransform;

		public GameObject visualGo;

		public Transform visualGoT;

		public GameObject collisionGo;

		public ConfigurableJoint joint1;

		public ConfigurableJoint joint2;

		public TreadsInfo treadsInfo;

		public float length = 1f;

		public float volume = 1f;

		public Vector3 pausedVelocity;

		public Vector3 pausedAngularVelocity;

		private Vector3 helpForceErrorSum = Vector3.zero;

		public SupportWheelHelpForceBehaviour helpBehaviour;

		public Vector3 FromToWorld(Vector3 right, Vector3 up, Vector3 forward)
		{
			Vector3 position = fromTransform.position;
			float x = fromCoord.x;
			float y = fromCoord.y;
			float z = fromCoord.z;
			return new Vector3(position.x + x * right.x + y * up.x + z * forward.x, position.y + x * right.y + y * up.y + z * forward.y, position.z + x * right.z + y * up.z + z * forward.z);
		}

		public Vector3 ToToWorld(Vector3 right, Vector3 up, Vector3 forward)
		{
			Vector3 position = toTransform.position;
			float x = toCoord.x;
			float y = toCoord.y;
			float z = toCoord.z;
			return new Vector3(position.x + x * right.x + y * up.x + z * forward.x, position.y + x * right.y + y * up.y + z * forward.y, position.z + x * right.z + y * up.z + z * forward.z);
		}

		public Vector3 GetTreadVelocity()
		{
			Vector3 normalized = (toTransform.position - fromTransform.position).normalized;
			return treadsInfo.speed * normalized;
		}

		public Vector3 GetHelpForceAt(Rigidbody thisRb, Rigidbody otherRb, Vector3 pos, Vector3 relVel, bool fresh)
		{
			Vector3 normalized = (toTransform.position - fromTransform.position).normalized;
			Vector3 vector = treadsInfo.speed * normalized;
			Vector3 lhs = relVel - vector;
			Vector3 vector2 = Vector3.Dot(lhs, normalized) * normalized;
			float num = Mathf.Abs(treadsInfo.speed);
			if (vector2.sqrMagnitude > num * num)
			{
				vector2 = vector2.normalized * num;
			}
			if (!fresh)
			{
				float num2 = 1f + 0.1f * helpForceErrorSum.magnitude;
				vector2 /= num2;
				helpForceErrorSum += vector2;
				helpForceErrorSum *= 0.95f;
			}
			else
			{
				helpForceErrorSum = Vector3.zero;
			}
			return 3f * vector2 * thisRb.mass;
		}

		public Vector3 GetForcePoint(Vector3 contactPoint)
		{
			return contactPoint;
		}

		public bool IsHelpForceActive()
		{
			return Mathf.Abs(treadsInfo.speed) > 0.01f;
		}

		public void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		{
			if (collisionGo != null)
			{
				UnityEngine.Object.Destroy(collisionGo);
			}
			Rigidbody rigidbody = visualGo.AddComponent<Rigidbody>();
			rigidbody.drag = 0.2f;
			rigidbody.angularDrag = 2f;
			visualGo.AddComponent<BoxCollider>();
			Block.AddExplosiveForce(rigidbody, visualGoT.position, chunkPos, chunkVel, chunkAngVel);
		}

		public void DestroyJoints()
		{
			if (joint1 != null)
			{
				UnityEngine.Object.Destroy(joint1);
				joint1 = null;
			}
			if (joint2 != null)
			{
				UnityEngine.Object.Destroy(joint2);
				joint2 = null;
			}
		}

		public void Destroy()
		{
			DestroyJoints();
			if (collisionGo != null)
			{
				UnityEngine.Object.Destroy(collisionGo);
				collisionGo = null;
			}
			if (visualGo != null)
			{
				MeshFilter component = visualGo.GetComponent<MeshFilter>();
				UnityEngine.Object.Destroy(component.sharedMesh);
				UnityEngine.Object.Destroy(component.mesh);
				UnityEngine.Object.Destroy(visualGo);
				visualGo = null;
			}
		}
	}

	public enum TreadsMode
	{
		Drive,
		Rolling,
		Spinning
	}

	public class TreadsInfo
	{
		public TreadsMode mode = TreadsMode.Rolling;

		public float driveSpeedTarget;

		public float spinSpeedTarget;

		public float rollSpeedTarget;

		public float speed;

		public float position;

		public List<TreadLink> links = new List<TreadLink>();

		public List<TreadLink> physicalLinks = new List<TreadLink>();

		public List<Vector3> simplifiedHullPoints = new List<Vector3>();

		public Dictionary<GameObject, TreadLink> gameObjectToTreadLink = new Dictionary<GameObject, TreadLink>();

		public List<Renderer> TreadRenders = new List<Renderer>();

		public float appliedWaterForceTime = -1f;

		public float uValue = 1f;
	}

	private class TankTreadsSATMeshesInfo
	{
		public CollisionMesh shape;

		public CollisionMesh joint1;

		public CollisionMesh joint2;

		public Vector3 position;

		public string key;

		public static string GetKey(BlockTankTreadsWheel wheel)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Dictionary<Block, Vector3> dictionary = wheel.ComputeLocalCoords(snap: true);
			Block[] blocks = wheel.group.GetBlocks();
			foreach (Block block in blocks)
			{
				stringBuilder.Append("W");
				if (block != wheel)
				{
					Vector3 vector = dictionary[block];
					for (int j = 0; j < 3; j++)
					{
						stringBuilder.Append(Mathf.RoundToInt(vector[j] * 2f));
					}
				}
				stringBuilder.Append("s");
				Vector3 vector2 = block.Scale();
				for (int k = 0; k < 3; k++)
				{
					stringBuilder.Append(Mathf.RoundToInt(vector2[k]));
				}
				stringBuilder.Append("r");
				Vector3 vector3 = Util.Round(block.GetRotation().eulerAngles);
				for (int l = 0; l < 3; l++)
				{
					stringBuilder.Append(Mathf.RoundToInt(vector3[l]));
				}
			}
			return stringBuilder.ToString();
		}
	}

	public static Predicate predicateTankTreadsAnalogStickControl;

	public static Predicate predicateTankTreadsDrive;

	public static Predicate predicateTankTreadsDriveAlongAnalogStick;

	public static Predicate predicateTankTreadsTurnAlongAnalogStick;

	private int treatAsVehicleStatus = -1;

	public Block externalControlBlock;

	private bool inPlayOrFrameCaptureMode;

	private float originalMaxDepenetrationVelocity;

	private GameObject chassis;

	private GameObject fakeChassis;

	private GameObject hideAxleN;

	private GameObject hideAxleP;

	private bool axlesDirty = true;

	private Material treadMaterial;

	public SupportWheelHelpForceBehaviour helpForceBehaviour;

	private float treadsReferenceMass;

	private Vector3 treadsReferenceInertia;

	private Vector3 treadsUp = Vector3.up;

	private Vector3 camHintDir = Vector3.zero;

	private List<ConfigurableJoint> joints = new List<ConfigurableJoint>();

	private JointDrive drive;

	private float scaleSpeed;

	private float wheelMass = 1f;

	private Vector3 helpForceErrorSum = Vector3.zero;

	private GameObject buildModeColliderGo;

	private bool treadsDirty = true;

	private bool ignoreCollisionsDirty;

	public float onGround;

	private const float MIN_LINK_LENGTH = 0.25f;

	private const float SUPPORT_WHEEL_RADIUS = 0.25f;

	private const float SUPPORT_WHEEL_MAX_DISTANCE = 4.5f;

	private const int SUPPORT_WHEEL_MAX_COUNT = 2;

	private const float SUSPENSION_BUMP_HEIGHT = 0.075f;

	private const float TREADS_WIDTH = 0.2f;

	private const float DEFAULT_UV_PER_UNIT = 0.5f;

	private float uvPerUnit = 0.5f;

	private Vector3 pausedVelocityAxle;

	private Vector3 pausedAngularVelocityAxle;

	private HashSet<Block> blocksInModelSet;

	private bool isInvisible;

	private bool engineLoopOn;

	private float engineLoopPitch = 1f;

	private float engineLoopVolume;

	private bool engineIncreasingPitch;

	private int idleEngineCounter;

	private float volScale = 1f;

	private float pitchScale = 1f;

	public List<BlockTankTreadsWheel> groupBlocks;

	private List<TankTreadsSupportWheel> supportWheels = new List<TankTreadsSupportWheel>();

	public TreadsInfo treadsInfo;

	private int engineIntervalCounter;

	private Dictionary<Block, Vector3> localCoords;

	private string cachedSatMeshesKey;

	private string cachedTreadsKey;

	private TankTreadsSATMeshesInfo cachedSatMeshes;

	private TankTreadsSATMeshesInfo cachedTreadsMeshes;

	public BlockTankTreadsWheel(List<List<Tile>> tiles)
		: base(tiles)
	{
		loopName = "Tank Tread Run Loop";
		go.GetComponent<Renderer>().enabled = false;
		hideAxleN = goT.Find("Treads X N").gameObject;
		hideAxleP = goT.Find("Treads X P").gameObject;
		hideAxleN.GetComponent<Renderer>().enabled = false;
		hideAxleP.GetComponent<Renderer>().enabled = false;
	}

	public new static void Register()
	{
		predicateTankTreadsDrive = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.Drive", (Block b) => ((BlockTankTreadsWheel)b).IsDrivingSensor, (Block b) => ((BlockTankTreadsWheel)b).Drive, new Type[1] { typeof(float) }, new string[1] { "Force" });
		predicateTankTreadsTurnAlongAnalogStick = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.TurnAlongAnalogStick", null, (Block b) => ((BlockTankTreadsWheel)b).TurnAlongAnalogStick, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Stick name", "Force" });
		predicateTankTreadsDriveAlongAnalogStick = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.DriveAlongAnalogStick", null, (Block b) => ((BlockTankTreadsWheel)b).DriveAlongAnalogStick, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Stick name", "Force" });
		predicateTankTreadsAnalogStickControl = PredicateRegistry.Add<BlockTankTreadsWheel>("TankTreads.AnalogStickControl", null, (Block b) => ((BlockTankTreadsWheel)b).AnalogStickControl, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Stick name", "Force" });
	}

	public override bool HasDefaultScript(List<List<Tile>> tilesToUse = null)
	{
		bool flag = base.HasDefaultScript(tilesToUse);
		if (!flag)
		{
			if (tilesToUse == null)
			{
				tilesToUse = tiles;
			}
			if (tilesToUse.Count == 2)
			{
				return tilesToUse[1].Count == 1;
			}
			return false;
		}
		return flag;
	}

	private void SetTreadsVisible(bool v)
	{
		for (int i = 0; i < treadsInfo.links.Count; i++)
		{
			TreadLink treadLink = treadsInfo.links[i];
			if (treadLink.visualGo != null)
			{
				treadLink.visualGo.GetComponent<Renderer>().enabled = v;
			}
			if (treadLink.collisionGo != null)
			{
				treadLink.collisionGo.GetComponent<Collider>().isTrigger = !v;
			}
		}
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			if (supportWheel.go != null)
			{
				Collider component = supportWheel.go.GetComponent<Collider>();
				if (component != null)
				{
					component.isTrigger = !v;
				}
			}
		}
	}

	public override void Appeared()
	{
		base.Appeared();
		if (IsMainBlockInGroup())
		{
			SetTreadsVisible(v: true);
		}
		if (GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
		{
			blockTankTreadsWheel.ignoreCollisionsDirty = true;
		}
	}

	public override void Vanished()
	{
		base.Vanished();
		if (IsMainBlockInGroup())
		{
			SetTreadsVisible(v: false);
		}
	}

	public override Vector3 GetWaterForce(float fractionWithin, Vector3 relativeVelocity, BlockAbstractWater water)
	{
		if (treadsInfo.mode == TreadsMode.Drive && Mathf.Abs(treadsInfo.speed) > 0.05f && treadsInfo.appliedWaterForceTime < Time.fixedTime)
		{
			Bounds waterBounds = water.GetWaterBounds();
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < treadsInfo.physicalLinks.Count; i++)
			{
				TreadLink treadLink = treadsInfo.physicalLinks[i];
				Vector3 normalized = (treadLink.toTransform.position - treadLink.fromTransform.position).normalized;
				Vector3 vector = treadsInfo.speed * normalized;
				Vector3 point = 0.5f * (treadLink.fromTransform.position + treadLink.toTransform.position);
				if (waterBounds.Contains(point))
				{
					zero -= (vector + relativeVelocity) * treadLink.volume;
				}
			}
			treadsInfo.appliedWaterForceTime = Time.fixedTime;
			return zero;
		}
		return Vector3.zero;
	}

	public TileResultCode AnalogStickControl(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			float maxSpeed = eInfo.floatArg * Util.GetFloatArg(args, 1, 5f);
			AnalogStickControl(stringArg, maxSpeed);
		}
		return TileResultCode.True;
	}

	public void DriveAlongAnalogStick(string stickName, float maxSpeed)
	{
		if (!broken && !(chassis == null))
		{
			Blocksworld.UI.Controls.EnableDPad(stickName, MoverDirectionMask.ALL);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stickName);
			worldDPadOffset = Util.ProjectOntoPlane(worldDPadOffset, Vector3.up);
			float magnitude = worldDPadOffset.magnitude;
			if (!(magnitude < 0.01f))
			{
				engineLoopOn = !broken;
				idleEngineCounter = 0;
				engineIncreasingPitch = true;
				Vector3 up = chassis.transform.up;
				Vector3 lhs = Vector3.Cross(goT.right, up);
				float num = maxSpeed * Vector3.Dot(lhs, worldDPadOffset);
				treadsInfo.driveSpeedTarget += num;
				treadsInfo.mode = TreadsMode.Drive;
			}
		}
	}

	public void AnalogStickControl(string stickName, float maxSpeed)
	{
		if (broken || chassis == null)
		{
			return;
		}
		Blocksworld.UI.Controls.EnableDPad(stickName, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stickName);
		worldDPadOffset = Util.ProjectOntoPlane(worldDPadOffset, Vector3.up);
		float magnitude = worldDPadOffset.magnitude;
		if (magnitude < 0.01f)
		{
			return;
		}
		engineLoopOn = !broken;
		idleEngineCounter = 0;
		engineIncreasingPitch = true;
		Transform transform = goT;
		Vector3 position = transform.position;
		Vector3 up = chassis.transform.up;
		Vector3 vector = Vector3.Cross(transform.right, up);
		camHintDir += worldDPadOffset;
		float num = Mathf.Clamp(1f - Vector3.Dot(vector, worldDPadOffset), 0f, 1f);
		float num2 = magnitude * Mathf.Min(15f * num, maxSpeed);
		UpdateConnectedCache();
		HashSet<Chunk> hashSet = Block.connectedChunks[this];
		Vector3 zero = Vector3.zero;
		int num3 = 0;
		foreach (Chunk item in hashSet)
		{
			if (item.go != null)
			{
				zero += item.go.transform.position;
				num3++;
			}
		}
		if (num3 > 0)
		{
			float num4 = num2 * Mathf.Sign(Vector3.Dot(up, Vector3.Cross(vector, worldDPadOffset)));
			zero /= (float)num3;
			Vector3 lhs = position - zero;
			float num5 = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(lhs, vector)));
			treadsInfo.driveSpeedTarget += num5 * num4;
		}
		float num6 = maxSpeed * Vector3.Dot(vector, worldDPadOffset);
		treadsInfo.driveSpeedTarget += num6;
		treadsInfo.mode = TreadsMode.Drive;
	}

	public void TurnAlongAnalogStick(string stickName, float maxSpeed)
	{
		if (broken || chassis == null)
		{
			return;
		}
		Blocksworld.UI.Controls.EnableDPad(stickName, MoverDirectionMask.ALL);
		Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(stickName);
		float magnitude = worldDPadOffset.magnitude;
		worldDPadOffset = Util.ProjectOntoPlane(worldDPadOffset, Vector3.up);
		if (worldDPadOffset.magnitude < 0.01f)
		{
			return;
		}
		engineLoopOn = !broken;
		idleEngineCounter = 0;
		engineIncreasingPitch = true;
		Vector3 up = chassis.transform.up;
		Vector3 vector = Vector3.Cross(goT.right, up);
		float num = Mathf.Clamp(1f - Vector3.Dot(vector, worldDPadOffset), 0f, 1f);
		float num2 = magnitude * Mathf.Min(15f * num, maxSpeed);
		float num3 = num2 * Mathf.Sign(Vector3.Dot(up, Vector3.Cross(vector, worldDPadOffset)));
		Transform transform = goT;
		Vector3 position = transform.position;
		UpdateConnectedCache();
		HashSet<Chunk> hashSet = Block.connectedChunks[this];
		Vector3 zero = Vector3.zero;
		int num4 = 0;
		foreach (Chunk item in hashSet)
		{
			if (item.go != null)
			{
				zero += item.go.transform.position;
				num4++;
			}
		}
		if (num4 != 0)
		{
			zero /= (float)num4;
			Vector3 vector2 = position - zero;
			float num5 = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(vector2.normalized, vector)));
			treadsInfo.driveSpeedTarget += num5 * num3;
			treadsInfo.mode = TreadsMode.Drive;
		}
	}

	public TileResultCode DriveAlongAnalogStick(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			float maxSpeed = eInfo.floatArg * Util.GetFloatArg(args, 1, 5f);
			DriveAlongAnalogStick(stringArg, maxSpeed);
		}
		return TileResultCode.True;
	}

	public override void ChunksAndJointsModified(Dictionary<Joint, Joint> oldToNew, Dictionary<Chunk, Chunk> oldToNewChunks, Dictionary<Chunk, Chunk> newToOldChunks)
	{
		if (!IsMainBlockInGroup() || broken || isTreasure)
		{
			return;
		}
		foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
		{
			for (int i = 0; i < groupBlock.joints.Count; i++)
			{
				ConfigurableJoint configurableJoint = groupBlock.joints[i];
				if (configurableJoint != null && oldToNew.TryGetValue(configurableJoint, out var value))
				{
					configurableJoint = (ConfigurableJoint)value;
					groupBlock.joints[i] = configurableJoint;
					groupBlock.chassis = configurableJoint.gameObject;
				}
			}
		}
	}

	public TileResultCode TurnAlongAnalogStick(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null)
		{
			string stringArg = Util.GetStringArg(args, 0, "L");
			float maxSpeed = eInfo.floatArg * Util.GetFloatArg(args, 1, 5f);
			TurnAlongAnalogStick(stringArg, maxSpeed);
		}
		return TileResultCode.True;
	}

	public override TileResultCode IsTapHoldingBlock(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (Block.goTouched != null && IsMainBlockInGroup())
		{
			foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
			{
				if (Block.goTouched == groupBlock.go)
				{
					Blocksworld.worldSessionHadBlockTap = true;
					return TileResultCode.True;
				}
			}
			for (int i = 0; i < treadsInfo.links.Count; i++)
			{
				TreadLink treadLink = treadsInfo.links[i];
				if (treadLink.collisionGo == Block.goTouched)
				{
					Blocksworld.worldSessionHadBlockTap = true;
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public override void RemoveBlockMaps()
	{
		base.RemoveBlockMaps();
		if (IsMainBlockInGroup())
		{
			DestroyTreads();
		}
	}

	private void RemoveTreadsBlockMaps()
	{
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			if (supportWheel.go != null)
			{
				BWSceneManager.RemoveChildBlockInstanceID(supportWheel.go);
			}
		}
		foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
		{
			BWSceneManager.RemoveChildBlockInstanceID(physicalLink.collisionGo);
		}
	}

	public override void IgnoreRaycasts(bool value)
	{
		Layer layer = (value ? Layer.IgnoreRaycast : Layer.Default);
		if (IsMainBlockInGroup())
		{
			foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
			{
				physicalLink.collisionGo.SetLayer(layer, recursive: true);
			}
			foreach (TankTreadsSupportWheel supportWheel in supportWheels)
			{
				if (supportWheel.go != null)
				{
					supportWheel.go.SetLayer(layer, recursive: true);
				}
			}
			if (buildModeColliderGo != null)
			{
				buildModeColliderGo.SetLayer(layer, recursive: true);
			}
		}
		if (group != null)
		{
			Block[] blocks = group.GetBlocks();
			foreach (Block block in blocks)
			{
				block.go.SetLayer(layer, recursive: true);
			}
		}
		else
		{
			base.IgnoreRaycasts(value);
		}
	}

	private void SetTreadMaterial()
	{
		if (treadMaterial != null)
		{
			UnityEngine.Object.Destroy(treadMaterial);
		}
		Material sharedMaterial = go.GetComponent<Renderer>().sharedMaterial;
		treadMaterial = new Material(sharedMaterial);
		bool flag = false;
		Mapping mapping = Materials.GetMapping(GetTexture());
		float num = 1f;
		if (mapping == Mapping.AllSidesTo4x1)
		{
			num = 0.25f;
		}
		if (treadsInfo.uValue != num)
		{
			flag = true;
			treadsInfo.uValue = num;
		}
		for (int i = 0; i < treadsInfo.links.Count; i++)
		{
			TreadLink treadLink = treadsInfo.links[i];
			GameObject visualGo = treadLink.visualGo;
			if (!(visualGo != null))
			{
				continue;
			}
			visualGo.GetComponent<Renderer>().sharedMaterial = treadMaterial;
			if (!flag)
			{
				continue;
			}
			MeshFilter component = visualGo.GetComponent<MeshFilter>();
			Mesh mesh = component.mesh;
			Vector2[] uv = mesh.uv;
			for (int j = 0; j < uv.Length; j++)
			{
				float num2 = uv[j][0];
				if (num2 > 0.01f)
				{
					uv[j][0] = num;
				}
			}
			mesh.uv = uv;
		}
	}

	public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		bool result = base.ScaleTo(scale, recalculateCollider, forceRescale);
		go.GetComponent<Renderer>().enabled = false;
		axlesDirty = true;
		return result;
	}

	public void TextureToBase(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		base.TextureTo(texture, normal, permanent, meshIndex, force);
		if (meshIndex == 0)
		{
			isInvisible = texture == "Invisible";
			go.GetComponent<Renderer>().enabled = false;
		}
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (group == null)
		{
			TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
			go.GetComponent<Renderer>().enabled = false;
			return result;
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (block is BlockTankTreadsWheel blockTankTreadsWheel)
			{
				blockTankTreadsWheel.TextureToBase(texture, normal, permanent, meshIndex, force);
			}
		}
		if (meshIndex == 0)
		{
			BlockTankTreadsWheel blockTankTreadsWheel2 = GetMainBlockInGroup() as BlockTankTreadsWheel;
			isInvisible = texture == "Invisible";
			ToggleTreadVisibility();
			blockTankTreadsWheel2.SetTreadMaterial();
			blockTankTreadsWheel2.go.GetComponent<Renderer>().enabled = false;
			go.GetComponent<Renderer>().enabled = false;
		}
		return TileResultCode.True;
	}

	private void ToggleTreadVisibility(bool skipPlay = false)
	{
		if (groupBlocks == null || (Blocksworld.CurrentState != State.Play && !skipPlay))
		{
			return;
		}
		for (int i = 0; i < groupBlocks.Count; i++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[i];
			List<Renderer> treadRenders = blockTankTreadsWheel.treadsInfo.TreadRenders;
			for (int j = 0; j < treadRenders.Count; j++)
			{
				treadRenders[j].enabled = !isInvisible;
			}
		}
	}

	public void PaintToBase(string paint, bool permanent, int meshIndex = 0)
	{
		base.PaintTo(paint, permanent, meshIndex);
		if (meshIndex == 0)
		{
			go.GetComponent<Renderer>().enabled = false;
		}
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (group == null)
		{
			TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
			go.GetComponent<Renderer>().enabled = false;
			return result;
		}
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (block is BlockTankTreadsWheel blockTankTreadsWheel)
			{
				blockTankTreadsWheel.PaintToBase(paint, permanent, meshIndex);
			}
		}
		if (meshIndex == 0)
		{
			BlockTankTreadsWheel blockTankTreadsWheel2 = GetMainBlockInGroup() as BlockTankTreadsWheel;
			blockTankTreadsWheel2.SetTreadMaterial();
			blockTankTreadsWheel2.go.GetComponent<Renderer>().enabled = false;
			go.GetComponent<Renderer>().enabled = false;
		}
		return TileResultCode.True;
	}

	public override void SetBlockGroup(BlockGroup group)
	{
		base.SetBlockGroup(group);
		if (!(group is TankTreadsBlockGroup))
		{
			return;
		}
		groupBlocks = new List<BlockTankTreadsWheel>();
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			groupBlocks.Add((BlockTankTreadsWheel)block);
		}
		if (IsMainBlockInGroup())
		{
			treadsInfo = new TreadsInfo();
			for (int j = 0; j < groupBlocks.Count; j++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[j];
				blockTankTreadsWheel.treadsInfo = treadsInfo;
			}
		}
		else if (tiles.Count > 2)
		{
			BWLog.Info("Too many tiles on non-main tank treads wheel");
			for (int num = tiles.Count - 1; num >= 2; num--)
			{
				tiles.RemoveAt(num);
			}
			tiles[1] = Block.EmptyTileRow();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		DestroyTreads();
	}

	private void DestroyBuildModeCollider()
	{
		if (buildModeColliderGo != null)
		{
			BWSceneManager.RemoveChildBlockInstanceID(buildModeColliderGo);
			MeshCollider component = buildModeColliderGo.GetComponent<MeshCollider>();
			UnityEngine.Object.Destroy(component.sharedMesh);
			UnityEngine.Object.Destroy(buildModeColliderGo);
			buildModeColliderGo = null;
		}
	}

	private void DestroyTreads()
	{
		if (!IsMainBlockInGroup())
		{
			return;
		}
		RemoveTreadsBlockMaps();
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			supportWheel.Destroy();
		}
		foreach (TreadLink link in treadsInfo.links)
		{
			link.Destroy();
		}
		supportWheels.Clear();
		treadsInfo.links.Clear();
		treadsInfo.physicalLinks.Clear();
		treadsInfo.gameObjectToTreadLink.Clear();
		treadsInfo.TreadRenders.Clear();
		treadsInfo.simplifiedHullPoints.Clear();
		DestroyBuildModeCollider();
		treadsDirty = true;
	}

	public override void Play()
	{
		base.Play();
		externalControlBlock = null;
		inPlayOrFrameCaptureMode = true;
		DestroyBuildModeCollider();
		treatAsVehicleStatus = -1;
		go.GetComponent<Renderer>().enabled = false;
		originalMaxDepenetrationVelocity = base.chunk.rb.maxDepenetrationVelocity;
		base.chunk.rb.maxDepenetrationVelocity = 1f;
		if (groupBlocks == null || groupBlocks.Count <= 0 || !IsMainBlockInGroup())
		{
			return;
		}
		treadsInfo.position = 0f;
		List<Block> list = ConnectionsOfType(2, directed: true);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		foreach (Block item2 in list)
		{
			if (!(item2 is BlockTankTreadsWheel item) || !groupBlocks.Contains(item))
			{
				hashSet.Add(item2.goT.parent);
			}
		}
		CreateSupportRigidBodies();
		treadsReferenceMass = 0.5f;
		treadsReferenceInertia = Vector3.one;
		Vector3 position = goT.position;
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			Chunk chunk = block.chunk;
			Rigidbody rb = chunk.rb;
			if (rb != null)
			{
				treadsReferenceMass += rb.mass;
				if (block == this)
				{
					treadsReferenceInertia += rb.inertiaTensor;
					continue;
				}
				float magnitude = (position - block.goT.position).magnitude;
				treadsReferenceInertia += Vector3.one * rb.mass * magnitude * magnitude;
			}
		}
		if (hashSet.Count == 0)
		{
			fakeChassis = new GameObject(go.name + " Fake Chassis");
			fakeChassis.transform.position = goT.position;
			Rigidbody rigidbody = fakeChassis.AddComponent<Rigidbody>();
			rigidbody.angularDrag = 2f;
			rigidbody.drag = 0.2f;
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigidbody.mass = treadsReferenceMass;
			for (int j = 0; j < groupBlocks.Count; j++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[j];
				blockTankTreadsWheel.fakeChassis = fakeChassis;
				blockTankTreadsWheel.CreateJoint(fakeChassis);
			}
			foreach (TankTreadsSupportWheel supportWheel in supportWheels)
			{
				CreateSupportWheelJoint(fakeChassis, supportWheel);
			}
		}
		else
		{
			foreach (Transform item3 in hashSet)
			{
				for (int k = 0; k < groupBlocks.Count; k++)
				{
					BlockTankTreadsWheel blockTankTreadsWheel2 = groupBlocks[k];
					blockTankTreadsWheel2.CreateJoint(item3.gameObject);
				}
				foreach (TankTreadsSupportWheel supportWheel2 in supportWheels)
				{
					CreateSupportWheelJoint(item3.gameObject, supportWheel2);
				}
			}
		}
		foreach (TankTreadsSupportWheel supportWheel3 in supportWheels)
		{
			if (supportWheel3.go != null)
			{
				supportWheel3.go.SetLayer(Layer.Default, recursive: true);
			}
		}
		foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
		{
			CreateSupportLinkJoints(physicalLink);
			physicalLink.collisionGo.SetLayer(Layer.Default, recursive: true);
		}
		for (int l = 0; l < groupBlocks.Count; l++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel3 = groupBlocks[l];
			Chunk chunk2 = blockTankTreadsWheel3.chunk;
			SupportWheelHelpForceBehaviour supportWheelHelpForceBehaviour = chunk2.go.AddComponent<SupportWheelHelpForceBehaviour>();
			supportWheelHelpForceBehaviour.giver = blockTankTreadsWheel3;
			blockTankTreadsWheel3.helpForceBehaviour = supportWheelHelpForceBehaviour;
		}
	}

	public override void RestoredMeshCollider()
	{
		base.RestoredMeshCollider();
		if (GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
		{
			blockTankTreadsWheel.ignoreCollisionsDirty = true;
		}
	}

	private void TurnOffInternalCollision()
	{
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		for (int i = 0; i < groupBlocks.Count; i++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[i];
			Collider component = blockTankTreadsWheel.go.GetComponent<Collider>();
			foreach (Block item in list)
			{
				if (blockTankTreadsWheel != item)
				{
					IgnoreCollision(item, component);
				}
			}
		}
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			if (!(supportWheel.go != null))
			{
				continue;
			}
			Collider component2 = supportWheel.go.GetComponent<Collider>();
			foreach (TankTreadsSupportWheel supportWheel2 in supportWheels)
			{
				if (supportWheel2 != supportWheel && supportWheel2.go != null)
				{
					Collider component3 = supportWheel2.go.GetComponent<Collider>();
					Physics.IgnoreCollision(component2, component3);
				}
			}
			foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
			{
				Collider component4 = physicalLink.collisionGo.GetComponent<Collider>();
				Physics.IgnoreCollision(component2, component4);
			}
			for (int j = 0; j < groupBlocks.Count; j++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel2 = groupBlocks[j];
				Collider component5 = blockTankTreadsWheel2.go.GetComponent<Collider>();
				Physics.IgnoreCollision(component2, component5);
			}
			foreach (Block item2 in list)
			{
				IgnoreCollision(item2, component2);
			}
		}
		foreach (TreadLink physicalLink2 in treadsInfo.physicalLinks)
		{
			Collider component6 = physicalLink2.collisionGo.GetComponent<Collider>();
			foreach (TreadLink physicalLink3 in treadsInfo.physicalLinks)
			{
				if (physicalLink3 != physicalLink2)
				{
					Collider component7 = physicalLink3.collisionGo.GetComponent<Collider>();
					Physics.IgnoreCollision(component6, component7);
				}
			}
			for (int k = 0; k < groupBlocks.Count; k++)
			{
				BlockTankTreadsWheel blockTankTreadsWheel3 = groupBlocks[k];
				Collider component8 = blockTankTreadsWheel3.go.GetComponent<Collider>();
				Physics.IgnoreCollision(component6, component8);
			}
			foreach (Block item3 in list)
			{
				IgnoreCollision(item3, component6);
			}
		}
	}

	private void IgnoreCollision(Block b, Collider collider)
	{
		Collider component = b.go.GetComponent<Collider>();
		if (component.enabled)
		{
			Physics.IgnoreCollision(collider, component);
			return;
		}
		Collider[] componentsInChildren = b.go.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider2 in array)
		{
			if (collider2.enabled)
			{
				Physics.IgnoreCollision(collider, collider2);
			}
		}
	}

	public override void Play2()
	{
		wheelMass = goT.parent.GetComponent<Rigidbody>().mass;
		float d = wheelMass;
		Block[] blocks = group.GetBlocks();
		for (int i = 0; i < joints.Count; i++)
		{
			ConfigurableJoint joint = joints[i];
			float num = 5f;
			float num2 = 0.5f;
			Block[] array = blocks;
			foreach (Block block in array)
			{
				num2 += block.GetMass();
			}
			BlockTankTreadsWheel blockTankTreadsWheel = GetMainBlockInGroup() as BlockTankTreadsWheel;
			foreach (TankTreadsSupportWheel supportWheel in blockTankTreadsWheel.supportWheels)
			{
				num2 += supportWheel.go.GetComponent<Rigidbody>().mass;
			}
			num2 += chassis.GetComponent<Rigidbody>().mass;
			num += num2;
			num *= 20f;
			SetSpring(joint, num, d);
		}
		Rigidbody component = chassis.GetComponent<Rigidbody>();
		if (component != null)
		{
			float num3 = treadsReferenceMass / component.mass;
			if (num3 > 1f)
			{
				float num4 = Mathf.Clamp(num3, 1f, 20f);
				component.mass *= num4;
			}
			float num5 = treadsReferenceInertia.magnitude / component.inertiaTensor.magnitude;
			if (num5 > 1f)
			{
				Vector3 inertiaTensor = component.inertiaTensor;
				float num6 = 1f / num5;
				try
				{
					component.inertiaTensor = num6 * inertiaTensor + (1f - num6) * treadsReferenceInertia;
				}
				catch
				{
					BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
				}
			}
		}
		scaleSpeed = 1f / GetRadius();
		if (IsMainBlockInGroup())
		{
			TurnOffInternalCollision();
		}
		if (treadMaterial != null && treadMaterial.shader == Shader.Find("Blocksworld/Invisible"))
		{
			isInvisible = true;
			ToggleTreadVisibility(skipPlay: true);
		}
	}

	public float GetRadius()
	{
		return 0.5f * GetScale().y;
	}

	private void CreateSupportRigidBodies()
	{
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			GameObject gameObject = supportWheel.go;
			if (!(gameObject.GetComponent<Rigidbody>() != null))
			{
				CapsuleCollider component = gameObject.GetComponent<CapsuleCollider>();
				component.material = go.GetComponent<Collider>().material;
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				}
				rigidbody.mass = 0.3f * component.height;
				rigidbody.useGravity = false;
				rigidbody.drag = 0.2f;
				rigidbody.angularDrag = 2f;
			}
		}
		float x = Scale().x;
		foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
		{
			float magnitude = (physicalLink.toTransform.position - physicalLink.fromTransform.position).magnitude;
			Rigidbody rigidbody2 = physicalLink.collisionGo.AddComponent<Rigidbody>();
			rigidbody2.mass = 0.2f * x * magnitude;
			rigidbody2.useGravity = false;
			rigidbody2.drag = 0.2f;
			rigidbody2.angularDrag = 2f;
		}
	}

	public Vector3 GetHelpForceAt(Rigidbody thisRb, Rigidbody otherRb, Vector3 pos, Vector3 relVel, bool fresh)
	{
		Vector3 position = goT.position;
		Vector3 lhs = pos - position;
		Vector3 normalized = Vector3.Cross(lhs, goT.right).normalized;
		Vector3 vector = ((!(otherRb == null)) ? otherRb.velocity : Vector3.zero);
		Vector3 lhs2 = treadsInfo.speed * normalized - thisRb.velocity + vector;
		Vector3 vector2 = Vector3.Dot(lhs2, normalized) * normalized;
		float num = Mathf.Abs(treadsInfo.speed);
		if (vector2.sqrMagnitude > num * num)
		{
			vector2 = vector2.normalized * num;
		}
		if (!fresh)
		{
			float num2 = 1f + 0.1f * helpForceErrorSum.magnitude;
			vector2 /= num2;
			helpForceErrorSum += vector2;
			helpForceErrorSum *= 0.95f;
		}
		else
		{
			helpForceErrorSum = Vector3.zero;
		}
		return 0.3f * thisRb.mass * vector2;
	}

	public Vector3 GetForcePoint(Vector3 contactPoint)
	{
		return goT.position;
	}

	public bool IsHelpForceActive()
	{
		if (!broken && treadsInfo.mode == TreadsMode.Drive)
		{
			return Mathf.Abs(treadsInfo.speed) > 0.01f;
		}
		return false;
	}

	public void DestroyJoint(ConfigurableJoint joint)
	{
		joints.Remove(joint);
		UnityEngine.Object.Destroy(joint);
	}

	private void CreateSupportLinkJoints(TreadLink link)
	{
		GameObject collisionGo = link.collisionGo;
		ConfigurableJoint configurableJoint = collisionGo.AddComponent<ConfigurableJoint>();
		ConfigurableJoint configurableJoint2 = collisionGo.AddComponent<ConfigurableJoint>();
		link.joint1 = configurableJoint;
		link.joint2 = configurableJoint2;
		Rigidbody component = link.fromTransform.gameObject.GetComponent<Rigidbody>();
		if (component == null)
		{
			component = chassis.GetComponent<Rigidbody>();
		}
		Rigidbody component2 = link.toTransform.gameObject.GetComponent<Rigidbody>();
		if (component2 == null)
		{
			component2 = chassis.GetComponent<Rigidbody>();
		}
		if (component != null && component2 != null)
		{
			configurableJoint.connectedBody = component;
			configurableJoint2.connectedBody = component2;
			Transform transform = collisionGo.transform;
			Vector3 forward = transform.forward;
			ConfigurableJoint[] array = new ConfigurableJoint[2] { configurableJoint, configurableJoint2 };
			for (int i = 0; i < array.Length; i++)
			{
				ConfigurableJoint configurableJoint3 = array[i];
				int num = ((i != 0) ? 1 : (-1));
				Vector3 anchor = transform.worldToLocalMatrix.MultiplyPoint(transform.position + num * forward * link.length * 0.5f);
				configurableJoint3.anchor = anchor;
				configurableJoint3.axis = Vector3.right;
				configurableJoint3.xMotion = ConfigurableJointMotion.Locked;
				configurableJoint3.yMotion = ConfigurableJointMotion.Locked;
				configurableJoint3.zMotion = ConfigurableJointMotion.Locked;
				configurableJoint3.angularXMotion = ConfigurableJointMotion.Free;
				configurableJoint3.angularYMotion = ConfigurableJointMotion.Locked;
				configurableJoint3.angularZMotion = ConfigurableJointMotion.Locked;
				configurableJoint3.angularYZLimitSpring = new SoftJointLimitSpring
				{
					spring = float.PositiveInfinity,
					damper = float.PositiveInfinity
				};
			}
		}
	}

	private void CreateSupportWheelJoint(GameObject chassis, TankTreadsSupportWheel wheel)
	{
		ConfigurableJoint configurableJoint = (wheel.joint = chassis.AddComponent<ConfigurableJoint>());
		GameObject gameObject = wheel.go;
		Rigidbody rigidbody = (configurableJoint.connectedBody = gameObject.GetComponent<Rigidbody>());
		Transform transform = gameObject.transform;
		configurableJoint.anchor = transform.position - chassis.transform.position;
		configurableJoint.axis = goT.right;
		configurableJoint.xMotion = ConfigurableJointMotion.Limited;
		configurableJoint.yMotion = ConfigurableJointMotion.Limited;
		configurableJoint.zMotion = ConfigurableJointMotion.Limited;
		configurableJoint.linearLimit = new SoftJointLimit
		{
			limit = 0f,
			bounciness = 0f
		};
		configurableJoint.linearLimitSpring = new SoftJointLimitSpring
		{
			spring = 30f * rigidbody.mass,
			damper = 2f * rigidbody.mass
		};
		configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularYZLimitSpring = new SoftJointLimitSpring
		{
			spring = float.PositiveInfinity,
			damper = float.PositiveInfinity
		};
	}

	private bool ShouldBumpSuspensionJoint()
	{
		float num = float.PositiveInfinity;
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (block is BlockTankTreadsWheel { goT: var transform })
			{
				num = Mathf.Min(Vector3.Dot(transform.up, transform.position + transform.up * GetRadius()), num);
			}
		}
		return Vector3.Dot(goT.up, goT.position) < num;
	}

	public ConfigurableJoint CreateJoint(GameObject chassis)
	{
		ConfigurableJoint configurableJoint = chassis.AddComponent<ConfigurableJoint>();
		configurableJoint.connectedBody = goT.parent.GetComponent<Rigidbody>();
		bool flag = ShouldBumpSuspensionJoint();
		Vector3 up = goT.up;
		if (flag)
		{
			goT.parent.Translate(-0.075f * up);
		}
		configurableJoint.anchor = goT.position - chassis.transform.position;
		if (flag)
		{
			goT.parent.Translate(0.075f * up);
		}
		configurableJoint.axis = goT.right;
		configurableJoint.xMotion = ConfigurableJointMotion.Locked;
		configurableJoint.yMotion = ConfigurableJointMotion.Limited;
		configurableJoint.zMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
		joints.Add(configurableJoint);
		this.chassis = chassis;
		return configurableJoint;
	}

	private void SetSpring(ConfigurableJoint joint, float force, float d)
	{
		if (joint != null)
		{
			joint.linearLimitSpring = new SoftJointLimitSpring
			{
				spring = Mathf.Max(1f, force),
				damper = d
			};
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		go.SetLayer(Layer.Default, recursive: true);
		inPlayOrFrameCaptureMode = false;
		helpForceBehaviour = null;
		foreach (ConfigurableJoint joint in joints)
		{
			UnityEngine.Object.Destroy(joint);
		}
		joints.Clear();
		if (fakeChassis != null)
		{
			DestroyFakeChassis();
		}
		if (IsMainBlockInGroup())
		{
			DestroyTreads();
			treadsInfo.position = 0f;
		}
		PlayLoopSound(play: false, GetLoopClip());
		base.Stop(resetBlock);
		go.GetComponent<Renderer>().enabled = false;
	}

	public override void Pause()
	{
		if (fakeChassis != null)
		{
			pausedVelocityAxle = fakeChassis.GetComponent<Rigidbody>().velocity;
			pausedAngularVelocityAxle = fakeChassis.GetComponent<Rigidbody>().angularVelocity;
			fakeChassis.GetComponent<Rigidbody>().isKinematic = true;
			if (IsMainBlockInGroup())
			{
				foreach (TankTreadsSupportWheel supportWheel in supportWheels)
				{
					Rigidbody component = supportWheel.go.GetComponent<Rigidbody>();
					if (component != null)
					{
						supportWheel.pausedVelocity = component.velocity;
						supportWheel.pausedAngularVelocity = component.angularVelocity;
					}
				}
				foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
				{
					Rigidbody component2 = physicalLink.collisionGo.GetComponent<Rigidbody>();
					if (component2 != null)
					{
						physicalLink.pausedVelocity = component2.velocity;
						physicalLink.pausedAngularVelocity = component2.angularVelocity;
					}
				}
			}
		}
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Resume()
	{
		if (!(fakeChassis != null))
		{
			return;
		}
		fakeChassis.GetComponent<Rigidbody>().isKinematic = false;
		fakeChassis.GetComponent<Rigidbody>().velocity = pausedVelocityAxle;
		fakeChassis.GetComponent<Rigidbody>().angularVelocity = pausedAngularVelocityAxle;
		if (!IsMainBlockInGroup())
		{
			return;
		}
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			Rigidbody component = supportWheel.go.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = false;
				component.velocity = supportWheel.pausedVelocity;
				component.angularVelocity = supportWheel.pausedAngularVelocity;
			}
		}
		foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
		{
			Rigidbody component2 = physicalLink.collisionGo.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.isKinematic = false;
				component2.velocity = physicalLink.pausedVelocity;
				component2.angularVelocity = physicalLink.pausedAngularVelocity;
			}
		}
	}

	public override void ResetFrame()
	{
		if (!IsMainBlockInGroup())
		{
			return;
		}
		float driveSpeedTarget = treadsInfo.driveSpeedTarget;
		treadsInfo.driveSpeedTarget = 0f;
		if (driveSpeedTarget != 0f)
		{
			return;
		}
		float num = 0f;
		float f = 0f;
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < groupBlocks.Count; i++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[i];
			float num3 = blockTankTreadsWheel.GetAngularVelocity() / blockTankTreadsWheel.scaleSpeed;
			num2 += num3 / (float)groupBlocks.Count;
			if (blockTankTreadsWheel.onGround > 0f)
			{
				flag = true;
				if (Mathf.Abs(num) < Mathf.Abs(num3))
				{
					num = num3;
				}
			}
			else if (Mathf.Abs(f) < Mathf.Abs(num3))
			{
				f = num3;
			}
		}
		if (flag)
		{
			treadsInfo.rollSpeedTarget = num;
			treadsInfo.mode = TreadsMode.Rolling;
		}
		else
		{
			treadsInfo.spinSpeedTarget = 0.99f * num2;
			treadsInfo.mode = TreadsMode.Spinning;
		}
	}

	private void UpdateBlocksInModelSetIfNecessary()
	{
		if (blocksInModelSet == null)
		{
			UpdateConnectedCache();
			blocksInModelSet = new HashSet<Block>(Block.connectedCache[this]);
		}
	}

	public void Drive(float f)
	{
		treadsInfo.driveSpeedTarget += f;
		treadsInfo.mode = TreadsMode.Drive;
		engineLoopOn = !broken;
		idleEngineCounter = 0;
		engineIncreasingPitch = true;
	}

	public TileResultCode Drive(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (externalControlBlock == null)
		{
			float f = (float)args[0] * eInfo.floatArg;
			Drive(f);
		}
		return TileResultCode.True;
	}

	public TileResultCode IsDrivingSensor(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (!((float)args[0] <= 0f))
		{
			return IsDriving();
		}
		return IsBreaking();
	}

	public TileResultCode IsDriving()
	{
		Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * goT.right;
		if (!(goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f) && !(Vector3.Dot(goT.parent.GetComponent<Rigidbody>().velocity, rhs) <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsBreaking()
	{
		Vector3 rhs = Quaternion.Euler(0f, -90f, 0f) * goT.right;
		if (!(goT.parent.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.25f) && !(Vector3.Dot(goT.parent.GetComponent<Rigidbody>().velocity, rhs) >= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	private void SetMotor(float speed)
	{
		if (isTreasure)
		{
			return;
		}
		float x = speed * scaleSpeed;
		bool flag = treadsInfo.mode == TreadsMode.Drive || onGround <= 0f;
		float num = Mathf.Clamp(wheelMass * 20f / (scaleSpeed * scaleSpeed), 20f, 1000f);
		bool flag2 = false;
		for (int i = 0; i < joints.Count; i++)
		{
			ConfigurableJoint configurableJoint = joints[i];
			if (configurableJoint != null && configurableJoint.connectedBody != null)
			{
				configurableJoint.targetAngularVelocity = new Vector3(x, 0f, 0f);
				drive.positionDamper = ((!flag) ? 0f : num);
				drive.maximumForce = num;
				configurableJoint.angularXDrive = drive;
				Rigidbody connectedBody = configurableJoint.connectedBody;
				if (connectedBody.IsSleeping())
				{
					connectedBody.WakeUp();
				}
			}
			else
			{
				flag2 = true;
			}
		}
		if (flag2 && IsMainBlockInGroup())
		{
			Blocksworld.AddFixedUpdateCommand(new DelegateCommand(delegate
			{
				Break(goT.position, Vector3.zero, Vector3.zero);
			}));
		}
	}

	private void UpdateEngineSound()
	{
		if (!Sound.sfxEnabled || isTreasure || vanished)
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
		else
		{
			if (goT.parent == null)
			{
				return;
			}
			Rigidbody component = goT.parent.GetComponent<Rigidbody>();
			if (component == null)
			{
				return;
			}
			int num = 4;
			bool flag = engineIntervalCounter % num == 0;
			engineIntervalCounter++;
			float num2 = Mathf.Abs(Vector3.Dot(component.angularVelocity, goT.right));
			float num3 = num2 / 10f;
			float max = pitchScale;
			float num4 = 0.1f;
			float num5 = 0.6f * pitchScale;
			float num6 = num5 + num3;
			float num7 = 0.02f;
			if (num6 < engineLoopPitch)
			{
				num7 *= -1f;
			}
			float num8 = ((!engineIncreasingPitch) ? (-0.07f) : num7);
			engineLoopPitch = Mathf.Clamp(engineLoopPitch + num8, (!engineLoopOn) ? num4 : num5, max);
			float num9 = ((!engineLoopOn) ? (-0.01f) : 0.1f);
			num9 *= volScale;
			engineLoopVolume = Mathf.Clamp(engineLoopVolume + num9, 0f, 0.2f * volScale);
			if (engineLoopOn && num8 < 0f)
			{
				idleEngineCounter++;
				if (idleEngineCounter > 100)
				{
					engineLoopOn = false;
				}
			}
			float num10 = engineLoopVolume;
			if (flag)
			{
				if (Sound.BlockIsMuted(this))
				{
					num10 = 0f;
				}
				float num11 = 1f;
				if (num10 > 0.01f)
				{
					float num12 = 0.1f;
					num11 = num12 * 2f * (Mathf.PerlinNoise(Time.time, 0f) - 0.5f) + 1f;
				}
				float pitch = num11 * engineLoopPitch;
				PlayLoopSound(num10 > 0.01f, GetLoopClip(), num10, null, pitch);
			}
			UpdateWithinWaterLPFilter();
		}
	}

	public override void Update()
	{
		base.Update();
		if (IsMainBlockInGroup())
		{
			if (Blocksworld.CurrentState == State.Build && treadsInfo != null && treadsDirty)
			{
				CreateTreads();
				treadsDirty = false;
			}
			if (ignoreCollisionsDirty)
			{
				TurnOffInternalCollision();
				ignoreCollisionsDirty = false;
			}
			UpdateTreads();
		}
		if (!axlesDirty)
		{
			return;
		}
		BlockTankTreadsWheel blockTankTreadsWheel = GetMainBlockInGroup() as BlockTankTreadsWheel;
		Vector3 position = goT.position;
		bool flag = false;
		bool flag2 = false;
		foreach (Block connection in blockTankTreadsWheel.connections)
		{
			if (!(connection.go == null))
			{
				Vector3 position2 = connection.goT.position;
				Vector3 lhs = position2 - position;
				float num = Vector3.Dot(lhs, goT.right);
				flag = flag || num < 0f;
				flag2 = flag2 || num > 0f;
			}
		}
		hideAxleN.GetComponent<Renderer>().enabled = flag;
		hideAxleP.GetComponent<Renderer>().enabled = flag2;
		axlesDirty = false;
	}

	public float GetAngularVelocity()
	{
		Rigidbody rb = chunk.rb;
		if (rb != null)
		{
			return Vector3.Dot(rb.angularVelocity, goT.right);
		}
		return 0f;
	}

	public override TileResultCode Freeze(bool informModelBlocks)
	{
		if (!didFix && IsMainBlockInGroup())
		{
			bool flag = ContainsTileWithPredicateInPlayMode(Block.predicateUnfreeze);
			SetMotor(0f);
			foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
			{
				Rigidbody component = physicalLink.collisionGo.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = true;
				}
				if (!flag)
				{
					UnityEngine.Object.Destroy(physicalLink.helpBehaviour);
				}
			}
			if (!flag)
			{
				foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
				{
					UnityEngine.Object.Destroy(groupBlock.helpForceBehaviour);
				}
			}
			foreach (TankTreadsSupportWheel supportWheel in supportWheels)
			{
				if (supportWheel.go != null)
				{
					Rigidbody component2 = supportWheel.go.GetComponent<Rigidbody>();
					if (component2 != null)
					{
						component2.isKinematic = true;
					}
				}
			}
		}
		return base.Freeze(informModelBlocks);
	}

	public override void Unfreeze()
	{
		if (didFix && IsMainBlockInGroup())
		{
			foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
			{
				Rigidbody component = physicalLink.collisionGo.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = false;
					component.WakeUp();
				}
			}
			foreach (TankTreadsSupportWheel supportWheel in supportWheels)
			{
				if (supportWheel.go != null)
				{
					Rigidbody component2 = supportWheel.go.GetComponent<Rigidbody>();
					if (component2 != null)
					{
						component2.isKinematic = false;
						component2.WakeUp();
					}
				}
			}
		}
		base.Unfreeze();
	}

	public void FixedUpdateDriveAndTurn()
	{
		bool flag = IsMainBlockInGroup();
		if (flag)
		{
			UpdateEngineSound();
		}
		if (isTreasure || broken || vanished || didFix)
		{
			return;
		}
		if (CollisionManager.bumping.Contains(goT.parent.gameObject))
		{
			onGround = 0.3f;
		}
		else
		{
			onGround -= Blocksworld.fixedDeltaTime;
		}
		if (joints.Count == 0)
		{
			return;
		}
		Rigidbody rb = chunk.rb;
		SetMotor(0f - treadsInfo.speed);
		if (flag)
		{
			switch (treadsInfo.mode)
			{
			case TreadsMode.Spinning:
				treadsInfo.speed += (treadsInfo.spinSpeedTarget - treadsInfo.speed) / 40f;
				treadsInfo.speed *= 0.97f;
				break;
			case TreadsMode.Rolling:
				treadsInfo.speed += (treadsInfo.rollSpeedTarget - treadsInfo.speed) / 10f;
				break;
			case TreadsMode.Drive:
				treadsInfo.speed += (treadsInfo.driveSpeedTarget - treadsInfo.speed) / 40f;
				break;
			}
			treadsInfo.position += treadsInfo.speed * Blocksworld.fixedDeltaTime;
		}
		if (!flag || !(rb != null) || rb.isKinematic || !(onGround > 0f) || treadsInfo.mode != TreadsMode.Drive || !(fakeChassis == null))
		{
			return;
		}
		bool flag2 = true;
		for (int i = 0; i < joints.Count; i++)
		{
			ConfigurableJoint configurableJoint = joints[i];
			if (configurableJoint != null)
			{
				Rigidbody connectedBody = configurableJoint.connectedBody;
				Rigidbody component = configurableJoint.GetComponent<Rigidbody>();
				if (connectedBody == null || connectedBody.isKinematic || component == null || component.isKinematic)
				{
					flag2 = false;
					break;
				}
			}
		}
		if (flag2)
		{
			float num = 5f;
			float num2 = treadsInfo.speed * num;
			Vector3 vec = ((!(camHintDir.sqrMagnitude > 0.1f)) ? (Quaternion.Euler(0f, -90f, 0f) * goT.right * Mathf.Clamp(num2, -10f * num, 10f * num)) : (camHintDir * Mathf.Min(10f * num, Mathf.Abs(num2))));
			vec = Util.ProjectOntoPlane(vec, Vector3.up);
			Blocksworld.blocksworldCamera.AddForceDirectionHint(chunk, vec);
			camHintDir = Vector3.zero;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Blocksworld.playFixedUpdateCounter == 0 && chassis != null)
		{
			Rigidbody component = chassis.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.mass = Mathf.Max(2f, component.mass);
			}
		}
		if (externalControlBlock == null)
		{
			FixedUpdateDriveAndTurn();
		}
		if (Blocksworld.playFixedUpdateCounter == 10)
		{
			chunk.rb.maxDepenetrationVelocity = originalMaxDepenetrationVelocity;
		}
	}

	private void UpdateTreads()
	{
		if (didFix || broken)
		{
			return;
		}
		foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
		{
			if (groupBlock.broken)
			{
				broken = true;
				return;
			}
		}
		Vector3 right = goT.right;
		Vector3 vector = treadsUp;
		if (inPlayOrFrameCaptureMode && chassis != null)
		{
			vector = chassis.transform.TransformDirection(vector);
		}
		Vector3 forward = Vector3.Cross(right, vector);
		List<TreadLink> links = treadsInfo.links;
		int count = links.Count;
		if (count > 0)
		{
			Vector3 vector2 = links[0].FromToWorld(right, vector, forward);
			for (int i = 0; i < count; i++)
			{
				TreadLink treadLink = links[i];
				Vector3 vector3 = treadLink.ToToWorld(right, vector, forward);
				Transform visualGoT = treadLink.visualGoT;
				visualGoT.position = 0.5f * (vector2 + vector3);
				Vector3 vector4 = vector3 - vector2;
				float magnitude = vector4.magnitude;
				Vector3 vector5 = vector4 / magnitude;
				Vector3 upwards = Vector3.Cross(vector5, right);
				visualGoT.rotation = Quaternion.LookRotation(vector5, upwards);
				float z = magnitude / treadLink.length;
				visualGoT.localScale = new Vector3(1f, 1f, z);
				vector2 = vector3;
			}
		}
		if (treadMaterial != null)
		{
			float num = (0f - treadsInfo.position) * uvPerUnit;
			float num2 = Mathf.Sign(num);
			num = num2 * Mathf.Repeat(num * num2, 2f);
			string propertyName = "_VOffset";
			if (treadMaterial.HasProperty(propertyName))
			{
				treadMaterial.SetFloat(propertyName, num);
			}
		}
	}

	private void DestroyFakeChassis()
	{
		if (fakeChassis != null)
		{
			UnityEngine.Object.Destroy(fakeChassis);
			fakeChassis = null;
		}
	}

	public override void BecameTreasure()
	{
		base.BecameTreasure();
		DestroyFakeChassis();
		if (!IsMainBlockInGroup())
		{
			return;
		}
		foreach (TankTreadsSupportWheel supportWheel in supportWheels)
		{
			supportWheel.go.transform.parent = goT;
			UnityEngine.Object.Destroy(supportWheel.joint);
			UnityEngine.Object.Destroy(supportWheel.go.GetComponent<Rigidbody>());
			supportWheel.go.GetComponent<Collider>().isTrigger = true;
		}
		foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
		{
			physicalLink.collisionGo.transform.parent = goT;
			UnityEngine.Object.Destroy(physicalLink.joint1);
			UnityEngine.Object.Destroy(physicalLink.joint2);
			UnityEngine.Object.Destroy(physicalLink.collisionGo.GetComponent<Rigidbody>());
			physicalLink.collisionGo.GetComponent<Collider>().isTrigger = true;
		}
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return TreatAsVehicleLikeBlockWithStatus(ref treatAsVehicleStatus);
	}

	public override bool CanScaleUpwards()
	{
		return false;
	}

	public override bool TBoxScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
	{
		Vector3 vector = Scale();
		scale = Util.Round(scale);
		if (Mathf.Abs(scale.y - scale.z) > 0.01f)
		{
			float num = Mathf.Abs(scale.y);
			scale.Set(Mathf.Abs(scale.x), num, num);
		}
		Vector3 vector2 = scale - vector;
		if (vector2.magnitude > 0.01f)
		{
			if (Mathf.Abs(vector2.x) > 0.01f && groupBlocks != null)
			{
				foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
				{
					Vector3 scale2 = groupBlock.Scale();
					scale2.x += vector2.x;
					groupBlock.ScaleTo(scale2);
				}
			}
			ScaleTo(scale, recalculateCollider, forceRescale);
			if (GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
			{
				blockTankTreadsWheel.CreateTreads(shapeOnly: true);
				blockTankTreadsWheel.treadsDirty = true;
			}
		}
		return true;
	}

	public override bool MoveTo(Vector3 pos)
	{
		axlesDirty = true;
		return base.MoveTo(pos);
	}

	public override bool TBoxMoveTo(Vector3 pos, bool forced = false)
	{
		Vector3 position = GetPosition();
		Vector3 rhs = pos - position;
		Vector3 vector = goT.right;
		if (forced && rhs.sqrMagnitude > 0.01f)
		{
			vector = rhs.normalized;
		}
		float num = Vector3.Dot(vector, rhs);
		if (Mathf.Abs(num) > 0.01f && groupBlocks != null)
		{
			foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
			{
				if (groupBlock != this)
				{
					Vector3 position2 = groupBlock.GetPosition();
					position2 += num * vector;
					groupBlock.MoveTo(position2);
				}
			}
		}
		bool result = base.MoveTo(pos);
		if (GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
		{
			blockTankTreadsWheel.CreateTreads(shapeOnly: true);
			blockTankTreadsWheel.treadsDirty = true;
		}
		return result;
	}

	public override void OnBlockGroupReconstructed()
	{
		base.OnBlockGroupReconstructed();
		if (IsMainBlockInGroup())
		{
			CreateTreads();
			treadsDirty = true;
		}
	}

	public override void BunchMoved()
	{
		if (GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
		{
			blockTankTreadsWheel.CreateTreads(shapeOnly: true);
			blockTankTreadsWheel.treadsDirty = true;
		}
	}

	public override void BunchRotated()
	{
		if (GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
		{
			blockTankTreadsWheel.CreateTreads(shapeOnly: true);
			blockTankTreadsWheel.treadsDirty = true;
		}
	}

	public override bool RotateTo(Quaternion rot)
	{
		axlesDirty = true;
		return base.RotateTo(rot);
	}

	public override bool TBoxRotateTo(Quaternion rot)
	{
		Quaternion rotation = GetRotation();
		float f = Quaternion.Angle(rot, rotation);
		bool flag = Mathf.Abs(f) > 5f;
		if (flag)
		{
			Quaternion quaternion = rot * Quaternion.Inverse(rotation);
			Block[] blocks = group.GetBlocks();
			foreach (Block block in blocks)
			{
				if (block != this)
				{
					block.RotateTo(quaternion * block.GetRotation());
				}
			}
		}
		bool result = RotateTo(rot);
		if (flag)
		{
			RestoreLocalCoords(localCoords);
		}
		return result;
	}

	public void CreateTreads(bool shapeOnly = false, bool parentIsBlock = false)
	{
		string currentKey = null;
		if (shapeOnly && TryReuseSATVolumes(ref currentKey))
		{
			return;
		}
		if (cachedTreadsMeshes != null && treadsInfo.links.Count > 0)
		{
			if (currentKey == null)
			{
				currentKey = TankTreadsSATMeshesInfo.GetKey(this);
			}
			if (currentKey == cachedTreadsKey)
			{
				Vector3 vector = goT.position - cachedTreadsMeshes.position;
				if (vector.magnitude < 0.0001f)
				{
					return;
				}
				bool flag = false;
				foreach (TreadLink physicalLink in treadsInfo.physicalLinks)
				{
					Vector3 vector2 = physicalLink.collisionGo.transform.position + vector;
					if (Mathf.Abs(Vector3.Dot(vector2 - goT.position, goT.right)) > 0.25f)
					{
						flag = true;
						break;
					}
				}
				foreach (TankTreadsSupportWheel supportWheel in supportWheels)
				{
					Vector3 vector3 = supportWheel.go.transform.position + vector;
					if (Mathf.Abs(Vector3.Dot(vector3 - goT.position, goT.right)) > 0.25f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (TreadLink physicalLink2 in treadsInfo.physicalLinks)
					{
						physicalLink2.collisionGo.transform.position += vector;
					}
					foreach (TankTreadsSupportWheel supportWheel2 in supportWheels)
					{
						supportWheel2.go.transform.position += vector;
					}
					cachedTreadsMeshes.position = goT.position;
					return;
				}
			}
		}
		Vector3 forward = goT.forward;
		Vector3 up = goT.up;
		List<Vector2> list = new List<Vector2>();
		List<Vector3> list2 = new List<Vector3>();
		List<Transform> list3 = new List<Transform>();
		List<BlockTankTreadsWheel> list4 = new List<BlockTankTreadsWheel>();
		Vector2 zero = Vector2.zero;
		Vector2 vector4 = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector5 = new Vector2(float.MinValue, float.MinValue);
		foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
		{
			Vector3 position = groupBlock.goT.position;
			Vector2 vector6 = new Vector2(Vector3.Dot(forward, position), Vector3.Dot(up, position));
			zero += vector6 / groupBlocks.Count;
			vector4.x = Mathf.Min(vector6.x - 0.5f, vector4.x);
			vector4.y = Mathf.Min(vector6.y - 0.5f, vector4.y);
			vector5.x = Mathf.Max(vector6.x + 0.5f, vector5.x);
			vector5.y = Mathf.Max(vector6.y + 0.5f, vector5.y);
		}
		float num = vector5.x - vector4.x;
		float num2 = vector5.y - vector4.y;
		float num3 = 0.03f;
		for (int i = 0; i < groupBlocks.Count; i++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[i];
			Vector3 position2 = blockTankTreadsWheel.GetPosition();
			float num4 = blockTankTreadsWheel.Scale().y * 0.5f;
			int num5 = Mathf.Clamp(Mathf.RoundToInt(num4 * 12f), 8, 20);
			float num6 = (float)Math.PI * 2f / (float)num5;
			Transform item = groupBlocks[i].goT;
			for (int j = 0; j < num5; j++)
			{
				Vector2 item2 = default(Vector2);
				float f = num6 * (float)j;
				Vector3 rhs = position2 + num4 * forward * Mathf.Sin(f) + num4 * up * Mathf.Cos(f);
				item2.x = Vector3.Dot(forward, rhs);
				item2.y = Vector3.Dot(up, rhs);
				Vector3 vector7 = Vector3.Cross(forward, up);
				float num7 = Vector3.Dot(vector7, rhs);
				float num8 = num3 * (1f - Mathf.Abs(item2.y - zero.y) / (num2 * 0.5f));
				float num9 = num3 * (1f - Mathf.Abs(item2.x - zero.x) / (num * 0.5f));
				float num10 = Mathf.Sign(item2.x - zero.x);
				float num11 = Mathf.Sign(item2.y - zero.y);
				item2.x += num10 * num8;
				item2.y += num11 * num9;
				rhs = item2.x * forward + item2.y * up + num7 * vector7;
				list2.Add(rhs);
				list.Add(item2);
				list3.Add(item);
				list4.Add(blockTankTreadsWheel);
			}
		}
		DestroyTreads();
		ComputeSimplifiedHullPoints();
		if (!shapeOnly)
		{
			List<int> indices = ComputeConvexHull(list);
			uvPerUnit = 1f / Scale().x;
			AddTreadLinks(indices, list2, list3, list4);
			SetTreadMaterial();
		}
		CreateBuildModeCollider();
		UpdateSATVolumes();
		if (!shapeOnly)
		{
			cachedTreadsKey = TankTreadsSATMeshesInfo.GetKey(this);
			cachedTreadsMeshes = new TankTreadsSATMeshesInfo
			{
				position = goT.position
			};
		}
		if (!parentIsBlock)
		{
			return;
		}
		UpdateTreads();
		Transform parent = goT;
		foreach (TankTreadsSupportWheel supportWheel3 in supportWheels)
		{
			supportWheel3.go.transform.parent = parent;
		}
		foreach (TreadLink link in treadsInfo.links)
		{
			link.visualGoT.parent = parent;
			if (link.collisionGo != null)
			{
				link.collisionGo.transform.parent = parent;
			}
		}
	}

	private void ComputeSimplifiedHullPoints()
	{
		Vector3 forward = goT.forward;
		Vector3 up = goT.up;
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		for (int i = 0; i < groupBlocks.Count; i++)
		{
			BlockTankTreadsWheel blockTankTreadsWheel = groupBlocks[i];
			Vector3 position = blockTankTreadsWheel.GetPosition();
			float num = blockTankTreadsWheel.Scale().y * 0.5f;
			for (int j = -1; j < 2; j += 2)
			{
				for (int k = -1; k < 2; k += 2)
				{
					Vector2 item = default(Vector2);
					Vector3 vector = position + (num - 0.1f) * forward * j + (num - 0.1f) * up * k;
					list.Add(vector);
					item.x = Vector3.Dot(forward, vector);
					item.y = Vector3.Dot(up, vector);
					list2.Add(item);
				}
			}
		}
		List<int> list3 = ComputeConvexHull(list2);
		for (int l = 0; l < list3.Count; l++)
		{
			int index = list3[l];
			treadsInfo.simplifiedHullPoints.Add(list[index]);
		}
	}

	private Vector3 GetTreadsUp(int fromIndex, int toIndex, List<Vector3> worldPoints)
	{
		Vector3 vector = worldPoints[fromIndex];
		Vector3 vector2 = worldPoints[toIndex];
		Vector3 normalized = (vector2 - vector).normalized;
		return Vector3.Cross(normalized, goT.right);
	}

	private Vector3 BumpPointIfNecessary(Vector3 point, BlockTankTreadsWheel wheel)
	{
		if (wheel.ShouldBumpSuspensionJoint())
		{
			Transform transform = wheel.goT;
			Vector3 vector = -transform.up * 0.075f;
			float magnitude = (transform.position - point).magnitude;
			Vector3 vector2 = wheel.goT.position + vector;
			float magnitude2 = (vector2 - point).magnitude;
			if (magnitude2 > magnitude)
			{
				point += vector;
			}
		}
		return point;
	}

	private void AddTreadLinks(List<int> indices, List<Vector3> worldPoints, List<Transform> transforms, List<BlockTankTreadsWheel> wheels)
	{
		float num = 0f;
		for (int i = 0; i < indices.Count; i++)
		{
			int index = indices[i];
			int index2 = indices[(i + 1) % indices.Count];
			num += (worldPoints[index] - worldPoints[index2]).magnitude;
		}
		float num2 = Mathf.Round(num * uvPerUnit);
		float uvFactor = 1f;
		if (num2 > 1f)
		{
			uvFactor = num2 / (num * uvPerUnit);
		}
		float num3 = 0f;
		Vector3 right = goT.right;
		Vector3 vector = Vector3.up;
		float f = Vector3.Dot(right, Vector3.up);
		if (Mathf.Abs(f) > 0.1f)
		{
			vector = Vector3.right;
		}
		treadsUp = vector;
		Vector3 vector2 = Vector3.Cross(right, vector);
		for (int j = 0; j < indices.Count; j++)
		{
			int num4 = indices[j];
			int num5 = indices[(j + 1) % indices.Count];
			int toIndex = indices[(j + 2) % indices.Count];
			int fromIndex = indices[(j + indices.Count - 1) % indices.Count];
			Vector3 vector3 = worldPoints[num4];
			Vector3 vector4 = worldPoints[num5];
			Transform transform = transforms[num4];
			Transform transform2 = transforms[num5];
			Vector3 vector5 = vector4 - vector3;
			float magnitude = vector5.magnitude;
			Vector3 normalized = vector5.normalized;
			Vector3 treadUp = Vector3.Cross(normalized, goT.right);
			Vector3 prevTreadUp = GetTreadsUp(fromIndex, num4, worldPoints);
			Vector3 nextTreadUp = GetTreadsUp(num5, toIndex, worldPoints);
			bool flag = false;
			float num6 = 0f;
			int num7 = Mathf.FloorToInt(Mathf.Clamp(magnitude / 4.5f, 0f, 2f));
			if (num7 > 0 && transform != transform2)
			{
				magnitude = (vector4 - vector3).magnitude;
				float num8 = magnitude / (float)(num7 + 1);
				float num9 = num8;
				Vector3 vector6 = vector3;
				for (int k = 0; k < num7; k++)
				{
					Vector3 fromPoint = vector6 + normalized * (num9 - num8);
					if (k == 0)
					{
						fromPoint = vector3;
					}
					Vector3 vector7 = vector6 + normalized * num9;
					TankTreadsSupportWheel tankTreadsSupportWheel = CreateSupportWheel(vector7, normalized, j, k);
					supportWheels.Add(tankTreadsSupportWheel);
					Transform fromTransform = transform;
					if (k > 0)
					{
						fromTransform = supportWheels[supportWheels.Count - 2].go.transform;
					}
					Transform transform3 = tankTreadsSupportWheel.go.transform;
					treadsInfo.links.Add(CreateTreadLink(fromPoint, vector7, fromTransform, transform3, right, vector, vector2, treadUp, prevTreadUp, nextTreadUp, num3 + num6, uvFactor));
					num9 += num8;
					flag = true;
					num6 += num8;
				}
			}
			if (flag)
			{
				TreadLink treadLink = treadsInfo.links[treadsInfo.links.Count - 1];
				treadsInfo.links.Add(CreateTreadLink(treadLink.ToToWorld(right, vector, vector2), vector4, treadLink.toTransform, transform2, right, vector, vector2, treadUp, prevTreadUp, nextTreadUp, num3 + num6, uvFactor));
			}
			else
			{
				treadsInfo.links.Add(CreateTreadLink(vector3, vector4, transform, transform2, right, vector, vector2, treadUp, prevTreadUp, nextTreadUp, num3, uvFactor));
			}
			num3 += magnitude;
		}
		foreach (TreadLink link in treadsInfo.links)
		{
			if (link.collisionGo != null)
			{
				treadsInfo.physicalLinks.Add(link);
			}
		}
	}

	private TreadLink CreateTreadLink(Vector3 fromPoint, Vector3 toPoint, Transform fromTransform, Transform toTransform, Vector3 coordRight, Vector3 coordUp, Vector3 coordForward, Vector3 treadUp, Vector3 prevTreadUp, Vector3 nextTreadUp, float distanceSoFar, float uvFactor)
	{
		Vector3 fromCoord = WorldToTreadsLocal(fromPoint, fromTransform, coordRight, coordUp, coordForward);
		Vector3 toCoord = WorldToTreadsLocal(toPoint, toTransform, coordRight, coordUp, coordForward);
		Vector3 vector = toPoint - fromPoint;
		float magnitude = vector.magnitude;
		Vector3 normalized = vector.normalized;
		Vector3 vector2 = 0.5f * (fromPoint + toPoint);
		Vector3 vector3 = Scale();
		float volume = vector3.x * 0.25f * 2f * magnitude;
		GameObject gameObject = null;
		if (fromTransform != toTransform)
		{
			gameObject = new GameObject(go.name + " support link");
			Transform transform = gameObject.transform;
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(vector3.x - 0.2f, 0.475f, magnitude);
			Vector3 vector4 = -treadUp * 0.25f;
			transform.position = vector2 + vector4;
			transform.rotation = Quaternion.LookRotation(normalized, treadUp);
			Util.SetLayerRaw(gameObject, go.layer, recursive: true);
		}
		GameObject gameObject2 = new GameObject("tread part");
		Transform transform2 = gameObject2.transform;
		Vector3 vector5 = treadUp * 0.2f;
		transform2.position = vector2 + vector5;
		transform2.rotation = Quaternion.LookRotation(normalized, treadUp);
		MeshFilter meshFilter = gameObject2.AddComponent<MeshFilter>();
		float topExpandNext = 0.1f * Mathf.Tan((float)Math.PI / 360f * Vector3.Angle(treadUp, nextTreadUp));
		float topExpandPrev = 0.1f * Mathf.Tan((float)Math.PI / 360f * Vector3.Angle(treadUp, prevTreadUp));
		meshFilter.mesh = CreateTreadMesh(new Vector3(vector3.x, 0.2f, magnitude), topExpandPrev, topExpandNext, distanceSoFar, uvFactor);
		Renderer item = gameObject2.AddComponent<MeshRenderer>();
		treadsInfo.TreadRenders.Add(item);
		Util.SetLayerRaw(gameObject2, go.layer, recursive: true);
		TreadLink treadLink = new TreadLink
		{
			fromCoord = fromCoord,
			toCoord = toCoord,
			fromTransform = fromTransform,
			toTransform = toTransform,
			collisionGo = gameObject,
			visualGo = gameObject2,
			visualGoT = gameObject2.transform,
			length = (toPoint - fromPoint).magnitude,
			volume = volume
		};
		if (gameObject != null)
		{
			SupportWheelHelpForceBehaviour supportWheelHelpForceBehaviour = gameObject.AddComponent<SupportWheelHelpForceBehaviour>();
			supportWheelHelpForceBehaviour.giver = treadLink;
			treadLink.helpBehaviour = supportWheelHelpForceBehaviour;
			supportWheelHelpForceBehaviour.forwardEvents = true;
			treadLink.treadsInfo = treadsInfo;
			treadsInfo.gameObjectToTreadLink[gameObject] = treadLink;
			BWSceneManager.AddChildBlockInstanceID(gameObject, this);
		}
		return treadLink;
	}

	private void CreateRectangularCuboidPlane(Vector3 cuboidSize, Vector3 up, Vector3 forward, List<Vector3> vertices, List<Vector3> normals, List<int> triangles, List<Vector2> uv, float expandPrev, float expandNext, float distanceSoFar, float uvFactor)
	{
		Vector3 vector = Vector3.Cross(up, forward);
		int count = vertices.Count;
		Vector3 vector2 = 0.5f * Mathf.Abs(Vector3.Dot(cuboidSize, up)) * up;
		float num = Mathf.Abs(Vector3.Dot(cuboidSize, forward));
		Vector3 vector3 = 0.5f * (num + 2f * expandNext) * forward;
		Vector3 vector4 = -0.5f * (num + 2f * expandPrev) * forward;
		Vector3 vector5 = 0.5f * Mathf.Abs(Vector3.Dot(cuboidSize, vector)) * vector;
		vertices.AddRange(new Vector3[4]
		{
			vector2 + vector4 - vector5,
			vector2 + vector4 + vector5,
			vector2 + vector3 + vector5,
			vector2 + vector3 - vector5
		});
		triangles.AddRange(new int[6]
		{
			count,
			count + 2,
			count + 1,
			count,
			count + 3,
			count + 2
		});
		normals.AddRange(new Vector3[4] { up, up, up, up });
		float x = 0f;
		float uValue = treadsInfo.uValue;
		float y = uvPerUnit * uvFactor * distanceSoFar;
		float y2 = uvPerUnit * uvFactor * (distanceSoFar + num);
		uv.AddRange(new Vector2[4]
		{
			new Vector2(x, y),
			new Vector2(uValue, y),
			new Vector2(uValue, y2),
			new Vector2(x, y2)
		});
	}

	private Mesh CreateTreadMesh(Vector3 meshSize, float topExpandPrev, float topExpandNext, float distanceSoFar, float uvFactor)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<int> list3 = new List<int>();
		List<Vector2> list4 = new List<Vector2>();
		List<Color> list5 = new List<Color>();
		Side[] array = new Side[6]
		{
			Side.Top,
			Side.Bottom,
			Side.Right,
			Side.Left,
			Side.Front,
			Side.Back
		};
		Side[] array2 = array;
		foreach (Side side in array2)
		{
			Vector3 up = Materials.SideToNormal(side);
			Vector3 forward = Materials.SideToForward(side);
			float expandPrev = 0f;
			float expandNext = 0f;
			if (side == Side.Top)
			{
				expandPrev = topExpandPrev;
				expandNext = topExpandNext;
			}
			CreateRectangularCuboidPlane(meshSize, up, forward, list, list2, list3, list4, expandPrev, expandNext, distanceSoFar, uvFactor);
			list5.AddRange(new Color[4]
			{
				Color.white,
				Color.white,
				Color.white,
				Color.white
			});
		}
		mesh.vertices = list.ToArray();
		mesh.normals = list2.ToArray();
		mesh.triangles = list3.ToArray();
		mesh.uv = list4.ToArray();
		mesh.colors = list5.ToArray();
		return mesh;
	}

	private Vector3 WorldToTreadsLocal(Vector3 point, Transform t, Vector3 right, Vector3 up, Vector3 forward)
	{
		Vector3 lhs = point - t.position;
		return new Vector3(Vector3.Dot(lhs, right), Vector3.Dot(lhs, up), Vector3.Dot(lhs, forward));
	}

	private TankTreadsSupportWheel CreateSupportWheel(Vector3 treadPos, Vector3 treadDir, int majorIndex, int minorIndex)
	{
		Vector3 right = goT.right;
		Vector3 vector = Vector3.Cross(treadDir, right);
		GameObject gameObject = new GameObject(go.name + " support wheel");
		Transform transform = gameObject.transform;
		Vector3 vector2 = Scale();
		transform.rotation = goT.rotation;
		transform.position = treadPos - vector * 0.25f;
		CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
		capsuleCollider.height = vector2.x - 0.2f;
		capsuleCollider.radius = 0.25f;
		capsuleCollider.direction = 0;
		Util.SetLayerRaw(gameObject, go.layer, recursive: true);
		BWSceneManager.AddChildBlockInstanceID(gameObject, this);
		return new TankTreadsSupportWheel
		{
			go = gameObject
		};
	}

	public static List<int> ComputeConvexHull(List<Vector2> pointList)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < pointList.Count; i++)
		{
			list.Add(i);
		}
		list.Sort(delegate(int a, int b)
		{
			Vector2 vector3 = pointList[a];
			Vector2 vector4 = pointList[b];
			if (vector3.x == vector4.x)
			{
				return vector3.y.CompareTo(vector4.y);
			}
			return (!(vector3.x <= vector4.x)) ? 1 : (-1);
		});
		List<int> list2 = new List<int>();
		int num = 0;
		int num2 = 0;
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			Vector2 vector = pointList[list[num3]];
			Vector2 vector2;
			while (num >= 2 && Vector2Cross((vector2 = pointList[list2[list2.Count - 1]]) - pointList[list2[list2.Count - 2]], vector - vector2) >= 0f)
			{
				list2.RemoveAt(list2.Count - 1);
				num--;
			}
			list2.Add(list[num3]);
			num++;
			while (num2 >= 2 && Vector2Cross((vector2 = pointList[list2[0]]) - pointList[list2[1]], vector - vector2) <= 0f)
			{
				list2.RemoveAt(0);
				num2--;
			}
			if (num2 != 0)
			{
				list2.Insert(0, list[num3]);
			}
			num2++;
		}
		list2.RemoveAt(list2.Count - 1);
		return list2;
	}

	private static float Vector2Cross(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	public override GAF GetIconGaf()
	{
		int num = group.GetBlocks().Length;
		return new GAF(Block.predicateCreate, "Tank Treads Wheel x" + num);
	}

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		if (group == null)
		{
			base.Break(chunkPos, chunkVel, chunkAngVel);
		}
		else
		{
			if (broken)
			{
				return;
			}
			foreach (TreadLink link in treadsInfo.links)
			{
				link.DestroyJoints();
				link.Break(chunkPos, chunkVel, chunkAngVel);
			}
			foreach (TankTreadsSupportWheel supportWheel in supportWheels)
			{
				supportWheel.DestroyJoint();
			}
			foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
			{
				foreach (ConfigurableJoint item in new List<ConfigurableJoint>(groupBlock.joints))
				{
					groupBlock.DestroyJoint(item);
				}
				groupBlock.broken = true;
			}
			broken = true;
		}
	}

	public override void RemovedPlayBlock(Block b)
	{
		base.RemovedPlayBlock(b);
		if (b.go == chassis)
		{
			chassis = null;
		}
		else if (externalControlBlock == b)
		{
			externalControlBlock = null;
		}
		else if (this == b)
		{
			SetTreadsVisible(v: false);
		}
	}

	private void CreateBuildModeCollider()
	{
		DestroyBuildModeCollider();
		buildModeColliderGo = new GameObject(go.name + " build mode collider");
		Util.SetLayerRaw(buildModeColliderGo, go.layer, recursive: true);
		BWSceneManager.AddChildBlockInstanceID(buildModeColliderGo, this);
		Mesh mesh = new Mesh();
		List<Vector3> simplifiedHullPoints = treadsInfo.simplifiedHullPoints;
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		Vector3 vector = goT.right * Scale().x * 0.5f;
		int count = simplifiedHullPoints.Count;
		for (int i = 0; i < simplifiedHullPoints.Count; i++)
		{
			int num = i;
			int num2 = (num + 1) % count;
			Vector3 vector2 = simplifiedHullPoints[i];
			list.Add(vector2 + vector);
			list.Add(vector2 - vector);
			list2.AddRange(new int[6]
			{
				0,
				num * 2,
				num2 * 2,
				1,
				num * 2 + 1,
				num2 * 2 + 1
			});
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		MeshCollider meshCollider = buildModeColliderGo.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		meshCollider.convex = true;
	}

	private bool TryReuseSATVolumes(ref string currentKey)
	{
		currentKey = TankTreadsSATMeshesInfo.GetKey(this);
		if (currentKey == cachedSatMeshesKey)
		{
			shapeMeshes[0] = cachedSatMeshes.shape;
			jointMeshes[0] = cachedSatMeshes.joint1;
			jointMeshes[1] = cachedSatMeshes.joint2;
			Vector3 offset = goT.position - cachedSatMeshes.position;
			CollisionVolumes.TranslateMeshes(shapeMeshes, offset);
			CollisionVolumes.TranslateMeshes(jointMeshes, offset);
			cachedSatMeshes.position = goT.position;
			foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
			{
				groupBlock.shapeMeshes = shapeMeshes;
			}
			return true;
		}
		return false;
	}

	public override void UpdateSATVolumes()
	{
		if (!IsMainBlockInGroup())
		{
			glueMeshes = new CollisionMesh[0];
			shapeMeshes = new CollisionMesh[0];
			jointMeshes = new CollisionMesh[0];
			return;
		}
		if (treadsInfo.simplifiedHullPoints.Count == 0)
		{
			ComputeSimplifiedHullPoints();
		}
		if (glueMeshes == null)
		{
			glueMeshes = new CollisionMesh[0];
		}
		if (shapeMeshes == null || shapeMeshes.Length != 1)
		{
			shapeMeshes = new CollisionMesh[1];
		}
		shapeMeshes[0] = CreateConvexSATMesh(Scale().x - 0.2f, 0f);
		foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
		{
			groupBlock.shapeMeshes = shapeMeshes;
		}
		if (jointMeshes == null || jointMeshes.Length != 2)
		{
			jointMeshes = new CollisionMesh[2];
		}
		jointMeshes[0] = CreateConvexSATMesh(0.01f, Scale().x * 0.5f);
		jointMeshes[1] = CreateConvexSATMesh(0.01f, (0f - Scale().x) * 0.5f);
		cachedSatMeshesKey = TankTreadsSATMeshesInfo.GetKey(this);
		cachedSatMeshes = new TankTreadsSATMeshesInfo
		{
			joint1 = jointMeshes[0],
			joint2 = jointMeshes[1],
			shape = shapeMeshes[0],
			key = cachedSatMeshesKey,
			position = goT.position
		};
	}

	private CollisionMesh CreateConvexSATMesh(float thickness, float offset)
	{
		Vector3 position = goT.position;
		if (treadsInfo.simplifiedHullPoints.Count == 0)
		{
			Triangle[] triangles = new Triangle[1]
			{
				new Triangle(position, position + Vector3.up, position + Vector3.right)
			};
			return new CollisionMesh(triangles);
		}
		List<Triangle> list = new List<Triangle>();
		List<Vector3> simplifiedHullPoints = treadsInfo.simplifiedHullPoints;
		Vector3 right = goT.right;
		Vector3 v = simplifiedHullPoints[0] + (offset + thickness * 0.5f) * right;
		Vector3 v2 = simplifiedHullPoints[0] + (offset - thickness * 0.5f) * right;
		for (int i = 0; i < simplifiedHullPoints.Count - 1; i++)
		{
			Vector3 vector = simplifiedHullPoints[i] + (offset + thickness * 0.5f) * right;
			Vector3 v3 = simplifiedHullPoints[(i + 1) % simplifiedHullPoints.Count] + (offset + thickness * 0.5f) * right;
			Vector3 v4 = simplifiedHullPoints[i] + (offset - thickness * 0.5f) * right;
			Vector3 vector2 = simplifiedHullPoints[(i + 1) % simplifiedHullPoints.Count] + (offset - thickness * 0.5f) * right;
			if (i != 0)
			{
				list.Add(new Triangle(v, vector, v3));
				list.Add(new Triangle(v2, v4, vector2));
			}
			list.Add(new Triangle(vector, v4, vector2));
			list.Add(new Triangle(vector, vector2, v3));
		}
		return new CollisionMesh(list.ToArray());
	}

	private Bounds GetSimplifiedHullBounds()
	{
		Bounds result = new Bounds(goT.position, Vector3.one);
		if (treadsInfo == null)
		{
			return result;
		}
		Vector3 vector = goT.right * Scale().x * 0.5f;
		foreach (Vector3 simplifiedHullPoint in treadsInfo.simplifiedHullPoints)
		{
			result.Encapsulate(simplifiedHullPoint + vector);
			result.Encapsulate(simplifiedHullPoint - vector);
		}
		return result;
	}

	public override Bounds GetShapeCollisionBounds()
	{
		return GetSimplifiedHullBounds();
	}

	public override bool ShapeMeshCanCollideWith(Block b)
	{
		if (!(b is BlockTankTreadsWheel { group: not null } blockTankTreadsWheel) || group == null || blockTankTreadsWheel.group.groupId != group.groupId)
		{
			return true;
		}
		if (b != this && !Tutorial.InTutorialOrPuzzle())
		{
			Bounds bounds = b.go.GetComponent<Collider>().bounds;
			Bounds bounds2 = go.GetComponent<Collider>().bounds;
			bounds.size *= 0.9f;
			bounds2.size *= 0.9f;
			return bounds.Intersects(bounds2);
		}
		return false;
	}

	public override void TBoxStartRotate()
	{
		localCoords = ComputeLocalCoords();
	}

	private Dictionary<Block, Vector3> ComputeLocalCoords(bool snap = false)
	{
		Dictionary<Block, Vector3> dictionary = new Dictionary<Block, Vector3>();
		Transform transform = goT;
		Vector3 position = transform.position;
		Block[] blocks = group.GetBlocks();
		foreach (Block block in blocks)
		{
			if (block != this)
			{
				Vector3 rhs = block.GetPosition() - position;
				Vector3 vector = new Vector3(Vector3.Dot(transform.right, rhs), Vector3.Dot(transform.up, rhs), Vector3.Dot(transform.forward, rhs));
				if (Mathf.Abs(vector[0]) > 0.001f)
				{
					vector[0] = 0f;
				}
				dictionary[block] = ((!snap) ? vector : Util.Round2(vector));
			}
		}
		return dictionary;
	}

	public override void TBoxStopScale()
	{
		Dictionary<Block, Vector3> coords = ComputeLocalCoords();
		RestoreLocalCoords(coords, updateTreads: false);
	}

	public override void TBoxStopRotate()
	{
		if (localCoords == null)
		{
			BWLog.Info("TBoxStopRotate() called without calling TBoxStartRotate() first");
			return;
		}
		RestoreLocalCoords(localCoords);
		localCoords = null;
	}

	private void RestoreLocalCoords(Dictionary<Block, Vector3> coords, bool updateTreads = true)
	{
		if (coords == null)
		{
			return;
		}
		Transform transform = goT;
		Vector3 position = transform.position;
		foreach (KeyValuePair<Block, Vector3> coord in coords)
		{
			Vector3 vector = transform.TransformDirection(coord.Value);
			coord.Key.MoveTo(position + vector);
		}
		if (updateTreads && GetMainBlockInGroup() is BlockTankTreadsWheel blockTankTreadsWheel)
		{
			blockTankTreadsWheel.CreateTreads(shapeOnly: true);
			blockTankTreadsWheel.treadsDirty = true;
		}
	}

	public override bool IgnorePaintToIndexInTutorial(int meshIndex)
	{
		if (meshIndex != 3)
		{
			return meshIndex == 4;
		}
		return true;
	}

	public override bool IgnoreTextureToIndexInTutorial(int meshIndex)
	{
		if (meshIndex != 3)
		{
			return meshIndex == 4;
		}
		return true;
	}

	public override void ConnectionsChanged()
	{
		if (groupBlocks != null)
		{
			foreach (BlockTankTreadsWheel groupBlock in groupBlocks)
			{
				groupBlock.axlesDirty = true;
			}
		}
		base.ConnectionsChanged();
	}

	protected override void TranslateSATVolumes(Vector3 offset)
	{
	}
}
