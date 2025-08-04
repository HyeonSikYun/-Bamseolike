using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Boss : MonoBehaviour
{
    bool isLive = true;
    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    WaitForFixedUpdate wait;
    SortingGroup sortingGroup;
    public Rigidbody2D target;

    public float speed;
    public float health;
    public float maxHealth;

    // �������� Ư���� �Ӽ���
    public bool isBoss = true;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        wait = new WaitForFixedUpdate();
        sortingGroup = GetComponent<SortingGroup>();
    }

    // ���� ���� ���� ���� ������
    public float attackDistance = 3f; // ������ ������ �Ÿ�
    public float chargeSpeed = 10f; // ���� �ӵ�
    public float chargeTime = 0.5f; // ���� ���� �ð�
    public float chargeCooldown = 2f; // ���� �� ��ٿ�

    private bool isCharging = false;
    private bool canCharge = true;
    private Vector2 chargeDirection;
    private float chargeTimer = 0f;
    private float cooldownTimer = 0f;

    // �ܻ� ȿ���� ���� ������ (������ ���ʿ�)
    private float afterImageTimer = 0f;
    private float afterImageInterval = 0.08f; // �ܻ� ���� ����

    // ���� �� �÷��̾�� �浹 ó��
    private bool hasHitPlayerThisCharge = false;

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("DAMAGED"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        float distanceToPlayer = dirVec.magnitude;

        // ���⿡ ���� ��������Ʈ ������
        if (dirVec.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dirVec.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // ��ٿ� Ÿ�̸� ������Ʈ
        if (!canCharge)
        {
            cooldownTimer += Time.fixedDeltaTime;
            if (cooldownTimer >= chargeCooldown)
            {
                canCharge = true;
                cooldownTimer = 0f;
            }
        }

        if (isCharging)
        {
            // ���� ��
            chargeTimer += Time.fixedDeltaTime;
            afterImageTimer += Time.fixedDeltaTime;

            // �ܻ� ȿ�� ����
            if (afterImageTimer >= afterImageInterval)
            {
                CreateAfterImage();
                afterImageTimer = 0f;
            }

            rigid.MovePosition(rigid.position + chargeDirection * chargeSpeed * Time.fixedDeltaTime);

            if (chargeTimer >= chargeTime)
            {
                // ���� ����
                isCharging = false;
                canCharge = false;
                chargeTimer = 0f;
                hasHitPlayerThisCharge = false;
            }
        }
        else if (distanceToPlayer > attackDistance)
        {
            // ���� �Ÿ����� �ָ� �Ϲ������� ����
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }
        else if (canCharge && distanceToPlayer <= attackDistance)
        {
            // ���� �Ÿ� ���� �ְ� ���� �����ϸ� ���� ����
            isCharging = true;
            chargeDirection = dirVec.normalized;
            // ���� ���� �� �ִϸ��̼� Ʈ���� (�ִٸ�)
            // anim.SetTrigger("Charge");
        }

        rigid.linearVelocity = Vector2.zero;
    }

    private void OnEnable()
    {
        anim.SetBool("1_Move", true);
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        sortingGroup.sortingOrder = 3; // ������ �� �տ� ������
        health = maxHealth;
    }

    public void Init(BossData data)
    {
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

        // ���� ���� ���� �ʱ�ȭ
        isCharging = false;
        canCharge = true;
        chargeTimer = 0f;
        cooldownTimer = 0f;
        hasHitPlayerThisCharge = false;
        afterImageTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���� ���� ���� �÷��̾���� �浹 ó��
        if (isCharging && collision.CompareTag("Player") && !hasHitPlayerThisCharge)
        {
            hasHitPlayerThisCharge = true;
            // �÷��̾�� �������� �ִ� ������ ���⿡ �߰�
            // GameManager.instance.player.GetComponent<Player>().TakeDamage(damage);
            return; // �÷��̾�� �浹 �� �����ϹǷ� �ٸ� ó���� ���� ����
        }

        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        health -= collision.GetComponent<Bullet>().damage;

        // ������ �˹��� ������ ���� (KnockBack �ڷ�ƾ ȣ�� ����)
        // StartCoroutine(KnockBack());

        // �ǰ� �� ��� ���������� �����ϴ� ȿ���� �߰�
        StartCoroutine(HitEffect());

        if (health > 0)
        {
            anim.SetTrigger("3_Damaged");
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            StartCoroutine("Death");
            sortingGroup.sortingOrder = 1;

            // ���� óġ �� Ư���� ����
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
            // ������ óġ���� �� �߰� �����̳� ���� Ŭ���� ������ ���⿡ �߰��� �� �ֽ��ϴ�
            OnBossDefeated();
        }
    }

    // �˹� ��� ������ �ǰ� ȿ��
    IEnumerator HitEffect()
    {
        // �ڽ� ������Ʈ�� SpriteRenderer�� ã��
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        float fadeTime = 0.1f;

        // ���������� ����
        foreach (var sr in spriteRenderers)
        {
            sr.color = Color.red;
        }
        yield return new WaitForSeconds(fadeTime);

        // �ٽ� ������� ����
        foreach (var sr in spriteRenderers)
        {
            sr.color = Color.white;
        }
    }

    // �ܻ� ȿ�� ���� (����� ĳ���Ϳ�)
    void CreateAfterImage()
    {
        // ���� ������ ��� SpriteRenderer�� ã�Ƽ� �ܻ� ����
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            // �� ��������Ʈ���� �ܻ� ������Ʈ ����
            GameObject afterImage = new GameObject("AfterImage");
            afterImage.transform.position = sr.transform.position;
            afterImage.transform.rotation = sr.transform.rotation;
            afterImage.transform.localScale = sr.transform.lossyScale;

            // SpriteRenderer ������Ʈ �߰�
            SpriteRenderer afterImageSR = afterImage.AddComponent<SpriteRenderer>();
            afterImageSR.sprite = sr.sprite;
            afterImageSR.color = new Color(1f, 0.5f, 0.5f, 0.6f); // ������ ������
            afterImageSR.sortingLayerName = sr.sortingLayerName;
            afterImageSR.sortingOrder = sr.sortingOrder - 1;
            afterImageSR.flipX = sr.flipX;
            afterImageSR.flipY = sr.flipY;

            // �ܻ��� ������ ������� �ϴ� �ڷ�ƾ ����
            StartCoroutine(FadeAfterImage(afterImage));
        }
    }

    // �ܻ� ���̵� �ƿ�
    IEnumerator FadeAfterImage(GameObject afterImage)
    {
        SpriteRenderer sr = afterImage.GetComponent<SpriteRenderer>();
        float fadeSpeed = 3f;
        float scaleSpeed = 1.2f;
        Vector3 originalScale = afterImage.transform.localScale;

        while (sr.color.a > 0)
        {
            // ���İ� ����
            Color color = sr.color;
            color.a -= fadeSpeed * Time.deltaTime;
            sr.color = color;

            // �ణ ������ ���� ȿ��
            afterImage.transform.localScale = Vector3.Lerp(afterImage.transform.localScale,
                originalScale * scaleSpeed, Time.deltaTime * 2f);

            yield return null;
        }

        Destroy(afterImage);
    }

    IEnumerator Death()
    {
        anim.SetTrigger("4_Death");
        yield return new WaitForSeconds(0.7f);
        Dead();
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }

    // ������ óġ�Ǿ��� �� ȣ��Ǵ� �޼���
    void OnBossDefeated()
    {
        // ���⿡ ���� óġ �� Ư���� ������ �߰��� �� �ֽ��ϴ�
        // ��: ���� Ŭ����, Ư���� ������ ���, ���� ���������� �̵� ��
        Debug.Log("Boss Defeated!");
    }
}