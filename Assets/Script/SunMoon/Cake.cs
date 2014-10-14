using UnityEngine;
using System.Collections;

public enum CakeState {
	ThrowCake = 0,
	EatCake,
	HitStone,
	BeatenStone
}

public class Cake : MonoBehaviour {
	private const float HideY 				= -5.0f;
	private Vector3 CakeInitVector 			= new Vector3 (3.000f, -5.000f);
	private Vector3 TigerMouseVector 		= new Vector3 (0.000f, -2.380f);
	private Vector3 SecondPerStoneMoveSpeed = new Vector3 (6.666f, -6.666f);	// 0.3f
	private Vector3 SecondPerCakeDownSpeed 	= new Vector3 (0.000f, -6.666f);
	// move length / move time

	private float MoveCakeTime 				= 0.5f;
	private float ShowCakeTIme 				= 0.6f;
	private int state = 0;
	//private GameManagerSunMoon GM;

	private int beatIndex = -1;
	private int typeNo = 1;
	private float moveTime = 0;
	//private bool beatensStone = false;

	public void SetBeatIndex(object aIndex) {
		beatIndex = (int) aIndex;
	}
	public void SetTypeNo(object aType) {
		typeNo = (int) aType;
	}
	public void SetMoveTime(float aTime) {
		MoveCakeTime = aTime;
		ShowCakeTIme = aTime + 0.1f;
	}

	void Start() {
		moveTime = 0;
		//beatensStone = false;
		state = (int) CakeState.ThrowCake;
		//GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
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
				// Beaten Stone effect
				if (typeNo == 2) BeatenStone();

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
		/*} else if (state == (int) CakeState.BeatenStone) {
			if (this.transform.position.y < HideY) {
				Destroy (this.gameObject);
			} else {
				Vector3 moveVector = this.transform.position + (Time.deltaTime * SecondPerCakeDownSpeed);
				this.transform.position = moveVector;
			}*/
		}
	}

	public void EatCake(int index) {
		if (beatIndex == index) {
			// cake eat effect 
			
			// wait -> destroy cake		
			if (moveTime < MoveCakeTime) 
				StartCoroutine ("WaitEatCake", (MoveCakeTime - moveTime));
			else 
				StartCoroutine ("WaitEatCake", 0.0f);
		}
	}

	private IEnumerator WaitEatCake(float waitTime) {
		yield return new WaitForSeconds (waitTime);
		Destroy (this.gameObject);
	}

	public void HitStone(int index) {
		if (beatIndex == index) {
			// stone hit effect
			state = (int)CakeState.HitStone;
		}
	}

	private void BeatenStone() {
		//state = (int) CakeState.BeatenStone; 
		
		// effect play
	}
}
