using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapRenderer : MonoBehaviour {

	public Terrain ground;
	public GameObject btmLeftMarker;

	//in units
	private int BORDER = 15;
	private const int tileSize = 7;
	private float OFFSET = 0;
	private int columns = 0;
	private int rows = 0;

	private Dictionary<Vector2, string> baseTiles;
	private Dictionary<Vector2, string> iconTiles;
	private Dictionary<string, Object> gameTiles;
	private Dictionary<string, int> variations;
	private Dictionary<Vector2, string> objectives;

	private Vector3 playerSpawnLocation;
	[SerializeField]
	private GameObject player;

	private int MAX_NUM_VARIATIONS = 15;

	private bool renderBordersAsBoxes = false;

	// Use this for initialization
	void Awake () {
		OFFSET = tileSize / 2.0f;

		LoadGameTiles();
		GetNumberOfVariationsForEachGameTile();
	}

	void LoadGameTiles()
	{
		//base tiles prefabs
		gameTiles = new Dictionary<string, Object>();
		Object [] gameTilePrefab = Resources.LoadAll("Graphics/GameTiles/Legacy");

		Debug.Log ("Total types of base tiles loaded: " + gameTilePrefab.Length);

		foreach (Object tile in gameTilePrefab)
		{
			gameTiles.Add (tile.name, tile);
		}

		//objectives
		objectives = new Dictionary<Vector2, string>();
		Object [] objectiveTilePrefab = Resources.LoadAll ("Graphics/GameTiles/Objectives");
		Debug.Log ("Total types of objectives tiles loaded: " + objectiveTilePrefab.Length);

		foreach (Object tile in objectiveTilePrefab)
		{
			gameTiles.Add (tile.name, tile);
		}
	}

	void GetNumberOfVariationsForEachGameTile()
	{
		variations = new Dictionary<string, int>();

		foreach (string tile in gameTiles.Keys)
		{
			string baseTileString;
			if (tile.Contains ("door"))
			{
				baseTileString = tile.Substring(0, 9);
				variations.Add (baseTileString, 0);
				continue; // no variants for door prefabs
			} else if (tile.Contains("cover"))
			{
				baseTileString = tile.Substring(0, tile.Length - 1);
				variations.Add (baseTileString, 0);
				continue; // no variants for cover prefabs
			} else if (tile.Contains("Template") || tile.Contains("Objective"))
			{
				continue; //skip template and objective tiles
			} else
			{
				//we are left with base tiles
				baseTileString = tile.Substring(0, 4);
				int variant = 0;
				int.TryParse(tile.Substring(4), out variant);
				if (variations.ContainsKey(baseTileString))
				{
					variations[baseTileString] = (variant < MAX_NUM_VARIATIONS) ?
												Mathf.Max(variant, variations[baseTileString]) : 
												MAX_NUM_VARIATIONS;
				} else 
				{
					variations.Add (baseTileString, variant);
				}
			}
		}
	}

	public void RenderMap(MapData map)
	{
		InitialiseMap(map);
		RenderBaseAndIconTiles (map);
		SpawnObjectives();
		SpawnPlayer();
	}

	void InitialiseMap(MapData m)
	{
		columns = m.GetHorDimension();
		rows = m.GetVerDimension();
		// Terrain max height. 200 is an estimate subject to change; 
		// usually game objects (walls, players) will not exceed 200 units in height
		int terrainHeight = 200;
		int offsetZ = -(rows * tileSize) + (rows/2);
		int terrainColumns = columns * tileSize + BORDER*2;
		int terrainRows = rows * tileSize + BORDER*2;
		
		ground.terrainData.size = new Vector3(terrainColumns, terrainHeight, terrainRows);
		ground.transform.position = new Vector3(0.0f, 0.0f, offsetZ);
		btmLeftMarker.transform.position = new Vector3(-btmLeftMarker.transform.localScale.x/2, 
		                                               0.0f, offsetZ - btmLeftMarker.transform.localScale.z/2);
	}

	void RenderBaseAndIconTiles (MapData map)
	{
		Dictionary<Vector2, string> baseTiles = map.GetBaseTiles ();
		Dictionary<Vector2, string> iconTiles = map.GetIconTiles ();
		Dictionary<Vector2, string> iconTiles2 = map.GetIconTiles2 ();

		foreach (Vector2 pos in baseTiles.Keys) {
			Vector3 instantiationPos = GetInstantiationPos (pos.x, pos.y);

			string currBaseTile = GetCurrentBaseTileString (baseTiles, iconTiles, iconTiles2, pos);

			if (gameTiles.ContainsKey(currBaseTile)) {
				Object gameTilePrefab = gameTiles [currBaseTile];
				GameObject gameTile = GameObject.Instantiate (gameTilePrefab, instantiationPos, Quaternion.identity) as GameObject;
			} else if (currBaseTile == "DoNotRender") {
				Debug.Log ("Do not render such tiles");
			} else {
				Debug.Log ("There's an error rendering the map. Some of the tiles could not be rendered. Please try again with a new map.");
			}
		}
	}

	void SpawnObjectives()
	{
		foreach (Vector2 pos in objectives.Keys)
		{
			string str = objectives[pos];
			Vector3 instantiationPos = GetInstantiationPos(pos.x, pos.y);

			string objectiveTileString = "";
			switch (str)
			{
			case "neutralFlag": 
				objectiveTileString = "neutralObjective";
				break;
			case "redFlag": objectiveTileString = "redObjective";
				break;
			case "blueFlag": objectiveTileString = "blueObjective";
				break;
			}
			Object objectiveTilePrefab = gameTiles[objectiveTileString];
			GameObject objectiveTile = GameObject.Instantiate(objectiveTilePrefab, instantiationPos, Quaternion.identity) as GameObject;
		}
	}

	Vector3 GetInstantiationPos(float posX, float posY)
	{
		float instantiateY = 0.1f;
		float instantiateX = posX * tileSize + OFFSET + BORDER;
		float instantiateZ = -(posY * tileSize) + OFFSET + BORDER;
		return new Vector3(instantiateX, instantiateY, instantiateZ);
	}

	string GetCurrentBaseTileString (Dictionary<Vector2, string> baseTiles,
	                                 Dictionary<Vector2, string> iconTiles, 
	                                 Dictionary<Vector2, string> iconTiles2,
	                                 Vector2 pos)
	{
		string currBaseTile = baseTiles [pos];

		if (!renderBordersAsBoxes && currBaseTile == "kkkk") {
			return "DoNotRender";
		}

		// currTile contains only both i & k 
		if (!currBaseTile.Contains ("o") && currBaseTile.Contains ("k") && currBaseTile.Contains ("i"))
		{
			//replace k with o because there is currently no game tile prefab for such tiles
			currBaseTile = currBaseTile.Replace ('k', 'o');
		}

		if ( (IsTypeOfTile("door", iconTiles, pos) || IsTypeOfTile("door", iconTiles2, pos)) && currBaseTile.Contains("i") ) 
		{
			currBaseTile += "_door";
		}

		if ( IsTypeOfTile("coverHorizontal", iconTiles, pos) || IsTypeOfTile("coverHorizontal", iconTiles2, pos) ) {
			currBaseTile += "_coverHor";
		}

		if ( IsTypeOfTile("coverVertical", iconTiles, pos) || IsTypeOfTile("coverVertical", iconTiles2, pos)) {
			currBaseTile += "_coverVert";
		}

		if (IsObjective (iconTiles, pos)) 
		{
			if (!objectives.ContainsKey(pos)) {
				objectives.Add(pos, iconTiles[pos]);
			}
		}

		//finally, append the variation index behind 
		currBaseTile += Random.Range(0, variations[currBaseTile]);
		return currBaseTile;
	}

	bool IsTypeOfTile(string type, Dictionary<Vector2, string> iconTiles, Vector2 pos)
	{
		string currIconTile = GetCurrentIconTileString(iconTiles, pos);
		
		return (currIconTile != null && currIconTile == type);
	}

	bool IsObjective(Dictionary<Vector2, string> iconTiles, Vector2 pos)
	{
		string currIconTile = GetCurrentIconTileString(iconTiles, pos);
		return (currIconTile != null && currIconTile.Contains ("Flag"));
	}

	string GetCurrentIconTileString(Dictionary<Vector2, string> iconTiles, Vector2 pos)
	{
		return iconTiles.ContainsKey(pos)? iconTiles[pos] : null;
	}

	void SpawnPlayer()
	{
		playerSpawnLocation = GetInstantiationPos(columns/2, rows/2);
		playerSpawnLocation.y = 1.5f;
		player.transform.position = playerSpawnLocation;
	}
}
