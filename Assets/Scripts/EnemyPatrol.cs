using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointStopDistance = 0.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;

    private NavMeshAgent _agent;
    private Transform _player;
    private int _waypointIndex;
    private float _attackTimer;

    private enum State { Patrol, Chase, Attack }
    private State _currentState;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    private void Start()
    {
        EnterPatrol();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    // ── PATROL ──────────────────────────────────────────────

    private void EnterPatrol()
    {
        _currentState = State.Patrol;
        _agent.isStopped = false;
        GoToNextWaypoint();
    }

    private void UpdatePatrol()
    {
        if (!_agent.pathPending && _agent.remainingDistance < waypointStopDistance)
        {
            GoToNextWaypoint();
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hits.Length > 0)
        {
            _currentState = State.Chase;
            _agent.isStopped = false;
        }
    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_waypointIndex].position);
        _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
    }

    // ── CHASE ───────────────────────────────────────────────

    private void UpdateChase()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist > detectionRadius * 1.5f)
        {
            EnterPatrol();
            return;
        }

        if (dist <= attackRange)
        {
            _currentState = State.Attack;
            _agent.isStopped = true;
            return;
        }

        _agent.SetDestination(_player.position);
    }

    // ── ATTACK ──────────────────────────────────────────────

    private void UpdateAttack()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist > attackRange)
        {
            _currentState = State.Chase;
            _agent.isStopped = false;
            return;
        }

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            _player.GetComponent<Health>()?.TakeDamage(attackDamage);
        }
    }

    // ── GIZMOS ──────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}