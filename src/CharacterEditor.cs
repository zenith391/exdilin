using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200010F RID: 271
public class CharacterEditor
{
	// Token: 0x17000051 RID: 81
	// (get) Token: 0x0600131F RID: 4895 RVA: 0x00082949 File Offset: 0x00080D49
	public static CharacterEditor Instance
	{
		get
		{
			if (CharacterEditor._instance == null)
			{
				CharacterEditor._instance = new CharacterEditor();
			}
			return CharacterEditor._instance;
		}
	}

	// Token: 0x06001320 RID: 4896 RVA: 0x00082964 File Offset: 0x00080D64
	public bool GetGearUnderScreenPosition(Vector3 pos, out Block gearBlock, out CharacterEditor.SnapTarget snapTarget)
	{
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(pos * NormalizedScreen.scale);
		foreach (Block block in this._attachedGear)
		{
			if (this.GearHitTest(block, ray))
			{
				BlocksterGearType blocksterGearType = CharacterEditor.GetBlocksterGearType(block);
				if (blocksterGearType != BlocksterGearType.None)
				{
					if (blocksterGearType == BlocksterGearType.Back || blocksterGearType == BlocksterGearType.Body)
					{
						snapTarget = CharacterEditor.SnapTarget.Body;
					}
					else
					{
						snapTarget = this.GetSnapTargetUnderScreenPosition(pos, blocksterGearType, null);
					}
					gearBlock = block;
					return true;
				}
			}
		}
		gearBlock = null;
		snapTarget = CharacterEditor.SnapTarget.None;
		return false;
	}

