using UnityEngine;

public class CompanionWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 5f;              // 공격력
    public float fireRate = 1f;            // 발사 간격 (초)
    public float detectionRange = 7f;      // 적 탐지 범위
    public int bulletPrefabId = 1;         // 사용할 총알 프리팹 ID
    public int penetration = 1;            // 관통력

    [Header("Upgrade Limits")]
    public float maxDamage = 50f;          // 최대 공격력 제한
    public float minFireRate = 0.5f;       // 최소 발사 간격 (너무 빠르게 안 쏘게)
    public int upgradeCount = 0;           // 업그레이드 횟수 추적
    public int maxUpgrades = 10;           // 최대 업그레이드 횟수

    private float fireTimer = 0f;
    private Transform nearestEnemy;
    private CompanionAI companionAI;

    void Start()
    {
        companionAI = GetComponent<CompanionAI>();
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        // 가장 가까운 적 찾기
        FindNearestEnemy();

        // 발사 타이머 업데이트
        fireTimer += Time.deltaTime;

        // 적이 있고 발사 가능하면 총 발사
        if (nearestEnemy != null && fireTimer >= fireRate)
        {
            Fire();
            fireTimer = 0f;
        }
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = detectionRange;
        nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
    }

    void Fire()
    {
        if (nearestEnemy == null) return;

        // 적 방향 계산
        Vector3 targetPos = nearestEnemy.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        // 총알 생성
        Transform bullet = GameManager.instance.pool.Get(bulletPrefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // 총알 초기화
        bullet.GetComponent<Bullet>().Init(damage, penetration, dir);
    }

    // 동료 무기 업그레이드 (제한적으로)
    public void UpgradeWeapon(float newDamage, float newFireRate)
    {
        // 업그레이드 횟수 제한
        if (upgradeCount >= maxUpgrades)
        {
            return;
        }

        // 공격력은 최대치 제한
        damage = Mathf.Min(newDamage, maxDamage);

        // 발사속도는 최소 간격 이상으로 제한 (너무 빨라지지 않게)
        fireRate = Mathf.Max(newFireRate, minFireRate);

        upgradeCount++;

        Debug.Log($"Companion weapon upgraded: Damage={damage}, FireRate={fireRate}, Count={upgradeCount}");
    }

}