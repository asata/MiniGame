using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	void Start () {
		float screenRatio = ((float)Screen.height / (float)Screen.width);

		// 화면 해상도에 따라 카메라 크기(Size) 조정
		if (screenRatio >= 0.66f && screenRatio <= 0.67f) // 3:2
			this.camera.orthographicSize = 6.4f;
		else if (screenRatio >= 0.75f && screenRatio <= 0.76f) // 4:3
			this.camera.orthographicSize = 7.2f;
		else if (screenRatio >= 0.6f && screenRatio <= 0.61f) // 5:3
			this.camera.orthographicSize = 5.7f;
		else if (screenRatio >= 0.56f && screenRatio <= 0.57f) // 16:9
			this.camera.orthographicSize = 5.4f;

		this.camera.backgroundColor = new Color (0, 0, 0);
	}
}
