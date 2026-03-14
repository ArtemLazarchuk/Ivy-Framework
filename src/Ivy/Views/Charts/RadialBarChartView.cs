using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record RadialBarChartData(string? Dimension, double Measure);

public enum RadialBarChartStyles
{
    Default,
    Dashboard,
    Gauge
}

public interface IRadialBarChartStyle<TSource>
{
    RadialBarChart Design(RadialBarChartData[] data);
}

public static class RadialBarChartStyleHelpers
{
    public static IRadialBarChartStyle<TSource> GetStyle<TSource>(RadialBarChartStyles style)
    {
        return style switch
        {
            RadialBarChartStyles.Default => new DefaultRadialBarChartStyle<TSource>(),
            RadialBarChartStyles.Dashboard => new DashboardRadialBarChartStyle<TSource>(),
            RadialBarChartStyles.Gauge => new GaugeRadialBarChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultRadialBarChartStyle<TSource> : IRadialBarChartStyle<TSource>
{
    public RadialBarChart Design(RadialBarChartData[] data)
    {
        return new RadialBarChart(data)
            .ColorScheme(ColorScheme.Default)
            .RadialBar(nameof(RadialBarChartData.Measure))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            );
    }
}

public class DashboardRadialBarChartStyle<TSource> : IRadialBarChartStyle<TSource>
{
    public RadialBarChart Design(RadialBarChartData[] data)
    {
        return new RadialBarChart(data)
            .ColorScheme(ColorScheme.Default)
            .RadialBar(new RadialBar(nameof(RadialBarChartData.Measure)).Background(true))
            .InnerRadius("20%")
            .OuterRadius("85%")
            .BarGap(6)
            .Tooltip(new ChartTooltip().Animated(true));
    }
}

public class GaugeRadialBarChartStyle<TSource> : IRadialBarChartStyle<TSource>
{
    public RadialBarChart Design(RadialBarChartData[] data)
    {
        return new RadialBarChart(data)
            .ColorScheme(ColorScheme.Default)
            .RadialBar(new RadialBar(nameof(RadialBarChartData.Measure)).Background(true))
            .StartAngle(90)
            .EndAngle(450)
            .InnerRadius("30%")
            .OuterRadius("80%")
            .Tooltip(new ChartTooltip().Animated(true));
    }
}

public class RadialBarChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource> dimension,
    Measure<TSource> measure,
    IRadialBarChartStyle<TSource>? style = null,
    Func<RadialBarChart, RadialBarChart>? polish = null)
    : ViewBase
{
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        var radialBarChartData = UseState(ImmutableArray.Create<RadialBarChartData>);
        var loading = UseState(true);
        var exception = UseState<Exception?>((Exception?)null);

        UseEffect(async () =>
        {
            try
            {
                var results = await data
                    .ToPivotTable()
                    .Dimension(dimension).Measure(measure).Produces<RadialBarChartData>().ExecuteAsync()
                    .ToArrayAsync();
                radialBarChartData.Set([.. results]);
            }
            catch (Exception e)
            {
                exception.Set(e);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.OnMount()]);

        if (exception.Value is not null)
        {
            return new ErrorTeaserView(exception.Value);
        }

        if (loading.Value)
        {
            return new ChatLoading();
        }

        var resolvedDesigner = style ?? RadialBarChartStyleHelpers.GetStyle<TSource>(RadialBarChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
           radialBarChartData.Value.ToArray()
        );

        var configuredChart = scaffolded;

        if (_toolbox is not null)
        {
            configuredChart = configuredChart.Toolbox(_toolbox);
        }
        else if (_toolboxFactory is not null)
        {
            var baseToolbox = configuredChart.Toolbox ?? new Toolbox();
            configuredChart = configuredChart.Toolbox(_toolboxFactory(baseToolbox));
        }

        var result = polish?.Invoke(configuredChart) ?? configuredChart;

        if (_height is not null)
            result = result with { Height = _height };
        if (_width is not null)
            result = result with { Width = _width };

        return result;
    }

    public RadialBarChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public RadialBarChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public RadialBarChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public RadialBarChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public RadialBarChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}


public static partial class RadialBarChartExtensions
{
    public static RadialBarChartBuilder<TSource> ToRadialBarChart<TSource>(
    this IEnumerable<TSource> data,
    Expression<Func<TSource, object>> dimension,
    Expression<Func<IQueryable<TSource>, object>> measure,
    RadialBarChartStyles style = RadialBarChartStyles.Default,
    Func<RadialBarChart, RadialBarChart>? polish = null)
    {
        return data.AsQueryable().ToRadialBarChart(dimension, measure, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static RadialBarChartBuilder<TSource> ToRadialBarChart<TSource>(
    this IQueryable<TSource> data,
    Expression<Func<TSource, object>> dimension,
    Expression<Func<IQueryable<TSource>, object>> measure,
    RadialBarChartStyles style = RadialBarChartStyles.Default,
    Func<RadialBarChart, RadialBarChart>? polish = null)
    {
        return new RadialBarChartBuilder<TSource>(data,
            new Dimension<TSource>(nameof(RadialBarChartData.Dimension), dimension),
            new Measure<TSource>(nameof(RadialBarChartData.Measure), measure),
            RadialBarChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}
