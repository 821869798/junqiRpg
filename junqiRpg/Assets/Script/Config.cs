using UnityEngine;
using System.Collections;

public class Config {
	private static Config instance = null;
	public int CellButtonWidth = 75;
	public int gameType = 0;
	public string name = "";
	public string IPConfig = "121.42.152.177";

	// 40 司令
	// 39 军长
	// 	38 师长
	//	37 旅长
	//	36 团长
	//	35 营长
	//	34 连长
	//	33 排长
	//	32 工兵
	//	31 地雷
	//	30 炸弹
	// 10 军棋
	public int [,]defaultBoard = new int[,]{
		{31,10,31,33,34},
		{33,38,37,32,31},
		{37,0,35,0,34},
		{39,30,0,32,36},
		{30,0,32,0,40},
		{38,34,35,36,33}
		};

		public readonly int [,] gameBoardType = new int[,] {  //0表示普通路，1表示铁道，2表示行营，3表示大本营
		{0,3,0,3,0},
		{1,1,1,1,1},
		{1,2,0,2,1},
		{1,0,2,0,1},
		{1,2,0,2,1},
		{1,1,1,1,1},
		{1,-1,1,-1,1},
		{1,-1,1,-1,1},
		{1,-1,1,-1,1},
		{1,1,1,1,1},
		{1,2,0,2,1},
		{1,0,2,0,1},
		{1,2,0,2,1},
		{1,1,1,1,1},
		{0,3,0,3,0}
	};

	public static  Config getInstance(){
		if(instance==null){
			instance = new Config();
		}
		return instance;
	}

}
