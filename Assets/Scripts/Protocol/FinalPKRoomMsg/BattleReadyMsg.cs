using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleReadyMsg : BaseMsg
{
    public BattleReady battleReadyData;
    public override int GetBytesNum()
    {
        int num = 8;
        num += battleReadyData.CalculateSize();//获取长度
        return num;
    }
    public override byte[] Writing()
    {
        int index = 0;
        byte[] bytes = new byte[GetBytesNum()];
        WriteInt(bytes, GetID(), ref index);
        WriteInt(bytes, bytes.Length - 8, ref index);
        byte[] bs = ProtobufTool.GetProtoBytes(battleReadyData);
        bs.CopyTo(bytes, index);
        index += bs.Length;
        Debug.Log("发送给服务器PlayerMsg数组长度为" + bytes.Length);
        return bytes;
    }
    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        battleReadyData = ProtobufTool.GetProtoType<BattleReady>(bytes);
        return index - beginIndex;
    }
    public override int Reading(byte[] bytes, int dataBytesLength, int beginIndex = 0)
    {
        int index = beginIndex;
        //需要判断read到多少位
        byte[] bt = new byte[dataBytesLength];
        Array.Copy(bytes, index, bt, 0, dataBytesLength);
        Debug.Log("需要读取位数" + dataBytesLength);
        battleReadyData = ProtobufTool.GetProtoType<BattleReady>(bt);
        return index - beginIndex;
    }
    public override int GetID()
    {
        return 1013;
    }
}
