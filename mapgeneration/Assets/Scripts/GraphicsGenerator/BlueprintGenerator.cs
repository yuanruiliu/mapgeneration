using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BlueprintGenerator : MonoBehaviour {

	//each cell size = 32 x 32
	public ContentSizeFitter bases;
	public ContentSizeFitter icons;
	public ContentSizeFitter icons2;
	//each cell size = 16 x 16, so total count = times 4 of bases/icons
	public ContentSizeFitter highlights;
	public Image tilesetPrefab;
	public ScaleImage content;

	private Dictionary <string, Sprite> sprites;
	private Dictionary<Vector2, string> baseTiles;
	private Dictionary<Vector2, string> iconTiles;
	private Dictionary<Vector2, string> iconTiles2;
	private Dictionary<Vector2, Color> highlightTiles;

	private int columns = 0;
	private int rows = 0;
	private const float TILE_SPRITE_SIZE = 32.0f;
	
	// Use this for initialization
	void Awake () {
		InitialiseSpriteDictionary ();
	}

	//called by Map Manager to display blueprint map
	public void DisplayBlueprintMap(MapData map)
	{
		InitialiseGridLayout (map);
		InitialiseTiles(map);
		ComputeWhereToColour(map);
		DrawTilesToScene();
		RedrawContentSize();
	}

	void InitialiseTiles(MapData map)
	{
		baseTiles = map.GetBaseTiles();
		iconTiles = map.GetIconTiles();
		iconTiles2 = map.GetIconTiles2();
		highlightTiles = new Dictionary<Vector2, Color>();
	}

	void ComputeWhereToColour(MapData map)
	{
		List<RegionData> allRegionsInMap = map.GetRegions();

		foreach (RegionData region in allRegionsInMap) {
			foreach (Vector2 position in region.GetMiniTilePosList()) {
				highlightTiles.Add(position, region.GetColor());
			}
		}
		//Debug.Log("Supposed to highlight: " + highlightTiles.Count + " tiles.");
	}

	void DrawTilesToScene()
	{
		DrawBaseAndIconTiles();
		DrawHighlightTiles();
	}

	void DrawHighlightTiles()
	{
		for (int j = 0; j < rows*2 ; j++)
		{
			for (int i = 0; i < columns*2 ; i++)
			{	
				//use transparent color as default
				Color paintBrush = Color.clear;
				//if there's supposed to be a highlight, then change current paintbrush to intended colour
				if (highlightTiles.ContainsKey(new Vector2(i,j))) {
					paintBrush = highlightTiles[new Vector2(i,j)];
				}
				//color it with paintbrush
				CreateColoredTile(paintBrush);
			}
		}
	}

	void CreateColoredTile(Color c)
	{
		Image tile = Instantiate ( tilesetPrefab , highlights.transform.position, Quaternion.identity) as Image;
		tile.transform.SetParent(highlights.transform);
		tile.rectTransform.localScale = Vector3.one;
		tile.sprite = sprites["highlight"];
		tile.color = c;
	}

	void DrawBaseAndIconTiles()
	{	
		for (int j = 0; j < rows ; j++)
		{
			for (int i = 0; i < columns ; i++)
			{
				Vector2 currentPos = new Vector2(i, j);

				//base
				if (baseTiles.ContainsKey(currentPos))
				{
					string spriteName = baseTiles[currentPos];
					CreateTile(spriteName, bases);
				} else 
				{
					Debug.Log ("Base Tile missing");
				}

				//icons
				if (iconTiles.ContainsKey(currentPos))
				{
					string spriteName = iconTiles[currentPos];
					CreateTile(spriteName, icons);
				} else 
				{
					//default tile is actually a transparent tile
					CreateTile ("default", icons);
				}

				//if there are more icons from icon2
				if (iconTiles2.Count > 0 && iconTiles2.ContainsKey(currentPos)) 
				{
					string spriteName = iconTiles2[currentPos];
					CreateTile(spriteName, icons2);
				} else
				{
					//default tile is actually a transparent tile
					CreateTile ("default", icons2);
				}
			}
		}
	}

	Image CreateTile(string spriteName, ContentSizeFitter content)
	{
		Image tile = Instantiate ( tilesetPrefab , content.transform.position, Quaternion.identity) as Image;
		tile.transform.SetParent(content.transform);
		tile.rectTransform.localScale = Vector3.one;

		if (sprites.ContainsKey(spriteName))
		{
			tile.sprite = sprites[spriteName];
		} else
		{
			Debug.Log ("Invalid sprite name: " + spriteName);
		}

		return tile;
	}

	void RedrawContentSize() 
	{
		RectTransform contentRectTransform = (RectTransform) content.gameObject.transform;
		contentRectTransform.sizeDelta = new Vector2(columns * TILE_SPRITE_SIZE, rows * TILE_SPRITE_SIZE);

		if (columns > 24 || rows > 24) 
		{
			content.SetMinScrollValue(0.5f);
		} else if (columns > 19 || rows > 19)
		{
			content.SetMinScrollValue(0.8f);
		}
	}

	void InitialiseGridLayout (MapData map)
	{
		columns = map.GetHorDimension();
		rows = map.GetVerDimension();

		GridLayoutGroup glgBases = bases.GetComponent<GridLayoutGroup> ();
		glgBases.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		glgBases.constraintCount = columns;
		RemoveCurrentChildrenIfAny(glgBases);

		GridLayoutGroup glgHighlights = highlights.GetComponent<GridLayoutGroup> ();
		glgHighlights.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		glgHighlights.constraintCount = columns * 2;
		RemoveCurrentChildrenIfAny(glgHighlights);

		GridLayoutGroup glgIcons = icons.GetComponent<GridLayoutGroup> ();
		glgIcons.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		glgIcons.constraintCount = columns;
		RemoveCurrentChildrenIfAny(glgIcons);

		GridLayoutGroup glgIcons2 = icons2.GetComponent<GridLayoutGroup> ();
		glgIcons2.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		glgIcons2.constraintCount = columns;
		RemoveCurrentChildrenIfAny(glgIcons2);
	}

	void RemoveCurrentChildrenIfAny(GridLayoutGroup parent)
	{
		foreach (Transform child in parent.transform)
		{
			Destroy(child.gameObject);
		}
	}

	void InitialiseSpriteDictionary ()
	{
		sprites = new Dictionary<string, Sprite>();
		Sprite [] spritesArray = Resources.LoadAll<Sprite> ("Sprites");

		foreach (Sprite spr in spritesArray)
		{
			sprites.Add (spr.name, spr);
		}
		Debug.Log ("Loaded : " + sprites.Count + " types of tile sprites in memory.");
	}
}
