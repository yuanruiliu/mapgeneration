using UnityEngine;
using System.Collections;

public class MapSettings{
	int horDimension;
	int verDimension;
	int numOfRedFlags;
	int numOfBlueFlags;
	int numOfNeutralFlags;
	bool isSymmetric;
	int numOfBases;
	int density;

	int size;

	public MapSettings(){ // Creates according to default values
		this.horDimension = 15;
		this.verDimension = 15;
		this.numOfRedFlags = 1;
		this.numOfBlueFlags = 1;
		this.numOfNeutralFlags = 3;
		this.isSymmetric = false;
		this.numOfBases = 2;
		this.density = 10;
		this.size = this.horDimension*this.verDimension;
	}

	public void SetHorDimension(int horDim){
		this.horDimension = horDim;
		this.size = this.horDimension*this.verDimension;
	}
	public void SetVerDimension(int verDim){
		this.verDimension = verDim;
		this.size = this.horDimension*this.verDimension;
	}
	public void SetNumOfRedFlags(int numRedFlags){
		this.numOfRedFlags = numRedFlags;
	}
	public void SetNumOfBlueFlags(int numBlueFlags){
		this.numOfBlueFlags = numBlueFlags;
	}
	public void SetNumOfNeutralFlags(int numNeutralFlags){
		this.numOfNeutralFlags = numNeutralFlags;
	}
	public void SetSymmetry(bool isSym){
		this.isSymmetric = isSym;
	}
	public void SetNumOfBases(int numBases){
		this.numOfBases = numBases;
	}
	public void SetDensity (int density) {
		this.density = density;
	}

	public int GetHorDimension(){
		return horDimension;
	}
	public int GetVerDimension(){
		return verDimension;
	}
	public int GetNumOfRedFlags(){
		return numOfRedFlags;
	}
	public int GetNumOfBlueFlags(){
		return numOfBlueFlags;
	}
	public int GetNumOfNeutralFlags(){
		return numOfNeutralFlags;
	}
	public bool IsSymmetric(){
		return isSymmetric;
	}
	public int GetNumOfBases(){
		return numOfBases;
	}
	public int GetDensity () {
		return density;
	}
	public int GetMaxNumOfObjectives(){
		if (size==900){
			return 10;
		} else if (size>=784){
			return 9;
		} else if (size>=676){
			return 8;
		} else if (size>=576){
			return 7;
		} else if (size>=484){
			return 6;
		} else if (size>=400){
			return 5;
		} else if (size>=324){
			return 4;
		} else if (size>=225){
			return 3;
		} else {
			Debug.Log ("No defined maximum number of objectives. Invalid map size.");
			return -1;
		}
	}

}
