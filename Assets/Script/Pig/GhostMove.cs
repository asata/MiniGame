using UnityEngine;
using System.Collections;

public class GhostMove : MonoBehaviour {
	private Vector3 GhostMoveSpeed = new Vector3(8.666f, 0.0f);	// move time : 1.5f, move length : 12
	private int ghostLane;
	private int beatIndex;
	private bool printMiss = true;

	public void SetGhostLane(int aLane) {
		ghostLane = aLane;
	}
	public void SetBeatIndex(int aIndex) {
		beatIndex = aIndex;
	}
	public int GetBeatIndex() {
		return beatIndex;
	}
	public void SetPrintMiss(int index) {
		if (beatIndex == index)
			printMiss = false;
	}

	void Update () {
		if (this.gameObject.transform.position.x > 7.0f) {
			if (printMiss) {
				GameManagerPig GM = GameObject.Find ("GameManager").GetComponent<GameManagerPig> ();
				GM.SendMessage("PrintMissMessage", ghostLane);
			}
			Destroy(this.gameObject);
		} else {
			float moveX = this.gameObject.transform.position.x + (GhostMoveSpeed.x * Time.deltaTime);
			float moveY = this.gameObject.transform.position.y;
			float moveZ = this.gameObject.transform.position.z;
			this.gameObject.transform.position = new Vector3 (moveX, moveY, moveZ);
		}
	}

	public void DestroyGhost(int index) {
		if (beatIndex == index)
			Destroy (this.gameObject);
	}
}
