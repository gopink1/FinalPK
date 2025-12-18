using System.Diagnostics;

namespace GamePlayer
{
	public class PlayerMsgHandler : BaseHandler
	{

		public override void MsgHandle()
		{
			PlayerMsg msg = message as PlayerMsg;
			if (msg != null)
			{
				//对消息处理
				//这个消息适用于同步本地的玩家信息的
				//LittleGameManager.Instance.UpdateMClientData(msg);
			}
		}
	}
}
