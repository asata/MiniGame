using UnityEngine;
using System.Collections;

public class WolfMove : MonoBehaviour {
	private const float yValue = 3.0f;
	private Vector3 WolfPosition1 = new Vector3(-8, yValue);
	private Vector3 WolfPosition2 = new Vector3(-8, 0);
	private Vector3 WolfPosition3 = new Vector3(-8, -yValue);

	private int wolfPosition = 1;
	private float moveTime = 0;
	private float moveLength = 0;
	private float moveValue = 0;

	public void SetWolfMoveTime(float aTime) {
		moveTime = aTime;
	}
	public void SetWolfPosition(int position) {
		if (wolfPosition == position) {
			moveLength = 0;
		} else if (position == 1) {
			moveLength = yValue - this.transform.position.y;
		} else if (position == 2) {
			moveLength = 0 - this.transform.position.y;
		} else if (position == 3) {
			moveLength = -yValue - this.transform.position.y;
		}
		moveValue = moveLength / moveTime;
		wolfPosition = position;
	}

	// Update is called once per frame
	void Update () {
		if (wolfPosition == 1) {
			if (this.transform.position.y < yValue) {
				float moveX = this.gameObject.transform.position.x;
				float moveY = this.gameObject.transform.position.y + (moveValue * Time.deltaTime);
				this.gameObject.transform.position = new Vector3 (moveX, moveY);
			} else {
				this.gameObject.transform.position = WolfPosition1;
			}
		} else if (wolfPosition == 2) {
			if (moveTime == 0) {
				this.gameObject.transform.position = WolfPosition2;
			} else {
				float moveX = this.gameObject.transform.position.x;
				float moveY = this.gameObject.transform.position.y + (moveValue * Time.deltaTime);
				this.gameObject.transform.position = new Vector3 (moveX, moveY);
			}
		} else if (wolfPosition == 3) {
			if (this.transform.position.y > -yValue) {
				float moveX = this.gameObject.transform.position.x;
				float moveY = this.gameObject.transform.position.y + (moveValue * Time.deltaTime);
				this.gameObject.transform.position = new Vector3 (moveX, moveY);
			} else {
				this.gameObject.transform.position = WolfPosition3;
			}
		}

		moveTime -= Time.deltaTime;
	}
}
