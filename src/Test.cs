using System;
using System.Collections.Generic;
using Blocks;

public class Test
{
	public string name = string.Empty;

	public string worldTitle = string.Empty;

	public TestType type;

	public float durationBeforeFail = 5f;

	public float timeScale = 10f;

	public TestResult result = new TestResult();

	public TestStatus status;

	public Dictionary<string, Action<Test, string, Block>> actions = new Dictionary<string, Action<Test, string, Block>>();

	public Dictionary<string, Func<Test, string, Block, bool>> sensors = new Dictionary<string, Func<Test, string, Block, bool>>();

	public HashSet<string> objectivesDone = new HashSet<string>();

	public HashSet<string> objectivesTested = new HashSet<string>();

	public bool IsTestObjectiveDone(string name)
	{
		objectivesTested.Add(name);
		return objectivesDone.Contains(name);
	}

	public void SetTestObjectiveDone(string name)
	{
		objectivesDone.Add(name);
	}

	public void Log(string text, bool showInConsole = true)
	{
		result.testLog.Add(text);
		if (showInConsole)
		{
			BWLog.Info(text);
		}
	}

	public void LogObjectives()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string item in objectivesTested)
		{
			if (objectivesDone.Contains(item))
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		if (list.Count > 0)
		{
			Log("Objectives tested and done: " + string.Join(", ", list.ToArray()));
		}
		if (list2.Count > 0)
		{
			Log("Objectives tested and NOT done: " + string.Join(", ", list2.ToArray()));
		}
	}

	public void ExecuteTestAction(string actionName, string data, Block block)
	{
		string text = actionName.ToLower();
		if (status != TestStatus.RUNNING)
		{
			return;
		}
		switch (text)
		{
		case "pass":
			Log("Test passed");
			LogObjectives();
			status = TestStatus.PASSED;
			break;
		case "fail":
			Log("Test failed");
			LogObjectives();
			status = TestStatus.FAILED;
			break;
		case "duration":
			if (float.TryParse(data, out durationBeforeFail))
			{
				Log("Setting test duration to " + durationBeforeFail + " seconds");
			}
			else
			{
				Log("Could not set duration '" + data + "'");
			}
			break;
		default:
			if (actions.ContainsKey(actionName))
			{
				actions[actionName](this, data, block);
			}
			else
			{
				Log("Could not find action with name " + actionName + " in test " + name);
			}
			break;
		}
	}

	public bool CheckTestSensor(string sensorName, string data, Block block)
	{
		if (sensors.ContainsKey(sensorName))
		{
			return sensors[sensorName](this, data, block);
		}
		Log("Could not find sensor with name " + sensorName + " in test " + name);
		return false;
	}
}
