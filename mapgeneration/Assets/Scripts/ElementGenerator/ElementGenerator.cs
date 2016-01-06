using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; //for LIST

public class ElementGenerator {
	MapData map;
	List<RegionData> listOfRegions;
	List<RegionData> listOfRedFlagRegions;
	List<RegionData> listOfBlueFlagRegions;
	List<RegionData> listOfNeutralFlagRegions;

	int diagLength;
	int maxRegionSize;
	int minRegionSize;

	//Constants
	const int NUM_OF_COLLISION_POINTS_OPTIMAL = 2;
	const int NUM_OF_COLLISION_POINTS_MAX = 3;
	const int NUM_FOR_COLLISION_POINT_THRESHOLD = 7;

	const int MAX_NUM_OF_REGIONS = 200;
	const float PROBABILITY_FOR_SPAWNING_SECOND_DOOR = 0.3f;
	const string ICON = "icons";
	const string BASE = "bases";

	const char UNASSIGNED = 'U';
	Vector2 INVALID_POSITION = new Vector2(-1,-1);

	enum IconNum {
		UNASSIGNED = -1,
		CENTRE_POINT = 0,
		COLLISION_POINT = 1,
		RED_BASE = 2,
		BLUE_BASE = 3,
		RED_FLAG = 4,
		BLUE_FLAG = 5,
		NEUTRAL_FLAG = 6,
		DOOR = 7,
		HORIZONTAL_COVER = 8,
		VERTICAL_COVER = 9
	}

	// fitness
	float connectivityFitness = 0;	// connectivity (0 or 1)
	float collisionPtFitness = 0;	// collision points (0:0, 1:1, 2:0.5, >=2:0)
	float teamFlagFairnessFitness = 0;	// team flag fairness - difference in dijkstra dist to team flags
	float overallFlagFairnessFitness = 0;	// overall flag fairness - difference in mean dijkstra dist to all flags
	float coverFitness = 0;	//ratio of actual covers:density required

	public ElementGenerator() {
	}

	public MapData CreateNewMap(MapSettings settings){
		InitializeMapMetaData (settings);
		map = new MapData(settings);
		GenerateFrame ();
		CompleteMap ();
		return map;
	}
	
	public MapData SendMapForMutation(MapData mapFromManager){
		this.map = mapFromManager;
		InitializeMapMetaData(map.GetSettings());
		MutateMap ();
		CompleteMap ();
		return map;
	}

	void InitializeMapMetaData(MapSettings settings){
		int horLength = settings.GetHorDimension();
		int verLength = settings.GetVerDimension();

		this.diagLength = Mathf.CeilToInt(Mathf.Sqrt (Mathf.Pow(horLength, 2)+ Mathf.Pow(verLength,2)));
		this.maxRegionSize = Mathf.CeilToInt (horLength * verLength / 5);
		if (horLength * verLength < 400) {
			this.minRegionSize = 5;
		} else {
			this.minRegionSize = 10;
		}

		this.listOfRegions = new List<RegionData>();
	}

	void GenerateFrame () {
		for (int i=0; i<map.GetHorDimension(); i++) {
			for (int j=0; j<map.GetVerDimension(); j++) {
				if (i == 0 || j == 0 || i == map.GetHorDimension()-1 || j == map.GetVerDimension()-1) {
					map.AddBorderTile (new Vector2 (i, j));
				} else {
					map.AddEmptyTile (new Vector2 (i, j));
				}
			}
		}

		Debug.Log ("Created brand new map frame");
	}

	void CompleteMap () {
		FillTiles();
		CreateRegions();
		ConnectRegions();
		AssignRegionsToRoles ();
		AssignFitnessToMap();
	}

	void MutateMap () {
		Vector2 randomMutationPoint = new Vector2 (UnityEngine.Random.Range (2 + 1, map.GetHorDimension() - 2 - 1), 
		                                           UnityEngine.Random.Range (2 + 1, map.GetVerDimension() - 2 - 1));
		map.ClearIconTiles ();
		map.ResetRegions ();
		map.ResetOverallFitness ();
		RemoveRandomTiles(randomMutationPoint);

		Vector2 pos = new Vector2 ((int)(randomMutationPoint.x), (int)(randomMutationPoint.y));
		string tileString = CreateMatchingTile ((int)pos.x, (int)pos.y);
		//Creates ONE random tile in the centre of removed tiles to ensure that a different set of tiles 
		//will be generated in FillTiles() every time.
		map.AddTile (pos, tileString, BASE);//Since all adj tiles will be null, tile added will be random.
	}

	void RemoveRandomTiles (Vector2 mutationPoint) {
		for (int i=-2; i<3; i++)
			for (int j=-2; j<3; j++)
				map.RemoveTile (new Vector2((int)(mutationPoint.x) + i, (int)(mutationPoint.y) + j));	
	}

	void CopyFromSide (string neighbour, ref char newTop, ref char newBottom, string side) {
		if (neighbour != null && side == "left") {
			newTop = neighbour [1];
			newBottom = neighbour [3];
		} else if (neighbour != null && side == "right") {
			newTop = neighbour [0];
			newBottom = neighbour [2];
		}
	}
		
	// create a tile that matches adjacent tiles, return tileString
	// tileX = x-coord of tile undergoing mutation, tileY = y-coord of tile undergoing mutation
	string CreateMatchingTile (int tileX, int tileY) {
		string topNeighbourTileString = null;
		string leftNeighbourTileString = null;
		string rightNeighbourTileString = null;
		string bottomNeighbourTileString = null;
		GetAdjacentTileStrings (tileX, tileY, 
		                  out topNeighbourTileString, out leftNeighbourTileString, 
		                  out rightNeighbourTileString, out bottomNeighbourTileString);

		char newTopLeft = UNASSIGNED;
		char newTopRight = UNASSIGNED;
		char newBottomLeft = UNASSIGNED;
		char newBottomRight = UNASSIGNED;

		CopyFromSide(leftNeighbourTileString, ref newTopLeft, ref newBottomLeft, "left");
		CopyFromSide(rightNeighbourTileString, ref newTopRight, ref newBottomRight, "right");

		//Debug.Log ("CreateMatchingTile ("+tileX.ToString () + ", " + tileY.ToString ()+")'s newTileString: " 
		//           + newTopLeft+ newTopRight+ newBottomLeft+ newTopRight);

		if (topNeighbourTileString != null) {
			CopyFromTopBottomIfUnassigned(topNeighbourTileString, ref newTopLeft, ref newTopRight, "top");
			RemoveNeighbourIfMismatch (tileX, tileY, topNeighbourTileString, newTopLeft, newTopRight, "top");
		}

		if (bottomNeighbourTileString != null) {
			CopyFromTopBottomIfUnassigned(bottomNeighbourTileString, ref newBottomLeft, ref newBottomRight, "bottom");
			RemoveNeighbourIfMismatch (tileX, tileY, bottomNeighbourTileString, newBottomLeft, newBottomRight, "bottom");
		}

		RandomlyFillUnassignedMinitiles (ref newTopLeft, ref newTopRight, ref newBottomLeft, ref newBottomRight);

		string tileString = "" + newTopLeft + newTopRight + newBottomLeft + newBottomRight;
		
		return tileString;
	}
	 
