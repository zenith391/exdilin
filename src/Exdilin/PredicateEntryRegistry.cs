using System.Collections.Generic;

namespace Exdilin;

public static class PredicateEntryRegistry
{
	private static Dictionary<string, PredicateEntry> predicateEntries = new Dictionary<string, PredicateEntry>();

	public static Dictionary<string, PredicateEntry> GetPredicateEntries()
	{
		return predicateEntries;
	}

	public static void AddPredicate(PredicateEntry entry)
	{
		predicateEntries.Add(entry.id, entry);
	}
}
