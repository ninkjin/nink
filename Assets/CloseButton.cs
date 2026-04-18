using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CloseButton : CloseParentWindow
{
    public override void CloseTargetWindow()
    {
        // 完全复制父类的逻辑，只删掉countdown.StartCountdown()这一行
        Transform targetWindow = FindParentWindow(transform, targetWindowName);

        if (targetWindow != null)
        {
            
            targetWindow.gameObject.SetActive(false);
            

            
        }
        else
        {
            Debug.LogWarning($"未找到名称为{targetWindowName}的父类窗口");
        }
    }
}
