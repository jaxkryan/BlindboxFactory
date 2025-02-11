using UnityEngine;

public class GamePiece : MonoBehaviour
{
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
    public ClearablePiece ClearableComponent { get {return clearableComponent;} }
    private void Awake()
    {
        moveableComponent = GetComponent<MoveablePiece>();
        colorComponent = GetComponent<ColorPiece>();
        clearableComponent=GetComponent<ClearablePiece>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(int _x, int _y, Grids _grid, Grids.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;

    }

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
