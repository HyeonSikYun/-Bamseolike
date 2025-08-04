using UnityEngine;

public class Flip : MonoBehaviour
{
    [SerializeField] private Player player;

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = player.IsFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}
