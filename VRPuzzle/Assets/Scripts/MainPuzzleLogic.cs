using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleBlockType
{
    Block_X_Active,
    Block_O_Active,
    Block_X,
    Block_O,
    Open
}

public class MainPuzzleLogic : MonoBehaviour
{
    #region Initial board information
    int BoardWidth = 7;
    int BoardHeight = 9;
    List<PuzzleBlockType> PuzzleBlockArray;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Init_PuzzleBlockArray();

        PrintBoardToConsole();
    }

    // Update is called once per frame
    float testTimer = 0f;
    int counterX = 0;
    int counterY = 0;
    void Update()
    {
        testTimer += Time.deltaTime;
        if(testTimer > 0.05f)
        {
            testTimer = 0f;

            if (counterX > BoardWidth + 1) // BoardWidth due to +2 width
            {
                counterX = 0;
                ++counterY;

                ClearBoardWalls();
            }

            if (counterY > BoardHeight - 1) return;

            SetBlockAtBoardPosition(counterX, counterY, PuzzleBlockType.Block_X_Active);
            
            ++counterX;

            print("X: " + counterX);
            print("Y: " + counterY);
            print("--------------------------------------");

            PrintBoardToConsole();
        }
    }

    void Init_PuzzleBlockArray()
    {
        PuzzleBlockArray = new List<PuzzleBlockType>();

        for (int i = 0; i < (BoardWidth + 2) * BoardHeight; ++i)
            PuzzleBlockArray.Add(PuzzleBlockType.Open);
    }

    PuzzleBlockType GetBlockAtBoardPosition( int x_, int y_ )
    {
        // if (x_ == 0 || x_ == BoardWidth + 1) return PuzzleBlockType.Open;

        return PuzzleBlockArray[(y_ * (BoardWidth + 2)) + x_];
    }

    void SetBlockAtBoardPosition( int x_, int y_, PuzzleBlockType blockType_ )
    {
        PuzzleBlockArray[(y_ * (BoardWidth + 2)) + x_] = blockType_;
    }

    void ClearBoardWalls()
    {
        for( int i = 0; i < BoardHeight; ++i )
        {
            SetBlockAtBoardPosition(0, i, PuzzleBlockType.Open);
            SetBlockAtBoardPosition(BoardWidth + 1, i, PuzzleBlockType.Open);
        }
    }

    void PrintBoardToConsole()
    {
        // Ran in reverse vertically for sake of console printing
        for (int y = BoardHeight - 1; y >= 0; --y)
        {
            string lineString = "";

            for( int x = 0; x < BoardWidth + 2; ++x )
            {
                lineString += "[ " + GetBlockAtBoardPosition(x, y) + " ]";
            }

            print( lineString );
        }
    }
}
