using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public int[,]gameBoard = new int[15,5];
	public static GameController instance;

	// Use this for initialization
	void Start () {
		instance = this;
		Array.Clear(gameBoard,0,gameBoard.Length);
		int [,]defaultBoard = Config.getInstance().defaultBoard;
		for(int i=0;i<6;i++){
			for(int j=0;j<5;j++){
				gameBoard[i,j]=defaultBoard[i,j];
			}
		}
		for(int i=0;i<6;i++){
			for(int j=0;j<5;j++){
				if(defaultBoard[i,j]>0)
					gameBoard[14-i,j]=100;
			}
		}
	}
	
	public bool judgeAbleToChess(Point fromP,Point toP){  //判断走棋是否合法
		int [,]boardType = Config.getInstance().gameBoardType;
		if(!(gameBoard[toP.y,toP.x]==0||gameBoard[toP.y,toP.x]==100)){
			return false;
		}
		if((Mathf.Abs(fromP.y-toP.y)+Mathf.Abs(fromP.x-toP.x))==1){  //横竖走一格
			return true;
		}
		if((fromP.y!=toP.y&&fromP.x!=toP.x)&&(Mathf.Abs(fromP.y-toP.y)+Mathf.Abs(fromP.x-toP.x))==2){
			if(boardType[fromP.y,fromP.x]==2||boardType[toP.y,toP.x]==2)  //行营斜着走
				return true;
		}
		if(boardType[fromP.y,fromP.x]==1&&boardType[toP.y,toP.x]==1){  //走铁道
			if(fromP.x==toP.x){  //纵向
				bool flag = true;
				if(fromP.y>toP.y){
					for (int i=toP.y+1;i<fromP.y;i++){
						if(!(gameBoard[i,fromP.x]==0&&boardType[i,fromP.x]==1)){
							flag = false;
							break;
						}
					}
				}else{
					for (int i=fromP.y+1;i<toP.y;i++){
						if(!(gameBoard[i,fromP.x]==0&&boardType[i,fromP.x]==1)){
							flag = false;
							break;
						}
					}
				}
				if(flag)
					return true;
			}else if(fromP.y==toP.y){ //横向
				bool flag = true;
				if(fromP.x>toP.x){
					for (int j=toP.x+1;j<fromP.x;j++){
						if(!(gameBoard[fromP.y,j]==0&&(boardType[fromP.y,j]==1||boardType[fromP.y,j]==-1))){
							flag = false;
							break;
						}
					}
				}else{
					for (int j=fromP.x+1;j<toP.x;j++){
						if(!(gameBoard[fromP.y,j]==0&&(boardType[fromP.y,j]==1||boardType[fromP.y,j]==-1))){
							flag = false;
							break;
						}
					}
				}
				if(flag)
					return true;
			}
		}
		if(gameBoard[fromP.y,fromP.x]==32){  //小兵任意走铁道
			bool isAble = false;
			int [,]tempArray = new int[15,5];
			Array.Clear(tempArray,0,tempArray.Length);
			Queue<Point> pointQue = new Queue<Point>();
			pointQue.Enqueue(fromP);
			tempArray[fromP.y,fromP.x] = 1;
			while(pointQue.Count>0){
				Point point = pointQue.Dequeue();
				print("Point:"+point.y+","+point.x);
				if(point.y==toP.y&&point.x==toP.x){
					isAble = true;
					pointQue.Clear();
					break;
				}
				bfsChess(point,toP,tempArray,pointQue);
			}
			if(isAble)
				return true;
		}
		return false;
	}

	private void bfsChess(Point tp,Point toP,int [,]tempArray,Queue<Point> pointQue){
		int [,]boardType = Config.getInstance().gameBoardType;
		if(tp.y+1<15&&boardType[tp.y+1,tp.x]==1&&tempArray[tp.y+1,tp.x]==0&&(gameBoard[tp.y+1,tp.x]==0||(tp.y+1==toP.y&&tp.x==toP.x))){
			Point temp = new Point(tp.y+1,tp.x);
			tempArray[tp.y+1,tp.x]=1;
			pointQue.Enqueue(temp);
		}
		if(tp.y-1>=0&&boardType[tp.y-1,tp.x]==1&&tempArray[tp.y-1,tp.x]==0&&(gameBoard[tp.y-1,tp.x]==0||(tp.y-1==toP.y&&tp.x==toP.x))){
			Point temp = new Point(tp.y-1,tp.x);
			tempArray[tp.y-1,tp.x]=1;
			pointQue.Enqueue(temp);
		}
		if(tp.x+1<5&&(boardType[tp.y,tp.x+1]==1||boardType[tp.y,tp.x+1]==-1)&&tempArray[tp.y,tp.x+1]==0&&(gameBoard[tp.y,tp.x+1]==0||(tp.y==toP.y&&tp.x+1==toP.x))){
			Point temp = new Point(tp.y,tp.x+1);
			tempArray[tp.y,tp.x+1]=1;
			pointQue.Enqueue(temp);
		}
		if(tp.x-1>=0&&(boardType[tp.y,tp.x-1]==1||boardType[tp.y,tp.x-1]==-1)&&tempArray[tp.y,tp.x-1]==0&&(gameBoard[tp.y,tp.x-1]==0||(tp.y==toP.y&&tp.x-1==toP.x))){
			Point temp = new Point(tp.y,tp.x-1);
			tempArray[tp.y,tp.x-1]=1;
			pointQue.Enqueue(temp);
		}
	}
}
