using System.Collections.Generic;
using UnityEngine;
using static BwmpsTools.Utils.Structs;

namespace BwmpsTools.Data
{
    internal class BaseSettings
    {
        public string BaseName { get; private set; }
        public virtual Dictionary<HBBAndNames, PhysboneSettings> BoneSettings { get; private set; }

        public static HBBAndNames hips = new HBBAndNames() { names = new List<string>() { "hips" }, bone = HumanBodyBones.Hips };
        public static HBBAndNames chest = new HBBAndNames() { names = new List<string>() { "chest" }, bone = HumanBodyBones.Chest };
        public static HBBAndNames head = new HBBAndNames() { names = new List<string>() { "head" }, bone = HumanBodyBones.Head };

        public BaseSettings(string name) 
        {
            BaseName = name;
            BoneSettings = new Dictionary<HBBAndNames, PhysboneSettings>();
        }

        public PhysboneSettings GetPhysboneSettings(HBBAndNames bone)
        {
            return BoneSettings[bone];
        }
    }
}
