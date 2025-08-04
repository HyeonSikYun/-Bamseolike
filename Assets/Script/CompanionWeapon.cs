using UnityEngine;

public class CompanionWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 5f;              // ���ݷ�
    public float fireRate = 1f;            // �߻� ���� (��)
    public float detectionRange = 7f;      // �� Ž�� ����
    public int bulletPrefabId = 1;         // ����� �Ѿ� ������ ID
    public int penetration = 1;            // �����

    [Header("Upgrade Limits")]
    public float maxDamage = 50f;          // �ִ� ���ݷ� ����
    public float minFireRate = 0.5f;       // �ּ� �߻� ���� (�ʹ� ������ �� ���)
    public int upgradeCount = 0;           // ���׷��̵� Ƚ�� ����
    public int maxUpgrades = 10;           // �ִ� ���׷��̵� Ƚ��

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

        // ���� ����� �� ã��
        FindNearestEnemy();

        // �߻� Ÿ�̸� ������Ʈ
        fireTimer += Time.deltaTime;

        // ���� �ְ� �߻� �����ϸ� �� �߻�
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

        // �� ���� ���
        Vector3 targetPos = nearestEnemy.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        // �Ѿ� ����
        Transform bullet = GameManager.instance.pool.Get(bulletPrefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // �Ѿ� �ʱ�ȭ
        bullet.GetComponent<Bullet>().Init(damage, penetration, dir);
    }

    // ���� ���� ���׷��̵� (����������)
    public void UpgradeWeapon(float newDamage, float newFireRate)
    {
        // ���׷��̵� Ƚ�� ����
        if (upgradeCount >= maxUpgrades)
        {
            return;
        }

        // ���ݷ��� �ִ�ġ ����
        damage = Mathf.Min(newDamage, maxDamage);

        // �߻�ӵ��� �ּ� ���� �̻����� ���� (�ʹ� �������� �ʰ�)
        fireRate = Mathf.Max(newFireRate, minFireRate);

        upgradeCount++;

        Debug.Log($"Companion weapon upgraded: Damage={damage}, FireRate={fireRate}, Count={upgradeCount}");
    }

}