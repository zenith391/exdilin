using System;

// Token: 0x020002E1 RID: 737
public class TutorialOperationPose
{
	// Token: 0x060021DD RID: 8669 RVA: 0x000FE1DD File Offset: 0x000FC5DD
	public override string ToString()
	{
		return string.Format("[TutorialOperationPose {0} {1} {2}]", this.state, this.meshIndex, this.poseName);
	}

	// Token: 0x04001CCF RID: 7375
	public TutorialState state;

	// Token: 0x04001CD0 RID: 7376
	public int meshIndex;

	// Token: 0x04001CD1 RID: 7377
	public string poseName;
}
