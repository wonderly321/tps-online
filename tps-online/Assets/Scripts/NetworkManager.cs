using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public struct Message {
    public int clientFrameID;
    public int executor;
    public char command;
    public String content;
}


public sealed class Client
{
    private Client() {}
    private static Client instance = null;
    public static Client client
    {
        get {
            if(instance == null) instance = new Client();
            return instance;
        }
    }
    internal Boolean socket_ready = false;
    TcpClient tcp_socket;
    NetworkStream net_stream;

    StreamWriter socket_writer;
    StreamReader socket_reader;
    
    private String _host;
    private Int32 _port;
    
    byte[] receiveBuffer = new byte[4096];
    public void setupSocket(String host = "localhost", Int32 port = 5000)
    {
        try
        {
            tcp_socket = new TcpClient(host, port);
            tcp_socket.NoDelay = true;
            net_stream = tcp_socket.GetStream();
            socket_writer = new StreamWriter(net_stream);
            socket_reader = new StreamReader(net_stream);
            socket_ready = true;
            _host = host;
            _port = port;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }

    public void sendMsg(Message msg)
    {
        if(!socket_ready) return;
        if (net_stream.CanWrite)
        {
            try
            {
                byte[] bytes;
                msg_to_bytes(msg, out bytes);
                net_stream.Write(bytes, 0, bytes.Length);
            }
            catch (System.IO.IOException e)
            {
                Debug.Log(e);
            }
        }
    }

    public Message recvMsg()
    {
        Message msg = new Message();
        if (!socket_ready) return msg;
        if (net_stream.DataAvailable) {
            int length = net_stream.Read(receiveBuffer, 0, receiveBuffer.Length);
            if(length > 0) {
                byte[] bytes = new byte[length];
                Array.Copy(receiveBuffer, bytes, length);
                bytes_to_msg(bytes, out msg);
            }
        }
        return msg;
    }
 
    public void closeSocket()
    {
        if (!socket_ready)
            return;

        socket_writer.Close();
        socket_reader.Close();
        tcp_socket.Close();
        socket_ready = false;
    }

    private void msg_to_bytes(Message message, out byte[] ret)
    {
        ret = null;
        try {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, message);
            ret = ms.ToArray();
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    private void bytes_to_msg(byte[] bytes, out Message msg)
    {
        msg = new Message();
        try {
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            msg = (Message)bf.Deserialize(ms);
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    public void writeSocket(byte[] bytes)
    {
        if(socket_ready)
        {
            if (net_stream.CanWrite)
            {
                try
                {
                    net_stream.Write(bytes, 0, bytes.Length);
                }
                catch (System.IO.IOException e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    public void readSocket(ref byte[] bytes)
    {
        if (socket_ready && net_stream.DataAvailable)
        {
            int length = net_stream.Read(receiveBuffer, 0, receiveBuffer.Length);
            bytes = new byte[length];
            Array.Copy(receiveBuffer, bytes, length);
        }
    }
}
