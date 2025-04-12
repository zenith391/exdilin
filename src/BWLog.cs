using System;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class BWLog
{
	// Token: 0x06000200 RID: 512 RVA: 0x0000B84C File Offset: 0x00009C4C
	public static void Info(string s)
	{
        if (BWLog.loggingLevel >= LOG_LEVEL_INFO)
		{
			Debug.Log(s);
		}
	}

	// Token: 0x06000201 RID: 513 RVA: 0x0000B860 File Offset: 0x00009C60
	public static void Warning(string s)
	{
		if (BWLog.loggingLevel >= LOG_LEVEL_WARNING)
		{
			string str = StackTraceUtility.ExtractStackTrace();
			Debug.LogWarning(s + "\nCallstack:\n" + str);
		}
	}

	// Token: 0x06000202 RID: 514 RVA: 0x0000B890 File Offset: 0x00009C90
	public static void Error(string s)
	{
		if (BWLog.loggingLevel >= LOG_LEVEL_ERROR)
		{
			string str = StackTraceUtility.ExtractStackTrace();
			Debug.LogError(s + "\nCallstack:\n" + str);
		}
	}

	// Token: 0x040001EB RID: 491
	private const int LOG_LEVEL_INFO = 4;

	// Token: 0x040001EC RID: 492
	private const int LOG_LEVEL_WARNING = 3;

	// Token: 0x040001ED RID: 493
	private const int LOG_LEVEL_ERROR = 2;

	// Token: 0x040001EE RID: 494
	public static int loggingLevel = LOG_LEVEL_INFO;
}
