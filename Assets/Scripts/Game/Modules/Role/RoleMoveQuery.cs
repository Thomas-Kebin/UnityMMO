﻿using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class RoleMoveQuery : MonoBehaviour
{
    // [Serializable]
    // public struct Settings
    // {
        public float slopeLimit;
        public float stepOffset;
        public float skinWidth;
        public float minMoveDistance;
        public float3 center;
        public float radius;
        public float height;
    // }

    [NonSerialized] public int collisionLayer;
    [NonSerialized] public float3 moveQueryStart;
    [NonSerialized] public float3 moveQueryEnd;
    [NonSerialized] public float3 moveQueryResult;
    [NonSerialized] public bool isGrounded;

    [NonSerialized] public CharacterController charController;
    // [NonSerialized] public Settings settings;
    
    // public void Initialize(Settings settings, Entity hitCollOwner)
    public void Initialize()
    {
        //GameDebug.Log("RoleMoveQuery.Initialize");
        // this.settings = settings;
        // var go = new GameObject("MoveColl_" + name,typeof(CharacterController), typeof(HitCollision));
        var go = new GameObject("MoveColl_" + name,typeof(CharacterController));
        charController = go.GetComponent<CharacterController>();
        charController.transform.position = transform.position;
        charController.transform.SetParent(transform.parent);
        // charController.slopeLimit = settings.slopeLimit;
        // charController.stepOffset = settings.stepOffset;
        // charController.skinWidth = settings.skinWidth;
        // charController.minMoveDistance = settings.minMoveDistance;
        // charController.center = settings.center; 
        // charController.radius = settings.radius; 
        // charController.height = settings.height;

        // var hitCollision = go.GetComponent<HitCollision>();
        // hitCollision.owner = hitCollOwner;
    }

    public void Shutdown()
    {
        //GameDebug.Log("RoleMoveQuery.Shutdown");
        GameObject.Destroy(charController.gameObject);
    }
}


[DisableAutoCreation]
class HandleMovementQueries : BaseComponentSystem
{
    ComponentGroup Group;
	
    public HandleMovementQueries(GameWorld world) : base(world) {}
	
    protected override void OnCreateManager()
    {
        base.OnCreateManager();
        Group = GetComponentGroup(typeof(RoleMoveQuery));
    }

    protected override void OnUpdate()
    {
        // Profiler.BeginSample("HandleMovementQueries");
        
        var queryArray = Group.GetComponentArray<RoleMoveQuery>();

        // Debug.Log("queryArray.Length : "+queryArray.Length);
        for (var i = 0; i < queryArray.Length; i++)
        {
            var query = queryArray[i];

            var charController = query.charController;

            // if (charController.gameObject.layer != query.collisionLayer)
                // charController.gameObject.layer = query.collisionLayer;
            
            float3 currentControllerPos = charController.transform.position;
            // if (math.distance(currentControllerPos, query.moveQueryStart) > 0.01f)
            {
                currentControllerPos = query.moveQueryStart;
                charController.transform.position = currentControllerPos;
            }

            var deltaPos = query.moveQueryEnd - currentControllerPos; 
            Debug.Log("deltaPos : "+deltaPos.x+" "+deltaPos.y+" "+deltaPos.z);
            // Debug.Log("deltaPos : "+deltaPos.x+" "+deltaPos.y+" "+deltaPos.z+" query.moveQueryEnd:"+query.moveQueryEnd.x+" "+query.moveQueryEnd.y+" "+query.moveQueryEnd.z+" query.moveQueryStart:"+query.moveQueryStart.x+" "+query.moveQueryStart.y+" "+query.moveQueryStart.z);
            charController.Move(deltaPos);
            query.moveQueryResult = charController.transform.position;
            query.isGrounded = charController.isGrounded;
            Debug.Log("res pos : "+query.moveQueryResult.x+" "+query.moveQueryResult.y+" "+query.moveQueryResult.z);
            query.transform.position = query.moveQueryResult;
        }
        
        // Profiler.EndSample();
    }
}