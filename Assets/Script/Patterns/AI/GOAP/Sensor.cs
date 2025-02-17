using System;
using UnityEngine;

    public abstract class Sensor : MonoBehaviour {
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private float _timerInterval = 1f;

        private SphereCollider _detectionRange;
        
        public event Action onTargetChanged = delegate { };
        
        public Vector3 TargetPosition => _target ? _target.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        private GameObject _target;
        private Vector3 _lastKnownPos = Vector3.zero;

        CountdownTimer _timer;
        
        protected virtual void Awake() {
            _detectionRange = GetComponent<SphereCollider>();
            _detectionRange.isTrigger = true;
            _detectionRange.radius = _detectionRadius;
        }

        protected virtual void Start() {
             _timer = new CountdownTimer(_timerInterval);
             _timer.OnTimerStop += () => {
                 UpdateTargetPosition(_target ? _target : null);
                 _timer.Start();
             };
             _timer.Start();
        }

        protected virtual void Update() {
            _timer.Tick(Time.deltaTime);
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (!IsTarget(other)) return;
            UpdateTargetPosition(other.gameObject);
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (!IsTarget(other)) return;
            UpdateTargetPosition();
        }

        protected abstract bool IsTarget(Collider other);

        void UpdateTargetPosition(GameObject target = null) {
            _target = target;
            if (IsTargetInRange && (_lastKnownPos != TargetPosition || _lastKnownPos != Vector3.zero)) {
                _lastKnownPos = TargetPosition;
                onTargetChanged?.Invoke();
            }
        }

        protected virtual void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}