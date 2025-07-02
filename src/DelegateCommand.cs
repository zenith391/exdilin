using System;

public class DelegateCommand<T> : Command
{
	private Action<T> action;

	private T data;

	public DelegateCommand(T data, Action<T> action)
	{
		this.action = action;
		this.data = data;
	}

	public override void Execute()
	{
		base.Execute();
		action(data);
	}
}
public class DelegateCommand<S, T> : Command
{
	private Action<S, T> action;

	private S data1;

	private T data2;

	public DelegateCommand(S data1, T data2, Action<S, T> action)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
	}

	public override void Execute()
	{
		base.Execute();
		action(data1, data2);
	}
}
public class DelegateCommand<S, T, U> : Command
{
	private Action<S, T, U> action;

	private S data1;

	private T data2;

	private U data3;

	public DelegateCommand(S data1, T data2, U data3, Action<S, T, U> action)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
		this.data3 = data3;
	}

	public override void Execute()
	{
		base.Execute();
		action(data1, data2, data3);
	}
}
public class DelegateCommand<S, T, U, V> : Command
{
	private Action<S, T, U, V> action;

	private S data1;

	private T data2;

	private U data3;

	private V data4;

	public DelegateCommand(S data1, T data2, U data3, V data4, Action<S, T, U, V> action)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
		this.data3 = data3;
		this.data4 = data4;
	}

	public override void Execute()
	{
		base.Execute();
		action(data1, data2, data3, data4);
	}
}
public class DelegateCommand : Command
{
	private Action<DelegateCommand> action;

	public DelegateCommand(Action<DelegateCommand> action)
	{
		this.action = action;
	}

	public override void Execute()
	{
		base.Execute();
		action(this);
	}
}
