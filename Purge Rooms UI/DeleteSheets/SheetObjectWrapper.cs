using System.ComponentModel;
using Autodesk.Revit.DB;

namespace Purge_Rooms_UI.DeleteSheets
{
    public class SheetObjectWrapper : INotifyPropertyChanged

    {
        public string Name { get; set; }
        public string Number { get; set; }
        public ElementId Id { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged(nameof(IsSelected)); }
        }

        public SheetObjectWrapper(ViewSheet sheet)
        {
            Name = sheet.Name;
            Number = sheet.SheetNumber;
            Id = sheet.Id;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
