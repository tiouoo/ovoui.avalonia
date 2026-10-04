namespace OvoUi.Controls;

public class OvoNumericIntUpDown : OvoNumericUpDownBase<int>
{
    static OvoNumericIntUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericIntUpDown>(int.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericIntUpDown>(int.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericIntUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override int Zero => 0;

    protected override bool ParseText(string? text, out int number) =>
        int.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(int? value)
    {
        return value?.ToString(FormatString, NumberFormat);
    }

    protected override int? Add(int? a, int? b)
    {
        var result = a + b;
        return result < Value ? Maximum : result;
    }

    protected override int? Minus(int? a, int? b)
    {
        var result = a - b;
        return result > Value ? Minimum : result;
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericUIntUpDown : OvoNumericUpDownBase<uint>
{
    static OvoNumericUIntUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericUIntUpDown>(uint.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericUIntUpDown>(uint.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericUIntUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override uint Zero => 0;

    protected override bool ParseText(string? text, out uint number)
    {
        return uint.TryParse(text, ParsingNumberStyle, NumberFormat, out number);
    }

    protected override string? ValueToString(uint? value)
    {
        return value?.ToString(FormatString, NumberFormat);
    }

    protected override uint? Add(uint? a, uint? b)
    {
        var result = a + b;
        return result < Value ? Maximum : result;
    }

    protected override uint? Minus(uint? a, uint? b)
    {
        var result = a - b;
        return result > Value ? Minimum : result;
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericDoubleUpDown : OvoNumericUpDownBase<double>
{
    static OvoNumericDoubleUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericDoubleUpDown>(double.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericDoubleUpDown>(double.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericDoubleUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override double Zero => 0;

    protected override bool ParseText(string? text, out double number) =>
        double.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(double? value) => value?.ToString(FormatString, NumberFormat);

    protected override double? Add(double? a, double? b) => a + b;

    protected override double? Minus(double? a, double? b) => a - b;

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericByteUpDown : OvoNumericUpDownBase<byte>
{
    static OvoNumericByteUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericByteUpDown>(byte.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericByteUpDown>(byte.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericByteUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override byte Zero => 0;

    protected override bool ParseText(string? text, out byte number) =>
        byte.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(byte? value) => value?.ToString(FormatString, NumberFormat);

    protected override byte? Add(byte? a, byte? b)
    {
        var result = a + b;
        return (byte?)(result < Value ? Maximum : result);
    }

    protected override byte? Minus(byte? a, byte? b)
    {
        var result = a - b;
        return (byte?)(result > Value ? Minimum : result);
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericSByteUpDown : OvoNumericUpDownBase<sbyte>
{
    static OvoNumericSByteUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericSByteUpDown>(sbyte.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericSByteUpDown>(sbyte.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericSByteUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override sbyte Zero => 0;

    protected override bool ParseText(string? text, out sbyte number) =>
        sbyte.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(sbyte? value) => value?.ToString(FormatString, NumberFormat);

    protected override sbyte? Add(sbyte? a, sbyte? b)
    {
        var result = a + b;
        return (sbyte?)(result < Value ? Maximum : result);
    }

    protected override sbyte? Minus(sbyte? a, sbyte? b)
    {
        var result = a - b;
        return (sbyte?)(result > Value ? Minimum : result);
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericShortUpDown : OvoNumericUpDownBase<short>
{
    static OvoNumericShortUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericShortUpDown>(short.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericShortUpDown>(short.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericShortUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override short Zero => 0;

    protected override bool ParseText(string? text, out short number) =>
        short.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(short? value) => value?.ToString(FormatString, NumberFormat);

    protected override short? Add(short? a, short? b)
    {
        var result = a + b;
        return (short?)(result < Value ? Maximum : result);
    }

    protected override short? Minus(short? a, short? b)
    {
        var result = a - b;
        return (short?)(result > Value ? Minimum : result);
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericUShortUpDown : OvoNumericUpDownBase<ushort>
{
    static OvoNumericUShortUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericUShortUpDown>(ushort.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericUShortUpDown>(ushort.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericUShortUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override ushort Zero => 0;

    protected override bool ParseText(string? text, out ushort number) =>
        ushort.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(ushort? value) => value?.ToString(FormatString, NumberFormat);

    protected override ushort? Add(ushort? a, ushort? b)
    {
        var result = a + b;
        return (ushort?)(result < Value ? Maximum : result);
    }

    protected override ushort? Minus(ushort? a, ushort? b)
    {
        var result = a - b;
        return (ushort?)(result > Value ? Minimum : result);
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericLongUpDown : OvoNumericUpDownBase<long>
{
    static OvoNumericLongUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericLongUpDown>(long.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericLongUpDown>(long.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericLongUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override long Zero => 0;

    protected override bool ParseText(string? text, out long number) =>
        long.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(long? value) => value?.ToString(FormatString, NumberFormat);

    protected override long? Add(long? a, long? b)
    {
        var result = a + b;
        return result < Value ? Maximum : result;
    }

    protected override long? Minus(long? a, long? b)
    {
        var result = a - b;
        return result > Value ? Minimum : result;
    }

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericULongUpDown : OvoNumericUpDownBase<ulong>
{
    static OvoNumericULongUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericULongUpDown>(ulong.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericULongUpDown>(ulong.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericULongUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override ulong Zero => 0;

    protected override bool ParseText(string? text, out ulong number) =>
        ulong.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(ulong? value) => value?.ToString(FormatString, NumberFormat);

    protected override ulong? Add(ulong? a, ulong? b) => a + b;

    protected override ulong? Minus(ulong? a, ulong? b) => a - b;

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericFloatUpDown : OvoNumericUpDownBase<float>
{
    static OvoNumericFloatUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericFloatUpDown>(float.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericFloatUpDown>(float.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericFloatUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override float Zero => 0;

    protected override bool ParseText(string? text, out float number) =>
        float.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(float? value) => value?.ToString(FormatString, NumberFormat);

    protected override float? Add(float? a, float? b) => a + b;

    protected override float? Minus(float? a, float? b) => a - b;

    public override void Clear()
    {
        base.Clear();
    }
}

public class OvoNumericDecimalUpDown : OvoNumericUpDownBase<decimal>
{
    static OvoNumericDecimalUpDown()
    {
        MaximumProperty.OverrideDefaultValue<OvoNumericDecimalUpDown>(decimal.MaxValue);
        MinimumProperty.OverrideDefaultValue<OvoNumericDecimalUpDown>(decimal.MinValue);
        StepProperty.OverrideDefaultValue<OvoNumericDecimalUpDown>(1);
    }

    protected override Type StyleKeyOverride { get; } = typeof(OvoNumericUpDown);

    protected override decimal Zero => 0;

    protected override bool ParseText(string? text, out decimal number) =>
        decimal.TryParse(text, ParsingNumberStyle, NumberFormat, out number);

    protected override string? ValueToString(decimal? value) => value?.ToString(FormatString, NumberFormat);

    protected override decimal? Add(decimal? a, decimal? b) => a + b;

    protected override decimal? Minus(decimal? a, decimal? b) => a - b;

    public override void Clear()
    {
        base.Clear();
    }
}