using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Purge_Rooms_UI.Bimorph
{
    public class BimorphViewModel : ViewModelBase
    {
        public BimorphModel Model { get; set; }             // property to hold the model
        public RelayCommand<Window> Process { get; set; }   // property ho hold the command/process

        public BimorphViewModel(BimorphModel model)         // constructor for the view-model
        {
            Model = model;
            Process = new RelayCommand<Window>(OnExecuteRun);
        }

        private void OnExecuteRun(Window win)               // method called when button is clicked
        {
            Model.MainCode();
            win.Close();
        }
    }
}
