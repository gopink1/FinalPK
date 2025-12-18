

public class GameStartMsgHandler : BaseHandler
{
    public override void MsgHandle()
    {
        GameStartMsg msg = message as GameStartMsg;
        if (msg != null)
        {
            //对消息处理
            RoomNetSystem.MainInstance.OnGameStartReceived(msg);
        }
    }
}
