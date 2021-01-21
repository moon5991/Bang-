using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Удалять выбраную карту после защиты
//Бесконечное уменьшение здоровье после атаки, _isAtaked не применяется, нужно применить.

public enum GameStates { START, SherifTake, SherifTurn, SherifProtect, AnotherPlayerTake, AnotherPlayerTurn, AnotherPlayerProtect }
public class gameLogic : MonoBehaviour, IPointerClickHandler
{
    public GameStates state, tepmState;
    public GameObject activePlayer;
    public GameObject[] cards;
    private int BangCounter, playerID;
    public GameObject[] Players;
    public GameObject coverDeckDeactivate;
    public GameObject selectedCard, activeCard, atackedPlayer;
    private cardBehavior cardActivateAction;

    private void Awake()
    {
        cardActivateAction = GetComponent<cardBehavior>();
        BangCounter = 0;
        playerID = 0;
        activePlayer = Players[playerID];
        cards = GameObject.FindGameObjectsWithTag("card");
        coverDeckDeactivate.SetActive(true);
    }
    void Start()
    {
        foreach (var Player in Players)
        {
            Player.transform.Find("CoverDeactivate").gameObject.SetActive(true);
            Player.transform.Find("trigerAttak").gameObject.SetActive(false);
        }
        state = GameStates.START;
        StartCoroutine(gameInitialization());



    }


