using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
录屏路径：`Assets/Q2/CandyCrush.MP4`

请仔细观察根目录中提供的知名消除游戏 Candy Crush 录屏中，选关界面对话框 Play 按钮的动画效果，请复刻这一效果，使用代码实现或者 Animation 均可，动画包括：
1. 按钮出现
2. 按钮按下
3. 按钮弹起
*/

public class Q2 : MonoBehaviour
{
    [SerializeField]
    private Button button = null; 

    public void OnShowBtnClick()
    {
        // TODO: 请在此处开始作答
        if (state == 1)
        {
            state = 0;
            button.transform.localScale = baseScale;
        }
        else
        {
            state = 1;
            button.transform.GetComponent<Image>().color = Color.white;
        }
    }

    public void OnTouchDownBtnClick()
    {
        // TODO: 请在此处开始作答
        if (state == 2)
        {
            state = 0;
            button.transform.localScale = baseScale;
            button.transform.GetComponent<Image>().color = Color.white;
        }
        else
        {
            state = 2;
            bounceStartTime = Time.time;
            button.transform.GetComponent<Image>().color = Color.grey;
        }
    }

    public void OnTouchUpBtnClick()
    {
        // TODO: 请在此处开始作答
        if (state == 3)
        {
            state = 0;
            button.transform.localScale = baseScale;
        }
        else
        {
            state = 3;
            bounceStartTime = Time.time;
            button.transform.GetComponent<Image>().color = Color.white;
        }
    }

    private Vector2 baseScale;
    const float speed = Mathf.PI*2.5f;
    const float amplitude = 0.03f;
    const int bounceTimes = 5;
    const float bounceScale = 0.7f;
    const float touchScale = 0.8f;
    const float touchScaleTime = 0.1f;
    private float bounceStartTime;
    private int state = 0;

    private void Awake()
    {
        baseScale = button.transform.localScale;
    }

    private void Update()
    {
        var totalScale = baseScale;
        if (state == 1 || state == 2 || state == 3)
        {
            float sin = Mathf.Sin(speed * Time.time);
            var showScaleX = 1 + amplitude * sin;
            var showScaleY = 1 - amplitude * sin;
            totalScale.x *= showScaleX;
            totalScale.y *= showScaleY;
        }
        if (state == 2)
        {
            totalScale *= GetBounceScale(1f, touchScale, Time.time - bounceStartTime);
        }
        else if (state == 3)
        {
            totalScale *= GetBounceScale(touchScale, 1f, Time.time - bounceStartTime);
        }
        button.transform.localScale = totalScale;
    }

    private float GetBounceScale(float startScale,float targetScale,float durTime)
    {
        var offset = startScale - targetScale;
        durTime += touchScaleTime;
        var scaleTime = touchScaleTime * 2;
        var isBouncing = false;
        for (var i = 0; i < bounceTimes; i++)
        {
            if (durTime <= scaleTime)
            {
                float scale = 1f;
                if (durTime < scaleTime / 2f)
                {
                    scale = targetScale + durTime / (scaleTime / 2f) * offset;
                }
                else
                {
                    scale = targetScale + offset - (durTime / (scaleTime / 2f) -1) * offset;
                }
                return scale;
            }
            durTime -= scaleTime;
            scaleTime *= bounceScale;
            offset *= bounceScale;
        }
        return targetScale;
    }
}
