using GamePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        InputMsg msg = message as InputMsg;
    }
}
