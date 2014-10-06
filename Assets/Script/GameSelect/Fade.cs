using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour {
	GUITexture Black_screen;
	public float Fade_Time = 1.0f;
	public float Fade_Max = 1.0f;
	float _time;
	public bool FadeIn_ing = false;
	public bool FadeOut_ing = false;
	private float guiRatio = 1.0f;

	public GameObject infoGroup;
	public GUITexture[] buttonList;

	private bool FadeOutComplete = false;
	private const float ShowGameInfoAlpha = 0.6f;
	private const float ShowGameInfoButton = 0.75f;
	private const float ButtonMoveLength = 0.8f;
	private const float ButtonReEffectTime = 10.0f;
	private const float ButtonMoveWaitTime = 0.01f;
	private const float ButtonWaitTime = 0.1f;

	private int moveCount = 0;
	
	public void SetGUIRatio(float aRatio) {
		guiRatio = aRatio;
	}

	void Start () {
		Black_screen = GetComponent<GUITexture> ();
	}

	void Update () {
		if (FadeIn_ing) {
			_time += Time.deltaTime;
			Black_screen.color = Color.Lerp (new Color (0, 0, 0, Fade_Max), new Color (0, 0, 0, 0), _time / Fade_Time);
			
			if (Black_screen.color.a <= ShowGameInfoAlpha) {
				infoGroup.SetActive(false);

				for (int i = 0; i < buttonList.Length; i++) {
					buttonList [i].enabled = false;
				}
			}
		}

		if (FadeOut_ing) {
			_time += Time.deltaTime;
			Black_screen.color = Color.Lerp (new Color (0, 0, 0, 0), new Color (0, 0, 0, Fade_Max), _time / Fade_Time);

			if (Black_screen.color.a >= ShowGameInfoButton) {
				for (int i = 0; i < buttonList.Length; i++) {
					buttonList [i].enabled = true;
				}
			} else if (Black_screen.color.a >= ShowGameInfoAlpha) {
				infoGroup.SetActive(true);
			}
		}

		if (_time >= Fade_Time) {	 
			if(FadeOut_ing || FadeIn_ing) {				
				if (FadeIn_ing) {
					Black_screen.enabled = false;
					FadeOutComplete = false;
					gameInfo.SetActive(false);
				}
				if (FadeOut_ing) FadeOutComplete = true;

				FadeIn_ing = false;
				FadeOut_ing = false;
			}
		}

		if (FadeOutComplete) {
			StartCoroutine ("StartButtonEffect");
			FadeOutComplete = false;
		}
	}
	
	public IEnumerator StartButtonEffect() {
		for (int i = moveCount; i < 10; i++) {
			ButtonDown();
			yield return new WaitForSeconds (ButtonMoveWaitTime);
		}
		
		yield return new WaitForSeconds (ButtonWaitTime);
		
		for (int i = moveCount; i > 0; i--) {
			ButtonUp();
			yield return new WaitForSeconds (ButtonMoveWaitTime);
		}
		yield return new WaitForSeconds (ButtonReEffectTime);
		StartCoroutine ("StartButtonEffect");
	}
	
	public void ButtonDown (int index = 0, int count = 1) {
		buttonList[index].guiTexture.pixelInset = new Rect(buttonList[index].guiTexture.pixelInset.x,
		                                                   buttonList[index].guiTexture.pixelInset.y - (ButtonMoveLength * guiRatio * count),
		                                                   buttonList[index].guiTexture.pixelInset.width,
		                                                   buttonList[index].guiTexture.pixelInset.height);
		moveCount += count;
	}
	
	public void ButtonUp (int index = 0, int count = 1) {
		buttonList[index].guiTexture.pixelInset = new Rect(buttonList[index].guiTexture.pixelInset.x,
		                                                   buttonList[index].guiTexture.pixelInset.y + (ButtonMoveLength * guiRatio * count),
		                                                   buttonList[index].guiTexture.pixelInset.width,
		                                                   buttonList[index].guiTexture.pixelInset.height);                             
		moveCount -= count;
	}

	private GameObject gameInfo;
	public void FadeIn (GameObject hideObject = null) {
		_time = 0;
		FadeIn_ing = true;
		FadeOut_ing = false;
		if(hideObject != null) gameInfo = hideObject;
	}
	
	public void FadeOut () {
		if (infoGroup.activeInHierarchy) {
			FadeIn ();
		} else {
			Black_screen.enabled = true;
			_time = 0;
			FadeIn_ing = false;
			FadeOut_ing = true;
		}
	}
	
	public void HideInfo() {
		infoGroup.SetActive (false);
		for (int i = 0; i < buttonList.Length; i++) {
			buttonList [i].enabled = false;
		}
	}

	public bool InfoGroupActive() {
		return infoGroup.activeInHierarchy;
	}
}