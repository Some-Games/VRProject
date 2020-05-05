using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleBlockType
{
    Block_X_Active = 0,
    Block_O_Active = 1,
    Block_X,
    Block_O,
    Open,
    Closed
}

public enum PuzzleBlockSize
{
    Square,
    ThreeWide,
    ThreeTall
}

public class MainPuzzleLogic : MonoBehaviour
{
    #region Initial board information
    int BoardWidth = 7;
    int BoardHeight = 9;
    List<PuzzleBlockType> PuzzleBlockArray;
    #endregion

    #region Current Block Information
    Vector2Int BottomLeftCornerPosition;
    [SerializeField] PuzzleBlockSize BlockSize;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Init_PuzzleBlockArray();

        PrintBoardToConsole();
    }

    // Update is called once per frame
    void Update()
    {
        // UPDATE_TEST();
        if (Input.GetKeyDown(KeyCode.A))
        {
            RotateActiveBlocks_CounterClockwise();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
        else if( Input.GetKeyDown(KeyCode.D))
        {
            RotateActiveBlocks_Clockwise();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
    }

    void Init_PuzzleBlockArray()
    {
        PuzzleBlockArray = new List<PuzzleBlockType>();

        for (int i = 0; i < (BoardWidth + 2) * BoardHeight; ++i)
            PuzzleBlockArray.Add(PuzzleBlockType.Open);

        SpawnNewBlock();

        // TEST
        // StartCoroutine(MoveBlocksTest());
    }

    IEnumerator MoveBlocksTest()
    {
        int i = 0;

        while( i < 20 )
        {
            yield return new WaitForSeconds(2.0f);
            print("---");
            MoveActiveBlocks_Down();

            // TODO: Remove when close to shipping
            PrintBoardToConsole();

            yield return new WaitForSeconds(2.0f);
            print("---");
            MoveActiveBlocks_Right();

            // TODO: Remove when close to shipping
            PrintBoardToConsole();

            i++;
        }
    }

    public void SetPuzzleBlockInformation( PuzzleBlockSize blockSize_ )
    {
        BlockSize = blockSize_;
    }

    void SpawnNewBlock()
    {
        // TEST
        // BlockSize = PuzzleBlockSize.ThreeWide;

        // Set X position of next block
        BottomLeftCornerPosition.x = (BoardWidth / 2);
        #region Old
        /*
        if (BlockSize == PuzzleBlockSize.Square || BlockSize == PuzzleBlockSize.ThreeTall)
            BottomLeftCornerPosition.x = (BoardWidth / 2);
        else
            BottomLeftCornerPosition.x = (BoardWidth / 2) + 1;
        */
        #endregion

        // Set Y position of next block
        if (BlockSize == PuzzleBlockSize.ThreeTall)
            BottomLeftCornerPosition.y = BoardHeight - 3;
        else
            BottomLeftCornerPosition.y = BoardHeight - 2;

        // Populate a new useable block onto the board.
        // In all scenarios, there will be a 2x2 placed.
        PuzzleBlockType[] tempArray;
        if (BlockSize == PuzzleBlockSize.Square) tempArray = new PuzzleBlockType[4]; else tempArray = new PuzzleBlockType[6];
        for (int i = 0; i < tempArray.Length; ++i)
            tempArray[i] = (PuzzleBlockType)UnityEngine.Random.Range(0, 2);

        SetBlockAtBoardPosition(BottomLeftCornerPosition.x, BottomLeftCornerPosition.y, tempArray[0]);
        SetBlockAtBoardPosition(BottomLeftCornerPosition.x + 1, BottomLeftCornerPosition.y, tempArray[1]);
        SetBlockAtBoardPosition(BottomLeftCornerPosition.x, BottomLeftCornerPosition.y + 1, tempArray[2]);
        SetBlockAtBoardPosition(BottomLeftCornerPosition.x + 1, BottomLeftCornerPosition.y + 1, tempArray[3]);

        if(BlockSize != PuzzleBlockSize.Square)
        {
            if(BlockSize == PuzzleBlockSize.ThreeTall)
            {
                SetBlockAtBoardPosition(BottomLeftCornerPosition.x, BottomLeftCornerPosition.y + 2, tempArray[4]);
                SetBlockAtBoardPosition(BottomLeftCornerPosition.x + 1, BottomLeftCornerPosition.y + 2, tempArray[5]);
            }
            else
            {
                SetBlockAtBoardPosition(BottomLeftCornerPosition.x + 2, BottomLeftCornerPosition.y, tempArray[4]);
                SetBlockAtBoardPosition(BottomLeftCornerPosition.x + 2, BottomLeftCornerPosition.y + 1, tempArray[5]);
            }
        }
    }

    PuzzleBlockType GetBlockAtBoardPosition( int x_, int y_ )
    {
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
            SetBlockAtBoardPosition( 0, i, PuzzleBlockType.Open );
            SetBlockAtBoardPosition( BoardWidth + 1, i, PuzzleBlockType.Open );
        }
    }

    #region Move Blocks
    void MoveActiveBlocks_Down()
    {
        // Find Bottom blocks, test for new positioning below
        // print( BottomLeftCornerPosition.x + ", " + BottomLeftCornerPosition.y );
        // print( GetBlockAtBoardPosition( BottomLeftCornerPosition.x, BottomLeftCornerPosition.y ) );

        // Ensure all blocks are one row above the baseline first
        if (BottomLeftCornerPosition.y >= 1)
        {
            // Cross through the bottom active blocks and ensure there's nothing below them
            int blockWidth = 2;
            if (BlockSize == PuzzleBlockSize.ThreeWide) blockWidth = 3;

            // Confirm all blocks in lower position are empty, otherwise exit out
            for(int i = 0; i < blockWidth; ++i)
            {
                if ( GetBlockAtBoardPosition( BottomLeftCornerPosition.x + i, BottomLeftCornerPosition.y - 1 ) != PuzzleBlockType.Open )
                    return; // TODO: Replace with 'Drop/Lock Blocks' later
            }

            // Begin navigating blocks to their new position
            int blockHeight = 2;
            if ( BlockSize == PuzzleBlockSize.ThreeTall ) blockHeight = 3;

            for( int y_ = 0; y_ < blockHeight; ++y_ )
            {
                for (int x_ = 0; x_ < blockWidth; ++x_)
                {
                    PuzzleBlockType tempBlock = GetBlockAtBoardPosition(BottomLeftCornerPosition.x + x_, BottomLeftCornerPosition.y + y_ );
                    SetBlockAtBoardPosition( BottomLeftCornerPosition.x + x_, BottomLeftCornerPosition.y + y_ - 1, tempBlock );

                    // Replace old position with Empty
                    SetBlockAtBoardPosition(BottomLeftCornerPosition.x + x_, BottomLeftCornerPosition.y + y_, PuzzleBlockType.Open );
                }
            }

            // Realign Y position with new position
            BottomLeftCornerPosition.y -= 1;
        }
    }

    void MoveActiveBlocks_Left()
    {
        // Ensure all blocks are one position from the left edge
        if (BottomLeftCornerPosition.x >= 1)
        {
            // Cross through the bottom active blocks and ensure there's nothing below them
            int blockHeight = 2;
            if (BlockSize == PuzzleBlockSize.ThreeTall) blockHeight = 3;

            // Confirm all blocks in left position are empty, otherwise exit out
            for (int i = 0; i < blockHeight; ++i)
            {
                if (GetBlockAtBoardPosition(BottomLeftCornerPosition.x - 1, BottomLeftCornerPosition.y + i) != PuzzleBlockType.Open)
                    return; // TODO: Replace with 'Drop/Lock Blocks' later
            }

            // Begin navigating blocks to their new position
            int blockWidth = 2;
            if (BlockSize == PuzzleBlockSize.ThreeWide) blockWidth = 3;

            for (int y_ = 0; y_ < blockHeight; ++y_)
            {
                for (int x_ = 0; x_ < blockWidth; ++x_)
                {
                    PuzzleBlockType tempBlock = GetBlockAtBoardPosition(BottomLeftCornerPosition.x + x_, BottomLeftCornerPosition.y + y_);
                    SetBlockAtBoardPosition(BottomLeftCornerPosition.x + x_ - 1, BottomLeftCornerPosition.y + y_, tempBlock);

                    // Replace old position with Empty
                    SetBlockAtBoardPosition(BottomLeftCornerPosition.x + x_, BottomLeftCornerPosition.y + y_, PuzzleBlockType.Open);
                }
            }

            // Realign Y position with new position
            BottomLeftCornerPosition.x -= 1;
        }
    }

    void MoveActiveBlocks_Right()
    {
        // Begin navigating blocks to their new position
        int numBlocksWide = 2;
        if (BlockSize == PuzzleBlockSize.ThreeWide) numBlocksWide = 3;

        // Ensure all blocks are one position from the left edge
        if ( BottomLeftCornerPosition.x + numBlocksWide < BoardWidth + 2 )
        {
            // Cross through the bottom active blocks and ensure there's nothing below them
            int blockHeight = 2;
            if (BlockSize == PuzzleBlockSize.ThreeTall) blockHeight = 3;

            // Confirm all blocks in right-side position are empty, otherwise exit out
            for (int i = 0; i < blockHeight; ++i)
            {
                if (GetBlockAtBoardPosition(BottomLeftCornerPosition.x + numBlocksWide + 1, BottomLeftCornerPosition.y + i) != PuzzleBlockType.Open)
                    return; // TODO: Replace with 'Drop/Lock Blocks' later
            }

            for (int y_ = 0; y_ < blockHeight; ++y_)
            {
                for (int x_ = numBlocksWide - 1; x_ >= 0; --x_)
                {
                    int xPos = BottomLeftCornerPosition.x + x_;
                    int yPos = BottomLeftCornerPosition.y + y_;

                    PuzzleBlockType tempBlock = GetBlockAtBoardPosition(xPos, yPos);
                    SetBlockAtBoardPosition(xPos + 1, yPos, tempBlock);

                    // Replace old position with Empty
                    SetBlockAtBoardPosition(xPos, yPos, PuzzleBlockType.Open);
                }
            }

            // Realign Y position with new position
            BottomLeftCornerPosition.x += 1;
        }
    }

    void RotateActiveBlocks_CounterClockwise()
    {
        // Temp X & Y
        int tempX = BottomLeftCornerPosition.x;
        int tempY = BottomLeftCornerPosition.y;

        // Store block in bottom left
        PuzzleBlockType spareBlock = GetBlockAtBoardPosition(tempX, tempY);

        // (0,0) <- (0,1)
        SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, ++tempY));

        // If three tall, (0,1) <- (0,2)
        if(BlockSize == PuzzleBlockSize.ThreeTall)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, ++tempY));

        // Shift left (0, 1/2) <- (1, 1/2)
        SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(++tempX, tempY));

        // If three wide, (1, 1) <- (2, 1)
        if(BlockSize == PuzzleBlockSize.ThreeWide)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(++tempX, tempY));

        // Shift Up (1/2, 0) <- (1/2, 1)
        SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, --tempY));

        // If three tall, shift up
        if(BlockSize == PuzzleBlockSize.ThreeTall)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, --tempY));
        // If Three Wide, shift right
        else if(BlockSize == PuzzleBlockSize.ThreeWide)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(--tempX, tempY));

        // Store temp block in final position
        SetBlockAtBoardPosition(tempX, tempY, spareBlock);
    }

    void RotateActiveBlocks_Clockwise()
    {
        // Temp X & Y
        int tempX = BottomLeftCornerPosition.x;
        int tempY = BottomLeftCornerPosition.y;

        // Store block in bottom left
        PuzzleBlockType spareBlock = GetBlockAtBoardPosition(tempX, tempY);

        // Shift left (0,0) <- (1,0)
        SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(++tempX, tempY));

        // If three wide, shift left
        if(BlockSize == PuzzleBlockSize.ThreeWide)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(++tempX, tempY));

        // Shift down (1, 0) <- (1, 1)
        SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, ++tempY));

        // If three tall, Shift down
        if (BlockSize == PuzzleBlockSize.ThreeTall)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, ++tempY));

        // Shift Right
        SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(--tempX, tempY));

        // If three wide, shift right
        if(BlockSize == PuzzleBlockSize.ThreeWide)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(--tempX, tempY));
        // If three tall, shift up
        else if (BlockSize == PuzzleBlockSize.ThreeTall)
            SetBlockAtBoardPosition(tempX, tempY, GetBlockAtBoardPosition(tempX, --tempY));

        // Store temp block in final position
        SetBlockAtBoardPosition(tempX, tempY, spareBlock);
    }

    void DropBlocks()
    {
        // Find Bottom
    }
    #endregion

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

    float testTimer = 0f;
    int counterX = 0;
    int counterY = 0;
    void UPDATE_TEST()
    {
        testTimer += Time.deltaTime;
        if (testTimer > 0.05f)
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
}
