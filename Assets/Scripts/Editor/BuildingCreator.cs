using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingCreator : EditorWindow
{
    private GameObject root;
    private List<BeamData>[,] beamDatas;
    private int width = 6;
    private int height = 6;

    public class BeamData
    {
        public GameObject beam;
        public Vector3 rootPosition;
        public Vector3 rootDirection;
        public (int, int)[] connections;
    }

    [MenuItem("DemolitionMan/Building Creator")]
    public static void ShowWindow()
    {
        GetWindow<BuildingCreator>(false, "Building Creator", true);
    }

    private void OnGUI()
    {
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        if (GUILayout.Button("Generate"))
        {
            GenerateBuilding(width, height);
        }
    }

    private void GenerateBuilding(int width, int height)
    {
        Debug.Log("Generating!");
        //Setup root
        beamDatas = new List<BeamData>[width,height];
        DestroyImmediate(root);
        root = new GameObject($"BuildingRoot: Width {width}, Height {height}");

        CreateBeams(width, height);
        PrintBeams(beamDatas);
        JoinBeams(beamDatas);
    }

    private void PrintBeams(List<BeamData>[,] beamDatas)
    {
        foreach (var beamList in beamDatas)
        {
            foreach (var beam in beamList)
            {
                PrintBeam(beam);
            }
        }
    }

    private void PrintBeam(BeamData data)
    {
        Debug.Log($"Position: {data.rootPosition} Direction: {data.rootDirection} Connections: {data.connections}");
    }

    private void CreateBeams(int width, int height)
    {
        for (var w = 0; w < width; w++)
        {
            for (var h = 0; h < height; h++)
            {
                beamDatas[w, h] = new List<BeamData>();

                //Verticals
                var vertBeamRootPosition = new Vector3(w, h, 0);
                var vertBeamRootRotation = Vector3.up;
                var vertBeam = SpawnBeam(vertBeamRootPosition, vertBeamRootRotation);
                var vertBeamData = new BeamData
                {
                    rootPosition = vertBeamRootPosition,
                    rootDirection = vertBeamRootRotation,
                    beam = vertBeam,
                    connections = h + 1 < height ? new (int, int)[]{(w, h + 1), (w, h)} : new (int, int)[] { (w, h) }
                };
                beamDatas[w, h].Add(vertBeamData);
                
                //Horizontals
                if (w < width - 1)
                {
                    var hozBeamRootPosition = new Vector3(w, h, 0);
                    var hozBeamRootRotation = Vector3.right;
                    var hozBeam = SpawnBeam(hozBeamRootPosition, hozBeamRootRotation);
                    var hozBeamData = new BeamData
                    {
                        rootPosition = hozBeamRootPosition,
                        rootDirection = hozBeamRootRotation,
                        beam = hozBeam,
                        connections = new(int, int)[] { (w, h), (w + 1, h) }
                    };
                    beamDatas[w, h].Add(hozBeamData);
                } 
            }
        }
    }

    private void JoinBeams(List<BeamData>[,] beamDatas)
    {
        for (var w = 0; w < beamDatas.GetLength(0); w++)
        {
            for (var h = 0; h < beamDatas.GetLength(1); h++)
            {
                //This is awful and I'm sorry
                foreach (var beamData in beamDatas[w, h])
                {
                    var thisBeam = beamData.beam;
                    foreach (var connection in beamData.connections)
                    {
                        var beamsToConnectTo = beamDatas[connection.Item1, connection.Item2];
                        foreach (var beamToConnect in beamsToConnectTo)
                        {
                            if (beamToConnect.beam != thisBeam)
                            {
                                var joint = thisBeam.AddComponent<ConfigurableJoint>();
                                joint.xMotion = ConfigurableJointMotion.Locked;
                                joint.yMotion = ConfigurableJointMotion.Locked;
                                joint.zMotion = ConfigurableJointMotion.Locked;
                                joint.angularXMotion = ConfigurableJointMotion.Locked;
                                joint.angularYMotion = ConfigurableJointMotion.Locked;
                                joint.angularZMotion = ConfigurableJointMotion.Locked;
                                joint.projectionMode = JointProjectionMode.None;
                                joint.projectionDistance = 0.3f;
                                joint.projectionAngle = 5f;
                                joint.breakForce = 5000;
                                joint.breakTorque = 3000;
                                joint.connectedBody = beamToConnect.beam.GetComponent<Rigidbody>();
                                joint.enablePreprocessing = false;
                                //If we are connecting to things at our root
                                if (connection.Item1 == w && connection.Item2 == h)
                                {
                                    joint.anchor = Vector3.zero;
                                }
                                else
                                {
                                    joint.anchor = Vector3.forward;
                                }
                                joint.autoConfigureConnectedAnchor = true;
                            }
                        }
                    }
                }
            }
        }
    }

    private GameObject SpawnBeam(Vector3 rootPosition, Vector3 rootRotation)
    {
        var beam = Instantiate(Resources.Load("Beam")) as GameObject;
        beam.transform.position = new Vector3(rootPosition.x, rootPosition.y, rootPosition.z);
        beam.transform.rotation = Quaternion.LookRotation(rootRotation);
        beam.transform.parent = root.transform;
        beam.GetComponent<Rigidbody>().mass = 10;
        return beam;
    }
}