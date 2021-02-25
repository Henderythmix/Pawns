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
    [HideInInspector] public bool controlled = false;
    // For player characters
    public bool player = false;
    public bool cache = false;
    public bool startingCharacter = false;


    [Tooltip("Put the actual health bar here, and put the empty GO in the health bar holder variable.")]
    public Transform healthBar;
    [Tooltip("Make sure to put the empty GO that is holding the health bar here, and the actual healthbar in Health Bar")]
    public Transform healthBarHolder;
    public Transform objectHoldingHealthBarHolder;

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
    public Animator aiAnimator;
    public AudioSource shootSound;
    public AudioSource painSound;

    public List<Transform> visibleTargets;

    //[Header("TURN OFF WHEN ENTERING PLAY")]
    //// Unity be randomly crashing due to the gizmo drawing, but need it for visualization. 
    //// So for now we'll just use this.
    //public bool turnOn = true;

    [Header("Public for debugging, dont touch")]
    public aiState currentState;
    public float health = 0;
    public float maxHealth = 0;
    bool attackCooldown = false;
    bool canSeeHiddenEnemies = false;
    //bool movedIntoLevelArea = false;
    Transform originalDestination;
    public NavMeshAgent aiAgent;
    [HideInInspector] public Quaternion orgRotation;
    private void Awake()
    {
        if (viewRadius > 0)
        {
            StartCoroutine(FindTargetWithDelay(.2f));
            aiAgent = gameObject.GetComponent<NavMeshAgent>();
        }
        if (!player && !aiAnimator)
            aiAnimator = gameObject.GetComponent<Animator>();

        //if (destination == null)
        //{
        //    stayPut = true;
        //}
    }

    private void Start()
    {
        if (cache)
        {
            LevelManager.instance.currentCache.Add(this);
            InitAI(aiType, false);
            player = true;
        }
        if (aiAnimator)
            aiAnimator.SetBool("Moving", true);

    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        //if (player) destination = null;
        if (destination != null && !player) aiAgent.SetDestination(destination.position);
        else if (visibleTargets.Count > 0)
        {
            if (visibleTargets.Count > 1)
            {
                if (visibleTargets[0] == null)
                    visibleTargets.Remove(visibleTargets[0]);
            }
            destination = visibleTargets[0];
        }
        else if (LevelManager.instance.playerCharactersSpawned.Count > 0 && !player)
            destination = sEnemySpawner.instance.FindClosestTarget(transform.position);

        if (currentState.Equals(aiState.COMBAT))
        {
            if (destination == null)
                return;
            else if (visibleTargets[0] == null)
                return;

            Transform closestEnemy = visibleTargets[0];
            if (visibleTargets.Count > 1)            // Find the closest enemy if there's more then one
            {
                for (int i = 0; i < visibleTargets.Count; i++)
                {
                    if (visibleTargets.Count == 0) break;
                    else i++;
                    float distance1 = Vector3.Distance(visibleTargets[i].position, transform.position);
                    float distance2 = Vector3.Distance(closestEnemy.position, transform.position);
                    if (distance1 < distance2)
                    {
                        closestEnemy = visibleTargets[i];
                    }
                }
            }
            //Debug.Log(closestEnemy.position);
            Vector3 lookPos = closestEnemy.position - transform.position;
            lookPos.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(lookPos);

            //Debug.Log(destination.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            if (!player)
            {
                if (aiAgent.remainingDistance > stopMovingAt)
                {
                    if (aiAgent.isStopped)
                    {
                        aiAgent.isStopped = false;
                        aiAnimator.SetBool("Moving", true);
                    }
                    //Debug.Log("Moving the ai");
                }
                else
                {
                    //Debug.Log("Stopping the ai");
                    aiAgent.isStopped = true;
                    aiAnimator.SetBool("Moving", false);
                }
                //Debug.Log(aiAgent.remainingDistance);

                if (destination != closestEnemy) destination = closestEnemy;
            }


            if (!attackCooldown)
            {
                //Debug.Log("Firing at enemy. I am " + gameObject.name);
                if (bulletPrefab) Shoot();
                else Melee();
                StartCoroutine("AttackCooldown");
            }
        }
        else if (currentState.Equals(aiState.FINDING))
        {
            if (player)
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
        player = _playerCharacter;
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
            healthBar.parent = objectHoldingHealthBarHolder;

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

    // TODO: Have this keep track of all the players characters (or keep track of it in sPlayerController) so that if the health is reduced completely enough for one character, they are eliminated, otherwise they come out with that much health
    public void AiTakeDamage(float damage)
    {
        //health -= damage;
        if (painSound)
            painSound.Play();
        if (healthBarHolder) SetHealth(-damage, false);

        //Debug.Log(gameObject.name + " is taking damage");
        if (health <= 0)
        {
            if (!cache)
            {
                if (player)
                {
                    LevelManager lmi = LevelManager.instance;
                    lmi.playerCharactersSpawned.Remove(gameObject.GetComponent<sPlayerController>());
                    if (lmi.playerCharactersSpawned.Count == 0) lmi.LoseGame();
                    //Find this aiType and set it to dead
                    for (int i = 0; i < lmi.playerCharactersGlobal.Count; i++)
                    {
                        if (lmi.playerCharactersGlobal[i].playerType.playerCharacterType == aiType)
                        {
                            lmi.playerCharactersGlobal[i].dead = true;
                        }
                    }
                    //LevelManager.instance.deadPlayers.Add(gameObject.GetComponent<sPlayerController>());
                }
                else
                {
                    LevelManager.instance.playersMoney += aiType.rewardForEliminating;
                    cHudManager.instance.moneyText.text = LevelManager.instance.playersMoney.ToString();
                }
            }
            else
            {
                Debug.Log("A cache was destroyed");
                LevelManager.instance.currentCache.Remove(this);
                Debug.Log(LevelManager.instance.currentCache.Count);
                if (LevelManager.instance.currentCache.Count == 0) LevelManager.instance.LoseGame();
                else Debug.Log(LevelManager.instance.currentCache.Count);
            }

            // Is a player to be added to the dead
            if (!player || cache)
                Destroy(gameObject);
            else
                Destroy(transform.parent.gameObject);
        }
    }


    IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        yield return new WaitForSeconds(aiType.attackRate);
        attackCooldown = false;
    }

    public void Melee()
    {
        destination.GetComponent<sAiController>().AiTakeDamage(aiType.damageOutput);
    }

    public void Shoot()
    {
        if (shootSound)
            shootSound.Play();
        sBullet bullet = Instantiate(bulletPrefab).GetComponent<sBullet>();
        float dmg = aiType.damageOutput;
        if (player)
            if (gameObject.transform.parent.gameObject.GetComponent<sPlayerController>()) dmg = gameObject.transform.parent.gameObject.GetComponent<sPlayerController>().damageOutput;
        bullet.damage = dmg;
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
            visibleTargets = new List<Transform>();
            if (!controlled)
            {
                FindVisibleTargets();
            }
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
        if (player)
        {
            transform.rotation = orgRotation;
        }
        else
        {
            if (originalDestination == null) originalDestination = sEnemySpawner.instance.FindClosestTarget(transform.position);
            aiAgent.SetDestination(originalDestination.position);
        }
    }

    // Get all the original stuff for when we need to return;
    void CollectOrginals()
    {
        if (player)
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

                if (!Physics.Raycast(transform.position, dirToTaget, dsToTarget, obstacleMask))
                {
                    //if (!target.GetComponent<sAiController>().aiType.isHidden || canSeeHiddenEnemies)
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
}

//    private void OnDrawGizmosSelected()
//    {
//        //if (turnOn)
//        //{
//        //Debug.Log(" Selected ");
//        //SFieldOfView fow = (SFieldOfView)target;
//        Handles.color = Color.white;
//        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, viewRadius);
//        Vector3 viewAngleA = DirFromAnagle(-viewAngle / 2, false);
//        Vector3 viewAngleB = DirFromAnagle(viewAngle / 2, false);

//        Handles.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
//        Handles.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

//        Handles.color = Color.red;

//        if (visibleTargets.Count > 0)
//        {
//            foreach (Transform visibleTarget in visibleTargets)
//            {
//                if (visibleTarget != null)
//                    Handles.DrawLine(transform.position, visibleTarget.position);
//            }
//        }
//        //}
//    }
//}
