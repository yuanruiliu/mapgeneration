using UnityEngine;
using System.Collections;

public class DoNotDestroyOnLoad : MonoBehaviour {
	private static DoNotDestroyOnLoad singleMM;

	void Awake() {
		if (singleMM == null) {
			singleMM = this;
			DontDestroyOnLoad (gameObject);
		} else {
			Destroy (this);
		}
	}
}
