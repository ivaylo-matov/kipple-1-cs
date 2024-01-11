using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Purge_Rooms_UI.PlaceFamily
{
    public class PlaceFamilyViewModel
    {
        public ExternalEvent ExternalEvent { get; set; }
        private readonly PlaceFamilyModel model;

        public PlaceFamilyViewModel(PlaceFamilyModel model)
        {
            this.model = model;
            ExternalEvent = ExternalEvent.Create(new PlaceFamilyExternalEventHandler());

            SelectAndPlaceFamilyCommand = new RelayCommand(SelectAndPlaceFamily);
        }

        public ICommand SelectAndPlaceFamilyCommand { get; }

        private void SelectAndPlaceFamily()
        {
            try
            {
                ExternalEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error: {ex.Message}");
            }
        }
    }
}


