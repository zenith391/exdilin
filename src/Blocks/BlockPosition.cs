using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000BE RID: 190
	public class BlockPosition : Block
	{
		// Token: 0x06000EA6 RID: 3750 RVA: 0x00062BA4 File Offset: 0x00060FA4
		public BlockPosition(List<List<Tile>> tiles, bool isVisible = false) : base(tiles)
		{
			this.visibleBlock = isVisible;
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x00062BB4 File Offset: 0x00060FB4
		public override void Play()
		{
			base.Play();
			this.HideAndMakeTrigger();
			this.IgnoreRaycasts(true);
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x00062BC9 File Offset: 0x00060FC9
		public bool IsHiddenAndTrigger()
		{
			return this.go.GetComponent<Collider>().isTrigger && !this.go.GetComponent<Renderer>().enabled;
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x00062BF8 File Offset: 0x00060FF8
		public void HideAndMakeTrigger()
		{
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().isTrigger = true;
				this.go.GetComponent<Collider>().enabled = true;
			}
			if (!this.visibleBlock)
			{
				this.go.GetComponent<Renderer>().enabled = false;
				this.hiddenInTutorial = true;
			}
			else
			{
				if (Tutorial.state == TutorialState.None)
				{
					this.go.GetComponent<Renderer>().enabled = false;
				}
				this.hideAfterTutorial = true;
			}
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x00062CAC File Offset: 0x000610AC
		public void ShowAndRemoveIsTrigger()
		{
			if (this.go.GetComponent<Collider>() != null)
			{
				this.go.GetComponent<Collider>().isTrigger = false;
			}
			if (this.go.GetComponent<Renderer>() != null)
			{
				this.go.GetComponent<Renderer>().enabled = true;
			}
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = true;
			}
			this.hiddenInTutorial = false;
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x00062D30 File Offset: 0x00061130
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.UpdateStopVisibility();
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x00062D40 File Offset: 0x00061140
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			if (Blocksworld.CurrentState != State.Play)
			{
				string text = base.BlockType();
				if (text.Contains("Position"))
				{
					texture = "Glass";
				}
				else
				{
					texture = "Volume";
				}
			}
			return base.TextureTo(texture, normal, permanent, meshIndex, true);
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x00062D90 File Offset: 0x00061190
		public void UpdateStopVisibility()
		{
			if (Tutorial.state == TutorialState.None)
			{
				this.ShowAndRemoveIsTrigger();
				this.IgnoreRaycasts(false);
			}
			else
			{
				this.HideAndMakeTrigger();
			}
			if (base.BlockType() == "Position Camera Hint")
			{
				if (Options.BlockCameraHintDisplay)
				{
					this.ShowAndRemoveIsTrigger();
					this.IgnoreRaycasts(false);
				}
				else
				{
					this.HideAndMakeTrigger();
					this.IgnoreRaycasts(true);
				}
			}
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x00062E00 File Offset: 0x00061200
		public override void OnCreate()
		{
			base.OnCreate();
			this.UpdateStopVisibility();
			string text = base.BlockType();
			if (text.Contains("Position"))
			{
				this.TextureTo("Glass", base.GetTextureNormal(), true, 0, true);
			}
			else
			{
				this.TextureTo("Volume", base.GetTextureNormal(), true, 0, true);
			}
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x00062E5F File Offset: 0x0006125F
		public override void Pause()
		{
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x00062E61 File Offset: 0x00061261
		public override void Resume()
		{
		}

		// Token: 0x06000EB1 RID: 3761 RVA: 0x00062E63 File Offset: 0x00061263
		public override void ResetFrame()
		{
		}

		// Token: 0x06000EB2 RID: 3762 RVA: 0x00062E68 File Offset: 0x00061268
		public override void Update()
		{
			base.Update();
			if (!this.hiddenInTutorial && (Tutorial.mode == TutorialMode.Puzzle || Tutorial.state != TutorialState.None))
			{
				this.HideAndMakeTrigger();
				if (!this.visibleBlock)
				{
					this.IgnoreRaycasts(true);
				}
			}
			if (Blocksworld.CurrentState == State.Play && this.hideAfterTutorial)
			{
				this.go.GetComponent<Renderer>().enabled = false;
				this.hideAfterTutorial = false;
			}
		}

		// Token: 0x06000EB3 RID: 3763 RVA: 0x00062EE1 File Offset: 0x000612E1
		public override bool VisibleInPlayMode()
		{
			return this.visibleBlock;
		}

		// Token: 0x06000EB4 RID: 3764 RVA: 0x00062EE9 File Offset: 0x000612E9
		public override bool ColliderIsTriggerInPlayMode()
		{
			return true;
		}

		// Token: 0x06000EB5 RID: 3765 RVA: 0x00062EEC File Offset: 0x000612EC
		protected override void UpdateBlockPropertiesForTextureAssignment(int meshIndex, bool forceEnabled)
		{
		}

		// Token: 0x06000EB6 RID: 3766 RVA: 0x00062EEE File Offset: 0x000612EE
		public override bool IsRuntimeInvisible()
		{
			return true;
		}

		// Token: 0x04000B5E RID: 2910
		private bool visibleBlock;

		// Token: 0x04000B5F RID: 2911
		private bool hiddenInTutorial;

		// Token: 0x04000B60 RID: 2912
		private bool hideAfterTutorial;
	}
}
