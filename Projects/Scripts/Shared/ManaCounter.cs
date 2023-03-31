using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Reflection;

namespace Extension.Shared
{
    [Serializable]
    public class ManaCounter
    {
        public ManaCounter(TechnoExt Owner,int rof = 10)
        {
            Owner.GameObject.CreateScriptComponent(nameof(ManaBarScript), ManaBarScript.UniqueId, "manaBarScriptable", Owner, rof);
            manaBarScript = Owner.GameObject.GetComponent<ManaBarScript>();
        }

        private ManaBarScript manaBarScript;

        public bool Cost(int num)
        {
            if(manaBarScript == null)
            {
                return false;
            }
            return manaBarScript.Cost(num);
        }


        public int Current
        {
            get
            {
                return manaBarScript != null ? manaBarScript.Current : 0;
            }
        }



    }

    [ScriptAlias(nameof(ManaBarScript))]
    [Serializable]
    public class ManaBarScript : TechnoScriptable
    {
        public ManaBarScript(TechnoExt owner,int rof) : base(owner)
        {
            recoverDuration = rof;
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        public static int UniqueId = 2022121620;

        private SwizzleablePointer<AnimClass> pAnim;

        private int max = 100;

        private int currentMana = 100;

        private int recoverDuration = 5;

        private int currentIntervel = 0;

        public int Current
        {
            get { return currentMana; }
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

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            //CreateAnim();
        }

        public override void OnRemove()
        {
            KillAnim();
        }

        static Pointer<AnimTypeClass> manaAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ManaBarAnim");


        public override void OnUpdate()
        {
            if(Owner.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }


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
                }
            }

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            var visible = false;

            if (Owner.OwnerObject.Ref.Owner == HouseClass.Player)
            {
                if (Owner.OwnerObject.Ref.Base.IsSelected)
                {
                    visible = true;
                }
            }

            pAnim.Ref.Animation.Value = (currentMana / 5);
            pAnim.Ref.Pause();
            pAnim.Ref.Invisible = !visible;

        }
     

        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }
       
            var anim = YRMemory.Create<AnimClass>(manaAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            //anim.Ref.SetOwnerObject(Owner.OwnerObject.Convert<ObjectClass>());
            pAnim.Pointer = anim;
        }

        private void KillAnim()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit(); 
                pAnim.Pointer = IntPtr.Zero;
            }
        }
    }

}
