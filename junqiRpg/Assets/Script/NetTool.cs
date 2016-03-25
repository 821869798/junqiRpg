using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using Pathfinding.Serialization.JsonFx;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

public class NetTool : MonoBehaviour
{

	//网络部分
	private Socket clientSocket;
	private bool isReceive = false;
	private int responseTime = 0;
	private int netLost = 0;
	public static NetTool instance;
	private Thread sendT;
	private Thread receiveT;
	private Queue<string> msgList = new Queue<string> ();

	void Start ()
	{
		initNet ();
		instance = this;
	}

	private void initNet ()
	{
		try {
			IPAddress ip = IPAddress.Parse (Config.getInstance().IPConfig);
			this.clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			clientSocket.Connect (new IPEndPoint (ip, 6771)); //将套接字与远程服务器地址相连
			SendData data = new SendData ();
			data.type = 0;
			data.name = Config.getInstance ().name;
			string jsonStr = JsonWriter.Serialize (data);
			clientSocket.Send (Encoding.UTF8.GetBytes (jsonStr));

			receiveT = new Thread (ReceiveThread);
			receiveT.Start (clientSocket);
			receiveT.IsBackground = true;
			//clientSocket.Send (Encoding.UTF8.GetBytes ("连上了吧"));

			sendT = new Thread (sendThread);
			sendT.Start ();
			sendT.IsBackground = true;
		} catch (Exception e) {
			netLost = 1;
			clientSocket.Close ();
		}


	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isReceive) {
			isReceive = false;
			while (msgList.Count>0) {
				dealData (msgList.Dequeue ());
			}
		}
		if (netLost == 1) {
			print ("断网");
			MessageTool.instance.AddItem ("[系统消息]:网络已中断，请重进", 1);
			netLost ++;
		}
	}

	private void dealData (string str)
	{
		str = Regex.Unescape (str);
		print (str);
		SendData data = JsonReader.Deserialize<SendData> (str);
		if (data.type == 10) {
			string itemText = "[" + data.name + "说]:" + data.text;
			print (itemText);
			MessageTool.instance.AddItem (itemText, 0);
		} else if (data.type == 1) {
			MessageTool.instance.setOpponentName (data.name);
			string itemText = "[系统消息]:" + data.name + "已经进入房间";
			MessageTool.instance.AddItem (itemText, 1);
			BoardManage.instance.currentType = 1;
		} else if (data.type == 2) {
			BoardManage.instance.addEnemyChess ();
		} else if (data.type == 3) {
			if (data.which == 1) {
				BoardManage.instance.addEnemyChess ();
				BoardManage.instance.AbleToChess (true);
				MessageTool.instance.AddItem ("[系统消息]:比赛开始，你先手", 1);
			} else {
				MessageTool.instance.AddItem ("[系统消息]:比赛开始，对方先手", 1);
			}
		} else if (data.type == 20) {
			if (data.result >= 0) {
				if (data.result == 0) {
					MessageTool.instance.AddItem ("[系统消息]:比赛结束，和棋", 1);
				} else if (data.result == 1) {
					MessageTool.instance.AddItem ("[系统消息]:比赛结束，你赢了", 1);
				} else if (data.result == 2) {
					MessageTool.instance.AddItem ("[系统消息]:比赛结束，你输了", 1);
				} else {
					MessageTool.instance.AddItem ("[系统消息]:对方掉线，你赢了", 1);
				}
			}
			if(data.fromP==null||data.toP==null)
				return;
			if (data.which == 1) {
				BoardManage.instance.AbleToChess (true);				
			}
			BoardManage.instance.setMaskCheck (data.fromP, data.toP);
			if (data.tempP != null) {
				BoardManage.instance.showOpponent40 (data.tempP);
			}
			if (data.isAble == 1) {
				BoardManage.instance.playChess (data.fromP, data.toP);
			} else if (data.isAble == -1) {
				BoardManage.instance.noPlayChess (data.fromP);
			} else {
				BoardManage.instance.togetherChessDead (data.fromP, data.toP);
			}

		} else if (data.type == 50) {
			if (data.result > 0) {
				if (data.result == 1) {
					MessageTool.instance.AddItem ("[系统消息]:" + data.name + "跳过5次，你赢了", 1);
				} else {
					MessageTool.instance.AddItem ("[系统消息]:你跳过5次，你输了", 1);
				}
			} else {
				if (data.which == 1) {
					BoardManage.instance.AbleToChess (true);				
				}
				MessageTool.instance.AddItem ("[系统消息]:" + data.name + "跳过了" + data.isAble + "次", 1);
			}
		} else if (data.type == 40) {
			MessageTool.instance.drawCount = data.result;
			MessageTool.instance.infoType = 0;
			MessageTool.instance.showInfoMessagePanel ("对方请求和棋，是否同意？");
		}
	}

	public void ReceiveThread ()
	{
		while (true) {

			byte[] result = new byte[2048];
			int receiveLength = this.clientSocket.Receive (result);//返回成功读取的字节数
			string receiveData = Encoding.UTF8.GetString (result, 0, receiveLength);

			if (receiveData == " ") {
				print ("服务端验证包");
			} else if (string.IsNullOrEmpty (receiveData)) {
				print ("空包");
			} else {
				msgList.Enqueue (receiveData);
				isReceive = true;
			}
			responseTime = 0;
		}
		
	}
	
	public void sendThread ()  //发送心跳包
	{
		while (true) {
			Thread.Sleep (20000);
			try {
				clientSocket.Send (Encoding.UTF8.GetBytes (" "));
				responseTime += 1;
				if (responseTime > 3) {
					netLost = 1;
					clientSocket.Close ();
					print ("服务器3次以上");
					return;
				}
			} catch (WebException e) {
				netLost = 1;
				clientSocket.Close ();
				print ("发送失败");
				return;
			}
		}
	}

	public void sendToServer (string content)
	{
		if (netLost == 0) {
			try {
				clientSocket.Send (Encoding.UTF8.GetBytes (content));
			} catch (WebException e) {
				netLost = 1;
				clientSocket.Close ();
				print ("发送失败");
				return;
			}
		}
	}

	public int getNetStatus ()
	{
		return this.netLost;
	}

	void OnDestroy ()
	{
		if (sendT != null && sendT.IsAlive) {
			sendT.Abort ();
		}
		if (receiveT != null && receiveT.IsAlive) {
			receiveT.Abort ();
		}
		Thread.CurrentThread.Abort();
	}
}
