using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x02000175 RID: 373
	public class BlockDuplicateGesture : BaseGesture
	{
		// Token: 0x0600159E RID: 5534 RVA: 0x00096A27 File Offset: 0x00094E27
		public BlockDuplicateGesture(BlockDuplicateBeginDelegate dupeBegan, BlockDuplicateGestureDelegate dupeEnded)
		{
			this.DuplicateBegan += dupeBegan;
			this.DuplicateEnded += dupeEnded;
		}

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600159F RID: 5535 RVA: 0x00096A40 File Offset: 0x00094E40
		// (remove) Token: 0x060015A0 RID: 5536 RVA: 0x00096A78 File Offset: 0x00094E78
		public event BlockDuplicateBeginDelegate DuplicateBegan;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060015A1 RID: 5537 RVA: 0x00096AB0 File Offset: 0x00094EB0
		// (remove) Token: 0x060015A2 RID: 5538 RVA: 0x00096AE8 File Offset: 0x00094EE8
		public event BlockDuplicateGestureDelegate DuplicateEnded;

		// Token: 0x060015A3 RID: 5539 RVA: 0x00096B20 File Offset: 0x00094F20
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (allTouches.Count != 1 || Tutorial.InTutorialOrPuzzle() || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Vector3 vector = allTouches[0].Position;
			if (TBox.HitMove(vector) || TBox.HitRotate(vector) || TBox.HitScale(vector))
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (CharacterEditor.Instance.InEditMode())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(allTouches[0].Position * NormalizedScreen.scale);
			RaycastHit[] array = Physics.RaycastAll(ray);
			if (array.Length <= 0)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Array.Sort<RaycastHit>(array, new RaycastDistanceComparer(ray.origin));
			RaycastHit[] array2 = array;
			int i = 0;
			while (i < array2.Length)
			{
				RaycastHit raycastHit = array2[i];
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
				if (block != null && !(block is BlockWater) && (Tutorial.state == TutorialState.None || !block.IsLocked() || block.isTerrain) && (!(block is BlockVolume) || Options.BlockVolumeDisplay))
				{
					if (this.DuplicateBegan != null && !this.DuplicateBegan(this, block))
					{
						base.EnterState(GestureState.Failed);
						return;
					}
					this._startMouseBlock = block;
					break;
				}
				else
				{
					i++;
				}
			}
			if (this._startMouseBlock == null)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			this._startPos = vector;
			if (this._startMouseBlock.isTerrain && (!this._startMouseBlock.SelectableTerrain() || this._startMouseBlock.DisableBuildModeMove()))
			{
				base.EnterState(GestureState.Failed);
			}
			else
			{
				base.EnterState(GestureState.Active);
				this._startTime = Time.time;
			}
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x00096D28 File Offset: 0x00095128
		public override void TouchesStationary(List<Touch> allTouches)
		{
			if (allTouches.Count != 1)
			{
				base.EnterState(GestureState.Cancelled);
			}
			else if (Time.time - this._startTime > 0.7f)
			{
				base.EnterState(GestureState.Ended);
				Dictionary<GAF, int> d = Scarcity.CalculateWorldGafUsage(false, false);
				if (this.DuplicateEnded != null)
				{
					this.DuplicateEnded(this, this._startMouseBlock, allTouches[0]);
				}
				if (Blocksworld.selectedBunch != null || Blocksworld.selectedBlock != null)
				{
					Blocksworld.tBoxGesture.ActivateDrag(allTouches[0]);
					Dictionary<GAF, int> d2 = Scarcity.CalculateWorldGafUsage(false, false);
					Dictionary<GAF, int> dictionary = Scarcity.CompareGafUsages(d, d2, "d1", "d2");
					if (dictionary.Count > 0)
					{
						List<GAF> list = new List<GAF>(dictionary.Keys);
						foreach (GAF key in list)
						{
							Scarcity.inventoryScales[key] = 1.5f;
						}
						TBoxGesture.SetGafHighlights(list);
					}
				}
			}
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x00096E4C File Offset: 0x0009524C
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Vector2 position = allTouches[0].Position;
			if ((position - this._startPos).sqrMagnitude > 400f)
			{
				base.EnterState(GestureState.Cancelled);
			}
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x00096E8B File Offset: 0x0009528B
		public override void Reset()
		{
			base.EnterState(GestureState.Possible);
			this._startMouseBlock = null;
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x00096E9B File Offset: 0x0009529B
		public override string ToString()
		{
			return string.Format("BlockDuplicate({0})", this._startMouseBlock);
		}

		// Token: 0x060015A8 RID: 5544 RVA: 0x00096EAD File Offset: 0x000952AD
		public Block GetStartMouseBlock()
		{
			return this._startMouseBlock;
		}

		// Token: 0x040010DA RID: 4314
		private Block _startMouseBlock;

		// Token: 0x040010DB RID: 4315
		private Vector2 _startPos;

		// Token: 0x040010DC RID: 4316
		private float _startTime;

		// Token: 0x040010DD RID: 4317
		private const float DUPLICATE_HOLD_TIME = 0.7f;
	}
}
