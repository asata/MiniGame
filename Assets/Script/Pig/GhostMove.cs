using UnityEngine;
using System.Collections;

public class GhostMove : MonoBehaviour {
	private Vector3 GhostMoveSpeed = new Vector3(9.333f, 0.0f);	// move time : 1.5f, move length : 12
	private int beatIndex;

	public void SetBeatIndex(int aIndex) {
		beatIndex = aIndex;
	}
	public int GetBeatIndex() {
		return beatIndex;
	}

	void Update () {
		if (this.gameObject.transform.position.x > 9.5f) {
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
