using UnityEngine;
using System.Collections;

public enum AxState {
	Null,
	Gold,
	Silver,
	Bronze
}
public class GameManagerAx : GameManager {
	// UI start
	public GUIText printPoint;			// 점수 표시
	public GUIText printTime;			// 시간 표시
	// UI end

	private AxState AS = AxState.Null;	// 도끼 상태
	public Animator animator;			// 산신령 애니메이터

	private int showAxTime = 50;		// 문제 및 휴식 표시 시간
	private Vector2 touchPosition;		// 터치한 위치(스와이프시 터치 시작 위치 저장)
	private bool touchInput = false;

	private bool correctInput = false;

	//public AudioClip backgroundMusic;	// 배경음악
	//public GameObject AnotherSpaker;	// 효과음


	void Awake() {
		SetGameState(GameState.Ready);
		printPoint.text = gameScore.ToString();
	}

	void Start () {	
		ChangeUI ();
		GameStart();

		// 튜토리얼이 필요한지 검사한 후 필요할 경우 재생
		/*if (PlayerPrefs.GetInt ("TutorialAX") == 0) {
			SetGameState(GameState.Tutorial);
			// 설명 보여주는 창 Open

			UITutorial.SetActive(true);
			UITutorial.SendMessage("SetImage", "Ax");
		} else {
			GameStart();
		}*/
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		// 터치 이벤트 처리
		int count = Input.touchCount;		
		if (count == 1) {	
			TouchHandling(Input.touches[0]);
		}
		
		// back butoon touch
		if (Application.platform == RuntimePlatform.Android) {
			if (Input.GetKey(KeyCode.Escape) && GetGameState() == GameState.Play) {
				PauseOn();
			}
		}
		if (GetGameState() == GameState.Play) {
			//if (gameTime <= 0 || gameLife <= 0) GameEnd();
			
			// 일정 간격으로 도끼 변경
			if (AS == AxState.Null) {
				if (showAxTime == 0) {
					ShowAx ();
					showAxTime = 100;		// 난이도 조정시 해당 값 변경
				}
			} else if (AS != AxState.Null) {			
				if (showAxTime == 0) {
					animator.SetTrigger ("SetBasic");
					AS = AxState.Null;
					correctInput = false;
					showAxTime = 40;		// 난이도 조정시 해당 값 변경
				}
			}
			
			showAxTime--;
		}
	}

	public override void GameStart() {
		Init ();
		
		showAxTime = 50;
		touchInput = false;

		if (PlayerPrefs.GetInt("BackgroundSound") == 0) {
			audio.clip = backgroundMusic;
			//audio.loop = true;
			audio.Play ();
		}
		
		Time.timeScale = 1f;
		SetGameState(GameState.Play);
	}

	public override void ResetGame () {
		audio.Stop ();
	}

	/// <summary>
	/// Touchs the handling.
	/// </summary>
	public override void TouchHandlingGame (Touch touch) {
		// 버튼 터치가 아닌 경우
		if (touch.phase == TouchPhase.Began) {
			touchPosition = touch.position;
			touchInput = true;
		} else if (touch.phase == TouchPhase.Ended) {
			if (touchInput) {
				// 터치 위치 비교
				float x = Mathf.Abs (touch.position.x - touchPosition.x);
				float y = Mathf.Abs (touch.position.y - touchPosition.y);

				CheckCorrect (x, y);
			}

			touchInput = false;
		} 

		if (UIButton[(int)UIButtonList.Pause].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			PauseOn ();				
			touchInput = false;
		}
	}
	
	void CheckCorrect (float x, float y) {
		// 스와이프 방향 판별
		if (x > y) {
			// 좌 또는 우로 스와이프
			if (AS == AxState.Bronze)
				Incorrct ();
			else if (AS == AxState.Gold || AS == AxState.Silver)
				Correct ();
			else
				InputTimeOver ();
		} else {
			// 상 또는 하로 스와이프 
			if (AS == AxState.Bronze)
				Correct ();
			else if (AS == AxState.Gold || AS == AxState.Silver)
				Incorrct ();
			else
				InputTimeOver ();
		}
	}

	// 정답 처리
	// 현재 정답 입력시 계속 입력 가능
	// 다음 도끼가 나오기 전까지 입력 차단?
	void Correct() {
		if (correctInput) return;
		if (PlayerPrefs.GetInt("EffectSound") == 0) {
			AnotherSpaker.SendMessage("SoundCorrectPlay");
		}

		correctInput = true;
		correctCount++;
		gameComboCount++;

		// 점수는 우선 고정(추후 난이도 or Combo에 따라 가산점)
		gameScore += 10;
		printPoint.text = gameScore.ToString();

		// 일정 콤보시 애니메이션 속도 증가
		// 난이도 조정시 해당 값 변경
		//if (comboCount % 10) animator.speed += 0.1f;
	}

	// 오답 처리
	void Incorrct() {
		if (correctInput) return;
		Handheld.Vibrate ();
		
		correctInput = true;
		incorrectCount++;
		//gameLife--;
		gameComboCount = 0;
		animator.speed = 1.0f;
	}

	// 도끼가 보여지지 않을때 처리?
	void InputTimeOver() {
		//printText.text = "TimeOver";
	}

	// 도끼를 일정 간격으로 선택하여 보여줌
	void ShowAx() {
		int selectAx = Random.Range(0, 3);

		if (selectAx == 0) {
			animator.SetTrigger ("SetGold");
			AS = AxState.Gold;
		} else if (selectAx == 1) {
			animator.SetTrigger ("SetSilver");
			AS = AxState.Silver;
		} else if (selectAx == 2) {
			animator.SetTrigger ("SetBronze");
			AS = AxState.Bronze;
		}
	}
}