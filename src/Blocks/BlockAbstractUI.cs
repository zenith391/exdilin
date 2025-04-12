using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200006F RID: 111
	public class BlockAbstractUI : Block
	{
		// Token: 0x060008EA RID: 2282 RVA: 0x0002E17C File Offset: 0x0002C57C
		public BlockAbstractUI(List<List<Tile>> tiles, int index) : base(tiles)
		{
			this.index = index;
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x0002E1F8 File Offset: 0x0002C5F8
		public override void Play()
		{
			base.Play();
			this.dirty = true;
			this._uiEnabled = true;
			this._uiVisible = true;
			this.physicalVisible = true;
			this.uiPaused = false;
			this.text = string.Empty;
			this.ShowPhysical(true);
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x0002E235 File Offset: 0x0002C635
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.ShowPhysical(true);
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060008ED RID: 2285 RVA: 0x0002E245 File Offset: 0x0002C645
		protected bool uiVisible
		{
			get
			{
				return this._uiVisible && this._uiEnabled;
			}
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x0002E25B File Offset: 0x0002C65B
		public void EnableUI(bool enable)
		{
			this._uiEnabled = enable;
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x0002E264 File Offset: 0x0002C664
		public TileResultCode ShowUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool uivisible = args.Length <= 0 || (int)args[0] != 0;
			this.SetUIVisible(uivisible);
			return TileResultCode.True;
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x0002E297 File Offset: 0x0002C697
		protected virtual void SetUIVisible(bool v)
		{
			this.dirty = (this.dirty || v != this._uiVisible);
			this._uiVisible = v;
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x0002E2C0 File Offset: 0x0002C6C0
		private void ShowPhysical(bool v)
		{
			if (this.chunk != null && this.chunk.go != null)
			{
				Rigidbody rb = this.chunk.rb;
				if (rb != null && this.connections.Count == 0)
				{
					rb.isKinematic = !v;
					if (v)
					{
						rb.WakeUp();
					}
				}
			}
			this.go.GetComponent<Collider>().isTrigger = !v;
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0002E340 File Offset: 0x0002C740
		public TileResultCode ShowPhysical(ScriptRowExecutionInfo eInfo, object[] args)
		{
			bool flag = this.physicalVisible;
			this.physicalVisible = (args.Length <= 0 || (int)args[0] != 0);
			if (flag != this.physicalVisible)
			{
				this.dirty = true;
				this.ShowPhysical(this.physicalVisible);
			}
			return TileResultCode.True;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x0002E398 File Offset: 0x0002C798
		protected string ParseText(string s)
		{
			foreach (string text in Blocksworld.customIntVariables.Keys)
			{
				s = s.Replace("<" + text + ">", Blocksworld.customIntVariables[text].ToString());
			}
			s = s.Replace("/name/", WorldSession.current.config.currentUserUsername);
			s = s.Replace("/coins/", WorldSession.current.config.currentUserCoins.ToString());
			return s;
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x0002E440 File Offset: 0x0002C840
		public virtual TileResultCode SetText(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string a = this.text;
			this.text = ((args.Length <= 0) ? string.Empty : this.ParseText((string)args[0]));
			this.dirty = (this.dirty || a != this.text);
			return TileResultCode.True;
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x0002E49C File Offset: 0x0002C89C
		public TileResultCode PauseUI(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 1f : ((float)args[0]);
			if (eInfo.timer >= num)
			{
				this.PauseUI(false);
				return TileResultCode.True;
			}
			this.PauseUI(true);
			return TileResultCode.Delayed;
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0002E4E2 File Offset: 0x0002C8E2
		public virtual void PauseUI(bool p)
		{
			this.uiPaused = p;
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x0002E4EC File Offset: 0x0002C8EC
		public virtual TileResultCode Flash(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = 0f;
			float num2 = -1f;
			bool uivisible = true;
			int num3 = Util.GetIntArg(args, 0, 0) % this.flashDurations.Count;
			foreach (float num4 in this.flashDurations[num3])
			{
				float num5 = Mathf.Abs(num4);
				num += num5;
				if (eInfo.timer >= num2 & eInfo.timer < num)
				{
					uivisible = (num4 > 0f);
					break;
				}
				num2 = num;
			}
			this.SetUIVisible(uivisible);
			return (eInfo.timer < num) ? TileResultCode.Delayed : TileResultCode.True;
		}

		// Token: 0x040006FA RID: 1786
		public int index;

		// Token: 0x040006FB RID: 1787
		protected bool dirty = true;

		// Token: 0x040006FC RID: 1788
		private bool _uiEnabled;

		// Token: 0x040006FD RID: 1789
		private bool _uiVisible = true;

		// Token: 0x040006FE RID: 1790
		protected bool physicalVisible = true;

		// Token: 0x040006FF RID: 1791
		protected bool uiPaused;

		// Token: 0x04000700 RID: 1792
		protected string text = string.Empty;

		// Token: 0x04000701 RID: 1793
		protected List<float[]> flashDurations = new List<float[]>
		{
			new float[]
			{
				1f,
				-0.1f
			},
			new float[]
			{
				0.1f,
				-0.1f,
				0.1f,
				-0.1f,
				0.1f,
				-0.1f,
				0.1f,
				-0.1f,
				0.1f,
				-0.1f,
				0.5f
			}
		};
	}
}
