using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAnimatedCharacter : BlockWalkable, IBlockster
{
	public static Dictionary<BlockAnimatedCharacter, CharacterStateHandler> stateControllers;

	public static Vector3 rightHandPlacementOffset;

	public static Vector3 leftHandPlacementOffset;

	public CharacterStateHandler stateHandler;

	public Block attachedBottomBlock;

	public Block attachedFrontBlock;

	public Block attachedLeftBlock;

	public Block attachedRightBlock;

	public List<Block> attachedBackBlocks = new List<Block>();

	public List<Block> attachedHeadBlocks = new List<Block>();

	public List<Block> attachedLeftHandBlocks = new List<Block>();

	public List<Block> attachedRightHandBlocks = new List<Block>();

	public bool attachmentsPreventLookAnim;

	public bool isConnectedToUnsupportedActionBlock;

	private bool haveDeterminedAttachments;

	private Vector3 pausedVelocityHands0;

	private Vector3 pausedAngularVelocityHands0;

	private Vector3 pausedVelocityHands1;

	private Vector3 pausedAngularVelocityHands1;

	private Vector3 pausedVelocityMiddle;

	private Vector3 pausedAngularVelocityMiddle;

	private bool canUseCharacterShadow;

	private Transform footLt;

	private Transform footRt;

	private Transform hair;

	public static Predicate predicateCharacterMover;

	public static Predicate predicateChracterTiltMover;

	public static Predicate predicateCharacterJump;

	public static Predicate predicateCharacterGotoTag;

	public static Predicate predicateCharacterChaseTag;

	public static Predicate predicateCharacterAvoidTag;

	public static Predicate predicateReplaceLimb;

	private static GameObject prefabCharacterShadow;

	private List<Transform> characterPieces = new List<Transform>();

	private Renderer headClone;

	private bool _playingAttack;

	private bool _playingDodge;

	private int canceledAnimOnRow = -1;

	private int currentAnimRow = -1;

	private bool lockPaintAndTexture;

	public float blocksterRigScale = 1.25f;

	private Dictionary<string, string> blocksterToSkeleton = new Dictionary<string, string>
	{
		{ "Character Body", "bJoint_hips" },
		{ "Character Short Hair", "bJoint_head" },
		{ "Character Head", "bJoint_head" }
	};

	private Dictionary<BlocksterBody.Bone, Transform> boneLookup;

	public CharacterType characterType;

	public static int idxBottom;

	public static int idxFront;

	public static int idxLeft;

	public static int idxRight;

	public static int idxBack;

	private static bool gotGlueIndices;

	private const float miniBodyScale = 0.8f;

	private bool gotSlopedGlue;

	public BlocksterBody bodyParts;

	public BlockAnimatedCharacter(List<List<Tile>> tiles, float standingCharacterHeight, CharacterType charType)
		: base(tiles)
	{
		characterType = charType;
		maxStepHeight = 1.5f;
		maxStepLength = 0.75f;
		capsuleColliderOffset = Vector3.up * 0.15f;
		capsuleColliderHeight = standingCharacterHeight;
		capsuleColliderRadius = 0.7f;
		moveCMOffsetFeetCenter = 0.75f;
		moveCMMaxDistance = 1f;
		moveCM = true;
		if (!gotGlueIndices)
		{
			idxBottom = CollisionTest.IndexOfGlueCollisionMesh("Character", "Bottom");
			idxFront = CollisionTest.IndexOfGlueCollisionMesh("Character", "Front");
			idxLeft = CollisionTest.IndexOfGlueCollisionMesh("Character", "Left");
			idxRight = CollisionTest.IndexOfGlueCollisionMesh("Character", "Right");
			idxBack = CollisionTest.IndexOfGlueCollisionMesh("Character", "Back");
			gotGlueIndices = true;
		}
		stateHandler = new CharacterStateHandler();
		if (stateHandler == null)
		{
			BWLog.Error("Out of memory!");
			return;
		}
		GameObject gameObject = null;
		UnityEngine.Object obj = Resources.Load("Animation/Character/Blockster");
		if (null == obj)
		{
			BWLog.Error("Unable to load character rig");
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(obj) as GameObject;
			gameObject.name = "BlocksterRig";
		}
		if (gameObject != null)
		{
			stateHandler.SetTarget(this, gameObject);
			FindBones();
			string name = "Character Body";
			middle = goT.Find(name).gameObject;
			head = go;
			ParentToSkeleton();
			SizeMini();
			stateHandler.SetRole(DefaultRoleForCharacterType(characterType));
			stateHandler.ForceSit();
			stateControllers[this] = stateHandler;
			gameObject.transform.localScale = blocksterRigScale * Vector3.one;
			bodyParts = new BlocksterBody(this);
			ResetBodyParts();
		}
	}

	public void IBlockster_FindAttachments()
	{
		DetermineAttachments();
	}

	public Block IBlockster_BottomAttachment()
	{
		return attachedBottomBlock;
	}

	public List<Block> IBlockster_HeadAttachments()
	{
		return attachedHeadBlocks;
	}

	public Block IBlockster_FrontAttachment()
	{
		return attachedFrontBlock;
	}

	public Block IBlockster_BackAttachment()
	{
		if (attachedBackBlocks == null || attachedBackBlocks.Count == 0)
		{
			return null;
		}
		return attachedBackBlocks[0];
	}

	public Block IBlockster_RightHandAttachment()
	{
		return attachedRightBlock;
	}

	public Block IBlockster_LeftHandAttachement()
	{
		return attachedLeftBlock;
	}

	public new static void Register()
	{
		predicateCharacterJump = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Jump", (Block b) => ((BlockWalkable)b).IsJumping, (Block b) => ((BlockWalkable)b).Jump, new Type[1] { typeof(float) }, new string[1] { "Force" });
		predicateCharacterGotoTag = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.GotoTag", null, (Block b) => ((BlockWalkable)b).GotoTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3]
		{
			string.Empty,
			"Speed",
			string.Empty
		});
		predicateCharacterChaseTag = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ChaseTag", null, (Block b) => ((BlockWalkable)b).ChaseTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		}, new string[3]
		{
			string.Empty,
			"Speed",
			string.Empty
		});
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.GotoTap", null, (Block b) => ((BlockWalkable)b).GotoTap, new Type[1] { typeof(float) }, new string[1] { "Speed" });
		predicateCharacterMover = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.DPadControl", null, (Block b) => ((BlockWalkable)b).DPadControl, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		predicateChracterTiltMover = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TiltMover", null, (Block b) => b.TiltMoverControl, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Translate", null, (Block b) => ((BlockWalkable)b).Translate, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Turn", null, (Block b) => ((BlockWalkable)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TurnTowardsTag", null, (Block b) => ((BlockWalkable)b).TurnTowardsTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TurnTowardsTap", null, (Block b) => ((BlockWalkable)b).TurnTowardsTap);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.TurnAlongCam", null, (Block b) => ((BlockWalkable)b).TurnAlongCam);
		predicateCharacterAvoidTag = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.AvoidTag", null, (Block b) => ((BlockWalkable)b).AvoidTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAnimatedCharacter>("Block.StartAnimFirstPersonCamera", (Block b) => b.IsFirstPersonBlock, (Block b) => b.FirstPersonCamera, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Idle", null, (Block b) => ((BlockWalkable)b).Idle);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.FreezeRotation", null, (Block b) => ((BlockWalkable)b).FreezeRotation);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Stand", null, (Block b) => ((BlockAnimatedCharacter)b).Stand);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Sit", null, (Block b) => ((BlockAnimatedCharacter)b).Sit);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Collapse", (Block b) => ((BlockAnimatedCharacter)b).IsCollapsed, (Block b) => ((BlockAnimatedCharacter)b).Collapse);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Recover", (Block b) => ((BlockAnimatedCharacter)b).IsNotCollapsed, (Block b) => ((BlockAnimatedCharacter)b).Recover);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.QueueState", null, (Block b) => ((BlockAnimatedCharacter)b).QueueState, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.InterruptState", null, (Block b) => ((BlockAnimatedCharacter)b).InterruptState, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.PlayAnim", null, (Block b) => ((BlockAnimatedCharacter)b).PlayAnim, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.DebugPlayAnim", null, (Block b) => ((BlockAnimatedCharacter)b).DebugPlayAnim, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ToggleCrawl", null, (Block b) => ((BlockAnimatedCharacter)b).ToggleCrawl, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsJumping", (Block b) => ((BlockAnimatedCharacter)b).IsJumping);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsSwimming", (Block b) => ((BlockAnimatedCharacter)b).IsSwimming);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsCrawling", (Block b) => ((BlockAnimatedCharacter)b).IsCrawling);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsWalking", (Block b) => ((BlockAnimatedCharacter)b).IsWalking);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.IsProne", (Block b) => ((BlockAnimatedCharacter)b).IsProne);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.DoubleJump", (Block b) => ((BlockAnimatedCharacter)b).CanDoubleJump, (Block b) => ((BlockAnimatedCharacter)b).DoubleJump);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ApplyDiveForce", null, (Block b) => ((BlockAnimatedCharacter)b).ApplyDiveForce, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.SetRole", (Block b) => ((BlockAnimatedCharacter)b).IsRole, (Block b) => ((BlockAnimatedCharacter)b).SetRole, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ResetRole", null, (Block b) => ((BlockAnimatedCharacter)b).ResetRole);
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Attack", null, (Block b) => ((BlockAnimatedCharacter)b).Attack, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.StandingAttack", null, (Block b) => ((BlockAnimatedCharacter)b).StandingAttack, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Shield", null, (Block b) => ((BlockAnimatedCharacter)b).Shield, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.Dodge", null, (Block b) => ((BlockAnimatedCharacter)b).Dodge, new Type[1] { typeof(string) });
		predicateReplaceLimb = PredicateRegistry.Add<BlockAnimatedCharacter>("AnimCharacter.ReplaceBodyPart", null, (Block b) => ((BlockAnimatedCharacter)b).ReplaceBodyPart, new Type[2]
		{
			typeof(string),
			typeof(string)
		});
	}

	public static void StripNonCompatibleTiles(List<List<Tile>> tiles)
	{
		HashSet<Predicate> hashSet = new HashSet<Predicate>();
		hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockAnimatedCharacter)));
		hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockWalkable)));
		foreach (List<Tile> tile in tiles)
		{
			for (int num = tile.Count - 1; num >= 0; num--)
			{
				if (!hashSet.Contains(tile[num].gaf.Predicate))
				{
					BWLog.Info("Removing non-compatible tile " + tile[num].gaf);
					tile.RemoveAt(num);
				}
			}
		}
	}

	public void RebuildSubMeshes()
	{
		subMeshes = null;
		FindSubMeshes();
	}

	public void RebuildSubMeshPaintTiles()
	{
		List<Tile> list = tiles[0];
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Tile tile = list[num];
			string name = tile.gaf.Predicate.Name;
			if (name == "Block.PaintTo" && tile.gaf.Args.Length > 1)
			{
				int num2 = (int)tile.gaf.Args[1];
				if (num2 > 0)
				{
					list.RemoveAt(num);
				}
			}
		}
		for (int i = 0; i < subMeshPaints.Count; i++)
		{
			list.Add(new Tile(new GAF("Block.PaintTo", subMeshPaints[i], i + 1)));
		}
	}

	private void ResetBodyParts()
	{
		string text = "Limb Old";
		if (characterType == CharacterType.Skeleton)
		{
			text = "Limb Skeleton";
		}
		bodyParts.currentBodyPartVersions.Clear();
		HashSet<BlocksterBody.BodyPart> hashSet = new HashSet<BlocksterBody.BodyPart>
		{
			BlocksterBody.BodyPart.LeftArm,
			BlocksterBody.BodyPart.LeftLeg,
			BlocksterBody.BodyPart.RightArm,
			BlocksterBody.BodyPart.RightLeg
		};
		HashSet<BlocksterBody.BodyPart> hashSet2 = new HashSet<BlocksterBody.BodyPart>();
		List<Tile> list = tiles[0];
		foreach (Tile item in list)
		{
			if (item.gaf.Predicate.Name == "AnimCharacter.ReplaceBodyPart" && item.gaf.Args.Length > 1)
			{
				string value = (string)item.gaf.Args[1];
				string bodyPartVersionStr = (string)item.gaf.Args[0];
				BlocksterBody.BodyPart bodyPart = (BlocksterBody.BodyPart)Enum.Parse(typeof(BlocksterBody.BodyPart), value);
				bodyParts.SubstitutePart(bodyPart, bodyPartVersionStr);
				hashSet.Remove(bodyPart);
				hashSet2.Add(bodyPart);
			}
		}
		using (HashSet<BlocksterBody.BodyPart>.Enumerator enumerator2 = hashSet.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				switch (enumerator2.Current)
				{
				case BlocksterBody.BodyPart.LeftArm:
					bodyParts.SubstitutePart(BlocksterBody.BodyPart.LeftArm, text + " Arm Left");
					break;
				case BlocksterBody.BodyPart.RightArm:
					bodyParts.SubstitutePart(BlocksterBody.BodyPart.RightArm, text + " Arm Right");
					break;
				case BlocksterBody.BodyPart.LeftLeg:
					bodyParts.SubstitutePart(BlocksterBody.BodyPart.LeftLeg, text + " Leg Left");
					break;
				case BlocksterBody.BodyPart.RightLeg:
					bodyParts.SubstitutePart(BlocksterBody.BodyPart.RightLeg, text + " Leg Right");
					break;
				}
			}
		}
		UpdateCachedBodyParts();
		RebuildSubMeshes();
		ReapplyInitPaintAndTexures();
		foreach (BlocksterBody.BodyPart item2 in hashSet)
		{
			bodyParts.ApplyDefaultPaints(item2);
			SaveBodyPartAssignmentToTiles(item2);
		}
	}

	public void ReapplyInitPaintAndTexures()
	{
		if (characterType == CharacterType.Avatar)
		{
			bool flag = lockPaintAndTexture;
			lockPaintAndTexture = false;
			PaintTo("Dark Blue", permanent: true);
			for (int i = 0; i < subMeshGameObjects.Count; i++)
			{
				PaintTo("Dark Blue", permanent: true, i);
			}
			lockPaintAndTexture = flag;
			return;
		}
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
		Dictionary<int, Vector3> dictionary3 = new Dictionary<int, Vector3>();
		List<Tile> list = tiles[0];
		foreach (Tile item in list)
		{
			Predicate predicate = item.gaf.Predicate;
			object[] args = item.gaf.Args;
			if (predicate == Block.predicatePaintTo && args.Length != 0)
			{
				int key = ((args.Length > 1) ? ((int)item.gaf.Args[1]) : 0);
				string value = (string)item.gaf.Args[0];
				dictionary[key] = value;
			}
			if (predicate == Block.predicateTextureTo && args.Length > 1)
			{
				int key2 = ((item.gaf.Args.Length > 2) ? ((int)item.gaf.Args[2]) : 0);
				string value2 = (string)item.gaf.Args[0];
				Vector3 value3 = (Vector3)item.gaf.Args[1];
				dictionary2[key2] = value2;
				dictionary3[key2] = value3;
			}
		}
		foreach (KeyValuePair<int, string> item2 in dictionary)
		{
			PaintTo(item2.Value, permanent: true, item2.Key);
		}
		foreach (KeyValuePair<int, string> item3 in dictionary2)
		{
			TextureTo(item3.Value, dictionary3[item3.Key], permanent: true, item3.Key);
		}
	}

	protected override void FindSubMeshes()
	{
		if (subMeshes != null)
		{
			return;
		}
		if (bodyParts == null)
		{
			base.FindSubMeshes();
			return;
		}
		string text = "Yellow";
		string text2 = "Plain";
		if (subMeshPaints != null && subMeshPaints.Count > 0)
		{
			text = subMeshPaints[0];
		}
		if (subMeshTextures != null && subMeshTextures.Count > 0)
		{
			text2 = subMeshTextures[0];
		}
		string text3 = "Brown";
		if (hair != null)
		{
			int subMeshIndex = GetSubMeshIndex(hair.gameObject);
			if (subMeshIndex > 0 && subMeshIndex <= subMeshPaints.Count)
			{
				text3 = subMeshPaints[subMeshIndex - 1];
			}
		}
		subMeshes = new List<CollisionMesh>();
		subMeshGameObjects = new List<GameObject>();
		subMeshPaints = new List<string>();
		subMeshTextures = new List<string>();
		subMeshTextureNormals = new List<Vector3>();
		List<GameObject> list = new List<GameObject>();
		list.Add(head);
		list.Add(middle);
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightFoot));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftFoot));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightHand));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftHand));
		if (hair != null)
		{
			list.Add(hair.gameObject);
		}
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightLowerLeg));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightUpperLeg));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftLowerLeg));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftUpperLeg));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightLowerArm));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightUpperArm));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftLowerArm));
		list.AddRange(bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftUpperArm));
		canBeTextured = new bool[list.Count];
		canBeMaterialTextured = new bool[list.Count];
		canBeTextured[0] = true;
		canBeMaterialTextured[0] = true;
		childMeshes = new Dictionary<string, Mesh>();
		for (int i = 1; i < list.Count; i++)
		{
			GameObject gameObject = list[i];
			subMeshes.Add(null);
			subMeshGameObjects.Add(gameObject);
			bool flag = false;
			bool flag2 = true;
			string item = "Black";
			string item2 = "Plain";
			if (i == 1)
			{
				item = text;
				item2 = text2;
				flag = true;
			}
			else if (hair != null && gameObject == hair.gameObject)
			{
				item = text3;
				flag = true;
			}
			else
			{
				BodyPartInfo component = gameObject.GetComponent<BodyPartInfo>();
				if (component != null)
				{
					if (!string.IsNullOrEmpty(component.currentPaint))
					{
						item = component.currentPaint;
					}
					if (!string.IsNullOrEmpty(component.currentTexture))
					{
						item2 = component.currentTexture;
					}
					flag = component.canBeTextured;
					flag2 = component.canBeMaterialTextured;
				}
			}
			subMeshPaints.Add(item);
			subMeshTextures.Add(item2);
			subMeshTextureNormals.Add(Vector3.up);
			canBeTextured[i] = flag;
			canBeMaterialTextured[i] = flag2;
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			if (component2 != null)
			{
				childMeshes[gameObject.name] = component2.mesh;
			}
		}
	}

	public override int GetMeshIndexForRay(Ray ray, bool refresh, out Vector3 point, out Vector3 normal)
	{
		int meshIndexForRay = base.GetMeshIndexForRay(ray, refresh, out point, out normal);
		if (meshIndexForRay > 0)
		{
			return meshIndexForRay;
		}
		MeshRenderer meshRenderer = GetCloneHead() as MeshRenderer;
		Bounds bounds = meshRenderer.bounds;
		List<Bounds> list = new List<Bounds>();
		for (int i = 0; i < subMeshGameObjects.Count; i++)
		{
			GameObject gameObject = subMeshGameObjects[i];
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			list.Add(component.bounds);
		}
		float magnitude = (bounds.center - ray.origin).magnitude;
		Vector3 vector = ray.origin + ray.direction * magnitude;
		Vector3 vector2 = bounds.ClosestPoint(vector);
		float num = (vector2 - vector).sqrMagnitude;
		int result = 0;
		for (int j = 0; j < list.Count; j++)
		{
			float magnitude2 = (list[j].center - ray.origin).magnitude;
			Vector3 vector3 = ray.origin + ray.direction * magnitude2;
			Vector3 vector4 = list[j].ClosestPoint(vector3);
			float sqrMagnitude = (vector4 - vector3).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = j + 1;
			}
		}
		return result;
	}

	private CharacterRole DefaultRoleForCharacterType(CharacterType type)
	{
		CharacterRole result = CharacterRole.Male;
		switch (type)
		{
		case CharacterType.Female:
			result = CharacterRole.Female;
			break;
		case CharacterType.Skeleton:
			result = CharacterRole.Skeleton;
			break;
		case CharacterType.MiniMale:
			result = CharacterRole.Mini;
			break;
		case CharacterType.MiniFemale:
			result = CharacterRole.MiniFemale;
			break;
		}
		return result;
	}

	private void SizeMini()
	{
		if (stateHandler != null && (characterType == CharacterType.MiniMale || characterType == CharacterType.MiniFemale))
		{
			stateHandler.targetRig.transform.localScale = Vector3.one * 0.8f;
			Transform transform = FindRecursive("bJoint_head_scale", stateHandler.targetRig.transform);
			transform.localScale = Vector3.one * 1.25f;
		}
	}

	private Renderer GetCloneHead()
	{
		if (headClone == null)
		{
			GameObject gameObject = new GameObject("Character Head");
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshFilter.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
			meshRenderer.sharedMaterial = go.GetComponent<MeshRenderer>().sharedMaterial;
			headClone = gameObject.GetComponent<Renderer>();
			headClone.transform.parent = goT;
		}
		return headClone;
	}

	private Transform FindRecursive(string name, Transform parent)
	{
		if (parent.name == name)
		{
			return parent;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.name == name)
			{
				return child;
			}
			Transform transform = FindRecursive(name, child);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	private void ParentToSkeleton()
	{
		GetCloneHead();
		foreach (KeyValuePair<string, string> item in blocksterToSkeleton)
		{
			string key = item.Key;
			Transform transform = goT.FindChild(key);
			if (!(transform != null))
			{
				continue;
			}
			transform.gameObject.SetActive(value: true);
			if (transform.GetComponent<MeshRenderer>() != null)
			{
				transform.GetComponent<MeshRenderer>().enabled = true;
			}
			string value = item.Value;
			Transform transform2 = FindRecursive(value, stateHandler.targetRig.transform);
			if (transform2 != null)
			{
				if (transform.GetComponent<Rigidbody>() != null)
				{
					UnityEngine.Object.Destroy(transform.GetComponent<Rigidbody>());
					if (transform.GetComponent<BoxCollider>() != null)
					{
						transform.GetComponent<BoxCollider>().enabled = false;
					}
				}
				transform.parent = transform2;
				transform.localScale = GetRelativeMeshToBoneScale(key);
				transform.localEulerAngles = Vector3.zero;
				transform.localPosition = GetRelativeMeshToBonePos(key);
				if (key == "Character Short Hair")
				{
					hair = transform;
				}
				if (!characterPieces.Contains(transform))
				{
					characterPieces.Add(transform);
				}
			}
			else
			{
				BWLog.Error("Failed to find Bone " + value + " for " + transform.name);
			}
		}
	}

	public void PrepareForModelIconRender(Layer screenshotLayer)
	{
		goLayerAssignment = screenshotLayer;
		stateHandler.ForceSit();
		stateHandler.preventLook = true;
		stateHandler.targetRig.SetLayer(screenshotLayer, recursive: true);
	}

	private Vector3 GetRelativeMeshToBonePos(string ourName)
	{
		Vector3 result = Vector3.zero;
		switch (ourName)
		{
		case "Character Short Hair":
		case "Character Head":
			result = new Vector3(0f, -0.082f, 0.06f);
			break;
		case "Character Body":
			result = new Vector3(0f, 0.27f, 0.051f);
			break;
		}
		return result;
	}

	private Vector3 GetRelativeMeshToBoneScale(string ourName)
	{
		Vector3 result = Vector3.one;
		switch (ourName)
		{
		case "Character Short Hair":
		case "Character Head":
			result = new Vector3(0.8f, 0.8f, 0.8f);
			break;
		case "Character Body":
			result = new Vector3(0.7f, 0.68f, 0.44f);
			break;
		}
		return result;
	}

	public override GameObject GetHandAttach(int hand)
	{
		switch (hand)
		{
		case 0:
		{
			string name2 = "bJoint_handRight_attach";
			return FindRecursive(name2, stateHandler.targetRig.transform).gameObject;
		}
		default:
			return base.GetHandAttach(hand);
		case 1:
		{
			string name = "bJoint_handLeft_attach";
			return FindRecursive(name, stateHandler.targetRig.transform).gameObject;
		}
		}
	}

	public Transform GetLeftFootBoneTransform()
	{
		return FindRecursive("bJoint_footLeft", stateHandler.targetRig.transform);
	}

	public Transform GetRightFootBoneTransform()
	{
		return FindRecursive("bJoint_footRight", stateHandler.targetRig.transform);
	}

	public GameObject GetHeadAttach()
	{
		string name = "bJoint_head_attach";
		GameObject gameObject = FindRecursive(name, stateHandler.targetRig.transform).gameObject;
		if (gameObject != null)
		{
			return gameObject.gameObject;
		}
		return null;
	}

	public GameObject GetBodyAttach()
	{
		string name = "bJoint_hips";
		return FindRecursive(name, stateHandler.targetRig.transform).gameObject;
	}

	public override void OnCreate()
	{
		HideSourceBlockster();
		if (characterType == CharacterType.Avatar)
		{
			lockPaintAndTexture = true;
		}
	}

	public string GetCurrentHeadColor()
	{
		return GetPaint();
	}

	public string GetCurrentBodyColor()
	{
		return GetPaint(1);
	}

	public override void OnReconstructed()
	{
		HideSourceBlockster();
	}

	private void HideSourceBlockster()
	{
		go.GetComponent<MeshRenderer>().enabled = false;
	}

	public override void Destroy()
	{
		stateControllers.Remove(this);
		stateHandler = null;
		if (hands != null)
		{
			for (int i = 0; i < hands.Length; i++)
			{
				if (hands[i] != null && hands[i].gameObject != null)
				{
					UnityEngine.Object.Destroy(hands[i].gameObject);
				}
			}
		}
		if (middle != null)
		{
			UnityEngine.Object.Destroy(middle);
		}
		bodyParts.ClearBodyPartObjectLists();
		base.Destroy();
	}

	public override void RemoveBlockMaps()
	{
		if (hands != null)
		{
			for (int i = 0; i < hands.Length; i++)
			{
				if (hands[i].gameObject != null)
				{
					BWSceneManager.RemoveChildBlockInstanceID(hands[i].gameObject);
				}
			}
		}
		if (middle != null)
		{
			BWSceneManager.RemoveChildBlockInstanceID(middle);
		}
		base.RemoveBlockMaps();
	}

	private void FindBones()
	{
		boneLookup = new Dictionary<BlocksterBody.Bone, Transform>();
		Transform transform = stateHandler.targetRig.transform;
		boneLookup[BlocksterBody.Bone.Root] = FindRecursive("bJoint_hips", transform);
		boneLookup[BlocksterBody.Bone.Spine] = FindRecursive("bJoint_spine", transform);
		boneLookup[BlocksterBody.Bone.Head] = FindRecursive("bJoint_head", transform);
		boneLookup[BlocksterBody.Bone.LeftUpperArm] = FindRecursive("bJoint_upperArmLeft", transform);
		boneLookup[BlocksterBody.Bone.LeftLowerArm] = FindRecursive("bJoint_lowerArmLeft", transform);
		boneLookup[BlocksterBody.Bone.LeftHand] = FindRecursive("bJoint_handLeft", transform);
		boneLookup[BlocksterBody.Bone.RightUpperArm] = FindRecursive("bJoint_upperArmRight", transform);
		boneLookup[BlocksterBody.Bone.RightLowerArm] = FindRecursive("bJoint_lowerArmRight", transform);
		boneLookup[BlocksterBody.Bone.RightHand] = FindRecursive("bJoint_handRight", transform);
		boneLookup[BlocksterBody.Bone.LeftUpperLeg] = FindRecursive("bJoint_upperLegLeft", transform);
		boneLookup[BlocksterBody.Bone.LeftLowerLeg] = FindRecursive("bJoint_lowerLegLeft", transform);
		boneLookup[BlocksterBody.Bone.LeftFoot] = FindRecursive("bJoint_footLeft", transform);
		boneLookup[BlocksterBody.Bone.RightUpperLeg] = FindRecursive("bJoint_upperLegRight", transform);
		boneLookup[BlocksterBody.Bone.RightLowerLeg] = FindRecursive("bJoint_lowerLegRight", transform);
		boneLookup[BlocksterBody.Bone.RightFoot] = FindRecursive("bJoint_footRight", transform);
	}

	public Transform GetBoneTransform(BlocksterBody.Bone bone)
	{
		if (!boneLookup.TryGetValue(bone, out var value))
		{
			BWLog.Error("No Transform assigned to bone: " + bone);
		}
		return value;
	}

	public void UpdateCachedBodyParts()
	{
		feet = new FootInfo[2 * legPairCount];
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i] = new FootInfo();
		}
		footRt = null;
		footLt = null;
		List<GameObject> objectsForBone = bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightFoot);
		List<GameObject> objectsForBone2 = bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftFoot);
		if (objectsForBone != null && objectsForBone.Count > 0)
		{
			feet[0].go = objectsForBone[0];
			footRt = objectsForBone[0].transform;
		}
		if (objectsForBone2 != null && objectsForBone2.Count > 0)
		{
			feet[1].go = objectsForBone2[0];
			footLt = objectsForBone2[0].transform;
		}
		hands[0] = bodyParts.GetObjectsForBone(BlocksterBody.Bone.RightHand)[0];
		hands[1] = bodyParts.GetObjectsForBone(BlocksterBody.Bone.LeftHand)[0];
		if (hands[0] != null && middle != null)
		{
			handPos = hands[0].transform.localPosition;
			handsOutXMod = middle.transform.localScale.x * 0.5f;
			handsOutYMod -= 0f - (0.6f + hands[0].transform.localPosition.y);
		}
	}

	public override void Pause()
	{
		base.Pause();
		if (hands[0].GetComponent<Rigidbody>() != null)
		{
			pausedVelocityHands0 = hands[0].GetComponent<Rigidbody>().velocity;
			pausedAngularVelocityHands0 = hands[0].GetComponent<Rigidbody>().angularVelocity;
			hands[0].GetComponent<Rigidbody>().isKinematic = true;
		}
		if (hands[1].GetComponent<Rigidbody>() != null)
		{
			pausedVelocityHands1 = hands[1].GetComponent<Rigidbody>().velocity;
			pausedAngularVelocityHands1 = hands[1].GetComponent<Rigidbody>().angularVelocity;
			hands[1].GetComponent<Rigidbody>().isKinematic = true;
		}
		if (middle.GetComponent<Rigidbody>() != null)
		{
			pausedVelocityMiddle = middle.GetComponent<Rigidbody>().velocity;
			pausedAngularVelocityMiddle = middle.GetComponent<Rigidbody>().angularVelocity;
			middle.GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (hands[0].GetComponent<Rigidbody>() != null)
		{
			hands[0].GetComponent<Rigidbody>().isKinematic = false;
			hands[0].GetComponent<Rigidbody>().velocity = pausedVelocityHands0;
			hands[0].GetComponent<Rigidbody>().angularVelocity = pausedAngularVelocityHands0;
		}
		if (hands[1].GetComponent<Rigidbody>() != null)
		{
			hands[1].GetComponent<Rigidbody>().isKinematic = false;
			hands[1].GetComponent<Rigidbody>().velocity = pausedVelocityHands1;
			hands[1].GetComponent<Rigidbody>().angularVelocity = pausedAngularVelocityHands1;
		}
		if (middle.GetComponent<Rigidbody>() != null)
		{
			middle.GetComponent<Rigidbody>().isKinematic = false;
			middle.GetComponent<Rigidbody>().velocity = pausedVelocityMiddle;
			middle.GetComponent<Rigidbody>().angularVelocity = pausedAngularVelocityMiddle;
		}
	}

	public override void Break(Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
	{
		base.Break(chunkPos, chunkVel, chunkAngVel);
		RemoveMeshFromSkeleton();
		Blocksworld.AddFixedUpdateCommand(new DelayedBreakCharacterCommand(this, chunkPos, chunkVel, chunkAngVel));
	}

	public override void ReassignedToChunk(Chunk c)
	{
		StopPull();
		stateHandler.rb = c.rb;
		foreach (Block block in c.blocks)
		{
			if (block == attachedLeftBlock || attachedLeftHandBlocks.Contains(block))
			{
				block.goT.SetParent(GetHandAttach(1).transform);
			}
			else if (block == attachedRightBlock || attachedRightHandBlocks.Contains(block))
			{
				block.goT.SetParent(GetHandAttach(0).transform);
			}
			else if (attachedHeadBlocks.Contains(block))
			{
				block.goT.SetParent(GetHeadAttach().transform);
			}
			else if (attachedBackBlocks.Contains(block))
			{
				block.goT.SetParent(GetBodyAttach().transform);
			}
		}
	}

	public bool IsAttachment(Block b)
	{
		if (!haveDeterminedAttachments)
		{
			DetermineAttachments();
		}
		if (attachedLeftBlock != b && attachedRightBlock != b && attachedBottomBlock != b && attachedFrontBlock != b && !attachedBackBlocks.Contains(b))
		{
			return attachedHeadBlocks.Contains(b);
		}
		return true;
	}

	private bool IsUnsupportedAttachmentType(Block b)
	{
		if (b != this)
		{
			if (!(b is BlockWalkable) && !(b is BlockCharacter) && !(b is BlockAbstractLegs) && !(b is BlockAbstractWheel) && !(b is BlockAbstractMotor) && !(b is BlockAbstractTorsionSpring) && !(b is BlockTankTreadsWheel) && !(b is BlockMissile) && !(b is BlockAbstractPlatform))
			{
				return b is BlockPiston;
			}
			return true;
		}
		return false;
	}

	public void DetermineAttachments()
	{
		if (goT == null)
		{
			BWLog.Error("Transform (goT) is null in DetermineAttachments");
			return;
		}
		if (connections == null)
		{
			BWLog.Error("Connections list is null in DetermineAttachments");
			return;
		}
		if (glueMeshes == null || glueMeshes.Length == 0)
		{
			BWLog.Error("Glue meshes are null or empty in DetermineAttachments");
			return;
		}
		ResetAttachments();
		HashSet<Block> hashSet = new HashSet<Block>();
		Matrix4x4 worldToLocalMatrix = goT.worldToLocalMatrix;
		for (int i = 0; i < connections.Count; i++)
		{
			Block block = connections[i];
			if (block != null && !block.isTerrain)
			{
				try
				{
					ProcessBlockAttachmentInternal(block, worldToLocalMatrix, hashSet);
				}
				catch (Exception ex)
				{
					BWLog.Error("Error processing block attachment: " + ex.Message);
				}
			}
		}
		ProcessAdditionalAttachmentsInternal(hashSet);
		haveDeterminedAttachments = true;
	}

	public Vector3 GetRightHandAttachOffset()
	{
		if (attachedRightBlock == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = goT.InverseTransformPoint(attachedRightBlock.GetPosition());
		vector -= new Vector3(1f, -0.5f, 0f);
		Vector3 vector2 = new Vector3(0f - vector.y, vector.x, vector.z);
		return vector2 + rightHandPlacementOffset + attachedRightBlock.GetBlockMetaData().attachOffset;
	}

	public Vector3 GetLeftHandAttachOffset()
	{
		if (attachedLeftBlock == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = goT.InverseTransformPoint(attachedLeftBlock.GetPosition());
		vector -= new Vector3(-1f, -0.5f, 0f);
		Vector3 vector2 = new Vector3(vector.y, vector.x, vector.z);
		return vector2 + leftHandPlacementOffset;
	}

	public override void BeforePlay()
	{
		DetermineAttachments();
		goLayerAssignment = Layer.Default;
		go.SetLayer(goLayerAssignment, recursive: true);
	}

	public override void ConnectionsChanged()
	{
		base.ConnectionsChanged();
		haveDeterminedAttachments = false;
		DetermineAttachments();
	}

	private bool IsBlockGluedToMesh(Block b, CollisionMesh characterGlueMesh)
	{
		if (!CollisionTest.MultiMeshMeshTest2(characterGlueMesh, b.glueMeshes))
		{
			return CollisionTest.MultiMeshMeshTest2(characterGlueMesh, b.jointMeshes);
		}
		return true;
	}

	private bool CheckBottomGlueForSlopes(CollisionMesh bottomGlue, CollisionMesh[] glueBeneathCharacter, CollisionMesh[] jointsBeneathCharacter)
	{
		for (int i = 0; i < glueBeneathCharacter.Length; i++)
		{
			if (CollisionTest.MeshMeshTest(bottomGlue, glueBeneathCharacter[i]))
			{
				gotSlopedGlue = (gotSlopedGlue ? gotSlopedGlue : CheckForAngledMesh(glueBeneathCharacter[i].localRot));
				return true;
			}
		}
		for (int j = 0; j < jointsBeneathCharacter.Length; j++)
		{
			if (CollisionTest.MeshMeshTest(bottomGlue, jointsBeneathCharacter[j]))
			{
				gotSlopedGlue = (gotSlopedGlue ? gotSlopedGlue : CheckForAngledMesh(jointsBeneathCharacter[j].localRot));
				return true;
			}
		}
		return false;
	}

	private bool CheckForAngledMesh(Vector3 ourRot)
	{
		if (Util.HasNoAngle(ourRot.x) && Util.HasNoAngle(ourRot.y))
		{
			return !Util.HasNoAngle(ourRot.z);
		}
		return true;
	}

	public override void BecameTreasure()
	{
		base.BecameTreasure();
		stateHandler.InterruptQueue(CharacterState.Sitting);
	}

	private bool NotOnlyBlocksterInChunk()
	{
		if (chunk != null)
		{
			bool flag = false;
			for (int i = 0; i < chunk.blocks.Count; i++)
			{
				if (chunk.blocks[i] is BlockAnimatedCharacter)
				{
					if (flag)
					{
						return true;
					}
					flag = true;
				}
			}
		}
		return false;
	}

	public override void Play()
	{
		unmoving = attachedBottomBlock != null || attachedFrontBlock != null;
		unmoving |= IsFixed() || gotSlopedGlue || NotOnlyBlocksterInChunk();
		unmoving |= isConnectedToUnsupportedActionBlock;
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		RemoveMeshFromSkeleton();
		ParentToSkeleton();
		base.Play();
		canUseCharacterShadow = !Blocksworld.renderingShadows;
		foreach (Block item in list)
		{
			if (item is BlockWalkable && item != this)
			{
				canUseCharacterShadow = false;
			}
		}
		if (canUseCharacterShadow)
		{
			HashSet<Predicate> manyPreds = new HashSet<Predicate> { predicateCharacterMover };
			canUseCharacterShadow = ContainsTileWithAnyPredicateInPlayMode2(manyPreds);
		}
		if (canUseCharacterShadow)
		{
			if (prefabCharacterShadow == null)
			{
				prefabCharacterShadow = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Character Shadow")) as GameObject;
				prefabCharacterShadow.SetActive(value: false);
				Material material = prefabCharacterShadow.GetComponent<Renderer>().material;
				if (material == null)
				{
					BWLog.Info("Character shadow material was null");
				}
			}
			if (goShadow != null)
			{
				UnityEngine.Object.Destroy(goShadow);
			}
			InstantiateShadow(prefabCharacterShadow);
			goShadow.GetComponent<Renderer>().enabled = true;
		}
		if (stateHandler != null)
		{
			buoyancyMultiplier = 25f;
			stateHandler.Play();
			HideSourceBlockster();
		}
		_playingAttack = false;
	}

	public override void Play2()
	{
		base.Play2();
		BoxCollider component = go.GetComponent<BoxCollider>();
		PhysicMaterial physicMaterialTexture = MaterialTexture.GetPhysicMaterialTexture("Blockster");
		if (component != null && physicMaterialTexture != null)
		{
			component.material = physicMaterialTexture;
		}
	}

	public override bool CanMergeShadow()
	{
		return false;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		Activate();
		gotSlopedGlue = false;
		if (stateHandler != null)
		{
			stateHandler.Stop();
			stateHandler.SetRole(DefaultRoleForCharacterType(characterType));
			HideSourceBlockster();
			RemoveMeshFromSkeleton();
			if (headClone != null)
			{
				headClone.enabled = true;
				HideSourceBlockster();
			}
			ParentToSkeleton();
			SizeMini();
		}
		GetCloneHead().enabled = true;
		if (walkController != null)
		{
			walkController.CancelPull();
		}
		ResetBodyParts();
	}

	private void RemoveMeshFromSkeleton()
	{
		if (characterPieces == null)
		{
			return;
		}
		for (int i = 0; i < characterPieces.Count; i++)
		{
			characterPieces[i].parent = goT;
			if (characterPieces[i].name == "Character Short Hair" || characterPieces[i].name == "Character Head")
			{
				characterPieces[i].localPosition = Vector3.zero;
				characterPieces[i].localEulerAngles = Vector3.zero;
			}
			Collider component = characterPieces[i].GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	public override void IgnoreRaycasts(bool value)
	{
		base.IgnoreRaycasts(value);
		int layer = (int)((!value) ? goLayerAssignment : Layer.IgnoreRaycast);
		middle.layer = layer;
		if (hands[0] != null)
		{
			hands[0].layer = layer;
		}
		if (hands[1] != null)
		{
			hands[1].layer = layer;
		}
	}

	public override void SetFPCGearVisible(bool visible)
	{
		if (headClone != null)
		{
			headClone.enabled = visible;
		}
		if (hair != null)
		{
			hair.gameObject.GetComponent<MeshRenderer>().enabled = visible;
		}
		base.SetFPCGearVisible(visible);
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (lockPaintAndTexture)
		{
			return TileResultCode.False;
		}
		if (bodyParts == null)
		{
			return TileResultCode.False;
		}
		if (permanent && meshIndex == 1 && GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && GetDefaultTexture(1) != "Plain")
		{
			TextureTo("Clothing Underwear", Vector3.forward, permanent, 1);
		}
		TileResultCode tileResultCode = base.PaintTo(paint, permanent, meshIndex);
		if (TileResultCode.True != tileResultCode)
		{
			return tileResultCode;
		}
		if (meshIndex == 0 && headClone != null)
		{
			CopySourceHeadToHeadClone();
			HideSourceBlockster();
		}
		switch (meshIndex)
		{
		case 0:
			bodyParts.PaintSkinColor(paint, permanent);
			break;
		case 1:
			bodyParts.PaintShirtColor(paint, permanent);
			break;
		default:
		{
			if (meshIndex > subMeshGameObjects.Count)
			{
				break;
			}
			GameObject subMeshGameObject = GetSubMeshGameObject(meshIndex);
			BodyPartInfo component = subMeshGameObject.GetComponent<BodyPartInfo>();
			if (component != null)
			{
				component.currentPaint = paint;
				if (component.colorGroup != BodyPartInfo.ColorGroup.None)
				{
					bodyParts.PaintColorGroup(component.colorGroup, paint, permanent);
				}
			}
			break;
		}
		}
		return tileResultCode;
	}

	private void CopySourceHeadToHeadClone()
	{
		Renderer component = go.GetComponent<MeshRenderer>();
		MeshFilter component2 = go.GetComponent<MeshFilter>();
		Mesh mesh = null;
		if (component2 != null)
		{
			mesh = component2.sharedMesh;
		}
		Renderer cloneHead = GetCloneHead();
		Mesh mesh2 = ((!(cloneHead.GetComponent<MeshFilter>() != null)) ? null : cloneHead.GetComponent<MeshFilter>().mesh);
		if (mesh != null && mesh2 != null)
		{
			if (mesh.vertexCount == mesh2.vertexCount)
			{
				mesh2.uv = mesh.uv;
			}
			else
			{
				BWLog.Info("Mesh vert count mismatch, source verts: " + mesh.vertexCount + " targetVerts " + mesh2.vertexCount);
			}
		}
		if (component != null && cloneHead != null)
		{
			cloneHead.sharedMaterial = component.sharedMaterial;
		}
	}

	public override TileResultCode IsTexturedTo(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int meshIndex = 1;
		string text = (string)args[0];
		if (IsCharacterFaceTexture(text))
		{
			meshIndex = 0;
		}
		if (GetTexture(meshIndex) == text)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public override TileResultCode TextureToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string text = (string)args[0];
		Vector3 newNormal = (Vector3)args[1];
		int meshIndex = 1;
		if (args.Length > 2)
		{
			meshIndex = (int)args[2];
		}
		else if (IsCharacterFaceTexture(text))
		{
			meshIndex = 0;
			if (!BlockType().EndsWith("Skeleton"))
			{
				int meshIndex2 = 6;
				if (IsCharacterFaceWrapAroundTexture(text))
				{
					base.TextureToAction(text, newNormal, meshIndex2);
				}
				else
				{
					base.TextureToAction("Plain", newNormal, meshIndex2);
				}
			}
		}
		else if (Materials.IsMaterialShaderTexture(text))
		{
			meshIndex = 0;
			for (int i = 1; i <= subMeshGameObjects.Count; i++)
			{
				base.TextureToAction(text, newNormal, i);
			}
		}
		return base.TextureToAction(text, newNormal, meshIndex);
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (lockPaintAndTexture)
		{
			return TileResultCode.False;
		}
		if (permanent && meshIndex == 1 && texture == "Plain" && Block.skinPaints.Contains(GetPaint(1)) && GetDefaultTexture(1) != "Plain")
		{
			texture = "Clothing Underwear";
		}
		if (meshIndex > 0 && meshIndex <= subMeshGameObjects.Count)
		{
			GameObject subMeshGameObject = GetSubMeshGameObject(meshIndex);
			BodyPartInfo component = subMeshGameObject.GetComponent<BodyPartInfo>();
			if (component != null)
			{
				if (Materials.IsMaterialShaderTexture(texture))
				{
					if (!component.canBeMaterialTextured)
					{
						return TileResultCode.False;
					}
				}
				else if (component.canBeTextured)
				{
					return TileResultCode.False;
				}
			}
		}
		TileResultCode tileResultCode = base.TextureTo(texture, normal, permanent, meshIndex, force);
		if (tileResultCode != TileResultCode.True)
		{
			Debug.Log("Texture Failed " + texture + " to mesh index " + meshIndex + " result " + tileResultCode);
			return tileResultCode;
		}
		if (meshIndex == 0)
		{
			if (null != headClone)
			{
				CopySourceHeadToHeadClone();
				HideSourceBlockster();
			}
		}
		else if (meshIndex <= subMeshGameObjects.Count)
		{
			GameObject subMeshGameObject2 = GetSubMeshGameObject(meshIndex);
			BodyPartInfo component2 = subMeshGameObject2.GetComponent<BodyPartInfo>();
			if (component2 != null)
			{
				component2.currentTexture = texture;
				if (component2.colorGroup != BodyPartInfo.ColorGroup.None)
				{
					bodyParts.TextureColorGroup(component2.colorGroup, texture, normal, permanent);
				}
			}
		}
		return tileResultCode;
	}

	protected override void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
	{
		Renderer component = go.GetComponent<Renderer>();
		bool enabled = component.enabled;
		component.enabled = true;
		base.UpdateBlockPropertiesForTextureAssignment(meshIndex, forceEnabled);
		GetCloneHead().enabled = component.enabled;
		component.enabled = enabled;
	}

	public TileResultCode Stand(ScriptRowExecutionInfo eInfo, object[] args)
	{
		stateHandler.InterruptState(CharacterState.StandUp);
		return TileResultCode.True;
	}

	public TileResultCode Sit(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.currentState == CharacterState.SitDown)
		{
			if (stateHandler.playAnimFinished)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}
		stateHandler.InterruptState(CharacterState.SitDown);
		if (stateHandler.currentState == CharacterState.SitDown)
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode IsCollapsed(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.currentState == CharacterState.Collapsed)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsNotCollapsed(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = stateHandler.currentState == CharacterState.Collapsed;
		flag |= stateHandler.currentState == CharacterState.Collapse;
		if (flag | (stateHandler.currentState == CharacterState.Recover))
		{
			return TileResultCode.False;
		}
		return TileResultCode.True;
	}

	public TileResultCode Collapse(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.currentState == CharacterState.Collapse)
		{
			if (stateHandler.playAnimFinished)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}
		if (stateHandler.currentState == CharacterState.Collapsed)
		{
			return TileResultCode.True;
		}
		stateHandler.InterruptState(CharacterState.Collapse);
		if (stateHandler.currentState == CharacterState.Collapse)
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode Recover(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.currentState == CharacterState.Recover)
		{
			if (stateHandler.playAnimFinished)
			{
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}
		stateHandler.InterruptState(CharacterState.Recover);
		if (stateHandler.currentState == CharacterState.Recover)
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public void SetLayer(Layer layerEnum)
	{
		foreach (GameObject allBodyPartObject in GetAllBodyPartObjects())
		{
			allBodyPartObject.layer = (int)layerEnum;
		}
		middle.layer = (int)layerEnum;
		go.layer = (int)layerEnum;
		goLayerAssignment = layerEnum;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!broken && !unmoving)
		{
			walkController.torqueMultiplier = 1f;
			chunk.SetAngularDragMultiplier(1f);
		}
	}

	private void SetHandsAndBodyVisibleState(bool v)
	{
		Shader shader = InvisibleShader;
		for (int i = 0; i < hands.Length; i++)
		{
			GameObject gameObject = hands[i];
			if (gameObject.GetComponent<Renderer>().sharedMaterial.shader != shader)
			{
				gameObject.GetComponent<Renderer>().enabled = v;
				gameObject.SetActive(v);
			}
			gameObject.GetComponent<Collider>().enabled = v;
		}
		if (middle.GetComponent<Renderer>().sharedMaterial.shader != shader)
		{
			middle.GetComponent<Renderer>().enabled = v;
			middle.SetActive(v);
		}
		middle.GetComponent<Collider>().enabled = v;
	}

	public override void Vanished()
	{
		stateHandler.SaveAnimatorState();
		Deactivate();
		if (broken)
		{
			SetHandsAndBodyVisibleState(v: false);
		}
	}

	public override void Appeared()
	{
		Activate();
		base.Appeared();
		if (broken)
		{
			SetHandsAndBodyVisibleState(v: true);
		}
		stateHandler.RestoreAnimatorState();
	}

	public override List<GameObject> GetIgnoreRaycastGOs()
	{
		List<GameObject> list = base.GetIgnoreRaycastGOs();
		list.Add(middle);
		return list;
	}

	protected override void UpdateShadow()
	{
		if (!canUseCharacterShadow)
		{
			base.UpdateShadow();
			return;
		}
		Collider component = goT.GetComponent<Collider>();
		Vector3 origin = component.bounds.center + Vector3.down * component.bounds.extents.y * 0.9f;
		RaycastHit hitInfo = default(RaycastHit);
		float num = 50f;
		for (int i = 0; i < feet.Length; i++)
		{
			if (feet[i].go != null)
			{
				feet[i].go.layer = 2;
			}
		}
		if (Physics.Raycast(origin, Vector3.down, out hitInfo, num, 4113))
		{
			float num2 = Mathf.Clamp(0.25f - hitInfo.distance * (0.25f / num), 0f, 1f);
			num2 *= Mathf.Clamp(hitInfo.normal.y, 0f, 1f);
			SetShadowAlpha(num2);
			Transform transform = goShadow.transform;
			Vector3 point = hitInfo.point;
			transform.position = point + Vector3.up * 0.02f;
			float num3 = 0.6f;
			transform.rotation = Quaternion.FromToRotation(toDirection: oldShadowHitNormal = (num3 * oldShadowHitNormal + (1f - num3) * hitInfo.normal).normalized, fromDirection: Vector3.up);
			Vector3 vector = Vector3.Cross(hitInfo.normal, -goT.right);
			if (!broken && footLt != null && footRt != null && walkController != null && Blocksworld.CurrentState == State.Play)
			{
				stateControllers[this].GetState();
				_ = 3;
			}
		}
		else
		{
			SetShadowAlpha(0f);
		}
	}

	public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		SetLayer(Layer.ChunkedBlock);
		for (int i = 0; i < connections.Count; i++)
		{
			Block block = connections[i];
			int num = connectionTypes[i];
			if (num == 1)
			{
				block.go.SetLayer(Layer.ChunkedBlock);
			}
		}
		RemoveMeshFromSkeleton();
		return base.Explode(eInfo, args);
	}

	public override TileResultCode SmashLocal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		TileResultCode result = base.SmashLocal(eInfo, args);
		Break(Vector3.zero, Vector3.zero, Vector3.zero);
		return result;
	}

	public override List<Collider> GetColliders()
	{
		List<Collider> colliders = base.GetColliders();
		foreach (GameObject allBodyPartObject in GetAllBodyPartObjects())
		{
			Collider component = allBodyPartObject.GetComponent<Collider>();
			if (component != null)
			{
				colliders.Add(component);
			}
		}
		Collider component2 = middle.GetComponent<Collider>();
		if (component2 != null)
		{
			colliders.Add(component2);
		}
		return colliders;
	}

	public override void Activate()
	{
		if (goShadow != null)
		{
			goShadow.SetActive(value: true);
		}
		if (go == null)
		{
			return;
		}
		Collider component = go.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = true;
		}
		foreach (object item in stateHandler.targetRig.transform)
		{
			Transform transform = (Transform)item;
			transform.gameObject.SetActive(value: true);
		}
	}

	public override void Deactivate()
	{
		if (goShadow != null)
		{
			goShadow.SetActive(value: false);
		}
		Collider component = go.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = false;
		}
		GameObject targetRig = stateHandler.targetRig;
		if (targetRig == null)
		{
			return;
		}
		foreach (object item in targetRig.transform)
		{
			Transform transform = (Transform)item;
			transform.gameObject.SetActive(value: false);
		}
	}

	public override TileResultCode Freeze(bool informModelBlocks)
	{
		stateHandler.Freeze();
		return base.Freeze(informModelBlocks);
	}

	public override void Unfreeze()
	{
		base.Unfreeze();
		stateHandler.Unfreeze();
	}

	public override void ChunkInModelFrozen()
	{
		base.ChunkInModelFrozen();
		stateHandler.Freeze();
	}

	public override void ChunkInModelUnfrozen()
	{
		base.ChunkInModelUnfrozen();
		stateHandler.Unfreeze();
	}

	public TileResultCode StandingAttack(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (canceledAnimOnRow == eInfo.rowIndex)
		{
			canceledAnimOnRow = -1;
			return TileResultCode.False;
		}
		string stringArgSafe = Util.GetStringArgSafe(args, 0, string.Empty);
		CharacterState characterState = (CharacterState)Enum.Parse(typeof(CharacterState), stringArgSafe, ignoreCase: true);
		if (stateHandler.InAttack())
		{
			if (!stateHandler.CanStartNewStandingAttack())
			{
				return TileResultCode.Delayed;
			}
			if (_playingAttack)
			{
				_playingAttack = false;
				if (currentAnimRow != eInfo.rowIndex)
				{
					canceledAnimOnRow = currentAnimRow;
				}
				return TileResultCode.True;
			}
			stateHandler.StandingAttack(characterState);
			currentAnimRow = eInfo.rowIndex;
			_playingAttack = true;
			return TileResultCode.Delayed;
		}
		if (_playingAttack)
		{
			currentAnimRow = -1;
			_playingAttack = false;
			return TileResultCode.True;
		}
		stateHandler.StandingAttack(characterState);
		if (stateHandler.GetState() == characterState)
		{
			currentAnimRow = eInfo.rowIndex;
			_playingAttack = true;
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode Attack(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (canceledAnimOnRow == eInfo.rowIndex)
		{
			canceledAnimOnRow = -1;
			return TileResultCode.False;
		}
		string stringArgSafe = Util.GetStringArgSafe(args, 0, string.Empty);
		UpperBodyState upperBodyState = (UpperBodyState)Enum.Parse(typeof(UpperBodyState), stringArgSafe, ignoreCase: true);
		if (stateHandler.InAttack())
		{
			if (!stateHandler.CanStartNewAttack())
			{
				return TileResultCode.Delayed;
			}
			if (_playingAttack)
			{
				_playingAttack = false;
				if (currentAnimRow != eInfo.rowIndex)
				{
					canceledAnimOnRow = currentAnimRow;
				}
				return TileResultCode.True;
			}
			stateHandler.Attack(upperBodyState);
			_playingAttack = true;
			currentAnimRow = eInfo.rowIndex;
			return TileResultCode.Delayed;
		}
		if (_playingAttack)
		{
			currentAnimRow = -1;
			_playingAttack = false;
			return TileResultCode.True;
		}
		stateHandler.Attack(upperBodyState);
		if (stateHandler.upperBody.GetState() == upperBodyState)
		{
			currentAnimRow = eInfo.rowIndex;
			_playingAttack = true;
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public TileResultCode Shield(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler == null)
		{
			return TileResultCode.False;
		}
		string stringArgSafe = Util.GetStringArgSafe(args, 0, "ShieldBlockLeft");
		UpperBodyState shieldState = (UpperBodyState)Enum.Parse(typeof(UpperBodyState), stringArgSafe, ignoreCase: true);
		stateHandler.Shield(shieldState);
		return TileResultCode.True;
	}

	public TileResultCode Dodge(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArgSafe = Util.GetStringArgSafe(args, 0, "Left");
		CharacterState characterState = ((!(stringArgSafe == "Right")) ? CharacterState.DodgeLeft : CharacterState.DodgeRight);
		float dodgeSpeed = stateHandler.dodgeSpeed;
		Vector3 dir = ((characterState != CharacterState.DodgeLeft) ? Vector3.left : Vector3.right);
		if (stateHandler.currentState == characterState)
		{
			walkController.Translate(dir, dodgeSpeed);
			return TileResultCode.Delayed;
		}
		if (stateHandler.currentState == CharacterState.DodgeLeft || stateHandler.currentState == CharacterState.DodgeRight)
		{
			return TileResultCode.False;
		}
		if (_playingDodge && currentAnimRow == eInfo.rowIndex)
		{
			_playingDodge = false;
			return TileResultCode.True;
		}
		stateHandler.InterruptState(characterState);
		if (stateHandler.currentState == characterState)
		{
			currentAnimRow = eInfo.rowIndex;
			_playingDodge = true;
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	public static bool HitByHandAttachment(Block block)
	{
		if (block is BlockAnimatedCharacter)
		{
			return HitModelByHandAttachment(block);
		}
		bool flag = false;
		BlockAnimatedCharacter blockAnimatedCharacter = FindBlockOwner(block);
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			BlockAnimatedCharacter targetObject = value.targetObject;
			if (targetObject != blockAnimatedCharacter)
			{
				flag |= value.HitByHandAttachment(block);
			}
		}
		return flag;
	}

	public static bool HitByFoot(Block block)
	{
		if (block is BlockAnimatedCharacter)
		{
			return HitModelByFoot(block);
		}
		bool flag = false;
		BlockAnimatedCharacter blockAnimatedCharacter = FindBlockOwner(block);
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			BlockAnimatedCharacter targetObject = value.targetObject;
			if (targetObject != blockAnimatedCharacter)
			{
				flag |= value.HitByFoot(block);
			}
		}
		return flag;
	}

	public static bool HitModelByFoot(Block block)
	{
		bool flag = false;
		BlockAnimatedCharacter blockAnimatedCharacter = FindBlockOwner(block);
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			BlockAnimatedCharacter targetObject = value.targetObject;
			if (targetObject != blockAnimatedCharacter)
			{
				flag |= value.HitByFoot(block);
				flag |= value.HitModelByFoot(block.modelBlock);
			}
		}
		return flag;
	}

	public static bool HitByTaggedHandAttachment(Block block, string tag)
	{
		bool flag = false;
		BlockAnimatedCharacter blockAnimatedCharacter = FindBlockOwner(block);
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			BlockAnimatedCharacter targetObject = value.targetObject;
			if (targetObject != blockAnimatedCharacter)
			{
				flag |= value.HitByTaggedHandAttachment(block, tag);
			}
		}
		return flag;
	}

	public static bool HitModelByHandAttachment(Block block)
	{
		bool flag = false;
		BlockAnimatedCharacter blockAnimatedCharacter = FindBlockOwner(block);
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			BlockAnimatedCharacter targetObject = value.targetObject;
			if (targetObject != blockAnimatedCharacter)
			{
				flag |= value.HitByHandAttachment(block);
				flag |= value.HitModelByHandAttachment(block.modelBlock);
			}
		}
		return flag;
	}

	public static bool HitModelByTaggedHandAttachment(Block block, string tag)
	{
		bool flag = false;
		BlockAnimatedCharacter blockAnimatedCharacter = FindBlockOwner(block);
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			BlockAnimatedCharacter targetObject = value.targetObject;
			if (targetObject != blockAnimatedCharacter)
			{
				flag |= value.HitModelByTaggedHandAttachment(block.modelBlock, tag);
			}
		}
		return flag;
	}

	public static BlockAnimatedCharacter FindAttackingPropHolder(Block block)
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in stateControllers)
		{
			BlockAnimatedCharacter key = stateController.Key;
			CharacterStateHandler value = stateController.Value;
			Block rightHandAttachment = value.combatController.GetRightHandAttachment();
			if (value.combatController.rightHandAttachmentIsAttacking && rightHandAttachment == block)
			{
				return key;
			}
			Block leftHandAttachment = value.combatController.GetLeftHandAttachment();
			if (value.combatController.leftHandAttachmentIsAttacking && leftHandAttachment == block)
			{
				return key;
			}
		}
		return null;
	}

	public static BlockAnimatedCharacter FindBlockingPropHolder(Block block)
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in stateControllers)
		{
			BlockAnimatedCharacter key = stateController.Key;
			CharacterStateHandler value = stateController.Value;
			Block rightHandAttachment = value.combatController.GetRightHandAttachment();
			if (value.combatController.rightHandAttachmentIsShielding && rightHandAttachment == block)
			{
				return key;
			}
			Block leftHandAttachment = value.combatController.GetLeftHandAttachment();
			if (value.combatController.leftHandAttachmentIsShielding && leftHandAttachment == block)
			{
				return key;
			}
		}
		return null;
	}

	public static BlockAnimatedCharacter FindBlockOwner(Block block)
	{
		if (block is BlockAnimatedCharacter)
		{
			return block as BlockAnimatedCharacter;
		}
		return FindPropHolder(block);
	}

	public static BlockAnimatedCharacter FindPropHolder(Block block)
	{
		foreach (KeyValuePair<BlockAnimatedCharacter, CharacterStateHandler> stateController in stateControllers)
		{
			BlockAnimatedCharacter key = stateController.Key;
			if (key.IsAttachment(block))
			{
				return key;
			}
		}
		return null;
	}

	public void ShieldHitReact(Block shieldBlock)
	{
		Block rightHandAttachment = stateHandler.combatController.GetRightHandAttachment();
		Block leftHandAttachment = stateHandler.combatController.GetLeftHandAttachment();
		if (shieldBlock == rightHandAttachment)
		{
			stateHandler.combatController.HitRightShield();
		}
		else if (shieldBlock == leftHandAttachment)
		{
			stateHandler.combatController.HitLeftShield();
		}
	}

	public override bool CanRepelAttack(Vector3 attackPosition, Vector3 attackDirection)
	{
		if (!base.CanRepelAttack(attackPosition, attackDirection))
		{
			return IsShieldingFromAttack(attackPosition, attackDirection);
		}
		return true;
	}

	public override void OnAttacked(Vector3 attackPosition, Vector3 attackDirection)
	{
		stateHandler.QueueHitReact(attackDirection);
	}

	public bool IsShieldingFromAttack(Vector3 attackPosition, Vector3 attackDirection)
	{
		Block leftHandAttachment = stateHandler.combatController.GetLeftHandAttachment();
		Block rightHandAttachment = stateHandler.combatController.GetRightHandAttachment();
		bool flag = leftHandAttachment != null && Invincibility.IsInvincible(leftHandAttachment);
		bool flag2 = rightHandAttachment != null && Invincibility.IsInvincible(rightHandAttachment);
		bool flag3 = false;
		if (flag)
		{
			Vector3 forward = leftHandAttachment.goT.forward;
			bool flag4 = Vector3.Dot(attackDirection, forward) < -0.12f;
			bool flag5 = (leftHandAttachment.GetPosition() - GetPosition()).sqrMagnitude < (attackPosition - GetPosition()).sqrMagnitude;
			flag3 = flag3 || (flag4 && flag5);
		}
		if (flag2)
		{
			Vector3 forward2 = rightHandAttachment.goT.forward;
			bool flag6 = Vector3.Dot(attackDirection, forward2) < -0.12f;
			bool flag7 = (rightHandAttachment.GetPosition() - GetPosition()).sqrMagnitude < (attackPosition - GetPosition()).sqrMagnitude;
			flag3 = flag3 || (flag6 && flag7);
		}
		return flag3;
	}

	public static bool FiredAsWeapon(Block block)
	{
		bool flag = false;
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			flag |= value.FiredAsWeapon(block);
		}
		return flag;
	}

	public static void ClearAttackFlags()
	{
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			value.ClearAttackFlags();
		}
	}

	public TileResultCode QueueState(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler == null)
		{
			return TileResultCode.True;
		}
		stateHandler.QueueState((CharacterState)Enum.Parse(typeof(CharacterState), Util.GetStringArg(args, 0, "Idle")));
		return TileResultCode.True;
	}

	public TileResultCode InterruptState(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler == null)
		{
			return TileResultCode.True;
		}
		stateHandler.InterruptState((CharacterState)Enum.Parse(typeof(CharacterState), Util.GetStringArg(args, 0, "Idle")));
		return TileResultCode.True;
	}

	public TileResultCode PlayAnim(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler == null)
		{
			return TileResultCode.True;
		}
		stateHandler.requestingPlayAnim = true;
		string stringArg = Util.GetStringArg(args, 0, string.Empty);
		if (string.IsNullOrEmpty(stringArg))
		{
			return TileResultCode.True;
		}
		if (canceledAnimOnRow == eInfo.rowIndex)
		{
			canceledAnimOnRow = -1;
			return TileResultCode.False;
		}
		if (stateHandler.targetController.IsInTransition(0))
		{
			return TileResultCode.Delayed;
		}
		if (stateHandler.currentState == CharacterState.PlayAnim)
		{
			if (!stateHandler.playAnimFinished)
			{
				if (eInfo.rowIndex != currentAnimRow)
				{
					stateHandler.PlayAnim(stringArg);
					canceledAnimOnRow = currentAnimRow;
					currentAnimRow = eInfo.rowIndex;
					return TileResultCode.Delayed;
				}
				return TileResultCode.Delayed;
			}
			if (stateHandler.playAnimCurrent != null)
			{
				stateHandler.ClearAnimation();
				return TileResultCode.True;
			}
			stateHandler.PlayAnim(stringArg);
			currentAnimRow = eInfo.rowIndex;
			return TileResultCode.Delayed;
		}
		if (stateHandler.currentState != CharacterState.PlayAnim && stateHandler.currentState != CharacterState.Idle && stateHandler.playAnimCurrent != null)
		{
			stateHandler.ClearAnimation();
			return TileResultCode.False;
		}
		stateHandler.PlayAnim(stringArg);
		currentAnimRow = eInfo.rowIndex;
		return TileResultCode.Delayed;
	}

	public TileResultCode DebugPlayAnim(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler == null)
		{
			return TileResultCode.True;
		}
		stateHandler.DebugPlayAnim(Util.GetStringArg(args, 0, "Idle"));
		return TileResultCode.True;
	}

	public TileResultCode ToggleCrawl(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler == null)
		{
			return TileResultCode.True;
		}
		if (stateHandler.IsCrawling())
		{
			stateHandler.InterruptState(CharacterState.CrawlExit);
		}
		else
		{
			stateHandler.InterruptState(CharacterState.CrawlEnter);
		}
		return TileResultCode.True;
	}

	public new TileResultCode IsJumping(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.IsJumping())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode CanDoubleJump(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.CanDoubleJump())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsSwimming(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.IsSwimming())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsCrawling(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.IsCrawling())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsDodging(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.IsDodging())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsWalking(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.IsWalking())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsProne(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (stateHandler.IsProne())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode DoubleJump(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float power = (float)args[0] * eInfo.floatArg;
		if (stateHandler.DoubleJump(power))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode ApplyDiveForce(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float force = (float)args[0] * eInfo.floatArg;
		if (stateHandler.ApplyDiveForce(force))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsRole(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArgSafe = Util.GetStringArgSafe(args, 0, "Male");
		CharacterRole characterRole = (CharacterRole)Enum.Parse(typeof(CharacterRole), stringArgSafe);
		if (characterRole == stateHandler.currentRole)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetRole(ScriptRowExecutionInfo eInfo, object[] args)
	{
		string stringArgSafe = Util.GetStringArgSafe(args, 0, "Male");
		CharacterRole characterRole = (CharacterRole)Enum.Parse(typeof(CharacterRole), stringArgSafe);
		CharacterRole characterRole2 = DefaultRoleForCharacterType(characterType);
		bool flag = characterRole2 == CharacterRole.Mini || characterRole2 == CharacterRole.MiniFemale;
		if (flag && characterRole == CharacterRole.Male)
		{
			characterRole = CharacterRole.Mini;
		}
		if (flag && characterRole == CharacterRole.Female)
		{
			characterRole = CharacterRole.MiniFemale;
		}
		if (characterRole != stateHandler.currentRole)
		{
			stateHandler.SetRole(characterRole);
			stateHandler.InterruptState(stateHandler.currentState);
		}
		return TileResultCode.True;
	}

	public TileResultCode ResetRole(ScriptRowExecutionInfo eInfo, object[] args)
	{
		CharacterRole characterRole = DefaultRoleForCharacterType(characterType);
		if (characterRole != stateHandler.currentRole)
		{
			stateHandler.SetRole(characterRole);
			stateHandler.InterruptState(stateHandler.currentState);
		}
		return TileResultCode.True;
	}

	public TileResultCode ReplaceBodyPart(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (bodyParts == null)
		{
			return TileResultCode.False;
		}
		string stringArgSafe = Util.GetStringArgSafe(args, 0, string.Empty);
		string stringArgSafe2 = Util.GetStringArgSafe(args, 1, string.Empty);
		if (string.IsNullOrEmpty(stringArgSafe) || string.IsNullOrEmpty(stringArgSafe2))
		{
			return TileResultCode.False;
		}
		BlocksterBody.BodyPart bodyPart = (BlocksterBody.BodyPart)Enum.Parse(typeof(BlocksterBody.BodyPart), stringArgSafe2);
		SubstituteBodyPart(bodyPart, stringArgSafe, applyDefaultPaints: false);
		return TileResultCode.True;
	}

	public void SubstituteBodyPart(BlocksterBody.BodyPart bodyPart, string partStr, bool applyDefaultPaints)
	{
		bodyParts.SubstitutePart(bodyPart, partStr);
		RebuildSubMeshes();
		UpdateCachedBodyParts();
		if (applyDefaultPaints)
		{
			bodyParts.ApplyDefaultPaints(bodyPart);
			RebuildSubMeshPaintTiles();
		}
	}

	public void SetLimbsToDefaults()
	{
		string text = "Limb Default";
		if (characterType == CharacterType.Skeleton)
		{
			text = "Limb Skeleton";
		}
		bool flag = lockPaintAndTexture;
		lockPaintAndTexture = false;
		SubstituteBodyPart(BlocksterBody.BodyPart.LeftArm, text + " Arm Left", applyDefaultPaints: true);
		SubstituteBodyPart(BlocksterBody.BodyPart.RightArm, text + " Arm Right", applyDefaultPaints: true);
		SubstituteBodyPart(BlocksterBody.BodyPart.LeftLeg, text + " Leg Left", applyDefaultPaints: true);
		SubstituteBodyPart(BlocksterBody.BodyPart.RightLeg, text + " Leg Right", applyDefaultPaints: true);
		SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.LeftArm);
		SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.RightArm);
		SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.LeftLeg);
		SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart.RightLeg);
		lockPaintAndTexture = flag;
	}

	public void SaveBodyPartAssignmentToTiles(BlocksterBody.BodyPart bodyPart)
	{
		List<Tile> list = tiles[0];
		int num = 0;
		string text = bodyPart.ToString();
		string text2 = bodyParts.currentBodyPartVersions[bodyPart];
		foreach (Tile item in list)
		{
			string name = item.gaf.Predicate.Name;
			if (name == "AnimCharacter.ReplaceBodyPart" && item.gaf.Args.Length > 1 && (string)item.gaf.Args[1] == text)
			{
				break;
			}
			num++;
		}
		if (num < list.Count)
		{
			Tile tile = list[num];
			tile.gaf.Args[0] = text2;
		}
		else
		{
			list.Add(new Tile(new GAF("AnimCharacter.ReplaceBodyPart", text2, text)));
		}
	}

	public string CurrentlyAssignedPartVersion(BlocksterBody.BodyPart bodyPart)
	{
		return bodyParts.currentBodyPartVersions[bodyPart];
	}

	public void SetShaderForBodyPart(BlocksterBody.BodyPart bodyPart, ShaderType shader)
	{
		bodyParts.SetShaderForBodyPart(bodyPart, shader);
	}

	public List<GameObject> GetAllBodyPartObjects()
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.RightArm));
		list.AddRange(bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.LeftArm));
		list.AddRange(bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.RightLeg));
		list.AddRange(bodyParts.GetObjectsForBodyPart(BlocksterBody.BodyPart.LeftLeg));
		return list;
	}

	public void CalculateBodyPartsGAFUsage(Dictionary<GAF, int> gafUsage)
	{
		foreach (string value in bodyParts.currentBodyPartVersions.Values)
		{
			string text = value;
			if (value.EndsWith(" Left"))
			{
				text = value.Remove(value.Length - 5, 5);
			}
			else if (value.EndsWith(" Right"))
			{
				text = value.Remove(value.Length - 6, 6);
			}
			GAF key = new GAF("AnimCharacter.ReplaceBodyPart", text);
			if (gafUsage.ContainsKey(key))
			{
				gafUsage[key]++;
			}
			else
			{
				gafUsage[key] = 1;
			}
		}
	}

	public bool CanBounce(out float bounciness)
	{
		bool flag = NearGround(0.4f) > 0f;
		bounciness = GetGroundBounciness();
		float num = Vector3.Dot(-walkController.legsRb.velocity, walkController.groundNormal);
		if (flag && bounciness > 0f)
		{
			return num > 2f;
		}
		return false;
	}

	private float GetGroundBounciness()
	{
		return walkController.groundBounciness;
	}

	public static void PlayQueuedHitReacts()
	{
		foreach (CharacterStateHandler value in stateControllers.Values)
		{
			value.PlayQueuedHitReact();
		}
	}

	static BlockAnimatedCharacter()
	{
		stateControllers = new Dictionary<BlockAnimatedCharacter, CharacterStateHandler>();
		rightHandPlacementOffset = new Vector3(0.12f, 0.2f, 0f);
		leftHandPlacementOffset = new Vector3(-0.12f, 0.2f, 0f);
		gotGlueIndices = false;
	}

	private void ProcessBlockAttachment(Block block, Matrix4x4 worldToLocalMatrix, HashSet<Block> hashSet)
	{
		if (IsUnsupportedAttachmentType(block))
		{
			isConnectedToUnsupportedActionBlock = true;
			return;
		}
		if (block.goT == null || block.GetBlockMetaData() == null)
		{
			BWLog.Warning("Block or its transform is null");
			return;
		}
		Vector3 vector = worldToLocalMatrix.MultiplyPoint(block.goT.position) + block.GetBlockMetaData().attachOffset;
		if (attachedBottomBlock == null && vector.y < 1.1f && CheckBottomGlueForSlopes(glueMeshes[idxBottom], block.glueMeshes, block.jointMeshes))
		{
			attachedBottomBlock = block;
		}
		else if (attachedFrontBlock == null && vector.z > -1.1f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxFront], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxFront], block.jointMeshes)))
		{
			if (!CharacterEditor.IsGear(block))
			{
				attachmentsPreventLookAnim = true;
			}
			attachedFrontBlock = block;
		}
		else if (vector.z < 1.1f && IsBlockGluedToMesh(block, glueMeshes[idxBack]))
		{
			attachedBackBlocks.Add(block);
		}
		else if (attachedLeftBlock == null && IsBlockGluedToMesh(block, glueMeshes[idxLeft]))
		{
			attachedLeftBlock = block;
		}
		else if (attachedRightBlock == null && IsBlockGluedToMesh(block, glueMeshes[idxRight]))
		{
			attachedRightBlock = block;
		}
		else
		{
			if (!CharacterEditor.IsGear(block))
			{
				attachmentsPreventLookAnim = true;
			}
			attachedHeadBlocks.Add(block);
		}
		hashSet.Add(block);
	}

	private void ProcessAdditionalAttachments(HashSet<Block> hashSet)
	{
		ProcessBackBlockAttachments(hashSet);
		ProcessHeadBlockAttachments(hashSet);
		ProcessHandBlockAttachments(hashSet);
	}

	private void ProcessBackBlockAttachments(HashSet<Block> hashSet)
	{
		for (int i = 0; i < attachedBackBlocks.Count; i++)
		{
			Block block = attachedBackBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedBackBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
	}

	private void ProcessHeadBlockAttachments(HashSet<Block> hashSet)
	{
		for (int i = 0; i < attachedHeadBlocks.Count; i++)
		{
			Block block = attachedHeadBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedHeadBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
	}

	private void ProcessHandBlockAttachments(HashSet<Block> hashSet)
	{
		ProcessLeftHandBlockAttachments(hashSet);
		ProcessRightHandBlockAttachments(hashSet);
	}

	private void ProcessLeftHandBlockAttachments(HashSet<Block> hashSet)
	{
		if (attachedLeftBlock == null)
		{
			return;
		}
		attachedLeftHandBlocks.Add(attachedLeftBlock);
		for (int i = 0; i < attachedLeftHandBlocks.Count; i++)
		{
			Block block = attachedLeftHandBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedLeftHandBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
		attachedLeftHandBlocks.Remove(attachedLeftBlock);
	}

	private void ProcessRightHandBlockAttachments(HashSet<Block> hashSet)
	{
		if (attachedRightBlock == null)
		{
			return;
		}
		attachedRightHandBlocks.Add(attachedRightBlock);
		for (int i = 0; i < attachedRightHandBlocks.Count; i++)
		{
			Block block = attachedRightHandBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedRightHandBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
		attachedRightHandBlocks.Remove(attachedRightBlock);
	}

	private void ResetAttachments()
	{
		attachedBottomBlock = null;
		attachedFrontBlock = null;
		attachedLeftBlock = null;
		attachedRightBlock = null;
		if (attachedBackBlocks == null)
		{
			attachedBackBlocks = new List<Block>();
		}
		else
		{
			attachedBackBlocks.Clear();
		}
		if (attachedHeadBlocks == null)
		{
			attachedHeadBlocks = new List<Block>();
		}
		else
		{
			attachedHeadBlocks.Clear();
		}
		if (attachedLeftHandBlocks == null)
		{
			attachedLeftHandBlocks = new List<Block>();
		}
		else
		{
			attachedLeftHandBlocks.Clear();
		}
		if (attachedRightHandBlocks == null)
		{
			attachedRightHandBlocks = new List<Block>();
		}
		else
		{
			attachedRightHandBlocks.Clear();
		}
		attachmentsPreventLookAnim = false;
		isConnectedToUnsupportedActionBlock = false;
	}

	private void ProcessBlockAttachmentInternal(Block block, Matrix4x4 worldToLocalMatrix, HashSet<Block> hashSet)
	{
		if (IsUnsupportedAttachmentType(block))
		{
			isConnectedToUnsupportedActionBlock = true;
			return;
		}
		if (block.goT == null || block.GetBlockMetaData() == null)
		{
			Debug.LogWarning("Block or its transform is null");
			return;
		}
		Vector3 vector = worldToLocalMatrix.MultiplyPoint(block.goT.position) + block.GetBlockMetaData().attachOffset;
		if (attachedBottomBlock == null && vector.y < 1.1f && CheckBottomGlueForSlopes(glueMeshes[idxBottom], block.glueMeshes, block.jointMeshes))
		{
			attachedBottomBlock = block;
		}
		else if (attachedFrontBlock == null && vector.z > -1.1f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxFront], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxFront], block.jointMeshes)))
		{
			if (!CharacterEditor.IsGear(block))
			{
				attachmentsPreventLookAnim = true;
			}
			attachedFrontBlock = block;
		}
		else if (vector.z < 1.1f && IsBlockGluedToMesh(block, glueMeshes[idxBack]))
		{
			attachedBackBlocks.Add(block);
		}
		else if (attachedLeftBlock == null && IsBlockGluedToMesh(block, glueMeshes[idxLeft]))
		{
			attachedLeftBlock = block;
		}
		else if (attachedRightBlock == null && IsBlockGluedToMesh(block, glueMeshes[idxRight]))
		{
			attachedRightBlock = block;
		}
		else
		{
			if (!CharacterEditor.IsGear(block))
			{
				attachmentsPreventLookAnim = true;
			}
			attachedHeadBlocks.Add(block);
		}
		hashSet.Add(block);
	}

	private void ProcessAdditionalAttachmentsInternal(HashSet<Block> hashSet)
	{
		ProcessBackBlockAttachmentsInternal(hashSet);
		ProcessHeadBlockAttachmentsInternal(hashSet);
		ProcessHandBlockAttachmentsInternal(hashSet);
	}

	private void ProcessBackBlockAttachmentsInternal(HashSet<Block> hashSet)
	{
		for (int i = 0; i < attachedBackBlocks.Count; i++)
		{
			Block block = attachedBackBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedBackBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
	}

	private void ProcessHeadBlockAttachmentsInternal(HashSet<Block> hashSet)
	{
		for (int i = 0; i < attachedHeadBlocks.Count; i++)
		{
			Block block = attachedHeadBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedHeadBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
	}

	private void ProcessHandBlockAttachmentsInternal(HashSet<Block> hashSet)
	{
		ProcessLeftHandBlockAttachmentsInternal(hashSet);
		ProcessRightHandBlockAttachmentsInternal(hashSet);
	}

	private void ProcessLeftHandBlockAttachmentsInternal(HashSet<Block> hashSet)
	{
		if (attachedLeftBlock == null)
		{
			return;
		}
		attachedLeftHandBlocks.Add(attachedLeftBlock);
		for (int i = 0; i < attachedLeftHandBlocks.Count; i++)
		{
			Block block = attachedLeftHandBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedLeftHandBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
		attachedLeftHandBlocks.Remove(attachedLeftBlock);
	}

	private void ProcessRightHandBlockAttachmentsInternal(HashSet<Block> hashSet)
	{
		if (attachedRightBlock == null)
		{
			return;
		}
		attachedRightHandBlocks.Add(attachedRightBlock);
		for (int i = 0; i < attachedRightHandBlocks.Count; i++)
		{
			Block block = attachedRightHandBlocks[i];
			for (int j = 0; j < block.connections.Count; j++)
			{
				Block block2 = block.connections[j];
				if (!hashSet.Contains(block2))
				{
					if (IsUnsupportedAttachmentType(block2))
					{
						isConnectedToUnsupportedActionBlock = true;
					}
					else if (!(block2 is BlockAnimatedCharacter))
					{
						attachedRightHandBlocks.Add(block2);
					}
					hashSet.Add(block2);
				}
			}
		}
		attachedRightHandBlocks.Remove(attachedRightBlock);
	}
}
