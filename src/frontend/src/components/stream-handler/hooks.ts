import { useContext, useEffect, useRef } from "react";
import { StreamSubscriber } from "./types";
import { StreamHandlerContext } from "./context";

export const useStreamSubscriber = (): StreamSubscriber => {
  const context = useContext(StreamHandlerContext);
  if (!context) {
    throw new Error("useStreamSubscriber must be used within a StreamHandlerProvider");
  }
  return context.subscribeToStream;
};

/**
 * Hook to subscribe to a stream and receive data.
 * Automatically handles subscription lifecycle.
 * Gracefully no-ops if StreamHandlerProvider is not in the tree.
 */
export const useStream = <T = unknown>(
  streamId: string | undefined,
  onData: (data: T) => void,
): void => {
  const context = useContext(StreamHandlerContext);
  const subscribeToStream = context?.subscribeToStream;
  const callbackRef = useRef(onData);
  useEffect(() => {
    callbackRef.current = onData;
  });

  useEffect(() => {
    if (!streamId || !subscribeToStream) return;

    const unsubscribe = subscribeToStream(streamId, (data) => {
      callbackRef.current(data as T);
    });

    return unsubscribe;
  }, [streamId, subscribeToStream]);
};
