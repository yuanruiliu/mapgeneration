using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO; 
using System.Text; 

using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization; 

/*
 * Use this class to save a single map for testing or maybe for export/import map function
 * Pre-cond: Original MapData must have been generated first.
 */
[Serializable()]
public class SaveData : ISerializable {
	//DATA VAR NAMES
	const string TILE_COMPONENTS = "tileComponents";
	const string BT_KEYS = "baseTileKeys";
	const string BT_VALUES = "baseTileValues";
	const string IT_KEYS = "iconTileKeys";
	const string IT_VALUES = "iconTileValues";
	const string LDL_POS_A = "lineDataListPosA";
	const string LDL_POS_B = "lineDataListPosB";
	const string LDL_CONNECTION_TYPES = "lineDataListConnectionTypes";
	const string REGIONS_LIST = "listOfRegions";
	const string MAP_SETTINGS = "settings";

	//Declaration of sub-types used that also need to be custom-made serializable: Vector2, RegionData
	//Vector2
	[Serializable]
	public struct SerializableVector2 {
		public float x;
		public float y;

		public SerializableVector2 (Vector2 vec) {
			x = vec.x;
			y = vec.y;
		}
		public Vector2 vec { 
			get { return new Vector2(x, y); }	 
		}
	}
	
	//RegionData
	//Serializable Region Data
	[Serializable]
	public struct SerializableRegionData {
		public int regionID;
		public char regionType;
		public int regionRole;
		public int length;
		public int height;
		public SerializableVector2 center;
		public List<SerializableVector2> minitilePos;
			
		public SerializableRegionData (RegionData region) {
			regionID = region.GetID();
			regionType = region.GetType ();
			regionRole = region.GetRole ();
			length = region.GetLength ();
			height = region.GetHeight ();
			center = new SerializableVector2(region.GetCentre ());

			minitilePos = new List<SerializableVector2> ();
			List<Vector2> minitilePosTemp = region.GetMiniTilePosList();
			foreach (Vector2 vec in minitilePosTemp) {
				minitilePos.Add (new SerializableVector2 (vec));
			}
		}

		public RegionData convertToRegionData () {
			RegionData region = new RegionData (regionType, regionID);
			region.SetRole (regionRole);
			region.SetDimensions (length, height);
			region.SetCentre (center.vec);

			List<Vector2> minitilePosTemp = new List<Vector2> ();
			foreach (SerializableVector2 pos in minitilePos) {
				minitilePosTemp.Add (pos.vec);
			}
			region.SetMinitilePos (minitilePosTemp);

			return region;
		}

		/*
		 * regionColor = region.GetColor ();
		//public Color regionColor;
		//for each List<RegionData> var, declare int xxCount and int xx(indexOf)FirstChild vars
		//List<RegionData> connectedRegions;
		public int connectedRegionsCount;
		public int connectedRegionsFirstChild;	//indexOfFirstChild
		
		//Dictionary<RegionData, float> distToConnectedRegions;
		//First do the List<RegionData>
		public int distRegionDataCount;
		public int distRegionDataFirstChild;
		//Then replace the float part with List<float>
		public List<float> distFloat;
		
		//List<RegionData> adjacentUnconnectedRegions;
		public int unconnectedCount;
		public int unconnectedFirstChild;
		*/
	}

	//MapSettings
	[Serializable]
	public struct SerializableMapSettings {
		int horDimension;
		int verDimension;
		int numOfRedFlags;
		int numOfBlueFlags;
		int numOfNeutralFlags;
		bool isSymmetric;
		int numOfBases;
		int density;

		public SerializableMapSettings (MapSettings settings) {
			horDimension = settings.GetHorDimension();
			verDimension = settings.GetVerDimension();
			numOfRedFlags = settings.GetNumOfRedFlags();
			numOfBlueFlags = settings.GetNumOfBlueFlags();
			numOfNeutralFlags = settings.GetNumOfNeutralFlags();
			isSymmetric = settings.IsSymmetric();
			numOfBases = settings.GetNumOfBases();
			density = settings.GetDensity();
		}
		
