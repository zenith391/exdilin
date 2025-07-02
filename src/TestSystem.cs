using Blocks;

public class TestSystem
{
	public static Test currentTest;

	public static void ExecuteTestAction(string name, string data, Block block)
	{
		if (currentTest != null)
		{
			currentTest.ExecuteTestAction(name, data, block);
		}
		else
		{
			BWLog.Info("No current test found for action " + name);
		}
	}

	public static bool CheckTestSensor(string name, string data, Block block)
	{
		if (currentTest != null)
		{
			return currentTest.CheckTestSensor(name, data, block);
		}
		BWLog.Info("No current test found for sensor " + name);
		return false;
	}

	public static bool IsTestObjectiveDone(string name)
	{
		if (currentTest != null)
		{
			return currentTest.IsTestObjectiveDone(name);
		}
		BWLog.Info("No current test found for testing objective " + name);
		return false;
	}

	public static void SetTestObjectiveDone(string name)
	{
		if (currentTest != null)
		{
			currentTest.SetTestObjectiveDone(name);
		}
		else
		{
			BWLog.Info("No current test found for testing objective " + name);
		}
	}
}
