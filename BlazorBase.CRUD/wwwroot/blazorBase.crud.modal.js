if (!window.blazoriseBootstrap) {
    window.blazoriseBootstrap = {};
}

window.blazoriseBootstrap = {
    tooltip: {
        initialize: (element, elementId) => {
            if (element.querySelector(".custom-control-input,.btn")) {
                element.classList.add("b-tooltip-inline");
            }

            return true;
        }
    },

    modal: {
        openedModals: new Set(),

        setModalBodyScrollable: (element) => {
            const modalContent = element.querySelector('.modal-content');
            if (!modalContent)
                return;

            const modalBody = modalContent.querySelector('.modal-body');
            if (!modalBody)
                return;

            let totalHeight = 0;
            Array.from(modalContent.children).forEach(child => {
                if (!child.classList.contains('modal-body')) {
                    totalHeight += child.offsetHeight;
                }
            });

            modalBody.style.maxHeight = `calc(100dvh - ${totalHeight}px - 3.5rem - 10px)`;

            if (!modalBody.classList.contains('overflow-auto')) {
                modalBody.classList.add('overflow-auto');
            }
        },

        recalculateOpenedModalsBodyMaxHeight: () => {
            window.blazoriseBootstrap.modal.openedModals.forEach(modalId => {
                const modalElement = document.getElementById(modalId);
                if (modalElement)
                    window.blazoriseBootstrap.modal.setModalBodyScrollable(modalElement);
            });
        },

        open: (element, scrollToTop) => {
            window.blazoriseBootstrap.modal.openedModals.add(element.id);
            var modals = Number(document.body.getAttribute("data-modals") || "0");

            if (modals === 0) {
                window.blazorise.addClassToBody("modal-open");
            }

            modals += 1;
            document.body.setAttribute("data-modals", modals.toString());

            window.blazoriseBootstrap.modal.setModalBodyScrollable(element);

            if (scrollToTop) {
                element.querySelector('.modal-body').scrollTop = 0;
            }

            return true;
        },
        close: (element) => {
            if (!window.blazoriseBootstrap.modal.openedModals.has(element.id))
                return false;
            window.blazoriseBootstrap.modal.openedModals.delete(element.id);

            var modals = Number(document.body.getAttribute("data-modals") || "0");

            modals -= 1;

            if (modals < 0) {
                modals = 0;
            }

            if (modals === 0) {
                window.blazorise.removeClassFromBody("modal-open");
            }

            document.body.setAttribute("data-modals", modals.toString());

            return true;
        }
    }

    //activateDatePicker: (elementId, formatSubmit) => {
    //    const element = $(`#${elementId}`);

    //    element.datepicker({
    //        uiLibrary: 'bootstrap4',
    //        format: 'yyyy-mm-dd',
    //        showOnFocus: true,
    //        showRightIcon: true,
    //        select: function (e, type) {
    //            // trigger onchange event on the DateEdit component
    //            mutateDOMChange(elementId);
    //        }
    //    });
    //    return true;
    //}
};

function mutateDOMChange(id) {
    el = document.getElementById(id);
    ev = document.createEvent('Event');
    ev.initEvent('change', true, false);
    el.dispatchEvent(ev);
}

window.addEventListener('resize', () => {
    if (window.blazoriseBootstrap && window.blazoriseBootstrap.modal)
        window.blazoriseBootstrap.modal.recalculateOpenedModalsBodyMaxHeight();
});