		public MapSettings ConvertToMapSettings () {
			MapSettings settings = new MapSettings();
			settings.SetHorDimension(horDimension);
			settings.SetVerDimension(verDimension);
			settings.SetNumOfRedFlags(numOfRedFlags);
			settings.SetNumOfBlueFlags(numOfBlueFlags);
			settings.SetNumOfNeutralFlags(numOfNeutralFlags);
			settings.SetSymmetry(isSymmetric);
			settings.SetNumOfBases(numOfBases);
			settings.SetDensity(density);
			return settings;
		}
	}

	//MAP DATA INFORMATION
	//naturally Serializable types
	public char[,] tileComponents;
	public float overallFitness;

	//baseTiles dictionary
	public List<SerializableVector2> baseTileKeys = new List<SerializableVector2> ();
	public List<string> baseTileValues = new List<string> ();
	
	//iconTiles dictionary
	public List<SerializableVector2> iconTileKeys = new List<SerializableVector2> ();
	public List<string> iconTileValues = new List<string> ();
	
	//lineDataList tuple
	public List<SerializableVector2> lineDataListPosA = new List<SerializableVector2> ();
	public List<SerializableVector2> lineDataListPosB = new List<SerializableVector2> ();
	public List<int> lineDataListConnectionTypes = new List<int> ();

	//Regions
	public List<SerializableRegionData> listOfRegions = new List<SerializableRegionData> ();

	//MapSettings
	public SerializableMapSettings settings;

	public SaveData(){

	}

	public SaveData(MapData map){
		SetSerializableTypes (map);

		SetDictionaries (map);

		SetTupleLists (map);

		SetRegions (map);

		SetMapSettings(map);
	}

	void SetSerializableTypes(MapData map){
		tileComponents = map.GetTileComponentArray ();
		overallFitness = map.GetFitnessFunction ();
	}

	void SetDictionaries(MapData map){
		SetDictionary (map.GetBaseTiles (), baseTileKeys, baseTileValues);
		SetDictionary (map.GetIconTiles (), iconTileKeys, iconTileValues);
	}

	void SetTupleLists (MapData map){
		SetTupleList (map.GetLineDataList (), lineDataListPosA, lineDataListPosB, lineDataListConnectionTypes);
	}

	void SetDictionary(Dictionary<Vector2, string> d, List<SerializableVector2> keys, List<string> values){
		Vector2[] dictionaryKeys = new Vector2[d.Count];
		string[] dictionaryValues = new string[d.Count];
		d.Keys.CopyTo (dictionaryKeys, 0);
		d.Values.CopyTo (dictionaryValues, 0);

		for(int i=0; i<d.Count; i++){
			SerializableVector2 keyToAdd = new SerializableVector2(dictionaryKeys[i]);
			keys.Add (keyToAdd);
			values.Add (dictionaryValues[i]);
		}
	}

	void SetTupleList(List<Tuple<Vector2, Vector2, int>> list, 
	                         List<SerializableVector2> posAs, List<SerializableVector2> posBs, List<int> types){

		for (int i=0; i<list.Count; i++) {
			Tuple<Vector2,Vector2,int> tuple = list[i];
			SerializableVector2 posAToAdd = new SerializableVector2(tuple.Item1);
			posAs.Add (posAToAdd);
			SerializableVector2 posBToAdd = new SerializableVector2(tuple.Item2);
			posBs.Add (posBToAdd);
			types.Add (tuple.Item3);
		}
	}

	void SetRegions(MapData map){
		//public List<SerializableRegionData> listOfRegions = new List<SerializableRegionData> ();
		List<RegionData> regionsList = map.GetRegions ();
		foreach (RegionData region in regionsList) {
			listOfRegions.Add (new SerializableRegionData (region));
		}
	}

	void SetMapSettings(MapData map){
		settings = new SerializableMapSettings(map.GetSettings());
	}


