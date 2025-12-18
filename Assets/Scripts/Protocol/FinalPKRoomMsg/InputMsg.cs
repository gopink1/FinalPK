using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GamePlayer
{
    public class InputMsg : BaseMsg
    {
        public int playerID;
        public InputData data;
        public override int GetBytesNum()
        {
            int num = 8;
            num += 4;
            num += data.CalculateSize();//获取长度
            return num;
        }
        public override byte[] Writing()
        {
            int index = 0;
            byte[] bytes = new byte[GetBytesNum()];
            WriteInt(bytes, GetID(), ref index);
            WriteInt(bytes, bytes.Length - 8, ref index);
            WriteInt(bytes, playerID, ref index);
            byte[] bs = ProtobufTool.GetProtoBytes(data);
            bs.CopyTo(bytes, index);
            index += bs.Length;
            Debug.Log("发送给服务器InputData数组长度为"+bytes.Length);
            return bytes;
        }
        public override int Reading(byte[] bytes, int beginIndex = 0)
        {
            int index = beginIndex;
            playerID = ReadInt(bytes, ref index);
            data = ProtobufTool.GetProtoType<InputData>(bytes);
            return index - beginIndex;
        }
        public override int GetID()
        {
            return 1004;
        }
    }

}
