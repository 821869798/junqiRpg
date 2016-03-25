using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using Pathfinding.Serialization.JsonFx;

public class BoardManage : MonoBehaviour
{

	public GameObject cellButton;
	public GameObject chessImg;
	public GameObject enemyChess;
	public GameObject CheckImg;
	public GameObject board1;
	public GameObject board2;
	public GameObject board3;
	public GameObject changeBoardPanel;
	public GameObject gamePanel;
	public GameObject readyGamePanel;
	public GameObject chatPanel;
	public InputField msgInput;
	public static BoardManage instance;
	private GameObject[,]cellButtonArray = new GameObject[15, 5];
	private GameObject[,]chessImgArray = new GameObject[15,5];
	private Dictionary<int,float> dic = new Dictionary<int, float>();
	private bool isCheck = false;
	private GameObject checkImg = null;
	private GameObject checkImg1 = null;
	private int oldX,oldY;
	public int currentType = 0;   //0一个人，1两个人 

	private bool isReady = false;
	private bool isAbleChess = false;
	// Use this for initializatio
	void Start ()
	{
		instance = this;
		dic.Add(0,(float)0/13);
		dic.Add(39,(float)1/13);
		dic.Add(38,(float)2/13);
		dic.Add(37,(float)3/13);
		dic.Add(36,(float)4/13);
		dic.Add(35,(float)5/13);
		dic.Add(34,(float)6/13);
		dic.Add(33,(float)7/13);
		dic.Add(32,(float)8/13);
		dic.Add(10,(float)9/13);
		dic.Add(40,(float)10/13);
		dic.Add(31,(float)11/13);
		dic.Add(30,(float)12/13);

		for(int i=0;i<6;i++){
			for(int j=0;j<5;j++){
				GameObject cell = Instantiate(cellButton,board1.transform.position,Quaternion.identity) as GameObject;
				cell.transform.SetParent(board1.transform);
				cell.GetComponent<BoardCell>().setPosition(i,j);
				cellButtonArray[i,j] = cell;
				cell.transform.SetParent(this.transform);
				cell.transform.SetSiblingIndex(10);
				cell.transform.localScale = new Vector3(1,1,1);
			}
		}

		for(int i=0;i<3;i++){
			for(int j=0;j<5;j+=2){
				GameObject cell = Instantiate(cellButton,board2.transform.position,Quaternion.identity) as GameObject;
				cell.transform.SetParent(board2.transform);
				cell.GetComponent<BoardCell>().setPosition(i*2,j);
				cell.GetComponent<BoardCell>().setYX(i+6,j);
				cellButtonArray[i+6,j] = cell;
				cell.transform.SetParent(this.transform);
				cell.transform.SetSiblingIndex(10);
				cell.transform.localScale = new Vector3(1,1,1);
			}
		}

		for(int i=0;i<6;i++){
			for(int j=0;j<5;j++){
				GameObject cell = Instantiate(cellButton,board3.transform.position,Quaternion.identity) as GameObject;
				cell.transform.SetParent(board3.transform);
				cell.GetComponent<BoardCell>().setPosition(i,j);
				cell.GetComponent<BoardCell>().setYX(i+9,j);
				cellButtonArray[i+9,j] = cell;
				cell.transform.SetParent(this.transform);
				cell.transform.SetSiblingIndex(10);
				cell.transform.localScale = new Vector3(1,1,1);
			}
		}

		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("temp")){
			Destroy(obj);
		}

		checkImg = Instantiate(CheckImg,this.transform.position,Quaternion.identity) as GameObject;
		checkImg.transform.SetParent(this.transform);
		checkImg.transform.SetSiblingIndex(0);
		checkImg.transform.localScale = new Vector3(1,1,1);
		checkImg.SetActive(false);
		checkImg1 = Instantiate(CheckImg,this.transform.position,Quaternion.identity) as GameObject;
		checkImg1.transform.SetParent(this.transform);
		checkImg1.transform.SetSiblingIndex(0);
		checkImg1.transform.localScale = new Vector3(1,1,1);
		checkImg1.SetActive(false);

