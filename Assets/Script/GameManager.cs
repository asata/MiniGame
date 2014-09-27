using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

public class BeatInfo {
	public float beatTime;		// 처음부터 시간
	public float intervalTime;	// 비트간 간격
	public int beatAction;		// 해당 동작
	
	public BeatInfo(float aInterval, int aAction) {
		intervalTime = aInterval;
		beatAction = aAction;
	}
	public BeatInfo(float aBeat, float aInterval, int aAction) {
		beatTime = aBeat;
		intervalTime = aInterval;
		beatAction = aAction;
	}
}

public enum GameState {
	//Tutorial,
	Ready,
	Play,
	Pause,
	End
}
public enum ResultMessage {
	Null = 0,
	Excellent,
	Good,
	Miss
}

public enum UIGroupList {
	UIPause = 0,
	UIEnd
}
public enum UIButtonList {
	Pause = 0,
	UnPause,
	RestartPause,
	MainPause,
	RestartEnd,
	MainEnd,
	FacebookEnd
}
// 게임 종료시 나타날 GUIText Array
public enum GameEndTextNumber {
	TotalScore = 0,
	PlayTime,
	MaxCombo,
	HighScore,
	Grade
}
abstract public class GameManager : MonoBehaviour {	
	private   const string	StageDBName = "/StageInfo.db";
	private   const int 	GameCount = 2;					// 현재 개발된 게임 갯수
	private   const int 	NextGameOpenScore = 3000;		// 다음 스테이지를 여는데 필요한 최소 점수

	// 게임 시작시 Ready, Start 메시지 출력 시간
	private   const float 	ShowReadyTime = 2.0f;
	private   const float 	ShowStartTime = 2.5f;

	// 정답 체크 시간 
	protected const float 	CorrectTime1 = 0.05f;
	protected const float 	CorrectTime2 = 0.15f;
	
	protected const float 	RabbitWaitInputTime = 0.2f;		// 달토끼 - 사용자 입력 대기 추가 시간
	protected const float 	HeungbuWaitInputTime = 0.2f;	// 흥부전 - 사용자 입력 대기 추가 시간
	protected const float 	GourdOpenTime = 2.0f;			// 흥부전 - 박이 열리는 시간

	protected const float 	GameSpeedStop 	= 0.0f;
	protected const float	GameSpeedNormal = 1.0f;	

	// 정답 포인트
	protected const int 	CorrectPoint1 = 500;
	protected const int 	CorrectPoint2 = 300;
	
	protected const int 	PoundingAllPoint 	= 50;		// 달토끼 - 해당 턴에서 모두 터치를 한 경우
	protected const int 	GourdOpenPoint 		= 200;		// 흥부전 - 박을 연 경우
	private   const int 	BeatEndPoint 		= 800;		// 지정된 파일을 모두 연주한 경우

	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	private GameState GS;
	protected int gameScore;
	protected int gameComboCount;
	protected int gameMaxCombo;
	protected int correctCount;
	protected int incorrectCount;
	protected int missCount;
	protected float playTime;	
	private bool firstStart = false;
	private bool readyState = false;	
	
	public AudioClip backgroundMusic;	// 배경음악
	public GameObject AnotherSpaker;	// 효과음
	public Texture2D logoImageTexture;
	public Animator logoAnimator;

	public GameState GetGameState() {
		return GS;
	}
	public void SetGameState(GameState state) {
		GS = state;
	}

	public IEnumerator LogoShow (string gameName) {
		gameLogo.SetActive (true);
		gameLogo.guiTexture.texture = logoImageTexture;
		if (gameName =="MoonRabbit") logoAnimator.SetTrigger ("ShowMoonRabbit");
		else if (gameName =="Heungbu") logoAnimator.SetTrigger ("ShowHeungbu");

		yield return new WaitForSeconds (1.0f);

		gameLogo.SetActive (false);
	}

	public void Init() {
		GS = GameState.Ready;
		
		if (Application.platform == RuntimePlatform.Android)
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// 애니메이션 초기화
		if (firstStart) 
			ResetGame ();
		firstStart = true;

		// 주요 변수 초기화
		gameScore = 0;
		gameMaxCombo = 0;
		gameComboCount = 0;
		correctCount = 0;
		incorrectCount = 0;
		missCount = 0;

		playTime = 0.0f;
		readyState = false;

		// 게임 기본 설정
		Time.timeScale = GameSpeedNormal;
		stateTime = 0f;
		stateShow.texture = stateTexture [0];
	}
	                          
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////


