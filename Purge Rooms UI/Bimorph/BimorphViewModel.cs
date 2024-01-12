using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Purge_Rooms_UI.Bimorph
{
    public class BimorphViewModel : ViewModelBase
    {
        public BimorphModel Model { get; set; }
        public RelayCommand<Window> Process { get; set; }

        public BimorphViewModel(BimorphModel model)
        {
            Model = model;
            Process = new RelayCommand<Window>(OnExecuteRun);
        }

        private void OnExecuteRun(Window win)
        {
            Model.MainCode();
            win.Close();
        }
    }
}
