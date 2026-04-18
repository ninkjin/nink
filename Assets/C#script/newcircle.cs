using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHole : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("覆盖检测参数")]
    public Vector2 checkSize = new Vector2(50f, 50f); // 检测范围（适配UI单位，和方框大小匹配）
    public LayerMask redDotLayer; // 红点所在层（需提前创建）

    // 对Hole自身RectTransform的缓存
    private RectTransform rectTransform;
    private List<DotMovement> coveredDots = new List<DotMovement>(); // 存储当前覆盖的红点

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // 实现IDragHandler接口，当拖拽时自动调用
    public void OnDrag(PointerEventData eventData)
    {
        // 将屏幕坐标转换为Hole父物体（MaskPanel）的本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        // 更新Hole的位置
        rectTransform.localPosition = localPoint;

        CheckCoveredRedDots();
    }

    private void CheckCoveredRedDots()
    {
        ClearAllCoveredDots();

        // 修复：改用世界坐标检测（替换原有anchoredPosition）
        Vector2 boxWorldPos = rectTransform.TransformPoint(Vector2.zero);
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            boxWorldPos, // 世界坐标（精准匹配视觉位置）
            checkSize,
            0f,
            redDotLayer
        );

       

        coveredDots.Clear();
        foreach (var collider in hitColliders)
        {
            DotMovement redDot = collider.GetComponent<DotMovement>();
            if (redDot != null)
            {
                redDot.SetCovered(true);
                coveredDots.Add(redDot);
                
            }
        }
    }

    private void ClearAllCoveredDots()
    {
        foreach (var dot in coveredDots)
        {
            if (dot != null)
            {
                dot.SetCovered(false);
                
            }
        }
        coveredDots.Clear();
    }

    // 新增：按下鼠标时开始检测
    public void OnPointerDown(PointerEventData eventData)
    {
       
        CheckCoveredRedDots();
    }

    // 新增：松开鼠标时停止检测
    public void OnPointerUp(PointerEventData eventData)
    {
        ClearAllCoveredDots();
        
    }
}