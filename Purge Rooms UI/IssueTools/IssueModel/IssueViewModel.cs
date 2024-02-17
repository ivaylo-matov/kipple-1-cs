using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;
using Autodesk.Revit.UI;

namespace Purge_Rooms_UI
{
    public class IssueViewModel : ViewModelBase
    {
        public IssueModel Model { get; set; }
        public RelayCommand<Window> Process { get; set; }
        public RelayCommand<Window> SelectFolderCommand { get; set; }

        #region Enable CheckBoxes
        // Enable CheckBoxes
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
        private bool _exportIFC;
        public bool ExportIFC
        {
            get { return _exportIFC; }
            set { _exportIFC = value; RaisePropertyChanged(() => ExportIFC); }
        }
        private bool _exportNWC;
        public bool ExportNWC
        {
            get { return _exportNWC; }
            set { _exportNWC = value; RaisePropertyChanged(() => ExportNWC); }
        }
        #endregion

        #region CkechBoxes status
        // Set CheckBoxes status
        private bool _isCheckedRVT;
        public bool IsCheckedRVT
        {
            get { return _isCheckedRVT; }
            set { _isCheckedRVT = value; RaisePropertyChanged(() => IsCheckedRVT);}
        }
        private bool _isCheckedCAD;
        public bool IsCheckedCAD
        {
            get { return _isCheckedCAD; }
            set { _isCheckedCAD = value; RaisePropertyChanged(() => IsCheckedCAD); }
        }
        private bool _isCheckedIMG;
        public bool IsCheckedIMG
        {
            get { return _isCheckedIMG; }
            set { _isCheckedIMG = value; RaisePropertyChanged(() => IsCheckedIMG); }
        }
        private bool _isCheckedViews;
        public bool IsCheckedViews
        {
            get { return _isCheckedViews; }
            set { _isCheckedViews = value; RaisePropertyChanged(() => IsCheckedViews); }
        }
        private bool _isCheckedNonCoordViews;
        public bool IsCheckedNonCoordViews
        {
            get { return _isCheckedNonCoordViews; }
            set { _isCheckedNonCoordViews = value; RaisePropertyChanged(() => IsCheckedNonCoordViews); }
        }
        private bool _isCheckedLib;
        public bool IsCheckedLib
        {
            get { return _isCheckedLib; }
            set { _isCheckedLib = value; RaisePropertyChanged(() => IsCheckedLib); }
        }
        private bool _isCheckedGroups;
        public bool IsCheckedGroups
        {
            get { return _isCheckedGroups; }
            set { _isCheckedGroups = value; RaisePropertyChanged(() => IsCheckedGroups); }
        }
        private bool _isCheckedIFC;
        public bool IsCheckedIFC
        {
            get { return _isCheckedIFC; }
            set { _isCheckedIFC = value; RaisePropertyChanged(() => IsCheckedIFC); }
        }
        #endregion

        // Meta-data labels
        private string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set { _inputText = value; RaisePropertyChanged(() => InputText); }
        }

        private string _issuedTo;
        public string IssuedTo
        {
            get { return _issuedTo; }
            set { _issuedTo = value; RaisePropertyChanged(() => IssuedTo); }
        }
        private string _issuedBy;
        public string IssuedBy
        {
            get { return _issuedBy; }
            set { _issuedBy = value; RaisePropertyChanged(() => IssuedBy); }
        }
        private string _approvedBy;
        public string ApprovedBy
        {
            get { return _approvedBy; }
            set { _approvedBy = value; RaisePropertyChanged(() => ApprovedBy); }
        }
        private string _revDescription;
        public string RevDescription
        {
            get { return _revDescription; }
            set { _revDescription = value; RaisePropertyChanged(() => RevDescription); }
        }
        private string _targetDir;
        public string TargetDir
        {
            get { return _targetDir; }
            set { _targetDir = value; RaisePropertyChanged(() => TargetDir); }
        }
        private string _targetFileName;
        public string TargetFileName
        {
            get { return _targetFileName; }
            set { _targetFileName = value; RaisePropertyChanged(() => TargetFileName); }
        }






        public IssueViewModel(IssueModel model)
        {
            Model = model;

            // Set CheckBox enabled if the model contains relevant elements 
            EnableRVT = model.EnableRVTLinks();
            EnableCAD = model.EnableCADLinks();
            EnableIMG = model.EnableIMGLinks();
            EnableView = model.EnableViews();
            EnableLib = model.EnableLibPhase();
            EnableGroup = model.EnableGroups();

            // Set initial revision parameter values
            IssuedTo = model.CollectCurrentMetaData()["IssuedTo"];
            IssuedBy = model.CollectCurrentMetaData()["IssuedBy"];
            ApprovedBy = model.CollectCurrentMetaData()["ApprovedBy"];
            RevDescription = model.CollectCurrentMetaData()["RevDescription"];
            TargetDir = model.CollectCurrentMetaData()["TargetDir"];
            TargetFileName = model.CollectCurrentMetaData()["TargetFileName"];

            Process = new RelayCommand<Window>(OnExecuteRun);
            SelectFolderCommand = new RelayCommand<Window>(OnSelectFolderCommand);
        }
        private void OnExecuteRun(Window win)
        {
            if (TargetDir != IssueModel.TargetFolderNotFoundMessage)
            {
                try
                {
                    Model.UpdateMetaData(RevDescription, IssuedBy, IssuedTo, ApprovedBy);
                    Model.SyncCloudModel();
                    Model.SaveIssueModel(TargetDir, TargetFileName);

                    if (IsCheckedRVT) Model.RemoveRVTLinks();
                    if (IsCheckedCAD) Model.RemoveCADLinks();
                    if (IsCheckedIMG) Model.RemoveIMGLinks();
                    if (IsCheckedViews) Model.RemoveViews();
                    if (IsCheckedLib) Model.RemoveLibPhaseElements();
                    if (IsCheckedGroups) Model.UngroupGroups();

                    if (IsCheckedIFC) Model.ExportIFC(TargetDir, TargetFileName);

                    MessageBoxResult result = MessageBox.Show(
                        $"Success! The model is ready in {TargetDir}",
                        "Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK) win.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"SHIT.{Environment.NewLine} {ex}",
                        "Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    win.Close();
                }
            }
            else
            {
                MessageBox.Show("Error!", "Please provide a target folder where the file should be saved.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                win.Close();
            }
            
        }

        /// <summary>
        /// Opens a folder browser dialog at the specified target directory.
        /// </summary>
        /// <param name="win"></param>
        private void OnSelectFolderCommand(Window win)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();

            if (TargetDir != IssueModel.TargetFolderNotFoundMessage)
            {
                if (!Directory.Exists(TargetDir)) Directory.CreateDirectory(TargetDir);
                dialog.SelectedPath = TargetDir;
            }     

            WinForms.DialogResult result = dialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                TargetDir = dialog.SelectedPath;
                RaisePropertyChanged(() => TargetDir);
            }
        }
    }
}
