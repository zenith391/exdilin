using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000088 RID: 136
	public class BlockEmitter : Block
	{
		// Token: 0x06000B86 RID: 2950 RVA: 0x00053494 File Offset: 0x00051894
		public BlockEmitter(List<List<Tile>> tiles) : base(tiles)
		{
			this._emitterSystemInfo = new BlockEmitterSystemInfo("System");
			this._smokeEmitterSystemInfo = new BlockEmitterSystemInfo("Smoke System");
			this._fastEmitterSystemInfo = new BlockEmitterSystemInfo("Fast System");
			this._infos = new List<BlockEmitterSystemInfo>
			{
				this._emitterSystemInfo,
				this._smokeEmitterSystemInfo,
				this._fastEmitterSystemInfo
			};
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x00053514 File Offset: 0x00051914
		public new static void Register()
		{
			PredicateRegistry.Add<BlockEmitter>("Emitter.Emit", null, (Block b) => new PredicateActionDelegate(((BlockEmitter)b).Emit), null, null, null);
			PredicateRegistry.Add<BlockEmitter>("Emitter.EmitFast", null, (Block b) => new PredicateActionDelegate(((BlockEmitter)b).EmitFast), null, null, null);
			PredicateRegistry.Add<BlockEmitter>("Emitter.EmitSmoke", null, (Block b) => new PredicateActionDelegate(((BlockEmitter)b).EmitSmoke), null, null, null);
			string name = "Emitter.HitByParticle";
			if (BlockEmitter.f__mg_cache0 == null)
			{
				BlockEmitter.f__mg_cache0 = new PredicateSensorConstructorDelegate(BlockEmitter.IsHitByParticle);
			}
			PredicateRegistry.Add<Block>(name, BlockEmitter.f__mg_cache0, null, null, null, null);
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x000535D4 File Offset: 0x000519D4
		public static PredicateSensorDelegate IsHitByParticle(Block block)
		{
			return delegate(ScriptRowExecutionInfo eInfo, object[] args)
			{
				Bounds bounds = block.go.GetComponent<Collider>().bounds;
				bounds.Expand(1f);
				foreach (BlockEmitterSystemInfo blockEmitterSystemInfo in BlockEmitter._allInfos)
				{
					if (blockEmitterSystemInfo.AnyWithinBounds(bounds))
					{
						return TileResultCode.True;
					}
				}
				return TileResultCode.False;
			};
		}

		// Token: 0x06000B89 RID: 2953 RVA: 0x000535FC File Offset: 0x000519FC
		public override void Destroy()
		{
			foreach (BlockEmitterSystemInfo blockEmitterSystemInfo in this._infos)
			{
				blockEmitterSystemInfo.Destroy();
			}
			base.Destroy();
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x00053660 File Offset: 0x00051A60
		public override void Play()
		{
			base.Play();
			BlockEmitter._allInfos.Add(this._emitterSystemInfo);
			BlockEmitter._allInfos.Add(this._smokeEmitterSystemInfo);
			BlockEmitter._allInfos.Add(this._fastEmitterSystemInfo);
			string texture = base.GetTexture(0);
			Material material = this.renderer.material;
			foreach (BlockEmitterSystemInfo blockEmitterSystemInfo in this._infos)
			{
				blockEmitterSystemInfo.Activate();
				blockEmitterSystemInfo.UpdateMaterial(material, texture);
			}
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x00053710 File Offset: 0x00051B10
		public override void Stop(bool resetBlock = true)
		{
			foreach (BlockEmitterSystemInfo blockEmitterSystemInfo in this._infos)
			{
				blockEmitterSystemInfo.Deactivate();
			}
			BlockEmitter._allInfos.Clear();
			base.Stop(resetBlock);
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x0005377C File Offset: 0x00051B7C
		public TileResultCode Emit(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float particlesPerS = (args.Length <= 0) ? 4f : ((float)args[0]);
			this._emitterSystemInfo.Emit(particlesPerS);
			return TileResultCode.True;
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x000537B4 File Offset: 0x00051BB4
		public TileResultCode EmitSmoke(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float particlesPerS = (args.Length <= 0) ? 4f : ((float)args[0]);
			this._smokeEmitterSystemInfo.Emit(particlesPerS);
			return TileResultCode.True;
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x000537EC File Offset: 0x00051BEC
		public TileResultCode EmitFast(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float particlesPerS = (args.Length <= 0) ? 4f : ((float)args[0]);
			this._fastEmitterSystemInfo.Emit(particlesPerS);
			return TileResultCode.True;
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x00053824 File Offset: 0x00051C24
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			foreach (BlockEmitterSystemInfo blockEmitterSystemInfo in this._infos)
			{
				blockEmitterSystemInfo.UpdateTransform(this.go);
				blockEmitterSystemInfo.FixedUpdate();
			}
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x00053894 File Offset: 0x00051C94
		public override void ResetFrame()
		{
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x00053896 File Offset: 0x00051C96
		public override void Pause()
		{
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x00053898 File Offset: 0x00051C98
		public override void Resume()
		{
		}

		// Token: 0x0400092A RID: 2346
		private List<BlockEmitterSystemInfo> _infos = new List<BlockEmitterSystemInfo>();

		// Token: 0x0400092B RID: 2347
		private static List<BlockEmitterSystemInfo> _allInfos = new List<BlockEmitterSystemInfo>();

		// Token: 0x0400092C RID: 2348
		private Material _material;

		// Token: 0x0400092D RID: 2349
		private BlockEmitterSystemInfo _emitterSystemInfo;

		// Token: 0x0400092E RID: 2350
		private BlockEmitterSystemInfo _smokeEmitterSystemInfo;

		// Token: 0x0400092F RID: 2351
		private BlockEmitterSystemInfo _fastEmitterSystemInfo;

		// Token: 0x04000930 RID: 2352
		[CompilerGenerated]
		private static PredicateSensorConstructorDelegate f__mg_cache0;
	}
}
