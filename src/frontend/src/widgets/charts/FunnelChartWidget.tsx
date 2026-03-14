import React, { useMemo } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
  generateTooltip,
  generateEChartLegend,
} from './sharedUtils';
import { FunnelChartWidgetProps } from './chartTypes';
import { generateDataProps } from './sharedUtils';
import { getChartThemeColors } from './styles';
import {
  FUNNEL_DEFAULTS,
  PIE_LEGEND_DEFAULTS,
  applyDefaults,
} from './chartDefaults';

const EMPTY_ARRAY: never[] = [];

const FunnelChartWidget: React.FC<FunnelChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = 'Full',
  height = 'Full',
  funnels = EMPTY_ARRAY,
  tooltip,
  toolbox,
  legend,
  colorScheme = 'Default',
  sort = 'Descending',
  orientation = 'Vertical',
  gap = 0,
}) => {
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  const themeColors = useMemo(
    () => getChartThemeColors(colors, isDark),
    [colors, isDark]
  );

  const heightStyle = height ? getHeight(height) : {};
  const isFull = height?.toLowerCase().startsWith('full');

  const styles: React.CSSProperties = {
    ...getWidth(width),
    position: 'relative',
    ...(isFull
      ? { display: 'flex', flexDirection: 'column', height: '100%' }
      : {}),
  };

  const chartStyles: React.CSSProperties = {
    ...(isFull
      ? { flex: 1, minHeight: '200px' }
      : { ...heightStyle, minHeight: '200px' }),
    width: '100%',
  };

  const { valueKeys } = generateDataProps(data);

  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const funnelData = useMemo(
    () => data.map(d => ({ value: d.measure, name: d.dimension as string })),
    [data]
  );

  const echartsSort = useMemo(() => {
    switch (sort?.toLowerCase()) {
      case 'ascending':
        return 'ascending';
      case 'none':
        return 'none';
      case 'descending':
      default:
        return 'descending';
    }
  }, [sort]);

  const echartsOrient = useMemo(() => {
    return orientation?.toLowerCase() === 'horizontal'
      ? 'horizontal'
      : 'vertical';
  }, [orientation]);

  const series = useMemo(
    () =>
      valueKeys.map(key => {
        const rawFunnelConfig = funnels?.find(
          f => f.dataKey.toLowerCase() === key
        );
        const funnelConfig = rawFunnelConfig
          ? applyDefaults(rawFunnelConfig, FUNNEL_DEFAULTS)
          : FUNNEL_DEFAULTS;

        return {
          name: key,
          type: 'funnel' as const,
          sort: echartsSort,
          orient: echartsOrient,
          gap: gap ?? 0,
          left: '10%',
          right: '10%',
          top: '10%',
          bottom: '10%',
          min: 0,
          max: Math.max(...funnelData.map(d => d.value as number), 100),
          minSize: funnelConfig.minSize ?? '0%',
          maxSize: funnelConfig.maxSize ?? '100%',
          animation: funnelConfig.animated ?? FUNNEL_DEFAULTS.animated,
          label: {
            show: true,
            position: 'inside' as const,
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
          },
          labelLine: {
            show: false,
          },
          itemStyle: {
            color: funnelConfig.fill ?? undefined,
            opacity: funnelConfig.fillOpacity ?? undefined,
            borderColor: funnelConfig.stroke ?? undefined,
            borderWidth: funnelConfig.strokeWidth ?? FUNNEL_DEFAULTS.strokeWidth,
          },
          emphasis: {
            label: {
              fontSize: 20,
            },
          },
          data: funnelData,
        };
      }),
    [valueKeys, funnels, funnelData, echartsSort, echartsOrient, gap, themeColors]
  );

  const option = useMemo(() => {
    const leg = legend ? applyDefaults(legend, PIE_LEGEND_DEFAULTS) : null;

    return {
      color: chartColors,
      ...(leg && {
        legend: {
          orient:
            leg.layout?.toLowerCase() === 'vertical'
              ? 'vertical'
              : 'horizontal',
          left:
            leg.align?.toLowerCase() === 'left'
              ? 'left'
              : leg.align?.toLowerCase() === 'right'
                ? 'right'
                : 'center',
          top:
            leg.verticalAlign?.toLowerCase() === 'top'
              ? 'top'
              : leg.verticalAlign?.toLowerCase() === 'middle'
                ? 'middle'
                : 'bottom',
          icon: leg.iconType ?? 'circle',
          itemWidth: leg.iconSize ?? PIE_LEGEND_DEFAULTS.iconSize,
          itemHeight: leg.iconSize ?? PIE_LEGEND_DEFAULTS.iconSize,
          type: 'scroll',
          textStyle: generateTextStyle(
            themeColors.foreground,
            themeColors.fontSans
          ),
        },
      }),
      textStyle: generateTextStyle(
        themeColors.foreground,
        themeColors.fontSans
      ),
      tooltip: {
        ...generateTooltip(tooltip, undefined, {
          foreground: themeColors.foreground,
          fontSans: themeColors.fontSans,
          background: themeColors.background,
          mutedForeground: themeColors.mutedForeground,
        }),
        trigger: 'item' as const,
        formatter: '{a} <br/>{b}: {c}',
      },
      series: series,
      toolbox: generateEChartToolbox(
        toolbox && { ...toolbox, magicType: false }
      ),
    };
  }, [chartColors, legend, themeColors, tooltip, series, toolbox]);

  return (
    <div style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
      />
    </div>
  );
};

export default FunnelChartWidget;
