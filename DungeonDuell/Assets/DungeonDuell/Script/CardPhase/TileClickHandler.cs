using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace dungeonduell
{

    public class TileClickHandler : MonoBehaviour
    {
        public Camera cam;

        public Tilemap tilemap;

        public Card currentCard;
        public bool[] currentDoorDir = new bool[] { true, true, true, true, true, true };
        public DisplayCard displayCardUi;

        public GameObject indiactorDoor;
        public Transform indiactorDoorAnker;

        public TileBase resetTile;

        public TileBase[] setAbleTiles;

         public TileBase[] shadowSetAbleTiles;

        public List<Card> CardShelled; // Includes every Shelled Card played on board so only SheelsCard on Board are checked

        public ConnectionsCollector connectCollector;

        [Header("Player Objects")]

        public CardToHand HandPlayer1;

        public CardToHand HandPlayer2;

        public DiscardPile discardPile;

        public DiscardPile discardPile2;

        public TurnManager turnManager; // Referenz zum TurnManager

        public VirtualMouseInput[] cousors = new VirtualMouseInput[2];

        Vector3Int[] aroundHexDiffVectorEVEN = {
            new Vector3Int(-1, 1), // TopLeft
            new Vector3Int(0, 1), // TopRight

            new Vector3Int(-1, 0), // left
            new Vector3Int(1, 0), // right 

            new Vector3Int(-1, -1), // BottonLeft
            new Vector3Int(0, -1), // BottonRight 
        };

        Vector3Int[] aroundHexDiffVectorODD = {
            new Vector3Int(0, 1), // TopLeft
            new Vector3Int(1, 1), // TopRight

            new Vector3Int(-1, 0), // left
            new Vector3Int(1, 0), // right 

            new Vector3Int(0, -1), // BottonLeft
            new Vector3Int(1, -1), // BottonRight 
        };


        private void Start()
        {
            connectCollector = FindObjectOfType<ConnectionsCollector>();
            tilemap = FindObjectOfType<Tilemap>();
            turnManager = FindObjectOfType<TurnManager>(); // Finde den TurnManager         CardShelled = FindObjectOfType<SpawnWorldTiles>().WorldCard.ToList(); // Not ideal for cohesion but fine for now
            turnManager = FindObjectOfType<TurnManager>(); // Finde den TurnManager

            foreach(VirtualMouseInput go in FindObjectsOfType<VirtualMouseInput>())
            {
                if(go.tag == "Player1")
                {
                    cousors[0] = go;
                }
                else
                {
                    cousors[1] = go;
                }
            }
        }

        void Update()
        {
            if (currentCard != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cam.transform.position.z));
                    SpawnTile(mouseWorldPos, currentCard, true, true, turnManager.isPlayer1Turn ? 1 : 2);
                }
                if (Input.GetKeyDown(KeyCode.R)) // Test
                {
                    currentDoorDir = ShiftRight(currentDoorDir);
                    displayCardUi?.UpdateDirectionIndicator(currentDoorDir); // this might be better be resolved with an event later
                }
            }
        }
        public void CursourInput()
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint((new Vector3(cousors[turnManager.isPlayer1Turn ? 0 : 1].virtualMouse.position.x.value, cousors[turnManager.isPlayer1Turn ? 0 : 1].virtualMouse.position.y.value, -cam.transform.position.z)));
            print("PressedInput:" + mouseWorldPos);
            if (currentCard != null)
            {
                SpawnTile(mouseWorldPos, currentCard, true, true,turnManager.isPlayer1Turn ? 1 : 2);
            }

        }

        public void SpawnTile(Vector3 mouseWorldPos, Card card, bool PlayerMove,bool spawnSourroundSetables,int owner)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, cam.transform.position.z));

            TileBase clickedTile = tilemap.GetTile(cellPosition);

            if (card.SheelCard)
            {
                if (card.Tile is AnimatedTile) // for Some reason Animated Tile need to be set on a free Tile to Work ;
                {
                    tilemap.SetTile(cellPosition, null);
                }
                tilemap.SetTile(cellPosition, card.Tile);
                return; // Rest is Handled when the Tile is clicked by the player , so no further action needed
            }

            if(clickedTile != resetTile | !PlayerMove)
            {


               if ((setAbleTiles.Contains(clickedTile)  && currentCard != null) | !PlayerMove)
               {

                    Card shelledTileCard = CardShelled.FirstOrDefault(x => x.Tile == clickedTile);

                    if (shelledTileCard != null)
                    {
                        tilemap.SetTile(cellPosition, setAbleTiles[owner - 1]); // ! Not Ideal Solotions, make later System that checks sourround Tiles or make Contested Version of this tile
                        clickedTile = setAbleTiles[owner - 1];

                        shelledTileCard.startDoorConcellation = card.startDoorConcellation; // giving it direction of clicked card, other elements are preset of sheel card
                        // CardUsingHandling(shelledTileCard, PlayerMove, spawnSourroundSetables, cellPosition, clickedTile, owner);
                    }                 
                    CardUsingHandling(card, PlayerMove, spawnSourroundSetables, cellPosition,clickedTile,owner);

               }
                else
                {
                    Debug.Log("Denied_SetOrNoCard");
                }
            }
            else
            {
                Debug.Log("OutOfReachTile");
            }

        }

        private void CardUsingHandling(Card card, bool PlayerMove, bool spawnSourroundSetables, Vector3Int cellPosition, TileBase clickedTile,int owner)
        {

        

            bool[] OverriteCurrentDoorDir =  new bool[] { false, false, false, false, false, false };
            bool connectionForcing = false;
                                if(clickedTile == setAbleTiles[setAbleTiles.Length - 1]) // Hited Contested
                                {
                                    print("7777");
                                    connectionForcing = true;
                                    Vector3Int[] offset = (cellPosition.y % 2 == 0) ? aroundHexDiffVectorEVEN : aroundHexDiffVectorODD;
                                    for (int i = 0; i < offset.Length; i++)
                                    {
                                        if (connectCollector.GetFullRoomList().Any(entry => entry.Item1 == cellPosition + offset[i]))
                                        {
                                            print("777788");
                                            currentDoorDir[i] = true; // Connect to all rooms that are there
                                            OverriteCurrentDoorDir[i] = true;
                                        }
                                    }
                                }




            Tuple<Vector3Int, ConnectionDir>[] sourroundCorr = GetSouroundCorr(cellPosition, currentDoorDir);

            if (CheckConnectAblity(sourroundCorr) | !PlayerMove)
            {
                Debug.Log("Tile clicked at position: " + cellPosition);
                // Set Tiles
                // Main Spawn

                tilemap.SetTile(cellPosition, card.Tile);

                        //Sourround
                        if (spawnSourroundSetables)
                        {
                            foreach (Tuple<Vector3Int, ConnectionDir> SourrendTilePos in GetSouroundCorr(cellPosition, new bool[] { true, true, true, true, true, true }))
                            {

                                TileBase souroundTile = tilemap.GetTile(SourrendTilePos.Item1);

                                if(setAbleTiles.Contains(souroundTile))
                                {
                                    if (clickedTile != souroundTile)
                                    {
                                        tilemap.SetTile(SourrendTilePos.Item1, setAbleTiles[setAbleTiles.Length - 1]);
                                    }

                                }
                                else if(shadowSetAbleTiles.Contains(souroundTile))
                                {
                                    int i = Array.FindIndex(setAbleTiles, entity => entity == clickedTile);
                                    if(i == setAbleTiles.Length -1) // Aka. Hitted Contest all Souround no Have to Contested Also
                                    {
                                        tilemap.SetTile(SourrendTilePos.Item1, setAbleTiles[setAbleTiles.Length - 1]);
                                    }
                                    else if (souroundTile != shadowSetAbleTiles[Array.FindIndex(setAbleTiles, entity => entity == clickedTile)])
                                    {
                                        tilemap.SetTile(SourrendTilePos.Item1, setAbleTiles[setAbleTiles.Length - 1]);
                                    }

                                }

                                if(souroundTile == resetTile & PlayerMove)
                                {
                                    int i = Array.FindIndex(setAbleTiles, entity => entity == clickedTile);
                                    if(i < shadowSetAbleTiles.Length)
                                    {
                                        tilemap.SetTile(SourrendTilePos.Item1,shadowSetAbleTiles[Array.FindIndex(setAbleTiles, entity => entity == clickedTile)]);
                                    }
                                }

                            }
                            foreach (Tuple<Vector3Int, ConnectionDir> SourrendTilePos in GetSouroundCorr(cellPosition, currentDoorDir))
                            {
                                TileBase souroundTile = tilemap.GetTile(SourrendTilePos.Item1);

                                if(souroundTile == resetTile | shadowSetAbleTiles.Contains(souroundTile))
                                {
                                    if(setAbleTiles.Contains(clickedTile))
                                    {

                                        tilemap.SetTile(SourrendTilePos.Item1, clickedTile);
                                    }
                                    else
                                    {
                                        tilemap.SetTile(SourrendTilePos.Item1,  setAbleTiles[owner - 1]);
                                    }
                                }
                            }
                        }



                // Create Room Info
                CreateRoom(cellPosition, card.roomtype, card.roomElement, currentDoorDir, owner,connectionForcing);

                        // Card Disposal
                        if (PlayerMove)
                        {
                            // Karte zum Abwurfstapel hinzufügen und vom CardHolder entfernen    
                            discardPile.AddCardToDiscardPile(card);
                            RemoveCardFromCardHolder(turnManager.isPlayer1Turn);
                            RemoveCardFromCardHolder(!turnManager.isPlayer1Turn);
                            turnManager.EndPlayerTurn(); // Übergib die Verantwortung an den TurnManager
                            currentCard = null;

                }
                GameObject indicator = Instantiate(indiactorDoor, tilemap.CellToWorld(cellPosition), Quaternion.identity);
                if (indiactorDoorAnker == null)
                {
                    indiactorDoorAnker = GameObject.Find("IndicatorsAnker").transform; // TODO HotFix ; Make better later

                }
                indicator.transform.parent = indiactorDoorAnker;
                indicator.GetComponent<DoorIndicator>().SetDoorIndiactor(currentDoorDir);
                if(connectionForcing)
                    {
                        indicator.GetComponent<DoorIndicator>().OverExtend(OverriteCurrentDoorDir);
                    }


            }
            else
            {
                Debug.Log("Denied_NotRightRoation");
            }
        }

        private bool CheckConnectAblity(Tuple<Vector3Int, ConnectionDir>[] sourroundCorr)
        {
            // sourroundCorr Being an Tuple might overcomplicated , but tried solutation had edge cases where they failed

            // Reduce to relvant element so fewer opertion with find later 
            List<Tuple<Vector3Int, RoomInfo>> filteredList = connectCollector.GetFullRoomList().Where(item => sourroundCorr.Any(tuple => tuple.Item1 == item.Item1)).ToList();

            if(filteredList.Count <= 0)
            {
                return false;
            }

            foreach (Tuple<Vector3Int, ConnectionDir> InfoSourround in sourroundCorr)
            {
                Tuple<Vector3Int, RoomInfo> room = filteredList.Find(tuple => tuple.Item1 == InfoSourround.Item1);
                if(room != null)
                {
                    if (room.Item2.allowedDoors.Contains(InfoSourround.Item2.GetInvert()))
                    {
                        return true;
                    }
                }        
            }   
            return false;
        }

        private void RemoveCardFromCardHolder(bool player1)
        {
            Transform cardHolder = ((player1) ? HandPlayer1.transform.GetChild(0) : HandPlayer2.transform.GetChild(0));

            if (cardHolder.transform.childCount > 0)
            {
                if (displayCardUi != null)
                {
                    Card cardToDiscard = displayCardUi.card;
                    discardPile.AddCardToDiscardPile(cardToDiscard);

                    // Karte vom CardHolder entfernen
                    Destroy(displayCardUi.gameObject);
                }
            }
        }

        private void CreateRoom(Vector3Int clickedTile, RoomType type, RoomElement element, bool[] allowedDoors,int owner,bool forceOnRoom)
        {
            Vector3Int[] aroundpos = GetSouroundCorr(clickedTile); // 

            int[] establishConnection = connectCollector.GetPossibleConnects(aroundpos, allowedDoors,forceOnRoom);

            List<RoomConnection> Conncection = new List<RoomConnection>();
            List<ConnectionDir> newConnectionDir = new List<ConnectionDir>();

            for (int i = 0; i < establishConnection.Length; i++)
            {
                if (establishConnection[i] != -1) //All used
                {
                    Conncection.Add(new RoomConnection(establishConnection[i], (ConnectionDir)i));
                }
                if (allowedDoors[i]) // all possible 
                {
                    newConnectionDir.Add((ConnectionDir)i);
                    //print(((ConnectionDir)i).ToString());
                }
            }
            connectCollector.AddRoom(clickedTile, Conncection, type, element, newConnectionDir, owner);
        }

        private Vector3Int[] GetSouroundCorr(Vector3Int clickedTile)
        {
            Vector3Int[] aroundpos = new Vector3Int[6];

            Vector3Int[] offsets = GetOffsetsCorrd(ref clickedTile);

            for (int i = 0; i < offsets.Length; i++)
            {
                aroundpos[i] = clickedTile + offsets[i];
            }

            return aroundpos;
        }

        private Vector3Int[] GetOffsetsCorrd(ref Vector3Int clickedTile)
        {
            return (clickedTile.y % 2 == 0) ? aroundHexDiffVectorEVEN : aroundHexDiffVectorODD;
        }

        private Tuple<Vector3Int,ConnectionDir>[] GetSouroundCorr(Vector3Int clickedTile,bool[] setDirections)
        {
            List<Tuple<Vector3Int, ConnectionDir>> aroundpos = new List<Tuple<Vector3Int, ConnectionDir>>();

            var offsets = GetOffsetsCorrd(ref clickedTile);

            for (int i = 0; i < offsets.Length; i++)
            {
                if (setDirections[i])
                {
                    aroundpos.Add(new Tuple<Vector3Int, ConnectionDir>(clickedTile + offsets[i], (ConnectionDir)i));
                }
                
            }

            return aroundpos.ToArray();
        }



        public void ChangeCard(Card newCard, bool[] newcurrentDoorDir, DisplayCard newcurrentCardUi)
        {
            currentCard = newCard;
            currentDoorDir = newcurrentDoorDir;
            displayCardUi = newcurrentCardUi;
        }

        public bool[] ShiftRight(bool[] array)
        {
            bool[] coveredClockwiese = { array[1], array[3], array[5], array[4], array[2], array[0] };

            // Create a new array with the same size
            bool[] shiftedArray = new bool[coveredClockwiese.Length];

            // Shift the elements to the right
            for (int i = 0; i < (coveredClockwiese.Length - 1); i++)
            {
                shiftedArray[i + 1] = coveredClockwiese[i];
            }

            // Move the last element to the first position
            shiftedArray[0] = coveredClockwiese[coveredClockwiese.Length - 1];

            shiftedArray = new bool[] { shiftedArray[5], shiftedArray[0], shiftedArray[4], shiftedArray[1], shiftedArray[3], shiftedArray[2] };

            return shiftedArray;
        }

        public void ShiftRightInput(InputAction.CallbackContext context)
        {
            if (context.started == true)
            {
                print("PressedShift");
                currentDoorDir = ShiftRight(currentDoorDir);
                displayCardUi?.UpdateDirectionIndicator(currentDoorDir); // this might be better be resolved with an event later
            }

        }


        public void AddShellCardTypeToCheck(Card card)
        {
            CardShelled.Add(card);
        }
    }
}
