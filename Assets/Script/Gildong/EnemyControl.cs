using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour {
	//private Vector3 MakeLeftArrow = new Vector3 (-8, 3, 0);
	//private Vector3 MakeRightArrow = new Vector3 (8, 3, 0);
	
	public Animator EnemyAnimator;
	public GameObject ArrowObject;
	private bool makeArrow = true;
	private float arrowSpeed = 1.0f;

	public void SetArrowSpeed(float aSpeed) {
		arrowSpeed = aSpeed;
	}

	// Use this for initialization
	void Start () {
		makeArrow = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (EnemyAnimator.GetCurrentAnimatorStateInfo (0).IsName ("ThrowArrow") && makeArrow) {
			// make arrow
			GameObject Arrow = (GameObject) Instantiate(ArrowObject, this.transform.position, transform.rotation);
			Arrow.SendMessage("SetDirection", this.name);
			Arrow.SendMessage("SetSpeed", arrowSpeed);
			makeArrow = false;
		} else if (EnemyAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Hide")) {
			makeArrow = true;
		}
	}
}
