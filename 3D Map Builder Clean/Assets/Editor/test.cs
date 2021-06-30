using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using UtilitiesGenetic;

public class test : EditorWindow
{
    private MapTileGridCreatorWindow mapWindow;

    private int numberTypes = 5;
    private int numberIterations = 1000;
    private UnityEngine.Vector3Int sizeGrid = new UnityEngine.Vector3Int(12,8,10);
    private System.Random rand;
    private SharpNeatLib.Maths.FastRandom fastRand;

    private int[][][] gridInt;
    private int[,,] gridArrayInt;
    private int[][][] gridCopy;

    private TypeParams[] typeTable;
    private TypeParams[][][] gridType;

    [MenuItem("Example/Experiment")]
    static void Init()
    {
        EditorWindow editorWindow = GetWindow(typeof(test));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
    }

    public void OnEnable()
    {

    }

    public void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        numberTypes = EditorGUILayout.IntField("SizePallet", numberTypes);
        sizeGrid = EditorGUILayout.Vector3IntField("SizeGrid", sizeGrid);
        numberIterations = EditorGUILayout.IntField("Number Iterations", numberIterations);

        if (GUILayout.Button("Experiment Access Data"))
        {
            SetExperiment();
            ExperimentAccessData();
        }

        if (GUILayout.Button("Experiment Set Type"))
        {
            SetExperiment();
            ExperimentSetType();
        }

        if (GUILayout.Button("Experiment size.x VS sizeX"))
        {
            SetExperiment();
            ExperimentBetterLoops();
        }

        if (GUILayout.Button("Experiment System.rand vs FastRandom"))
        {
            SetExperiment();
            ExperimentRand();
        }

        if (GUILayout.Button("Experiment Copy"))
        {
            SetExperiment();
            ExperimentCopy();
        }

