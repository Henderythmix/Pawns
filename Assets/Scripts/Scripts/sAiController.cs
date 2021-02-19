using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class sAiController : MonoBehaviour
{
    public AI aiType;
    public Transform destination;
    public Transform shootPoint;
    public float rotationSpeed = 1.5f;
    [HideInInspector]public bool controlled = false;
    // For player characters
    public bool stayPut = false;
    public bool cache = false;
    public bool startingCharacter = false;

    [HideInInspector]public aiState currentState;
    [HideInInspector]public float health = 0;
    [HideInInspector]public float maxHealth = 0;
    [Tooltip("Put the actual health bar here, and put the empty GO in the health bar holder variable.")]
    public Transform healthBar;
    [Tooltip("Make sure to put the empty GO that is holding the health bar here, and the actual healthbar in Health Bar")]
    public Transform healthBarHolder;

    [Tooltip("At what distance should the AI stop moving if moving")]
    public float stopMovingAt = 3;
    [Tooltip("Put the projectile used for this ai in here, otherwise leave it for the ai to use melee")]
    public GameObject bulletPrefab;
    [Header("Detection Variables")]
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    //[Header("TURN OFF WHEN ENTERING PLAY")]
    //// Unity be randomly crashing due to the gizmo drawing, but need it for visualization. 
    //// So for now we'll just use this.
    //public bool turnOn = true;

    bool attackCooldown = false;
    bool canSeeHiddenEnemies = false;
    //bool movedIntoLevelArea = false;
    Transform originalDestination;
    public NavMeshAgent aiAgent;
    [HideInInspector]public Quaternion orgRotation;
    private void Awake()
    {
        if (viewRadius > 0)
        {
            StartCoroutine(FindTargetWithDelay(.2f));
            aiAgent = gameObject.GetComponent<NavMeshAgent>();
        }

        //if (destination == null)
        //{
        //    stayPut = true;
        //}
    }

    private void Start()
    {
        if (cache)
            LevelManager.instance.currentCache.Add(this);
    }

    private void Update()
    {
        if (!stayPut)
        {
            if (destination != null) aiAgent.SetDestination(destination.position);
            else if (LevelManager.instance.playerCharactersAlive.Count > 0)
                destination = sEnemySpawner.instance.FindClosestTarget(transform.position); 
        }

        if (currentState.Equals(aiState.COMBAT))
        {
            if (destination == null)
                return;

            Transform closestEnemy = visibleTargets[0];
            if (visibleTargets.Count > 1)            // Find the closest enemy if there's more then one
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
            Quaternion lookRotation = Quaternion.LookRotation(destination.position);
            lookRotation.y = 0;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            if (!stayPut)
            {
                if (aiAgent.remainingDistance > stopMovingAt)
                {
                    if (aiAgent.isStopped)
                        aiAgent.isStopped = false;
                    //Debug.Log("Moving the ai");
                }
                else
                {
                    //Debug.Log("Stopping the ai");
                    aiAgent.isStopped = true;
                }
                //Debug.Log(aiAgent.remainingDistance);

                if (destination != closestEnemy) destination = closestEnemy;
            }


            if (!attackCooldown)
            {
                if (bulletPrefab) Shoot();
                else Melee();
                StartCoroutine("AttackCooldown");
            }
        }
        else if (currentState.Equals(aiState.FINDING))
        {
            if (stayPut)
            {
                if (transform.rotation != orgRotation)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, orgRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void InitAI(AI _aiType, bool _playerCharacter)
    {
        //health = _aiType.health;
        //maxHealth = _aiType.health;
        if (!startingCharacter)
        {
            SetHealth(_aiType.health, true);
            SetNewMaxHealth(_aiType.health, false, true, true);
        }
        aiType = _aiType;
        stayPut = _playerCharacter;
        canSeeHiddenEnemies = _aiType.canSeeHiddenEnemies;
    }

    // Make sure this function is called first if paired with setting health
    public void SetNewMaxHealth(float _newMaxHealth, bool _reAdjustHealth, bool set, bool reAttach)
    {
        if (!set)
            maxHealth += _newMaxHealth;
        else
            maxHealth = _newMaxHealth;
        // Ungroup and then regroup the health bar to accomdate new max health
        if (healthBar.parent = healthBarHolder)
            healthBar.parent = transform;

        healthBarHolder.localScale = new Vector3(maxHealth, healthBarHolder.localScale.y, healthBarHolder.localScale.z);

        if (reAttach)
        {
            healthBar.parent = healthBarHolder;
            ResetupHealthBar();
        }
        //Debug.Log(healthBar.localScale.x + " After reattaching parent");
        if (_reAdjustHealth)
            healthBarHolder.localScale = new Vector3(maxHealth - (maxHealth - health), healthBarHolder.localScale.y, healthBarHolder.localScale.z);
    }

    public void ResetupHealthBar()
    {
        healthBar.localScale = new Vector3(0.005f, healthBar.localScale.y, healthBar.localScale.z);
        healthBar.localPosition = new Vector3(0.0025f, healthBar.localPosition.y, healthBar.localPosition.z);
    }

    public void SetHealth(float _amount, bool set)
    {
        if (!set)
            health += _amount;
        else
            health = _amount;
        //Debug.Log(health + " Health");
        healthBarHolder.localScale = new Vector3(health, healthBarHolder.localScale.y, healthBarHolder.localScale.z);
    }

    public void TakeDamage(float damage)
    {
        //health -= damage;
        if (healthBarHolder) SetHealth(-damage, false);

        Debug.Log(gameObject.name + " is taking damage");
        if (health <= 0)
        {
            if (!cache)
            {
                if (stayPut)
                {
                    LevelManager.instance.playerCharactersAlive.Remove(gameObject.GetComponent<sPlayerController>());
                    if (LevelManager.instance.playerCharactersAlive.Count == 0) LevelManager.instance.LoseGame();
                }
                else
                {
                    LevelManager.instance.playersMoney += aiType.rewardForEliminating;
                    cHudManager.instance.moneyText.text = LevelManager.instance.playersMoney.ToString();
                }
            }
            else
            {
                LevelManager.instance.currentCache.Remove(this);
                if (LevelManager.instance.currentCache.Count == 0) LevelManager.instance.LoseGame();
                else Debug.Log(LevelManager.instance.currentCache.Count);
            }

            Destroy(gameObject);
        }
    }

    //IEnumerator AttackWhenReady()
    //{
    //    while (currentState.Equals(aiState.COMBAT))
    //    {
    //        yield return new WaitForSeconds(aiType.attackRate);
    //        if (bulletPrefab)
    //            Shoot();
    //        else if (aiAgent.isStopped)
    //            Melee();
    //    }


    //}

    IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        yield return new WaitForSeconds(aiType.attackRate);
        attackCooldown = false;
    }

    public void Melee()
    {
        destination.GetComponent<sAiController>().TakeDamage(aiType.damageOutput);
    }

    public void Shoot()
    {
        sBullet bullet = Instantiate(bulletPrefab).GetComponent<sBullet>();
        bullet.damage = aiType.damageOutput;
        bullet.enemy = aiType.enemy;
        if (aiType.enemy == "None") Debug.LogError("There isn't any tag setup on the aiType.enemy.");
        bullet.transform.rotation = shootPoint.rotation;
        //bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bullet.speed);
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
                //Debug.Log("Found a enemy, entering combat");
                CollectOrginals();
                currentState = aiState.COMBAT;
                //StartCoroutine("AttackWhenReady");
            }
            // No longer in combat, go back to finding.
            else if (currentState.Equals(aiState.COMBAT))
            {
                ReturnToOriginals();
                //Debug.Log("No more targets, going back to finding");
                currentState = aiState.FINDING;
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
            aiAgent.SetDestination(originalDestination.position);
        }
    }

    // Get all the original stuff for when we need to return;
    void CollectOrginals()
    {
        if (stayPut)
            orgRotation = transform.rotation;
        else
            originalDestination = destination;
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

                if (!Physics.Raycast(transform.position, dirToTaget, dsToTarget, obstacleMask) && (!target.GetComponent<sAiController>().aiType.isHidden || canSeeHiddenEnemies))
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
        //if (turnOn)
        //{
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
        //}
    }
}
