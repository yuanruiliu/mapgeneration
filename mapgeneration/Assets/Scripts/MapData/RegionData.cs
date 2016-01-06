using UnityEngine;
using System.Collections.Generic;
using System;

public class RegionData {

	/*************/
	/* Constants */
	/*************/
	const int UNASSIGNED_ROLE = -1;
	const int NUM_OF_MINITILES_IN_BIGTILE = 4;
	Vector2 INVALID_POSITION = new Vector2(-1,-1);

	/*************/
	/* Variables */
	/*************/
	int length;
	int height;
	int regionID;
	char regionType;
	Color regionColor;
	int regionRole = UNASSIGNED_ROLE; // -1:unassigned, 0:none, 1:collisionPoint, 2:redBase, 3:blueBase, 4:redFlag, 5:blueFlag, 6:neutralFlag

	Vector2 centre; 	// minitile coords, must call 'computeCenter' first - how to add this as a test?
	List<Vector2> minitilePos;
	List<RegionData> connectedRegions;
	Dictionary<RegionData, float> distToConnectedRegions;
	List<RegionData> adjacentUnconnectedRegions;

	public RegionData (char regionType, int regionID) {
		minitilePos = new List<Vector2> ();
		connectedRegions = new List<RegionData> ();
		distToConnectedRegions = new Dictionary<RegionData, float> ();
		adjacentUnconnectedRegions = new List<RegionData> ();
		this.regionID = regionID;
		this.regionType = regionType;
		SetRandomRegionColourFromType();
	}


	public void SetRandomRegionColourFromType(){
		if (this.regionType=='i'){ // darker (more opaque), more blue
			this.regionColor = new Color (UnityEngine.Random.Range (0.0f, 0.6f), UnityEngine.Random.Range (0.1f, 1.0f), UnityEngine.Random.Range (0.5f, 1.0f), 0.5f);
		} else { // lighter (less opaque), more red
			this.regionColor = new Color (UnityEngine.Random.Range (0.5f, 1.0f), UnityEngine.Random.Range (0.1f, 1.0f), UnityEngine.Random.Range (0.0f, 0.6f), 0.3f);
		}
	}
	
	public void AddMinitilePos (float minitileX, float minitileY) {
		if (!minitilePos.Contains (new Vector2 (minitileX, minitileY)))
			minitilePos.Add (new Vector2 (minitileX, minitileY));
	}
	
	public RegionData MergeIntoAdjRegion () {
		if (connectedRegions.Count == 0)
			return null;
		else {
			RegionData smallestConnectedRegion = connectedRegions [0];	// STUB
			MergeIntoRegion (smallestConnectedRegion);
			return smallestConnectedRegion;
		}
	}

	public void MergeIntoRegion (RegionData parentRegion) {
		foreach (Vector2 pos in minitilePos)
			parentRegion.AddMinitilePos (pos.x, pos.y);
		Destroy ();
	}
	
	public void ConnectAdjacentSimilarRegions (RegionData otherRegion) {
		if (connectedRegions.Contains (otherRegion) || adjacentUnconnectedRegions.Contains (otherRegion))	// already know each other
			return;
		if (otherRegion.regionType == regionType) {
			otherRegion.GetConnectedRegionsList ().Add (this);
			connectedRegions.Add (otherRegion);
		} else {
			otherRegion.GetAdjacentUnconnectedRegionsList ().Add (this);
			adjacentUnconnectedRegions.Add (otherRegion);
		}
	}

	Vector2 FindDirectionToSearch (Vector2 otherRegionTile, Vector2 currRegionTile) {
		Vector2 dirToSearch = -((otherRegionTile - currRegionTile).normalized);
		float dirX = dirToSearch.x<0 ? Mathf.Floor(dirToSearch.x) : Mathf.Ceil(dirToSearch.x);
		float dirY = dirToSearch.y<0 ? Mathf.Floor(dirToSearch.y) : Mathf.Ceil(dirToSearch.y);
		dirToSearch = new Vector2(dirX, dirY);
		return dirToSearch;
	}

