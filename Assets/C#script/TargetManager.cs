using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    [Header("目标设置")]
    public GameObject targetPrefab; // 目标预制体
    public int totalTargets = 5; // 固定总数
    public float moveSpeedMin = 100f; // 最小移动速度
    public float moveSpeedMax = 300f; // 最大移动速度
    public float changeDirIntervalMin = 0.5f; // 最小转向间隔
    public float changeDirIntervalMax = 2f; // 最大转向间隔

    private List<GameObject> targets = new List<GameObject>(); // 存储所有目标
    private Camera mainCamera;
    private Vector2 cameraBoundsMin; // 摄像机左下角边界
    private Vector2 cameraBoundsMax; // 摄像机右上角边界

    void Start()
    {
        mainCamera = Camera.main;
        // 计算摄像机视角边界（2D正交相机）
        CalculateCameraBounds();
        // 生成固定数量的目标
        SpawnTargets();
    }

    // 计算摄像机在世界空间中的可视范围
    void CalculateCameraBounds()
    {
        // 正交相机的高度 = 2 * orthographicSize
        float cameraHeight = mainCamera.orthographicSize * 2f;
        // 宽度 = 高度 * 屏幕宽高比
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // 计算边界（以摄像机位置为中心）
        cameraBoundsMin = new Vector2(
            mainCamera.transform.position.x - cameraWidth / 2f,
            mainCamera.transform.position.y - cameraHeight / 2f
        );
        cameraBoundsMax = new Vector2(
            mainCamera.transform.position.x + cameraWidth / 2f,
            mainCamera.transform.position.y + cameraHeight / 2f
        );
    }

    // 生成固定数量的目标
    void SpawnTargets()
    {
        for (int i = 0; i < totalTargets; i++)
        {
            // 随机生成位置（在摄像机边界内）
            Vector2 randomPos = new Vector2(
                Random.Range(cameraBoundsMin.x, cameraBoundsMax.x),
                Random.Range(cameraBoundsMin.y, cameraBoundsMax.y)
            );

            // 实例化目标
            GameObject target = Instantiate(targetPrefab, randomPos, Quaternion.identity);
            targets.Add(target);

            // 给目标添加随机移动逻辑
            TargetMovement movement = target.AddComponent<TargetMovement>();
            movement.Initialize(
                Random.Range(moveSpeedMin, moveSpeedMax),
                Random.Range(changeDirIntervalMin, changeDirIntervalMax),
                cameraBoundsMin,
                cameraBoundsMax
            );
        }
    }
}