using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllInputMsg : BaseMsg
{
    public int playerID;
    public AllInputData data;
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
        Debug.Log("发送给服务器InputData数组长度为" + bytes.Length);
        return bytes;
    }
    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        playerID = ReadInt(bytes, ref index);
        data = ProtobufTool.GetProtoType<AllInputData>(bytes);
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

        // 解析
        try
        {
            data = ProtobufTool.GetProtoType<AllInputData>(bt);
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析失败：{ex.Message}，截取长度={dataBytesLength}，beginIndex={beginIndex}");
        }
        //data = ProtobufTool.GetProtoType<AllInputData>(bt);
        return index - beginIndex;
    }
    public override int GetID()
    {
        return 1005;
    }
}
