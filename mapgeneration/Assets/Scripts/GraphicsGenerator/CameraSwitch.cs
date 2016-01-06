using UnityEngine;
using System.Collections;

public class CameraSwitch : MonoBehaviour {

	public GameObject flyCam;
	private GameObject playerCam;
	public GameObject player;

	private bool isActive = true;
	private bool isUiOpen = true;

	// Use this for initialization
	void Start () {
		playerCam = player.GetComponentInChildren<Camera>().gameObject;

		flyCam.SetActive(isActive);
		SetPlayerMovement(!isActive);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.C))
		{
			SwitchCamera();
		}
	}

	void SwitchCamera()
	{
		isActive = !isActive;

		flyCam.SetActive(isActive);
		SetPlayerMovement(!isActive);
	}

	void SetPlayerMovement(bool enabled)
	{
		MonoBehaviour [] playerScripts = player.GetComponents<MonoBehaviour>();

		foreach (MonoBehaviour script in playerScripts)
		{
			script.enabled = enabled;
		}

		playerCam.SetActive(enabled);
	}

	public void toggleUi()
	{
		isUiOpen = !isUiOpen;
	}
}
