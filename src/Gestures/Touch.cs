using System;
using UnityEngine;

namespace Gestures
{
	// Token: 0x0200018E RID: 398
	public class Touch
	{
		// Token: 0x06001683 RID: 5763 RVA: 0x000A0A4C File Offset: 0x0009EE4C
		public Touch(Vector2 initialPosition)
		{
			this.LastPosition = initialPosition;
			this.Position = initialPosition;
			this.Phase = TouchPhase.Began;
			this.moveFrameCount = 0;
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x000A0A80 File Offset: 0x0009EE80
		public void Moved(Vector2 newPosition)
		{
			this.LastPosition = this.Position;
			this.Position = newPosition;
			this.Phase = ((!(this.Position == this.LastPosition)) ? TouchPhase.Moved : TouchPhase.Stationary);
			this.moveFrameCount++;
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x000A0AD1 File Offset: 0x0009EED1
		public void End()
		{
			this.LastPosition = this.Position;
			this.Phase = TouchPhase.Ended;
			this.moveFrameCount = 0;
		}

		// Token: 0x04001196 RID: 4502
		public int fingerId;

		// Token: 0x04001197 RID: 4503
		public TouchPhase Phase;

		// Token: 0x04001198 RID: 4504
		public Vector2 Position;

		// Token: 0x04001199 RID: 4505
		public Vector2 LastPosition;

		// Token: 0x0400119A RID: 4506
		public GestureCommand Command;

		// Token: 0x0400119B RID: 4507
		public int moveFrameCount;
	}
}
