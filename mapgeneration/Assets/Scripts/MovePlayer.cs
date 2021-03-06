﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class Boundary
{
	public float xMin, xMax, zMin, zMax;
}

public class MovePlayer : MonoBehaviour {
	public float speed;
	public Boundary boundary;
	private int lookFactor = 10;

	void FixedUpdate(){
		float moveVertical = Input.GetAxis ("Vertical");
		float moveHorizontal = Input.GetAxis ("Horizontal");

		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		GetComponent<Rigidbody>().velocity = movement * speed;

		GetComponent<Rigidbody>().position = new Vector3 
			(
				Mathf.Clamp (GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax), 
				0.0f, 
				Mathf.Clamp (GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
				);
	}

	void Update(){
		float distance = (transform.position.z - Camera.main.transform.position.z)*lookFactor;
		Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
		transform.LookAt(position); 

		if (Input.GetButton ("Cancel")) {
			Application.LoadLevel ("GundownUI");
		}
	}
}
