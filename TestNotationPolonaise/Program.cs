using System.Globalization;

namespace TestNotationPolonaise;

internal static class Program
{
    private static string TryReadConsole()
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (input == null)
            {
                Console.WriteLine("Please enter a valid input");
            }
            else return input;
        }
    }
    
    private static char Prompt(string message)
    {
        char response;
        do
        {
            Console.WriteLine();
            Console.Write(message + " (y/n) ");
            response = Console.ReadKey().KeyChar;
        } while (response != 'y' && response != 'n');

        return response;
    }

    private static void Main()
    {
        char response;
        do
        {
            Console.WriteLine();
            Console.WriteLine("Enter a Polish formula, separating each part with a space = ");

            var expression = TryReadConsole();
            var parsedEval = "Error parsing polish formula. See above for more details.";
            try
            {
                parsedEval = Evaluator.Eval(expression, new Dictionary<string, double>())
                    .ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Result =  " + parsedEval);
            response = Prompt("Would you like to continue?");
        } while (response == 'y');
    }

    /// <summary>
    /// The Evaluator class provides a method to evaluate mathematical expressions
    /// using Prefix Notation (PN), including support for basic arithmetic operations and functions.
    /// </summary>
    private static class Evaluator
    {
        /// <summary>
        /// Evaluates the given mathematical expression in Prefix Notation (PN) using a dictionary of variables.
        /// </summary>
        /// <param name="exp">The expression in Prefix Notation (e.g., "- / 6 2 3").</param>
        /// <param name="vars">A dictionary of variable names and their values, used to replace variables in the expression.</param>
        /// <returns>The result of the evaluated expression.</returns>
        /// <exception cref="InvalidExpressionException">Thrown when the expression format is invalid or unknown tokens are encountered.</exception>
        /// <exception cref="DivideByZeroException">Thrown when there is an attempt to divide by zero.</exception>
        public static double Eval(string exp, Dictionary<string, double> vars)
        {
            try
            {
                return new Parser(exp, vars).Parse();
            }
            catch (Exception ex) when (ex is InvalidExpressionException or DivideByZeroException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidExpressionException($"Error evaluating expression: {ex.Message}");
            }
        }

        /// <summary>
        /// Internal Parser class for parsing and evaluating Prefix Notation (PN) expressions.
        /// </summary>
        private class Parser
        {
            private readonly string[] _tokens;
            private readonly Dictionary<string, double> _vars;
            private int _currentToken;
            private bool _expectOperator = true;

            /// <summary>
            /// Initializes a new instance of the Parser class for evaluating expressions.
            /// </summary>
            /// <param name="exp">The mathematical expression in Prefix Notation (PN).</param>
            /// <param name="vars">The dictionary of variables to use in the expression.</param>
            public Parser(string exp, Dictionary<string, double> vars)
            {
                _tokens = exp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _vars = vars;
                _currentToken = 0;
            }

            /// <summary>
            /// Parses and evaluates the entire expression.
            /// </summary>
            /// <returns>The result of the evaluated expression.</returns>
            /// <exception cref="InvalidExpressionException">Thrown when the expression is empty or invalid.</exception>
            public double Parse()
            {
                if (_currentToken >= _tokens.Length)
                {
                    throw new InvalidExpressionException(
                        "Empty or invalid expression");
                }

                return ParseNext();
            }

            /// <summary>
            /// Parses the next token and evaluates it.
            /// </summary>
            /// <returns>The result of the current parsed token.</returns>
            /// <exception cref="InvalidExpressionException">Thrown when an unknown token or invalid operator order is encountered.</exception>
            private double ParseNext()
            {
                if (_currentToken >= _tokens.Length)
                {
                    throw new InvalidExpressionException(
                        "Unexpected end of expression");
                }

                var token = _tokens[_currentToken++];

                // If it's a number, return it directly
                if (!double.TryParse(token, out var number))
                    return _vars.TryGetValue(token, out var variable)
                        ? variable
                        :
                        // Otherwise, it's an operator or function, apply it
                        ApplyOperatorOrFunction(token);

                if (_tokens.Length > 1 && _expectOperator)
                {
                    throw new InvalidExpressionException(
                        "Invalid expression: numbers must be prefixed with an operator in PN");
                }
                return number;
            }

            /// <summary>
            /// Applies an operator or function to the next token(s) in the expression.
            /// </summary>
            /// <param name="token">The operator or function token.</param>
            /// <returns>The result of applying the operator or function.</returns>
            /// <exception cref="InvalidExpressionException">Thrown when an unknown operator or function is encountered.</exception>
            private double ApplyOperatorOrFunction(string token)
            {
                _expectOperator = false;
                return token switch
                {
                    // Basic operators
                    "+" => ParseNext() + ParseNext(),
                    "-" => ParseNext() - ParseNext(),
                    "*" => ParseNext() * ParseNext(),
                    "/" => Divide(),
                    "^" => Math.Pow(ParseNext(), ParseNext()),

                    // Unary operators and functions
                    "sqrt" => Math.Sqrt(ParseNext()),
                    "sin" => Math.Sin(ParseNext() * Math.PI / 180),
                    "cos" => Math.Cos(ParseNext() * Math.PI / 180),
                    "tan" => Math.Tan(ParseNext() * Math.PI / 180),

                    // Invalid token handling
                    _ => throw new InvalidExpressionException($"Unknown operator or function: {token}")
                };
            }

            /// <summary>
            /// Handles division, checking for divide-by-zero cases.
            /// </summary>
            /// <returns>The result of the division.</returns>
            /// <exception cref="DivideByZeroException">Thrown when division by zero occurs.</exception>
            private double Divide()
            {
                var d1 = ParseNext();
                var d2 = ParseNext();
                if (d2 == 0)
                {
                    throw new DivideByZeroException("Division by zero is not allowed");
                }

                return d1 / d2;
            }
        }

        /// <summary>
        /// Custom exception class to represent invalid expression errors.
        /// </summary>
        private class InvalidExpressionException(string message) : Exception(message);
    }
}