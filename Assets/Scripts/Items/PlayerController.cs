using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : Item
{
    enum Direction { Up, Down, Left, Right };

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

    private bool isMoving = false;
    private Vector3 targetPosition;
    private GridManager gridManager;

    private Vector2 moveInput;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        Vector3 startWorldPos =
            gridManager.GridToWorld(x, y) + Vector3.up * heightOffset;

        transform.position = startWorldPos;
        targetPosition = startWorldPos;
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
            return;
        }

        if (moveInput != Vector2.zero)
        {
            ProcessMoveInput();
            moveInput = Vector2.zero; // consume input (1 tile per press)
        }
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

        activeLauncher = null;

        while((!IsInsideGrid(newX, newY) || gridManager.GetItemAt(newX, newY) is Empty) && (extraX > 0 || extraY > 0))
        {
            if (extraX > 0)
                extraX = (int)(Mathf.Sign(extraX) * (Mathf.Abs(extraX) - 1));

            if (extraY > 0)
                extraY = (int)(Mathf.Sign(extraY) * (Mathf.Abs(extraY) - 1));
            newX = x + moveX + extraX;
            newY = y + moveY + extraY;
        }
        if (!IsInsideGrid(newX, newY) || gridManager.GetItemAt(newX, newY) is Empty)
        {
            return;
        }
           

        Item item = gridManager.GetItemAt(newX, newY);

        if (item == null)
        {
            ExecuteMove(newX, newY);
            return;
        }

        // Dispatch by item type
        if (item is DollPiece dollPiece)
        {
            HandleDollPiece(dollPiece, newX, newY);
            return;
        }

        if (item is WinningGate winningGate)
        {
            HandleWinningGate(winningGate, newX, newY);
            return;
        }
        if (item is Launcher launcher)
        {
            HandleLauncher(launcher, newX, newY);
            return;
        }

        
    }
    private void HandleLauncher(Launcher launcher, int newX, int newY)
    {
        // Step onto the launcher normally
        activeLauncher = launcher;
        ExecuteMove(newX, newY);
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

        isMoving = true;

        Flip(dx, dy);

        if(Mathf.Abs(dx) > 1 || Mathf.Abs(dy) > 1)
            FindObjectOfType<SFXManager>().PlayClip("jump");
        else
            FindObjectOfType<SFXManager>().PlayClip("step");


    }

    private void HandleDollPiece(DollPiece piece, int newX, int newY)
    {
        int activeSize = isUpsideDown ? bottomSize : topSize;

        // Size rule: must be exactly +1
        if (piece.size != activeSize + 1)
            return;

        bool orientationMatch =
            (!isUpsideDown && piece.type == DollPieceType.Top) ||
            (isUpsideDown && piece.type == DollPieceType.Bottom);

        if (!orientationMatch)
            return;

        AttachPiece(piece);
        ExecuteMove(newX, newY);
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
        gate.TryWin(topSize, bottomSize, isUpsideDown, gameObject);

    }

    private void AttachPiece(DollPiece piece)
    {
        Vector3 localOffset;

        if (piece.type == DollPieceType.Top)
        {
            // Stack upward from the top half
            localOffset = Vector3.up * (topSize * 0.4f);
            topSize = piece.size;

        }
        else // Bottom
        {
            // Stack downward from the bottom half
            localOffset = Vector3.down * (bottomSize * 0.4f);
            bottomSize = piece.size;

        }

        gridManager.RemoveItem(piece);

        piece.AttachTo(transform, localOffset);
        FindObjectOfType<SFXManager>().PlayClip("attach");


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
            case Direction.Up: yRotation = 0f;break;
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





}









