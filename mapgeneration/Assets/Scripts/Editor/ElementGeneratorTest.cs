//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	[TestFixture]
	public class ElementGeneratorTest
	{
		/* NOTE: DEFAULT MAP SETTINGS ARE AS FOLLOWS
			this.horDimension = 15;
			this.verDimension = 15;
			this.numOfRedFlags = 1;
			this.numOfBlueFlags = 1;
			this.numOfNeutralFlags = 3;
			this.isSymmetric = false;
			this.numOfBases = 2;
		*/

		const int NUM_OF_LOOPS = 20;

		[Test, Combinatorial]
		public void NumOfTiles ([Values(15,22,30)]int x, [Values(15,22,30)]int y) 
		{
			ElementGenerator eg = new ElementGenerator();
			MapSettings settings = new MapSettings();
			settings.SetHorDimension(x);
			settings.SetVerDimension(y);
	
			// To ensure that the test is not passed by chance, since results 
			// are random, a for loop is used.
			for (int i=0; i<NUM_OF_LOOPS; i++){ 
				MapData map = eg.CreateNewMap(settings);
				Assert.AreEqual(map.GetBaseTiles().Count, x*y);
			}
		}

		[Test, Combinatorial]
		public void NumOfBases ([Values(0,1,2)]int numOfBasesInput, [Values(15,30)]int size) 
		{
			ElementGenerator eg = new ElementGenerator();
			MapSettings settings = new MapSettings();
			settings.SetHorDimension(size);
			settings.SetVerDimension(size);
			settings.SetNumOfBases(numOfBasesInput);
			
			// To ensure that the test is not passed by chance, since results 
			// are random, a for loop is used.
			for (int i=0; i<NUM_OF_LOOPS; i++){ 
				MapData map = eg.CreateNewMap(settings);				
				Dictionary<Vector2, string> iconTiles = map.GetIconTiles();
				int red = 0;
				int blue = 0;

				foreach(string icon in iconTiles.Values){
					if (icon=="redTeam"){
						red++;
					} else if (icon=="blueTeam"){
						blue++;
					}
				}

				if (numOfBasesInput==2){
					Assert.That(red==1 && blue==1);
				} else if (numOfBasesInput==1){
					Assert.That(red==0 && blue==1);
				} else { //numOfBasesInput==0
					Assert.That(red==0 && blue==0);
				}				
			}
		}

		[Test, Combinatorial]
		public void NumOfFlags ([Values(0,5)]int redFlags, [Values(0,5)]int blueFlags, 
		                        [Values(0,5)]int neutralFlags, [Values(15,30)]int size) 
		{
			ElementGenerator eg = new ElementGenerator();
			MapSettings settings = new MapSettings();
			settings.SetHorDimension(size);
			settings.SetVerDimension(size);
			settings.SetNumOfBlueFlags(blueFlags);
			settings.SetNumOfRedFlags(redFlags);
			settings.SetNumOfNeutralFlags(neutralFlags);
			int maxNumOfObjectives = settings.GetMaxNumOfObjectives();
			
			// To ensure that the test is not passed by chance, since results 
			// are random, a for loop is used.
			for (int i=0; i<NUM_OF_LOOPS; i++){ 
				MapData map = eg.CreateNewMap(settings);				
				Dictionary<Vector2, string> iconTiles = map.GetIconTiles();
				int red = 0;
				int blue = 0;
				int neutral = 0;
				
				foreach(string icon in iconTiles.Values){
					if (icon=="redFlag"){
						red++;
					} else if (icon=="blueFlag"){
						blue++;
					} else if (icon=="neutralFlag"){
						neutral++;
					}
				}

				Assert.AreEqual(red, redFlags);
				Assert.AreEqual(blue, blueFlags);
				Assert.AreEqual(neutral, neutralFlags);
				
			}
		}

		[Test, Combinatorial]
		public void CoverDensity ([Values(5,10,15)]int density, [Values(15,30)]int size) 
		{
			ElementGenerator eg = new ElementGenerator();
			MapSettings settings = new MapSettings();
			settings.SetHorDimension(size);
			settings.SetVerDimension(size);
			settings.SetDensity(density);
			
			// To ensure that the test is not passed by chance, since results 
			// are random, a for loop is used.
			for (int i=0; i<NUM_OF_LOOPS; i++){ 
				MapData map = eg.CreateNewMap(settings);				
				Dictionary<Vector2, string> iconTiles = map.GetIconTiles();
				int numOfCovers = 0;
				
				foreach(string icon in iconTiles.Values){
					if (icon=="coverVertical" || icon=="coverHorizontal"){
						numOfCovers++;
					} 
				}

				float mapArea = map.GetHorDimension()*map.GetVerDimension();
				float finalDensity = (float)numOfCovers/mapArea * 100f;

				Assert.That((density-finalDensity)<=0.5);
				
			}
		}
	}
}

