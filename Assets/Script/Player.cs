using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rigid;

    public Vector2 inputVec;
    public Scanner scanner;
    public float speed;
    private bool isFacingRight = false;
    public bool IsFacingRight => isFacingRight; // 외부에서 방향을 읽기용

    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        scanner = GetComponent<Scanner>();
    }

    private void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

        anim.SetBool("1_Move", inputVec.magnitude > 0.01f);

        // 방향 정보만 판단, 실제 Flip()은 자식에서!
        if (inputVec.x < 0) isFacingRight = true;
        else if (inputVec.x > 0) isFacingRight = false;
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;
        GameManager.instance.health -= Time.deltaTime * 10;

        if(GameManager.instance.health < 0 )
        {
            for(int i=3;i<transform.childCount;i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            anim.SetBool("isDeath", true);
            anim.SetTrigger("4_Death");
            GameManager.instance.GameOver();
        }
    }
}
