using UnityEngine;
using System.Collections;

public class Cake : MonoBehaviour {
	private float throwForceX = -500.0f;
	private float throwForceY = 400.0f;
	
	public void SetForceX(float aForceX) {
		throwForceX = aForceX;
	}
	public void SetForceY(float aForceY) {
		throwForceY = aForceY;
	}

	void ThrowCake() {
		rigidbody.AddForce(new Vector3(throwForceX, throwForceY));
	}

	void OnTriggerEnter(Collider other) {
		GameManagerSunMoon GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();

		if (other.name == "Ground") {
			GM.SendMessage ("MissCake");
			Destroy (this.gameObject);
		} else if (other.name == "Mouse") {
			GM.SendMessage ("CatchCake");
			Destroy (this.gameObject);
		} else if (other.name == "Tiger") {
			GM.SendMessage ("HitCake");
			Destroy (this.gameObject);
		} else {

		}
	}
}
