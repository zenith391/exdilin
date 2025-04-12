using System;

namespace SimpleJSON
{
	// Token: 0x02000286 RID: 646
	public class ParseError : Exception
	{
		// Token: 0x06001E52 RID: 7762 RVA: 0x000D865F File Offset: 0x000D6A5F
		public ParseError(string message, int position) : base(message)
		{
			this.Position = position;
		}

		// Token: 0x0400187F RID: 6271
		public readonly int Position;
	}
}
