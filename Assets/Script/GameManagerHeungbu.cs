using UnityEngine;
using System.Collections;

public class GameManagerHeungbu : GameManager {
	// 애니메이션 재생 시간(속도 : 1.0f 기준)
	private const float AnimationLeftToRightTime = 0.667f;
	private const float AnimationRightToLeftTime = 0.583f;	
	
	private const float GourdEffetValue 	= 1.5f;			// 박이 열릴때 정답에 대한 가중치
	private const int ResultMessageLeft 	= 0;
	private const int ResultMessageRight 	= 1;

	private const float SawDonwValue 	= 0.2f;				// 톱이 아래로 내려갈 높이
	private const float GourdOpenYValue = -3.5f;			// 박이 열릴 높이

	public GameObject gourdOpenEffect;
	
	private ArrayList GourdBeatList;
	public Animator SawAnimator;
	public GUITexture[] resultMessage;

	private int touchCount = 0;
	private bool touchCheck = false;
	private float touchTime = 0f;
	private float gourdTime = 0f;
	private float waitTime = 0f;
	private int beatIndex = 0;
	private bool sawDirection;			// true일 경우 좌->우, false 좌<-우
	private bool gourdOpen;
	private bool waitSaw = false;

	private int beatTurnCount = 0;
	private int correctTrunCount = 0;

	void Start () {	
		ChangeUI ();		
		LogoShow("Heungbu");
		if (!showLogo) GameStart ();
	}

	public override void GameStart() {
		Init ();

		// 주요 변수 초기화
		SawAnimator.speed = 0f;
		beatTurnCount = 0;
		correctTrunCount = 0;
		
		gourdOpen = false;
		waitSaw = false;

		// 비트 파일로부터 정보 읽어들이기
		if (GourdBeatList != null)
			GourdBeatList.Clear ();
		GourdBeatList = LoadBeatFileTime ("Beat/Heungbu01");
		beatIndex = 0;
		
		// 배경음악 재생 여부에 따라 음악 재생
		audio.clip = backgroundMusic;
		audio.volume = 1.0f;
		if (PlayerPrefs.GetInt ("BackgroundSound") != 0) 
			audio.volume = 0.0f;
		audio.Play ();
	}	
	
	public override void ResetGame () {
		if (GourdBeatList != null)
			GourdBeatList.Clear ();

		audio.Stop ();

		StopCoroutine ("SawMoveFirst");
		StopCoroutine ("SawMoveWaitTime");
		StopCoroutine ("MakeParticle");
		SawAnimator.Play ("TurnWaitLeft");
	}

