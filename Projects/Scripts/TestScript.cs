using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class TestScript : TechnoScriptable
    {
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public TestScript(TechnoExt owner) : base(owner) {

        }

        private int delay = 200;

        bool exported = false;

        private int fireCount = 0;

        private bool typeChanged = false;

        public override void OnUpdate()
        {
            //var mission = Owner.OwnerObject.Convert<MissionClass>();

            //Logger.Log($"Mission：{mission.Ref.CurrentMission}");

            //if(mission.Ref.CurrentMission == Mission.Hunt)
            //{
            //    mission.Ref.ForceMission(Mission.Guard);
            //}


            if (delay-- > 0)
                return;
            delay = 200;
            ref DynamicVectorClass<Pointer<TechnoClass>> technos = ref TechnoClass.Array;
            ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;

            Logger.Log($"单位数：{technos.Count},抛射体数：{bullets.Count}");
            //if (delay-- > 0)
            //{
            //    return;
            //}

            //delay = 20;


            //ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;

            //ref DynamicVectorClass<Pointer<TechnoClass>> technos = ref TechnoClass.Array;

            //if (exported == false)
            //{
            //    Logger.Log($"抛射体数量：{bullets.Count},单位数量：{technos.Count}");
            //}

            ////if (technos.Count >= 600 && exported == false)
            ////{
            ////    exported = true;
            //for (var i = 0; i < technos.Count; i++)
            //{
            //    var pTechno = technos.Get(i);
            //    Logger.Log($"HashCode：{pTechno.Ref.Base.Base.GetHashCode()}单位：{pTechno.Ref.Type.Ref.Base.Base.ID}，存活:{pTechno.Ref.Base.IsAlive},在地图上：{pTechno.Ref.Base.IsOnMap}，Limbo:{pTechno.Ref.Base.InLimbo},血量:{pTechno.Ref.Base.Health}");
            //}
            //    //for (var i = 0; i < bullets.Count; i++)
            //    //{
            //    //    var pBullet = bullets.Get(i);
            //    //    Logger.Log($"{(pBullet.Ref.Owner.IsNull ? "   " : pBullet.Ref.Owner.Ref.Type.Ref.Base.Base.ID)}发射了{pBullet.Ref.Type.Ref.Base.Base.ID}");
            //    //}
            //}

            //if (bullets.Count>=5000&& exported==false)
            //{
            //    exported = true;
            //    for (var i= 0;i<bullets.Count;i++)
            //    {
            //        var pBullet = bullets.Get(i);
            //        Logger.Log($"{(pBullet.Ref.Owner.IsNull? "   ":pBullet.Ref.Owner.Ref.Type.Ref.Base.Base.ID)}发射了{pBullet.Ref.Type.Ref.Base.Base.ID}");
            //    }
            //}

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            fireCount++;
            if (fireCount >= 5)
            {
                if (typeChanged == false)
                {
                    typeChanged = true;
                    var type = Owner.OwnerObject.Ref.Type;
                    SwizzleablePointer<TechnoTypeClass> pointer = new SwizzleablePointer<TechnoTypeClass>(type);

                    object retval;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, type);
                        ms.Seek(0, SeekOrigin.Begin);
                        retval = bf.Deserialize(ms);
                        ms.Close();
                    }
                    var targetType = (SwizzleablePointer<TechnoTypeClass>)retval;
                    targetType.Ref.ROT = 1;
                    targetType.Ref.Speed = 20;
                    Owner.OwnerObject.Convert<UnitClass>().Ref.Type = targetType.Pointer.Convert<UnitTypeClass>();

                }
            }
           
            //if (delay-- > 0)
            //    return;
            //delay = 200;
            //ref DynamicVectorClass<Pointer<TechnoClass>> technos = ref TechnoClass.Array;
            //ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;

            //Logger.Log($"单位数：{technos.Count},抛射体数：{bullets.Count}");

            //var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t => t.Ref.Type.Ref.Base.Base.ID == "MTNK", FindRange.Owner);

            //technos.ForEach(t => 
            //    {
            //        var pTechno = t.Get();
            //        Logger.Log("找到单位");
            //        //pTechno.OwnerObject.Ref.SetTarget(pTarget);
            //        //pTechno.OwnerObject.Ref.TurretFacing = new FacingStruct()(pTarget);
            //        Logger.Log(pTechno.OwnerObject.Ref.GetFireErrorWithoutRange(pTarget, 0).ToString());
            //        pTechno.OwnerObject.Ref.SetTarget(pTarget);
            //        pTechno.OwnerObject.Ref.Fire(pTarget, 0);
            //        ////pTechno.OwnerObject.Ref.SetTarget(default);
            //    }
            //);
        }
    }
}
