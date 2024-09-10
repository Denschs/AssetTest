using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dungeonduell
{
    public class PlayerDeck : MonoBehaviour
    {
        // Liste der verf�gbaren Karten. Mit allen ScriptableObjectCards im Editor f�llen
        public List<Card> availableCards = new List<Card>();

        public List<Card> playerDeck = new List<Card>();

        public int deckSize = 20;

        void Awake()
        {
            
            GenerateRandomDeck();
        }


        // Erstellen eines Decks mit einer festgelegten Anzahl von Karten
        void GenerateRandomDeck()
        {
           
            if (availableCards.Count == 0)
            {
                Debug.LogError("Die Liste der verf�gbaren Karten ist leer!");
                return;
            }

            playerDeck.Clear();

            // Zuf�llig Karten ausw�hlen und dem Deck hinzuf�gen 
            for (int i = 0; i < deckSize; i++)
            {
                int randomIndex = Random.Range(0, availableCards.Count); 
                playerDeck.Add(availableCards[randomIndex]); 
            }

            Debug.Log("Deck erfolgreich generiert mit " + playerDeck.Count + " Karten.");
        }
    }
}
