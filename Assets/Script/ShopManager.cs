using UnityEngine;
using System.Collections;

public class ShopManager : MonoBehaviour {
	public ArrayList itemList = new ArrayList();
	public GUITexture buttonClose;
	public GUISkin shopStyle;

	private Vector2 scrollViewVector = Vector2.zero;
	//private float sWidth;
	private float guiRatio;
	//private Vector3 GUIsF;
	//private bool kkk;

	// Use this for initialization
	void Start () {
		// 화면 해상도 처리 끝

		// 소지금 및 캐시 등을 출력

		// item info load(for web or xml file)

		// ItemInfo 객체로 생성 -> itemList.Add

		// itemList Print - OnGUI
		Texture temp = (Texture)Resources.Load ("Item/item1");
		itemList.Add (temp);
	}

	public void SetGUIRatio(float aRatio) {
		guiRatio = aRatio;
	}
		
	void ShopTouchHandling () {
		if (buttonClose.HitTest (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))) {
			gameObject.SetActive(false);
		}
	}
	void ShopScrollHandling () {
		Touch touch = Input.touches[0];
		scrollViewVector.x += touch.deltaPosition.x;
	}

	void OnGUI () {
		GUI.skin = shopStyle;
		GUI.matrix = Matrix4x4.TRS (new Vector3 (guiRatio, guiRatio, 0), Quaternion.identity, new Vector3 (guiRatio, guiRatio, 1));
				
		// begin scrollbar
		scrollViewVector = GUI.BeginScrollView (new Rect (160, 160, 480, 210), 
		                                        scrollViewVector, 
		                                        new Rect (0, 0, 600, 190));

		GUILayout.BeginHorizontal ();
		// itemList Print start
		for (int i = 0; i < 4; i++) {
			// button type print 
			GUILayout.BeginVertical ();
			GUILayout.Label ("Item" + i.ToString());
			GUILayout.Box ((Texture)itemList [0]);
			// buy button press -> alert message print
			GUILayout.Button ("Buy");
			GUILayout.EndVertical ();
		}
		// itemList Print end	
		GUILayout.EndHorizontal ();

		// end scrollbar		
		GUI.EndScrollView ();

		// 영역 확인을 위해 넣은 코드임, 실제 코드는 좌우 스크롤바로 대체
		//GUI.Box (new Rect (160, 160, 480, 210), "test"); 
	}	        
}
