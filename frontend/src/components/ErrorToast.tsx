interface ErrorToastProps {
  message: string;
  onDismiss: () => void;
}

/**
 * Visible signal surface for usePlaylistImport's `error` state (ADR-008).
 * Covers both Import failures (e.g. playlist not found) and Refresh
 * network/server failures — the hook sets the same `error` field for both,
 * so one toast component handles it.
 */
export function ErrorToast({ message, onDismiss }: ErrorToastProps) {
  return (
    <div className="error-toast" role="alert">
      <span>{message}</span>
      <button type="button" onClick={onDismiss} aria-label="Затвори поруку">
        ×
      </button>
    </div>
  );
}
