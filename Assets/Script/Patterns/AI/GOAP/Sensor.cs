using System;
using UnityEngine;

    public class Sensor : MonoBehaviour {
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private float _timerInterval = 1f;

        private CircleCollider2D _detectionRange;
        
        public event Action onTargetChanged = delegate { };
        
        public Vector3 TargetPosition => _currentTarget ? _currentTarget.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        public GameObject Target { get; set; }

        private GameObject _currentTarget;
        private Vector3 _lastKnownPos = Vector3.zero;

        CountdownTimer _timer;
        
        protected virtual void Awake() {
            _detectionRange = GetComponent<CircleCollider2D>();
            _detectionRange.isTrigger = true;
            _detectionRange.radius = _detectionRadius;
        }

        protected virtual void Start() {
             _timer = new CountdownTimer(_timerInterval);
             _timer.OnTimerStop += () => {
                 UpdateTargetPosition(_currentTarget ? _currentTarget : null);
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

        protected bool IsTarget(Collider other) => other.gameObject.Equals(Target);

        void UpdateTargetPosition(GameObject target = null) {
            _currentTarget = target;
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