	// 'draw' a line connecting center of both regions,
	// and Find the big tile that is between them
	Vector2 FindConnectingTile (RegionData otherRegion, bool mustBeAdjacent) {
		Vector2 currRegionCentre = this.GetCentreBig ();
		Vector2 otherRegionCentre = otherRegion.GetCentreBig();

		Vector2 dirToSearch = FindDirectionToSearch (otherRegionCentre, currRegionCentre);

		Vector2 currPos = otherRegionCentre;
		bool tileCannotBeFound = false;
		int count = 0;

		//Debug.Log ("Search from ("+otherRegionCentre.x+","+otherRegionCentre.y+") to ("+
		//           currRegionCentre.x+","+currRegionCentre.y+")");

		// start from other region's centre, search towards curr region's centre
		// to find tile inbetween both regions
		while (!tileCannotBeFound){	
			count ++;
			// if any of the minitile in the big tile does not belong to this region,
			// then found
			Vector2 testPos = new Vector2(currPos.x*2, currPos.y*2);
			bool thisContain = false;
			bool otherContain = false;
			for (int x=0; x<2; x++) {
				for (int y=0; y<2; y++) {
					if (otherRegion.GetMiniTilePosList().Contains (testPos + new Vector2 (x, y))){
						otherContain = true;
					} else if (this.minitilePos.Contains (testPos + new Vector2 (x, y))){
						thisContain = true;
					}
					if(thisContain && otherContain){
						return currPos;
					}
				}
			}

			currPos = currPos + dirToSearch;
			dirToSearch = FindDirectionToSearch (currPos, currRegionCentre); 

			if (currPos==currRegionCentre){
				tileCannotBeFound = true;
			}
			//Debug.Log ("Current position: ("+currPos.x.ToString()+","+currPos.y.ToString()+
			//           "), Direction to search: ("+dirToSearch.x.ToString()+","+
			//           dirToSearch.y.ToString()+").");
		}

		/*
		// if cannot Find along line, then check each tile manually
		HashSet<Vector2> bigTilesChecked = new HashSet<Vector2>();
		foreach(Vector2 miniTile in minitilePos){
			Vector2 bigTile = new Vector2((int)(miniTile.x/2), (int)(miniTile.y/2));
			if(!bigTilesChecked.Contains(bigTile)){
				bool thisContain = false;
				bool otherContain = false;
				for (int x=0; x<2; x++) {
					for (int y=0; y<2; y++) {
						if (!otherContain && otherRegion.minitilePos.Contains (bigTile*2 + new Vector2 (x, y)))
							otherContain = true;
						if (!thisContain && minitilePos.Contains (bigTile*2 + new Vector2 (x, y)))
							thisContain = true;
						if(thisContain && otherContain)
							return bigTile;
					}
				}
			}
		}
		*/

		Debug.Log ("Cannot locate connecting tile!");
		return INVALID_POSITION;
	}
	
	// for collision points
	// return list of big tiles that are connecting to this region's connecting & similar regions
	public List<Vector2> FindAdjacentConnectedSimilarRegionsTiles () {
		List<Vector2> tilesPos = new List<Vector2> ();
		foreach (RegionData otherRegion in connectedRegions) {
			if (otherRegion.GetType () == regionType) {	// similar region
				Vector2 tempPos = FindConnectingTile (otherRegion, true);
				if (tempPos != INVALID_POSITION){
					tilesPos.Add (tempPos);
				}
			}
		}
		return tilesPos;
	}
	
	// connect the regions and return the big position of door
	public Vector2 ConnectAdjacentOpposingRegions (RegionData otherRegion) {
		if (!adjacentUnconnectedRegions.Contains (otherRegion)) {
			Debug.Log ("regions are not adjacent! cannot connect!");
			return INVALID_POSITION;
		}

		Vector2 doorPos = FindConnectingTile (otherRegion, true);

		if (doorPos!=INVALID_POSITION){
			otherRegion.GetConnectedRegionsList ().Add (this);
			connectedRegions.Add (otherRegion);
			otherRegion.GetAdjacentUnconnectedRegionsList ().Remove (this);
			adjacentUnconnectedRegions.Remove (otherRegion);
		}

		return doorPos;
	}
	
	void Destroy () {
		regionID = UNASSIGNED_ROLE;
		// sever connections
		foreach (RegionData adjacentUnconnectedRegion in adjacentUnconnectedRegions)
			adjacentUnconnectedRegion.GetAdjacentUnconnectedRegionsList ().Remove (this);
		foreach (RegionData connectedRegion in connectedRegions) {
			int idxToRemove = connectedRegion.GetConnectedRegionsList ().IndexOf (this);
			connectedRegion.GetConnectedRegionsList ().RemoveAt (idxToRemove);
		}
	}