	void Update () {
		// 터치 이벤트 처리
		int count = Input.touchCount;		
		if (count == 1) {	
			TouchHandling(Input.touches[0]);
		}
		
		// Back Key Touch
		BackKeyTouch ();
		
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoDelayTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();

			// 게임 시작시 초기 설정
			if (stateShow.texture == stateTexture[2] && !waitSaw) {
				waitSaw = true;
				StartCoroutine("SawMoveFirst");
			}
		} else if (GetGameState() == GameState.Play) {
			if (audio.clip.samples <= audio.timeSamples && beatIndex == -1)
				GameEnd(true);

			SawEvent ();
		}	
	}

	public IEnumerator SawMoveFirst () {
		BeatInfo beat = (BeatInfo)GourdBeatList [beatIndex];
		SawAnimator.speed = 1.0f;
		if (beat.beatTime > (audio.time - AnimationLeftToRightTime)) {
			yield return new WaitForSeconds (beat.beatTime - audio.time - AnimationLeftToRightTime);

			SawAnimator.Play ("SawLeftToRight");
			if (beat.beatAction == 1)
				sawDirection = true;
			else 
				sawDirection = false;

			beatIndex++;
			waitSaw = false;
		}
	}
	
	public void SawTypeSelect () {
		// 흥부전은 beatAction을 사용하지 않음 - 초기 방향 설정시에만 사용함
		if (beatIndex >= GourdBeatList.Count) {
			beatIndex = -1;
		} else {
			BeatInfo beat = (BeatInfo)GourdBeatList [beatIndex];

			if (beat.beatAction == 1) {
				StartCoroutine ("SawMoveWaitTime", beat);
			} else {
				// beat.beatAction에 맞는 효과 재생
				// 일정 시간 동안만 재생
				if (beatTurnCount <= (correctTrunCount * GourdEffetValue)) 
					StartCoroutine ("OpenGourd", beat); 
			
				// 변수 초기화
				beatTurnCount = 0;
				correctTrunCount = 0;
			}

			beatIndex++;
		}
	}
	public IEnumerator SawMoveWaitTime (BeatInfo beat) {
		SawAnimator.speed = beat.intervalTime;
		float beatLength = beat.intervalTime;

		if (sawDirection) {
			beatLength *= AnimationRightToLeftTime;
		} else {
			beatLength *= AnimationLeftToRightTime;
		}
		float waitMoveTime = beat.beatTime - beatLength - audio.time;
		
		waitSaw = true;
		// 다음 beat가 재생되기 전까지 입력 대기
		yield return new WaitForSeconds (waitMoveTime);
		
		if (touchCount == 0) {
			missCount++;			
			Incorrect ();
		}
		
		// 필요 정보 초기화
		touchCount = 0;
		touchTime = 0.0f;
		gourdTime = 0.0f;
		waitTime = 0.0f;
		touchCheck = false;
		
		// 톱질 방향 변경
		if (sawDirection)
			SawAnimator.SetTrigger ("SawTurnLeft");
		else
			SawAnimator.SetTrigger ("SawTurnRight");		
		sawDirection = !sawDirection;

		beatTurnCount++;
		waitSaw = false;
	}
	
	// 박 Open시 파티클 효과 재생
	public IEnumerator OpenGourd(BeatInfo beat) {
		gourdOpen = true;
		// 해당 beatTime이 될 때까지 대기
		float waitMoveTime = beat.beatTime - audio.time;
		yield return new WaitForSeconds (waitMoveTime);

		// 음악 중간에 효과 재생
		Object particle = new Object ();
		if (beat.beatAction == 2) {
			particle = Instantiate (gourdOpenEffect, new Vector3 (0, -5, -10), transform.rotation);
		} else if (beat.beatAction == 3) {
			// Goblin
			particle = Instantiate (gourdOpenEffect, new Vector3 (0, -5, -10), transform.rotation);
		}
		yield return new WaitForSeconds (beat.intervalTime);
		//yield return new WaitForSeconds (GourdOpenTime);

		gourdOpen = false;
		Destroy(particle);
	}
	
	private void SawEvent () {
		if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("TurnWaitLeft") || 
		    SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("TurnWaitRight")) {
			if (waitSaw) return;

			if (waitTime == 0.0f) {
				gourdTime = Time.fixedTime;
				
				// 톱이 움직이는 동안 터치를 헀을 경우
				if (touchCount > 0 && !touchCheck) {
					CorrectCheck();
				}
			} else if (waitTime >= HeungbuWaitInputTime) {
				if (!gourdOpen && !waitSaw && beatIndex > -1) {
					SawTypeSelect();
				}
			}

			waitTime += Time.deltaTime;
		} else if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawLeftToRight") || 
		           SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawRightToLeft")) {
			// 톱질 애니메이션 재생 중일 때 해당값 초기화 
			// 우->좌로 이동시 해당 값이 초기화 되지 않는 문제가 발생하여 추가함
			// 정, 오답 체크한 부분이외에 대하서는 무시 처리
			waitTime = 0.0f;
			gourdTime = 0.0f;

			if (touchCount > 0 && !touchCheck) {
				// 경과 시간 체크, 정답 입력 가능 시간이 지났을 경우 오답 처리
				if(Time.fixedTime - touchTime > CorrectTime2) {
					touchCheck = true;
					incorrectCount++;
					Incorrect();
				}
			}
		}
	}

	public override void TouchHandlingGame(Touch touch) {
		if (UIButton [(int)UIButtonList.Pause].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0)) && touch.phase == TouchPhase.Began) {
			UIGroup [(int)UIGroupList.UIPause].SendMessage ("ShowPausePanel");
			PauseOn ();
		} else {
			if (touch.phase == TouchPhase.Began) {
			} else if (touch.phase == TouchPhase.Ended) {
				if (gourdTime == 0.0f && touchCount == 0) {
				// Saw moving animation play
					touchTime = Time.fixedTime;
				} else if (gourdTime > 0.0f && touchCount == 0) {
					// Saw player input wait
					touchTime = Time.fixedTime;
					CorrectCheck ();
				} else if (gourdTime == 0.0f && touchCount > 0 || gourdTime > 0.0f && touchCount > 0) {
					// 이전 터치 값들로 정답을 판별함. 다른 값들은 무시 처리함.
				}

				touchCount++;	
			}
		}
	}

	private void CorrectCheck() {
		float compareTime = Mathf.Abs (gourdTime - touchTime);
		touchCheck = true;

		// 시간에 따라 차등 점수 및 톱 내려간 위치 차등 조정하도록 함
		if (compareTime < CorrectTime1) {
			gameScore += (CorrectPoint1 + 2 * gameComboCount);

			PrintResultMessage((int) ResultMessage.Excellent);
			Correct();
		} else if (compareTime < CorrectTime2) {
			gameScore += (CorrectPoint2 + gameComboCount);

			PrintResultMessage((int) ResultMessage.Good);
			Correct();
		} else {
			incorrectCount++;
			Incorrect();
		}
	}
	
	// 정답 처리 - 공용 처리 부분
	void Correct() {		
		gameComboCount++;
		correctCount++;
		correctTrunCount++;

		if (gameMaxCombo < gameComboCount)
			gameMaxCombo = gameComboCount;
	
		if (PlayerPrefs.GetInt("EffectSound") == 0) {
			AnotherSpaker.SendMessage("SoundPlay");
		}
	}

	void Incorrect() {
		gameComboCount = 0;

		PrintResultMessage ((int) ResultMessage.Miss);
	}
	
	private void PrintResultMessage(int imageNumber) {
		if (sawDirection) resultMessage[ResultMessageRight].SendMessage("SetImage", imageNumber);
		else resultMessage[ResultMessageLeft].SendMessage("SetImage", imageNumber);
	}
}
