using UnityEngine;
using System.Collections;

public class PauseControl : MonoBehaviour {
	public Texture2D[] soundEffect;

	public GUITexture buttonEffectSound;

	//public GUITexture buttonReStart;
	//public GUITexture buttonUnPause;
	public GUITexture buttonMain;

	// Use this for initialization
	void Start () {
	
	}

	void ShowPausePanel() {
		buttonEffectSound.texture = soundEffect[PlayerPrefs.GetInt("EffectSound")];
	}

	// name에 따라 옵션 이미지를 가져오는 변수 조정
	void ChangeSoundOption(GUITexture button, string name) {
		int value = PlayerPrefs.GetInt(name);
		
		if (value == 1) {
			PlayerPrefs.SetInt(name, 0);
			button.texture = soundEffect[PlayerPrefs.GetInt(name)];
		} else {
			PlayerPrefs.SetInt(name, 1);
			button.texture = soundEffect[PlayerPrefs.GetInt(name)];
		}
	}
	
	void PauseTouchHandling() {
		if (buttonEffectSound.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			ChangeSoundOption(buttonEffectSound, "EffectSound");
		//} else if (buttonReStart.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
		//	gameObject.SetActive(false);
		//} else if (buttonUnPause.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
		//	gameObject.SetActive(false);
		} else if (buttonMain.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			Time.timeScale = 1f;
			Application.LoadLevel("GameSelect");
		}
	}
}
