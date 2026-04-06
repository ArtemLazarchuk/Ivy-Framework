export const formatBytes = (bytes: number, precision?: number): string => {
  if (bytes === 0) return "0 B";

  const units = ["B", "KB", "MB", "GB", "TB", "PB"];
  const base = 1024;
  const exponent = Math.floor(Math.log(Math.abs(bytes)) / Math.log(base));
  const unitIndex = Math.min(Math.max(exponent, 0), units.length - 1);
  const value = bytes / Math.pow(base, unitIndex);

  const effectivePrecision = precision ?? (value >= 10 ? 0 : 2);
  return `${value.toFixed(effectivePrecision)} ${units[unitIndex]}`;
};
