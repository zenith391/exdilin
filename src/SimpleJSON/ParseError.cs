using System;

namespace SimpleJSON;

public class ParseError : Exception
{
	public readonly int Position;

	public ParseError(string message, int position)
		: base(message)
	{
		Position = position;
	}
}
