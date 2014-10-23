using UnityEngine;
using System.Collections;

public class GameManagerGildong : GameManager {
	private const float EnemyShowTime = 1.0f;
	private const int BeatFileNum = 1;
	
	private bool waitEnemy = false;
	public GUITexture resultMessage;
	public Animator GildongAnimator;
	public Animator EnemyAnimator1;
	public Animator EnemyAnimator2;
	
	void Start () {	
		ChangeUI ();		
		
		// 게임 로고 출력 - 로고 애니메이터 추가 필요
		LogoShow("Gildong");
		if (!showLogo) 
			GameStart ();
	}
	
	public override void GameStart() {
		Init ();
		
		// 주요 변수 초기화
		waitEnemy = false;
		
		// 비트 파일로부터 정보 읽어들이기
		int randomBeatFileNum = Random.Range (0, BeatFileNum);
		BeatNote = LoadBeatFileTime ("Beat/SunMoon" + randomBeatFileNum);
		beatIndex = 0;
		checkIndex = 0;
		
		InitBackgroundMusic ();
		AnotherSpaker.SendMessage ("Init", "Gildong");
	}	
	
	public override void ResetGame () {
		audio.Stop ();

		DestoyItem ("GildongArrow");
		StopCoroutine ("WaitEvent");
	
		GildongAnimator.Play ("Stand");
		EnemyAnimator1.Play("Hide");
		EnemyAnimator2.Play("Hide");
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
			GildongAnimator.SetTrigger ("HitArrow");
			CorrectCheck ();
		}
		
		// Back Key TouchS
		BackKeyTouch ();
		
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoDelayTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();
			
			// 게임 시작시 초기 설정
			if (stateShow.texture == stateTexture[2] && !waitEnemy) {
				waitEnemy = true;
				
				// beat 재생 시작 이곳 혹은 GameState.Play에서 처리
				StartCoroutine("WaitEvent");
			}
		} else if (GetGameState() == GameState.Play) {
			if (audio.clip.samples <= audio.timeSamples)
				GameEnd(true);

			if (!waitEnemy) {
				waitEnemy = true;
				
				// beat 재생 시작 이곳 혹은 GameState.Play에서 처리
				StartCoroutine("WaitEvent");
			}
			
		}	
	}
	
	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Began) {
			GildongAnimator.SetTrigger ("HitArrow");
			CorrectCheck();
		} else if (touch.phase == TouchPhase.Ended) {
		}
	}
	public override void MouseHandlingGame() {
		GildongAnimator.SetTrigger ("HitArrow");
		CorrectCheck ();
	}
	
	// beat note의 값과 비교하여 정답 체크
	public override void CorrectCheck() {	
		if (BeatNote == null) return;

		for (int i = checkIndex; i < BeatNote.Count; i++) {
			BeatInfo beat = (BeatInfo) BeatNote[i];
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
			} else if (beat.beatTime > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}
	
	public IEnumerator WaitEvent () {
		if (beatIndex < BeatNote.Count) {
			BeatInfo beat = (BeatInfo)BeatNote [beatIndex];
			// beat.animation : arrow move time
			float waitTime = beat.beatTime - audio.time - beat.animation - EnemyShowTime;
			waitEnemy = true;
			yield return new WaitForSeconds (waitTime);

			if (beat.beatAction == 1) {
				EnemyAnimator1.SetTrigger ("EnemyShow");
				EnemyAnimator1.SendMessage ("SetArrowSpeed", beat.animation * 2);
			} else if (beat.beatAction == 2) {
				EnemyAnimator2.SetTrigger ("EnemyShow");
				EnemyAnimator2.SendMessage ("SetArrowSpeed", beat.animation * 2);
			}

			waitEnemy = false;
			beatIndex++;		// 호출하는 위치에 따라 다른 위치에 있어야 함
		}
	}
	
	private void DestoyItem(string tagName) {
		GameObject[] arrowList = GameObject.FindGameObjectsWithTag (tagName);
		if (arrowList.Length > 0) {
			for(int i = 0; i < arrowList.Length; i++) {
				Destroy(arrowList[i]);
			}
		}
	}
	private void PrintMissMessage() {
		missCount++;
		PrintResultMessage(resultMessage, (int) ResultMessage.Miss);
	}
}

