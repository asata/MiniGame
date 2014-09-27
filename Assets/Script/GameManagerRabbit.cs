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
	
	private ArrayList RabbitBeatList;	// File로부터 토끼 절구질 정보를 읽어들임
	private ArrayList RabbitHitBeat;	// 토끼 절구질 시간 간격
	private float preTouchTime;			// 이전 터치 시간
	
	private float poundingTimeRabbit = 0;
	private float poundingTimePlayer = 0;
	//private float lastPoundingTimeRabbit = 0;
	private int beatIndex = 0;
	
	private int touchCount = 0;			// 플레이어 절구질 횟수
	
	
	void Start () {
		ChangeUI ();
		LogoShow("MoonRabbit");
		GameStart();
	}
	
	public override void GameStart() {
		Init ();		

		// 필요 정보 초기화
		RS = RabbitState.Standby;

		poundingTimeRabbit = 0;
		poundingTimePlayer = 0;
		RabbitHitBeat = new ArrayList ();

		beatIndex = 0;
		// 비트 파일로부터 정보를 읽어들임
		if (RabbitBeatList != null)
			RabbitBeatList.Clear ();
		RabbitBeatList = LoadBeatFileTime ("Beat/MoonRabbit01");

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
		}

		// Back Key Touch
		BackKeyTouch ();
		
		// 달토끼 이벤트 처리
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoShowTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();
		} else if (GetGameState() == GameState.Play) {
			MoonRabbitEvent();

			if (audio.clip.samples <= audio.timeSamples && beatIndex == -1) {
				GameEnd(true);
			}
		}
	}
	
	public override void TouchHandlingGame(Touch touch) {
		if (UIButton [(int)UIButtonList.Pause].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0)) && touch.phase == TouchPhase.Began) {
			UIGroup [(int)UIGroupList.UIPause].SendMessage ("ShowPausePanel");
			PauseOn ();
		} else {
			if (touch.phase == TouchPhase.Began) {
				// 달토끼가 입력 대기 상태일때만 터치를 처리
				if (RS == RabbitState.Wait) {
					PlayerAnimator.SetTrigger("PlayerPounding");
					CorrectCheck();
					
					preTouchTime = Time.fixedTime;
					touchCount++;	
				}
			} else if (touch.phase == TouchPhase.Ended) {
			}
		}
	}
	
	// 정답 체크
	private void CorrectCheck() {
		float playerTime = Time.fixedTime - preTouchTime;
		//float playerTime = audio.time;
		float rabbitTime = (float)RabbitHitBeat [touchCount];
		//lastPoundingTimeRabbit += (float)RabbitHitBeat [touchCount];
		float compareTime = Mathf.Abs(rabbitTime - playerTime);
		//float compareTime = Mathf.Abs(lastPoundingTimeRabbit - playerTime);
		
		if (compareTime < CorrectTime1) {
			gameScore += (CorrectPoint1 + 2 * gameComboCount);
			resultMessage.SendMessage("SetImage", 1);
			Correct();
		} else if (compareTime < CorrectTime2) {
			gameScore += (CorrectPoint2 + gameComboCount);
			resultMessage.SendMessage("SetImage", 2);
			Correct();
		} else {
			RabbitAnimator.SetTrigger("PlayerIncorrect");
			incorrectCount++;
			Incorrect();
		}
	}
	
	private void Correct() {
		gameComboCount++;
		correctCount++;
		
		if (gameMaxCombo < gameComboCount)
			gameMaxCombo = gameComboCount;
		
		if (PlayerPrefs.GetInt("EffectSound") == 0) {
			AnotherSpaker.SendMessage("SoundPlay");
		}
	}
	
	private void Incorrect(int missCount = 1) {
		gameComboCount = 0;
	}
	
	private void MoonRabbitEvent() {
		if (RS == RabbitState.Standby) {
			RS = RabbitState.Ready;
			RabbitAnimator.speed = 1f;
			PlayerAnimator.speed = 1f;
		} else if (RS == RabbitState.Wait) {
			// 사용자 입력이 끝날때까지 대기
			if (beatIndex > -1) {
				BeatInfo nextBeat = (BeatInfo) RabbitBeatList[beatIndex];
				BeatInfo thisBeat = (BeatInfo) RabbitBeatList[beatIndex - 1];
				poundingTimeRabbit = nextBeat.beatTime - thisBeat.beatTime;
				
				if ((poundingTimeRabbit - RabbitWaitInputTime) <= poundingTimePlayer) {
					RabbitAnimator.SetTrigger("PlayerInputEnd");
					RS = RabbitState.Ready;
				}
				
				poundingTimePlayer += Time.deltaTime;
			}
		} else if (RS == RabbitState.Ready) {
			poundingTimePlayer = 0;
			poundingTimeRabbit = 0;
			
			if (RabbitHitBeat.Count == touchCount) {
				// 지정된 절구질을 모두 한 경우 가산점 부여
				if (gameComboCount >= touchCount && beatIndex > 0) 
					gameScore += PoundingAllPoint;
			} else {
				// 절구질 횟수보다 더 많거나 더 적게 터치한 경우
				int count = Mathf.Abs(RabbitHitBeat.Count - touchCount);
				missCount += count;
				Incorrect(count);
			}
			
			RabbitHitBeat.Clear();
			touchCount = 0;
			
			RS = RabbitState.Pounding;	
			
			if (beatIndex > -1) StartCoroutine("WaitPounding");
		} else if (RS == RabbitState.Pounding) {	
			// 총 절구질 시간 기록
			//poundingTimeRabbit += Time.deltaTime;
		}
	}
	
	// 달토끼 절구질 
	public IEnumerator WaitPounding() {
		BeatInfo beat = (BeatInfo) RabbitBeatList[beatIndex];
		if ((beat.beatTime - audio.time) - 0.2f > 0) {
			yield return new WaitForSeconds ((beat.beatTime - audio.time) - 0.2f);
		} else {
			audio.time = beat.beatTime;
		}
		
		//Debug.Log (beat.beatTime.ToString() + " : " + audio.time.ToString());
		
		// 절구질간 간격
		RabbitHitBeat.Add (beat.intervalTime);
		if (beatIndex >= (RabbitBeatList.Count) - 1)
			beatIndex = -1;
		else 
			beatIndex++;
		
		// beatAction 1 : 절구질 후 대기
		//			  2 : 절구질 후 사용자 입력 대기
		if (beat.beatAction == 1) {
			// 다음 절구질 대기
			RabbitAnimator.SetTrigger ("Pounding");
			//RabbitAnimator.SetTrigger("RabbitReady");
			StartCoroutine ("WaitPounding");
		} else if (beat.beatAction == 2) {
			RabbitAnimator.SetTrigger ("PoundingDone");
			
			RS = RabbitState.Wait;
			//lastPoundingTimeRabbit = audio.time;
			preTouchTime = Time.fixedTime + 0.01f;
		} else if (beat.beatAction == 3) {
			RabbitAnimator.SetTrigger("PoundingLow");
			StartCoroutine ("WaitPounding");
		} else if (beat.beatAction == 4) {
			RabbitAnimator.SetTrigger ("PoundingLowDone");
			
			RS = RabbitState.Wait;
			preTouchTime = Time.fixedTime + 0.01f;
		}
	}
}