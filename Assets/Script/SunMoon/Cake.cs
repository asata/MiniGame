using UnityEngine;
using System.Collections;

public class Cake : MonoBehaviour {
	// move length / move time
	private const float SecondMoveLengthX 	= 6.0f;
	private const float SecondMoveLengthY 	= 4.0f;
	private const float Curvature 			= 1.56f;
	private GameManagerSunMoon GM;

	private int beatIndex = -1;
	private int typeNo = 1;
	private float moveTime = 0;

	public void SetBeatIndex(object aIndex) {
		beatIndex = (int) aIndex;
	}
	public int GetBeatIndex() {
		return beatIndex;
	}
	public void SetTypeNo(object aType) {
		typeNo = (int) aType;
	}
	public int GetTypeNo() {
		return typeNo;
	}

	void Start() {
		moveTime = 0;
		GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
	}

	void Update() {
		if (moveTime < 0.5f) {
			// move time : 0.5f
			float moveX = Time.deltaTime * SecondMoveLengthX;
			float moveY = Time.deltaTime * SecondMoveLengthY;

			if(moveTime < 0.25f) {
				moveX /= Curvature;
				moveY *= Curvature;
			} else { // if (moveTime > 0.4f) {
				moveX *= Curvature;
				moveY /= Curvature;
			}
			
			float xValue = this.transform.position.x - moveX;
			float yValue = this.transform.position.y + moveY;

			if (xValue < 0) xValue = 0;
			if (yValue > -3.0) yValue = -3.0f;

			this.transform.position = new Vector3 (xValue, yValue);
		} else if (moveTime > 0.5f && moveTime < 0.8f) {
			this.transform.position = new Vector3(0, -3.0f);
		} else if (moveTime > 0.8f) {
			Destroy(this.gameObject);
		}

		moveTime += Time.deltaTime;
	}

	public void DestroyCake() {
		Destroy (this.gameObject);
	}

	public void DestroyIndex(int index) {
		if (beatIndex == index) 
			Destroy (this.gameObject);
	}
}
