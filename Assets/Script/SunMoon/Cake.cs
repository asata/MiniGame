using UnityEngine;
using System.Collections;

public class Cake : MonoBehaviour {
	private GameManagerSunMoon GM;

	private int beatIndex = -1;
	private int typeNo = 1;

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


	private float throwForceX = -500.0f;
	private float throwForceY = 400.0f;

	public void SetForceX(float aForceX) {
		throwForceX = aForceX;
	}
	public void SetForceY(float aForceY) {
		throwForceY = aForceY;
	}

	void Start() {
		GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
	}

	void ThrowCake() {
		rigidbody.AddForce(new Vector3(throwForceX, throwForceY));
	}

	void OnTriggerEnter(Collider other) {
		if (other.name == "HitZone") {
			GM.SendMessage ("HitZoneJoin", this);
		} else if (other.name == "Tiger") {
			GM.SendMessage ("HitTiger", this);
		} else if (other.name == "Ground") {
			Destroy (this.gameObject);
		} else  {

		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.name == "HitZone") {
			GM.SendMessage ("HitZoneOut", this);
		}
	}

	public void DestroyCake() {
		Destroy (this.gameObject);
	}
}
