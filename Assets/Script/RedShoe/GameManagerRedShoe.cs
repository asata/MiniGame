using UnityEngine;
using System.Collections;

public enum ThinkState {
	ThinkStart, 
	Thinking,
	ThinkEnd,
	InputWait
}
public class GameManagerRedShoe : GameManager {
	private const int BeatFileNum = 1;
	private const int LeftLeg = 7;
	private const int RightLeg = 8;
	private float touchHalf = 0;
	private ThinkState TS = ThinkState.ThinkStart;
	private int touchCount = 0;
	private int kickCount = 0;

	private bool waitThink = false;
	public GUITexture resultMessage;
	public Animator playerFootAnimator;
	public Animator thinkFootAnimator;
	//public GameObject[] cloud;

	void Start () {	
		touchHalf = Screen.width / 2;
		ChangeUI ();		
		
		// 게임 로고 출력 - 로고 애니메이터 추가 필요
		LogoShow("RedShoe");
		if (!showLogo) 
			GameStart ();
	}
	
	public override void GameStart() {
		Init ();
		
		// 주요 변수 초기화
		waitThink = false;
		TS = ThinkState.InputWait;

		// 비트 파일로부터 정보 읽어들이기
		int randomBeatFileNum = Random.Range (0, BeatFileNum);
		BeatNote = LoadBeatFile ("Beat/RedShoe" + randomBeatFileNum);
		beatIndex = 0;
		checkIndex = 0;
		touchCount = 0;
		kickCount = 0;
		
		InitBackgroundMusic ();
		AnotherSpaker.SendMessage ("Init", "RedShoe");
	}	
	
	public override void ResetGame () {
		audio.Stop ();
		
		StopCoroutine ("Thinking");

		playerFootAnimator.Play ("PlayerWait");
		thinkFootAnimator.Play ("ThinkWait");
	}
	
	void Update () {
		// 터치 이벤트 처리
		int count = Input.touchCount;		
		if (count == 1) {	
			TouchHandling(Input.touches[0]);
		} else if (Input.GetMouseButtonDown(0)) {
			MouseHandling();
		}
		
		// Back Key TouchS
		BackKeyTouch ();
		
		if (GetGameState () == GameState.Logo) {
			if(showLogo) StartCoroutine("LogoDelayTime");
		} else if (GetGameState () == GameState.Ready) {
			GameReady();
		} else if (GetGameState() == GameState.Play) {
			if (audio.clip.samples <= audio.timeSamples) {
				RhythmTurnEnd();
				GameEnd(true);
			}

			ShoeEvent();
		}	
	}
	
	public override void TouchHandlingGame(Touch touch) {
		if (touch.phase == TouchPhase.Began) {
			if(touch.position.x < touchHalf) {
				// left
				playerFootAnimator.SetTrigger("SetLeftLegUp");
				CorrectCheckLeg(LeftLeg);
			} else {
				// right
				playerFootAnimator.SetTrigger("SetRightLegUp");
				CorrectCheckLeg(RightLeg);
			}

			touchCount++;
		} else if (touch.phase == TouchPhase.Ended) {
		}
	}

	public override void MouseHandlingGame() {
		if (Input.mousePosition.x < 0) {
			// left
			playerFootAnimator.SetTrigger("SetLeftLegUp");
			CorrectCheckLeg(LeftLeg);
		} else {
			// right
			playerFootAnimator.SetTrigger("SetRightLegUp");
			CorrectCheckLeg(RightLeg);
		}

		touchCount++;
	}
	
	// beat note의 값과 비교하여 정답 체크
	public override void CorrectCheck() {}
	private void CorrectCheckLeg(int legNo) {
		if (BeatNote == null) return;
		
		for (int i = checkIndex; i < BeatNote.Count; i++) {
			BeatInfo beat = (BeatInfo) BeatNote[i];
			if (beat.beatAction != 8 && beat.beatAction != 9) continue;
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
	
	private IEnumerator Thinking () {
		if (beatIndex < BeatNote.Count) {
			BeatInfo beat = (BeatInfo)BeatNote [beatIndex];
			float waitTime = beat.beatTime - audio.time;
			waitThink = true;
			yield return new WaitForSeconds (waitTime);
			
			if (beat.beatAction == 3) {
				thinkFootAnimator.SetTrigger("SetJump");
			} else if (beat.beatAction == 1) {
				thinkFootAnimator.SetTrigger("SetLeftLegUp");
				kickCount++;
			} else if (beat.beatAction == 2) {
				thinkFootAnimator.SetTrigger("SetRightLegUp");
				kickCount++;
			} else if (beat.beatAction == 9) {
				playerFootAnimator.SetTrigger("SetJump");
				TS = ThinkState.InputWait;
			}
			
			waitThink = false;
			beatIndex++;		// 호출하는 위치에 따라 다른 위치에 있어야 함
		}
	}

	private void ShoeEvent() { 
		if (TS == ThinkState.InputWait) {
			if (beatIndex < BeatNote.Count) {
				BeatInfo nextBeat = (BeatInfo)BeatNote [beatIndex];

				if (nextBeat.beatAction == 7 || nextBeat.beatAction == 8 || nextBeat.beatAction == 9) {
					beatIndex++;
				} else if ((nextBeat.beatTime - audio.time) <= RabbitWaitInputTime) {
					RhythmTurnEnd();
					TS = ThinkState.Thinking;
				}
			}
		} else if (TS == ThinkState.Thinking) {
			if(!waitThink) StartCoroutine("Thinking");
		}
	}
	
	private void RhythmTurnEnd() {
		if (kickCount == touchCount) {
			if(gameComboCount >= touchCount)
				gameScore += PoundingAllPoint;
		} else if (kickCount > touchCount) {
			missCount = kickCount - touchCount;
			gameComboCount = 0;
		}
		
		touchCount = 0;
		kickCount = 0;
	}
}