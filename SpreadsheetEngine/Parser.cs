namespace SpreadsheetEngine;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;

    private Parser(List<Token> tokens) => _tokens = tokens;

    public static Node Parse(string expression)
    {
        var tokens = Lexer.Tokenize(expression);
        var parser = new Parser(tokens);

        if (parser.Current.Type == TokenType.End)
            throw new SyntaxErrorException("порожній вираз", 1);

        Node result = parser.ParseExpression();

        if (parser.Current.Type != TokenType.End)
        {
            var t = parser.Current;
            string hint = t.Type == TokenType.RParen ? "зайва закриваюча дужка" : $"неочікувана лексема \"{t.Text}\"";
            if (IsComparison(t.Type)) hint = "порівняння не можна записувати ланцюжком — використайте дужки";
            if (t.Type is TokenType.Number or TokenType.CellRef or TokenType.LParen)
                hint = $"пропущено знак операції перед \"{t.Text}\"";
            throw new SyntaxErrorException(hint, t.Position, t.Text.Length);
        }
        return result;
    }

    public static SyntaxErrorException? Validate(string expression)
    {
        try { Parse(expression); return null; }
        catch (SyntaxErrorException e) { return e; }
    }

    private Token Current => _tokens[_pos];

    private Token Advance() => _tokens[_pos++];

    private Token Expect(TokenType type, string what)
    {
        if (Current.Type != type)
            throw new SyntaxErrorException($"очікувалось {what}, а знайдено {Describe(Current)}",
                Current.Position, Current.Text.Length);
        return Advance();
    }

    private Node ParseExpression() => ParseNot();

    private Node ParseNot()
    {
        if (Current.Type == TokenType.Not)
        {
            Advance();
            return new NotNode(ParseNot());
        }
        return ParseComparison();
    }

    private Node ParseComparison()
    {
        Node left = ParseAdditive();
        if (IsComparison(Current.Type))
        {
            var op = Advance().Type switch
            {
                TokenType.Equal => BinaryOp.Equal,
                TokenType.Less => BinaryOp.Less,
                _ => BinaryOp.Greater
            };
            left = new BinaryNode(op, left, ParseAdditive());
        }
        return left;
    }

    private Node ParseAdditive()
    {
        Node left = ParseTerm();
        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Advance().Type == TokenType.Plus ? BinaryOp.Add : BinaryOp.Subtract;
            left = new BinaryNode(op, left, ParseTerm());
        }
        return left;
    }

    private Node ParseTerm()
    {
        Node left = ParseOperand();
        while (Current.Type is TokenType.Star or TokenType.Slash)
        {
            var op = Advance().Type == TokenType.Star ? BinaryOp.Multiply : BinaryOp.Divide;
            left = new BinaryNode(op, left, ParseOperand());
        }
        return left;
    }

    private Node ParseOperand()
    {
        Token t = Current;
        switch (t.Type)
        {
            case TokenType.Number:
                Advance();
                return new NumberNode(t.Number);

            case TokenType.CellRef:
                Advance();
                return new CellRefNode(t.Cell);

            case TokenType.True:
                Advance();
                return new BoolNode(true);

            case TokenType.False:
                Advance();
                return new BoolNode(false);

            case TokenType.LParen:
            {
                Advance();
                Node inner = ParseExpression();
                if (Current.Type != TokenType.RParen)
                    throw new SyntaxErrorException(
                        $"очікувалась закриваюча дужка ')' до дужки на позиції {t.Position}, а знайдено {Describe(Current)}",
                        Current.Position, Current.Text.Length);
                Advance();
                return inner;
            }

            case TokenType.Max:
            case TokenType.Min:
            case TokenType.MMax:
            case TokenType.MMin:
                return ParseFunction();

            case TokenType.Not:
                throw new SyntaxErrorException("тут 'not' треба взяти в дужки, напр. A1 = (not B1)", t.Position, t.Text.Length);

            case TokenType.Minus:
                throw new SyntaxErrorException("унарний мінус не підтримується, пишіть напр. 0 - 5", t.Position);

            default:
                throw new SyntaxErrorException(
                    $"очікувалось число, клітинка, true/false, функція або '(', а знайдено {Describe(t)}",
                    t.Position, t.Text.Length);
        }
    }

    private Node ParseFunction()
    {
        Token t = Advance();
        var (kind, name) = t.Type switch
        {
            TokenType.Max => (FunctionKind.Max, "max"),
            TokenType.Min => (FunctionKind.Min, "min"),
            TokenType.MMax => (FunctionKind.MMax, "mmax"),
            _ => (FunctionKind.MMin, "mmin")
        };
        bool exactlyTwo = kind is FunctionKind.Max or FunctionKind.Min;

        Expect(TokenType.LParen, $"'(' після {name}");
        if (Current.Type == TokenType.RParen)
            throw new SyntaxErrorException(
                exactlyTwo ? $"функція {name} приймає рівно два аргументи" : $"функція {name} потребує хоча б один аргумент",
                Current.Position);

        var args = new List<Node> { ParseExpression() };
        while (Current.Type == TokenType.Comma)
        {
            Token comma = Advance();
            if (exactlyTwo && args.Count == 2)
                throw new SyntaxErrorException($"функція {name} приймає рівно два аргументи", comma.Position);
            args.Add(ParseExpression());
        }

        if (exactlyTwo && args.Count != 2)
            throw new SyntaxErrorException($"функція {name} приймає рівно два аргументи", Current.Position, Current.Text.Length);
        Expect(TokenType.RParen, $"')' або ',' в аргументах {name}");
        return new FunctionNode(kind, args);
    }

    private static bool IsComparison(TokenType t) => t is TokenType.Equal or TokenType.Less or TokenType.Greater;

    private static string Describe(Token t) => t.Type == TokenType.End ? "кінець виразу" : $"\"{t.Text}\"";
}
