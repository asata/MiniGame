using UnityEngine;
using System.Collections;

public class GameManagerSunMoon : GameManager {
	public GUIText labelTime;

	public GameObject Tiger;
	public GameObject Cake;
	public GameObject CakeItem;
	public GUITexture resultMessage;

	private int beatIndex = 0;
	private ArrayList CakeBeatList;

	void Start () {	
		ChangeUI ();
		GameStart();
	}
	
	public override void GameStart() {
		Init ();

		beatIndex = 0;
		// 비트 파일로부터 정보를 읽어들임
		if (CakeBeatList != null)
			CakeBeatList.Clear ();
		CakeBeatList = LoadBeatFile ("Beat/MoonRabbit01");
		
		if (PlayerPrefs.GetInt("BackgroundSound") == 0) {
			audio.clip = backgroundMusic;
			//audio.loop = true;
			audio.volume = 1.0f;
			audio.Play ();
		}
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
		} else if (GetGameState() == GameState.Play) {	
			// 비트 index가 0일 경우 
			if (beatIndex == 0) {
				StartCoroutine ("WaitThrowCake");
			}
		}	
	}

	public IEnumerator WaitThrowCake() {
		BeatInfo beat = (BeatInfo) CakeBeatList[beatIndex];
		beatIndex++;	// 여기서 증가를 시키지 않으면 Update 함수에서 계속 호출됨
		yield return new WaitForSeconds (beat.intervalTime);
				
		// beat.beatAction으로 떡과 돌을 구분
		GameObject makeCake = null;
		if (beat.beatAction == 1) {
			makeCake = (GameObject) Instantiate(Cake, new Vector3(8.0f, -0.4f), transform.rotation);
		} else if (beat.beatAction == 2){
			makeCake = (GameObject) Instantiate(CakeItem, new Vector3(8.0f, -0.4f), transform.rotation);
			makeCake.SendMessage("SetItemName", "Stone");
		}
		makeCake.SendMessage("ThrowCake");

		if (GetGameState () == GameState.Play) {			
			StartCoroutine ("WaitThrowCake");
		}
	}
	
	public override void ResetGame () {
		if (CakeBeatList != null)
			CakeBeatList.Clear ();
		
		audio.Stop ();
		StopCoroutine ("WaitThrowCake");

		// 호랑이 애니메이션 종료 및 바닥에 착지하도록 함 - 이건 애니메이션이 적용되면 하도록 함.
		//RabbitAnimator.Play ("RabbitReady");
		//PlayerAnimator.Play ("player_hand_wait");
	}

	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Ended) {
			// 연속 점프등은 추후 고려
			Tiger.SendMessage("Jump");
		}
	}

	// Tiger Trigger에서 호출됨
	public void CatchCake() {
		// 추후 콤보시 점수를 추가로 주도록 함
		gameScore += (CorrectPoint1 + 2 * gameComboCount);
		gameComboCount++;
		resultMessage.SendMessage("SetImage", 1);
	}
	public void HitCake() {
		gameScore += (CorrectPoint2 + gameComboCount);
		resultMessage.SendMessage("SetImage", 2);
	}
	private void Correct () {		
		correctCount++;
		
		if (gameMaxCombo < gameComboCount)
			gameMaxCombo = gameComboCount;
		
		if (PlayerPrefs.GetInt("EffectSound") == 0) {
			AnotherSpaker.SendMessage("SoundPlay");
		}
	}
	public void MissCake() {
		Debug.Log ("Miss Cake");
		resultMessage.SendMessage("SetImage", 3);
		gameComboCount = 0;
	}
	public void CatchItem(string itemName) {
		if (itemName == "Stone") {
			// 돌을 잡거나 맞았을 경우
		}
		/*} else if (itemName == "aaa") {
		} else if (itemName == "aaa") {
		} else if (itemName == "aaa") {
		} else if (itemName == "aaa") {
		} else if (itemName == "aaa") {
		} else {
		} */
	}
}