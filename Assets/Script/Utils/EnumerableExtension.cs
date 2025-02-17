using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtension {
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (var t in source) { action(t); }
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
        Random rng = new Random();
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (rng == null) throw new ArgumentNullException(nameof(rng));

        return source.ShuffleIterator(rng);
    }

    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng) {
        var buffer = source.ToList();
        for (int i = 0; i < buffer.Count; i++) {
            int j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}