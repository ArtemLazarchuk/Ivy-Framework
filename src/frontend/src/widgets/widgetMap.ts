import { LoadingScreen } from "@/components/LoadingScreen";
import {
  ArticleWidget,
  BadgeWidget,
  CardWidget,
  ChatLoadingWidget,
  ChatMessageWidget,
  ChatStatusWidget,
  DropDownMenuWidget,
  ExpandableWidget,
  ProgressWidget,
  SheetWidget,
  SlotWidget,
  TooltipWidget,
  PaginationWidget,
} from "@/widgets";
import { ToolbarWidget } from "@/widgets/toolbar";
import { BreadcrumbsWidget } from "@/widgets/breadcrumbs";
import { StackedProgressWidget } from "@/widgets/stackedProgress";
import { FileDialogWidget, SaveDialogWidget, FolderDialogWidget } from "@/widgets/filePicker";
import { BladeContainerWidget, BladeWidget } from "@/widgets/blades";
import { DetailsWidget, DetailWidget } from "@/widgets/details";
import {
  DialogWidget,
  DialogHeaderWidget,
  DialogBodyWidget,
  DialogFooterWidget,
} from "@/widgets/dialogs";
import { FormWidget } from "@/widgets/forms";
import {
  FieldWidget,
  TextInputWidget,
  BoolInputWidget,
  NumberInputWidget,
  NumberRangeInputWidget,
  ReadOnlyInputWidget,
  ColorInputWidget,
  IconInputWidget,
  FileInputWidget,
  SignatureInputWidget,
  ContentInputWidget,
} from "@/widgets/inputs";
import {
  StackLayoutWidget,
  GridLayoutWidget,
  HeaderLayoutWidget,
  FooterLayoutWidget,
  SidebarLayoutWidget,
  SidebarMenuWidget,
  ResizablePanelGroupWidget,
  ResizablePanelWidget,
  FloatingPanelWidget,
} from "@/widgets/layouts";
import { ListItemWidget } from "@/widgets/lists";
import { TreeWidget } from "@/widgets/tree";
import {
  TextBlockWidget,
  HtmlWidget,
  ErrorWidget,
  SvgWidget,
  ImageWidget,
  IframeWidget,
  FragmentWidget,
  SeparatorWidget,
  SkeletonWidget,
  IconWidget,
  BoxWidget,
  CalloutWidget,
  KbdWidget,
  EmptyWidget,
  AvatarWidget,
  IvyLogoWidget,
  SpacerWidget,
  LoadingWidget,
  AppHostWidget,
  AutoScrollWidget,
  AudioPlayerWidget,
  VideoPlayerWidget,
} from "@/widgets/primitives";
import { TableWidget, TableRowWidget, TableCellWidget } from "@/widgets/tables";
import { SmartSearch } from "@/docs-internal/SmartSearch";
import { lazyWithRetry } from "@/lib/lazyWithRetry";

