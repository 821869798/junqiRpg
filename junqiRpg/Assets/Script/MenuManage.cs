using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManage : MonoBehaviour {

	public GameObject inputNamePanel;
	public InputField input;
	public Text msgText;
	public void onStartGameClicked(){
		inputNamePanel.SetActive(true);
	}

	public void onBoardSettingClicked(){
		Config.getInstance().gameType = 0;
		Application.LoadLevel("game");
	}
	public void CancelBtn(){
		msgText.text = "请输入一个名字";
		input.text = "";
		inputNamePanel.SetActive(false);
	}

	public void confirmBtn(){
		string name = input.text;
		if(name==""){
			msgText.text = "输入错误";
		}else{
			Config.getInstance().name = name;
			Config.getInstance().gameType = 1;
			Application.LoadLevel("game");
		}
	}
}
