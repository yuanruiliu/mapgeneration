using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActivateButton : MonoBehaviour {
	private string[] buttonTags = {"ExportButton", "RenderButton"};
	private GameObject buttonToggles;
	private Button button;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < buttonTags.Length; i++) {
			buttonToggles = GameObject.FindWithTag (buttonTags[i]);
			
			if (buttonToggles != null) {
				button = buttonToggles.GetComponent<Button>();
				button.interactable = true;
				button.image.color = new Color32(4, 163, 242, 150);
			}
		}
	}
}
