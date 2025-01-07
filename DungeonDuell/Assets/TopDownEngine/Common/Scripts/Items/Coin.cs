using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

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
			Debug.Log("M�nze aufgenommen");
			Character character = picker.GetComponent<Character>();
			if (character != null)
			{
				Debug.Log($"Player ID: {character.PlayerID}");
				GameManager.Instance.AddCoins(character.PlayerID, 1);
			}
			else
			{
				Debug.LogError("Character konnte nicht gefunden werden.");
			}
		}
		
	}
}