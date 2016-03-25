using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Serialization.JsonFx;

public class MessageTool : MonoBehaviour {

	public GameObject content;
	public GameObject item_pre;
	public InputField msgInput;
	public static Queue<GameObject> queue = new Queue<GameObject>();
	public static MessageTool instance;
	public Text nameText1;
	public GameObject nameText2;
	public ScrollRect scrollRect;
	public GameObject infoMsgPanel;
	public int drawCount = 0;
	public int infoType = 0;
	// Use this for initialization
	void Start () {
		instance = this;
		nameText1.text = Config.getInstance().name;
	}


	public void AddItem( string text,int color)
	{
		if(queue.Count>=20){
			Destroy(queue.Dequeue());
		}
		GameObject item = Instantiate(this.item_pre,content.transform.position,Quaternion.identity) as GameObject;
		item.GetComponent<Text>().text = text;
		item.transform.SetParent(this.content.transform);
		item.transform.localScale = new Vector3(1,1,1);
		if(color==1){
			item.GetComponent<Text>().color = Color.red;
		}
		queue.Enqueue(item);
		this.msgInput.text = "";
		Canvas.ForceUpdateCanvases();
		scrollRect.normalizedPosition = new Vector2(0,0);
	}

	public void setOpponentName(string name){
		nameText2.SetActive(true);
		nameText2.GetComponent<Text>().text = name;
	}
	

	public void onSendMsgClicked(){
		if(NetTool.instance.getNetStatus()>0||BoardManage.instance.currentType==0)
			return;
		string content = msgInput.text;
		if(content!=""){
			SendData data = new SendData();
			data.name = Config.getInstance().name;
			data.type = 10;
			data.text = content;
			string jsonStr = JsonWriter.Serialize(data);
			NetTool.instance.sendToServer(jsonStr);
		}
	}

	public void showInfoMessagePanel(string contentText){
		infoMsgPanel.SetActive(true);
		infoMsgPanel.GetComponentInChildren<Text>().text = contentText;
	}

	public void onOkBtnClicked(){
		infoMsgPanel.SetActive(false);
		if(infoType==0){
			SendData data = new SendData();
			data.type = 40;
			data.which = 1;
			data.result = drawCount;
			string jsonstr = JsonWriter.Serialize(data);
			NetTool.instance.sendToServer(jsonstr);
		}
	}

	public void onCancelClicked(){
		infoMsgPanel.SetActive(false);
	}
}