	//How to load: Get the values from info and assign them to the appropriate properties
	public SaveData(SerializationInfo info, StreamingContext ctxt) {  
		LoadSerializableTypes (info, ctxt);
		LoadDictionaries (info, ctxt);
		LoadTuples (info, ctxt);
		LoadRegions (info, ctxt);
		LoadMapSettings (info, ctxt);
	}

	void LoadSerializableTypes (SerializationInfo info, StreamingContext ctxt) {
		tileComponents = (char[,])info.GetValue (TILE_COMPONENTS, typeof(char[,]));
	}

	void LoadDictionaries (SerializationInfo info, StreamingContext ctxt) {
		baseTileKeys = (List<SerializableVector2>)info.GetValue (BT_KEYS,typeof(List<SerializableVector2>));
		baseTileValues = (List<string>)info.GetValue (BT_VALUES,typeof(List<string>));
		
		iconTileKeys = (List<SerializableVector2>)info.GetValue (IT_KEYS,typeof(List<SerializableVector2>));
		iconTileValues = (List<string>)info.GetValue (IT_VALUES,typeof(List<string>));
	}

	void LoadTuples (SerializationInfo info, StreamingContext ctxt) {
		lineDataListPosA = (List<SerializableVector2>)info.GetValue (LDL_POS_A,typeof(List<SerializableVector2>));
		lineDataListPosB = (List<SerializableVector2>)info.GetValue (LDL_POS_B,typeof(List<SerializableVector2>));
		lineDataListConnectionTypes = (List<int>)info.GetValue (LDL_CONNECTION_TYPES,typeof(List<int>));
	}

	void LoadRegions (SerializationInfo info, StreamingContext ctxt) {
		listOfRegions = (List<SerializableRegionData>)info.GetValue (REGIONS_LIST, typeof(List<SerializableRegionData>));
	}

	void LoadMapSettings(SerializationInfo info, StreamingContext ctxt) {
		settings = (SerializableMapSettings)info.GetValue (MAP_SETTINGS, typeof(SerializableMapSettings));
	}

	//Convert from SaveData to MapData
	public MapData ConvertToMapData () {
		//MapData map = new MapData (horDimension, verDimension);
		MapData map = new MapData(settings.ConvertToMapSettings());
		map.SetTileComponentsArray (tileComponents);

		Dictionary<Vector2,string> dictionaryToAdd = CreateDictionary (baseTileKeys, baseTileValues);
		map.SetBaseTiles (dictionaryToAdd);

		dictionaryToAdd = CreateDictionary (iconTileKeys, iconTileValues);
		map.SetIconTiles (dictionaryToAdd);

		List<Tuple<Vector2, Vector2, int>> lineDataListToAdd = CreateTupleList (lineDataListPosA, lineDataListPosB, lineDataListConnectionTypes);
		map.SetLineDataList (lineDataListToAdd);

		List<RegionData> listOfRegionsToAdd = CreateRegionsList (listOfRegions);
		map.SetListOfRegions (listOfRegionsToAdd);



		return map;
	}

	Dictionary<Vector2,string> CreateDictionary (List<SerializableVector2> keys, List<string> values) {
		Dictionary<Vector2,string> dictionary = new Dictionary<Vector2,string> ();
		for (int i=0; i<keys.Count; i++) {
			dictionary.Add (keys[i].vec, values[i]);
		}
		return dictionary;
	}

	List<Tuple<Vector2, Vector2, int>> CreateTupleList (List<SerializableVector2> posAs, 
	                                                           List<SerializableVector2> posBs, List<int> types) {
		List<Tuple<Vector2, Vector2, int>> tupleList = new List<Tuple<Vector2, Vector2, int>> ();
		for (int i=0; i<types.Count; i++) {
			Tuple<Vector2, Vector2, int> tupleToAdd = new Tuple<Vector2, Vector2, int>(posAs[i].vec, posBs[i].vec, types[i]);
			tupleList.Add (tupleToAdd);
		}
		return tupleList;
	}

