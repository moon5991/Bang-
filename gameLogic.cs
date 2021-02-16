using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

enum GameStates { START, PlayerTakeCard, PlayerTurn }

public class GameLogic : MonoBehaviour, IPointerClickHandler
{
    GameStates state;
    GameObject[] cards, players;
    [SerializeField]
    GameObject cardHolder, droppingPlace, InGamePlace;
    [SerializeField]
    GameObject activePlayer, activeCard, targetPlayer, defensiveCard, tempTarget;
    bool activeTurn;
    int activeID, bangCount;
    List<GameObject> cardPool = new List<GameObject>();
    int ActiveID
    {
        get { return activeID; }
        set
        {
            if (value > players.Length - 1)
            {
                activeID = 0;
            }
            else activeID = value;
        }
    }


    void Start()
    {
        bangCount = 0;        
        activeTurn = false;
        state = GameStates.START;
        cards = GameObject.FindGameObjectsWithTag("card");
        players = GameObject.FindGameObjectsWithTag("player");
        ActiveID = 0;
        activePlayer = players[ActiveID];
        StartCoroutine(GameInitialization());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            ClearCardAndTarget();
            bangCount = 0;
            activeTurn = false;
            ActivateDeck(activePlayer);
            Debug.Log("Вызов!");
        }


    }

    // Переносим карты из колоды в руку каждого игрока в количестве равной количеству жизней
    IEnumerator GameInitialization()
    {
        yield return new WaitForSeconds(1f);

        foreach (var player in players)
        {
            PlayerStats stat = player.GetComponent<PlayerStats>();

            while (stat.cardAmount < stat.health)
            {
                
                CardParametrs card = cards[Random.Range(0, cards.Length)].GetComponent<CardParametrs>();
                if (card.positionInGame == PositionInGame.Deck) //Проверяем позицию карты, если в колоде переносим карту в руку и меняем ее состояние на в "В руке"
                {
                    Transform newHolder = Instantiate(cardHolder, stat.deck).transform;
                    card.positionInGame = PositionInGame.InHand;
                    stat.cardAmount++;
                    StartCoroutine(card.CardMoving(newHolder));
                    yield return new WaitForSeconds(0.5f);

                }
                else
                {
                    yield return null;
                }
                    
            }

        }

        state = GameStates.PlayerTakeCard;
        yield break;
    }

    // Берем карты из колоды в фазе набора в количестве 2
    IEnumerator CardTaking()
    {
        PlayerStats stat = activePlayer.GetComponent<PlayerStats>();
        int i = 0;

        while (i < 2)
        {

            CardParametrs card = cards[Random.Range(0, cards.Length)].GetComponent<CardParametrs>();
            if (card.positionInGame == PositionInGame.Deck)
            {
                i++;
                Transform newHolder = Instantiate(cardHolder, stat.deck).transform;
                card.positionInGame = PositionInGame.InHand;
                stat.cardAmount++;
                StartCoroutine(card.CardMoving(newHolder));
                yield return new WaitForSeconds(0.5f);

            }
            else
            {
                yield return null;
            }

        }
        
        yield break;
       
    }

    // Берем карту из колоды. Передаем сюда масть карты и цифру для проверки в карте из колоды, времено храним эту карту, чтобы потом сбросить
    bool CardChecking( SuitCard suitCard, int[] numbers, out GameObject CheckCard)
    {
        CheckCard = null;
        foreach (var card in cards)
        {
                CheckCard = card;
               CardParametrs cardParam = card.GetComponent<CardParametrs>();
            if (cardParam.positionInGame == PositionInGame.Deck)
            {

                StartCoroutine(InGamePlacing(card, 0.5f, InGamePlace,false));
               
                if (suitCard == cardParam.suitCard && numbers.Contains(cardParam.number))
                {
                    return true;                    
                }
                else return false;
            }
            else
            {
                continue;
            }
           
        }
        
        return false;
    }

    // Проверяем наличие защиты бочкой у игрока, а также смотрим если бочку вообще можно применить в качестве защиты.
    bool CardChekingBarrel(out int[] digits)
    {
        string text = "";
        digits = new int[9];
        for (int i = 1; i < 10; i++)
        {
            digits[i-1] = i;
            text = text + "  " + digits[i - 1];            
        }
        Debug.Log(text);
         string name = activeCard.GetComponent<CardParametrs>().nameOfCard;
        if (name != "Duel" && name != "Reds" && targetPlayer.GetComponent<PlayerStats>().Barrel == true)
        {
            return true;
        }
        else return false;
    }

    // Сбрасываем карту в сброс
    IEnumerator DropingCard(GameObject card, float timer)
    {
        yield return new WaitForSeconds(timer);
        
        GameObject oldHolder = card.transform.parent.gameObject;
        while (oldHolder==gameObject)
        {
            oldHolder = card.transform.parent.gameObject;
            yield return null;
        }
        card.transform.SetParent(transform);
        Destroy(oldHolder);
        CardParametrs cardParam = card.GetComponent<CardParametrs>();
        cardParam.positionInGame = PositionInGame.Dropping;
        StartCoroutine(cardParam.CardMoving(droppingPlace.transform));
    }

    // Помещаем карту в указаное игровое поле, также есть возможность запретить уничтожение родителя карты
    IEnumerator InGamePlacing(GameObject card, float timer, GameObject inGamePlace,bool destroy = true)
    {
        yield return new WaitForSeconds(timer);
        GameObject oldHolder = card.transform.parent.gameObject;
        while (oldHolder == gameObject)
        {
            oldHolder = card.transform.parent.gameObject;
            yield return null;
        }
        card.transform.SetParent(transform);
        if (destroy)
        {
            Destroy(oldHolder);
        }
        CardParametrs cardParam = card.GetComponent<CardParametrs>();
        cardParam.positionInGame = PositionInGame.InGame;
        Transform newHolder = Instantiate(cardHolder, inGamePlace.transform).transform;
        StartCoroutine(cardParam.CardMoving(newHolder));
    }

    // Выключаем блокирующий UI у активного игрока и включаем у остальных
    void ActivateDeck(GameObject playerCheck)
    {        
        
        foreach (var player in players)
        {
            PlayerStats stat = player.GetComponent<PlayerStats>();
            stat.deactivationCover.SetActive(false);
            if (player == playerCheck)
            {
                stat.deactivationCover.SetActive(false);
            }
            else if (player != playerCheck)
                { stat.deactivationCover.SetActive(true); }
        }
    }


    //Отслеживем клики мышкой, смотрим по тегам что это
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.pointerCurrentRaycast.gameObject.tag)
        {
            case "deck":
                if (state == GameStates.PlayerTakeCard)
                {
                    Debug.Log("Колода!");
                    state = GameStates.PlayerTurn;
                    ActivateDeck(activePlayer);
                    StartCoroutine(CardTaking());
                    return;
                }
                break;
            case "card":
                if (!activeTurn)
                {                    
                    activeCard = eventData.pointerCurrentRaycast.gameObject;
                    
                }
                else defensiveCard= eventData.pointerCurrentRaycast.gameObject;


                break;
            case "selectPlayer":
                if (activeCard != null && !activeTurn)
                {
                    targetPlayer = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
                }
                else return;
                break;
            default:
                if (!activeTurn)
                {
                    activeCard = null;
                    targetPlayer = null;
                }
                else defensiveCard = null;

                Debug.Log("Ничего не выбрано!  " + eventData.pointerCurrentRaycast.gameObject.name);
                break;
        }
    }

    // Корутина для проверки типа карты и ожидания реакции атакованого игрока
    IEnumerator CardState( bool specialState = false)
    {
             

        if (activeCard != null)
        {
            
            // Проверяем возможность использования карты
            if (!CardAvailability())
            {
                activeTurn = false;
                ClearCardAndTarget();
                Debug.Log("Нельзя Использовать");
                yield break;
            }

            switch (activeCard.GetComponent<CardParametrs>().typeOfCard)
                {
                    case TypeOfCard.Target:
                    
                    if (targetPlayer != null)
                    {
                        // В некоторых случая корутина запускается во время активного хода с множеством карт, чтобы не перемещать активную карту каждый раз, то вот.
                        if (cardPool.Count==0)
                        {
                            StartCoroutine(InGamePlacing(activeCard, 0.3f, InGamePlace));
                        }

                        // Проверям наличии бочки и результат проверки бочки
                        int[] digit;
                        if (CardChekingBarrel(out digit))
                        {
                            yield return new WaitForSeconds(1f);
                            GameObject CheckCard;
                            if (CardChecking(SuitCard.Clubs, digit, out CheckCard))
                            {
                                Debug.Log("Бочка");
                                CardAction(true);
                                activeTurn = false;
                                StartCoroutine(DropingCard(activeCard, 1f));
                                StartCoroutine(DropingCard(CheckCard, 1f));
                                ClearCardAndTarget();
                                yield break;
                            }
                            else StartCoroutine(DropingCard(CheckCard, 1f));

                        }

                        // Зацикленно состояние ожидания игрока, пока атакуемый не закончит ход.
                        ActivateDeck(targetPlayer);
                        Debug.Log("Карта с целью");
                        while (true)
                        {
                            
                            if (activeTurn)
                            {
                                
                                yield return null;
                            }
                            else
                            {
                                Debug.Log("Применяем эффект карты");
                                CardAction();
                                yield break;
                            }
                        }
                        
                    }
                    else
                    {
                        ClearCardAndTarget();
                        activeTurn = false;
                        Debug.Log("Нет цели!");
                        yield break;
                    };

                       

                    case TypeOfCard.NoTarget:
                    StartCoroutine(InGamePlacing(activeCard, 0.3f, InGamePlace));
                    targetPlayer = null;
                    CardAction();
                        Debug.Log("Карта без цели");
                        break;

                    case TypeOfCard.Passive:
                    targetPlayer = null;
                    CardAction();
                        Debug.Log("Пасивная");
                        break;
                    default:
                        yield return null;
                        break;
                }
            
            
        }
        else
        {
            activeTurn = false;
            Debug.Log("Нет Карты");
            yield break;
        }
    }

    // Метод хранящий в себе все действия карт
    void CardAction(bool pass = false)
    {
        
        switch (activeCard.GetComponent<CardParametrs>().nameOfCard)
        {
            // 
            case "Bang!":
              
                bangCount++;

                if (pass)
                {
                    break;
                }

                if (defensiveCard!=null && defensiveCard.GetComponent<CardParametrs>().nameOfCard=="PassBy")
                {
                   
                    StartCoroutine(InGamePlacing(defensiveCard, 0.5f, InGamePlace));
                    StartCoroutine(DropingCard(activeCard, 2f));
                    StartCoroutine(DropingCard(defensiveCard, 2f));
                    Debug.Log("Мимо");
                    ClearCardAndTarget();
                    
                }
                else
                {
                    StartCoroutine(DropingCard(activeCard, .5f));
                    targetPlayer.GetComponent<PlayerStats>().health--;
                    ClearCardAndTarget();
                }
               
                break;

            case "Duel":
                // добавляем лист для добавления в него всех сыграных бэнгов, чтобы потом разом всех их сбросить
                

                if (defensiveCard == null)
                {
                    cardPool.Add(activeCard);
                    foreach (var card in cardPool)
                    {
                        StartCoroutine(DropingCard(card, .5f));
                    }
                    cardPool.Clear();
                    
                    targetPlayer.GetComponent<PlayerStats>().health--;
                    ClearCardAndTarget();

                }
                else if (defensiveCard != null && targetPlayer!=activePlayer && defensiveCard.GetComponent<CardParametrs>().nameOfCard == "Bang!")
                {
                    cardPool.Add(defensiveCard);
                    StartCoroutine(InGamePlacing(defensiveCard, 0.5f, InGamePlace));
                    Debug.Log("Нужен Бэнг");
                    defensiveCard = null;
                    tempTarget = targetPlayer;
                    targetPlayer = activePlayer;                    
                    activeTurn = true;
                    StartCoroutine(CardState());
                   
                }
                else if (defensiveCard != null && targetPlayer==activePlayer && defensiveCard.GetComponent<CardParametrs>().nameOfCard == "Bang!")
                {
                    cardPool.Add(defensiveCard);
                    Debug.Log("Нужен Бэнг");
                    defensiveCard = null;                    
                    targetPlayer = tempTarget;
                    activeTurn = true;
                    StartCoroutine(CardState());
                }
                break;

            case "Beer":
                activeTurn = false;
                StartCoroutine(DropingCard(activeCard, 2f));
                activePlayer.GetComponent<PlayerStats>().health++;
                ClearCardAndTarget();
                break;

            case "Barrel":
                activeTurn = false;
                if (activePlayer.GetComponent<PlayerStats>().Barrel == true)
                {
                    Debug.Log("Уже есть!");
                    break;
                }
                StartCoroutine(InGamePlacing(activeCard, 0.5f, activePlayer.GetComponent<PlayerStats>().passivePlace));
                activePlayer.GetComponent<PlayerStats>().Barrel=true;
                ClearCardAndTarget();
                break;

            case "Reds":
                if (targetPlayer!=null)
                {
                    activeTurn = true;
                    
                    if (defensiveCard != null && defensiveCard.GetComponent<CardParametrs>().nameOfCard == "Bang!")
                    {
                        StartCoroutine(InGamePlacing(defensiveCard, 0.3f, InGamePlace));
                        cardPool.Add(defensiveCard);
                        defensiveCard = null;
                    }
                    else targetPlayer.GetComponent<PlayerStats>().health--;
                }
                else
                {
                    cardPool.Add(activeCard);
                }
                activeCard.GetComponent<CardParametrs>().typeOfCard = TypeOfCard.Target;
                ActiveID++;
                targetPlayer = players[ActiveID];
                if (targetPlayer==activePlayer)
                {
                    activeTurn = false;
                    activeCard.GetComponent<CardParametrs>().typeOfCard = TypeOfCard.NoTarget;
                    foreach (var card in cardPool)
                    {
                        StartCoroutine(DropingCard(card, 1f));
                    }
                    cardPool.Clear();
                    ClearCardAndTarget();
                    break;
                }
                StartCoroutine(CardState());
                break;

                

            default:
                Debug.Log("Карта не из списка");
                StopAllCoroutines();
                ClearCardAndTarget();
                break;
        }
    }

    // Проверка на возможность розыгрыша карты
    bool CardAvailability()
    {
        switch (activeCard.GetComponent<CardParametrs>().nameOfCard)       
        {
            case "Bang!":
                if (bangCount > 0)
                {
                    Debug.Log("Нельзя Bang");
                    return false;
                }
                else return true;
            case "Beer":
                if (activePlayer.GetComponent<PlayerStats>().health < activePlayer.GetComponent<PlayerStats>().MaxHealth)
                {
                    return true;
                }
                else return false;
            case "PassBy":
                Debug.Log("Мимо без нападения");
                return false;
            default: return true;
        }
    }

    // Метод для вызова во время нажатия кнопки
    public void ButtonPresed()
    {
        if (state==GameStates.PlayerTakeCard || state == GameStates.START)
        {
            return;
        }

        if (activeCard!=null)
        {
            if (!activeTurn)
            {
                activeTurn = true;
                StartCoroutine(CardState());
            }
            else if (activeTurn)
            {
                activeTurn = false;
            }
        }
        else
        {
            ChangeTurn();
        }
        
        
    }

    // Метод для смены активного игрока и смены хода
    void ChangeTurn()
    {
        bangCount = 0;
        ActiveID++;
        activePlayer = players[ActiveID];
        ActivateDeck(activePlayer);
        state = GameStates.PlayerTakeCard;
        Debug.Log("Новый игрок");
    }

    // Очищаем все выбраные карты и цели
    void ClearCardAndTarget()
    {
        activeCard = null;
        targetPlayer = null;
        defensiveCard = null;
        tempTarget = null;
        ActivateDeck(activePlayer);
    }




}




