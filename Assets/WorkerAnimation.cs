using UnityEngine;
using UnityEngine.AI;

public class WorkerAnimation : MonoBehaviour
{
    private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
    private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
    [SerializeField] Transform _target;
    private NavMeshAgent _agent;
    private Animator _anim;
    [SerializeField] private bool _updatePosition;
    [SerializeField] Vector2 _velocity;
    [SerializeField] RuntimeAnimatorController _controller;
    
    private void Awake() {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.SetDestination(_target.position);
        if (_controller) _anim.runtimeAnimatorController = _controller;
        var sr = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if (_updatePosition && _agent.destination != _target.position) {
            _agent.SetDestination(_target.position);
        }
        _anim.SetFloat(HorizontalMovement, _agent.velocity.x);
        _anim.SetFloat(VerticalMovement, _agent.velocity.y);
        _velocity = new Vector2(_agent.velocity.x, _agent.velocity.y);
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
