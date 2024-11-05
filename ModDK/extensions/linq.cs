namespace ModDK {
    namespace Extensions {
        public static class LinqExtensions {
            public static IEnumerable<List<T>> SplitBy<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
                List<T> group = new List<T>();
                foreach (var item in source) {
                    if (predicate(item)) {
                        yield return new List<T>(group);
                        group.Clear();
                    } else {
                        group.Add(item);
                    }
                }

                if (group.Count > 0) {
                    yield return new List<T>(group);
                }
            }
        }
    }
}