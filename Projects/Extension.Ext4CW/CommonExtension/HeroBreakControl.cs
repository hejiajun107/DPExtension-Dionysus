using DynamicPatcher;
using Extension.CWUtilities;
using PatcherYRpp;

namespace Extension.CW
{
    //public partial class TechnoGlobalExtension
    //{
    //    private int heroCheckDelay = 20;

    //    [UpdateAction]
    //    public void TechnoClass_Update_HeroBreakControl()
    //    {
    //        if (!Data.IsHero)
    //            return;

    //        Logger.Log("是英雄");

    //        if (heroCheckDelay-- > 0)
    //            return;

    //        heroCheckDelay = 20;

    //        Logger.Log("是英雄");

    //        var pTransport = Owner.OwnerObject.Ref.Transporter;
    //        if (pTransport.IsNull)
    //            return;

    //        Logger.Log("有载具");


    //        if (pTransport.Ref.Base.InLimbo)
    //            return;
    //        Logger.Log("载具在战场上");


    //        if (pTransport.Ref.Owner == Owner.OwnerObject.Ref.Owner)
    //            return;
    //        Logger.Log("载具和单位不同所属");


    //        if (pTransport.Ref.IsMindControlled())
    //        {
    //            Logger.Log("载具被心控了");
    //            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(pTransport.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BreakMindControlWh"), 100, false) ;
    //            pInviso.Ref.DetonateAndUnInit(pTransport.Ref.Base.Base.GetCoords());
    //        }
    //    }


    //}
}
