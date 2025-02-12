using UnityEngine;
using UnityEngine.EventSystems; // Required for pointer interfaces

public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    public int score;

    private int x;
    private int y;

    public int X
    {
        get { return x; }
        set
        {
            if (IsMoveable())
            {
                x = value;
            }
        }
    }
    public int Y
    {
        get { return y; }
        set
        {
            if (IsMoveable())
            {
                y = value;
            }
        }
    }

    private Grids.PieceType type;
    public Grids.PieceType Type { get { return type; } }
    private Grids grid;
    public Grids GridRef { get { return grid; } }

    private MoveablePiece moveableComponent;
    public MoveablePiece MoveableComponent { get { return moveableComponent; } }

    private ColorPiece colorComponent;
    public ColorPiece ColorComponent { get { return colorComponent; } }

    private ClearablePiece clearableComponent;
    public ClearablePiece ClearableComponent { get { return clearableComponent; } }

    private void Awake()
    {
        moveableComponent = GetComponent<MoveablePiece>();
        colorComponent = GetComponent<ColorPiece>();
        clearableComponent = GetComponent<ClearablePiece>();
    }

    public void Init(int _x, int _y, Grids _grid, Grids.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
    }

    // These methods allow the piece to respond to touch or mouse input.
    public void OnPointerEnter(PointerEventData eventData)
    {
        grid.EnteredPiece(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        grid.PressedPiece(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        grid.ReleasePiece();
    }

    // Optional: You can also leave the legacy mouse callbacks for debugging in the Editor,
    // but they aren’t necessary if you’re only targeting mobile.
    private void OnMouseEnter()
    {
        grid.EnteredPiece(this);
    }

    private void OnMouseDown()
    {
        grid.PressedPiece(this);
    }

    private void OnMouseUp()
    {
        grid.ReleasePiece();
    }

    public bool IsMoveable()
    {
        return moveableComponent != null;
    }

    public bool IsColored()
    {
        return colorComponent != null;
    }

    public bool IsClearable()
    {
        return clearableComponent != null;
    }
}
