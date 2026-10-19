using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UISkeleton : MonoBehaviour
    {
        [Space]
        [SerializeField] protected PhysSettings Settings;
        #region SETTINGS
        [Serializable]
        protected struct PhysSettings
        {
            [Space]
            public float VelocityDamping;
            public float Elasticity;
            public float ElasticityDamping;
            public float CorrectionPower;
            public float ImpulsePower;

            [Space]
            public Vector2 Gravity;
            public Vector3 Up;

            [HideInInspector] public float Delta;
            [HideInInspector] public float DeltaSQ;
        }
        #endregion

        [Space]
        [SerializeField] protected List<Bone> BoneData;
        #region BONE
        [Serializable]
        protected class Bone
        {
            public RectTransform Parent;
            public RectTransform Transform;

            [Space]
            public List<RectTransform> Siblings;

            [Space]
            public Data Value;

            [Serializable]
            public struct Data
            {
                public Vector2 Position;

                [HideInInspector] public Vector2 Rest;
                [HideInInspector] public Vector2 Previous;
                [HideInInspector] public Vector2 Forward;
                [HideInInspector] public float Scale;

                [HideInInspector] public FixedList128Bytes<int> Siblings;
                [HideInInspector] public FixedList128Bytes<float> Distances;
            }
        }
        #endregion

        NativeArray<Bone.Data> Bones;

        [Space]
        [SerializeField] ImpulseTarget[] ImpulseTargets;
        #region IMPULSE TARGET
        [Serializable]
        class ImpulseTarget
        {
            public int Index;
            [Range(0f, 1f)] public float Multiplier = 1f;
        }
        #endregion

        void Start()
        {
            Build();
        }
        void FixedUpdate()
        {
            SimulateStep();
        }
        void LateUpdate()
        {
            ApplyVisualization();
        }
        void OnDestroy()
        {
            if (Bones.IsCreated)
                Bones.Dispose();
        }

        public void AddImpulse(Vector2 pos)
        {
            var target = new Vector2(pos.x / Screen.width, pos.y / Screen.height);
            var impulse = target.x - transform.position.x / Screen.width + target.y - transform.position.y / Screen.height;

            for (int t = 0; t < ImpulseTargets.Length; t++)
                ImpulseToTarget(ImpulseTargets[t].Index, ImpulseTargets[t].Multiplier * impulse, target);
        }

        void Build()
        {
            Bones = new NativeArray<Bone.Data>(BoneData.Count, Allocator.Persistent);
            for (int b = 0; b < Bones.Length; b++)
                Bones[b] = BoneData[b].Value;
        }
        void SimulateStep()
        {
            Settings.Delta = Time.deltaTime;
            Settings.DeltaSQ = Settings.Delta * Settings.Delta;

            new MoveBonesJob
            {
                Settings = Settings,

                Bones = Bones,
            }
            .Schedule(Bones.Length, Bones.Length / JobsUtility.JobWorkerCount)
            .Complete();

            for (int iter = 0; iter < 4; iter++)
                for (int b = 0; b < Bones.Length - 1; b++)
                {
                    var bone = Bones[b];

                    var oldPos = bone.Position;
                    for (int s = 0; s < bone.Siblings.Length; s++)
                    {
                        var sibling = Bones[bone.Siblings[s]];
                        var d = sibling.Position - oldPos;
                        var dist = d.magnitude;
                        if (dist < 0.001f)
                            continue;

                        var diff = (dist - bone.Distances[s]) / dist;
                        var correction = 0.5f * diff * d;

                        if (b == 0)
                            sibling.Position -= 2f * Settings.Delta * Settings.CorrectionPower * correction;
                        else
                        {
                            bone.Position += Settings.Delta * Settings.CorrectionPower * correction;
                            sibling.Position -= Settings.Delta * Settings.CorrectionPower * correction;
                        }

                        Bones[bone.Siblings[s]] = sibling;
                    }

                    Bones[b] = bone;
                }

            for (int b = 0; b < Bones.Length - 1; b++)
            {
                var bone = Bones[b];
                for (int s = 0; s < bone.Siblings.Length; s++)
                {
                    var index = bone.Siblings[s];
                    var sibling = Bones[index];

                    var bPos = bone.Position;
                    bPos.x *= Screen.width;
                    bPos.y *= Screen.height;

                    var sPos = sibling.Position;
                    sPos.x *= Screen.width;
                    sPos.y *= Screen.height;

                    var dir = bPos - sPos;

                    sibling.Forward = dir.normalized;
                    sibling.Scale = dir.magnitude;

                    Bones[index] = sibling;
                }
            }
        }
        void ImpulseToTarget(int nodeIndex, float power, Vector2 target)
        {
            var bone = Bones[nodeIndex];

            bone.Previous += Settings.ImpulsePower * Time.deltaTime * power * (target - Bones[nodeIndex].Previous).normalized;

            Bones[nodeIndex] = bone;
        }
        void ApplyVisualization()
        {
            var scale = 1080f / Screen.height;
            for (int b = 0; b < BoneData.Count; b++)
            {
                var bone = Bones[b];
                var t = BoneData[b].Transform;

                t.position = ConvertToScreen(bone.Position);
                t.rotation = Quaternion.LookRotation(Settings.Up, bone.Forward);
                t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scale * bone.Scale);
            }
        }
        Vector2 ConvertToScreen(Vector2 scaled) => new Vector2(Screen.width * scaled.x, Screen.height * scaled.y);

        [BurstCompile]
        struct MoveBonesJob : IJobParallelFor
        {
            [ReadOnly] public PhysSettings Settings;

            public NativeArray<Bone.Data> Bones;

            public void Execute(int index)
            {
                if (index == 0)
                    return;

                var bone = Bones[index];

                var velocity = Settings.VelocityDamping * (bone.Position - bone.Previous);

                var restOffset = bone.Rest - bone.Position;
                var springAccel = Settings.Elasticity * restOffset - (Settings.ElasticityDamping / Settings.Delta) * velocity;

                var newPos = bone.Position + Settings.DeltaSQ * (Settings.Gravity + springAccel) + velocity;

                bone.Previous = bone.Position;
                bone.Position = newPos;

                Bones[index] = bone;
            }
        }