	void GetAdjacentTileStrings (int tileX, int tileY,
	                      		 out string top, out string left, 
	                             out string right, out string bottom) {
		Vector2 topPos = new Vector2 (tileX, tileY - 1);
		top = map.GetTileString (topPos);

		Vector2 leftPos = new Vector2 (tileX - 1, tileY);
		left = map.GetTileString (leftPos);

		Vector2 rightPos = new Vector2 (tileX + 1, tileY);
		right = map.GetTileString (rightPos);

		Vector2 bottomPos = new Vector2 (tileX, tileY + 1);
		bottom = map.GetTileString (bottomPos);
	}

	void CopyFromTopBottomIfUnassigned (string neighbour, ref char newLeft, ref char newRight, string side) {
		if (side == "top") { //copy from top
			if (newLeft == UNASSIGNED) {
				newLeft = neighbour[2];
			} 
			if (newRight == UNASSIGNED) {
				newRight = neighbour[3];
			}
		} else if (side == "bottom") { //copy from bottom
			if (newLeft == UNASSIGNED) {
				newLeft = neighbour[0];
			} 
			if (newRight == UNASSIGNED) {
				newRight = neighbour[1];
			}
		} else {
			Debug.Log("CreateMatchingTile -> CopyFromTopBottomIfUnassigned received invalid side param");
		}
	}
	void RemoveNeighbourIfMismatch (int tileX, int tileY, string neighbour, char newLeft, char newRight, string side) {
		if (side == "top") {
			if (newLeft != neighbour [2] || newRight != neighbour [3]) {
				map.RemoveTile (new Vector2 (tileX, tileY - 1));
			}
		} else if (side == "bottom") {
			if (newLeft != neighbour [0] || newRight != neighbour [1]) {
				map.RemoveTile (new Vector2 (tileX, tileY + 1));
			}
		} else {
			Debug.Log ("CreateMatchingTile -> RemoveNeighbourIfMismatch received invalid side param");
		}
	}

	void RandomlyFillUnassignedMinitiles (ref char newTopLeft, ref char newTopRight, ref char newBottomLeft, ref char newBottomRight) {
		// replace unassigned minitile with some random value based on adjacent tiles
		if (newTopLeft == UNASSIGNED)
			newTopLeft = GetRandomTileValBasedOnAdjTiles (newTopRight, newBottomLeft);
		if (newTopRight == UNASSIGNED)
			newTopRight = GetRandomTileValBasedOnAdjTiles (newTopLeft, newBottomRight);
		if (newBottomLeft == UNASSIGNED)
			newBottomLeft = GetRandomTileValBasedOnAdjTiles (newTopLeft, newBottomRight);
		if (newBottomRight == UNASSIGNED)
			newBottomRight = GetRandomTileValBasedOnAdjTiles (newTopRight, newBottomLeft);
	}
	
	char GetRandomTileValBasedOnAdjTiles (char adjTile1, char adjTile2) {
		
		char currAdjTile = adjTile1;
		int numO = 0;
		int numI = 0;
		int numK = 0;
		
		for (int i=0; i<2; i++) {
			switch (currAdjTile) {
			case 'o':
				numO++;
				break;
			case 'i':
				numI++;
				break;
			case 'k':
				numK++;
				break;
			}
			currAdjTile = adjTile2;
		}
		
		char randTileVal;
		if (numO == 2)						//Both outdoor
			randTileVal = GetRandomTileVal (0.9, 0.1, 0);
		else if (numI == 2)					//Both indoor
			randTileVal = GetRandomTileVal (0.1, 0.9, 0);
		else if (numK == 2)					//Both inaccessible
			randTileVal = GetRandomTileVal (0.1, 0.1, 0.8);
		else if (numO == 1 && numI == 1)	//One O, one I
			randTileVal = GetRandomTileVal (0.4, 0.6, 0);
		else if (numO == 1 && numK == 1)	//One O, one K
			randTileVal = GetRandomTileVal (0.8, 0.1, 0.1);
		else if (numI == 1 && numK == 1)	//One I, one K
			randTileVal = GetRandomTileVal (0, 0.3, 0.7);		// special condition : cannot link k with i directly
		else if (numO == 1)					//One O, one null
			randTileVal = GetRandomTileVal (0.9, 0.1, 0);
		else if (numI == 1)					//One I, one null
			randTileVal = GetRandomTileVal (0.1, 0.9, 0);
		else if (numK == 1)					//One K, one null
			randTileVal = GetRandomTileVal (0.25, 0.25, 0.5);
		else								//both null
			randTileVal = GetRandomTileVal (0.6, 0.4, 0);  
		
		return randTileVal;
	}

	char GetRandomTileVal (double oProb, double iProb, double kProb) {
		float randomVal = (float)(UnityEngine.Random.Range (0.0f, 1.0f));
		if (randomVal < oProb)
			return 'o';
		else if (randomVal < oProb + iProb)
			return 'i';
		else
			return 'k';
	}


	void FillTiles () {
		while (map.HasEmptyTiles()) {
			Vector2 pos = map.GetRandomEmptyTilePos();		
			string tileString = CreateMatchingTile ((int)pos.x, (int)pos.y);
			map.AddTile (pos, tileString, BASE);
		}
	}

	// horizontal example: 
	// o o o i i o o o 
	//       ^ outliers ( i i ) will become:
	// o o o o o o o o

	// replace outliers (horizontally)
	public void ReplaceHorOutlierTiles(){
		int lastO = -1;
		for (int j=0; j<map.GetVerDimension()*2; j++) {
			for (int i=0; i<map.GetHorDimension()*2; i++) {
				char tileComponent = map.GetTileComponent(i, j);
				if (tileComponent == 'o' || tileComponent == 'k') {
					if (lastO != -1) {
						if (i - lastO == 3) {	//presence of outliers
							map.UpdateSingleTileComponent (i-1, j, 'o');
							map.UpdateSingleTileComponent (i-2, j, 'o');
						}
					}
					lastO = i;
				}
			}
			lastO = -1;
		}
	}

