using UnityEngine;
using System.Collections;

public class GameManagerPig : GameManager {
	private Vector3 GhostCreatePosition = new Vector3 (-7, 0, 1);
	private const string GhostTagName = "PigGhost";
	private const float GhostMoveTime = 1.5f;
	private const int BeatFileNum = 1;
	
	private bool waitGhost = false;
	public GUITexture[] resultMessage;
	public GameObject Ghost;
	public GameObject Wolf;
	public Animator[] pigAnimator;

	public GUIText show;

	void Start () {	
		ChangeUI ();		
		
		// 게임 로고 출력 - 로고 애니메이터 추가 필요
		LogoShow("Pig");
		if (!showLogo) 
			GameStart ();
	}
	
	public override void GameStart() {
		Init ();
		
		// 주요 변수 초기화
		waitGhost = false;
		
		// 비트 파일로부터 정보 읽어들이기
		int randomBeatFileNum = Random.Range (0, BeatFileNum);
		BeatNote = LoadBeatFileTime ("Beat/Pig" + randomBeatFileNum);
		beatIndex = 0;
		checkIndex = 0;
		
		InitBackgroundMusic ();
		AnotherSpaker.SendMessage ("Init", "Pig");
	}	
	
	public override void ResetGame () {
		audio.Stop ();

		DestoyItem ();
		StopCoroutine ("WaitGhost");
		pigAnimator [0].Play ("StandBy");
		pigAnimator [1].Play ("StandBy");
		pigAnimator [2].Play ("StandBy");
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
		} else if (GetGameState() == GameState.Play) {
			if (audio.clip.samples <= audio.timeSamples)
				GameEnd(true);
			
			if (!waitGhost) {
				waitGhost = true;
				
				// beat 재생 시작 이곳 혹은 GameState.Play에서 처리
				StartCoroutine("WaitGhost");
			}
		}	
	}
	
	public override void TouchHandlingGame(Touch touch) {
		Vector2 pos = touch.position;
		Vector3 theTouch = new Vector3(pos.x, pos.y);
		
		Ray ray = Camera.main.ScreenPointToRay(theTouch);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
			if (touch.phase == TouchPhase.Began) {
				if (hit.transform.name == "Pig1") {
					pigAnimator[0].SetTrigger("HitGhost");
					CorrectCheckPig (1);		
				} else if (hit.transform.name == "Pig2") {
					pigAnimator[1].SetTrigger("HitGhost");
					CorrectCheckPig (2);				
				} else if (hit.transform.name == "Pig3") {
					pigAnimator[2].SetTrigger("HitGhost");
					CorrectCheckPig (3);				
				}
			} else if (touch.phase == TouchPhase.Ended) {
			}
		}
	}
	public override void MouseHandlingGame() {
		RaycastHit hit = new RaycastHit();
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if (Physics.Raycast(ray.origin,ray.direction, out hit)) {   
			if (hit.transform.name == "Pig1") {
				pigAnimator[0].SetTrigger("HitGhost");
				CorrectCheckPig (1);
			} else if (hit.transform.name == "Pig2") {
				pigAnimator[1].SetTrigger("HitGhost");
				CorrectCheckPig (2);
			} else if (hit.transform.name == "Pig3") {
				pigAnimator[2].SetTrigger("HitGhost");
				CorrectCheckPig (3);
			}
		}
	}
	
	// beat note의 값과 비교하여 정답 체크
	public override void CorrectCheck() {}
	private void CorrectCheckPig(int pigNo) {
		for (int i = checkIndex; i < BeatNote.Count; i++) {
			BeatInfo beat = (BeatInfo) BeatNote[i];
			float compareTime = Mathf.Abs(beat.beatTime - audio.time);
			if (pigNo != beat.beatAction) continue;
			
			if (compareTime < CorrectTime1) {
				gameScore += (CorrectPoint1 + 2 * gameComboCount);
				PrintResultMessage(resultMessage[(beat.beatAction - 1)], (int) ResultMessage.Excellent);

				Correct();
				DestoryItem(i);
				checkIndex = i;
				break;
			} else if (compareTime < CorrectTime2) {
				gameScore += (CorrectPoint1 + gameComboCount);
				PrintResultMessage(resultMessage[(beat.beatAction - 1)], (int) ResultMessage.Good);
			
				Correct();
				DestoryItem(i);
				checkIndex = i;
				break;
			} else if (beat.beatTime < audio.time) {
				PrintResultMessage(resultMessage[(beat.beatAction - 1)], (int) ResultMessage.Miss);
				Incorrect();
			} else if (beat.beatTime > audio.time) {
				checkIndex = i;
				break;
			}
		}
	}
	
	public IEnumerator WaitGhost () {
		if (beatIndex < BeatNote.Count) {
			BeatInfo beat = (BeatInfo)BeatNote [beatIndex];

			if (beat.beatTime > audio.time) {
				waitGhost = true;
				Wolf.SendMessage("SetWolfMoveTime", (beat.beatTime - audio.time - GhostMoveTime));
				Wolf.SendMessage("SetWolfPosition", beat.beatAction);
				yield return new WaitForSeconds (beat.beatTime - audio.time - GhostMoveTime);

				if (beat.beatAction == 1) {
					GhostCreatePosition.y = 3;
				} else if (beat.beatAction == 2) {
					GhostCreatePosition.y = 0;
				} else if (beat.beatAction == 3) {
					GhostCreatePosition.y = -3;
				}
				GameObject ghost = (GameObject) Instantiate (Ghost, GhostCreatePosition, transform.rotation);
				ghost.SendMessage("SetGhostLane", beat.beatAction);
				ghost.SendMessage("SetBeatIndex", beatIndex);

				waitGhost = false;
				beatIndex++;		// 호출하는 위치에 따라 다른 위치에 있어야 함
			}
		}
	}
	
	private void DestoyItem() {
		GameObject[] ghostList = GameObject.FindGameObjectsWithTag (GhostTagName);
		if (ghostList.Length > 0) {
			for(int i = 0; i < ghostList.Length; i++) {
				Destroy(ghostList[i]);
			}
		}
	}
	private void DestoryItem(int index) {
		GameObject[] ghostList = GameObject.FindGameObjectsWithTag (GhostTagName);
		if (ghostList.Length > 0) {
			for(int i = 0; i < ghostList.Length; i++) {
				ghostList[i].SendMessage("DestroyGhost", index);
			}
		}
	}
	private void SetPrintMiss(int index) {
		GameObject[] ghostList = GameObject.FindGameObjectsWithTag (GhostTagName);
		if (ghostList.Length > 0) {
			for(int i = 0; i < ghostList.Length; i++) {
				ghostList[i].SendMessage("SetPrintMiss", index);
			}
		}
	}
	private void PrintMissMessage(int index) {
		missCount++;
		PrintResultMessage(resultMessage[(index - 1)], (int) ResultMessage.Miss);
	}
}
