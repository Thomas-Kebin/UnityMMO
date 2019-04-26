using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using XLua;

namespace UnityMMO
{
    
[Hotfix]
[LuaCallCSharp]
public class SceneMgr : MonoBehaviour
{
	public static SceneMgr Instance;
    GameWorld m_GameWorld;
    // public Transform container;
	// EntityManager entityManager;
    // public EntityArchetype RoleArchetype;
    // public EntityArchetype MonsterArchetype;
    // public EntityArchetype NPCArchetype;
    Dictionary<long, Entity> entityDic;
    SceneInfo curSceneInfo;
    Entity mainRole;
    public SceneDetectorBase detector;
    private SceneObjectLoadController m_Controller;
    const string SceneInfoPath = "Assets/AssetBundleRes/scene/";

    // GameObject mainRolePrefab;
    // GameObject rolePrefab;
    private bool isLoadingScene = false;
    bool isBaseWorldLoadOk = false;
    public LayerMask groundLayer = 1 << 0;
    float lastCheckMainRolePosTime = 0;

    public EntityManager EntityManager { get => m_GameWorld.GetEntityManager();}
    public bool IsLoadingScene { get => isLoadingScene; set => isLoadingScene = value; }
    public CinemachineFreeLook FreeLookCamera { get => freeLookCamera; set => freeLookCamera = value; }
    public Transform MainCameraTrans { get => mainCameraTrans; }
    public Transform FreeLookCameraTrans { get => freeLookCameraTrans; }
    public SceneInfo CurSceneInfo { get => curSceneInfo; }

    Cinemachine.CinemachineFreeLook freeLookCamera;
    Transform freeLookCameraTrans;
    Transform mainCameraTrans;

    void Awake()
	{
		Instance = this; // worst singleton ever but it works
        entityDic = new Dictionary<long, Entity>();
        var mainCamera = GameObject.Find("MainCamera");
        mainCameraTrans = mainCamera.transform;
        var camera = GameObject.Find("FreeLookCamera");
        if (camera != null)
        {
            freeLookCameraTrans = camera.transform;
            FreeLookCamera = camera.GetComponent<Cinemachine.CinemachineFreeLook>();
        }
	}

    void Update()
    {
        // Debug.Log("detector : "+(detector!=null).ToString()+" cont : "+(m_Controller != null).ToString());
        if (detector != null && m_Controller != null)
            m_Controller.RefreshDetector(detector);
        
        // CheckMainRolePos();
    }

    public void CheckMainRolePos()
    {
        if (Time.time - lastCheckMainRolePosTime < 3)
            return;
        var mainRole = RoleMgr.GetInstance().GetMainRole();
        if (!isBaseWorldLoadOk || mainRole == null || curSceneInfo==null)
            return;
        
        lastCheckMainRolePosTime = Time.time;

        Vector3 curPos = mainRole.transform.localPosition;
        bool isInScene = curSceneInfo.Bounds.Contains(curPos);
        if (!isInScene)
            CorrectMainRolePos();
    }

	public void Init(GameWorld world)
	{
        m_GameWorld = world;

        ResMgr.GetInstance().Init();
        RoleMgr.GetInstance().Init(world);
        MonsterMgr.GetInstance().Init(world);
	}

	public void OnDestroy()
	{
		Instance = null;
	}

    private static string Repalce(string str)
    {
        str = System.Text.RegularExpressions.Regex.Replace(str, @"<", "lt;");
        str = System.Text.RegularExpressions.Regex.Replace(str, @">", "gt;");
        // str = System.Text.RegularExpressions.Regex.Replace(str, @"\\", "quot;");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\r", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\n", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\/", "/");
        return str;
    }

