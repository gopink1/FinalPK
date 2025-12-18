using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReadyMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        PlayerReadyMsg msg = message as PlayerReadyMsg;
        if (msg != null)
        {
            //对消息处理
        }
    }
}
