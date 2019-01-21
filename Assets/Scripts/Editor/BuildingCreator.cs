using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingCreator : EditorWindow
{
    private GameObject root;
    private List<BeamData>[,,] beamDatas;
    private int width = 6;
    private int height = 6;
    private int depth = 6;
    private float beamLength = 3;
    private int beamMass = 10;
    private float breakForce = 10000;
    private float breakTorque = 10000;
    private float spawnHeightOffset = 0.1f;
    private float linearDrag = 0.1f;
    private float angularDrag = 0.1f;

    private class BeamData
    {
        public GameObject beam;
        public Vector3 rootPosition;
        public Vector3 rootDirection;
        public (int, int, int)[] connections;
    }

    [MenuItem("DemolitionMan/Building Creator")]
    public static void ShowWindow()
    {
        GetWindow<BuildingCreator>(false, "Building Creator", true);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Building Dimentions");
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        depth = EditorGUILayout.IntField("Depth", depth);
        EditorGUILayout.LabelField("Beam Properties");
        beamLength = EditorGUILayout.FloatField("Beam Length", beamLength);
        beamMass = EditorGUILayout.IntField("Beam Mass", beamMass);
        breakForce = EditorGUILayout.FloatField("Break Force", breakForce);
        breakTorque = EditorGUILayout.FloatField("Break Torque", breakTorque);
        linearDrag = EditorGUILayout.FloatField("Linear Drag", linearDrag);
        angularDrag = EditorGUILayout.FloatField("Angular Drag", angularDrag);
        if (GUILayout.Button("Generate"))
        {
            GenerateBuilding(width, height, depth);
        }
    }

    private void GenerateBuilding(int width, int height, int depth)
    {
        Debug.Log("Generating!");
        //Setup root
        beamDatas = new List<BeamData>[width,height,depth];
        DestroyImmediate(root);
        root = new GameObject($"BuildingRoot: Width {width}, Height {height}");

        //Create beamDatas
        CreateBeams(width, height, depth);

        PrintBeams(beamDatas);
        JoinBeams(beamDatas);
    }

    private static void PrintBeams(List<BeamData>[,,] beamDatas)
    {
        foreach (var beamList in beamDatas)
        {
            foreach (var beam in beamList)
            {
                PrintBeam(beam);
            }
        }
    }

    private static void PrintBeam(BeamData data)
    {
        Debug.Log($"Position: {data.rootPosition} Direction: {data.rootDirection} Connections: {data.connections}");
    }

    private void CreateBeams(int width, int height, int depth)
    {
        for (var w = 0; w < width; w++)
        {
            for (var h = 0; h < height; h++)
            {
                for (var d = 0; d < depth; d++)
                {
                    beamDatas[w, h, d] = new List<BeamData>();

                    if (h < height - 1)
                    {
                        //Verticals
                        var vertBeamRootPosition = new Vector3(w * beamLength, h * beamLength + spawnHeightOffset,
                            d * beamLength);
                        var vertBeamRootRotation = Vector3.up;
                        var vertBeam = SpawnBeam(vertBeamRootPosition, vertBeamRootRotation);
                        var vertBeamData = new BeamData
                        {
                            rootPosition = vertBeamRootPosition,
                            rootDirection = vertBeamRootRotation,
                            beam = vertBeam,
                            connections = new(int, int, int)[] { (w, h, d), (w, h + 1, d) }
                        };
                        beamDatas[w, h, d].Add(vertBeamData);
                    }

                    //Horizontals x
                    if (w < width - 1)
                    {
                        var hozBeamRootPosition = new Vector3(w * beamLength, h * beamLength + spawnHeightOffset, d * beamLength);
                        var hozBeamRootRotation = Vector3.right;
                        var hozBeam = SpawnBeam(hozBeamRootPosition, hozBeamRootRotation);
                        var hozBeamData = new BeamData
                        {
                            rootPosition = hozBeamRootPosition,
                            rootDirection = hozBeamRootRotation,
                            beam = hozBeam,
                            connections = new (int, int, int)[] {(w, h, d), (w + 1, h, d)}
                        };
                        beamDatas[w, h, d].Add(hozBeamData);
                    }

                    //Horizontals z
                    if (d < depth - 1)
                    {
                        var hozBeamRootPosition = new Vector3(w * beamLength, h * beamLength + spawnHeightOffset, d * beamLength);
                        var hozBeamRootRotation = Vector3.forward;
                        var hozBeam = SpawnBeam(hozBeamRootPosition, hozBeamRootRotation);
                        var hozBeamData = new BeamData
                        {
                            rootPosition = hozBeamRootPosition,
                            rootDirection = hozBeamRootRotation,
                            beam = hozBeam,
                            connections = new(int, int, int)[] { (w, h, d), (w, h, d + 1) }
                        };
                        beamDatas[w, h, d].Add(hozBeamData);
                    }
                }
            }
        }
    }

    private void JoinBeams(List<BeamData>[,,] beamDatas)
    {
        for (var w = 0; w < beamDatas.GetLength(0); w++)
        {
            for (var h = 0; h < beamDatas.GetLength(1); h++)
            {
                for (var d = 0; d < beamDatas.GetLength(2); d++)
                {
                    //This is awful and I'm sorry
                    foreach (var beamData in beamDatas[w, h, d])
                    {
                        var thisBeam = beamData.beam;
                        foreach (var connection in beamData.connections)
                        {
                            var beamsToConnectTo = beamDatas[connection.Item1, connection.Item2, connection.Item3];
                            foreach (var beamToConnect in beamsToConnectTo)
                            {
                                if (beamToConnect.beam != thisBeam)
                                {
                                    var joint = thisBeam.AddComponent<ConfigurableJoint>();
                                    joint.xMotion = ConfigurableJointMotion.Limited;
                                    joint.yMotion = ConfigurableJointMotion.Limited;
                                    joint.zMotion = ConfigurableJointMotion.Limited;
                                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                                    joint.angularZMotion = ConfigurableJointMotion.Limited;

                                    var softLimit = new SoftJointLimitSpring
                                    {
                                        spring = 99999,
                                        damper = 0
                                    };
                                    joint.linearLimitSpring = softLimit;
                                    joint.angularXLimitSpring = softLimit;
                                    joint.angularYZLimitSpring = softLimit;

                                    joint.projectionMode = JointProjectionMode.None;
                                    joint.projectionDistance = 5f;
                                    joint.projectionAngle = 20f;
                                    joint.breakForce = breakForce;
                                    joint.breakTorque = breakTorque;
                                    joint.connectedBody = beamToConnect.beam.GetComponent<Rigidbody>();
                                    joint.enablePreprocessing = false;
                                    joint.enableCollision = false;
//                                    joint.massScale = 0.1f;
//                                    joint.connectedMassScale = 0.1f;
                                    //If we are connecting to things at our root
                                    if (connection.Item1 == w && connection.Item2 == h)
                                    {
                                        joint.anchor = Vector3.zero;
                                    }
                                    else
                                    {
                                        joint.anchor = Vector3.forward * beamLength;
                                    }
                                    joint.autoConfigureConnectedAnchor = true;
                                }
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
        var rb = beam.GetComponent<Rigidbody>();
        rb.mass = beamMass;
        rb.drag = linearDrag;
        rb.angularDrag = angularDrag;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        return beam;
    }
}
