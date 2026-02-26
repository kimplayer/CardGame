using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if (singleLaneElement.selectedCard != "")
        {
            string selected_card_name = singleLaneElement.selectedCard;
            GameObject selected_object = GameObject.Find(selected_card_name);

            selected_object.transform.position = new Vector3(0, 0, 0); //선택된 카드 이동
            Debug.Log(selected_card_name + " Moved to (0,0,0)");
        }
        else
        {
            Debug.Log("No card selected");
        }
    }

    public void SetHand(GameObject card, GameObject canvas)
    {
        singleLaneElement.SetHand();
        Vector2 position = new Vector2(0, -200);
        int position_x = -3 * 250;
        foreach( var item in singleLaneElement.handCard)
        {
            GameObject temp = Instantiate(card, canvas.transform);

            position_x += 250;
            position.x = position_x;
            temp.transform.localPosition = position;
            
            temp.transform.Find("Text (Legacy)").GetComponent<Text>().text = item.Value.ToString();

            string card_name = "Card_" + item.Key;
            temp.name = card_name;

            temp.GetComponent<Button>().onClick.AddListener(delegate { ClickCard(); });

            Debug.Log(card_name + " Created at x : " + position_x.ToString());

        }
    }
}
