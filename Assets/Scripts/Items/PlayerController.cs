using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Button;

public class PlayerController : Item
{
    enum Direction { Up, Down, Left, Right };

    [Header("Teleporter")]
    [SerializeField] float teleportFallDistance = 1.2f;
    [SerializeField] float teleportFallDuration = 0.18f;
    [SerializeField] float teleportRiseHeight = 1.5f;
    [SerializeField] float teleportRiseDuration = 0.25f;

    private bool arrivedByTeleport = false;
    private Coroutine teleportCoroutine;


    [Header("Player Pieces")]
    [SerializeField] GameObject topPiece;
    [SerializeField] GameObject bottomPiece;

    [Header("Jump Movement")]
    [SerializeField] float jumpDuration = 0.35f;
    [SerializeField] float jumpArcHeight = 1.2f;


    [Header("Launcher State")]
    private Launcher activeLauncher = null;

    [Header("Rotation")]
    public float rotationDuration = 0.15f;

    private Coroutine rotationCoroutine;


    [Header("Flip Animation")]
    public float flipDuration = 0.2f;
    private Coroutine flipRoutine;

    [Header("Stacking State")]
    public int topSize = 1;
    public int bottomSize = 1;
    public bool isUpsideDown = false; // false = upright, true = upside down

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float heightOffset = 0.6f;
    public float heightOffsetIncrease = 0.05f;
    [Header("Invalid Orientation Feedback")]
    public Color invalidFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] float flashDuration = 0.12f;

    [SerializeField] float recoilDistance = 0.08f;
    [SerializeField] float recoilDuration = 0.15f;
    [SerializeField] AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    GameManager gameManager;
    private bool isMoving = false;
    private bool isTeleporting = false;
    private Vector3 targetPosition;
    private GridManager gridManager;

    private Vector2 moveInput;

    private Coroutine movementCoroutine;
    private Coroutine feedbackCoroutine;

    private CameraShake cameraShake;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.UpdateSizeText(topSize, bottomSize);

        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
            return;
        }
        if(isTeleporting)
        {
            return;
        }
        if (moveInput != Vector2.zero)
        {
            ProcessMoveInput();
            moveInput = Vector2.zero; // consume input (1 tile per press)
        }
    }

    public void StartPlayerPos()
    {
        gridManager = FindFirstObjectByType<GridManager>();

        Vector3 startWorldPos = gridManager.GridToWorld(x, y) + Vector3.up * heightOffset * 2;

        transform.position = startWorldPos;
        targetPosition = startWorldPos;
    }
    // This gets called automatically by PlayerInput component
    public void OnMove(InputAction.CallbackContext ctx)
    {

        if (ctx.performed)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }
    }

    private void ProcessMoveInput()
    {
        // Force strict 4-direction
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            moveInput = new Vector2(Mathf.Sign(moveInput.x), 0f);
        else
            moveInput = new Vector2(0f, Mathf.Sign(moveInput.y));

        int moveX = 0;
        int moveY = 0;

        // Isometric mapping
        if (moveInput.y > 0.1f)       // W / Up
        {
            moveX = 1;
            moveY = 0;
        }
        else if (moveInput.y < -0.1f) // S / Down
        {
            moveX = -1;
            moveY = 0;
        }
        else if (moveInput.x < -0.1f) // A / Left
        {
            moveX = 0;
            moveY = 1;
        }
        else if (moveInput.x > 0.1f)  // D / Right
        {
            moveX = 0;
            moveY = -1;
        }

        AttemptMove(moveX, moveY);
    }

    private void AttemptMove(int moveX, int moveY)
    {
        if (moveX == 0 && moveY == 0)
            return;

        // ============================
        // ✅ LAUNCHER CHECK (CURRENT TILE)
        // ============================

        activeLauncher = null;
        List<Item> currentTileItems = gridManager.GetItemsAt(x, y);

        foreach (Item item in currentTileItems)
        {
            if (item is Launcher launcher)
            {
                activeLauncher = launcher;
                break;
            }
        }

        // ============================
        // ✅ DISTANCE CALCULATION
        // ============================

        int distance = 0;

        if (activeLauncher != null)
        {
            distance = activeLauncher.GetDistance(moveX, moveY);
        }

        int extraX = 0;
        int extraY = 0;

        switch (GetDirection(moveX, moveY))
        {
            case Direction.Up: extraX = distance; break;
            case Direction.Down: extraX = -distance; break;
            case Direction.Right: extraY = distance; break;
            case Direction.Left: extraY = -distance; break;
        }

        int newX = x + moveX + extraX;
        int newY = y + moveY + extraY;

        // ============================
        // ✅ SKIP EMPTY / OUT OF BOUNDS
        // ============================

        while ((!IsInsideGrid(newX, newY) || ContainsEmpty(gridManager.GetItemsAt(newX, newY)))
               && (extraX != 0 || extraY != 0))
        {
            if (extraX != 0)
                extraX = (int)(Mathf.Sign(extraX) * (Mathf.Abs(extraX) - 1));

            if (extraY != 0)
                extraY = (int)(Mathf.Sign(extraY) * (Mathf.Abs(extraY) - 1));

            newX = x + moveX + extraX;
            newY = y + moveY + extraY;
        }

        if (!IsInsideGrid(newX, newY) || ContainsEmpty(gridManager.GetItemsAt(newX, newY)))
            return;

        // ============================
        // ✅ TILE ITEM RESOLUTION
        // ============================

        List<Item> targetItems = gridManager.GetItemsAt(newX, newY);

        // 1. Doll Piece has highest priority
        foreach (Item item in targetItems)
        {
            if (item is DollPiece dollPiece)
            {
                HandleDollPiece(dollPiece, newX, newY);
                return;
            }
        }

        // 2. Launcher
        foreach (Item item in targetItems)
        {
            if (item is Launcher)
            {
                ExecuteMove(newX, newY);
                return;
            }
        }

        // 3. Winning Gate
        foreach (Item item in targetItems)
        {
            if (item is WinningGate winningGate)
            {
                HandleWinningGate(winningGate, newX, newY);
                return;
            }
        }

        // 4. Teleporter
        foreach (Item item in targetItems)
        {
            if (item is Teleporter teleporter && teleporter.active)
            {
                ExecuteMove(newX, newY);
                HandleTeleporter(teleporter);
                return;
            }
        }

        // 5. Button
        foreach (Item item in targetItems)
        {
            ExecuteMove(newX, newY);
            if (item is Button button && button.active)
            {
                
                HandleButton(button);
                return;
            }
        }

        // 4. Empty tile is already filtered earlier

        // 5. No blocking item → normal move
        if (targetItems.Count == 0)
        {
            ExecuteMove(newX, newY);
            return;
        }
    }
    private void HandleButton(Button button)
    {
        if (!button.active)
            return;

        int minSize = Mathf.Min(topSize, bottomSize);

        if (minSize < button.size)
        {
            button.FailedPress();
            return;
        }


        // ✅ ACTIVATE BUTTON EFFECT
        switch (button.type)
        {
            case ButtonType.DeactivateBlocks:
                foreach (Item item in gridManager.items)
                {
                    if (item is Block block)
                    {
                        block.Deactivate();
                    }
                }
                break;

            case ButtonType.ActivateTeleporters:
                foreach (Item item in gridManager.items)
                {
                    if (item is Teleporter teleporter)
                    {
                        teleporter.active = true;
                    }
                }
                break;
        }

        button.Pressed();
        FindFirstObjectByType<SFXManager>().PlayClip("button");
    }

    private void HandleTeleporter(Teleporter source)
    {
        // Prevent infinite teleport loops
        if (arrivedByTeleport)
        {
            arrivedByTeleport = false;
            return;
        }

        if (teleportCoroutine != null)
            StopCoroutine(teleportCoroutine);

        teleportCoroutine = StartCoroutine(TeleportRoutine(source));
    }
    private IEnumerator TeleportRoutine(Teleporter source)
    {
        FindFirstObjectByType<SFXManager>().PlayClip("teleport");

        // Wait until normal move finishes
        while (isMoving)
            yield return null;
        isTeleporting = true;
        Teleporter target = FindClosestOtherTeleporter(source);

        if (target == null)
        {
            Debug.LogError("TELEPORTER ERROR: No other active teleporter found!");
            yield break;
        }

        arrivedByTeleport = true;

        // ======================
        // FALL INTO PORTAL
        // ======================
        Vector3 fallStart = transform.position;
        Vector3 fallEnd = fallStart + Vector3.down * teleportFallDistance;

        float t = 0f;
        while (t < teleportFallDuration)
        {
            t += Time.deltaTime;
            float lerp = t / teleportFallDuration;
            transform.position = Vector3.Lerp(fallStart, fallEnd, lerp);
            yield return null;
        }

        transform.position = fallEnd;

        // ======================
        // INSTANT RELOCATE (HIDDEN)
        // ======================
        x = target.x;
        y = target.y;

        Vector3 targetBasePos =
            gridManager.GridToWorld(x, y) + Vector3.up * heightOffset;

        transform.position = targetBasePos + Vector3.down * teleportRiseHeight;

        // ======================
        // JUMP OUT OF PORTAL
        // ======================
        float jumpT = 0f;
        Vector3 riseStart = transform.position;
        Vector3 riseEnd = targetBasePos;

        while (jumpT < teleportRiseDuration)
        {
            jumpT += Time.deltaTime;
            float lerp = jumpT / teleportRiseDuration;

            float arc = 4f * teleportRiseHeight * lerp * (1f - lerp);
            transform.position = Vector3.Lerp(riseStart, riseEnd, lerp) + Vector3.up * arc;

            yield return null;
        }

        transform.position = riseEnd;
        targetPosition = riseEnd;

        arrivedByTeleport = false;
        teleportCoroutine = null;
        isTeleporting = false;

    }
    private Teleporter FindClosestOtherTeleporter(Teleporter source)
    {
        Teleporter closest = null;
        float closestDist = float.MaxValue;

        foreach (Item item in gridManager.items)
        {
            if (item is not Teleporter tp)
                continue;

            if (!tp.active || tp == source)
                continue;

            float dist =
                Mathf.Abs(tp.x - source.x) +
                Mathf.Abs(tp.y - source.y);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = tp;
            }
        }

        return closest;
    }

    private bool ContainsEmpty(List<Item> items)
    {
        foreach (Item item in items)
        {
            if (item is Empty || (item is Block block && block.active))
                return true;
        }

        return false;
    }



    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < gridManager.width &&
               y >= 0 && y < gridManager.height;
    }
    private void ExecuteMove(int newX, int newY)
    {
        int dx = newX - x;
        int dy = newY - y;

        x = newX;
        y = newY;

        targetPosition =
            gridManager.GridToWorld(x, y) + Vector3.up * heightOffset;

        bool isJump = Mathf.Abs(dx) > 1 || Mathf.Abs(dy) > 1;

        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        if (isJump)
            movementCoroutine = StartCoroutine(JumpToTarget());
        else
            movementCoroutine = StartCoroutine(MoveToTargetLinear());

        isMoving = true;

        Flip(dx, dy);

        if (isJump)
            FindFirstObjectByType<SFXManager>().PlayClip("jump");
        else
            FindFirstObjectByType<SFXManager>().PlayClip("step");
    }

    private IEnumerator MoveToTargetLinear()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;

        if (cameraShake != null)
        {
            int totalSize = topSize + bottomSize;
            cameraShake.Shake(totalSize, false);
        }

    }

    private IEnumerator JumpToTarget()
    {
        Vector3 start = transform.position;
        Vector3 end = targetPosition;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            // Horizontal interpolation
            Vector3 flatPos = Vector3.Lerp(start, end, t);

            // Vertical parabolic arc
            float height = 4f * jumpArcHeight * t * (1f - t);

            transform.position = flatPos + Vector3.up * height;

            yield return null;
        }

        transform.position = end;
        isMoving = false;

        if (cameraShake != null)
        {
            int totalSize = topSize + bottomSize;
            cameraShake.Shake(totalSize, true);
        }

    }

    private void Flip(int dx, int dy)
    {
        bool startedUpsideDown = isUpsideDown;

        // Toggle vertical orientation (this is the logical state)
        isUpsideDown = !isUpsideDown;

        float yRotation = 0f;
        float zRotation = isUpsideDown ? 180f : 0f;

        Direction direction = GetDirection(dx, dy);

        switch (direction)
        {
            case Direction.Up: yRotation = 0f; break;
            case Direction.Down: yRotation = 180f; break;
            case Direction.Right: yRotation = -90f; break;
            case Direction.Left: yRotation = 90f; break;

        }


        Quaternion targetRotation = Quaternion.Euler(0f, yRotation, zRotation);

        // If a previous rotation is running, snap to its target first
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            // Snap to the new *correct* rotation so logic and visuals match
            transform.rotation = targetRotation;
        }


        rotationCoroutine = StartCoroutine(
            RotateTo(targetRotation, startedUpsideDown)
        );
    }


    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }




    private IEnumerator RotateTo(Quaternion targetRotation, bool startedUpsideDown)
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        // Determine the primary axis of rotation in local space
        // Since you only rotate around Y and Z, we can safely animate in Euler space
        Vector3 startEuler = startRotation.eulerAngles;
        Vector3 targetEuler = targetRotation.eulerAngles;

        // Normalize angles to -180..180 for clean delta math
        float deltaY = Mathf.DeltaAngle(startEuler.y, targetEuler.y);
        float deltaZ = Mathf.DeltaAngle(startEuler.z, targetEuler.z);


        deltaY = -deltaY;
        deltaZ = -deltaZ;


        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);

            float currentY = startEuler.y + deltaY * t;
            float currentZ = startEuler.z + deltaZ * t;

            transform.rotation = Quaternion.Euler(0f, currentY, currentZ);

            yield return null;
        }

        // Hard snap to the correct logical rotation at the end
        transform.rotation = targetRotation;
        rotationCoroutine = null;
    }


    private void HandleDollPiece(DollPiece piece, int newX, int newY)
    {
        int activeSize = isUpsideDown ? bottomSize : topSize;

        // Size rule: must be exactly +1
        if (piece.size != activeSize + 1)
        {
            if(isUpsideDown)
                PlayInvalidOrientationFeedback(piece.gameObject.GetComponentInChildren<MeshRenderer>().gameObject, topPiece);
            else
                PlayInvalidOrientationFeedback(piece.gameObject.GetComponentInChildren<MeshRenderer>().gameObject, bottomPiece);

            return;
        }
        

        bool orientationMatch =
            (!isUpsideDown && piece.type == DollPieceType.Top) ||
            (isUpsideDown && piece.type == DollPieceType.Bottom);

        if (!orientationMatch)
        {
            if(isUpsideDown && piece.type == DollPieceType.Top)
            {
                PlayInvalidOrientationFeedback(piece.gameObject.GetComponentInChildren<MeshRenderer>().gameObject, topPiece);
            }
            else
            {
                PlayInvalidOrientationFeedback(piece.gameObject.GetComponentInChildren<MeshRenderer>().gameObject, bottomPiece);


            }
            return;
        }
           
   
        AttachPiece(piece);
        ExecuteMove(newX, newY);
    }

    public void PlayInvalidOrientationFeedback(GameObject dollPieceObj, GameObject playerPieceObj)
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(
            InvalidOrientationRoutine(dollPieceObj, playerPieceObj)
        );

        FindFirstObjectByType<SFXManager>().PlayClip("error");
    }
    private IEnumerator InvalidOrientationRoutine(GameObject dollPieceObj, GameObject playerPieceObj)
    {
        // Start both flashes in parallel
        Coroutine flash1 = StartCoroutine(FlashObject(dollPieceObj));
        Coroutine flash2 = StartCoroutine(FlashObject(playerPieceObj));

        // Start recoil at the same time
        yield return StartCoroutine(RecoilFromTarget(dollPieceObj.transform));

        // Ensure flashes are fully finished
        if (flash1 != null) yield return flash1;
        if (flash2 != null) yield return flash2;

        feedbackCoroutine = null;
    }
    private IEnumerator FlashObject(GameObject obj)
    {
        if (obj == null)
            yield break;

        Renderer r = obj.GetComponentInChildren<Renderer>();
        if (r == null)
            yield break;

        Material mat = r.material; // instance copy
        Color original = mat.color;

        mat.color = invalidFlashColor;
        yield return new WaitForSeconds(flashDuration);
        mat.color = original;
    }
    private IEnumerator RecoilFromTarget(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 startPos = transform.position;

        // Direction toward the invalid piece (flattened vertically)
        Vector3 dir = (target.position - startPos);
        dir.y = 0f;
        dir.Normalize();

        Vector3 recoilTarget = startPos + dir * recoilDistance;

        float elapsed = 0f;

        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / recoilDuration);
            float eased = recoilCurve.Evaluate(t);

            // Ping-pong style recoil (toward then back)
            float pingPong = Mathf.Sin(t * Mathf.PI);

            transform.position = Vector3.Lerp(
                startPos,
                recoilTarget,
                pingPong * eased
            );

            yield return null;
        }

        transform.position = startPos;
    }

    private void HandleWinningGate(WinningGate gate, int newX, int newY)
    {
        ExecuteMove(newX, newY);
        StartCoroutine(TryWin(gate));
    }
    IEnumerator TryWin(WinningGate gate)
    {
        while (isMoving)
        {
            yield return new WaitForEndOfFrame();
        }
        bool win = gate.TryWin(topSize, bottomSize, isUpsideDown, gameObject);
        if(!win)
        {
            PlayInvalidOrientationFeedback(gate.gameObject, bottomPiece);
            
        }
    }
    
    private void AttachPiece(DollPiece piece)
    {
        Vector3 localOffset;

        if (piece.type == DollPieceType.Top)
        {
            // Stack upward from the top half
            topPiece = piece.gameObject.GetComponentInChildren<MeshRenderer>().gameObject;
            localOffset = Vector3.up * (topSize * 0.4f);
            topSize = piece.size;

        }
        else // Bottom
        {
            // Stack downward from the bottom half
            bottomPiece = piece.gameObject.GetComponentInChildren<MeshRenderer>().gameObject;

            localOffset = Vector3.down * (bottomSize * 0.4f);
            bottomSize = piece.size;

        }
        heightOffset += heightOffsetIncrease;
        gameManager.UpdateSizeText(topSize, bottomSize);
        gridManager.RemoveItem(piece);

        piece.AttachTo(transform, localOffset);
        FindFirstObjectByType<SFXManager>().PlayClip("attach");


    }

    private Direction GetDirection(int dx, int dy)
    {
        // Your corrected isometric-facing mapping:
        if (dy == 0 && dx > 0)        // Up (in your grid)
            return Direction.Up;
        else if (dy == 0 && dx < 0)   // Down
            return Direction.Down;
        else if (dy > 0 && dx == 0)   // Right
            return Direction.Right;
        else if (dy < 0 && dx == 0)   // Left
            return Direction.Left;

        return Direction.Up;
    }





}