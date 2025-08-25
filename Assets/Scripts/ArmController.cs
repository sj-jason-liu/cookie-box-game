using System.Collections.Generic;
using UnityEngine;

public class ArmController : MonoBehaviour
{
    [Header("手臂設定")]
    public GameObject armSegmentPrefab; // 手臂節點預製體
    public GameObject handPrefab;       // 手部預製體
    public float segmentSize = 0.5f;    // 節點大小
    public float minDistance = 0.3f;    // 最小節點間距
    public float maxExtendSpeed = 5f;   // 最大延伸速度

    [Header("抖動效果")]
    public bool enableShake = true;
    public float shakeStrength = 0.02f;
    public float shakeSpeed = 10f;

    private List<Transform> armSegments = new List<Transform>();
    private Transform handTransform;
    private Vector3 lastMousePosition;
    private Camera mainCamera;
    private bool isExtending = false;
    private Vector3 currentTip; // 當前手臂尖端位置

    // 碰撞檢測
    private List<Vector2> occupiedPositions = new List<Vector2>();

    void Start()
    {
        mainCamera = Camera.main;

        // 創建初始手部
        CreateInitialHand();
    }

    void Update()
    {
        HandleMouseInput();

        if (enableShake)
        {
            ApplyShakeEffect();
        }
    }

    void CreateInitialHand()
    {
        if (handPrefab == null)
        {
            Debug.LogError("handPrefab 是空的！請在Inspector中指定手部預製體");
            return;
        }

        // 在螢幕頂部中央創建手部
        Vector3 startPos = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.9f, 10f));
        startPos.z = 0f;

        GameObject hand = Instantiate(handPrefab, startPos, Quaternion.identity);
        handTransform = hand.transform;
        currentTip = startPos;

        // 記錄初始位置
        occupiedPositions.Add(new Vector2(startPos.x, startPos.y));

        Debug.Log($"成功創建手部於位置: {startPos}");
    }

    void HandleMouseInput()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            isExtending = true;
            lastMousePosition = mouseWorldPos;
        }

        if (Input.GetMouseButton(0) && isExtending)
        {
            // 計算滑鼠移動距離和速度
            Vector3 mouseDelta = mouseWorldPos - lastMousePosition;
            float mouseSpeed = mouseDelta.magnitude / Time.deltaTime;

            // 限制延伸速度
            float extendSpeed = Mathf.Clamp(mouseSpeed, 0f, maxExtendSpeed);

            if (mouseDelta.magnitude > 0.1f) // 避免微小抖動
            {
                ExtendArm(mouseWorldPos, extendSpeed);
            }

            lastMousePosition = mouseWorldPos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isExtending = false;
        }

        // 右鍵縮回手臂
        if (Input.GetMouseButton(1))
        {
            RetractArm();
        }
    }

    void ExtendArm(Vector3 targetPos, float speed)
    {
        Vector3 direction = (targetPos - currentTip).normalized;
        float distanceToMove = speed * Time.deltaTime;

        Vector3 newTipPos = currentTip + direction * distanceToMove;

        Debug.Log($"ExtendArm: CurrentTip={currentTip}, Target={targetPos}, NewTip={newTipPos}, Distance={Vector3.Distance(currentTip, newTipPos)}");

        // 檢查是否會碰到自己
        if (CheckSelfCollision(newTipPos))
        {
            Debug.Log("不能碰到自己的手臂！");
            return;
        }

        // 大幅降低節點間距需求進行測試
        float testMinDistance = 0.1f;
        float actualDistance = Vector3.Distance(currentTip, newTipPos);

        Debug.Log($"實際距離: {actualDistance}, 需要距離: {testMinDistance}");

        // 檢查是否需要添加新節點
        if (actualDistance >= testMinDistance)
        {
            CreateNewSegment(newTipPos);
            currentTip = newTipPos;
            Debug.Log($"創建新節點於: {newTipPos}, 總節點數: {armSegments.Count}");
        }
        else
        {
            Debug.Log("距離不足，不創建新節點");
        }
    }

    void CreateNewSegment(Vector3 position)
    {
        if (armSegmentPrefab == null)
        {
            Debug.LogError("armSegmentPrefab 是空的！請在Inspector中指定預製體");
            return;
        }

        GameObject newSegment = Instantiate(armSegmentPrefab, position, Quaternion.identity);
        armSegments.Add(newSegment.transform);

        // 記錄佔用位置
        occupiedPositions.Add(new Vector2(position.x, position.y));

        // 設定父物件以便管理
        newSegment.transform.SetParent(transform);

        Debug.Log($"成功創建節點: {newSegment.name} 於位置 {position}");
    }

    void RetractArm()
    {
        if (armSegments.Count > 0)
        {
            // 移除最後一個節點
            Transform lastSegment = armSegments[armSegments.Count - 1];
            armSegments.RemoveAt(armSegments.Count - 1);

            // 移除佔用位置記錄
            if (occupiedPositions.Count > 1) // 保留手部位置
            {
                occupiedPositions.RemoveAt(occupiedPositions.Count - 1);
            }

            Destroy(lastSegment.gameObject);

            // 更新當前尖端位置
            if (armSegments.Count > 0)
            {
                currentTip = armSegments[armSegments.Count - 1].position;
            }
            else
            {
                currentTip = handTransform.position;
            }
        }
    }

    bool CheckSelfCollision(Vector3 newPosition)
    {
        foreach (Vector2 occupiedPos in occupiedPositions)
        {
            if (Vector2.Distance(new Vector2(newPosition.x, newPosition.y), occupiedPos) < segmentSize * 0.8f)
            {
                return true;
            }
        }
        return false;
    }

    void ApplyShakeEffect()
    {
        float time = Time.time * shakeSpeed;

        // 對每個手臂節點應用輕微抖動
        for (int i = 0; i < armSegments.Count; i++)
        {
            if (armSegments[i] != null)
            {
                Vector3 originalPos = occupiedPositions[i + 1]; // +1 因為第0個是手部位置
                Vector3 shakeOffset = new Vector3(
                    Mathf.Sin(time + i * 0.5f) * shakeStrength,
                    Mathf.Cos(time * 1.3f + i * 0.3f) * shakeStrength,
                    0f
                );
                armSegments[i].position = originalPos + shakeOffset;
            }
        }

        // 手部也輕微抖動
        if (handTransform != null)
        {
            Vector3 originalPos = occupiedPositions[0];
            Vector3 shakeOffset = new Vector3(
                Mathf.Sin(time * 0.8f) * shakeStrength * 0.5f,
                Mathf.Cos(time * 1.1f) * shakeStrength * 0.5f,
                0f
            );
            handTransform.position = originalPos + shakeOffset;
        }
    }

    // 獲取當前手臂尖端位置（供其他系統使用）
    public Vector3 GetTipPosition()
    {
        return currentTip;
    }

    // 重置手臂（新關卡時使用）
    public void ResetArm()
    {
        // 清除所有節點
        foreach (Transform segment in armSegments)
        {
            if (segment != null)
                Destroy(segment.gameObject);
        }

        armSegments.Clear();
        occupiedPositions.Clear();

        // 重新創建手部
        if (handTransform != null)
            Destroy(handTransform.gameObject);

        CreateInitialHand();
    }
}