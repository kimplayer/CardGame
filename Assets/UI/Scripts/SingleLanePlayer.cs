using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SingleLanePlayer : MonoBehaviour
{
    SingleLaneElement singleLaneElement;

    private void Awake()
    {
        singleLaneElement = new SingleLaneElement();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickCard()
    {
        string selected_card_name = EventSystem.current.currentSelectedGameObject.name;
        singleLaneElement.selectedCard = selected_card_name;
        Debug.Log(selected_card_name + " Selected");
    }

    public void ClickConfirm()
    {
        string selected_card_name = singleLaneElement.selectedCard;
        GameObject selected_object = GameObject.Find(selected_card_name);
        selected_object.transform.position = new Vector3(0, 0, 0);
        Debug.Log(selected_card_name + " Moved to (0,0,0)");
    }
}