	//Given TOP LEFT minitile, get all minitiles in the same big tile as given minitile
	//return true if all minitiles are in same region, otherwise false
	bool IsInSameRegion (Vector2 minitile) {
		List<Vector2> minitiles = GetAllMinitilesOfBigTileContaining (minitile);

		for (int i = 0; i < 4; i++) {
			if (!minitilePos.Contains (minitiles [i])) {
				return false;
			}
		}
		return true;
	}

	enum Dir {Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft};
	List<Vector2> GetSurroundingMinitilesGivenDir (List<Vector2> originalMinitiles, Dir dir) {
		List<Vector2> minitiles = new List<Vector2> ();

		for(int i=0; i<NUM_OF_MINITILES_IN_BIGTILE; i++){
			float xPos = originalMinitiles[i].x;
			float yPos = originalMinitiles[i].y;

			switch(dir){
			case Dir.Top: minitiles.Add (new Vector2(xPos, yPos-2)); break;
			case Dir.TopRight: minitiles.Add (new Vector2(xPos+2, yPos-2)); break;
			case Dir.Right: minitiles.Add (new Vector2(xPos+2, yPos)); break;
			case Dir.BottomRight: minitiles.Add (new Vector2(xPos+2, yPos+2)); break;
			case Dir.Bottom: minitiles.Add (new Vector2(xPos, yPos+2)); break;
			case Dir.BottomLeft: minitiles.Add (new Vector2(xPos-2, yPos+2)); break;
			case Dir.Left: minitiles.Add (new Vector2(xPos-2, yPos)); break;
			case Dir.TopLeft: minitiles.Add (new Vector2(xPos-2, yPos-2)); break;
			}
		}
		return minitiles;
	}

	Vector2 GetSurroundingTileWhichFallsEntirelyInOneRegion (Vector2 originalTile) {
		List<Vector2> originalMinitiles = GetAllMinitilesOfBigTileContaining (originalTile);	 

		foreach(Dir dir in Enum.GetValues(typeof(Dir))) {
			List<Vector2> minitiles = GetSurroundingMinitilesGivenDir (originalMinitiles, dir);
			if (IsInSameRegion (minitiles[0])) {
				return minitiles[0];
			} 
		}
		return originalTile;
	}

	public Vector2 ComputeCentre () {
		float totalX = 0;
		float totalY = 0;
		int numX = 0;
		int numY = 0;

		foreach (Vector2 pos in minitilePos) {
			totalX += pos.x;
			totalY += pos.y;
			numX++;
			numY++;
		}

		Vector2 tempCentre = new Vector2 (Mathf.RoundToInt(totalX/numX), Mathf.RoundToInt(totalY/numY));

		//ALTERNATIVE WAY TO CALCULATE CENTRE
		/*
		int minX, minY, maxX, maxY;
		FindMinAndMaxXYCoordsOfRegion (out minX, out minY, out maxX, out maxY);
		SetDimensions (maxX - minX, maxY - minY);
		SetCentre (new Vector2 ((int)(minX + length / 2), (int)(minY + height / 2)));
		*/

		if (!IsInSameRegion (tempCentre)) {
			tempCentre = GetSurroundingTileWhichFallsEntirelyInOneRegion (tempCentre);
		}

		SetCentre(tempCentre);

		return tempCentre;
	}
	
	//private List<Vector2> GetBigMinitiles(Vector2 miniTile){
	List<Vector2> GetAllMinitilesOfBigTileContaining (Vector2 minitile) {
		float xPos = Mathf.Floor(minitile.x/2)*2;
		float yPos = Mathf.Floor(minitile.y/2)*2;
		List<Vector2> minitiles = new List<Vector2>();
		minitiles.Add(new Vector2(xPos, yPos));
		minitiles.Add(new Vector2(xPos+1, yPos));
		minitiles.Add(new Vector2(xPos, yPos+1));
		minitiles.Add(new Vector2(xPos+1, yPos+1));
		return minitiles;
	}
	
