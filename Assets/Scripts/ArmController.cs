using System.Collections.Generic;
using UnityEngine;

public class ArmController : MonoBehaviour
{
    public float recordDistance = 10f;  // 間隔距離
    public float snapBackDistance = 15f; // 退回距離

    private List<Vector3> pathPoints = new List<Vector3>();
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // TODO: 滑鼠追蹤邏輯
    }
}