using System;
using System.Collections.Generic;
using Gestures;

// Token: 0x0200013D RID: 317
public class GestureCommand
{
	// Token: 0x0600144D RID: 5197 RVA: 0x0008E510 File Offset: 0x0008C910
	public virtual void Execute(HashSet<Touch> touches)
	{
		this.done = true;
	}

	// Token: 0x04001004 RID: 4100
	public bool done;
}
