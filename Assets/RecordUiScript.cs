using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 必须加，否则Button/Transform相关会报错

public class RecordUiScript : CloseParentWindow
{
    [Header("子UI配置")]
    public string targetChildWindowName = "RecordPanel"; // 手动填要找的子UI名称（比如你的记录面板名）
    public GameObject recordui; // 拖拽要显示的目标UI（隐形的那个）

    

    void Start()
    {

        
        // 重新绑定按钮点击事件（防止父类绑定失效，双重保险）
        Button clickButton = GetComponent<Button>();
        if (clickButton != null)
        {
            // 按钮点击直接调用自定义的“打开子UI”方法，逻辑更清晰
            clickButton.onClick.AddListener(OpenTargetChildWindow);
        }
        else
        {
            Debug.LogError("脚本未挂载在Button上，无法绑定点击事件！");
        }

        
    }

    // 自定义：按钮点击后执行的核心逻辑（打开子UI）
    private void OpenTargetChildWindow()
    {
        // 1. 调用父类的FindChildWindow查找目标子UI（验证查找是否成功）
        Transform childWindow = base.FindChildWindow(transform, targetChildWindowName);

        // 2. 查找成功 → 显示UI；失败 → 提示（但仍尝试显示手动赋值的recordui）
        if (childWindow != null)
        {
            
            recordui.SetActive(true);
        }
        else
        {
            
            // 即使查找失败，只要recordui赋值了，仍显示（容错）
            if (recordui != null)
            {
                recordui.SetActive(true);
                
            }
        }
    }

    

    
}