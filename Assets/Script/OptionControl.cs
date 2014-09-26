using UnityEngine;
using System.Collections;

/*
 * 게임 전체에서 사용하는 옵션 패널 동작 처리
 * 버튼 터치등 처리는 각 Scene Touch를 처리하는 부분에서 처리 하도록 함
 *  - 수정할 경우 터치 후 OptionControl에 있는 함수를 호출하여 처리하도록 수정??
 * 
 * 사운드 및 진동 조정의 경우 On, Off로 나눠 이미지 제작
 * 다른 옵션 처리는 게임 개발 진행에 따라 추가하도록 
 */
public class OptionControl : MonoBehaviour {	
	public Texture2D textureOn;
	public Texture2D textureOff;
	
	public GUITexture buttonBackgroundSound;
	public GUITexture buttonEffectSound;
	public GUITexture buttonOptionClose;

	void Start() {
		//Common common = new Common ();
		//common.ChangeUISize ("Option");
	}

	// 옵션 패널을 열 때 초기값 설정
	void ShowOptionPanel() {
		SetButtonTexture(buttonBackgroundSound, PlayerPrefs.GetInt("BackgroundSound"));
		SetButtonTexture(buttonEffectSound, PlayerPrefs.GetInt("EffectSound"));
	}

	// 버튼별 On, Off 이미지 출력
	// 추후 각 button별 on, off 이미지가 달라지므로 수정해야 함!!!
	void SetButtonTexture(GUITexture button, int value) {
		if (value == 0)
			button.texture = textureOn;
		else
			button.texture = textureOff;
	}

	// 옵션 버튼을 선택하여 변경할 경우
	// 동일 항목(On, Off로 처리)이 늘어날 경우 별도 함수로 구성함
	void ChangeSoundOption(GUITexture button, string name) {
		int value = PlayerPrefs.GetInt(name);

		if (value == 1) {
			SetButtonTexture(button, 0);
			PlayerPrefs.SetInt(name, 0);
		} else {
			SetButtonTexture(button, 1);
			PlayerPrefs.SetInt(name, 1);
		}
	}

	void OptionTouchHandling() {
		if (buttonBackgroundSound.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			ChangeSoundOption(buttonBackgroundSound, "BackgroundSound");
		} else if (buttonEffectSound.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			ChangeSoundOption(buttonEffectSound, "EffectSound");
		} else if (buttonOptionClose.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			gameObject.SetActive(false);
		}
	}
}
