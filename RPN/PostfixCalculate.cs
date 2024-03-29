using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RPN
{
    internal class PostfixCalculate
    {
        private readonly IEnumerable<IOperator> _operators;
        private readonly Stack<decimal> _stack;
        private readonly Queue<string> _queue;

        public PostfixCalculate(IEnumerable<IOperator> operators)
        {
            _operators = operators;
            _stack = new Stack<decimal>();
            _queue = new Queue<string>();
        }

        public decimal Calculate(string postfixString)
        {
            _stack.Clear();
            _queue.Clear();

            foreach (var entry in postfixString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                _queue.Enqueue(entry);

            while (_queue.Count > 0)
            {
                var entry = _queue.Dequeue();

                if (string.IsNullOrEmpty(entry))
                    continue;

                if (!IsOperator(entry))
                {
                    _stack.Push(decimal.Parse(entry, new NumberFormatInfo{ NumberDecimalSeparator = "." }));
                }

                else if (IsUnaryOperator(entry))
                {
                    var x = _stack.Pop();
                    foreach (var @operator in _operators)
                    {
                        if (@operator.Symbol == entry[0])
                        {
                            var op = (IUnaryOperator)@operator;
                            _stack.Push(op.Calculate(x));
                            break;
                        }
                    }
                }
                else if (IsBinaryOperator(entry))
                {
                    if (_stack.Count < 2)
                        throw new InvalidOperationException("Operator imbalance.");

                    var rightOperand = _stack.Pop();
                    var leftOperand = _stack.Pop();
                    foreach (var @operator in _operators)
                    {
                        if (@operator.Symbol == entry[0])
                        {
                            var op = (IBinaryOperator)@operator;
                            _stack.Push(op.Calculate(leftOperand, rightOperand));
                            break;
                        }
                    }
                }
                else
                    throw new Exception("Unknown symbol!");
            }

            if (_stack.Count > 1)
                throw new Exception("More than one result on the stack.");
            if (_stack.Count < 1)
                throw new KeyNotFoundException("No results on the stack.");

            return _stack.Pop();
        }

        private bool IsUnaryOperator(string entry)
        {
            return _operators.Any(op => op.Symbol == entry[0] && op.IsUnary);
        }

        private bool IsBinaryOperator(string entry)
        {
            return _operators.Any(op => op.Symbol == entry[0] && op.IsBinary);
        }

        private bool IsOperator(string entry)
        {
            return IsBinaryOperator(entry) || IsUnaryOperator(entry);
        }

    }
}