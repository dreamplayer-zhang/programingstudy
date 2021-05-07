using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Recipe_ViewModel : ObservableObject, IDialogRequestClose
    {
        MainWindow_ViewModel main;
        public Dlg_Recipe_ViewModel(MainWindow_ViewModel main)
        {
            this.main = main;
        }

        public enum RecipeMode
        {
            Sequence,
            Measure
        }

        private RecipeMode m_recipeMode = RecipeMode.Sequence;
        public RecipeMode p_recipeMode
        {
            get
            {
                return m_recipeMode;
            }
            set
            {
                if(m_recipeMode == value)
                {
                    return;
                }
                SetProperty(ref m_recipeMode, value);
                //if (m_mapMode != MapMRecipeModeode.Select)
                //{
                //    //test.DataCandidateSelectedPoint.Clear();
                //    //RecipeMode();
                //}
            }
        }

        object m_DataContext;
        public object p_DataContext
        {
            get
            {
                return m_DataContext;
            }
            set
            {
                SetProperty(ref m_DataContext, value);
            }
        }
        

        public ICommand CmdChangeMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(p_recipeMode == RecipeMode.Sequence)
                    {
                        p_DataContext = null;
                    }
                    else if(p_recipeMode == RecipeMode.Measure)
                    {
                        p_DataContext = main.RecipeViewModel;
                    }
                });
            }
        }

        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
