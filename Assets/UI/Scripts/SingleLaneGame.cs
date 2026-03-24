using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleLaneGame : MonoBehaviour
{
    public SingleLanePlayer me;
    public SingleLanePlayer you;
    bool gameOver;

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        me.Initialize(false);
        you.Initialize(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 턴 종료 클릭
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

    // 전투
    private IEnumerator Battle()
    {
        you.AISelectCard();
        me.Ready();
        you.Ready();
        yield return new WaitForSeconds(1f);

        int you_damage = me.GetDamage(you);
        int me_damage = you.GetDamage(me);
        me.Fight(me_damage);
        you.Fight(you_damage);
        yield return new WaitForSeconds(0.5f);

        CheckGameOver();
        SetButton(true, true);
        yield return null;
    }

    // 게임종료 확인
    private bool CheckGameOver()
    {
        int me_life = me.GetLife();
        int you_life = you.GetLife();

        if (me_life <= 0 || you_life <= 0)
        {
            GameObject canvas = GameObject.Find("Canvas");
            GameObject gameover_object = canvas.transform.Find("GameOver").gameObject;
            Text gameover_text = gameover_object.transform.Find("Text").GetComponent<Text>();
            if (me_life <= 0 && you_life <= 0)
            {
                gameover_text.text = "Draw";
            }
            else if (me_life <= 0)
            {
                gameover_text.text = "You Lose!";
            }
            else
            {
                gameover_text.text = "Win!";
            }
            gameover_object.SetActive(true);
            gameOver = true;

            return true;
        }

        return false;
    }

    // 버튼 활성 설정
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
