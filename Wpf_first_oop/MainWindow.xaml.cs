using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wpf_first_oop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string MainText = "0";
        private int countOfOpenBrackets = 0;
        private int countOfCloseBrackets = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
 
        void OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string buttonContent = btn.Content.ToString();

            if (buttonContent == "back")
            {
                MainText = RemoveLastCharacter(MainText);
            }
            else if(buttonContent == "C")
            {
                MainText = "0";
            }
            else
            {
                MainText = AppendInput(MainText, buttonContent);

                if (buttonContent == "=" && MainText.Length > 1)
                {
                    MainText = EvaluateExpression(MainText);
                }
            }

            UpdateDisplay(MainText);
        }


        private string AppendInput(string text, string input)
        {
            if (countOfCloseBrackets == countOfOpenBrackets && input == ")")
            {
                return text;
            }
            else if (input == "(")
            {
                countOfOpenBrackets += 1;
            }
            else if (input == ")")
            {
                countOfCloseBrackets += 1;
            }
            if ((text.Length == 1 && text == "-") && !IsOperator(input))
            {
                return text + input;
            }
            else if (text == "0" || (text.Length == 1 && IsOperator(text)))
            {
                return input;
            }
            
            else if (text.Length > 1 && ((IsOperator(input) && IsOperator(text[text.Length - 1].ToString())) || (input == "," && text[text.Length - 1].ToString() == ",")))
            {
                return text.Substring(0, text.Length - 1) + input;
            }
            
            else if (text == "0" && input != "back")
            {
                return input;
            }
            else if (input != "back")
            {
                return text + input;
            }
            return text;
        }

        private string RemoveLastCharacter(string text)
        {
            if (text.Length > 1)
            {
                string newText = text.Substring(0, text.Length - 1);
                return newText;
            }
            else
            {
                return "0";
            }
        }

        private bool IsOperator(string s)
        {
            return s == "=" || s == "+" || s == "-"  || s == "*" || s =="/";
        }

        private bool IsOperator(char ch)
        {
            return ch == '=' || ch == '+' || ch == '-' || ch == '*' || ch == '/';
        }
        private List<string> TokenizeExpression(string expression)
        {
            List<string> tokens = new List<string>();
            string number = "";
            List <string> open_brackets = new List<string>();
            List<string> close_brackets = new List<string>();

            foreach (char ch in expression)
            {
                if (char.IsDigit(ch) || ch == ',')
                {
                    number += ch;
                }
                else
                {
                    if (number.Length > 0)
                    {
                        tokens.Add(number);
                        number = "";
                    }
                    if (IsOperator(ch) || ch == '(' || ch == ')')
                    {
                        if (tokens.Count > 0 && tokens[^1] == "(" && ch == '-')
                        {
                            tokens.Add("0");
                            tokens.Add("-");

                        }
                        else if (tokens.Count > 0 && ch == '('&& (decimal.TryParse(tokens[^1], out _) || tokens[^1] == ")"))
                        {
                            tokens.Add("*");
                            tokens.Add("(");
                        }
                        else if (tokens.Count > 0 && ch == ')' && tokens[^1] == "(")
                        {
                            tokens.Add("0");
                            tokens.Add(")");
                        }
                        
                        else if (tokens.Count > 0 && ch == '-')
                        {
                            tokens.Add("+");
                            tokens.Add("0");
                            tokens.Add("-");
                        }
                        else if (ch == '-')
                        {       
                            tokens.Add("0");
                            tokens.Add("-");
                        }
                        else
                        {
                            tokens.Add(ch.ToString());
                        }
                        if (ch == '(') {
                            open_brackets.Add("(");
                        }
                        else if (ch == ')')
                        {
                            close_brackets.Add(")");
                        }
                        
                    }
                }
            }

            if (number.Length > 0)
            {
                tokens.Add(number);
            }
            if (open_brackets.Count != close_brackets.Count)
            {
                for (int i = 0; i < open_brackets.Count - close_brackets.Count; i++)
                {
                    tokens.Add(")");
                }
            }

            return tokens;
        }


        private string EvaluateExpression(string expression)
        {
            List<string> tokens = TokenizeExpression(expression);
            List<string> outputStack = new List<string>();
            Stack<string> operatorStack = new Stack<string>();

            foreach (string token in tokens)
            {
                if (decimal.TryParse(token, out _))
                {
                    outputStack.Add(token);
                }
                else if (token == "+" || token == "-")
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        outputStack.Add(operatorStack.Pop());
                    }
                    operatorStack.Push(token);
                }
                else if (token == "*" || token == "/")
                {
                    while (operatorStack.Count > 0 && (operatorStack.Peek() == "*" || operatorStack.Peek() == "/"))
                    {
                        outputStack.Add(operatorStack.Pop());
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
                        outputStack.Add(operatorStack.Pop());
                    }
                    operatorStack.Pop();
                }
            }

            while (operatorStack.Count > 0) 
            {
                outputStack.Add(operatorStack.Pop());
            }

            return EvaluateRPN(outputStack);
        }

        private string EvaluateRPN(List<string> outputStack)
        {
            Stack<decimal> evaluationStack = new Stack<decimal>();

            foreach (string token in outputStack)
            {
                if (decimal.TryParse(token, out decimal number))
                {
                    evaluationStack.Push(number);
                }
                else 
                {
                    if (evaluationStack.Count < 2)
                    {
                        MessageBox.Show("Некорректное выражение, подумайте лучше");
                        return "0";
                    }

                    decimal rightOperand = evaluationStack.Pop();
                    decimal leftOperand = evaluationStack.Pop();
                    decimal result = 0;

                    switch (token)
                    {
                        case "+":
                            result = leftOperand + rightOperand;
                            break;
                        case "-":
                            result = leftOperand - rightOperand;
                            break;
                        case "*":
                            result = leftOperand * rightOperand;
                            break;
                        case "/":
                            if (rightOperand == 0)
                            {
                                MessageBox.Show("Деление на ноль запрещено в стране");
                                return "0";
                            }
                            result = leftOperand / rightOperand;
                            break;
                        
                    }

                    evaluationStack.Push(result);
                }
            }

            if (evaluationStack.Count != 1)
            {
                throw new Exception("Некорректное выражение.");
            }

            return evaluationStack.Pop().ToString();
        }

        private void UpdateDisplay(string text)
        {
            Text1.Text = text;
        }
    }
}
