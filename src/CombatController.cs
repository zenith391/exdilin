using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class CombatController
{
	public Transform rightHandAttachmentParent;

	public Transform leftHandAttachmentParent;

	public Transform rightFootAttachmentParent;

	public Transform leftFootAttachmentParent;

	public bool rightHandAttachmentIsAttacking;

	public bool leftHandAttachmentIsAttacking;

	public bool rightHandAttachmentIsShielding;

	public bool leftHandAttachmentIsShielding;

	private Block _leftHandAttachment;

	private Block _rightHandAttachment;

	private const int _handPositionBufferSize = 20;

	private List<Vector3> _rightHandPositionHistory = new List<Vector3>(40);

	private List<Vector3> _leftHandPositionHistory = new List<Vector3>(40);

	private List<Vector3> _leftFootPositionHistory = new List<Vector3>(40);

	private List<Vector3> _rightFootPositionHistory = new List<Vector3>(40);

	private Vector3 _rightHandAttachmentCapsulePoint1;

	private Vector3 _rightHandAttachmentCapsulePoint2;

	private float _rightHandAttachmentCapsuleRadius;

	private Vector3 _leftHandAttachmentCapsulePoint1;

	private Vector3 _leftHandAttachmentCapsulePoint2;

	private float _leftHandAttachmentCapsuleRadius;

	private float _footRadius = 0.65f;

	private RaycastHit[] _rightHandHitBuffer;

	private RaycastHit[] _leftHandHitBuffer;

	private RaycastHit[] _rightFootHitBuffer;

	private RaycastHit[] _leftFootHitBuffer;

	private HashSet<Block> _blocksHitByRightHandThisFrame = new HashSet<Block>();

	private HashSet<Block> _blocksHitByLeftHandThisFrame = new HashSet<Block>();

	private HashSet<Block> _blocksHitByFeetThisFrame = new HashSet<Block>();

	private bool _rightHandAttachmentWeaponFire;

	private bool _leftHandAttachmentWeaponFire;

	private bool _rightAttachmentShieldImpact;

	private bool _leftAttachmentShieldImpact;

	private bool _isShielding;

	private bool _attackDamageDone;

	private HashSet<Block> _blocksHitByRightHandDuringAttack = new HashSet<Block>();

	private HashSet<Block> _blocksHitByLeftHandDuringAttack = new HashSet<Block>();

	private HashSet<Block> _blocksHitByFeetDuringAttack = new HashSet<Block>();

	private CharacterStateHandler _characterStateHandler;

	public void AttachToStateHandler(CharacterStateHandler handler)
	{
		_characterStateHandler = handler;
	}

	public void Play()
	{
		bool flag = _leftHandAttachment != null;
		bool flag2 = _rightHandAttachment != null;
		_rightHandHitBuffer = new RaycastHit[10];
		_leftHandHitBuffer = new RaycastHit[10];
		_rightFootHitBuffer = new RaycastHit[10];
		_leftFootHitBuffer = new RaycastHit[10];
		if (flag)
		{
			Collider component = _leftHandAttachment.goT.GetComponent<Collider>();
			GetCapsuleData(leftHandAttachmentParent, component, out _leftHandAttachmentCapsulePoint1, out _leftHandAttachmentCapsulePoint2, out _leftHandAttachmentCapsuleRadius);
			component.isTrigger = true;
			_leftHandAttachment.PlaceInCharacterHand(_characterStateHandler.targetObject);
		}
		if (flag2)
		{
			Collider component2 = _rightHandAttachment.goT.GetComponent<Collider>();
			GetCapsuleData(rightHandAttachmentParent, component2, out _rightHandAttachmentCapsulePoint1, out _rightHandAttachmentCapsulePoint2, out _rightHandAttachmentCapsuleRadius);
			component2.isTrigger = true;
			_rightHandAttachment.PlaceInCharacterHand(_characterStateHandler.targetObject);
		}
		ClearAttackFlags();
		_blocksHitByLeftHandDuringAttack.Clear();
		_blocksHitByRightHandDuringAttack.Clear();
		_blocksHitByFeetDuringAttack.Clear();
		_rightHandPositionHistory.Clear();
		_leftHandPositionHistory.Clear();
		_rightFootPositionHistory.Clear();
		_leftFootPositionHistory.Clear();
	}

	public void Stop()
	{
		bool flag = _leftHandAttachment != null;
		bool flag2 = _rightHandAttachment != null;
		if (flag)
		{
			Collider component = _leftHandAttachment.goT.GetComponent<Collider>();
			component.isTrigger = false;
			_leftHandAttachment.RemoveFromCharacterHand();
		}
		if (flag2)
		{
			Collider component2 = _rightHandAttachment.goT.GetComponent<Collider>();
			component2.isTrigger = false;
			_rightHandAttachment.RemoveFromCharacterHand();
		}
		ClearAttackFlags();
		_blocksHitByLeftHandDuringAttack.Clear();
		_blocksHitByRightHandDuringAttack.Clear();
		_blocksHitByRightHandDuringAttack.Clear();
		_blocksHitByFeetDuringAttack.Clear();
		_rightHandPositionHistory.Clear();
		_leftHandPositionHistory.Clear();
		_rightFootPositionHistory.Clear();
		_leftFootPositionHistory.Clear();
	}

	public void Update()
	{
		if (_rightHandAttachment != null)
		{
			_rightHandPositionHistory.Insert(0, rightHandAttachmentParent.TransformPoint(_rightHandAttachmentCapsulePoint2));
		}
		else
		{
			_rightHandPositionHistory.Insert(0, rightHandAttachmentParent.position);
		}
		if (_leftHandAttachment != null)
		{
			_leftHandPositionHistory.Insert(0, leftHandAttachmentParent.TransformPoint(_leftHandAttachmentCapsulePoint2));
		}
		else
		{
			_leftHandPositionHistory.Insert(0, leftHandAttachmentParent.position);
		}
		_rightFootPositionHistory.Insert(0, rightFootAttachmentParent.position);
		_leftFootPositionHistory.Insert(0, leftFootAttachmentParent.position);
		if (_rightHandPositionHistory.Count >= 20)
		{
			_rightHandPositionHistory.RemoveAt(19);
		}
		if (_leftHandPositionHistory.Count >= 20)
		{
			_leftHandPositionHistory.RemoveAt(19);
		}
		if (_rightFootPositionHistory.Count >= 20)
		{
			_rightFootPositionHistory.RemoveAt(19);
		}
		if (_leftFootPositionHistory.Count >= 20)
		{
			_leftFootPositionHistory.RemoveAt(19);
		}
	}

	public void ClearAttackFlags()
	{
		_blocksHitByLeftHandThisFrame.Clear();
		_blocksHitByRightHandThisFrame.Clear();
		_blocksHitByFeetThisFrame.Clear();
		_rightHandAttachmentWeaponFire = false;
		_leftHandAttachmentWeaponFire = false;
		_isShielding = false;
	}

	public void IgnoreRaycasts(bool ignore, Layer ignoreLayer)
	{
		if (_rightHandAttachment != null)
		{
			_rightHandAttachment.go.SetLayer(ignoreLayer, recursive: true);
		}
		if (_leftHandAttachment != null)
		{
			_leftHandAttachment.go.SetLayer(ignoreLayer, recursive: true);
		}
	}

	public void AddRightHandAttachment(Block block, bool applyOffset)
	{
		_rightHandAttachment = block;
		if (_rightHandAttachment != null)
		{
			_rightHandAttachment.goT.SetParent(rightHandAttachmentParent);
			if (applyOffset)
			{
				_rightHandAttachment.goT.localPosition = _characterStateHandler.targetObject.GetRightHandAttachOffset();
			}
		}
	}

	public void AddLeftHandAttachment(Block block, bool applyOffset)
	{
		_leftHandAttachment = block;
		if (_leftHandAttachment != null)
		{
			_leftHandAttachment.goT.SetParent(leftHandAttachmentParent);
			if (applyOffset)
			{
				_leftHandAttachment.goT.localPosition = _characterStateHandler.targetObject.GetLeftHandAttachOffset();
			}
		}
	}

	public void UnparentHandAttachments()
	{
		if (_leftHandAttachment != null)
		{
			_leftHandAttachment.goT.SetParent(null);
		}
		if (_rightHandAttachment != null)
		{
			_rightHandAttachment.goT.SetParent(null);
		}
	}

	public Block GetRightHandAttachment()
	{
		return _rightHandAttachment;
	}

	public Block GetLeftHandAttachment()
	{
		return _leftHandAttachment;
	}

	public void RemoveRightHandAttachment()
	{
		_rightHandAttachment = null;
	}

	public void RemoveLeftHandAttachment()
	{
		_leftHandAttachment = null;
	}

	public void ClearRightHandAttackHitBlocks()
	{
		_blocksHitByRightHandDuringAttack.Clear();
	}

	public void ClearLeftHandAttackHitBlocks()
	{
		_blocksHitByLeftHandDuringAttack.Clear();
	}

	public void ClearFeetAttackHitBlocks()
	{
		_blocksHitByFeetDuringAttack.Clear();
	}

	public bool IsHitByBlockThisFrame(Block hitBlock, Block attackingBlock)
	{
		bool flag = _rightHandAttachment != null && attackingBlock == _rightHandAttachment && _blocksHitByRightHandThisFrame.Contains(hitBlock);
		bool flag2 = _leftHandAttachment != null && attackingBlock == _leftHandAttachment && _blocksHitByLeftHandThisFrame.Contains(hitBlock);
		return flag || flag2;
	}

	public bool IsHitByFootThisFrame(Block hitBlock)
	{
		return _blocksHitByFeetThisFrame.Contains(hitBlock);
	}

	public bool AlreadyHitDuringAttack(Block hitBlock)
	{
		bool flag = _blocksHitByRightHandDuringAttack.Contains(hitBlock) || _blocksHitByLeftHandDuringAttack.Contains(hitBlock);
		return flag | _blocksHitByFeetDuringAttack.Contains(hitBlock);
	}

	public HashSet<Block> BlocksHitThisFrameByBlock(Block attackingBlock)
	{
		if (attackingBlock == _rightHandAttachment)
		{
			return _blocksHitByRightHandThisFrame;
		}
		if (attackingBlock == _leftHandAttachment)
		{
			return _blocksHitByLeftHandThisFrame;
		}
		return null;
	}

	public HashSet<Block> BlocksHitThisFrameByFeet()
	{
		return _blocksHitByFeetThisFrame;
	}

	public void OnHitByMeleeAttack(Block hitBlock, Block attackingBlock)
	{
		if (attackingBlock == _rightHandAttachment)
		{
			_blocksHitByRightHandDuringAttack.Add(hitBlock);
			_blocksHitByRightHandThisFrame.Add(hitBlock);
		}
		if (attackingBlock == _leftHandAttachment)
		{
			_blocksHitByLeftHandDuringAttack.Add(hitBlock);
			_blocksHitByLeftHandThisFrame.Add(hitBlock);
		}
	}

	public void OnHitByFoot(Block hitBlock)
	{
		_blocksHitByFeetThisFrame.Add(hitBlock);
		_blocksHitByFeetDuringAttack.Add(hitBlock);
	}

	public bool RightAttachmentFired()
	{
		return _rightHandAttachmentWeaponFire;
	}

	public bool LeftAttachmentFired()
	{
		return _leftHandAttachmentWeaponFire;
	}

	public HashSet<Block> CheckLeftHandMeleeCollisions()
	{
		if (_leftHandAttachment == null || _leftHandPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = _leftHandPositionHistory[0];
		Vector3 lastPos = _leftHandPositionHistory[1];
		return SweepCapsule(leftHandAttachmentParent, _leftHandAttachmentCapsulePoint1, _leftHandAttachmentCapsulePoint2, _leftHandAttachmentCapsuleRadius, pos, lastPos, _leftHandHitBuffer);
	}

	public HashSet<Block> CheckRightHandMeleeCollisions()
	{
		if (_rightHandAttachment == null || _rightHandPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = _rightHandPositionHistory[0];
		Vector3 lastPos = _rightHandPositionHistory[1];
		return SweepCapsule(rightHandAttachmentParent, _rightHandAttachmentCapsulePoint1, _rightHandAttachmentCapsulePoint2, _rightHandAttachmentCapsuleRadius, pos, lastPos, _rightHandHitBuffer);
	}

	public HashSet<Block> CheckLeftFootCollisions()
	{
		if (_leftFootPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = _leftFootPositionHistory[0];
		Vector3 lastPos = _leftFootPositionHistory[1];
		return SweepSphere(leftFootAttachmentParent, _footRadius, pos, lastPos, _leftFootHitBuffer);
	}

	public HashSet<Block> CheckRightFootCollisions()
	{
		if (_rightFootPositionHistory.Count < 2)
		{
			return null;
		}
		Vector3 pos = _rightFootPositionHistory[0];
		Vector3 lastPos = _rightFootPositionHistory[1];
		return SweepSphere(rightFootAttachmentParent, _footRadius, pos, lastPos, _rightFootHitBuffer);
	}

	private void GetCapsuleData(Transform relativeToTransform, Collider c, out Vector3 capsulePoint1, out Vector3 capsulePoint2, out float capsuleRadius)
	{
		if (c is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = c as CapsuleCollider;
			Vector3 vector = ((capsuleCollider.direction == 0) ? relativeToTransform.right : ((capsuleCollider.direction != 1) ? relativeToTransform.forward : relativeToTransform.up));
			Vector3 vector2 = relativeToTransform.TransformPoint(capsuleCollider.center);
			Vector3 vector3 = vector * (capsuleCollider.height / 2f + capsuleCollider.radius);
			Vector3 position = vector2 - vector3;
			Vector3 position2 = vector2 + vector3;
			capsulePoint1 = relativeToTransform.InverseTransformPoint(position);
			capsulePoint2 = relativeToTransform.InverseTransformPoint(position2);
			capsuleRadius = capsuleCollider.radius;
		}
		else
		{
			Bounds bounds = c.bounds;
			Vector3 forward = relativeToTransform.forward;
			Vector3 vector4 = bounds.ClosestPoint(relativeToTransform.position + forward * 10f);
			Vector3 vector5 = bounds.ClosestPoint(relativeToTransform.position - forward * 10f);
			Vector3 vector6 = vector4 - relativeToTransform.position;
			Vector3 vector7 = vector5 - relativeToTransform.position;
			vector4 = relativeToTransform.position + forward * vector6.magnitude * Vector3.Dot(vector6.normalized, forward);
			vector5 = relativeToTransform.position;
			capsuleRadius = 0.75f;
			capsulePoint1 = relativeToTransform.InverseTransformPoint(vector5 + forward * capsuleRadius);
			capsulePoint2 = relativeToTransform.InverseTransformPoint(vector4);
		}
	}

	private HashSet<Block> SweepCapsule(Transform parentTransform, Vector3 capsulePoint1, Vector3 capsulePoint2, float capsuleRadius, Vector3 pos, Vector3 lastPos, RaycastHit[] hitBuffer)
	{
		Vector3 vector = pos - lastPos;
		HashSet<Block> hashSet = new HashSet<Block>();
		if (vector.sqrMagnitude < Mathf.Epsilon)
		{
			return hashSet;
		}
		Vector3 point = parentTransform.TransformPoint(capsulePoint1) - vector;
		Vector3 point2 = parentTransform.TransformPoint(capsulePoint2) - vector;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		int num = Physics.CapsuleCastNonAlloc(point, point2, capsuleRadius, normalized, hitBuffer, magnitude);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hitBuffer[i];
			if ((_rightHandAttachment == null || !(raycastHit.collider.gameObject == _rightHandAttachment.go)) && (_leftHandAttachment == null || !(raycastHit.collider.gameObject == _leftHandAttachment.go)) && !(raycastHit.collider.gameObject == _characterStateHandler.targetObject.go))
			{
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
				if (block != null && !block.isTerrain && !_characterStateHandler.targetObject.IsAttachment(block))
				{
					hashSet.Add(block);
				}
			}
		}
		return hashSet;
	}

	private HashSet<Block> SweepSphere(Transform parentTransform, float radius, Vector3 pos, Vector3 lastPos, RaycastHit[] hitBuffer)
	{
		Vector3 vector = pos - lastPos;
		HashSet<Block> hashSet = new HashSet<Block>();
		if (vector.sqrMagnitude < Mathf.Epsilon)
		{
			return hashSet;
		}
		Vector3 origin = parentTransform.position - vector;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		int num = Physics.SphereCastNonAlloc(origin, radius, normalized, hitBuffer, magnitude);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hitBuffer[i];
			if ((_rightHandAttachment == null || !(raycastHit.collider.gameObject == _rightHandAttachment.go)) && (_leftHandAttachment == null || !(raycastHit.collider.gameObject == _leftHandAttachment.go)) && !(raycastHit.collider.gameObject == _characterStateHandler.targetObject.go))
			{
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
				if (block != null && !block.isTerrain && !_characterStateHandler.targetObject.IsAttachment(block))
				{
					hashSet.Add(block);
				}
			}
		}
		return hashSet;
	}

	public void OnFireLeftHandWeapon()
	{
		_leftHandAttachmentWeaponFire = true;
	}

	public void OnFireRightHandWeapon()
	{
		_rightHandAttachmentWeaponFire = true;
	}

	public void SetAttackDamageDone(bool done)
	{
		_attackDamageDone = done;
	}

	public bool AttackDamageDone()
	{
		return _attackDamageDone;
	}

	public void ActivateLeftShield()
	{
		if (_leftHandAttachment != null)
		{
			leftHandAttachmentIsShielding = true;
			_leftHandAttachment.go.GetComponent<Collider>().isTrigger = false;
		}
	}

	public void ActivateRightShield()
	{
		if (_rightHandAttachment != null)
		{
			rightHandAttachmentIsShielding = true;
			_rightHandAttachment.go.GetComponent<Collider>().isTrigger = false;
		}
	}

	public void DeactivateLeftShield()
	{
		leftHandAttachmentIsShielding = false;
		_leftHandAttachment.go.GetComponent<Collider>().isTrigger = true;
	}

	public void DeactivateRightShield()
	{
		rightHandAttachmentIsShielding = false;
		_rightHandAttachment.go.GetComponent<Collider>().isTrigger = true;
	}

	public void HitRightShield()
	{
		_rightAttachmentShieldImpact = true;
	}

	public void HitLeftShield()
	{
		_leftAttachmentShieldImpact = true;
	}

	public bool IsShieldHit()
	{
		if (!_rightAttachmentShieldImpact)
		{
			return _leftAttachmentShieldImpact;
		}
		return true;
	}

	public void SetIsShielding()
	{
		_isShielding = true;
	}

	public void ClearShieldHitFlags()
	{
		_rightAttachmentShieldImpact = false;
		_leftAttachmentShieldImpact = false;
	}

	public bool IsShielding()
	{
		return _isShielding;
	}

	public Vector3 RightHandDelta()
	{
		if (_rightHandPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return _rightHandPositionHistory[0] - _rightHandPositionHistory[1];
	}

	public Vector3 LeftHandDelta()
	{
		if (_leftHandPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return _leftHandPositionHistory[0] - _leftHandPositionHistory[1];
	}

	public Vector3 RightFootDelta()
	{
		if (_rightFootPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return _rightFootPositionHistory[0] - _rightFootPositionHistory[1];
	}

	public Vector3 LeftFootDelta()
	{
		if (_leftFootPositionHistory.Count < 2)
		{
			return Vector3.zero;
		}
		return _leftFootPositionHistory[0] - _leftFootPositionHistory[1];
	}
}
