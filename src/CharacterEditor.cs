using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class CharacterEditor
{
	public enum SnapTarget
	{
		None,
		Head,
		Body,
		HeadLeft,
		HeadRight,
		HeadBack,
		HeadFront,
		LeftHand,
		RightHand
	}

	private static CharacterEditor _instance;

	private BlockAnimatedCharacter _character;

	private Vector3 _restoreColliderSize;

	private List<Block> _attachedGear = new List<Block>();

	private List<Block> _displacedGear = new List<Block>();

	private HashSet<Block> _nonGearAttachments = new HashSet<Block>();

	private GameObject _TColliderGameObject;

	private Dictionary<SnapTarget, Vector3> _snapTargetPositions = new Dictionary<SnapTarget, Vector3>();

	private Dictionary<SnapTarget, Quaternion> _snapTargetRotations = new Dictionary<SnapTarget, Quaternion>();

	private Dictionary<BlocksterGearType, Quaternion> _gearSnapRotations = new Dictionary<BlocksterGearType, Quaternion>();

	private static List<SnapTarget> AllSnapTargets = new List<SnapTarget>
	{
		SnapTarget.Head,
		SnapTarget.Body,
		SnapTarget.HeadLeft,
		SnapTarget.HeadRight,
		SnapTarget.HeadBack,
		SnapTarget.HeadFront,
		SnapTarget.RightHand,
		SnapTarget.LeftHand
	};

	public static CharacterEditor Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new CharacterEditor();
			}
			return _instance;
		}
	}

	public bool GetGearUnderScreenPosition(Vector3 pos, out Block gearBlock, out SnapTarget snapTarget)
	{
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(pos * NormalizedScreen.scale);
		foreach (Block item in _attachedGear)
		{
			if (!GearHitTest(item, ray))
			{
				continue;
			}
			BlocksterGearType blocksterGearType = GetBlocksterGearType(item);
			if (blocksterGearType != BlocksterGearType.None)
			{
				if (blocksterGearType == BlocksterGearType.Back || blocksterGearType == BlocksterGearType.Body)
				{
					snapTarget = SnapTarget.Body;
				}
				else
				{
					snapTarget = GetSnapTargetUnderScreenPosition(pos, blocksterGearType);
				}
				gearBlock = item;
				return true;
			}
		}
		gearBlock = null;
		snapTarget = SnapTarget.None;
		return false;
	}

	public SnapTarget GetSnapTargetUnderScreenPosition(Vector3 pos, BlocksterGearType gearType, List<SnapTarget> allowedTargets = null)
	{
		if (allowedTargets == null)
		{
			allowedTargets = AllSnapTargets;
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(pos * NormalizedScreen.scale);
		SnapTarget result = SnapTarget.None;
		float num = float.MaxValue;
		foreach (SnapTarget allowedTarget in allowedTargets)
		{
			Vector3 snapTargetPosition = Instance.GetSnapTargetPosition(allowedTarget, gearType);
			bool flag = new Bounds(snapTargetPosition, Vector3.one).IntersectRay(ray);
			float sqrMagnitude = (ray.origin - snapTargetPosition).sqrMagnitude;
			if (flag && sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = allowedTarget;
			}
		}
		return result;
	}

	private bool GearHitTest(Block b, Ray ray)
	{
		if (b.go == null)
		{
			return false;
		}
		foreach (Collider collider in b.GetColliders())
		{
			if (collider.Raycast(ray, out var _, 20f))
			{
				return true;
			}
		}
		return false;
	}

	public Vector3 GetSnapTargetPosition(SnapTarget snapTarget, BlocksterGearType gearType)
	{
		Vector3 result = _snapTargetPositions[snapTarget];
		switch (gearType)
		{
		case BlocksterGearType.None:
			switch (snapTarget)
			{
			case SnapTarget.Head:
				result += _character.goT.up;
				break;
			case SnapTarget.Body:
				result -= _character.goT.forward;
				break;
			}
			break;
		case BlocksterGearType.HeadTop:
			result += _character.goT.up;
			break;
		case BlocksterGearType.Back:
			result -= _character.goT.forward;
			break;
		}
		return result;
	}

	public Quaternion GetSnapTargetRotation(SnapTarget snapTarget)
	{
		return _snapTargetRotations[snapTarget];
	}

	public Quaternion GetGearTypeRotation(BlocksterGearType gearType)
	{
		return _gearSnapRotations[gearType];
	}

	public Quaternion GetGearDefaultOrientation(Block block)
	{
		BlockMetaData blockMetaData = block.GetBlockMetaData();
		return Quaternion.Euler(blockMetaData.defaultOrientation);
	}

	public Quaternion GetGearCharacterEditorOrientation(Block block)
	{
		BlockMetaData blockMetaData = block.GetBlockMetaData();
		if (blockMetaData.characterEditModeUsesDefaultOrientation)
		{
			return Quaternion.Euler(blockMetaData.defaultOrientation);
		}
		return Quaternion.Euler(blockMetaData.characterEditModeOrientation);
	}

	public SnapTarget GetSnappedTarget(Block block)
	{
		BlocksterGearType blocksterGearType = GetBlocksterGearType(block);
		foreach (SnapTarget allSnapTarget in AllSnapTargets)
		{
			if (IsSnappedToTarget(block, blocksterGearType, allSnapTarget))
			{
				return allSnapTarget;
			}
		}
		return SnapTarget.None;
	}

	private bool IsSnappedToTarget(Block block, BlocksterGearType gearType, SnapTarget snapTarget)
	{
		Vector3 vector = block.GetPosition() - BlockSnapOffset(block, snapTarget);
		Vector3 snapTargetPosition = GetSnapTargetPosition(snapTarget, gearType);
		return (vector - snapTargetPosition).sqrMagnitude < 0.1f;
	}

	public void AddAttachment(Block attachment)
	{
		BWSceneManager.AddBlock(attachment);
		_attachedGear.Add(attachment);
		HashSet<Block> exclude = new HashSet<Block> { _character };
		for (int num = _attachedGear.Count - 1; num >= 0; num--)
		{
			Block block = _attachedGear[num];
			if (block != attachment && block.IsColliding(0f, exclude))
			{
				DisplaceBlock(block);
				_attachedGear.Remove(block);
			}
		}
	}

	public void RemoveAttachment(Block attachment)
	{
		int num = _attachedGear.IndexOf(attachment);
		if (num >= 0)
		{
			_attachedGear.RemoveAt(num);
		}
		BWSceneManager.RemoveBlock(attachment);
	}

	private void DisplaceBlock(Block b)
	{
		b.Deactivate();
		_displacedGear.Add(b);
		BWSceneManager.RemoveBlock(b);
	}

	public void RestoreAllDisplacedBlocks()
	{
		foreach (Block item in _displacedGear)
		{
			item.Activate();
			_attachedGear.Add(item);
			BWSceneManager.AddBlock(item);
		}
		_displacedGear.Clear();
	}

	public void RemoveAllDisplacedBlocks()
	{
		foreach (Block item in _displacedGear)
		{
			BWSceneManager.RemoveBlock(item);
			item.Destroy();
		}
		_displacedGear.Clear();
	}

	public List<Block> GetDisplacedBlocks()
	{
		return _displacedGear;
	}

	public BlockAnimatedCharacter CharacterBlock()
	{
		return _character;
	}

	public bool IsCharacterBlock(Block b)
	{
		return b == _character;
	}

	public bool IsCharacterAttachment(Block b)
	{
		return _attachedGear.Contains(b);
	}

	public void EditCharacter(BlockAnimatedCharacter character)
	{
		_character = character;
		BoxCollider component = _character.go.GetComponent<BoxCollider>();
		_restoreColliderSize = component.size;
		component.size = new Vector3(1f, 3f, 1f);
		component.center = new Vector3(0f, 0.8f, 0f);
		_TColliderGameObject = new GameObject("TCollider");
		BoxCollider boxCollider = _TColliderGameObject.AddComponent<BoxCollider>();
		_TColliderGameObject.transform.position = _character.goT.position;
		boxCollider.center = new Vector3(0f, 1.15f, 0.2f);
		boxCollider.size = new Vector3(3f, 0.5f, 0.5f);
		_TColliderGameObject.transform.rotation = _character.goT.rotation;
		BWSceneManager.AddChildBlockInstanceID(_TColliderGameObject, _character);
		_character.stateHandler.ForceSitStill();
		_character.DetermineAttachments();
		_attachedGear.Clear();
		_displacedGear.Clear();
		if (Blocksworld.cWidgetGesture.IsActive)
		{
			Blocksworld.cWidgetGesture.Cancel();
		}
		TBox.Show(show: false);
		TBox.tileCharacterEditExitIcon.Show();
		Vector3 position = character.GetPosition();
		Vector3 vector = position + 0.75f * Vector3.up;
		Vector3 vector2 = position + character.goT.forward * 5f + 0.75f * Vector3.up;
		Vector3 vector3 = vector - vector2;
		Vector3 lhs = Vector3.Cross(vector3, Vector3.up);
		Vector3 upwards = Vector3.Cross(lhs, vector3);
		Quaternion quaternion = Quaternion.LookRotation(vector3, upwards);
		Blocksworld.blocksworldCamera.Store();
		Blocksworld.blocksworldCamera.Unfollow();
		Blocksworld.blocksworldCamera.PlaceCamera(quaternion.eulerAngles, vector2);
		Blocksworld.blocksworldCamera.SetTargetDistance(5f);
		Blocksworld.blocksworldCamera.SetTargetPosition(vector);
		Vector3 position2 = _character.GetHeadAttach().transform.position;
		Vector3 position3 = _character.GetBodyAttach().transform.position;
		Vector3 forward = _character.goT.forward;
		Vector3 up = _character.goT.up;
		Vector3 right = _character.goT.right;
		Vector3 vector4 = _character.GetPosition() + 0.5f * Vector3.up;
		Vector3 vector5 = _character.GetPosition() - 0.5f * Vector3.up;
		Transform transform = _character.GetHandAttach(0).transform;
		Transform transform2 = _character.GetHandAttach(1).transform;
		Vector3 vector6 = position + right;
		Vector3 vector7 = position - right;
		foreach (Block attachedHeadBlock in _character.attachedHeadBlocks)
		{
			if (IsGear(attachedHeadBlock))
			{
				_attachedGear.Add(attachedHeadBlock);
				continue;
			}
			_nonGearAttachments.Add(attachedHeadBlock);
			attachedHeadBlock.goT.SetParent(_character.GetHeadAttach().transform);
		}
		foreach (Block attachedBackBlock in _character.attachedBackBlocks)
		{
			if (IsGear(attachedBackBlock))
			{
				_attachedGear.Add(attachedBackBlock);
				continue;
			}
			_nonGearAttachments.Add(attachedBackBlock);
			attachedBackBlock.goT.SetParent(_character.GetBodyAttach().transform);
		}
		if (_character.attachedRightBlock != null)
		{
			Block attachedRightBlock = _character.attachedRightBlock;
			foreach (Block attachedRightHandBlock in _character.attachedRightHandBlocks)
			{
				attachedRightHandBlock.goT.SetParent(attachedRightBlock.goT);
			}
			if (IsGear(attachedRightBlock))
			{
				_attachedGear.Add(attachedRightBlock);
			}
			else
			{
				_nonGearAttachments.Add(attachedRightBlock);
				attachedRightBlock.goT.SetParent(transform);
			}
		}
		if (_character.attachedLeftBlock != null)
		{
			Block attachedLeftBlock = _character.attachedLeftBlock;
			foreach (Block attachedLeftHandBlock in _character.attachedLeftHandBlocks)
			{
				attachedLeftHandBlock.goT.SetParent(attachedLeftBlock.goT);
			}
			if (IsGear(attachedLeftBlock))
			{
				_attachedGear.Add(attachedLeftBlock);
			}
			else
			{
				_nonGearAttachments.Add(attachedLeftBlock);
				attachedLeftBlock.goT.SetParent(transform2);
			}
		}
		_character.stateHandler.ForceEditPose();
		Vector3 position4 = _character.GetHeadAttach().transform.position;
		Vector3 position5 = _character.GetBodyAttach().transform.position;
		Vector3 vector8 = position4 - position2;
		Vector3 vector9 = position5 - position3;
		Vector3 vector10 = vector4 + vector8;
		Vector3 value = vector5 + vector9;
		Vector3 value2 = transform.TransformPoint(BlockAnimatedCharacter.rightHandPlacementOffset);
		Vector3 value3 = transform2.TransformPoint(BlockAnimatedCharacter.leftHandPlacementOffset);
		Quaternion rotation = _character.GetRotation();
		Quaternion value4 = rotation * Quaternion.AngleAxis(180f, Vector3.up);
		Quaternion value5 = rotation * Quaternion.AngleAxis(90f, Vector3.up);
		Quaternion value6 = rotation * Quaternion.AngleAxis(-90f, Vector3.up);
		Quaternion value7 = rotation * Quaternion.Euler(-90f, 180f, 0f);
		Quaternion value8 = rotation * Quaternion.Euler(-90f, 180f, 0f);
		_gearSnapRotations.Clear();
		_gearSnapRotations.Add(BlocksterGearType.Head, rotation);
		_gearSnapRotations.Add(BlocksterGearType.HeadSide, rotation);
		_gearSnapRotations.Add(BlocksterGearType.HeadTop, rotation);
		_gearSnapRotations.Add(BlocksterGearType.Body, rotation);
		_gearSnapRotations.Add(BlocksterGearType.Back, rotation);
		_snapTargetPositions.Clear();
		_snapTargetPositions.Add(SnapTarget.Body, value);
		_snapTargetPositions.Add(SnapTarget.Head, vector10);
		_snapTargetPositions.Add(SnapTarget.HeadBack, vector10 - forward);
		_snapTargetPositions.Add(SnapTarget.HeadLeft, vector10 - right);
		_snapTargetPositions.Add(SnapTarget.HeadRight, vector10 + right);
		_snapTargetPositions.Add(SnapTarget.HeadFront, vector10 + forward);
		_snapTargetPositions.Add(SnapTarget.RightHand, value2);
		_snapTargetPositions.Add(SnapTarget.LeftHand, value3);
		_snapTargetRotations.Clear();
		_snapTargetRotations.Add(SnapTarget.Body, rotation);
		_snapTargetRotations.Add(SnapTarget.Head, rotation);
		_snapTargetRotations.Add(SnapTarget.HeadBack, value4);
		_snapTargetRotations.Add(SnapTarget.HeadLeft, value6);
		_snapTargetRotations.Add(SnapTarget.HeadRight, value5);
		_snapTargetRotations.Add(SnapTarget.HeadFront, rotation);
		_snapTargetRotations.Add(SnapTarget.RightHand, value7);
		_snapTargetRotations.Add(SnapTarget.LeftHand, value8);
		foreach (Block attachedHeadBlock2 in _character.attachedHeadBlocks)
		{
			if (IsGear(attachedHeadBlock2))
			{
				attachedHeadBlock2.MoveTo(attachedHeadBlock2.GetPosition() + vector8);
			}
			if (GetSnappedTarget(attachedHeadBlock2) == SnapTarget.None)
			{
				_attachedGear.Remove(attachedHeadBlock2);
				_nonGearAttachments.Add(attachedHeadBlock2);
				attachedHeadBlock2.goT.SetParent(_character.GetHeadAttach().transform);
			}
		}
		foreach (Block attachedBackBlock2 in _character.attachedBackBlocks)
		{
			if (IsGear(attachedBackBlock2))
			{
				attachedBackBlock2.MoveTo(attachedBackBlock2.GetPosition() + vector9);
			}
			if (GetSnappedTarget(attachedBackBlock2) == SnapTarget.None)
			{
				_attachedGear.Remove(attachedBackBlock2);
				_nonGearAttachments.Add(attachedBackBlock2);
				attachedBackBlock2.goT.SetParent(_character.GetBodyAttach().transform);
			}
		}
		if (_character.attachedRightBlock != null && IsGear(_character.attachedRightBlock))
		{
			_character.attachedRightBlock.MoveTo(GetSnapTargetPosition(SnapTarget.RightHand, BlocksterGearType.RightHand) + BlockSnapOffset(_character.attachedRightBlock, SnapTarget.RightHand));
			_character.attachedRightBlock.RotateTo(GetSnapTargetRotation(SnapTarget.RightHand) * GetGearCharacterEditorOrientation(_character.attachedRightBlock));
		}
		if (_character.attachedLeftBlock != null && IsGear(_character.attachedLeftBlock))
		{
			_character.attachedLeftBlock.MoveTo(GetSnapTargetPosition(SnapTarget.LeftHand, BlocksterGearType.LeftHand) + BlockSnapOffset(_character.attachedLeftBlock, SnapTarget.LeftHand));
			_character.attachedLeftBlock.RotateTo(GetSnapTargetRotation(SnapTarget.LeftHand) * GetGearCharacterEditorOrientation(_character.attachedLeftBlock));
		}
		Blocksworld.buildPanel.Layout();
	}

	public static bool IsGear(Block b)
	{
		return GetBlocksterGearType(b) != BlocksterGearType.None;
	}

	public void Exit()
	{
		Dictionary<SnapTarget, List<Block>> dictionary = new Dictionary<SnapTarget, List<Block>>();
		foreach (Block item in _attachedGear)
		{
			SnapTarget snappedTarget = GetSnappedTarget(item);
			if (snappedTarget == SnapTarget.None)
			{
				BWLog.Info("Failed to get snap target for attachment " + item);
				continue;
			}
			if (!dictionary.TryGetValue(snappedTarget, out var value))
			{
				value = new List<Block>();
				dictionary.Add(snappedTarget, value);
			}
			value.Add(item);
		}
		_character.stateHandler.ForceSitStill();
		foreach (Block nonGearAttachment in _nonGearAttachments)
		{
			nonGearAttachment.goT.SetParent(null);
			nonGearAttachment.MoveTo(nonGearAttachment.goT.position);
		}
		BoxCollider component = _character.go.GetComponent<BoxCollider>();
		component.size = _restoreColliderSize;
		component.center = Vector3.zero;
		BWSceneManager.RemoveChildBlockInstanceID(_TColliderGameObject);
		Object.Destroy(_TColliderGameObject);
		Vector3 vector = _character.GetPosition() + 0.5f * Vector3.up;
		Vector3 vector2 = _character.GetPosition() - 0.5f * Vector3.up;
		Vector3 forward = _character.goT.forward;
		Vector3 right = _character.goT.right;
		_snapTargetPositions.Clear();
		_snapTargetPositions.Add(SnapTarget.Body, vector2);
		_snapTargetPositions.Add(SnapTarget.Head, vector);
		_snapTargetPositions.Add(SnapTarget.HeadBack, vector - forward);
		_snapTargetPositions.Add(SnapTarget.HeadLeft, vector - right);
		_snapTargetPositions.Add(SnapTarget.HeadRight, vector + right);
		_snapTargetPositions.Add(SnapTarget.HeadFront, vector + forward);
		_snapTargetPositions.Add(SnapTarget.RightHand, vector2 + right);
		_snapTargetPositions.Add(SnapTarget.LeftHand, vector2 - right);
		foreach (KeyValuePair<SnapTarget, List<Block>> item2 in dictionary)
		{
			SnapTarget key = item2.Key;
			foreach (Block item3 in item2.Value)
			{
				BlocksterGearType blocksterGearType = GetBlocksterGearType(item3);
				Vector3 snapTargetPosition = GetSnapTargetPosition(key, blocksterGearType);
				item3.MoveTo(snapTargetPosition + BlockSnapOffset(item3, key));
				switch (key)
				{
				case SnapTarget.RightHand:
					item3.RotateTo(_character.GetRotation() * Quaternion.Euler(new Vector3(0f, 90f, 90f)) * GetGearCharacterEditorOrientation(item3));
					break;
				case SnapTarget.LeftHand:
					item3.RotateTo(_character.GetRotation() * Quaternion.Euler(new Vector3(0f, -90f, -90f)) * GetGearCharacterEditorOrientation(item3));
					break;
				}
				ConnectednessGraph.Update(item3);
			}
		}
		_character.DetermineAttachments();
		_character = null;
		bool show = TBox.selected != null;
		TBox.Show(show);
		Blocksworld.blocksworldCamera.Restore();
		Blocksworld.buildPanel.Layout();
		_attachedGear.Clear();
		_nonGearAttachments.Clear();
		_TColliderGameObject = null;
		_snapTargetPositions.Clear();
		_snapTargetRotations.Clear();
		_gearSnapRotations.Clear();
	}

	public Vector3 BlockSnapOffset(Block b, SnapTarget snapTarget)
	{
		BlockMetaData blockMetaData = b.GetBlockMetaData();
		float num;
		float num2;
		float num3;
		if (blockMetaData.characterEditModeSnapOffset.sqrMagnitude > 0f)
		{
			num = blockMetaData.characterEditModeSnapOffset.x;
			num2 = blockMetaData.characterEditModeSnapOffset.y;
			num3 = blockMetaData.characterEditModeSnapOffset.z;
		}
		else
		{
			Vector3 vector = Vector3.Scale(blockMetaData.blockSize, blockMetaData.defaultScale);
			float num4 = Mathf.Max(1f, vector.x);
			float num5 = Mathf.Max(1f, vector.y);
			float num6 = Mathf.Max(1f, vector.z);
			num = 0f;
			num2 = num5 / 2f - 0.5f;
			num3 = num6 / 2f - 0.5f;
			if (snapTarget == SnapTarget.RightHand || snapTarget == SnapTarget.LeftHand)
			{
				num3 = num2;
				num = 0f;
				num2 = 0f;
			}
			else
			{
				if (num2 > 0f && (snapTarget == SnapTarget.Head || snapTarget == SnapTarget.HeadFront || snapTarget == SnapTarget.HeadBack || snapTarget == SnapTarget.HeadLeft || snapTarget == SnapTarget.HeadRight))
				{
					num2 -= 1f;
				}
				if (num3 > 0f)
				{
					switch (snapTarget)
					{
					case SnapTarget.Body:
					case SnapTarget.HeadBack:
						num3 = 0f - num3;
						break;
					case SnapTarget.HeadLeft:
					{
						float num8 = num;
						num = 0f - num3;
						num3 = num8;
						break;
					}
					case SnapTarget.HeadRight:
					{
						float num7 = num;
						num = num3;
						num3 = num7;
						break;
					}
					}
				}
			}
		}
		Vector3 direction = new Vector3(num, num2, num3);
		return _character.goT.TransformDirection(direction);
	}

	public void RefreshAttachments()
	{
		_attachedGear.Clear();
		Collider[] array = new Collider[8];
		Vector3 one = Vector3.one;
		foreach (SnapTarget allSnapTarget in AllSnapTargets)
		{
			HashSet<BlocksterGearType> hashSet = new HashSet<BlocksterGearType>();
			switch (allSnapTarget)
			{
			case SnapTarget.Head:
				hashSet.Add(BlocksterGearType.Head);
				hashSet.Add(BlocksterGearType.HeadTop);
				break;
			case SnapTarget.Body:
				hashSet.Add(BlocksterGearType.Back);
				hashSet.Add(BlocksterGearType.Body);
				break;
			case SnapTarget.HeadLeft:
			case SnapTarget.HeadRight:
			case SnapTarget.HeadBack:
			case SnapTarget.HeadFront:
				hashSet.Add(BlocksterGearType.HeadSide);
				break;
			case SnapTarget.LeftHand:
			case SnapTarget.RightHand:
				hashSet.Add(BlocksterGearType.RightHand);
				hashSet.Add(BlocksterGearType.LeftHand);
				break;
			}
			foreach (BlocksterGearType item in hashSet)
			{
				Vector3 snapTargetPosition = GetSnapTargetPosition(allSnapTarget, item);
				Physics.OverlapBoxNonAlloc(snapTargetPosition, one, array);
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (!(collider == null) && !(collider.gameObject == null))
					{
						Block block = BWSceneManager.FindBlock(collider.gameObject, checkChildGos: true);
						if (block != null && block.go != null && GetBlocksterGearType(block) == item)
						{
							ConnectednessGraph.Remove(block);
							_attachedGear.Add(block);
						}
					}
				}
			}
		}
	}

	public bool InEditMode()
	{
		return _character != null;
	}

	public static BlocksterGearType GetBlocksterGearType(Block b)
	{
		BlockMetaData blockMetaData = b.GetBlockMetaData();
		BlocksterGearType blocksterGearType = blockMetaData.gearType;
		if (blocksterGearType != BlocksterGearType.None)
		{
			return blocksterGearType;
		}
		bool flag = false;
		if (blockMetaData != null)
		{
			for (int i = 0; i < blockMetaData.canOccupySameGrid.Length; i++)
			{
				if (blockMetaData.canOccupySameGrid[i] == "Character")
				{
					flag = true;
					blocksterGearType = BlocksterGearType.Head;
					break;
				}
			}
			for (int j = 0; j < blockMetaData.shapeCategories.Length; j++)
			{
				if (blockMetaData.shapeCategories[j] == "Head Gear" || blockMetaData.shapeCategories[j] == "Eyewear" || blockMetaData.shapeCategories[j] == "Beard" || blockMetaData.shapeCategories[j] == "Mustache" || blockMetaData.shapeCategories[j] == "Bangs" || blockMetaData.shapeCategories[j] == "Head Back Gear" || blockMetaData.shapeCategories[j] == "Ponytail")
				{
					blocksterGearType = (flag ? BlocksterGearType.Head : BlocksterGearType.HeadSide);
					break;
				}
			}
			for (int k = 0; k < blockMetaData.shapeCategories.Length; k++)
			{
				if (blockMetaData.shapeCategories[k] == "Head Top Gear" || blockMetaData.shapeCategories[k] == "Top Ears" || blockMetaData.shapeCategories[k] == "Hat")
				{
					blocksterGearType = BlocksterGearType.HeadTop;
					break;
				}
			}
			for (int l = 0; l < blockMetaData.shapeCategories.Length; l++)
			{
				if (blockMetaData.shapeCategories[l] == "Back Gear")
				{
					blocksterGearType = ((!flag) ? BlocksterGearType.Back : BlocksterGearType.Body);
					break;
				}
			}
			for (int m = 0; m < blockMetaData.shapeCategories.Length; m++)
			{
				if (blockMetaData.shapeCategories[m] == "Chest Gear" || blockMetaData.shapeCategories[m] == "Necklace" || blockMetaData.shapeCategories[m] == "Torso" || blockMetaData.shapeCategories[m] == "Belt")
				{
					blocksterGearType = BlocksterGearType.Body;
					break;
				}
			}
		}
		return blocksterGearType;
	}
}
