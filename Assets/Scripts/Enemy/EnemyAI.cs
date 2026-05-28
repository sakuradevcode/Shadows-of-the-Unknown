using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrolling, Chasing, Attacking, Fleeing, Dead }
    public State currentState;

    [Header("References")]
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    [Header("AI Configuration")]
    public float visionRadius = 10f;
    public float loseFocusDistance = 15f;
    public float attackRange = 2f; 
    public LayerMask obstacleLayer; 
    public float patrolRadius = 10f;

    private Vector3 startPosition;
    
    private bool isAttacking = false;
    
    private float pathUpdateTimer = 0f;
    private float pathUpdateInterval = 0.2f;
    
    private float fleeTimer = 0f; 
    

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;
        agent.autoBraking = false;
        
        agent.acceleration = 60f;
        agent.angularSpeed = 500f;

        currentState = State.Patrolling;
        FindNewPatrolPoint();
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("El enemigo no pudo encontrar al jugador.");
            }
        }
    }

    void Update()
    {
        if (currentState == State.Dead) return;
        
        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.deltaTime;
            
            if (fleeTimer <= 0f && currentState == State.Fleeing)
            {
                currentState = State.Chasing; 
            }
        }

        switch (currentState)
        {
            case State.Patrolling:
                PatrolBehavior();
                break;
            case State.Chasing:
                ChaseBehavior();
                break;
            case State.Attacking:
                AttackBehavior();
                break;
            case State.Fleeing: 
                FleeBehavior();
                break;
        }

        UpdateAnimations();
    }

    // Se llama la linterna cuando le da la luz
    public void Repel()
    {
        if (currentState == State.Dead) return;
        
        // Cada vez que le da la luz, se renueva el tiempo de ceguera/huida
        fleeTimer = 0.5f; 
        currentState = State.Fleeing;
        isAttacking = false; 
    }

    // ¿Qué hace cuando huye?
    void FleeBehavior()
    {
        agent.updateRotation = true;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;

        pathUpdateTimer -= Time.deltaTime;
        
        if (pathUpdateTimer <= 0f)
        {
            // Calcula la dirección contraria a donde está el jugador
            Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
            
            // Le dice al NavMesh que corra hacia un punto que está 5 metros detrás de él
            Vector3 fleeTarget = transform.position + (directionAwayFromPlayer * 5f); 

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeTarget, out hit, 5.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            pathUpdateTimer = pathUpdateInterval;
        }
    }

    void PatrolBehavior()
    {   
        agent.updateRotation = true;
        agent.stoppingDistance = 0f;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            FindNewPatrolPoint();
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= visionRadius)
        {
            Vector3 origin = transform.position; 
            Vector3 targetPosition = player.position; 
            Vector3 directionToPlayer = (targetPosition - origin).normalized;
            
            float sphereRadius = 0.5f; 

            Debug.DrawRay(origin, directionToPlayer * distanceToPlayer, Color.yellow);

            if (!Physics.SphereCast(origin, sphereRadius, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstacleLayer))
            {
                currentState = State.Chasing;
            }
        }
    }

    void ChaseBehavior()
    {   
        agent.updateRotation = false;
        agent.stoppingDistance = attackRange - 0.5f;
        agent.isStopped = false;
        
        pathUpdateTimer -= Time.deltaTime;
        
        if (pathUpdateTimer <= 0f && !agent.pathPending)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                if (Vector3.Distance(agent.destination, hit.position) > 1.0f || !agent.hasPath)
                {
                    agent.SetDestination(hit.position);
                }
            }
            pathUpdateTimer = pathUpdateInterval; 
        }
        
        FacePlayer();
        
        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatPlayerPos = new Vector3(player.position.x, 0f, player.position.z);
        float flatDistance = Vector3.Distance(flatEnemyPos, flatPlayerPos);

        if (flatDistance > loseFocusDistance)
        {
            currentState = State.Patrolling;
            FindNewPatrolPoint();
            return;
        }

        if (flatDistance <= attackRange)
        {
            currentState = State.Attacking;
            agent.isStopped = true; 
        }
        
        if (agent.hasPath)
        {
            Debug.Log($"PHYSICS DIAGNOSTIC:" +
                      $" Velocity: {agent.velocity.magnitude:F2} |" +
                      $" Desired Velocity: {agent.desiredVelocity.magnitude:F2} |" +
                      $" Stopping Distance: {agent.stoppingDistance:F2} |" +
                      $" Remaining Distance: {agent.remainingDistance:F2} |" +
                      $" isStopped: {agent.isStopped} |" +
                      $" Path Pending: {agent.pathPending} |" +
                      $" Path Status: {agent.pathStatus}");
        }
    }
    
    void AttackBehavior()
    {
        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatPlayerPos = new Vector3(player.position.x, 0f, player.position.z);
        float flatDistance = Vector3.Distance(flatEnemyPos, flatPlayerPos);
        
        FacePlayer();

        if (flatDistance > attackRange && !isAttacking)
        {
            agent.isStopped = false;
            currentState = State.Chasing;
            return;
        }

        if (!isAttacking)
        {
            AttackPlayer();
        }
    }

    // --- Helper Methods ---

    void FindNewPatrolPoint()
    {
        Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    void AttackPlayer()
    {
        isAttacking = true;
        animator.SetTrigger("Attack"); 
        
        Invoke(nameof(EndAttack), 1.5f); 
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    public void Die()
    {
        currentState = State.Dead;
        agent.isStopped = true;
        animator.SetTrigger("Die"); 
    }

    void UpdateAnimations()
    {
        float speed = agent.desiredVelocity.magnitude;
        
        if (agent.isStopped)
        {
            speed = 0f;
        }

        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime); 
    }
    
    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; 
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseFocusDistance);
    }
}