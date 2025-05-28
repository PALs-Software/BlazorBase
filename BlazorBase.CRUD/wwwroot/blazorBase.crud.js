class BlazorbaseDataListInput {
    static dotNetReferences = new Object();
    static listActive;

    static init(id, dotNetReference, resetValueAfterSelection) {
        document.getElementById(id).addEventListener('focusin', BlazorbaseDataListInput.listShow);
        document.getElementById(id).addEventListener('focusout', BlazorbaseDataListInput.onFocusOut);

        BlazorbaseDataListInput.dotNetReferences[id] = { dotNetReference: dotNetReference, resetValueAfterSelection: resetValueAfterSelection, isSelfNavigating: false };
    }

    static listShow(e) {

        const input = BlazorbaseDataListInput.target(e);
        if (!input) return;

        if (input.list) {

            // setup of datalist control
            const dl = input.list;
            input.datalist = dl;
            input.removeAttribute('list');

            dl.input = input;
            dl.setAttribute('tabindex', -1);

            // event handlers
            input.addEventListener('input', BlazorbaseDataListInput.listLimit);
            input.addEventListener('keydown', BlazorbaseDataListInput.listControl);
            dl.addEventListener('keydown', BlazorbaseDataListInput.listKey);
            dl.addEventListener('click', BlazorbaseDataListInput.listSet);
            dl.addEventListener('focusout', BlazorbaseDataListInput.onFocusOut);
        }

        // show datalist
        const dl = input.datalist;
        if (dl && !dl.shown) {

            BlazorbaseDataListInput.listHide(BlazorbaseDataListInput.listActive);

            dl.shown = true;
            BlazorbaseDataListInput.listLimit(e);
            if (input.offsetWidth < 230)
                dl.style.width = '230px';
            else
                dl.style.width = input.offsetWidth + 'px';

            dl.style.left = input.offsetLeft + 'px';
            dl.style.display = 'block';
            BlazorbaseDataListInput.listActive = dl;

        }

    }

    static onFocusOut(e) {
        const element = BlazorbaseDataListInput.target(e);
        if (!element)
            return;

        let input, datalist;
        if (element.tagName === "INPUT") {
            input = element;
            datalist = input.datalist;
        } else if (element.tagName === "DATALIST") {
            datalist = element;
            input = datalist.previousElementSibling;
        } else if (element.tagName === "OPTION") {
            datalist = element.parentElement;
            input = datalist.previousElementSibling;
        } else {
            return;
        }

        if (BlazorbaseDataListInput.dotNetReferences[input.id].isSelfNavigating) {
            BlazorbaseDataListInput.dotNetReferences[input.id].isSelfNavigating = false;
            return;
        }

        setTimeout(function () { // Wait for new focus, so active element can be checked
            if (document.activeElement &&
                (
                    document.activeElement == input ||
                    document.activeElement == datalist ||
                    (document.activeElement.parentElement && document.activeElement.parentElement == datalist)
                ))
                return;

            BlazorbaseDataListInput.listHide(input.datalist);
        }, 2);
    }

    // hide datalist
    static listHide(dl) {

        if (dl && dl.shown) {
            dl.style.display = 'none';
            dl.shown = false;
        }

    }

    // enable valid and disable invalid options
    static listLimit(e) {

        const input = BlazorbaseDataListInput.target(e);
        if (!input || !input.datalist) return;

        if (!input.datalist.shown)
            BlazorbaseDataListInput.listShow({ target: input });

        const v = input.value.trim().toLowerCase();
        const valueArr = v.split(' ');
        Array.from(input.datalist.getElementsByTagName('option')).forEach(opt => {
            opt.setAttribute('tabindex', 0);
            opt.style.display = !v || BlazorbaseDataListInput.valueContainsAllSearchArrValues(valueArr, opt.getAttribute('label').toLowerCase()) ? 'block' : 'none';
        });

    }

    static valueContainsAllSearchArrValues(searchValueArr, value) {
        for (let i = 0; i < searchValueArr.length; i++) {
            if (!value.includes(searchValueArr[i])) {
                return false;
            }
        }
        return true;
    }

    // key event on input
    static listControl(e) {

        const input = BlazorbaseDataListInput.target(e);
        if (!input || !input.datalist) return;

        switch (e.keyCode) {

            case 40: {
                // arrow down
                let opt = input.datalist.firstElementChild;
                if (!opt.offsetHeight) opt = BlazorbaseDataListInput.visibleSibling(opt, 1);
                if (opt) {
                    BlazorbaseDataListInput.dotNetReferences[input.id].isSelfNavigating = true;
                    opt.focus();
                }
                break;
            }

            case 9:   // tab
                BlazorbaseDataListInput.listHide(input.datalist);
                break;

            case 13:  // enter
                BlazorbaseDataListInput.listSet(e);
                break;

            case 27:  // esc
                BlazorbaseDataListInput.listHide(input.datalist);
                break;

        }

    }

    // key event on datalist
    static keymap = {
        33: -12,
        34: 12,
        38: -1,
        40: 1
    };

    static listKey(e) {
        const t = BlazorbaseDataListInput.target(e);
        if (!t) return;

        const
            kc = e.keyCode,
            dir = BlazorbaseDataListInput.keymap[kc],
            dl = t.parentElement;

        if (dir) {
            // move through list
            let opt = BlazorbaseDataListInput.visibleSibling(t, dir);
            if (opt) {
                BlazorbaseDataListInput.dotNetReferences[dl.input.id].isSelfNavigating = true;
                opt.focus();
            }
            e.preventDefault();

        }
        else if (kc === 9 || kc === 13 || kc === 32) {

            // tab, enter, space: use value
            BlazorbaseDataListInput.listSet(e);

        }
        else if (kc === 8) {

            // backspace: return to input
            dl.input.focus();

        }
        else if (kc === 27) {

            // esc: hide list
            BlazorbaseDataListInput.listHide(dl);

        }
    }

    // get previous/next visible sibling
    static visibleSibling(opt, dir) {

        let newOpt = opt;

        do {

            if (dir < 0) {
                newOpt = newOpt.previousElementSibling;
            }
            else if (dir > 0) {
                newOpt = newOpt.nextElementSibling;
            }

            if (newOpt && newOpt.offsetHeight) {
                opt = newOpt;
                dir -= Math.sign(dir);
            }

        } while (newOpt && dir);

        return opt;

    }

    // set datalist option to input value
    static async listSet(e) {
        let t = BlazorbaseDataListInput.target(e);
        let dl = t && t.parentElement;
        let input = dl.input;

        if (!dl || !dl.input) {
            // Enter without selecting a value from the drop down list
            const v = t.value.trim().toLowerCase();
            const valueArr = v.split(' ');
            let firstOpenOption = Array.from(t.datalist.getElementsByTagName('option')).find(opt => {
                return BlazorbaseDataListInput.valueContainsAllSearchArrValues(valueArr, opt.getAttribute('label').toLowerCase())
            });

            if (!firstOpenOption)
                return;

            input = t;
            t = firstOpenOption;
            dl = firstOpenOption.parentElement;
        }

        input.value = (t && t.getAttribute('label')) || '';
        input.setAttribute('data-value', (t && t.value) || '');
        await BlazorbaseDataListInput.dotNetReferences[input.id].dotNetReference.invokeMethodAsync('ValueChanged', (t && t.value) || '');
        BlazorbaseDataListInput.listHide(dl);

        if (BlazorbaseDataListInput.dotNetReferences[input.id].resetValueAfterSelection) {
            input.value = '';
        }
    }

    // fetch target node
    static target(t) {
        return t && t.target;
    }
}