	// Token: 0x06001321 RID: 4897 RVA: 0x00082A24 File Offset: 0x00080E24
	public CharacterEditor.SnapTarget GetSnapTargetUnderScreenPosition(Vector3 pos, BlocksterGearType gearType, List<CharacterEditor.SnapTarget> allowedTargets = null)
	{
		if (allowedTargets == null)
		{
			allowedTargets = CharacterEditor.AllSnapTargets;
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(pos * NormalizedScreen.scale);
		CharacterEditor.SnapTarget result = CharacterEditor.SnapTarget.None;
		float num = float.MaxValue;
		foreach (CharacterEditor.SnapTarget snapTarget in allowedTargets)
		{
			Vector3 snapTargetPosition = CharacterEditor.Instance.GetSnapTargetPosition(snapTarget, gearType);
			Bounds bounds = new Bounds(snapTargetPosition, Vector3.one);
			bool flag = bounds.IntersectRay(ray);
			float sqrMagnitude = (ray.origin - snapTargetPosition).sqrMagnitude;
			if (flag && sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = snapTarget;
			}
		}
		return result;
	}

	// Token: 0x06001322 RID: 4898 RVA: 0x00082AF4 File Offset: 0x00080EF4
	private bool GearHitTest(Block b, Ray ray)
	{
		if (b.go == null)
		{
			return false;
		}
		foreach (Collider collider in b.GetColliders())
		{
			RaycastHit raycastHit;
			if (collider.Raycast(ray, out raycastHit, 20f))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001323 RID: 4899 RVA: 0x00082B7C File Offset: 0x00080F7C
	public Vector3 GetSnapTargetPosition(CharacterEditor.SnapTarget snapTarget, BlocksterGearType gearType)
	{
		Vector3 vector = this._snapTargetPositions[snapTarget];
		if (gearType != BlocksterGearType.Back)
		{
			if (gearType != BlocksterGearType.HeadTop)
			{
				if (gearType == BlocksterGearType.None)
				{
					if (snapTarget == CharacterEditor.SnapTarget.Head)
					{
						vector += this._character.goT.up;
					}
					else if (snapTarget == CharacterEditor.SnapTarget.Body)
					{
						vector -= this._character.goT.forward;
					}
				}
			}
			else
			{
				vector += this._character.goT.up;
			}
		}
		else
		{
			vector -= this._character.goT.forward;
		}
		return vector;
	}

	// Token: 0x06001324 RID: 4900 RVA: 0x00082C2E File Offset: 0x0008102E
	public Quaternion GetSnapTargetRotation(CharacterEditor.SnapTarget snapTarget)
	{
		return this._snapTargetRotations[snapTarget];
	}

	// Token: 0x06001325 RID: 4901 RVA: 0x00082C3C File Offset: 0x0008103C
	public Quaternion GetGearTypeRotation(BlocksterGearType gearType)
	{
		return this._gearSnapRotations[gearType];
	}

	// Token: 0x06001326 RID: 4902 RVA: 0x00082C4C File Offset: 0x0008104C
	public Quaternion GetGearDefaultOrientation(Block block)
	{
		BlockMetaData blockMetaData = block.GetBlockMetaData();
		return Quaternion.Euler(blockMetaData.defaultOrientation);
	}

	// Token: 0x06001327 RID: 4903 RVA: 0x00082C6C File Offset: 0x0008106C
	public Quaternion GetGearCharacterEditorOrientation(Block block)
	{
		BlockMetaData blockMetaData = block.GetBlockMetaData();
		if (blockMetaData.characterEditModeUsesDefaultOrientation)
		{
			return Quaternion.Euler(blockMetaData.defaultOrientation);
		}
		return Quaternion.Euler(blockMetaData.characterEditModeOrientation);
	}

	// Token: 0x06001328 RID: 4904 RVA: 0x00082CA4 File Offset: 0x000810A4
	public CharacterEditor.SnapTarget GetSnappedTarget(Block block)
	{
		BlocksterGearType blocksterGearType = CharacterEditor.GetBlocksterGearType(block);
		foreach (CharacterEditor.SnapTarget snapTarget in CharacterEditor.AllSnapTargets)
		{
			if (this.IsSnappedToTarget(block, blocksterGearType, snapTarget))
			{
				return snapTarget;
			}
		}
		return CharacterEditor.SnapTarget.None;
	}

	// Token: 0x06001329 RID: 4905 RVA: 0x00082D18 File Offset: 0x00081118
	private bool IsSnappedToTarget(Block block, BlocksterGearType gearType, CharacterEditor.SnapTarget snapTarget)
	{
		Vector3 a = block.GetPosition() - this.BlockSnapOffset(block, snapTarget);
		Vector3 snapTargetPosition = this.GetSnapTargetPosition(snapTarget, gearType);
		return (a - snapTargetPosition).sqrMagnitude < 0.1f;
	}

	// Token: 0x0600132A RID: 4906 RVA: 0x00082D58 File Offset: 0x00081158
	public void AddAttachment(Block attachment)
	{
		BWSceneManager.AddBlock(attachment);
		this._attachedGear.Add(attachment);
		HashSet<Block> exclude = new HashSet<Block>
		{
			this._character
		};
		for (int i = this._attachedGear.Count - 1; i >= 0; i--)
		{
			Block block = this._attachedGear[i];
			if (block != attachment)
			{
				if (block.IsColliding(0f, exclude))
				{
					this.DisplaceBlock(block);
					this._attachedGear.Remove(block);
				}
			}
		}
	}

	// Token: 0x0600132B RID: 4907 RVA: 0x00082DE8 File Offset: 0x000811E8
	public void RemoveAttachment(Block attachment)
	{
		int num = this._attachedGear.IndexOf(attachment);
		if (num >= 0)
		{
			this._attachedGear.RemoveAt(num);
		}
		BWSceneManager.RemoveBlock(attachment);
	}

	// Token: 0x0600132C RID: 4908 RVA: 0x00082E1B File Offset: 0x0008121B
	private void DisplaceBlock(Block b)
	{
		b.Deactivate();
		this._displacedGear.Add(b);
		BWSceneManager.RemoveBlock(b);
	}

	// Token: 0x0600132D RID: 4909 RVA: 0x00082E38 File Offset: 0x00081238
	public void RestoreAllDisplacedBlocks()
	{
		foreach (Block block in this._displacedGear)
		{
			block.Activate();
			this._attachedGear.Add(block);
			BWSceneManager.AddBlock(block);
		}
		this._displacedGear.Clear();
	}

	// Token: 0x0600132E RID: 4910 RVA: 0x00082EB0 File Offset: 0x000812B0
	public void RemoveAllDisplacedBlocks()
	{
		foreach (Block block in this._displacedGear)
		{
			BWSceneManager.RemoveBlock(block);
			block.Destroy();
		}
		this._displacedGear.Clear();
	}

	// Token: 0x0600132F RID: 4911 RVA: 0x00082F1C File Offset: 0x0008131C
	public List<Block> GetDisplacedBlocks()
	{
		return this._displacedGear;
	}

	// Token: 0x06001330 RID: 4912 RVA: 0x00082F24 File Offset: 0x00081324
	public BlockAnimatedCharacter CharacterBlock()
	{
		return this._character;
	}

	// Token: 0x06001331 RID: 4913 RVA: 0x00082F2C File Offset: 0x0008132C
	public bool IsCharacterBlock(Block b)
	{
		return b == this._character;
	}

	// Token: 0x06001332 RID: 4914 RVA: 0x00082F37 File Offset: 0x00081337
	public bool IsCharacterAttachment(Block b)
	{
		return this._attachedGear.Contains(b);
	}

	// Token: 0x06001333 RID: 4915 RVA: 0x00082F48 File Offset: 0x00081348
	public void EditCharacter(BlockAnimatedCharacter character)
	{
		this._character = character;
		BoxCollider component = this._character.go.GetComponent<BoxCollider>();
		this._restoreColliderSize = component.size;
		component.size = new Vector3(1f, 3f, 1f);
		component.center = new Vector3(0f, 0.8f, 0f);
		this._TColliderGameObject = new GameObject("TCollider");
		BoxCollider boxCollider = this._TColliderGameObject.AddComponent<BoxCollider>();
		this._TColliderGameObject.transform.position = this._character.goT.position;
		boxCollider.center = new Vector3(0f, 1.15f, 0.2f);
		boxCollider.size = new Vector3(3f, 0.5f, 0.5f);
		this._TColliderGameObject.transform.rotation = this._character.goT.rotation;
		BWSceneManager.AddChildBlockInstanceID(this._TColliderGameObject, this._character);
		this._character.stateHandler.ForceSitStill();
		this._character.DetermineAttachments();
		this._attachedGear.Clear();
		this._displacedGear.Clear();
		if (Blocksworld.cWidgetGesture.IsActive)
		{
			Blocksworld.cWidgetGesture.Cancel();
		}
		TBox.Show(false);
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
		Vector3 position2 = this._character.GetHeadAttach().transform.position;
		Vector3 position3 = this._character.GetBodyAttach().transform.position;
		Vector3 forward = this._character.goT.forward;
		Vector3 up = this._character.goT.up;
		Vector3 right = this._character.goT.right;
		Vector3 a = this._character.GetPosition() + 0.5f * Vector3.up;
		Vector3 a2 = this._character.GetPosition() - 0.5f * Vector3.up;
		Transform transform = this._character.GetHandAttach(0).transform;
		Transform transform2 = this._character.GetHandAttach(1).transform;
		Vector3 vector4 = position + right;
		Vector3 vector5 = position - right;
		foreach (Block block in this._character.attachedHeadBlocks)
		{
			if (CharacterEditor.IsGear(block))
			{
				this._attachedGear.Add(block);
			}
			else
			{
				this._nonGearAttachments.Add(block);
				block.goT.SetParent(this._character.GetHeadAttach().transform);
			}
		}
		foreach (Block block2 in this._character.attachedBackBlocks)
		{
			if (CharacterEditor.IsGear(block2))
			{
				this._attachedGear.Add(block2);
			}
			else
			{
				this._nonGearAttachments.Add(block2);
				block2.goT.SetParent(this._character.GetBodyAttach().transform);
			}
		}
		if (this._character.attachedRightBlock != null)
		{
			Block attachedRightBlock = this._character.attachedRightBlock;
			foreach (Block block3 in this._character.attachedRightHandBlocks)
			{
				block3.goT.SetParent(attachedRightBlock.goT);
			}
			if (CharacterEditor.IsGear(attachedRightBlock))
			{
				this._attachedGear.Add(attachedRightBlock);
			}
			else
			{
				this._nonGearAttachments.Add(attachedRightBlock);
				attachedRightBlock.goT.SetParent(transform);
			}
		}
		if (this._character.attachedLeftBlock != null)
		{
			Block attachedLeftBlock = this._character.attachedLeftBlock;
			foreach (Block block4 in this._character.attachedLeftHandBlocks)
			{
				block4.goT.SetParent(attachedLeftBlock.goT);
			}
			if (CharacterEditor.IsGear(attachedLeftBlock))
			{
				this._attachedGear.Add(attachedLeftBlock);
			}
			else
			{
				this._nonGearAttachments.Add(attachedLeftBlock);
				attachedLeftBlock.goT.SetParent(transform2);
			}
		}
		this._character.stateHandler.ForceEditPose();
		Vector3 position4 = this._character.GetHeadAttach().transform.position;
		Vector3 position5 = this._character.GetBodyAttach().transform.position;
		Vector3 b = position4 - position2;
		Vector3 b2 = position5 - position3;
		Vector3 vector6 = a + b;
		Vector3 value = a2 + b2;
		Vector3 value2 = transform.TransformPoint(BlockAnimatedCharacter.rightHandPlacementOffset);
		Vector3 value3 = transform2.TransformPoint(BlockAnimatedCharacter.leftHandPlacementOffset);
		Quaternion rotation = this._character.GetRotation();
		Quaternion value4 = rotation * Quaternion.AngleAxis(180f, Vector3.up);
		Quaternion value5 = rotation * Quaternion.AngleAxis(90f, Vector3.up);
		Quaternion value6 = rotation * Quaternion.AngleAxis(-90f, Vector3.up);
		Quaternion value7 = rotation * Quaternion.Euler(-90f, 180f, 0f);
		Quaternion value8 = rotation * Quaternion.Euler(-90f, 180f, 0f);
		this._gearSnapRotations.Clear();
		this._gearSnapRotations.Add(BlocksterGearType.Head, rotation);
		this._gearSnapRotations.Add(BlocksterGearType.HeadSide, rotation);
		this._gearSnapRotations.Add(BlocksterGearType.HeadTop, rotation);
		this._gearSnapRotations.Add(BlocksterGearType.Body, rotation);
		this._gearSnapRotations.Add(BlocksterGearType.Back, rotation);
		this._snapTargetPositions.Clear();
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.Body, value);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.Head, vector6);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadBack, vector6 - forward);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadLeft, vector6 - right);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadRight, vector6 + right);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadFront, vector6 + forward);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.RightHand, value2);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.LeftHand, value3);
		this._snapTargetRotations.Clear();
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.Body, rotation);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.Head, rotation);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.HeadBack, value4);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.HeadLeft, value6);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.HeadRight, value5);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.HeadFront, rotation);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.RightHand, value7);
		this._snapTargetRotations.Add(CharacterEditor.SnapTarget.LeftHand, value8);
		foreach (Block block5 in this._character.attachedHeadBlocks)
		{
			if (CharacterEditor.IsGear(block5))
			{
				block5.MoveTo(block5.GetPosition() + b);
			}
			if (this.GetSnappedTarget(block5) == CharacterEditor.SnapTarget.None)
			{
				this._attachedGear.Remove(block5);
				this._nonGearAttachments.Add(block5);
				block5.goT.SetParent(this._character.GetHeadAttach().transform);
			}
		}
		foreach (Block block6 in this._character.attachedBackBlocks)
		{
			if (CharacterEditor.IsGear(block6))
			{
				block6.MoveTo(block6.GetPosition() + b2);
			}
			if (this.GetSnappedTarget(block6) == CharacterEditor.SnapTarget.None)
			{
				this._attachedGear.Remove(block6);
				this._nonGearAttachments.Add(block6);
				block6.goT.SetParent(this._character.GetBodyAttach().transform);
			}
		}
		if (this._character.attachedRightBlock != null && CharacterEditor.IsGear(this._character.attachedRightBlock))
		{
			this._character.attachedRightBlock.MoveTo(this.GetSnapTargetPosition(CharacterEditor.SnapTarget.RightHand, BlocksterGearType.RightHand) + this.BlockSnapOffset(this._character.attachedRightBlock, CharacterEditor.SnapTarget.RightHand));
			this._character.attachedRightBlock.RotateTo(this.GetSnapTargetRotation(CharacterEditor.SnapTarget.RightHand) * this.GetGearCharacterEditorOrientation(this._character.attachedRightBlock));
		}
		if (this._character.attachedLeftBlock != null && CharacterEditor.IsGear(this._character.attachedLeftBlock))
		{
			this._character.attachedLeftBlock.MoveTo(this.GetSnapTargetPosition(CharacterEditor.SnapTarget.LeftHand, BlocksterGearType.LeftHand) + this.BlockSnapOffset(this._character.attachedLeftBlock, CharacterEditor.SnapTarget.LeftHand));
			this._character.attachedLeftBlock.RotateTo(this.GetSnapTargetRotation(CharacterEditor.SnapTarget.LeftHand) * this.GetGearCharacterEditorOrientation(this._character.attachedLeftBlock));
		}
		Blocksworld.buildPanel.Layout();
	}

	// Token: 0x06001334 RID: 4916 RVA: 0x000839C8 File Offset: 0x00081DC8
	public static bool IsGear(Block b)
	{
		return CharacterEditor.GetBlocksterGearType(b) != BlocksterGearType.None;
	}

	// Token: 0x06001335 RID: 4917 RVA: 0x000839D8 File Offset: 0x00081DD8
	public void Exit()
	{
		Dictionary<CharacterEditor.SnapTarget, List<Block>> dictionary = new Dictionary<CharacterEditor.SnapTarget, List<Block>>();
		foreach (Block block in this._attachedGear)
		{
			CharacterEditor.SnapTarget snappedTarget = this.GetSnappedTarget(block);
			if (snappedTarget == CharacterEditor.SnapTarget.None)
			{
				BWLog.Info("Failed to get snap target for attachment " + block);
			}
			else
			{
				List<Block> list;
				if (!dictionary.TryGetValue(snappedTarget, out list))
				{
					list = new List<Block>();
					dictionary.Add(snappedTarget, list);
				}
				list.Add(block);
			}
		}
		this._character.stateHandler.ForceSitStill();
		foreach (Block block2 in this._nonGearAttachments)
		{
			block2.goT.SetParent(null);
			block2.MoveTo(block2.goT.position);
		}
		BoxCollider component = this._character.go.GetComponent<BoxCollider>();
		component.size = this._restoreColliderSize;
		component.center = Vector3.zero;
		BWSceneManager.RemoveChildBlockInstanceID(this._TColliderGameObject);
		UnityEngine.Object.Destroy(this._TColliderGameObject);
		Vector3 vector = this._character.GetPosition() + 0.5f * Vector3.up;
		Vector3 vector2 = this._character.GetPosition() - 0.5f * Vector3.up;
		Vector3 forward = this._character.goT.forward;
		Vector3 right = this._character.goT.right;
		this._snapTargetPositions.Clear();
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.Body, vector2);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.Head, vector);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadBack, vector - forward);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadLeft, vector - right);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadRight, vector + right);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.HeadFront, vector + forward);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.RightHand, vector2 + right);
		this._snapTargetPositions.Add(CharacterEditor.SnapTarget.LeftHand, vector2 - right);
		foreach (KeyValuePair<CharacterEditor.SnapTarget, List<Block>> keyValuePair in dictionary)
		{
			CharacterEditor.SnapTarget key = keyValuePair.Key;
			foreach (Block block3 in keyValuePair.Value)
			{
				BlocksterGearType blocksterGearType = CharacterEditor.GetBlocksterGearType(block3);
				Vector3 snapTargetPosition = this.GetSnapTargetPosition(key, blocksterGearType);
				block3.MoveTo(snapTargetPosition + this.BlockSnapOffset(block3, key));
				if (key == CharacterEditor.SnapTarget.RightHand)
				{
					block3.RotateTo(this._character.GetRotation() * Quaternion.Euler(new Vector3(0f, 90f, 90f)) * this.GetGearCharacterEditorOrientation(block3));
				}
				else if (key == CharacterEditor.SnapTarget.LeftHand)
				{
					block3.RotateTo(this._character.GetRotation() * Quaternion.Euler(new Vector3(0f, -90f, -90f)) * this.GetGearCharacterEditorOrientation(block3));
				}
				ConnectednessGraph.Update(block3);
			}
		}
		this._character.DetermineAttachments();
		this._character = null;
		bool show = TBox.selected != null;
		TBox.Show(show);
		Blocksworld.blocksworldCamera.Restore();
		Blocksworld.buildPanel.Layout();
		this._attachedGear.Clear();
		this._nonGearAttachments.Clear();
		this._TColliderGameObject = null;
		this._snapTargetPositions.Clear();
		this._snapTargetRotations.Clear();
		this._gearSnapRotations.Clear();
	}

	// Token: 0x06001336 RID: 4918 RVA: 0x00083E40 File Offset: 0x00082240
	public Vector3 BlockSnapOffset(Block b, CharacterEditor.SnapTarget snapTarget)
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
			if (snapTarget == CharacterEditor.SnapTarget.RightHand || snapTarget == CharacterEditor.SnapTarget.LeftHand)
			{
				num3 = num2;
				num = 0f;
				num2 = 0f;
			}
			else
			{
				if (num2 > 0f && (snapTarget == CharacterEditor.SnapTarget.Head || snapTarget == CharacterEditor.SnapTarget.HeadFront || snapTarget == CharacterEditor.SnapTarget.HeadBack || snapTarget == CharacterEditor.SnapTarget.HeadLeft || snapTarget == CharacterEditor.SnapTarget.HeadRight))
				{
					num2 -= 1f;
				}
				if (num3 > 0f)
				{
					if (snapTarget == CharacterEditor.SnapTarget.HeadBack || snapTarget == CharacterEditor.SnapTarget.Body)
					{
						num3 = -num3;
					}
					else if (snapTarget == CharacterEditor.SnapTarget.HeadLeft)
					{
						float num7 = num;
						num = -num3;
						num3 = num7;
					}
					else if (snapTarget == CharacterEditor.SnapTarget.HeadRight)
					{
						float num7 = num;
						num = num3;
						num3 = num7;
					}
				}
			}
		}
		Vector3 direction = new Vector3(num, num2, num3);
		return this._character.goT.TransformDirection(direction);
	}

	// Token: 0x06001337 RID: 4919 RVA: 0x00083FCC File Offset: 0x000823CC
	public void RefreshAttachments()
	{
		this._attachedGear.Clear();
		Collider[] array = new Collider[8];
		Vector3 one = Vector3.one;
		foreach (CharacterEditor.SnapTarget snapTarget in CharacterEditor.AllSnapTargets)
		{
			HashSet<BlocksterGearType> hashSet = new HashSet<BlocksterGearType>();
			switch (snapTarget)
			{
			case CharacterEditor.SnapTarget.Head:
				hashSet.Add(BlocksterGearType.Head);
				hashSet.Add(BlocksterGearType.HeadTop);
				break;
			case CharacterEditor.SnapTarget.Body:
				hashSet.Add(BlocksterGearType.Back);
				hashSet.Add(BlocksterGearType.Body);
				break;
			case CharacterEditor.SnapTarget.HeadLeft:
			case CharacterEditor.SnapTarget.HeadRight:
			case CharacterEditor.SnapTarget.HeadBack:
			case CharacterEditor.SnapTarget.HeadFront:
				hashSet.Add(BlocksterGearType.HeadSide);
				break;
			case CharacterEditor.SnapTarget.LeftHand:
			case CharacterEditor.SnapTarget.RightHand:
				hashSet.Add(BlocksterGearType.RightHand);
				hashSet.Add(BlocksterGearType.LeftHand);
				break;
			}
			foreach (BlocksterGearType blocksterGearType in hashSet)
			{
				Vector3 snapTargetPosition = this.GetSnapTargetPosition(snapTarget, blocksterGearType);
				Physics.OverlapBoxNonAlloc(snapTargetPosition, one, array);
				foreach (Collider collider in array)
				{
					if (!(collider == null) && !(collider.gameObject == null))
					{
						Block block = BWSceneManager.FindBlock(collider.gameObject, true);
						if (block != null && block.go != null && CharacterEditor.GetBlocksterGearType(block) == blocksterGearType)
						{
							ConnectednessGraph.Remove(block);
							this._attachedGear.Add(block);
						}
					}
				}
			}
		}
	}

	// Token: 0x06001338 RID: 4920 RVA: 0x000841C0 File Offset: 0x000825C0
	public bool InEditMode()
	{
		return this._character != null;
	}

	// Token: 0x06001339 RID: 4921 RVA: 0x000841D0 File Offset: 0x000825D0
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
					blocksterGearType = ((!flag) ? BlocksterGearType.HeadSide : BlocksterGearType.Head);
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

	// Token: 0x04000F12 RID: 3858
	private static CharacterEditor _instance;

	// Token: 0x04000F13 RID: 3859
	private BlockAnimatedCharacter _character;

	// Token: 0x04000F14 RID: 3860
	private Vector3 _restoreColliderSize;

	// Token: 0x04000F15 RID: 3861
	private List<Block> _attachedGear = new List<Block>();

	// Token: 0x04000F16 RID: 3862
	private List<Block> _displacedGear = new List<Block>();

	// Token: 0x04000F17 RID: 3863
	private HashSet<Block> _nonGearAttachments = new HashSet<Block>();

	// Token: 0x04000F18 RID: 3864
	private GameObject _TColliderGameObject;

	// Token: 0x04000F19 RID: 3865
	private Dictionary<CharacterEditor.SnapTarget, Vector3> _snapTargetPositions = new Dictionary<CharacterEditor.SnapTarget, Vector3>();

	// Token: 0x04000F1A RID: 3866
	private Dictionary<CharacterEditor.SnapTarget, Quaternion> _snapTargetRotations = new Dictionary<CharacterEditor.SnapTarget, Quaternion>();

	// Token: 0x04000F1B RID: 3867
	private Dictionary<BlocksterGearType, Quaternion> _gearSnapRotations = new Dictionary<BlocksterGearType, Quaternion>();

	// Token: 0x04000F1C RID: 3868
	private static List<CharacterEditor.SnapTarget> AllSnapTargets = new List<CharacterEditor.SnapTarget>
	{
		CharacterEditor.SnapTarget.Head,
		CharacterEditor.SnapTarget.Body,
		CharacterEditor.SnapTarget.HeadLeft,
		CharacterEditor.SnapTarget.HeadRight,
		CharacterEditor.SnapTarget.HeadBack,
		CharacterEditor.SnapTarget.HeadFront,
		CharacterEditor.SnapTarget.RightHand,
		CharacterEditor.SnapTarget.LeftHand
	};

	// Token: 0x02000110 RID: 272
	public enum SnapTarget
	{
		// Token: 0x04000F1E RID: 3870
		None,
		// Token: 0x04000F1F RID: 3871
		Head,
		// Token: 0x04000F20 RID: 3872
		Body,
		// Token: 0x04000F21 RID: 3873
		HeadLeft,
		// Token: 0x04000F22 RID: 3874
		HeadRight,
		// Token: 0x04000F23 RID: 3875
		HeadBack,
		// Token: 0x04000F24 RID: 3876
		HeadFront,
		// Token: 0x04000F25 RID: 3877
		LeftHand,
		// Token: 0x04000F26 RID: 3878
		RightHand
	}
}
