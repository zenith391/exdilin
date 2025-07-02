using UnityEngine;

public class BWLog
{
	private const int LOG_LEVEL_INFO = 4;

	private const int LOG_LEVEL_WARNING = 3;

	private const int LOG_LEVEL_ERROR = 2;

	public static int loggingLevel = 4;

	public static void Info(string s)
	{
		if (loggingLevel >= 4)
		{
			Debug.Log(s);
		}
	}

	public static void Warning(string s)
	{
		if (loggingLevel >= 3)
		{
			string text = StackTraceUtility.ExtractStackTrace();
			Debug.LogWarning(s + "\nCallstack:\n" + text);
		}
	}

	public static void Error(string s)
	{
		if (loggingLevel >= 2)
		{
			string text = StackTraceUtility.ExtractStackTrace();
			Debug.LogError(s + "\nCallstack:\n" + text);
		}
	}
}