	public void ReplaceVerOutlierTiles(){
		int lastO = -1;
		for (int i=0; i<map.GetHorDimension()*2; i++) {
			for (int j=0; j<map.GetVerDimension()*2; j++) {
				char tileComponent = map.GetTileComponent(i,j);
				if (tileComponent == 'o' || tileComponent == 'k') {
					if (lastO != -1) {
						if (j - lastO == 3) {	//presence of outliers
							map.UpdateSingleTileComponent (i, j-1, 'o');
							map.UpdateSingleTileComponent (i, j-2, 'o');
						}
					}
					lastO = j;
				}
			}
			lastO = -1;
		}
	}

	bool IsBorder(char tileComponent) {
		if(tileComponent == 'k') { 
			return true;
		} else {
			return false;
		}
	}

	bool IsSameTileComponent(char currTileComponent, char regionTileComponent){
		if (currTileComponent == regionTileComponent) {
				return true;
		} else {
				return false;
		}
	}

	void MakeMapSymmetric(){
		int miniHorLength = map.GetHorDimension() * 2;
		int miniVerLength = map.GetVerDimension() * 2;

		for (int y=0; y<miniVerLength; y++){
			for (int x=0; x<miniHorLength/2; x++){
				int correspondingX = miniHorLength-x-1;
				int correspondingY = miniVerLength-y-1;

				if (IsMiddleStrip(x,y)){
					UpdateMiddleStrip(correspondingX, correspondingY);
				}
				
				map.UpdateSingleTileComponent (x, y, 
				                       map.GetTileComponent(correspondingX, correspondingY));
				
			}
		}
	}

	private void UpdateMiddleStrip(int correspondingX, int correspondingY){
		char middleTile;
		if (map.GetHorDimension()%2 == 0){ 
			middleTile = 'o';
		} else {
			middleTile = map.GetTileComponent(correspondingX+1,correspondingY); // copy tile from the right (opposite side)
		}
		map.UpdateSingleTileComponent (correspondingX, correspondingY, middleTile);
	}
	
	private bool IsMiddleStrip(int x, int y){ // excludes borders
		bool isMiddleX;
		int widthOfMiddle = 1+(map.GetHorDimension()%2); // if map.GetHorDimension() is odd, need middle strip to be 2 minitiles wide
		isMiddleX = (x >= (map.GetHorDimension()-widthOfMiddle));

		bool isMiddleY = (y<(map.GetVerDimension()*2-3)) && (y>=3);

		return isMiddleY && isMiddleX;
	}

	void AddPositionToRegion (Vector2 position, RegionData currRegion, int regionId, ref int regionSize,
	                          Dictionary<Vector2, int> visitedTiles, Queue<Vector2> neighboursQueue) {
		//if pos is in the same region as currRegion (e.g. both indoors), add pos to currRegion
		visitedTiles.Add(position,regionId);
		currRegion.AddMinitilePos(position.x, position.y);
		regionSize++;
		EnqueueNeighbours(position,visitedTiles,neighboursQueue);
	}

	bool CheckIfRegionIsSmallAndMerge (Vector2 position, char regionTileComponent, RegionData currRegion, int currRegionSize) {
		bool isMerged = false;
		if (currRegionSize < minRegionSize) {
			//Debug.Log ("Small region detected at " + position.x.ToString () + "," + position.y.ToString ());
			List<RegionData> listOfNeighbouringRegions = currRegion.GetConnectedRegionsList ();
			foreach (RegionData currNeighbourRegion in listOfNeighbouringRegions) {
				if (currNeighbourRegion.GetType () == regionTileComponent) {
					currRegion.MergeIntoRegion (currNeighbourRegion);
					isMerged = true;
					//Debug.Log ("Merged small region with neighbour.");
					break;
				}
			}
		}
		return isMerged;
	}

	int FillCurrRegion(int currRegionId, Dictionary<Vector2, int> visitedTiles, Vector2 position){
		char regionTileComponent = map.GetTileComponent ((int)position.x,(int)position.y);
		RegionData currRegion = new RegionData (regionTileComponent, currRegionId);

		//bfs: Declare & Initialize Params For Filling CurrRegion
		Vector2 currPosition;
		int currRegionSize = 0;

		Queue<Vector2> neighboursQueue = new Queue<Vector2> ();
		neighboursQueue.Enqueue (position); //starting point

		while (neighboursQueue.Count != 0 && currRegionSize < maxRegionSize) {
			currPosition = neighboursQueue.Dequeue();
			char currTileComponent = map.GetTileComponent((int)currPosition.x, (int)currPosition.y);

			//don't add in borders
			if(!IsBorder(currTileComponent)) {
				if(IsSameTileComponent(currTileComponent, regionTileComponent) && !visitedTiles.ContainsKey(currPosition)) {
					AddPositionToRegion(currPosition, currRegion, currRegionId, ref currRegionSize, visitedTiles, neighboursQueue);
				}

				if (visitedTiles.ContainsKey(currPosition) && listOfRegions.Count > visitedTiles[currPosition]){
					RegionData otherRegion = listOfRegions[visitedTiles[currPosition]];
					currRegion.ConnectAdjacentSimilarRegions(otherRegion);
					/*Debug.Log ("Connection between Region "+currRegionId.ToString()+" and Region "+
				           otherRegion.GetID().ToString()+" processed.");
					*/

				}

			}
		}

		bool isMerged = CheckIfRegionIsSmallAndMerge (position, regionTileComponent, currRegion, currRegionSize);

		if (!isMerged){
			listOfRegions.Add (currRegion);
			return currRegionId+1; 
		} else {
			return currRegionId;
		}

	}

	void EnqueueNeighbours(Vector2 position, Dictionary<Vector2, int> visitedTiles, Queue<Vector2> queue){
		Vector2 neighbour;
		//top
		neighbour = new Vector2 (position.x, position.y - 1);
		CheckAndEnqueueNeighbour (neighbour, visitedTiles, queue);
		//bottom
		neighbour = new Vector2 (position.x, position.y + 1);
		CheckAndEnqueueNeighbour (neighbour, visitedTiles, queue);
		//left 
		neighbour = new Vector2 (position.x - 1, position.y);
		CheckAndEnqueueNeighbour (neighbour, visitedTiles, queue);
		//right
		neighbour = new Vector2 (position.x + 1, position.y);
		CheckAndEnqueueNeighbour (neighbour, visitedTiles, queue);

	}

