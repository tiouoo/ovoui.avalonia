using Avalonia.Animation;

namespace OvoUi.Controls;

public class Int32Displayer : NumberDisplayer<int>
{
    protected override Type StyleKeyOverride { get; } = typeof(NumberDisplayerBase);

    protected override InterpolatingAnimator<int> GetAnimator()
    {
        return new IntAnimator();
    }

    protected override string GetString(int value)
    {
        return value.ToString(StringFormat);
    }

    private class IntAnimator : InterpolatingAnimator<int>
    {
        public override int Interpolate(double progress, int oldValue, int newValue)
        {
            return oldValue + (int)((newValue - oldValue) * progress);
        }
    }
}

public class Int64Displayer : NumberDisplayer<long>
{
    protected override Type StyleKeyOverride { get; } = typeof(NumberDisplayerBase);

    protected override InterpolatingAnimator<long> GetAnimator()
    {
        return new LongAnimator();
    }

    protected override string GetString(long value)
    {
        return value.ToString(StringFormat);
    }

    private class LongAnimator : InterpolatingAnimator<long>
    {
        public override long Interpolate(double progress, long oldValue, long newValue)
        {
            return oldValue + (long)((newValue - oldValue) * progress);
        }
    }
}

public class DoubleDisplayer : NumberDisplayer<double>
{
    protected override Type StyleKeyOverride { get; } = typeof(NumberDisplayerBase);

    protected override InterpolatingAnimator<double> GetAnimator()
    {
        return new DoubleAnimator();
    }

    protected override string GetString(double value)
    {
        return value.ToString(StringFormat);
    }

    private class DoubleAnimator : InterpolatingAnimator<double>
    {
        public override double Interpolate(double progress, double oldValue, double newValue)
        {
            return oldValue + (newValue - oldValue) * progress;
        }
    }
}

public class DateDisplay : NumberDisplayer<DateTime>
{
    protected override Type StyleKeyOverride { get; } = typeof(NumberDisplayerBase);

    protected override InterpolatingAnimator<DateTime> GetAnimator()
    {
        return new DateAnimator();
    }

    protected override string GetString(DateTime value)
    {
        return value.ToString(StringFormat);
    }

    private class DateAnimator : InterpolatingAnimator<DateTime>
    {
        public override DateTime Interpolate(double progress, DateTime oldValue, DateTime newValue)
        {
            var diff = (newValue - oldValue).TotalSeconds;
            try
            {
                return oldValue + TimeSpan.FromSeconds(diff * progress);
            }
            catch
            {
                return oldValue;
            }
        }
    }
}