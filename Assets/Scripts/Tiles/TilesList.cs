using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Tiles 
{
	[CreateAssetMenu(fileName = "TilesList", menuName = "Labyrinth/Create Tiles List", order = 1)]
	public class TilesList : ScriptableObject
	{
		public List<TileListItem> availableTiles;
	}
}
