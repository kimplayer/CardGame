using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleLaneGame : MonoBehaviour
{
    public SingleLanePlayer me;
    bool gameOver;

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        me.SetHand();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickEndTurn()
    {
        if (!gameOver)
        {
            if (me.CheckSelectedCard())
            {
                SetButton(false, false);
                StartCoroutine("Battle");
            }
            else
            {
                Debug.Log("Card not selected");
            }
        }
    }

    private IEnumerator Battle()
    {
        me.Ready();
        yield return new WaitForSeconds(0.5f);

        me.Fight();
        yield return new WaitForSeconds(0.2f);

        CheckGameOver();
        SetButton(true, true);
        yield return null;
    }

    private bool CheckGameOver()
    {
        int life = int.Parse(GameObject.Find("Score").GetComponent<Text>().text);
        if (life <= 0)
        {
            GameObject canvas = GameObject.Find("Canvas");
            canvas.transform.Find("GameOver").gameObject.SetActive(true);
            gameOver = true;

            return true;
        }

        return false;
    }

    private void SetButton(bool left_ClickEnabled, bool right_ClickEnabled)
    {
        GameObject.Find("EndTurn").GetComponent<Button>().interactable = left_ClickEnabled;
        List<int> remain_cards = me.GetRemainCards();
        GameObject me_object = GameObject.Find("Me");
        foreach (var card in remain_cards)
        {
            string card_name = "Card_" + card.ToString();
            GameObject card_object = me_object.transform.Find(card_name).gameObject;
            card_object.GetComponent<Button>().interactable = left_ClickEnabled;
            card_object.GetComponent<Card>().InfoTextSetActive(false, right_ClickEnabled);
        }
    }
}