	List<RegionData> CreateRegionsList (List<SerializableRegionData> listOfRegions) {
		List<RegionData> regionsList = new List<RegionData> ();
		foreach (SerializableRegionData region in listOfRegions) {
			regionsList.Add(region.convertToRegionData());
		}
		return regionsList;
	}

	//How to save/Serialization function
	public void GetObjectData (SerializationInfo info, StreamingContext ctxt) {
		SaveSerializableTypes (info, ctxt);
		SaveDictionaries (info, ctxt);
		SaveTuples (info, ctxt);
		SaveRegions (info, ctxt);
		SaveMapSettings (info, ctxt);
	}

	public void SaveSerializableTypes (SerializationInfo info, StreamingContext ctxt) {
		info.AddValue (TILE_COMPONENTS, tileComponents);
	}

	public void SaveDictionaries (SerializationInfo info, StreamingContext ctxt) { 
		info.AddValue (BT_KEYS, baseTileKeys);
		info.AddValue (BT_VALUES, baseTileValues);
		info.AddValue (IT_KEYS, iconTileKeys);
		info.AddValue (IT_VALUES, iconTileValues);
	}

	public void SaveTuples (SerializationInfo info, StreamingContext ctxt) { 
		info.AddValue (LDL_POS_A, lineDataListPosA);
		info.AddValue (LDL_POS_B, lineDataListPosB);
		info.AddValue (LDL_CONNECTION_TYPES, lineDataListConnectionTypes);
	}
	
	public void SaveRegions (SerializationInfo info, StreamingContext ctxt) {
		info.AddValue (REGIONS_LIST, listOfRegions);
	}

	public void SaveMapSettings (SerializationInfo info, StreamingContext ctxt) {
		info.AddValue (MAP_SETTINGS, settings);
	}
	
}



/** OTHER METHOD. TEMPORARILY PUT HERE **/
/**
//The following codes are used to create one serializable map 
//	For testing on one map and maybe for export function
public MapData serializableBestMap;

[SerializeField]
public struct SerializableMapData {
	//naturally Serializable types
	public int horDimension;
	public int verDimension;
	public List<RegionData> listOfRegions;	//NOTE: Make RegionData serializable
	public char[,] tileComponents; //access using MINI TILE COORDS***
	
	//baseTiles dictionary
	public List<Vector2> baseTileKeys = new List<Vector2> ();
	public List<string> baseTileValues = new List<string> ();
	
	//iconTiles dictionary
	public List<Vector2> iconTileKeys = new List<Vector2> ();
	public List<string> iconTileValues = new List<string> ();
	
	//lineDataList tuple
	public List<Vector2> lineDataListPointA = new List<Vector2> ();
	public List<Vector2> lineDataListPointB = new List<Vector2> ();
	public List<int> lineDataListConnectionType = new List<int> ();
}

public void OnBeforeSerialize() {
	serializableBestMap = null;
	saveBestToSerializableBestMap(best);
}

void saveBestToSerializableBestMap(MapData best) {
	var serializedBestMap = new SerializableMapData () {
		horDimension = best.GetHorDimension();
		verDimension = best.GetVerDimension();
		//childCount = n.children.Count,
		//indexOfFirstChild = serializedNodes.Count+1
	};
	
	serializedNodes.Add (serializedNode);
	foreach (var child in n.children)
		AddNodeToSerializedNodes (child);
}

public void OnAfterDeserialize()
{
	//Unity has just written new data into the serializedNodes field.
	//let's populate our actual runtime data with those new values.
	
	if (serializedNodes.Count > 0)
		root = ReadNodeFromSerializedNodes (0);
	else
		root = new Node ();
}

Node ReadNodeFromSerializedNodes(int index)
{
	var serializedNode = serializedNodes [index];
	var children = new List<Node> ();
	for(int i=0; i!= serializedNode.childCount; i++)
		children.Add(ReadNodeFromSerializedNodes(serializedNode.indexOfFirstChild + i));
	
	return new Node() {
		interestingValue = serializedNode.interestingValue,
		children = children
	};
}

**/

