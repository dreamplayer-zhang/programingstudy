﻿using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
    class FrontsideSpec_ViewModel : ObservableObject
    {

        private FrontsideSpecTool_ViewModel m_ImageViewer_VM;
        public FrontsideSpecTool_ViewModel p_ImageViewer_VM
        {
            get
            {
                return m_ImageViewer_VM;
            }
            set
            {
                SetProperty(ref m_ImageViewer_VM, value);
            }
        }


        public Recipe m_Recipe;
        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            m_Recipe = recipe;

            m_cMask = new ObservableCollection<Mask>();
            m_cInspMethod = new ObservableCollection<Type>();
            m_cInspItem = new ObservableCollection<InspectionItem>();

            p_ImageViewer_VM = new FrontsideSpecTool_ViewModel();
            p_ImageViewer_VM.init(setup, recipe);


            MaskDummy();
        }
        void MaskDummy()
        {
            Mask mask1 = new Mask();
            mask1.p_sName = "Mask1";
            mask1.p_Color = Color.Red;
            mask1.p_dHeight = 200;
            mask1.p_dWidth = 200;
            mask1.p_PtMask = new System.Windows.Point(30, 30);
            Mask mask2 = new Mask();
            mask2.p_sName = "Mask2";
            mask2.p_Color = Color.Blue;
            mask2.p_dHeight = 400;
            mask2.p_dWidth = 400;
            mask2.p_PtMask = new System.Windows.Point(50, 50);
            Mask mask3 = new Mask();
            mask3.p_sName = "Mask3";
            mask3.p_Color = Color.Green;
            mask3.p_dHeight = 800;
            mask3.p_dWidth = 800;
            mask3.p_PtMask = new System.Windows.Point(100, 100);

            p_cMask.Add(mask1);
            p_cMask.Add(mask2);
            p_cMask.Add(mask3);


            ObservableCollection<Type> workList = Tools.GetInheritedClasses(typeof(WorkBase));

            p_cInspMethod = workList;
            //p_cInspMethod = workList;
        }


        #region Property
        private ObservableCollection<Mask> m_cMask;
        public ObservableCollection<Mask> p_cMask
        {
            get
            {
                return m_cMask;
            }
            set
            {
                SetProperty(ref m_cMask, p_cMask);
            }
        }

        private ObservableCollection<Type> m_cInspMethod;
        public ObservableCollection<Type> p_cInspMethod
        {
            get
            {

                return m_cInspMethod;
            }
            set
            {
                SetProperty(ref m_cInspMethod, value);
            }
        }

        private ObservableCollection<InspectionItem> m_cInspItem;
        public ObservableCollection<InspectionItem> p_cInspItem
        {
            get
            {
                return m_cInspItem;
            }
            set
            {
                SetProperty(ref m_cInspItem, value);
            }
        }

        private Mask m_selectedMask;
        public Mask p_selectedMask
        {
            get
            {
                return m_selectedMask;
            }
            set
            {
                if (p_selectedInspItem == null)
                    return;
                m_selectedInspItem.p_Mask = value;
                SetProperty(ref m_selectedMask, value);
            }
        }

        private InspectionItem m_selectedInspItem;
        public InspectionItem p_selectedInspItem
        {
            get
            {
                return m_selectedInspItem;
            }
            set
            {
                SetProperty(ref m_selectedInspItem, value);

                p_selectedMethodItem = m_selectedInspItem.p_InspMethod;
                //m_selectedInspItem.p_cInspMethod.
            }
        }

        private Type m_selectedMethodItem;
        public Type p_selectedMethodItem
        {
            get
            {
                return m_selectedMethodItem;
            }
            set
            {
                SetProperty(ref m_selectedMethodItem, value);
                //m_selectedInspItem.p_cInspMethod.
            }
        }
        #endregion
        //private void Item_changeMode(object e)
        //{
        //    p_selectedMethod = null;
        //    p_selectedMethod = e as WorkBase;
        //}
        //public ICommand btnAddInspMethod
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {

        //        });
        //    }
        //}
        //public ICommand btnDeleteInspMethod
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            p_cInspMethod.Remove(p_selectedMethod);
        //            if (p_cInspMethod.Count == 0)
        //                return;
        //            p_selectedMethod = p_cInspMethod.Last();
        //            //InspMethodRefresh();
        //        });
        //    }
        //}
        public ICommand btnAddInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InspectionItem item = new InspectionItem();
                    item.p_cMask = p_cMask;
                    item.p_cInspMethod = p_cInspMethod;
                    p_cInspItem.Add(item);
                });
            }
        }


        public ICommand btnDeleteInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_cInspItem.Remove(p_selectedInspItem);

                });
            }
        }

        public ICommand ComboBoxSelectionChanged_MethodItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MessageBox.Show("DDD");
                });
            }
        }
    }
}
