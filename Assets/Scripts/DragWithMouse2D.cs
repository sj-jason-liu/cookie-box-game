using UnityEngine;
using UnityEngine.InputSystem;

public class DragWithMouse2D : MonoBehaviour
{
    private Camera cam;
    private Vector2 dragOffset;
    private bool isDragging = false;
    private Vector2 currentMousePosition;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("無法找到主攝影機！");
            enabled = false;
        }
    }

    void Start()
    {
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError($"物件 {gameObject.name} 需要 Collider2D 組件！");
            enabled = false;
        }
    }

    void Update()
    {
        if (Mouse.current == null || cam == null) return;

        // 獲取當前滑鼠位置
        currentMousePosition = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryStartDrag();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            UpdateDragPosition();
        }
    }

    private void TryStartDrag()
    {
        Vector2 mousePos = currentMousePosition;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            Vector3 objectScreenPosition = cam.WorldToScreenPoint(transform.position);
            dragOffset = currentMousePosition - (Vector2)objectScreenPosition;
        }
    }

    private void UpdateDragPosition()
    {
        Vector3 screenPosition = (Vector3)currentMousePosition - (Vector3)dragOffset;
        screenPosition.z = cam.nearClipPlane;
        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);
        worldPosition.z = transform.position.z; // 保持原始 Z 座標
        transform.position = worldPosition;
    }
}