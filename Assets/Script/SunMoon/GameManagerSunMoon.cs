using UnityEngine;
using System.Collections;

public enum HitZoneInItem {
	None = 0,
	Cake,
	Stone,
}
public class GameManagerSunMoon : GameManager {
	private const float HitZoneX = -5.00f;
	private const float HitZoneY = 1.50f;
	private const float MakeCakeX = 4.00f;
	private const float MakeCakeY = -4.50f;

	//public GameObject Tiger;
	public GameObject Cake;
	public GameObject Stone;
	public GUITexture resultMessage;
	public Animator TigerAnimator;
	
	public GUIText show;
	public GUIText show2;

	private int beatIndex = 0;
	private ArrayList CakeBeatList;
	private float FlyCakeTime = 1.333f;

	void Start () {	
		ChangeUI ();
		//LogoShow("SunMoon");
		//if (!showLogo) 
			GameStart ();
	}
	
	public override void GameStart() {
		Init ();

		// 비트 파일로부터 정보를 읽어들임
		if (CakeBeatList != null)
			CakeBeatList.Clear ();
		CakeBeatList = LoadBeatFile ("Beat/SunMoon01");
		beatIndex = 0;
		
		// 배경음악 재생 여부에 따라 음악 재생
		audio.clip = backgroundMusic;
		audio.volume = 1.0f;
		if (PlayerPrefs.GetInt ("BackgroundSound") != 0) 
			audio.volume = 0.0f;
		audio.Play ();
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
	
	void Update () {
		// 터치 이벤트 처리
		int count = Input.touchCount;		
		if (count == 1) {	
			TouchHandling (Input.touches [0]);
		} else if (Input.GetKeyDown (KeyCode.Space) && GetGameState() == GameState.Play) {
			CorrectCheck ();
		}
		
		// Back Key Touch
		BackKeyTouch ();
		
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoDelayTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();

			if (stateShow.texture == stateTexture[2]) {
				//StartCoroutine ("WaitThrowCake");
			}
		} else if (GetGameState() == GameState.Play) {	
			if (audio.clip.samples <= audio.timeSamples)
				GameEnd(true);

			// 비트 index가 0일 경우 
			if (beatIndex == 0) {
			}
		}	
	}

	public IEnumerator WaitThrowCake() {
		if (beatIndex < CakeBeatList.Count) {
			BeatInfo beat = (BeatInfo)CakeBeatList [beatIndex];
			float waitMoveTime = beat.beatTime - (audio.time + FlyCakeTime);
			yield return new WaitForSeconds (waitMoveTime);
	
			// beat.beatAction으로 떡과 돌을 구분
			GameObject makeCake = null;
			if (beat.beatAction == 1) {
				makeCake = (GameObject)Instantiate (Cake, new Vector3 (MakeCakeX, MakeCakeY), transform.rotation);
				makeCake.SendMessage ("SetTime", beat.beatTime);
			} else if (beat.beatAction == 2) {
				makeCake = (GameObject)Instantiate (Stone, new Vector3 (MakeCakeX, MakeCakeY), transform.rotation);
				makeCake.SendMessage ("SetTypeNo", HitZoneInItem.Stone);
				makeCake.SendMessage ("SetTime", beat.beatTime);
			}
			makeCake.SendMessage ("ThrowCake");
			beatIndex++;	// 여기서 증가를 시키지 않으면 Update 함수에서 계속 호출됨

			if (GetGameState () == GameState.Play) {
				//StartCoroutine ("WaitThrowCake");
			}
		}
	}

	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Began) {
			CorrectCheck ();
		} else if (touch.phase == TouchPhase.Ended) {
		}
	}

	private void CorrectCheck () {
		if (itemZoneInside == (int) HitZoneInItem.None) {
			Incorrect();
		} else if (itemZoneInside == (int) HitZoneInItem.Cake) {
			if (cakeInfo == null) return;

			float distance = DistanceCalculation(cakeInfo.transform.position.x, cakeInfo.transform.position.y);
			Debug.Log(distance.ToString());
			if (distance < CorrectDistance1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Excellent);
				Correct();				
				
				// 애니메이션 재생

				cakeInfo.SendMessage("DestroyCake");
			} else if (distance < CorrectDistance2) {
				gameScore += (CorrectPoint2 + gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Good);
				Correct();
				
				// 애니메이션 재생

				cakeInfo.SendMessage("DestroyCake");
			} else {
				Incorrect();
			}
		} else if (itemZoneInside == (int) HitZoneInItem.Stone) {
			if (cakeInfo == null) return;
			
			float distance = DistanceCalculation(cakeInfo.transform.position.x, cakeInfo.transform.position.y);
			if (distance < CorrectDistance1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Excellent);
				Correct();

				// 애니메이션 재생

				cakeInfo.SendMessage("DestroyCake");
			} else if (distance < CorrectDistance2) {
				gameScore += (CorrectPoint2 + gameComboCount);
				PrintResultMessage(resultMessage, (int) ResultMessage.Good);
				Correct();
				
				// 애니메이션 재생

				cakeInfo.SendMessage("DestroyCake");
			} else {
				Incorrect();
			}
		}
	}

	private float DistanceCalculation(float x, float y) {		
		float result = Mathf.Sqrt(Mathf.Pow ((HitZoneX - x), 2) + Mathf.Pow ((HitZoneY - y), 2));

		if (HitZoneX - x > 0) {
			result = Mathf.Abs(result - 0.5f);
		}
		return result;
	}

	private void Correct () {	
		correctCount++;
		gameComboCount++;
				
		if (gameMaxCombo < gameComboCount)
			gameMaxCombo = gameComboCount;
		
		if (PlayerPrefs.GetInt("EffectSound") == 0) {
			AnotherSpaker.SendMessage("SoundPlay");
		}
	}
	private void Incorrect() {
		gameComboCount = 0;
		incorrectCount++;
		
		PrintResultMessage(resultMessage, (int) ResultMessage.Miss);
	}
	
	private Cake cakeInfo;
	private int itemZoneInside = 0;
	public void HitZoneJoin(object item) {
		if (cakeInfo != null)
			Debug.Log ("Exist" + cakeInfo.GetTime().ToString());
		cakeInfo = (Cake)item;
		//show.text = cakeInfo.transform.position.x.ToString() + " : " + cakeInfo.transform.position.y.ToString ();
		//show2.text = DistanceCalculation (cakeInfo.transform.position.x, cakeInfo.transform.position.y).ToString ();
		itemZoneInside = cakeInfo.GetTypeNo();
	}
	public void HitZoneOut() {
		itemZoneInside = (int) HitZoneInItem.None;
		
		//show.text = cakeInfo.transform.position.x.ToString() + " : " + cakeInfo.transform.position.y.ToString ();
		//show2.text = DistanceCalculation (cakeInfo.transform.position.x, cakeInfo.transform.position.y).ToString ();
		cakeInfo = null;
	}
	public void HitTiger(object item) {
		Cake garbageCake = (Cake)item;
		garbageCake.SendMessage ("DestroyCake");
	}
}