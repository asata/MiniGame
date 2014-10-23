using UnityEngine;
using System.Collections;

public class GameManagerSunMoon : GameManager {
	private const float AnimationMoveCakeTime = 0.5f;			// throw time 사용시 제거
	private Vector3 CakeInitVector = new Vector3 (3.0f, -5.6f);
	private const string CakeTagName = "SunMoonCake";
	private const string StoneTagName = "SunMoonStone";
	private const int BeatFileNum = 1;

	public GameObject Cake;
	public GameObject Stone;
	public GUITexture resultMessage;
	public Animator TigerAnimator;
	private bool throwCake = false;

	void Start () {	
		ChangeUI ();
		LogoShow("SunMoon");
		if (!showLogo) 
			GameStart ();
	}
	
	public override void GameStart() {
		Init ();

		throwCake = false;

		// 비트 파일로부터 정보를 읽어들임
		int randomBeatFileNum = Random.Range (0, BeatFileNum);
		BeatNote = LoadBeatFileTime ("Beat/SunMoon" + randomBeatFileNum);  	// beat time, throw time, cake type
		// throw time : 0.3f(fast)~0.8f(slow), default : 0.5f
		beatIndex = 0;
		checkIndex = 0;

		InitBackgroundMusic ();		
		AnotherSpaker.SendMessage ("Init", "SunMoon");
	}	
	
	public override void ResetGame () {		
		// 날아다니는 떡 및 돌 소멸 처리
		DestoyItem (CakeTagName);
		DestoyItem (StoneTagName);

		audio.Stop ();
		StopCoroutine ("WaitThrowCake");

		// 호랑이 애니메이션 init
		TigerAnimator.Play ("StandBy");
	}

	private void DestoyItem(string tagName) {
		GameObject[] cakeList = GameObject.FindGameObjectsWithTag (tagName);
		if (cakeList.Length > 0) {
			for(int i = 0; i < cakeList.Length; i++) {
				Destroy(cakeList[i]);
			}
		}
	}
	
	void Update () {
		// 터치 이벤트 처리
		int count = Input.touchCount;		
		if (count == 1) {	
			TouchHandling (Input.touches [0]);
		} else if (Input.GetMouseButtonDown(0)) {
			MouseHandling();
		} else if (Input.GetKeyDown (KeyCode.Space) && GetGameState() == GameState.Play) {
			// keyboadrd space bar press
			CorrectCheck ();
		}
		
		// Back Key Touch
		BackKeyTouch ();
		
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoDelayTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();

			if (stateShow.texture == stateTexture[2] && !throwCake) {
				StartCoroutine ("WaitThrowCake");
				throwCake = true;
			}
		} else if (GetGameState() == GameState.Play) {	
			if (audio.clip.samples <= audio.timeSamples)
				GameEnd(true);

			if (!throwCake) {
				throwCake = true;
				StartCoroutine ("WaitThrowCake");
			}
		}	
	}

	public IEnumerator WaitThrowCake() {
		if (beatIndex < BeatNote.Count) {
			BeatInfo beat = (BeatInfo)BeatNote [beatIndex];
			float waitMoveTime = beat.beatTime - audio.time - AnimationMoveCakeTime;
			yield return new WaitForSeconds (waitMoveTime);
	
			// beat.beatAction으로 떡과 돌을 구분
			GameObject makeCake = null;
			if (beat.beatAction == 1) {
				makeCake = (GameObject)Instantiate (Cake, CakeInitVector, transform.rotation);
			} else if (beat.beatAction == 2) {
				makeCake = (GameObject)Instantiate (Stone, CakeInitVector, transform.rotation);
				makeCake.SendMessage ("SetTypeNo", beat.beatAction);
			}

			// throw time 사용시 use
			makeCake.SendMessage ("SetMoveTime", beat.animation);
			makeCake.SendMessage ("SetBeatIndex", beatIndex);
			beatIndex++;
			throwCake = false;
		}
	}

	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Began) {
			CorrectCheck ();
		} else if (touch.phase == TouchPhase.Ended) {

		}
	}
	public override void MouseHandlingGame() {
		CorrectCheck ();
	}

	public override void CorrectCheck () {
		if (BeatNote == null) return;

		for (int i = checkIndex; i < BeatNote.Count; i++) {
			BeatInfo beat = (BeatInfo) BeatNote[i];
			float compareTime = Mathf.Abs(beat.beatTime - audio.time);

			if (compareTime < CorrectTime1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				bool soundPlay = true;
				if (beat.beatAction == 1) {
					TigerAnimator.SetTrigger("HitCake");
					FindEatCake(i);
				} else if (beat.beatAction == 2) {
					TigerAnimator.SetTrigger("HitStone");
					HitStone(i); 
					if (PlayerPrefs.GetInt("EffectSound") == 0 && AnotherSpaker != null) 
						AnotherSpaker.SendMessage("SoundPlayLoadFile", (int) EffectSoundTiger.HitStone);
					soundPlay = false;
				}
				PrintResultMessage(resultMessage, (int) ResultMessage.Excellent);
				Correct(soundPlay);

				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				gameScore += (CorrectPoint1 + gameComboCount);
				bool soundPlay = true;
				if (beat.beatAction == 1) {
					TigerAnimator.SetTrigger("HitCake");
					FindEatCake(i);
				} else if (beat.beatAction == 2) {
					TigerAnimator.SetTrigger("HitStone");
					HitStone(i);
					if (PlayerPrefs.GetInt("EffectSound") == 0 && AnotherSpaker != null) 
						AnotherSpaker.SendMessage("SoundPlayLoadFile", (int) EffectSoundTiger.HitStone);
					soundPlay = false;
				}
				PrintResultMessage(resultMessage, (int) ResultMessage.Good);
				Correct(soundPlay);

				checkIndex = i;
				break;
			} else if (beat.beatTime < audio.time) {
				// miss beat					
				PrintResultMessage(resultMessage, (int) ResultMessage.Miss);
				Incorrect();
			} else if (beat.beatTime > audio.time) {
				if (beat.beatAction == 1) {
					TigerAnimator.SetTrigger("HitCake");
				} else if (beat.beatAction == 2) {
					TigerAnimator.SetTrigger("HitStone");
				}
				checkIndex = i;
				break;
			}
		}
	}

	private void HitStone(int index) {
		GameObject[] cakeList = GameObject.FindGameObjectsWithTag (StoneTagName);
		for (int i = 0; i < cakeList.Length; i++) {
			cakeList[i].SendMessage("HitStone", index);
		}
	}
	
	private void FindEatCake(int index) {
		GameObject[] cakeList = GameObject.FindGameObjectsWithTag (CakeTagName);
		for (int i = 0; i < cakeList.Length; i++) {
			cakeList[i].SendMessage("EatCake", index);
		}
	}
}