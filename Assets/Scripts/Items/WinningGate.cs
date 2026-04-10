using System.Collections;
using TMPro;
using UnityEngine;

public class WinningGate : Item
{
    [Header("Win Condition")]
    public int requiredSize;
    [SerializeField] GameObject teleportEffect;

    private void Start()
    {
        if(GetComponentInChildren<TextMeshPro>() != null)
        GetComponentInChildren<TextMeshPro>().text = requiredSize + "";
    }
    public bool TryWin(int playerTopSize, int playerBottomSize, GameObject playerObject)
    {
        if (playerTopSize == requiredSize &&
            playerBottomSize == requiredSize)
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

        yield return null;

     }
}
