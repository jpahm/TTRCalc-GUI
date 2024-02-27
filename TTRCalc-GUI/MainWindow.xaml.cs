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
        // The calculator being used
        Calculator calculator = new Calculator();

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

        private void DoCalculation()
        {
            if (ShortsPreferredBox is null)
                return;
            uint CurrentPoints;
            uint NeededPoints;
            if (!uint.TryParse(CurrentPointsBox.Text, out CurrentPoints))
                CurrentPoints = 0;
            if (!uint.TryParse(NeededPointsBox.Text, out NeededPoints))
                NeededPoints = 0;
            if (CurrentPoints > NeededPoints)
                return;
            OutputList.ItemsSource = calculator.CalculateList(NeededPoints - CurrentPoints);
        }

        private void FacilitySelected(object sender, RoutedEventArgs e)
        {
            RadioButton FacilityButton = (RadioButton)sender;
            switch (FacilityButton.Name)
            {
                case "SellbotButton":
                    calculator.SelectedFacilityType = FacilityType.Sellbot;
                    break;
                case "CashbotButton":
                    calculator.SelectedFacilityType = FacilityType.Cashbot;
                    break;
                case "LawbotButton":
                    calculator.SelectedFacilityType = FacilityType.Lawbot;
                    break;
                case "BossbotButton":
                    calculator.SelectedFacilityType = FacilityType.Bossbot;
                    break;
            }
            if (NeededPointsBox == null)
                return;
            if (calculator.SelectedFacilityType != FacilityType.Sellbot)
            {
                ShortsPreferredBox.Visibility = Visibility.Hidden;
            }
            else
            {
                ShortsPreferredBox.Visibility = Visibility.Visible;
            }
            string PointType = PointTypes[calculator.SelectedFacilityType];
            NeededPointsBox.Text = "";
            NeededPointsText.Text = $"How many {PointType} do you need?";
            CurrentPointsBox.Text = "0";
            CurrentPointsText.Text = $"How many {PointType} do you have?";
        }

        void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var TextBox = (TextBox)sender;
            if (!TextBox.IsKeyboardFocusWithin)
            {
                TextBox.Focus();
                e.Handled = true;
            }
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoCalculation();
        }

        private void Box_Clicked(object sender, RoutedEventArgs e)
        {
            DoCalculation();
        }
    }
}