	void CheckAndEnqueueNeighbour(Vector2 neighbour, Dictionary<Vector2, int> visitedTiles, Queue<Vector2> queue){
		if (!queue.Contains(neighbour) && map.IsValidTileComponent((int)neighbour.x, (int)neighbour.y)){
			queue.Enqueue (neighbour);
		}
	}

	void DiscoverRegions () {
		Dictionary<Vector2, int> visitedTiles = new Dictionary<Vector2, int> ();
		int currRegionId = 0;

		for (int j=0; j<map.GetVerDimension()*2; j++){
			for (int i=0; i<map.GetHorDimension()*2; i++){
				Vector2 position = new Vector2(i,j);
				if ((!visitedTiles.ContainsKey(position)) && (map.GetTileComponent(i, j) != 'k')){
					currRegionId = FillCurrRegion(currRegionId, visitedTiles, position);
				}
			}
		}
		map.SetListOfRegions(listOfRegions);
	}
	
	private bool IsNotOutdoors(char c){
		if (c=='i'||c=='k'){
			return true;
		} else {
			return false;
		}
	}

	private bool IsSurrounded(int i, int j){
		if (j-1>0 && j+2<(map.GetHorDimension()*2)){
			bool aboveNotOutdoors = IsNotOutdoors(map.GetTileComponent(i-1, j-1))&& IsNotOutdoors(map.GetTileComponent(i-2,j-1));
			bool belowNotOutdoors = IsNotOutdoors(map.GetTileComponent(i-1, j+2)) && IsNotOutdoors(map.GetTileComponent(i-2,j+2));
			bool sidesNotOutdoors = IsNotOutdoors(map.GetTileComponent(i-3, j+1)) && IsNotOutdoors(map.GetTileComponent(i,j+1));

			return aboveNotOutdoors && belowNotOutdoors && sidesNotOutdoors;
		} else {
			return false;
		}
	}

	private bool HasISurrounding(int i,int j){
		if (map.GetTileComponent(i-1, j-1)=='i'||map.GetTileComponent(i-2,j-1)=='i'
				|| map.GetTileComponent(i-1, j+2)=='i'||map.GetTileComponent(i-2,j+2)=='i'
				|| map.GetTileComponent(i-3, j+1)=='i'||map.GetTileComponent(i,j+1)=='i'
				|| map.GetTileComponent(i-3, j)=='i'||map.GetTileComponent(i,j)=='i'){
			return true;
		} else {
			return false;
		}
	}
	public void ReplaceOneTiledOutdoors(){
		int lastI = -1;
		char replacementChar = 'k';
		for (int j=0; j<map.GetVerDimension()*2; j++) {
			for (int i=0; i<map.GetHorDimension()*2; i++) {
				char tileComponent = map.GetTileComponent(i, j);
				if (IsNotOutdoors(tileComponent)) {
					bool hasOutliers = lastI != -1 && i-lastI == 3; 
					if (hasOutliers && IsSurrounded(i,j)) {
						if (HasISurrounding(i,j)){
							replacementChar = 'i';
						}
						map.UpdateSingleTileComponent (i-1, j, replacementChar);
						map.UpdateSingleTileComponent (i-2, j, replacementChar);
						map.UpdateSingleTileComponent (i-1, j+1, replacementChar);
						map.UpdateSingleTileComponent (i-2, j+1, replacementChar);
					}
					lastI = i;
				}
			}
			lastI = -1;
		}
	}

	void CreateRegions () {
		ReplaceHorOutlierTiles();
		ReplaceVerOutlierTiles();
		ReplaceOneTiledOutdoors();
		if (map.IsSymmetric()) {
			MakeMapSymmetric();
		}
		DiscoverRegions();
	}

	void ConnectRegions () {
		Queue<RegionData> toVisit = new Queue<RegionData>();
		List<RegionData> listOfUnvisitedRegions = new List<RegionData> (listOfRegions);
		MarkCentrePoints (listOfRegions);

		for (int i = 0; i < listOfUnvisitedRegions.Count; i++) {
			toVisit.Enqueue(listOfRegions[i]);
			while(toVisit.Count!=0) {
				VisitSameTypeRegions(toVisit,listOfUnvisitedRegions);
			}
			// At this point, any regions left in listOfUnvisitedRegions are not connected to 
			// the main graph. We will need to connect them with doors.
			foreach(RegionData unvisitedRegion in listOfUnvisitedRegions){
				bool success = ConnectUnconnectedRegionsWithDoors(unvisitedRegion, listOfUnvisitedRegions);
				if (success) {
					toVisit.Enqueue(unvisitedRegion);
				}
			}
		}

		if(listOfUnvisitedRegions.Count ==0){
			connectivityFitness = 1;//Entire graph is connected. Connect regions is done
		} else {
			connectivityFitness=0;//Graph is not connected. There is an ERROR.
			Debug.Log ("Graph is not connected.");
		}
				
		List<Tuple<Vector2, Vector2, int>> lineDataList = map.GetLineDataList();
		foreach (RegionData region in listOfRegions) {
			// Saves a list of 3-tuples of (centrePointOfRegion1, centrePointOfRegion2, linetype) 
			// in MapData where linetype =0 when same region type (e.g. outdoor and outdoor), 
			// linetype=1 when they are different.

			ComputeDistanceAndStoreLineData (region, lineDataList);
		}
	}

	bool CreateDoor(RegionData currRegion, RegionData otherRegion){
		Vector2 bigPosOfDoor = currRegion.ConnectAdjacentOpposingRegions(otherRegion);
		if (map.IsValidPos(bigPosOfDoor)) {
			if (!HasAdjDoors (bigPosOfDoor) && IsValidPosToAddIconTile (bigPosOfDoor)) {
				AddIconTile (bigPosOfDoor, IconNum.DOOR);
				return true;
			} 
		}
		return false;
	}

	bool HasAdjDoors(Vector2 bigPos){
		float xPos = bigPos.x;
		float yPos = bigPos.y;
		Dictionary<Vector2, string> iconTiles = map.GetIconTiles();
		if (iconTiles.ContainsKey(new Vector2 (xPos+1, yPos)) && iconTiles [new Vector2 (xPos+1, yPos)] == "door") {
			return true;
		} else if(iconTiles.ContainsKey(new Vector2 (xPos-1, yPos)) && iconTiles [new Vector2 (xPos-1, yPos)] == "door") {
			return true;
		} else if(iconTiles.ContainsKey(new Vector2 (xPos, yPos+1)) && iconTiles [new Vector2 (xPos, yPos+1)] == "door") {
			return true;
		} else if(iconTiles.ContainsKey(new Vector2 (xPos, yPos-1)) && iconTiles [new Vector2 (xPos, yPos-1)] == "door") {
			return true;
		} else{
			return false;
		}
	}

