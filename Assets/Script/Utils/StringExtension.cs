public static class StringExtension {
    public static int GetFNV1aHash(this string str) {
        uint hash = 2166136261;
        foreach (var c in str) { hash = (hash ^ c) * 16777619; }

        return unchecked((int)hash);
    }
}