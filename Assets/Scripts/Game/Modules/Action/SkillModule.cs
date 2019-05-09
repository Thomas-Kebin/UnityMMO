using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityMMO;

public class SkillManager
{
    static SkillManager instance;
    int curComboIndex;
    int career;
    int[] skillIDs = new int[4];//主界面里的四个技能id，因为人物有多于4技能可以选择，所以需要后端记录下来哪四个常用的放在主界面
    public static SkillManager GetInstance()
    {
        if (instance != null)
            return instance;
        instance = new SkillManager();
        return instance;
    }

    public void Init(int career)
    {
        this.career = career;
        this.curComboIndex = 0;
        //just for test
        for (int i = 0; i < 4; i++)
        {
            skillIDs[i] = 100000+career*10000+10+i;
        }
    }

    public int GetCurAttackID()
    {
        return GetAttackID(career, curComboIndex);
    }

    public void ResetCombo()
    {
        curComboIndex = 0;
    }

    public void IncreaseCombo()
    {
        //普攻有四个
        curComboIndex++;
        if (curComboIndex>=4)
            curComboIndex = 0;
    }

    public int GetSkillIDByIndex(int skillIndex)
    {
        if (skillIndex == -1)
            return GetCurAttackID();
        else
            return skillIDs[skillIndex];
    }

    public string GetSkillResPath(int skillID)
    {
        string assetPath;
        int scene_obj_type = GetSceneObjTypeBySkillID(skillID);
        if (scene_obj_type == (int)SceneObjectType.Role)
            assetPath = GameConst.GetRoleSkillResPath(GetCareerBySkillID(skillID), skillID);
        else if(scene_obj_type == (int)SceneObjectType.Monster)
            assetPath = GameConst.GetMonsterSkillResPath(skillID);
        else
            assetPath = "";
        return assetPath;
    }

    public int GetCareerBySkillID(int skillID)
    {
        int scene_obj_type = GetSceneObjTypeBySkillID(skillID);
        if (scene_obj_type == (int)SceneObjectType.Role)
            return (int)math.floor((skillID%10000)/1000);
        return 1;
    }

    public int GetSceneObjTypeBySkillID(int skillID)
    {
        return (int)math.floor((skillID/100000));
    }

    private static int GetAttackID(int career, int comboIndex)
    {
        //技能id：十万位是类型1角色，2怪物，3NPC，万位为职业，个十百位随便用
        return 100000+career*10000+comboIndex;
    }

    private SkillManager()
    {
    }
}

public struct SkillSpawnRequest : IComponentData
{
    public long UID;
    public int SkillID;
    private SkillSpawnRequest(long UID, int SkillID)
    {
        this.UID = UID;
        this.SkillID = SkillID;
    }

    public static void Create(EntityCommandBuffer commandBuffer, long UID, int SkillID)
    {
        var data = new SkillSpawnRequest(UID, SkillID);
        commandBuffer.CreateEntity();
        commandBuffer.AddComponent(data);
    }
}


[DisableAutoCreation]
public class SkillSpawnSystem : BaseComponentSystem
{
    public SkillSpawnSystem(GameWorld world) : base(world) {}

    ComponentGroup RequestGroup;

    protected override void OnCreateManager()
    {
        base.OnCreateManager();
        RequestGroup = GetComponentGroup(typeof(SkillSpawnRequest));
    }

    protected override void OnUpdate()
    {
        float dt = Time.deltaTime;
        var requestArray = RequestGroup.GetComponentDataArray<SkillSpawnRequest>();
        if (requestArray.Length == 0)
            return;

        var requestEntityArray = RequestGroup.GetEntityArray();
        
        // Copy requests as spawning will invalidate Group
        var requests = new SkillSpawnRequest[requestArray.Length];
        for (var i = 0; i < requestArray.Length; i++)
        {
            requests[i] = requestArray[i];
            PostUpdateCommands.DestroyEntity(requestEntityArray[i]);
        }

        for(var i = 0; i < requests.Length; i++)
        {
            
        }
    }
}