	// visits regions of the same type (e.g. outdoor and outdoor)
	void VisitSameTypeRegions(Queue<RegionData> toVisit, List<RegionData> listOfUnvisitedRegions){
		RegionData currRegion = toVisit.Dequeue();
		if (listOfUnvisitedRegions.Contains (currRegion)) {
			if(currRegion.GetConnectedRegionsList().Count != 0) {	//Ruio: ADDED THIS CUZ IF NOT HE ANYHOW REMOVE FROM LIST OF UNVISITED REGIONS???
				listOfUnvisitedRegions.Remove (currRegion);
				foreach (RegionData connectedRegion in currRegion.GetConnectedRegionsList()) {
					toVisit.Enqueue (connectedRegion);
				}
			}
		}
	}

	void ComputeDistanceAndStoreLineData(RegionData currRegion, List<Tuple<Vector2, Vector2, int>> list){
		currRegion.ComputeDistToConnectedRegion ();
		List<RegionData> listOfConnectedRegions = currRegion.GetConnectedRegionsList ();
		foreach (RegionData connectedRegion in listOfConnectedRegions) {
			int lineType;
			bool isSameRegion = connectedRegion.GetType()==currRegion.GetType();
			if(isSameRegion){
				lineType = 0;
			}else{
				lineType = 1;
			}
			Tuple<Vector2, Vector2, int> lineData = Tuple.Create(currRegion.GetCentre(), connectedRegion.GetCentre(), lineType);
			list.Add(lineData);
		}
	}
	
	bool ConnectUnconnectedRegionsWithDoors(RegionData currRegion, List<RegionData> listOfUnvisitedRegions){
		int highestDegree = -1;
		RegionData highestDegreeRegion=null;
		RegionData secondHighestDegreeRegion=null;

		foreach(RegionData adjRegion in currRegion.GetAdjacentUnconnectedRegionsList()){
			bool isAdjRegionUnvisited = listOfUnvisitedRegions.Contains(adjRegion);
			int adjRegionDegree = adjRegion.GetConnectivityDegree();
			if(!isAdjRegionUnvisited){ //This causes doors to not be spawned on RARE occasions
				if(highestDegree<adjRegionDegree){
					secondHighestDegreeRegion = highestDegreeRegion;
					highestDegree = adjRegionDegree;
					highestDegreeRegion = adjRegion;
					
				}
			}
		}

		bool created = false;
		if(highestDegreeRegion!=null){
			created = CreateDoor(currRegion, highestDegreeRegion);
			//Debug.Log ("Creating door between Region "+currRegion.GetID().ToString()+" and Region "+highestDegreeRegion.GetID().ToString());

			//Adding in second door by chance
			if(secondHighestDegreeRegion!=null && UnityEngine.Random.Range(0.0f,1.0f)<PROBABILITY_FOR_SPAWNING_SECOND_DOOR){
				CreateDoor(currRegion, secondHighestDegreeRegion);
			}
		} else {
			Debug.Log ("Cannot connect Region "+currRegion.GetID().ToString());
		}

		return created;

	}
	
	void MarkCentrePoints(List<RegionData> listOfRegions){
		foreach (RegionData region in listOfRegions) {
			region.ComputeCentre();
			if (!map.HasIconTile(region.GetCentreBig())){
				AddIconTile(region.GetCentreBig(), IconNum.CENTRE_POINT);
			}
		}
	}

	bool IsReplacableIconTile (Vector2 bigPos) {
		if (map.GetIcon (bigPos) == "centrePt" || map.GetIcon (bigPos) == "collisionPt") {
			return true;
		} else {
			return false;
		}
	}
	bool IsValidPosToAddIconTile (Vector2 bigPos) {
		if (map.HasIconTile (bigPos)) {	
			if (IsReplacableIconTile (bigPos)) {
				//Debug.Log (map.GetIconTiles()[bigPos]+" at "+bigPos.ToString()+ " replaced with "+markType.ToString());
				map.RemoveIcon (bigPos);	
			} else {
				return false;
			}
		} 
		return true;
	}

	void AddIconTile (Vector2 bigPos, IconNum markType) {
		if (!map.HasIconTile (bigPos)) {
			switch (markType) {
			case IconNum.CENTRE_POINT:
				map.AddTile (bigPos, "centrePt", ICON);
				break;
			case IconNum.COLLISION_POINT:
				map.AddTile (bigPos, "collisionPt", ICON);
				break;
			case IconNum.RED_BASE:
				map.AddTile (bigPos, "redTeam", ICON);
				break;
			case IconNum.BLUE_BASE:
				map.AddTile (bigPos, "blueTeam", ICON);
				break;
			case IconNum.RED_FLAG:
				map.AddTile (bigPos, "redFlag", ICON);
				break;
			case IconNum.BLUE_FLAG:
				map.AddTile (bigPos, "blueFlag", ICON);
				break;
			case IconNum.NEUTRAL_FLAG:
				map.AddTile (bigPos, "neutralFlag", ICON);
				break;
			case IconNum.DOOR:
				map.AddTile (bigPos, "door", ICON);
				break;
			case IconNum.HORIZONTAL_COVER:
				map.AddTile (bigPos, "coverHorizontal", ICON);
				break;
			case IconNum.VERTICAL_COVER:
				map.AddTile (bigPos, "coverVertical", ICON);
				break;
			default:
				Debug.Log("No such icon tile. Please check IconNum for valid Icon numbers.");
				break;
			}
		} else {
			Debug.Log("There is already a "+map.GetIcon(bigPos)+" at this location. Failed to add new tile of type: " + markType.ToString());
		}
	}

	void AssignRegionsToRoles(){
		RegionData redRegion = null;
		RegionData blueRegion = null;
		listOfRedFlagRegions = new List<RegionData>();
		listOfBlueFlagRegions = new List<RegionData>();
		listOfNeutralFlagRegions = new List<RegionData>();

		AssignCollisionPoints ();
		AssignTeamBases (ref redRegion, ref blueRegion);
		AssignFlags (redRegion, blueRegion);
		CalculateTeamFlagFairnessFitness (redRegion, blueRegion);
		CalculateOverallFlagFairnessFitness (redRegion, blueRegion);
		AssignCovers ();
		CalculateCoverFitness ();
	}

