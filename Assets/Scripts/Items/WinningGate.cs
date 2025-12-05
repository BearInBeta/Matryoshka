using System.Collections;
using TMPro;
using UnityEngine;

public class WinningGate : Item
{
    [Header("Win Condition")]
    public int requiredSize;
    [SerializeField] GameObject teleportEffect;
    [SerializeField] float waitTime = 0.5f;

    private void Start()
    {
        if(GetComponentInChildren<TextMeshPro>() != null)
        GetComponentInChildren<TextMeshPro>().text = requiredSize + "";
    }
    public bool TryWin(int playerTopSize, int playerBottomSize, bool isUpsideDown, GameObject playerObject)
    {
        if (playerTopSize == requiredSize &&
            playerBottomSize == requiredSize && !isUpsideDown)
        {
            StartCoroutine(TeleportPlayer(playerObject));
            return true;
        }
        return false;
    }

    IEnumerator TeleportPlayer(GameObject playerObject)
    {
        FindFirstObjectByType<SFXManager>().PlayClip("win");
        Instantiate(teleportEffect,transform.position, Quaternion.identity);
        Destroy(playerObject);

        yield return new WaitForSeconds(waitTime);

   
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().NextLevel();
        yield return null;
    }
}
