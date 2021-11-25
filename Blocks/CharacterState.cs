using System;

namespace Blocks
{
	// Token: 0x0200029F RID: 671
	public enum CharacterState
	{
		// Token: 0x040018F9 RID: 6393
		None,
		// Token: 0x040018FA RID: 6394
		Treasure,
		// Token: 0x040018FB RID: 6395
		Unmoving,
		// Token: 0x040018FC RID: 6396
		Idle,
		// Token: 0x040018FD RID: 6397
		Walk,
		// Token: 0x040018FE RID: 6398
		Balance,
		// Token: 0x040018FF RID: 6399
		CrawlIdle,
		// Token: 0x04001900 RID: 6400
		Crawl,
		// Token: 0x04001901 RID: 6401
		CrawlEnter,
		// Token: 0x04001902 RID: 6402
		CrawlExit,
		// Token: 0x04001903 RID: 6403
		Jump,
		// Token: 0x04001904 RID: 6404
		Flail,
		// Token: 0x04001905 RID: 6405
		Fly,
		// Token: 0x04001906 RID: 6406
		SwimIn,
		// Token: 0x04001907 RID: 6407
		SwimIdle,
		// Token: 0x04001908 RID: 6408
		Swim,
		// Token: 0x04001909 RID: 6409
		SwimOut,
		// Token: 0x0400190A RID: 6410
		GoProne,
		// Token: 0x0400190B RID: 6411
		Prone,
		// Token: 0x0400190C RID: 6412
		GetUp,
		// Token: 0x0400190D RID: 6413
		Push,
		// Token: 0x0400190E RID: 6414
		Pull,
		// Token: 0x0400190F RID: 6415
		Climb,
		// Token: 0x04001910 RID: 6416
		Slide,
		// Token: 0x04001911 RID: 6417
		Attack,
		// Token: 0x04001912 RID: 6418
		Win,
		// Token: 0x04001913 RID: 6419
		Fail,
		// Token: 0x04001914 RID: 6420
		Dance,
		// Token: 0x04001915 RID: 6421
		SitDown,
		// Token: 0x04001916 RID: 6422
		StandUp,
		// Token: 0x04001917 RID: 6423
		Sitting,
		// Token: 0x04001918 RID: 6424
		SittingStill,
		// Token: 0x04001919 RID: 6425
		EditSitting,
		// Token: 0x0400191A RID: 6426
		EditSittingStill,
		// Token: 0x0400191B RID: 6427
		EditModePose,
		// Token: 0x0400191C RID: 6428
		Hover,
		// Token: 0x0400191D RID: 6429
		ImpactLeft,
		// Token: 0x0400191E RID: 6430
		ImpactRight,
		// Token: 0x0400191F RID: 6431
		SoftHitLeft,
		// Token: 0x04001920 RID: 6432
		SoftHitRight,
		// Token: 0x04001921 RID: 6433
		SoftHitFront,
		// Token: 0x04001922 RID: 6434
		SoftHitBack,
		// Token: 0x04001923 RID: 6435
		SoftHitTop,
		// Token: 0x04001924 RID: 6436
		UprightDefault,
		// Token: 0x04001925 RID: 6437
		ProneDefault,
		// Token: 0x04001926 RID: 6438
		Collapse,
		// Token: 0x04001927 RID: 6439
		Collapsed,
		// Token: 0x04001928 RID: 6440
		Recover,
		// Token: 0x04001929 RID: 6441
		PlayAnim,
		// Token: 0x0400192A RID: 6442
		SwordLungeRight,
		// Token: 0x0400192B RID: 6443
		SwordLungeLeft,
		// Token: 0x0400192C RID: 6444
		SwordJumpAttack,
		// Token: 0x0400192D RID: 6445
		KickFrontLeft,
		// Token: 0x0400192E RID: 6446
		KickFrontRight,
		// Token: 0x0400192F RID: 6447
		DodgeLeft,
		// Token: 0x04001930 RID: 6448
		DodgeRight
	}
}
