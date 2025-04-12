using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002A5 RID: 677
public class CombatController
{
	// Token: 0x06001F5A RID: 8026 RVA: 0x000E2824 File Offset: 0x000E0C24
	public void AttachToStateHandler(CharacterStateHandler handler)
	{
		this._characterStateHandler = handler;
	}

	// Token: 0x06001F5B RID: 8027 RVA: 0x000E2830 File Offset: 0x000E0C30
	public void Play()
	{
		bool flag = this._leftHandAttachment != null;
		bool flag2 = this._rightHandAttachment != null;
		this._rightHandHitBuffer = new RaycastHit[10];
		this._leftHandHitBuffer = new RaycastHit[10];
		this._rightFootHitBuffer = new RaycastHit[10];
		this._leftFootHitBuffer = new RaycastHit[10];
		if (flag)
		{
			Collider component = this._leftHandAttachment.goT.GetComponent<Collider>();
			this.GetCapsuleData(this.leftHandAttachmentParent, component, out this._leftHandAttachmentCapsulePoint1, out this._leftHandAttachmentCapsulePoint2, out this._leftHandAttachmentCapsuleRadius);
			component.isTrigger = true;
			this._leftHandAttachment.PlaceInCharacterHand(this._characterStateHandler.targetObject);
		}
		if (flag2)
		{
			Collider component2 = this._rightHandAttachment.goT.GetComponent<Collider>();
			this.GetCapsuleData(this.rightHandAttachmentParent, component2, out this._rightHandAttachmentCapsulePoint1, out this._rightHandAttachmentCapsulePoint2, out this._rightHandAttachmentCapsuleRadius);
			component2.isTrigger = true;
			this._rightHandAttachment.PlaceInCharacterHand(this._characterStateHandler.targetObject);
		}
		this.ClearAttackFlags();
		this._blocksHitByLeftHandDuringAttack.Clear();
		this._blocksHitByRightHandDuringAttack.Clear();
		this._blocksHitByFeetDuringAttack.Clear();
		this._rightHandPositionHistory.Clear();
		this._leftHandPositionHistory.Clear();
		this._rightFootPositionHistory.Clear();
		this._leftFootPositionHistory.Clear();
	}

	// Token: 0x06001F5C RID: 8028 RVA: 0x000E2984 File Offset: 0x000E0D84
	public void Stop()
	{
		bool flag = this._leftHandAttachment != null;
		bool flag2 = this._rightHandAttachment != null;
		if (flag)
		{
			Collider component = this._leftHandAttachment.goT.GetComponent<Collider>();
			component.isTrigger = false;
			this._leftHandAttachment.RemoveFromCharacterHand();
		}
		if (flag2)
		{
			Collider component2 = this._rightHandAttachment.goT.GetComponent<Collider>();
			component2.isTrigger = false;
			this._rightHandAttachment.RemoveFromCharacterHand();
		}
		this.ClearAttackFlags();
		this._blocksHitByLeftHandDuringAttack.Clear();
		this._blocksHitByRightHandDuringAttack.Clear();
		this._blocksHitByRightHandDuringAttack.Clear();
		this._blocksHitByFeetDuringAttack.Clear();
		this._rightHandPositionHistory.Clear();
		this._leftHandPositionHistory.Clear();
		this._rightFootPositionHistory.Clear();
		this._leftFootPositionHistory.Clear();
	}

