﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnityMMO
{
    public struct UID : IComponentData
    {
        public long Value;
    }

    public struct SceneObjectTypeData : IComponentData
    {
        public SceneObjectType Value;
    }

    //Role类型的话对应RoleID，Monster和NPC类型对应TypeID
    public struct TypeID : IComponentData
    {
        public long Value;
    }

    public struct TimelineState : IComponentData
    {
        public enum NewState
        {
            Allow,//允许新Timeline加进队列
            Forbid,//禁止新Timeline加进队列
        }
        public NewState NewStatus;

        public enum InterruptState
        {
            Allow,//可以播放队列里新的Timeline了
            Forbid,//禁止播放新的Timeline
        }
        public InterruptState InterruptStatus;
    }

    public struct LocomotionState : IComponentData
    {
        public enum State
        {
            Idle,
            Run,
            Sprint,
            Jump,
            DoubleJump,
            TrebleJump,
            InAir,
            BeHit,
            StateNum,
        }
        public State LocoState;

        public bool IsOnGround()
        {
            return LocoState == LocomotionState.State.Idle || LocoState == LocomotionState.State.Run || LocoState == LocomotionState.State.Sprint || LocoState == LocomotionState.State.BeHit;
        }
        public bool IsInJump()
        {
            return LocoState == LocomotionState.State.Jump || LocoState == LocomotionState.State.DoubleJump || LocoState == LocomotionState.State.TrebleJump || LocoState == LocomotionState.State.InAir;
        }
        public float StartTime;
        public int JumpCount;
    }

    public struct ActionData : IComponentData
    {
        public int Jump;
        public static ActionData Empty = new ActionData{Jump=0};
    }

    public struct NameboardData : IComponentData
    {
        public enum ResState 
        {
            WaitLoad,//等待判断是否离主角够近，够近才进入此状态等待加载prefab
            Loading,//加载中
            Loaded,//已加载
            DontLoad,//远离主角，别加载了
        }
        public ResState UIResState;
        public Entity UIEntity;
    }

    public struct PosOffset : IComponentData
    {
        public float3 Value;
    }

    public struct JumpState : IComponentData
    {
        public enum State
        {
            None = 0,
            StartJump,
            InAir,
            EndJump,
        }
        public State JumpStatus;
        public int JumpCount;
        public float OriginYPos;
        public float AscentHeight;
        // public float StartTime;
    }

    public struct ActionInfo : IComponentData
    {
        public enum Type
        {
            None = 0,
            Skill1 = 1,
            Skill2 = 2,
            Skill3 = 3,
            Skill4 = 4,
        }
        public Type Value;
    }

    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }
    
    public struct TargetPosition : IComponentData
    {
        public float3 Value;
    }

    public struct PosSynchInfo : IComponentData
    {
        public float3 LastUploadPos;
    }

    public struct LooksInfo : IComponentData
    {
        public enum State
        {
            None = 0,
            Loading = 1,
            Loaded = 2,
            // Deleting = 3,
        }
        public Entity LooksEntity;
        public State CurState;
    }

    public struct GroundInfo : IComponentData
    {
        // public Collider groundCollider;
        public Vector3 GroundNormal;
        public float Altitude; 
        // public GroundInfo(){}
    }

    public struct SprintInfo : IComponentData
    {
        public int IsSprinting;//1:yes
    }

    public struct Shot : IComponentData
    {
        public float TimeToLive;
        public float Energy;
    }

    public struct Factions
    {
        public const int kPlayer = 0;
        public const int kEnemy = 1;
    }
    public struct ModifyHealthQueue : IComponentData
    {
        public NativeArray<float> queue;
    }
    public struct Health : IComponentData
    {
        public float Value;
    }
    public struct SceneObjectData : IComponentData
    {
        public enum Type 
        {
            Role,Monster,NPC
        }
        Type type;
    }
    // Pure marker types
    public struct Enemy : IComponentData { }

    public struct EnemyShootState : IComponentData
    {
        public float Cooldown;
    }

    // TODO: Call out that this is better than storing state in the system, because it can support things like replay.
    public struct EnemySpawnCooldown : IComponentData
    {
        public float Value;
    }

    public struct EnemySpawnSystemState : IComponentData
    {
        public int SpawnedEnemyCount;
        public UnityEngine.Random.State RandomState;
    }
}
