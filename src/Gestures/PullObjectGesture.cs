using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000184 RID: 388
	public class PullObjectGesture : BaseGesture
	{
		// Token: 0x06001622 RID: 5666 RVA: 0x0009BC76 File Offset: 0x0009A076
		public PullObjectGesture(LineRenderer lineRenderer)
		{
			this._lineRenderer = lineRenderer;
		}

		// Token: 0x06001623 RID: 5667 RVA: 0x0009BC90 File Offset: 0x0009A090
		public static bool IsPullLocked(Block b)
		{
			return PullObjectGesture._lockPulledBlocks.Contains(b);
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x0009BC9D File Offset: 0x0009A09D
		public static void PullLock(Block b)
		{
			PullObjectGesture._lockPulledBlocks.Add(b);
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x0009BCAB File Offset: 0x0009A0AB
		public static bool HasWorldTapPos()
		{
			return PullObjectGesture.hasWorldTapPos;
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x0009BCB2 File Offset: 0x0009A0B2
		public static Vector3 GetWorldTapPos()
		{
			return PullObjectGesture.worldTapPos;
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x0009BCB9 File Offset: 0x0009A0B9
		public static float GetWorldTapTime()
		{
			return PullObjectGesture.worldTapTime;
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x0009BCC0 File Offset: 0x0009A0C0
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count > 1 || Blocksworld.lockInput)
			{
				this.StopPull();
				base.EnterState(GestureState.Failed);
				return;
			}
			if (allTouches[0].Phase != TouchPhase.Began)
			{
				return;
			}
			if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
			{
				this._lastTouch = allTouches[0].Position;
				Blocksworld.blocksworldCamera.firstPersonLookOffset = Vector3.zero;
				base.EnterState(GestureState.Active);
				return;
			}
			Ray ray = Blocksworld.CameraScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit, 10000f, 234011))
			{
				GameObject gameObject = raycastHit.collider.transform.gameObject;
				Block.goTouchStarted = true;
				Block.goTouched = gameObject;
				if (Blocksworld.IsGlobalLockPull())
				{
					return;
				}
				this._hitBlock = BWSceneManager.FindBlock(gameObject, true);
				if (PullObjectGesture._lockPulledBlocks.Contains(this._hitBlock))
				{
					return;
				}
				if (this._hitBlock != null && this._hitBlock.go != Block.goTouched)
				{
					Block.goTouched = this._hitBlock.go;
				}
				Rigidbody rigidbody = null;
				if (raycastHit.rigidbody != null)
				{
					rigidbody = raycastHit.rigidbody;
				}
				else if (raycastHit.transform.parent != null && raycastHit.transform.parent.GetComponent<Rigidbody>() != null)
				{
					rigidbody = raycastHit.transform.parent.GetComponent<Rigidbody>();
				}
				PullObjectGesture.possibleWorldTapPos = raycastHit.point;
				if (rigidbody != null && !rigidbody.isKinematic)
				{
					this._hitRigidbody = rigidbody;
					this._hitPoint = raycastHit.point;
					this._hitHandle = Quaternion.Inverse(rigidbody.transform.rotation) * (raycastHit.point - rigidbody.transform.position);
					this._lastTouch = allTouches[0].Position;
					this.StartPull();
					base.EnterState(GestureState.Active);
					return;
				}
			}
			base.EnterState(GestureState.Failed);
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x0009BEEC File Offset: 0x0009A2EC
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
			{
				Blocksworld.blocksworldCamera.firstPersonLookOffset = (allTouches[0].Position - this._lastTouch) * NormalizedScreen.scale;
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
				return;
			}
			if (PullObjectGesture._lockPulledBlocks.Contains(this._hitBlock) || Blocksworld.IsGlobalLockPull())
			{
				this.StopPull();
				base.EnterState(GestureState.Failed);
				return;
			}
			this._lastTouch = allTouches[0].Position;
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x0009BFCD File Offset: 0x0009A3CD
		public override void TouchesStationary(List<Touch> allTouches)
		{
			if (allTouches[0].Phase != TouchPhase.Stationary)
			{
				return;
			}
			if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
			{
				return;
			}
			this._lastTouch = allTouches[0].Position;
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x0009C008 File Offset: 0x0009A408
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (allTouches[0].Phase != TouchPhase.Ended)
			{
				return;
			}
			if (Blocksworld.blocksworldCamera.firstPersonMode > 0)
			{
				Blocksworld.blocksworldCamera.firstPersonLookOffset = Vector3.zero;
			}
			if (base.gestureState == GestureState.Tracking)
			{
				this._pushOnNextFixedUpdate = true;
			}
			else if (base.gestureState == GestureState.Active)
			{
				this.StopPull();
			}
			PullObjectGesture.hasWorldTapPos = true;
			PullObjectGesture.worldTapPos = PullObjectGesture.possibleWorldTapPos;
			PullObjectGesture.worldTapTime = Time.time;
			base.EnterState(GestureState.Ended);
		}

		// Token: 0x0600162C RID: 5676 RVA: 0x0009C092 File Offset: 0x0009A492
		public override void Cancel()
		{
			this.StopPull();
			base.EnterState(GestureState.Cancelled);
		}

		// Token: 0x0600162D RID: 5677 RVA: 0x0009C0A1 File Offset: 0x0009A4A1
		public override void Reset()
		{
			if (!this._pushOnNextFixedUpdate)
			{
				this._hitRigidbody = null;
				this._hitBlock = null;
				base.EnterState(GestureState.Possible);
			}
			this._oldPull = Vector3.zero;
			PullObjectGesture.hasWorldTapPos = false;
		}

		// Token: 0x0600162E RID: 5678 RVA: 0x0009C0D4 File Offset: 0x0009A4D4
		public override string ToString()
		{
			return "PullObject(" + this._hitRigidbody + ")";
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x0009C0EC File Offset: 0x0009A4EC
		public void FixedUpdate()
		{
			if (!PullObjectGesture._lockPulledBlocks.Contains(this._hitBlock) && base.gestureState == GestureState.Active && this._hitRigidbody != null)
			{
				this.ContinuePull(this._lastTouch);
			}
			PullObjectGesture._lockPulledBlocks.Clear();
		}

		// Token: 0x06001630 RID: 5680 RVA: 0x0009C144 File Offset: 0x0009A544
		private void StartPull()
		{
			Blocksworld.blocksworldCamera.SetCameraStill(true);
			this._planePoint = this._hitPoint;
			this._planeNormal = -Blocksworld.cameraForward;
			this._lineRenderer.enabled = true;
			this.ContinuePull(this._lastTouch);
			if (this._hitBlock != null)
			{
				this._hitBlock.StartPull();
			}
		}

		// Token: 0x06001631 RID: 5681 RVA: 0x0009C1A8 File Offset: 0x0009A5A8
		private void ContinuePull(Vector2 touch)
		{
			Vector3 handlePos = this._hitRigidbody.transform.position + this._hitRigidbody.transform.rotation * this._hitHandle;
			Vector3 vector = Util.ProjectScreenPointOnWorldPlane(this._planePoint, this._planeNormal, touch);
			Vector3 pull = 50f * (vector - handlePos);
			float magnitude = this._oldPull.magnitude;
			float magnitude2 = pull.magnitude;
			float num = magnitude2 - magnitude;
			if (num > 30f)
			{
				Sound.PlaySound("Object Pull", Sound.GetOrCreateOneShotAudioSource(), true, (num - 30f) / 200f, 1f, false);
			}
			this._oldPull = pull;
			float num2 = magnitude2;
			float num3 = 150f;
			if (num2 > num3)
			{
				pull = pull.normalized * num3 * this._hitRigidbody.mass;
			}
			this._hitRigidbody.AddForceAtPosition(pull, handlePos);
			this._lineRenderer.SetPosition(0, handlePos);
			this._lineRenderer.SetPosition(1, vector);
			Blocksworld.blocksworldCamera.SetCameraStill(true);
			if (this._hitBlock != null)
			{
				if (this._hitBlock.broken)
				{
					this._hitBlock.chunk.rb.AddForceAtPosition(pull, handlePos);
				}
				else
				{
					List<Block> list = this._hitBlock.ConnectionsOfType(2, true);
					list.ForEach(delegate(Block b)
					{
						b.chunk.rb.AddForceAtPosition(pull, handlePos);
					});
				}
			}
		}

		// Token: 0x06001632 RID: 5682 RVA: 0x0009C35D File Offset: 0x0009A75D
		private void StopPull()
		{
			this._lineRenderer.enabled = false;
			Blocksworld.blocksworldCamera.SetCameraStill(false);
			if (this._hitBlock != null)
			{
				this._hitBlock.StopPull();
			}
		}

		// Token: 0x06001633 RID: 5683 RVA: 0x0009C38C File Offset: 0x0009A78C
		public void HideLine()
		{
			this._lineRenderer.enabled = false;
		}

		// Token: 0x04001136 RID: 4406
		private readonly LineRenderer _lineRenderer;

		// Token: 0x04001137 RID: 4407
		private static HashSet<Block> _lockPulledBlocks = new HashSet<Block>();

		// Token: 0x04001138 RID: 4408
		private Rigidbody _hitRigidbody;

		// Token: 0x04001139 RID: 4409
		private Block _hitBlock;

		// Token: 0x0400113A RID: 4410
		private Vector3 _hitPoint;

		// Token: 0x0400113B RID: 4411
		private Vector3 _hitHandle;

		// Token: 0x0400113C RID: 4412
		private Vector3 _planePoint;

		// Token: 0x0400113D RID: 4413
		private Vector3 _planeNormal;

		// Token: 0x0400113E RID: 4414
		private Vector2 _lastTouch;

		// Token: 0x0400113F RID: 4415
		private Vector3 _oldPull = Vector3.zero;

		// Token: 0x04001140 RID: 4416
		private bool _pushOnNextFixedUpdate;

		// Token: 0x04001141 RID: 4417
		private static Vector3 worldTapPos = default(Vector3);

		// Token: 0x04001142 RID: 4418
		private static Vector3 possibleWorldTapPos = default(Vector3);

		// Token: 0x04001143 RID: 4419
		private static bool hasWorldTapPos = false;

		// Token: 0x04001144 RID: 4420
		private static float worldTapTime = 0f;

		// Token: 0x04001145 RID: 4421
		private const int LAYER_MASK = 234011;
	}
}
