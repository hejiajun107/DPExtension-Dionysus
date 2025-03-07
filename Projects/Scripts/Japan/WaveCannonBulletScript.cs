using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(WaveCannonBulletScript))]
    [Serializable]
    public class WaveCannonBulletScript : BulletScriptable
    {
        private static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BLMCKExpWH");

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<AnimTypeClass> pAnimLow => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGICESMALL");//AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("1202SPARK");

        private static Pointer<AnimTypeClass> pAnimHigh => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGROCX04");// AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("1201SPARK");

        public WaveCannonBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;

                var tLocation = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                var location = Owner.OwnerObject.Ref.SourceCoords;

                var targets = FindIntervalPoints(location.X, location.Y, tLocation.X, tLocation.Y,256);
                var lastPoint = targets.Count > 0 ? targets[targets.Count - 1] : null;

                foreach (var target in targets)
                {
                    if (!Owner.OwnerObject.Ref.Owner.IsNull)
                    {
                        var clocation = new CoordStruct(target.Item1, target.Item2, location.Z);
                        var distance = clocation.DistanceFrom(location);
                        if (distance == double.NaN)
                        {
                            distance = 2560;
                        }
                        distance = distance > 2560 ? 2560 : distance;

                        var baseDamage = 10;
                        var extraDamage = ((2560d + 256d - distance) / 2560) * (320 + (distance <= 760 ? 160 : 0));

                        var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner, (int)((baseDamage + extraDamage) * Owner.OwnerObject.Ref.Owner.Ref.FirepowerMultiplier), pWarhead, 100, true);
                        pBullet.Ref.DetonateAndUnInit(clocation);
                        if (target.Equals(lastPoint))
                        {
                            YRMemory.Create<AnimClass>(pAnimHigh, clocation);
                            YRMemory.Create<AnimClass>(pAnimLow, clocation);
                        }
                    }
                }
            }
        }

        static List<Tuple<int, int>> FindIntervalPoints(double x1, double y1, double x2, double y2, double interval)
        {
            double d = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            if (d == 0) return new List<Tuple<int, int>>();

            double ux = (x2 - x1) / d;
            double uy = (y2 - y1) / d;

            var points = new List<Tuple<int, int>>();
            for (int i = 1; i * interval <= d; i++)
            {
                double px = x1 + i * interval * ux;
                double py = y1 + i * interval * uy;
                // 直接转换为int，向下取整
                points.Add(Tuple.Create((int)Math.Round(px), (int)Math.Round(py)));

                // 如果下一个点超出或正好到达终点，则提前终止
                if (i * interval >= d - interval / 2.0) // 考虑浮点数运算误差，使用interval/2.0作为容差
                {
                    // 添加精确的终点
                    points.Add(Tuple.Create((int)x2, (int)y2));
                    break;
                }
            }

            return points;
        }
    }



    [ScriptAlias(nameof(WaveCannonAnimAEScript))]
    public class WaveCannonAnimAEScript : AttachEffectScriptable
    {
        public WaveCannonAnimAEScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<AnimTypeClass> pAnimLow => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGROCX02");


        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);

        }

        public override void OnAttachEffectRemove()
        {
            YRMemory.Create<AnimClass>(pAnimLow, Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }
    }
}
