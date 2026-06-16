using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer sr;

    // ОГРАНИЧЕНИЯ ДВИЖЕНИЯ
    public float minX = -6.5f;
    public float maxX = 6.5f;
    public float minY = -4f;
    public float maxY = -2f;

    private bool isFirstSpawn = true;

    // БЛОКИРОВКА ДВИЖЕНИЯ (ДОБАВИТЬ)
    private bool isMovementBlocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (isFirstSpawn && GameManager.Instance != null)
        {
            isFirstSpawn = false;
        }
    }

    // Метод для блокировки/разблокировки (ДОБАВИТЬ)
    public void SetMovementBlocked(bool blocked)
    {
        isMovementBlocked = blocked;

        // Если заблокировали - останавливаем анимацию движения
        if (blocked)
        {
            animator.SetFloat("Speed", 0f);
            movement = Vector2.zero;
        }
    }

    void Update()
    {
        // НЕ ДВИГАЕМСЯ, если пазл открыт (ДОБАВИТЬ)
        if (isMovementBlocked)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.magnitude > 1)
            movement.Normalize();

        bool isMoving = movement.x != 0 || movement.y != 0;
        animator.SetFloat("Speed", isMoving ? 1f : 0f);

        float direction = 0;
        if (movement.x > 0) direction = 1;
        else if (movement.x < 0) direction = -1;
        animator.SetFloat("Direction", direction);
    }

    void FixedUpdate()
    {
        // НЕ ДВИГАЕМСЯ, если пазл открыт (ДОБАВИТЬ)
        if (isMovementBlocked) return;

        Vector2 newPosition = rb.position + movement * speed * Time.fixedDeltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        rb.MovePosition(newPosition);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0),
                             new Vector3(maxX - minX, maxY - minY, 0));
    }
}