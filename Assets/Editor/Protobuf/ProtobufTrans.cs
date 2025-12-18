using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
public class ProtobufTrans : MonoBehaviour
{
    private static string PROTO_PATH = "E:\\unity\\URP2021\\FinalPK\\Assets\\Scripts\\ProtobufStudy\\proto";
    private static string PROTOOUT_PATH = "E:\\unity\\URP2021\\FinalPK\\Assets\\Scripts\\ProtobufStudy\\csharp";
    private static string PROTOEXE_PATH = "E:\\unity\\URP2021\\FinalPK\\ProtoBuf\\protoc.exe";
    [MenuItem("Protobuf/Csharp")]
    public static void BornCsharpProtoBuf()
    {
        //获取文件夹
        DirectoryInfo directoryInfo = Directory.CreateDirectory(PROTO_PATH);
        //获取所有文件信息
        FileInfo[] files = directoryInfo.GetFiles();
        //遍历所有的文件生成协议脚本
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Extension == ".proto")
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = PROTOEXE_PATH;
                cmd.StartInfo.Arguments = $"-I={PROTO_PATH} --csharp_out={PROTOOUT_PATH} {files[i].Name}";
                cmd.Start();
                UnityEngine.Debug.Log(files[i].Name +"生成结束");
            }
        }
        UnityEngine.Debug.Log("所有内容都生成完毕");
    }

}
