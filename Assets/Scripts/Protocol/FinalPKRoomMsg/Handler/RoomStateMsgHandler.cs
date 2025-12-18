using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomStateMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        RoomStateMsg msg = message as RoomStateMsg;
        if (msg != null)
        {
            //对消息处理
            RoomNetSystem.MainInstance.OnRoomStateReceived(msg);
        }
    }
}
