using Autodesk.Revit.UI;
using System.Windows;
using Purge_Rooms_UI.PlaceFamily;


namespace Purge_Rooms_UI.PlaceFamily
{
    public partial class PlaceFamilyView : Window
    {
        public PlaceFamilyViewModel ViewModel { get; }

        public PlaceFamilyView(PlaceFamilyModel model)
        {
            ViewModel = new PlaceFamilyViewModel(model);
            DataContext = ViewModel;

            InitializeComponent();

            ViewModel.ExternalEvent = ExternalEvent.Create(new PlaceFamilyExternalEventHandler());
        }
    }
}
