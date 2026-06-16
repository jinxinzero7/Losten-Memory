using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;           // Игрок
    public float smoothSpeed = 0.05f;  // Плавность
    public Vector3 offset;             // Смещение камеры (X=0, Y=0, Z=-10)

    // Границы камеры
    public float leftBound;
    public float rightBound;
    public float bottomBound;
    public float topBound;

    private float cameraHalfWidth;
    private float cameraHalfHeight;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cameraHalfHeight = cam.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * cam.aspect;

        // Если target не назначен вручную, ищем игрока
        FindPlayer();
    }

    void LateUpdate()
    {
        // Если игрока нет, пробуем найти
        if (target == null)
        {
            FindPlayer();
            if (target == null) return;
        }

        // Желаемая позиция камеры (за игроком)
        float desiredX = target.position.x + offset.x;
        float desiredY = target.position.y + offset.y;

        // Ограничиваем камеру границами уровня
        float clampedX = Mathf.Clamp(desiredX,
            leftBound + cameraHalfWidth,
            rightBound - cameraHalfWidth);

        float clampedY = Mathf.Clamp(desiredY,
            bottomBound + cameraHalfHeight,
            topBound - cameraHalfHeight);

        Vector3 desiredPosition = new Vector3(clampedX, clampedY, offset.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("Камера нашла игрока!");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(leftBound, topBound, 0), new Vector3(rightBound, topBound, 0));
        Gizmos.DrawLine(new Vector3(leftBound, bottomBound, 0), new Vector3(rightBound, bottomBound, 0));
        Gizmos.DrawLine(new Vector3(leftBound, topBound, 0), new Vector3(leftBound, bottomBound, 0));
        Gizmos.DrawLine(new Vector3(rightBound, topBound, 0), new Vector3(rightBound, bottomBound, 0));
    }
}