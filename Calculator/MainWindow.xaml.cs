using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculator
{
    public class MyDataObject
    {
        public string Label1 { get; set; }
        public string Label2 { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool LastEval = false;
        public static string Error = "Last Error: ";
        public static string pattern1 = @"[+\-*/](?!.*[+\-*/])";
        public static string pattern2 = @"[+\-*\/]";
        public static Regex regex = new Regex(pattern2);
        public ObservableCollection<MyDataObject> dataObjects;

        public MainWindow()
        {
            InitializeComponent();
            errorLabel.Foreground = Brushes.White;
            dataObjects = new ObservableCollection<MyDataObject>();

            stackPanelContainer.ItemsSource = dataObjects;
        }

        private void numButton_Click(object sender, RoutedEventArgs e)
        {
            var inpt = ((Button)sender).Content?.ToString();

            if (LastEval)
            {
                clrButton_Click(this, new RoutedEventArgs());
                expressionLabel.Content = inpt;
                LastEval = false;
                return;
            }

            var expression = expressionLabel.Content.ToString();
            if (!string.IsNullOrEmpty(expression) && expression[expression.Length - 1] == ')')
            {
                expression += "*";
            }

            string remainingString = GetNumberAfterLastSymbol(expression);

            if (remainingString == "0")
            {
                if (inpt != "0")
                {
                    expression = expression?.Substring(0, expression.Length - 1) + inpt;
                }
            }
            else
            {
                expression += inpt;
            }
            
            EvaluateExpression(expression);
            expressionLabel.Content = expression;
        }

        private void opeButton_Click(object sender, RoutedEventArgs e)
        {
            if (expressionLabel.Content.ToString()?.Length == 0)
            {
                errorLabel.Content = Error + "Invalid Operation.";
                errorLabel.Foreground = Brushes.Red;
                return;
            }

            var opeType = ((Button)sender).Content.ToString();
            var expression = expressionLabel.Content.ToString();
            var lstVal = expression?[expression.Length - 1];

            if (lstVal == '+' || lstVal == '-' || lstVal == '*' || lstVal == '/') 
            {
                expressionLabel.Content = expression?.Substring(0, expression.Length - 1) + opeType;
            }
            else
            {
                expressionLabel.Content = expression + opeType;
            }
            resultLabel.Content = "";
        }
       
        private void clrButton_Click(object sender, RoutedEventArgs e)
        {
            resultLabel.Content = "";
            expressionLabel.Content = "";
            LastEval = false;
        }

        private void evalButton_Click(object sender, RoutedEventArgs e)
        {
            if (expressionLabel.Content.ToString()?.Length == 0)
            {
                errorLabel.Content = Error + "Invalid Operation.";
                errorLabel.Foreground = Brushes.Red;
                return;
            }

            var expression = expressionLabel?.Content.ToString();
            var lstVal = expression?[expression.Length - 1];

            if (lstVal == '+' || lstVal == '-' || lstVal == '*' || lstVal == '/')
            {
                expression = expression?.Substring(0, expression.Length - 1);
            }

            if (regex.IsMatch(expression))
            {
                EvaluateExpression(expression);
            }
            else
            {
                resultLabel.Content = expression;
            }

            var historyData = new MyDataObject() { Label1 = expression, Label2 = resultLabel.Content.ToString() };
            dataObjects.Add(historyData);

            resultLabel.Content = "=" + resultLabel.Content;
            expressionLabel.Content = "";
            LastEval = true;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (expressionLabel.Content.ToString()?.Length == 0)
            {
                errorLabel.Content = Error + "Invalid Operation.";
                errorLabel.Foreground = Brushes.Red;
                return;
            }

            var expression = expressionLabel?.Content.ToString();

            if (expression[expression.Length - 1] == ')')
            {
                expression = expression.Substring(2, expression.Length - 3);
            }
            else
            {
                expression = expression?.Substring(0, expression.Length - 1);
            }

            EvaluateExpression(expression);
            expressionLabel.Content = expression;
        }
        private void invButton_Click(object sender, RoutedEventArgs e)
        {
            if (expressionLabel.Content.ToString()?.Length == 0)
            {
                errorLabel.Content = Error + "Invalid Operation.";
                errorLabel.Foreground = Brushes.Red;
                return;
            }

            var expression = expressionLabel?.Content.ToString();
            var lstVal = expression?[expression.Length - 1];

            if (lstVal == '+' || lstVal == '-' || lstVal == '*' || lstVal == '/')
            {
                return;
            }

            if (expression[0] == '-' && expression[expression.Length - 1] == ')')
            {
                expression = expression.Substring(2, expression.Length - 3);
            }
            else
            {
                expression = "-(" + expression + ")";
            }

            EvaluateExpression(expression);
            expressionLabel.Content = expression;
        }

        private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var bor = sender as Border;

            var ob = bor?.DataContext as MyDataObject;

            LastEval = false;
            expressionLabel.Content = ob?.Label1;
            resultLabel.Content = ob?.Label2;
        }

        private void EvaluateExpression(string expression)
        {
            if (CanEvaluate(expression))
            {
                var loDataTable = new DataTable();
                var loDataColumn = new DataColumn("Eval", typeof(double), expression);
                loDataTable.Columns.Add(loDataColumn);
                loDataTable.Rows.Add(0);
                var result = (double)(loDataTable.Rows[0]["Eval"]);
                resultLabel.Content = Math.Round(result, 2);
            }
            else
            {
                resultLabel.Content = "";
            }
        }

        private bool CanEvaluate(string expression)
        {
            var lstVal = expression?.Length > 0 ? expression?[expression.Length - 1] : '\0';

            if (lstVal == '+' || lstVal == '-' || lstVal == '*' || lstVal == '/' || string.IsNullOrEmpty(expression))
            {
                return false;
            }

            if (regex.IsMatch(expression))
            {
                return true;
            }

            return false;
        }

        private string GetNumberAfterLastSymbol(string? expression)
        {
            Match match = Regex.Match(expression, pattern1);
            string remainingString = "";

            if (match.Success)
            {
                remainingString = expression.Substring(match.Index + 1);
            }

            return remainingString;
        }

    }
}