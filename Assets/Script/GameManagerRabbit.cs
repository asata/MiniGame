using UnityEngine;
using System.Collections;

public enum RabbitState {
	Standby, 		// 사용자 입력 대기
	Wait,
	Ready,			// 절구질 준비 
	Pounding		// 절구질 애니메이션 재생
}
public class PlayerBeatInput {
	private float touchTime;
	private bool correctCheck;

	public PlayerBeatInput(float aTime, bool aCheck = false){
		touchTime = aTime;
		correctCheck = aCheck;
	}

	public float GetTime() {
		return touchTime;
	}
	public bool GetCheck() {
		return correctCheck;
	}
	public void SetCheck(bool aCheck) {
		correctCheck = aCheck;
	}
}
public class GameManagerRabbit : GameManager {
	private const float WaitChageRabbitState 	= 0.5f;
	private const int 	MaxGameLife 			= 20;
	
	// UI start
	public GUITexture resultMessage;
	
	public RabbitState RS;				// 토끼의 현재 상태
	public Animator RabbitAnimator;		// 토끼 애니메이터
	public Animator PlayerAnimator;		// 토끼 애니메이터
	
	private ArrayList RabbitBeatList;	// File로부터 토끼 절구질 정보를 읽어들임
	private ArrayList RabbitHitBeat;	// 토끼 절구질 시간 간격
	private float preTouchTime;			// 이전 터치 시간

	private int checkIndex = 0;
	private int beatIndex = 0;	
	private int touchCount = 0;			// 플레이어 절구질 횟수
	
	
	void Start () {
		ChangeUI ();
		LogoShow("MoonRabbit");
		if (!showLogo) GameStart();
	}
	
	public override void GameStart() {
		Init ();		

		// 필요 정보 초기화
		RS = RabbitState.Standby;

		RabbitHitBeat = new ArrayList ();

		checkIndex = 0;
		beatIndex = 0;

		// 비트 파일로부터 정보를 읽어들임
		if (RabbitBeatList != null)
			RabbitBeatList.Clear ();
		RabbitBeatList = LoadBeatFileTime ("Beat/MoonRabbit02");

		audio.clip = backgroundMusic;
		audio.volume = 1.0f;

		if (PlayerPrefs.GetInt("BackgroundSound") != 0) 
			audio.volume = 0.0f;
		audio.Play ();
	}
	
	public override void ResetGame () {
		if (RabbitHitBeat != null)
			RabbitHitBeat.Clear ();
		
		audio.Stop ();
		StopCoroutine ("WaitPounding");
		
		RabbitAnimator.Play ("RabbitReady");
		PlayerAnimator.Play ("player_hand_wait");
	}
	
	void Update () {
		// 터치 이벤트 처리
		int count = Input.touchCount;		
		if (count == 1) {	
			TouchHandling (Input.touches[0]);
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
	
	// 정답 체크
	private void CorrectCheck() {
		for (int i = checkIndex; i < RabbitHitBeat.Count; i++) {
			PlayerBeatInput inputBeat = (PlayerBeatInput) RabbitHitBeat[i];
			if (inputBeat.GetCheck()) continue;
			float compareTime = Mathf.Abs(inputBeat.GetTime() - audio.time);

			if (compareTime < CorrectTime1) {
				inputBeat.SetCheck(true);
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Excellent);
				Correct();
				
				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				inputBeat.SetCheck(true);
				gameScore += (CorrectPoint1 + gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Good);
				Correct();
				
				checkIndex = i;
				break;
			} else if (inputBeat.GetTime() < audio.time) {
				// miss beat					
				inputBeat.SetCheck(true);
				PrintResultMessage(resultMessage, (int) ResultMessage.Miss);
				Incorrect();
				i++;
			} else if (inputBeat.GetTime() > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}

	private void RhythmTurnEnd() {
		if (RabbitHitBeat.Count == touchCount) {
			// 지정된 절구질을 모두 한 경우 가산점 부여
			if (gameComboCount >= touchCount) 
				gameScore += PoundingAllPoint;
		} else if (RabbitHitBeat.Count > touchCount) {
			for (int i = 0; i < RabbitHitBeat.Count; i++) {
				PlayerBeatInput beat = (PlayerBeatInput) RabbitHitBeat[i];
				if (beat.GetCheck()) continue;

				missCount++;
				gameComboCount = 0;
			}
		}		
		
		RabbitHitBeat.Clear();
		touchCount = 0;
	}
	private void MoonRabbitEvent() {
		if (RS == RabbitState.Standby) {
			RS = RabbitState.Ready;
			RabbitAnimator.speed = 1.0f;
			PlayerAnimator.speed = 1.0f;
		} else if (RS == RabbitState.Wait) {
			// 사용자 입력이 끝날때까지 대기
			if (beatIndex < RabbitBeatList.Count) {
				BeatInfo nextBeat = (BeatInfo) RabbitBeatList[beatIndex];

				if ((nextBeat.beatTime - audio.time) <= RabbitWaitInputTime) {
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
		BeatInfo beat = (BeatInfo) RabbitBeatList[beatIndex];
		if ((beat.beatTime - audio.time) - 0.1f > 0) {
			yield return new WaitForSeconds ((beat.beatTime - audio.time) - 0.1f);
		} else {
			audio.time = beat.beatTime;
		}
				
		// 절구질간 간격
		if (beatIndex >= (RabbitBeatList.Count) - 1)
			beatIndex = -1;
		else 
			beatIndex++;
		
		// beatAction 1 : 절구질 후 대기
		//			  2 : 절구질 후 사용자 입력 대기
		if (beat.beatAction == 1) {
			// 다음 절구질 대기
			RabbitAnimator.SetTrigger ("Pounding");
			StartCoroutine ("WaitPounding");
		} else if (beat.beatAction == 2) {
			RabbitAnimator.SetTrigger ("PoundingDone");
			
			RS = RabbitState.Wait;
			RabbitBeatListAdd();
		} else if (beat.beatAction == 3) {
			RabbitAnimator.SetTrigger("PoundingLow");
			StartCoroutine ("WaitPounding");
		} else if (beat.beatAction == 4) {
			RabbitAnimator.SetTrigger ("PoundingLowDone");
			
			RS = RabbitState.Wait;
			RabbitBeatListAdd();
		}
	}

	private void RabbitBeatListAdd() {
		for (; beatIndex < RabbitBeatList.Count; beatIndex++) {
			BeatInfo beat = (BeatInfo) RabbitBeatList[beatIndex];
			if (beat.beatAction != 0) break;

			RabbitHitBeat.Add(new PlayerBeatInput(beat.beatTime));
		}

		checkIndex = 0;
	}
}