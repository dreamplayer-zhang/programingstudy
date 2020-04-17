﻿using System.Windows.Controls;

namespace RootTools.Memory
{
    /// <summary>
    /// MemoryPool_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemoryPool_UI : UserControl
    {
        public MemoryPool_UI()
        {
            InitializeComponent();
        }

        MemoryPool m_memoryPool;
        public void Init(MemoryPool memoryPool)
        {
            m_memoryPool = memoryPool;
            this.DataContext = memoryPool;
            listViewGroup.ItemsSource = memoryPool.p_aGroup;
            memoryPool.OnMemoryChanged += MemoryPool_OnMemoryChanged;
        }

        private void MemoryPool_OnMemoryChanged()
        {
            listViewMemory.ItemsSource = null;
            MemoryGroup group = (MemoryGroup)listViewGroup.SelectedItem;
            listViewMemory.ItemsSource = (group == null) ? null : group.p_aMemory;
        }

        private void ListViewGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MemoryPool_OnMemoryChanged();
        }
    }
}
