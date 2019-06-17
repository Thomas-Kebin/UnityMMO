﻿using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;
using UnityMMO;
using UnityMMO.Component;

namespace UnityMMO
{
public struct FindWayInfo
{
    public Vector3 destination;
    //到达目的地前的一段距离就停下来
    public float stoppingDistance;
    public Action onStop;
}
public class MoveQuery : MonoBehaviour
{
    public float slopeLimit;
    public float stepOffset;
    public float skinWidth;
    public float minMoveDistance;
    public float3 center;
    public float radius;
    public float height;
    private bool isAutoFinding;

    [NonSerialized] public int collisionLayer;
    [NonSerialized] public float3 moveQueryStart;
    [NonSerialized] public float3 moveQueryEnd;
    [NonSerialized] public float3 moveQueryResult;
    [NonSerialized] public bool isGrounded;
    [NonSerialized] public GameObject queryObj;
    [NonSerialized] public GameObjectEntity ownerGOE;

    [NonSerialized] public CharacterController charController;
    [NonSerialized] public NavMeshAgent navAgent;
    [NonSerialized] public Action onStop;

    public bool IsAutoFinding { get => isAutoFinding; set => isAutoFinding = value; }

    public void Initialize(bool isNeedNavAgent=false)
    {
        ownerGOE = GetComponent<GameObjectEntity>();
        //GameDebug.Log("MoveQuery.Initialize");
        // this.settings = settings;
        // var go = new GameObject("MoveColl_" + name,typeof(CharacterController), typeof(HitCollision));
        GameObject go = null;
        if (isNeedNavAgent)
            go = new GameObject("MoveColl_" + name, typeof(CharacterController), typeof(UIDProxy), typeof(NavMeshAgent));
        else
            go = new GameObject("MoveColl_" + name, typeof(CharacterController), typeof(UIDProxy));
        queryObj = go;
        charController = go.GetComponent<CharacterController>();
        charController.transform.position = transform.position;
        moveQueryStart = transform.position;
        moveQueryEnd = transform.position;
        // charController.transform.SetParent(transform.parent);
        charController.transform.SetParent(UnityMMO.SceneMgr.Instance.MoveQueryContainer);
        charController.slopeLimit = this.slopeLimit;
        charController.stepOffset = this.stepOffset;
        charController.skinWidth = this.skinWidth;
        charController.minMoveDistance = this.minMoveDistance;
        charController.center = this.center; 
        charController.radius = this.radius; 
        charController.height = this.height;
        go.layer = gameObject.layer;

        var uid = go.GetComponent<UIDProxy>();
        uid.Value = ownerGOE.EntityManager.GetComponentData<UID>(ownerGOE.Entity);
        Debug.Log("uid.Value : "+uid.Value.Value);
        isAutoFinding = false;
        if (isNeedNavAgent)
        {
            navAgent = go.GetComponent<NavMeshAgent>();
            navAgent.radius = this.radius;
            navAgent.height = this.height;
        }
        // var hitCollision = go.GetComponent<HitCollision>();
        // hitCollision.owner = hitCollOwner;
    }

    public void StartFindWay(FindWayInfo info)
    {
        if (!navAgent)
        {
            Debug.LogError("has no NavMeshAgent Component!call my method Initialize(true)");
            return;
        }
        navAgent.isStopped = false;
        var moveSpeed = ownerGOE.EntityManager.GetComponentData<MoveSpeed>(ownerGOE.Entity);
        navAgent.speed = moveSpeed.Value/GameConst.RealToLogic;
        navAgent.acceleration = 1000;
        navAgent.angularSpeed = 1000;
        navAgent.stoppingDistance = info.stoppingDistance;
        navAgent.destination = info.destination;
        onStop = info.onStop;
        isAutoFinding = true;
    }

    public void StopFindWay()
    {
        navAgent.isStopped = true;
        var newPos = navAgent.transform.localPosition;
        this.isAutoFinding = false;
        this.moveQueryStart = newPos;
        this.moveQueryEnd = newPos;
        this.moveQueryResult = newPos;
        if (onStop != null)
        {
            onStop();
            onStop = null;
        }
    }

    public void Shutdown()
    {
        //GameDebug.Log("MoveQuery.Shutdown");
        GameObject.Destroy(charController.gameObject);
    }
}

[DisableAutoCreation]
class HandleMovementQueries : BaseComponentSystem
{
    EntityQuery Group;
	
    public HandleMovementQueries(GameWorld world) : base(world) {}
	
    protected override void OnCreateManager()
    {
        base.OnCreateManager();
        Group = GetEntityQuery(typeof(MoveQuery));
    }

    protected override void OnUpdate()
    {
        // Profiler.BeginSample("HandleMovementQueries");
        var queryArray = Group.ToComponentArray<MoveQuery>();
        // Debug.Log("queryArray.Length : "+queryArray.Length);
        for (var i = 0; i < queryArray.Length; i++)
        {
            var query = queryArray[i];
            if (!query.IsAutoFinding)
            {
                var charController = query.charController;
                float3 currentControllerPos = charController.transform.position;
                if (math.distance(currentControllerPos, query.moveQueryStart) > 0.01f)
                {
                    currentControllerPos = query.moveQueryStart;
                    charController.transform.position = currentControllerPos;
                }

                var deltaPos = query.moveQueryEnd - currentControllerPos; 
                // Debug.Log("deltaPos : "+deltaPos.x+" "+deltaPos.y+" "+deltaPos.z);
                charController.Move(deltaPos);
                query.moveQueryResult = charController.transform.position;
                query.isGrounded = charController.isGrounded;
                // Debug.Log("res pos : "+query.moveQueryResult.x+" "+query.moveQueryResult.y+" "+query.moveQueryResult.z);
                query.transform.localPosition = query.moveQueryResult;
            }
            else
            {
                var isReachTarget = query.navAgent.remainingDistance<=query.navAgent.stoppingDistance;
                var newPos = query.navAgent.transform.localPosition;
                // Debug.Log("newPos :"+newPos.x+" "+newPos.y+" "+newPos.z+" reach:"+isReachTarget);
                query.isGrounded = query.charController.isGrounded;
                query.transform.localPosition = newPos;
                if (isReachTarget)
                {
                    query.StopFindWay();
                }
                else
                {
                    var nextTargetPos = query.navAgent.nextPosition;
                    // var lastTargetPos = query.ownerGOE.EntityManager.GetComponentData<TargetPosition>(query.ownerGOE.Entity);
                    // if (nextTargetPos != lastTargetPos.Value)
                    // var lastTargetPosVal = lastTargetPos.Value;
                    // if (nextTargetPos.x != lastTargetPosVal.x || nextTargetPos.y != lastTargetPosVal.y || nextTargetPos.z != lastTargetPosVal.z)
                    var dir = nextTargetPos - query.navAgent.transform.position;
                    nextTargetPos = nextTargetPos + dir*10;//for rotation in MovementUpdateSystem only
                    // lastTargetPos.Value = nextTargetPos;
                    query.ownerGOE.EntityManager.SetComponentData<TargetPosition>(query.ownerGOE.Entity, new TargetPosition{Value=nextTargetPos});
                }
            }
        }
        // Profiler.EndSample();
    }
}
}