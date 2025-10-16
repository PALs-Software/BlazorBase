window.blazorBase = {
    version: "1.0.0.0"
};

const componentsReconnectObserver = new MutationObserver(() => {
    let dialog = document.getElementById('components-reconnect-modal')?.shadowRoot?.querySelector('.components-reconnect-dialog');
    if (dialog)
        dialog.style.backgroundColor = "var(--bs-body-bg)";
});

componentsReconnectObserver.observe(document.body, { childList: true, subtree: true });