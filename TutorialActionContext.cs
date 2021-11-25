using System;

// Token: 0x020002DE RID: 734
public enum TutorialActionContext
{
	// Token: 0x04001CB7 RID: 7351
	TutorialStart,
	// Token: 0x04001CB8 RID: 7352
	BeforeThisBlockCreate,
	// Token: 0x04001CB9 RID: 7353
	AfterThisBlockCreate,
	// Token: 0x04001CBA RID: 7354
	DuringScriptThisRow,
	// Token: 0x04001CBB RID: 7355
	DuringScriptNextTile,
	// Token: 0x04001CBC RID: 7356
	BlockDone,
	// Token: 0x04001CBD RID: 7357
	EnterPlay,
	// Token: 0x04001CBE RID: 7358
	AutoAddBlockWait,
	// Token: 0x04001CBF RID: 7359
	RemoveBlock,
	// Token: 0x04001CC0 RID: 7360
	Paint,
	// Token: 0x04001CC1 RID: 7361
	Texture,
	// Token: 0x04001CC2 RID: 7362
	BeginScripting,
	// Token: 0x04001CC3 RID: 7363
	BeforeEnterPlay,
	// Token: 0x04001CC4 RID: 7364
	BeforePlayMoverUse
}