	// Token: 0x06001F5D RID: 8029 RVA: 0x000E2A5C File Offset: 0x000E0E5C
	public void Update()
	{
		if (this._rightHandAttachment != null)
		{
			this._rightHandPositionHistory.Insert(0, this.rightHandAttachmentParent.TransformPoint(this._rightHandAttachmentCapsulePoint2));
		}
		else
		{
			this._rightHandPositionHistory.Insert(0, this.rightHandAttachmentParent.position);
		}
		if (this._leftHandAttachment != null)
		{
			this._leftHandPositionHistory.Insert(0, this.leftHandAttachmentParent.TransformPoint(this._leftHandAttachmentCapsulePoint2));
		}
		else
		{
			this._leftHandPositionHistory.Insert(0, this.leftHandAttachmentParent.position);
		}
		this._rightFootPositionHistory.Insert(0, this.rightFootAttachmentParent.position);
		this._leftFootPositionHistory.Insert(0, this.leftFootAttachmentParent.position);
		if (this._rightHandPositionHistory.Count >= 20)
		{
			this._rightHandPositionHistory.RemoveAt(19);
		}
		if (this._leftHandPositionHistory.Count >= 20)
		{
			this._leftHandPositionHistory.RemoveAt(19);
		}
		if (this._rightFootPositionHistory.Count >= 20)
		{
			this._rightFootPositionHistory.RemoveAt(19);
		}
		if (this._leftFootPositionHistory.Count >= 20)
		{
			this._leftFootPositionHistory.RemoveAt(19);
		}
	}

	// Token: 0x06001F5E RID: 8030 RVA: 0x000E2B9B File Offset: 0x000E0F9B
	public void ClearAttackFlags()
	{
		this._blocksHitByLeftHandThisFrame.Clear();
		this._blocksHitByRightHandThisFrame.Clear();
		this._blocksHitByFeetThisFrame.Clear();
		this._rightHandAttachmentWeaponFire = false;
		this._leftHandAttachmentWeaponFire = false;
		this._isShielding = false;
	}

	// Token: 0x06001F5F RID: 8031 RVA: 0x000E2BD3 File Offset: 0x000E0FD3
	public void IgnoreRaycasts(bool ignore, Layer ignoreLayer)
	{
		if (this._rightHandAttachment != null)
		{
			this._rightHandAttachment.go.SetLayer(ignoreLayer, true);
		}
		if (this._leftHandAttachment != null)
		{
			this._leftHandAttachment.go.SetLayer(ignoreLayer, true);
		}
	}

	// Token: 0x06001F60 RID: 8032 RVA: 0x000E2C10 File Offset: 0x000E1010
	public void AddRightHandAttachment(Block block, bool applyOffset)
	{
		this._rightHandAttachment = block;
		if (this._rightHandAttachment != null)
		{
			this._rightHandAttachment.goT.SetParent(this.rightHandAttachmentParent);
			if (applyOffset)
			{
				this._rightHandAttachment.goT.localPosition = this._characterStateHandler.targetObject.GetRightHandAttachOffset();
			}
		}
	}

	// Token: 0x06001F61 RID: 8033 RVA: 0x000E2C6C File Offset: 0x000E106C
	public void AddLeftHandAttachment(Block block, bool applyOffset)
	{
		this._leftHandAttachment = block;
		if (this._leftHandAttachment != null)
		{
			this._leftHandAttachment.goT.SetParent(this.leftHandAttachmentParent);
			if (applyOffset)
			{
				this._leftHandAttachment.goT.localPosition = this._characterStateHandler.targetObject.GetLeftHandAttachOffset();
			}
		}
	}

	// Token: 0x06001F62 RID: 8034 RVA: 0x000E2CC7 File Offset: 0x000E10C7
	public void UnparentHandAttachments()
	{
		if (this._leftHandAttachment != null)
		{
			this._leftHandAttachment.goT.SetParent(null);
		}
		if (this._rightHandAttachment != null)
		{
			this._rightHandAttachment.goT.SetParent(null);
		}
	}

	// Token: 0x06001F63 RID: 8035 RVA: 0x000E2D01 File Offset: 0x000E1101
	public Block GetRightHandAttachment()
	{
		return this._rightHandAttachment;
	}

	// Token: 0x06001F64 RID: 8036 RVA: 0x000E2D09 File Offset: 0x000E1109
	public Block GetLeftHandAttachment()
	{
		return this._leftHandAttachment;
	}

	// Token: 0x06001F65 RID: 8037 RVA: 0x000E2D11 File Offset: 0x000E1111
	public void RemoveRightHandAttachment()
	{
		this._rightHandAttachment = null;
	}

	// Token: 0x06001F66 RID: 8038 RVA: 0x000E2D1A File Offset: 0x000E111A
	public void RemoveLeftHandAttachment()
	{
		this._leftHandAttachment = null;
	}

