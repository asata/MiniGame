using UnityEngine;
using System.Collections;

public class GameManagerHeungbu : GameManager {
	// 애니메이션 재생 시간(속도 : 1.0f 기준)
	private const float AnimationLeftToRightTime = 0.667f;
	private const float AnimationRightToLeftTime = 0.583f;	
	
	private const int ResultMessageLeft 	= 0;
	private const int ResultMessageRight 	= 1;

	private const float SawDonwValue 	= 0.2f;				// 톱이 아래로 내려갈 높이
	private const float GourdOpenYValue = -3.5f;			// 박이 열릴 높이

	public GUIText labelTime;
	public GUIText show;
	public GUIText show2;

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
	private bool correctDownSaw;
	private bool waitSaw = false;

	void Start () {	
		ChangeUI ();
		GameStart ();
	}
	
	public override void GameStart() {
		Init ();

		// 주요 변수 초기화
		SawAnimator.transform.position = new Vector3(0, 0);
		SawAnimator.speed = 0f;
		
		// 기본 60.0f 변경시 수정
		//SetMaxGameTime (240);
		correctDownSaw = false;
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
		
		if (GetGameState () == GameState.Ready) {
			GameReady();

			// 게임 시작시 초기 설정
			if (stateShow.texture == stateTexture[2] && !waitSaw) {
				waitSaw = true;
				StartCoroutine("WaitSawMoveFirst");
			}
		} else if (GetGameState() == GameState.Play) {
			if (audio.clip.samples <= audio.timeSamples && beatIndex == -1) 
				GameEnd(true);

			SawEvent ();
			if (!gourdOpen) 
				ChangeProgressBar();

			labelPoint.text = gameScore.ToString();
		}	
	}

	public IEnumerator WaitSawMoveFirst () {
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
	
	public IEnumerator WaitSawMoveTime () {
		// 흥부전은 beatAction을 사용하지 않음 - 초기 방향 설정시에만 사용함
		if (beatIndex >= GourdBeatList.Count) {
			beatIndex = -1;
			return false;
		}

		BeatInfo beat = (BeatInfo) GourdBeatList[beatIndex];
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
			Incorrect();
		}
		
		// 필요 정보 초기화
		touchCount 	= 0;
		touchTime 	= 0.0f;
		gourdTime 	= 0.0f;
		waitTime 	= 0.0f;
		touchCheck 	= false;
		
		// 톱질 방향 변경
		if (sawDirection)
			SawAnimator.SetTrigger("SawTurnLeft");
		 else
			SawAnimator.SetTrigger("SawTurnRight");		
		sawDirection = !sawDirection;

		beatIndex++;
		waitSaw = false;
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
				// 방향 변경시 애니메이션 재생 속도를 지정
				// 파일 길이보다 긴시간 플레이 할 경우 게임 종료(Clear)
				if (beatIndex >= GourdBeatList.Count)
					GameEnd (true);

				if (!gourdOpen && !waitSaw && beatIndex > -1) {
					StartCoroutine("WaitSawMoveTime");
				}
			}

