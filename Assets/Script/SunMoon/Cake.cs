using UnityEngine;
using System.Collections;

public enum CakeState {
	ThrowCake = 0,
	HitStone,
}

public class Cake : MonoBehaviour {
	private const float HideY 				= -5.0f;
	private Vector3 CakeInitVector 			= new Vector3 (3.0f, -5.0f);
	private Vector3 TigerMouseVector 		= new Vector3 (0.0f, -2.38f);
	private Vector3 SecondPerStoneMoveSpeed = new Vector3 (6.666f, -6.666f);	// 0.3f
	private Vector3 SecondPerCakeDownSpeed 	= new Vector3 (0.0f, -6.666f);
	// move length / move time
	private float MoveCakeTime 				= 0.5f;
	private float ShowCakeTIme 				= 0.65f;
	private int state = 0;
	//private GameManagerSunMoon GM;

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
		state = (int) CakeState.ThrowCake;
		//GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
	}

	public void SetMoveTime(float aTime) {
		MoveCakeTime = aTime;
		ShowCakeTIme = aTime + 0.15f;
	}

	void Update() {
		if (state == (int) CakeState.ThrowCake) { 
			if (moveTime < MoveCakeTime) {
				Vector3 center = (CakeInitVector + TigerMouseVector) * 0.8f;
				center -= new Vector3(0, 1);

				Vector3 riseRelCenter = CakeInitVector - center;
				Vector3 setRelCenter = TigerMouseVector - center;
				float fracComplete = moveTime / MoveCakeTime;

				this.transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
				this.transform.position += center;
			} else if (moveTime > MoveCakeTime && moveTime < ShowCakeTIme) {
				this.transform.position = TigerMouseVector;
			} else if (moveTime > ShowCakeTIme) {
				if (this.transform.position.y < HideY) {
					Destroy (this.gameObject);
				} else {
					Vector3 moveVector = this.transform.position + (Time.deltaTime * SecondPerCakeDownSpeed);
					this.transform.position = moveVector;
				}
			}
			
			moveTime += Time.deltaTime;
		} else if (state == (int) CakeState.HitStone) {
			if (this.transform.position.y < HideY) {
				Destroy (this.gameObject);
			} else {
				Vector3 moveVector = this.transform.position + (Time.deltaTime * SecondPerStoneMoveSpeed);
				this.transform.position = moveVector;
			}
		}
	}

	public void HitStone() {
		state = (int)CakeState.HitStone;
	}

	public void DestroyCake() {
		Destroy (this.gameObject);
	}

	public void DestroyIndex(int index) {
		if (beatIndex == index) 
			Destroy (this.gameObject);
	}
}