    void LoadSceneInfo(int scene_id, Action<int> action)
    {
        //load scene info from json file(which export from SceneInfoExporter.cs)
        XLuaFramework.ResourceManager.GetInstance().LoadAsset<TextAsset>(SceneInfoPath+"scene_"+scene_id.ToString() +"/scene_info.json", delegate(UnityEngine.Object[] objs) {
            TextAsset txt = objs[0] as TextAsset;
            string scene_json = txt.text;
            scene_json = Repalce(scene_json);
            SceneInfo scene_info = JsonUtility.FromJson<SceneInfo>(scene_json);
            curSceneInfo = scene_info;
            ApplyLightInfo(scene_info);
            
            m_Controller = gameObject.GetComponent<SceneObjectLoadController>();
            if (m_Controller == null)
                m_Controller = gameObject.AddComponent<SceneObjectLoadController>();

            int max_create_num = 19;
            int min_create_num = 0;
            m_Controller.Init(scene_info.Bounds.center, scene_info.Bounds.size, true, max_create_num, min_create_num, SceneSeparateTreeType.QuadTree);

            Debug.Log("scene_info.ObjectInfoList.Count : "+scene_info.ObjectInfoList.Count.ToString());
            for (int i = 0; i < scene_info.ObjectInfoList.Count; i++)
            {
                m_Controller.AddSceneBlockObject(scene_info.ObjectInfoList[i]);
            }

            GameObjectEntity mainRoleGOE = RoleMgr.GetInstance().GetMainRole();
            if (mainRoleGOE != null)
            {
                detector = mainRoleGOE.GetComponent<SceneDetectorBase>();
            }
            action(1);
            string navmeshPath = "navmesh_"+scene_id;
            XLuaFramework.ResourceManager.GetInstance().LoadNavMesh(navmeshPath);
            AsyncOperation asy = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(navmeshPath, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            asy.completed += delegate(AsyncOperation asyOp){
                Debug.Log("load navmesh:"+asyOp.isDone.ToString());
            };
        });
    }

    public void LoadScene(int scene_id, float pos_x=0.0f, float pos_y=0.0f, float pos_z=0.0f)
    {
        Debug.Log("LoadScene scene_id "+(scene_id).ToString());
        isLoadingScene = true;
        isBaseWorldLoadOk = false;
        XLuaFramework.ResourceManager.GetInstance().LoadPrefabGameObjectWithAction(SceneInfoPath+"baseworld_"+scene_id+"/baseworld_"+scene_id+".prefab", delegate(UnityEngine.Object obj)
        {
            Debug.Log("load base world ok!");
            Transform baseworldTrans = (obj as GameObject).transform;
            for (int i = 0; i < baseworldTrans.childCount; i++)
            {
                baseworldTrans.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Ground");
            }
            // (obj as GameObject).layer = LayerMask.NameToLayer("Ground");
            LoadSceneInfo(scene_id, delegate(int result){
                isLoadingScene = false;
                isBaseWorldLoadOk = true;
                CorrectMainRolePos();
            });
        });
    }

    LightmapData[] lightmaps = null;
    public void ApplyDetector(SceneDetectorBase detector)
    {
        this.detector = detector;
    }

    private void ApplyLightInfo(SceneInfo scene_info)
    {
        LightmapSettings.lightmapsMode = scene_info.LightmapMode;
        int l1 = (scene_info.LightColorResPath == null) ? 0 : scene_info.LightColorResPath.Count;
        int l2 = (scene_info.LightDirResPath == null) ? 0 : scene_info.LightDirResPath.Count;
        int l = (l1 < l2) ? l2 : l1;
        // Debug.Log("ApplyLightInfo : "+ l.ToString());
        if (l > 0)
        {
            lightmaps = new LightmapData[l];
            for (int i = 0; i < l; i++)
            {
                if (i < l1)
                {
                    int temp_i = i;
                    XLuaFramework.ResourceManager.GetInstance().LoadAsset<Texture2D>(scene_info.LightColorResPath[i], delegate(UnityEngine.Object[] objs) {
                        // Debug.Log("load lightmap color texture : "+scene_info.LightColorResPath[i].ToString());
                        // Debug.Log("i : "+temp_i.ToString()+" objs:"+(objs!=null).ToString());
                        lightmaps[temp_i] = new LightmapData();
                        if (objs != null && objs.Length > 0)
                            lightmaps[temp_i].lightmapColor = objs[0] as Texture2D;
                        if (temp_i == l-1)
                            LightmapSettings.lightmaps = lightmaps;
                    });
                }
                if (i < l2)
                {
                    int temp_i = i;
                    XLuaFramework.ResourceManager.GetInstance().LoadAsset<Texture2D>(scene_info.LightDirResPath[i], delegate(UnityEngine.Object[] objs) {
                        // Debug.Log("load lightmap dir texture : "+scene_info.LightDirResPath[i].ToString());
                        lightmaps[temp_i] = new LightmapData();
                        if (objs != null && objs.Length > 0)
                            lightmaps[temp_i].lightmapDir = objs[0] as Texture2D;
                        if (temp_i == l-1)
                            LightmapSettings.lightmaps = lightmaps;
                    });
                }
            }
 
        }
    }

    //校正角色的坐标
    public void CorrectMainRolePos()
    {
        var mainRole = RoleMgr.GetInstance().GetMainRole();
        if (!isBaseWorldLoadOk || mainRole == null || curSceneInfo==null)
            return;
        Vector3 oldPos = mainRole.transform.localPosition;
        Vector3 newPos = GetCorrectPos(oldPos);
        Debug.Log("old pos:"+oldPos.x+" "+oldPos.y+" "+oldPos.z+" newPos:"+newPos.x+" "+newPos.y+" "+newPos.z);
        mainRole.transform.localPosition = newPos;
    }

    public Vector3 GetCorrectPos(Vector3 originPos)
    {
        Vector3 newPos = originPos;
        Ray ray1 = new Ray(originPos + new Vector3(0, 10000, 0), Vector3.down);
        RaycastHit groundHit;
        if (Physics.Raycast(ray1, out groundHit, 12000, groundLayer))
        {
            newPos = groundHit.point;
            newPos.y += 0.5f;
        }
        else
        {
            //wrong pos, return a nearest safe position
            if (curSceneInfo != null)
            {
                float minDistance = int.MaxValue;
                // int nearestIndex = 0;
                for (int i = 0; i < curSceneInfo.BornList.Count; i++)
                {
                    var bornInfo = curSceneInfo.BornList[i];
                    var bornPos = bornInfo.pos;
                    var dis = Vector3.Distance(bornPos, originPos);
                    if (dis < minDistance)
                    {
                        // nearestIndex = i;
                        minDistance = dis;
                        newPos = bornPos;
                    }
                }
                // if (nearestIndex < curSceneInfo.bornList.Count)
                // {
                //     var bornInfo = curSceneInfo.bornList[nearestIndex];
                // }
            }
        }
        return newPos;
    }
    
    public void ApplyMainRole(GameObjectEntity mainRole)
    {
        ApplyDetector(mainRole.GetComponent<SceneDetectorBase>());
        if (FreeLookCamera)
        {
            var mainRoleTrans = mainRole.GetComponent<Transform>();
            FreeLookCamera.m_Follow = mainRoleTrans;
            // FreeLookCamera.m_LookAt = mainRoleTrans;
            FreeLookCamera.m_LookAt = mainRoleTrans.Find("CameraLook");
        }
        CorrectMainRolePos();
    }

    public Entity AddMainRole(long uid, long typeID, string name, int career, Vector3 pos)
	{
        Entity role = RoleMgr.GetInstance().AddMainRole(uid, typeID, name, career, pos);
        entityDic.Add(uid, role);

        SkillManager.GetInstance().Init(career);
        return role;
    }

    public Entity AddSceneObject(long uid, string content)
    {
        string[] info_strs = content.Split(',');
        SceneObjectType type = (SceneObjectType)Enum.Parse(typeof(SceneObjectType), info_strs[0]);
        long typeID = Int64.Parse(info_strs[1]);
        long new_x = Int64.Parse(info_strs[2]);
        long new_y = Int64.Parse(info_strs[3]);
        long new_z = Int64.Parse(info_strs[4]);
        long target_x = Int64.Parse(info_strs[5]);
        long target_y = Int64.Parse(info_strs[6]);
        long target_z = Int64.Parse(info_strs[7]);
        var pos = GetCorrectPos(new Vector3(new_x/GameConst.RealToLogic, new_y/GameConst.RealToLogic, new_z/GameConst.RealToLogic));
        var targetPos = GetCorrectPos(new Vector3(target_x/GameConst.RealToLogic, target_y/GameConst.RealToLogic, target_z/GameConst.RealToLogic));
        if (type == SceneObjectType.Role)
        {
            Entity role = RoleMgr.GetInstance().AddRole(uid, typeID, pos, targetPos);
            entityDic.Add(uid, role);
            return role;
        }
        else if (type == SceneObjectType.Monster)
        {
            Entity monster = MonsterMgr.GetInstance().AddMonster(uid, typeID, pos, targetPos);
            entityDic.Add(uid, monster);
            return monster;
        }
        // else if (type == SceneObjectType.NPC)
            // return AddNPC(uid);
        return Entity.Null;
    }

    public string GetNameByUID(long uid)
    {
        SceneObjectType type = GetSceneObjTypeByUID(uid);
        string name;
        switch (type)
        {
            case SceneObjectType.Role:
                name = RoleMgr.GetInstance().GetName(uid);
                break;
            case SceneObjectType.Monster:
                name = MonsterMgr.GetInstance().GetName(GetSceneObject(uid));
                break;
            default:
                name = "";
                break;
        }
        return name;
    }

    public SceneObjectType GetSceneObjTypeByUID(long uid)
    {
        int value = (int)math.floor(uid/10000000000);
        switch (value)
        {
            case 1:
                return SceneObjectType.Role;
            case 2:
                return SceneObjectType.Monster;
            case 3:
                return SceneObjectType.NPC;
            default:
                return SceneObjectType.None;
        }
    }

    public void RemoveSceneObject(long uid)
    {
        Entity entity = GetSceneObject(uid);
        if (entity!=Entity.Null)
        {
            var moveQuery = EntityManager.GetComponentObject<MoveQuery>(entity);
            EntityManager.DestroyEntity(entity);
            entityDic.Remove(uid);
            if (moveQuery!=null)
            {
                GameObject.Destroy(moveQuery.gameObject);
                GameObject.Destroy(moveQuery.queryObj);
            }
        }
    }

    public Entity GetSceneObject(long uid)
    {
        // Debug.Log("GetSceneObject uid : "+uid.ToString()+" ContainsKey:"+entityDic.ContainsKey(uid).ToString());
        if (entityDic.ContainsKey(uid))
            return entityDic[uid];
        return Entity.Null;
    }

}

}