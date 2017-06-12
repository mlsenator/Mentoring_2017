using System;

namespace SimpleCalculator
{
	public class Caluclator : ICalculator
	{
        private const string AvailableOperators = "+-";

        public CalculatedResult Process(string expression)
		{
			var parsedInput = Parse(expression);
			if (parsedInput.Success)
			{
				long result = 0;

				switch (parsedInput.Operator)
				{
					case "+":
						result = Sum(parsedInput.FirstArgument, parsedInput.SecondArgument);
						break;

					case "-":
						result = Subtract(parsedInput.FirstArgument, parsedInput.SecondArgument);
						break;
				}

				return new CalculatedResult
				{
					Result = result,
					Success = true
				};
			}

			return new CalculatedResult
			{
				Success = false
			};
		}

        private ParsedInput Parse(string expression)
        {
            char[] delimiters = { ' ' };
            var operands = expression.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            int firstArgument;
            int secondArgument;

            if (operands.Length == 3 && int.TryParse(operands[0], out firstArgument)
                    && int.TryParse(operands[2], out secondArgument) && AvailableOperators.Contains(operands[1]))
            {
                return new ParsedInput
                {
                    FirstArgument = firstArgument,
                    SecondArgument = secondArgument,
                    Operator = operands[1],
                    Success = true
                };
            }

            return new ParsedInput
            {
                Success = false
            };
        }

        private long Sum(int firstArgument, int secondArgument)
        {
            return firstArgument + secondArgument;
        }

        private long Subtract(int firstArgument, int secondArgument)
        {
            return firstArgument - secondArgument;
        }
    }
}
