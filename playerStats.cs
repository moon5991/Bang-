using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class playerStats : MonoBehaviour
{
    public GameObject cardHolder, contentObj;

    public string role;

    public string character;

    
    public int health;

    public int maxHealth;
  
    public int cardAmount;

    public RectTransform deckPosition;
    
    public int counterCadr;

    public bool _isAttacked;

    //public GameObject[] healthImages;

    //private void Awake()
    //{
    //    healthImages = GameObject.Find("health").GetComponentsInChildren<GameObject>();
    //}
    private void Start()
    {
        health = maxHealth;
        _isAttacked = false;
        counterCadr = 0;
    }

    // Метод взятия карты из переданого массива карт, с активацией метода самой карты для перемещения карты в колоду игрока
    // Для перемещения карты к новому родителю, сначало родитель Holder создается в руке игрока и передается в метод карты
    public IEnumerator cardTakingDeck(GameObject[] cards, GameStates states)
    {
        Debug.Log("Courutine Works");
        if (states == GameStates.START)
        {

             while (cardAmount < health)
            {
                GameObject curentCard = cards[Random.Range(0, cards.Length - 1)];
                if (curentCard.GetComponent<CardChar>().gamePos == positionInGame.Deck)
                {
                    GameObject newCardHolder = Instantiate(cardHolder, contentObj.transform);
                    StartCoroutine(curentCard.GetComponent<cardLogic>().cardMoving(newCardHolder));
                    cardAmount++;
                    Debug.Log(transform.name + "  takingCard");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    Debug.Log("InHand");
                    yield return null;
                }
            }
        }

    }

    public IEnumerator cardTakingDeck(GameObject[] cards)
    {
        Debug.Log("Courutine Works");


        while (counterCadr < 2)
        {
            GameObject curentCard = cards[Random.Range(0, cards.Length - 1)];
            if (curentCard.GetComponent<CardChar>().gamePos == positionInGame.Deck)
            {
                GameObject newCardHolder = Instantiate(cardHolder, contentObj.transform);
                StartCoroutine(curentCard.GetComponent<cardLogic>().cardMoving(newCardHolder));
                cardAmount++;
                counterCadr++;
                Debug.Log("After inizializtion " + transform.name + "  takingCard");

                yield return new WaitForSeconds(1f);
            }
        }
               

            
        
    }



}
