using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal class PathfindingBlock
{
    internal Vector2Int BoardLocation;
    internal Direction NextDirection;

    internal PathfindingBlock()
    {
        BoardLocation = new Vector2Int();
        NextDirection = Direction.Down;
    }

    internal PathfindingBlock(Vector2Int BoardLocation_ , Direction NextDirection_ )
    {
        BoardLocation = BoardLocation_;
        NextDirection = NextDirection_;
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

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
    int BoardWidth = 8;
    int BoardHeight = 9;
    int xPosCenter_LeftSide;
    int BoardEdge_Horiz_Left;
    int BoardEdge_Horiz_Right;
    List<PuzzleBlockType> PuzzleBlockArray;
    #endregion

    #region Current Block Information
    Vector2Int BottomLeftCornerPosition;
    [SerializeField] PuzzleBlockSize BlockSize;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        xPosCenter_LeftSide = ((BoardWidth + 2) / 2) - 1; // +2 for sidebar, /2 for middle, -1 for center adjust (Works even/odd board width)
        BoardEdge_Horiz_Left = 1;
        BoardEdge_Horiz_Right = BoardWidth + 1;

        Init_PuzzleBlockArray();

        PrintBoardToConsole();
    }

    // Update is called once per frame
    void Update()
    {
        // UPDATE_TEST();
        if( Input.GetKeyDown(KeyCode.A))
        {
            MoveActiveBlocks_Left();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            MoveActiveBlocks_Right();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveActiveBlocks_Down();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateActiveBlocks_CounterClockwise();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateActiveBlocks_Clockwise();
            PrintBoardToConsole();
            print("--------------------------------------");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
            DropAndLockBlocks();

        if (Input.GetKeyDown(KeyCode.Keypad1))
            TEST_Board_1();
        if (Input.GetKeyDown(KeyCode.Keypad2))
            TEST_Board_2();
    }

    void Init_PuzzleBlockArray()
    {
        PuzzleBlockArray = new List<PuzzleBlockType>();

        for (int i = 0; i < (BoardWidth + 2) * BoardHeight; ++i)
            PuzzleBlockArray.Add(PuzzleBlockType.Open);

        SpawnNewBlock();

        // TEST
        // SetBlockAtBoardPosition(BoardWidth / 2, 3, PuzzleBlockType.Closed);
        // SetBlockAtBoardPosition((BoardWidth / 2) + 1, 3, PuzzleBlockType.Closed);
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
        print("NEW BLOCK SPAWNED");

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

    PuzzleBlockType GetBlockAtBoardPosition( Vector2Int pos_ )
    {
        return GetBlockAtBoardPosition(pos_.x, pos_.y);
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
                {
                    DropAndLockBlocks();
                    return;
                }
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
                if (GetBlockAtBoardPosition(BottomLeftCornerPosition.x + numBlocksWide, BottomLeftCornerPosition.y + i) != PuzzleBlockType.Open)
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

    void DropAndLockBlocks()
    {
        // For each row, starting with 1 and going to (width + 1)
        for ( int x = 0; x < BoardWidth + 2; ++x )
        {
            // Store if we had to shift a block down this column
            bool shiftedBlocksDown = false;

            // For each column, starting with the bottom and moving up
            for ( int y = 0; y < BoardHeight; ++y )
            {
                // Far left and right edges are cleared
                if ( x == 0 || x == BoardWidth + 1)
                {
                    SetBlockAtBoardPosition(x, y, PuzzleBlockType.Open);
                }
                // Internal blocks are shifted and locked
                else
                {
                    // Store block at current position for evaluation
                    PuzzleBlockType tempBlock = GetBlockAtBoardPosition( x, y );

                    // If the current position is open and valid, continue
                    if( tempBlock == PuzzleBlockType.Open && ( ( y + 1 ) < BoardHeight ) )
                    {
                        // We only want to attempt to shift the block down if the above position is valid
                        PuzzleBlockType aboveTempBlock = GetBlockAtBoardPosition( x, y + 1 );

                        // Only shift a block if not open or closed (X or O types only)
                        if ( aboveTempBlock != PuzzleBlockType.Open && aboveTempBlock != PuzzleBlockType.Closed )
                        {
                            // Replace with the new 'locked' type
                            if (aboveTempBlock == PuzzleBlockType.Block_O_Active) aboveTempBlock = PuzzleBlockType.Block_O;
                            else if(aboveTempBlock == PuzzleBlockType.Block_X_Active) aboveTempBlock = PuzzleBlockType.Block_X;

                            SetBlockAtBoardPosition( x, y, aboveTempBlock);
                            SetBlockAtBoardPosition( x, y + 1, PuzzleBlockType.Open );

                            shiftedBlocksDown = true;
                        }
                    }
                }
            }

            // Repeat this column
            if (shiftedBlocksDown) --x;
        }

        // TODO: Begin pathfinding
        // StartCoroutine(BlocksPathfinding());
        BlocksPathfinding();

        // Spawn new block and continue
        SpawnNewBlock();

        PrintBoardToConsole();
    }
    #endregion

    List<int> yPositions;
    void BlocksPathfinding()
    {
        // TODO: Remove print checks
        bool print_stage_1 = false;
        bool print_stage_2 = false;
        bool print_stage_3 = true;

        print("---------------------");
        print("BEGIN PATHFINDING");
        print("---------------------");
        bool continuePathfinding = false;

        #region CONFIRM: Center columns have same block type
        
        yPositions = new List<int>();

        bool BlockTypeFound_X = false;
        bool BlockTypeFound_O = false;

        List<PathfindingBlock> BlockList_X_Master_LeftSection;
        List<PathfindingBlock> BlockList_X_Master_RightSection;
        List<PathfindingBlock> BlockList_O_Master_LeftSection;
        List<PathfindingBlock> BlockList_O_Master_RightSection;

        if(print_stage_1) print("X: " + (xPosCenter_LeftSide) + ", " + (xPosCenter_LeftSide + 1));

        // CONFIRM: Block X/O exists in (X, Y) & (X + 1, Y)
        // RECORD ABOVE POSSIBILITIES
        for (int i = 0; i < BoardHeight; ++i)
        {
            PuzzleBlockType xBlock_Left = GetBlockAtBoardPosition(xPosCenter_LeftSide, i);
            if (xBlock_Left == PuzzleBlockType.Closed || xBlock_Left == PuzzleBlockType.Open) continue;

            PuzzleBlockType xBlock_Right = GetBlockAtBoardPosition(xPosCenter_LeftSide + 1, i);
            if (xBlock_Right == PuzzleBlockType.Closed || xBlock_Right == PuzzleBlockType.Open) continue;

            if (print_stage_1) print("Y: " + xBlock_Left + ", " + xBlock_Right);

            if (xBlock_Left == xBlock_Right)
            {
                yPositions.Add(i);

                if (xBlock_Left == PuzzleBlockType.Block_O) BlockTypeFound_O = true;
                else BlockTypeFound_X = true;

                // We already know enough that further pathfinding is needed
                continuePathfinding = true;
            }
        }

        string yPos = "";
        for (int i = 0; i < yPositions.Count; ++i) yPos += (yPositions[i] + " ");
        if (print_stage_1)
        {
            print("Y Positions: " + yPos);
            print("X Found: " + BlockTypeFound_X);
            print("O Found: " + BlockTypeFound_O);
        }
        #endregion

        #region CONFIRM: Block of given types exist in every column

        // No need to continue if nothing found
        if (continuePathfinding)
        {
            // Resetting check
            continuePathfinding = false;

            bool searchingFor_O = BlockTypeFound_O;
            bool searchingFor_X = BlockTypeFound_X;

            // for( int x_ = 1; x_ < BoardWidth + 1; ++x_ )
            for (int x_ = 1; x_ < BoardWidth + 1; ++x_)
            {
                // Skip if center columns
                if (x_ == xPosCenter_LeftSide || x_ == xPosCenter_LeftSide + 1) continue;

                // bool canContinue = false;
                bool foundThisColumn_O = false;
                bool foundThisColumn_X = false;

                for( int y_ = 0; y_ < BoardHeight; ++y_ )
                {
                    if(print_stage_2) print("Testing X/Y: (" + x_ + "," + y_ + ")");
                    
                    PuzzleBlockType currBlock = GetBlockAtBoardPosition(x_, y_);

                    if ( currBlock == PuzzleBlockType.Closed ) continue;

                    if( searchingFor_O && !foundThisColumn_O && currBlock == PuzzleBlockType.Block_O )
                    {
                        if (print_stage_2) print("Found O: " + y_);
                        foundThisColumn_O = true;
                    }

                    if( searchingFor_X && !foundThisColumn_X && currBlock == PuzzleBlockType.Block_X )
                    {
                        if (print_stage_2) print("Found X: " + y_);
                        foundThisColumn_X = true;
                    }

                    // Attempting a XOR operator. Left and right conditions must match and result in true.
                    if( !(searchingFor_X ^ foundThisColumn_X) && !(searchingFor_O ^ foundThisColumn_O) )
                    {
                        if (print_stage_2) print("Column " + x_ + " passed:" );
                        if (print_stage_2) print("Finding X: " + foundThisColumn_X + ", Finding O: " + foundThisColumn_O);

                        y_ = BoardHeight - 1;

                        // If we've reached the far right edge and confirmed all possible blocks in each position, continue
                        if(x_ == BoardWidth)
                        {
                            continuePathfinding = true;
                            continue;
                        }
                    }

                    if (print_stage_2) print("Curr Y: " + y_);

                    // If we've reached the top of the column...
                    if ( y_ == (BoardHeight - 1) )
                    {
                        if (print_stage_2) print( "Reached top of column" );

                        // If we haven't found either X or O, move X to the far end to forcibly end
                        if( !foundThisColumn_X && !foundThisColumn_O)
                        {
                            if (print_stage_2) print( "Check failed - Forcibly ending pathfinding" );

                            x_ = BoardWidth;

                            continue;
                        }
                        // If we've found one, turn off checks that are necessary
                        else if( foundThisColumn_X || foundThisColumn_O )
                        {
                            if ( !foundThisColumn_X ) searchingFor_X = false;
                            if ( !foundThisColumn_O ) searchingFor_O = false;
                            if (print_stage_2) print( "Turning off necessary checks - X: " + searchingFor_X + ", O: " + searchingFor_O );
                        }
                    }
                }
            }
        }
        #endregion

        #region BEGIN: Pathfinding check
        if (continuePathfinding)
        {
            if(print_stage_3) print("Pathfinding - STARTING FINAL PHASE");

            BlockList_X_Master_LeftSection = new List<PathfindingBlock>();
            BlockList_X_Master_RightSection = new List<PathfindingBlock>();
            BlockList_O_Master_LeftSection = new List<PathfindingBlock>();
            BlockList_O_Master_RightSection = new List<PathfindingBlock>();

            // THOUGHT PROCESS:
            // Start in center, pathfind outward, since we already know potential paths
            foreach (var thisY in yPositions) // Oddly, this only needs to return the relevant Y coordinates
            {
                // Get the block type at this Y position for consideration
                PuzzleBlockType tempBlock = GetBlockAtBoardPosition(xPosCenter_LeftSide, thisY);

                // Left half, begin pathing left toward left edge
                Vector2Int tempV2Int_Left = new Vector2Int(xPosCenter_LeftSide, thisY);
                Vector2Int tempV2Int_Right = new Vector2Int(xPosCenter_LeftSide + 1, thisY);

                // Pre-load each side of the midpoint with this block type, AND give a direction
                if (tempBlock == PuzzleBlockType.Block_X)
                {
                    BlockList_X_Master_LeftSection.Add(new PathfindingBlock(tempV2Int_Left, Direction.Left));
                    BlockList_X_Master_RightSection.Add(new PathfindingBlock(tempV2Int_Right, Direction.Right));
                }
                else
                {
                    BlockList_O_Master_LeftSection.Add(new PathfindingBlock(tempV2Int_Left, Direction.Left));
                    BlockList_O_Master_RightSection.Add(new PathfindingBlock(tempV2Int_Right, Direction.Right));
                }
            }

            List<IEnumerator> PathfindingCoRoutines = new List<IEnumerator>();

            // Add a series of paths for each valid possibility
            foreach (PathfindingBlock path in BlockList_O_Master_LeftSection)
                PathfindingCoRoutines.Add(Thread_Pathfinding(path));

            foreach (PathfindingBlock path in BlockList_O_Master_RightSection)
                PathfindingCoRoutines.Add(Thread_Pathfinding(path));

            foreach (PathfindingBlock path in BlockList_X_Master_LeftSection)
                PathfindingCoRoutines.Add(Thread_Pathfinding(path));

            foreach (PathfindingBlock path in BlockList_X_Master_RightSection)
                PathfindingCoRoutines.Add(Thread_Pathfinding(path));

            // Start
            foreach (IEnumerator path in PathfindingCoRoutines)
                StartCoroutine(path);

            /*
        foreach( PathfindingBlock path in BlockList_O_Master_LeftSection )
            StartCoroutine(Thread_Pathfinding(path));

        foreach (PathfindingBlock path in BlockList_O_Master_RightSection)
            StartCoroutine(Thread_Pathfinding(path));

        foreach (PathfindingBlock path in BlockList_X_Master_LeftSection)
            StartCoroutine(Thread_Pathfinding(path));

        foreach (PathfindingBlock path in BlockList_X_Master_RightSection)
            StartCoroutine(Thread_Pathfinding(path));
        */

            // Start on left side, bottom of first column

            // Find first block of X/O

            // Confirm all columns contain at least one of that

            // Begin two threads, one from left half, center position. Moves left.
            // Second thread from right half, center position. Moves right.

            // For each thread: If blocks split, create new list with copied path. Current goes left, new goes up and/or down.

            // If block cannot progress, end thread (instead of backtrack).
        }
        #endregion

        print("---------------------");
        print("END PATHFINDING");
        print("---------------------");

        // yield return null;
    }

    IEnumerator Thread_Pathfinding( PathfindingBlock thisBlock_ )
    {
        bool FoundEnd = false;

        List<PathfindingBlock> tempList = new List<PathfindingBlock>();
        tempList.Add(thisBlock_);

        PuzzleBlockType tempBlock = GetBlockAtBoardPosition(thisBlock_.BoardLocation);

        // PATH: Desired direction, down, up, opposite direction
        while( !FoundEnd )
        {
            Direction thisQuadrant = Direction.Left;
            if(thisBlock_.BoardLocation.x >= (xPosCenter_LeftSide + 1))
                thisQuadrant = Direction.Right;

            #region Check possible routes. Left/Right, Down, Up, Right/Left
            bool canGoLeft = false;
            bool canGoDown = false;
            bool canGoUp = false;
            bool canGoRight = false;
            Vector2Int thisPos = thisBlock_.BoardLocation;
            Direction nextDir = thisBlock_.NextDirection;

            // TODO: ENSURE BLOCK CHECKED DOES NOT EXIST IN LIST

            // Check Left (Right)
            int tempX = thisPos.x - 1;
            if (thisQuadrant == Direction.Right) tempX = thisPos.x + 1;

            PuzzleBlockType nextBlockType = GetBlockAtBoardPosition(tempX, thisPos.y);
            if (nextBlockType == tempBlock)
            {
                // Since we're progressing in direction, if block is on far left (right) edge, success!
                if(thisPos.x == BoardEdge_Horiz_Left || thisPos.x == BoardEdge_Horiz_Right)
                {
                    // Add final block to list

                    // Break out
                }
                else
                {
                    // Compare against list for safety
                    if(PreviousBlocksSafe(tempX, thisPos.y, tempList))
                    {
                        if (thisQuadrant == Direction.Left) canGoLeft = true;
                        else canGoRight = true;
                    }
                }
            }

            // Check Down
            int tempY = thisPos.y - 1;
            if(tempY >= 0)
            {
                nextBlockType = GetBlockAtBoardPosition(thisPos.x, tempY);

                if (nextBlockType == tempBlock)
                {
                    // Compare against list for safety
                    if (PreviousBlocksSafe(tempX, thisPos.y, tempList))
                    {
                        canGoDown = true;
                    }
                }
            }

            // Check Up
            tempY = thisPos.y + 1;
            if(tempY <= BoardHeight - 1)
            {
                nextBlockType = GetBlockAtBoardPosition(thisPos.x, tempY);

                if (nextBlockType == tempBlock)
                {
                    // Compare against list for safety
                    if (PreviousBlocksSafe(tempX, thisPos.y, tempList))
                    {
                        canGoUp = true;
                    }
                }
            }

            // Check Right (Left)
            tempX = thisPos.x + 1;
            if (thisQuadrant == Direction.Right) tempX = thisPos.x - 1;

            // Can't be center columns (YET)
            if(tempX != xPosCenter_LeftSide && tempX != xPosCenter_LeftSide + 1)
            {
                nextBlockType = GetBlockAtBoardPosition(tempX, thisPos.y);

                if(nextBlockType == tempBlock)
                {
                    // Compare against list for safety
                    if (PreviousBlocksSafe(tempX, thisPos.y, tempList))
                    {
                        if (thisQuadrant == Direction.Left) canGoRight = true;
                        else canGoLeft = true;
                    }
                }
            }
            #endregion

            print("(Quad: " + thisQuadrant + ", " + tempBlock + ")");
            print("LEFT: " + canGoLeft);
            print("DOWN: " + canGoDown);
            print("UP: " + canGoUp);
            print("RIGHT: " + canGoRight);


            FoundEnd = true;

            // If multiple exist, run additional threads with new info
        }

        yield return FoundEnd;
    }


    bool PreviousBlocksSafe(Vector2Int nextPos_, List<PathfindingBlock> testList_)
    {
        bool isSafe = true;

        // If the previou
        if (testList_[testList_.Count - 1].BoardLocation == nextPos_) isSafe = false;

        if(testList_.Count >= 4)
            for(int i = 3; i < testList_.Count - 1; ++i)
                if (testList_[i].BoardLocation == nextPos_) isSafe = false;

        return isSafe;
    }
    bool PreviousBlocksSafe(int x_, int y_, List<PathfindingBlock> testList_)
    {
        Vector2Int tempV2Int = new Vector2Int(x_, y_);

        return PreviousBlocksSafe(tempV2Int, testList_);
    }

    void PrintBoardToConsole()
    {
        // Ran in reverse vertically for sake of console printing
        for (int y = BoardHeight - 1; y >= 0; --y)
        {
            string lineString = "";

            for( int x = 0; x < BoardWidth + 2; ++x )
            {
                string stringTemp = "" + GetBlockAtBoardPosition(x, y);

                PuzzleBlockType temp = GetBlockAtBoardPosition(x, y);
                if( temp == PuzzleBlockType.Block_O_Active ) stringTemp = "A__O";
                if( temp == PuzzleBlockType.Block_X_Active ) stringTemp = "A__X";

                lineString += "[ " + stringTemp + " ]";
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

    void TEST_Board_1()
    {
        ClearBoard();

        for( int i = 1; i < BoardWidth + 1; ++i )
        {
            SetBlockAtBoardPosition(i, 0, PuzzleBlockType.Block_O);
        }

        DropAndLockBlocks();
    }

    void TEST_Board_2()
    {
        ClearBoard();

        for( int y = 0; y < BoardHeight; ++y)
        {
            for (int x = 1; x < BoardWidth + 1; ++x)
            {
                if( y < 4 )
                {
                    if (x < (((BoardWidth + 2) / 2) - 1) || x > (((BoardWidth + 2) / 2) + 1))
                    {
                        SetBlockAtBoardPosition(x, y, PuzzleBlockType.Block_O);
                    }
                    else
                    {
                        SetBlockAtBoardPosition(x, y, PuzzleBlockType.Block_X);
                    }
                }
                else if (y == 4)
                {
                    if( x >= ((BoardWidth / 2) - 1) || x < ((BoardWidth / 2) + 1) )
                    {
                        SetBlockAtBoardPosition(x, y, PuzzleBlockType.Block_O);
                    }
                }
            }
        }
        

        DropAndLockBlocks();
    }

    void ClearBoard()
    {
        PuzzleBlockType tempBlock;

        for (int y = 0; y < BoardHeight; ++y)
        {
            for (int x = 0; x < BoardWidth + 2; ++x)
            {
                tempBlock = GetBlockAtBoardPosition(x, y);

                if (tempBlock == PuzzleBlockType.Block_O_Active || tempBlock == PuzzleBlockType.Block_X_Active)
                    continue;

                SetBlockAtBoardPosition(x, y, PuzzleBlockType.Open);
            }
        }
    }
}
