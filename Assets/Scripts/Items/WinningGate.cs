using System.Collections;
using UnityEngine;

public class WinningGate : Item
{
    [Header("Win Condition")]
    public int requiredSize;
    [SerializeField] GameObject teleportEffect;
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
        GameObject particle = Instantiate(teleportEffect,transform.position, Quaternion.identity);
        Destroy(playerObject);
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().NextLevel();
        yield return null;
    }
}
