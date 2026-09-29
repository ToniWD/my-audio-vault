import React, { createContext, useContext, useState, useCallback } from 'react';
import { GlobalModalPopup } from './GlobalModalPopup';

export interface ModalConfig {
  title?: string;
  message: string;
  acceptLabel?: string;
  refuseLabel?: string;
  isDestructive?: boolean;

  hasInput?: boolean;
  inputPlaceholder?: string;
  secureTextEntry?: boolean;

  onAccept?: (inputValue?: string) => void | Promise<void>;
  onRefuse?: () => void | Promise<void>;
}

interface ModalContextType {
  showModal: (config: ModalConfig) => void;
}

const ModalContext = createContext<ModalContextType | undefined>(undefined);

export const ModalProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [queue, setQueue] = useState<ModalConfig[]>([]);

  const showModal = useCallback((config: ModalConfig) => {
    setQueue((prevQueue) => [...prevQueue, config]);
  }, []);

  const handleDismiss = useCallback(
    async (action?: () => void | Promise<void>) => {
      if (action) {
        await action();
      }
      setQueue((prevQueue) => prevQueue.slice(1));
    },
    []
  );

  const currentModal = queue[0];

  return (
    <ModalContext.Provider value={{ showModal }}>
      {children}
      {/* This component is mounted only once */}
      {currentModal && (
        <GlobalModalPopup
          key={currentModal.title || currentModal.message}
          config={currentModal}
          onAccept={(val) => handleDismiss(() => currentModal.onAccept?.(val))}
          onRefuse={() => handleDismiss(currentModal.onRefuse)}
        />
      )}
    </ModalContext.Provider>
  );
};

export const useModal = () => {
  const context = useContext(ModalContext);
  if (!context) {
    throw new Error('useModal must be used inside a ModalProvider');
  }
  return context;
};