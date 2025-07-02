using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures;

public class PullObjectGesture : BaseGesture
{
	private readonly LineRenderer _lineRenderer;

	private static HashSet<Block> _lockPulledBlocks;

	private Rigidbody _hitRigidbody;

	private Block _hitBlock;

	private Vector3 _hitPoint;

	private Vector3 _hitHandle;

	private Vector3 _planePoint;

	private Vector3 _planeNormal;

	private Vector2 _lastTouch;

	private Vector3 _oldPull = Vector3.zero;

	private bool _pushOnNextFixedUpdate;

	private static Vector3 worldTapPos;

	private static Vector3 possibleWorldTapPos;

	private static bool hasWorldTapPos;

	private static float worldTapTime;

	private const int LAYER_MASK = 234011;

	public PullObjectGesture(LineRenderer lineRenderer)
	{
		_lineRenderer = lineRenderer;
	}

	public static bool IsPullLocked(Block b)
	{
		return _lockPulledBlocks.Contains(b);
	}

	public static void PullLock(Block b)
	{
		_lockPulledBlocks.Add(b);
	}

	public static bool HasWorldTapPos()
	{
		return hasWorldTapPos;
	}

	public static Vector3 GetWorldTapPos()
	{
		return worldTapPos;
	}

	public static float GetWorldTapTime()
	{
		return worldTapTime;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (allTouches.Count > 1 || Blocksworld.lockInput)
		{
			StopPull();
			EnterState(GestureState.Failed);
		}
		else
		{
			if (allTouches[0].Phase != TouchPhase.Began)
			{
				return;
			}
			if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
			{
				_lastTouch = allTouches[0].Position;
				Blocksworld.blocksworldCamera.firstPersonLookOffset = Vector3.zero;
				EnterState(GestureState.Active);
				return;
			}
			Ray ray = Blocksworld.CameraScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
			if (Physics.Raycast(ray, out var hitInfo, 10000f, 234011))
			{
				GameObject gameObject = hitInfo.collider.transform.gameObject;
				Block.goTouchStarted = true;
				Block.goTouched = gameObject;
				if (Blocksworld.IsGlobalLockPull())
				{
					return;
				}
				_hitBlock = BWSceneManager.FindBlock(gameObject, checkChildGos: true);
				if (_lockPulledBlocks.Contains(_hitBlock))
				{
					return;
				}
				if (_hitBlock != null && _hitBlock.go != Block.goTouched)
				{
					Block.goTouched = _hitBlock.go;
				}
				Rigidbody rigidbody = null;
				if (hitInfo.rigidbody != null)
				{
					rigidbody = hitInfo.rigidbody;
				}
				else if (hitInfo.transform.parent != null && hitInfo.transform.parent.GetComponent<Rigidbody>() != null)
				{
					rigidbody = hitInfo.transform.parent.GetComponent<Rigidbody>();
				}
				possibleWorldTapPos = hitInfo.point;
				if (rigidbody != null && !rigidbody.isKinematic)
				{
					_hitRigidbody = rigidbody;
					_hitPoint = hitInfo.point;
					_hitHandle = Quaternion.Inverse(rigidbody.transform.rotation) * (hitInfo.point - rigidbody.transform.position);
					_lastTouch = allTouches[0].Position;
					StartPull();
					EnterState(GestureState.Active);
					return;
				}
			}
			EnterState(GestureState.Failed);
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
		{
			Blocksworld.blocksworldCamera.firstPersonLookOffset = (allTouches[0].Position - _lastTouch) * NormalizedScreen.scale;
			if (Blocksworld.blocksworldCamera.firstPersonLookXFlip)
			{
				BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
				blocksworldCamera.firstPersonLookOffset.x = blocksworldCamera.firstPersonLookOffset.x * -1f;
			}
			if (Blocksworld.blocksworldCamera.firstPersonLookYFlip)
			{
				BlocksworldCamera blocksworldCamera2 = Blocksworld.blocksworldCamera;
				blocksworldCamera2.firstPersonLookOffset.y = blocksworldCamera2.firstPersonLookOffset.y * -1f;
			}
		}
		else if (_lockPulledBlocks.Contains(_hitBlock) || Blocksworld.IsGlobalLockPull())
		{
			StopPull();
			EnterState(GestureState.Failed);
		}
		else
		{
			_lastTouch = allTouches[0].Position;
		}
	}

	public override void TouchesStationary(List<Touch> allTouches)
	{
		if (allTouches[0].Phase == TouchPhase.Stationary && Blocksworld.blocksworldCamera.firstPersonMode <= 0)
		{
			_lastTouch = allTouches[0].Position;
		}
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		if (allTouches[0].Phase == TouchPhase.Ended)
		{
			if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
			{
				Blocksworld.blocksworldCamera.firstPersonLookOffset = Vector3.zero;
			}
			if (base.gestureState == GestureState.Tracking)
			{
				_pushOnNextFixedUpdate = true;
			}
			else if (base.gestureState == GestureState.Active)
			{
				StopPull();
			}
			hasWorldTapPos = true;
			worldTapPos = possibleWorldTapPos;
			worldTapTime = Time.time;
			EnterState(GestureState.Ended);
		}
	}

	public override void Cancel()
	{
		StopPull();
		EnterState(GestureState.Cancelled);
	}

	public override void Reset()
	{
		if (!_pushOnNextFixedUpdate)
		{
			_hitRigidbody = null;
			_hitBlock = null;
			EnterState(GestureState.Possible);
		}
		_oldPull = Vector3.zero;
		hasWorldTapPos = false;
	}

	public override string ToString()
	{
		return "PullObject(" + _hitRigidbody?.ToString() + ")";
	}

	public void FixedUpdate()
	{
		if (!_lockPulledBlocks.Contains(_hitBlock) && base.gestureState == GestureState.Active && _hitRigidbody != null)
		{
			ContinuePull(_lastTouch);
		}
		_lockPulledBlocks.Clear();
	}

	private void StartPull()
	{
		Blocksworld.blocksworldCamera.SetCameraStill(still: true);
		_planePoint = _hitPoint;
		_planeNormal = -Blocksworld.cameraForward;
		_lineRenderer.enabled = true;
		ContinuePull(_lastTouch);
		if (_hitBlock != null)
		{
			_hitBlock.StartPull();
		}
	}

	private void ContinuePull(Vector2 touch)
	{
		Vector3 handlePos = _hitRigidbody.transform.position + _hitRigidbody.transform.rotation * _hitHandle;
		Vector3 vector = Util.ProjectScreenPointOnWorldPlane(_planePoint, _planeNormal, touch);
		Vector3 pull = 50f * (vector - handlePos);
		float magnitude = _oldPull.magnitude;
		float magnitude2 = pull.magnitude;
		_oldPull = pull;
		float num = 150f;
		if (magnitude2 > num)
		{
			pull = pull.normalized * num * _hitRigidbody.mass;
		}
		_hitRigidbody.AddForceAtPosition(pull, handlePos);
		_lineRenderer.SetPosition(0, handlePos);
		_lineRenderer.SetPosition(1, vector);
		Blocksworld.blocksworldCamera.SetCameraStill(still: true);
		if (_hitBlock == null)
		{
			return;
		}
		if (_hitBlock.broken)
		{
			_hitBlock.chunk.rb.AddForceAtPosition(pull, handlePos);
			return;
		}
		_hitBlock.ConnectionsOfType(2, directed: true).ForEach(delegate(Block b)
		{
			b.chunk.rb.AddForceAtPosition(pull, handlePos);
		});
	}

	private void StopPull()
	{
		_lineRenderer.enabled = false;
		Blocksworld.blocksworldCamera.SetCameraStill(still: false);
		if (_hitBlock != null)
		{
			_hitBlock.StopPull();
		}
	}

	public void HideLine()
	{
		_lineRenderer.enabled = false;
	}

	static PullObjectGesture()
	{
		_lockPulledBlocks = new HashSet<Block>();
		worldTapPos = default(Vector3);
		possibleWorldTapPos = default(Vector3);
		hasWorldTapPos = false;
		worldTapTime = 0f;
	}
}
