using System;

public readonly struct BlackboardKey : IEquatable<BlackboardKey> {
    private readonly string _key;
    readonly int hashedKey;

    public BlackboardKey(string key) {
        _key = key;
        hashedKey = key.GetFNV1aHash();
    }

    public bool Equals(BlackboardKey other) => hashedKey == other.hashedKey;

    public override bool Equals(object obj) => obj is BlackboardKey other && Equals(other);

    public override int GetHashCode() => hashedKey;

    public override string ToString() => _key;

    public static bool operator ==(BlackboardKey left, BlackboardKey right) => left.hashedKey == right.hashedKey;

    public static bool operator !=(BlackboardKey left, BlackboardKey right) => !(left == right);
}