using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour
{
    bool isLive = true;
    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    WaitForFixedUpdate wait;
    SortingGroup sortingGroup;


    public Rigidbody2D target;
    //public RuntimeAnimatorController[] animCon;
    public float speed;
    public float health;
    public float maxHealth;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        wait = new WaitForFixedUpdate();
        sortingGroup = GetComponent<SortingGroup>();
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("DAMAGED"))
            return;
            

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        if (dirVec.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }
        else if (dirVec.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1); 
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
        sortingGroup.sortingOrder = 2;
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        //anim.runtimeAnimatorController = animCon[data.enemyType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive) 
            return;

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());
                
        if(health>0)
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
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
            
        }

    }

    IEnumerator KnockBack()
    {
        yield return wait;

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

        // 자식 오브젝트의 SpriteRenderer들 찾기
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        float targetAlpha = 0.8f;  // 살짝만 줄일 알파값
        float originalAlpha = 1.0f;
        float fadeTime = 0.09f;     // 짧은 시간에 알파 전환

        // 알파 줄이기
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = targetAlpha;
            sr.color = c;
        }

        yield return new WaitForSeconds(fadeTime);

        // 다시 원래대로 복원
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = originalAlpha;
            sr.color = c;
        }
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
}
