using System.Collections.Generic;
using UnityEngine;

namespace dungeonduell
{
    public class DiscardPile : MonoBehaviour
    {
        public List<Card> discardPile = new List<Card>();

        // Methode zum Hinzuf�gen einer Karte zum Abwurfstapel
        public void AddCardToDiscardPile(Card card)
        {
            discardPile.Add(card);
            Debug.Log($"Karte {card.cardName} zum Abwurfstapel hinzugef�gt.");
        }
    }
}