	void AssignCollisionPoints(){
		int numOfCollisionPoints = 0;
		foreach (RegionData currRegion in listOfRegions) {
			if(currRegion.GetConnectivityDegree()>=NUM_FOR_COLLISION_POINT_THRESHOLD){
				Vector2 posToAddIcon = currRegion.GetCentreBig();
				if (IsValidPosToAddIconTile(posToAddIcon)) {
					AddIconTile (posToAddIcon, IconNum.COLLISION_POINT);
					currRegion.SetRole((int)IconNum.COLLISION_POINT);
					CreateCover(currRegion);
					numOfCollisionPoints++;
				}
			}
		}
		collisionPtFitness = CalculateCollisionPtFitness (numOfCollisionPoints);
	}

	void CreateCover(RegionData currRegion){
		List<Vector2> collisionptCovers = currRegion.FindAdjacentConnectedSimilarRegionsTiles ();
		foreach (Vector2 newCover in collisionptCovers) {

			if (HasConsistentTileString(newCover)) {
				Vector2 directionOfCoverFromCentre = newCover - currRegion.GetCentreBig ();
				bool isHorizontalDistanceLonger = Mathf.Abs (directionOfCoverFromCentre.x) > Mathf.Abs (directionOfCoverFromCentre.y);
				if (isHorizontalDistanceLonger) {	// is at left or right
					AddIconTile (newCover, IconNum.VERTICAL_COVER);
				} else {							// is at top or bottom
					AddIconTile (newCover, IconNum.HORIZONTAL_COVER);
				}
				map.AddCover ();
			}
		}
	}

	void AssignTeamBases(ref RegionData redRegion, ref RegionData blueRegion){
		double furthestDist = -1;
		for (int i=0; i<listOfRegions.Count-1; i++) {
			for (int j=i+1; j<listOfRegions.Count; j++) {
				RegionData currRegion = listOfRegions[i];
				RegionData otherRegion = listOfRegions[j];
				if (currRegion.HasRole() || otherRegion.HasRole())
					continue;
				
				double distanceBetweenCurrAndOtherRegion = Vector2.Distance (currRegion.GetCentre(), otherRegion.GetCentre ());
				if (furthestDist < distanceBetweenCurrAndOtherRegion) {
					furthestDist = distanceBetweenCurrAndOtherRegion;
					redRegion = currRegion;
					blueRegion = otherRegion;
				}
			}
		}
		CheckThatTeamRegionsAreAssigned (redRegion, blueRegion);

		int numOfBases = map.GetNumOfBases();
		if (numOfBases >= 1) {
			AssignATeamBase (blueRegion, IconNum.BLUE_BASE);
		} 
		if (numOfBases == 2) {
			AssignATeamBase (redRegion, IconNum.RED_BASE);
		}
	}

	void AssignATeamBase(RegionData region, IconNum role){
		bool assigned = false;
		region.SetRole ((int)role);
		assigned = AssignIconTile (region.GetCentreBig (), role);	
		if (!assigned) {
			map.SetIsBaseMissing (true);
		}
	}

	void CheckThatTeamRegionsAreAssigned (RegionData redRegion, RegionData blueRegion) {
		if (redRegion == null) {
			Debug.Log ("Unable to assign red team!");
		}
		if (blueRegion == null) {
			Debug.Log ("Unable to assign blue team!");
		}
	}

	enum Dir {Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft};	//WHAT ORDER TO SEARCH IS BEST???
	Vector2 GetNextBestPosForIconTile (Vector2 origTile) {
		Vector2 surroundingTile = new Vector2 ();
		foreach(Dir dir in Enum.GetValues(typeof(Dir))) {
			switch(dir){
				case Dir.Top: surroundingTile = new Vector2(origTile.x, origTile.y-1); break;
				case Dir.TopRight: surroundingTile = new Vector2(origTile.x+1, origTile.y-1); break;
				case Dir.Right: surroundingTile = new Vector2(origTile.x+1, origTile.y-1); break;
				case Dir.BottomRight: surroundingTile = new Vector2(origTile.x+1, origTile.y+1); break;
				case Dir.Bottom: surroundingTile = new Vector2(origTile.x, origTile.y+1); break;
				case Dir.BottomLeft: surroundingTile = new Vector2(origTile.x-1, origTile.y+1); break;
				case Dir.Left: surroundingTile = new Vector2(origTile.x-1, origTile.y); break;
				case Dir.TopLeft: surroundingTile = new Vector2(origTile.x-1, origTile.y-1); break;	
			}
			if(HasConsistentTileString(surroundingTile) && IsValidPosToAddIconTile (surroundingTile)) {
				return surroundingTile;
			}
		}
		return INVALID_POSITION;
	}

	void AssignFlags(RegionData redRegion, RegionData blueRegion){
		for (int i = 0; i<map.GetNumOfRedFlags(); i++) {
			AssignTeamFlag("Red", redRegion);
		}
		for (int i = 0; i<map.GetNumOfBlueFlags(); i++) {
			AssignTeamFlag("Blue", blueRegion);
		}
		AssignNeutralFlag(map.GetNumOfNeutralFlags(), redRegion, blueRegion);
	}

	void AssignTeamFlag(string teamColor, RegionData coloredRegion){
		RegionData nearestRegion = null;
		double nearestDistance = diagLength;
		IconNum roleNumber;
		foreach (RegionData currRegion in listOfRegions) {
			if (currRegion.HasRole ())
				continue;
			double distanceBetweenRegions = Vector2.Distance (coloredRegion.GetCentre (), currRegion.GetCentre ());
			if (nearestDistance > distanceBetweenRegions && distanceBetweenRegions > 5) {
				nearestDistance = distanceBetweenRegions;
				nearestRegion = currRegion;
			}
		}

		if (nearestRegion == null) {
			map.SetIsFlagMissing (true);
			Debug.Log ("Unable to assign team flag!");
		} else {
			if (teamColor == "Red") {
				roleNumber = IconNum.RED_FLAG;
				listOfRedFlagRegions.Add (nearestRegion);
			} else if (teamColor == "Blue") {
				roleNumber = IconNum.BLUE_FLAG;
				listOfBlueFlagRegions.Add (nearestRegion);
			} else {
				roleNumber = IconNum.UNASSIGNED;
			}
			nearestRegion.SetRole ((int)roleNumber);

			Vector2 flagPos = nearestRegion.GetCentreBig ();
			bool assigned = AssignIconTile (flagPos, roleNumber);
			if (!assigned) {
				map.SetIsFlagMissing (true);
			}
		}
	}

