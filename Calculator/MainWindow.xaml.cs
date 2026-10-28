using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private const int MaxDigits = 16;          

        private string current = "0";             
        private decimal accumulator;              
        private string pendingOp;     
        private bool newEntry;               
        private bool hasOperand;                   
        private bool justEvaluated;            
        private string lastOp;                     
        private decimal lastOperand;
        private string operandText;       
        private bool error;          

        private decimal memory;
        private bool hasMemory;

        private Button[] errorDisabledButtons;

        public MainWindow()
        {
            InitializeComponent();
            errorDisabledButtons = new[]
            {
                btnPercent, btnInv, btnSqr, btnSqrt, btnDiv, btnMul, btnSub, btnAdd,
                btnNegate, btnDot, btnMPlus, btnMMinus, btnMS
            };
            UpdateDisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this); 
        }


        private void Digit_Click(object sender, RoutedEventArgs e) =>
            InputDigit((string)((Button)sender).Content);

        private void Dot_Click(object sender, RoutedEventArgs e) => InputDot();

        private void Operator_Click(object sender, RoutedEventArgs e) =>
            InputOperator((string)((Button)sender).Tag);

        private void Unary_Click(object sender, RoutedEventArgs e) =>
            ApplyUnary((string)((Button)sender).Tag);

        private void Equals_Click(object sender, RoutedEventArgs e) => Evaluate();
        private void Percent_Click(object sender, RoutedEventArgs e) => Percent();
        private void Negate_Click(object sender, RoutedEventArgs e) => Negate();
        private void Backspace_Click(object sender, RoutedEventArgs e) => Backspace();
        private void ClearEntry_Click(object sender, RoutedEventArgs e) => ClearEntry();
        private void Clear_Click(object sender, RoutedEventArgs e) => ClearAll();

        private void MemoryClear_Click(object sender, RoutedEventArgs e)
        {
            memory = 0;
            hasMemory = false;
            UpdateDisplay();
        }

        private void MemoryRecall_Click(object sender, RoutedEventArgs e)
        {
            if (error || !hasMemory) return;
            if (justEvaluated) justEvaluated = false;
            current = Format(memory);
            operandText = null;
            newEntry = true;
            hasOperand = true;
            RefreshHistory();
            UpdateDisplay();
        }

        private void MemoryAdd_Click(object sender, RoutedEventArgs e) => ChangeMemory(+1);
        private void MemorySubtract_Click(object sender, RoutedEventArgs e) => ChangeMemory(-1);

        private void MemoryStore_Click(object sender, RoutedEventArgs e)
        {
            if (error) return;
            memory = CurrentValue;
            hasMemory = true;
            newEntry = true;
            UpdateDisplay();
        }

        private void ChangeMemory(int sign)
        {
            if (error) return;
            try
            {
                memory += sign * CurrentValue;
                hasMemory = true;
                newEntry = true;
            }
            catch (OverflowException)
            {
                ShowError("Переповнення");
                return;
            }
            UpdateDisplay();
        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string t = e.Text;
            if (t.Length != 1) return;
            char c = t[0];

            if (char.IsDigit(c)) InputDigit(t);
            else if (c == '.' || c == ',') InputDot();
            else if (c == '+' || c == '-' || c == '*' || c == '/') InputOperator(t);
            else if (c == '=') Evaluate();
            else if (c == '%') Percent();
            else if (c == 'r' || c == 'R') ApplyUnary("inv"); 
            else if (c == 'q' || c == 'Q') ApplyUnary("sqr");
            else if (c == '@') ApplyUnary("sqrt");             
            else return;

            e.Handled = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: Evaluate(); break;
                case Key.Back: Backspace(); break;
                case Key.Escape: ClearAll(); break;
                case Key.Delete: ClearEntry(); break;
                case Key.F9: Negate(); break;
                default: return;
            }
            e.Handled = true;
        }
        // logic
        private void InputDigit(string d)
        {
            if (error) ClearAll();
            StartNewEntryIfNeeded();

            if (current.Count(char.IsDigit) >= MaxDigits) return;

            if (current == "0") current = d;
            else if (current == "-0") current = "-" + d;
            else current += d;

            UpdateDisplay();
        }

        private void InputDot()
        {
            if (error) ClearAll();
            StartNewEntryIfNeeded();

            if (!current.Contains('.'))
                current += ".";

            UpdateDisplay();
        }

        private void StartNewEntryIfNeeded()
        {
            if (justEvaluated)
            {
                justEvaluated = false;
                txtHistory.Text = "";
            }

            if (newEntry)
            {
                current = "0";
                newEntry = false;
            }

            if (operandText != null)
            {
                operandText = null;
                RefreshHistory();
            }

            hasOperand = true;
        }

        private void InputOperator(string op)
        {
            if (error) return;

            decimal value = CurrentValue;

            if (pendingOp != null && hasOperand)
            {
                if (!TryCompute(accumulator, pendingOp, value, out decimal result)) return;
                accumulator = result;
            }
            else if (pendingOp == null)
            {
                accumulator = value;
            }

            pendingOp = op;
            current = Format(accumulator);
            newEntry = true;
            hasOperand = false;
            justEvaluated = false;
            operandText = null;

            RefreshHistory();
            UpdateDisplay();
        }
        private void Evaluate()
        {
            if (error)
            {
                ClearAll();
                return;
            }

            decimal value = CurrentValue;
            decimal result;

            if (pendingOp != null)
            {
                txtHistory.Text = $"{Format(accumulator)} {Symbol(pendingOp)} {operandText ?? Format(value)} =";
                if (!TryCompute(accumulator, pendingOp, value, out result)) return;

                lastOp = pendingOp;
                lastOperand = value;
                pendingOp = null;
            }
            else if (lastOp != null)
            {
                txtHistory.Text = $"{Format(value)} {Symbol(lastOp)} {Format(lastOperand)} =";
                if (!TryCompute(value, lastOp, lastOperand, out result)) return;
            }
            else
            {
                txtHistory.Text = $"{operandText ?? Format(value)} =";
                result = value;
            }

            accumulator = result;
            current = Format(result);
            operandText = null;
            newEntry = true;
            hasOperand = false;
            justEvaluated = true;

            UpdateDisplay();
        }
        private void ApplyUnary(string kind)
        {
            if (error) return;

            decimal x = CurrentValue;
            decimal result;
            string inner = operandText ?? Format(x);
            string label;
            try
            {
                switch (kind)
                {
                    case "sqr":
                        result = x * x;
                        label = $"sqr({inner})";
                        break;

                    case "sqrt":
                        if (x < 0) { ShowError("Неприпустиме введення"); return; }
                        result = DecimalSqrt(x);
                        label = $"√({inner})";
                        break;

                    case "inv":
                        if (x == 0) { ShowError("Ділення на нуль неможливе"); return; }
                        result = 1 / x;
                        label = $"1/({inner})";
                        break;

                    case "negate":
                        result = -x;
                        label = $"negate({inner})";
                        break;

                    default:
                        return;
                }
            }
            catch (OverflowException)
            {
                ShowError("Переповнення");
                return;
            }
            justEvaluated = false;
            current = Format(result);
            operandText = label;
            newEntry = true;
            hasOperand = true;

            RefreshHistory();
            UpdateDisplay();
        }

        private void Negate()
        {
            if (error) return;

            if (newEntry)
            {
                ApplyUnary("negate");
                return;
            }

            if (current == "0") return;
            current = current.StartsWith("-") ? current.Substring(1) : "-" + current;
            UpdateDisplay();
        }

        private void Percent()
        {
            if (error) return;

            decimal x = CurrentValue;
            decimal result;
            if (pendingOp == "+" || pendingOp == "-")
                result = accumulator * x / 100;
            else if (pendingOp == "*" || pendingOp == "/")
                result = x / 100;
            else
                result = 0;
            justEvaluated = false;
            current = Format(result);
            operandText = Format(result);
            newEntry = true;
            hasOperand = true;

            RefreshHistory();
            UpdateDisplay();
        }

        private void Backspace()
        {
            if (error)
            {
                ClearAll();
                return;
            }
            if (newEntry)
            {
                if (justEvaluated) txtHistory.Text = "";
                return;
            }
            current = current.Substring(0, current.Length - 1);
            if (current == "" || current == "-")
                current = "0";

            UpdateDisplay();
        }
        private void ClearEntry()
        {
            if (error)
            {
                ClearAll();
                return;
            }

            current = "0";
            newEntry = false;
            operandText = null;
            hasOperand = pendingOp != null;
            justEvaluated = false;

            RefreshHistory();
            UpdateDisplay();
        }
        private void ClearAll()
        {
            current = "0";
            accumulator = 0;
            pendingOp = null;
            newEntry = false;
            hasOperand = false;
            justEvaluated = false;
            lastOp = null;
            lastOperand = 0;
            operandText = null;
            error = false;
            txtHistory.Text = "";
            UpdateDisplay();
        }
        private bool TryCompute(decimal a, string op, decimal b, out decimal result)
        {
            result = 0;
            try
            {
                switch (op)
                {
                    case "+": result = a + b; break;
                    case "-": result = a - b; break;
                    case "*": result = a * b; break;
                    case "/":
                        if (b == 0)
                        {
                            ShowError(a == 0 ? "Результат не визначено" : "Ділення на нуль неможливе");
                            return false;
                        }
                        result = a / b;
                        break;
                }
                return true;
            }
            catch (OverflowException)
            {
                ShowError("Переповнення");
                return false;
            }
        }
        private void ShowError(string message)
        {
            error = true;
            current = message;
            pendingOp = null;
            lastOp = null;
            operandText = null;
            newEntry = true;
            hasOperand = false;
            justEvaluated = false;
            UpdateDisplay();
        }

        private decimal CurrentValue =>
            decimal.TryParse(current, NumberStyles.Float, Inv, out decimal v) ? v : 0;
        private void RefreshHistory()
        {
            string text = operandText ?? "";
            if (pendingOp != null)
                text = $"{Format(accumulator)} {Symbol(pendingOp)} {text}".TrimEnd();
            txtHistory.Text = text;
        }

        private void UpdateDisplay()
        {
            txtCurrent.Text = current;

            foreach (var b in errorDisabledButtons)
                b.IsEnabled = !error;

            btnMC.IsEnabled = btnMR.IsEnabled = hasMemory && !error;
        }

        private static string Symbol(string op) => op switch
        {
            "+" => "+",
            "-" => "−",
            "*" => "×",
            "/" => "÷",
            _ => op
        };
        private static string Format(decimal v)
        {
            if (v == 0) return "0";

            decimal abs = Math.Abs(v);

            if (abs >= 1e16m || abs < 1e-15m)
                return ((double)v).ToString("G16", Inv);

            int intDigits = abs >= 1 ? ((long)Math.Truncate(abs)).ToString(Inv).Length : 0;
            int decimals = Math.Max(0, MaxDigits - intDigits);

            decimal rounded = Math.Round(v, decimals, MidpointRounding.AwayFromZero);
            if (rounded == 0) return "0";

            return decimals == 0
                ? rounded.ToString("0", Inv)
                : rounded.ToString("0." + new string('#', decimals), Inv);
        }

        private static decimal DecimalSqrt(decimal x)
        {
            if (x == 0) return 0;
            decimal r = (decimal)Math.Sqrt((double)x);
            for (int i = 0; i < 3; i++)
                r = (r + x / r) / 2;
            return r;
        }
    }
}
