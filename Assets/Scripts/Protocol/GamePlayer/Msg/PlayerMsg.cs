using System;
using UnityEngine;
namespace GamePlayer
{
    /// <summary>
    /// 玩家连接消息
    /// 用于玩家首次连接，传输位置消息
    /// </summary>
    public class PlayerMsg : BaseMsg
    {
        public int playerID;
        public PlayerData data;
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
            Debug.Log("发送给服务器PlayerMsg数组长度为" + bytes.Length);
            return bytes;
        }
        public override int Reading(byte[] bytes, int beginIndex = 0)
        {
            int index = beginIndex;
            playerID = ReadInt(bytes, ref index);
            data = ProtobufTool.GetProtoType<PlayerData>(bytes);
            return index - beginIndex;
        }
        public override int Reading(byte[] bytes, int dataBytesLength, int beginIndex = 0)
        {

            int index = beginIndex;
            playerID = ReadInt(bytes, ref index);

            Console.WriteLine(bytes.Length - beginIndex);

            //需要判断read到多少位
            byte[] bt = new byte[dataBytesLength - 4];
            Array.Copy(bytes, index, bt, 0, dataBytesLength-4);
            Debug.Log("需要读取位数" + dataBytesLength);
            data = ProtobufTool.GetProtoType<PlayerData>(bt);
            return index - beginIndex;
        }
        public override int GetID()
        {
            return 1001;
        }
    }
}