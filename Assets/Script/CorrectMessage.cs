using UnityEngine;
using System.Collections;

public class CorrectMessage : MonoBehaviour {
	public Texture2D[] messageList;
	public int showTime = 50;

	void Update () {
		if (showTime < 0) {
			this.guiTexture.texture = messageList[(int) ResultMessage.Null];
		}

		showTime--;
	}

	public void SetShowTime(int time) {
		showTime = time;
	}
	public void SetImage(int type) {
		this.guiTexture.texture = messageList [type];
		showTime = 50;
	}
}
