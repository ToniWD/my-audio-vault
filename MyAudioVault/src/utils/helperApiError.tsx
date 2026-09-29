import { ProblemDetails } from "@/api/types.gen";
import { useModal } from "@/components/modal/ModalContext";

// Helper for showing error modal
export const handleApiError = (
    error: ProblemDetails | null | undefined,
    showModal: ReturnType<typeof useModal>['showModal']
) => {
    const errorMessage = error?.detail || error?.title || 'An unexpected error occurred. Please try again.';

    const modalTitle = error?.status
        ? `Error ${error.status}`
        : (error?.title || 'Error');

    showModal({
        title: modalTitle,
        message: errorMessage,
        acceptLabel: 'OK',
    });
};