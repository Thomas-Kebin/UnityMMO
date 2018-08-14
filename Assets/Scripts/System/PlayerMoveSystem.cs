﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms2D;
using Unity.Transforms;

namespace UnityMMO
{
    public class PlayerMoveSystem : ComponentSystem
    {
        public struct Data
        {
            public readonly int Length;
            public ComponentDataArray<Position> Position;
            // public ComponentDataArray<Heading2D> Heading;
            public ComponentDataArray<PlayerInput> Input;
            public ComponentDataArray<MovementSpeed> Speed;
        }

        [Inject] private Data m_Data;

        protected override void OnUpdate()
        {
            // var settings = TwoStickBootstrap.Settings;
            float dt = Time.deltaTime;
            for (int index = 0; index < m_Data.Length; ++index)
            {
                var position = m_Data.Position[index].Value;
                // var heading = m_Data.Heading[index].Value;

                var playerInput = m_Data.Input[index];

                // position += dt * playerInput.Move * m_Data.Speed[index].Speed;
                position.x += dt * playerInput.Move.x * m_Data.Speed[index].Value;
                position.z += dt * playerInput.Move.y * m_Data.Speed[index].Value;
                // Debug.Log("player move system update position :"+position.ToString());

                // if (playerInput.Fire)
                // {
                //     heading = math.normalize(playerInput.Shoot);

                    // playerInput.FireCooldown = settings.playerFireCoolDown;

                    // PostUpdateCommands.CreateEntity(TwoStickBootstrap.ShotSpawnArchetype);
                    // PostUpdateCommands.SetComponent(new ShotSpawnData
                    // {
                    //     Shot = new Shot
                    //     {
                    //         TimeToLive = settings.bulletTimeToLive,
                    //         Energy = settings.playerShotEnergy,
                    //     },
                    //     Position = new Position2D{ Value = position },
                    //     Heading = new Heading2D{ Value = heading },
                    //     Faction = Factions.kPlayer,
                    // });
                // }

                m_Data.Position[index] = new Position {Value = position};
                // m_Data.Heading[index] = new Heading2D {Value = heading};
                m_Data.Input[index] = playerInput;
            }
        }
    }
}
