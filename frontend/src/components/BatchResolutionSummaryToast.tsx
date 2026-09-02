interface BatchResolutionSummaryToastProps {
  resolvedCount: number;
  failedCount: number;
  onDismiss: () => void;
}

/**
 * Visible summary after a batch Track Resolution run (Phase 1 decision: batch never
 * stops early, always reports a full resolved/failed summary at the end).
 */
export function BatchResolutionSummaryToast({ resolvedCount, failedCount, onDismiss }: BatchResolutionSummaryToastProps) {
  return (
    <div className="batch-summary-toast" role="status">
      <span>
        Нађено {resolvedCount}
        {failedCount > 0 && <>, неуспешно {failedCount}</>}
      </span>
      <button type="button" onClick={onDismiss} aria-label="Затвори поруку">
        ×
      </button>
    </div>
  );
}
