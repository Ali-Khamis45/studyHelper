import { useSyncExternalStore } from "react";

const emptySubscribe = () => () => {};

/**
 * True only after the client has hydrated. Unlike `useState` + `useEffect(() => setMounted(true))`,
 * this doesn't set state inside an effect — React's hydration pass resolves the server/client
 * snapshot mismatch itself, so components that render differently pre/post-hydration (e.g. reading
 * `localStorage` or the resolved theme) don't flash the wrong value or trip up-to-date lint rules.
 */
export function useMounted() {
  return useSyncExternalStore(
    emptySubscribe,
    () => true,
    () => false,
  );
}
