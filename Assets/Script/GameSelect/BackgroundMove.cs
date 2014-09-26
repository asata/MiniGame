using UnityEngine;
using System.Collections;

public class BackgroundMove : MonoBehaviour {
	
	private const float backgroundMoveSpeed = 0.5f;
	float TargetOffset;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {		
		TargetOffset += Time.deltaTime + backgroundMoveSpeed;
		renderer.material.mainTextureOffset = new Vector2 (TargetOffset, 0);
	}
}
