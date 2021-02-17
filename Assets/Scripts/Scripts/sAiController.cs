using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions.Must;
using UnityEngine.AI;

public class sAiController : MonoBehaviour
{
    public AI aiType;
    public Transform destination;
    public Transform shootPoint;
    public float rotationSpeed = 1.5f;
    public bool controlled;
    public bool stayPut;

    [Tooltip("At what distance should the AI stop moving if moving")]
    public float stopMovingAt = 3;
    public float firingRate = 0.25f;
    public GameObject bulletPrefab;
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    [Header("TURN OFF WHEN ENTERING PLAY")]
    // Unity be randomly crashing due to the gizmo drawing, but need it for visualization. 
    // So for now we'll just use this.
    public bool turnOn = true;

    Vector3 originalDestination;
    NavMeshAgent aiAgent;
    Quaternion orgRotation;
    private void Start()
    {
        StartCoroutine(FindTargetWithDelay(.2f));
        aiAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (this.aiType.currentState.Equals(aiState.COMBAT))
        {
            //if (visibleTargets.Count == 0)
            //{
            //    Debug.Log("Found no more targets, going back to finding");
            //    this.aiType.currentState = aiState.FINDING;
            //    return;
            //}
            Quaternion lookRotation = Quaternion.LookRotation(destination.position);
            lookRotation.y = 0;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            if (!stayPut && aiAgent.remainingDistance > stopMovingAt)
            {
                aiAgent.destination = destination.position;
            }

            // Find the closest enemy if there's more then one
            Transform closestEnemy = visibleTargets[0];
            if (visibleTargets.Count > 1)
            {
                for (int i = 0; i < visibleTargets.Count; i++)
                {
                    float distance1 = Vector3.Distance(visibleTargets[i].position, transform.position);
                    float distance2 = Vector3.Distance(closestEnemy.position, transform.position);
                    if (distance1 < distance2)
                    {
                        closestEnemy = visibleTargets[i];
                    }
                }
            }
            if (destination != closestEnemy)
            {
                destination = closestEnemy;
            }

            //if (bulletPrefab)
            //{
            //    Debug.Log("Firing on the enemy");
            //    //Shoot(new Vector3(destination.position.x, destination.position.y + 2, destination.position.z));
            //}
            //else
            //{
            //    Debug.LogError("Enemy does not have the projectile set on the sAiController script");
            //}
        }
        else if (this.aiType.currentState.Equals(aiState.FINDING))
        {
            if (!stayPut)
            {
                if (destination != null)
                    aiAgent.destination = destination.position;
                else
                {
                    // Ask for a new destination from spawner
                }
            }
            else
            {
                if (transform.rotation != orgRotation)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, orgRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        this.aiType.health -= damage;
        if (this.aiType.health <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ShootWhenReady()
    {
        while (this.aiType.currentState.Equals(aiState.COMBAT) && bulletPrefab)
        {
            yield return new WaitForSeconds(firingRate);
            Shoot();
        }

        if (!bulletPrefab)
        {
            Debug.LogError("This enemy does not have a projectile on the sAiController Bullet Prefab variable");
        }
    }

    void Shoot()
    {
        sBullet bullet = Instantiate(bulletPrefab).GetComponent<sBullet>();
        bullet.transform.position = shootPoint.position;
        //bullet.transform.LookAt(target);
    }

    IEnumerator FindTargetWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            // If this ai is not being controlled
            if (!controlled)
                FindVisibleTargets();
            else if (visibleTargets.Count > 0)
                visibleTargets.Clear();

            if (visibleTargets.Count > 0)
            {
                Debug.Log("Found a enemy, entering combat");
                CollectOrginals();
                this.aiType.currentState = aiState.COMBAT;
                StartCoroutine("ShootWhenReady");
            }
            else if (this.aiType.currentState.Equals(aiState.COMBAT))
            {
                Debug.Log("Found no more targets, going back to finding");
                this.aiType.currentState = aiState.FINDING;
            }
        }
    }

    void ReturnToOriginals()
    {
        if (stayPut)
        {
            transform.rotation = orgRotation;
        }
        else
        {
            aiAgent.destination = originalDestination;
        }
    }

    // Get all the original stuff for when we need to return;
    void CollectOrginals()
    {
        if (stayPut)
            orgRotation = transform.rotation;
        else
            originalDestination = aiAgent.destination;
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTaget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTaget) < viewAngle / 2)
            {
                float dsToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTaget, dsToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAnagle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmosSelected()
    {
        if (turnOn)
        {
            //Debug.Log(" Selected ");
            //SFieldOfView fow = (SFieldOfView)target;
            Handles.color = Color.white;
            Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, viewRadius);
            Vector3 viewAngleA = DirFromAnagle(-viewAngle / 2, false);
            Vector3 viewAngleB = DirFromAnagle(viewAngle / 2, false);

            Handles.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
            Handles.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

            Handles.color = Color.red;

            if (visibleTargets.Count > 0)
            {
                foreach (Transform visibleTarget in visibleTargets)
                {
                    Handles.DrawLine(transform.position, visibleTarget.position);
                }
            }
        }
    }
}
