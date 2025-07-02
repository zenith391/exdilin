using System.Collections.Generic;

public abstract class Command
{
	protected bool done;

	public virtual void Execute()
	{
		done = true;
	}

	public virtual void OnHudMesh()
	{
	}

	public virtual bool IsDone()
	{
		return done;
	}

	public void SetDone(bool d)
	{
		done = d;
	}

	public virtual void Reset()
	{
		done = false;
	}

	public virtual void Removed()
	{
	}

	public static void ExecuteCommands(List<Command> commands)
	{
		if (commands.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < commands.Count; i++)
		{
			Command command = commands[i];
			command.Execute();
		}
		for (int num = commands.Count - 1; num >= 0; num--)
		{
			Command command2 = commands[num];
			if (command2.IsDone())
			{
				command2.Removed();
				commands.RemoveAt(num);
			}
		}
	}

	public static void RemoveCommands(List<Command> commands)
	{
		foreach (Command command in commands)
		{
			command.Removed();
		}
		commands.Clear();
	}

	public static void AddUniqueCommand(List<Command> commands, Command command, bool resetWhenAdded = true)
	{
		if (!commands.Contains(command))
		{
			commands.Add(command);
			if (resetWhenAdded)
			{
				command.Reset();
			}
		}
	}
}
