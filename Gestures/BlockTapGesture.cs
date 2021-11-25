using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000178 RID: 376
	public class BlockTapGesture : BaseGesture
	{
		// Token: 0x060015B1 RID: 5553 RVA: 0x00096EB5 File Offset: 0x000952B5
		public BlockTapGesture(BlockTapBeginDelegate tapBegan, BlockTapGestureDelegate tapEnded)
		{
			this.TapBegan += tapBegan;
			this.TapEnded += tapEnded;
		}

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060015B2 RID: 5554 RVA: 0x00096ECC File Offset: 0x000952CC
		// (remove) Token: 0x060015B3 RID: 5555 RVA: 0x00096F04 File Offset: 0x00095304
		public event BlockTapBeginDelegate TapBegan;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x060015B4 RID: 5556 RVA: 0x00096F3C File Offset: 0x0009533C
		// (remove) Token: 0x060015B5 RID: 5557 RVA: 0x00096F74 File Offset: 0x00095374
		public event BlockTapGestureDelegate TapEnded;

		// Token: 0x060015B6 RID: 5558 RVA: 0x00096FAC File Offset: 0x000953AC
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count != 1 || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Vector2 position = allTouches[0].Position;
			if (Blocksworld.UI.SidePanel.Hit(position))
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (CharacterEditor.Instance.InEditMode())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(position * NormalizedScreen.scale);
			RaycastHit[] array = Physics.RaycastAll(ray);
			if (array.Length <= 0)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			float num = float.MaxValue;
			Block block = null;
			Array.Sort<RaycastHit>(array, new RaycastDistanceComparer(ray.origin));
			foreach (RaycastHit raycastHit in array)
			{
				Block block2 = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
				float num2 = raycastHit.distance;
				if (block2 != null && block2 == Blocksworld.mouseBlock)
				{
					num2 -= 0.1f;
				}
				BlockWater blockWater = block2 as BlockWater;
				if (blockWater != null && !blockWater.isSolid)
				{
					num2 += 50f;
				}
				if (num2 <= num)
				{
					if (block2 != null && (Tutorial.state == TutorialState.None || !block2.IsLocked() || block2.isTerrain) && (!(block2 is BlockVolume) || Options.BlockVolumeDisplay))
					{
						block = block2;
						num = num2;
					}
				}
			}
			if (block != null)
			{
				if (this.TapBegan != null && !this.TapBegan(this, block))
				{
					Blocksworld.mouseBlock = block;
					base.EnterState(GestureState.Failed);
					return;
				}
				this._startMouseBlock = block;
			}
			if (this._startMouseBlock == null)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			this._startPos = allTouches[0].Position;
			if (this._startMouseBlock.isTerrain)
			{
				base.EnterState(GestureState.Tracking);
			}
			else
			{
				base.EnterState(GestureState.Active);
			}
			this._startCameraForward = Blocksworld.cameraTransform.forward;
		}

		// Token: 0x060015B7 RID: 5559 RVA: 0x000971E4 File Offset: 0x000955E4
		public override void TouchesMoved(List<Touch> allTouches)
		{
			if (base.gestureState == GestureState.Tracking)
			{
				Vector2 position = allTouches[0].Position;
				if ((position - this._startPos).sqrMagnitude > 400f)
				{
					base.EnterState(GestureState.Cancelled);
					return;
				}
			}
			if (base.gestureState == GestureState.Active)
			{
				Vector3 forward = Blocksworld.cameraTransform.forward;
				if (Vector3.Dot(this._startCameraForward, forward) < 0.99f)
				{
					base.EnterState(GestureState.Cancelled);
					return;
				}
			}
		}

		// Token: 0x060015B8 RID: 5560 RVA: 0x00097265 File Offset: 0x00095665
		public override void TouchesEnded(List<Touch> allTouches)
		{
			base.EnterState(GestureState.Ended);
			if (this.TapEnded != null)
			{
				Blocksworld.mouseBlock = this._startMouseBlock;
				this.TapEnded(this, this._startMouseBlock);
			}
		}

		// Token: 0x060015B9 RID: 5561 RVA: 0x00097296 File Offset: 0x00095696
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
			this._startMouseBlock = null;
		}

		// Token: 0x060015BA RID: 5562 RVA: 0x000972A6 File Offset: 0x000956A6
		public override string ToString()
		{
			return string.Format("BlockTap({0})", this._startMouseBlock);
		}

		// Token: 0x060015BB RID: 5563 RVA: 0x000972B8 File Offset: 0x000956B8
		public Block GetStartMouseBlock()
		{
			return this._startMouseBlock;
		}

		// Token: 0x040010E0 RID: 4320
		private Block _startMouseBlock;

		// Token: 0x040010E1 RID: 4321
		private Vector2 _startPos;

		// Token: 0x040010E2 RID: 4322
		private Vector3 _startCameraForward;
	}
}