	bool AssignIconTile (Vector2 pos, IconNum icon) {
		if (IsValidPosToAddIconTile (pos) && HasConsistentTileString (pos)) {
			AddIconTile (pos, icon);
		} else {
			Vector2 nextBestPos = GetNextBestPosForIconTile (pos);
			if (map.IsValidPos(nextBestPos)) {
				AddIconTile (nextBestPos, icon);
			} else if (icon == IconNum.BLUE_BASE || icon == IconNum.RED_BASE) {
				AddIconTile (pos, icon);
			} else {
				return false;
			}
		}

		return true;
	}

	void AssignNeutralFlag(int numOfNeutralFlags, RegionData redRegion, RegionData blueRegion){
		List<RegionData> regionsToLeftOfCentreLine = new List<RegionData> ();
		List<RegionData> regionsToRightOfCentreLine = new List<RegionData> ();
		
		double xBlue = blueRegion.GetCentre ().x;
		double yBlue = blueRegion.GetCentre ().y;
		double xRed = redRegion.GetCentre ().x;
		double yRed = redRegion.GetCentre ().y;
		
		// determine which regions lie to the left and right of line
		foreach (RegionData currRegion in listOfRegions) {
			double xCurr = currRegion.GetCentre ().x;
			double yCurr = currRegion.GetCentre ().y;
			if (currRegion.HasRole ())
				continue;
			if ((xBlue - xRed) * (yCurr - yRed) - (yBlue - yRed) * (xCurr - xRed) < 0)	// determinant of matrix formula
				regionsToLeftOfCentreLine.Add (currRegion);
			else
				regionsToRightOfCentreLine.Add (currRegion);
		}
		
		// determine the point to any one side that is closest to the middle of both bases
		bool isRight = UnityEngine.Random.Range (0.0f, 1.0f) < 0.5;
		for (int i = 0; i<numOfNeutralFlags; i++) {
			if(isRight){
				AssignSingleNeutralFlag(redRegion, blueRegion, regionsToRightOfCentreLine);
			} else{
				AssignSingleNeutralFlag(redRegion, blueRegion, regionsToLeftOfCentreLine);
			}
			isRight = !isRight;
		}
	}

	void AssignSingleNeutralFlag(RegionData redRegion, RegionData blueRegion, List<RegionData> regionsToCheck){
		RegionData finalFlagRegion = null;
		double smallestError = double.MaxValue;
		foreach (RegionData currRegion in regionsToCheck) {
			double error = Mathf.Abs (Vector2.Distance (redRegion.GetCentre (), currRegion.GetCentre ()) -
			                          Vector2.Distance (blueRegion.GetCentre (), currRegion.GetCentre ()));
			if (smallestError > error) {
				smallestError = error;
				finalFlagRegion = currRegion;
			}
		}

		regionsToCheck.Remove (finalFlagRegion);
		listOfNeutralFlagRegions.Add (finalFlagRegion);
		finalFlagRegion.SetRole ((int)IconNum.NEUTRAL_FLAG);

		Vector2 flagPos = finalFlagRegion.GetCentreBig();
		bool assigned = AssignIconTile (flagPos, IconNum.NEUTRAL_FLAG);
		if (!assigned && regionsToCheck.Count > 0) {
			AssignSingleNeutralFlag (redRegion, blueRegion, regionsToCheck);
		} else if (!assigned) {
			map.SetIsFlagMissing(true);
			Debug.Log ("Failed to assign a neutral flag");
		}
	}

	void AssignCovers() {
		int coversLeftToAdd = CalculateCoversLeftToAdd ();
		bool coversAreMaxedOut = false;

		while (coversLeftToAdd != 0 && !coversAreMaxedOut) {
			AssignACoverToAllRegions (ref coversLeftToAdd, ref coversAreMaxedOut);
		}
	}

	int CalculateCoversLeftToAdd() {
		int mapArea = map.GetBaseTiles ().Count;
		int totalNumOfCoversNeeded = mapArea * map.GetDensity () / 100;
		return totalNumOfCoversNeeded - map.GetNumOfCovers ();
	}

	void CalculateCoverFitness(){
		int mapArea = map.GetBaseTiles ().Count;
		float finalDensity = (float)map.GetNumOfCovers ()/mapArea * 100;
		coverFitness = finalDensity / (float)map.GetDensity ();
		Debug.Log ("Final cover density: " + map.GetNumOfCovers () + "/" + mapArea + "= %" + finalDensity);
	}

	void AssignACoverToAllRegions(ref int coversLeftToAdd, ref bool coversAreMaxedOut) {
		int numOfRegions = listOfRegions.Count;
		int numOfFullRegions = 0;
		bool assigned;
		foreach (RegionData currRegion in listOfRegions) {
			if (coversLeftToAdd != 0){
				Vector2 coverPos = currRegion.GetCentreBig ();
				assigned = AssignACover (coverPos, currRegion, ref coversLeftToAdd);
				if (!assigned) {
					numOfFullRegions++;
				}
			}
		}
		if (numOfRegions == numOfFullRegions) {
			coversAreMaxedOut = true;
		}
	}

	bool AssignACover(Vector2 coverPos, RegionData currRegion, ref int coversLeftToAdd) {
		bool assigned = AssignIconTile (coverPos, GetRandomDirection());	// vertical cover
		if (assigned) {
			map.AddCover ();
			coversLeftToAdd--;
		}
		return assigned;
	}

	IconNum GetRandomDirection () {
		float start = 0.0f;
		float end = 1.0f;
		float randomVal = UnityEngine.Random.Range (start, end);
		if (randomVal < (start+end)/2.0) {
			return IconNum.HORIZONTAL_COVER;	// horizontal cover
		} else {
			return IconNum.VERTICAL_COVER;	// vertical cover
		}
	}

	bool HasConsistentTileString(Vector2 tile){
		//consistent = iiii / oooo
		string tileStr = map.GetTileString (tile);
		return (tileStr.Equals ("iiii") || tileStr.Equals ("oooo")) ? true : false;
	}

	void AssignFitnessToMap () {
		float finalFitness = 0;
		finalFitness += collisionPtFitness;
		if (!float.IsNaN(teamFlagFairnessFitness)) {
			finalFitness += teamFlagFairnessFitness;
		}
		if (!float.IsNaN(overallFlagFairnessFitness)) {
			finalFitness += overallFlagFairnessFitness;
		}
		finalFitness += coverFitness;
		finalFitness = finalFitness * CheckEssentialElements ();
		map.SetFitnessFunction (finalFitness);

		Debug.Log ("col: " + collisionPtFitness.ToString () +
			", teamFlag: " + teamFlagFairnessFitness.ToString () +
			", overallFlag: " + overallFlagFairnessFitness.ToString () +
			", cover: " + coverFitness.ToString () +
			", essential: " + CheckEssentialElements ().ToString ());
	}

