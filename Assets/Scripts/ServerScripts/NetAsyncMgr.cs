using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetAsyncMgr : MonoBehaviour
{
    private static NetAsyncMgr instance;
    public static NetAsyncMgr Instance => instance;

    //和服务器进行连接的 Socket
    private Socket socket;
    public bool IsConnected { get; private set; }
    //接受消息用的 缓存容器
    private byte[] cacheBytes = new byte[1024 * 1024];
    private int cacheNum = 0;

    private Queue<BaseHandler> receiveQueue = new Queue<BaseHandler>();

    MsgPool msgPool = new MsgPool();
    //发送心跳消息的间隔时间
    private int SEND_HEART_MSG_TIME = 2;
    private HeartMsg hearMsg = new HeartMsg();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        //过场景不移除
        DontDestroyOnLoad(this.gameObject);
        //客户端循环定时给服务端发送心跳消息
        InvokeRepeating("SendHeartMsg", 0, SEND_HEART_MSG_TIME);
    }

    private void SendHeartMsg()
    {
        if (socket != null && socket.Connected)
            Debug.Log("客户端发送心跳消息");
            Send(hearMsg);
    }

    // Update is called once per frame
    void Update()
    {
        if (receiveQueue.Count > 0)
        {
            BaseHandler baseHander = receiveQueue.Dequeue();
            baseHander.MsgHandle();
        }
    }

    //连接服务器的代码
    public void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
            return;

        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true; // 客户端也要禁用

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = ipPoint;
        args.Completed += (socket, args) =>
        {
            if(args.SocketError == SocketError.Success)
            {
                print("连接成功");
                //收消息
                IsConnected = true;
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(cacheBytes, 0, cacheBytes.Length);
                receiveArgs.Completed += ReceiveCallBack;
                this.socket.ReceiveAsync(receiveArgs);
            }
            else
            {
                print("连接失败" + args.SocketError);
            }
        };
        socket.ConnectAsync(args);
    }

    //收消息完成的回调函数
    private void ReceiveCallBack(object obj, SocketAsyncEventArgs args)
    {
        if(args.SocketError == SocketError.Success)
        {
            HandleReceiveMsg(args.BytesTransferred);
            //继续去收消息
            args.SetBuffer(cacheNum, args.Buffer.Length - cacheNum);
            //继续异步收消息
            if (this.socket != null && this.socket.Connected)
                socket.ReceiveAsync(args);
            else
                Close();
        }
        else
        {
            print("接受消息出错" + args.SocketError);
            //关闭客户端连接
            Close();
        }
    }

    public void Close()
    {
        if(socket != null)
        {
            QuitMsg msg = new QuitMsg();
            socket.Send(msg.Writing());
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
            print("断开连接");
            IsConnected = false;
            socket = null;
        }
    }

    public void Send(BaseMsg msg)
    {
        if(this.socket != null && this.socket.Connected)
        {
            byte[] bytes = msg.Writing();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.Completed += (socket, args) =>
            {
                if (args.SocketError != SocketError.Success)
                {
                    print("发送消息失败" + args.SocketError);
                    Close();
                }
            };
            this.socket.SendAsync(args);
        }
        else
        {
            Close();
        }
    }

    //处理接受消息 分包、黏包问题的方法
    private void HandleReceiveMsg(int receiveNum)
    {
        int msgID = 0;
        int msgLength = 0;
        int nowIndex = 0;

        cacheNum += receiveNum;

        while (true)
        {
            //每次将长度设置为-1 是避免上一次解析的数据 影响这一次的判断
            msgLength = -1;
            //处理解析一条消息
            if (cacheNum - nowIndex >= 8)
            {
                //解析ID
                msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
                //解析长度
                msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
            }

            if (cacheNum - nowIndex >= msgLength && msgLength != -1)
            {

                //解析消息体
                BaseMsg baseMsg = null;
                BaseHandler hander = null;

                //通过msgpool 怎么进行
                baseMsg = msgPool.GetMessage(msgID);
                hander = msgPool.GetHandler(msgID);
                if (baseMsg != null && hander != null)
                {
                    // ================== 【核心修改开始】 ==================

                    // 2. 【立即解析】！不要等到Update！
                    // 此时 cacheBytes 还是正确的数据
                    baseMsg.Reading(cacheBytes, msgLength, nowIndex);

                    // 3. 将解析好的完整消息赋值给 Handler
                    hander.message = baseMsg;

                    // 注意：下面这几行赋值已经不需要了，因为消息已经解析完了

                    // 4. 入队
                    receiveQueue.Enqueue(hander);
                    // ================== 【核心修改结束】 ==================
                }
                else
                {
                    Debug.LogError($"未找到ID为 {msgID} 的消息或Handler，请检查MsgPool注册");
                }
                nowIndex += msgLength;
                if (nowIndex == cacheNum)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else
            {
                if (msgLength != -1)
                    nowIndex -= 8;
                //就是把剩余没有解析的字节数组内容 移到前面来 用于缓存下次继续解析
                Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);

                cacheNum = cacheNum - nowIndex;
                break;
            }
        }

    }

    private void OnDestroy()
    {
        Close();
    }
}
