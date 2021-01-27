using System.Windows.Controls;

namespace RootTools.Lens
{
    public interface ILens
    {
        string p_id { get; set; }

        void ThreadStop();

        UserControl p_ui { get; }
    }
}
