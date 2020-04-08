using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCube))]
public class MarchingCubeEditor : Editor
{
	public float densityThreshold = 1;
    bool gizmoCube = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
	
	public override void OnInspectorGUI()
    {
        gizmoCube = EditorGUILayout.Toggle("辅助网格：", gizmoCube);
		densityThreshold = EditorGUILayout.Slider("密度阈值：",densityThreshold,0.001f, 1);
        
	}
    static Vector3[] gizmoLines = null;

    private void OnSceneGUI()
    {
        UnityEngine.Mesh mesh = null;
        int cellNum = 10;
        MarchingCube mc = target as MarchingCube;
        Dictionary<int, List<Vector3>> ss = new Dictionary<int, List<Vector3>>();
        if (mc.gameObject != null)
        {
            mesh = mc.gameObject.GetComponent<MeshFilter>().sharedMesh;

            Vector3[] verts = mesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 worldPos = mc.gameObject.transform.TransformPoint(verts[i]);
                Vector3 pos = worldPos;
                //check bound 
                if (pos.x < 0 || pos.y < 0 || pos.z < 0)
                {
                    continue;
                }

                int hash = mc.MakeHash(pos);
                if (ss.ContainsKey(hash))
                {
                    ss[hash].Add(pos);
                }
                else
                {
                    List<Vector3> ptlist = new List<Vector3>();
                    ptlist.Add(pos);
                    ss.Add(hash, ptlist);
                }
            }
        }

        gizmoLines = new Vector3[cellNum * cellNum * cellNum];
        int cnt = 0;

        BitMap3d bm3d = new BitMap3d(cellNum, cellNum, cellNum);
        for (int i = 0; i < cellNum; i++)
        {
            for (int j = 0; j < cellNum; j++)
            {
                for (int k = 0; k < cellNum; k++)
                {
                    Vector3 start = new Vector3(k, j, i);
                    int hash = mc.MakeHash(start);
                    float dens = 0;
                    if (ss.ContainsKey(hash))
                    {   //verts inside cellgrid, calculate density
                        //8 point in cube
                        Vector3 pt1 = start;
                        Vector3 pt2 = start + new Vector3(0,0,1);
                        Vector3 pt3 = start + new Vector3(0,1,0);
                        Vector3 pt4 = start + new Vector3(1,0,0);
                        Vector3 pt5 = start + new Vector3(0,1,1);
                        Vector3 pt6 = start + new Vector3(1,0,1);
                        Vector3 pt7 = start + new Vector3(1,1,0);
                        Vector3 pt8 = start + new Vector3(1,1,1);
                        Vector3[] cubePts = { pt1, pt2, pt3, pt4, pt5, pt6, pt7, pt8 };

                        List<Vector3> objverts = ss[hash]; //obj verts in cell
                        //for (int ii = 0; ii < 8; ii++)
                        {
                            float dd = 0;
                            foreach (var vert in objverts)
                            {
                                dd += Vector3.Magnitude(vert - start);
                            }
							dd = dd / objverts.Count; 
                            if (dd > 0 && dd < densityThreshold )
                            {
								Handles.color = new Color(dd, 0.5f, 0.0f);
								//Handles.DrawLine(vert, cubePts[ii]); 
                                Handles.SphereHandleCap(0, start, Quaternion.identity, 0.2f, EventType.Repaint);
                                Handles.Label(start, start.ToString());
                            }
                        }
                    }
                    else
                    {
                        dens = 0;
                    }

                    gizmoLines[cnt] = start;
                    Handles.color = new Color(0.3f, 0.5f, 0.5f);
                    Vector3 gizmoCubeCenter = start + Vector3.one * 0.5f;
                    if (gizmoCube)
                    {
                        Handles.DrawWireCube(gizmoCubeCenter, Vector3.one);
                    }
                   
                    cnt++;
                }
            }
        }
    }

}
