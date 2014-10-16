using UnityEngine;
using System.Collections;

public class GameManagerHeungbu : GameManager {
	// 애니메이션 재생 시간(속도 : 1.0f 기준)
	private const float SawAnimationSlowSpeed 	= 0.6697f;	// 0.9f
	private const float SawAnimationNormalSpeed = 0.6075f;	// 1.0f
	private const float SawAnimationFastSpeed 	= 0.5532f;	// 1.1f

	private 	Vector3 GourdOpenPosition 	= new Vector3 (0, -5, -10);
	private const float GourdEffetValue 	= 1.5f;			// 박이 열릴때 정답에 대한 가중치
	private const int ResultMessageLeft 	= 0;
	private const int ResultMessageRight 	= 1;
	private const int BeatFileNum 			= 1;

	public GameObject gourdOpenEffect;
	public Animator SawAnimator;
	public GUITexture[] resultMessage;
	private float waitTime = 0f;
	private bool sawDirection;			// true일 경우 좌->우, false 좌<-우
	private bool gourdOpen;
	private bool waitSaw = false;
	private int beatTurnCount = 0;
	private int correctTrunCount = 0;

	void Start () {	
		ChangeUI ();		
		LogoShow("Heungbu");
		if (!showLogo) 
			GameStart ();
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
		int randomBeatFileNum = Random.Range (0, BeatFileNum);
		BeatNote = LoadBeatFileTime ("Beat/Heungbu" + randomBeatFileNum);
		beatIndex = 0;
		checkIndex = 0;
		
		InitBackgroundMusic ();
		AnotherSpaker.SendMessage ("Init", "Heungbu");
	}	
	
	public override void ResetGame () {
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
		} else if (Input.GetMouseButtonDown(0)) {
			MouseHandling();
		} else if (Input.GetKeyDown (KeyCode.Space) && GetGameState() == GameState.Play) {
			// keyboadrd space bar press
			CorrectCheck ();
		}
		
		// Back Key TouchS
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
		if (beatIndex >= BeatNote.Count) {
			beatIndex = -1;
		} else {
			BeatInfo beat = (BeatInfo)BeatNote [beatIndex];
			
			if (beat.beatAction == 1 || beat.beatAction == 2) {
				beatTurnCount++;
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
		BeatInfo beat = (BeatInfo)BeatNote [beatIndex];
		SawAnimator.speed = beat.animation;

		if (beat.beatTime > (audio.time - SawAnimationNormalSpeed)) {
			yield return new WaitForSeconds (beat.beatTime - audio.time - SawAnimationNormalSpeed);
			
			SawAnimator.Play ("SawLeftToRight");
			if (beat.beatAction == 1)
				sawDirection = true;
			else if (beat.beatAction == 2)
				sawDirection = false;
			
			beatIndex++;
			beatTurnCount++;
			waitSaw = false;
		}
	}

	public IEnumerator SawMoveWaitTime (BeatInfo beat) {
		SawAnimator.speed = beat.animation;

		float beatLength = SawAnimationNormalSpeed;
		if (beat.animation == 0.9) {
			beatLength = SawAnimationSlowSpeed;
		} else if (beat.animation == 1.0) {
			beatLength = SawAnimationNormalSpeed;
		} else if (beat.animation == 1.1) {
			beatLength = SawAnimationFastSpeed;
		}

		
		waitSaw = true;
		// 다음 beat가 재생되기 전까지 입력 대기
		float waitMoveTime = beat.beatTime - audio.time - beatLength;
		yield return new WaitForSeconds (waitMoveTime);
		
		// 필요 정보 초기화
		waitTime = 0.0f;
		
		// 톱질 방향 변경
		if (beat.beatAction == 1) {
			SawAnimator.SetTrigger ("SawTurnRight");	
			sawDirection = true;
		} else if (beat.beatAction == 2) {
			SawAnimator.SetTrigger ("SawTurnLeft");	
			sawDirection = false;
		}

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
		AnotherSpaker.SendMessage ("SoundPlayLoadFile", (int) EffectSoundHeunbu.BombGourd);
		Object particle = new Object ();
		if (beat.beatAction == 3) {
			particle = Instantiate (gourdOpenEffect, GourdOpenPosition, transform.rotation);
		} else if (beat.beatAction == 4) {
			// Goblin
			particle = Instantiate (gourdOpenEffect, GourdOpenPosition, transform.rotation);
		}
		yield return new WaitForSeconds (beat.animation);

		gourdOpen = false;
		Destroy(particle);
	}

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

			if (waitTime >= HeungbuWaitInputTime) {
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

			BeatInfo beat = (BeatInfo) BeatNote[beatIndex - 1];
			if(beat.beatTime < audio.time) {
				if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawLeftToRight")) 
					SawAnimator.SetTrigger("WaitRight");
				else if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawRightToLeft")) 
					SawAnimator.SetTrigger("WaitLeft");
			}
		}
	}

	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Began) {
			CorrectCheck();
		} else if (touch.phase == TouchPhase.Ended) {
		}
	}
	public override void MouseHandlingGame() {
		CorrectCheck ();
	}

	public override void CorrectCheck() {		
		for (int i = checkIndex; i < BeatNote.Count; i++) {
			BeatInfo beat = (BeatInfo) BeatNote[i];
			float compareTime = Mathf.Abs(beat.beatTime - audio.time);

			if (compareTime < CorrectTime1) {
				correctTrunCount++;
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				if (sawDirection) PrintResultMessage(resultMessage[ResultMessageRight], (int) ResultMessage.Excellent);
				else PrintResultMessage(resultMessage[ResultMessageLeft], (int) ResultMessage.Excellent);
				Correct();
				
				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				correctTrunCount++;
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
			} else if (beat.beatTime > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}
}
