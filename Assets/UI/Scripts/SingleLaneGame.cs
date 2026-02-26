using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleLaneGame : MonoBehaviour
{
    public GameObject card;
    public GameObject canvas;
    public SingleLanePlayer singleLanePlayer;

    // Start is called before the first frame update
    void Start()
    {
        SetHand();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetHand()
    {
        singleLanePlayer.SetHand(card, canvas);
    }
}
