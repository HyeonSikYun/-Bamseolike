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

    // 보스만의 특별한 속성들
    public bool isBoss = true;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        wait = new WaitForFixedUpdate();
        sortingGroup = GetComponent<SortingGroup>();
    }

    // 보스 전용 공격 패턴 변수들
    public float attackDistance = 3f; // 공격을 시작할 거리
    public float chargeSpeed = 10f; // 돌격 속도
    public float chargeTime = 0.5f; // 돌격 지속 시간
    public float chargeCooldown = 2f; // 돌격 후 쿨다운

    private bool isCharging = false;
    private bool canCharge = true;
    private Vector2 chargeDirection;
    private float chargeTimer = 0f;
    private float cooldownTimer = 0f;

    // 잔상 효과를 위한 변수들 (프리팹 불필요)
    private float afterImageTimer = 0f;
    private float afterImageInterval = 0.08f; // 잔상 생성 간격

    // 돌격 중 플레이어와 충돌 처리
    private bool hasHitPlayerThisCharge = false;

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("DAMAGED"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        float distanceToPlayer = dirVec.magnitude;

        // 방향에 따른 스프라이트 뒤집기
        if (dirVec.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dirVec.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // 쿨다운 타이머 업데이트
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
            // 돌격 중
            chargeTimer += Time.fixedDeltaTime;
            afterImageTimer += Time.fixedDeltaTime;

            // 잔상 효과 생성
            if (afterImageTimer >= afterImageInterval)
            {
                CreateAfterImage();
                afterImageTimer = 0f;
            }

            rigid.MovePosition(rigid.position + chargeDirection * chargeSpeed * Time.fixedDeltaTime);

            if (chargeTimer >= chargeTime)
            {
                // 돌격 종료
                isCharging = false;
                canCharge = false;
                chargeTimer = 0f;
                hasHitPlayerThisCharge = false;
            }
        }
        else if (distanceToPlayer > attackDistance)
        {
            // 공격 거리보다 멀면 일반적으로 접근
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }
        else if (canCharge && distanceToPlayer <= attackDistance)
        {
            // 공격 거리 내에 있고 돌격 가능하면 돌격 시작
            isCharging = true;
            chargeDirection = dirVec.normalized;
            // 돌격 시작 시 애니메이션 트리거 (있다면)
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
        sortingGroup.sortingOrder = 3; // 보스는 더 앞에 렌더링
        health = maxHealth;
    }

    public void Init(BossData data)
    {
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

        // 보스 공격 패턴 초기화
        isCharging = false;
        canCharge = true;
        chargeTimer = 0f;
        cooldownTimer = 0f;
        hasHitPlayerThisCharge = false;
        afterImageTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 돌격 중일 때는 플레이어와의 충돌 처리
        if (isCharging && collision.CompareTag("Player") && !hasHitPlayerThisCharge)
        {
            hasHitPlayerThisCharge = true;
            // 플레이어에게 데미지를 주는 로직을 여기에 추가
            // GameManager.instance.player.GetComponent<Player>().TakeDamage(damage);
            return; // 플레이어와 충돌 시 관통하므로 다른 처리는 하지 않음
        }

        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        health -= collision.GetComponent<Bullet>().damage;

        // 보스는 넉백을 당하지 않음 (KnockBack 코루틴 호출 제거)
        // StartCoroutine(KnockBack());

        // 피격 시 잠시 빨간색으로 변경하는 효과만 추가
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

            // 보스 처치 시 특별한 보상
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
            // 보스를 처치했을 때 추가 보상이나 게임 클리어 로직을 여기에 추가할 수 있습니다
            OnBossDefeated();
        }
    }

    // 넉백 대신 간단한 피격 효과
    IEnumerator HitEffect()
    {
        // 자식 오브젝트의 SpriteRenderer들 찾기
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        float fadeTime = 0.1f;

        // 빨간색으로 변경
        foreach (var sr in spriteRenderers)
        {
            sr.color = Color.red;
        }
        yield return new WaitForSeconds(fadeTime);

        // 다시 원래대로 복원
        foreach (var sr in spriteRenderers)
        {
            sr.color = Color.white;
        }
    }

    // 잔상 효과 생성 (리깅된 캐릭터용)
    void CreateAfterImage()
    {
        // 현재 보스의 모든 SpriteRenderer를 찾아서 잔상 생성
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            // 각 스프라이트마다 잔상 오브젝트 생성
            GameObject afterImage = new GameObject("AfterImage");
            afterImage.transform.position = sr.transform.position;
            afterImage.transform.rotation = sr.transform.rotation;
            afterImage.transform.localScale = sr.transform.lossyScale;

            // SpriteRenderer 컴포넌트 추가
            SpriteRenderer afterImageSR = afterImage.AddComponent<SpriteRenderer>();
            afterImageSR.sprite = sr.sprite;
            afterImageSR.color = new Color(1f, 0.5f, 0.5f, 0.6f); // 붉은빛 반투명
            afterImageSR.sortingLayerName = sr.sortingLayerName;
            afterImageSR.sortingOrder = sr.sortingOrder - 1;
            afterImageSR.flipX = sr.flipX;
            afterImageSR.flipY = sr.flipY;

            // 잔상을 서서히 사라지게 하는 코루틴 시작
            StartCoroutine(FadeAfterImage(afterImage));
        }
    }

    // 잔상 페이드 아웃
    IEnumerator FadeAfterImage(GameObject afterImage)
    {
        SpriteRenderer sr = afterImage.GetComponent<SpriteRenderer>();
        float fadeSpeed = 3f;
        float scaleSpeed = 1.2f;
        Vector3 originalScale = afterImage.transform.localScale;

        while (sr.color.a > 0)
        {
            // 알파값 감소
            Color color = sr.color;
            color.a -= fadeSpeed * Time.deltaTime;
            sr.color = color;

            // 약간 스케일 증가 효과
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

    // 보스가 처치되었을 때 호출되는 메서드
    void OnBossDefeated()
    {
        // 여기에 보스 처치 시 특별한 로직을 추가할 수 있습니다
        // 예: 게임 클리어, 특별한 아이템 드롭, 다음 스테이지로 이동 등
        Debug.Log("Boss Defeated!");
    }
}