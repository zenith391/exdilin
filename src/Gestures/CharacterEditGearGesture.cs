using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200017B RID: 379
	public class CharacterEditGearGesture : BaseGesture
	{
		// Token: 0x060015C8 RID: 5576 RVA: 0x00097B1C File Offset: 0x00095F1C
		public CharacterEditGearGesture(BuildPanel buildPanel)
		{
			this._buildPanel = buildPanel;
			this.touchBeginWindow = 12f;
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x00097B38 File Offset: 0x00095F38
		public override void TouchesBegan(List<Touch> allTouches)
		{
			if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (base.gestureState == GestureState.Active)
			{
				return;
			}
			if (!TBox.dragBlockTween.IsFinished())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			if (!CharacterEditor.Instance.InEditMode())
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			this._targetCharacter = CharacterEditor.Instance.CharacterBlock();
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = (allTouches[0].Phase == TouchPhase.Moved || allTouches[0].Phase == TouchPhase.Stationary) && (float)allTouches[0].moveFrameCount < this.touchBeginWindow;
			if (!flag && !flag2)
			{
				base.EnterState(GestureState.Failed);
				return;
			}
			Blocksworld.blocksworldCamera.SetCameraStill(true);
			Vector2 position = allTouches[0].Position;
			this._dragBlock = null;
			if (this._buildPanel.Hit(position) && !Blocksworld.scriptPanel.Hit(position))
			{
				Tile tile = this._buildPanel.HitTile(position, false);
				if (tile != null && tile.IsCreate())
				{
					CharacterEditGearGesture._buildPanelTileGAF = tile.gaf;
					this._dragTile = tile.Clone();
					this._dragOffset = Vector2.zero;
					this._dragBlock = Blocksworld.bw.AddNewBlock(this._dragTile, false, null, true);
					this._snappedTo = CharacterEditor.SnapTarget.None;
					this._overBuildPanel = true;
					this._gearType = CharacterEditor.GetBlocksterGearType(this._dragBlock);
					if (this._gearType == BlocksterGearType.None)
					{
						BWLog.Info(this._dragBlock + " has no gear type, section " + tile.panelSection);
						base.EnterState(GestureState.Failed);
						CharacterEditor.Instance.Exit();
						return;
					}
					this._enableRotationDrag = false;
					this._nonSnappedRotation = this._dragBlock.GetRotation();
				}
			}
			else
			{
				Block block;
				CharacterEditor.SnapTarget snappedTo;
				bool gearUnderScreenPosition = CharacterEditor.Instance.GetGearUnderScreenPosition(position, out block, out snappedTo);
				if (gearUnderScreenPosition)
				{
					int num = block.BlockItemId();
					BlockItem blockItem = (num <= 0) ? null : BlockItem.FindByID(num);
					if (blockItem != null)
					{
						this._dragBlock = block;
						CharacterEditGearGesture._buildPanelTileGAF = new GAF(blockItem);
						this._dragTile = new Tile(CharacterEditGearGesture._buildPanelTileGAF);
						this._snappedTo = snappedTo;
						this._overBuildPanel = false;
						this._gearType = CharacterEditor.GetBlocksterGearType(this._dragBlock);
						if (this._gearType == BlocksterGearType.None)
						{
							base.EnterState(GestureState.Failed);
							return;
						}
						this._enableRotationDrag = true;
						Vector2 a = Blocksworld.mainCamera.WorldToScreenPoint(this._dragBlock.GetPosition());
						this._dragOffset = a - position * NormalizedScreen.scale;
					}
				}
			}
			if (this._dragBlock != null)
			{
				BWLog.Info(string.Concat(new object[]
				{
					"Dragging gear ",
					this._dragBlock,
					" gearType ",
					this._gearType
				}));
				this._snapTargets = new List<CharacterEditor.SnapTarget>();
				this._rotationTargets = new List<CharacterEditor.SnapTarget>();
				switch (this._gearType)
				{
				case BlocksterGearType.Head:
				case BlocksterGearType.HeadTop:
					this._snapTargets.Add(CharacterEditor.SnapTarget.Head);
					this._rotationTargets.Add(CharacterEditor.SnapTarget.HeadFront);
					this._rotationTargets.Add(CharacterEditor.SnapTarget.HeadLeft);
					this._rotationTargets.Add(CharacterEditor.SnapTarget.HeadBack);
					this._rotationTargets.Add(CharacterEditor.SnapTarget.HeadRight);
					break;
				case BlocksterGearType.HeadSide:
					this._snapTargets.Add(CharacterEditor.SnapTarget.HeadLeft);
					this._snapTargets.Add(CharacterEditor.SnapTarget.HeadRight);
					this._snapTargets.Add(CharacterEditor.SnapTarget.HeadBack);
					this._snapTargets.Add(CharacterEditor.SnapTarget.HeadFront);
					break;
				case BlocksterGearType.Body:
				case BlocksterGearType.Back:
					this._snapTargets.Add(CharacterEditor.SnapTarget.Body);
					break;
				case BlocksterGearType.RightHand:
					this._snapTargets.Add(CharacterEditor.SnapTarget.RightHand);
					break;
				case BlocksterGearType.LeftHand:
					this._snapTargets.Add(CharacterEditor.SnapTarget.LeftHand);
					break;
				case BlocksterGearType.EitherHand:
					this._snapTargets.Add(CharacterEditor.SnapTarget.RightHand);
					this._snapTargets.Add(CharacterEditor.SnapTarget.LeftHand);
					break;
				}
				this.Drag(position);
				CharacterEditGearGesture._displacedBlockGAFs = new HashSet<GAF>();
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), true, 0.2f, 1f, false);
				base.EnterState(GestureState.Active);
				return;
			}
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x00097F98 File Offset: 0x00096398
		public override void TouchesMoved(List<Touch> allTouches)
		{
			Vector2 position = allTouches[0].Position;
			this.Drag(position);
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x00097FB9 File Offset: 0x000963B9
		public override void TouchesEnded(List<Touch> allTouches)
		{
			if (base.gestureState == GestureState.Active && allTouches[0].Phase == TouchPhase.Ended)
			{
				History.AddStateIfNecessary();
				this.Reset();
			}
		}

		// Token: 0x060015CC RID: 5580 RVA: 0x00097FE8 File Offset: 0x000963E8
		public override void Reset()
		{
			CharacterEditGearGesture._buildPanelTileGAF = null;
			if (CharacterEditGearGesture._displacedBlockGAFs != null)
			{
				CharacterEditGearGesture._displacedBlockGAFs.Clear();
			}
			CharacterEditor.Instance.RemoveAllDisplacedBlocks();
			if (!this._partIsSnapped && this._dragBlock != null)
			{
				CharacterEditor.Instance.RemoveAttachment(this._dragBlock);
				BWSceneManager.RemoveBlock(this._dragBlock);
				this._dragBlock.Destroy();
				Scarcity.UpdateInventory(true, null);
			}
			if (this._dragTile != null)
			{
				this._dragTile.Destroy();
			}
			this._partIsSnapped = false;
			this._snappedTo = CharacterEditor.SnapTarget.None;
			this._rotateSnappedTo = CharacterEditor.SnapTarget.None;
			this._dragTile = null;
			this._dragBlock = null;
			this._gearType = BlocksterGearType.None;
			this._targetCharacter = null;
			base.EnterState(GestureState.Possible);
		}

		// Token: 0x060015CD RID: 5581 RVA: 0x000980AC File Offset: 0x000964AC
		private void Drag(Vector2 pos)
		{
			Vector3 pos2 = pos - NormalizedScreen.pixelScale * 0.5f * 80f * Vector2.one;
			pos2.z = -0.5f;
			this._dragTile.MoveTo(pos2, false);
			Vector3 vector = Blocksworld.mainCamera.transform.InverseTransformPoint(this._targetCharacter.goT.position);
			Vector2 v = pos * NormalizedScreen.scale + this._dragOffset;
			Plane plane = new Plane(Vector3.up, this._targetCharacter.goT.position - Vector3.up);
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(v);
			float num = vector.z;
			foreach (CharacterEditor.SnapTarget snapTarget in this._snapTargets)
			{
				num = Mathf.Min(Blocksworld.mainCamera.transform.InverseTransformPoint(CharacterEditor.Instance.GetSnapTargetPosition(snapTarget, this._gearType)).z, num);
			}
			float a;
			if (plane.Raycast(ray, out a))
			{
				num = Mathf.Min(a, num);
			}
			num -= 0.05f;
			Vector3 position = new Vector3(v.x, v.y, num);
			Vector3 pos3 = Blocksworld.mainCamera.ScreenToWorldPoint(position);
			this._dragBlock.MoveTo(pos3);
			Vector3 vector2 = Vector3.zero;
			Quaternion quaternion = this._dragBlock.GetRotation();
			CharacterEditor.SnapTarget snappedTo = this._snappedTo;
			this._snappedTo = CharacterEditor.Instance.GetSnapTargetUnderScreenPosition(pos, this._gearType, this._snapTargets);
			if (this._snappedTo != CharacterEditor.SnapTarget.None)
			{
				vector2 = CharacterEditor.Instance.GetSnapTargetPosition(this._snappedTo, this._gearType);
				Vector3 b = CharacterEditor.Instance.BlockSnapOffset(this._dragBlock, this._snappedTo);
				vector2 += b;
				if (this._snappedTo != snappedTo)
				{
					quaternion = CharacterEditor.Instance.GetSnapTargetRotation(this._snappedTo) * CharacterEditor.Instance.GetGearCharacterEditorOrientation(this._dragBlock);
				}
				else if (this._enableRotationDrag && this._rotationTargets.Count > 0)
				{
					CharacterEditor.SnapTarget rotateSnappedTo = this._rotateSnappedTo;
					this._rotateSnappedTo = CharacterEditor.Instance.GetSnapTargetUnderScreenPosition(pos, this._gearType, this._rotationTargets);
					Quaternion rhs = Quaternion.LookRotation(Vector3.right);
					Quaternion rhs2 = Quaternion.LookRotation(Vector3.left);
					if ((rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront))
					{
						quaternion *= rhs2;
					}
					else if ((rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadRight && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadBack && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft) || (rotateSnappedTo == CharacterEditor.SnapTarget.HeadLeft && this._rotateSnappedTo == CharacterEditor.SnapTarget.HeadFront))
					{
						quaternion *= rhs;
					}
				}
			}
			if (this._snappedTo == CharacterEditor.SnapTarget.None && this._enableRotationDrag)
			{
				this._enableRotationDrag = false;
			}
			this._partIsSnapped = (this._snappedTo != CharacterEditor.SnapTarget.None);
			if (this._partIsSnapped)
			{
				this._dragBlock.MoveTo(vector2);
				this._dragBlock.RotateTo(quaternion);
			}
			else
			{
				this._dragBlock.RotateTo(this._nonSnappedRotation);
			}
			if (snappedTo != this._snappedTo)
			{
				bool flag = false;
				if (snappedTo != CharacterEditor.SnapTarget.None)
				{
					CharacterEditor.Instance.RestoreAllDisplacedBlocks();
					CharacterEditor.Instance.RemoveAttachment(this._dragBlock);
					flag = true;
				}
				if (this._partIsSnapped)
				{
					CharacterEditor.Instance.AddAttachment(this._dragBlock);
					List<Block> displacedBlocks = CharacterEditor.Instance.GetDisplacedBlocks();
					foreach (Block block in displacedBlocks)
					{
						CharacterEditGearGesture._displacedBlockGAFs.Add(block.BlockCreateGAF());
					}
					flag = true;
				}
				if (flag)
				{
					Scarcity.inventoryScales[this._dragTile.gaf] = 1.5f;
					Scarcity.UpdateInventory(true, null);
				}
			}
			bool overBuildPanel = this._overBuildPanel;
			this._overBuildPanel = this._buildPanel.Hit(pos);
			if (this._overBuildPanel)
			{
				this._dragBlock.Deactivate();
				this._dragTile.Show(true);
				this._dragTile.MoveTo(pos2, false);
			}
			else
			{
				this._dragBlock.Activate();
				this._dragTile.Show(false);
			}
		}

		// Token: 0x060015CE RID: 5582 RVA: 0x000985D0 File Offset: 0x000969D0
		public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
		{
			if (CharacterEditGearGesture._buildPanelTileGAF != null)
			{
				if (result == null)
				{
					result = new HashSet<GAF>();
				}
				result.Add(CharacterEditGearGesture._buildPanelTileGAF);
				if (CharacterEditGearGesture._displacedBlockGAFs != null)
				{
					result.UnionWith(CharacterEditGearGesture._displacedBlockGAFs);
				}
			}
			return result;
		}

		// Token: 0x040010F0 RID: 4336
		private BuildPanel _buildPanel;

		// Token: 0x040010F1 RID: 4337
		private BlockAnimatedCharacter _targetCharacter;

		// Token: 0x040010F2 RID: 4338
		private Tile _dragTile;

		// Token: 0x040010F3 RID: 4339
		private Block _dragBlock;

		// Token: 0x040010F4 RID: 4340
		private BlocksterGearType _gearType;

		// Token: 0x040010F5 RID: 4341
		private List<CharacterEditor.SnapTarget> _snapTargets;

		// Token: 0x040010F6 RID: 4342
		private List<CharacterEditor.SnapTarget> _rotationTargets;

		// Token: 0x040010F7 RID: 4343
		private CharacterEditor.SnapTarget _snappedTo;

		// Token: 0x040010F8 RID: 4344
		private CharacterEditor.SnapTarget _rotateSnappedTo;

		// Token: 0x040010F9 RID: 4345
		private bool _partIsSnapped;

		// Token: 0x040010FA RID: 4346
		private bool _overBuildPanel;

		// Token: 0x040010FB RID: 4347
		private bool _enableRotationDrag;

		// Token: 0x040010FC RID: 4348
		private Vector2 _dragOffset;

		// Token: 0x040010FD RID: 4349
		private Quaternion _nonSnappedRotation;

		// Token: 0x040010FE RID: 4350
		private static GAF _buildPanelTileGAF;

		// Token: 0x040010FF RID: 4351
		private static HashSet<GAF> _displacedBlockGAFs;
	}
}
