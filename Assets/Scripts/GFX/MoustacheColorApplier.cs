using UnityEngine;

public class MoustacheColorApplier : MonoBehaviour
{
    public Color color;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GetComponent<SpriteRenderer>().color != color)
        {
            GetComponent<SpriteRenderer>().color = color;
        }
    }
}
