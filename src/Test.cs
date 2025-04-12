using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020002C4 RID: 708
public class Test
{
	// Token: 0x0600205E RID: 8286 RVA: 0x000EDCA2 File Offset: 0x000EC0A2
	public bool IsTestObjectiveDone(string name)
	{
		this.objectivesTested.Add(name);
		return this.objectivesDone.Contains(name);
	}

	// Token: 0x0600205F RID: 8287 RVA: 0x000EDCBD File Offset: 0x000EC0BD
	public void SetTestObjectiveDone(string name)
	{
		this.objectivesDone.Add(name);
	}

	// Token: 0x06002060 RID: 8288 RVA: 0x000EDCCC File Offset: 0x000EC0CC
	public void Log(string text, bool showInConsole = true)
	{
		this.result.testLog.Add(text);
		if (showInConsole)
		{
			BWLog.Info(text);
		}
	}

	// Token: 0x06002061 RID: 8289 RVA: 0x000EDCEC File Offset: 0x000EC0EC
	public void LogObjectives()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string item in this.objectivesTested)
		{
			if (this.objectivesDone.Contains(item))
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
			this.Log("Objectives tested and done: " + string.Join(", ", list.ToArray()), true);
		}
		if (list2.Count > 0)
		{
			this.Log("Objectives tested and NOT done: " + string.Join(", ", list2.ToArray()), true);
		}
	}

	// Token: 0x06002062 RID: 8290 RVA: 0x000EDDCC File Offset: 0x000EC1CC
	public void ExecuteTestAction(string actionName, string data, Block block)
	{
		string text = actionName.ToLower();
		if (this.status == TestStatus.RUNNING)
		{
			if (text != null)
			{
				if (text == "pass")
				{
					this.Log("Test passed", true);
					this.LogObjectives();
					this.status = TestStatus.PASSED;
					return;
				}
				if (text == "fail")
				{
					this.Log("Test failed", true);
					this.LogObjectives();
					this.status = TestStatus.FAILED;
					return;
				}
				if (text == "duration")
				{
					if (float.TryParse(data, out this.durationBeforeFail))
					{
						this.Log("Setting test duration to " + this.durationBeforeFail + " seconds", true);
					}
					else
					{
						this.Log("Could not set duration '" + data + "'", true);
					}
					return;
				}
			}
			if (this.actions.ContainsKey(actionName))
			{
				this.actions[actionName](this, data, block);
			}
			else
			{
				this.Log("Could not find action with name " + actionName + " in test " + this.name, true);
			}
		}
	}

	// Token: 0x06002063 RID: 8291 RVA: 0x000EDF04 File Offset: 0x000EC304
	public bool CheckTestSensor(string sensorName, string data, Block block)
	{
		if (this.sensors.ContainsKey(sensorName))
		{
			return this.sensors[sensorName](this, data, block);
		}
		this.Log("Could not find sensor with name " + sensorName + " in test " + this.name, true);
		return false;
	}

	// Token: 0x04001BA1 RID: 7073
	public string name = string.Empty;

	// Token: 0x04001BA2 RID: 7074
	public string worldTitle = string.Empty;

	// Token: 0x04001BA3 RID: 7075
	public TestType type;

	// Token: 0x04001BA4 RID: 7076
	public float durationBeforeFail = 5f;

	// Token: 0x04001BA5 RID: 7077
	public float timeScale = 10f;

	// Token: 0x04001BA6 RID: 7078
	public TestResult result = new TestResult();

	// Token: 0x04001BA7 RID: 7079
	public TestStatus status;

	// Token: 0x04001BA8 RID: 7080
	public Dictionary<string, Action<Test, string, Block>> actions = new Dictionary<string, Action<Test, string, Block>>();

	// Token: 0x04001BA9 RID: 7081
	public Dictionary<string, Func<Test, string, Block, bool>> sensors = new Dictionary<string, Func<Test, string, Block, bool>>();

	// Token: 0x04001BAA RID: 7082
	public HashSet<string> objectivesDone = new HashSet<string>();

	// Token: 0x04001BAB RID: 7083
	public HashSet<string> objectivesTested = new HashSet<string>();
}
