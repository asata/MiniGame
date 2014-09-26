using UnityEngine;
using System.Collections;

public enum TigerState {
	StandBy,
	Jump
}
public class Tiger : MonoBehaviour {
	private float JumpPower = 450f;
	public TigerState TS;

	void Start () {
		TS = TigerState.StandBy;
	}

	void Update () {
	
	}

	void Jump() {
		if (TS == TigerState.StandBy) {
			TS = TigerState.Jump;
			rigidbody.AddForce (new Vector3 (0, JumpPower));
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (TS != TigerState.StandBy) {
			TS = TigerState.StandBy;
		}
	}

	public void SetTigerJumpPower(float aPower) {
		JumpPower = aPower;
	}
	public float GetTigerJumpPower() {
		return JumpPower;
	}	                                          
}
