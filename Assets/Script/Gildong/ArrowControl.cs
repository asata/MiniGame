﻿using UnityEngine;
using System.Collections;

public class ArrowControl : MonoBehaviour {
	private float ArrowDestroyYPosion = -5.0f;
	// Gildong's position change this value change
	private Vector3 SecondPerDownSpeedLeft = new Vector3(8.0f, -5.5f);
	private Vector3 SecondPerDownSpeedRight = new Vector3(-8.0f, -5.5f);
	public bool arrowdirection = true; 	// true : left, false : right

	public void SetSpeed(float aSpeed) {
		if (arrowdirection) {
			SecondPerDownSpeedLeft = new Vector3(8.0f / aSpeed, -5.5f / aSpeed);
		} else {
			SecondPerDownSpeedRight = new Vector3(-8.0f / aSpeed, -5.5f / aSpeed);
		}
	}
	public void SetDirection(string name) {
		if (name == "EnemyLeft") 
			arrowdirection = true;
		else 
			arrowdirection = false;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (this.transform.position.y < ArrowDestroyYPosion) 
			Destroy (this.gameObject);
		
		Vector3 moveVector = this.transform.position;
		if (arrowdirection)
			moveVector += (Time.deltaTime * SecondPerDownSpeedLeft);
		else
			moveVector += (Time.deltaTime * SecondPerDownSpeedRight);
		this.transform.position = moveVector;
	}
}