using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Mediator<T> : MonoBehaviour where T : Component, IVisitable {
    protected readonly List<T> entities = new();

    public void Register(T entity) {
        if (!entities.Contains(entity)) {
            entities.Add(entity);
            OnRegistered(entity);
        }
    }

    protected abstract void OnRegistered(T entity);

    public void Deregister(T entity) {
        if (entities.Contains(entity)) {
            entities.Remove(entity);
            OnDeregistered(entity);
        }
    }

    protected abstract void OnDeregistered(T entity);

    protected internal virtual void Message(T source, T target, IVisitor message) {
        entities.FirstOrDefault(e => e.Equals(target))?.Accept(message);
    }

    protected internal virtual void Broadcast(T source, IVisitor message, Func<T, bool> predicate = null) {
        entities.Where(e => e != source
                            && SenderConditionMet(e, predicate)
                            && MediatorConditionMet(e))
            .ForEach(e => e.Accept(message));
    }

    protected abstract bool MediatorConditionMet(T component);

    protected bool SenderConditionMet(T e, Func<T, bool> predicate) => predicate == null || predicate(e);
}