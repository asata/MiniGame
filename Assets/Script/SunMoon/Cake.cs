using UnityEngine;
using System.Collections;

public enum CakeState {
	ThrowCake = 0,
	EatCake,
	HitStone,
	BeatenStone
}

public class Cake : MonoBehaviour {
	private const float HideY 				= -5.6f;
	private const int BeatenEffectCount 	= 10;
	private Vector3 CakeInitVector 			= new Vector3 (3.000f, -5.600f, -2.000f);
	private Vector3 TigerMouseVector 		= new Vector3 (0.000f, -2.780f);
	private Vector3 TigerTearVector 		= new Vector3 (1.000f, -2.000f);
	private Vector3 SecondPerStoneMoveSpeed = new Vector3 (6.666f, -6.666f);	// 0.3f
	private Vector3 SecondPerCakeDownSpeed 	= new Vector3 (0.000f, -6.666f);
	// move length / move time

	private GameObject hitStoneEffect;
	public GameObject[] effect;
	private GameObject[] beatenEffect;

	private float MoveCakeTime 				= 0.5f;
	private float ShowCakeTIme 				= 0.6f;
	private int beatenCount 				= -1;
	private int state = 0;
	//private GameManagerSunMoon GM;

	private int beatIndex = -1;
	private int typeNo = 1;
	private float moveTime = 0;
	private bool beatensStone = false;

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
		beatensStone = false;
		state = (int) CakeState.ThrowCake;
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
				if (typeNo == 2 && !beatensStone) BeatenStone();

				if (this.transform.position.y < HideY) {
					Destroy (this.gameObject);
				} else {
					Vector3 moveVector = this.transform.position + (Time.deltaTime * SecondPerCakeDownSpeed);
					this.transform.position = moveVector;

					if (beatenCount > 0) {
						Vector3 tearVector = beatenEffect[1].transform.position + (Time.deltaTime * SecondPerCakeDownSpeed);
						beatenEffect[1].transform.position = tearVector;

						beatenCount--;
					} else if (beatenCount == 0) {
						Destroy(beatenEffect[0]);
						Destroy(beatenEffect[1]);

						beatenCount--;
					}
				}
			}
			
			moveTime += Time.deltaTime;
		} else if (state == (int) CakeState.HitStone) {
			if (this.transform.position.y < HideY) {
				Destroy (this.gameObject);
			} else {

				Vector3 moveVector = this.transform.position + (Time.deltaTime * SecondPerStoneMoveSpeed);
				this.transform.position = moveVector;

				if (beatenCount > 0) {
					beatenEffect[0].transform.position = moveVector;
					beatenCount--;
				} else if (beatenCount == 0) {
					Destroy(beatenEffect[0]);
					beatenCount--;
				}
			}
		}
	}

	public void EatCake(int index) {
		if (beatIndex == index) {
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
			state = (int)CakeState.HitStone;
			
			beatenEffect = new GameObject[1];
			beatenEffect[0] = (GameObject) Instantiate (effect [0], this.transform.position, transform.rotation);
			beatenCount = BeatenEffectCount / 2;
		}
	}

	private void BeatenStone() {
		GameManager GM = GameObject.Find ("GameManager").GetComponent<GameManagerSunMoon> ();
		if (PlayerPrefs.GetInt ("EffectSound") == 0 && GM.AnotherSpaker != null) {
			GM.AnotherSpaker.SendMessage ("SoundPlayLoadFile", (int) EffectSoundTiger.HitTiger);
		}

		beatensStone = true;

		beatenEffect = new GameObject[2];
		beatenEffect[0] = (GameObject) Instantiate (effect [1], TigerMouseVector, transform.rotation);
		beatenEffect[1] = (GameObject) Instantiate (effect [2], TigerTearVector, transform.rotation);
		beatenCount = BeatenEffectCount;
	}
}