class InputVisibilityObserver {
    constructor(containerId) {
        this.containerId = containerId;
        this.container = null;
        this.intersectionObserver = null;
        this.mutationObserver = null;
        this.observedInputs = new WeakSet();
    }

    start() {
        this.container = document.getElementById(this.containerId);
        if (!this.container) {
            console.warn('Container not found with selector:', this.tableSelector);
            return;
        }

        this.intersectionObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                const input = entry.target;
                if (!entry.isIntersecting && document.activeElement === input)
                    input.blur();
            });
        }, {
            root: null,
            threshold: 0.01,
        });

        this.observeExistingInputs();

        this.mutationObserver = new MutationObserver((mutations) => {
            mutations.forEach(mutation => {
                this.handleRemovedNodes(mutation.removedNodes);
                this.handleAddedNodes(mutation.addedNodes);
            });
        });

        this.mutationObserver.observe(this.container, {
            childList: true,
            subtree: true,
        });
    }

    observeExistingInputs() {
        const inputs = this.container.querySelectorAll('input');
        inputs.forEach(input => {
            if (!this.observedInputs.has(input)) {
                this.intersectionObserver.observe(input);
                this.observedInputs.add(input);
            }
        });
    }

    handleAddedNodes(nodes) {
        nodes.forEach(node => {
            if (node.nodeType !== 1) return; // not an element
            if (node.tagName === 'INPUT') {
                if (!this.observedInputs.has(node)) {
                    this.intersectionObserver.observe(node);
                    this.observedInputs.add(node);
                }
            } else {
                node.querySelectorAll?.('input')?.forEach(input => {
                    if (!this.observedInputs.has(input)) {
                        this.intersectionObserver.observe(input);
                        this.observedInputs.add(input);
                    }
                });
            }
        });
    }

    handleRemovedNodes(nodes) {
        nodes.forEach(node => {
            if (node.nodeType !== 1) return; // not an element
            if (node.tagName === 'INPUT') {
                this.intersectionObserver.unobserve(node);
                this.observedInputs.delete(node);
            } else {
                node.querySelectorAll?.('input')?.forEach(input => {
                    this.intersectionObserver.unobserve(input);
                    this.observedInputs.delete(input);
                });
            }
        });
    }

    stop() {
        if (this.intersectionObserver) {
            if (this.container) {
                this.container.querySelectorAll('input').forEach(input => {
                    this.intersectionObserver.unobserve(input);
                });
            }
            this.intersectionObserver.disconnect();
            this.intersectionObserver = null;
        }

        if (this.mutationObserver) {
            this.mutationObserver.disconnect();
            this.mutationObserver = null;
        }

        this.observedInputs = new WeakSet();
        this.container = null;
    }
}

window.blazorBase.crud = {
    blazorbaseDataListInput: BlazorbaseDataListInput,
    inputVisibilityObserver: {
        observers: {},

        createObserver: (id, containerId) => {
            const observers = window.blazorBase.crud.inputVisibilityObserver.observers;

            if (observers[id]) {
                console.warn(`Observer with id '${id}' already exists.`);
                return;
            }

            observers[id] = new InputVisibilityObserver(containerId);
            observers[id]?.start();
        },

        deleteObserver: (id) => {
            const observers = window.blazorBase.crud.inputVisibilityObserver.observers;
            if (observers[id]) {
                observers[id].stop();
                delete observers[id];
            }
        }
    }
};