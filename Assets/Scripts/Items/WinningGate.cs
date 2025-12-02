using System.Collections;
using UnityEngine;

public class WinningGate : Item
{
    [Header("Win Condition")]
    public int requiredSize;
    [SerializeField] float teleportWait = 0.5f;
    [SerializeField] GameObject teleportEffect;
    public void TryWin(int playerTopSize, int playerBottomSize, bool isUpsideDown, GameObject playerObject)
    {
        if (playerTopSize == requiredSize &&
            playerBottomSize == requiredSize && !isUpsideDown)
        {
            StartCoroutine(TeleportPlayer(playerObject));
        }
    }

    IEnumerator TeleportPlayer(GameObject playerObject)
    {
        FindObjectOfType<SFXManager>().PlayClip("win");
        GameObject particle = Instantiate(teleportEffect,transform.position, Quaternion.identity);
        Destroy(playerObject);
        yield return new WaitForSeconds(teleportWait);
        Destroy(particle);
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().NextLevel();
        yield return new WaitForSeconds(teleportWait);
    }
}
