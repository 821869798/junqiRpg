using UnityEngine;
using System.Collections;

public class SendData {
	public int type;  //0为自己连接的信息，1表示对方连接，10表示消息,2表示准备好了,3开始游戏，20走棋
	public string text;
	public string name;
	public Point fromP;
	public Point toP;
	public int which;
	public int[,] board;
	public int result;
	public int isAble;
	public Point tempP;
}

public class Point{
	public int y;
	public int x;
	public Point(){
	}
	public Point(int y,int x){
		this.y = y;
		this.x = x;
	}
}

