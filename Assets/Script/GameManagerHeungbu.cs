using UnityEngine;
using System.Collections;

public class GameManagerHeungbu : GameManager {
	// 애니메이션 재생 시간(속도 : 1.0f 기준)
	private const float AnimationLeftToRightTime = 0.5833333f;	
	private const float AnimationRightToLeftTime = 0.5833333f;	//0.6666667f;
	//private const float AnimationLeftToRightTime = 0.6666666f;
	//private const float AnimationRightToLeftTime = 0.5833333f;	
	
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
	private int checkIndex = 0;
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
		checkIndex = 0;
		
		// 배경음악 재생 여부에 따라 음악 재생
		audio.clip = backgroundMusic;
		audio.time = 0.0f;
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

			// 게임 시작시 초기 설정
			if (stateShow.texture == stateTexture[2] && !waitSaw) {
				waitSaw = true;
				StartCoroutine("SawMoveFirst");
			}
		} else if (GetGameState() == GameState.Play) {
			if (audio.clip.samples <= audio.timeSamples)
				GameEnd(true);

			SawEvent ();
		}	
	}

	public void SawTypeSelect () {
		// 흥부전은 beatAction을 사용하지 않음 - 초기 방향 설정시에만 사용함
		if (beatIndex >= GourdBeatList.Count) {
			beatIndex = -1;
		} else {
			BeatInfo beat = (BeatInfo)GourdBeatList [beatIndex];
			
			if (beat.beatAction == 1 || beat.beatAction == 2) {
				StartCoroutine ("SawMoveWaitTime", beat);
			} else if (beat.beatAction == 3) {
				// beat.beatAction에 맞는 효과 재생, 일정 시간 동안만 재생
				if (beatTurnCount <= (correctTrunCount * GourdEffetValue)) 
					StartCoroutine ("OpenGourd", beat); 
				
				// 변수 초기화
				beatTurnCount = 0;
				correctTrunCount = 0;
			}
			
			beatIndex++;
		}
	}
	
	public IEnumerator SawMoveFirst () {
		BeatInfo beat = (BeatInfo)GourdBeatList [beatIndex];
		SawAnimator.speed = beat.intervalTime;

		if (beat.beatTime > (audio.time - AnimationRightToLeftTime)) {
			yield return new WaitForSeconds (beat.beatTime - audio.time - AnimationRightToLeftTime);
			
			SawAnimator.Play ("SawLeftToRight");
			if (beat.beatAction == 1)
				sawDirection = true;
			else if (beat.beatAction == 2)
				sawDirection = false;
			
			beatIndex++;
			waitSaw = false;
		}

	}

	public IEnumerator SawMoveWaitTime (BeatInfo beat) {
		SawAnimator.speed = beat.intervalTime;
		float beatLength = 1.0f + (1.0f - beat.intervalTime);

		if (beat.beatAction == 1) {
			beatLength *= AnimationLeftToRightTime;
		} else if (beat.beatAction == 2) {
			beatLength *= AnimationRightToLeftTime;
		}
		float waitMoveTime = beat.beatTime - audio.time - beatLength;
		
		waitSaw = true;
		// 다음 beat가 재생되기 전까지 입력 대기
		yield return new WaitForSeconds (waitMoveTime);
		
		if (touchCount == 0) {
			missCount++;			
			Incorrect ();
		}
		
		// 필요 정보 초기화
		waitTime = 0.0f;
		
		// 톱질 방향 변경
		if (beat.beatAction == 1)
			SawAnimator.SetTrigger ("SawTurnRight");	
		else if (beat.beatAction == 2)
			SawAnimator.SetTrigger ("SawTurnLeft");	
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
		if (beat.beatAction == 3) {
			particle = Instantiate (gourdOpenEffect, new Vector3 (0, -5, -10), transform.rotation);
		} else if (beat.beatAction == 4) {
			// Goblin
			particle = Instantiate (gourdOpenEffect, new Vector3 (0, -5, -10), transform.rotation);
		}
		yield return new WaitForSeconds (beat.intervalTime);

		gourdOpen = false;
		Destroy(particle);
	}
	
	private bool showTime = false;
	private void SawEvent () {
		if (SawAnimator.GetCurrentAnimatorStateInfo (0).IsName ("TurnWaitLeft") || 
			SawAnimator.GetCurrentAnimatorStateInfo (0).IsName ("TurnWaitRight") || 
			SawAnimator.GetCurrentAnimatorStateInfo (0).IsName ("TurnWaitLeft0") || 
			SawAnimator.GetCurrentAnimatorStateInfo (0).IsName ("TurnWaitRight0")) {
			if (waitSaw) return;

			SawAnimator.ResetTrigger ("WaitLeft");
			SawAnimator.ResetTrigger ("WaitRight");
			SawAnimator.ResetTrigger ("SawTurnLeft");
			SawAnimator.ResetTrigger ("SawTurnRight");

			if (showTime) {
				BeatInfo beat = (BeatInfo)GourdBeatList [beatIndex - 1];
				if (Mathf.Abs (audio.time - beat.beatTime) > 0.01f) {
					Debug.Log (audio.time.ToString () + " => " + (audio.time - beat.beatTime).ToString ());
				}
				showTime = false;
			}

			if (waitTime == 0.0f) {
				gourdTime = Time.fixedTime;
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
			showTime = true;

			BeatInfo beat = (BeatInfo) GourdBeatList[beatIndex - 1];
			if(beat.beatTime < audio.time) {
				audio.time -= (audio.time - beat.beatTime);
				if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawLeftToRight")) 
					SawAnimator.SetTrigger("WaitRight");
				else if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawRightToLeft")) 
					SawAnimator.SetTrigger("WaitLeft");
			}

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
		if (touch.phase == TouchPhase.Began) {
			CorrectCheck();
		} else if (touch.phase == TouchPhase.Ended) {
		}
	}

	private void CorrectCheck() {		
		for (int i = checkIndex; i < GourdBeatList.Count; i++) {
			BeatInfo beat = (BeatInfo) GourdBeatList[i];
			float compareTime = Mathf.Abs(beat.beatTime - audio.time);
			
			if (compareTime < CorrectTime1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				if (sawDirection) PrintResultMessage(resultMessage[ResultMessageRight], (int) ResultMessage.Excellent);
				else PrintResultMessage(resultMessage[ResultMessageLeft], (int) ResultMessage.Excellent);
				Correct();
				
				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				gameScore += (CorrectPoint1 + gameComboCount);
				if (sawDirection) PrintResultMessage(resultMessage[ResultMessageRight], (int) ResultMessage.Good);
				else PrintResultMessage(resultMessage[ResultMessageLeft], (int) ResultMessage.Good);
				Correct();
				
				checkIndex = i;
				break;
			} else if (beat.beatTime < audio.time) {
				// miss beat					
				if (sawDirection) PrintResultMessage(resultMessage[ResultMessageRight], (int) ResultMessage.Miss);
				else PrintResultMessage(resultMessage[ResultMessageLeft], (int) ResultMessage.Miss);
				Incorrect();
				i++;
			} else if (beat.beatTime > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}
}
