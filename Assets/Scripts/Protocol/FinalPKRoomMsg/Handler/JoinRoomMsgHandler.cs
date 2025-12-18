using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinRoomMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        JoinRoomMsg msg = message as JoinRoomMsg;
        if (msg != null)
        {
            //对消息处理
        }
    }
}
