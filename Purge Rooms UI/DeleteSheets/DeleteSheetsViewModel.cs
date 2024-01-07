using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System;
using System.Text;

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
