using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cardBehavior : MonoBehaviour {

    public GameObject dropping;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ActivateCardTarget(GameObject card, GameObject player)
    {
        CardChar cardChar = card.GetComponent<CardChar>();
        playerStats playerStats = player.GetComponent<playerStats>();

        if (cardChar.cardName=="Bang!")
        {
            playerStats.health--;
            CardDroping(card);
        }

        if (cardChar.cardName == "Beer" && playerStats.health<playerStats.maxHealth)
        {
            playerStats.health++;
            CardDroping(card);
        }

        if (cardChar.cardName == "Beer" && playerStats.health >= playerStats.maxHealth)
        {
            Debug.Log("Полная жизнь");
        }
    }

    public void CardDroping(GameObject card)
    {
        GameObject cardHolder = card.transform.parent.gameObject;
        card.transform.SetParent(dropping.transform, true);
        Destroy(cardHolder);
        card.GetComponent<CardChar>().gamePos = positionInGame.Dropping;
        card.GetComponent<cardLogic>().startPlace = card.transform;
        StartCoroutine(card.GetComponent<cardLogic>().cardMoving(dropping));
    }
}
