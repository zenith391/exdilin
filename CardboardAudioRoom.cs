using System;
using UnityEngine;

// Token: 0x0200000E RID: 14
[AddComponentMenu("Cardboard/Audio/CardboardAudioRoom")]
public class CardboardAudioRoom : MonoBehaviour
{
	// Token: 0x06000059 RID: 89 RVA: 0x00004437 File Offset: 0x00002837
	private void Awake()
	{
		this.surfaceMaterials = new CardboardAudioRoom.SurfaceMaterial[6];
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00004445 File Offset: 0x00002845
	private void OnEnable()
	{
		this.InitializeRoom();
	}

	// Token: 0x0600005B RID: 91 RVA: 0x0000444D File Offset: 0x0000284D
	private void Start()
	{
		this.InitializeRoom();
	}

	// Token: 0x0600005C RID: 92 RVA: 0x00004455 File Offset: 0x00002855
	private void OnDisable()
	{
		this.ShutdownRoom();
	}

	// Token: 0x0600005D RID: 93 RVA: 0x0000445D File Offset: 0x0000285D
	private void Update()
	{
		CardboardAudio.UpdateAudioRoom(this.id, base.transform, this.GetSurfaceMaterials(), this.reflectivity, this.reverbGainDb, this.reverbBrightness, this.reverbTime, this.size);
	}

	// Token: 0x0600005E RID: 94 RVA: 0x00004494 File Offset: 0x00002894
	public CardboardAudioRoom.SurfaceMaterial[] GetSurfaceMaterials()
	{
		this.surfaceMaterials[0] = this.leftWall;
		this.surfaceMaterials[1] = this.rightWall;
		this.surfaceMaterials[2] = this.floor;
		this.surfaceMaterials[3] = this.ceiling;
		this.surfaceMaterials[4] = this.backWall;
		this.surfaceMaterials[5] = this.frontWall;
		return this.surfaceMaterials;
	}

	// Token: 0x0600005F RID: 95 RVA: 0x000044FC File Offset: 0x000028FC
	private void InitializeRoom()
	{
		if (this.id < 0)
		{
			this.id = CardboardAudio.CreateAudioRoom();
			CardboardAudio.UpdateAudioRoom(this.id, base.transform, this.GetSurfaceMaterials(), this.reflectivity, this.reverbGainDb, this.reverbBrightness, this.reverbTime, this.size);
		}
	}

	// Token: 0x06000060 RID: 96 RVA: 0x00004555 File Offset: 0x00002955
	private void ShutdownRoom()
	{
		if (this.id >= 0)
		{
			CardboardAudio.DestroyAudioRoom(this.id);
			this.id = -1;
		}
	}

	// Token: 0x06000061 RID: 97 RVA: 0x00004575 File Offset: 0x00002975
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, this.size);
	}

	// Token: 0x040000B2 RID: 178
	public CardboardAudioRoom.SurfaceMaterial leftWall = CardboardAudioRoom.SurfaceMaterial.ConcreteBlockCoarse;

	// Token: 0x040000B3 RID: 179
	public CardboardAudioRoom.SurfaceMaterial rightWall = CardboardAudioRoom.SurfaceMaterial.ConcreteBlockCoarse;

	// Token: 0x040000B4 RID: 180
	public CardboardAudioRoom.SurfaceMaterial floor = CardboardAudioRoom.SurfaceMaterial.ParquetOnConcrete;

	// Token: 0x040000B5 RID: 181
	public CardboardAudioRoom.SurfaceMaterial ceiling = CardboardAudioRoom.SurfaceMaterial.PlasterRough;

	// Token: 0x040000B6 RID: 182
	public CardboardAudioRoom.SurfaceMaterial backWall = CardboardAudioRoom.SurfaceMaterial.ConcreteBlockCoarse;

	// Token: 0x040000B7 RID: 183
	public CardboardAudioRoom.SurfaceMaterial frontWall = CardboardAudioRoom.SurfaceMaterial.ConcreteBlockCoarse;

	// Token: 0x040000B8 RID: 184
	public float reflectivity = 1f;

	// Token: 0x040000B9 RID: 185
	public float reverbGainDb;

	// Token: 0x040000BA RID: 186
	public float reverbBrightness;

	// Token: 0x040000BB RID: 187
	public float reverbTime = 1f;

	// Token: 0x040000BC RID: 188
	public Vector3 size = Vector3.one;

	// Token: 0x040000BD RID: 189
	private int id = -1;

	// Token: 0x040000BE RID: 190
	private CardboardAudioRoom.SurfaceMaterial[] surfaceMaterials;

	// Token: 0x0200000F RID: 15
	public enum SurfaceMaterial
	{
		// Token: 0x040000C0 RID: 192
		Transparent,
		// Token: 0x040000C1 RID: 193
		AcousticCeilingTiles,
		// Token: 0x040000C2 RID: 194
		BrickBare,
		// Token: 0x040000C3 RID: 195
		BrickPainted,
		// Token: 0x040000C4 RID: 196
		ConcreteBlockCoarse,
		// Token: 0x040000C5 RID: 197
		ConcreteBlockPainted,
		// Token: 0x040000C6 RID: 198
		CurtainHeavy,
		// Token: 0x040000C7 RID: 199
		FiberglassInsulation,
		// Token: 0x040000C8 RID: 200
		GlassThin,
		// Token: 0x040000C9 RID: 201
		GlassThick,
		// Token: 0x040000CA RID: 202
		Grass,
		// Token: 0x040000CB RID: 203
		LinoleumOnConcrete,
		// Token: 0x040000CC RID: 204
		Marble,
		// Token: 0x040000CD RID: 205
		ParquetOnConcrete,
		// Token: 0x040000CE RID: 206
		PlasterRough,
		// Token: 0x040000CF RID: 207
		PlasterSmooth,
		// Token: 0x040000D0 RID: 208
		PlywoodPanel,
		// Token: 0x040000D1 RID: 209
		PolishedConcreteOrTile,
		// Token: 0x040000D2 RID: 210
		Sheetrock,
		// Token: 0x040000D3 RID: 211
		WaterOrIceSurface,
		// Token: 0x040000D4 RID: 212
		WoodCeiling,
		// Token: 0x040000D5 RID: 213
		WoodPanel
	}
}
