using UnityEngine;
using System.Collections;

public enum RabbitState {
	Standby, 		// 사용자 입력 대기
	Wait,
	Ready,			// 절구질 준비 
	Pounding		// 절구질 애니메이션 재생
}
public class GameManagerRabbit : GameManager {
	private const float WaitChageRabbitState 	= 0.5f;
	private const int 	MaxGameLife 			= 20;
	
	// UI start
	public GUITexture resultMessage;
	
	public RabbitState RS;				// 토끼의 현재 상태
	public Animator RabbitAnimator;		// 토끼 애니메이터
	public Animator PlayerAnimator;		// 토끼 애니메이터
	private int touchCount = 0;			// 플레이어 절구질 횟수
	private int poundingCount = 0;

	void Start () {
		ChangeUI ();
		LogoShow("MoonRabbit");
		if (!showLogo) GameStart();
	}
	
	public override void GameStart() {
		Init ();		

		// 필요 정보 초기화
		RS = RabbitState.Standby;

		checkIndex = 0;
		beatIndex = 0;
		poundingCount = 0;

		// 비트 파일로부터 정보를 읽어들임
		BeatNote = LoadBeatFileTime ("Beat/MoonRabbit02");

		InitBackgroundMusic ();
	}
	
	public override void ResetGame () {		
		audio.Stop ();
		StopCoroutine ("WaitPounding");
		
		RabbitAnimator.Play ("RabbitReady");
		PlayerAnimator.Play ("player_hand_wait");
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
			if (RS == RabbitState.Wait) {
				PlayerAnimator.SetTrigger("PlayerPounding");
				CorrectCheck();
				touchCount++;	
			}
		}

		// Back Key Touch
		BackKeyTouch ();
		
		// 달토끼 이벤트 처리
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoDelayTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();
		} else if (GetGameState() == GameState.Play) {
			MoonRabbitEvent();

			// 게임 종료 처리
			if (audio.clip.samples <= audio.timeSamples) {
				RhythmTurnEnd();
				GameEnd(true);
			}
		}
	}
	
	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Began) {
			// 달토끼가 입력 대기 상태일때만 터치를 처리
			if (RS == RabbitState.Wait) {
				PlayerAnimator.SetTrigger("PlayerPounding");
				CorrectCheck();
				touchCount++;	
			}
		} else if (touch.phase == TouchPhase.Ended) {
		}
	}

	public override void MouseHandlingGame() {
		if (RS == RabbitState.Wait) {
			PlayerAnimator.SetTrigger("PlayerPounding");
			CorrectCheck();
			touchCount++;	
		}
	}
	
	// 정답 체크
	public override void CorrectCheck() {
		for (int i = checkIndex; i < BeatNote.Count; i++) {
			BeatInfo beat = (BeatInfo) BeatNote[i];
			if(beat.beatAction != 0) continue;
			float compareTime = Mathf.Abs(beat.beatTime - audio.time);

			if (compareTime < CorrectTime1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Excellent);
				Correct();
				
				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				gameScore += (CorrectPoint1 + gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Good);
				Correct();
				
				checkIndex = i;
				break;
			} else if (beat.beatTime < audio.time) {
				// miss beat			
				PrintResultMessage(resultMessage, (int) ResultMessage.Miss);
				Incorrect();
				i++;
			} else if (beat.beatTime > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}

	private void RhythmTurnEnd() {
		if (poundingCount == touchCount) {
			// 지정된 절구질을 모두 한 경우 가산점 부여
			if (gameComboCount >= touchCount) 
				gameScore += PoundingAllPoint;
		} else if (poundingCount > touchCount) {
			missCount = poundingCount - touchCount;
			gameComboCount = 0;
		}

		touchCount = 0;
	}
	private void MoonRabbitEvent() {
		if (RS == RabbitState.Standby) {
			RS = RabbitState.Ready;
			RabbitAnimator.speed = 1.0f;
			PlayerAnimator.speed = 1.0f;
		} else if (RS == RabbitState.Wait) {
			// 사용자 입력이 끝날때까지 대기
			if (beatIndex < BeatNote.Count) {
				BeatInfo nextBeat = (BeatInfo) BeatNote[beatIndex];

				if (nextBeat.beatAction == 0) {
					beatIndex++;
				} else if ((nextBeat.beatTime - audio.time) <= RabbitWaitInputTime) {
					RabbitAnimator.SetTrigger("PlayerInputEnd");
					RS = RabbitState.Ready;
				}
			}
		} else if (RS == RabbitState.Ready) {
			RhythmTurnEnd();
			
			RS = RabbitState.Pounding;	
			
			if (beatIndex > -1) StartCoroutine("WaitPounding");
		} else if (RS == RabbitState.Pounding) {	
		}
	}
	
	// 달토끼 절구질 
	public IEnumerator WaitPounding() {
		BeatInfo beat = (BeatInfo) BeatNote[beatIndex];
		if ((beat.beatTime - audio.time) - 0.1f > 0) {
			yield return new WaitForSeconds ((beat.beatTime - audio.time) - 0.1f);
		} else {
			audio.time = beat.beatTime;
		}
				
		// 절구질간 간격
		if (beatIndex >= (BeatNote.Count) - 1)
			beatIndex = -1;
		else 
			beatIndex++;
		
		// beatAction 1 : 절구질 후 대기
		//			  2 : 절구질 후 사용자 입력 대기
		if (beat.beatAction == 1) {
			// 다음 절구질 대기
			poundingCount++;
			RabbitAnimator.SetTrigger ("Pounding");
			StartCoroutine ("WaitPounding");
		} else if (beat.beatAction == 2) {
			poundingCount++;
			RabbitAnimator.SetTrigger ("PoundingDone");
			
			RS = RabbitState.Wait;
		} else if (beat.beatAction == 3) {
			poundingCount++;
			RabbitAnimator.SetTrigger("PoundingLow");
			StartCoroutine ("WaitPounding");
		} else if (beat.beatAction == 4) {
			poundingCount++;
			RabbitAnimator.SetTrigger ("PoundingLowDone");
			
			RS = RabbitState.Wait;
		}
	}
}