	/// <summary>
	/// 게임 화면 UI
	/// </summary>
	public GameObject	gameLogo;
	public GUITexture 	stateShow;			// 게임 시작시 Ready, Start 출력
	public Texture2D[] 	stateTexture;		// stateShow에 출력되는 이미지
	protected float 	stateTime;			// stateShow가 표시될 시간

	public GameObject[] UIGroup;
	public GUITexture[] UIButton;
	public GUIText[] 	labelGameEnd;		// 게임 종료시 출력될 점수들

	public void GameReady () {
		if(stateTime > ShowStartTime) {
			readyState = true;
			SetGameState(GameState.Play);
			stateShow.texture = stateTexture [2];
		} else if(stateTime > ShowReadyTime) {
			stateShow.texture = stateTexture [1];
		} 

		stateTime += Time.deltaTime;
	}

	public void ChangeUI () {
		UIGroup[(int)UIGroupList.UIPause].SetActive (true);
		UIGroup[(int)UIGroupList.UIEnd].SetActive (true);
		
		ChangeUISize ();

		UIGroup[(int)UIGroupList.UIPause].SetActive (false);
		UIGroup[(int)UIGroupList.UIEnd].SetActive (false);
	}	

	// UI를 화면에 맞게 조정
	private void ChangeUISize() {			
		// 화면 해상도 처리 시작
		Screen.SetResolution (Screen.width, Screen.height, true);
		float guiRatio = Screen.width / 1600.0f;
		// 화면 해상도 처리 끝
		
		string strTexture = "UITexture";
		string strText = "UIText";

		GameObject[] UITextureList = GameObject.FindGameObjectsWithTag (strTexture);
		foreach (GameObject temp in UITextureList) {
			if (!temp.transform.guiTexture.enabled) continue;
			temp.transform.guiTexture.pixelInset = new Rect(temp.transform.guiTexture.pixelInset.x * guiRatio,
			                                                temp.transform.guiTexture.pixelInset.y * guiRatio,
			                                                temp.transform.guiTexture.pixelInset.width * guiRatio,
			                                                temp.transform.guiTexture.pixelInset.height * guiRatio);
		}
		
		GameObject[] UITextList = GameObject.FindGameObjectsWithTag (strText);
		foreach (GameObject temp in UITextList) {
			if (!temp.transform.guiText.enabled) continue;
			temp.transform.guiText.pixelOffset = new Vector2 (temp.transform.guiText.pixelOffset.x * guiRatio,
			                                                  temp.transform.guiText.pixelOffset.y * guiRatio);
			temp.transform.guiText.fontSize = (int)(temp.transform.guiText.fontSize * guiRatio);
		}
	}

	// 남은 시간 표시
	public void ChangeProgressBar() {
	/*	gameTime -= Time.deltaTime;
		playTime += Time.deltaTime;

		//if (gameTime < 0) GameEnd ();
		progressTimeBar.transform.guiTexture.pixelInset = new Rect(progressTimeBar.transform.guiTexture.pixelInset.x,
		                                                           progressTimeBar.transform.guiTexture.pixelInset.y,
		                                                           gameTime / MaxGameTime * progressBarWidth,
		                                                           progressTimeBar.transform.guiTexture.pixelInset.height);*/
	}
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////