		readDefault();
		//addEnemyChess();
		if(Config.getInstance().gameType==0){
			changeBoardPanel.SetActive(true);
		}else{
			readyGamePanel.SetActive(true);
			chatPanel.SetActive(true);
			//this.GetComponent<NetTool>().enabled = true;
			this.gameObject.AddComponent<NetTool>();
		}
	}
	

	public void  readDefault(){
		int [,]defaultBoard = Config.getInstance().defaultBoard;
		for(int i=0;i<6;i++){
			for(int j=0;j<5;j++){
				if(defaultBoard[i,j]!=0){
					GameObject cell = Instantiate(chessImg,board3.transform.position,Quaternion.identity) as GameObject;
					cell.transform.SetParent(this.transform);
					chessImgArray[i,j] = cell;
					cell.transform.SetSiblingIndex(1);
					Rect rect = cell.GetComponent<RawImage>().uvRect;
					rect.x = dic[defaultBoard[i,j]];
					cell.GetComponent<RawImage>().uvRect = new Rect(rect);
					cell.transform.localPosition = cellButtonArray[i,j].transform.localPosition;
					cell.transform.localScale = new Vector3(1,1,1);
				}
			}
		}
	}

	public void dealCellClicked(int y,int x){
		int [,]defaultBoard = GameController.instance.gameBoard;
		if(Config.getInstance().gameType==0||!isReady){
			if(y<6&&defaultBoard[y,x]!=0&&isCheck){
				isCheck = false;
				checkImg.SetActive(false);
				doSwapChess(y,x);
			}else if((!isCheck)&&y<6&&defaultBoard[y,x]!=0){
				isCheck = true;
				checkImg.SetActive(true);
				checkImg.transform.localPosition = cellButtonArray[y,x].transform.localPosition;
				oldX = x;
				oldY = y;
			}
		}else if(isAbleChess){
			if(isCheck&&!(oldX==x&&oldY==y)){
				Point fromP = new Point(oldY,oldX);
				Point toP = new Point(y,x);
				if(GameController.instance.judgeAbleToChess(fromP,toP)){
					SendData data = new SendData();
					data.type = 20;
					data.fromP = fromP;
					data.toP = toP;
					string jsonStr = JsonWriter.Serialize(data);
					NetTool.instance.sendToServer(jsonStr);
					AbleToChess(false);
					print ("zhen");
				}else{
					print ("jia");
				}
				isCheck = false;
				checkImg.SetActive(false);
			}else if((!isCheck)&&(defaultBoard[y,x]>0&&defaultBoard[y,x]<100)&&defaultBoard[y,x]!=31&&Config.getInstance().gameBoardType[y,x]!=3){
				disMaskCheck();
				isCheck = true;
				checkImg.SetActive(true);
				checkImg.transform.localPosition = cellButtonArray[y,x].transform.localPosition;
				oldX = x;
				oldY = y;
			}
		}
	}

	public void doSwapChess(int y,int x){  //交换棋子
		int [,]defaultBoard = GameController.instance.gameBoard;
		if(oldX==x&&oldY==y) return;   //不能交换同一颗棋子
		if((defaultBoard[oldY,oldX]==10&&Config.getInstance().gameBoardType[y,x]!=3)||(defaultBoard[y,x]==10&&Config.getInstance().gameBoardType[oldY,oldX]!=3)) //军旗只能在大本营
			return;
		if((defaultBoard[oldY,oldX]==30&&y==5)||(defaultBoard[y,x]==30&&oldY==5))  //炸弹不能摆在第一排
			return;
		if((defaultBoard[oldY,oldX]==31&&y>1)||(defaultBoard[y,x]==31&&oldY>1))  //地雷只能摆在最后两排
			return;
		//更改数组
		int temp = defaultBoard[oldY,oldX]; 
		defaultBoard[oldY,oldX] = defaultBoard[y,x];
		defaultBoard[y,x] = temp;
		//更改游戏对象数组以及棋子位置
		chessImgArray[oldY,oldX].transform.localPosition = cellButtonArray[y,x].transform.localPosition;
		chessImgArray[y,x].transform.localPosition = cellButtonArray[oldY,oldX].transform.localPosition;
		GameObject tempObj = chessImgArray[oldY,oldX];
		chessImgArray[oldY,oldX] = chessImgArray[y,x];
		chessImgArray[y,x] = tempObj;
	}


	public void onChangeSubmitClicked(){
		if(!isReady){
			int [,]gameBoard = GameController.instance.gameBoard;
			int [,]defaultBoard = Config.getInstance().defaultBoard;
			for(int i=0;i<6;i++){
				for(int j=0;j<5;j++){
					defaultBoard[i,j] = gameBoard[i,j];
				}
			}
		}
	}

	public void onReturnMenuClicked(){
		Application.LoadLevel("menu");
	}

	public void onReadyGameClicked(){
		if(currentType==1){
			readyGamePanel.SetActive(false);
			isCheck = false;
			checkImg.SetActive(false);
			SendData data = new SendData();
			data.type = 2;
			data.name = Config.getInstance().name;
			data.board = new int[6,5];
			int [,]gameBoard = GameController.instance.gameBoard;
			for(int i=0;i<6;i++){
				for(int j=0;j<5;j++){
					data.board[i,j] = gameBoard [i,j];
				}
			}
			string jsonStr = JsonWriter.Serialize(data);
			NetTool.instance.sendToServer(jsonStr);
			isReady = true;
		}
	}
	
	public void onDefeatClicked(){
		AbleToChess(false);
		SendData data = new SendData();
		data.type = 30;
		string jsonStr = JsonWriter.Serialize(data);
		NetTool.instance.sendToServer(jsonStr);
	}

	public void onPassClicked(){
		AbleToChess(false);
		SendData data = new SendData();
		data.type = 50;
		string jsonStr = JsonWriter.Serialize(data);
		NetTool.instance.sendToServer(jsonStr);
	}

	//求和
	public void onDrawChessClicked(){
		SendData data = new SendData();
		data.type = 40;
		data.which = 0;
		string jsonStr = JsonWriter.Serialize(data);
		NetTool.instance.sendToServer(jsonStr);
	}

	public void addEnemyChess(){
		int [,]defaultBoard = GameController.instance.gameBoard;
		for(int i=9;i<15;i++){
			for(int j=0;j<5;j++){
				if(defaultBoard[i,j]==100){
					GameObject cell = Instantiate(enemyChess,this.transform.position,Quaternion.identity) as GameObject;
					cell.transform.SetParent(this.transform);
					chessImgArray[i,j] = cell;
					cell.transform.SetSiblingIndex(1);
					cell.transform.localPosition = cellButtonArray[i,j].transform.localPosition;
					cell.transform.localScale = new Vector3(1,1,1);
				}
			}
		}
	}

	public void AbleToChess(bool able){
		if(able){
			isAbleChess = true;
			gamePanel.SetActive(true);
		}else{
			isAbleChess = false;
			gamePanel.SetActive(false);
		}
	}

	public void playChess(Point fromP,Point toP){
		int [,]gameBoard = GameController.instance.gameBoard ;
		if(chessImgArray[toP.y,toP.x]!=null){
			Destroy(chessImgArray[toP.y,toP.x]);
		}
		chessImgArray[toP.y,toP.x] = chessImgArray[fromP.y,fromP.x];
		chessImgArray[fromP.y,fromP.x] = null;
		chessImgArray[toP.y,toP.x].transform.localPosition =cellButtonArray[toP.y,toP.x].transform.localPosition;
		gameBoard[toP.y,toP.x] = gameBoard[fromP.y,fromP.x];
		gameBoard[fromP.y,fromP.x] = 0;
	}

	public void noPlayChess(Point fromP){
		int [,]gameBoard = GameController.instance.gameBoard ;
		Destroy(chessImgArray[fromP.y,fromP.x]);
		chessImgArray[fromP.y,fromP.x] = null;
		gameBoard[fromP.y,fromP.x] = 0;
	}

	public void togetherChessDead(Point fromP,Point toP){
		int [,]gameBoard = GameController.instance.gameBoard ;
		Destroy(chessImgArray[fromP.y,fromP.x]);
		chessImgArray[fromP.y,fromP.x] = null;
		gameBoard[fromP.y,fromP.x] = 0;
		Destroy(chessImgArray[toP.y,toP.x]);
		chessImgArray[toP.y,toP.x] = null;
		gameBoard[toP.y,toP.x] = 0;
	}
	

	public void setMaskCheck(Point fromP,Point toP){
		checkImg1.SetActive(true);
		checkImg.SetActive(true);
		checkImg.transform.localPosition = cellButtonArray[fromP.y,fromP.x].transform.localPosition;
		checkImg1.transform.localPosition = cellButtonArray[toP.y,toP.x].transform.localPosition;
	}

	private void disMaskCheck(){
		checkImg1.SetActive(false);
		checkImg.SetActive(false);
	}

	public void showOpponent40(Point tempP){
		GameObject cell = chessImgArray[tempP.y,tempP.x] ;
		Rect rect = cell.GetComponent<RawImage>().uvRect;
		rect.x = dic[10];
		cell.GetComponent<RawImage>().uvRect = new Rect(rect);

	}
}