export const widgetMap = {
  $loading: LoadingScreen,

  // Primitives
  "Ivy.TextBlock": TextBlockWidget,
  "Ivy.RichTextBlock": lazyWithRetry(() =>
    import("@/widgets/primitives/RichTextBlockWidget").then((m) => ({
      default: m.RichTextBlockWidget,
    })),
  ),
  "Ivy.Markdown": lazyWithRetry(() => import("@/widgets/primitives/MarkdownWidget")),
  "Ivy.Json": lazyWithRetry(() => import("@/widgets/primitives/JsonWidget")),
  "Ivy.Html": HtmlWidget,
  "Ivy.Xml": lazyWithRetry(() => import("@/widgets/primitives/XmlWidget")),
  "Ivy.Error": ErrorWidget,
  "Ivy.Svg": SvgWidget,
  "Ivy.Image": ImageWidget,
  "Ivy.Iframe": IframeWidget,
  "Ivy.CodeBlock": lazyWithRetry(() => import("@/widgets/primitives/CodeBlockWidget")),
  "Ivy.Fragment": FragmentWidget,
  "Ivy.Separator": SeparatorWidget,
  "Ivy.Skeleton": SkeletonWidget,
  "Ivy.Icon": IconWidget,
  "Ivy.Box": BoxWidget,
  "Ivy.Embed": lazyWithRetry(() => import("@/widgets/primitives/EmbedWidget")),
  "Ivy.Script": lazyWithRetry(() => import("@/widgets/primitives/ScriptWidget")),
  "Ivy.Callout": CalloutWidget,
  "Ivy.Kbd": KbdWidget,
  "Ivy.Empty": EmptyWidget,
  "Ivy.Avatar": AvatarWidget,
  "Ivy.IvyLogo": IvyLogoWidget,
  "Ivy.Spacer": SpacerWidget,
  "Ivy.Loading": LoadingWidget,
  "Ivy.AppHost": AppHostWidget,
  "Ivy.AutoScroll": AutoScrollWidget,
  "Ivy.AudioPlayer": AudioPlayerWidget,
  "Ivy.VideoPlayer": VideoPlayerWidget,
  "Ivy.Stepper": lazyWithRetry(() => import("@/widgets/primitives/StepperWidget")),
  "Ivy.Terminal": lazyWithRetry(() => import("@/widgets/primitives/TerminalWidget")),

  // Widgets
  "Ivy.Article": ArticleWidget,
  "Ivy.Button": lazyWithRetry(() =>
    import("@/widgets/button/ButtonWidget").then((m) => ({
      default: m.ButtonWidget,
    })),
  ),
  "Ivy.Progress": ProgressWidget,
  "Ivy.StackedProgress": StackedProgressWidget,
  "Ivy.Tooltip": TooltipWidget,
  "Ivy.Toolbar": ToolbarWidget,
  "Ivy.Slot": SlotWidget,
  "Ivy.Card": CardWidget,
  "Ivy.Sheet": SheetWidget,
  "Ivy.Badge": BadgeWidget,
  "Ivy.Breadcrumbs": BreadcrumbsWidget,
  "Ivy.Expandable": ExpandableWidget,
  "Ivy.Chat": lazyWithRetry(() =>
    import("@/widgets/chat/ChatWidget").then((m) => ({
      default: m.ChatWidget,
    })),
  ),
  "Ivy.ChatMessage": ChatMessageWidget,
  "Ivy.ChatLoading": ChatLoadingWidget,
  "Ivy.ChatStatus": ChatStatusWidget,
  "Ivy.DropDownMenu": DropDownMenuWidget,
  "Ivy.Pagination": PaginationWidget,
  "Ivy.Kanban": lazyWithRetry(() =>
    import("@/widgets/kanban/KanbanWidget").then((m) => ({
      default: m.KanbanWidget,
    })),
  ),
  "Ivy.KanbanCard": lazyWithRetry(() =>
    import("@/widgets/kanban/KanbanCardWidget").then((m) => ({
      default: m.KanbanCardWidget,
    })),
  ),
  "Ivy.Calendar": lazyWithRetry(() =>
    import("@/widgets/calendar/CalendarWidget").then((m) => ({
      default: m.CalendarWidget,
    })),
  ),
  "Ivy.CalendarEvent": lazyWithRetry(() =>
    import("@/widgets/calendar/CalendarEventWidget").then((m) => ({
      default: m.CalendarEventWidget,
    })),
  ),

  // Layouts
  "Ivy.StackLayout": StackLayoutWidget,
  "Ivy.GridLayout": GridLayoutWidget,
  "Ivy.HeaderLayout": HeaderLayoutWidget,
  "Ivy.FooterLayout": FooterLayoutWidget,
  "Ivy.TabsLayout": lazyWithRetry(() =>
    import("@/widgets/layouts/tabs/TabsLayoutWidget").then((m) => ({
      default: m.TabsLayoutWidget,
    })),
  ),
  "Ivy.Tab": lazyWithRetry(() =>
    import("@/widgets/layouts/tabs/TabWidget").then((m) => ({
      default: m.TabWidget,
    })),
  ),
  "Ivy.SidebarLayout": SidebarLayoutWidget,
  "Ivy.SidebarMenu": SidebarMenuWidget,
  "Ivy.ResizablePanelGroup": ResizablePanelGroupWidget,
  "Ivy.ResizablePanel": ResizablePanelWidget,
  "Ivy.FloatingPanel": FloatingPanelWidget,

  // Inputs
  "Ivy.Field": FieldWidget,
  "Ivy.TextInput": TextInputWidget,
  "Ivy.BoolInput": BoolInputWidget,
  "Ivy.DateTimeInput": lazyWithRetry(() =>
    import("@/widgets/inputs/DateTimeInputWidget").then((m) => ({
      default: m.DateTimeInputWidget,
    })),
  ),
  "Ivy.NumberInput": NumberInputWidget,
  "Ivy.NumberRangeInput": NumberRangeInputWidget,
  "Ivy.SelectInput": lazyWithRetry(() =>
    import("@/widgets/inputs/SelectInputWidget").then((m) => ({
      default: m.SelectInputWidget,
    })),
  ),
  "Ivy.ReadOnlyInput": ReadOnlyInputWidget,
  "Ivy.ColorInput": ColorInputWidget,
  "Ivy.IconInput": IconInputWidget,
  "Ivy.FeedbackInput": lazyWithRetry(() =>
    import("@/widgets/inputs/FeedbackInputWidget").then((m) => ({
      default: m.FeedbackInputWidget,
    })),
  ),
  "Ivy.AsyncSelectInput": lazyWithRetry(() =>
    import("@/widgets/inputs/AsyncSelectInputWidget").then((m) => ({
      default: m.AsyncSelectInputWidget,
    })),
  ),
  "Ivy.DateRangeInput": lazyWithRetry(() =>
    import("@/widgets/inputs/DateRangeInputWidget").then((m) => ({
      default: m.DateRangeInputWidget,
    })),
  ),
  "Ivy.FileInput": FileInputWidget,
  "Ivy.ContentInput": ContentInputWidget,
  "Ivy.SignatureInput": SignatureInputWidget,

  "Ivy.CodeInput": lazyWithRetry(() => import("@/widgets/inputs/code/CodeInputWidget")),
  "Ivy.AudioInput": lazyWithRetry(() => import("@/widgets/inputs/AudioInputWidget")),
  "Ivy.CameraInput": lazyWithRetry(() => import("@/widgets/cameraInput/CameraInputWidget")),

  // Forms
  "Ivy.Form": FormWidget,

  // File Pickers
  "Ivy.FileDialog": FileDialogWidget,
  "Ivy.SaveDialog": SaveDialogWidget,
  "Ivy.FolderDialog": FolderDialogWidget,

  // Dialogs
  "Ivy.Dialog": DialogWidget,
  "Ivy.DialogHeader": DialogHeaderWidget,
  "Ivy.DialogBody": DialogBodyWidget,
  "Ivy.DialogFooter": DialogFooterWidget,

  // Blades
  "Ivy.BladeContainer": BladeContainerWidget,
  "Ivy.Blade": BladeWidget,

  // Tables
  "Ivy.Table": TableWidget,
  "Ivy.TableRow": TableRowWidget,
  "Ivy.TableCell": TableCellWidget,

  // DataTables
  "Ivy.DataTable": lazyWithRetry(() => import("@/widgets/dataTables/DataTableWidget")),

  // Lists
  "Ivy.List": lazyWithRetry(() =>
    import("@/widgets/lists/ListWidget").then((m) => ({
      default: m.ListWidget,
    })),
  ),
  "Ivy.ListItem": ListItemWidget,

  // Tree
  "Ivy.Tree": TreeWidget,

  // Details
  "Ivy.Details": DetailsWidget,
  "Ivy.Detail": DetailWidget,

  // Charts
  "Ivy.LineChart": lazyWithRetry(() => import("@/widgets/charts/LineChartWidget")),
  "Ivy.PieChart": lazyWithRetry(() => import("@/widgets/charts/PieChartWidget")),
  "Ivy.AreaChart": lazyWithRetry(() => import("@/widgets/charts/AreaChartWidget")),
  "Ivy.BarChart": lazyWithRetry(() => import("@/widgets/charts/BarChartWidget")),
  "Ivy.ScatterChart": lazyWithRetry(() => import("@/widgets/charts/ScatterChartWidget")),
  "Ivy.RadarChart": lazyWithRetry(() => import("@/widgets/charts/RadarChartWidget")),
  "Ivy.SankeyChart": lazyWithRetry(() => import("@/widgets/charts/SankeyChartWidget")),
  "Ivy.ChordChart": lazyWithRetry(() => import("@/widgets/charts/ChordChartWidget")),
  "Ivy.FunnelChart": lazyWithRetry(() => import("@/widgets/charts/FunnelChartWidget")),
  "Ivy.GaugeChart": lazyWithRetry(() => import("@/widgets/charts/GaugeChartWidget")),

  // Effects
  "Ivy.Confetti": lazyWithRetry(() => import("@/widgets/effects/ConfettiWidget")),
  "Ivy.Animation": lazyWithRetry(() => import("@/widgets/effects/AnimationWidget")),

  // Internal
  "Ivy.Docs.Shared.Internal.SmartSearch": SmartSearch,
  "Ivy.Widgets.Internal.SidebarNews": lazyWithRetry(
    () => import("@/widgets/internal/SidebarNewsWidget"),
  ),
  "Ivy.Widgets.Internal.ThemeColorPicker": lazyWithRetry(() =>
    import("@/widgets/internal/ThemeColorPickerWidget").then((m) => ({
      default: m.ThemeColorPickerWidget,
    })),
  ),
};
