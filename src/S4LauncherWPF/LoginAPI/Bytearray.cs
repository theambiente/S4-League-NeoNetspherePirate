﻿using S4LauncherWPF.LoginAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using S4LauncherWPF;

namespace S4LauncherWPF.LoginAPI
{
    public class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public Socket WorkSocket;
    }

    internal class LoginClient
    {
        public const short Magic = 0x5713;
        
        public static Socket _netSocket;

        public static bool _connected;

        //not actually needed 
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        private static readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);


        public static void UpdateLabel(string msg)
        {
            Constants.LoginWindow.UpdateLabel(msg);
        }

        public static void Connect(IPEndPoint localEndPoint)
        {
            Constants.LoginWindow.UpdateLabel("Connecting..");
            var sck =
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    LingerState = { Enabled = false }
                };
            _connected = false;
            try
            {
                sck.BeginConnect(localEndPoint, ConnectCallback, sck);
                _netSocket = sck;
                var timer = new Thread(() =>
                {
                    Thread.Sleep(15000);
                    if (!_connected)
                    {
                        sck.Close();
                        Constants.LoginWindow.Reset();
                        Constants.LoginWindow.UpdateLabel("Can´t connect (Server Offline?)");
                    }
                });
                timer.Start();
            }
            catch (Exception e)
            {
                // ignored
#if DEBUG
                Constants.LoginWindow.UpdateLabel($"Error -> {e.ToString()}");
#endif
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;

                client.EndConnect(ar);
                Constants.LoginWindow.UpdateLabel("Connected.");

                ConnectDone.Set();
                var state = new StateObject { WorkSocket = client };

                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                // ignored
#if DEBUG
                Constants.LoginWindow.UpdateLabel($"Error -> {e.ToString()}");
#endif
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    var msg = new CCMessage(state.Buffer, bytesRead);
                    short magic = 0;
                    ByteArray packet = new CCMessage();
                    if (msg.Read(ref magic)
                        && magic == Magic
                        && msg.Read(ref packet))
                    {
                        CCMessage.MessageType coreId = 0;
                        var message = new CCMessage(packet);
                        message.Read(ref coreId);
                        switch (coreId)
                        {
                            case CCMessage.MessageType.Notify:
                                {
                                    var info = "";
                                    message.Read(ref info);
                                    if (info.Contains("<region>") && info.Contains("</region>"))
                                    {
                                        info = info.Replace("<region>", "");
                                        info = info.Replace("</region>", "");
                                        if (info == "Region")
                                        {
                                            var sendmsg = new CCMessage();
                                            sendmsg.Write(Constants.LoginWindow.GetUsername());
                                            sendmsg.Write(Constants.LoginWindow.GetPassword());
                                            sendmsg.Write(FingerPrint.Value());
                                            sendmsg.Write("test");

                                            RmiSend(client, 15, sendmsg);
                                            Constants.LoginWindow.UpdateLabel("Connecting...");
                                        }
                                        else
                                        {
                                            Constants.LoginWindow.UpdateLabel("Error");
                                        }
                                    }

                                    break;
                                }
                            case CCMessage.MessageType.Rmi:
                                {
                                    short rmiId = 0;
                                    if (!message.Read(ref rmiId))
                                        Constants.LoginWindow.UpdateLabel("Received corrupted Rmi message.");
                                    else
                                        switch (rmiId)
                                        {
                                            case 16:
                                                {
                                                    var success = false;
                                                    if (message.Read(ref success) && success)
                                                    {
                                                        ReceiveDone.Set();
                                                        var code = "";
                                                        message.Read(ref code);
                                                        Constants.LoginWindow.UpdateLabel(
                                                              $"");
                                                        Constants.LoginWindow.Ready(code);
                                                        _connected = true;
                                                        var sendmsg = new CCMessage();
                                                        RmiSend(client, 17, sendmsg);
                                                        client.Disconnect(false);
                                                        client.Close();

                                                        Process.Start("S4client.exe", string.Format("-rc:eu -lac:eng -auth_server_ip:{0} -aeria_acc_code:{1}", Constants.ConnectEndPoint.Address, Constants.LoginWindow.AuthCode));
                                                    }
                                                    else
                                                    {
                                                        ReceiveDone.Set();
                                                        var errcode = "";
                                                        message.Read(ref errcode);
                                                        Constants.LoginWindow.UpdateLabel($"Failed: {errcode}");
                                                        Constants.LoginWindow.Reset();
                                                       _connected = true;
                                                        client.Disconnect(false);
                                                        client.Close();
                                                    }
                                                }
                                                break;
                                            default:
                                                Constants.LoginWindow.UpdateLabel($"Received unknown rmiId. {rmiId}");
                                                break;
                                        }
                                    break;
                                }
                            case CCMessage.MessageType.Encrypted:
                                {
                                    break;
                                }
                        }

                        if (!_connected)
                            client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                                ReceiveCallback, state);
                    }
                    else
                    {
                        ReceiveDone.Set();
                        client.Disconnect(true);
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
#if DEBUG
                Constants.LoginWindow.UpdateLabel($"Error -> {e.ToString()}");
#endif
            }
        }


        private static void RmiSend(Socket handler, short rmiId, CCMessage msg)
        {
            var rmiframe = new CCMessage();
            rmiframe.Write(CCMessage.MessageType.Rmi);
            rmiframe.Write(rmiId);
            rmiframe.Write(msg);
            Send(handler, rmiframe);
        }

        private static void Send(Socket handler, CCMessage data)
        {
            try
            {
                var coreframe = new CCMessage();
                coreframe.Write(Magic);
                coreframe.WriteScalar(data.Length);
                coreframe.Write(data);
                handler.BeginSend(coreframe.Buffer, 0, coreframe.WriteOffset, 0,
                    SendCallback, handler);
            }
            catch (Exception ex)
            {
                // ignored
#if DEBUG
                Constants.LoginWindow.UpdateLabel($"Error -> {ex.ToString()}");
#endif
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                SendDone.Set();
            }
            catch (Exception e)
            {
                // ignored
#if DEBUG
                Constants.LoginWindow.UpdateLabel($"Error -> {e.ToString()}");
#endif
            }
        }
    }
}
