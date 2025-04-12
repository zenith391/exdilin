using System;
using Blocks;

// Token: 0x020002C0 RID: 704
public class TestSystem
{
	// Token: 0x06002058 RID: 8280 RVA: 0x000EDB5D File Offset: 0x000EBF5D
	public static void ExecuteTestAction(string name, string data, Block block)
	{
		if (TestSystem.currentTest != null)
		{
			TestSystem.currentTest.ExecuteTestAction(name, data, block);
		}
		else
		{
			BWLog.Info("No current test found for action " + name);
		}
	}

	// Token: 0x06002059 RID: 8281 RVA: 0x000EDB8B File Offset: 0x000EBF8B
	public static bool CheckTestSensor(string name, string data, Block block)
	{
		if (TestSystem.currentTest != null)
		{
			return TestSystem.currentTest.CheckTestSensor(name, data, block);
		}
		BWLog.Info("No current test found for sensor " + name);
		return false;
	}

	// Token: 0x0600205A RID: 8282 RVA: 0x000EDBB6 File Offset: 0x000EBFB6
	public static bool IsTestObjectiveDone(string name)
	{
		if (TestSystem.currentTest != null)
		{
			return TestSystem.currentTest.IsTestObjectiveDone(name);
		}
		BWLog.Info("No current test found for testing objective " + name);
		return false;
	}

	// Token: 0x0600205B RID: 8283 RVA: 0x000EDBDF File Offset: 0x000EBFDF
	public static void SetTestObjectiveDone(string name)
	{
		if (TestSystem.currentTest != null)
		{
			TestSystem.currentTest.SetTestObjectiveDone(name);
		}
		else
		{
			BWLog.Info("No current test found for testing objective " + name);
		}
	}

	// Token: 0x04001B96 RID: 7062
	public static Test currentTest;
}
