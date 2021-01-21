using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum typeOfCard { Active, Passive, Attack, AttackBang}
public enum positionInGame { Deck, inHand, activate, Dropping }
public class CardChar : MonoBehaviour
{

    public typeOfCard typeCard;

    public string cardName;

    public positionInGame gamePos;



}