    void Update()
    {
        switch (state)
        {
            case GameStates.START:
                break;
            case GameStates.SherifTake:
                //При взятии двух карт включаем деактивирующий объект на колоду
                foreach (var Player in Players)
                {
                    Player.transform.Find("CoverDeactivate").gameObject.SetActive(true);
                    Player.transform.Find("trigerAttak").gameObject.SetActive(false);
                }
                if (activePlayer.GetComponent<playerStats>().counterCadr == 2)
                {
                    StopAllCoroutines();
                    activePlayer.GetComponent<playerStats>().counterCadr = 0;
                    coverDeckDeactivate.SetActive(true);
                    state = GameStates.SherifTurn;
                }
                else
                {
                    coverDeckDeactivate.SetActive(false);
                }

                break;
            case GameStates.SherifTurn:

                foreach (var Player in Players)
                {
                    if (Player != activePlayer)
                    {

                        Player.transform.Find("trigerAttak").gameObject.SetActive(true);

                    }
                    else
                    {
                        Player.transform.Find("CoverDeactivate").gameObject.SetActive(false);
                    }

                }


                if (selectedCard != null)
                {
                    SekectedCardActivate(selectedCard.GetComponent<CardChar>().typeCard);
                }
                else
                {
                    atackedPlayer = null;
                }
                break;

            case GameStates.SherifProtect:
                break;

            case GameStates.AnotherPlayerTake:
                foreach (var Player in Players)
                {
                    Player.transform.Find("CoverDeactivate").gameObject.SetActive(true);
                    Player.transform.Find("trigerAttak").gameObject.SetActive(false);
                }

                if (activePlayer.GetComponent<playerStats>().counterCadr == 2)
                {
                    StopAllCoroutines();
                    activePlayer.GetComponent<playerStats>().counterCadr = 0;
                    coverDeckDeactivate.SetActive(true);
                    state = GameStates.AnotherPlayerTurn;
                }
                else
                {
                    coverDeckDeactivate.SetActive(false);
                }
                break;

            case GameStates.AnotherPlayerTurn:
                foreach (var Player in Players)
                {
                    if (Player != activePlayer)
                    {

                        Player.transform.Find("trigerAttak").gameObject.SetActive(true);

                    }
                    else
                    {
                        Player.transform.Find("CoverDeactivate").gameObject.SetActive(false);
                    }

                }


                if (selectedCard != null)
                {
                    SekectedCardActivate(selectedCard.GetComponent<CardChar>().typeCard);
                }
                else
                {
                    atackedPlayer = null;
                }
                break;
                

            case GameStates.AnotherPlayerProtect:
                activePlayer.transform.Find("CoverDeactivate").gameObject.SetActive(true);
                atackedPlayer.transform.Find("CoverDeactivate").gameObject.SetActive(false);
                atackedPlayer.transform.Find("trigerAttak").gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    // Инициализация метода взятия карты у каждого игрока с передачей масива имеющихся карт
    // Смена состояния игра на ход Шерифа
    public IEnumerator gameInitialization()
    {

        yield return new WaitForSeconds(1f);

        foreach (var player in Players)
        {
            StartCoroutine(player.GetComponent<playerStats>().cardTakingDeck(cards, state));
            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(1f);
        state = GameStates.SherifTake;


    }



    public void OnPointerClick(PointerEventData eventData)
    {
        // Фиксируем клик по карте и клик выбора цели
        // При клике на колоде вызываем Корутину взятия карты

        switch (eventData.pointerCurrentRaycast.gameObject.tag)
        {

            case "card":
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<CardChar>().gamePos != positionInGame.Dropping)
                {
                    selectedCard = eventData.pointerCurrentRaycast.gameObject;
                    Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);

                }
                
                break;

            case "selectPlayer":
                if (state == GameStates.SherifTurn || state == GameStates.AnotherPlayerTurn)
                {
                    atackedPlayer = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
                }

                Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
                break;

            case "deck":
                StartCoroutine(activePlayer.GetComponent<playerStats>().cardTakingDeck(cards));
                Debug.Log("Clicked");
                break;

            default:
                selectedCard = null;
                if (state == GameStates.SherifTurn || state == GameStates.AnotherPlayerTurn)
                {
                    atackedPlayer = null;
                }

                break;
        }


    }

    public void ButtonPress()
    {
        if (state == GameStates.SherifProtect || state == GameStates.AnotherPlayerProtect)
        {
            if (true)
            {
                if (selectedCard != null  && selectedCard.GetComponent<CardChar>().typeCard == typeOfCard.Active && selectedCard.GetComponent<CardChar>().cardName == "pastBy")
                {
                    activePlayer.transform.Find("CoverDeactivate").gameObject.SetActive(false);
                    atackedPlayer.transform.Find("CoverDeactivate").gameObject.SetActive(true);
                    atackedPlayer.transform.Find("trigerAttak").gameObject.SetActive(true);                   
                    cardActivateAction.CardDroping(selectedCard);
                    cardActivateAction.CardDroping(activeCard);
                    atackedPlayer = null;
                    selectedCard = null;
                    state = tepmState;
                }
                else
                {
                    atackedPlayer.GetComponent<playerStats>()._isAttacked = true;
                    cardActivateAction.ActivateCardTarget(activeCard, atackedPlayer);                   
                    activePlayer.transform.Find("CoverDeactivate").gameObject.SetActive(false);
                    atackedPlayer.transform.Find("CoverDeactivate").gameObject.SetActive(true);
                    atackedPlayer.transform.Find("trigerAttak").gameObject.SetActive(true);
                    atackedPlayer = null;
                    selectedCard = null;
                    activeCard = null;
                    state = tepmState;
                }
            }

            
        }
        else
        {
            if (state == GameStates.AnotherPlayerTurn && playerID >= Players.Length - 1)
            {
                BangCounter = 0;
                playerID = 0;
                activePlayer = Players[playerID];
                state = GameStates.SherifTake;
            }
            else if (state==GameStates.SherifTurn || state==GameStates.AnotherPlayerTurn)
            {
                BangCounter = 0;
                state = GameStates.AnotherPlayerTake;
                playerID++;
                activePlayer = Players[playerID];
            }
        }
    }

    private void SekectedCardActivate(typeOfCard _typeOfCard)
    {
        if (_typeOfCard == typeOfCard.AttackBang && BangCounter != 0)
        {
            //Выводим ошибку об ограничении использования карты
            Debug.Log("НЕЛЬЗЯ!");
            selectedCard = null;
        }

        else if (_typeOfCard == typeOfCard.Active)
        {
            //Применяем моментальный эффект карты
            cardActivateAction.ActivateCardTarget(selectedCard, activePlayer);
            selectedCard = null;
        }

        else if (_typeOfCard == typeOfCard.Passive)
        {
            Debug.Log("Пасивная");
            selectedCard = null;
            //берем Holder пасивных карт и помещаем туда активную карту и накладываем свойство
        }

        else if (atackedPlayer != null)
        {
            if ((_typeOfCard == typeOfCard.AttackBang && BangCounter == 0)|| _typeOfCard == typeOfCard.Attack)
            {
                //применяем метод атаки
                // переносимся в этап защиты атакованого



                
                tepmState = state;
                BangCounter++;
                activeCard = selectedCard;
                selectedCard = null;
                state = GameStates.AnotherPlayerProtect;
                //cardActivateAction.ActivateCard(activeCard, atackedPlayer.transform.parent.gameObject);


                Debug.Log("Aтака");
            }

        }
        else
        {
            return;
        }
    }
}
