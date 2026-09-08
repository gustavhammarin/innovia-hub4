import type { ReactNode } from "react";
import { Modal } from "./Modal";

export function ConfirmDialog({
  title,
  children,
  confirmLabel = "Bekräfta",
  cancelLabel = "Avbryt",
  danger,
  isPending,
  error,
  onConfirm,
  onClose,
}: {
  title: string;
  children: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
  isPending?: boolean;
  error?: string | null;
  onConfirm: () => void;
  onClose: () => void;
}) {
  return (
    <Modal title={title} onClose={onClose}>
      <div className="space-y-4">
        <div className="text-sm text-gray-600 dark:text-gray-300">{children}</div>
        {error && <p className="text-sm text-red-600">{error}</p>}
        <div className="flex justify-end gap-2">
          <button
            onClick={onClose}
            disabled={isPending}
            className="text-sm font-medium text-gray-500 hover:text-gray-900 dark:hover:text-gray-100 disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <button
            onClick={onConfirm}
            disabled={isPending}
            className={`rounded-md px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50 ${
              danger ? "bg-red-600 hover:bg-red-500" : "bg-indigo-600 hover:bg-indigo-500"
            }`}
          >
            {isPending ? "..." : confirmLabel}
          </button>
        </div>
      </div>
    </Modal>
  );
}
