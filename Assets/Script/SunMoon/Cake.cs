using UnityEngine;
using System.Collections;

public class Cake : MonoBehaviour {
	private GameManagerSunMoon GM;
	private float hitTime = 0.00f;
	private int typeNo = 1;
	private float throwForceX = -500.0f;
	private float throwForceY = 400.0f;
	
	public void SetTypeNo(object aType) {
		typeNo = (int) aType;
	}
	public int GetTypeNo() {
		return typeNo;
	}
	public void SetTime(object aTime) {
		hitTime = (float) aTime;
	}
	public float GetTime() {
		return hitTime;
	}

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
		//Debug.Log (other.name);
		//GM.SendMessage ("CakeProcess", other);
		//GM.SendMessage ("CatchCake");
		if (other.name == "HitZone") {
			GM.SendMessage ("HitZoneJoin", this);
		} else if (other.name == "Tiger") {
			GM.SendMessage ("HitTiger", this);
		} else if (other.name == "Ground") {
			Destroy (this.gameObject);
		} else  {

		}

		//Destroy (this.gameObject);
	}
	
	void OnTriggerExit(Collider other) {
		if (other.name == "HitZone") {
			GM.SendMessage ("HitZoneOut");
		}
	}

	public void DestroyCake() {
		Destroy (this.gameObject);
	}
}
