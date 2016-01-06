using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour {

	public Canvas canvas;
	//public CameraSwitch cam;
	private bool isUiOpen = false;
	private GameObject objectToBeToggled;
	void Start () {
		objectToBeToggled = canvas.gameObject.transform.Find("ScrollRect").gameObject;
		objectToBeToggled.SetActive(isUiOpen);
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.M))
		{
			isUiOpen = !isUiOpen;
			objectToBeToggled.SetActive(isUiOpen);
		}
	}
}