        if (GUILayout.Button("Experiment Jagged VS Array"))
        {
            SetExperiment();
            ExperimentJagged();
        }
    }

    private void ExperimentAccessData()
    {
        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeGrid.x + 2; x++)
            {
                for (int y = 0; y < sizeGrid.y + 2; y++)
                {
                    for (int z = 0; z < sizeGrid.z + 2; z++)
                    {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                        int u;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
                        if (gridType[2][2][2].blockPath)
                            u = 5;
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Grid TypeParams " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeGrid.x + 2; x++)
            {
                for (int y = 0; y < sizeGrid.y + 2; y++)
                {
                    for (int z = 0; z < sizeGrid.z + 2; z++)
                    {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                        int u;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
                        if (typeTable[gridInt[2][2][2]].blockPath)
                            u = 5;
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Grid Ints " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }

    private void ExperimentSetType()
    {
        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeGrid.x + 2; x++)
            {
                for (int y = 0; y < sizeGrid.y + 2; y++)
                {
                    for (int z = 0; z < sizeGrid.z + 2; z++)
                    {
                        gridType[2][2][2] = typeTable[rand.Next(numberTypes)];
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Grid TypeParams " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeGrid.x + 2; x++)
            {
                for (int y = 0; y < sizeGrid.y + 2; y++)
                {
                    for (int z = 0; z < sizeGrid.z + 2; z++)
                    {
                        gridInt[2][2][2] = rand.Next(numberTypes);
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Grid Ints " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }

    private void ExperimentBetterLoops()
    {
        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeGrid.x + 2; x++)
            {
                for (int y = 0; y < sizeGrid.y + 2; y++)
                {
                    for (int z = 0; z < sizeGrid.z + 2; z++)
                    {
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run size.x " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
        stopWatch.Start();

        int sizeX = sizeGrid.x;
        int sizeY = sizeGrid.y;
        int sizeZ = sizeGrid.z;

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    for (int z = 0; z < sizeZ + 2; z++)
                    {
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To sizeX " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }

    private void ExperimentRand()
    {
        int sizeX = sizeGrid.x;
        int sizeY = sizeGrid.y;
        int sizeZ = sizeGrid.z;

        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    for (int z = 0; z < sizeZ + 2; z++)
                    {
                        rand.NextDouble();
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run System.rand " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    for (int z = 0; z < sizeZ + 2; z++)
                    {
                        fastRand.NextDouble();
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run FastRandom " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }

    private void ExperimentCopy()
    {
        int sizeX = sizeGrid.x;
        int sizeY = sizeGrid.y;
        int sizeZ = sizeGrid.z;

        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    for (int z = 0; z < sizeZ + 2; z++)
                    {
                        gridCopy[x][y][z] = gridInt[x][y][z];
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run ForLoop " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            gridCopy = Copy(gridInt);
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Copy " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }

    public static int[][][] Copy(int[][][] source)
    {
        int[][][] dest = new int[source.Length][][];
        for (int x = 0; x < source.Length; x++)
        {
            int[][] s = new int[source[x].Length][];
            for (int y = 0; y < source[x].Length; y++)
            {
                int[] n = new int[source[x][y].Length];
                int length = source[x][y].Length * sizeof(int);
                Buffer.BlockCopy(source[x][y], 0, n, 0, length);
                s[y] = n;
            }
            dest[x] = s;
        }
        return dest;
    }

    private void ExperimentJagged()
    {
        int sizeX = sizeGrid.x;
        int sizeY = sizeGrid.y;
        int sizeZ = sizeGrid.z;

        Stopwatch stopWatch;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    for (int z = 0; z < sizeZ + 2; z++)
                    {
                        gridArrayInt[x, y, z] = 5;
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Array " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
        stopWatch.Start();

        for (int i = 1; i < numberIterations; i++)
        {
            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    for (int z = 0; z < sizeZ + 2; z++)
                    {
                        gridInt[x][y][z] = 5;
                    }
                }
            }
        }

        stopWatch.Stop();
        UnityEngine.Debug.Log("Time To Run Jagged " + stopWatch.ElapsedMilliseconds + "ms");
        stopWatch.Reset();
    }
    private void SetExperiment()
    {
        gridInt = new int[sizeGrid.x + 2][][];
        gridArrayInt = new int[sizeGrid.x + 2, sizeGrid.y + 2, sizeGrid.z + 2];
        gridCopy = new int[sizeGrid.x + 2][][];
        gridType = new TypeParams[sizeGrid.x + 2][][];
        typeTable = new TypeParams[numberTypes + 1];
        rand = new System.Random();
        fastRand = new SharpNeatLib.Maths.FastRandom();

        typeTable[0] = new TypeParams();
        typeTable[0].ground = false;
        typeTable[0].blockPath = false;
        typeTable[0].wall = false;
        typeTable[0].floor = false;

        for (int i = 1; i < numberTypes + 1; i++)
        {
            typeTable[i] = new TypeParams();
            typeTable[i].ground = rand.Next(2) == 0 ? true : false;
            typeTable[i].blockPath = rand.Next(2) == 0 ? true : false;
            typeTable[i].wall = rand.Next(2) == 0 ? true : false;
            typeTable[i].floor = rand.Next(2) == 0 ? true : false;
        }

        for (int x = 0; x < sizeGrid.x + 2; x++)
        {
            int[][] gridIntYZ = new int[sizeGrid.y + 2][];
            int[][] gridCopyYZ = new int[sizeGrid.y + 2][];

            TypeParams[][] gridTypeYZ = new TypeParams[sizeGrid.y + 2][];
            for (int y = 0; y < sizeGrid.y + 2; y++)
            {
                int[] gridIntZ = new int[sizeGrid.z + 2];
                int[] gridCopyZ = new int[sizeGrid.z + 2];
                TypeParams[] gridTypeZ = new TypeParams[sizeGrid.z + 2];
                for (int z = 0; z < sizeGrid.z + 2; z++)
                {
                    //Genes are bigger than cluster to have empty borders thus it is easier to get neighbors  
                    if (x == 0 || y == 0 || z == 0 || x == sizeGrid.x + 1 || y == sizeGrid.y + 1 || z == sizeGrid.z + 1)
                    {
                        gridIntZ[z] = 0;
                        gridCopyZ[z] = 0;
                        gridArrayInt[x, y, z] = 0;

                        gridTypeZ[z] = new TypeParams();
                        gridTypeZ[z].ground = false;
                        gridTypeZ[z].blockPath = false;
                        gridTypeZ[z].wall = false;
                        gridTypeZ[z].floor = false;
                    }
                    else
                    {
                        gridIntZ[z] = rand.Next(numberTypes);
                        gridCopyZ[z] = rand.Next(numberTypes);
                        gridArrayInt[x, y, z] = rand.Next(numberTypes);

                        gridTypeZ[z] = new TypeParams();
                        gridTypeZ[z].ground = rand.Next(2) == 0 ? true : false;
                        gridTypeZ[z].blockPath = rand.Next(2) == 0 ? true : false;
                        gridTypeZ[z].wall = rand.Next(2) == 0 ? true : false;
                        gridTypeZ[z].floor = rand.Next(2) == 0 ? true : false;
                    }
                }
                gridIntYZ[y] = gridIntZ;
                gridCopyYZ[y] = gridCopyZ;
                gridTypeYZ[y] = gridTypeZ;
            }
            gridInt[x] = gridIntYZ;
            gridCopy[x] = gridCopyYZ;
            gridType[x] = gridTypeYZ;
        }
    }
}