	// Token: 0x06001F67 RID: 8039 RVA: 0x000E2D23 File Offset: 0x000E1123
	public void ClearRightHandAttackHitBlocks()
	{
		this._blocksHitByRightHandDuringAttack.Clear();
	}

	// Token: 0x06001F68 RID: 8040 RVA: 0x000E2D30 File Offset: 0x000E1130
	public void ClearLeftHandAttackHitBlocks()
	{
		this._blocksHitByLeftHandDuringAttack.Clear();
	}

	// Token: 0x06001F69 RID: 8041 RVA: 0x000E2D3D File Offset: 0x000E113D
	public void ClearFeetAttackHitBlocks()
	{
		this._blocksHitByFeetDuringAttack.Clear();
	}

	// Token: 0x06001F6A RID: 8042 RVA: 0x000E2D4C File Offset: 0x000E114C
	public bool IsHitByBlockThisFrame(Block hitBlock, Block attackingBlock)
	{
		bool flag = this._rightHandAttachment != null && attackingBlock == this._rightHandAttachment && this._blocksHitByRightHandThisFrame.Contains(hitBlock);
		bool flag2 = this._leftHandAttachment != null && attackingBlock == this._leftHandAttachment && this._blocksHitByLeftHandThisFrame.Contains(hitBlock);
		return flag || flag2;
	}

	// Token: 0x06001F6B RID: 8043 RVA: 0x000E2DB1 File Offset: 0x000E11B1
	public bool IsHitByFootThisFrame(Block hitBlock)
	{
		return this._blocksHitByFeetThisFrame.Contains(hitBlock);
	}

	// Token: 0x06001F6C RID: 8044 RVA: 0x000E2DC0 File Offset: 0x000E11C0
	public bool AlreadyHitDuringAttack(Block hitBlock)
	{
		bool flag = this._blocksHitByRightHandDuringAttack.Contains(hitBlock) || this._blocksHitByLeftHandDuringAttack.Contains(hitBlock);
		return flag | this._blocksHitByFeetDuringAttack.Contains(hitBlock);
	}

	// Token: 0x06001F6D RID: 8045 RVA: 0x000E2DFE File Offset: 0x000E11FE
	public HashSet<Block> BlocksHitThisFrameByBlock(Block attackingBlock)
	{
		if (attackingBlock == this._rightHandAttachment)
		{
			return this._blocksHitByRightHandThisFrame;
		}
		if (attackingBlock == this._leftHandAttachment)
		{
			return this._blocksHitByLeftHandThisFrame;
		}
		return null;
	}

	// Token: 0x06001F6E RID: 8046 RVA: 0x000E2E27 File Offset: 0x000E1227
	public HashSet<Block> BlocksHitThisFrameByFeet()
	{
		return this._blocksHitByFeetThisFrame;
	}

	// Token: 0x06001F6F RID: 8047 RVA: 0x000E2E30 File Offset: 0x000E1230
	public void OnHitByMeleeAttack(Block hitBlock, Block attackingBlock)
	{
		if (attackingBlock == this._rightHandAttachment)
		{
			this._blocksHitByRightHandDuringAttack.Add(hitBlock);
			this._blocksHitByRightHandThisFrame.Add(hitBlock);
		}
		if (attackingBlock == this._leftHandAttachment)
		{
			this._blocksHitByLeftHandDuringAttack.Add(hitBlock);
			this._blocksHitByLeftHandThisFrame.Add(hitBlock);
		}
	}

	// Token: 0x06001F70 RID: 8048 RVA: 0x000E2E89 File Offset: 0x000E1289
	public void OnHitByFoot(Block hitBlock)
	{
		this._blocksHitByFeetThisFrame.Add(hitBlock);
		this._blocksHitByFeetDuringAttack.Add(hitBlock);
	}

	// Token: 0x06001F71 RID: 8049 RVA: 0x000E2EA5 File Offset: 0x000E12A5
	public bool RightAttachmentFired()
	{
		return this._rightHandAttachmentWeaponFire;
	}

