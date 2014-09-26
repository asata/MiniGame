using UnityEngine;
using System.Collections;

public class CakeItem : Cake {
	private string itemName;

	public void SetItemName(string aName) {
		itemName = aName;
	}
	public string GetItemName() {
		return itemName;
	}

	void OnTriggerEnter(Collider other) {
		if (other.name == "Mouse") {
			GameManagerSunMoon GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
			GM.SendMessage ("CatchItem", itemName);
			Destroy (this.gameObject);
		//} else if (other.name == "Ground") {
		//	Destroy (this.gameObject);
		//} else if (other.name == "Tiger") {
		//	Destroy (this.gameObject);
		} else {
			Destroy (this.gameObject);			
		}
	}
}
