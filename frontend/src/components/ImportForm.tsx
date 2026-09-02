import { useState, type FormEvent } from 'react';

interface ImportFormProps {
  onImport: (spotifyPlaylistId: string) => void;
  isLoading: boolean;
}

/**
 * Accepts either a raw Spotify playlist ID or a full playlist URL/URI and
 * extracts the ID, since that's what users will naturally paste in.
 */
function extractPlaylistId(input: string): string {
  const trimmed = input.trim();
  const urlMatch = trimmed.match(/playlist\/([a-zA-Z0-9]+)/);
  if (urlMatch) return urlMatch[1];
  const uriMatch = trimmed.match(/spotify:playlist:([a-zA-Z0-9]+)/);
  if (uriMatch) return uriMatch[1];
  return trimmed;
}

export function ImportForm({ onImport, isLoading }: ImportFormProps) {
  const [input, setInput] = useState('');

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const id = extractPlaylistId(input);
    if (!id) return;
    onImport(id);
    setInput('');
  }

  return (
    <form onSubmit={handleSubmit} className="import-form">
      <input
        type="text"
        value={input}
        onChange={(e) => setInput(e.target.value)}
        placeholder="Spotify линк плејлисте или ID"
        disabled={isLoading}
        aria-label="Spotify плејлиста"
      />
      <button type="submit" disabled={isLoading || input.trim().length === 0}>
        {isLoading ? 'Увозим…' : 'Увези'}
      </button>
    </form>
  );
}
