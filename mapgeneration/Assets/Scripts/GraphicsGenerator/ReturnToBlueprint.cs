using UnityEngine;
using System.Collections;

public class ReturnToBlueprint : MonoBehaviour {
	private CreateSettings uiController;

	public void Return() {
		Application.LoadLevelAsync("GundownUI");
	}
}
