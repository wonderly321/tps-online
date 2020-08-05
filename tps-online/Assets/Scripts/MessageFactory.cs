using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class MessageFactory{
    public class Utils {
        public static byte[] Encode_Vector3(Vector3 vec) {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((Int32)(vec.x*1000));
            writer.Write((Int32)(vec.y*1000));
            writer.Write((Int32)(vec.z*1000));
            return stream.ToArray();
        }

        public static Vector3 Decode_Vector3(byte[] bytes, int begin) {
            int x = System.BitConverter.ToInt32(bytes, begin);
            int y = System.BitConverter.ToInt32(bytes, begin + 4);
            int z = System.BitConverter.ToInt32(bytes, begin + 8);
            return new Vector3(x*0.001f, y*0.001f, z*0.001f);
        }
    }

    public class GameMsg {
        public int msg_type;
        public int frameID;
        public int selfID;
        public int roomID;
        public byte[] msg;
        public int Length;

        public GameMsg(byte[] msg_with_headers, int begin = 0) {
            msg_type = System.BitConverter.ToInt32(msg_with_headers, begin + 0);
            frameID = System.BitConverter.ToInt32(msg_with_headers, begin + 4);
            selfID = System.BitConverter.ToInt32(msg_with_headers, begin + 8);
            roomID = System.BitConverter.ToInt32(msg_with_headers, begin + 12);
            int msg_len = System.BitConverter.ToInt32(msg_with_headers, begin + 16);
            msg = new byte[msg_len];
            Array.Copy(msg_with_headers, begin + 20, msg, 0, msg_len);
            Length = 20 + msg_len;
        }
    }

    public class ReconnectInfo {
        public int roomID;
        public Vector3 position;
        public int hp;
        public int gun;
        public int bag;
        public int score;
        public ReconnectInfo(byte[] msg, int begin = 0) {
            roomID = System.BitConverter.ToInt32(msg, begin + 0);
            int x = System.BitConverter.ToInt32(msg, begin + 4);
            int y = System.BitConverter.ToInt32(msg, begin + 8);
            int z = System.BitConverter.ToInt32(msg, begin + 12);
            position = new Vector3(x*0.001f, y*0.001f, z*0.001f);
            hp = System.BitConverter.ToInt32(msg, begin + 16);
            gun = System.BitConverter.ToInt32(msg, begin + 20);
            bag = System.BitConverter.ToInt32(msg, begin + 24);
            score = System.BitConverter.ToInt32(msg, begin + 28);

        }
    }

    public static byte[] SetHeaders(int frameID, int selfID, int roomID, byte[] msg)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.MSG_BROADCAST);
        writer.Write(frameID);
        writer.Write(selfID);
        writer.Write(roomID);
        writer.Write(msg.Length);
        writer.Write(msg);
        return stream.ToArray();
    }

    

    public static byte[] SetLoginMsg(string name, string password)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.REQ_LOGIN);
        writer.Write(name.Length);
        writer.Write(password.Length);
        writer.Write(System.Text.Encoding.Default.GetBytes(name));
        writer.Write(System.Text.Encoding.Default.GetBytes(password));
        return stream.ToArray();
    }

    public static byte[] SetRegisterMsg(string name, string password)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.REQ_REGISTER);
        writer.Write(name.Length);      
        writer.Write(password.Length);
        writer.Write(System.Text.Encoding.Default.GetBytes(name));
        writer.Write(System.Text.Encoding.Default.GetBytes(password));
        return stream.ToArray();
    }

    public static byte[] SetCreateRoomMsg(int roomCapacity, int playerID)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.REQ_CREATE_ROOM);
        writer.Write(playerID);
        writer.Write(roomCapacity);
        return stream.ToArray();
    }
    public static byte[] SetJoinRoomMsg(int roomID, int playerID)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.REQ_JOIN_ROOM);
        writer.Write(playerID);
        writer.Write(roomID);
        return stream.ToArray();
    }  
    public static byte[] SetExitRoomMsg(int playerID, int roomID){
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.REQ_EXIT_ROOM);
        writer.Write(playerID);
        writer.Write(roomID);
        return stream.ToArray();
    }
    public static byte[] SetStartGameMsg(int roomID)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.REQ_START_GAME);
        writer.Write(roomID);
        return stream.ToArray();
    }
    public static byte[] SetMoveMsg(byte[] newPos, float X, float Y, byte[] rotation, float y)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.MSG_PLAYER_MOVE);
        writer.Write(newPos);
        writer.Write((int)(X*1000));
        writer.Write((int)(Y*1000));
        writer.Write(rotation);
        writer.Write((int)(y * 1000));
        return stream.ToArray();
    }
    public static byte[] SetShootMsg(int playerID, int bullets, int hurt)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.MSG_PLAYER_SHOOT);
        writer.Write(playerID);
        writer.Write(bullets);
        writer.Write(hurt);
        return stream.ToArray();
    }
    public static byte[] SetReloadMsg()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.MSG_PLAYER_RELOAD);
        return stream.ToArray();
    }
    public static byte[] SetJumpMsg()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(ProtoSettings.MSG_PLAYER_JUMP);
        return stream.ToArray();
    }
}
