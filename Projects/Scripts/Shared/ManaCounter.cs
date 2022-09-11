using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Shared
{
    [Serializable]
    public class ManaCounter
    {
        public ManaCounter(int rof = 10)
        {
            recoverDuration = rof;
        }

        private int max = 100;

        private int currentMana = 100;


        private int recoverDuration = 5;

        private int currentIntervel = 0;

        private int manaCheckRof = 0;

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SKILLPOWEREDWH");

        static Pointer<WarheadTypeClass> pWH1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MANAPIPWh1");
        static Pointer<WarheadTypeClass> pWH2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MANAPIPWh2");

        //static Pointer<WarheadTypeClass> pWH1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EnerygyBarWH1");
        //static Pointer<WarheadTypeClass> pWH2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EnerygyBarWH2");
        //static Pointer<WarheadTypeClass> pWH3 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EnerygyBarWH3");
        //static Pointer<WarheadTypeClass> pWH4 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EnerygyBarWH4");
        //static Pointer<WarheadTypeClass> pWH5 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EnerygyBarWH5");




        public void OnUpdate(TechnoExt owner)
        {
            if (currentMana < max)
            {
                if (currentIntervel < recoverDuration)
                {
                    currentIntervel++;
                }
                else
                {
                    currentMana++;
                    currentIntervel = 0;
                    //if (currentMana == 100)
                    //{
                    //    ShowBar(owner);
                    //}
                }
            }
            if(manaCheckRof-->0)
            {
                return;
            }
            manaCheckRof = 15;
            RefreshBar(owner);
        }

        private void RefreshBar(TechnoExt owner)
        {
            if(currentMana>=50)
            {
                var ptarget = owner.OwnerObject.Ref;
                Pointer<WarheadTypeClass> warhead;
                if (currentMana < 100)
                {
                    warhead = pWH1;
                }
                else
                {
                    warhead = pWH2;
                }
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, 1, warhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(ptarget.Base.Base.GetCoords());
            }
      
        }

        private void ShowBar(TechnoExt owner)
        {
            var ptarget = owner.OwnerObject.Ref;

            //Pointer<WarheadTypeClass> warhead;

            //if (currentMana < 20)
            //{
            //    warhead = pWH1;
            //}
            //else if (currentMana >= 20 && currentMana < 60)
            //{
            //    warhead = pWH2;
            //}
            //else if (currentMana >= 60 && currentMana < 80)
            //{
            //    warhead = pWH3;
            //}
            //else if (currentMana >= 80 && currentMana < 100)
            //{
            //    warhead = pWH4;
            //}
            //else
            //{
            //    warhead = pWH5;
            //}

            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, 1, pWH, 100, false);

            pBullet.Ref.DetonateAndUnInit(ptarget.Base.Base.GetCoords());
        }

        public bool Cost(int num)
        {
            if (num > currentMana)
            {
                return false;
            }
            else
            {
                currentMana -= num;
                return true;
            }
        }

        public int Current
        {
            get { return currentMana; }
        }

    }
}
