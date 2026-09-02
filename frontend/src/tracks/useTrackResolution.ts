import { useCallback, useState } from 'react';
import { resolveTrack, resolveTracksBatch } from './api';
import { saveTracks } from '../library/db';
import type { Track } from '../playlists/types';

export interface BatchResolutionSummary {
  resolvedCount: number;
  failedCount: number;
}

/**
 * Orchestrates the Track Resolution use cases against the API, then persists the
 * result into the client-side Library (Track Resolution is stored as fields on the
 * existing Track record, per Phase 1 — no separate entity).
 */
export function useTrackResolution() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastBatchSummary, setLastBatchSummary] = useState<BatchResolutionSummary | null>(null);

  const doResolve = useCallback(async (track: Track): Promise<Track | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const { resolved, unresolved } = await resolveTrack(track);
      if (resolved) {
        await saveTracks([resolved]);
        return resolved;
      }
      // "No confident match" is an expected outcome, not an error — surface it as
      // informational rather than a failure toast (Phase 1 UCD decision).
      setError(unresolved?.reason ?? 'Could not resolve this track.');
      return null;
    } catch {
      setError("Couldn't reach the server to resolve this track. Please try again.");
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const doResolveBatch = useCallback(async (tracks: Track[]): Promise<Track[]> => {
    setIsLoading(true);
    setError(null);
    setLastBatchSummary(null);
    try {
      const { resolved, failed } = await resolveTracksBatch(tracks);
      if (resolved.length > 0) {
        await saveTracks(resolved);
      }
      setLastBatchSummary({ resolvedCount: resolved.length, failedCount: failed.length });
      return resolved;
    } catch {
      setError("Couldn't reach the server to resolve these tracks. Please try again.");
      return [];
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearError = useCallback(() => setError(null), []);
  const clearBatchSummary = useCallback(() => setLastBatchSummary(null), []);

  return { doResolve, doResolveBatch, isLoading, error, lastBatchSummary, clearError, clearBatchSummary };
}
