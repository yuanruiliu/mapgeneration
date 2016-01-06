using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreateSettings : MonoBehaviour {
	private int num_neutral_objectives, num_red_objectives, num_blue_objectives, num_bases,
				horz, vert, density, max_allowed_objectives;
	public Text neutral_objectives, red_objectives, blue_objectives, bases, horzTiles, vertTiles,
				percentageDensity, allowedMaxObjs;
	public bool isSymmetricMap;

	private string[] gameTags = {"NeutralButtonUp", "NeutralButtonDown", "RedButtonUp", "RedButtonDown", "BlueButtonUp", "BlueButtonDown"};

	private const int OBJECTIVES_NEUTRAL = 0;
	private const int OBJECTIVES_RED = 1;
	private const int OBJECTIVES_BLUE = 2;

	private const int MIN_ALLOWED_OBJ = 0;
	private const int MAX_ALLOWED_OBJ_FOR_30_BY_30 = 10;
	private const int MAX_ALLOWED_OBJ_FOR_28_BY_28 = 9;
	private const int MAX_ALLOWED_OBJ_FOR_26_BY_26 = 8;
	private const int MAX_ALLOWED_OBJ_FOR_24_BY_24 = 7;
	private const int MAX_ALLOWED_OBJ_FOR_22_BY_22 = 6;
	private const int MAX_ALLOWED_OBJ_FOR_20_BY_20 = 5;
	private const int MAX_ALLOWED_OBJ_FOR_18_BY_18 = 4;
	private const int MAX_ALLOWED_OBJ_FOR_15_BY_15 = 3;

	private const int _30_BY_30_TILES = 900;
	private const int _28_BY_28_TILES = 784;
	private const int _26_BY_26_TILES = 676;
	private const int _24_BY_24_TILES = 576;
	private const int _22_BY_22_TILES = 484;
	private const int _20_BY_20_TILES = 400;
	private const int _18_BY_18_TILES = 324;
	private const int _15_BY_15_TILES = 225;

	private const int MIN_NUM_BASES = 0;
	private const int MAX_NUM_BASES = 2;
	private const int INITIAL_NUM_NEUTRAL_OBJ = 1;
	private const int INITIAL_NUM_RED_OBJ = 0;
	private const int INITIAL_NUM_BLUE_OBJ = 0;
	private const int INITIAL_NUM_BASE = 2;
	private const int INITIAL_HORZ_DIMENSION = 20;
	private const int INITIAL_VERT_DIMENSION = 20;
	private const int MAX_THRESHOLD_DIMENSIONS = 50;

	// Use this for initialization
	void Start () {
		num_neutral_objectives = INITIAL_NUM_NEUTRAL_OBJ;
		num_red_objectives = INITIAL_NUM_RED_OBJ;
		num_blue_objectives = INITIAL_NUM_BLUE_OBJ;
		num_bases = INITIAL_NUM_BASE;
		horz = INITIAL_HORZ_DIMENSION;
		vert = INITIAL_VERT_DIMENSION;
		max_allowed_objectives = MAX_ALLOWED_OBJ_FOR_20_BY_20;

		density = 10;
		isSymmetricMap = false;

		neutral_objectives.text = "";
		red_objectives.text = "";
		blue_objectives.text = "";
		bases.text = "";
		horzTiles.text = "";
		percentageDensity.text = "";
		vertTiles.text = "";
		allowedMaxObjs.text = "";

		UpdateObjectives (OBJECTIVES_NEUTRAL);
		UpdateObjectives (OBJECTIVES_RED);
		UpdateObjectives (OBJECTIVES_BLUE);
		UpdateBases ();
		UpdateHorz (horz);
		UpdateVert (vert);
		UpdateDensity (density);
		UpdateMaxAllowedObjs (max_allowed_objectives);
	}

	public int GetAllowedMaxObjectives(){
		int total_num_tiles = horz * vert;
		int maxObjectives = 0;

		if (total_num_tiles >= _30_BY_30_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_30_BY_30;
		} else if (total_num_tiles >= _28_BY_28_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_28_BY_28;
		} else if (total_num_tiles >= _26_BY_26_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_26_BY_26;
		} else if (total_num_tiles >= _24_BY_24_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_24_BY_24;
		} else if (total_num_tiles >= _22_BY_22_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_22_BY_22;
		} else if (total_num_tiles >= _20_BY_20_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_20_BY_20;
		} else if (total_num_tiles >= _18_BY_18_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_18_BY_18;
		} else if (total_num_tiles >= _15_BY_15_TILES) {
			maxObjectives = MAX_ALLOWED_OBJ_FOR_15_BY_15;
		} else {
			Debug.Log ("Total number of tiles below minimum allowed dimensions.");
		}
		return maxObjectives;
	}

	public void IncreaseNumObjs (int objective_type) {
		int maxObjectives = GetAllowedMaxObjectives ();

		switch (objective_type) {
			case OBJECTIVES_NEUTRAL:
				if (num_neutral_objectives + num_red_objectives + num_blue_objectives < maxObjectives) {
					num_neutral_objectives++;
					UpdateObjectives (OBJECTIVES_NEUTRAL);
				} else {
					Debug.Log ("Sum of all objectives exceeds maximum allowed total.");
				}
				break;
			case OBJECTIVES_RED:
				if (num_neutral_objectives + num_red_objectives + num_blue_objectives < maxObjectives) {
					num_red_objectives++;
					UpdateObjectives (OBJECTIVES_RED);
				} else {
					Debug.Log ("Sum of all objectives exceeds maximum allowed total.");
				}
				break;
			case OBJECTIVES_BLUE:
				if (num_neutral_objectives + num_red_objectives + num_blue_objectives < maxObjectives) {
					num_blue_objectives++;
					UpdateObjectives (OBJECTIVES_BLUE);
				} else {
					Debug.Log ("Sum of all objectives exceeds maximum allowed total.");
				}
				break;
			default:
				Debug.Log ("Invalid objective_type received to increase objectives.");
				break;
		}
	}

	public void ResetRedObjs(){
		num_red_objectives = INITIAL_NUM_RED_OBJ;
		UpdateObjectives (OBJECTIVES_RED);
	}

	public void ResetBlueObjs(){
		num_blue_objectives = INITIAL_NUM_BLUE_OBJ;
		UpdateObjectives (OBJECTIVES_BLUE);
	}

	public void DecreaseNumObjs (int objective_type) {
		switch (objective_type) {
			case OBJECTIVES_NEUTRAL:
				if (num_neutral_objectives + num_red_objectives + num_blue_objectives > MIN_ALLOWED_OBJ && num_neutral_objectives > MIN_ALLOWED_OBJ) {
					num_neutral_objectives--;
					UpdateObjectives (OBJECTIVES_NEUTRAL);
				} else {
					Debug.Log ("Sum of all objectives is below minimum allowed total.");
				}
				break;
			case OBJECTIVES_RED:
				if (num_neutral_objectives + num_red_objectives + num_blue_objectives > MIN_ALLOWED_OBJ  && num_red_objectives > MIN_ALLOWED_OBJ) {
					num_red_objectives--;
					UpdateObjectives (OBJECTIVES_RED);
				} else {
					Debug.Log ("Sum of all objectives is below minimum allowed total.");
				}
				break;
			case OBJECTIVES_BLUE:
				if (num_neutral_objectives + num_red_objectives + num_blue_objectives > MIN_ALLOWED_OBJ  && num_blue_objectives > MIN_ALLOWED_OBJ) {
					num_blue_objectives--;
					UpdateObjectives (OBJECTIVES_BLUE);
				} else {
					Debug.Log ("Sum of all objectives is below minimum allowed total.");
				}
				break;
			default:
				Debug.Log ("Invalid objective_type received to reduce objective count.");
				break;
		}
	}
	
	//Check validity before allowing an update
	public void IncreaseNumBases(){
		if (num_bases < MAX_NUM_BASES) {
			num_bases++;
			UpdateBases ();
		}
	}
	
	public void DecreaseNumBases(){
		if (num_bases > MIN_NUM_BASES) {
			num_bases--;
			UpdateBases ();
		}
	}

	private void UpdateObjectives(int objective_type){
		switch (objective_type) {
		case OBJECTIVES_NEUTRAL:
			neutral_objectives.text = num_neutral_objectives.ToString();
			break;
		case OBJECTIVES_RED:
			red_objectives.text = num_red_objectives.ToString();
			break;
		case OBJECTIVES_BLUE:
			blue_objectives.text = num_blue_objectives.ToString();
			break;
		default:
			Debug.Log ("Invalid objective_type received to update objectives.");
			break;
		}
	}

	public void UpdateBases(){
		bases.text = num_bases.ToString ();
	}

	public void UpdateHorz(int horzValue){
		if(horzValue > MAX_THRESHOLD_DIMENSIONS){
			Debug.Log("Selected value exceeds threshold. Reverting to 50 to maintain stability of program.");
			horzValue = 50;
		}
	
		horzTiles.text = horzValue.ToString ();
		horz = horzValue;

		int maxObjectives = GetAllowedMaxObjectives ();
		UpdateMaxAllowedObjs (maxObjectives);

		ResetObjectiveCounts (maxObjectives);
	}

	public void UpdateVert(int vertValue){
		if(vertValue > MAX_THRESHOLD_DIMENSIONS){
			Debug.Log("Selected value exceeds threshold. Reverting to 50 to maintain stability of program.");
			vertValue = 50;
		}
	
		vertTiles.text = vertValue.ToString ();
		vert = vertValue;

		int maxObjectives = GetAllowedMaxObjectives ();
		UpdateMaxAllowedObjs (maxObjectives);

		ResetObjectiveCounts (maxObjectives);
	}

	public void ResetObjectiveCounts(int maxObjectives){
		if (num_neutral_objectives + num_red_objectives + num_blue_objectives > maxObjectives) {
			num_neutral_objectives = maxObjectives;
			num_red_objectives = MIN_ALLOWED_OBJ;
			num_blue_objectives = MIN_ALLOWED_OBJ;

			ResetButtonClickability ();
			UpdateObjectives (OBJECTIVES_NEUTRAL);
			UpdateObjectives (OBJECTIVES_RED);
			UpdateObjectives (OBJECTIVES_BLUE);
		} else if (num_neutral_objectives + num_red_objectives + num_blue_objectives < maxObjectives) {
			for (int i = 0; i < gameTags.Length; i+=2) {
				GameObject taggedObject = GameObject.FindWithTag (gameTags [i]);

				if (taggedObject != null) {
					Button buttonToReset = taggedObject.GetComponent<Button> ();
					buttonToReset.interactable = true;
				}
			}
		} else {

		}
	}

	private void ResetButtonClickability(){
		for (int i = 0; i < gameTags.Length; i++) {
			GameObject taggedObject = GameObject.FindWithTag(gameTags[i]);

			if(taggedObject != null){
				Button buttonToReset = taggedObject.GetComponent<Button>();

				if(i != 1){
					buttonToReset.interactable = false;
				} else {
					buttonToReset.interactable = true;
				}
			}
		}
	}

	public void UpdateDensity(int densityValue){
		percentageDensity.text = densityValue.ToString ();
		density = densityValue;
	}

	public void UpdateSymmetricMap(bool isSymmetric){
		isSymmetricMap = isSymmetric;
	}

	private void UpdateMaxAllowedObjs(int maxObjectives){
		allowedMaxObjs.text = maxObjectives.ToString ();
	}

	public int[] GetInputValues(){
		int isSymmetric = 0;

		if (isSymmetricMap) {
			isSymmetric = 1;
		}

		int[] data = new int[] {num_neutral_objectives, num_red_objectives, num_blue_objectives, num_bases,
			horz, vert, density, isSymmetric};

		return data;
	}
}