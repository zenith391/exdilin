using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Blocks;

public class BlockEmitter : Block
{
	private List<BlockEmitterSystemInfo> _infos = new List<BlockEmitterSystemInfo>();

	private static List<BlockEmitterSystemInfo> _allInfos = new List<BlockEmitterSystemInfo>();

	private Material _material;

	private BlockEmitterSystemInfo _emitterSystemInfo;

	private BlockEmitterSystemInfo _smokeEmitterSystemInfo;

	private BlockEmitterSystemInfo _fastEmitterSystemInfo;

	[CompilerGenerated]
	private static PredicateSensorConstructorDelegate f__mg_cache0;

	public BlockEmitter(List<List<Tile>> tiles)
		: base(tiles)
	{
		_emitterSystemInfo = new BlockEmitterSystemInfo("System");
		_smokeEmitterSystemInfo = new BlockEmitterSystemInfo("Smoke System");
		_fastEmitterSystemInfo = new BlockEmitterSystemInfo("Fast System");
		_infos = new List<BlockEmitterSystemInfo> { _emitterSystemInfo, _smokeEmitterSystemInfo, _fastEmitterSystemInfo };
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockEmitter>("Emitter.Emit", null, (Block b) => ((BlockEmitter)b).Emit);
		PredicateRegistry.Add<BlockEmitter>("Emitter.EmitFast", null, (Block b) => ((BlockEmitter)b).EmitFast);
		PredicateRegistry.Add<BlockEmitter>("Emitter.EmitSmoke", null, (Block b) => ((BlockEmitter)b).EmitSmoke);
		string name = "Emitter.HitByParticle";
		PredicateRegistry.Add<Block>(name, IsHitByParticle);
	}

	public static PredicateSensorDelegate IsHitByParticle(Block block)
	{
		return delegate
		{
			Bounds bounds = block.go.GetComponent<Collider>().bounds;
			bounds.Expand(1f);
			foreach (BlockEmitterSystemInfo allInfo in _allInfos)
			{
				if (allInfo.AnyWithinBounds(bounds))
				{
					return TileResultCode.True;
				}
			}
			return TileResultCode.False;
		};
	}

	public override void Destroy()
	{
		foreach (BlockEmitterSystemInfo info in _infos)
		{
			info.Destroy();
		}
		base.Destroy();
	}

	public override void Play()
	{
		base.Play();
		_allInfos.Add(_emitterSystemInfo);
		_allInfos.Add(_smokeEmitterSystemInfo);
		_allInfos.Add(_fastEmitterSystemInfo);
		string texture = GetTexture();
		Material material = renderer.material;
		foreach (BlockEmitterSystemInfo info in _infos)
		{
			info.Activate();
			info.UpdateMaterial(material, texture);
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		foreach (BlockEmitterSystemInfo info in _infos)
		{
			info.Deactivate();
		}
		_allInfos.Clear();
		base.Stop(resetBlock);
	}

	public TileResultCode Emit(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float particlesPerS = ((args.Length == 0) ? 4f : ((float)args[0]));
		_emitterSystemInfo.Emit(particlesPerS);
		return TileResultCode.True;
	}

	public TileResultCode EmitSmoke(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float particlesPerS = ((args.Length == 0) ? 4f : ((float)args[0]));
		_smokeEmitterSystemInfo.Emit(particlesPerS);
		return TileResultCode.True;
	}

	public TileResultCode EmitFast(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float particlesPerS = ((args.Length == 0) ? 4f : ((float)args[0]));
		_fastEmitterSystemInfo.Emit(particlesPerS);
		return TileResultCode.True;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		foreach (BlockEmitterSystemInfo info in _infos)
		{
			info.UpdateTransform(go);
			info.FixedUpdate();
		}
	}

	public override void ResetFrame()
	{
	}

	public override void Pause()
	{
	}

	public override void Resume()
	{
	}
}
