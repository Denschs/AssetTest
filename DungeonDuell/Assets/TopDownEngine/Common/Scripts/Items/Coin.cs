using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;


namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Coin manager
	/// </summary>
	[AddComponentMenu("TopDown Engine/Items/Coin")]
	public class Coin : PickableItem
	{
		/// The amount of points to add when collected
		[Tooltip("The amount of points to add when collected")]
		public int PointsToAdd = 10;

		/// <summary>
		/// Triggered when something collides with the coin
		/// </summary>
		/// <param name="collider">Other.</param>
		protected override void Pick(GameObject picker) 
		{


            TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsToAdd);

            Debug.Log("M�nze aufgenommen");
			Character character = picker.GetComponent<Character>();
			if (character != null)
			{
				Debug.Log($"Player ID: {character.PlayerID}");
				// Rufe die AddCoins-Methode im Level-Manager auf
				/*
				var levelManager = FindObjectOfType<DungeonDuellMultiplayerLevelManager>();
				if (levelManager != null)
				{
					levelManager.AddCoins(character.PlayerID, PointsToAdd);
				}
				else
				{
					Debug.LogError("DungeonDuellMultiplayerLevelManager konnte nicht gefunden werden.");
				}
				*/
			}
			else
			{
				Debug.LogError("Character konnte nicht gefunden werden.");
			}
		}
		
	}
}