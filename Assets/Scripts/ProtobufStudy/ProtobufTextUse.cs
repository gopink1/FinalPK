using GamePlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtobufTextUse : MonoBehaviour
{
    TextMsg textMsg;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.V)) {

            //≥¢ ‘∑¢ÀÕPlayerMsg
            PlayerData data = new PlayerData();
            Pos3 pos = new Pos3();
            pos.X = 5;
            pos.Y = 0.5f;
            pos.Z = 5;
            data.Position = pos;
            data.Id = 3;
            data.Name = "mike";

            PlayerMsg msg = new PlayerMsg();
            msg.playerID = 3;
            msg.data = data;
            byte[] bs = ProtobufTool.GetProtoBytes(data);

            Debug.Log(bs.Length);
            PlayerData obj = ProtobufTool.GetProtoType<PlayerData>(bs);
            Debug.Log(obj.Id);

            msg.Reading(bs);
        }
    }
}
