using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.HumanResource.Worker {
    [Serializable]
    public class CoreDrain {
        [SerializeField][Min(0f)] public float DrainOverTime;
        [SerializeField][Min(1)] public int TimeInterval = 1;
        [SerializeField][Min(0f)] public float DrainOnWork;
    }
}