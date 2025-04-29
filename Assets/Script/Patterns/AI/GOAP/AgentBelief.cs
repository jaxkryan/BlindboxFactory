using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentBelief {
    public string Name { get; }

    private Func<bool> _condition = () => false;
    private Func<Vector3> _observedLocation = () => Vector3.zero;

    public Vector3 Location => _observedLocation();

    public bool Evaluate() =>_condition();

    AgentBelief(string name) {
        Name = name;
    }

    public class Builder {
        readonly AgentBelief _belief;

        public Builder(string belief) => _belief = new AgentBelief(belief);

        public Builder WithLocation(Func<Vector3> observedLocation) {
            _belief._observedLocation = observedLocation;
            return this;
        }

        public Builder WithCondition(Func<bool> condition) {
            _belief._condition = condition;
            return this;
        }

        public AgentBelief Build() => _belief;
    }
}

public class BeliefFactory {
    private readonly GoapAgent _goapAgent;
    private readonly Dictionary<string, AgentBelief> _beliefs;

    public BeliefFactory(GoapAgent goapAgent, Dictionary<string, AgentBelief> beliefs) {
        _goapAgent = goapAgent;
        _beliefs = beliefs;
    }

    public void AddBelief(string key, Func<bool> condition) {
        _beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(condition)
            .Build());
    }

    public void AddSensorBelief(string key, Sensor sensor) {
        _beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(() => sensor.IsTargetInRange)
            .WithLocation(() => sensor.TargetPosition)
            .Build());
    }

    public void AddLocationBelief(string key, float distance, Transform transform) =>
        AddLocationBelief(key, distance, transform.position);


    public void AddLocationBelief(string key, float distance, Vector3 location) {
        _beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(() => InRangeOf(location, distance))
            .WithLocation(() => location)
            .Build());
    }

    bool InRangeOf(Vector3 position, float range) => Vector3.Distance(_goapAgent.transform.position, position) < range;
}