	// Token: 0x06001F72 RID: 8050 RVA: 0x000E2EAD File Offset: 0x000E12AD
	public bool LeftAttachmentFired()
	{
		return this._leftHandAttachmentWeaponFire;
	}

	// Token: 0x06001F73 RID: 8051 RVA: 0x000E2EB8 File Offset: 0x000E12B8
	public HashSet<Block> CheckLeftHandMeleeCollisions()
	{
		if (this._leftHandAttachment == null || this._leftHandPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = this._leftHandPositionHistory[0];
		Vector3 lastPos = this._leftHandPositionHistory[1];
		return this.SweepCapsule(this.leftHandAttachmentParent, this._leftHandAttachmentCapsulePoint1, this._leftHandAttachmentCapsulePoint2, this._leftHandAttachmentCapsuleRadius, pos, lastPos, this._leftHandHitBuffer);
	}

	// Token: 0x06001F74 RID: 8052 RVA: 0x000E2F24 File Offset: 0x000E1324
	public HashSet<Block> CheckRightHandMeleeCollisions()
	{
		if (this._rightHandAttachment == null || this._rightHandPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = this._rightHandPositionHistory[0];
		Vector3 lastPos = this._rightHandPositionHistory[1];
		return this.SweepCapsule(this.rightHandAttachmentParent, this._rightHandAttachmentCapsulePoint1, this._rightHandAttachmentCapsulePoint2, this._rightHandAttachmentCapsuleRadius, pos, lastPos, this._rightHandHitBuffer);
	}

	// Token: 0x06001F75 RID: 8053 RVA: 0x000E2F90 File Offset: 0x000E1390
	public HashSet<Block> CheckLeftFootCollisions()
	{
		if (this._leftFootPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = this._leftFootPositionHistory[0];
		Vector3 lastPos = this._leftFootPositionHistory[1];
		return this.SweepSphere(this.leftFootAttachmentParent, this._footRadius, pos, lastPos, this._leftFootHitBuffer);
	}

	// Token: 0x06001F76 RID: 8054 RVA: 0x000E2FE4 File Offset: 0x000E13E4
	public HashSet<Block> CheckRightFootCollisions()
	{
		if (this._rightFootPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = this._rightFootPositionHistory[0];
		Vector3 lastPos = this._rightFootPositionHistory[1];
		return this.SweepSphere(this.rightFootAttachmentParent, this._footRadius, pos, lastPos, this._rightFootHitBuffer);
	}

	// Token: 0x06001F77 RID: 8055 RVA: 0x000E3038 File Offset: 0x000E1438
	private void GetCapsuleData(Transform relativeToTransform, Collider c, out Vector3 capsulePoint1, out Vector3 capsulePoint2, out float capsuleRadius)
	{
		if (c is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = c as CapsuleCollider;
			Vector3 a = (capsuleCollider.direction != 0) ? ((capsuleCollider.direction != 1) ? relativeToTransform.forward : relativeToTransform.up) : relativeToTransform.right;
			Vector3 a2 = relativeToTransform.TransformPoint(capsuleCollider.center);
			Vector3 b = a * (capsuleCollider.height / 2f + capsuleCollider.radius);
			Vector3 position = a2 - b;
			Vector3 position2 = a2 + b;
			capsulePoint1 = relativeToTransform.InverseTransformPoint(position);
			capsulePoint2 = relativeToTransform.InverseTransformPoint(position2);
			capsuleRadius = capsuleCollider.radius;
			return;
		}
		Bounds bounds = c.bounds;
		Vector3 forward = relativeToTransform.forward;
		Vector3 vector = bounds.ClosestPoint(relativeToTransform.position + forward * 10f);
		Vector3 a3 = bounds.ClosestPoint(relativeToTransform.position - forward * 10f);
		Vector3 vector2 = vector - relativeToTransform.position;
		Vector3 vector3 = a3 - relativeToTransform.position;
		vector = relativeToTransform.position + forward * vector2.magnitude * Vector3.Dot(vector2.normalized, forward);
		a3 = relativeToTransform.position;
		capsuleRadius = 0.75f;
		capsulePoint1 = relativeToTransform.InverseTransformPoint(a3 + forward * capsuleRadius);
		capsulePoint2 = relativeToTransform.InverseTransformPoint(vector);
	}

	// Token: 0x06001F78 RID: 8056 RVA: 0x000E31C4 File Offset: 0x000E15C4
	private HashSet<Block> SweepCapsule(Transform parentTransform, Vector3 capsulePoint1, Vector3 capsulePoint2, float capsuleRadius, Vector3 pos, Vector3 lastPos, RaycastHit[] hitBuffer)
	{
		Vector3 b = pos - lastPos;
		HashSet<Block> hashSet = new HashSet<Block>();
		if (b.sqrMagnitude < Mathf.Epsilon)
		{
			return hashSet;
		}
		Vector3 point = parentTransform.TransformPoint(capsulePoint1) - b;
		Vector3 point2 = parentTransform.TransformPoint(capsulePoint2) - b;
		Vector3 normalized = b.normalized;
		float magnitude = b.magnitude;
		int num = Physics.CapsuleCastNonAlloc(point, point2, capsuleRadius, normalized, hitBuffer, magnitude);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hitBuffer[i];
			if (this._rightHandAttachment == null || !(raycastHit.collider.gameObject == this._rightHandAttachment.go))
			{
				if (this._leftHandAttachment == null || !(raycastHit.collider.gameObject == this._leftHandAttachment.go))
				{
					if (!(raycastHit.collider.gameObject == this._characterStateHandler.targetObject.go))
					{
						Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
						if (block != null)
						{
							if (!block.isTerrain)
							{
								if (!this._characterStateHandler.targetObject.IsAttachment(block))
								{
									hashSet.Add(block);
								}
							}
						}
					}
				}
			}
		}
		return hashSet;
	}

	// Token: 0x06001F79 RID: 8057 RVA: 0x000E3340 File Offset: 0x000E1740
	private HashSet<Block> SweepSphere(Transform parentTransform, float radius, Vector3 pos, Vector3 lastPos, RaycastHit[] hitBuffer)
	{
		Vector3 b = pos - lastPos;
		HashSet<Block> hashSet = new HashSet<Block>();
		if (b.sqrMagnitude < Mathf.Epsilon)
		{
			return hashSet;
		}
		Vector3 origin = parentTransform.position - b;
		Vector3 normalized = b.normalized;
		float magnitude = b.magnitude;
		int num = Physics.SphereCastNonAlloc(origin, radius, normalized, hitBuffer, magnitude);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hitBuffer[i];
			if (this._rightHandAttachment == null || !(raycastHit.collider.gameObject == this._rightHandAttachment.go))
			{
				if (this._leftHandAttachment == null || !(raycastHit.collider.gameObject == this._leftHandAttachment.go))
				{
					if (!(raycastHit.collider.gameObject == this._characterStateHandler.targetObject.go))
					{
						Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
						if (block != null)
						{
							if (!block.isTerrain)
							{
								if (!this._characterStateHandler.targetObject.IsAttachment(block))
								{
									hashSet.Add(block);
								}
							}
						}
					}
				}
			}
		}
		return hashSet;
	}

	// Token: 0x06001F7A RID: 8058 RVA: 0x000E34A6 File Offset: 0x000E18A6
	public void OnFireLeftHandWeapon()
	{
		this._leftHandAttachmentWeaponFire = true;
	}

	// Token: 0x06001F7B RID: 8059 RVA: 0x000E34AF File Offset: 0x000E18AF
	public void OnFireRightHandWeapon()
	{
		this._rightHandAttachmentWeaponFire = true;
	}

	// Token: 0x06001F7C RID: 8060 RVA: 0x000E34B8 File Offset: 0x000E18B8
	public void SetAttackDamageDone(bool done)
	{
		this._attackDamageDone = done;
	}

	// Token: 0x06001F7D RID: 8061 RVA: 0x000E34C1 File Offset: 0x000E18C1
	public bool AttackDamageDone()
	{
		return this._attackDamageDone;
	}

	// Token: 0x06001F7E RID: 8062 RVA: 0x000E34C9 File Offset: 0x000E18C9
	public void ActivateLeftShield()
	{
		if (this._leftHandAttachment == null)
		{
			return;
		}
		this.leftHandAttachmentIsShielding = true;
		this._leftHandAttachment.go.GetComponent<Collider>().isTrigger = false;
	}

	// Token: 0x06001F7F RID: 8063 RVA: 0x000E34F4 File Offset: 0x000E18F4
	public void ActivateRightShield()
	{
		if (this._rightHandAttachment == null)
		{
			return;
		}
		this.rightHandAttachmentIsShielding = true;
		this._rightHandAttachment.go.GetComponent<Collider>().isTrigger = false;
	}

	// Token: 0x06001F80 RID: 8064 RVA: 0x000E351F File Offset: 0x000E191F
	public void DeactivateLeftShield()
	{
		this.leftHandAttachmentIsShielding = false;
		this._leftHandAttachment.go.GetComponent<Collider>().isTrigger = true;
	}

	// Token: 0x06001F81 RID: 8065 RVA: 0x000E353E File Offset: 0x000E193E
	public void DeactivateRightShield()
	{
		this.rightHandAttachmentIsShielding = false;
		this._rightHandAttachment.go.GetComponent<Collider>().isTrigger = true;
	}

	// Token: 0x06001F82 RID: 8066 RVA: 0x000E355D File Offset: 0x000E195D
	public void HitRightShield()
	{
		this._rightAttachmentShieldImpact = true;
	}

	// Token: 0x06001F83 RID: 8067 RVA: 0x000E3566 File Offset: 0x000E1966
	public void HitLeftShield()
	{
		this._leftAttachmentShieldImpact = true;
	}

	// Token: 0x06001F84 RID: 8068 RVA: 0x000E356F File Offset: 0x000E196F
	public bool IsShieldHit()
	{
		return this._rightAttachmentShieldImpact || this._leftAttachmentShieldImpact;
	}

	// Token: 0x06001F85 RID: 8069 RVA: 0x000E3585 File Offset: 0x000E1985
	public void SetIsShielding()
	{
		this._isShielding = true;
	}

	// Token: 0x06001F86 RID: 8070 RVA: 0x000E358E File Offset: 0x000E198E
	public void ClearShieldHitFlags()
	{
		this._rightAttachmentShieldImpact = false;
		this._leftAttachmentShieldImpact = false;
	}

	// Token: 0x06001F87 RID: 8071 RVA: 0x000E359E File Offset: 0x000E199E
	public bool IsShielding()
	{
		return this._isShielding;
	}

	// Token: 0x06001F88 RID: 8072 RVA: 0x000E35A6 File Offset: 0x000E19A6
	public Vector3 RightHandDelta()
	{
		if (this._rightHandPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return this._rightHandPositionHistory[0] - this._rightHandPositionHistory[1];
	}

	// Token: 0x06001F89 RID: 8073 RVA: 0x000E35DC File Offset: 0x000E19DC
	public Vector3 LeftHandDelta()
	{
		if (this._leftHandPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return this._leftHandPositionHistory[0] - this._leftHandPositionHistory[1];
	}

	// Token: 0x06001F8A RID: 8074 RVA: 0x000E3612 File Offset: 0x000E1A12
	public Vector3 RightFootDelta()
	{
		if (this._rightFootPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return this._rightFootPositionHistory[0] - this._rightFootPositionHistory[1];
	}

	// Token: 0x06001F8B RID: 8075 RVA: 0x000E3648 File Offset: 0x000E1A48
	public Vector3 LeftFootDelta()
	{
		if (this._leftFootPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return this._leftFootPositionHistory[0] - this._leftFootPositionHistory[1];
	}

	// Token: 0x0400198C RID: 6540
	public Transform rightHandAttachmentParent;

	// Token: 0x0400198D RID: 6541
	public Transform leftHandAttachmentParent;

	// Token: 0x0400198E RID: 6542
	public Transform rightFootAttachmentParent;

	// Token: 0x0400198F RID: 6543
	public Transform leftFootAttachmentParent;

	// Token: 0x04001990 RID: 6544
	public bool rightHandAttachmentIsAttacking;

	// Token: 0x04001991 RID: 6545
	public bool leftHandAttachmentIsAttacking;

	// Token: 0x04001992 RID: 6546
	public bool rightHandAttachmentIsShielding;

	// Token: 0x04001993 RID: 6547
	public bool leftHandAttachmentIsShielding;

	// Token: 0x04001994 RID: 6548
	private Block _leftHandAttachment;

	// Token: 0x04001995 RID: 6549
	private Block _rightHandAttachment;

	// Token: 0x04001996 RID: 6550
	private const int _handPositionBufferSize = 20;

	// Token: 0x04001997 RID: 6551
	private List<Vector3> _rightHandPositionHistory = new List<Vector3>(40);

	// Token: 0x04001998 RID: 6552
	private List<Vector3> _leftHandPositionHistory = new List<Vector3>(40);

	// Token: 0x04001999 RID: 6553
	private List<Vector3> _leftFootPositionHistory = new List<Vector3>(40);

	// Token: 0x0400199A RID: 6554
	private List<Vector3> _rightFootPositionHistory = new List<Vector3>(40);

	// Token: 0x0400199B RID: 6555
	private Vector3 _rightHandAttachmentCapsulePoint1;

	// Token: 0x0400199C RID: 6556
	private Vector3 _rightHandAttachmentCapsulePoint2;

	// Token: 0x0400199D RID: 6557
	private float _rightHandAttachmentCapsuleRadius;

	// Token: 0x0400199E RID: 6558
	private Vector3 _leftHandAttachmentCapsulePoint1;

	// Token: 0x0400199F RID: 6559
	private Vector3 _leftHandAttachmentCapsulePoint2;

	// Token: 0x040019A0 RID: 6560
	private float _leftHandAttachmentCapsuleRadius;

	// Token: 0x040019A1 RID: 6561
	private float _footRadius = 0.65f;

	// Token: 0x040019A2 RID: 6562
	private RaycastHit[] _rightHandHitBuffer;

	// Token: 0x040019A3 RID: 6563
	private RaycastHit[] _leftHandHitBuffer;

	// Token: 0x040019A4 RID: 6564
	private RaycastHit[] _rightFootHitBuffer;

	// Token: 0x040019A5 RID: 6565
	private RaycastHit[] _leftFootHitBuffer;

	// Token: 0x040019A6 RID: 6566
	private HashSet<Block> _blocksHitByRightHandThisFrame = new HashSet<Block>();

	// Token: 0x040019A7 RID: 6567
	private HashSet<Block> _blocksHitByLeftHandThisFrame = new HashSet<Block>();

	// Token: 0x040019A8 RID: 6568
	private HashSet<Block> _blocksHitByFeetThisFrame = new HashSet<Block>();

	// Token: 0x040019A9 RID: 6569
	private bool _rightHandAttachmentWeaponFire;

	// Token: 0x040019AA RID: 6570
	private bool _leftHandAttachmentWeaponFire;

	// Token: 0x040019AB RID: 6571
	private bool _rightAttachmentShieldImpact;

	// Token: 0x040019AC RID: 6572
	private bool _leftAttachmentShieldImpact;

	// Token: 0x040019AD RID: 6573
	private bool _isShielding;

	// Token: 0x040019AE RID: 6574
	private bool _attackDamageDone;

	// Token: 0x040019AF RID: 6575
	private HashSet<Block> _blocksHitByRightHandDuringAttack = new HashSet<Block>();

	// Token: 0x040019B0 RID: 6576
	private HashSet<Block> _blocksHitByLeftHandDuringAttack = new HashSet<Block>();

	// Token: 0x040019B1 RID: 6577
	private HashSet<Block> _blocksHitByFeetDuringAttack = new HashSet<Block>();

	// Token: 0x040019B2 RID: 6578
	private CharacterStateHandler _characterStateHandler;
}
