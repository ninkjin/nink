using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Return_menu : CloseParentWindow
{

    public GameObject gameprocess;
    public override void CloseTargetWindow()
    {
        // 完全复制父类的逻辑，只删掉countdown.StartCountdown()这一行
        Transform targetWindow = FindParentWindow(transform, targetWindowName);

        if (targetWindow != null)
        {
            // 关闭窗口（保留父类逻辑）
            targetWindow.gameObject.SetActive(false);
            // 删掉：countdown.StartCountdown(); // 屏蔽倒计时启动
            countdown.ResetCountTime();
            // 保留打开另一个UI的逻辑
            OpenAnotherUI();
            

            gameprocess.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"未找到名称为{targetWindowName}的父类窗口");
        }
    }

}
