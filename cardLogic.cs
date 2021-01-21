using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cardLogic : MonoBehaviour {

    
    public Transform startPlace;
    public float speed;

    private void Start()
    {
        startPlace = transform;
        
    }
    private void Update()
    {
        
        
    }
    public void starCourtin()
    {

    }



    // Перемещаем карту в переданый Holder и помещаем ее в иерархию Holder, меняем позицию в колоде на "в руке"
    public IEnumerator cardMoving(GameObject newCardPosition)
    {
        


        while (startPlace.position!= newCardPosition.transform.position)
        {
            
            startPlace.position = Vector3.MoveTowards(startPlace.position, newCardPosition.transform.position, 10f*speed*Time.deltaTime);
            transform.rotation = Quaternion.Lerp(startPlace.rotation, newCardPosition.transform.rotation, 0.1f * speed * Time.deltaTime);
            if (GetComponent<CardChar>().gamePos != positionInGame.Dropping)
            {
                GetComponent<CardChar>().gamePos = positionInGame.inHand;
            }
            
            yield return null;
            
        }
        while ( true)
        {
            if (GetComponent<CardChar>().gamePos != positionInGame.Dropping)
            {
                transform.SetParent(newCardPosition.transform, true);
            }
                      
            yield break;

        }

        
        
    }

}
