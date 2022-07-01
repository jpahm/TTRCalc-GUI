using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TTRCalc_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Currently selected facility type
        FacilityType SelectedFacility = FacilityType.Sellbot;

        // Point names associated w/ facilities
        private static Dictionary<FacilityType, string> PointTypes = new Dictionary<FacilityType, string>()
        {
            {
                FacilityType.Sellbot,
                "Merits"
            },
            {
                FacilityType.Cashbot,
                "Cogbucks"
            },
            {
                FacilityType.Lawbot,
                "Jury Notices"
            },
            {
                FacilityType.Bossbot,
                "Stock Options"
            }
        };
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FacilitySelected(object sender, RoutedEventArgs e)
        {
            RadioButton FacilityButton = (RadioButton)sender;
            switch (FacilityButton.Name)
            {
                case "SellbotButton":
                    SelectedFacility = FacilityType.Sellbot;
                    break;
                case "CashbotButton":
                    SelectedFacility = FacilityType.Cashbot;
                    break;
                case "LawbotButton":
                    SelectedFacility = FacilityType.Lawbot;
                    break;
                case "BossbotButton":
                    SelectedFacility = FacilityType.Bossbot;
                    break;
            }
            if (NeededPointsBox == null)
                return;
            string PointType = PointTypes[SelectedFacility];
            NeededPointsBox.Text = "";
            NeededPointsText.Text = $"How many {PointType} do you need?";
            CurrentPointsBox.Text = "";
            CurrentPointsText.Text = $"How many {PointType} do you have?";
        }

        private void CalculateButton_Clicked(object sender, RoutedEventArgs e)
        {
            Calculator calc = new Calculator(SelectedFacility);
            uint CurrentPoints;
            uint NeededPoints;
            if (!uint.TryParse(CurrentPointsBox.Text, out CurrentPoints))
                return;
            if (!uint.TryParse(NeededPointsBox.Text, out NeededPoints))
                return;
            if (CurrentPoints > NeededPoints)
                return; 
            OutputList.ItemsSource = calc.CalculateList(NeededPoints - CurrentPoints); ;
        }
    }
}
