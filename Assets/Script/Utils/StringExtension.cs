using System;
using System.Collections.Generic;
using System.Linq;

public static class StringExtension {
    public static int GetFNV1aHash(this string str) {
        uint hash = 2166136261;
        foreach (var c in str) { hash = (hash ^ c) * 16777619; }

        return unchecked((int)hash);
    }

    public static string ToNormalString(this string str,
        StringCapitalizationSetting capitalization = StringCapitalizationSetting.KeepCapitalization) {
        List<char> chars = new();
        if (!char.IsLetter(str[0])) str = str[1..];
        foreach (var c in str) {
            if (AddSpaceBefore(c)) chars.Add(' ');
            chars.Add(c);
            if (AddSpaceAfter(c)) chars.Add(' ');
        }
        return Capitalize(chars);

        string Capitalize(List<char> chars) {
            bool capitalize = false;
            switch (capitalization) {
                case StringCapitalizationSetting.KeepCapitalization:
                    break;
                case StringCapitalizationSetting.NoCapitalization:
                    for (int i = 0; i < chars.Count; i++) {
                        if (char.IsUpper(chars[i]) && char.IsLetter(chars[i])) chars[i] = char.ToLower(chars[i]);
                    }

                    break;
                case StringCapitalizationSetting.CapitalizeSentences:
                    capitalize = true;
                    for (int i = 0; i < chars.Count; i++) {
                        if (!char.IsLetter(chars[i])) continue;
                        if (capitalize) {
                            capitalize = false;
                            chars[i] = char.ToUpper(chars[i]);
                        } 
                        else chars[i] = char.ToLower(chars[i]);
                        
                        if (chars[i] == '.') capitalize = true;
                    }

                    break;
                case StringCapitalizationSetting.CapitalizeFirstLetter:
                    capitalize = true;
                    for (int i = 0; i < chars.Count; i++) {
                        if (!char.IsLetter(chars[i])) continue;
                        if (capitalize) {
                            capitalize = false;
                            chars[i] = char.ToUpper(chars[i]);
                        } 
                        else chars[i] = char.ToLower(chars[i]);
                    }

                    break;
                case StringCapitalizationSetting.CapitalizeAllLetters:
                    for (int i = 0; i < chars.Count; i++) {
                        if (!char.IsLetter(chars[i])) continue;
                        chars[i] = char.ToUpper(chars[i]);
                    }

                    break;
                case StringCapitalizationSetting.CapitalizeEachWords:
                    capitalize = true;
                    for (int i = 0; i < chars.Count; i++) {
                        if (!char.IsLetter(chars[i])) {
                            capitalize = true;
                            continue;
                        }
                        if (capitalize) {
                            capitalize = false;
                            chars[i] = char.ToUpper(chars[i]);
                        } 
                        else chars[i] = char.ToLower(chars[i]);
                    }

                    break;
                default:
                    break;
            }
                    return string.Join("", chars);
        }
        bool AddSpaceBefore(char c) {
            if (char.IsUpper(c)) return true;
            return false;
        }

        bool AddSpaceAfter(char c) {
            if (c == '.') return true;
            return false;
        }
    }

    public enum StringCapitalizationSetting {
        KeepCapitalization,
        NoCapitalization,
        CapitalizeSentences,
        CapitalizeFirstLetter,
        CapitalizeAllLetters,
        CapitalizeEachWords
    }

    public static string DescriptionFormatter(this string str, params (Func<bool> func, object[] strs)[] args) {
        const char openCharacter = '<';
        const char closeCharacter = '>';

        var segments = GetSegments(str, openCharacter, closeCharacter);

        for (int i = 0; i < args.Length; i++) {
            var arg = args[i];
            if (segments.Count <= i) continue;
            var seg = segments.ElementAt(i);
            if (!arg.func()) { str = str.Replace(seg, string.Empty); }

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