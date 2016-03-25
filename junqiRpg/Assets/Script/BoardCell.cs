using UnityEngine;
using System.Collections;

public class BoardCell : MonoBehaviour {

	public int y;
	public int x;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onButtonClicked(){
		BoardManage.instance.dealCellClicked(y,x);
	}

	public void setPosition(int y,int x){
		this.y = y;
		this.x = x;
		this.transform.localPosition = new Vector3(Config.getInstance().CellButtonWidth*x,Config.getInstance().CellButtonWidth*y,0);
	}

	public void setYX(int y,int x){
		this.y = y;
		this.x = x;
	}
}
