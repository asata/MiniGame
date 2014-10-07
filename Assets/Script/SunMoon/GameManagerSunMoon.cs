using UnityEngine;
using System.Collections;

public enum HitZoneInItem {
	None = 0,
	Cake,
	Stone,
}
public class GameManagerSunMoon : GameManager {
	private const float AnimationMoveCakeTime = 0.5f;			// throw time 사용시 제거
	private Vector3 CakeInitVector = new Vector3 (3.0f, -5.0f);

	public GameObject Cake;
	public GameObject Stone;
	public GUITexture resultMessage;
	public Animator TigerAnimator;
	
	public GUIText show;
	public GUIText show2;

	private int beatIndex = 0;
	private int checkIndex = 0;
	private ArrayList CakeBeatList;
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
		if (CakeBeatList != null)
			CakeBeatList.Clear ();
		CakeBeatList = LoadBeatFile ("Beat/SunMoon01");			// beat time, cake type
		//CakeBeatList = LoadBeatFileTime ("Beat/SunMoon01");   // beat time, throw time, cake type
		// throw time : 0.3f(fast)~0.8f(slow), default : 0.5f
		beatIndex = 0;
		checkIndex = 0;
		
		// 배경음악 재생 여부에 따라 음악 재생
		audio.clip = backgroundMusic;
		audio.volume = 1.0f;
		if (PlayerPrefs.GetInt ("BackgroundSound") != 0) 
			audio.volume = 0.0f;
		audio.Play ();
	}	
	
	public override void ResetGame () {
		if (CakeBeatList != null)
			CakeBeatList.Clear ();
		
		// 날아다니는 떡 및 돌 소멸 처리
		DestoyItem ("SunMoonCake");
		DestoyItem ("SunMoonStone");

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
		if (beatIndex < CakeBeatList.Count) {
			BeatInfo beat = (BeatInfo)CakeBeatList [beatIndex];
			float waitMoveTime = beat.beatTime - audio.time - AnimationMoveCakeTime;
			yield return new WaitForSeconds (waitMoveTime);
	
			// beat.beatAction으로 떡과 돌을 구분
			GameObject makeCake = null;
			if (beat.beatAction == 1) {
				makeCake = (GameObject)Instantiate (Cake, CakeInitVector, transform.rotation);
			} else if (beat.beatAction == 2) {
				makeCake = (GameObject)Instantiate (Stone, CakeInitVector, transform.rotation);
				makeCake.SendMessage ("SetTypeNo", HitZoneInItem.Stone);
			}

			// throw time 사용시 use
			//makeCake.SendMessage ("SetMoveTime", beat.intervalTime);
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
		if (CakeBeatList == null) return;

		for (int i = checkIndex; i < CakeBeatList.Count; i++) {
			BeatInfo beat = (BeatInfo) CakeBeatList[i];
			float compareTime = Mathf.Abs(beat.beatTime - audio.time);

			if (compareTime < CorrectTime1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				if (beat.beatAction == 1) {
					TigerAnimator.SetTrigger("HitCake");
					FindDestroyCake("SunMoonCake", i);
				} else if (beat.beatAction == 2) {
					TigerAnimator.SetTrigger("HitStone");
					HitStone(i); 
				}
				PrintResultMessage(resultMessage, (int) ResultMessage.Excellent);
				Correct();

				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				gameScore += (CorrectPoint1 + gameComboCount);
				if (beat.beatAction == 1) {
					TigerAnimator.SetTrigger("HitCake");
					FindDestroyCake("SunMoonCake", i);
				} else if (beat.beatAction == 2) {
					TigerAnimator.SetTrigger("HitStone");
					HitStone(i);
				}
				PrintResultMessage(resultMessage, (int) ResultMessage.Good);
				Correct();

				checkIndex = i;
				break;
			} else if (beat.beatTime < audio.time) {
				// miss beat					
				PrintResultMessage(resultMessage, (int) ResultMessage.Miss);
				Incorrect();
			} else if (beat.beatTime > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}

	private void HitStone(int index) {
		GameObject[] cakeList = GameObject.FindGameObjectsWithTag ("SunMoonStone");
		for (int i = 0; i < cakeList.Length; i++) {
			cakeList[i].SendMessage("HitStone", index);
		}
	}
	
	private void FindDestroyCake(string tagName, int index) {
		GameObject[] cakeList = GameObject.FindGameObjectsWithTag (tagName);
		for (int i = 0; i < cakeList.Length; i++) {
			cakeList[i].SendMessage("DestroyIndex", index);
		}
	}
}