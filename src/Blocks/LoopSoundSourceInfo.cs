using UnityEngine;

namespace Blocks;

public class LoopSoundSourceInfo
{
	public Vector3 pos;

	public float pitch = 1f;

	public Block block;

	public bool playing;

	public LoopSoundSourceInfo(Vector3 pos, Block block)
	{
		this.pos = pos;
		this.block = block;
	}

	public void Update(Vector3 pos, float pitch, bool playing)
	{
		this.pos = pos;
		this.pitch = pitch;
		this.playing = playing;
	}
}