			waitTime += Time.deltaTime;
		} else if (SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawLeftToRight") || 
		           SawAnimator.GetCurrentAnimatorStateInfo(0).IsName("SawRightToLeft")) {
			// 정답일 경우 톱을 아래쪽으로 내림
			if (correctDownSaw) {
				SawAnimator.transform.position = new Vector3(SawAnimator.transform.position.x, 
				                                             SawAnimator.transform.position.y - (SawDonwValue * Time.deltaTime));

				for (int i = 0; i < resultMessage.Length; i++) {
					resultMessage[i].transform.guiTexture.pixelInset = new Rect(resultMessage[i].transform.guiTexture.pixelInset.x,
					                                                            resultMessage[i].transform.guiTexture.pixelInset.y - (SawDonwValue * Time.deltaTime * 100),
					                                                            resultMessage[i].transform.guiTexture.pixelInset.width,
					                                                            resultMessage[i].transform.guiTexture.pixelInset.height);
				}
			}
			
			// 톱질 애니메이션 재생 중일 때 해당값 초기화 
			// 우->좌로 이동시 해당 값이 초기화 되지 않는 문제가 발생하여 추가함
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
		if (gourdTime == 0.0f && touchCount == 0) {
			// Saw moving animation play
			touchTime = Time.fixedTime;
		} else if (gourdTime > 0.0f && touchCount == 0) {
			// Saw player input wait
			touchTime = Time.fixedTime;
			CorrectCheck();
		} else if (gourdTime == 0.0f && touchCount > 0) {
			// 이전 touch값에 대한 오답 처리
			Incorrect();
		} else if (gourdTime > 0.0f && touchCount > 0) {
			// saw wait state에서 이뤄진 touch에 대해서 처리(오답)
			Incorrect();
		}

		touchCount++;	
	}

	private void CorrectCheck() {
		float compareTime = Mathf.Abs (gourdTime - touchTime);
		//labelTime.text = compareTime.ToString ();
		touchCheck = true;

		// 시간에 따라 차등 점수 및 톱 내려간 위치 차등 조정하도록 함
		if (compareTime < CorrectTime1) {
			gameScore += (CorrectPoint1 + 2 * gameComboCount);
			
			if (sawDirection) resultMessage[ResultMessageRight].SendMessage("SetImage", 1);
			else resultMessage[ResultMessageLeft].SendMessage("SetImage", 1);
			Correct();
		} else if (compareTime < CorrectTime2) {
			gameScore += (CorrectPoint2 + gameComboCount);
			
			if (sawDirection) resultMessage[ResultMessageRight].SendMessage("SetImage", 2);
			else resultMessage[ResultMessageLeft].SendMessage("SetImage", 2);
			Correct();
		} else {
			incorrectCount++;
			Incorrect();
		}
	}

	// 정답 처리 - 공용 처리 부분
	void Correct() {		
		correctDownSaw = true;
		gameComboCount++;
		correctCount++;

		if (gameMaxCombo < gameComboCount)
			gameMaxCombo = gameComboCount;
	
		if (PlayerPrefs.GetInt("EffectSound") == 0) {
			AnotherSpaker.SendMessage("SoundPlay");
		}

		// 더이상 톱을 아래로 내릴 수 없을 때 박을 Open
		if (SawAnimator.transform.position.y < GourdOpenYValue) {
			// gourd open			
			StartCoroutine ("MakeParticle"); 
			gameScore += GourdOpenPoint;
		}

		GameTimeCorrect ();
	}

	void Incorrect() {
		GameTimeIncorrect ();
		correctDownSaw = false;
		gameComboCount = 0;

		if (sawDirection) 
			resultMessage[ResultMessageRight].SendMessage("SetImage", 3);
		else 
			resultMessage[ResultMessageLeft].SendMessage("SetImage", 3);
	}

	// 박 Open시 파티클 효과 재생 - 동전 1초간 막 나옴
	public IEnumerator MakeParticle() {
		gourdOpen = true;
		audio.Pause ();
		Object particle = Instantiate (gourdOpenEffect, new Vector3(0, -5, -10), transform.rotation);
		yield return new WaitForSeconds (GourdOpenTime);

		// init Saw position
		SawAnimator.transform.position = new Vector3(0, 0, 1);
		for (int i = 0; i < resultMessage.Length; i++) {
			resultMessage[i].transform.guiTexture.pixelInset = new Rect(resultMessage[i].transform.guiTexture.pixelInset.x,
			                                                            -50,
			                                                            resultMessage[i].transform.guiTexture.pixelInset.width,
			                                                            resultMessage[i].transform.guiTexture.pixelInset.height);
		}

		gourdOpen = false;
		audio.Play ();
		Destroy(particle);
	}
}
