using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrolling, Chasing, Attacking, Dead }
    public State currentState;

    [Header("References")]
    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;

    [Header("AI Configuration")]
    public float visionRadius = 10f;
    public float loseFocusDistance = 15f;
    public float attackRange = 2f; // Remember to check if this is 2 or 6 in your Inspector!
    public LayerMask obstacleLayer; 
    public float patrolRadius = 10f;

    private bool isAttacking = false;
    
    private float pathUpdateTimer = 0f;
    private float pathUpdateInterval = 0.2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        agent.autoBraking = false;
        
        // CRITICAL FIX: Force extremely high acceleration and turning speed via code.
        // This guarantees the agent reaches its max speed (4) almost instantly,
        // preventing the "slow buildup" that gets interrupted by path recalculations.
        agent.acceleration = 60f;
        agent.angularSpeed = 500f;

        currentState = State.Patrolling;
        FindNewPatrolPoint();
    }

    void Update()
    {
        if (currentState == State.Dead) return;

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
        }

        UpdateAnimations();
    }

    // --- State Logic ---

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
            // We search for a valid walkable point within 5 meters of the player
            if (NavMesh.SamplePosition(player.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                // CRITICAL FIX: The Anti-Stutter Shield for Partial Paths.
                // We ONLY ask Unity to recalculate the route if the player has moved 
                // more than 1 meter away from the monster's current destination.
                // This stops the engine from spamming 0 velocity to recalculate impossible paths.
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
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * patrolRadius;
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
        // CRITICAL FIX: Use 'desiredVelocity' instead of 'velocity'.
        // 'velocity' is the actual physical speed (which stutters on bumps and corners).
        // 'desiredVelocity' is what the AI WANTS to do (always max speed when chasing).
        // This keeps the Run animation playing perfectly smooth no matter what the physics engine is doing.
        float speed = agent.desiredVelocity.magnitude;
        
        // If the agent is forcefully stopped (like when dead or attacking), force speed to 0
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
        
        // Safety check to prevent Unity error if enemy is exactly inside the player
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