	/// <summary>
	/// 게임 일시 정지, 해제, 종료등 게임 진행 UI 관련
	/// </summary>
	public void PauseOn() {
		if (Application.platform == RuntimePlatform.Android)
			Screen.sleepTimeout = SleepTimeout.SystemSetting;

		Time.timeScale = GameSpeedStop;
		audio.Pause ();
		GS = GameState.Pause;
		UIGroup[(int)UIGroupList.UIPause].SetActive (true);
	}
	public void PauseOff() {
		if (Application.platform == RuntimePlatform.Android)
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

		Time.timeScale = GameSpeedNormal;
		audio.Play ();
		if (readyState)
			GS = GameState.Play;
		else
			GS = GameState.Ready;
		UIGroup[(int)UIGroupList.UIPause].SetActive (false);
	}
	public void GameEnd(bool beatEnd = false) {		
		if (Application.platform == RuntimePlatform.Android)
			Screen.sleepTimeout = SleepTimeout.SystemSetting;

		UIGroup[(int)UIGroupList.UIEnd].SetActive (true);
		UIButton[(int)UIButtonList.FacebookEnd].gameObject.SetActive (FB.IsLoggedIn);

		Time.timeScale = GameSpeedStop;
		audio.Stop ();
		GS = GameState.End;

		// 점수에따라 등급 세팅
		// DB에는 기록 하지 않음
		string grade = CalGrade();

		if (beatEnd)
			gameScore += BeatEndPoint;

		int gameNo = PlayerPrefs.GetInt ("GameNo");
		int highScore = PlayerPrefs.GetInt ("HighScore");
		if (highScore <= gameScore) {
			// DB기록 갱신
			highScore = gameScore;
			PlayerPrefs.SetInt ("HighScore", highScore);
			UpdateHighScore(gameNo, highScore, grade);

			// 다음 열 게임이 있는지 검사
			// 있을 경우 Open한지 검사 후 열도록 함
			// 가짜버튼도 있어서 제한을 걸어야 함
			if (highScore >= NextGameOpenScore && gameNo < GameCount) {
				bool newGameOpen = NextGameOpen(gameNo);			
				if(newGameOpen) {
					// 새게임 오픈시 알람창 오픈하도록 함??
					//Debug.Log("새게임 오픈");
				}
			}
		}

		// UI에 게임 기록 출력
		labelGameEnd [(int)GameEndTextNumber.TotalScore].text = gameScore.ToString();
		labelGameEnd [(int)GameEndTextNumber.PlayTime].text = playTime.ToString("00");	// 초단위로 출력함, 분으로 변환시 수정 필요
		labelGameEnd [(int)GameEndTextNumber.MaxCombo].text = gameMaxCombo.ToString();
		labelGameEnd [(int)GameEndTextNumber.HighScore].text = highScore.ToString ();
		labelGameEnd [(int)GameEndTextNumber.Grade].text = grade;
	}

	private void UpdateHighScore (int gameNo, int highScore, string grade) {		
		string strOut = Application.persistentDataPath + StageDBName;
		SqliteDatabase sql = new SqliteDatabase();
		sql.Open(strOut);
		sql.ExecuteQuery("update StageInfo set score=" + highScore + ", grade='" + grade + "' where id=" + gameNo);
		sql.Close();
	}

	private bool NextGameOpen(int gameNo) {
		string strOut = Application.persistentDataPath + StageDBName;
		SqliteDatabase sql = new SqliteDatabase();
		sql.Open(strOut);

		DataTable _data = sql.ExecuteQuery("select * from StageInfo where id=" + (gameNo + 1));
		bool result = false;

		// 다음 게임이 있는지 검사
		if (_data.Rows.Count == 1) {
			// 다음 게임이 열려져있는지 검사
			if (int.Parse(_data.Rows [0]["open"].ToString()) == 0) {
				sql.ExecuteQuery("update StageInfo set open=1 where id=" + (gameNo + 1));
				result = true;
			}
		}

		sql.Close();
		return result;
	}

