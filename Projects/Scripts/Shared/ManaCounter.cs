using DynamicPatcher;
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
      
            if (manaCheckRof-->0)
            {
                return;
            }
            manaCheckRof = 15;
            RefreshBar(owner);
        }

        public void OnRender(TechnoExt owner)
        {
            var location = owner.OwnerObject.Ref.Base.Base.GetCoords();
            DrawManadBar(8, new Point2D(location.X, location.Y));
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

        //, RectangleStruct pBound
        public void DrawManadBar(int iLength, Point2D pLocation)
        {
            Point2D vPos = new Point2D(0,0);
            Point2D vLoc = pLocation;
            int frame, XOffset, YOffset;
            YOffset = 0;//this->Techno->GetTechnoType()->PixelSelectionBracketDelta + this->Type->BracketDelta;
            vLoc.Y -= 5;

            var pipBoard = FileSystem.PIPBRD_SHP;

            if (iLength == 8)
            {
                vPos.X = vLoc.X + 11;
                vPos.Y = vLoc.Y - 25 + YOffset;
                frame = pipBoard.Ref.Frames > 2 ? 3 : 1;
                XOffset = -5;
                YOffset -= 24;
            }
            else
            {
                vPos.X = vLoc.X + 1;
                vPos.Y = vLoc.Y - 26 + YOffset;
                frame = pipBoard.Ref.Frames > 2 ? 2 : 0;
                XOffset = -15;
                YOffset -= 25;
            }

            Logger.Log("格子数：" + (int)(Math.Round((currentMana / 100d)) * iLength));

            for (int i = 0; i < (int)(Math.Round((currentMana / 100d)) * iLength); ++i)
            {
                vPos.X = vLoc.X + XOffset + 2 * i;
                vPos.Y = vLoc.Y + YOffset;

                //Surface.Current.Ref.Blit(Surface.ViewBound, drawRect
                //   , surface.Pointer.Convert<Surface>(), srcSurface.GetRect(), srcSurface.GetRect(), true, true);
                //Surface.Current.Ref.DrawSHP(pipBoard, frame, FileSystem.PALETTE_PAL, vPos,new RectangleStruct(1,1,1,1),BlitterFlags.None,ZGradient.Ground, 1000, 0, IntPtr.Zero, 0, 0, 0);
                //Surface.Current.Ref.DrawSHP(FileSystem.PALETTE_PAL, pipBoard, frame, vPos, new RectangleStruct(1, 1, 1, 1), BlitterFlags.None, ZGradient.Ground, 1000, 0, 0, 0, IntPtr.Zero, 0, 0, 0);

            }
            //surface.Ref.DrawSHP(FileSystem.PALETTE_PAL, pipBoard, frame, vPos, pBound, );
            //DSurface::Temp->DrawSHP(FileSystem::PALETTE_PAL, pipBoard,
            //frame, &vPos, pBound, BlitterFlags(0xE00), 0, 0, ZGradient::Ground, 1000, 0, 0, 0, 0, 0);

            //Pointer<ConvertClass> Palette, Pointer< SHPStruct > SHP, int frameIdx,
            //Pointer< Point2D > pos, Pointer<RectangleStruct> boundingRect, BlitterFlags flags, ZGradient arg7,
            //int zAdjust, int arg9, int bright, int TintColor, Pointer< SHPStruct > BUILDINGZ_SHA, uint argD, int ZS_X, int ZS_Y
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
