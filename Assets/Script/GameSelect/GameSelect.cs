using UnityEngine;
using System.Collections;

public enum FixedTextureName {
	GameStart = 0,
	InfoClose,
	Tutorial
}

public enum FixedTextName {
	HighScore = 0
}

public class GameSelect : MonoBehaviour {
	private const float backgroundMoveSpeed 	= 2.5f;
	private const float backgroundWidth 		= 20;
	private const float backgroundHeight 		= 10;
	private const int 	DatabaseVersion1 		= 1;
	private const int 	DatabaseVersion2 		= 2;
	private const int 	DatabaseVersion3 		= 3;
	private float guiRatio;							// 화면 비율

	private Fade fade;
	private DataTable _data;						// SQL
	private ArrayList GameList;						// SQL에서 읽은 게임 정보
	public GUITexture buttonOption;
	public GUITexture buttonGameStart;				// 게임 선택 UI에서 나타남
	public GUITexture buttonInfoClose;				// 게임 선택 UI에서 나타남

	public GameObject[] buttonList;					// 게임 선택할 버튼 목록
	public Texture[] buttonImage;					// 게임 버튼에 표시될 이미지

	public Texture2D[] tutorialImage;

	private int resizeIndex = -1;
	public GUITexture[] fixedTexture;
	public GUIText[] fixedText;	

	public GameObject UIGameInfo;	
	public GameObject UIOption;	
	public GameObject UIShop;	

	private bool gameInfoHide = false;

	// 배경화면 이동 관련 변수
	public GameObject BackgroundImage;
	private GameObject[] BackgroundImageBlock;
	private GameObject BackgroundImageB;
	private GameObject BackgroundImageC;
	private GameObject BackgroundImageD;
	
