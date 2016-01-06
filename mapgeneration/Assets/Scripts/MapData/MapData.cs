using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MapData {

	const string ICON = "icons";
	const string BASE = "bases";
	const string EMPTY = "empty";
	const string BORDER = "border";

	// stores position (Vector2) and 
	// tile type (use string to save space, strings are converted to Sprites in GG)
	public Dictionary<Vector2, string> baseTiles;
	//2 icon tiles because sometimes icon needs to overlay on top of icons also
	public Dictionary<Vector2, string> iconTiles;
	public Dictionary<Vector2, string> iconTiles2;

	MapSettings settings;
	int numOfCovers;

	List<Vector2> emptyTiles;
	List<RegionData> listOfRegions;
	char[,] tileComponents; //access using Mini Tile coordinates
	List<Tuple<Vector2, Vector2, int>> lineDataList; //stores data for the lines showing connected regions

	float overallFitness = 0.0f;

	// For calculating fitness
	bool isBaseMissing = false; 
	bool isFlagMissing = false;

	/***************/
	/* Constructor */
	/***************/
	public MapData(MapSettings settings) {
		baseTiles = new Dictionary<Vector2, string>();
		iconTiles = new Dictionary<Vector2, string>();
		iconTiles2 = new Dictionary<Vector2, string>();

		this.settings = settings;
		
		tileComponents = new char[settings.GetHorDimension()*2,settings.GetVerDimension()*2];
		emptyTiles = new List<Vector2> ();
		listOfRegions = new List<RegionData> ();
		lineDataList = new List<Tuple<Vector2, Vector2, int>>();
		
	}

	/*****************/
	/* Set Functions */
	/*****************/
	public void SetTileComponentsArray (char[,] TC) {
		this.tileComponents = TC;
	}
	public void SetBaseTiles (Dictionary<Vector2,string> tiles) {
		this.baseTiles = tiles;
	}
	public void SetIconTiles (Dictionary<Vector2,string> tiles) {
		this.iconTiles = tiles;
	}
	public void SetIconTiles2 (Dictionary<Vector2,string> tiles2) {
		this.iconTiles2 = tiles2;
	}
	public void SetListOfRegions (List<RegionData> regions) {
		this.listOfRegions = regions;
	}
	public void SetLineDataList (List<Tuple<Vector2, Vector2, int>> LDL) {
		this.lineDataList = LDL;
	}
	public void SetFitnessFunction(float newFitness){
		overallFitness = newFitness;
	}
	public void AddCover(){
		numOfCovers++;
	}
	public void SetIsBaseMissing (bool missing) {
		isBaseMissing = missing;
	}
	public void SetIsFlagMissing (bool missing) {
		isFlagMissing = missing;
	}

	/*****************/
	/* Get Functions */
	/*****************/
	public MapSettings GetSettings(){
		return settings;
	}
	public int GetHorDimension(){
		return settings.GetHorDimension();
	}
	public int GetVerDimension(){
		return settings.GetVerDimension();
	}
	public int GetNumOfRedFlags(){
		return settings.GetNumOfRedFlags();
	}
	public int GetNumOfBlueFlags(){
		return settings.GetNumOfBlueFlags();
	}
	public int GetNumOfNeutralFlags(){
		return settings.GetNumOfNeutralFlags();
	}
	public bool IsSymmetric(){
		return settings.IsSymmetric();
	}
	public int GetNumOfBases(){
		return settings.GetNumOfBases();
	}
	public int GetDensity(){
		return settings.GetDensity ();
	}

	public int GetNumOfCovers(){
		return numOfCovers;
	}
	public bool IsBaseMissing (){
		return isBaseMissing;
	}
	public bool IsFlagMissing (){
		return isFlagMissing;
	}

	//i,j are minitile coordinates
	public char GetTileComponent(int i, int j){
		char tileComp = new char();
		if (IsValidTileComponent(i,j)){
			tileComp = tileComponents [i, j];
		} else {
			Debug.Log("Attempt to access tileComponents["+i.ToString()+","+j.ToString()+"]");
		}
		return tileComp;
	}
	public char[,] GetTileComponentArray() {
		return tileComponents;
	}
	
	public Vector2 GetRandomEmptyTilePos(){
		int randomIdx = Random.Range (0, emptyTiles.Count);
		Vector2 pos = emptyTiles [randomIdx];
		
		return pos;
	}

	public Dictionary<Vector2, string> GetBaseTiles() {
		return baseTiles;
	}
	
	public Dictionary<Vector2, string> GetIconTiles() {
		return iconTiles;
	}

	public Dictionary<Vector2, string> GetIconTiles2() {
		return iconTiles2;
	}
	
	public List<Tuple<Vector2, Vector2, int>> GetLineDataList(){
		return lineDataList;
	}
	
	public string GetTileString(Vector2 position) {
		if (baseTiles.ContainsKey(position)) {
			return baseTiles [position];
		} else {
			return null;
		}
	}

	public string GetIcon(Vector2 position) {
		if (iconTiles.ContainsKey(position)) {
			return iconTiles [position];
		} else {
			return null;
		}
	}

	public string GetIcon2(Vector2 position) {
		if (iconTiles2.ContainsKey(position)) {
			return iconTiles2 [position];
		} else {
			return null;
		}
	}
	
	public float GetFitnessFunction(){
		return overallFitness;
	}

	public List<RegionData> GetRegions(){
		return listOfRegions;
	}

	/*******************/
	/* Boolean Queries */
	/*******************/
	public bool HasIconTile(Vector2 position){
		return iconTiles.ContainsKey(position);
	}

	public bool HasIconTile2(Vector2 position2){
		return iconTiles2.ContainsKey(position2);
	}

	public bool HasEmptyTiles(){
		if (emptyTiles.Count == 0){
			return false;
		} else {
			return true;
		}
	}

	public bool IsValidTileComponent(int i, int j){
		if (i>=0 && i<settings.GetHorDimension()*2 && j>=0 && j<settings.GetVerDimension()*2){
			return true;
		} else {
			return false;
		}
	}

	// big pos
	public bool IsValidPos(Vector2 pos){
		if (pos.x>=0 && pos.x<settings.GetHorDimension() && pos.y>=0 && pos.y<settings.GetVerDimension()){
			return true;
		} else {
			return false;
		}
	}

	public bool ContainsKey (Vector2 position) {
		if (baseTiles.ContainsKey(position)) {
			return true;
		} else {
			return false;
		}
	}

	/************************************/
	/* Add/Update/Remove Tile Functions */
	/************************************/
	public void AddEmptyTile(Vector2 position) {
		emptyTiles.Add (position);
	}
	
	public void AddBorderTile(Vector2 position) {
		string tile = "kkkk";
		baseTiles.Add (position, tile);
		UpdateTileComponents(position, tile);
	}
	
	public void AddTile (Vector2 position, string tile, string type) {
		if (type == BASE) {
			baseTiles.Add (position, tile);
			UpdateTileComponents(position, tile);
			
			CheckAndRemoveEmptyTile(position);
			
		} else if (type == ICON) {
			iconTiles.Add (position, tile);
		}	
		
	}

	// Everytime baseTiles is modified, this function should be called.
	private void UpdateTileComponents(Vector2 position, string tile){
		int x = (int)position.x*2;
		int y = (int)position.y*2;
		
		//Debug.Log ((x/2).ToString ()+", "+(y/2).ToString ()+": "+tileComponents[x,y]+" becomes "+tile[0].ToString());
		tileComponents [x, y] = tile [0];
		tileComponents [x + 1, y] = tile [1];
		tileComponents [x, y + 1] = tile [2];
		tileComponents [x + 1, y + 1] = tile [3];
		
	}

	private bool isEven(int x){
		if(x%2==0){
			return true;
		}else{
			return false;
		}
	}

	public void UpdateSingleTileComponent (int i, int j, char replacementTile) {
		Vector2 position = new Vector2(Mathf.Floor((float)i/2),Mathf.Floor ((float)j/2));
		string tile = baseTiles[position];
		int tileComponentPosition = 0;
		
		// assume tileComponent is bottom left or right and check which side it is
		// if i is even, tileComponent is on the left. i.e. tile[2]
		if (isEven(i)) {
			tileComponentPosition = 2;
		} else {
			tileComponentPosition = 3;
		}
		
		//if j is even, tileComponent is top left or right thus we minus 2 to get tile[0] or tile[1]
		if (isEven(j)) {
			tileComponentPosition -= 2;
		}
		
		//tile[tileComponentPosition] = 'o';
		char[] tileCharArray = tile.ToCharArray();
		tileCharArray[tileComponentPosition] = replacementTile;
		tile = new string(tileCharArray);

		//Debug.Log ("Update single tile component at "+position.x+", "+position.y+": " 
		//           + baseTiles [position] + " becomes " + tile);
		RemoveTile (position);
		AddTile (position, tile, BASE);
	}

	public void CheckAndRemoveEmptyTile(Vector2 position) {
		if (emptyTiles.Contains(position)) {
			emptyTiles.Remove (position);
		}
	}
	
	public void RemoveTile (Vector2 position) {
		if (baseTiles.ContainsKey(position)) {
			baseTiles.Remove (position);
			if (iconTiles.ContainsKey(position)) {
				iconTiles.Remove (position);
			}
		}
		
		AddEmptyTile(position);
	}

	public void RemoveIcon(Vector2 position){
		iconTiles.Remove (position);
	}

	/*******************/
	/* Reset Functions */
	/*******************/
	public void ClearIconTiles(){
		iconTiles = new Dictionary<Vector2, string>();
		numOfCovers = 0;
		isFlagMissing = false;
		isBaseMissing = false;
	}
	public void ResetRegions(){
		listOfRegions = new List<RegionData> ();
	}
	public void ResetOverallFitness(){
		SetFitnessFunction(0.0f);
	}

	/***********************************************/
	/* Methods Used for Testing and Unused Methods */
	/***********************************************/
	public void SaveMapToFile(string fileName){
		string path = fileName;

		// Create a file to write to. 
		using (StreamWriter sw = File.CreateText(path)) 
		{							
			for (int j=0; j<settings.GetVerDimension(); j++) {
				char[] row = new char[settings.GetHorDimension()*2];
				char[] row2 = new char[settings.GetHorDimension()*2];
				for (int i=0; i<settings.GetHorDimension(); i++) {
					string tile = baseTiles[new Vector2(i, j)];
					row[i*2] = tile[0];
					row[i*2+1] = tile[1];
					row2[i*2] = tile[2];
					row2[i*2+1] = tile[3];
				}
				sw.WriteLine (new string(row));
				sw.WriteLine (new string(row2));
			}

			Debug.Log("Map saved to "+path);
		}	
	}

	
	/*
	 * 	Illustration of positions of a 5 by 5 map
	 * 	[0,0][0,1][0,2][0,3][0,4]
	 *  [1,0][1,1][1,2][1,3][1,4]
	 *  [2,0][2,1][2,2][2,3][2,4]
	 *  [3,0][3,1][3,2][3,3][3,4]
	 *  [4,0][4,1][4,2][4,3][4,4]
	 * 
	 */
	
	//creates a 5 by 5 map with a indoor building right smack in the middle
	//blueTeam base = top left, redTeam base = btm right, collisionPt = in and around the building
	void Test()
	{
		settings.SetHorDimension(5);
		settings.SetVerDimension(5);
		
		AddTile(new Vector2(1,1), "blueTeam", ICON);
		AddTile(new Vector2(3,3), "redTeam", ICON);
		AddTile (new Vector2(1,3), "collisionPt", ICON);
		AddTile (new Vector2(2,2), "collisionPt", ICON);
		AddTile (new Vector2(3,1), "collisionPt", ICON);		
		
		AddTile (new Vector2 (1, 1), "oooi", BASE);
		AddTile (new Vector2 (1, 2), "ooii", BASE);
		AddTile (new Vector2 (1, 3), "ooio", BASE);
		
		AddTile (new Vector2 (2, 1), "oioi", BASE);
		AddTile (new Vector2 (2, 2), "iiii", BASE);
		AddTile (new Vector2 (2, 3), "ioio", BASE);
		
		AddTile (new Vector2 (3, 1), "oioo", BASE);
		AddTile (new Vector2 (3, 2), "iioo", BASE);
		AddTile (new Vector2 (3, 3), "iooo", BASE);
		
		//AddTile (new Vector2 (0, 0), "kkkk", BASE);
		AddTile (new Vector2 (0, 1), "kkkk", BASE);
		AddTile (new Vector2 (0, 2), "kkkk", BASE);
		AddTile (new Vector2 (0, 3), "kkkk", BASE);
		AddTile (new Vector2 (0, 4), "kkkk", BASE);
		
		//AddTile (new Vector2 (0, 4), "kkkk", BASE);
		AddTile (new Vector2 (1, 4), "kkkk", BASE);
		AddTile (new Vector2 (2, 4), "kkkk", BASE);
		AddTile (new Vector2 (3, 4), "kkkk", BASE);
		AddTile (new Vector2 (4, 4), "kkkk", BASE);
		
		AddTile (new Vector2 (0, 0), "kkkk", BASE);
		AddTile (new Vector2 (1, 0), "kkkk", BASE);
		AddTile (new Vector2 (2, 0), "kkkk", BASE);
		AddTile (new Vector2 (3, 0), "kkkk", BASE);
		//AddTile (new Vector2 (4, 0), "kkkk", BASE);
		
		AddTile (new Vector2 (4, 0), "kkkk", BASE);
		AddTile (new Vector2 (4, 1), "kkkk", BASE);
		AddTile (new Vector2 (4, 2), "kkkk", BASE);
		AddTile (new Vector2 (4, 3), "kkkk", BASE);
		//AddTile (new Vector2 (4, 4), "kkkk", BASE);
	}
}