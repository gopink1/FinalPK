using GamePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllPlayerMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        AllPlayerMsg msg = message as AllPlayerMsg;
        if (msg != null)
        {
            //对消息处理
            //这个消息适用于同步本地的玩家信息的
            //LittleGameManager.Instance.UpdateAllPlayerData(msg);
        }
    }
}
