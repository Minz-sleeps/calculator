using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace Wpf_first_oop
{
    public partial class MainWindow : Window
    {
        private string _displayText = "0";
        private int _openBracketCount = 0;
        private int _closeBracketCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            UpdateDisplay(_displayText);
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            string buttonContent = button.Content.ToString();

            switch (buttonContent)
            {
                case "n!":
                    buttonContent = "!";
                    break;
                case "10^x":
                    buttonContent = "10^(";
                    _openBracketCount++;
                    break;
                case "sin":
                case "cos":
                case "ln":
                case "log":
                case "exp":
                    buttonContent += "(";
                    _openBracketCount++;
                    break;
                case ",":
                    buttonContent = ".";
                    break;
            }

            if (buttonContent == "back")
            {
                _displayText = RemoveLastCharacter(_displayText);
            }
            else if (buttonContent == "C")
            {
                _displayText = "0";
                _openBracketCount = _closeBracketCount = 0;
            }
            else if (buttonContent == "=")
            {
                if (_displayText.Length > 0 && _displayText != "0")
                {
                    while (_openBracketCount > _closeBracketCount)
                    {
                        _displayText += ")";
                        _closeBracketCount++;
                    }
                    _displayText = EvaluateExpression(_displayText);
                    _openBracketCount = _closeBracketCount = 0;
                }
            }
            else
            {
                _displayText = AppendInput(_displayText, buttonContent);
            }

            UpdateDisplay(_displayText);
        }

        private string AppendInput(string currentText, string input)
        {
            if (input == "(")
            {
                _openBracketCount++;
            }
            else if (input == ")")
            {
                if (_closeBracketCount < _openBracketCount)
                {
                    _closeBracketCount++;
                }
                else
                {
                    return currentText;
                }
            }

            if (currentText == "0" && input != "back" && input != ")")
            {
                if (IsOperator(input) && input != "(")
                {
                    return currentText + input;
                }
                if (char.IsDigit(input.FirstOrDefault()) || input == "(" || IsFunction(input.Replace("(", "")) || IsConstant(input) || input == ".")
                {
                    return input;
                }
            }

            var lastChar = currentText.Length > 0 ? currentText.Last() : '\0';

            if (IsOperator(input))
            {
                if (IsOperator(lastChar.ToString()))
                {
                    if (input == "-" && lastChar == '(')
                    {
                        return currentText + input;
                    }
                    if (input == "-" && (char.IsDigit(lastChar) || lastChar == ')' || IsConstant(lastChar.ToString())))
                    {
                        return currentText + input;
                    }
                    else if (input != "-")
                    {
                        return currentText.Substring(0, currentText.Length - 1) + input;
                    }
                    else
                    {
                        return currentText;
                    }
                }
            }
            else if (input == "." && lastChar == '.')
            {
                return currentText.Substring(0, currentText.Length - 1) + input;
            }

            bool needsImplicitMultiplication = false;

            if (char.IsDigit(lastChar) || lastChar == '.')
            {
                if (input == "(" || IsFunction(input.Replace("(", "")) || IsConstant(input))
                {
                    needsImplicitMultiplication = true;
                }
            }
            else if (lastChar == ')')
            {
                if (input == "(" || IsFunction(input.Replace("(", "")) || IsConstant(input) || char.IsDigit(input.FirstOrDefault()))
                {
                    needsImplicitMultiplication = true;
                }
            }
            else if (IsConstant(lastChar.ToString()))
            {
                if (char.IsDigit(input.FirstOrDefault()) || input == "(" || IsFunction(input.Replace("(", "")) || IsConstant(input))
                {
                    needsImplicitMultiplication = true;
                }
            }

            if (needsImplicitMultiplication)
            {
                return currentText + "*" + input;
            }

            return currentText + input;
        }

        private string RemoveLastCharacter(string text)
        {
            if (text.Length <= 1) return "0";

            char lastChar = text.Last();
            if (lastChar == '(')
            {
                _openBracketCount--;
            }
            else if (lastChar == ')')
            {
                _closeBracketCount--;
            }
            else if (text.EndsWith("sin(") || text.EndsWith("cos(") ||
                     text.EndsWith("ln(") || text.EndsWith("log(") ||
                     text.EndsWith("exp(") || text.EndsWith("10^("))
            {
                int lengthToRemove = 0;
                if (text.EndsWith("sin(")) lengthToRemove = 4;
                else if (text.EndsWith("cos(")) lengthToRemove = 4;
                else if (text.EndsWith("ln(")) lengthToRemove = 3;
                else if (text.EndsWith("log(")) lengthToRemove = 4;
                else if (text.EndsWith("exp(")) lengthToRemove = 4;
                else if (text.EndsWith("10^(")) lengthToRemove = 4;

                _openBracketCount--;
                return text.Substring(0, text.Length - lengthToRemove);
            }
            else if (text.Length > 1 && char.IsDigit(lastChar) && IsConstant(text[text.Length - 2].ToString()))
            {
            }

            return text.Substring(0, text.Length - 1);
        }

        private bool IsOperator(string s) =>
            s == "+" || s == "-" || s == "*" || s == "/" || s == "^";

        private bool IsFunction(string tok) =>
            tok == "cos" || tok == "sin" || tok == "ln" || tok == "exp" || tok == "log";

        private bool IsConstant(string s) => s == "π" || s == "e";

        private List<string> TokenizeExpression(string expr)
        {
            var tokens = new List<string>();
            string buffer = "";

            for (int i = 0; i < expr.Length; i++)
            {
                char ch = expr[i];

                if (char.IsLetter(ch))
                {
                    buffer += ch;
                    if (IsFunction(buffer) || IsConstant(buffer))
                    {
                        tokens.Add(buffer);
                        buffer = "";
                    }
                }
                else if (char.IsDigit(ch) || ch == '.')
                {
                    buffer += ch;
                }
                else
                {
                    if (buffer.Length > 0)
                    {
                        tokens.Add(buffer);
                        buffer = "";
                    }

                    if (ch == '^' && tokens.Any() && tokens.Last() == "10")
                    {
                        tokens.Add("^");
                    }
                    else if ("+-*/()!".Contains(ch) || ch == '^')
                    {
                        tokens.Add(ch.ToString());
                    }
                }
            }

            if (buffer.Length > 0)
                tokens.Add(buffer);

            return tokens;
        }

        private string EvaluateExpression(string expression)
        {
            try
            {
                var tokens = TokenizeExpression(expression);
                var outputQueue = new List<string>();
                var operatorStack = new Stack<string>();

                foreach (var token in tokens)
                {
                    if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        outputQueue.Add(token);
                    }
                    else if (IsConstant(token))
                    {
                        outputQueue.Add(token);
                    }
                    else if (IsFunction(token))
                    {
                        operatorStack.Push(token);
                    }
                    else if (token == "!")
                    {
                        while (operatorStack.Count > 0 && Precedence(operatorStack.Peek()) >= Precedence(token) && operatorStack.Peek() != "(")
                        {
                            outputQueue.Add(operatorStack.Pop());
                        }
                        operatorStack.Push(token);
                    }
                    else if (IsOperator(token))
                    {
                        while (operatorStack.Count > 0 &&
                               Precedence(operatorStack.Peek()) >= Precedence(token) &&
                               operatorStack.Peek() != "(" &&
                               !(token == "^" && operatorStack.Peek() == "^"))
                        {
                            outputQueue.Add(operatorStack.Pop());
                        }
                        operatorStack.Push(token);
                    }
                    else if (token == "(")
                    {
                        operatorStack.Push(token);
                    }
                    else if (token == ")")
                    {
                        while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                        {
                            outputQueue.Add(operatorStack.Pop());
                        }
                        if (operatorStack.Count > 0)
                        {
                            operatorStack.Pop();
                        }
                        if (operatorStack.Count > 0 && IsFunction(operatorStack.Peek()))
                        {
                            outputQueue.Add(operatorStack.Pop());
                        }
                    }
                }

                while (operatorStack.Count > 0)
                {
                    var op = operatorStack.Pop();
                    if (op == "(")
                    {
                        return "Закрывай скобки и открывай правильно";
                    }
                    outputQueue.Add(op);
                }

                return EvaluateRPN(outputQueue);
            }
            catch (DivideByZeroException)
            {
                MessageBox.Show("ДЕЛЕНИЕ НА НОЛЬ, ALERT", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "Error";
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка вычисления: {ex.Message}", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "Error";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Переделай: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "Error";
            }
        }

        private int Precedence(string op) => op switch
        {
            "!" => 5,
            "^" => 4,
            "sin" or "cos" or "ln" or "log" or "exp" => 4,
            "*" or "/" => 3,
            "+" or "-" => 2,
            "(" => 1,
            _ => 0
        };

        private string EvaluateRPN(List<string> rpnTokens)
        {
            var stack = new Stack<double>();

            foreach (var token in rpnTokens)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                {
                    stack.Push(number);
                }
                else if (IsConstant(token))
                {
                    if (token == "e") stack.Push(Math.E);
                    else if (token == "π") stack.Push(Math.PI);
                }
                else if (token == "!")
                {
                    if (stack.Count < 1) throw new InvalidOperationException("Переделай");
                    double operand = stack.Pop();
                    stack.Push(Factorial(operand));
                }
                else if (IsFunction(token))
                {
                    if (stack.Count < 1) throw new InvalidOperationException("Переделай");
                    double operand = stack.Pop();
                    double result = token switch
                    {
                        "sin" => Math.Sin(operand),
                        "cos" => Math.Cos(operand),
                        "ln" => Math.Log(operand),
                        "log" => Math.Log10(operand),
                        "exp" => Math.Exp(operand),
                        _ => throw new InvalidOperationException($"Неизвестная функция: {token}")
                    };
                    stack.Push(result);
                }
                else if (IsOperator(token))
                {
                    if (stack.Count < 2) throw new InvalidOperationException($"Пропустил оператор: {token}");
                    double operand2 = stack.Pop();
                    double operand1 = stack.Pop();
                    double result = token switch
                    {
                        "+" => operand1 + operand2,
                        "-" => operand1 - operand2,
                        "*" => operand1 * operand2,
                        "/" => operand2 == 0 ? throw new DivideByZeroException() : operand1 / operand2,
                        "^" => Math.Pow(operand1, operand2),
                        _ => throw new InvalidOperationException($"Неизвестность: {token}")
                    };
                    stack.Push(result);
                }
            }

            if (stack.Count == 1)
            {
                double finalResult = stack.Pop();
                if (Math.Abs(finalResult) < 1e-10)
                {
                    return "0";
                }
                return finalResult.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return "Ошибка, неверное выражение";
            }
        }

        private double Factorial(double number)
        {
            if (number < 0 || number != Math.Floor(number))
            {
                throw new ArgumentException("Факториал принимает только int");
            }
            long n = (long)number;
            double result = 1;
            for (long i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private void UpdateDisplay(string text)
        {
            Text1.Text = text;
        }
    }
}