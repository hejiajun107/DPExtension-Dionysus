using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Experimental
{
    [ScriptAlias(nameof(VacuumImploderScript))]
    [Serializable]
    public class VacuumImploderScript : TechnoScriptable
    {
        public VacuumImploderScript(TechnoExt owner) : base(owner)
        {
        }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; private set; } = 1000;
        /// <summary>
        /// 持续时间（帧数）
        /// </summary>
        public int Duration { get; private set; } = 600;

        private readonly string effectWarhead = "Super";

        private readonly int range = 10;

        private int checkRof = 10;


        public override void OnUpdate()
        {
            if (Duration > 0)
            {
                Duration--;
            }
            else
            {
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
            }

            if (checkRof-- <= 0)
            {
                checkRof = 10;

                var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Game.CellSize * range);

                foreach(var techno in technos)
                {
                    if(techno.CastToTechno(out var pTechno))
                    {
                        if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                            continue;

                        if (MapClass.GetTotalDamage(10000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(effectWarhead), pTechno.Ref.Type.Ref.Base.Armor, 0) <= 0)
                            continue;

                        var ext = TechnoExt.ExtMap.Find(pTechno);
                        if (ext.IsNullOrExpired())
                            continue;

                        if (ext.GameObject.GetComponent<VacuumImploderEffectScript>() != null)
                            continue;

                        var effectScript = new VacuumImploderEffectScript(ext, this);
                        ext.GameObject.AddComponent(effectScript);
                    }
                }

                return;
            }

            base.OnUpdate();
        }

    }


    [ScriptAlias(nameof(VacuumImploderScript))]
    [Serializable]

    public class VacuumImploderEffectScript : TechnoScriptable
    {
        public VacuumImploderEffectScript(TechnoExt owner, VacuumImploderScript invoker) : base(owner)
        {
            Invoker = invoker;
        }

        VacuumImploderScript Invoker;

        private bool inited = false;

        private double radius = 0;

        private double angle = 0; // 当前角度（弧度）
        private readonly double rotationSpeed = Math.PI / 180 * 10; // 每帧旋转角度（这里是10度）
        private readonly double radiusDecreaseSpeed = 10; // 每帧半径减少量
        private readonly double minRadius = Game.CellSize * 1; // 最小半径
        private CoordStruct centerCoord; // 目标中心点坐标

        public override void OnUpdate()
        {
            if (Invoker.Owner.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }

            if(inited == false)
            {
                inited = true;
                // 获取目标中心点（比如发射者位置 + 高度）
                centerCoord = Invoker.Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, Invoker.Height);

                // 初始半径：单位当前位置到中心点的距离
                radius = Math.Floor((Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 1000))
                    .BigDistanceForm(centerCoord + new CoordStruct(0, 0, 1000)));

                angle = 0;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            mission.Ref.ForceMission(Mission.Stop);
            if (Owner.OwnerObject.Ref.Base.Health > 10)
            {
                Owner.OwnerObject.Ref.Base.Health--;
            }

            var targetCoord = Invoker.Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, Invoker.Height);

            if (radius <= minRadius)
            {
                // 已经到达中心，可执行爆炸或销毁
            }

            // 极坐标转笛卡尔坐标（2D 平面绕中心点）
            int newX = (int)(centerCoord.X + radius * Math.Cos(angle));
            int newY = (int)(centerCoord.Y + radius * Math.Sin(angle));
            int newZ = centerCoord.Z;
            if (Owner.OwnerObject.Ref.Base.Base.GetCoords().Z < centerCoord.Z)
            {
               newZ = Owner.OwnerObject.Ref.Base.Base.GetCoords().Z + (centerCoord.Z - Owner.OwnerObject.Ref.Base.Base.GetCoords().Z) / 10; 
            }

            CoordStruct newPosition = new CoordStruct(newX, newY, newZ);
            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pfoot))
            {
                var source = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                pfoot.Ref.Locomotor.Force_Track(-1, source);
                Owner.OwnerObject.Ref.Base.UnmarkAllOccupationBits(source);
                Owner.OwnerObject.Ref.Base.SetLocation(newPosition);
                Owner.OwnerObject.Ref.Base.UnmarkAllOccupationBits(newPosition);
            }
          
            base.OnUpdate();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(pWH.Ref.Base.ID == "Super")
            {
                return;
            }

            pDamage.Ref = 0;


            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }
    }
}