	void Start () { 	
		Time.timeScale = 1.0f;

		if (Application.platform == RuntimePlatform.Android)
			Screen.sleepTimeout = SleepTimeout.SystemSetting;

		BackgroundImageBlock = new GameObject[9];
		int blockCount = 0;
		// 이동할 배경화면
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				BackgroundImageBlock[blockCount] = Instantiate (BackgroundImage, new Vector3 (backgroundWidth * j, backgroundHeight * i, 0), transform.rotation) as GameObject;
				blockCount++;
			}
		}

		fade = GameObject.Find("BackgroundGameInfo").GetComponent<Fade> ();
		GameList = new ArrayList ();
	
		LoadGameInfo ();
		AddGameButton ();

		UIOption.SetActive (true);
		UIShop.SetActive (true);
		UIGameInfo.SetActive (true);

		ChangeUISize ();
		fade.SetGUIRatio (guiRatio);
		fade.HideInfo ();
		UIShop.SendMessage ("SetGUIRatio", guiRatio);

		UIOption.SetActive (false);
		UIShop.SetActive (false);
		UIGameInfo.SetActive (false);
	}

	private void ReadSQL () {
		string strOut = Application.persistentDataPath + "/StageInfo.db";

		/*if (!System.IO.File.Exists(strOut)) {
			//Debug.Log("Exists ******  " + strOut);
			TextAsset ta = (TextAsset)Resources.Load("eng.db");
			System.IO.File.WriteAllBytes(strOut, ta.bytes); // 64MB limit on File.WriteAllBytes.
			ta = null;
		} else {
			//Debug.Log("Exists ******  " + strOut);
		}*/
		
		SqliteDatabase sql = new SqliteDatabase();
		sql.Open(strOut);

		int db_version = PlayerPrefs.GetInt ("DatabaseVersion");
		// 최종빌드 이후에는 버전에 따라 디비를 추가, 수정
		// 추가시 그냥 insert구문 실행 혹은 버전과 관계없이 갯수 파악 후 추가
		// 수정시 해당항목 update or 미존재시 추가하도록 함
		if (db_version < DatabaseVersion1) {
			sql.ExecuteQuery("drop table if exists StageInfo");
		} else if (db_version < DatabaseVersion2) {
			sql.ExecuteQuery("update StageInfo set scene='SunMoon', open=1 where id=3");
			sql.ExecuteQuery("update StageInfo set scene='Gildong', open=1 where id=4");
		} else if (db_version < DatabaseVersion3) {
			sql.ExecuteQuery("update StageInfo set scene='Gildong', open=1 where id=4");
		}

		// 이전 게임을 했던 기기라면 StageInfo를 삭제하고 해야함
		sql.ExecuteQuery("create table if not exists StageInfo(id int primary key not null, scene text, open  integer default 0, score integer default 0, grade text)");

		_data = sql.ExecuteQuery("select * from StageInfo");
		if (_data.Rows.Count == 0) {
			sql.ExecuteQuery("insert into StageInfo values(1, 'MoonRabbit', 1, 0, 'F')");
			sql.ExecuteQuery("insert into StageInfo values(2, 'Heungbu', 1, 0, 'F')");
			sql.ExecuteQuery("insert into StageInfo values(3, 'SunMoon', 1, 0, 'F')");
			sql.ExecuteQuery("insert into StageInfo values(4, 'Gildong', 1, 0, 'F')");
			sql.ExecuteQuery("insert into StageInfo values(5, 'Test4', 0, 0, 'F')");

			_data = sql.ExecuteQuery("select * from StageInfo");
		}
		
		PlayerPrefs.SetInt("DatabaseVersion", DatabaseVersion3);
		sql.Close();
	}

	private void LoadGameInfo() {		
		ReadSQL ();	

		if (null == _data) return;

		for (int i = 0; i < _data.Rows.Count; i++) {
			DataRow dataRow = _data.Rows[i];
			GameInfo info = new GameInfo(int.Parse(dataRow["id"].ToString()), 
			                             dataRow["scene"].ToString(), 
			                             int.Parse(dataRow["open"].ToString()), 
			                             int.Parse(dataRow["score"].ToString()), 
			                             dataRow["grade"].ToString());
			GameList.Add(info);
		}		
	}

	private void AddGameButton() {		
		buttonList = new GameObject[GameList.Count];

		for (int i = 0; i < GameList.Count; i++) {
			GameInfo info = (GameInfo) GameList[i];

			GameObject button = GameObject.CreatePrimitive (PrimitiveType.Quad);
			button.AddComponent<GUITexture> ();

			button.name = info.scene;
			button.tag = "UITexture";
			if (info.open)
				button.guiTexture.texture = buttonImage [info.no];
			else 
				button.guiTexture.texture = buttonImage [0];

			button.transform.position = new Vector3 (0.5f, 0.5f, 0);
			button.transform.localScale = new Vector3 (0, 0, 0);
			button.transform.guiTexture.pixelInset = new Rect (-100 + ((i - 2) * 260), -100, 200, 200);
			
			buttonList[i] = button;
		}
	}

	private int touchButtonIndex = -1;
	void Update () {
		// 배경화면 이동
		BackgroundMove ();

		// back butoon touch
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyUp(KeyCode.Escape)) {
			if (fade.InfoGroupActive()) {
				fade.FadeIn();
			} else {
				Application.Quit(); 
			}
		}

		if (gameInfoHide && UIGameInfo.activeInHierarchy && !fade.FadeIn_ing) {
			UIGameInfo.SetActive(false);
			gameInfoHide = false;
		}
		
		int count = Input.touchCount;	
		if (count == 1) {	
			TouchHandling();
		} else if (Input.GetMouseButton(0)) {
			MouseHandling();
		}
	} 

	private void TouchHandling() {
		Touch touch = Input.touches[0];
		if (!UIOption.activeInHierarchy && !UIShop.activeInHierarchy) {
			if (UIGameInfo.activeInHierarchy) {
				if (touch.phase == TouchPhase.Began) {
					if (buttonGameStart.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
						fade.ButtonDown(0, 10);	
						touchButtonIndex = 0;
					} else if (buttonInfoClose.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
						fade.ButtonDown(1, 10);
						touchButtonIndex = 1;
					}
				} else if (touch.phase == TouchPhase.Ended) {
					if (buttonGameStart.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
						fade.ButtonUp(0, 10);
						
						GameInfo info = (GameInfo) GameList[resizeIndex];
						PlayerPrefs.SetString("GameName", buttonList[resizeIndex].name);
						PlayerPrefs.SetInt("GameNo", info.no);
						PlayerPrefs.SetInt("HighScore", info.score);
						
						Application.LoadLevel(buttonList[resizeIndex].name);					
					} else if (buttonInfoClose.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
						fade.ButtonUp(1, 10);
						fade.FadeIn(UIGameInfo);
					} else if (touchButtonIndex != -1) {
						fade.ButtonUp(touchButtonIndex, 10);
					}
					
					touchButtonIndex = -1;
				} 
			} else {
				for (int i = 0; i < buttonList.Length; i++) {
					GUITexture buttonTexture = buttonList[i].guiTexture;
					if (buttonTexture.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
						if (buttonTexture.texture == buttonImage[0]) continue;
						
						if (fade != null)
							fade.FadeOut();
						
						resizeIndex = i;
						UIGameInfo.SetActive(true);
						SetGameInfo(i);
					}
				} 
				
				if (buttonOption.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
					UIOption.SetActive(true);
					UIOption.SendMessage("ShowOptionPanel"); 
				}
			}
		} else if (UIOption.activeInHierarchy && touch.phase == TouchPhase.Ended) {
			UIOption.SendMessage("OptionTouchHandling");
		} 
	}

	private void MouseHandling () {
		if (!UIOption.activeInHierarchy && !UIShop.activeInHierarchy) {
			if (UIGameInfo.activeInHierarchy) {
				if (buttonGameStart.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
					GameInfo info = (GameInfo) GameList[resizeIndex];
					PlayerPrefs.SetString("GameName", buttonList[resizeIndex].name);
					PlayerPrefs.SetInt("GameNo", info.no);
					PlayerPrefs.SetInt("HighScore", info.score);
					
					Application.LoadLevel(buttonList[resizeIndex].name);					
				} else if (buttonInfoClose.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
					fade.FadeIn(UIGameInfo);
				}
			} else {
				for (int i = 0; i < buttonList.Length; i++) {
					GUITexture buttonTexture = buttonList[i].guiTexture;
					if (buttonTexture.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
						if (buttonTexture.texture == buttonImage[0]) continue;
						
						if (fade != null)
							fade.FadeOut();
						
						resizeIndex = i;
						UIGameInfo.SetActive(true);
						SetGameInfo(i);
					}
				} 

				if (buttonOption.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
					UIOption.SetActive(true);
					UIOption.SendMessage("ShowOptionPanel"); 
				}
			}
		} else if (UIOption.activeInHierarchy) {
			UIOption.SendMessage("OptionTouchHandling");
		}
	}
	
	private void SetGameInfo(int gameNo) {
		fixedTexture [(int) FixedTextureName.Tutorial].texture = tutorialImage [gameNo];
	}

	// 배경화면 이동
	private void BackgroundMove() {
		for (int i = 0; i < BackgroundImageBlock.Length; i++) {
			BackgroundImageBlock[i].transform.Translate(Vector3.left * backgroundMoveSpeed * Time.deltaTime);
			BackgroundImageBlock[i].transform.Translate(Vector3.down * backgroundMoveSpeed * Time.deltaTime / 2);
		}

		if (BackgroundImageBlock[0].transform.position.x <= -20) {
			Destroy(BackgroundImageBlock[0]);
			Destroy(BackgroundImageBlock[1]);
			Destroy(BackgroundImageBlock[2]);
			Destroy(BackgroundImageBlock[3]);
			Destroy(BackgroundImageBlock[6]);

			BackgroundImageBlock[0] = BackgroundImageBlock[4];
			BackgroundImageBlock[4] = BackgroundImageBlock[8];
			BackgroundImageBlock[1] = BackgroundImageBlock[5];
			BackgroundImageBlock[3] = BackgroundImageBlock[7];
			BackgroundMake();
		}
	}
	private void BackgroundMake() {		
		BackgroundImageBlock[2] = Instantiate (BackgroundImage, new Vector3 (BackgroundImageBlock[0].transform.position.x + backgroundWidth * 2, BackgroundImageBlock[0].transform.position.y + backgroundHeight * 0, 0), transform.rotation) as GameObject;
		BackgroundImageBlock[5] = Instantiate (BackgroundImage, new Vector3 (BackgroundImageBlock[0].transform.position.x + backgroundWidth * 2, BackgroundImageBlock[0].transform.position.y + backgroundHeight * 1, 0), transform.rotation) as GameObject;
		BackgroundImageBlock[6] = Instantiate (BackgroundImage, new Vector3 (BackgroundImageBlock[0].transform.position.x + backgroundWidth * 0, BackgroundImageBlock[0].transform.position.y + backgroundHeight * 2, 0), transform.rotation) as GameObject;
		BackgroundImageBlock[7] = Instantiate (BackgroundImage, new Vector3 (BackgroundImageBlock[0].transform.position.x + backgroundWidth * 1, BackgroundImageBlock[0].transform.position.y + backgroundHeight * 2, 0), transform.rotation) as GameObject;
		BackgroundImageBlock[8] = Instantiate (BackgroundImage, new Vector3 (BackgroundImageBlock[0].transform.position.x + backgroundWidth * 2, BackgroundImageBlock[0].transform.position.y + backgroundHeight * 2, 0), transform.rotation) as GameObject;
	}

	// UI를 화면에 맞게 조정
	public void ChangeUISize() {			
		// 화면 해상도 처리 시작
		Screen.SetResolution (Screen.width, Screen.height, true);
		float sWidth = Screen.width;
		guiRatio = sWidth / 1600.0f;
		// 화면 해상도 처리 끝
		
		string strTexture = "UITexture";
		string strText = "UIText";
		
		GameObject[] UITextureList = GameObject.FindGameObjectsWithTag (strTexture);
		foreach (GameObject temp in UITextureList) {
			temp.transform.guiTexture.pixelInset = new Rect(temp.transform.guiTexture.pixelInset.x * guiRatio,
			                                                temp.transform.guiTexture.pixelInset.y * guiRatio,
			                                                temp.transform.guiTexture.pixelInset.width * guiRatio,
			                                                temp.transform.guiTexture.pixelInset.height * guiRatio);
		}
		
		GameObject[] UITextList = GameObject.FindGameObjectsWithTag (strText);
		foreach (GameObject temp in UITextList) {
			temp.transform.guiText.pixelOffset = new Vector2 (temp.transform.guiText.pixelOffset.x * guiRatio,
			                                                  temp.transform.guiText.pixelOffset.y * guiRatio);
			temp.transform.guiText.fontSize = (int)(temp.transform.guiText.fontSize * guiRatio);
		}
	}
}
