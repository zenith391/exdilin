using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockCharacter : BlockAbstractLegs, IBlockster
{
	public GameObject[] hands = new GameObject[2];

	public GameObject middle;

	private ConfigurableJoint middleJoint;

	public BoxCollider collider;

	private Vector3 colliderSize;

	private Vector3 colliderCenter;

	public Block bottom;

	public Block front;

	public Block left;

	public Block right;

	public Block back;

	public List<Block> headAttachments;

	private float footHeight = -1.35f;

	private Vector3 handPos = new Vector3(0.5f, -0.5f, 0.4f);

	private float handsOutXMod = 0.7f;

	private float handsOutYMod = -0.1f;

	private Vector3 footPos = new Vector3(0.25f, -0.8f, 0.4f);

	private Vector3 footRot = new Vector3(270f, 0f, 0f);

	private Vector3 pausedVelocityHands0;

	private Vector3 pausedAngularVelocityHands0;

	private Vector3 pausedVelocityHands1;

	private Vector3 pausedAngularVelocityHands1;

	private Vector3 pausedVelocityMiddle;

	private Vector3 pausedAngularVelocityMiddle;

	private Vector3 middleLocalPosition;

	private bool hasLegMesh;

	private bool canUseCharacterShadow;

	public bool isAvatar;

	private bool lockPaintAndTexture;

	public static Predicate predicateCharacterMover;

	public static Predicate predicateCharacterTiltMover;

	public static Predicate predicateCharacterJump;

	public static Predicate predicateCharacterGotoTag;

	public static Predicate predicateCharacterChaseTag;

	public static Predicate predicateCharacterAvoidTag;

	private static int idxBottom;

	private static int idxFront;

	private static int idxLeft;

	private static int idxRight;

	private static int idxBack;

	private static bool gotGlueIndices;

	private static GameObject prefabCharacterShadow;

	private Dictionary<GameObject, BoxColliderData> boxColliderData = new Dictionary<GameObject, BoxColliderData>();

	public BlockCharacter(List<List<Tile>> tiles, Dictionary<string, string> partNames, bool hasLegMesh, float footHeight = -1.35f)
		: base(tiles, partNames)
	{
		maxStepHeight = 1.5f;
		maxStepLength = 0.75f;
		onGroundHeight = 0f - footHeight + 0.15f;
		replaceWithCapsuleCollider = true;
		if (BlockType().EndsWith("Headless"))
		{
			capsuleColliderOffset = 0.4f * Vector3.down;
			capsuleColliderHeight = 0.9f;
			capsuleColliderRadius = 0.6f;
		}
		else if (footHeight != -1.35f)
		{
			capsuleColliderOffset = 0.125f * Vector3.up;
			capsuleColliderHeight = 1.75f;
			capsuleColliderRadius = 0.7f;
		}
		else
		{
			capsuleColliderOffset = Vector3.zero;
			capsuleColliderHeight = 2f;
			capsuleColliderRadius = 0.7f;
		}
		this.hasLegMesh = hasLegMesh;
		this.footHeight = footHeight;
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
	}

	public void IBlockster_FindAttachments()
	{
		FindAttachements();
	}

	public Block IBlockster_BottomAttachment()
	{
		return bottom;
	}

	public List<Block> IBlockster_HeadAttachments()
	{
		return headAttachments;
	}

	public Block IBlockster_FrontAttachment()
	{
		return front;
	}

	public Block IBlockster_BackAttachment()
	{
		return back;
	}

	public Block IBlockster_RightHandAttachment()
	{
		return right;
	}

	public Block IBlockster_LeftHandAttachement()
	{
		return left;
	}

	public new static void Register()
	{
		predicateCharacterJump = PredicateRegistry.Add<BlockCharacter>("Character.Jump", (Block b) => ((BlockAbstractLegs)b).IsJumping, (Block b) => ((BlockAbstractLegs)b).Jump, new Type[1] { typeof(float) }, new string[1] { "Force" });
		predicateCharacterGotoTag = PredicateRegistry.Add<BlockCharacter>("Character.GotoTag", null, (Block b) => ((BlockAbstractLegs)b).GotoTag, new Type[3]
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
		predicateCharacterChaseTag = PredicateRegistry.Add<BlockCharacter>("Character.ChaseTag", null, (Block b) => ((BlockAbstractLegs)b).ChaseTag, new Type[3]
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
		PredicateRegistry.Add<BlockCharacter>("Character.GotoTap", null, (Block b) => ((BlockAbstractLegs)b).GotoTap, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Speed", "Wackiness" });
		predicateCharacterMover = PredicateRegistry.Add<BlockCharacter>("Character.DPadControl", null, (Block b) => ((BlockAbstractLegs)b).DPadControl, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		predicateCharacterTiltMover = PredicateRegistry.Add<BlockCharacter>("Character.TiltMover", null, (Block b) => b.TiltMoverControl, new Type[2]
		{
			typeof(float),
			typeof(int)
		});
		PredicateRegistry.Add<BlockCharacter>("Character.Translate", null, (Block b) => ((BlockAbstractLegs)b).Translate, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCharacter>("Character.WackyMode", null, (Block b) => ((BlockAbstractLegs)b).WackyMode);
		PredicateRegistry.Add<BlockCharacter>("Character.Turn", null, (Block b) => ((BlockAbstractLegs)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCharacter>("Character.TurnTowardsTag", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTag, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockCharacter>("Character.TurnTowardsTap", null, (Block b) => ((BlockAbstractLegs)b).TurnTowardsTap);
		PredicateRegistry.Add<BlockCharacter>("Character.TurnAlongCam", null, (Block b) => ((BlockAbstractLegs)b).TurnAlongCam);
		predicateCharacterAvoidTag = PredicateRegistry.Add<BlockCharacter>("Character.AvoidTag", null, (Block b) => ((BlockAbstractLegs)b).AvoidTag, new Type[3]
		{
			typeof(string),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCharacter>("Character.Idle", null, (Block b) => ((BlockAbstractLegs)b).Idle);
		PredicateRegistry.Add<BlockCharacter>("Character.FreezeRotation", null, (Block b) => ((BlockAbstractLegs)b).FreezeRotation);
	}

	public static void StripNonCompatibleTiles(List<List<Tile>> tiles)
	{
		HashSet<Predicate> hashSet = new HashSet<Predicate>();
		hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockCharacter)));
		hashSet.UnionWith(PredicateRegistry.ForType(typeof(BlockAbstractLegs)));
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

	public override void OnCreate()
	{
		lockPaintAndTexture = isAvatar;
	}

	public override void Destroy()
	{
		if (hands != null)
		{
			for (int i = 0; i < hands.Length; i++)
			{
				if (hands[i].gameObject != null)
				{
					UnityEngine.Object.Destroy(hands[i].gameObject);
				}
			}
		}
		if (middle != null)
		{
			UnityEngine.Object.Destroy(middle);
		}
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

	public override void FindFeet()
	{
		string name = ((!partNames.ContainsKey("Foot Right")) ? "Character Foot Right" : partNames["Foot Right"]);
		string name2 = ((!partNames.ContainsKey("Foot Left")) ? "Character Foot Left" : partNames["Foot Left"]);
		string name3 = ((!partNames.ContainsKey("Hand Right")) ? "Character Hand Right" : partNames["Hand Right"]);
		string name4 = ((!partNames.ContainsKey("Hand Left")) ? "Character Hand Left" : partNames["Hand Left"]);
		string name5 = ((!partNames.ContainsKey("Body")) ? "Character Body" : partNames["Body"]);
		feet = new FootInfo[2 * legPairCount];
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i] = new FootInfo();
		}
		feet[0].go = goT.Find(name).gameObject;
		feet[1].go = goT.Find(name2).gameObject;
		if (feet[0] != null)
		{
			footPos = feet[0].go.transform.localPosition;
			footRot = feet[0].go.transform.localEulerAngles;
		}
		hands[0] = goT.Find(name3).gameObject;
		hands[1] = goT.Find(name4).gameObject;
		middle = goT.Find(name5).gameObject;
		if (hands[0] != null && middle != null)
		{
			handPos = hands[0].transform.localPosition;
			handsOutXMod = middle.transform.localScale.x * 0.5f;
			handsOutYMod -= 0f - (0.6f + hands[0].transform.localPosition.y);
		}
		collider = go.GetComponent<BoxCollider>();
		colliderSize = collider.size;
		colliderCenter = collider.center;
		middleLocalPosition = middle.transform.localPosition;
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
		BreakCollider();
		Blocksworld.AddFixedUpdateCommand(new DelayedBreakCharacterCommand(this, chunkPos, chunkVel, chunkAngVel));
	}

	public override void BeforePlay()
	{
		FindAttachements();
		bool flag = left == null || right == null;
		replaceWithCapsuleCollider = front == null && bottom == null && flag;
		goLayerAssignment = Layer.Default;
		go.SetLayer(goLayerAssignment, recursive: true);
	}

	public void FindAttachements()
	{
		bottom = null;
		front = null;
		left = null;
		right = null;
		back = null;
		headAttachments = new List<Block>();
		Matrix4x4 worldToLocalMatrix = goT.worldToLocalMatrix;
		for (int i = 0; i < connections.Count; i++)
		{
			Block block = connections[i];
			if (!block.isTerrain)
			{
				Vector3 vector = worldToLocalMatrix.MultiplyPoint(block.goT.position);
				bool flag = false;
				if (bottom == null && vector.y < 0.5f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxBottom], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxBottom], block.jointMeshes)))
				{
					bottom = block;
					flag = true;
				}
				if (front == null && vector.z > -0.5f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxFront], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxFront], block.jointMeshes)))
				{
					front = block;
					flag = true;
				}
				if (left == null && vector.x < 0.5f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxLeft], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxLeft], block.jointMeshes)))
				{
					left = block;
					flag = true;
				}
				if (right == null && vector.x > -0.5f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxRight], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxRight], block.jointMeshes)))
				{
					right = block;
					flag = true;
				}
				if (back == null && vector.z < 0.5f && (CollisionTest.MultiMeshMeshTest2(glueMeshes[idxBack], block.glueMeshes) || CollisionTest.MultiMeshMeshTest2(glueMeshes[idxBack], block.jointMeshes)))
				{
					back = block;
					flag = true;
				}
				if (!flag)
				{
					headAttachments.Add(block);
				}
			}
		}
	}

	public override void BecameTreasure()
	{
		base.BecameTreasure();
		for (int i = 0; i < 2; i++)
		{
			FootInfo footInfo = feet[i];
			footInfo.go.transform.localPosition = new Vector3((i != 0) ? (0f - footPos.x) : footPos.x, footPos.y, footPos.z);
			footInfo.go.transform.localEulerAngles = footRot;
		}
	}

	public override void Play()
	{
		if (front != null)
		{
			Vector3 localPosition = handPos;
			localPosition.z += 0.3f;
			hands[0].transform.localPosition = localPosition;
			localPosition.x *= -1f;
			hands[1].transform.localPosition = localPosition;
		}
		keepCollider = true;
		base.Play();
		middle.GetComponent<Collider>().enabled = true;
		hands[0].GetComponent<Collider>().enabled = true;
		hands[1].GetComponent<Collider>().enabled = true;
		unmoving = unmoving || bottom != null || front != null || IsFixed();
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		canUseCharacterShadow = !Blocksworld.renderingShadows;
		foreach (Block item in list)
		{
			if (item is BlockAbstractLegs && item != this)
			{
				canUseCharacterShadow = false;
			}
		}
		if (canUseCharacterShadow)
		{
			HashSet<Predicate> manyPreds = new HashSet<Predicate> { predicateCharacterMover };
			canUseCharacterShadow = ContainsTileWithAnyPredicateInPlayMode2(manyPreds);
		}
		if (!canUseCharacterShadow)
		{
			return;
		}
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

	public override void PlayLegs1()
	{
		if (unmoving)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			FootInfo footInfo = feet[i];
			Transform transform = footInfo.go.transform;
			transform.localPosition = new Vector3((i != 0) ? (footPos.x * -1.25f) : (footPos.x * 1.25f), footHeight, 0f);
			transform.transform.localEulerAngles = Vector3.zero;
			footInfo.position = transform.position;
			FootInfo footInfo2 = footInfo;
			footInfo2.position.y = footInfo2.position.y + 0.5f;
			if ((i != 0 || right == null) && (i != 1 || left == null))
			{
				hands[i].transform.localPosition = new Vector3((i != 0) ? (0f - handsOutXMod) : handsOutXMod, -0.6f, 0f);
				hands[i].transform.localEulerAngles = new Vector3(0f, 0f, (i != 0) ? (-25f) : 25f);
			}
		}
		if (back == null)
		{
			middle.transform.localPosition = middleLocalPosition + Vector3.forward * 0.1f;
		}
		base.PlayLegs1();
	}

	public override void PlayLegs2()
	{
		if (!unmoving)
		{
			base.PlayLegs2();
			for (int i = 0; i < 2; i++)
			{
				BoxCollider component = feet[i].go.GetComponent<BoxCollider>();
				component.size = new Vector3(0.125f, 0.25f, 0.25f);
			}
			if (canUseCharacterShadow)
			{
				walkController.climbOn = true;
			}
		}
	}

	public override bool FeetPartOfGo()
	{
		if (unmoving)
		{
			return !broken;
		}
		return false;
	}

	public override bool CanMergeShadow()
	{
		return false;
	}

	protected override int GetPrimaryMeshIndex()
	{
		if (BlockType().EndsWith("Headless"))
		{
			return 1;
		}
		return 0;
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		RestoreCollider();
		middle.GetComponent<Collider>().enabled = false;
		hands[0].GetComponent<Collider>().enabled = false;
		hands[1].GetComponent<Collider>().enabled = false;
		for (int i = 0; i < 2; i++)
		{
			if (hands[i].transform.parent == null)
			{
				UnityEngine.Object.Destroy(hands[i].GetComponent<Rigidbody>());
				hands[i].transform.parent = goT;
			}
			hands[i].transform.localPosition = new Vector3((i != 0) ? (0f - handPos.x) : handPos.x, handPos.y, handPos.z);
			hands[i].transform.localEulerAngles = Vector3.zero;
			FootInfo footInfo = feet[i];
			Transform transform = footInfo.go.transform;
			if (transform.parent == null)
			{
				UnityEngine.Object.Destroy(footInfo.go.GetComponent<Rigidbody>());
				transform.parent = goT;
			}
			transform.localPosition = new Vector3((i != 0) ? (0f - footPos.x) : footPos.x, footPos.y, footPos.z);
			transform.localEulerAngles = footRot;
		}
		if (middle.transform.parent == null)
		{
			UnityEngine.Object.Destroy(middle.GetComponent<Rigidbody>());
			middle.transform.parent = goT;
			collider.size = colliderSize;
			collider.center = colliderCenter;
		}
		middle.transform.localPosition = middleLocalPosition;
		middle.transform.localEulerAngles = Vector3.zero;
	}

	public override void PositionAnkle(int i)
	{
		if (hasLegMesh)
		{
			base.PositionAnkle(i);
		}
	}

	public override void IgnoreRaycasts(bool value)
	{
		base.IgnoreRaycasts(value);
		int layer = (int)((!value) ? goLayerAssignment : Layer.IgnoreRaycast);
		middle.layer = layer;
		hands[0].layer = layer;
		hands[1].layer = layer;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (lockPaintAndTexture)
		{
			return TileResultCode.False;
		}
		if (permanent && meshIndex == 1 && GetTexture(1) == "Plain" && Block.skinPaints.Contains(paint) && GetDefaultTexture(1) != "Plain")
		{
			TextureTo("Clothing Underwear", Vector3.forward, permanent, 1);
		}
		return base.PaintTo(paint, permanent, meshIndex);
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
		int num = 1;
		if (args.Length > 2)
		{
			num = (int)args[2];
		}
		else if (IsCharacterFaceTexture(text))
		{
			num = 0;
			if (!BlockType().EndsWith("Skeleton"))
			{
				int meshIndex = 6;
				if (IsCharacterFaceWrapAroundTexture(text))
				{
					base.TextureToAction(text, newNormal, meshIndex);
				}
				else
				{
					base.TextureToAction("Plain", newNormal, meshIndex);
				}
			}
		}
		else if (Materials.IsMaterialShaderTexture(text))
		{
			num = (BlockType().EndsWith("Headless") ? 1 : 0);
			for (int i = num + 1; i <= subMeshGameObjects.Count; i++)
			{
				base.TextureToAction(text, newNormal, i);
			}
		}
		return base.TextureToAction(text, newNormal, num);
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
		return base.TextureTo(texture, normal, permanent, meshIndex, force);
	}

	public void SetLayer(Layer layerEnum)
	{
		feet[0].go.layer = (int)layerEnum;
		feet[1].go.layer = (int)layerEnum;
		hands[0].layer = (int)layerEnum;
		hands[1].layer = (int)layerEnum;
		middle.layer = (int)layerEnum;
		go.layer = (int)layerEnum;
		goLayerAssignment = layerEnum;
	}

	protected void UpdateHands()
	{
		Rigidbody component = body.GetComponent<Rigidbody>();
		Vector3 velocity = component.velocity;
		float h = 1f;
		float num = Ipm(velocity.z, h);
		float num2 = Ipm(velocity.x, h);
		for (int i = 0; i < 2; i++)
		{
			if ((i == 0 && right != null) || (i == 1 && left != null))
			{
				continue;
			}
			float num3 = 0.6f;
			GameObject gameObject = hands[i];
			if (controllerWasActive)
			{
				walkController.SetHandPosition(i, gameObject, handsOutXMod, handsOutYMod);
				continue;
			}
			Vector3 vector = new Vector3(((i != 0) ? (0f - num3) : num3) - num2, -0.6f, 0f - num);
			Vector3 vector2 = vector - gameObject.transform.localPosition;
			Vector3 localPosition = gameObject.transform.localPosition + 0.1f * vector2;
			localPosition.x = ((i != 0) ? Mathf.Clamp(localPosition.x, -1f, 0f - num3) : Mathf.Clamp(localPosition.x, num3, 1f));
			if (localPosition.sqrMagnitude < 1f)
			{
				gameObject.transform.localPosition = localPosition;
			}
			gameObject.transform.LookAt(goT.position + goT.up);
			gameObject.transform.Rotate(0f, 0f, 90f);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (broken || unmoving || chunk.IsFrozen())
		{
			return;
		}
		UpdateHands();
		Rigidbody component = body.GetComponent<Rigidbody>();
		if (back == null && !controllerWasActive && !hasLegMesh)
		{
			middle.transform.LookAt(goT.position + goT.rotation * (0.5f * hands[0].transform.localPosition + 0.5f * hands[1].transform.localPosition + new Vector3(0f, 0f, 1f)), goT.up + 0.1f * Vector3.up);
		}
		if (controllerWasActive && hasMovedCM)
		{
			walkController.torqueMultiplier = 2f;
			float num = 5f;
			chunk.SetAngularDragMultiplier(num);
			component.angularDrag = 2f * num;
			bool enabled = walkController.IsOnGround();
			for (int i = 0; i < feet.Length; i++)
			{
				feet[i].go.GetComponent<Collider>().enabled = enabled;
			}
		}
		else
		{
			walkController.torqueMultiplier = 1f;
			chunk.SetAngularDragMultiplier(1f);
		}
	}

	protected override void ReplaceCollider()
	{
		BoxCollider[] componentsInChildren = go.GetComponentsInChildren<BoxCollider>();
		BoxCollider[] array = componentsInChildren;
		foreach (BoxCollider boxCollider in array)
		{
			GameObject gameObject = boxCollider.gameObject;
			boxColliderData[gameObject] = new BoxColliderData(boxCollider);
		}
		for (int j = 0; j < hands.Length; j++)
		{
			GameObject gameObject2 = hands[j];
			if (!boxColliderData.ContainsKey(gameObject2))
			{
				BoxCollider boxCollider2 = (BoxCollider)gameObject2.GetComponent<Collider>();
				if (boxCollider2 != null)
				{
					boxColliderData[gameObject2] = new BoxColliderData(boxCollider2);
				}
			}
		}
		base.ReplaceCollider();
	}

	protected BoxCollider RestoreBoxCollider(GameObject o)
	{
		BoxCollider boxCollider = ((!(o.GetComponent<BoxCollider>() != null)) ? o.AddComponent<BoxCollider>() : o.GetComponent<BoxCollider>());
		if (this.boxColliderData.ContainsKey(o))
		{
			BoxColliderData boxColliderData = this.boxColliderData[o];
			boxCollider.center = boxColliderData.center;
			boxCollider.size = boxColliderData.size;
		}
		return boxCollider;
	}

	protected override void RestoreCollider()
	{
		bool flag = false;
		if (go.GetComponent<Collider>() is CapsuleCollider)
		{
			UnityEngine.Object.Destroy(go.GetComponent<CapsuleCollider>());
			flag = true;
		}
		if (go.GetComponent<Collider>() == null || flag)
		{
			collider = RestoreBoxCollider(go);
		}
		if (middle.GetComponent<Collider>() == null)
		{
			RestoreBoxCollider(middle);
		}
		for (int i = 0; i < hands.Length; i++)
		{
			GameObject gameObject = hands[i];
			if (gameObject.GetComponent<Collider>() == null)
			{
				RestoreBoxCollider(gameObject);
			}
		}
		boxColliderData.Clear();
	}

	private void BreakCollider()
	{
		bool flag = false;
		if (go.GetComponent<Collider>() is CapsuleCollider)
		{
			UnityEngine.Object.Destroy(go.GetComponent<CapsuleCollider>());
			flag = true;
		}
		if (BlockType().EndsWith("Headless"))
		{
			Blocksworld.blocksworldCamera.Unfollow(chunk);
			Blocksworld.chunks.Remove(chunk);
			chunk.Destroy(delayed: true);
			go.SetActive(value: false);
		}
		else if (go.GetComponent<Collider>() == null || flag)
		{
			collider = RestoreBoxCollider(go);
		}
		if (middle.GetComponent<Collider>() == null)
		{
			RestoreBoxCollider(middle);
		}
		for (int i = 0; i < hands.Length; i++)
		{
			GameObject gameObject = hands[i];
			if (gameObject.GetComponent<Collider>() == null)
			{
				RestoreBoxCollider(gameObject);
			}
		}
	}

	protected override void UnVanishFeet()
	{
		base.UnVanishFeet();
		Shader shader = InvisibleShader;
		if (middle.GetComponent<Renderer>().sharedMaterial.shader != shader)
		{
			middle.GetComponent<Renderer>().enabled = true;
			middle.SetActive(value: true);
		}
		for (int i = 0; i < hands.Length; i++)
		{
			if (hands[i].GetComponent<Renderer>().sharedMaterial.shader != shader)
			{
				hands[i].GetComponent<Renderer>().enabled = true;
				hands[i].SetActive(value: true);
			}
		}
	}

	private void SetHandsAndBodyVisibleState(bool v)
	{
		if (hands == null)
		{
			Debug.LogWarning("Hands array is null in SetHandsAndBodyVisibleState");
			return;
		}
		Shader shader = InvisibleShader;
		for (int i = 0; i < hands.Length; i++)
		{
			GameObject gameObject = hands[i];
			if (gameObject == null)
			{
				BWLog.Warning($"Hand at index {i} is null");
				continue;
			}
			Renderer component = gameObject.GetComponent<Renderer>();
			Collider component2 = gameObject.GetComponent<Collider>();
			if (component != null && component.sharedMaterial.shader != shader)
			{
				component.enabled = v;
				gameObject.SetActive(v);
			}
			if (component2 != null)
			{
				component2.enabled = v;
			}
		}
		if (middle != null)
		{
			Renderer component3 = middle.GetComponent<Renderer>();
			Collider component4 = middle.GetComponent<Collider>();
			if (component3 != null && component3.sharedMaterial.shader != shader)
			{
				component3.enabled = v;
				middle.SetActive(v);
			}
			if (component4 != null)
			{
				component4.enabled = v;
			}
		}
		else
		{
			BWLog.Warning("Middle object is null in SetHandsAndBodyVisibleState");
		}
	}

	public override void Vanished()
	{
		base.Vanished();
		if (broken)
		{
			SetHandsAndBodyVisibleState(v: false);
		}
	}

	public override void Appeared()
	{
		base.Appeared();
		if (broken)
		{
			SetHandsAndBodyVisibleState(v: true);
		}
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
		Transform transform = goT;
		Vector3 position = transform.position;
		Vector3 vector = 2.2f * Vector3.up;
		position.y -= footHeight;
		RaycastHit hitInfo = default(RaycastHit);
		float num = 50f;
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i].go.layer = 2;
		}
		if (Physics.Raycast(position - vector, -Vector3.up, out hitInfo, num, 4113))
		{
			float num2 = Mathf.Clamp(0.25f - hitInfo.distance * (0.25f / num), 0f, 1f);
			num2 *= Mathf.Clamp(hitInfo.normal.y, 0f, 1f);
			SetShadowAlpha(num2);
			Transform transform2 = goShadow.transform;
			Vector3 point = hitInfo.point;
			transform2.position = point + Vector3.up * 0.02f;
			float num3 = 0.6f;
			transform2.rotation = Quaternion.FromToRotation(toDirection: oldShadowHitNormal = (num3 * oldShadowHitNormal + (1f - num3) * hitInfo.normal).normalized, fromDirection: Vector3.up);
		}
		else
		{
			SetShadowAlpha(0f);
		}
		for (int j = 0; j < feet.Length; j++)
		{
			feet[j].go.layer = 0;
		}
	}

	protected override Vector3 GetFeetCenter()
	{
		return goT.position - Vector3.up * 1.35f;
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
		FootInfo[] array = feet;
		foreach (FootInfo footInfo in array)
		{
			colliders.Add(footInfo.go.GetComponent<Collider>());
		}
		GameObject[] array2 = hands;
		foreach (GameObject gameObject in array2)
		{
			colliders.Add(gameObject.GetComponent<Collider>());
		}
		colliders.Add(middle.GetComponent<Collider>());
		return colliders;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		for (int i = 0; i < hands.Length; i++)
		{
			hands[i].SetActive(value: false);
		}
	}

	private void SetVanished(bool forever = false)
	{
		Vanished();
		if (!vanished)
		{
			vanished = true;
			CollisionManager.WakeUpObjectsRestingOn(this);
		}
		if (goShadow != null)
		{
			Renderer component = goShadow.GetComponent<Renderer>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		if (!forever)
		{
			return;
		}
		try
		{
			if (!TreasureHandler.IsPartOfTreasureModel(this))
			{
				BWSceneManager.RemovePlayBlock(this);
			}
		}
		catch (Exception ex)
		{
			BWLog.Error("Error in SetVanished: " + ex.Message);
		}
	}
}
