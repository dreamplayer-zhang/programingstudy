using System;
using System.Windows;

namespace RootTools
{
    public interface IDialogService
    {
        void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose
                                           where TView : IDialog;

        Nullable<bool> ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;
        IDialog GetDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;
    }
}