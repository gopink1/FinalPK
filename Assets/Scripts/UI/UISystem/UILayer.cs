using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayer
{
    private int curLayerIndex = 50;
    public void ResetLayer()
    {
        curLayerIndex = 50;
    }

    /// <summary>
    /// 设置页面层级
    /// </summary>
    /// <param name="obj">需要设置层级的页面物体</param>
    public void SetLayer(GameObject obj)
    {
        //获取组件
        Canvas[] canvas = obj.GetComponentsInChildren<Canvas>();
        if (canvas != null)
        {
            //把所有的UI设置层级
            for (int i = 0; i < canvas.Length; i++)
            {
                canvas[i].sortingOrder += curLayerIndex;
            }
        }
        curLayerIndex++;
    }
}
