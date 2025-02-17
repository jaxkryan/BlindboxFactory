using UnityEngine;

public abstract class Payload<TData> : IVisitor {
    public abstract TData Content { get; set; }
    public abstract void Visit<T>(T visitable) where T : Component, IVisitable;
}