	int CheckEssentialElements (){
		//bases don't tally 
		if (map.IsBaseMissing ()) {
			Debug.Log ("check essential elts: base is missing");
			return 0;
		}

		//objectives don't tally
		if (map.IsFlagMissing ()) {
			Debug.Log ("check essential elts: flag is missing");
			return 0;
		}

		//doors
		if ((int)connectivityFitness != 1) {
			Debug.Log ("check essential elts: connectivity failed");
			return 0;
		}

		return 1; 
	
	}
		 

	float CalculateCollisionPtFitness(int noOfCollisionPt){
		if (noOfCollisionPt == 0 || noOfCollisionPt > NUM_OF_COLLISION_POINTS_MAX) {
			return 0;
		} else if (noOfCollisionPt == NUM_OF_COLLISION_POINTS_OPTIMAL) {
			return 1;
		} else {
			return 0.5f;
		}
	}

	void CalculateTeamFlagFairnessFitness(RegionData redRegion, RegionData blueRegion){
		float distToRedFlag = CalculateAverageDistOfFlags(redRegion, listOfRedFlagRegions);
		float distToBlueFlag = CalculateAverageDistOfFlags(blueRegion, listOfBlueFlagRegions);
		float diff = Mathf.Abs(distToRedFlag - distToBlueFlag);
		if(diff > 30) diff = 30;
		teamFlagFairnessFitness = (30-diff)/30;
	}

	void CalculateOverallFlagFairnessFitness(RegionData redRegion, RegionData blueRegion){
		float distToRedFlag = CalculateAverageDistOfFlags(redRegion, listOfRedFlagRegions);
		float distToBlueFlag = CalculateAverageDistOfFlags(blueRegion, listOfBlueFlagRegions);
		float meanDistToFlagsRed = (1.0f/4)*(distToRedFlag + redRegion.PathDistanceTo(listOfRegions, blueRegion) + 
		                                     CalculateTotalPathDistance(listOfNeutralFlagRegions, redRegion));
		float meanDistToFlagsBlue = (1.0f/4)*(distToBlueFlag + blueRegion.PathDistanceTo(listOfRegions, redRegion) + 
		                                      CalculateTotalPathDistance(listOfNeutralFlagRegions, blueRegion));
		float diff = Mathf.Abs(meanDistToFlagsRed - meanDistToFlagsBlue);
		if (diff > 15) {
			diff = 15;
		}
		overallFlagFairnessFitness = (15-diff)/15; 
	}

	float CalculateAverageDistOfFlags(RegionData currRegion, List<RegionData> regionsToCheck){
		float finalDistance = 0;
		foreach (RegionData currFlagRegion in regionsToCheck) {
			finalDistance = finalDistance + currRegion.PathDistanceTo (listOfRegions, currFlagRegion);
		}
		finalDistance = finalDistance / regionsToCheck.Count;
		return finalDistance;
	}

	float CalculateTotalPathDistance(List<RegionData> regionsToCheck,RegionData currRegion){
		float finalDistance = 0;
		foreach (RegionData currFlagRegion in regionsToCheck) {
			finalDistance = finalDistance + currRegion.PathDistanceTo (listOfRegions, currFlagRegion);
		}
		return finalDistance;
	}

	/***********************************************/
	/* Methods Used for Testing and Unused Methods */
	/***********************************************/
	Vector2 GetNewDoorPosIfAtCorner(Vector2 bigPosOfDoor){
		Vector2 rightBigPos = new Vector2(bigPosOfDoor.x+1, bigPosOfDoor.y);
		Vector2 leftBigPos = new Vector2(bigPosOfDoor.x-1, bigPosOfDoor.y);
		Vector2 upBigPos = new Vector2(bigPosOfDoor.x, bigPosOfDoor.y-1);
		Vector2 downBigPos = new Vector2(bigPosOfDoor.x, bigPosOfDoor.y+1);
		
		// if pos is at a corner, set isChanged = false, then if it is changed then isChanged = true. 
		// Not very readable, should change this
		bool isChanged = true; 
		string tileStr = map.GetTileString(bigPosOfDoor);
		Vector2 tempBigPos;
		if (tileStr=="oooi" || tileStr=="oioo" || tileStr=="iiio" || tileStr=="ioii"){ //top left and bottom left corners
			isChanged = false;
			string newTileStr = map.GetTileString(rightBigPos); // try moving right
			tempBigPos = rightBigPos;
			if ((newTileStr=="ooii" || newTileStr=="iioo") && !map.HasIconTile(tempBigPos)){ 
				bigPosOfDoor = tempBigPos;
				isChanged = true;
			} else { 
				if (tileStr=="oooi" || tileStr=="iiio"){ // if top left corner, move down
					newTileStr = map.GetTileString(downBigPos);
					tempBigPos = downBigPos;
				} else { //if bottom left corner(oioo or ioii), move up
					newTileStr = map.GetTileString(upBigPos);
					tempBigPos = upBigPos;
				}
				if ((newTileStr=="oioi"||newTileStr=="ioio")&& !map.HasIconTile(tempBigPos)){
					bigPosOfDoor = tempBigPos;
					isChanged = true;
				}
			}
		} else if (tileStr=="ooio" || tileStr=="iooo" || tileStr=="iioi" || tileStr=="oiii"){ //top right and bottom right corners
			isChanged = false;
			string newTileStr = map.GetTileString(leftBigPos); // try moving left
			tempBigPos = leftBigPos;
			if ((newTileStr=="ooii" || newTileStr=="iioo") && !map.HasIconTile(tempBigPos)){ 
				bigPosOfDoor = tempBigPos;
				isChanged = true;
			} else { 
				if (tileStr=="ooio" || tileStr=="iioi"){ // if top right corner, move down
					newTileStr = map.GetTileString(downBigPos);
					tempBigPos = downBigPos;
				} else { //if bottom right corner(iooo or oiii), move up 
					newTileStr = map.GetTileString(upBigPos);
					tempBigPos = upBigPos;
				}
				if ((newTileStr=="oioi"||newTileStr=="ioio")&& !map.HasIconTile(tempBigPos)){
					bigPosOfDoor = tempBigPos;
					isChanged = true;
				}
			}
		}
		if (isChanged){
			return bigPosOfDoor;
		} else {
			return new Vector2(-1f,-1f); //cannot Find a new position for the door
		}
	}
	
}
