using System.Numerics;

namespace SpreadsheetEngine;

public interface ICellValueProvider
{
    Value GetCellValue(CellAddress address);
}

public abstract record Node
{
    public abstract Value Evaluate(ICellValueProvider cells);

    public abstract IEnumerable<CellAddress> References();
}

public sealed record NumberNode(BigInteger Value) : Node
{
    public override Value Evaluate(ICellValueProvider cells) => SpreadsheetEngine.Value.FromNumber(Value);
    public override IEnumerable<CellAddress> References() => [];
}

public sealed record BoolNode(bool Value) : Node
{
    public override Value Evaluate(ICellValueProvider cells) => SpreadsheetEngine.Value.FromBool(Value);
    public override IEnumerable<CellAddress> References() => [];
}

public sealed record CellRefNode(CellAddress Address) : Node
{
    public override Value Evaluate(ICellValueProvider cells) => cells.GetCellValue(Address);
    public override IEnumerable<CellAddress> References() => [Address];
}

public sealed record NotNode(Node Operand) : Node
{
    public override Value Evaluate(ICellValueProvider cells) =>
        SpreadsheetEngine.Value.FromBool(!Operand.Evaluate(cells).AsBool());

    public override IEnumerable<CellAddress> References() => Operand.References();
}

public enum BinaryOp { Add, Subtract, Multiply, Divide, Equal, Less, Greater }

public sealed record BinaryNode(BinaryOp Op, Node Left, Node Right) : Node
{
    public override Value Evaluate(ICellValueProvider cells)
    {
        Value l = Left.Evaluate(cells);
        Value r = Right.Evaluate(cells);

        return Op switch
        {
            BinaryOp.Add => SpreadsheetEngine.Value.FromNumber(l.AsNumber() + r.AsNumber()),
            BinaryOp.Subtract => SpreadsheetEngine.Value.FromNumber(l.AsNumber() - r.AsNumber()),
            BinaryOp.Multiply => SpreadsheetEngine.Value.FromNumber(l.AsNumber() * r.AsNumber()),
            BinaryOp.Divide => r.AsNumber().IsZero
                ? throw new EvaluationException("Ділення на нуль")
                : SpreadsheetEngine.Value.FromNumber(BigInteger.Divide(l.AsNumber(), r.AsNumber())),
            BinaryOp.Equal => SpreadsheetEngine.Value.FromBool(l.Equals(r)),
            BinaryOp.Less => SpreadsheetEngine.Value.FromBool(l.AsNumber() < r.AsNumber()),
            BinaryOp.Greater => SpreadsheetEngine.Value.FromBool(l.AsNumber() > r.AsNumber()),
            _ => throw new InvalidOperationException()
        };
    }

    public override IEnumerable<CellAddress> References() => Left.References().Concat(Right.References());
}

public enum FunctionKind { Max, Min, MMax, MMin }

public sealed record FunctionNode(FunctionKind Kind, IReadOnlyList<Node> Args) : Node
{
    public override Value Evaluate(ICellValueProvider cells)
    {
        var values = Args.Select(a => a.Evaluate(cells).AsNumber()).ToList();
        BigInteger result = Kind is FunctionKind.Max or FunctionKind.MMax
            ? values.Aggregate(BigInteger.Max)
            : values.Aggregate(BigInteger.Min);
        return SpreadsheetEngine.Value.FromNumber(result);
    }

    public override IEnumerable<CellAddress> References() => Args.SelectMany(a => a.References());
}
