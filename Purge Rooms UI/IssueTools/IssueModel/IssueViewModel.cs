using System.Windows;
using Autodesk.Revit.DB;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Web.Caching;

namespace Purge_Rooms_UI
{ 
    public class IssueViewModel : ViewModelBase
    {
        public IssueModel Model { get; set; }
        public RelayCommand<Window> Process { get; set; }

        private bool _enableRVT;
        public bool EnableRVT
        {
            get { return _enableRVT; }
            set { _enableRVT = value; RaisePropertyChanged(() => EnableRVT); }
        }
        private bool _enableCAD;
        public bool EnableCAD
        {
            get { return _enableCAD; }
            set { _enableCAD = value; RaisePropertyChanged(() => EnableCAD); }
        }
        private bool _enableIMG;
        public bool EnableIMG
        {
            get { return _enableIMG; }
            set { _enableIMG = value; RaisePropertyChanged(() => EnableIMG); }
        }
        private bool _enableView;
        public bool EnableView
        {
            get { return _enableView; }
            set { _enableView = value; RaisePropertyChanged(() => EnableView); }
        }
        private bool _enableLib;
        public bool EnableLib
        {
            get { return _enableLib; }
            set { _enableLib = value; RaisePropertyChanged(() => EnableLib); }
        }
        private bool _enableGroup;
        public bool EnableGroup
        {
            get { return _enableGroup; }
            set { _enableGroup = value; RaisePropertyChanged(() => EnableGroup); }
        }



        public IssueViewModel(IssueModel model)
        {
            Model = model;

            EnableRVT = model.EnableRVTLinks();
            EnableCAD = model.EnableCADLinks();
            EnableIMG = model.EnableIMGLinks();
            EnableView = model.EnableViews();
            EnableLib = model.EnableLibPhase();
            EnableGroup = model.EnableGroups();

            Process = new RelayCommand<Window>(OnExecuteRun);
        }
        private void OnExecuteRun(Window win)
        {
            Model.Run();
            win.Show();
        }
    }
}
