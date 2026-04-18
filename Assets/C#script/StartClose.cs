using UnityEngine;
using UnityEngine.UI;

public class CloseParentWindow : MonoBehaviour
{
    // 目标窗口的名称（根据实际场景修改，例如"DialogWindow"、"PopupWindow"）
    public string targetWindowName = "DialogWindow";
    public UIBaseCountdown countdown;

    [Header("要打开的另一个UI")]
    public GameObject anotherUI; // 直接拖拽赋值要打开的UI对象（推荐）

     public DotSpawner dotSpawner;

    void Start()
    {
        // 给按钮添加点击事件（如果挂载在Button上）
        Button closeButton = GetComponentInChildren<Button>();
        if (closeButton != null)
        {
            
            closeButton.onClick.AddListener(CloseTargetWindow);
            
        }
    }

    // 查找并关闭目标窗口
    public virtual void CloseTargetWindow()
    {
        // 从当前物体开始，向上查找父类中的目标窗口
        Transform targetWindow = FindParentWindow(transform, targetWindowName);

        if (targetWindow != null)
        {
            // 关闭窗口（示例：禁用窗口）
            targetWindow.gameObject.SetActive(false);
            
            
            countdown.StartCountdown();// // 启动倒计时
            

            OpenAnotherUI();
            dotSpawner.RestartDotSpawn();
        }
        else
        {
            Debug.LogWarning($"未找到名称为{targetWindowName}的父类窗口");
        }
    }

    // 新增：向下查找子物体（核心方法）
    protected virtual Transform FindChildWindow(Transform currentTransform, string windowName)
    {
        // 递归终止条件：当前物体为空 或 目标名称为空
        if (currentTransform == null || string.IsNullOrEmpty(windowName))
            return null;

        // 第一步：检查当前物体的直接子物体
        foreach (Transform child in currentTransform)
        {
            // 找到匹配名称的子物体，直接返回
            if (child.name == windowName)
            {
                return child;
            }

            // 第二步：递归查找当前子物体的子物体（多级子物体）
            Transform grandChild = FindChildWindow(child, windowName);
            if (grandChild != null)
            {
                return grandChild;
            }
        }

        // 遍历完所有子物体都没找到，返回null
        return null;
    }

    // 递归查找父类中名称匹配的窗口
    protected Transform FindParentWindow(Transform currentTransform, string windowName)
    {
        // 递归终止条件：如果当前节点是根节点（没有父节点），返回null
        if (currentTransform == null)
            return null;

        // 检查当前节点是否是目标窗口
        if (currentTransform.name == windowName)
        {
            return currentTransform;
        }
        else
        {
            // 向上查找父节点
            return FindParentWindow(currentTransform.parent, windowName);
        }
    }

    protected void OpenAnotherUI()
    {
        // 方式1：直接通过拖拽赋值的GameObject打开（推荐，更稳定）
        if (anotherUI != null)
        {
            anotherUI.SetActive(true);
            

        }
        

        return;

    }
}