	// using dijkstra algorithm, find shortest distance from this region to dest region,
	// note that direct distance between regions are used, so it's approximate
	public float PathDistanceTo (List<RegionData> allRegions, RegionData destRegion) {

		Queue<RegionData> queue = new Queue<RegionData> ();
		float[] dist = new float[allRegions.Count];
		bool[] visited = new bool[allRegions.Count];
		RegionData[] prev = new RegionData[allRegions.Count];
		
		foreach (RegionData region in allRegions) {
			dist [region.GetID ()] = float.MaxValue;	// Mark distances from source to v as not yet computed
			visited [region.GetID ()] = false;			// Mark all nodes as unvisited
			prev [region.GetID ()] = null;				// Previous node in optimal path from source
		}
		
		dist [regionID] = 0;	// Distance from source to itself is zero
		queue.Enqueue (this);	// Start off with the source node
		
		while (queue.Count != 0) {
			
			RegionData u = queue.Dequeue ();
			visited [u.GetID ()] = true;
			
			if (u == destRegion)	// completed
				break;
			
			foreach (RegionData v in u.GetConnectedRegionsList()) {	// v is u's neighbour
				float newDist = dist [u.GetID ()] + u.GetDistanceToConnectedRegion (v);
				if (newDist < dist [v.GetID ()]) {
					dist [v.GetID ()] = newDist;
					prev [v.GetID ()] = u;
					if (!visited [v.GetID ()])
						queue.Enqueue (v);
				}
			}
		}

		/*
		string output = "";
		foreach (RegionData region in allRegions) {
			output = "Region " + region.GetID () + " : ";
			foreach (RegionData connected in region.GetConnectedRegionsList()) {
				output += "{" + connected.GetID () + ": " + region.GetDistanceToConnectedRegion (connected) + "}, ";
			}
			Debug.Log(output);
		}
		*/

		return dist [destRegion.GetID ()];
	}
	
	public void ComputeDistToConnectedRegion () {
		foreach (RegionData region in connectedRegions)
			if (!distToConnectedRegions.ContainsKey (region))
				distToConnectedRegions.Add (region, Vector2.Distance (centre, region.GetCentre ()));
	}

	/*******************/
	/* Boolean Queries */
	/*******************/
	public bool IsTaller () {
		return length < height;
	}

	public bool HasRole () {
		return regionRole != UNASSIGNED_ROLE;
	}

	/*****************/
	/* Get Functions */
	/*****************/
	public int GetLength() {
		return length;
	}

	public int GetHeight() {
		return height;
	}

	public int GetID () {
		return regionID;
	}

	public char GetType () {
		return regionType;
	}

	public Color GetColor() {
		return regionColor;
	}

	public int GetRole() {
		return regionRole;
	}

	public List<Vector2> GetMiniTilePosList() {
		return minitilePos;
	}

	public Vector2 GetCentre () {
		return centre;
	}
	
	public Vector2 GetCentreBig () {
		return new Vector2 ((int)(centre.x/2), (int)(centre.y/2));
	}
	
	public List<RegionData> GetConnectedRegionsList () {
		return connectedRegions;
	}
	
	public Dictionary<RegionData, float> GetConnectedRegionsDistList () {
		return distToConnectedRegions;
	}
	
	public List<RegionData> GetAdjacentUnconnectedRegionsList () {
		return adjacentUnconnectedRegions;
	}
	
	public int GetConnectivityDegree () {
		return connectedRegions.Count;
	}

	public float GetDistanceToConnectedRegion (RegionData otherRegion) {
		if (!distToConnectedRegions.ContainsKey (otherRegion)) {	// not found
			Debug.Log ("Cannot Get distance to non-connecting region");
			return -9999;
		} else
			return (float)(distToConnectedRegions [otherRegion]);
	}

	/*****************/
	/* Set Functions */
	/*****************/
	public void SetRole (int role) {
		regionRole = role;
	}

	public void SetDimensions (int length, int height) {
		Debug.Log ("IS THIS BEING CALLED - RD");
		this.length = length;
		this.height = height;
	}

	public void SetCentre (Vector2 center) {
		this.centre = center;
	}

	public void SetMinitilePos (List<Vector2> minitilePos) {
		this.minitilePos = minitilePos;
	}

	/***********************************************/
	/* Methods Used for Testing and Unused Methods */
	/***********************************************/
	void InitializeMinAndMaxXYCoordsOfRegion (out int minX, out int minY, out int maxX, out int maxY) {
		minX = int.MaxValue;
		minY = int.MaxValue;
		maxX = int.MinValue;
		maxY = int.MinValue;
	}
	
	void FindMinAndMaxXYCoordsOfRegion (out int minX, out int minY, out int maxX, out int maxY) {
		InitializeMinAndMaxXYCoordsOfRegion (out minX, out minY, out maxX, out maxY);
		
		foreach (Vector2 pos in minitilePos) {
			if (pos.x < minX)
				minX = (int)pos.x;
			if (pos.x > maxX)
				maxX = (int)pos.x;
			if (pos.y < minY)
				minY = (int)pos.y;
			if (pos.y > maxY)
				maxY = (int)pos.y;
		}
	}

}
