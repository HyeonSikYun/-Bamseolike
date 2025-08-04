using UnityEngine;
using System.Collections.Generic;

public class CompanionAI : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followDistance = 3f;
    public float stopDistance = 1.5f;
    public float moveSpeed = 5f;
    public float smoothTime = 0.3f;

    [Header("Behavior Settings")]
    public float catchUpSpeedMultiplier = 1.5f;
    public float maxCatchUpDistance = 8f;
    public float separationDistance = 1.5f;  // 동료 간 최소 거리
    public float separationForce = 1f;       // 밀어내는 힘

    [Header("Weapon Settings")]
    public ItemData meleeWeaponData;
    public Weapon companionMeleeWeapon;

    private Transform player;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CompanionWeapon companionWeapon;

    private enum CompanionState { Following, Idle, CatchingUp }
    private CompanionState currentState = CompanionState.Following;

    private static List<CompanionAI> allCompanions = new List<CompanionAI>();

    void Start()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
            player = GameManager.instance.player.transform;
        else
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("CompanionAI: Player not found!");
            return;
        }

        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        companionWeapon = GetComponent<CompanionWeapon>();

        Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        transform.position = player.position + offset;

        if (meleeWeaponData != null && meleeWeaponData.itemType == ItemData.ItemType.Melee)
        {
            GameObject weaponObj = new GameObject("CompanionMeleeWeapon");
            weaponObj.transform.parent = transform;
            weaponObj.transform.localPosition = Vector3.zero;

            companionMeleeWeapon = weaponObj.AddComponent<Weapon>();
            companionMeleeWeapon.Init(meleeWeaponData, transform);
        }

        allCompanions.Add(this);
    }

    void OnDestroy()
    {
        allCompanions.Remove(this);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        DetermineState(distanceToPlayer);

        switch (currentState)
        {
            case CompanionState.Following:
                FollowPlayer();
                break;
            case CompanionState.Idle:
                StayIdle();
                break;
            case CompanionState.CatchingUp:
                CatchUpToPlayer();
                break;
        }

        UpdateAnimationAndDirection();
    }

    void DetermineState(float distanceToPlayer)
    {
        if (distanceToPlayer > maxCatchUpDistance)
            currentState = CompanionState.CatchingUp;
        else if (distanceToPlayer <= stopDistance)
            currentState = CompanionState.Idle;
        else if (distanceToPlayer > followDistance)
            currentState = CompanionState.Following;
    }

    void FollowPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        targetPosition = player.position - directionToPlayer * followDistance;
        Vector3 randomOffset = new Vector3(Mathf.Sin(Time.time * 2f) * 0.5f, Mathf.Cos(Time.time * 1.5f) * 0.3f, 0);
        targetPosition += randomOffset;

        // Separation 적용
        Vector3 separationVector = Vector3.zero;
        foreach (var companion in allCompanions)
        {
            if (companion != this)
            {
                float distance = Vector3.Distance(transform.position, companion.transform.position);
                if (distance < separationDistance)
                {
                    Vector3 away = (transform.position - companion.transform.position).normalized;
                    separationVector += away * (separationDistance - distance);
                }
            }
        }

        targetPosition += separationVector * separationForce;
        MoveToTarget(moveSpeed);
    }

    void StayIdle()
    {
        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 5f);
        if (rb2D != null)
            rb2D.linearVelocity = velocity;
        else
            transform.position += velocity * Time.deltaTime;
    }

    void CatchUpToPlayer()
    {
        targetPosition = player.position;
        MoveToTarget(moveSpeed * catchUpSpeedMultiplier);
    }

    void MoveToTarget(float speed)
    {
        if (rb2D != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb2D.linearVelocity = direction * speed;
            velocity = rb2D.linearVelocity;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, speed);
        }
    }

    void UpdateAnimationAndDirection()
    {
        float moveSpeed = velocity.magnitude;
        bool isMoving = moveSpeed > 0.1f;
        if (animator != null)
            animator.SetBool("1_Move", isMoving);

        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = velocity.x > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        Vector3 spawnOffset = new Vector3(Random.Range(-followDistance, followDistance), Random.Range(-followDistance, followDistance), 0);
        transform.position = player.position + spawnOffset;

        if (companionWeapon != null)
            companionWeapon.enabled = true;
    }

    public void UpgradeCompanionWeapon(float damageMultiplier = 0.2f, float fireRateMultiplier = 0.9f)
    {
        if (companionWeapon == null) return;

        Weapon[] playerWeapons = player.GetComponentsInChildren<Weapon>();
        if (playerWeapons.Length > 0)
        {
            float rangeWeaponDamage = 0f;
            float rangeWeaponFireRate = 1f;
            bool hasRangeWeapon = false;

            foreach (Weapon weapon in playerWeapons)
            {
                if (weapon.id != 0)
                {
                    rangeWeaponDamage = weapon.damage;
                    rangeWeaponFireRate = weapon.speed;
                    hasRangeWeapon = true;
                    break;
                }
            }

            if (hasRangeWeapon)
            {
                float newDamage = companionWeapon.damage + (rangeWeaponDamage * damageMultiplier);
                float newFireRate = Mathf.Max(0.3f, companionWeapon.fireRate * fireRateMultiplier);
                companionWeapon.UpgradeWeapon(newDamage, newFireRate);
            }
        }
    }
}
