using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyTools;
using Google.Protobuf;
using System.IO;
using System;
using System.Reflection;

public static class ProtobufTool
{
    //protobuf的工具类负责
    //本地：序列化和反序列化
    //网络上：序列化于反序列化
    /// <summary>
    /// 序列化一个Imessage消息类
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static byte[] GetProtoBytes(IMessage msg)
    {
        //基础写法
        //byte[] bytes = null;
        //using (MemoryStream ms = new MemoryStream())
        //{
        //    msg.WriteTo(ms);
        //    bytes = ms.ToArray();
        //}
        //TextMsg.Parser.ParseFrom
        return msg.ToByteArray();
    }
    public static T GetProtoType<T>(byte[] bytes) where T:class,IMessage
    {
        //使用反射进行对T类型的创建
        Type type = typeof(T);//获取类型
        PropertyInfo info = type.GetProperty("Parser");//获取静态属性
        var obj = info.GetValue(null, null);//获取静态属性对象
        Type parseType = obj.GetType();//获取静态属性的type
        MethodInfo getMethod = parseType.GetMethod("ParseFrom",new Type[] { typeof(byte[])});
        object msg = getMethod.Invoke(obj, new object []{ bytes });
        return msg as T; 
    }
}
