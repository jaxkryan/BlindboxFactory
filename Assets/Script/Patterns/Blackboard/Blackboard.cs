using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[Serializable]
public class Blackboard {
    Dictionary<string, BlackboardKey> keyRegistry = new();
    Dictionary<BlackboardKey, object> entries = new();

    public bool TryGetValue<T>(BlackboardKey key, out T value) {
        if (entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> castedEntry) {
            value = castedEntry.Value;
            return true;
        }

        value = default;
        return false;
    }

    public BlackboardKey GetOrRegister(string keyName) {
        if (keyName == null || keyName == string.Empty) { throw new ArgumentNullException(nameof(keyName)); }

        if (!keyRegistry.TryGetValue(keyName, out var key)) {
            key = new BlackboardKey(keyName);
            keyRegistry[keyName] = key;
        }

        return key;
    }

    public void SetValue<T>(BlackboardKey key, T value) { entries[key] = new BlackboardEntry<T>(key, value); }

    public bool ContainsKey(BlackboardKey keyName) => entries.ContainsKey(keyName);

    public void Remove(BlackboardKey keyName) => entries.Remove(keyName);
}