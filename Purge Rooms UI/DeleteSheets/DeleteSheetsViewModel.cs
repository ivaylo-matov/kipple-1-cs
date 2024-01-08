using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Events;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Purge_Rooms_UI.DeleteSheets
{ 
    public class DeleteSheetsViewModel : ViewModelBase
    {
        public DeleteSheetsModel Model { get; set; }
        public RelayCommand<Window> Close {  get; set; }
        public RelayCommand<Window> Delete {  get; set; }

        private ObservableCollection<SheetObjectWrapper> _sheetObjects;
        public ObservableCollection<SheetObjectWrapper> SheetObjects
        {
            get { return _sheetObjects; }
            set { _sheetObjects = value; RaisePropertyChanged(() => SheetObjects); }
        }

        public DeleteSheetsViewModel(DeleteSheetsModel model)
        {
            Model = model;
            SheetObjects = model.CollectionSheetObjects();
            Close = new RelayCommand<Window>(OnClose);
            Delete = new RelayCommand<Window>(OnDelete);
        }

        private void OnClose(Window DeleteSheetsWindow)
        {
            DeleteSheetsWindow.Close();
        }
        
        private void OnDelete(Window DeleteSheetsWindow)
        {
            var selected = SheetObjects.Where(s => s.IsSelected).ToList();
            Model.Delete(selected);
            DeleteSheetsWindow.Close();
        }
    }
}