#if UNITY_EDITOR
        [Space]
        [SerializeField] bool Recalculation;
        [SerializeField] RectTransform Root;
        [SerializeField] RectTransform[] Rig;

        [Space]
        [SerializeField] bool DrawBakingData;
        [SerializeField] Color BakingColor;

        [SerializeField] bool DrawRuntimeData;
        [SerializeField] Color RuntimeColor;

        void OnValidate()
        {
            if (!Recalculation || !Root)
                return;

            Recalculation = false;

            Rig = Root
                .GetComponentsInChildren<RectTransform>()
                .Where(t => !t.TryGetComponent<Image>(out var image)).ToArray();

            if (Rig != null && Rig.Length > 0)
            {
                BoneData = new List<Bone>();
                for (int r = 0; r < Rig.Length; r++)
                {
                    var rect = Rig[r];
                    var children = rect
                        .GetComponentsInChildren<RectTransform>()
                        .Where(t => !t.TryGetComponent<Image>(out var image))
                        .Where(t => !t.Equals(rect))
                        .ToList();

                    var parent = rect.parent as RectTransform;
                    for (int b = 0; b < BoneData.Count; b++)
                    {
                        var bone = BoneData[b];
                        if (bone.Transform.Equals(parent))
                        {
                            for (int c = 0; c < children.Count; c++)
                                for (int s = 0; s < bone.Siblings.Count; s++)
                                    if (bone.Siblings[s].Equals(children[c]))
                                    {
                                        bone.Siblings.RemoveAt(s);

                                        break;
                                    }

                            BoneData[b] = bone;

                            break;
                        }
                    }

                    BoneData.Add(new Bone
                    {
                        Parent = parent,
                        Transform = rect,
                        Siblings = children,
                    });
                }
            }

            if (BoneData != null)
                for (int b = 0; b < BoneData.Count; b++)
                {
                    var bone = BoneData[b];
                    bone.Transform.name = $"Segment {b}";
                    if (bone == null)
                        continue;

                    bone.Value = new Bone.Data();

                    var pos = new Vector3(bone.Transform.position.x / 1920f, bone.Transform.position.y / 1080f, 0f);
                    bone.Value.Rest =
                    bone.Value.Previous =
                    bone.Value.Position = pos;

                    for (int s = 0; s < bone.Siblings.Count; s++)
                    {
                        var index = GetIndex(bone.Siblings[s]);
                        if (index >= 0)
                        {
                            var sib = bone.Siblings[s];
                            var sibPos = new Vector3(sib.position.x / 1920f, sib.position.y / 1080f, 0f);

                            bone.Value.Siblings.Add(index);
                            bone.Value.Distances.Add((sibPos - pos).magnitude);
                        }
                    }
                }

            int GetIndex(RectTransform transform)
            {
                if (!transform)
                    return -1;

                for (int b = 0; b < BoneData.Count; b++)
                    if (BoneData[b].Transform.Equals(transform))
                        return b;

                return -1;
            }
        }
        void OnDrawGizmos()
        {
            if (DrawBakingData)
            {
                Gizmos.color = BakingColor;

                if (BoneData != null)
                    for (int b = 0; b < BoneData.Count; b++)
                    {
                        var bone = BoneData[b].Value;
                        for (int s = 0; s < bone.Siblings.Length; s++)
                            Gizmos.DrawLine(ConvertToScreen(bone.Position), ConvertToScreen(BoneData[bone.Siblings[s]].Value.Position));
                    }
            }

            if (DrawRuntimeData)
            {
                Gizmos.color = RuntimeColor;

                if (Bones.IsCreated)
                    for (int b = 0; b < Bones.Length; b++)
                    {
                        var bone = Bones[b];
                        for (int s = 0; s < bone.Siblings.Length; s++)
                            Gizmos.DrawLine(bone.Position, Bones[bone.Siblings[s]].Position);
                    }
            }
        }
#endif
    }
}