	// 게임별로 등급을 다르게???
	private string CalGrade() {
		if (gameScore > 5000) {
			return "A";
		} else if (gameScore > 3000) {
			return "B";
		} else if (gameScore > 1000) {
			return "C";
		} else if (gameScore > 500) {
			return "D";
		} else if (gameScore > 100) {
			return "E";
		} else {
			return "F";
		}
	}
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////

		
	void OnApplicationPause(bool pause) {
		if (!pause && GS != GameState.End) {
			PauseOn();
		}
	}
	public void BackKeyTouch () {
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyUp (KeyCode.Escape)) {
			if (GS == GameState.Ready || GS == GameState.Play) {
				UIGroup[(int)UIGroupList.UIPause].SendMessage("ShowPausePanel");
				PauseOn ();
			} else if (GS == GameState.Pause) {
				PauseOff ();
			} else if (GS == GameState.End) {
				Time.timeScale = GameSpeedNormal;
				Application.LoadLevel("GameSelect");
			}
		}
	}

	// 아래 함수는 각 게임에서 구현
	public abstract void GameStart ();		// 게임 시작시 초기 설정
	public abstract void ResetGame ();	
	public abstract void TouchHandlingGame (Touch touch);

	public void TouchHandling(Touch touch) {
		if (GS == GameState.Ready && touch.phase == TouchPhase.Ended) {
			if (UIButton[(int)UIButtonList.Pause].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				UIGroup[(int)UIGroupList.UIPause].SendMessage("ShowPausePanel");
				PauseOn ();
			}
		} else if (GS == GameState.Play) {
			// 해당 게임으로 이동 처리 하도록 함
			TouchHandlingGame (touch);
		} else if (GS == GameState.Pause && touch.phase == TouchPhase.Ended) {
			if (UIButton[(int)UIButtonList.UnPause].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				PauseOff ();
			} else if (UIButton[(int)UIButtonList.RestartPause].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				UIGroup[(int)UIGroupList.UIPause].SetActive (false);
				GameStart();
			} else if (UIButton[(int)UIButtonList.MainPause].HitTest(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				Time.timeScale = GameSpeedNormal;
				Application.LoadLevel("GameSelect");
			} else {
				UIGroup[(int)UIGroupList.UIPause].SendMessage("PauseTouchHandling");
			} 
		} else if (GS == GameState.End && touch.phase == TouchPhase.Ended) {
			// 게임 종료 상태일 경우
			if (UIButton[(int)UIButtonList.RestartEnd].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				UIGroup[(int)UIGroupList.UIEnd].SetActive (false);
				GameStart();
				/*} else if (UIButtonList[(int)UIButtonList.FacebookEnd].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				FB.Feed(link:"http://asata.pe.kr",
				        linkName:"teamSF Game",
				        linkCaption:"최고 기록 갱신");*/
			} else if (UIButton[(int)UIButtonList.MainEnd].HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
				Time.timeScale = GameSpeedNormal;
				Application.LoadLevel("GameSelect");
			} 
		}
	}

	// CSV 파일 읽기 관련 처리
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////		
	public ArrayList LoadBeatFile(string fileName) {
		TextAsset csv = (TextAsset)Resources.Load<TextAsset> (fileName);
		string[,] csvData = SplitCsvGrid (csv.text);
		
		ArrayList BeatList = new ArrayList ();
		
		for (int n = 0; n < csvData.GetUpperBound(1); n++) {
			if (csvData[0, n] == null) continue;
			
			BeatInfo beat = new BeatInfo(float.Parse(csvData[0, n]), int.Parse(csvData[1, n]));
			BeatList.Add(beat);
		}
		
		return BeatList;
	}
	public ArrayList LoadBeatFileTime(string fileName) {
		TextAsset csv = (TextAsset)Resources.Load<TextAsset> (fileName);
		string[,] csvData = SplitCsvGrid (csv.text);
		
		ArrayList BeatList = new ArrayList ();
		
		for (int n = 0; n < csvData.GetUpperBound(1); n++) {
			if (csvData[0, n] == null) continue;
			
			BeatInfo beat = new BeatInfo(float.Parse(csvData[0, n]), float.Parse(csvData[1, n]), int.Parse(csvData[2, n]));
			BeatList.Add(beat);
		}
		
		return BeatList;
	}

	static private string[,] SplitCsvGrid(string csvText) {
		string[] lines = csvText.Split ("\n" [0]);
		int width = 0;
		
		for (int i = 0; i < lines.Length; i++) {
			string[] row = SplitCsvLine(lines[i]);
			
			width = Mathf.Max(width, row.Length);
		}
		
		string[,] outputGrid = new string[width + 1, lines.Length + 1];
		for (int y = 0; y < lines.Length; y++) {
			string[] row = SplitCsvLine(lines[y]);
			
			for (int x = 0; x < row.Length; x++) {
				outputGrid[x, y] = row[x];
				outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
			}
		}
		
		return outputGrid;
	}
	
	static private string[] SplitCsvLine(string line) {
		return (from Match m in Regex.Matches (line, 
		                                       @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
		                                       RegexOptions.ExplicitCapture) select m.Groups [1].Value).ToArray ();
	}
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
	/////////////////////////////////////////////////////
}
