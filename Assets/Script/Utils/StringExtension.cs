using System;
using System.Collections.Generic;
using System.Linq;

public static class StringExtension {
    public static int GetFNV1aHash(this string str) {
        uint hash = 2166136261;
        foreach (var c in str) {
            hash = (hash ^ c) * 16777619;
        }

        return unchecked((int)hash);
    }

    public static string DescriptionFormatter(this string str, params (Func<bool> func, object[] strs)[] args) {
        const char openCharacter = '<';
        const char closeCharacter = '>';

        var segments = GetSegments(str, openCharacter, closeCharacter);

        for (int i = 0; i < args.Length; i++) {
            var arg = args[i];
            if (segments.Count <= i) continue;
            var seg = segments.ElementAt(i);
            if (!arg.func()) {
                str = str.Replace(seg, string.Empty);
            }

            try {
                var newSeg = String.Format(seg, arg.strs);
                newSeg = newSeg.Substring(1, newSeg.Length - 2);
                //Console.WriteLine($"{i}: {seg} => {newSeg}");
                str = str.Replace(seg, newSeg);
            }
            catch { }
        }

        return str;

        List<string> GetSegments(string str, char segmentStart, char segmentEnd) {
            List<string> segments = new();
            var isSegment = false;
            var segment = "";
            foreach (var c in str) {
                if (c == segmentStart) {
                    if (isSegment) Reset();
                    else isSegment = true;
                }

                if (isSegment) segment += c;
                if (c == segmentEnd) {
                    if (isSegment) {
                        segments.Add(segment);
                        Reset();
                    }
                }
            }

            return segments;

            void Reset() {
                isSegment = false;
                segment = "";
            }
        }
    }
}