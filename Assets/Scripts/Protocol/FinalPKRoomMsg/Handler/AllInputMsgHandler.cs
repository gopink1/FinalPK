using GamePlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllInputMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        AllInputMsg msg = message as AllInputMsg;
        if (msg != null)
        {
            //对消息处理
            //这个消息适用于同步本地的玩家信息的
            //调用reading方法进行消息的解读
            //执行接收到的全局真同步消息
            //LittleGameManager.Instance.UpdateFrame(msg);
            Debug.Log("接收到逻辑帧消息" + msg.data.GlobalFrame);
            RoomNetSystem.MainInstance.OnReceiveFrameInput(msg);
        }
    }
}
