using System.Collections.Generic;

namespace B.LargeNumbers;

public static class ListExtensions
{
    public static void Append<TItem>(this List<TItem> self, TItem item) => self.Add(item);
}
